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
            System.Threading.Thread.Sleep(100);
            var actorid = "order_actor_ids";
            var dbfile = @"C:\MyFile\Source\Repos\ProtoActorSample\ProtoActorSample\P010_Order\data.sqlite";
            var sqliteProvider = new SqliteProvider(new SqliteConnectionStringBuilder() { DataSource = dbfile });
            var props = Actor.FromProducer(() => new OrderActor(sqliteProvider, actorid));//.WithChildSupervisorStrategy(new OneForOneStrategy(SupervisorMode.Decide,5, TimeSpan.FromSeconds(1))); ;
            var pid = Actor.SpawnNamed(props, "order");
         
            Console.ReadLine();

        }
    }
    public class SupervisorMode
    {
        public static SupervisorDirective Decide(PID pid, Exception reason)
        {
            Console.WriteLine("   异常发生：" + reason.Message + "   " + pid);
            switch (reason)
            {
                //重新开始的异常
                case Exception _:
                    return SupervisorDirective.Restart;               
                default:
                    return SupervisorDirective.Escalate;
            }
        }
    }
}
