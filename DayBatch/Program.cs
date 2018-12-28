using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using DataProcess.GetData;
using System.IO;

namespace DayBatch
{
    /// <summary>
    /// 每天定时执行的批处理
    /// </summary>
    class Program
    {
        #region " 全局变量 "

        /// <summary>
        /// 取最多多少天数据
        /// </summary>
        private const int MAX_DAYS = 110;

        /// <summary>
        /// 趋势图X轴间隔的像素
        /// </summary>
        private const int IMG_X_STEP = 5;

        /// <summary>
        /// 数据路径信息
        /// </summary>
        private const string CSV_FOLDER = @"./Data/";

        /// <summary>
        /// 图片路径信息
        /// </summary>
        private const string IMG_FOLDER = @"./PngImg/";

        /// <summary>
        /// 趋势过滤结果路径信息
        /// </summary>
        private const string RESULT_FOLDER = @"./ChkResult/";

        /// <summary>
        /// 天数据的目录
        /// </summary>
        private const string DAY_FOLDER = @"Day/";

        /// <summary>
        /// 不要创业数据
        /// </summary>
        private const bool NO_CHUANGYE = true;

        #endregion

        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 取数据
            GetData();
        }

        /// <summary>
        /// 取数据
        /// </summary>
        private static void GetData()
        {
            List<string> logTxt = new List<string>();

            try
            {
                // 获取5分钟数据
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取5分钟数据 开始");
                GetMinuteData(TimeRange.M5);
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取5分钟数据 结束");

                // 获取15分钟数据
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取15分钟数据 开始");
                GetMinuteData(TimeRange.M15);
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取15分钟数据 结束");

                // 获取30分钟数据
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取30分钟数据 开始");
                GetMinuteData(TimeRange.M30);
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取30分钟数据 结束");

                // 获取整天的数据
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取天数据 开始");
                GetAllDayData();
                logTxt.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取天数据 结束");
            }
            catch (Exception e)
            {
                logTxt.Add(e.Message);
                logTxt.Add(e.StackTrace);
            }

            File.WriteAllLines(@"./Log/GetDataBatLog.txt", logTxt.ToArray(), Encoding.UTF8);
        }

        /// <summary>
        /// 取得所有分钟级别的数据
        /// </summary>
        private static void GetMinuteData(TimeRange timeRange)
        {
            // 设定结束日期
            DateTime now = DateTime.Now;
            string endDay;
            if (DateTime.Now.Hour > 15)
            {
                endDay = now.ToString("yyyy-MM-dd 15:00:00");
            }
            else if (DateTime.Now.Hour < 9)
            {
                endDay = now.AddDays(-1).ToString("yyyy-MM-dd 15:00:00");
            }
            else
            {
                endDay = now.ToString("yyyy-MM-dd HH:mm:00");
            }

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + timeRange.ToString() + "/");

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFromSina(CSV_FOLDER, endDay, timeRange);
            List<string> allStock = getData.Before();

            // 循环取得所有的数据
            foreach (string stockCd in allStock)
            {
                // 取得当前Stock数据
                getData.Start(stockCd, allCsv);
            }

            // 获取数据后的相关处理
            getData.After();
        }

        /// <summary>
        /// 取得所有整天的数据
        /// </summary>
        private static void GetAllDayData()
        {
            // 设定结束日期
            string endDay = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + DAY_FOLDER);

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFrom163(CSV_FOLDER, endDay);
            List<string> allStock = getData.Before();

            // 循环取得所有的数据
            foreach (string stockCd in allStock)
            {
                // 取得当前Stock数据
                getData.Start(stockCd, allCsv);
            }

            // 获取数据后的相关处理
            getData.After();
        }
    }
}
