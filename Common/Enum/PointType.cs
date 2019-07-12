using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 分型情报
    /// </summary>
    public enum PointType
    {
        /// <summary>
        /// 默认值
        /// </summary>
        Changing = 0,

        /// <summary>
        /// 当前点高于前一个点
        /// </summary>
        Up = 1,

        /// <summary>
        /// 当前点低于前一个点
        /// </summary>
        Down = 2,

        /// <summary>
        /// 底分型
        /// </summary>
        Bottom = 3,

        /// <summary>
        /// 顶分型
        /// </summary>
        Top = 4
    }
}
