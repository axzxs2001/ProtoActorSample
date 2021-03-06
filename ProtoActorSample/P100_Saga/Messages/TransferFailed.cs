﻿using System;
using System.Collections.Generic;
using System.Text;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 转帐失败
    /// </summary>
    class TransferFailed
    {
        /// <summary>
        /// 原因
        /// </summary>
        public string Reason { get; }
        /// <summary>
        /// 转帐失败
        /// </summary>
        /// <param name="reason">原因</param>
        public TransferFailed(string reason)
        {
            Reason = reason;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {Reason}";
        }
    }
}
