using P009_Lib;
using Proto;
using Proto.Remote;
using Proto.Serialization.Wire;
using System;

using System.Threading;
using System.Threading.Tasks;

namespace P009_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "服务端";
            Console.WriteLine("回车开始");
            Console.ReadLine();
            //设置序列化类型
            var wire = new WireSerializer(new[] { typeof(HelloRequest), typeof(HelloResponse) });
            Serialization.RegisterSerializer(wire, true);

            var props = Actor.FromProducer(() => new HelloQuestActor());         
            Remote.RegisterKnownKind("hello", props);
            Remote.Start("127.0.0.1", 12000);
            Console.WriteLine("服务端开始……");
            Console.ReadLine();
        }
    }

    class HelloQuestActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case HelloRequest msg:
                    Console.WriteLine(msg.Message);
                    context.Respond(new HelloResponse
                    {
                        Message = "回应：我是服务端",
                    });
                    break;
            }
            return Actor.Done;
        }
    }

}
