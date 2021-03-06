﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 时间级别
    /// </summary>
    public enum TimeRange
    {
        /// <summary>
        /// 5分钟
        /// </summary>
        M5 = 5,

        /// <summary>
        /// 15分钟
        /// </summary>
        M15 = 15,

        /// <summary>
        /// 30分钟
        /// </summary>
        M30 = 30,

        /// <summary>
        /// 天（240分钟）
        /// </summary>
        Day = 240
    }
}
