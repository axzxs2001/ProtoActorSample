using Microsoft.Data.Sqlite;
using Proto;
using Proto.Persistence;
using Proto.Persistence.Sqlite;
using System;
using System.Threading.Tasks;

namespace P008_Persistence
{
    class Program
    {
        static void Main(string[] args)
        {
            //用sqlite持久化后
            var actorid = "myactorid";
            var dbfile = @"C:\MyFile\Source\Repos\ProtoActorSample\ProtoActorSample\P008_Persistence\data.sqlite";
            var sqliteProvider = new SqliteProvider(new SqliteConnectionStringBuilder() { DataSource = dbfile });
            var props = Actor.FromProducer(() => new DataActor(sqliteProvider, actorid));
            var pid = Actor.Spawn(props);

            var result = true;
            while (result)
            {
                Console.WriteLine("1、Tell  2、删除持久化  3、退出");

                switch (Console.ReadLine())
                {
                    case "1":
                        var random = new Random();
                        var no = random.Next(5, 15);
                        Console.WriteLine(no);
                        pid.Tell(new Data { Amount = no });
                        break;
                    case "2":
                        //完成处理后清理持久化的操作          
                        sqliteProvider.DeleteEventsAsync(actorid, 10).Wait();
                        break;
                    case "3":
                        result = false;
                        break;
                }
            }
            sqliteProvider.DeleteEventsAsync(actorid, 10).Wait();
            

        }
    }
    public class Data
    {
        public int Amount { get; set; }
    }
    public class DataActor : IActor
    {
        private int _value = 0;
        private readonly Persistence _persistence;

        public DataActor(IEventStore eventStore, string actorId)
        {
            _persistence = Persistence.WithEventSourcing(eventStore, actorId, ApplyEvent);
        }
        private void ApplyEvent(Proto.Persistence.Event @event)
        {
            switch (@event.Data)
            {
                case Data msg:
                    _value = _value + msg.Amount;
                    Console.WriteLine($"累计：{_value}");
                    break;
            }
        }
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    await _persistence.RecoverStateAsync();
                    break;
                case Data msg:
                    if (msg.Amount > 7)
                    {
                        await _persistence.PersistEventAsync(new Data { Amount = msg.Amount });
                    }
                    break;
            }
        }
    }
}
