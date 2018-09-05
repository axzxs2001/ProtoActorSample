using System;

namespace P010_Libs
{
    /// <summary>
    /// 运单
    /// </summary>
    public class Ship
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 订单时间
        /// </summary>
        public DateTime Shiptime { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        public override string ToString()
        {
            return $"Address={Address},Mobile={Mobile},Shiptime={Shiptime}，Name={Name}，OrderNo={OrderNo}";
        }
    }
}
