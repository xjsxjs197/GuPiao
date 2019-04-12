using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 当前买卖的状态
    /// </summary>
    public enum BuySellStatus
    {
        /// <summary>
        /// 等待
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// 开始买，可以取消
        /// </summary>
        Buying = 1,

        /// <summary>
        /// 已经买过了，不能取消
        /// </summary>
        Buyed = 2,

        /// <summary>
        /// 开始卖，可以取消
        /// </summary>
        Selling = 3,

        /// <summary>
        /// 已经卖过了，不能取消
        /// </summary>
        Selled = 4
    }
}
