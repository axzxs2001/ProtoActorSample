using Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 失败但一致的结果
    /// </summary>
    class FailedButConsistentResult : Result
    {
        public FailedButConsistentResult(PID pid) : base(pid)
        {
        }
    }
}
