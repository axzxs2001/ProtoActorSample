using Proto;
using Proto.Mailbox;
using System;
using System.Threading.Tasks;

namespace P002_CustomProps
{
    class Program
    {
        static void Main(string[] args)
        {
            var props = new Props()
                // the producer is a delegate that returns a new instance of an IActor
                .WithProducer(() => new MyActor())
                // the default dispatcher uses the thread pool and limits throughput to 300 messages per mailbox run
                .WithDispatcher(new ThreadPoolDispatcher { Throughput = 300 })
                // the default mailbox uses unbounded queues
                .WithMailbox(() => UnboundedMailbox.Create())
                // the default strategy restarts child actors a maximum of 10 times within a 10 second window
                .WithChildSupervisorStrategy(new OneForOneStrategy((who, reason) =>
                SupervisorDirective.Restart, 10, TimeSpan.FromSeconds(10)))
                // middlewares can be chained to intercept incoming and outgoing messages
                // receive middlewares are invoked before the actor receives the message
                // sender middlewares are invoked before the message is sent to the target PID
                .WithReceiveMiddleware(
                next => async c =>
                {
                    Console.WriteLine($"middleware 1 enter {c.Message.GetType()}:{c.Message}");
                    await next(c);
                    Console.WriteLine($"middleware 1 exit");
                },
                next => async c =>
                {
                    Console.WriteLine($"middleware 2 enter {c.Message.GetType()}:{c.Message}");
                    await next(c);
                    Console.WriteLine($"middleware 2 exit");
                })
                .WithSenderMiddleware(
                next => async (c, target, envelope) =>
                {
                    Console.WriteLine($"middleware 1 enter {c.Message.GetType()}:{c.Message}");
                    await next(c, target, envelope);
                    Console.WriteLine($"middleware 1 enter {c.Message.GetType()}:{c.Message}");
                },
                next => async (c, target, envelope) =>
                {
                    Console.WriteLine($"middleware 2 enter {c.Message.GetType()}:{c.Message}");
                    await next(c, target, envelope);
                    Console.WriteLine($"middleware 2 enter {c.Message.GetType()}:{c.Message}");
                })
                // the default spawner constructs the Actor, Context and Process
                .WithSpawner(Props.DefaultSpawner);

            //从props衍生pid，pid代理一个actor的地址
            var pid = Actor.Spawn(props);
            //把Hello对象交给HelloActor处理
            pid.Tell(new MyEntity
            {
                Message = "this is message"
            });
            Console.ReadLine();
        }
    }

    public class MyActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is MyEntity myEntity)
            {
                Console.WriteLine(myEntity.Message);
            }
            return Actor.Done;
        }
    }

    public class MyEntity
    {
        public string Message { get; set; }
    }
}
