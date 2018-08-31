using Proto;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace P006_Supervision
{
    class Other
    {
        static void Main(string[] args)
        {


            var props = Actor.FromProducer(() => new ParentActor()).WithChildSupervisorStrategy(new OneForOneStrategy(Decider.Decide, 10, TimeSpan.FromSeconds(10)));
            var pid = Actor.Spawn(props);

            var sn = 1;
            while (true)
            {
                Console.WriteLine($"第{sn++}次");
                Console.ReadLine();
                pid.Request(new Hello { Who = "父亲", Value = 1 }, pid);

                Console.ReadLine();
                pid.Request(new Recoverable(), pid);
                // Console.ReadLine();
                // pid.Request(new Fatal(), pid);
            }

        }
    }

    internal class Decider
    {
        public static SupervisorDirective Decide(PID pid, Exception reason)
        {
            Console.WriteLine(reason.Message);
            switch (reason)
            {
                case RecoverableException _:
                    return SupervisorDirective.Restart;
                case FatalException _:
                    return SupervisorDirective.Stop;
                default:
                    return SupervisorDirective.Escalate;

            }
        }
    }

    internal class ParentActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {

            PID childPid;
            if (context.Children == null || context.Children.Count == 0)
            {
                var props = Actor.FromProducer(() => new ChildActor());
                childPid = context.Spawn(props);
            }
            else
            {
                childPid = context.Children.First();
            }


            switch (context.Message)
            {
                case Hello hello:
                    Console.WriteLine($"Hello {hello.Who }");
                    context.Request(childPid, new Hello { Who = "儿子", Value = 100 });
                    break;
                case Recoverable _:
                    context.Request(childPid, new Recoverable());
                    break;
                case Fatal _:

                    context.Request(childPid, new Fatal());
                    break;
                case Terminated r:
                    Console.WriteLine($"Watched actor was Terminated, {r.Who}");
                    break;
            }

            return Actor.Done;
        }
    }

    internal class ChildActor : IActor
    {

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Hello hello:
                    Console.WriteLine($"Hello {hello.Who}");
                    break;
                case Recoverable _:
                    Console.WriteLine("异常：RecoverableException");
                    throw new RecoverableException("RecoverableException有异常");
                case Fatal _:
                    Console.WriteLine("异常：FatalException");
                    throw new FatalException("RecoverableException有异常");
                case Started _:
                    Console.WriteLine("Started, child actor中");
                    break;
                case Stopping _:
                    Console.WriteLine("Stopping，child actor中");
                    break;
                case Stopped _:
                    Console.WriteLine("Stopped, child actor中");
                    break;
                case Restarting _:
                    Console.WriteLine("Restarting, child actor中");
                    break;
            }
            return Actor.Done;
        }
    }

    internal class Hello
    {
        public string Who;
        public int Value;
    }
    internal class RecoverableException : Exception
    {
        public RecoverableException(string message) : base(message) { }
    }
    internal class FatalException : Exception
    {
        public FatalException(string message) : base(message) { }
    }
    internal class Fatal { }
    internal class Recoverable { }
}
