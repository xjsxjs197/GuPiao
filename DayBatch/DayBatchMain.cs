using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;
using DataProcess.GetData;

namespace DayBatch
{
    /// <summary>
    /// 每天定时执行的批处理(取数据，画图)
    /// </summary>
    class DayBatchMain
    {
        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 取数据，画趋势图
            DayBatchProcess getData = new DayBatchProcess();
            getData.Start(args);
        }
    }
}
