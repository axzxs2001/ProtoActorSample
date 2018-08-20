using Proto;

namespace P100_Saga.Messages
{
    /// <summary>
    /// 转帐完成
    /// </summary>
    class TransferCompleted
    {
        /// <summary>
        /// 转出
        /// </summary>
        public PID From { get; }
        /// <summary>
        ///转出额度
        /// </summary>
        public decimal FromBalance { get; }
        /// <summary>
        /// 转入
        /// </summary>
        public PID To { get; }
        /// <summary>
        /// 转入额度
        /// </summary>
        public decimal ToBalance { get; }

        public TransferCompleted(PID from, decimal fromBalance, PID to, decimal toBalance)
        {
            From = @from;
            FromBalance = fromBalance;
            To = to;
            ToBalance = toBalance;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {From.Id} balance is {FromBalance}, {To.Id} balance is {ToBalance}";
        }
    }
}
