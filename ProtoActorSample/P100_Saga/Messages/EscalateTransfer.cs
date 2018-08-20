using System;
using System.Collections.Generic;
using System.Text;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 升级转移
    /// </summary>
    class EscalateTransfer
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; }

        public EscalateTransfer(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {Message}";
        }
    }
}
