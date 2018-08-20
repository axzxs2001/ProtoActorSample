using Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace P100_Saga.Messages
{
    class SuccessResult : Result
    {
        public SuccessResult(PID pid) : base(pid)
        {
        }
    }
}
