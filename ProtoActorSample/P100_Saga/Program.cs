using Proto;
using System;

namespace P100_Saga
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Starting");
            var numberOfTransfers = 20;
            var uptime = 59.99;
            var retryAttempts = 0;
            var refusalProbability = 0.31;
            var busyProbability = 0.81;

            var props = Actor.FromProducer(() => new Runner(numberOfTransfers, uptime, refusalProbability, busyProbability, retryAttempts, false))
                .WithChildSupervisorStrategy(new OneForOneStrategy((pid, reason) => SupervisorDirective.Restart, retryAttempts, null));

            Console.WriteLine("Spawning runner");
            var runner = Actor.SpawnNamed(props, "runner");

            Console.ReadLine();
            runner.Stop();

        }
    }
}
