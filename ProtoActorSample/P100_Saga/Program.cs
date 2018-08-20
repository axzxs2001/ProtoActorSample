using Proto;
using System;

namespace P100_Saga
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Starting");
                var numberOfTransfers = 1;
                var uptime =  99.99;
                var retryAttempts = 0;
                var refusalProbability = 0;// 0.01;
                var busyProbability = 0;// 0.01;

                var props = Actor.FromProducer(() => new Runner(numberOfTransfers, uptime, refusalProbability, busyProbability, retryAttempts, false))
                    .WithChildSupervisorStrategy(new OneForOneStrategy((pid, reason) => SupervisorDirective.Restart, retryAttempts, null));

                Console.WriteLine("Spawning runner");
                var runner = Actor.SpawnNamed(props, "runner");
               
                Console.ReadLine();
                runner.Stop();
            }
        }
    }
}
