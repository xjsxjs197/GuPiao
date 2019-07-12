using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;
using DataProcess.GetData;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;
using DataProcess.FenXing;

namespace DayBatch
{
    /// <summary>
    /// 每天定时执行的批处理(取数据，画图)
    /// </summary>
    public class DayBatchProcess
    {
        #region " 全局变量 "

        /// <summary>
        /// 取最大多少个点的数据
        /// </summary>
        private const int MAX_POINTS = 118;

        /// <summary>
        /// 不要创业数据
        /// </summary>
        private const bool NO_CHUANGYE = true;

        /// <summary>
        /// 所有数据信息
        /// </summary>
        private List<string> allStockCd = new List<string>();
        private List<string> needGetAllCd;
        private GetDataBase getData;
        private List<FilePosInfo> allCsv;

        /// <summary>
        /// 当前程序的路径
        /// </summary>
        private string basePath = System.AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 子目录
        /// </summary>
        private string subFolder;

        /// <summary>
        /// 输出Log的文件
        /// </summary>
        private string logFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Log/GetDataBatLog.txt";

        /// <summary>
        /// 当前处理的文件
        /// </summary>
        private string currentFile;

        /// <summary>
        /// 主要处理前的代理
        /// </summary>
        /// <param name="num"></param>
        public delegate void DelegateBefDo(int num);

        /// <summary>
        /// 主要处理后的代理
        /// </summary>
        public delegate void DelegateEndDo();

        /// <summary>
        /// 当前记录处理后的代理
        /// </summary>
        public delegate void DelegateRowEndDo();

        /// <summary>
        /// 主要处理前的方法
        /// </summary>
        private DelegateBefDo callBef;

        /// <summary>
        /// 主要处理后的方法
        /// </summary>
        private DelegateEndDo callEnd;

        /// <summary>
        /// 当前记录处理后的方法
        /// </summary>
        private DelegateRowEndDo callRowEnd;

        /// <summary>
        /// 分型处理
        /// </summary>
        private FenXing fenXing = new FenXing();

        /// <summary>
        /// 设定信息
        /// </summary>
        private BuySellSetting configInfo;

        /// <summary>
        /// 画图时的信息
        /// </summary>
        private DrawImgInfo drawImgInfo;

        #endregion

        #region " 初始化 "

        /// <summary>
        /// 初始化
        /// </summary>
        public DayBatchProcess()
        {
            this.callBef = null;
            this.callEnd = null;
            this.callRowEnd = null;

            // 初始化各种笔、画刷信息
            this.InitDrawImgInfo();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public DayBatchProcess(DelegateBefDo callBef, DelegateEndDo callEnd, DelegateRowEndDo callRowEnd)
        {
            this.callBef = callBef;
            this.callEnd = callEnd;
            this.callRowEnd = callRowEnd;

            // 初始化各种笔、画刷信息
            this.InitDrawImgInfo();
        }

        #endregion

        #region " 公有方法 "

        /// <summary>
        /// 取数据
        /// </summary>
        public void Start(string[] args)
        {
            
            bool hasM5 = false;
            bool hasM15 = false;
            bool hasM30 = false;
            bool hasDay = false;
            bool needGetData = true;
            bool needDrawImg = true;
            bool emuTest = false;
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
                    else if ("NoData".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        needGetData = false;
                    }
                    else if ("NoImg".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        needDrawImg = false;
                    }
                    else if ("emu".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        emuTest = true;
                    }
                }
            }

            // 取得所有数据的基本信息（代码）
            this.GetAllStockBaseInfo();

            if (needGetData)
            {
                // 取数据
                this.GetData(hasM5, hasM15, hasM30, hasDay);
            }

            if (needDrawImg)
            {
                // 画趋势图
                this.DrawQushiImg(hasM5, hasM15, hasM30, hasDay);
            }

            if (emuTest)
            {
                // 模拟运行
                this.StartEmuTest();
            }

            // 释放资源
            this.ReleaseResource();
        }

        /// <summary>
        /// 开始取得数据
        /// </summary>
        /// <param name="timeRange"></param>
        public void StartGetData(TimeRange timeRange)
        {
            // 取得所有数据的基本信息（代码）
            this.GetAllStockBaseInfo();

            // 取数据
            this.CopyDataFromM5(timeRange);
        }

        /// <summary>
        /// 开始画趋势图
        /// </summary>
        /// <param name="timeRange"></param>
        public void StartDrawQushiImg(TimeRange timeRange)
        {
            // 取得所有数据的基本信息（代码）
            this.GetAllStockBaseInfo();

            // 画趋势图
            this.DrawQushiImg(timeRange);

            // 释放资源
            this.ReleaseResource();
        }

        /// <summary>
        /// 根据StockCd取得相关数据信息
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetStockInfo(string stockCdData, string subFolder, string basePath)
        {
            // 读取所有信息
            List<BaseDataInfo> stockInfos = GetStockHistoryInfo(basePath + Consts.CSV_FOLDER + subFolder + stockCdData + ".csv");
            if (stockInfos.Count == 0)
            {
                return null;
            }

            // 取得最大、最小值
            // 期间还处理了一下0，等于前一天的值
            decimal[] minMaxInfo = Util.GetMaxMinStock(stockInfos);
            if (minMaxInfo[0] == 0 || minMaxInfo[1] == 0 || (minMaxInfo[1] - minMaxInfo[0]) == 0 || stockInfos.Count == 0)
            {
                return null;
            }

            Dictionary<string, object> dicRet = new Dictionary<string, object>();
            dicRet.Add("stockInfos", stockInfos);
            dicRet.Add("minMaxInfo", minMaxInfo);

            return dicRet;
        }

        /// <summary>
        /// 取得股票历史数据
        /// </summary>
        /// <param name="stockFile"></param>
        /// <returns></returns>
        public static List<BaseDataInfo> GetStockHistoryInfo(string stockFile)
        {
            List<BaseDataInfo> stockInfo = new List<BaseDataInfo>();
            if (string.IsNullOrEmpty(stockFile) || !File.Exists(stockFile))
            {
                return stockInfo;
            }

            string[] allLine = File.ReadAllLines(stockFile, Encoding.UTF8);
            int maxPoints = MAX_POINTS;
            if (stockFile.IndexOf(TimeRange.M30.ToString()) > 0)
            {
                maxPoints *= 4;
            }
            else if (stockFile.IndexOf(TimeRange.M15.ToString()) > 0)
            {
                maxPoints *= 8;
            }
            else if (stockFile.IndexOf(TimeRange.M5.ToString()) > 0)
            {
                maxPoints *= 12;
            }

            for (int i = 1; i < allLine.Length && i <= maxPoints; i++)
            {
                if (allLine[i].IndexOf("Error") > 0)
                {
                    stockInfo.Clear();
                    return stockInfo;
                }

                string[] allItems = allLine[i].Split(',');
                if (allItems.Length > 3)
                {
                    BaseDataInfo dayInfo = new BaseDataInfo();
                    dayInfo.Day = allItems[0].Replace("-", "").Replace(" ", "").Replace(":", "");

                    dayInfo.DayVal = decimal.Parse(allItems[3]);
                    dayInfo.DayMaxVal = decimal.Parse(allItems[4]);
                    dayInfo.DayMinVal = decimal.Parse(allItems[5]);

                    stockInfo.Add(dayInfo);
                }
            }

            return stockInfo;
        }

        /// <summary>
        /// 取得当前级别的信息
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <param name="jibie">5、10、30等级别</param>
        /// <returns></returns>
        public static List<BaseDataInfo> GetAverageLineInfo(List<BaseDataInfo> stockInfos, int jibie)
        {
            List<BaseDataInfo> stockInfo = new List<BaseDataInfo>();

            int index = 0;
            int jibieCount = jibie - 1;
            int maxCount = stockInfos.Count - jibie;
            decimal total = 0;
            decimal minVal = decimal.MaxValue;
            decimal maxVal = 0;

            while (index <= maxCount)
            {
                total = 0;
                minVal = decimal.MaxValue;
                maxVal = 0;
                for (int i = 0; i <= jibieCount; i++)
                {
                    total += stockInfos[index + i].DayVal;
                    maxVal = Math.Max(stockInfos[index + i].DayVal, maxVal);
                    minVal = Math.Max(stockInfos[index + i].DayVal, minVal);
                }

                BaseDataInfo item = new BaseDataInfo();
                item.Day = stockInfos[index].Day;
                item.DayVal = total / jibie;
                item.DayMaxVal = maxVal;
                item.DayMinVal = minVal;

                stockInfo.Add(item);

                index++;
            }

            if (stockInfo.Count > 0)
            {
                while (stockInfo.Count < stockInfos.Count)
                {
                    stockInfo.Add(stockInfo[stockInfo.Count - 1]);
                }
            }

            return stockInfo;
        }

        #endregion

        #region " 主要处理 "
        
        /// <summary>
        /// 取数据
        /// </summary>
        private void GetData(bool hasM5, bool hasM15, bool hasM30, bool hasDay)
        {
            // 获取5分钟数据
            if (hasM5)
            {
                bool isSuccess = this.GetMinuteData(TimeRange.M5);
                if (!isSuccess)
                {
                    // 失败了，再来一次
                    this.GetMinuteData(TimeRange.M5);
                }
            }

            // 获取15分钟数据
            if (hasM15)
            {
                this.CopyDataFromM5(TimeRange.M15);
            }

            // 获取30分钟数据
            if (hasM30)
            {
                this.CopyDataFromM5(TimeRange.M30);
            }

            // 获取整天的数据
            if (hasDay)
            {
                this.CopyDataFromM5(TimeRange.Day);
            }
        }

        /// <summary>
        /// 取得所有分钟级别的数据
        /// </summary>
        private bool GetMinuteData(TimeRange timeRange)
        {
            bool isSuccess = true;
            try
            {
                string tmp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取" + timeRange.ToString() + "数据 开始";
                Console.WriteLine();
                Console.WriteLine(tmp);
                File.AppendAllText(logFile, tmp + "\r\n", Encoding.UTF8);

                // 设定结束日期
                DateTime now = this.GetNotWeeklyDayDate();
                string endDay;
                if (timeRange == TimeRange.Day)
                {
                    endDay = now.ToString("yyyy-MM-dd");
                }
                else
                {
                    endDay = now.ToString("yyyy-MM-dd 15:00:00");
                }

                // 取得分钟级别数据的共通
                isSuccess = this.GetMinuteDataCommon(endDay, timeRange);

                tmp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取" + timeRange.ToString() + "数据 结束";
                Console.WriteLine();
                Console.WriteLine(tmp);
                File.AppendAllText(logFile, tmp + "\r\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                isSuccess = false;
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message + "\r\n", Encoding.UTF8);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.StackTrace + "\r\n", Encoding.UTF8);
            }

            return isSuccess;
        }

        /// <summary>
        /// 取得分钟级别数据的共通
        /// </summary>
        private bool GetMinuteDataCommon(string endDay, TimeRange timeRange)
        {
            bool isSuccess = true;

            // 取得已经存在的所有数据信息
            this.allCsv = Util.GetAllFiles(this.basePath + Consts.CSV_FOLDER + timeRange.ToString() + "/");

            // 获取所有的代码信息
            endDay = endDay.Replace("-", "").Replace(" ", "").Replace(":", "");
            this.getData = new GetDataFromSina(this.basePath + Consts.CSV_FOLDER, endDay, timeRange);

            // 设置进度条
            DosProgressBar dosProgressBar = new DosProgressBar();
            int idx = 1;
            if (this.callBef != null)
            {
                this.callBef(this.allStockCd.Count);
            }

            // 取得所有必要的数据
            this.needGetAllCd = this.getData.GetAllNeedCd(this.allStockCd, allCsv, endDay);
            int totalLen = this.needGetAllCd.Count;
            foreach (string stockCd in this.needGetAllCd)
            {
                try
                {
                    // 取得当前Stock数据
                    string errMsg = this.getData.Start(stockCd, this.allCsv);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + errMsg + "\r\n", Encoding.UTF8);
                    }

                    Thread.Sleep(1000);
                    dosProgressBar.Dispaly((int)((idx++ / (totalLen * 1.0)) * 100));
                }
                catch (Exception exp)
                {
                    isSuccess = false;

                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取 " + stockCd + " 数据时发生异常\r\n", Encoding.UTF8);
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + exp.Message + "\r\n", Encoding.UTF8);
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + exp.StackTrace + "\r\n", Encoding.UTF8);

                    Thread.Sleep(60000);
                }

                // 更新进度条
                if (this.callRowEnd != null)
                {
                    this.callRowEnd();
                }
            }

            // 获取数据后的相关处理
            this.getData.After();

            // 关闭进度条
            if (this.callEnd != null)
            {
                this.callEnd();
            }

            return isSuccess;
        }

        /// <summary>
        /// 从5分钟数据中复制数据
        /// </summary>
        /// <param name="timeRange"></param>
        private void CopyDataFromM5(TimeRange timeRange)
        {
            try
            {
                string tmp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 计算" + timeRange.ToString() + "数据 开始";
                Console.WriteLine();
                Console.WriteLine(tmp);
                File.AppendAllText(logFile, tmp + "\r\n", Encoding.UTF8);

                // 设定结束日期
                DateTime now = this.GetNotWeeklyDayDate();
                string endDay;
                if (timeRange == TimeRange.Day)
                {
                    endDay = now.ToString("yyyy-MM-dd");
                }
                else
                {
                    endDay = now.ToString("yyyy-MM-dd 15:00:00");
                }

                // 取得分钟级别数据
                // 取得已经存在的所有数据信息
                this.allCsv = Util.GetAllFiles(this.basePath + Consts.CSV_FOLDER + timeRange.ToString() + "/");

                // 获取所有的代码信息
                endDay = endDay.Replace("-", "").Replace(" ", "").Replace(":", "");
                this.getData = new GetDataFromSina(this.basePath + Consts.CSV_FOLDER, endDay, timeRange);

                // 设置进度条
                DosProgressBar dosProgressBar = new DosProgressBar();
                int idx = 1;
                if (this.callBef != null)
                {
                    this.callBef(this.allStockCd.Count);
                }

                // 取得所有必要的数据
                this.needGetAllCd = this.getData.GetAllNeedCd(this.allStockCd, allCsv, endDay);
                int totalLen = this.needGetAllCd.Count;
                foreach (string stockCd in this.needGetAllCd)
                {
                    try
                    {
                        // 取得当前Stock数据
                        string errMsg = this.getData.CopyM5(stockCd, this.allCsv);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + errMsg + "\r\n", Encoding.UTF8);
                        }
                        dosProgressBar.Dispaly((int)((idx++ / (totalLen * 1.0)) * 100));
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine(exp.Message);
                        Console.WriteLine(exp.StackTrace);
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 计算 " + stockCd + " 数据时发生异常\r\n", Encoding.UTF8);
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + exp.Message + "\r\n", Encoding.UTF8);
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + exp.StackTrace + "\r\n", Encoding.UTF8);
                    }

                    // 更新进度条
                    if (this.callRowEnd != null)
                    {
                        this.callRowEnd();
                    }
                }

                // 获取数据后的相关处理
                this.getData.After();

                // 关闭进度条
                if (this.callEnd != null)
                {
                    this.callEnd();
                }

                tmp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 计算" + timeRange.ToString() + "数据 结束";
                Console.WriteLine();
                Console.WriteLine(tmp);
                File.AppendAllText(logFile, tmp + "\r\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message + "\r\n", Encoding.UTF8);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.StackTrace + "\r\n", Encoding.UTF8);
            }
        }

        /// <summary>
        /// 取得所有整天的数据
        /// </summary>
        private void GetAllDayData()
        {
            // 设定结束日期
            string endDay = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(this.basePath + Consts.CSV_FOLDER + Consts.DAY_FOLDER);

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFrom163(this.basePath + Consts.CSV_FOLDER + Consts.DAY_FOLDER, endDay);

            // 循环取得所有的数据
            foreach (string stockCd in this.allStockCd)
            {
                // 取得当前Stock数据
                getData.Start(stockCd, allCsv);
            }

            // 获取数据后的相关处理
            getData.After();
        }

        /// <summary>
        /// 画趋势图
        /// </summary>
        /// <param name="hasM5"></param>
        /// <param name="hasM15"></param>
        /// <param name="hasM30"></param>
        /// <param name="hasDay"></param>
        private void DrawQushiImg(bool hasM5, bool hasM15, bool hasM30, bool hasDay)
        {
            // 读取设定信息
            this.configInfo = Util.GetBuyCellSettingInfo();

            // 画5分钟趋势图
            if (hasM5)
            {
                this.DrawQushiImg(TimeRange.M5);
            }

            // 画15分钟趋势图
            if (hasM15)
            {
                this.DrawQushiImg(TimeRange.M15);
            }

            // 画30分钟趋势图
            if (hasM30)
            {
                this.DrawQushiImg(TimeRange.M30);
            }

            // 画天趋势图
            if (hasDay)
            {
                this.DrawQushiImg(TimeRange.Day);
            }
        }

        /// <summary>
        /// 画趋势图
        /// </summary>
        private void DrawQushiImg(TimeRange timeRange)
        {
            try
            {
                string tmp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 画" + timeRange.ToString() + "趋势图 开始";
                Console.WriteLine();
                Console.WriteLine(tmp);
                File.AppendAllText(logFile, tmp + "\r\n", Encoding.UTF8);

                // 取得已经存在的所有数据信息
                this.subFolder = timeRange.ToString() + "/";
                List<FilePosInfo> allCsv = Util.GetAllFiles(this.basePath + Consts.CSV_FOLDER + this.subFolder);

                // 设置进度条
                DosProgressBar dosProgressBar = new DosProgressBar();
                int idx = 0;
                int totalLen = allCsv.Count;
                if (this.callBef != null)
                {
                    this.callBef(allCsv.Count);
                }

                foreach (FilePosInfo fileItem in allCsv)
                {
                    idx++;
                    if (fileItem.IsFolder)
                    {
                        continue;
                    }

                    this.currentFile = fileItem.File;

                    try
                    {
                        this.CreateQushiImg(Util.GetShortNameWithoutType(fileItem.File), timeRange);
                        
                        dosProgressBar.Dispaly((int)((idx / (totalLen * 1.0)) * 100));
                    }
                    catch (Exception e)
                    {
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + this.currentFile + "\r\n", Encoding.UTF8);
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message + "\r\n", Encoding.UTF8);
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.StackTrace + "\r\n", Encoding.UTF8);
                    }

                    // 更新进度条
                    if (this.callRowEnd != null)
                    {
                        this.callRowEnd();
                    }
                }

                // 关闭进度条
                if (this.callEnd != null)
                {
                    this.callEnd();
                }

                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 画" + timeRange.ToString() + "趋势图 结束\r\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + this.currentFile + "\r\n", Encoding.UTF8);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message + "\r\n", Encoding.UTF8);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.StackTrace + "\r\n", Encoding.UTF8);
            }
        }

        /// <summary>
        /// 画趋势图
        /// </summary>
        /// <param name="stockCdData"></param>
        private void CreateQushiImg(string stockCdData, TimeRange timeRange)
        {
            // 获得数据信息
            this.subFolder = timeRange.ToString() + "/";
            Dictionary<string, object> dataInfo = GetStockInfo(stockCdData, this.subFolder, this.basePath);
            if (dataInfo == null)
            {
                return;
            }

            // 基础数据信息
            List<BaseDataInfo> stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
            if (stockInfos.Count == 0)
            {
                return;
            }

            // 最大、最小值信息
            decimal[] minMaxInfo = (decimal[])dataInfo["minMaxInfo"];
            decimal yStep = Util.GetYstep(minMaxInfo);

            // 设定图片
            Bitmap imgQushi = new Bitmap((stockInfos.Count + 2) * Consts.IMG_X_STEP, Consts.IMG_H);
            Graphics grp = Graphics.FromImage(imgQushi);
            grp.SmoothingMode = SmoothingMode.AntiAlias;
            grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 开始当前的线（5分，15分，30分，天）
            this.DrawStockQushi(stockInfos, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.BlackLinePen, grp, true);

            // 画均线
            if (timeRange == TimeRange.Day)
            {
                // 取得5日均线信息
                List<BaseDataInfo> stockInfo5Jibie = GetAverageLineInfo(stockInfos, 5);

                // 开始画5日均线
                if (stockInfo5Jibie.Count > 0)
                {
                    this.DrawStockQushi(stockInfo5Jibie, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.GreenLinePen, grp, false);
                }

                /*
                // 取得10日均线信息
                List<BaseDataInfo> stockInfo10Jibie = GetJibieStockInfo(stockInfos, 10);

                // 开始画10日均线
                if (stockInfo10Jibie.Count > 0)
                {
                 *  this.DrawStockQushi(stockInfo10Jibie, step, minMaxInfo[0], imgQushi, this.drawImgInfo.RedLinePen, grp, false);
                }*/
            }
            else if (timeRange == TimeRange.M30)
            {
                // 取得日均线信息
                List<BaseDataInfo> stockInfoDayJibie = GetAverageLineInfo(stockInfos, 8);

                // 开始画日均线
                if (stockInfoDayJibie.Count > 0)
                {
                    this.DrawStockQushi(stockInfoDayJibie, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.GreenLinePen, grp, false);
                }
            }

            // 开始画分型、笔的线段
            List<BaseDataInfo> fenXingInfo;
            switch (timeRange)
            {
                case TimeRange.M30:
                    fenXingInfo = this.fenXing.DoFenXingSp(stockInfos, this.configInfo, "100000", minMaxInfo);
                    break;

                case TimeRange.M15:
                    fenXingInfo = this.fenXing.DoFenXingSp(stockInfos, this.configInfo, "094500", minMaxInfo);
                    break;

                case TimeRange.M5:
                    fenXingInfo = this.fenXing.DoFenXingSp(stockInfos, this.configInfo, "093500", minMaxInfo);
                    break;

                default:
                    fenXingInfo = this.fenXing.DoFenXingComn(stockInfos);
                    break;
            }
            this.DrawFenxingPen(fenXingInfo, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkOrangeLinePen, grp, Consts.IMG_X_STEP);

            // 在30分钟的分型图上画天的分型信息
            if (timeRange == TimeRange.M30)
            {
                dataInfo = GetStockInfo(stockCdData.Substring(0, 15), TimeRange.Day.ToString() + "/", this.basePath);
                if (dataInfo != null)
                {
                    // 基础数据信息
                    stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
                    if (stockInfos.Count > 0)
                    {
                        // 开始画分型、笔的线段
                        fenXingInfo = this.fenXing.DoFenXingComn(stockInfos);
                        this.DrawFenxingPen(fenXingInfo, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkBlueLinePen, grp, Consts.IMG_X_STEP * 8);
                    }
                }
            }

            // 保存图片
            imgQushi.Save(this.basePath + Consts.IMG_FOLDER + this.subFolder + stockCdData.Substring(0, 6) + ".png");

            // 释放Graphics和图片资源
            grp.Dispose();
            imgQushi.Dispose();
        }

        #endregion

        #region " 私有处理 "

        /// <summary>
        /// 开始画趋势图
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private void DrawStockQushi(List<BaseDataInfo> stockInfo, decimal yStep, decimal minVal, Bitmap img, Pen pen, Graphics grp, bool needJiangeLine)
        {
            int x1 = img.Width - Consts.IMG_X_STEP;
            int y1 = this.GetYPos(img.Height, stockInfo[0].DayVal, minVal, yStep);
            int x2 = 0;
            int y2 = 0;
            int index = 1;
            int maxCount = stockInfo.Count - 1;
            int maxHeight = img.Height - 1;
            int strYPos = img.Height - 20;

            while (index <= maxCount)
            {
                x2 = x1 - Consts.IMG_X_STEP;
                y2 = this.GetYPos(img.Height, stockInfo[index].DayVal, minVal, yStep);

                grp.DrawLine(pen, x1, y1, x2, y2);
                x1 = x2;
                y1 = y2;

                if (needJiangeLine)
                {
                    //if ((index - 7) % 8 == 0)
                    if (((index - 7) & 7) == 0)
                    {
                        grp.DrawLine(this.drawImgInfo.NormalLinePen, x2, 0, x2, maxHeight);
                        grp.DrawString(stockInfo[index].Day.Substring(4, 4), this.drawImgInfo.NormalFont,
                            this.drawImgInfo.BlueVioletBush, x2, strYPos);
                    }
                    else
                    {
                        grp.DrawLine(this.drawImgInfo.DashLinePen, x2, 0, x2, maxHeight);
                    }
                }

                index++;
            }
        }

        /// <summary>
        /// 取得Y坐标的位置
        /// </summary>
        /// <param name="imgH"></param>
        /// <param name="pointVal"></param>
        /// <param name="minVal"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private int GetYPos(int imgH, decimal pointVal, decimal minVal, decimal step)
        {
            return imgH - ((int)((pointVal - minVal) * step) + 10);
        }

        /// <summary>
        /// 取得所有数据的基本信息（代码）
        /// </summary>
        private void GetAllStockBaseInfo()
        {
            this.allStockCd.Clear();

            string[] allLine = File.ReadAllLines(this.basePath + Consts.CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
            if (allLine != null && allLine.Length > 0)
            {
                foreach (string codeName in allLine)
                {
                    if (string.IsNullOrEmpty(codeName))
                    {
                        continue;
                    }

                    this.allStockCd.Add(codeName.Substring(0, 6));
                }
            }
        }
        
        /// <summary>
        /// 日期类型转换
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetDateFromString(string date)
        {
            string dateFormat = "yyyyMMdd";
            if (date.Length > 8)
            {
                dateFormat = "yyyyMMddHHmmss";
            }

            return DateTime.ParseExact(date, dateFormat, System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// 取得非节假日的时间
        /// </summary>
        /// <returns></returns>
        private DateTime GetNotWeeklyDayDate()
        {
            DateTime dt = DateTime.Now;

            if (dt.Hour < 15)
            {
                dt = dt.AddDays(-1);
            }

            while (Util.IsHolidayByDate(dt))
            {
                dt = dt.AddDays(-1);
            }

            return dt;
        }

        /// <summary>
        /// 初始化各种笔、画刷信息
        /// </summary>
        private void InitDrawImgInfo()
        {
            this.drawImgInfo = new DrawImgInfo();
            this.drawImgInfo.BuyBush = new SolidBrush(Color.Red);
            this.drawImgInfo.SellBush = new SolidBrush(Color.Green);
            this.drawImgInfo.BlueVioletBush = new SolidBrush(Color.BlueViolet);
            this.drawImgInfo.BuySellFont = new Font(new FontFamily("Microsoft YaHei"), 6, FontStyle.Bold);
            this.drawImgInfo.NormalFont = new Font(new FontFamily("Microsoft YaHei"), 8, FontStyle.Regular);

            this.drawImgInfo.DashLinePen = new Pen(Color.FromArgb(50, Color.Gray));
            this.drawImgInfo.DashLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            this.drawImgInfo.DashLinePen.DashPattern = new float[] { 3, 3 };
            this.drawImgInfo.NormalLinePen = new Pen(Color.FromArgb(90, Color.Gray));

            this.drawImgInfo.BlackLinePen = new Pen(Color.Black, 1F);
            this.drawImgInfo.GreenLinePen = new Pen(Color.Green, 1F);
            this.drawImgInfo.RedLinePen = new Pen(Color.Red, 1F);
            this.drawImgInfo.DarkOrangeLinePen = new Pen(Color.DarkOrange, 1F);
            this.drawImgInfo.DarkGreenLinePen = new Pen(Color.DarkGreen, 1F);
            this.drawImgInfo.DarkBlueLinePen = new Pen(Color.DarkBlue, 1F);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        private void ReleaseResource()
        {
            if (this.drawImgInfo != null)
            {
                this.drawImgInfo.BuyBush.Dispose();
                this.drawImgInfo.SellBush.Dispose();
                this.drawImgInfo.BlueVioletBush.Dispose();
                this.drawImgInfo.BuySellFont.Dispose();
                this.drawImgInfo.NormalFont.Dispose();

                this.drawImgInfo.DashLinePen.Dispose();
                this.drawImgInfo.NormalLinePen.Dispose();

                this.drawImgInfo.BlackLinePen.Dispose();
                this.drawImgInfo.GreenLinePen.Dispose();
                this.drawImgInfo.RedLinePen.Dispose();
                this.drawImgInfo.DarkOrangeLinePen.Dispose();
                this.drawImgInfo.DarkGreenLinePen.Dispose();
                this.drawImgInfo.DarkBlueLinePen.Dispose();
            }
        }

        #region " 分型处理相关 "

        /// <summary>
        /// 开始分型笔的线段
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private bool DrawFenxingPen(List<BaseDataInfo> fenXingInfo, decimal yStep, decimal minVal, Bitmap img, Pen pen, Graphics grp, int xStep)
        {
            if (fenXingInfo.Count == 0)
            {
                return false;
            }

            int x1 = img.Width - ((fenXingInfo.Count - 1) * xStep + Consts.IMG_X_STEP);
            int y1 = this.GetYPos(img.Height, fenXingInfo[fenXingInfo.Count - 1].DayVal, minVal, yStep);
            int x2 = x1;
            int y2 = 0;
            decimal curVal;
            int maxCnt = fenXingInfo.Count - 2;
            
            bool hasBuyPoint = false;
            bool buyed = false;

            for (int index = maxCnt; index >= 0; index--)
            {
                x2 += xStep;
                if (fenXingInfo[index].PointType == PointType.Down || fenXingInfo[index].PointType == PointType.Top || index == 0)
                {
                    curVal = fenXingInfo[index].PointType == PointType.Top ? fenXingInfo[index].DayMaxVal : fenXingInfo[index].DayMinVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, yStep);

                    //// 写字用做标识
                    //if (fenXingInfo[index].CurPointType == PointType.Top)
                    //{
                    //    grp.DrawString("T", font, sellBush, x2, y2);
                    //}
                    //else if (fenXingInfo[index].CurPointType == PointType.Bottom)
                    //{
                    //    grp.DrawString("B", font, buyBush, x2, y2);
                    //}

                    grp.DrawLine(pen, x1, y1, x2, y2);
                    x1 = x2;
                    y1 = y2;
                }
                
                if (fenXingInfo[index].BuySellFlg > 0 && !buyed)
                {
                    curVal = fenXingInfo[index].DayVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, yStep);

                    grp.DrawString("B", this.drawImgInfo.BuySellFont, this.drawImgInfo.BuyBush, x2, y2);
                    buyed = true;
                }
                else if (fenXingInfo[index].BuySellFlg < 0 && buyed)
                {
                    curVal = fenXingInfo[index].DayVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, yStep);

                    grp.DrawString("T", this.drawImgInfo.BuySellFont, this.drawImgInfo.SellBush, x2, y2);
                    buyed = false;
                }
            }

            return hasBuyPoint;
        }

        #endregion

        #region " 模拟运行 "

        /// <summary>
        /// 模拟运行
        /// </summary>
        private void StartEmuTest()
        {
            // 取得已经存在的所有数据信息
            this.subFolder = TimeRange.M30.ToString() + "/";
            string startTime = "100000";
            List<FilePosInfo> allCsv = this.FilterRongziRongQuan(Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder));
            Dictionary<string, List<string>[]> buySellInfo = new Dictionary<string, List<string>[]>();
            StringBuilder notGoodSb = new StringBuilder();
            StringBuilder goodSb = new StringBuilder();
            BuySellSetting emuInfo = Util.GetBuyCellSettingInfo();

            // 设置进度条
            DosProgressBar dosProgressBar = new DosProgressBar();
            int idx = 0;
            int totalLen = allCsv.Count;
            Console.WriteLine();

            foreach (FilePosInfo fileItem in allCsv)
            {
                idx++;

                if (fileItem.IsFolder)
                {
                    continue;
                }

                string stockCdDate = Util.GetShortNameWithoutType(fileItem.File);
                if (NO_CHUANGYE && Util.IsChuangyeStock(stockCdDate))
                {
                    continue;
                }

                // 测试BuySell的逻辑
                this.CheckBuySellPoint(stockCdDate, buySellInfo, notGoodSb, goodSb, emuInfo, startTime);

                // 更新进度条
                dosProgressBar.Dispaly((int)((idx / (totalLen * 1.0)) * 100));
            }

            this.SaveTotalBuySellInfo(allCsv, buySellInfo, notGoodSb, goodSb, emuInfo);
        }

        /// <summary>
        /// 测试买卖点的逻辑
        /// </summary>
        /// <param name="stockCdDate"></param>
        private void CheckBuySellPoint(string stockCdDate, Dictionary<string, List<string>[]> buySellInfo,
            StringBuilder notGoodSb, StringBuilder goodSb, BuySellSetting emuInfo, string startTime)
        {
            // 获得数据信息
            Dictionary<string, object> dataInfo = DayBatchProcess.GetStockInfo(stockCdDate, this.subFolder, "./");
            if (dataInfo == null)
            {
                return;
            }

            List<BaseDataInfo> stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
            if (stockInfos.Count == 0)
            {
                return;
            }

            decimal[] minMaxInfo = (decimal[])dataInfo["minMaxInfo"];
            int befDay = emuInfo.BefDay;

            // 设置测试的开始时间
            string startDate;
            int startIdx = -1;
            if (this.subFolder.Equals(Consts.DAY_FOLDER))
            {
                startDate = DateTime.Now.AddDays(-befDay).ToString("yyyyMMdd");
            }
            else
            {
                startDate = DateTime.Now.AddDays(-befDay).ToString("yyyyMMdd090000");
            }

            // 取得分型的数据
            List<BaseDataInfo> fenxingInfo = this.fenXing.DoFenXingSp(stockInfos, emuInfo, startTime, minMaxInfo);
            for (int i = 0; i < fenxingInfo.Count; i++)
            {
                if (string.Compare(fenxingInfo[i].Day, startDate) < 0)
                {
                    startIdx = i;
                    break;
                }
            }

            if (startIdx == -1)
            {
                startIdx = fenxingInfo.Count - 1;
            }

            StringBuilder sb = new StringBuilder();
            string stockCd = stockCdDate.Substring(0, 6);
            decimal buyPrice = 0;
            bool buyed = false;
            List<string> buyInfo;
            List<string> sellInfo;
            for (int i = startIdx; i >= 0; i--)
            {
                if (fenxingInfo[i].BuySellFlg < 0 && buyed)
                {
                    buyed = false;
                    sb.Append(fenxingInfo[i].Day).Append(" ");
                    sb.Append(fenxingInfo[i].DayVal.ToString().PadLeft(8, ' ')).Append(" ");
                    decimal diff = ((fenxingInfo[i].DayVal / buyPrice) - 1) * 100;
                    sb.Append(diff.ToString("0.00").PadLeft(7, ' ')).Append("%\r\n");

                    if (diff < -1)
                    {
                        string[] tmp = sb.ToString().Split('\r');
                        notGoodSb.Append(stockCd).Append(" ").Append(tmp[tmp.Length - 2]).Append("\r\n");
                    }
                    else if (diff > 1)
                    {
                        string[] tmp = sb.ToString().Split('\r');
                        goodSb.Append(stockCd).Append(" ").Append(tmp[tmp.Length - 2]).Append("\r\n");
                    }

                    if (!buySellInfo.ContainsKey(fenxingInfo[i].Day))
                    {
                        buyInfo = new List<string>();
                        sellInfo = new List<string>();
                        List<string>[] listBuySell = new List<string>[2];
                        listBuySell[0] = buyInfo;
                        listBuySell[1] = sellInfo;
                        buySellInfo.Add(fenxingInfo[i].Day, listBuySell);
                    }
                    else
                    {
                        List<string>[] listBuySell = buySellInfo[fenxingInfo[i].Day];
                        buyInfo = listBuySell[0];
                        sellInfo = listBuySell[1];
                    }
                    sellInfo.Add(stockCd + " " + fenxingInfo[i].DayVal);
                }
                else if (fenxingInfo[i].BuySellFlg > 0 && !buyed)
                {
                    buyed = true;
                    buyPrice = fenxingInfo[i].DayVal;
                    sb.Append(fenxingInfo[i].Day).Append(" ");
                    sb.Append(buyPrice.ToString().PadLeft(8, ' ')).Append(" ");

                    if (!buySellInfo.ContainsKey(fenxingInfo[i].Day))
                    {
                        buyInfo = new List<string>();
                        sellInfo = new List<string>();
                        List<string>[] listBuySell = new List<string>[2];
                        listBuySell[0] = buyInfo;
                        listBuySell[1] = sellInfo;
                        buySellInfo.Add(fenxingInfo[i].Day, listBuySell);
                    }
                    else
                    {
                        List<string>[] listBuySell = buySellInfo[fenxingInfo[i].Day];
                        buyInfo = listBuySell[0];
                        sellInfo = listBuySell[1];
                    }
                    buyInfo.Add(stockCd + " " + buyPrice);
                }
            }

            if (buyed)
            {
                sb.Append("\r\n");
            }

            if (sb.Length > 0)
            {
                File.WriteAllText(Consts.BASE_PATH + Consts.BUY_SELL_POINT_HST + stockCd + ".txt", sb.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// 保存买卖信息
        /// </summary>
        /// <param name="buySellInfo"></param>
        private void SaveTotalBuySellInfo(List<FilePosInfo> allCsv, Dictionary<string, List<string>[]> buySellInfo,
            StringBuilder notGoodSb, StringBuilder goodSb, BuySellSetting emuInfo)
        {
            List<string> dayList = new List<string>(buySellInfo.Keys);
            dayList.Sort();

            int buyThread = emuInfo.ThreadCnt;
            decimal threadMoney = emuInfo.ThreadMoney;
            bool isReverse = emuInfo.IsReverse;

            List<string> buyedStock = new List<string>();
            List<Dictionary<string, object>> buySellHst = new List<Dictionary<string, object>>();
            Dictionary<string, object> buySellItem;
            StringBuilder sb;
            decimal diff;
            while (buyThread-- > 0)
            {
                buySellItem = new Dictionary<string, object>();
                buySellItem.Add("stockCd", string.Empty);
                buySellItem.Add("status", string.Empty);
                buySellItem.Add("price", (decimal)0);
                buySellItem.Add("buyCount", (decimal)0);
                buySellItem.Add("buyMoney", (decimal)0);
                buySellItem.Add("TotalMoney", threadMoney);

                sb = new StringBuilder();
                sb.Append("Thread").Append(buyThread).Append(" ");
                sb.Append("Start Total: " + threadMoney).Append("\r\n");
                buySellItem.Add("logBuf", sb);

                buySellHst.Add(buySellItem);
            }

            foreach (string day in dayList)
            {
                List<string> buyInfo = buySellInfo[day][0];
                List<string> sellInfo = buySellInfo[day][1];
                if (isReverse)
                {
                    buyInfo.Reverse();
                }
                buyedStock.Clear();

                foreach (Dictionary<string, object> bsp in buySellHst)
                {
                    if (!"B".Equals(bsp["status"] as string))
                    {
                        foreach (string bp in buyInfo)
                        {
                            string[] tmp = bp.Split(' ');
                            decimal price = decimal.Parse(tmp[1]);

                            if (!buyedStock.Contains(tmp[0]))
                            {
                                decimal totalMoney = (decimal)bsp["TotalMoney"];
                                int canBuyCnt = Util.CanBuyCount(totalMoney, price);
                                if (canBuyCnt > 0)
                                {
                                    buyedStock.Add(tmp[0]);

                                    bsp["stockCd"] = tmp[0];
                                    bsp["status"] = "B";
                                    bsp["price"] = price;
                                    bsp["buyCount"] = (decimal)(canBuyCnt * 100);
                                    bsp["buyMoney"] = (decimal)bsp["buyCount"] * price + 5;
                                    bsp["TotalMoney"] = (decimal)bsp["TotalMoney"] - (decimal)bsp["buyMoney"];

                                    sb = (StringBuilder)bsp["logBuf"];
                                    sb.Append(day).Append(" B ").Append(tmp[0]).Append(" ");
                                    sb.Append(price.ToString().PadLeft(8, ' ')).Append(" ");
                                    sb.Append(bsp["buyCount"].ToString().PadLeft(4, ' ')).Append("\r\n");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (string sp in sellInfo)
                        {
                            if (sp.StartsWith(bsp["stockCd"] as string, StringComparison.OrdinalIgnoreCase))
                            {
                                string[] tmp = sp.Split(' ');
                                decimal price = decimal.Parse(tmp[1]);
                                decimal sellMoney = (decimal)bsp["buyCount"] * price;

                                buyedStock.Remove(tmp[0]);

                                bsp["status"] = "S";
                                bsp["TotalMoney"] = (decimal)bsp["TotalMoney"] + sellMoney;

                                diff = ((sellMoney / (decimal)bsp["buyMoney"]) - 1) * 100;
                                sb = (StringBuilder)bsp["logBuf"];
                                sb.Append(day).Append(" S ").Append(bsp["stockCd"]).Append(" ");
                                sb.Append(price.ToString().PadLeft(8, ' ')).Append(" ");
                                sb.Append(bsp["buyCount"].ToString().PadLeft(4, ' ')).Append(" ");
                                sb.Append(diff.ToString("0.00")).Append(" ").Append(bsp["TotalMoney"]).Append("(");
                                diff = ((decimal)bsp["TotalMoney"] / threadMoney - 1) * 100;
                                sb.Append(diff.ToString("0.00")).Append(")");
                                sb.Append("\r\n");
                                break;
                            }
                        }
                    }
                }
            }

            StringBuilder sbAll = new StringBuilder();
            decimal totalAll = 0;

            foreach (Dictionary<string, object> bsp in buySellHst)
            {
                sb = (StringBuilder)bsp["logBuf"];
                decimal total = (decimal)bsp["TotalMoney"];
                sb.Append("End Total: ");

                if ("B".Equals(bsp["status"]))
                {
                    FilePosInfo lastInfo = allCsv.FirstOrDefault(p => p.File.IndexOf(bsp["stockCd"] as string) > 0);
                    // 获得数据信息
                    Dictionary<string, object> dataInfo = DayBatchProcess.GetStockInfo(
                        Util.GetShortNameWithoutType(lastInfo.File), this.subFolder, "./");
                    if (dataInfo != null)
                    {
                        List<BaseDataInfo> stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
                        if (stockInfos.Count > 0)
                        {
                            total += (decimal)bsp["buyCount"] * stockInfos[0].DayVal;
                        }
                    }
                }
                totalAll += total;
                sb.Append(total).Append(" ");
                diff = (total / threadMoney - 1) * 100;
                sb.Append(diff.ToString("0.00")).Append("%");

                sbAll.Append(sb.ToString()).Append("\r\n\r\n");
            }
            sbAll.Append("Total : ").Append(threadMoney * buySellHst.Count).Append(" ");
            diff = (totalAll / (threadMoney * buySellHst.Count) - 1) * 100;
            sbAll.Append(totalAll).Append(" ").Append(diff.ToString("0.00")).Append("%\r\n\r\n");

            File.WriteAllText(Consts.BASE_PATH + Consts.BUY_SELL_POINT_HST + "TotalBuySellInfo.txt", sbAll.ToString(), Encoding.UTF8);

            File.WriteAllText(Consts.BASE_PATH + Consts.BUY_SELL_POINT_HST + "BadBuySellPoint.txt", notGoodSb.ToString(), Encoding.UTF8);

            File.WriteAllText(Consts.BASE_PATH + Consts.BUY_SELL_POINT_HST + "GoodBuySellPoint.txt", goodSb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 过滤融资融券
        /// </summary>
        /// <param name="allCsv"></param>
        /// <returns></returns>
        private List<FilePosInfo> FilterRongziRongQuan(List<FilePosInfo> allCsv)
        {
            List<string> allRongzi = Util.GetRongZiRongQuan();

            List<FilePosInfo> newCsv = new List<FilePosInfo>();
            foreach (FilePosInfo item in allCsv)
            {
                if (item.IsFolder)
                {
                    continue;
                }

                string cdDate = Util.GetShortNameWithoutType(item.File);
                if (!allRongzi.Contains(cdDate.Substring(0, 6)))
                {
                    newCsv.Add(item);
                }
            }

            return newCsv;
        }

        #endregion

        #endregion
    }
}
