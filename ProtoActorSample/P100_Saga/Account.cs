using P100_Saga.Messages;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P100_Saga
{
    class Account : IActor
    {
        /// <summary>
        /// 名称
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// 服务时间
        /// </summary>
        private readonly double _serviceUptime;
        /// <summary>
        /// 拒绝概率
        /// </summary>
        private readonly double _refusalProbability;
        /// <summary>
        /// 繁忙的概率
        /// </summary>
        private readonly double _busyProbability;

        private readonly Dictionary<PID, object> _processedMessages = new Dictionary<PID, object>();
        /// <summary>
        /// 余额
        /// </summary>
        private decimal _balance = 10;
        private readonly Random _random;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="serviceUptime">服务时间</param>
        /// <param name="refusalProbability">拒绝概率</param>
        /// <param name="busyProbability">繁忙的概率</param>
        /// <param name="random"></param>
        public Account(string name, double serviceUptime, double refusalProbability, double busyProbability, Random random)
        {
            _name = name;
            _serviceUptime = serviceUptime;
            _refusalProbability = refusalProbability;
            _busyProbability = busyProbability;
            _random = random;
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                //贷方
                case Credit msg when _processedMessages.ContainsKey(msg.ReplyTo):
                    //msg.ReplyTo是PransferProcess的子Actor DebitAttempt
                    msg.ReplyTo.Tell(_processedMessages[msg.ReplyTo]);
                    return Actor.Done;
                //贷方
                case Credit msg:
                    return AdjustBalance(msg.ReplyTo, msg.Amount);
                //借方
                case Debit msg when _processedMessages.ContainsKey(msg.ReplyTo):
                    msg.ReplyTo.Tell(_processedMessages[msg.ReplyTo]);
                    return Actor.Done;
                //借方
                case Debit msg when msg.Amount + _balance >= 0:
                    return AdjustBalance(msg.ReplyTo, msg.Amount);
                //借方
                case Debit msg:
                    msg.ReplyTo.Tell(new InsufficientFunds());
                    break;
                //获取余额
                case GetBalance _:
                    context.Respond(_balance);
                    break;
            }
            return Actor.Done;
        }

      

        /// <summary>
        /// 调整帐户
        ///  we want to simulate the following 我们想模拟如下情况: 
        ///  * permanent refusals to process the message 永久拒绝处理消息
        ///  * temporary refusals to process the message 临时拒绝处理邮件
        ///  * failures before updating the balance 更新余额前的失败
        ///  * failures after updating the balance 更新余额后失败
        ///  * slow processing 处理速度慢
        ///  * successful processing 成功处理
        /// </summary>
        private Task AdjustBalance(PID replyTo, decimal amount)
        {
            #region
            //永久拒绝
            if (RefusePermanently())
            {
                _processedMessages.Add(replyTo, new Refused());
                replyTo.Tell(new Refused());
            }
            //繁忙
            if (Busy())
            {
                replyTo.Tell(new ServiceUnavailable());
            }

            // generate the behavior to be used whilst processing this message
            //生成处理此消息时要使用的行为
            var behaviour = DetermineProcessingBehavior();
            if (behaviour == Behavior.FailBeforeProcessing)
            {
                replyTo.Tell(new InternalServerError());
                return Actor.Done;
            }

            // simulate potential long-running process
            //模拟潜在的长期运行过程
            Thread.Sleep(_random.Next(0, 150));
            #endregion
            _balance += amount;
            _processedMessages.Add(replyTo, new OK());

            // simulate chance of failure after applying the change. This will force a retry of the operation which will test the operation is idempotent
            //模拟处理后失败，这将迫使引起重试操作，并测试幂等操作
            if (behaviour == Behavior.FailAfterProcessing)
            {
                //replyTo是PransferProcess的子Actor DebitAttempt
                replyTo.Tell(new InternalServerError());
                return Actor.Done;
            }

            replyTo.Tell(new OK());
            return Actor.Done;
        }
        /// <summary>
        /// 繁忙
        /// </summary>
        /// <returns></returns>
        private bool Busy()
        {
            var comparsion = _random.NextDouble() * 100;
            return comparsion <= _busyProbability;
        }
        /// <summary>
        /// 永久拒绝
        /// </summary>
        /// <returns></returns>
        private bool RefusePermanently()
        {
            var comparsion = _random.NextDouble() * 100;
            return comparsion <= _refusalProbability;
        }

        /// <summary>
        /// 确定处理行为
        /// </summary>
        /// <returns></returns>
        private Behavior DetermineProcessingBehavior()
        {
            var comparision = _random.NextDouble() * 100;
            if (comparision > _serviceUptime)
            {
                return _random.NextDouble() * 100 > 50 ? Behavior.FailBeforeProcessing : Behavior.FailAfterProcessing;
            }
            return Behavior.ProcessSuccessfully;
        }
     
        /// <summary>
        /// 处理结果
        /// </summary>
        private enum Behavior
        {
            /// <summary>
            /// 处理前失败
            /// </summary>
            FailBeforeProcessing,
            /// <summary>
            /// 处理后失败
            /// </summary>
            FailAfterProcessing,
            /// <summary>
            /// 处理成功
            /// </summary>
            ProcessSuccessfully
        }
    }


}

