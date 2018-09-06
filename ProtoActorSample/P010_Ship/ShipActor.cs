using P010_Libs;
using Proto;
using System;
using System.Threading.Tasks;

namespace P010_Ship
{
    class ShipActor : IActor
    {

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started started:
                    Console.WriteLine("shipping……");
                    break;
                case Ship  ship:
                    Console.WriteLine($"处理：{ship}");
                    context.Respond(true);
                    break;
            }
            return Actor.Done;
        }

    }
}
