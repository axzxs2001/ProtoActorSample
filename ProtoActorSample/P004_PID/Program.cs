using Proto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace P004_PID
{
    class Program
    {
        static void Main(string[] args)
        {
            var props = Actor.FromProducer(() => new MyActor()); 
            var pid = Actor.Spawn(props);           
            while (true)
            {
                Console.WriteLine("**************************************");
                Console.WriteLine("1、单向请求Tell  2、单向请求Request  3、双向请求RequestAsync");
                switch (Console.ReadLine())
                {
                    case "1":
                        Console.WriteLine("单向请求开始");
                        pid.Tell(new Request { Name = "单向请求 Tell", RequestType = "one-way", Time = DateTime.Now });
                        break;
                    case "2":
                        Console.WriteLine("单向请求开始");
                        //无法接回应签，与官网说法不一
                        pid.Request(new Request { Name = "单向请求 Request", RequestType = "two-way-1", Time = DateTime.Now }, pid);
                        
                        break;
                    case "3":
                        Console.WriteLine("双向请求开始");
                        var response = pid.RequestAsync<Response>(new Request { Name = "双向请求 RequestAsync", RequestType = "two-way-2", Time = DateTime.Now }).Result;
                        Console.WriteLine(response.Time + ":" + response.Name);
                        break;
                }
                Thread.Sleep(2000);
            }
        }
    }

    public class MyActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {

            if (context.Message is Request request)
            {
                switch (request.RequestType)
                {
                    case "one-way"://context.Sender为null
                        Console.WriteLine("接收到：" + request.RequestType + "," + request.Time + ":" + request.Name);
                        break;
                    case "two-way-1"://context.Sender= context.Self为自己
                        Console.WriteLine("接收到：" + request.RequestType + "," + request.Time + ":" + request.Name);                      
                        context.Respond(new Response() { Time = DateTime.Now, Name = "服务端应答 two-way-1" });                    
                        break;
                    case "two-way-2"://context.Sender!= context.Self为新实例
                        Console.WriteLine("接收到：" + request.RequestType + "," + request.Time + ":" + request.Name);                       
                        context.Respond(new Response() { Time = DateTime.Now, Name = "服务端应答 two-way-2" });
                        break;
                }
            }
            return Actor.Done;
        }
    }

    public class Request
    {
        public string Name
        { get; set; }
        public string RequestType
        { get; set; }
        public DateTime Time
        { get; set; }
    }

    public class Response
    {
        public string Name
        { get; set; }
        public DateTime Time
        { get; set; }
    }
}
