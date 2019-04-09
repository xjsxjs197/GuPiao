using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;
using System.Threading;
using System.IO;
using DayBatch;
using DataProcess.FenXing;

namespace GuPiao
{
    /// <summary>
    /// 自动交易系统
    /// </summary>
    public partial class AutoTrade : BaseForm
    {
        #region 全局变量

        /// <summary>
        /// 标识是否运行
        /// </summary>
        private bool isRuning = false;

        /// <summary>
        /// 交易的共通处理对象
        /// </summary>
        TradeUtil tradeUtil = new TradeUtil();

        /// <summary>
        /// 计时器
        /// </summary>
        private System.Timers.Timer timer = null;

        /// <summary>
        /// UI线程的同步上下文
        /// </summary>
        private SynchronizationContext mSyncContext = null;

        /// <summary>
        /// 股票的基本信息
        /// </summary>
        private Dictionary<string, GuPiaoInfo> guPiaoBaseInfo = new Dictionary<string, GuPiaoInfo>();

        /// <summary>
        /// 设定信息
        /// </summary>
        private BuySellSetting configInfo;

        /// <summary>
        /// 所有可以自动处理的数据信息
        /// </summary>
        private List<string> allStockCd = new List<string>();

        /// <summary>
        /// 分组取数据时的位置
        /// </summary>
        private int getDataGrpIdx = 0;

        /// <summary>
        /// 系统时间
        /// </summary>
        private DateTime sysDate;

        /// <summary>
        /// 代码和数据的映射
        /// </summary>
        private Dictionary<string, List<BaseDataInfo>> stockCdData = new Dictionary<string, List<BaseDataInfo>>();

        /// <summary>
        /// 实时数据过滤用
        /// </summary>
        private List<int> dataFilter = new List<int>();

        /// <summary>
        /// 分型处理
        /// </summary>
        private FenXing fenXing = new FenXing();

        /// <summary>
        /// 买卖的历史
        /// </summary>
        private List<Dictionary<string, object>> buySellHst = new List<Dictionary<string, object>>();

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public AutoTrade()
        {
            InitializeComponent();

            // 画面初始化
            this.Init();
        }

        #endregion

        #region 页面事件

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoTrade_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.rdoReal.Checked)
            {
                this.tradeUtil.TradeRelease();
            }
        }

        /// <summary>
        /// 定时事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 定时处理的逻辑
            this.TimerProcess();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 画面初始化
        /// </summary>
        private void Init()
        {
            // 保存系统时间
            this.sysDate = DateTime.Now;

            // 获取UI线程同步上下文
            this.mSyncContext = SynchronizationContext.Current;

            // 绑定事件
            this.FormClosing += new FormClosingEventHandler(this.AutoTrade_FormClosing);

            // 读取基础数据
            this.LoadBaseData();

            if (this.rdoReal.Checked)
            {
                // 实时交易的相关初始化
                this.InitRealTrade();
            }
        }

        /// <summary>
        /// 实时交易的相关初始化
        /// </summary>
        private void InitRealTrade()
        {
            // 修改系统时间
            this.ChangeSystemTime();

            // 设置异步关联信息
            this.tradeUtil.SetCallBack(this.ThreadAsyncCallBack);
            this.tradeUtil.SetGuPiaoInfo(this.guPiaoBaseInfo);

            // 设置交易的基础配置信息
            ComboBox cmbAccountType = new ComboBox();
            ComboBox cmbBrokerType = new ComboBox();
            tradeUtil.Init(cmbAccountType, cmbBrokerType);
        }

        /// <summary>
        /// 修改系统时间
        /// </summary>
        private void ChangeSystemTime()
        {
            // 取得当前系统时间
            DateTime t = DateTime.Now;
            // 修改为2018/4/23(证书的有效时间是从2018/4/23开始的一周)
            DateTime newTm = new DateTime(2018, 4, 23, t.Hour, t.Minute, t.Second, t.Millisecond);

            // 转换System.DateTime到SYSTEMTIME
            SYSTEMTIME st = new SYSTEMTIME();
            st.FromDateTime(newTm);
            // 调用Win32 API设置系统时间
            Win32API.SetLocalTime(ref st);
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            this.timer = new System.Timers.Timer();
            this.timer.Enabled = true;
            this.timer.Interval = 1000; // 每1秒更新数据
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            this.timer.SynchronizingObject = this;
            this.timer.Stop();
        }

        /// <summary>
        /// 显示各种操作信息
        /// </summary>
        /// <param name="retMsg"></param>
        private void DispMsg(string retMsg)
        {
            this.Text = this.configInfo.SystemTitle + " " + retMsg;
        }

        /// <summary>
        /// 读取基础数据
        /// </summary>
        private void LoadBaseData()
        {
            // 读取设定信息
            this.configInfo = Util.GetBuyCellSettingInfo();

            // 设置标题
            this.Text = this.configInfo.SystemTitle;

            // 取得所有可以自动处理的数据信息
            this.GetAllStockBaseInfo();

            // 设置代码和数据的映射
            this.SetCdDataMapping();

            // 读取历史买卖数据
            this.GetBuySellHstData();
        }

        /// <summary>
        /// 取得所有可以自动处理的数据信息
        /// </summary>
        private void GetAllStockBaseInfo()
        {
            this.allStockCd.Clear();

            // 取得融资融券信息
            List<string> allRongzi = new List<string>();
            if (!this.chkLiangRong.Checked)
            {
                allRongzi = Util.GetRongZiRongQuan();
            }

            // 取得所有数据的代码信息
            string[] allLine = File.ReadAllLines(Consts.BASE_PATH + Consts.CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
            if (allLine != null && allLine.Length > 0)
            {
                foreach (string codeName in allLine)
                {
                    if (string.IsNullOrEmpty(codeName))
                    {
                        continue;
                    }

                    string stockCd = codeName.Substring(0, 6);

                    // 不要创业数据
                    if (!this.chkChuangYe.Checked && Util.IsChuangyeStock(stockCd))
                    {
                        continue;
                    }

                    // 融资融券信息
                    if (!this.chkLiangRong.Checked && allRongzi.Contains(stockCd))
                    {
                        continue;
                    }

                    this.allStockCd.Add(stockCd);
                }
            }

            // 过滤当天停牌的数据 TODO
        }

        /// <summary>
        /// 设置代码和数据的映射
        /// </summary>
        private void SetCdDataMapping()
        {
            // 设置数据过滤器
            this.dataFilter.Clear();
            switch (this.configInfo.AutoTradeLevel)
            {
                case "M30":
                    this.dataFilter.Add(100000);
                    this.dataFilter.Add(103000);
                    this.dataFilter.Add(110000);
                    this.dataFilter.Add(113000);
                    this.dataFilter.Add(133000);
                    this.dataFilter.Add(140000);
                    this.dataFilter.Add(143000);
                    this.dataFilter.Add(150000);
                    break;

                case "M15":
                    this.dataFilter.Add(094500);
                    this.dataFilter.Add(100000);
                    this.dataFilter.Add(101500);
                    this.dataFilter.Add(103000);
                    this.dataFilter.Add(104500);
                    this.dataFilter.Add(110000);
                    this.dataFilter.Add(111500);
                    this.dataFilter.Add(113000);
                    this.dataFilter.Add(131500);
                    this.dataFilter.Add(133000);
                    this.dataFilter.Add(134500);
                    this.dataFilter.Add(140000);
                    this.dataFilter.Add(141500);
                    this.dataFilter.Add(143000);
                    this.dataFilter.Add(144500);
                    this.dataFilter.Add(150000);
                    break;

                case "M5":
                    for (int i = 93500; i <= 113000; i+=500)
                    {
                        this.dataFilter.Add(i);
                    }
                    for (int i = 130500; i <= 150000; i += 500)
                    {
                        this.dataFilter.Add(i);
                    }
                    break;
            }

            // 取得历史数据
            this.stockCdData.Clear();
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.configInfo.AutoTradeLevel + "/");
            foreach (FilePosInfo item in allCsv)
            {
                if (item.IsFolder)
                {
                    continue;
                }

                string stockCd = Util.GetShortNameWithoutType(item.File).Substring(0, 6);
                if (this.allStockCd.Contains(stockCd))
                {
                    if (stockCd.StartsWith("6"))
                    {
                        this.stockCdData.Add("sh" + stockCd, DayBatchProcess.GetStockHistoryInfo(item.File));
                    }
                    else
                    {
                        this.stockCdData.Add("sz" + stockCd, DayBatchProcess.GetStockHistoryInfo(item.File));
                    }
                }
            }
        }

        /// <summary>
        /// 读取历史买卖数据
        /// </summary>
        private void GetBuySellHstData()
        {
            // 读取设定文件内容
            int buyThread = this.configInfo.ThreadCnt;
            decimal threadMoney = this.configInfo.ThreadMoney;
            while (buyThread-- > 0)
            {
                Dictionary<string, object> buySellItem = new Dictionary<string, object>();
                buySellItem.Add("stockCd", string.Empty);
                buySellItem.Add("status", string.Empty);
                buySellItem.Add("price", (decimal)0);
                buySellItem.Add("buyCount", (decimal)0);
                buySellItem.Add("buyMoney", (decimal)0);
                buySellItem.Add("TotalMoney", threadMoney);

                this.buySellHst.Add(buySellItem);
            }

            // 读取历史BuySell信息
            List<FilePosInfo> allHstData = Util.GetAllFiles(Consts.BASE_PATH + Consts.BUY_SELL_POINT_REAL);
            foreach (FilePosInfo item in allHstData)
            {
                if (item.IsFolder)
                {
                    continue;
                }

                string[] allLine = File.ReadAllLines(item.File, Encoding.UTF8);
                if (allLine.Length > 0)
                {
                    // 20190328103000 1234   19.180
                    // 设置有Buy点的信息
                    string lastBuySellInfo = allLine[allLine.Length - 1];
                    if (lastBuySellInfo.Length == 28)
                    {
                        Dictionary<string, object> buySellItem = this.buySellHst[buyThread];
                        buySellItem["stockCd"] = Util.GetShortNameWithoutType(item.File);
                        buySellItem["status"] = "B";
                        buySellItem["price"] = Convert.ToDecimal(lastBuySellInfo.Substring(20));
                        buySellItem["buyCount"] = Convert.ToDecimal(lastBuySellInfo.Substring(15, 4));
                        buySellItem["buyMoney"] = (decimal)buySellItem["price"] * (decimal)buySellItem["buyCount"];
                        buySellItem["TotalMoney"] = threadMoney - (decimal)buySellItem["buyMoney"];

                        buyThread++;
                        if (buyThread >= this.configInfo.ThreadCnt)
                        {
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 定时处理的逻辑
        /// </summary>
        private void TimerProcess()
        {
            // 定时取得数据
            List<GuPiaoInfo> dataLst = this.TimerGetData();

            // 开线程，处理实时数据
            foreach (GuPiaoInfo item in dataLst)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ThreadCheckRealTimeData), item);
            }
        }

        /// <summary>
        /// 定时取得数据
        /// </summary>
        /// <returns></returns>
        private List<GuPiaoInfo> TimerGetData()
        {
            int maxCnt = (this.getDataGrpIdx + 1) * this.configInfo.DataCntPerSecond;
            if (maxCnt > this.allStockCd.Count)
            {
                maxCnt = this.allStockCd.Count;
            }

            List<GuPiaoInfo> dataLst = new List<GuPiaoInfo>();

            // 每次按照设定文件，取多个数据
            try
            {
                List<string> noList = new List<string>();
                while (this.getDataGrpIdx < maxCnt)
                {

                    if (this.allStockCd[this.getDataGrpIdx].StartsWith("6"))
                    {
                        noList.Add("sh" + this.allStockCd[this.getDataGrpIdx]);
                    }
                    else
                    {
                        noList.Add("sz" + this.allStockCd[this.getDataGrpIdx]);
                    }

                    this.getDataGrpIdx++;
                }

                // 从Sina取得基础数据
                string url = "http://hq.sinajs.cn/list=" + string.Join(",", noList.ToArray());
                string result = Util.HttpGet(url, string.Empty, Encoding.UTF8);
                if (!string.IsNullOrEmpty(result) && result.Length > 20)
                {
                    string[] guPiao = result.Split(';');
                    for (int i = 0; i < guPiao.Length; i++)
                    {
                        if (string.IsNullOrEmpty(guPiao[i]) || guPiao[i].Length < 10)
                        {
                            break;
                        }

                        string[] lines = guPiao[i].Split('=');
                        string[] details = lines[1].Split(',');

                        GuPiaoInfo item = new GuPiaoInfo();
                        dataLst.Add(item);

                        item.fundcode = noList[i];
                        item.jinriKaipanVal = details[1];
                        item.zuoriShoupanVal = details[2];
                        item.currentVal = details[3];
                        item.zuigaoVal = details[4];
                        item.zuidiVal = details[5];
                        item.jingmaiInVal = details[6];
                        item.jingmaiOutVal = details[7];
                        item.chengjiaoShu = details[8];
                        item.chengjiaoJine = details[9];
                        item.gushuIn1 = details[10];
                        item.valIn1 = details[11];
                        item.gushuIn2 = details[12];
                        item.valIn2 = details[13];
                        item.gushuIn3 = details[14];
                        item.valIn3 = details[15];
                        item.gushuIn4 = details[16];
                        item.valIn4 = details[17];
                        item.gushuIn5 = details[18];
                        item.valIn5 = details[19];
                        item.gushuOut1 = details[20];
                        item.valOut1 = details[21];
                        item.gushuOut2 = details[22];
                        item.valOut2 = details[23];
                        item.gushuOut3 = details[24];
                        item.valOut3 = details[25];
                        item.gushuOut4 = details[26];
                        item.valOut4 = details[27];
                        item.gushuOut5 = details[28];
                        item.valOut5 = details[29];
                        item.date = details[30];
                        item.time = details[31].Replace(":", "");
                    }
                }
            }
            catch (Exception e)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ThreadWriteLog), e.Message + "\r\n" + e.StackTrace);
            }

            if (this.getDataGrpIdx >= this.allStockCd.Count)
            {
                this.getDataGrpIdx = 0;
            }

            return dataLst;
        }

        #region 实时处理相关

        /// <summary>
        /// 异步的回调方法（多线程内调用）
        /// </summary>
        /// <param name="param"></param>
        private void ThreadAsyncCallBack(params object[] param)
        {
            // 在线程中更新UI（通过UI线程同步上下文mSyncContext）
            mSyncContext.Post(this.AsyncCallBack, param);
        }

        /// <summary>
        /// 异步的回调方法
        /// </summary>
        /// <param name="param"></param>
        private void AsyncCallBack(object threadParam)
        {
            object[] param = (object[])threadParam;

            // 第一步都是显示返回的信息
            this.DispMsg(this.tradeUtil.RetMsg);

            switch (this.tradeUtil.CurOpt)
            {
                case CurOpt.InitEvent:
                    if (this.tradeUtil.IsSuccess)
                    {
                        this.btnRun.Enabled = true;
                    }
                    break;

                case CurOpt.LoginEvent:
                    if (this.tradeUtil.isLoginOk)
                    {
                        this.btnRun.Enabled = false;
                        this.btnRun.Text = "运行中...";

                        // 初始化定时器
                        this.InitTimer();
                    }
                    break;

                case CurOpt.OrderOKEvent:
                    this.ThreadWriteTradeLog("订单成功");
                    break;

                case CurOpt.OrderSuccessEvent:
                    this.ThreadWriteTradeLog("交易成功");
                    break;

                case CurOpt.CancelOrder:
                    
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="msg"></param>
        private void ThreadWriteLog(object state)
        {
            string msg = state as string;
            string logFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Log/AutoTradeLog" + this.sysDate.ToString("yyyyMMdd") + ".txt";

            File.AppendAllText(logFile, msg + "\r\n", Encoding.UTF8);
        }

        /// <summary>
        /// 写交易的log
        /// </summary>
        /// <param name="msg"></param>
        private void ThreadWriteTradeLog(string msg)
        {
 
        }

        /// <summary>
        /// 取消交易
        /// </summary>
        /// <param name="cd"></param>
        private void ThreadCanceOrder(string cd)
        {
            this.ThreadWriteTradeLog("交易取消");
        }

        /// <summary>
        /// 处理实时数据
        /// </summary>
        /// <param name="data"></param>
        private void ThreadCheckRealTimeData(object data)
        {
            GuPiaoInfo item = data as GuPiaoInfo;
            if (item == null)
            {
                return;
            }

            // 处理当前时间
            int time = Convert.ToInt32(item.time);
            time = (int)(time / 500) * 500;
            if (!this.dataFilter.Contains(time))
            {
                return;
            }

            // 获得数据信息
            if (!this.stockCdData.ContainsKey(item.fundcode))
            {
                return;
            }
            
            List<BaseDataInfo> stockInfos = this.stockCdData[item.fundcode];
            if (stockInfos == null || stockInfos.Count == 0)
            {
                return;
            }

            // 判断当前的数据，是否已经追加到历史数据中
            BaseDataInfo lastItem = stockInfos[0];
            string stockCd = lastItem.Code;
            string lastDay = time.ToString().PadLeft(6, '0');
            if (lastItem.Day == lastDay)
            {
                return;
            }
            
            // 处理最后的数据
            lastItem = new BaseDataInfo();
            lastItem.Code = stockCd;
            lastItem.Day = lastDay;
            lastItem.DayVal = Convert.ToDecimal(item.currentVal);
            lastItem.DayMaxVal = Convert.ToDecimal(item.zuigaoVal);
            lastItem.DayMinVal = Convert.ToDecimal(item.zuidiVal);
            stockInfos.Insert(0, lastItem);

            // 取得分型的数据
            List<BaseDataInfo> fenxingInfo = 
                this.fenXing.DoFenXingSp(stockInfos, this.configInfo, this.dataFilter[0].ToString().PadLeft(6, '0'), null);
            if (fenxingInfo[0].BuySellFlg != 0)
            {
                // 开始自动买卖
                this.ThreadAutoTrade(item, lastItem);
            }
        }

        /// <summary>
        /// 开始自动买卖
        /// </summary>
        /// <param name="realTimeData"></param>
        private void ThreadAutoTrade(GuPiaoInfo realTimeData, BaseDataInfo lastItem)
        {
            try
            {
                if (lastItem.BuySellFlg > 0)
                {
                    foreach (Dictionary<string, object> buySell in this.buySellHst)
                    {
                        lock (buySell)
                        {
                            if (!"B".Equals(buySell["status"]))
                            {
                                decimal totalMoney = (decimal)buySell["TotalMoney"];
                                decimal price = Convert.ToDecimal(realTimeData.valOut2);
                                int canBuyCnt = Util.CanBuyCount(totalMoney, price);
                                if (canBuyCnt > 0)
                                {
                                    buySell["stockCd"] = lastItem.Code;
                                    buySell["status"] = "B";
                                    buySell["price"] = price;
                                    buySell["buyCount"] = (decimal)(canBuyCnt * 100);
                                    buySell["buyMoney"] = (decimal)buySell["buyCount"] * price + 5;
                                    buySell["TotalMoney"] = (decimal)buySell["TotalMoney"] - (decimal)buySell["buyMoney"];

                                    if (this.rdoReal.Checked)
                                    {
                                        // 实时交易
                                        this.ThreadRealBuy(lastItem.Code, canBuyCnt * 100, price);
                                    }
                                    else
                                    { 
                                        // 模拟交易
                                        this.tradeUtil.RetMsg = "模拟买 订单成功...";
                                        this.tradeUtil.CurOpt = CurOpt.OrderOKEvent;
                                        this.AsyncCallBack(null);
                                        
                                        this.tradeUtil.RetMsg = "模拟买 交易成功...";
                                        this.tradeUtil.CurOpt = CurOpt.OrderSuccessEvent;
                                        this.AsyncCallBack(null);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                else if (lastItem.BuySellFlg < 0)
                {
                    foreach (Dictionary<string, object> buySell in this.buySellHst)
                    {
                        lock (buySell)
                        {
                            if (lastItem.Code.Equals(buySell["stockCd"])
                                && "B".Equals(buySell["status"]))
                            {
                                decimal price = Convert.ToDecimal(realTimeData.valIn2);
                                decimal sellMoney = (decimal)buySell["buyCount"] * price;
                                buySell["status"] = "S";
                                buySell["TotalMoney"] = (decimal)buySell["TotalMoney"] + sellMoney;

                                if (this.rdoReal.Checked)
                                {
                                    // 实时交易
                                    this.ThreadRealSell(lastItem.Code, (int)buySell["buyCount"], price);
                                }
                                else
                                {
                                    // 模拟交易
                                    this.tradeUtil.RetMsg = "模拟卖 订单成功...";
                                    this.tradeUtil.CurOpt = CurOpt.OrderOKEvent;
                                    this.AsyncCallBack(null);

                                    this.tradeUtil.RetMsg = "模拟卖 交易成功...";
                                    this.tradeUtil.CurOpt = CurOpt.OrderSuccessEvent;
                                    this.AsyncCallBack(null);
                                }

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.ThreadWriteLog(e.Message + "\r\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 开始实时的买操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="buyCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private bool ThreadRealBuy(string cd, int buyCnt, decimal price)
        {
            this.tradeUtil.BuyStock(cd, (uint)buyCnt, (float)price, BuySellType.QuickBuy);
            return true;
        }

        /// <summary>
        /// 开始实时的卖操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="sellCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private bool ThreadRealSell(string cd, int sellCnt, decimal price)
        {
            this.tradeUtil.SellStock(cd, (uint)sellCnt, (float)price, BuySellType.QuickSell);
            return true;
        }

        #endregion
    }
}
