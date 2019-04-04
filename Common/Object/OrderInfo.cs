using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 订单信息
    /// </summary>
    public class OrderInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string time { get; set; }

        /// <summary>
        /// 交易请求序列号
        /// </summary>
        public string ReqId { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public string OrderDate { get; set; }

        /// <summary>
        /// 交易代码
        /// </summary>
        public string StockCd { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public uint Count { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderType OrderType { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// 订单信息（主要是错误的）
        /// </summary>
        public string RetMsg { get; set; }

    }
}
