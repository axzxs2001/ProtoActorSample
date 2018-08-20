using Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 失败和不一致的结果
    /// </summary>
    internal class FailedAndInconsistent : Result
    {
        public FailedAndInconsistent(PID pid) : base(pid)
        {
        }
    }
}
