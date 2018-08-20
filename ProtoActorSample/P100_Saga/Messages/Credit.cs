using Proto;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 贷方
    /// </summary>
    class Credit : ChangeBalance
    {
        /// <summary>
        /// 贷方 变更帐户额
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="replyTo"></param>
        public Credit(decimal amount, PID replyTo) : base(amount, replyTo)
        {
        }
    }
}
