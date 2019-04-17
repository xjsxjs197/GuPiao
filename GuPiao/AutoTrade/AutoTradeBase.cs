using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using DataProcess.FenXing;
using DayBatch;
using System.IO;

namespace GuPiao
{
    /// <summary>
    /// 自动交易的基类
    /// </summary>
    public abstract class AutoTradeBase
    {
        #region 全局变量

        /// <summary>
        /// 具体业务操作的方法
        /// </summary>
        public delegate void DoSomethingWithP(TradeEventParam param);

        /// <summary>
        /// 定义回调方法
        /// </summary>
        protected DoSomethingWithP callBackF = null;

        /// <summary>
        /// 交易的日期
        /// </summary>
        protected DateTime tradeDate;

        /// <summary>
        /// 是否是实时模拟交易
        /// </summary>
        protected bool isRealEmu;

        /// <summary>
        /// 股票的基本信息
        /// </summary>
        protected Dictionary<string, GuPiaoInfo> guPiaoBaseInfo = new Dictionary<string, GuPiaoInfo>();

        /// <summary>
        /// 设定信息
        /// </summary>
        protected BuySellSetting configInfo;

        /// <summary>
        /// 所有可以自动处理的数据信息
        /// </summary>
        protected List<string> allStockCd = new List<string>();

        /// <summary>
        /// 分组取数据时的位置
        /// </summary>
        protected int getDataGrpIdx = 0;

        /// <summary>
        /// 代码和数据的映射
        /// </summary>
        protected Dictionary<string, List<BaseDataInfo>> stockCdData = new Dictionary<string, List<BaseDataInfo>>();

        /// <summary>
        /// 实时数据过滤用
        /// </summary>
        protected List<int> dataFilter = new List<int>();

        /// <summary>
        /// 买卖的历史
        /// </summary>
        protected List<BuySellItem> buySellHst = new List<BuySellItem>();

        /// <summary>
        /// 当天的交易信息
        /// </summary>
        protected List<OrderInfo> todayGuPiao = new List<OrderInfo>();

        private readonly object Locker = new object();

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(DoSomethingWithP pCallBackF, bool needChuangYe, bool needLiangRong,
            DateTime tradeDate, bool isRealEmu)
        {
            this.callBackF = pCallBackF;
            this.tradeDate = tradeDate;
            this.isRealEmu = isRealEmu;

            // 读取基础数据
            this.LoadBaseData(needChuangYe, needLiangRong);

            // 初始化其他
            this.InitOther();
        }

        /// <summary>
        /// 取得配置信息
        /// </summary>
        /// <returns></returns>
        public BuySellSetting GetConfigInfo()
        {
            return this.configInfo;
        }

        /// <summary>
        /// 登陆服务器
        /// </summary>
        public void LoginServer()
        {
            this.LoginServerSub();
        }

        /// <summary>
        /// 获取定时器的间隔
        /// </summary>
        /// <returns></returns>
        public double GetTimerInterval()
        {
            return this.GetTradeTimer();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void TradeRelease()
        {
            this.TradeReleaseSub();
        }
        
        /// <summary>
        /// 定时取得数据
        /// </summary>
        /// <returns></returns>
        public List<GuPiaoInfo> TimerGetData()
        {
            List<GuPiaoInfo> dataLst = new List<GuPiaoInfo>();

            lock (Locker)
            {
                int maxCnt = this.getDataGrpIdx + this.configInfo.DataCntPerSecond;
                if (maxCnt > this.allStockCd.Count)
                {
                    maxCnt = this.allStockCd.Count;
                }

                // 每次按照设定文件，取多个数据
                try
                {
                    List<string> noList = new List<string>();
                    while (this.getDataGrpIdx < maxCnt)
                    {
                        string stockCd = this.allStockCd[this.getDataGrpIdx];
                        noList.Add(this.GetStockCdBefChar(stockCd) + stockCd);

                        this.getDataGrpIdx++;
                    }

                    //this.WriteComnLog("取数据 ： " + noList.Count);

                    // 通过代码列表取得数据
                    this.GetDataByCdList(noList, dataLst);
                }
                catch (Exception e)
                {
                    TradeEventParam eventParam = new TradeEventParam();
                    eventParam.CurOpt = CurOpt.GetStockData;
                    eventParam.IsSuccess = false;
                    eventParam.Msg = e.Message + "\r\n" + e.StackTrace;

                    this.WriteComnLog(eventParam.Msg);

                    this.callBackF(eventParam);
                }

                if (this.getDataGrpIdx >= this.allStockCd.Count)
                {
                    this.getDataGrpIdx = 0;

                    // 当前时间点，这一轮数据取得完成
                    this.GetDataOneLoopEnd();
                }
            }

            return dataLst;
        }

        /// <summary>
        /// 检查当前数据的买卖点信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int CheckDataBuySellFlg(GuPiaoInfo item)
        {
            try
            {
                // 处理当前时间
                int time = this.CheckTime(item.time);
                if (!this.dataFilter.Contains(time))
                {
                    return 0;
                }

                // 获得数据信息
                if (!this.stockCdData.ContainsKey(item.fundcode))
                {
                    return 0;
                }

                List<BaseDataInfo> stockInfos = this.stockCdData[item.fundcode];
                if (stockInfos == null || stockInfos.Count == 0)
                {
                    return 0;
                }

                // 检查当前数据的买卖点标志
                BaseDataInfo lastItem = this.CheckCurDataBuySellFlg(stockInfos, item, time);
                if (lastItem != null)
                {
                    return lastItem.BuySellFlg;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                this.WriteComnLog(e.Message + "\r\n" + e.StackTrace);
                return 0;
            }
        }

        /// <summary>
        /// 买操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="buyCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public bool Buy(string cd, int buyCnt, decimal price)
        {
            return this.BuyStock(cd, buyCnt, price);
        }

        /// <summary>
        /// 开始实时的卖操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="sellCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public bool Sell(string cd, int sellCnt, decimal price)
        {
            return this.SellStock(cd, sellCnt, price);
        }

        /// <summary>
        /// 写Log
        /// </summary>
        /// <param name="msg"></param>
        public void WriteComnLog(string msg)
        {
            File.AppendAllText(this.GetAllTradeLogFile(), msg + "\r\n", Encoding.UTF8);
        }

        /// <summary>
        /// 交易后的处理
        /// </summary>
        /// <param name="msg"></param>
        public void TradeAfter(string msg)
        {
            // 取得当天交易的信息
            this.GetTodayTradeInfo(this.todayGuPiao);

            // 判断当天未交易完成的信息
            for (int i = this.todayGuPiao.Count - 1; i >= 0; i--)
            {
                OrderInfo lastItem = this.todayGuPiao[i];
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

        /// <summary>
        /// 开始自动买卖
        /// </summary>
        /// <param name="realTimeData"></param>
        public void ThreadAutoTrade(GuPiaoInfo realTimeData, int buySellFlg)
        {
            try
            {
                if (buySellFlg > 0)
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
                                    buySell.StockCd = realTimeData.fundcode;
                                    buySell.Status = BuySellStatus.Buying;
                                    buySell.BuyPrice = price;
                                    buySell.BuyCnt = canBuyCnt * 100;
                                    buySell.TotalMoney -= (buySell.BuyCnt * price + 5);
                                    buySell.Time = this.CheckTime(realTimeData.time).ToString().PadLeft(6, '0');

                                    // 开始买的交易
                                    this.Buy(realTimeData.fundcode, buySell.BuyCnt, price);

                                    break;
                                }
                            }
                        }
                    }
                }
                else if (buySellFlg < 0)
                {
                    foreach (BuySellItem buySell in this.buySellHst)
                    {
                        lock (buySell)
                        {
                            if (buySell.Status == BuySellStatus.Buyed
                                && realTimeData.fundcode.EndsWith(buySell.StockCd))
                            {
                                buySell.SellCnt = buySell.BuyCnt;
                                buySell.SellPrice = Convert.ToDecimal(realTimeData.valIn2);
                                buySell.Status = BuySellStatus.Selling;
                                buySell.TotalMoney += buySell.SellCnt * buySell.SellPrice;
                                buySell.Time = this.CheckTime(realTimeData.time).ToString().PadLeft(6, '0');

                                // 开始卖的交易
                                this.Sell(realTimeData.fundcode, buySell.SellCnt, buySell.SellPrice);

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.WriteComnLog(e.Message + "\r\n" + e.StackTrace);
            }
        }

        #endregion

        #region 子类可以重写的父类虚方法

        /// <summary>
        /// 初始化其他
        /// </summary>
        protected virtual void InitOther()
        { 
        }

        /// <summary>
        /// 获取定时器间隔
        /// </summary>
        /// <returns></returns>
        protected virtual double GetTradeTimer()
        {
            throw new Exception("获取定时器间隔：未实现");
        }

        /// <summary>
        /// 登陆服务器
        /// </summary>
        protected virtual void LoginServerSub()
        {
            throw new Exception("登陆服务器：未实现");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void TradeReleaseSub()
        {
        }

        /// <summary>
        /// 取得代码前面的前缀
        /// </summary>
        /// <returns></returns>
        protected virtual string GetStockCdBefChar(string stockCd)
        {
            return string.Empty;
        }

        /// <summary>
        /// 通过代码列表取得数据
        /// </summary>
        /// <param name="noList"></param>
        /// <returns></returns>
        protected virtual void GetDataByCdList(List<string> noList, List<GuPiaoInfo> dataLst)
        {
            throw new Exception("取得数据：未实现");
        }

        /// <summary>
        /// 所有代码的数据取得完成
        /// </summary>
        protected virtual void GetDataOneLoopEnd()
        {
            //this.WriteComnLog(this.tradeDate.ToString("yyyy/MM/dd HH:mm:ss") + " 一轮数据取得完成");
        }

        /// <summary>
        /// 检查当前数据的买卖点标志
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual BaseDataInfo CheckCurDataBuySellFlg(List<BaseDataInfo> stockInfos, GuPiaoInfo item, int time)
        {
            throw new Exception("检查当前数据的买卖点标志：未实现");
        }

        /// <summary>
        /// 买操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="buyCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        protected virtual bool BuyStock(string cd, int buyCnt, decimal price)
        {
            // 模拟交易
            OrderInfo order = new OrderInfo();
            order.StockCd = cd;
            order.OrderType = OrderType.Buy;
            order.OrderStatus = OrderStatus.Waiting;

            TradeEventParam eventParam = new TradeEventParam();
            eventParam.Msg = "模拟买 订单成功...";
            eventParam.CurOpt = CurOpt.OrderOKEvent;
            this.todayGuPiao.Insert(0, order);
            this.callBackF(eventParam);

            eventParam = new TradeEventParam();
            eventParam.Msg = "模拟买 交易成功...";
            eventParam.CurOpt = CurOpt.OrderSuccessEvent;
            order.OrderStatus = OrderStatus.OrderOk;
            this.callBackF(eventParam);

            return true;
        }

        /// <summary>
        /// 卖操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="buyCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        protected virtual bool SellStock(string cd, int buyCnt, decimal price)
        {
            // 模拟交易
            OrderInfo order = new OrderInfo();
            order.StockCd = cd;
            order.OrderType = OrderType.Sell;
            order.OrderStatus = OrderStatus.Waiting;

            TradeEventParam eventParam = new TradeEventParam();
            eventParam.Msg = "模拟卖 订单成功...";
            eventParam.CurOpt = CurOpt.OrderOKEvent;
            this.todayGuPiao.Insert(0, order);
            this.callBackF(eventParam);

            eventParam = new TradeEventParam();
            eventParam.Msg = "模拟卖 交易成功...";
            eventParam.CurOpt = CurOpt.OrderSuccessEvent;
            order.OrderStatus = OrderStatus.OrderOk;
            this.callBackF(eventParam);

            return true;
        }

        /// <summary>
        /// 取得当天交易信息
        /// </summary>
        /// <param name="todayGuPiao"></param>
        protected virtual void GetTodayTradeInfo(List<OrderInfo> todayGuPiao)
        { 
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 读取基础数据
        /// </summary>
        private void LoadBaseData(bool needChuangYe, bool needLiangRong)
        {
            // 读取设定信息
            this.configInfo = Util.GetBuyCellSettingInfo();

            // 取得所有可以自动处理的数据信息
            this.GetAllStockBaseInfo(needChuangYe, needLiangRong);

            // 设置代码和数据的映射
            this.SetCdDataMapping();

            // 读取历史买卖数据
            this.GetBuySellHstData();
        }

        /// <summary>
        /// 取得所有可以自动处理的数据信息
        /// </summary>
        private void GetAllStockBaseInfo(bool needChuangYe, bool needLiangRong)
        {
            this.allStockCd.Clear();

            // 取得融资融券信息
            List<string> allRongzi = new List<string>();
            if (!needLiangRong)
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
                    if (!needChuangYe && Util.IsChuangyeStock(stockCd))
                    {
                        continue;
                    }

                    // 融资融券信息
                    if (!needLiangRong && allRongzi.Contains(stockCd))
                    {
                        continue;
                    }

                    this.allStockCd.Add(stockCd);
                }
            }

            // 过滤当天停牌的数据 TODO
            // 年线以下过滤 TODO
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
                    for (int i = 93500; i <= 113000; i += 500)
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

            // 设置进度条
            TradeEventParam eventParam = new TradeEventParam();
            eventParam.CurOpt = CurOpt.ResetProcessBar;
            eventParam.HstDataCount = allCsv.Count;
            eventParam.Msg = "开始取历史数据";
            this.callBackF(eventParam);

            foreach (FilePosInfo item in allCsv)
            {
                if (item.IsFolder)
                {
                    continue;
                }

                string stockCd = Util.GetShortNameWithoutType(item.File).Substring(0, 6);
                if (this.allStockCd.Contains(stockCd))
                {
                    this.stockCdData.Add(this.GetStockCdBefChar(stockCd) + stockCd, DayBatchProcess.GetStockHistoryInfo(item.File));
                }

                // 更新进度条
                eventParam = new TradeEventParam();
                eventParam.Msg = "开始取历史数据";
                eventParam.CurOpt = CurOpt.ProcessBarStep;
                this.callBackF(eventParam);
            }

            // 隐藏进度条
            eventParam = new TradeEventParam();
            eventParam.CurOpt = CurOpt.CloseProcessBar;
            eventParam.Msg = "历史数据取得结束";
            this.callBackF(eventParam);
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
                    buySellItem.Status = BuySellStatus.Buyed;
                }
                buySellItem.TotalMoney = Convert.ToDecimal(allTotalInfo[i].Substring(2 + 1 + 8 + 1 + 6 + 1 + 7 + 1 + 5 + 1, 8));
            }
        }

        /// <summary>
        /// 处理当前交易的时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int CheckTime(string strTime)
        {
            int time = Convert.ToInt32(strTime);
            return (int)(time / 500) * 500;
        }

        #region 写交易Log

        /// <summary>
        /// 取得Log文件
        /// </summary>
        /// <returns></returns>
        private string GetAllTradeLogFile()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory + @"Log/AutoTradeLog" + this.tradeDate.ToString("yyyyMMdd") + ".txt";
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
            sb.Append(this.tradeDate.ToString("yyyy/MM/dd")).Append(" ");
            sb.Append(buySell.Time.Substring(0, 2)).Append(":");
            sb.Append(buySell.Time.Substring(2, 2)).Append(":");
            sb.Append(buySell.Time.Substring(4, 2)).Append(":");
            sb.Append(" ");
            if (orderType == OrderType.Buy)
            {
                sb.Append("B ");
                sb.Append(buySell.StockCd).Append(" ");
                sb.Append(buySell.BuyPrice.ToString().PadLeft(7, ' ')).Append(" ");
                sb.Append(buySell.BuyCnt.ToString().PadLeft(5, ' ')).Append(" ");
                sb.Append(buySell.TotalMoney.ToString().PadLeft(8, ' '));
            }
            else
            {
                sb.Append("S ");
                sb.Append(buySell.StockCd).Append(" ");
                sb.Append(buySell.SellPrice.ToString().PadLeft(7, ' ')).Append(" ");
                sb.Append(buySell.SellCnt.ToString().PadLeft(5, ' ')).Append(" ");
                sb.Append(buySell.TotalMoney.ToString().PadLeft(8, ' ')).Append(" ");
                decimal diff = (buySell.TotalMoney / this.configInfo.ThreadMoney - 1) * 100;
                sb.Append(diff.ToString("0.00").PadLeft(6, ' ')).Append("%");
            }

            // 写总的Log文件
            this.WriteComnLog(sb.ToString());

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
                    sb.Append(buySell.StockCd).Append(" ");
                    sb.Append(buySell.BuyPrice.ToString().PadLeft(7, ' ')).Append(" ");
                    sb.Append(buySell.BuyCnt.ToString().PadLeft(5, ' ')).Append(" ");
                    sb.Append(buySell.TotalMoney.ToString().PadLeft(8, ' '));
                }
                else
                {
                    sb.Append(string.Empty.PadLeft(6, ' ')).Append(" ");
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

        #endregion

        #endregion
    }
}
