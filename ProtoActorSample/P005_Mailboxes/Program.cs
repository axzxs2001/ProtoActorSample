using Proto;
using Proto.Mailbox;
using System;
using System.Threading.Tasks;

namespace P005_Mailboxes
{
    class Program
    {
        static void Main(string[] args)
        {
            var props = new Props()
                // 用道具代理返回一个IActor实例
                .WithProducer(() => new MyActor())
                //默认邮箱使用无界队列
                .WithMailbox(() => UnboundedMailbox.Create(new MyMailboxStatistics()))
                // 默认的 spawner 构造  Actor, Context 和 Process
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
    public class MyMailboxStatistics : IMailboxStatistics
    {
        public void MailboxEmpty()
        {            
            Console.WriteLine("邮箱MailboxEmpty");
        }

        public void MailboxStarted()
        {
            Console.WriteLine("邮箱MailboxStarted");
        }

        public void MessagePosted(object message)
        {
            Console.WriteLine("邮箱MessagePosted:"+message);
        }

        public void MessageReceived(object message)
        {
            Console.WriteLine("邮箱MessageReceived:"+message);
        }
    }
}
