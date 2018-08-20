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
            var message = pid.RequestAsync<string>(new PressSwitch()).Result;
            Console.WriteLine(message);
            message = pid.RequestAsync<string>(new Touch()).Result;
            Console.WriteLine(message);
            message = pid.RequestAsync<string>(new PressSwitch()).Result;
            Console.WriteLine(message);
            message = pid.RequestAsync<string>(new Touch()).Result;
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }

    public class LightBulb : IActor
    {
        private readonly Behavior _behavior;

        public LightBulb()
        {
            _behavior = new Behavior();
            _behavior.Become(Off);
        }  
        public Task ReceiveAsync(IContext context)
        {
            return _behavior.ReceiveAsync(context);
        }
        private Task Off(IContext context)
        {
            switch (context.Message)
            {
                case PressSwitch _:
                    context.Respond("Turning on");
                    _behavior.Become(On);
                    break;
                case Touch _:
                    context.Respond("Cold");
                    break;
            }
            return Actor.Done;
        }
        private Task On(IContext context)
        {
            switch (context.Message)
            {
                case PressSwitch _:
                    context.Respond("Turning off");
                    _behavior.Become(Off);
                    break;
                case Touch _:
                    context.Respond("Hot!");
                    break;
            }
            return Actor.Done;
        }
        private Task Smashed(IContext context)
        {
            switch (context.Message)
            {
                case PressSwitch _:
                    context.Respond(""); // nothing happens!
                    break;
                case Touch _:
                    context.Respond("Owwww!");
                    break;
                case ReplaceBulb _:
                    _behavior.Become(Off);
                    break;
            }
            return Actor.Done;
        }
    }
    class PressSwitch
    {}
    class Touch
    {}
    class ReplaceBulb
    {}
}
