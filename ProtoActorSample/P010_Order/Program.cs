using Microsoft.Data.Sqlite;
using P010_Libs;
using Proto;
using Proto.Persistence.Sqlite;
using System;

namespace P010_Order
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Order系统";
            var actorid = "order_actor_id";
            var dbfile = @"C:\MyFile\Source\Repos\ProtoActorSample\ProtoActorSample\P010_Order\data.sqlite";
            var sqliteProvider = new SqliteProvider(new SqliteConnectionStringBuilder() { DataSource = dbfile });
            var props = Actor.FromProducer(() => new OrderActor(sqliteProvider,actorid));
            var pid = Actor.SpawnNamed(props, "order");
            pid.Tell(new Order { OrderNo="DJ000012352", Total=1524631.25m, OrderTime=DateTime .Now });
            Console.ReadLine();

        }
    }
}
