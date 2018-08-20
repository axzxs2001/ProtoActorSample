using Proto;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 变成帐户额
    /// </summary>
    abstract class ChangeBalance
    {
        /// <summary>
        /// 回答
        /// </summary>
        public PID ReplyTo { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        public decimal Amount { get; set; }

        protected ChangeBalance(decimal amount, PID replyTo)
        {
            ReplyTo = replyTo;
            Amount = amount;
        }
    }
}
