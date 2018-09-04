using P010_Libs;
using Proto;
using Proto.Persistence;
using Proto.Remote;
using Proto.Serialization.Wire;
using System;
using System.Threading.Tasks;


namespace P010_Order
{
    partial class OrderActor : IActor
    {
        readonly Behavior _behavior;
        readonly Persistence _persistence;
        readonly string _persistenceId;
        readonly IProvider _provider;
        public OrderActor(IProvider provider, string persistenceId)
        {
            _behavior = new Behavior();
            _persistence = Persistence.WithEventSourcing(provider, persistenceId, ApplyEvent);
        }



        private void ApplyEvent(Event obj)
        {
            switch (obj.Data)
            {
                case Started started:
                    _behavior.Become(Ordering);
                    break;
                case Order order:
                    _behavior.Become(Ordering);
                    break;
                case Ship ship:
                    _behavior.Become(OrderComplete);
                    break;
            }
        }


        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started msg:
                    _behavior.Become(Starting);
                    await _persistence.RecoverStateAsync();
                    break;
            }

            await _behavior.ReceiveAsync(context);
        }

        private async Task Starting(IContext context)
        {
            switch (context.Message)
            {
                case Started  started:
                    Console.WriteLine("开始处理");
                    await _persistence.PersistEventAsync(started);
                    break;
            } 
        }
        private async Task Ordering(IContext context)
        {
            switch(context.Message)
            {
                case Order order:
                    context.Spawn(Actor.FromProducer(()=>new OrderingActor()));
                    Console.WriteLine("保存订单，更新库存:" + order);
                    await _persistence.PersistEventAsync(new Ship());
                    break;
            }
           
        }
        private async Task OrderComplete(IContext context)
        {
            //设置序列化类型并注册
            var wire = new WireSerializer(new[] { typeof(Ship), typeof(bool) });
            Serialization.RegisterSerializer(wire, true);
            //设置自己监控端口5002
            Remote.Start("127.0.0.1", 5002);
            //连接服务端5001
            var pid = Remote.SpawnNamedAsync("127.0.0.1:5001", "shiping", "ship", TimeSpan.FromSeconds(50)).Result.Pid;

            var result = await pid.RequestAsync<bool>(context.Message);
            Console.WriteLine(result);
        }  

    }
}
