using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;
using DataProcess.GetData;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;

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
        /// 不作为变化的判断范围
        /// </summary>
        private const decimal LIMIT_VAL = (decimal)1.005;

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
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public DayBatchProcess(DelegateBefDo callBef, DelegateEndDo callEnd, DelegateRowEndDo callRowEnd)
        {
            this.callBef = callBef;
            this.callEnd = callEnd;
            this.callRowEnd = callRowEnd;
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
        }

        /// <summary>
        /// 根据StockCd取得相关数据信息
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetStockInfo(string stockCdData, string subFolder, string basePath)
        {
            // 读取所有信息
            List<BaseDataInfo> stockInfos = GetStockHistoryInfo(basePath + CSV_FOLDER + subFolder + stockCdData + ".csv");
            if (stockInfos.Count == 0)
            {
                return null;
            }

            // 取得最大、最小值
            // 期间还处理了一下0，等于前一天的值
            decimal[] minMaxInfo = GetMaxMinStock(stockInfos);
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
        /// 取得最大、最小值
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns></returns>
        public static decimal[] GetMaxMinStock(List<BaseDataInfo> stockInfos)
        {
            decimal[] minMaxInfo = new decimal[2];
            decimal minVal = decimal.MaxValue;
            decimal maxVal = 0;

            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                if (stockInfos[i].DayVal == 0)
                {
                    stockInfos.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                decimal curVal = stockInfos[i].DayVal;
                if (curVal > maxVal)
                {
                    maxVal = curVal;
                }

                if (curVal > 0 && curVal < minVal)
                {
                    minVal = curVal;
                }

                if (curVal == 0)
                {
                    BaseDataInfo item = new BaseDataInfo();
                    item.Day = stockInfos[i].Day;
                    item.DayVal = stockInfos[i + 1].DayVal;
                    item.DayMaxVal = stockInfos[i + 1].DayMaxVal;
                    item.DayMinVal = stockInfos[i + 1].DayMinVal;

                    stockInfos[i] = item;
                }
            }

            minMaxInfo[0] = minVal;
            minMaxInfo[1] = maxVal;

            return minMaxInfo;
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
        public static List<BaseDataInfo> GetJibieStockInfo(List<BaseDataInfo> stockInfos, int jibie)
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

        /// <summary>
        /// 设置分型情报
        /// </summary>
        /// <param name="stockInfo"></param>
        public static List<BaseDataInfo> SetFenxingInfo(List<BaseDataInfo> stockInfo)
        {
            if (stockInfo.Count < 3)
            {
                return stockInfo;
            }

            // 设置第一个点
            BaseDataInfo lastPoint = stockInfo[stockInfo.Count - 1];
            BaseDataInfo tmpPoint = new BaseDataInfo();
            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
            tmpPoint.DayMinVal = lastPoint.DayMinVal;
            int chkVal = 0;
            int lastChkVal = 0;
            int lastTopPos = -1;
            int lastBottomPos = -1;
            
            int maxCnt = stockInfo.Count - 2;
            int lastIdx = maxCnt + 1;

            for (int i = maxCnt; i >= 0; i--)
            {
                // 判断两个点的大小关系
                chkVal = ChkPointsVal(stockInfo[i], tmpPoint);

                if (chkVal > 0 && lastChkVal < 0)
                {
                    // 当前上升，前面是下降，说明前一个点是低点
                    lastPoint.CurPointType = PointType.Bottom;

                    // 判断是否是第三类买点
                    lastBottomPos = GeBefBottomPos(stockInfo, lastIdx, maxCnt);
                    if (lastBottomPos > 0 && lastPoint.DayMinVal > stockInfo[lastBottomPos].DayMinVal * LIMIT_VAL)
                    {
                        // 当前低点高于上一个低点，设置第三类买点
                        stockInfo[i].BuySellFlg = 3;
                    }

                    //// 取得前一个高点位置
                    //lastTopPos = GeBefTopVal(stockInfo, lastIdx, maxCnt);

                    //if (lastTopPos > 0)
                    //{
                    //    // 判断底点和上一个高点的间隔（中间需要三个点才行）
                    //    if (lastIdx - lastTopPos >= 3)
                    //    {
                    //        // 判断是否是第三类买点
                    //        lastBottomPos = GeBefBottomPos(stockInfo, lastIdx, maxCnt);
                    //        if (lastBottomPos > 0 && lastPoint.DayMinVal > stockInfo[lastBottomPos].DayMinVal * LIMIT_VAL)
                    //        {
                    //            // 当前低点高于上一个低点，设置第三类买点
                    //            stockInfo[i].BuySellFlg = 3;
                    //        }
                    //    }
                    //    else
                    //    { 
                    //        // 当前低点和上一个高点间隔太近，两个都不算
                    //        lastPoint.CurPointType = PointType.Changing;
                    //        stockInfo[lastTopPos].CurPointType = PointType.Changing;
                    //    }
                    //}
                }
                else if (chkVal < 0 && lastChkVal > 0)
                {
                    // 当前下降，前面是上升，说明前一个点是高点
                    lastPoint.CurPointType = PointType.Top;

                    // 设置第一类卖点
                    stockInfo[i].BuySellFlg = -1;

                    //// 取得前一个低点位置
                    //lastBottomPos = GeBefBottomPos(stockInfo, lastIdx, maxCnt);

                    //if (lastBottomPos > 0)
                    //{
                    //    // 判断高点和上一个低点的间隔（中间需要三个点才行）
                    //    if (lastIdx - lastBottomPos >= 3)
                    //    {
                    //        // 设置第一类卖点
                    //        stockInfo[i].BuySellFlg = -1;
                    //    }
                    //    else
                    //    {
                    //        // 当前高点和上一个低点间隔太近，两个都不算
                    //        lastPoint.CurPointType = PointType.Changing;
                    //        stockInfo[lastBottomPos].CurPointType = PointType.Changing;
                    //    }
                    //}
                    
                }

                // 更新当前的点
                if (chkVal != 0)
                {
                    lastChkVal = chkVal;
                    lastPoint = stockInfo[i];
                    lastIdx = i;
                    tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
                    tmpPoint.DayMinVal = lastPoint.DayMinVal;
                }
            }

            return stockInfo;
        }

        /// <summary>
        /// 设置分型情报（在30分钟中设置天的）
        /// </summary>
        /// <param name="stockInfo"></param>
        public static List<BaseDataInfo> SetFenxingInfoDayM30(List<BaseDataInfo> stockInfo)
        {
            // 设置第一个点
            BaseDataInfo lastPoint = new BaseDataInfo();
            while (stockInfo.Count > 0)
            {
                lastPoint = stockInfo[stockInfo.Count - 1];
                if (lastPoint.Day.IndexOf("100000") > 0)
                {
                    break;
                }
                else
                {
                    stockInfo.Remove(lastPoint);
                }
            }

            if (stockInfo.Count < 3)
            {
                return stockInfo;
            }

            // 特殊处理30分钟数据
            BaseDataInfo tmpPoint = new BaseDataInfo();
            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
            tmpPoint.DayMinVal = lastPoint.DayMinVal;
            int maxCnt = stockInfo.Count - 2;
            int lastIdx = maxCnt + 1;
            for (int i = maxCnt; i >= 0; i--)
            {
                if (stockInfo[i].DayMaxVal > tmpPoint.DayMaxVal)
                {
                    tmpPoint.DayMaxVal = stockInfo[i].DayMaxVal;
                }
                if (stockInfo[i].DayMinVal < tmpPoint.DayMinVal)
                {
                    tmpPoint.DayMinVal = stockInfo[i].DayMinVal;
                }

                if (stockInfo[i].Day.IndexOf("100000") > 0)
                {
                    tmpPoint.DayMaxVal = stockInfo[i].DayMaxVal;
                    tmpPoint.DayMinVal = stockInfo[i].DayMinVal;
                }
                else
                {
                    stockInfo[i].DayMaxVal = tmpPoint.DayMaxVal;
                    stockInfo[i].DayMinVal = tmpPoint.DayMinVal;
                }
            }

            // 开始比较
            int chkVal = 0;
            int lastChkVal = 0;
            int lastTopPos = -1;
            int lastBottomPos = -1;
            bool buyed = false;
            string buyDate = string.Empty;
            decimal buyPrice = 0;
            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
            tmpPoint.DayMinVal = lastPoint.DayMinVal;
            for (int i = maxCnt; i >= 0; i--)
            {
                // 判断两个点的大小关系
                chkVal = ChkPointsVal(stockInfo[i], tmpPoint);

                if (chkVal > 0 && lastChkVal < 0)
                {
                    // 当前上升，前面是下降，说明前一个点是低点
                    lastPoint.CurPointType = PointType.Bottom;

                    if (!buyed)
                    {
                        // 判断是否是第二类买点
                        lastBottomPos = GeBefBottomPos(stockInfo, lastIdx, maxCnt);
                        if (lastBottomPos > 0 && lastPoint.DayMinVal > stockInfo[lastBottomPos].DayMinVal * LIMIT_VAL)
                        {
                            // 当前低点高于上一个低点，设置第二类买点
                            stockInfo[i].BuySellFlg = 3;
                            buyed = true;
                            buyPrice = stockInfo[i].DayVal;
                            buyDate = stockInfo[i].Day.Substring(0, 8);
                        }
                    }
                }
                else if (chkVal < 0)
                {
                    if (lastChkVal > 0)
                    {
                        // 当前下降，前面是上升，说明前一个点是高点
                        lastPoint.CurPointType = PointType.Top;

                        if (buyed)
                        {
                            // 设置第一类卖点
                            lastTopPos = GeBefTopVal(stockInfo, lastIdx, maxCnt);
                            if (lastTopPos > 0 && stockInfo[i].DayVal * LIMIT_VAL < stockInfo[lastTopPos].DayMaxVal
                                && !stockInfo[i].Day.StartsWith(buyDate))
                            {
                                stockInfo[i].BuySellFlg = -1;
                                buyed = false;
                            }
                        }
                    }

                    // 只要下降到买点，就卖出
                    if (buyed && stockInfo[i].DayVal < buyPrice * LIMIT_VAL && !stockInfo[i].Day.StartsWith(buyDate))
                    {
                        stockInfo[i].BuySellFlg = -1;
                        buyed = false;
                    }
                }

                // 更新当前的点
                if (chkVal != 0)
                {
                    lastChkVal = chkVal;
                    lastPoint = stockInfo[i];
                    lastIdx = i;
                    tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
                    tmpPoint.DayMinVal = lastPoint.DayMinVal;
                }
            }

            return stockInfo;
        }

        /// <summary>
        /// 取得前一个顶分型的位置
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="idx"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        public static int GeBefTopVal(List<BaseDataInfo> fenXingInfo, int idx, int maxCnt)
        {
            for (int i = idx + 1; i < maxCnt; i++)
            {
                if (fenXingInfo[i].CurPointType == PointType.Top)
                {
                    //return fenXingInfo[i].DayVal;
                    //return fenXingInfo[i].DayMaxVal;
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 取得前一个底分型的位置
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="idx"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        public static int GeBefBottomPos(List<BaseDataInfo> fenXingInfo, int idx, int maxCnt)
        {
            for (int i = idx + 1; i < maxCnt; i++)
            {
                if (fenXingInfo[i].CurPointType == PointType.Bottom)
                {
                    //return fenXingInfo[i].DayVal;
                    //return fenXingInfo[i].DayMinVal;
                    return i;
                }
            }

            return -1;
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
                this.GetMinuteData(TimeRange.M5);
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
        private void GetMinuteData(TimeRange timeRange)
        {
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
                this.GetMinuteDataCommon(endDay, timeRange);

                tmp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 获取" + timeRange.ToString() + "数据 结束";
                Console.WriteLine(tmp);
                File.AppendAllText(logFile, tmp + "\r\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.Message + "\r\n", Encoding.UTF8);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + e.StackTrace + "\r\n", Encoding.UTF8);
            }
        }

        /// <summary>
        /// 取得分钟级别数据的共通
        /// </summary>
        private void GetMinuteDataCommon(string endDay, TimeRange timeRange)
        {
            // 取得已经存在的所有数据信息
            this.allCsv = Util.GetAllFiles(this.basePath + CSV_FOLDER + timeRange.ToString() + "/");

            // 获取所有的代码信息
            endDay = endDay.Replace("-", "").Replace(" ", "").Replace(":", "");
            this.getData = new GetDataFromSina(this.basePath + CSV_FOLDER, endDay, timeRange);

            // 设置进度条
            DosProgressBar dosProgressBar = new DosProgressBar();
            int idx = 0;
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
                this.allCsv = Util.GetAllFiles(this.basePath + CSV_FOLDER + timeRange.ToString() + "/");

                // 获取所有的代码信息
                endDay = endDay.Replace("-", "").Replace(" ", "").Replace(":", "");
                this.getData = new GetDataFromSina(this.basePath + CSV_FOLDER, endDay, timeRange);

                // 设置进度条
                DosProgressBar dosProgressBar = new DosProgressBar();
                int idx = 0;
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
            List<FilePosInfo> allCsv = Util.GetAllFiles(this.basePath + CSV_FOLDER + DAY_FOLDER);

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFrom163(this.basePath + CSV_FOLDER + DAY_FOLDER, endDay);

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
            // 画5分钟趋势图
            if (hasM5)
            {
                this.DrawQushiImg(TimeRange.M5);
            }

            // 画15分钟趋势图
            if (hasM15)
            {
                //this.DrawQushiImg(TimeRange.M15);
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
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " 画" + timeRange.ToString() + "趋势图 开始\r\n", Encoding.UTF8);

                // 取得已经存在的所有数据信息
                this.subFolder = timeRange.ToString() + "/";
                List<FilePosInfo> allCsv = Util.GetAllFiles(this.basePath + CSV_FOLDER + this.subFolder);

                // 设置进度条
                if (this.callBef != null)
                {
                    this.callBef(allCsv.Count);
                }

                foreach (FilePosInfo fileItem in allCsv)
                {
                    if (fileItem.IsFolder)
                    {
                        continue;
                    }

                    this.currentFile = fileItem.File;

                    try
                    {
                        this.CreateQushiImg(Util.GetShortNameWithoutType(fileItem.File), timeRange);
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
            decimal step = 370 / (minMaxInfo[1] - minMaxInfo[0]);

            // 设定图片
            Bitmap imgQushi = new Bitmap((stockInfos.Count + 2) * IMG_X_STEP, 400);
            Graphics grp = Graphics.FromImage(imgQushi);
            grp.SmoothingMode = SmoothingMode.AntiAlias;
            grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 开始当前的线（5分，15分，30分，天）
            this.DrawStockQushi(stockInfos, step, minMaxInfo[0], imgQushi, new Pen(Color.Black, 1F), grp);

            // 画均线
            if (timeRange == TimeRange.Day)
            {
                // 取得5日级别信息
                List<BaseDataInfo> stockInfo5Jibie = GetJibieStockInfo(stockInfos, 5);

                // 开始画5日线
                if (stockInfo5Jibie.Count > 0)
                {
                    this.DrawStockQushi(stockInfo5Jibie, step, minMaxInfo[0], imgQushi, new Pen(Color.Green, 1F), grp);
                }

                /*
                // 取得10日级别信息
                List<BaseDataInfo> stockInfo10Jibie = GetJibieStockInfo(stockInfos, 10);

                // 开始画10日线
                if (stockInfo10Jibie.Count > 0)
                {
                    this.DrawStockQushi(stockInfo10Jibie, step, minMaxInfo[0], imgQushi, new Pen(Color.Red, 1F), grp);
                }*/
            }

            // 开始画分型、笔的线段
            if (timeRange == TimeRange.M30)
            {
                List<BaseDataInfo> fenXingInfo = SetFenxingInfoDayM30(stockInfos);
                this.DrawFenxingPen(fenXingInfo, step, minMaxInfo[0], imgQushi, new Pen(Color.DarkOrange, 1F), grp, IMG_X_STEP);
            }
            else
            {
                List<BaseDataInfo> fenXingInfo = SetFenxingInfo(stockInfos);
                this.DrawFenxingPen(fenXingInfo, step, minMaxInfo[0], imgQushi, new Pen(Color.DarkOrange, 1F), grp, IMG_X_STEP);
            }

            // 在5,15分钟的分型图上画天的分型信息
            //if (timeRange == TimeRange.M5 || timeRange == TimeRange.M15)
            //{
            //    dataInfo = GetStockInfo(stockCdData.Substring(0, 15), TimeRange.Day.ToString() + "/", this.basePath);
            //    if (dataInfo != null)
            //    {
            //        // 基础数据信息
            //        stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
            //        if (stockInfos.Count > 0)
            //        {
            //            // 开始画分型、笔的线段
            //            fenXingInfo = SetFenxingInfo(stockInfos);
            //            this.DrawFenxingPen(fenXingInfo, step, minMaxInfo[0], imgQushi, new Pen(Color.DarkGreen, 1F), grp, timeRange == TimeRange.M5 ? IMG_X_STEP * 48 : IMG_X_STEP * 16);
            //        }
            //    }
            //}

            // 保存图片
            imgQushi.Save(this.basePath + IMG_FOLDER + this.subFolder + stockCdData.Substring(0, 6) + ".png");

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
        private void DrawStockQushi(List<BaseDataInfo> stockInfo, decimal step, decimal minVal, Bitmap img, Pen pen, Graphics grp)
        {
            int startX = img.Width - IMG_X_STEP;
            int x1 = startX;
            int y1 = this.GetYPos(img.Height, stockInfo[0].DayVal, minVal, step);
            int x2 = 0;
            int y2 = 0;
            int index = 1;
            while (index <= stockInfo.Count - 1)
            {
                x2 = startX - IMG_X_STEP;
                y2 = this.GetYPos(img.Height, stockInfo[index].DayVal, minVal, step);

                grp.DrawLine(pen, x1, y1, x2, y2);
                x1 = x2;
                y1 = y2;

                index++;
                startX -= IMG_X_STEP;
            }

            // 释放资源
            pen.Dispose();
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
            return imgH - ((int)((pointVal - minVal) * step)) - 10;
        }

        /// <summary>
        /// 取得所有数据的基本信息（代码）
        /// </summary>
        private void GetAllStockBaseInfo()
        {
            this.allStockCd.Clear();

            string[] allLine = File.ReadAllLines(this.basePath + CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
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

        #region " 分型处理相关 "

        /// <summary>
        /// 开始分型笔的线段
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private bool DrawFenxingPen(List<BaseDataInfo> fenXingInfo, decimal step, decimal minVal, Bitmap img, Pen pen, Graphics grp, int imgXStep)
        {
            if (fenXingInfo.Count == 0)
            {
                return false;
            }

            int x1 = img.Width - ((fenXingInfo.Count - 1) * imgXStep + IMG_X_STEP);
            int y1 = this.GetYPos(img.Height, fenXingInfo[fenXingInfo.Count - 1].DayVal, minVal, step);
            int x2 = 0;
            int y2 = 0;
            decimal curVal;
            int maxCnt = fenXingInfo.Count - 2;
            Brush buyBush = new SolidBrush(Color.Red);
            Brush sellBush = new SolidBrush(Color.Green);
            Font font = new Font(new FontFamily("Microsoft YaHei"), 6, FontStyle.Bold);
            bool hasBuyPoint = false;
            bool buyed = false;

            for (int index = maxCnt; index >= 0; index--)
            {
                x2 = img.Width - (index * imgXStep + IMG_X_STEP);

                if (fenXingInfo[index].CurPointType != PointType.Changing || index == 0)
                {
                    curVal = fenXingInfo[index].CurPointType == PointType.Top ? fenXingInfo[index].DayMaxVal : fenXingInfo[index].DayMinVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, step);

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
                    y2 = this.GetYPos(img.Height, curVal, minVal, step);

                    grp.DrawString("B", font, buyBush, x2, y2);
                    buyed = true;
                }
                else if (fenXingInfo[index].BuySellFlg < 0 && buyed)
                {
                    curVal = fenXingInfo[index].DayVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, step);

                    grp.DrawString("T", font, sellBush, x2, y2);
                    buyed = false;
                }
            }

            // 释放资源
            pen.Dispose();

            return hasBuyPoint;
        }

        /// <summary>
        /// 取得没有包括关系的值的位置
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        private static int GetNotIncludeInfo(List<BaseDataInfo> stockInfo, int idx)
        {
            BaseDataInfo curItem = stockInfo[idx];

            while (idx >= 0)
            {
                if (idx == 0)
                {
                    return 0;
                }

                BaseDataInfo nextItem = stockInfo[idx - 1];

                // 判断是否变化
                if ((curItem.DayVal > nextItem.DayVal && (curItem.DayVal - nextItem.DayVal) <= curItem.DayVal * (LIMIT_VAL - 1))
                    || (curItem.DayVal < nextItem.DayVal && (nextItem.DayVal - curItem.DayVal) <= nextItem.DayVal * (LIMIT_VAL - 1)))
                {
                    // 前后没有变化
                    idx--;
                    continue;
                }

                break;
            }

            return idx;
        }

        /// <summary>
        /// 判断两个点的关系
        /// </summary>
        /// <param name="point2"></param>
        /// <param name="point1"></param>
        /// <returns></returns>
        private static int ChkPointsVal(BaseDataInfo point2, BaseDataInfo point1)
        {
            // 判断是否变化
            if (point2.DayMaxVal > point1.DayMaxVal * LIMIT_VAL
                && point2.DayMinVal > point1.DayMinVal * LIMIT_VAL)
            {
                return 1;
            }
            else if (point1.DayMaxVal > point2.DayMaxVal * LIMIT_VAL
                && point1.DayMinVal > point2.DayMinVal * LIMIT_VAL)
            {
                return -1;
            }
            else
            {
                // 前后有包含关系，或者没有变化
                point1.DayMaxVal = point2.DayMaxVal;
                return 0;
            }

            //// 判断是否变化
            //if (point2.DayVal > point1.DayVal)
            //{
            //    if ((point2.DayVal - point1.DayVal) <= point1.DayVal * (LIMIT_VAL - 1))
            //    {
            //        // 前后没有变化
            //        return 0;
            //    }
            //    else
            //    {
            //        return 1;
            //    }
            //}
            //else if (point2.DayVal < point1.DayVal)
            //{
            //    if ((point1.DayVal - point2.DayVal) <= point2.DayVal * (LIMIT_VAL - 1))
            //    {
            //        // 前后没有变化
            //        return 0;
            //    }
            //    else
            //    {
            //        return -1;
            //    }
            //}
            //else
            //{
            //    // 前后没有变化
            //    return 0;
            //}
        }

        /// <summary>
        /// 查找更低的地点
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="lastBottomPos"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        private static bool hasMoreLowBottom(List<BaseDataInfo> fenXingInfo, int lastBottomPos, int maxCnt)
        {
            for (int i = lastBottomPos + 1; i < maxCnt; i++)
            {
                if (fenXingInfo[i].CurPointType == PointType.Bottom)
                {
                    if (fenXingInfo[i].DayMinVal < fenXingInfo[lastBottomPos].DayMinVal)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        #endregion

        #endregion
    }
}
