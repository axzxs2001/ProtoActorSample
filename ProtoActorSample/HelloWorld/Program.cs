using Proto;
using System;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var props = Actor.FromProducer(() => new HelloActor());
            var pid = Actor.Spawn(props);
            pid.Tell(new Hello
            {
                Who = "Alex"
            });
            Console.ReadLine();
        }
    }

    class Hello
    {
        public string Who;
    }
    class HelloActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            var msg = context.Message;
            if (msg is Hello r)
            {
                Console.WriteLine($"Hello {r.Who}");
            }
            return Actor.Done;
        }
    }
}
