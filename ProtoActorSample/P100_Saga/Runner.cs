using P100_Saga.Messages;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P100_Saga
{
    /// <summary>
    /// 运行Actor
    /// </summary>
    public class Runner : IActor
    {
        /// <summary>
        /// 迭代次数
        /// </summary>
        private readonly int _numberOfIterations;
        /// <summary>
        /// 服务时间
        /// </summary>
        private readonly double _uptime;
        /// <summary>
        /// 拒绝概率
        /// </summary>
        private readonly double _refusalProbability;

        /// <summary>
        /// 繁忙概率
        /// </summary>
        private readonly double _busyProbability;
        /// <summary>
        /// 重试
        /// </summary>
        private readonly int _retryAttempts;
        /// <summary>
        /// 输入事件流
        /// </summary>
        private readonly bool _outputEventStream;
        /// <summary>
        /// 
        /// </summary>
        private readonly HashSet<PID> _transfers = new HashSet<PID>();
        /// <summary>
        /// 
        /// </summary>
        private int _successResults;
        /// <summary>
        /// 失败不一致结果
        /// </summary>
        private int _failedAndInconsistentResults;
        /// <summary>
        /// 失败一致结果
        /// </summary>
        private int _failedButConsistentResults;
        /// <summary>
        /// 未知结果
        /// </summary>
        private int _unknownResults;
        /// <summary>
        /// 内存持久化提供者
        /// </summary>
        private InMemoryProvider _inMemoryProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfIterations">迭代次数</param>
        /// <param name="uptime">服务时间</param>
        /// <param name="refusalProbability">拒绝概率</param>
        /// <param name="busyProbability">繁忙概率</param>
        /// <param name="retryAttempts">重试</param>
        /// <param name="outputEventStream">输入事件流</param>
        public Runner(int numberOfIterations, double uptime, double refusalProbability, double busyProbability, int retryAttempts, bool outputEventStream)
        {
            _numberOfIterations = numberOfIterations;
            _uptime = uptime;
            _refusalProbability = refusalProbability;
            _busyProbability = busyProbability;
            _retryAttempts = retryAttempts;
            _outputEventStream = outputEventStream;
        }
        /// <summary>
        /// 创建帐户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private PID CreateAccount(string name, Random random)
        {
            var accountProps = Actor.FromProducer(() => new Account(name, _uptime, _refusalProbability, _busyProbability, random));
            return Actor.SpawnNamed(accountProps, name);
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case SuccessResult msg:
                    _successResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case UnknownResult msg:
                    _unknownResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case FailedAndInconsistent msg:
                    _failedAndInconsistentResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case FailedButConsistentResult msg:
                    _failedButConsistentResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case Started _:
                    var random = new Random();
                    _inMemoryProvider = new InMemoryProvider();

                    for (int i = 1; i <= _numberOfIterations; i++)
                    {
                        int j = i;
                        var fromAccount = CreateAccount($"FromAccount{j}", random);
                        var toAccount = CreateAccount($"ToAccount{j}", random);

                        var transferProps = Actor.FromProducer(() => new TransferProcess(fromAccount, toAccount, 10m, _inMemoryProvider, $"Transfer Process {j}", random, _uptime))
                            .WithChildSupervisorStrategy(
                                new OneForOneStrategy((pid, reason) => SupervisorDirective.Restart, _retryAttempts,
                                    null));

                        var transfer = context.SpawnNamed(transferProps, $"Transfer Process {j}");
                        _transfers.Add(transfer);

                        if (_numberOfIterations >= 10)
                        {
                            if (j % (_numberOfIterations / 10) == 0)
                                Console.WriteLine($"Started {j}/{_numberOfIterations} processes");
                        }
                        else
                        {
                            Console.WriteLine($"Started {j}/{_numberOfIterations} processes");
                        }
                    }
                    break;
            }
            return Actor.Done;
        }

        private void CheckForCompletion(PID pid)
        {
            _transfers.Remove(pid);

            var remaining = _transfers.Count;
            if (_numberOfIterations >= 10)
            {
                Console.Write(".");
                if (remaining % (_numberOfIterations / 10) == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{remaining} processes remaining");
                }
            }
            else
            {
                Console.WriteLine($"{remaining} processes remaining");
            }

            if (remaining == 0)
            {
                Thread.Sleep(250);
                Console.WriteLine();
                Console.WriteLine(
                    $"RESULTS for {_uptime}% uptime, {_refusalProbability}% chance of refusal, {_busyProbability}% of being busy and {_retryAttempts} retry attempts:");
                Console.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _successResults)}% ({_successResults}/{_numberOfIterations}) successful transfers");
                Console.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _failedButConsistentResults)}% ({_failedButConsistentResults}/{_numberOfIterations}) failures leaving a consistent system");
                Console.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _failedAndInconsistentResults)}% ({_failedAndInconsistentResults}/{_numberOfIterations}) failures leaving an inconsistent system");
                Console.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _unknownResults)}% ({_unknownResults}/{_numberOfIterations}) unknown results");

                if (_outputEventStream)
                {
                    foreach (var stream in _inMemoryProvider.Events)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Event log for {stream.Key}");
                        foreach (var @event in stream.Value)
                        {
                            Console.WriteLine(@event.Value);
                        }
                    }
                }
            }
        }

        private double AsPercentage(double numberOfIterations, double results)
        {
            return (results / numberOfIterations) * 100;
        }
    }
}
