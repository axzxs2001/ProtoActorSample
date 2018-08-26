using Microsoft.Data.Sqlite;
using Proto;
using Proto.Persistence;
using Proto.Persistence.SnapshotStrategies;
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

            Console.WriteLine("1、事件溯源   2、快照   3、带快照的事件溯源");
            switch (Console.ReadLine())
            {
                case "1":
                    CallES(actorid, sqliteProvider);
                    break;
                case "2":
                    CallSN(actorid, sqliteProvider);
                    break;
                case "3":
                    CallES_SN(actorid, sqliteProvider);
                    break;
            }

        }
        /// <summary>
        /// 事件溯源
        /// </summary>
        /// <param name="actorid"></param>
        /// <param name="sqliteProvider"></param>
        private static void CallES(string actorid, SqliteProvider sqliteProvider)
        {
            var props = Actor.FromProducer(() => new ESDataActor(sqliteProvider, actorid));
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

        private static void CallSN(string actorid, SqliteProvider sqliteProvider)
        {
            var props = Actor.FromProducer(() => new SNDataActor(sqliteProvider, actorid));
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

        private static void CallES_SN(string actorid, SqliteProvider sqliteProvider)
        {
            var props = Actor.FromProducer(() => new ES_SNDataActor(sqliteProvider, sqliteProvider, actorid));
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
        public long Amount { get; set; }
    }

    #region 事件溯源
    public class ESDataActor : IActor
    {
        private long _value = 0;
        private readonly Persistence _persistence;

        public ESDataActor(IEventStore eventStore, string actorId)
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
                    await _persistence.PersistEventAsync(new Data { Amount = msg.Amount });
                    break;
            }
        }
    }
    #endregion

    #region 快照
    public class SNDataActor : IActor
    {
        private long _value = 0;
        private readonly Persistence _persistence;

        public SNDataActor(ISnapshotStore snapshotStore, string actorId)
        {
            _persistence = Persistence.WithSnapshotting(snapshotStore, actorId, ApplySnapshot);
        }
        private void ApplySnapshot(Proto.Persistence.Snapshot snapshot)
        {
            switch (snapshot.State)
            {
                case long value:
                    _value = value;
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
                    _value = _value + msg.Amount;
                    await _persistence.DeleteSnapshotsAsync(10);
                    await _persistence.PersistSnapshotAsync(_value);
                    break;
            }
        }
    }
    #endregion

    #region 事件溯源and快照
    public class ES_SNDataActor : IActor
    {
        private long _value = 0;
        private readonly Persistence _persistence;

        public ES_SNDataActor(IEventStore @event, ISnapshotStore snapshotStore, string actorId)
        {
            _persistence = Persistence.WithEventSourcingAndSnapshotting(@event, snapshotStore, actorId, ApplyEvent, ApplySnapshot, new IntervalStrategy(10), () => { return _value; });
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
        private void ApplySnapshot(Proto.Persistence.Snapshot snapshot)
        {
            switch (snapshot.State)
            {
                case long value:
                    _value = value;
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
                    await _persistence.PersistEventAsync(new Data { Amount = msg.Amount });
                    if (ShouldTakeSnapshot())
                    {
                        //await _persistence.DeleteSnapshotsAsync(10);
                        await _persistence.PersistSnapshotAsync(_value);
                    }
                    break;
            }
        }

        private bool ShouldTakeSnapshot()
        {
            return true;
        }
    }
    #endregion
}
