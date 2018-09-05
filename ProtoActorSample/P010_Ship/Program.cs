using P010_Libs;
using Proto;
using Proto.Remote;
using Proto.Serialization.Wire;
using System;

namespace P010_Ship
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "服务端";
            //设置序列化类型并注册
            var wire = new WireSerializer(new[] { typeof(Ship) });
            Serialization.RegisterSerializer(wire, true);

            var props = Actor.FromProducer(() => new ShipActor());
            //注册一个为hello类别的          
            Remote.RegisterKnownKind("ship", props);
            //服务端监控端口5001
            Remote.Start("127.0.0.1", 5001);
            Console.WriteLine("服务端开始……");
            Console.ReadLine();
        }
    }



}
