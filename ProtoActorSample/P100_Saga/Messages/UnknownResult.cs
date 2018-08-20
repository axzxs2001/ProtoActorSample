using Proto;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 未知结果
    /// </summary>
    class UnknownResult : Result
    {
        public UnknownResult(PID pid) : base(pid)
        {

        }
    }
}
