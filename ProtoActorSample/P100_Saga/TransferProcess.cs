using P100_Saga.Messages;
using Proto;
using Proto.Persistence;
using System;
using System.Threading.Tasks;

namespace P100_Saga
{
    class TransferProcess : IActor
    {
        private readonly PID _from;
        private readonly PID _to;
        private readonly decimal _amount;
        private readonly Random _random;
        private readonly double _availability;
        private readonly Persistence _persistence;
        private readonly Behavior _behavior = new Behavior();
        private bool _restarting;
        private bool _stopping;
        private bool _processCompleted;

        public TransferProcess(PID from, PID to, decimal amount, IProvider provider, string persistenceId, Random random, double availability)
        {
            _from = from;
            _to = to;
            _amount = amount;
            _random = random;
            _availability = availability;
            _persistence = Persistence.WithEventSourcing(provider, persistenceId, ApplyEvent);
        }


        private void ApplyEvent(Event @event)
        {
            switch (@event.Data)
            {
                //转帐开始
                case TransferStarted msg:
                    //转换成等待贷方确认
                    _behavior.Become(AwaitingDebitConfirmation);
                    break;
                //借方扣除
                case AccountDebited msg:
                    //转换成等待借方确认
                    _behavior.Become(AwaitingCreditConfirmation);
                    break;
                //贷方拒绝
                case CreditRefused msg:
                    _behavior.Become(RollingBackDebit);
                    break;
                //贷方存入
                case AccountCredited _:
                //借方回滚
                case DebitRolledBack _:
                //转帐失败
                case TransferFailed _:
                    _processCompleted = true;
                    break;
            }
        }

        private bool Fail()
        {
            var comparison = _random.NextDouble() * 100;
            return comparison > _availability;
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started msg:
                    //自定义开始的行为
                    // default to Starting behavior
                    _behavior.Become(Starting);
                    //从持久化库中恢复，如果当有一些事件，当前行为将会改变
                    // recover state from persistence - if there are any events, the current behavior should change
                    await _persistence.RecoverStateAsync();
                    break;
                case Stopping msg:
                    _stopping = true;
                    break;
                case Restarting msg:
                    _restarting = true;
                    break;
                case Stopped _ when !_processCompleted:
                    await _persistence.PersistEventAsync(new TransferFailed($"Unknown. Transfer Process crashed"));
                    await _persistence.PersistEventAsync(new EscalateTransfer($"Unknown failure. Transfer Process crashed"));
                    context.Parent.Tell(new UnknownResult(context.Self));
                    return;
                case Terminated _ when _restarting || _stopping:
                    // if the TransferProcess itself is restarting or stopping due to failure, we will receive a Terminated message for any child actors due to them being stopped but we should not
                    // treat this as a failure of the saga, so return here to stop further processing
                    //如果TransferProcess本身由于失败而重新启动或停止，我们将不会对其进行处理，因为我们没有将其视为saga故障，因此请返回此处以停止进一步处理
                    return;
                default:
                    // simulate failures of the transfer process itself
                    //模拟转换过程的失败
                    if (Fail())
                    {
                        throw new Exception();
                    }
                    break;
            }

            // pass through all messages to the current behavior. Note this includes the Started message we
            // may have just handled as what we should do when started depends on the current behavior
            //将所有消息传递给当前行为。这是我们刚刚处理的已启动消息，因为我们应该开始执行的操作取决于当前行为
            await _behavior.ReceiveAsync(context);
        }

        private async Task Starting(IContext context)
        {
            if (context.Message is Started)
            {

                var props = Actor.FromProducer(() => new AccountProxy(_from, sender => new Debit(-_amount, sender)));
                context.SpawnNamed(props, "DebitAttempt");
                await _persistence.PersistEventAsync(new TransferStarted());
            }
        }
        /// <summary>
        /// 等待借方确认
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task AwaitingDebitConfirmation(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    // if we are in this state when restarted then we need to recreate the TryDebit actor
                    //如果我们在重新启动时处于这种状态，那么我们需要重新创建                   
                    var props_started = Actor.FromProducer(() => new AccountProxy(_from, sender => new Debit(-_amount, sender)));
                    context.SpawnNamed(props_started, "DebitAttempt");
                    break;
                case OK _:
                    // good to proceed to the credit
                    //借方扣除成功
                    await _persistence.PersistEventAsync(new AccountDebited());
                    var props_ok = Actor.FromProducer(() => new AccountProxy(_to, sender => new Credit(+_amount, sender)));
                    context.SpawnNamed(props_ok, "CreditAttempt");
                    break;
                case Refused _:
                    // the debit has been refused, and should not be retried 
                    //借方已被拒绝，不应重审
                    await _persistence.PersistEventAsync(new TransferFailed($"Debit refused"));
                    context.Parent.Tell(new FailedButConsistentResult(context.Self));
                    StopAll(context);
                    break;
                case Terminated _:
                    // the actor that is trying to make the debit has failed to respond with success
                    // we dont know why
                    //试图进行借记的Actor未能成功回应,不知道为什么
                    await _persistence.PersistEventAsync(new StatusUnknown());
                    StopAll(context);
                    break;
            }
        }
        /// <summary>
        /// 等待贷方确认
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task AwaitingCreditConfirmation(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    // if we are in this state when started then we need to recreate the TryCredit actor
                    //如果我们在启动时处于这种状态，那么我们需要重新创建
                    var props_started = Actor.FromProducer(() => new AccountProxy(_to, sender => new Credit(+_amount, sender)));
                    context.SpawnNamed(props_started, "CreditAttempt");
                    break;
                case OK msg:
                    decimal fromBalance = await _from.RequestAsync<decimal>(new GetBalance(), TimeSpan.FromMilliseconds(2000));
                    decimal toBalance = await _to.RequestAsync<decimal>(new GetBalance(), TimeSpan.FromMilliseconds(2000));
                    //贷方存入
                    await _persistence.PersistEventAsync(new AccountCredited());
                    await _persistence.PersistEventAsync(new TransferCompleted(_from, fromBalance, _to, toBalance));
                    context.Parent.Tell(new SuccessResult(context.Self));
                    StopAll(context);
                    break;
                case Refused msg:
                    // sometimes a remote service might say it refuses to perform some operation. 
                    // This is different from a failure
                    //有时远程服务可能会说它拒绝执行某些操作。
                    //这与失败不同
                    //代方拒绝
                    await _persistence.PersistEventAsync(new CreditRefused());
                    // we have definately debited the _from account as it was confirmed, and we haven't creidted to _to account, so try and rollback
                    //我们在借方帐户中扣除了，我们还没有在贷方帐户提存入，所以请尝试回滚
                    var props_refused = Actor.FromProducer(() => new AccountProxy(_from, sender => new Credit(+_amount, sender)));
                    context.SpawnNamed(props_refused, "RollbackDebit");
                    break;
                case Terminated msg:
                    // at this point, we do not know if the credit succeeded. The remote account has not confirmed success, but it might have succeeded then crashed, or failed to respond.
                    // Given that we don't know, just fail + escalate
                    //鉴于我们不知道，只是失败+升级，我们没有成功，我们不知道成功然后崩溃，或未能回应
                    await _persistence.PersistEventAsync(new StatusUnknown());
                    StopAll(context);
                    break;
            }
        }
        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task RollingBackDebit(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    // if we are in this state when started then we need to recreate the TryCredit actor
                    //如果我们在启动时处于这种状态，那么我们需要重新创建
                    var props_started = Actor.FromProducer(() => new AccountProxy(_from, sender => new Credit(+_amount, sender)));
                    context.SpawnNamed(props_started, $"RollbackDebit");
                    break;
                case OK _:
                    await _persistence.PersistEventAsync(new DebitRolledBack());
                    await _persistence.PersistEventAsync(new TransferFailed($"Unable to rollback debit to {_to.Id}"));
                    context.Parent.Tell(new FailedButConsistentResult(context.Self));
                    StopAll(context);
                    break;
                case Refused _: // in between making the credit and debit, the _from account has started refusing!! :O 在进行信用卡和借记卡之间，_来自帐户已开始拒绝!!：O
                case Terminated _:
                    await _persistence.PersistEventAsync(new TransferFailed($"Unable to rollback process. {_from.Id} is owed {_amount}"));
                    await _persistence.PersistEventAsync(new EscalateTransfer($"{_from.Id} is owed {_amount}"));
                    context.Parent.Tell(new FailedAndInconsistent(context.Self));
                    StopAll(context);
                    break;
            }
        }

        private void StopAll(IContext context)
        {
            _from.Stop();
            _to.Stop();
            context.Self.Stop();
        }
    }
}