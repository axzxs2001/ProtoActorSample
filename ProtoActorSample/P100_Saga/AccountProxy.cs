﻿using P100_Saga.Messages;
using Proto;
using System;
using System.Threading.Tasks;

namespace P100_Saga
{
    class AccountProxy : IActor
    {
        private readonly PID _target;
        private readonly Func<PID, object> _createMessage;
        public AccountProxy(PID target, Func<PID, object> createMessage)
        {
            _target = target;
            _createMessage = createMessage;
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                    // imagine this is some sort of remote call rather than a local actor call
                    //想象这是某种远程调用而不是本地Actor调用
                    _target.Tell(_createMessage(context.Self));
                    context.SetReceiveTimeout(TimeSpan.FromSeconds(100));
                   // context.SetReceiveTimeout(TimeSpan.FromMilliseconds(100));
                    break;
                case OK msg:
                    context.CancelReceiveTimeout();
                    context.Parent.Tell(msg);
                    break;
                case Refused msg:
                    context.CancelReceiveTimeout();
                    context.Parent.Tell(msg);
                    break;
                //This emulates a failed remote call
                //这里是模拟远程失败调用
                case InsufficientFunds _:
                case InternalServerError _:
                case ReceiveTimeout _:
                case ServiceUnavailable _: //TODO - this gives us more information than a failure
                    throw new Exception();
            }

            return Actor.Done;
        }
    }
}
