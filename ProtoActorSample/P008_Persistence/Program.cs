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
            while (true)
            {
                Console.WriteLine("1、事件溯源   2、快照   3、带快照的事件溯源  4、退出");
                switch (Console.ReadLine())
                {
                    case "1":
                        CallEventSource(actorid, sqliteProvider);
                        break;
                    case "2":
                        CallSnapShoot(actorid, sqliteProvider);
                        break;
                    case "3":
                        CallSnapShootEventSource(actorid, sqliteProvider);
                        break;
                    case "4":
                        return;
                }
            }
        }
        /// <summary>
        /// 事件溯源
        /// </summary>
        /// <param name="actorid"></param>
        /// <param name="sqliteProvider"></param>
        private static void CallEventSource(string actorid, SqliteProvider sqliteProvider)
        {          
            var props = Actor.FromProducer(() => new EventSourceDataActor(sqliteProvider, actorid));
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
                        Console.WriteLine($"随机产生的数字：{no}");
                        pid.Tell(new Data { Amount = no });
                        break;
                    case "2":
                        //完成处理后清理持久化的操作          
                        sqliteProvider.DeleteEventsAsync(actorid, 100).Wait();
                        break;
                    case "3":
                        result = false;
                        break;
                }
            }       
        }

        /// <summary>
        /// 快照
        /// </summary>
        /// <param name="actorid"></param>
        /// <param name="sqliteProvider"></param>
        private static void CallSnapShoot(string actorid, SqliteProvider sqliteProvider)
        {
            var props = Actor.FromProducer(() => new SnapShootDataActor(sqliteProvider, actorid));
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
                        Console.WriteLine($"随机产生的数字：{no}");
                        pid.Tell(new Data { Amount = no });
                        break;
                    case "2":
                        //完成处理后清理持久化的操作          
                        sqliteProvider.DeleteSnapshotsAsync(actorid, 100).Wait();
                        break;
                    case "3":
                        result = false;
                        break;
                }
            }
            
        }
        /// <summary>
        /// 快照事件溯源
        /// </summary>
        /// <param name="actorid"></param>
        /// <param name="sqliteProvider"></param>
        private static void CallSnapShootEventSource(string actorid, SqliteProvider sqliteProvider)
        {
            var props = Actor.FromProducer(() => new SnapShootEventSourceDataActor(sqliteProvider, sqliteProvider, actorid));
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
                        Console.WriteLine($"随机产生的数字：{no}");
                        pid.Tell(new Data { Amount = no });
                        break;
                    case "2":
                        //完成处理后清理持久化的操作          
                        sqliteProvider.DeleteEventsAsync(actorid, 100).Wait();
                        sqliteProvider.DeleteSnapshotsAsync(actorid, 100).Wait();
                        break;
                    case "3":
                        result = false;
                        break;
                }
            }         
        }
    }

    public class Data
    {
        public long Amount { get; set; }
    }

    #region 事件溯源
    public class EventSourceDataActor : IActor
    {
        private long _value = 0;
        private readonly Persistence _persistence;

        public EventSourceDataActor(IEventStore eventStore, string actorId)
        {
            //事件溯源持久化方式
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
    public class SnapShootDataActor : IActor
    {
        private long _value = 0;
        private readonly Persistence _persistence;

        public SnapShootDataActor(ISnapshotStore snapshotStore, string actorId)
        {
            //快照持久化方式
            _persistence = Persistence.WithSnapshotting(snapshotStore, actorId, ApplySnapshot);
        }
        private void ApplySnapshot(Proto.Persistence.Snapshot snapshot)
        {
            switch (snapshot.State)
            {
                case long value:
                    _value = value;
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
                    _value = _value + msg.Amount;
                    await _persistence.DeleteSnapshotsAsync(100);
                    await _persistence.PersistSnapshotAsync(_value);
                    break;
            }
        }
    }
    #endregion

    #region 事件溯源and快照
    public class SnapShootEventSourceDataActor : IActor
    {
        private long _value = 0;
        private readonly Persistence _persistence;

        public SnapShootEventSourceDataActor(IEventStore  eventStore, ISnapshotStore snapshotStore, string actorId)
        {
            //注释快照策略
            _persistence = Persistence.WithEventSourcingAndSnapshotting(eventStore, snapshotStore, actorId, ApplyEvent, ApplySnapshot, new IntervalStrategy(5), () => { return _value; });
            //无快照策略
            //_persistence = Persistence.WithEventSourcingAndSnapshotting(eventStore, snapshotStore, actorId, ApplyEvent, ApplySnapshot);
        }
        private void ApplyEvent(Proto.Persistence.Event @event)
        {
            switch (@event.Data)
            {
                case Data msg:
                    _value = _value + msg.Amount;
                    Console.WriteLine($"事件溯源累计：{_value}");
                    break;
            }
        }
        private void ApplySnapshot(Proto.Persistence.Snapshot snapshot)
        {
            switch (snapshot.State)
            {
                case long value:
                    _value = value;
                    Console.WriteLine($"快照累计：{_value}");
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
                    //无快照策略时启用
                    //await _persistence.PersistSnapshotAsync(_value);
                    break;
            }
        }
    }
    #endregion
}
