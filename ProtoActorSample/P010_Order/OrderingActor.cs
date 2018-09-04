using P010_Libs;
using Proto;
using System;
using System.Threading.Tasks;

namespace P010_Order
{
    public class OrderingActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {

            context.Parent.Tell(new Ship { Address = "aaabbbccc", Mobile = "025-5868-552", Shiptime = DateTime.Now });
            return Actor.Done;
        }
    }
}