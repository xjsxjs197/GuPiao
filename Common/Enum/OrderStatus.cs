using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        None = -1,
        Waiting = 0,
        OrderOk = 1,
        OrderCancel = 2,
        OrderError = 9
    }
}
