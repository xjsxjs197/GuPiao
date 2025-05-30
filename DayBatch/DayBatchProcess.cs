﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Common;
using DataProcess.FenXing;
using DataProcess.GetData;
using Devart.Data.Oracle;
using System.Diagnostics;

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
        /// 设定信息
        /// </summary>
        private BuySellSetting configInfo;

        /// <summary>
        /// 画图时的信息
        /// </summary>
        private DrawImgInfo drawImgInfo;

        /// <summary>
        /// 保存所有的代码、名称映射信息
        /// </summary>
        private Dictionary<string, string> allStockCdName = new Dictionary<string, string>();

        /// <summary>
        /// 数据库连接
        /// </summary>
        private OracleConnection conn = null;

        private string connStr = string.Empty;

        private const int MaxThreads = 3; // 最大并发线程数
        private static readonly object LockObj = new object();
        private static int _completedTasks = 0;
        private static int _totalTasks = 0;
        private static int _activeThreads = 0;
        private bool hasError = false;

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
            bool backAndCheck = false;
            bool delInvalidData = false;
            bool copyM5ToDb = false;
            if (args == null || args.Length == 0)
            {
                if (Util.IsHolidayByDate(DateTime.Now) > 0)
                {
                    string errLog = "节假日不用取数据，如果强制取得数据，需要命令后面追加各种参数";
                    Console.WriteLine();
                    Console.WriteLine(errLog);
                    File.AppendAllText(logFile, errLog + "\r\n", Encoding.UTF8);
                    return;
                }

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
                    else if ("backAndCheck".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        backAndCheck = true;
                    }
                    else if ("delInvalidData".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        delInvalidData = true;
                    }
                    else if ("copyM5ToDb".Equals(param, StringComparison.OrdinalIgnoreCase))
                    {
                        needGetData = false;
                        needDrawImg = false;
                        copyM5ToDb = true;
                    }
                }
            }

            if (backAndCheck)
            {
                // 备份并且取所有最新代码
                this.BackAndCheckData();
            }
            else if (delInvalidData)
            {
                // 删除无效数据
                this.CheckData();
            }
            else
            {
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

                if (copyM5ToDb)
                {
                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    string exeDirectory = Path.GetDirectoryName(exePath);

                    // 取得配置的DB信息
                    string[] dbAddrInfo = File.ReadAllLines(exeDirectory + @"\DbAddrInfo.txt");
                    this.connStr = dbAddrInfo[2];

                    // 创建数据连接
                    if (this.CreateDbConn(this.connStr))
                    {
                        // 复制M5的数据到数据库
                        this.ImportCsvToDb(exeDirectory, TimeRange.M5, "m5");
                    }
                }
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
        /// 根据StockCd取得相关数据信息
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetStockInfo(string stockCdData, string subFolder, string basePath, string endDate)
        {
            // 读取所有信息
            List<BaseDataInfo> stockInfos = GetStockHistoryInfo(basePath + Consts.CSV_FOLDER + subFolder + stockCdData + ".csv", endDate);
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
                maxPoints = MAX_POINTS * 8;
            }
            else if (stockFile.IndexOf(TimeRange.M15.ToString()) > 0)
            {
                maxPoints = MAX_POINTS * 16;
            }
            else if (stockFile.IndexOf(TimeRange.M5.ToString()) > 0)
            {
                maxPoints = MAX_POINTS * 48;
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
                    dayInfo.Day = Util.TrimDate(allItems[0]);

                    dayInfo.DayVal = decimal.Parse(allItems[3]);
                    dayInfo.DayMaxVal = decimal.Parse(allItems[4]);
                    dayInfo.DayMinVal = decimal.Parse(allItems[5]);

                    stockInfo.Add(dayInfo);
                }
            }

            return stockInfo;
        }

        /// <summary>
        /// 取得股票历史数据
        /// </summary>
        /// <param name="stockFile"></param>
        /// <returns></returns>
        public static List<BaseDataInfo> GetStockHistoryInfo(string stockFile, string endDate)
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
                maxPoints = MAX_POINTS * 8;
            }
            else if (stockFile.IndexOf(TimeRange.M15.ToString()) > 0)
            {
                maxPoints = MAX_POINTS * 16;
            }
            else if (stockFile.IndexOf(TimeRange.M5.ToString()) > 0)
            {
                maxPoints = MAX_POINTS * 48;
            }

            for (int i = 1; i < allLine.Length && maxPoints > 0; i++, maxPoints--)
            {
                if (allLine[i].IndexOf("Error") > 0)
                {
                    stockInfo.Clear();
                    return stockInfo;
                }

                string[] allItems = allLine[i].Split(',');
                if (allItems.Length > 3)
                {
                    string tmpDay = Util.TrimDate(allItems[0]);
                    if (string.Compare(tmpDay.Substring(0, 8), endDate) > 0)
                    {
                        maxPoints++;
                        continue;
                    }

                    BaseDataInfo dayInfo = new BaseDataInfo();
                    dayInfo.Day = tmpDay;
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
            decimal minVal = Consts.MAX_VAL;
            decimal maxVal = 0;

            while (index <= maxCount)
            {
                total = 0;
                minVal = Consts.MAX_VAL;
                maxVal = 0;
                for (int i = 0; i <= jibieCount; i++)
                {
                    total += stockInfos[index + i].DayVal;
                    maxVal = Math.Max(stockInfos[index + i].DayVal, maxVal);
                    minVal = Math.Min(stockInfos[index + i].DayVal, minVal);
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
        /// 画趋势图
        /// </summary>
        /// <param name="stockCdData"></param>
        public void CreateQushiImg(string stockCdData, TimeRange timeRange)
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
            List<List<BaseDataInfo>> drawQushiInfo = new List<List<BaseDataInfo>>();
            List<List<BaseDataInfo>> drawFenXingInfo = new List<List<BaseDataInfo>>();
            //decimal yStep = Util.GetYstep(minMaxInfo);

            // 设定图片
            Bitmap imgQushi = new Bitmap((stockInfos.Count + 2) * Consts.IMG_X_STEP, Consts.IMG_H);
            Graphics grp = Graphics.FromImage(imgQushi);
            grp.SmoothingMode = SmoothingMode.AntiAlias;
            grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            string stockCd = stockCdData.Substring(0, 6);
            //Metafile mf = new Metafile(this.basePath + Consts.IMG_FOLDER + this.subFolder + stockCd + ".wmf", grp.GetHdc());
            //grp = Graphics.FromImage(mf);

            // 开始当前的线（5分，15分，30分，天）
            //this.DrawStockQushi(stockInfos, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.BlackLinePen, grp, true);
            drawQushiInfo.Add(stockInfos);

            // 画均线
            decimal[] newMinMaxInfo;
            if (timeRange == TimeRange.Day)
            {
                // 取得5日均线信息
                List<BaseDataInfo> stockInfo5Jibie = GetAverageLineInfo(stockInfos, 5);

                // 开始画5日均线
                if (stockInfo5Jibie.Count > 0)
                {
                    //this.DrawStockQushi(stockInfo5Jibie, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.GreenLinePen, grp, false);
                    newMinMaxInfo = Util.GetMaxMinStock(stockInfo5Jibie);
                    minMaxInfo[0] = Math.Min(minMaxInfo[0], newMinMaxInfo[0]);
                    minMaxInfo[1] = Math.Max(minMaxInfo[1], newMinMaxInfo[1]);
                    drawQushiInfo.Add(stockInfo5Jibie);
                }
            }
            //else if (timeRange == TimeRange.M30)
            //{
            //    // 取得日均线信息
            //    List<BaseDataInfo> stockInfoDayJibie = GetAverageLineInfo(stockInfos, 8);

            //    // 开始画日均线
            //    if (stockInfoDayJibie.Count > 0)
            //    {
            //        //this.DrawStockQushi(stockInfoDayJibie, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.GreenLinePen, grp, false);
            //        decimal[] newMinMaxInfo = Util.GetMaxMinStock(stockInfoDayJibie);
            //        minMaxInfo[0] = Math.Min(minMaxInfo[0], newMinMaxInfo[0]);
            //        minMaxInfo[1] = Math.Max(minMaxInfo[1], newMinMaxInfo[1]);
            //        drawQushiInfo.Add(stockInfoDayJibie);
            //    }
            //}

            // 开始画分型、笔的线段
            List<BaseDataInfo> fenXingInfo = null;
            FenXing fenXing = new FenXing(false);
            switch (timeRange)
            {
                case TimeRange.M30:
                    fenXingInfo = fenXing.DoFenXingSp(stockInfos, this.configInfo, "100000", minMaxInfo);
                    break;

                case TimeRange.M15:
                    fenXingInfo = fenXing.DoFenXingSp(stockInfos, this.configInfo, "094500", minMaxInfo);
                    break;

                case TimeRange.M5:
                    fenXingInfo = fenXing.DoFenXingSp(stockInfos, this.configInfo, "093500", minMaxInfo);
                    break;

                default:
                    fenXing.SetCheckPoint(true);
                    fenXingInfo = fenXing.DoFenXingComn(stockInfos);
                    break;
            }
            if (fenXingInfo != null)
            {
                drawFenXingInfo.Add(fenXingInfo);
                newMinMaxInfo = Util.GetMaxMinStock(fenXingInfo);
                minMaxInfo[0] = Math.Min(minMaxInfo[0], newMinMaxInfo[0]);
                minMaxInfo[1] = Math.Max(minMaxInfo[1], newMinMaxInfo[1]);
            }

            //this.DrawFenxingPen(fenXingInfo, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkOrangeLinePen, grp, Consts.IMG_X_STEP);

            // 在30分钟的分型图上画天的分型信息
            if (timeRange == TimeRange.M30)
            {
                dataInfo = GetStockInfo(stockCdData.Substring(0, 15), TimeRange.Day.ToString() + "/", this.basePath);
                if (dataInfo != null)
                {
                    // 基础数据信息
                    stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
                    newMinMaxInfo = (decimal[])dataInfo["minMaxInfo"];
                    minMaxInfo[0] = Math.Min(minMaxInfo[0], newMinMaxInfo[0]);
                    minMaxInfo[1] = Math.Max(minMaxInfo[1], newMinMaxInfo[1]);

                    if (stockInfos.Count > 0)
                    {
                        // 开始画分型、笔的线段
                        fenXing = new FenXing(true);
                        List<BaseDataInfo> newFenXingInfo = fenXing.DoFenXingComn(stockInfos);

                        // 检查所有第一类买点
                        ChkAllFirstBuyPoints(newFenXingInfo, minMaxInfo);

                        drawFenXingInfo.Add(newFenXingInfo);
                    }
                }
            }

            // 画均线趋势图
            decimal yStep = Util.GetYstep(minMaxInfo);
            for (int i = 0; i < drawQushiInfo.Count; i++)
            {
                if (i == 0)
                {
                    this.DrawStockQushi(drawQushiInfo[0], yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.BlackLinePen, grp, true);
                }
                else
                {
                    this.DrawStockQushi(drawQushiInfo[i], yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.GreenLinePen, grp, false);
                }
            }

            // 画中枢图
            for (int i = 0; i < drawFenXingInfo.Count; i++)
            {
                if (i == 0)
                {
                    this.DrawZhongShu(drawFenXingInfo[0], yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkOrangeLinePen, grp, Consts.IMG_X_STEP);
                }
                else
                {
                    this.DrawZhongShu(drawFenXingInfo[i], yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkBlueLinePen, grp, Consts.IMG_X_STEP * 8);
                }
            }

            // 写名称
            string stockNm = string.Empty;
            if (this.allStockCdName.ContainsKey(stockCd))
            {
                stockNm = this.allStockCdName[stockCd];
            }
            else
            {
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + this.currentFile + "\r\n", Encoding.UTF8);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + stockCd + "对应的名称不存在！\r\n", Encoding.UTF8);
            }

            grp.DrawString(stockCd + "_" + stockNm + "  " + stockInfos[0].DayVal.ToString(), this.drawImgInfo.NameFont,
                                this.drawImgInfo.BlueVioletBush, 10, 10);

            // 保存图片
            grp.Save();
            imgQushi.Save(this.basePath + Consts.IMG_FOLDER + this.subFolder + stockCd + Consts.IMG_TYPE, ImageFormat.Png);

            // 释放Graphics和图片资源
            grp.Dispose();
            //mf.Dispose();
            imgQushi.Dispose();
        }

        /// <summary>
        /// 保存指定日期范围的趋势图
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        public void SaveM30QushiImg(string stockCdData, string dtStart, string dtEnd)
        {
            DateTime startDt = Convert.ToDateTime(dtStart);
            DateTime endDt = Convert.ToDateTime(dtEnd);
            bool firstFlg = true;
            string stockCd = stockCdData.Substring(0, 6);
            List<List<BaseDataInfo>> drawFenXingInfo = new List<List<BaseDataInfo>>();
            decimal[] minMaxInfo = null;
            dtEnd = dtEnd.Replace("/", "");

            while (endDt >= startDt)
            {
                // 获得M30数据信息
                Dictionary<string, object> dataInfoM30 = GetStockInfo(stockCdData, TimeRange.M30.ToString() + "/", this.basePath, dtEnd);
                if (dataInfoM30 != null)
                {
                    // 基础数据信息
                    List<BaseDataInfo> stockInfosM30 = (List<BaseDataInfo>)dataInfoM30["stockInfos"];
                    if (stockInfosM30.Count > 0)
                    {
                        drawFenXingInfo.Clear();

                        // 设定图片
                        Bitmap imgQushi = new Bitmap((stockInfosM30.Count + 2) * Consts.IMG_X_STEP, Consts.IMG_H);
                        Graphics grp = Graphics.FromImage(imgQushi);
                        grp.SmoothingMode = SmoothingMode.AntiAlias;
                        grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                        // 最大、最小值信息
                        if (firstFlg)
                        {
                            minMaxInfo = (decimal[])dataInfoM30["minMaxInfo"];
                        }

                        // 开始画M30分型
                        FenXing fenXing = new FenXing(false);
                        List<BaseDataInfo> fenXingInfo = fenXing.DoFenXingSp(stockInfosM30, this.configInfo, "100000", minMaxInfo);
                        drawFenXingInfo.Add(fenXingInfo);

                        // 获得天数据信息
                        Dictionary<string, object> dataInfoDay = GetStockInfo(stockCdData.Substring(0, 15), TimeRange.Day.ToString() + "/", this.basePath, dtEnd);
                        if (dataInfoDay != null)
                        {
                            // 基础数据信息
                            List<BaseDataInfo> stockInfosDay = (List<BaseDataInfo>)dataInfoDay["stockInfos"];
                            if (firstFlg)
                            {
                                decimal[] newMinMaxInfo = (decimal[])dataInfoDay["minMaxInfo"];
                                minMaxInfo[0] = Math.Min(minMaxInfo[0], newMinMaxInfo[0]);
                                minMaxInfo[1] = Math.Max(minMaxInfo[1], newMinMaxInfo[1]);
                            }

                            if (stockInfosDay.Count > 0)
                            {
                                // 开始画分型、笔的线段
                                fenXing = new FenXing(true);
                                List<BaseDataInfo> newFenXingInfo = fenXing.DoFenXingComn(stockInfosDay);

                                // 检查所有第一类买点
                                ChkAllFirstBuyPoints(newFenXingInfo, minMaxInfo);

                                drawFenXingInfo.Add(newFenXingInfo);
                            }
                        }

                        firstFlg = false;

                        // 画30分钟趋势图、分型图和天的分型图
                        decimal yStep = Util.GetYstep(minMaxInfo);
                        this.DrawStockQushi(stockInfosM30, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.BlackLinePen, grp, true);
                        this.DrawZhongShu(drawFenXingInfo[0], yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkOrangeLinePen, grp, Consts.IMG_X_STEP);
                        this.DrawZhongShu(drawFenXingInfo[1], yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkBlueLinePen, grp, Consts.IMG_X_STEP * 8);

                        // 写名称
                        string stockNm = string.Empty;
                        if (this.allStockCdName.ContainsKey(stockCd))
                        {
                            stockNm = this.allStockCdName[stockCd];
                        }
                        else
                        {
                            File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + this.currentFile + "\r\n", Encoding.UTF8);
                            File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + stockCd + "对应的名称不存在！\r\n", Encoding.UTF8);
                        }

                        grp.DrawString(stockCd + "_" + stockNm + "  " + stockInfosM30[0].DayVal.ToString(), this.drawImgInfo.NameFont,
                                            this.drawImgInfo.BlueVioletBush, 10, 10);

                        // 保存图片
                        grp.Save();
                        string folder = this.basePath + Consts.RESULT_FOLDER + "历史趋势/" + stockCd + "/";
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        imgQushi.Save(folder + dtEnd + Consts.IMG_TYPE, ImageFormat.Png);

                        // 释放Graphics和图片资源
                        grp.Dispose();
                        imgQushi.Dispose();
                    }
                }

                endDt = endDt.AddDays(-1);
                dtEnd = endDt.ToString("yyyyMMdd");
            }
        }

        /// <summary>
        /// 查看两点的趋势差异
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        public string ViewTwoPointQushiImg(string stockCdData, string dtStart, string dtEnd)
        {
            string stockCd = stockCdData.Substring(0, 6);
            List<List<BaseDataInfo>> drawFenXingInfo = new List<List<BaseDataInfo>>();
            decimal[] minMaxInfo = null;
            dtEnd = dtEnd.Replace("/", "");
            dtStart = dtStart.Replace("/", "");

            // 获得M30数据信息
            Dictionary<string, object> dataInfoM30 = GetStockInfo(stockCdData, TimeRange.M30.ToString() + "/", this.basePath, dtEnd);
            Dictionary<string, object> startDataInfoM30 = GetStockInfo(stockCdData, TimeRange.M30.ToString() + "/", this.basePath, dtStart);
            if (dataInfoM30 != null && startDataInfoM30 != null)
            {
                // 基础数据信息
                List<BaseDataInfo> stockInfosM30 = (List<BaseDataInfo>)dataInfoM30["stockInfos"];
                List<BaseDataInfo> startStockInfosM30 = (List<BaseDataInfo>)startDataInfoM30["stockInfos"];
                if (stockInfosM30.Count > 0 && startStockInfosM30.Count > 0)
                {
                    // 设定图片
                    Bitmap imgQushi = new Bitmap((stockInfosM30.Count + 2) * Consts.IMG_X_STEP, Consts.IMG_H);
                    Graphics grp = Graphics.FromImage(imgQushi);
                    grp.SmoothingMode = SmoothingMode.AntiAlias;
                    grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    // 最大、最小值信息
                    minMaxInfo = (decimal[])dataInfoM30["minMaxInfo"];

                    // 取得结束时间点的30分型数据
                    FenXing fenXing = new FenXing(false);
                    List<BaseDataInfo> fenXingInfo = fenXing.DoFenXingSp(stockInfosM30, this.configInfo, "100000", minMaxInfo);

                    // 画30分钟趋势图
                    decimal yStep = Util.GetYstep(minMaxInfo);
                    this.DrawStockQushi(stockInfosM30, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.BlackLinePen, grp, true);

                    // 取得开始时间点的30分型数据
                    List<BaseDataInfo> startM30FenXing = fenXing.DoFenXingSp(startStockInfosM30, this.configInfo, "100000", minMaxInfo);

                    // 画30分钟分型图，开始，结束时间在一张图上
                    this.DrawZhongShu2(fenXingInfo, startM30FenXing, yStep, minMaxInfo[0], imgQushi, this.drawImgInfo.DarkOrangeLinePen, grp, Consts.IMG_X_STEP);

                    // 写名称
                    string stockNm = string.Empty;
                    if (this.allStockCdName.ContainsKey(stockCd))
                    {
                        stockNm = this.allStockCdName[stockCd];
                    }
                    else
                    {
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + this.currentFile + "\r\n", Encoding.UTF8);
                        File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + stockCd + "对应的名称不存在！\r\n", Encoding.UTF8);
                    }

                    grp.DrawString(stockCd + "_" + stockNm + "  " + stockInfosM30[0].DayVal.ToString(), this.drawImgInfo.NameFont,
                                        this.drawImgInfo.BlueVioletBush, 10, 10);

                    // 保存图片
                    grp.Save();
                    string folder = this.basePath + Consts.RESULT_FOLDER + "历史趋势/" + stockCd + "/";
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    string imgName = folder + dtEnd + Consts.IMG_TYPE;
                    imgQushi.Save(imgName, ImageFormat.Png);

                    // 释放Graphics和图片资源
                    grp.Dispose();
                    imgQushi.Dispose();

                    return imgName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 检查所有第一类买点
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <returns></returns>
        public static bool ChkAllFirstBuyPoints(List<BaseDataInfo> fenXingInfo, decimal[] minMaxInfo)
        {
            int qushiDays = 0;
            int chkDate = 0;
            decimal bottom1 = 0;
            decimal bottom2 = 0;
            int bottom1Pos = 0;
            int firstBottomIdx = 0;
            int lastBottomIdx = 0;
            decimal top1 = 0;
            decimal top2 = 0;
            decimal UP_DOWN_DIFF = 1.15M;
            int CHK_MIN_DATE = 3;
            bool hasBuyPoint = false;
            int QUSHI_STRONGTH = 35;

            for (int i = 0; i < fenXingInfo.Count; i++)
            {
                if (qushiDays == 0)
                {
                    if (fenXingInfo[i].PointType == PointType.Bottom)
                    {
                        bottom1 = fenXingInfo[i].DayMinVal;
                        qushiDays++;

                        bottom1Pos = i;
                        firstBottomIdx = i;
                    }
                }
                else if (qushiDays == 1)
                {
                    if (fenXingInfo[i].PointType == PointType.Top)
                    {
                        top1 = fenXingInfo[i].DayMaxVal;
                        qushiDays++;
                    }
                }
                else if (qushiDays == 2)
                {
                    if (fenXingInfo[i].PointType == PointType.Bottom)
                    {
                        bottom2 = fenXingInfo[i].DayMinVal;
                        qushiDays++;

                        lastBottomIdx = i;
                    }
                }
                else if (qushiDays == 3)
                {
                    if (fenXingInfo[i].PointType == PointType.Top)
                    {
                        top2 = fenXingInfo[i].DayMaxVal;
                        if (bottom1 * UP_DOWN_DIFF < bottom2 && top1 * UP_DOWN_DIFF < top2)
                        {
                            chkDate = 0;
                            while (--firstBottomIdx > 0)
                            {
                                chkDate++;
                                if (chkDate <= CHK_MIN_DATE)
                                {
                                    if (fenXingInfo[firstBottomIdx].CheckPoint == -1)
                                    {
                                        if (GetQushiStrongth(bottom1Pos, firstBottomIdx, fenXingInfo[bottom1Pos].DayVal, fenXingInfo[firstBottomIdx].DayVal, 
                                            fenXingInfo.Count, minMaxInfo) > QUSHI_STRONGTH)
                                        {
                                            fenXingInfo[firstBottomIdx].BuySellFlg = 1;
                                            hasBuyPoint = true;
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }

                            }
                            qushiDays = 0;
                        }
                        else
                        {
                            i = lastBottomIdx;
                            firstBottomIdx = i;
                            bottom1Pos = i;
                            bottom1 = fenXingInfo[i].DayMinVal;
                            qushiDays = 1;
                        }
                    }
                }
            }

            return hasBuyPoint;
        }

        #endregion

        #region " 主要处理 "
        
        /// <summary>
        /// 取数据
        /// </summary>
        private void GetData(bool hasM5, bool hasM15, bool hasM30, bool hasDay)
        {
            List<string> stopCdList = this.CheckTingPaiData();

            // 获取5分钟数据
            if (hasM5)
            {
                bool isSuccess = this.GetMinuteData(TimeRange.M5);
                if (!isSuccess)
                {
                    // 失败了，再来一次
                    this.GetMinuteData(TimeRange.M5);
                }

                // 检查数据正确性
                this.CheckData(TimeRange.M5, stopCdList);
            }

            // 获取15分钟数据
            if (hasM15)
            {
                //this.CopyDataFromM5(TimeRange.M15);
            }

            // 获取30分钟数据
            if (hasM30)
            {
                this.CopyDataFromM5(TimeRange.M30);
                //this.GetMinuteData(TimeRange.M30);

                // 检查数据正确性
                this.CheckData(TimeRange.M30, stopCdList);
            }

            // 获取整天的数据
            if (hasDay)
            {
                this.CopyDataFromM5(TimeRange.Day);
                //this.GetMinuteData(TimeRange.Day);

                // 检查数据正确性
                this.CheckData(TimeRange.Day, stopCdList);
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
            endDay = Util.TrimDate(endDay);
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

                    Thread.Sleep(1500);
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
                endDay = Util.TrimDate(endDay);
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

        #endregion

        #region " 私有处理 "

        /// <summary>
        /// 写Log到数据库
        /// </summary>
        /// <param name="message"></param>
        private void WriteSqlLog(string message)
        {
            try
            {
                OracleCommand cmd = this.conn.CreateCommand();
                StringBuilder sb = new StringBuilder();

                sb.Length = 0;
                sb.Append("insert into LOG (log_date, log_info) VALUES (SYSDATE, '");
                sb.Append(message).Append("')");

                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                //MessageBox.Show(message + "\r\n" + e.Message + "\n" + e.StackTrace);
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + message + "\r\n" + e.Message + "\n" + e.StackTrace + "\r\n", Encoding.UTF8);
            }
        }

        /// <summary>
        /// 创建数据连接
        /// </summary>
        private bool CreateDbConn(string connString)
        {
            try
            {
                conn = new OracleConnection(connString);
                conn.Open();
                return true;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message + "\r\n" + e.StackTrace);
                File.AppendAllText(logFile, e.Message + "\r\n" + e.StackTrace + "\r\n", Encoding.UTF8);
                return false;
            }
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        private void CloseDbConn()
        {
            try
            {
                if (this.conn != null)
                {
                    this.conn.Close();
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message + "\r\n" + e.StackTrace);
                File.AppendAllText(logFile, e.Message + "\r\n" + e.StackTrace + "\r\n", Encoding.UTF8);
            }
        }

        /// <summary>
        /// 取得所有数据的最大日期
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllStockMaxDate()
        {
            Dictionary<string, string> allllStockMaxDate = new Dictionary<string, string>();
            try
            {
                OracleCommand insCmd = this.conn.CreateCommand();
                StringBuilder sb = new StringBuilder();

                sb.Length = 0;
                sb.Append("select to_char(max_trade_date, 'yyyy-MM-dd HH24:mi:ss'), stock_code from STOCK_INFO ");
                insCmd.CommandText = sb.ToString();
                insCmd.FetchSize = 5000;

                OracleDataReader dbResult = insCmd.ExecuteReader();
                if (dbResult != null)
                {
                    while (dbResult.Read())
                    {
                        allllStockMaxDate.Add(dbResult.GetString(1).ToString(), dbResult.GetString(0).ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteSqlLog("取得所有数据的最大日期时错误" + "\r\n" + ex.Message + "\n" + ex.StackTrace);
            }

            return allllStockMaxDate;
        }

        /// <summary>
        /// 等待空闲线程
        /// </summary>
        private void WaitForThreadSlot()
        {
            while (true)
            {
                lock (LockObj)
                {
                    if (_activeThreads < MaxThreads)
                    {
                        _activeThreads++;
                        return;
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 导入特定类型数据到Oracle
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="dbName"></param>
        private void ImportCsvToDb(string exeDirectory, TimeRange timeRange, string dbName)
        {
            // 取得已经存在的所有数据信息
            this.subFolder = timeRange.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(exeDirectory + "/" + Consts.CSV_FOLDER + this.subFolder).Where(p => !p.IsFolder).ToList();

            _totalTasks = allCsv.Count;

            // 创建线程池控制器
            using (var countdown = new ManualResetEvent(false))
            {
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + "CSV导入DB处理开始 " + allCsv.Count + "\r\n", Encoding.UTF8);

                Dictionary<string, string> allStockMaxDate = this.GetAllStockMaxDate();

                foreach (FilePosInfo fileItem in allCsv)
                {
                    string curStockCd = Util.GetShortNameWithoutType(fileItem.File).Substring(0, 6);
                    string curMaxDt = Util.GetShortNameWithoutType(fileItem.File).Substring(7);
                    // 比较当前CSV的数据，和数据库的最大时间数据
                    if (allStockMaxDate.ContainsKey(curStockCd))
                    {
                        if (curMaxDt.Equals(Util.TrimDate(allStockMaxDate[curStockCd])))
                        {
                            lock (LockObj)
                            {
                                if (++_completedTasks >= _totalTasks)
                                    countdown.Set();
                            }
                            continue;
                        }
                    }

                    // 等待可用线程
                    WaitForThreadSlot();

                    // 启动线程
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var csvFile = (string)state;
                        InsertStockData(csvFile, dbName, countdown);
                    }, fileItem.File);
                }

                // 等待所有线程完成
                countdown.WaitOne();

                if (hasError)
                {
                    //MessageBox.Show("出现错误 \n具体信息参考LOG表");
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + "CSV导入DB处理发生错误，具体信息参考Log表" + "\r\n", Encoding.UTF8);
                }
                else
                {
                    File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + "CSV导入DB处理成功完成" + "\r\n", Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// CSV的数据插入到数据库
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="dbName"></param>
        /// <param name="countdown"></param>
        private void InsertStockData(string csvFile, string dbName, ManualResetEvent countdown)
        {
            OracleConnection conn = null;
            StringBuilder sb = new StringBuilder();
            string stockCode = Util.GetShortNameWithoutType(csvFile).Substring(0, 6);
            string curMaxDt = Util.GetShortNameWithoutType(csvFile).Substring(7);

            try
            {
                if (!File.Exists(csvFile))
                {
                    return;
                }

                // 连接配置
                conn = new OracleConnection(this.connStr);
                conn.Open();

                OracleCommand insCmd = conn.CreateCommand();
                string maxDt = string.Empty;
                string maxDtChk = string.Empty;

                sb.Length = 0;
                sb.Append("select to_char(max(trade_date), 'yyyy-MM-dd HH24:mi:ss'), to_char(max(trade_date), 'yyyyMMddHH24miss') from ").Append(dbName);
                sb.Append(" Where stock_code = '").Append(stockCode).Append("'");
                insCmd.CommandText = sb.ToString();

                maxDt = string.Empty;
                OracleDataReader dbResult = insCmd.ExecuteReader();
                if (dbResult != null && dbResult.Read())
                {
                    if (!dbResult.GetOracleString(0).IsNull)
                    {
                        maxDt = dbResult.GetString(0).ToString();
                        maxDtChk = dbResult.GetString(1).ToString();
                    }
                }

                // 判断是否当前CSV已经导入了
                if (!maxDtChk.Equals(string.Empty) && Util.GetShortNameWithoutType(csvFile).EndsWith(maxDtChk))
                {
                    return;
                }

                //this.WriteSqlLog("Start Import " + stockCode);
                sb.Length = 0;
                int lineCnt = 0;

                // 读取CSV数据
                string[] allLine = File.ReadAllLines(csvFile);
                int maxLen = allLine.Length - 1;
                sb.Append(" BEGIN ");
                for (int i = 1; i <= maxLen; i++)
                {
                    // 2020-03-18 15:00:00,000001,,12.710,12.740,12.650,12.720
                    // datetime,code,name,close_val,max_val,min_val,open_val
                    string[] curLine = allLine[i].Split(',');
                    if (string.Compare(curLine[0], maxDt) > 0)
                    {
                        lineCnt++;

                        //sb.Append(" into ").Append(dbName).Append(" (");
                        sb.Append(" INSERT into ").Append(dbName).Append(" (");
                        sb.Append(" trade_date, stock_code, close_price, high_price, low_price, open_price) VALUES (");
                        sb.Append(" to_date('").Append(curLine[0]).Append("', 'yyyy-MM-dd HH24:mi:ss') ");
                        sb.Append(",'").Append(curLine[1].Replace("'", "")).Append("'");
                        sb.Append(",").Append(curLine[3]);
                        sb.Append(",").Append(curLine[4]);
                        sb.Append(",").Append(curLine[5]);
                        sb.Append(",").Append(curLine[6]);
                        sb.Append(") ;\r\n ");

                        if (lineCnt % 1000 == 0 && i < maxLen)
                        {
                            sb.Append(" COMMIT; END; ");
                            insCmd.CommandText = sb.ToString();
                            insCmd.ExecuteNonQuery();
                            sb.Length = 0;
                            sb.Append(" BEGIN ");

                            lineCnt = 0;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (lineCnt > 0)
                {
                    sb.Append(" COMMIT; END; ");
                    insCmd.CommandText = sb.ToString();
                    insCmd.ExecuteNonQuery();
                }

                sb.Length = 0;
                sb.Append("update STOCK_INFO ");
                sb.Append(" set max_trade_date = to_date('").Append(curMaxDt).Append("', 'YYYYMMDDHH24MISS') ");
                sb.Append(" Where stock_code = '").Append(stockCode).Append("'");
                insCmd.CommandText = sb.ToString();
                insCmd.ExecuteNonQuery();

                //this.WriteSqlLog("End Import " + stockCode);
            }
            catch (Exception ex)
            {
                hasError = true;
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + ex.Message + "\r\n" + ex.StackTrace + "\r\n", Encoding.UTF8);
                this.WriteSqlLog(stockCode + "\r\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
                // 任务完成标记
                lock (LockObj)
                {
                    _activeThreads--;
                    if (++_completedTasks >= _totalTasks)
                        countdown.Set();
                }
            }
        }

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
            int strYPos = img.Height - 15;
            bool drawDate = true;

            while (index <= maxCount)
            {
                x2 = x1 - Consts.IMG_X_STEP;
                y2 = this.GetYPos(img.Height, stockInfo[index].DayVal, minVal, yStep);

                grp.DrawLine(pen, x1, y1, x2, y2);
                x1 = x2;
                y1 = y2;

                if (needJiangeLine)
                {
                    if (((index - 7) & 7) == 0)
                    {
                        grp.DrawLine(this.drawImgInfo.NormalLinePen, x2, 0, x2, maxHeight);

                        if (drawDate)
                        {
                            grp.DrawString(stockInfo[index].Day.Substring(4, 4), this.drawImgInfo.NormalFont,
                                this.drawImgInfo.BlueVioletBush, x2, strYPos);
                            drawDate = false;
                        }
                        else
                        {
                            drawDate = true;
                        }
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
            this.allStockCdName.Clear();

            string[] allLine = File.ReadAllLines(this.basePath + Consts.CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
            if (allLine != null && allLine.Length > 0)
            {
                foreach (string codeName in allLine)
                {
                    if (string.IsNullOrEmpty(codeName))
                    {
                        continue;
                    }

                    string code = codeName.Substring(0, 6);
                    this.allStockCd.Add(code);
                    this.allStockCdName.Add(code, codeName.Substring(7));
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

            int holidayInfo = Util.IsHolidayByDate(dt);
            if (holidayInfo < 0)
            {
                this.WriteLog("取节假日信息失败", true);
            }
            while (holidayInfo > 0)
            {
                dt = dt.AddDays(-1);
                holidayInfo = Util.IsHolidayByDate(dt);
                if (holidayInfo < 0)
                {
                    this.WriteLog("取节假日信息失败", true);
                }
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
            this.drawImgInfo.NameFont = new Font(new FontFamily("Microsoft YaHei"), 16, FontStyle.Regular);

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

            this.GetAllStockBaseInfo();
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

            // 关闭数据库连接
            this.CloseDbConn();
        }

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="msg"></param>
        private void WriteLog(string msg)
        {
            File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + msg + "\r\n", Encoding.UTF8);
        }

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="msg"></param>
        private void WriteLog(string msg, bool needConsole)
        {
            if (needConsole)
            {
                Console.WriteLine();
                Console.WriteLine(msg + "\r\n");
            }

            this.WriteLog(msg);
        }

        /// <summary>
        /// 检查基础数据的正确性
        /// </summary>
        private void CheckData(TimeRange timeRange, List<string> stopCdList)
        {
            // 取得已经存在的所有数据信息
            this.subFolder = timeRange.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(this.basePath + Consts.CSV_FOLDER + this.subFolder);
            List<string> errData = new List<string>();

            // 设定结束日期
            DateTime now = this.GetNotWeeklyDayDate();
            string endDay = now.ToString("yyyy-MM-dd");

            this.WriteLog("开始检查" + timeRange.ToString() + "数据，共计：" + allCsv.Count, true);

            try
            {
                StringBuilder sb = new StringBuilder();
                string tmpCd = string.Empty;

                // 设置进度条
                DosProgressBar dosProgressBar = new DosProgressBar();
                int idx = 1;
                int totalLen = allCsv.Count;

                foreach (FilePosInfo fileItem in allCsv)
                {
                    if (fileItem.IsFolder)
                    {
                        continue;
                    }

                    string[] allLine = File.ReadAllLines(fileItem.File);
                    if (allLine.Length > 1)
                    {
                        if (!allLine[1].StartsWith(endDay))
                        {
                            //File.Delete(fileItem.File);
                            this.AddErrorData(errData, stopCdList, fileItem.File, allLine[1].Substring(0, 10));
                        }
                    }
                    else
                    {
                        //File.Delete(fileItem.File);
                        this.AddErrorData(errData, stopCdList, fileItem.File, allLine[1].Substring(0, 10));
                    }

                    // 更新进度条
                    dosProgressBar.Dispaly((int)((idx++ / (totalLen * 1.0)) * 100));
                }
            }
            catch (Exception e)
            {
                this.WriteLog(e.Message + "\r\n" + e.StackTrace, true);
            }

            File.WriteAllLines(this.basePath + Consts.CSV_FOLDER + "DataCheck/" + DateTime.Now.ToString("yyyyMMdd") + "_" + timeRange.ToString() + ".txt", errData.ToArray(), Encoding.UTF8);

            this.WriteLog("检查" + timeRange.ToString() + "数据完成，有" + errData.Count + "条数据有问题", true);
        }

        /// <summary>
        /// 添加错误的数据
        /// </summary>
        /// <param name="errData"></param>
        /// <param name="stopCdList"></param>
        /// <param name="stockCd"></param>
        private void AddErrorData(List<string> errData, List<string> stopCdList, string file, string curDate)
        {
            string shortName = Util.GetShortNameWithoutType(file);
            if (stopCdList.Contains(shortName.Substring(0, 6)))
            {
                errData.Add(shortName + " " + curDate + " 停牌");
            }
            else
            {
                errData.Add(shortName + " " + curDate);
            }
        }

        /// <summary>
        /// 过滤停牌的数据
        /// </summary>
        private List<string> CheckTingPaiData()
        {
            string url = "http://data.eastmoney.com/tfpxx/";
            string result = Util.HttpGet(url, string.Empty, Encoding.GetEncoding("GB2312"));
            List<string> stopCdList = new List<string>();
            if (!string.IsNullOrEmpty(result))
            {
                int idx = result.IndexOf("defjson:  {pages:1,data:[\"");
                if (idx <= 0)
                {
                    return stopCdList;
                }

                string tingpaiInfo = result.Substring(idx + 26, result.IndexOf("]},") - idx - 26);
                if (string.IsNullOrEmpty(tingpaiInfo))
                {
                    return stopCdList;
                }

                string[] tingpaiData = Regex.Split(tingpaiInfo, "\",\"", RegexOptions.IgnoreCase);
                for (int i = 0; i < tingpaiData.Length; i++)
                {
                    stopCdList.Add(tingpaiData[i].Substring(0, 6));
                }
            }

            return stopCdList;
        }

        /// <summary>
        /// 取得当前点时，趋势的强度（角度）
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="lastBottomPos"></param>
        /// <param name="idx"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        private static int GetQushiStrongth(int x1Pos, int x2Pos, decimal x1DayVal, decimal x2DayVal, int dataCnt, decimal[] minMaxInfo)
        {
            int xStep = Consts.IMG_X_STEP * 8;
            int imgWidth = (dataCnt + 2) * xStep;
            int x1 = imgWidth - (x1Pos * xStep + xStep);
            int x2 = imgWidth - (x2Pos * xStep + xStep);
            decimal step = Util.GetYstep(minMaxInfo);
            int y1 = (int)((x1DayVal - minMaxInfo[0]) * step);
            int y2 = (int)((x2DayVal - minMaxInfo[0]) * step);

            int retVal = (int)(Math.Atan2((y2 - y1), (x2 - x1)) * 180 / Math.PI);

            return retVal;
        }

        #region " 分型中枢处理相关 "

        /// <summary>
        /// 画中枢图
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="yStep"></param>
        /// <param name="minVal"></param>
        /// <param name="img"></param>
        /// <param name="pen"></param>
        /// <param name="grp"></param>
        /// <param name="xStep"></param>
        /// <returns></returns>
        private void DrawZhongShu(List<BaseDataInfo> fenXingInfo, decimal yStep, decimal minVal, Bitmap img, Pen pen, Graphics grp, int xStep)
        {
            if (fenXingInfo.Count == 0)
            {
                return;
            }

            // 画分型的笔
            this.DrawFenxingPen(fenXingInfo, yStep, minVal, img, pen, grp, xStep);

            // 根据分型画线段
            //this.DrawXianduan(fenXingInfo, yStep, minVal, img, this.drawImgInfo.RedLinePen, grp, xStep);
        }

        /// <summary>
        /// 画中枢图
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="yStep"></param>
        /// <param name="minVal"></param>
        /// <param name="img"></param>
        /// <param name="pen"></param>
        /// <param name="grp"></param>
        /// <param name="xStep"></param>
        /// <returns></returns>
        private void DrawZhongShu2(List<BaseDataInfo> fenXingEnd, List<BaseDataInfo> fenXingStart, decimal yStep, decimal minVal, Bitmap img, Pen pen, Graphics grp, int xStep)
        {
            if (fenXingEnd.Count == 0 || fenXingStart.Count == 0)
            {
                return;
            }

            // 画结束时间分型的笔
            int x1 = img.Width - ((fenXingEnd.Count - 1) * xStep + Consts.IMG_X_STEP);
            int y1 = this.GetYPos(img.Height, fenXingEnd[fenXingEnd.Count - 1].DayVal, minVal, yStep);
            this.DrawFenxingPen(fenXingEnd, yStep, minVal, img, pen, grp, xStep, x1, y1);

            // 画开始时间分型的笔
            this.DrawFenxingPen(fenXingStart, yStep, minVal, img, this.drawImgInfo.DarkBlueLinePen, grp, xStep, x1, y1);
        }

        /// <summary>
        /// 根据分型画线段
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="yStep"></param>
        /// <param name="minVal"></param>
        /// <param name="img"></param>
        /// <param name="pen"></param>
        /// <param name="grp"></param>
        /// <param name="xStep"></param>
        private void DrawXianduan(List<BaseDataInfo> fenXingInfo, decimal yStep, decimal minVal, Bitmap img, Pen pen, Graphics grp, int xStep)
        {
            int x1 = 0;
            int y1 = 0;
            int x2 = img.Width - ((fenXingInfo.Count - 1) * xStep + Consts.IMG_X_STEP);
            int y2 = 0;
            decimal curVal;
            int maxCnt = fenXingInfo.Count - 2;
            
            for (int index = maxCnt; index >= 0; index--)
            {
                x2 += xStep;
                if (fenXingInfo[index].PenType == PointType.Bottom || fenXingInfo[index].PenType == PointType.Top)
                {
                    curVal = fenXingInfo[index].PenType == PointType.Top ? fenXingInfo[index].DayMaxVal : fenXingInfo[index].DayMinVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, yStep);

                    if (x1 > 0)
                    {
                        grp.DrawLine(pen, x1, y1, x2, y2);
                    }

                    x1 = x2;
                    y1 = y2;
                }
            }
        }

        /// <summary>
        /// 开始分型笔的线段
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private bool DrawFenxingPen(List<BaseDataInfo> fenXingInfo, decimal yStep, decimal minVal, Bitmap img, Pen pen, Graphics grp, int xStep, int x1, int y1)
        {
            int x2 = x1;
            int y2 = 0;
            decimal curVal;
            int maxCnt = fenXingInfo.Count - 2;

            bool hasBuyPoint = false;
            bool buyed = false;

            for (int index = maxCnt; index >= 0; index--)
            {
                x2 += xStep;
                if (fenXingInfo[index].PointType == PointType.Bottom || fenXingInfo[index].PointType == PointType.Top || index == 0)
                {
                    if (fenXingInfo[index].PointType == PointType.Top)
                    {
                        curVal = fenXingInfo[index].DayMaxVal;
                    }
                    else if (fenXingInfo[index].PointType == PointType.Bottom)
                    {
                        curVal = fenXingInfo[index].DayMinVal;
                    }
                    else
                    {
                        curVal = fenXingInfo[index].DayVal;
                    }
                    y2 = this.GetYPos(img.Height, curVal, minVal, yStep);

                    if (index == 0)
                    {
                        pen.DashStyle = DashStyle.Dash;
                    }
                    grp.DrawLine(pen, x1, y1, x2, y2);
                    x1 = x2;
                    y1 = y2;
                }

                // 画检查点（到当前点时，判断出了前一个点的类型）
                //if (fenXingInfo[index].CheckPoint == -1)
                //{
                //    y2 = this.GetYPos(img.Height, fenXingInfo[index].DayVal, minVal, yStep);
                //    grp.FillEllipse(this.drawImgInfo.BuyBush, x2, y2, 5, 5);
                //}
                //else if (fenXingInfo[index].CheckPoint == 1)
                //{
                //    y2 = this.GetYPos(img.Height, fenXingInfo[index].DayVal, minVal, yStep);
                //    grp.FillEllipse(this.drawImgInfo.SellBush, x2, y2, 5, 5);
                //}

                if (fenXingInfo[index].BuySellFlg > 0)
                {
                    curVal = fenXingInfo[index].DayVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, yStep);

                    grp.DrawString("B", this.drawImgInfo.BuySellFont, this.drawImgInfo.BuyBush, x2, y2);
                    buyed = true;
                }
                //else if (fenXingInfo[index].BuySellFlg < 0 && buyed)
                //{
                //    curVal = fenXingInfo[index].DayVal;
                //    y2 = this.GetYPos(img.Height, curVal, minVal, yStep);

                //    grp.DrawString("T", this.drawImgInfo.BuySellFont, this.drawImgInfo.SellBush, x2, y2);
                //    buyed = false;
                //}
            }

            return hasBuyPoint;
        }

        /// <summary>
        /// 开始分型笔的线段
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private bool DrawFenxingPen(List<BaseDataInfo> fenXingInfo, decimal yStep, decimal minVal, Bitmap img, Pen pen, Graphics grp, int xStep)
        {
            int x1 = img.Width - ((fenXingInfo.Count - 1) * xStep + Consts.IMG_X_STEP);
            int y1 = this.GetYPos(img.Height, fenXingInfo[fenXingInfo.Count - 1].DayVal, minVal, yStep);

            return this.DrawFenxingPen(fenXingInfo, yStep, minVal, img, pen, grp, xStep, x1, y1);
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
            FenXing fenXing = new FenXing(false);
            List<BaseDataInfo> fenxingInfo = fenXing.DoFenXingSp(stockInfos, emuInfo, startTime, minMaxInfo);
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

        #region 备份并且取所有最新代码

        /// <summary>
        /// 备份并且取所有最新代码
        /// </summary>
        private void BackAndCheckData()
        {
            this.WriteLog("开始备份数据......", true);

            // 备份数据
            string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            Util.CompressRarOrZip(baseDir + Consts.CSV_FOLDER, baseDir + "Data" + DateTime.Now.ToString("yyyyMMdd") + ".rar", true, "");

            this.WriteLog("备份数据结束......", true);

            // 检查所有可用的代码
            this.CheckAllCd();
        }
        
        /// <summary>
        /// 检查所有可用的代码
        /// </summary>
        private void CheckAllCd()
        {
            List<string> allAvailableCd = new List<string>();
            DateTime dt = Util.GetAvailableDt();
            string endDay = dt.AddDays(-1).ToString("yyyyMMdd");
            string startDay = dt.AddDays(-9).ToString("yyyyMMdd");
            this.getData = new GetDataFromSina(this.basePath + Consts.CSV_FOLDER, endDay, TimeRange.Day);

            // 设置进度条
            DosProgressBar dosProgressBar = new DosProgressBar();
            int idx = 1;
            int totalLen = 3000;
            if (this.callBef != null)
            {
                this.callBef(totalLen);
            }
            this.WriteLog("开始检查000001--003000数据......", true);

            for (int i = 1; i <= 3000; i++)
            {
                //this.CheckAvailableCd163(i, allAvailableCd, endDay, startDay);
                this.CheckAvailableCdSina(i, allAvailableCd);

                // 更新进度条
                if (this.callRowEnd != null)
                {
                    this.callRowEnd();
                }
                dosProgressBar.Dispaly((int)((idx++ / (totalLen * 1.0)) * 100));
            }

            // 关闭进度条
            if (this.callEnd != null)
            {
                this.callEnd();
            }
            this.WriteLog("000001--003000数据检查完了......", true);

            // 设置进度条
            dosProgressBar = new DosProgressBar();
            idx = 1;
            totalLen = 999;
            if (this.callBef != null)
            {
                this.callBef(totalLen);
            }
            this.WriteLog("开始检查300001--300999数据......", true);

            for (int i = 300001; i <= 300999; i++)
            {
                //this.CheckAvailableCd163(i, allAvailableCd, endDay, startDay);
                this.CheckAvailableCdSina(i, allAvailableCd);

                // 更新进度条
                if (this.callRowEnd != null)
                {
                    this.callRowEnd();
                }
                dosProgressBar.Dispaly((int)((idx++ / (totalLen * 1.0)) * 100));
            }

            // 关闭进度条
            if (this.callEnd != null)
            {
                this.callEnd();
            }
            this.WriteLog("300001--300999数据检查完了......", true);

            // 设置进度条
            dosProgressBar = new DosProgressBar();
            idx = 1;
            totalLen = 3999;
            if (this.callBef != null)
            {
                this.callBef(totalLen);
            }
            this.WriteLog("开始检查600000--603999数据......", true);

            for (int i = 600000; i <= 603999; i++)
            {
                //this.CheckAvailableCd163(i, allAvailableCd, endDay, startDay);
                this.CheckAvailableCdSina(i, allAvailableCd);

                // 更新进度条
                if (this.callRowEnd != null)
                {
                    this.callRowEnd();
                }
                dosProgressBar.Dispaly((int)((idx++ / (totalLen * 1.0)) * 100));
            }

            // 关闭进度条
            if (this.callEnd != null)
            {
                this.callEnd();
            }
            this.WriteLog("600000--603999数据检查完了......", true);

            StringBuilder sb = new StringBuilder();
            // 判断删除的数据
            string[] oldAllLine = File.ReadAllLines(this.basePath + Consts.CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
            foreach (string cdName in oldAllLine) 
            {
                if (string.IsNullOrEmpty(cdName))
                {
                    continue;
                }

                if (!allAvailableCd.Contains(cdName))
                {
                    sb.Append(cdName).Append("\r\n");
                }
            }
            if (sb.Length > 0)
            {
                this.WriteLog("删除了如下数据：\r\n", false);
                this.WriteLog(sb.ToString(), false);
            }

            // 判断新增的数据
            sb.Length = 0;
            List<string> oldAllStock = new List<string>();
            oldAllStock.AddRange(oldAllLine);
            foreach (string cdName in allAvailableCd)
            {
                if (string.IsNullOrEmpty(cdName))
                {
                    continue;
                }

                if (!oldAllStock.Contains(cdName))
                {
                    sb.Append(cdName).Append("\r\n");
                }
            }
            if (sb.Length > 0)
            {
                this.WriteLog("新增了如下数据：\r\n", false);
                this.WriteLog(sb.ToString(), false);
            }

            File.WriteAllLines(this.basePath + Consts.CSV_FOLDER + "AllStockInfo.txt", allAvailableCd.ToArray(), Encoding.UTF8);
        }

        /// <summary>
        /// 检查可用的代码
        /// </summary>
        /// <param name="stockCd"></param>
        private string CheckAvailableCdSina(int stockCd, List<string> allAvailableCd)
        {
            string cdName = this.getData.CheckStockCd(stockCd.ToString());
            if (!string.IsNullOrEmpty(cdName))
            {
                allAvailableCd.Add(cdName);
            }

            return string.Empty;
        }

        /// <summary>
        /// 检查可用的代码
        /// </summary>
        /// <param name="stockCd"></param>
        private string CheckAvailableCd163(int stockCd, List<string> allAvailableCd, string endDay, string startDay)
        {
            string strCd = stockCd.ToString().PadLeft(6, '0');
            // 判断类型
            if (strCd.StartsWith("6"))
            {
                strCd = "0" + strCd;
            }
            else
            {
                strCd = "1" + strCd;
            }

            Encoding encoding = Encoding.GetEncoding("GBK");
            string result = string.Empty;

            try
            {
                result = Util.HttpGet(@"http://quotes.money.163.com/service/chddata.html?fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;&end=" + endDay + "&code="
                    + strCd + "&start=" + startDay, "", encoding);
            }
            catch (Exception e)
            {
                this.WriteLog("取得 " + strCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace);
                return e.Message;
            }

            if (!string.IsNullOrEmpty(result) && !"null".Equals(result, System.StringComparison.OrdinalIgnoreCase))
            {
                string[] lines = null;
                string[] lastRow = null;
                try
                {
                    lines = result.Split('\n');
                    if (lines.Length > 2)
                    {
                        lastRow = lines[1].Split(',');
                    }
                }
                catch (Exception e)
                {
                    this.WriteLog("处理Json " + strCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace);
                    return e.Message;
                }

                if (lastRow != null && lastRow.Length > 2)
                {
                    //// 取得最新的一条数据
                    //string lastDay = lastRow[0].Replace("-", "").Replace("/", "");

                    //if (endDay.Equals(lastDay) && !(lastRow[2].StartsWith("ST") || lastRow[2].StartsWith("*ST")))
                    //{
                    //    allAvailableCd.Add(stockCd.ToString().PadLeft(6, '0') + " " + lastRow[2]);
                    //}
                    allAvailableCd.Add(stockCd.ToString().PadLeft(6, '0') + " " + lastRow[2]);
                }
            }

            return string.Empty;
        }

        #endregion

        #region 检查并删除失效的数据

        /// <summary>
        /// 检查数据的正确性
        /// </summary>
        private void CheckData()
        {
            DateTime dt = Util.GetAvailableDt();
            List<string> delCdLst = new List<string>();
            this.GetAllStockBaseInfo();

            this.CheckCsvData(TimeRange.Day, delCdLst);
            this.CheckCsvData(TimeRange.M30, delCdLst);
            this.CheckCsvData(TimeRange.M5, delCdLst);

            if (delCdLst.Count > 0)
            {
                this.WriteLog("删除了如下文件：");
                this.WriteLog(string.Join("\r\n,", delCdLst.ToArray()));
            }
            else
            {
                this.WriteLog("没有删除的文件");
            }
        }

        /// <summary>
        /// 检查数据
        /// </summary>
        /// <param name="timeRange"></param>
        private void CheckCsvData(TimeRange timeRange, List<string> delCdLst)
        {
            // 取得已经存在的所有数据信息
            this.subFolder = timeRange.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder);

            // 设置进度条
            DosProgressBar dosProgressBar = new DosProgressBar();
            int idx = 1;
            int totalLen = allCsv.Count;
            if (this.callBef != null)
            {
                this.callBef(totalLen);
            }
            this.WriteLog("开始检查" + timeRange.ToString() + "的数据......", true);

            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                string stockCd = Util.GetShortNameWithoutType(fileItem.File).Substring(0, 6);
                if (!this.allStockCd.Contains(stockCd))
                {
                    File.Delete(fileItem.File);
                    delCdLst.Add(this.subFolder + stockCd);
                }

                // 更新进度条
                if (this.callRowEnd != null)
                {
                    this.callRowEnd();
                }
                dosProgressBar.Dispaly((int)((idx++ / (totalLen * 1.0)) * 100));
            }

            // 关闭进度条
            if (this.callEnd != null)
            {
                this.callEnd();
            }
            this.WriteLog(timeRange.ToString() + "的数据检查完了......", true);
        }

        #endregion

        #endregion
    }
}
