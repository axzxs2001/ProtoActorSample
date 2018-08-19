using Proto;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace P006_Supervision
{
    class Program
    {
        static void Main(string[] args)
        {


            var props = Actor.FromProducer(() => new ShopingCatActor()).WithChildSupervisorStrategy(new OneForOneStrategy(SupervisorMode.Decide, 10, TimeSpan.FromSeconds(10)));
            var pid = Actor.Spawn(props);

            var order = new Order();

            var sn = 1;
            while (true)
            {
                Console.WriteLine($"第{sn++}次");
                Console.ReadLine();
                pid.Request(order, pid);

            }

        }
    }
    class SupervisorMode
    {
        public static SupervisorDirective Decide(PID pid, Exception reason)
        {
            Console.WriteLine(reason.Message);
            switch (reason)
            {
                case RecoverableException _:
                    return SupervisorDirective.Restart;
                case FatalException _:
                    return SupervisorDirective.Stop;
                default:
                    return SupervisorDirective.Escalate;

            }
        }
    }

    class ShopingCatActor : IActor
    {
        ShopingCat _shopingCat;
        public ShopingCatActor()
        {
            _shopingCat = new ShopingCat();
            _shopingCat.UserName = "gsw";

        }
        public Task ReceiveAsync(IContext context)
        {
            PID childPid;
            if (context.Children == null || context.Children.Count == 0)
            {
                var props = Actor.FromProducer(() => new GoodsActor());
                childPid = context.Spawn(props);
            }
            else
            {
                childPid = context.Children.First();
            }
            switch (context.Message)
            {
                case Order order:
                    childPid.Request(_shopingCat, childPid);
                    order.ShopingCat = _shopingCat;
                    break;
            }
            return Actor.Done;
        }
    }
    class GoodsActor : IActor
    {

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case ShopingCat shopingCat:
                    var goods = new Goods { Name = "红茶", Price = 3.0m, Describe = "统一" };
                    var random = new Random();
                    goods.Quantity = random.Next(1, 3) - 1;

                    Console.WriteLine(goods.Quantity);
                    if (goods.Quantity <= 0)
                    {
                        throw new RecoverableException("数量不能小于等于0");
                    }
                    else
                    {
                        shopingCat.Goodses.Add(goods);
                        Console.WriteLine("添加商品到购物车里");
                    }
                    break;
            }
            return Actor.Done;
        }
    }

    class Order
    {
        public ShopingCat ShopingCat { get; set; }

        public Goods Goods { get; set; }
    }
    class ShopingCat
    {
        public string UserName { get; set; }
        public List<Goods> Goodses
        { get; set; } = new List<Goods>();
    }
    /// <summary>
    /// 商品
    /// </summary>
    class Goods
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string Describe { get; set; }
        public override string ToString()
        {
            return $"Name={Name},Quantity={Quantity}";
        }
    }

}
