using P010_Libs;
using Proto;
using Proto.Persistence;
using System;
using System.Threading.Tasks;


namespace P010_Order
{
    partial class OrderActor : IActor
    {
        readonly Behavior _behavior;
        readonly Persistence _persistence;
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
        }

        private async Task Starting(IContext context)
        {
            {
                var props = Actor.FromProducer(() => new OrderingActor());
                context.SpawnNamed(props, "Ordering");
                await _persistence.PersistEventAsync(new Order());
            }
        }
        private Task Ordering(IContext context)
        {
            throw new NotImplementedException();
        }
    }

    class OrderingActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            throw new NotImplementedException();
        }
    }

}
