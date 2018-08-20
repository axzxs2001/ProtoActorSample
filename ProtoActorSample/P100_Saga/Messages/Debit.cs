using Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 借方
    /// </summary>
    internal class Debit : ChangeBalance
    {
        public Debit(decimal amount, PID replyTo) : base(amount, replyTo)
        {
        }
    }
}
