using System;

namespace P010_Libs
{
    /// <summary>
    /// 订单
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 订单总额
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// 订单时间
        /// </summary>
        public DateTime OrderTime { get; set; }
    }
}
