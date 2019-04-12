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
        private List<BuySellItem> buySellHst = new List<BuySellItem>();

        /// <summary>
        /// 当天的交易信息
        /// </summary>
        List<OrderInfo> todayGuPiao = new List<OrderInfo>();

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
        /// 开始运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, EventArgs e)
        {

        }

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
            this.timer.Stop();
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            this.timer.SynchronizingObject = this;
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
            while (buyThread > 0)
            {
                BuySellItem buySellItem = new BuySellItem();
                buySellItem.Id = buyThread.ToString().PadLeft(2, '0');
                buySellItem.TotalMoney = threadMoney;

                this.buySellHst.Add(buySellItem);

                buyThread--;
            }

            // 读取汇总的信息
            string[] allTotalInfo = File.ReadAllLines(this.GetTradeHstLogFile(), Encoding.UTF8);
            for (int i = 2; i < allTotalInfo.Length; i++)
            {
                // 格式说明
                // 线程ID（2）、空格、原始金额（8）、空格、买入的代码（6）、空格、买入的价格（7）、空格、买入的数量（5）、空格、剩余的金额（8）、空格、盈亏比（7）
                BuySellItem buySellItem = this.buySellHst[i - 2];
                string buyedInfo = allTotalInfo[i].Substring(2 + 1 + 8 + 1, 6 + 1 + 7 + 1 + 5);
                if (!string.IsNullOrEmpty(buyedInfo.Trim()))
                {
                    buySellItem.StockCd = buyedInfo.Substring(0, 6);
                    buySellItem.BuyPrice = Convert.ToDecimal(buyedInfo.Substring(6 + 1, 7));
                    buySellItem.BuyCnt = Convert.ToInt32(buyedInfo.Substring(6 + 1 + 7 + 1, 5));
                }
                buySellItem.TotalMoney = Convert.ToDecimal(allTotalInfo[i].Substring(2 + 1 + 8 + 1 + 6 + 1 + 7 + 1 + 5 + 1, 8));
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

        #region 写交易Log

        /// <summary>
        /// 取得Log文件
        /// </summary>
        /// <returns></returns>
        private string GetAllTradeLogFile()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory + @"Log/AutoTradeLog" + this.sysDate.ToString("yyyyMMdd") + ".txt";
        }

        /// <summary>
        /// 取得Log文件
        /// </summary>
        /// <returns></returns>
        private string GetTradeHstLogFile()
        {
            return Consts.BASE_PATH + Consts.BUY_SELL_POINT_REAL + @"TotalBuySellInfo.txt";
        }

        /// <summary>
        /// 写交易的Log
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        private void WriteStockLog(OrderType orderType, BuySellItem buySell)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(buySell.Id).Append(" ");
            sb.Append(this.sysDate.ToString("yyyy/MM/dd")).Append(" ");
            sb.Append(DateTime.Now.ToString("HH:mm:ss")).Append(" ");
            if (orderType == OrderType.Buy)
            {
                sb.Append("B ");
                sb.Append(buySell.BuyPrice.ToString().PadLeft(7, ' ')).Append(" ");
                sb.Append(buySell.BuyCnt.ToString().PadLeft(5, ' ')).Append(" ");
                sb.Append(buySell.TotalMoney.ToString().PadLeft(8, ' '));
            }
            else
            {
                sb.Append("S ");
                sb.Append(buySell.SellPrice.ToString().PadLeft(7, ' ')).Append(" ");
                sb.Append(buySell.SellCnt.ToString().PadLeft(5, ' ')).Append(" ");
                sb.Append(buySell.TotalMoney.ToString().PadLeft(8, ' ')).Append(" ");
                decimal diff = (buySell.TotalMoney / this.configInfo.ThreadMoney - 1) * 100;
                sb.Append(diff.ToString("0.00").PadLeft(6, ' ')).Append("%");
            }

            // 写总的Log文件
            this.ThreadWriteLog(sb.ToString());

            // 写单个代码的Log文件
            File.AppendAllText(Consts.BASE_PATH + Consts.BUY_SELL_POINT_REAL + buySell.StockCd + @".txt", sb.Append("\r\n").ToString(), Encoding.UTF8);

            // 更新交易历史文件
            string[] allHstInfo = File.ReadAllLines(this.GetTradeHstLogFile(), Encoding.UTF8);
            int idx = this.configInfo.ThreadCnt - Convert.ToInt32(buySell.Id) + 2;
            if (idx >= 2 && idx < allHstInfo.Length)
            {
                // 格式说明
                // 线程ID（2）、空格、原始金额（8）、空格、买入的代码（6）、空格、买入的价格（7）、空格、买入的数量（5）、空格、剩余的金额（8）、空格、盈亏比（7）
                sb.Length = 0;
                sb.Append(buySell.Id).Append(" ");
                sb.Append(this.configInfo.ThreadMoney.ToString().PadLeft(8, ' ')).Append(" ");
                if (orderType == OrderType.Buy)
                {
                    sb.Append(buySell.BuyPrice.ToString().PadLeft(7, ' ')).Append(" ");
                    sb.Append(buySell.BuyCnt.ToString().PadLeft(5, ' ')).Append(" ");
                    sb.Append(buySell.TotalMoney.ToString().PadLeft(8, ' '));
                }
                else
                {
                    sb.Append(string.Empty.PadLeft(7, ' ')).Append(" ");
                    sb.Append(string.Empty.PadLeft(5, ' ')).Append(" ");
                    sb.Append(buySell.TotalMoney.ToString().PadLeft(8, ' ')).Append(" ");
                    decimal diff = (buySell.TotalMoney / this.configInfo.ThreadMoney - 1) * 100;
                    sb.Append(diff.ToString("0.00").PadLeft(6, ' ')).Append("%");
                }
                allHstInfo[idx] = sb.ToString();
                File.WriteAllLines(this.GetTradeHstLogFile(), allHstInfo, Encoding.UTF8);
            }
        }

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="msg"></param>
        private void ThreadWriteLog(object state)
        {
            string msg = state as string;
            File.AppendAllText(this.GetAllTradeLogFile(), msg + "\r\n", Encoding.UTF8);
        }

        /// <summary>
        /// 写交易的log
        /// </summary>
        /// <param name="msg"></param>
        private void ThreadWriteTradeLog(string msg)
        {
            // 取得当天交易的最后一条交易信息
            if (this.rdoReal.Checked)
            {
                this.tradeUtil.GetTodayPiaoInfo(this.todayGuPiao);
            }
            if (this.todayGuPiao.Count > 0)
            {
                OrderInfo lastItem = this.todayGuPiao[0];
                foreach (BuySellItem buySell in this.buySellHst)
                {
                    lock (buySell)
                    {
                        if (buySell.StockCd.Equals(lastItem.StockCd))
                        {
                            if (lastItem.OrderType == OrderType.Buy && lastItem.OrderStatus == OrderStatus.OrderOk
                                && buySell.Status == BuySellStatus.Buying)
                            {
                                // 交易成功
                                buySell.Status = BuySellStatus.Buyed;
                                this.WriteStockLog(lastItem.OrderType, buySell);
                            }
                            else if (lastItem.OrderType == OrderType.Buy && lastItem.OrderStatus == OrderStatus.OrderCancel
                                && buySell.Status == BuySellStatus.Buying)
                            {
                                // 交易取消
                                buySell.Status = BuySellStatus.Waiting;
                                buySell.TotalMoney += (buySell.BuyCnt * buySell.BuyPrice + 5);
                            }
                            else if (lastItem.OrderType == OrderType.Sell && lastItem.OrderStatus == OrderStatus.OrderOk
                                && buySell.Status == BuySellStatus.Selling)
                            {
                                // 交易成功
                                buySell.Status = BuySellStatus.Selled;
                                this.WriteStockLog(lastItem.OrderType, buySell);
                            }
                            else if (lastItem.OrderType == OrderType.Sell && lastItem.OrderStatus == OrderStatus.OrderCancel
                                && buySell.Status == BuySellStatus.Selling)
                            {
                                // 交易取消
                                buySell.Status = BuySellStatus.Buyed;
                                buySell.TotalMoney -= buySell.SellCnt * buySell.SellPrice;
                            }
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region 实时交易相关

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
                    foreach (BuySellItem buySell in this.buySellHst)
                    {
                        lock (buySell)
                        {
                            if (buySell.Status == BuySellStatus.Waiting || buySell.Status == BuySellStatus.Selled)
                            {
                                decimal price = Convert.ToDecimal(realTimeData.valOut2);
                                int canBuyCnt = Util.CanBuyCount(buySell.TotalMoney, price);
                                if (canBuyCnt > 0)
                                {
                                    buySell.StockCd = lastItem.Code;
                                    buySell.Status = BuySellStatus.Buying;
                                    buySell.BuyPrice = price;
                                    buySell.BuyCnt = canBuyCnt * 100;
                                    buySell.TotalMoney -= (buySell.BuyCnt * price + 5);

                                    if (this.rdoReal.Checked)
                                    {
                                        // 实时交易
                                        this.ThreadRealBuy(lastItem.Code, buySell.BuyCnt, price);
                                    }
                                    else
                                    { 
                                        // 模拟交易
                                        OrderInfo order = new OrderInfo();
                                        order.StockCd = lastItem.Code;
                                        order.OrderType = OrderType.Buy;
                                        order.OrderStatus = OrderStatus.Waiting;

                                        this.tradeUtil.RetMsg = "模拟买 订单成功...";
                                        this.tradeUtil.CurOpt = CurOpt.OrderOKEvent;
                                        this.todayGuPiao.Insert(0, order);
                                        this.AsyncCallBack(null);
                                        
                                        this.tradeUtil.RetMsg = "模拟买 交易成功...";
                                        this.tradeUtil.CurOpt = CurOpt.OrderSuccessEvent;
                                        order.OrderStatus = OrderStatus.OrderOk;
                                        this.todayGuPiao.Insert(0, order);
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
                    foreach (BuySellItem buySell in this.buySellHst)
                    {
                        lock (buySell)
                        {
                            if (lastItem.Code.Equals(buySell.StockCd)
                                && buySell.Status == BuySellStatus.Buyed)
                            {
                                buySell.SellCnt = buySell.BuyCnt;
                                buySell.SellPrice = Convert.ToDecimal(realTimeData.valIn2);
                                buySell.Status = BuySellStatus.Selling;
                                buySell.TotalMoney += buySell.SellCnt * buySell.SellPrice;

                                if (this.rdoReal.Checked)
                                {
                                    // 实时交易
                                    this.ThreadRealSell(lastItem.Code, buySell.SellCnt, buySell.SellPrice);
                                }
                                else
                                {
                                    // 模拟交易
                                    OrderInfo order = new OrderInfo();
                                    order.StockCd = lastItem.Code;
                                    order.OrderType = OrderType.Sell;
                                    order.OrderStatus = OrderStatus.Waiting;

                                    this.tradeUtil.RetMsg = "模拟卖 订单成功...";
                                    this.tradeUtil.CurOpt = CurOpt.OrderOKEvent;
                                    this.todayGuPiao.Insert(0, order);
                                    this.AsyncCallBack(null);

                                    this.tradeUtil.RetMsg = "模拟卖 交易成功...";
                                    this.tradeUtil.CurOpt = CurOpt.OrderSuccessEvent;
                                    order.OrderStatus = OrderStatus.OrderOk;
                                    this.todayGuPiao.Insert(0, order);
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

        #region 异步交易接口的事件处理

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
                    this.ThreadWriteTradeLog("交易取消");
                    break;
            }
        }

        #endregion

        #endregion
    }
}
