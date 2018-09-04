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

        public override string ToString()
        {
            return $"Address={Address},Mobile={Mobile},Shiptime={Shiptime}";
        }
    }
}
