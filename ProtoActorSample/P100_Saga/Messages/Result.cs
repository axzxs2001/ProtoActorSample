using Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 结果
    /// </summary>
    class Result
    {
        public PID Pid { get; }
        public Result(PID pid)
        {
            Pid = pid;
        }
    }
}
