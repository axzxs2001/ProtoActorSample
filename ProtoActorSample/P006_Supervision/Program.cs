using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P006_Supervision
{
    class Program
    {
        static void Main(string[] args)
        {
            //设置监督策略，指向SupervisiorMode方法，重启次数为3，两次之间间隔1s
            var props = Actor.FromProducer(() => new ShopingCatActor()).WithChildSupervisorStrategy(new OneForOneStrategy(SupervisorMode.Decide, 3, TimeSpan.FromSeconds(1)));
            var pid = Actor.Spawn(props);
            var user = new User { UserName = "gsw" };

            var sn = 1;
            while (true)
            {
                Console.WriteLine($"{sn++}--------------------begin-----------------");
                foreach (var goods in user.ShopingCat.Goodses)
                {
                    Console.WriteLine(goods);
                }
                Console.WriteLine("---------------------end------------------");
                Console.ReadLine();
                pid.Request(user, pid);
            }
        }
    }
    /// <summary>
    /// Supervisor 模式
    /// </summary>
    class SupervisorMode
    {
        /// <summary>
        /// 用来处理异常发生的后续操作
        /// </summary>
        /// <param name="pid">PID</param>
        /// <param name="reason">异常原因，Exception类型</param>
        /// <returns></returns>
        public static SupervisorDirective Decide(PID pid, Exception reason)
        {
            Console.WriteLine("   异常发生：" + reason.Message + "   " + pid);
            switch (reason)
            {
                //重新开始的异常
                case RecoverableException _:
                    return SupervisorDirective.Restart;
                //停止异常
                case FatalException _:
                    return SupervisorDirective.Stop;
                //其他都上报
                default:
                    return SupervisorDirective.Escalate;
            }
        }
    }
    /// <summary>
    /// 购物车actor
    /// </summary>
    class ShopingCatActor : IActor
    {
        ShopingCat _shopingCat;
        public ShopingCatActor()
        {
            _shopingCat = new ShopingCat();
            Console.WriteLine("*******************actor ShopingCatActor************************");
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
                case User user:
                    childPid.Request(_shopingCat, childPid);
                    user.ShopingCat = _shopingCat;
                    break;
            }
            return Actor.Done;
        }
    }
    /// <summary>
    /// 商品actor
    /// </summary>
    class GoodsActor : IActor
    { 
        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case ShopingCat shopingCat:

                    var goods = new Goods { Name = "红茶", Price = 3.0m, Describe = "统一" };
                    //用来随机产生异常
                    var random = new Random();
                    goods.Quantity = random.Next(1, 3) - 1;
                    if (goods.Quantity <= 0)
                    {
                        throw new RecoverableException("数量不能小于等于0");
                    }
                    else
                    {
                        shopingCat.Goodses.Add(goods);
                        Console.WriteLine($"添加 {goods} 到购物车里");
                    }
                    break;
            }
            return Actor.Done;
        }
    }
    /// <summary>
    /// 用户
    /// </summary>
    class User
    {
        public ShopingCat ShopingCat { get; set; } = new ShopingCat();
        public string UserName { get; set; }
    }
    /// <summary>
    /// 购物车
    /// </summary>
    class ShopingCat
    {
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
            return $"Name={Name}，Quantity={Quantity}，Price={Price}，Describe={Describe}";
        }
    }

}
