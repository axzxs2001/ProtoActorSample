using P009_Lib;
using Proto;
using Proto.Remote;
using Proto.Serialization.Wire;
using System;
using System.Threading.Tasks;

namespace P009_Client
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.Title = "客户端";
            Console.WriteLine("回车开始");
            Console.ReadLine();
            //设置序列化类型并注册
            var wire = new WireSerializer(new[] { typeof(HelloRequest), typeof(HelloResponse) });
            Serialization.RegisterSerializer(wire, true);
            //设置自己监控端口5002
            Remote.Start("127.0.0.1", 5002);
            //连接服务端5001
            var pid = Remote.SpawnNamedAsync("127.0.0.1:5001", "clientActor", "hello", TimeSpan.FromSeconds(50)).Result.Pid;
            while (true)
            {
                var res = pid.RequestAsync<HelloResponse>(new HelloRequest { Message = $"请求：我是客户端 【{DateTime.Now}】" }).Result;
                Console.WriteLine(res.Message);
                Console.ReadLine();
            }
        }
    }
}
