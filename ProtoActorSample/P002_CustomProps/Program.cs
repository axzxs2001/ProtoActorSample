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
                //用道具代理返回一个IActor实例
                .WithProducer(() => new MyActor())
                //默认调度器用线程池，邮箱中最多300个消息吞吐量
                .WithDispatcher(new ThreadPoolDispatcher { Throughput = 300 })
                //默认邮箱使用无界队列
                .WithMailbox(() => UnboundedMailbox.Create())
                //默认策略在10秒的窗口内最多重新启动子Actor 10次
                .WithChildSupervisorStrategy(new OneForOneStrategy((who, reason) =>
                SupervisorDirective.Restart, 10, TimeSpan.FromSeconds(10)))
                //可以将中间件链接起来以拦截传入和传出消息 
                //接收中间件在Actor接收消息之前被调用
                //发送者中间件在消息发送到目标PID之前被调用
                .WithReceiveMiddleware(
                next => async c =>
                {
                    Console.WriteLine($"Receive中间件 1 开始，{c.Message.GetType()}:{c.Message}");
                    await next(c);
                    Console.WriteLine($"Receive中间件 1 结束，{c.Message.GetType()}:{c.Message}");
                },
                next => async c =>
                {
                    Console.WriteLine($"Receive中间件 2 开始，{c.Message.GetType()}:{c.Message}");
                    await next(c);
                    Console.WriteLine($"Receive中间件 2 结束，{c.Message.GetType()}:{c.Message}");
                })
                .WithSenderMiddleware(
                next => async (c, target, envelope) =>
                {
                    Console.WriteLine($"Sender中间件 1 开始, {c.Message.GetType()}:{c.Message}");
                    await next(c, target, envelope);
                    Console.WriteLine($"Sender中间件 1 结束，{c.Message.GetType()}:{c.Message}");
                },
                next => async (c, target, envelope) =>
                {
                    Console.WriteLine($"Sender中间件 2 开始，{c.Message.GetType()}:{c.Message}");
                    await next(c, target, envelope);
                    Console.WriteLine($"Sender中间件 2 结束，{c.Message.GetType()}:{c.Message}");
                })
                // 默认的 spawner 构造  Actor, Context 和 Process
                .WithSpawner(Props.DefaultSpawner);

            //从props衍生pid，pid代理一个actor的地址
            var pid = Actor.Spawn(props);       
            //把Hello对象交给HelloActor处理
            pid.Tell(new MyEntity
            {
                Message = "我是MyEntity的Message，请求"
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
                context.Tell(context.Sender, new MyEntity() { Message = "我是MyEntity的Message,应答" });
            }
            return Actor.Done;
        }
    }
    public class MyEntity
    {
        public string Message { get; set; }
    }
}
