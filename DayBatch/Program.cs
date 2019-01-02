using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;
using DataProcess.GetData;

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
        private const string CSV_FOLDER = @"Data/";

        /// <summary>
        /// 图片路径信息
        /// </summary>
        private const string IMG_FOLDER = @"PngImg/";

        /// <summary>
        /// 趋势过滤结果路径信息
        /// </summary>
        private const string RESULT_FOLDER = @"ChkResult/";

        /// <summary>
        /// 天数据的目录
        /// </summary>
        private const string DAY_FOLDER = @"Day/";

        /// <summary>
        /// 不要创业数据
        /// </summary>
        private const bool NO_CHUANGYE = true;

        /// <summary>
        /// 所有数据信息
        /// </summary>
        private static List<string> allStockCd = new List<string>();

        #endregion

        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // 取数据
            GetData(args);
        }

        /// <summary>
        /// 取数据
        /// </summary>
        private static void GetData(string[] args)
        {
            string logFile = System.AppDomain.CurrentDomain.BaseDirectory + @"\Log\GetDataBatLog.txt";
            bool hasM5 = false;
            bool hasM15 = false;
            bool hasM30 = false;
            bool hasDay = false;
            if (args == null || args.Length == 0)
            {
                hasM5 = true;
                hasM15 = true;
                hasM30 = true;
                hasDay = true;
            }
            else
            { 
                foreach (string param in args)
                {
                    if ("M5".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        hasM5 = true;
                    }
                    else if ("M15".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        hasM15 = true;
                    }
                    else if ("M30".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        hasM30 = true;
                    }
                    else if ("DAY".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        hasDay = true;
                    }
                }
            }

            try
            {
                // 取得所有数据的基本信息（代码）
                GetAllStockBaseInfo();

                // 获取5分钟数据
                if (hasM5)
                {
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取5分钟数据 开始\r\n", Encoding.UTF8);
                    GetMinuteData(TimeRange.M5);
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取5分钟数据 结束\r\n", Encoding.UTF8);
                }

                // 获取15分钟数据
                if (hasM15)
                {
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取15分钟数据 开始\r\n", Encoding.UTF8);
                    GetMinuteData(TimeRange.M15);
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取15分钟数据 结束\r\n", Encoding.UTF8);
                }

                // 获取30分钟数据
                if (hasM30)
                {
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取30分钟数据 开始\r\n", Encoding.UTF8);
                    GetMinuteData(TimeRange.M30);
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取30分钟数据 结束\r\n", Encoding.UTF8);
                }

                // 获取整天的数据
                if (hasDay)
                {
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取天数据 开始\r\n", Encoding.UTF8);
                    GetAllDayData();
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取天数据 结束\r\n", Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message + "\r\n", Encoding.UTF8);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.StackTrace + "\r\n", Encoding.UTF8);
            }
        }

        /// <summary>
        /// 取得所有分钟级别的数据
        /// </summary>
        private static void GetMinuteData(TimeRange timeRange)
        {
            // 设定结束日期
            DateTime now = DateTime.Now;
            string endDay;
            if (DateTime.Now.Hour >= 15)
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
            string basePath = System.AppDomain.CurrentDomain.BaseDirectory;
            List<FilePosInfo> allCsv = Util.GetAllFiles(basePath + CSV_FOLDER + timeRange.ToString() + "/");

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFromSina(basePath + CSV_FOLDER, endDay, timeRange);

            // 循环取得所有的数据
            foreach (string stockCd in allStockCd)
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
            string basePath = System.AppDomain.CurrentDomain.BaseDirectory;
            List<FilePosInfo> allCsv = Util.GetAllFiles(basePath + CSV_FOLDER + DAY_FOLDER);

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFrom163(basePath + CSV_FOLDER + DAY_FOLDER, endDay);

            // 循环取得所有的数据
            foreach (string stockCd in allStockCd)
            {
                // 取得当前Stock数据
                getData.Start(stockCd, allCsv);
            }

            // 获取数据后的相关处理
            getData.After();
        }

        /// <summary>
        /// 取得所有数据的基本信息（代码）
        /// </summary>
        private static void GetAllStockBaseInfo()
        {
            allStockCd.Clear();

            string basePath = System.AppDomain.CurrentDomain.BaseDirectory;
            string[] allLine = File.ReadAllLines(basePath + CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
            if (allLine != null && allLine.Length > 0)
            {
                foreach (string codeName in allLine)
                {
                    if (string.IsNullOrEmpty(codeName))
                    {
                        continue;
                    }

                    allStockCd.Add(codeName.Substring(0, 6));
                }
            }
        }
    }
}
