using Proto;
using System;
using System.Threading.Tasks;

namespace P003_SpawningActors
{
    class Program
    {
        static void Main(string[] args)
        {
            var props = Actor.FromProducer(() => new MyActor());

            //产生一个自定义名称的PID
            var pid1 = Actor.Spawn(props);
            pid1.Tell(new MyEntity { ID = 1 });

            //产生一个有gsw前缀，跟自动生成的名称的PID
            var pid2 = Actor.SpawnPrefix(props, "gsw");
            pid2.Tell(new MyEntity { ID = 2 });

            //产生一个名称为gswpid的PID
            var pid3 = Actor.SpawnPrefix(props, "gswpid");
            pid3.Tell(new MyEntity { ID = 3 });
            Console.ReadLine();
        }
    }

    public class MyActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is MyEntity myEntity)
            {

                Console.WriteLine($"*********************{context.Self.Id}********************");
                Console.WriteLine(myEntity.ID);
                var cldProps = Actor.FromProducer(() => new MyChildActor());

                var pidCld1 = context.Spawn(cldProps);
                pidCld1.Tell(new MyChildEntity { Message = "1 message,myEntity.ID=" + myEntity.ID });

                var pidCld2 = context.SpawnPrefix(cldProps, "gswCld");
                pidCld2.Tell(new MyChildEntity { Message = "2 message,myEntity.ID=" + myEntity.ID });

                var pidCld3 = context.SpawnNamed(cldProps, "gswCldPid");
                pidCld3.Tell(new MyChildEntity { ID = 3, Message = "3 message,myEntity.ID=" + myEntity.ID });


            }
            return Actor.Done;
        }
    }
    public class MyChildActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is MyChildEntity myChildEntity)
            {              
                Console.WriteLine($"----------------{context.Self.Id}------------------");
                Console.WriteLine(myChildEntity.Message);
            }
            return Actor.Done;
        }
    }
    public class MyEntity
    {
        public int ID { get; set; }
    }
    public class MyChildEntity
    {
        public string Message { get; set; }
        public int ID { get; set; }
    }
}
