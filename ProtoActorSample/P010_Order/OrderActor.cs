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

        PID _pid;

        public OrderActor(IProvider provider, string persistenceId)
        {
            _behavior = new Behavior();
            _persistence = Persistence.WithEventSourcing(provider, persistenceId, ApplyEvent);
        }

        private async void ApplyEvent(Event obj)
        {
            switch (obj.Data)
            {
                case Started started:
                    _behavior.Become(Ordering);
                    _pid.Tell(new Order { OrderNo = "DJ000012352", Total = 1524631.25m, OrderTime = DateTime.Now });
                    break;
                case Order order:
                    _behavior.Become(OrderComplete);
                    _pid.Tell(new Ship { Address = "东京中央区茅埸町PMO", Mobile = "13453467144", Shiptime = DateTime.Now, Name = "桂素伟", OrderNo = order.OrderNo });
                    break;
                case Ship ship:
                    await RemoteSend(ship);
                    break;
            }
        }
        async Task RemoteSend(Ship ship)
        {
            //设置序列化类型并注册
            var wire = new WireSerializer(new[] { typeof(Ship), typeof(bool) });
            Serialization.RegisterSerializer(wire, true);
            //设置自己监控端口5002
            Remote.Start("127.0.0.1", 5002);
            //连接服务端5001
            var pid = Remote.SpawnNamedAsync("127.0.0.1:5001", "shiping", "ship", TimeSpan.FromSeconds(50)).Result.Pid;

            var result = await pid.RequestAsync<bool>(ship);

            Console.WriteLine($"下订单返回结果：{result}");
            if (result)
            {
                //await _persistence.DeleteEventsAsync(10);
            }
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started msg:
                    _pid = context.Self;
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
                case Started started:
                    Console.WriteLine("开始处理");
                    await _persistence.PersistEventAsync(started);
                    break;
            }
        }
        private async Task Ordering(IContext context)
        {
            switch (context.Message)
            {
                case Order order:
                    Console.WriteLine($"OrderActor:收到order {order}");
                    await _persistence.PersistEventAsync(order);

                    break;
            }

        }
        private async Task OrderComplete(IContext context)
        {
            switch (context.Message)
            {
                case Ship ship:
                    Console.WriteLine($"OrderActor:收到Ship {ship}");
                    await _persistence.PersistEventAsync(ship);
                    break;
            }
        }


    }
}
