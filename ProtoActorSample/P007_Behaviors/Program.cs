using Proto;
using System;
using System.Threading.Tasks;

namespace P007_Behaviors
{
    class Program
    {
        static void Main(string[] args)
        {
            var props = Actor.FromProducer(() => new LightBulb());
            var pid = Actor.Spawn(props);
            while (true)
            {
                Console.WriteLine("----------------------------");
                Console.WriteLine("按开关");
                Console.ReadLine();
                var message = pid.RequestAsync<string>(new PressSwitch()).Result;
                Console.WriteLine(message);

                message = pid.RequestAsync<string>(new Touch()).Result;
                Console.WriteLine(message);
            }
        }
    }

    public class LightBulb : IActor
    {
        private readonly Behavior _behavior;

        public LightBulb()
        {
            _behavior = new Behavior();
            //把Off方法放入栈
            _behavior.BecomeStacked(Off);
        }
        public Task ReceiveAsync(IContext context)
        {
            //切换到behavior指定的方法,来充当ReceiveAsync
            return _behavior.ReceiveAsync(context);
        }
        /// <summary>
        /// 关
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task Off(IContext context)
        {
            switch (context.Message)
            {
                case PressSwitch _:
                    context.Respond("打开");
                    _behavior.Become(On);
                    break;
                case Touch _:
                    context.Respond("凉的");
                    break;
            }
            return Actor.Done;
        }
        /// <summary>
        /// 开
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task On(IContext context)
        {
            switch (context.Message)
            {
                case PressSwitch _:
                    context.Respond("关闭");
                    _behavior.Become(Off);
                    break;
                case Touch _:
                    context.Respond("烫手");
                    break;
            }
            return Actor.Done;
        }
    }
    class PressSwitch
    { }
    class Touch
    { }

}
