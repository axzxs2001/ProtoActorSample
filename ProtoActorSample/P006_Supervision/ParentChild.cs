using Proto;
using System;

namespace P006_Supervision
{
    class ParentChild
    {
        static void Main(string[] args)
        {
            var childProps = Actor.FromFunc(context =>
            {
                Console.WriteLine($"       子 Actor {context.Self.Id}: MSG: {context.Message.GetType()}");
                switch (context.Message)
                {
                    case Started _:
                        throw new Exception("child failure");
                }
                return Actor.Done;
            });

            var rootProps = Actor.FromFunc(context =>
            {
                Console.WriteLine($"父 Actor {context.Self.Id}: MSG: {context.Message.GetType()}");
                switch (context.Message)
                {
                    case Started _:
                        context.SpawnNamed(childProps, "child");
                        break;
                    case Terminated terminated:
                        Console.WriteLine($"父 终止 {terminated.Who}");
                        break;
                }
                return Actor.Done;
            })
            .WithChildSupervisorStrategy(new OneForOneStrategy((pid, reason) => SupervisorDirective.Restart, 8, TimeSpan.FromSeconds(1)));

            Actor.SpawnNamed(rootProps, "root");

            Console.ReadLine();
        }
    }
}
