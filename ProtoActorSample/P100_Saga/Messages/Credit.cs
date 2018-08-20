using Proto;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 贷方
    /// </summary>
    class Credit : ChangeBalance
    {
        public Credit(decimal amount, PID replyTo) : base(amount, replyTo)
        {
        }
    }
}
