using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZMTradeCom;
using GuPiaoTool;
using System.IO;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 交易共通类
    /// </summary>
    public class TradeUtil
    {
        #region 全局变量

        /// <summary>
        ///  接口对象定义，每账号支持多个服务器的多连接。高级版支持多账号，通过定义多个对象，具体每个对象设置不同的账号和连接服务器地址即可。
        /// </summary>
        StockTradeClass m_StockTrade = null;

        /// <summary>
        /// 接口对象事件处理对象
        /// </summary>
        TradeEventSink m_TradeEvent = null;

        /// <summary>
        /// 股票基础信息
        /// </summary>
        Dictionary<string, GuPiaoInfo> guPiaoBaseInfo;

        /// <summary>
        /// 券商信息
        /// </summary>
        Dictionary<int, EZMBrokerType> BrokerMap = new Dictionary<int, EZMBrokerType>();

        /// <summary>
        /// 定义回调方法
        /// </summary>
        GuPiao.AutoTradeBase.DoSomethingWithP callBackF = null;

        /// <summary>
        /// 事件的参数
        /// </summary>
        public TradeEventParam eventParam = new TradeEventParam();

        List<string> tradeData = new List<string>();
        List<string> hisData = new List<string>();

        string serverAddr = "";/// 券商的交易服务器IP，这儿默认模拟服务器
        string serverPost = "";
        string tradeAccount = "";    ///你的交易账号
        string loginId = "";         ///你的登录账号
        string loginPw = "";
        string deptId = "";
        string clentVer = ""; /// 客户端版本

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置股票基础信息
        /// </summary>
        /// <param name="pGuPiaoBaseInfo"></param>
        public void SetGuPiaoInfo(Dictionary<string, GuPiaoInfo> pGuPiaoBaseInfo)
        {
            this.guPiaoBaseInfo = pGuPiaoBaseInfo;
        }

        /// <summary>
        /// 设置回调方法
        /// </summary>
        /// <param name="pCallBackF"></param>
        public void SetCallBack(GuPiao.AutoTradeBase.DoSomethingWithP pCallBackF)
        {
            this.callBackF = pCallBackF;
        }

        /// <summary>
        /// 退出程序时释放资源
        /// </summary>
        public void TradeRelease()
        {
            this.eventParam.CurOpt = CurOpt.TradeRelease;

            /// 移除事件挂接
            m_StockTrade.InitEvent -= m_TradeEvent.InitEvent;
            m_StockTrade.LoginEvent -= m_TradeEvent.LoginEvent;
            m_StockTrade.OrderOKEvent -= m_TradeEvent.OrderOKEvent;
            m_StockTrade.OrderErrEvent -= m_TradeEvent.OrderErrEvent;
            m_StockTrade.OrderSuccessEvent -= m_TradeEvent.OrderSuccessEvent;
            m_StockTrade.StockQuoteEvent -= m_TradeEvent.StockQuoteEvent;

            m_StockTrade.ServerErrEvent -= m_TradeEvent.ServerErrEvent;
            m_StockTrade.ServerChangedEvent -= m_TradeEvent.ServerChangedEvent;

            if (null != m_StockTrade)
            {
                m_TradeEvent.ReleaseTrade();
                m_TradeEvent = null;
            }

            m_StockTrade = null;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(ComboBox cmbAccountType, ComboBox cmbBrokerType)
        {
            this.eventParam.CurOpt = CurOpt.Init;
            this.eventParam.IsSuccess = false;

            // 初始化基础数据
            InitBaseData(cmbAccountType, cmbBrokerType);

            // 初始化对象，事件
            InitObjEvent();
        }

        /// <summary>
        /// 链接服务器
        /// </summary>
        public void ConnServer(int accountIndex, int brokerIndex)
        {
            this.eventParam.CurOpt = CurOpt.ConnServer;

            /// 自动保持连接
            m_StockTrade.AutoKeepConn = true;
            /// 设置为普通账号,true为信用账号
            m_StockTrade.CreditAccount = false;
            /// 设置成交自动回报定时器1000毫秒，设为0表示不启用
            m_StockTrade.ReportSuccessTimer = 1000;

            /// 设置券商
            m_StockTrade.BrokerType = this.BrokerMap[brokerIndex];

            /// 设置登录账号类型
            EZMLoginAccountType eAccountType = EZMLoginAccountType.LOGINIACCOUNTTYPE_MNCS;
            if (1 == accountIndex)
            {
                eAccountType = EZMLoginAccountType.LOGINIACCOUNTTYPE_CAPITAL;
            }
            else if (2 == accountIndex)
            {
                eAccountType = EZMLoginAccountType.LOGINIACCOUNTTYPE_CUSTOMER;
            }
            m_StockTrade.AccountType = eAccountType;

            /// 设置登录服务器
            m_StockTrade.CurServerHost = this.serverAddr;
            m_StockTrade.CurServerPort = ushort.Parse(this.serverPost);

            /// 设置服务器交易账户和密码
            m_StockTrade.LoginID = this.loginId;
            m_StockTrade.TradePassword = this.loginPw;
            if (0 == this.loginPw.Length)
            {
                this.eventParam.Msg = "请先输入登录密码！";
                this.eventParam.IsSuccess = false;
                this.DoCallBack(null);
                return;
            }

            /// 设置其他参数
            m_StockTrade.TradeAccount = this.tradeAccount;/// 交易账号，一般为资金账号
            m_StockTrade.DepartmentID = ushort.Parse(this.deptId);/// 营业部ID

            /// 指定异步连接,事件回调的时候会传递自己的交易接口对象
            bool bRet = m_StockTrade.LogIn(true);
            if (!bRet)
            {
                /// 连接失败时获取错误描述和类型
                this.eventParam.Msg = m_StockTrade.LastErrDesc;
                this.eventParam.IsSuccess = false;
                this.DoCallBack(null);
            }
        }

        /// <summary>
        /// 关闭链接
        /// </summary>
        public void CloseServer()
        {
            this.eventParam.CurOpt = CurOpt.CloseServer;

            if (m_StockTrade.CurTradeID > 0)
            {
                m_StockTrade.LogOut(m_StockTrade.CurTradeID);
            }
        }

        /// <summary>
        /// 获取5档信息
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        public ITradeRecord GetStockInfo(string stockCd)
        {
            this.eventParam.CurOpt = CurOpt.GetStockInfo;

            ITradeRecord StockRecord = null;
            if (stockCd.Length < 6)
            {
                this.eventParam.Msg = "查询股票代码应该是6位！";
                this.eventParam.IsSuccess = false;
                return StockRecord;/// 代码错误
            }
            if (0 == m_StockTrade.CurTradeID)
            {
                this.eventParam.Msg = "请先登录服务器再操作！";
                this.eventParam.IsSuccess = false;
                return StockRecord;/// 没有登录
            }

            StockRecord = m_StockTrade.GetStockQuote(m_StockTrade.CurTradeID, stockCd);
            if (null == StockRecord)
            {
                this.eventParam.Msg = "获取指定股票实时5档行情失败！";
                this.eventParam.IsSuccess = false;
                return StockRecord;
            }
            if (0 == StockRecord.RecordCount)
            {
                this.eventParam.Msg = "获取指定股票实时5档行情数据无记录！";
                this.eventParam.IsSuccess = false;
                return StockRecord;/// 没有记录
            }

            this.eventParam.IsSuccess = true;
            this.eventParam.Msg = "获取指定股票实时5档行情数据成功！";

            return StockRecord;
        }

        /// <summary>
        /// 开始买
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        public string BuyStock(string stockCd, uint count, float price, BuySellType buySellType) 
        {
            this.eventParam.CurOpt = CurOpt.BuyStock;
            ITradeRecord StockRecord = null;

            try
            {
                StockRecord = this.GetStockInfo(stockCd);
                if (!this.eventParam.IsSuccess)
                {
                    return this.eventParam.Msg;
                }

                /// 取当前价
                float buyPrice = price;
                if (buySellType == BuySellType.QuickBuy)
                {
                    var vBuy5 = StockRecord.GetValueByName(0, "卖一价");
                    buyPrice = (float)vBuy5;
                }
                else if (buySellType == BuySellType.SuperQuickBuy)
                {
                    var vBuy5 = StockRecord.GetValueByName(0, "卖三价");
                    buyPrice = (float)vBuy5;
                }

                EZMExchangeType eExchangeType = this.GetExchangeType(stockCd);

                /// 通过AddOrder重复调用可以实现提交多条委托，然后调用CommitOrder一次性提交到服务器
                if (buySellType == BuySellType.SuperQuickBuy)
                {
                    uint nReq1 = m_StockTrade.AddOrder(EZMStockOrderType.STOCKORDERTYPE_BUY, EZMOrderPriceType.ORDERPRICETYPE_MARKETFIVETOCANCEL, stockCd, buyPrice, count, eExchangeType);
                }
                else
                {
                    uint nReq1 = m_StockTrade.AddOrder(EZMStockOrderType.STOCKORDERTYPE_BUY, EZMOrderPriceType.ORDERPRICETYPE_LIMIT, stockCd, buyPrice, count, eExchangeType);
                }

                /// 真正提交委托操作，每个委托结果通过事件来通知，通过AddOrder返回的请求ID标识
                m_StockTrade.CommitOrder(m_StockTrade.CurTradeID, true, EZMRunPriType.RUNPRITYPE_NORMAL);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                if (StockRecord != null)
                {
                    StockRecord.Clear();
                    StockRecord = null;
                }
            }

            return "开始买......";
        }

        /// <summary>
        /// 开始卖
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        public string SellStock(string stockCd, uint count, float price, BuySellType buySellType)
        {
            this.eventParam.CurOpt = CurOpt.SellStock;
            ITradeRecord StockRecord = null;

            try
            {
                StockRecord = this.GetStockInfo(stockCd);
                if (!this.eventParam.IsSuccess)
                {
                    return this.eventParam.Msg;
                }

                if (this.guPiaoBaseInfo.ContainsKey(stockCd))
                {
                    GuPiaoInfo guPiaoItem = this.guPiaoBaseInfo[stockCd];
                    if (guPiaoItem.CanUseCount == 0)
                    {
                        this.eventParam.IsSuccess = false;
                        this.eventParam.Msg = "没有可用股份";
                        return this.eventParam.Msg;
                    }
                    else
                    {
                        float sellPrice = price;
                        if (buySellType == BuySellType.QuickSell)
                        {
                            var varVal = StockRecord.GetValueByName(0, "买一价");
                            sellPrice = (float)varVal;
                        }
                        else if (buySellType == BuySellType.SuperQuickSell)
                        {
                            var varVal = StockRecord.GetValueByName(0, "买三价");
                            sellPrice = (float)varVal;
                        }

                        EZMExchangeType eExchangeType = this.GetExchangeType(stockCd);

                        /// 返回的请求ID，会由事件通知的时候传回，从而知道每个委托的实际结果
                        if (buySellType == BuySellType.SuperQuickSell)
                        {
                            uint nReqID = m_StockTrade.AddOrder(EZMStockOrderType.STOCKORDERTYPE_SALE,
                                EZMOrderPriceType.ORDERPRICETYPE_MARKETFIVETOCANCEL, stockCd, sellPrice, count, eExchangeType);
                        }
                        else
                        {
                            uint nReqID = m_StockTrade.AddOrder(EZMStockOrderType.STOCKORDERTYPE_SALE,
                                EZMOrderPriceType.ORDERPRICETYPE_LIMIT, stockCd, sellPrice, count, eExchangeType);
                        }

                        /// 批量提交委托，结果通过事件通知得到
                        m_StockTrade.CommitOrder(m_StockTrade.CurTradeID, true, EZMRunPriType.RUNPRITYPE_NORMAL);
                    }
                }
                else
                {
                    this.eventParam.IsSuccess = false;
                    this.eventParam.Msg = "没有持股";
                    return this.eventParam.Msg;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                if (StockRecord != null)
                {
                    StockRecord.Clear();
                    StockRecord = null;
                }
            }

            return "开始卖......";
        }

        /// <summary>
        /// 取得持股信息
        /// </summary>
        /// <returns></returns>
        public void GetGuPiaoInfo(Dictionary<string, GuPiaoInfo> guPiaoInfo)
        {
            this.eventParam.CurOpt = CurOpt.GetGuPiaoInfo;
            ITradeRecord StockRecord = null;

            try
            {
                if (0 == m_StockTrade.CurTradeID)
                {
                    this.eventParam.IsSuccess = false;
                    this.eventParam.Msg = "没有登录";
                    this.callBackF(this.eventParam);
                    return;/// 没有登录
                }

                if (!this.eventParam.IsSuccess)
                {
                    this.eventParam.Msg = "没有正常登录";
                    this.callBackF(this.eventParam);
                    return;/// 没有登录
                }

                StockRecord = m_StockTrade.QueryTradeData(m_StockTrade.CurTradeID,
                    EZMStockQueryType.STOCKQUERYTYPE_STOCK);
                if (null == StockRecord)
                {
                    this.eventParam.IsSuccess = false;
                    this.eventParam.Msg = m_StockTrade.LastErrDesc;
                    this.callBackF(this.eventParam);
                    return;
                }

                uint nRecordCount = StockRecord.RecordCount;
                if (0 == nRecordCount)
                {
                    this.eventParam.IsSuccess = true;
                    this.eventParam.Msg = "没有持股";
                    this.callBackF(this.eventParam);
                    return;/// 没有持股
                }

                for (uint nIndex = 0; nIndex < nRecordCount; nIndex++)
                {
                    //MessageBox.Show(StockRecord.GetJsonString());
                    var canUseVal = StockRecord.GetValueByName(nIndex, "可用股份");
                    var totalVal = StockRecord.GetValueByName(nIndex, "股份余额");
                    //string strStockName = StockRecord.GetValueByName(nIndex, "证券名称").ToString();
                    //string strHolderCode = StockRecord.GetValueByName(nIndex, "股东代码").ToString();
                    string strStockCode = StockRecord.GetValueByName(nIndex, "证券代码").ToString();

                    GuPiaoInfo item = new GuPiaoInfo();
                    item.fundcode = strStockCode;
                    item.CanUseCount = Convert.ToUInt32(canUseVal);
                    item.TotalCount = Convert.ToUInt32(totalVal);

                    if (guPiaoInfo.ContainsKey(strStockCode))
                    {
                        guPiaoInfo[strStockCode] = item;
                    }
                    else
                    {
                        guPiaoInfo.Add(strStockCode, item);
                    }
                }

                this.eventParam.IsSuccess = true;
                this.eventParam.Msg = "取得信息成功";
                this.callBackF(this.eventParam);
            }
            finally
            {
                if (StockRecord != null)
                {
                    StockRecord.Clear();
                    StockRecord = null;
                }
            }
        }

        /// <summary>
        /// 取得当天委托信息
        /// </summary>
        /// <returns></returns>
        public void GetTodayPiaoInfo(List<OrderInfo> guPiaoInfo)
        {
            this.eventParam.CurOpt = CurOpt.GetTodayPiaoInfo;
            ITradeRecord StockRecord = null;

            try
            {
                if (0 == m_StockTrade.CurTradeID)
                {
                    this.eventParam.IsSuccess = false;
                    this.eventParam.Msg = "没有登录";
                    this.callBackF(this.eventParam);
                    return;/// 没有登录
                }

                if (!this.eventParam.IsSuccess)
                {
                    this.eventParam.Msg = "没有正常登录";
                    this.callBackF(this.eventParam);
                    return;/// 没有登录
                }

                StockRecord = m_StockTrade.QueryTradeData(m_StockTrade.CurTradeID,
                    EZMStockQueryType.STOCKQUERYTYPE_TODAYORDER);
                if (null == StockRecord)
                {
                    this.eventParam.IsSuccess = false;
                    this.eventParam.Msg = m_StockTrade.LastErrDesc;
                    this.callBackF(this.eventParam);
                    return;
                }

                uint nRecordCount = StockRecord.RecordCount;
                if (0 == nRecordCount)
                {
                    this.eventParam.IsSuccess = true;
                    this.eventParam.Msg = "当天没有委托";
                    this.callBackF(this.eventParam);
                    return;/// 没有持股
                }

                //MessageBox.Show(StockRecord.GetTitleJson() + "\n" + StockRecord.GetJsonString());
                //MessageBox.Show(nRecordCount + " ");
                for (uint nIndex = 0; nIndex < nRecordCount; nIndex++)
                {
                    OrderInfo item;
                    string orderId = Convert.ToString(StockRecord.GetValueByName(nIndex, "委托编号"));
                    OrderInfo oldItem = guPiaoInfo.FirstOrDefault(p => orderId.Equals(p.OrderId));
                    if (oldItem != null && !string.IsNullOrEmpty(orderId) && orderId.Equals(oldItem.OrderId))
                    {
                        // 更新旧的信息
                        item = oldItem;
                    }
                    else
                    {
                        // 追加新的信息
                        item = new OrderInfo();
                        guPiaoInfo.Insert(0, item);
                    }

                    item.OrderDate = Convert.ToString(StockRecord.GetValueByName(nIndex, "委托时间"));
                    item.OrderId = orderId;
                    item.StockCd = Convert.ToString(StockRecord.GetValueByName(nIndex, "证券代码"));
                    if ("卖出".Equals(Convert.ToString(StockRecord.GetValueByName(nIndex, "买卖标志1"))))
                    {
                        item.OrderType = OrderType.Sell;
                    }
                    else
                    {
                        item.OrderType = OrderType.Buy;
                    }
                    item.Price = Convert.ToString(StockRecord.GetValueByName(nIndex, "委托价格"));
                    item.Count = Convert.ToUInt32(StockRecord.GetValueByName(nIndex, "委托数量"));
                    item.OrderStatus = OrderStatus.Waiting;
                    if (Convert.ToUInt32(StockRecord.GetValueByName(nIndex, "成交数量")) > 0)
                    {
                        item.OrderStatus = OrderStatus.OrderOk;
                    }
                    else if (Convert.ToUInt32(StockRecord.GetValueByName(nIndex, "已撤数量")) > 0)
                    {
                        item.OrderStatus = OrderStatus.OrderCancel;
                    }
                }

                guPiaoInfo.Sort(this.OrderInfoCompare);

                //MessageBox.Show(guPiaoInfo.Count + " ");

                this.eventParam.IsSuccess = true;
                this.eventParam.Msg = "取得当天委托信息成功，共 " + nRecordCount + " 条委托";
                this.callBackF(this.eventParam);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                if (StockRecord != null)
                {
                    StockRecord.Clear();
                    StockRecord = null;
                }
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderID"></param>
        public void CancelOrder(string stockCd, string orderID)
        {
            this.eventParam.CurOpt = CurOpt.CancelOrder;
            if (string.IsNullOrEmpty(stockCd) || stockCd.Length != 6)
            {
                this.eventParam.Msg = "取消订单：交易代码不对 " + stockCd;
                this.eventParam.IsSuccess = false;
                this.callBackF(this.eventParam);
                return;
            }

            if (string.IsNullOrEmpty(orderID))
            {
                this.eventParam.Msg = "取消订单：订单ID为空";
                this.eventParam.IsSuccess = false;
                this.callBackF(this.eventParam);
                return;
            }

            EZMExchangeType eExchangeType = this.GetExchangeType(stockCd);
            ITradeRecord OrderRecord = m_StockTrade.CancelOrder(eExchangeType, orderID);

            // 修改订单状态
            if (this.CommonCheckOrder(OrderRecord, "", OrderStatus.OrderCancel))
            {
                // 刷新履历信息
                this.eventParam.IsSuccess = true;
                this.eventParam.Msg = "取消订单：成功";
                this.callBackF(this.eventParam);
            }
            else
            {
                this.callBackF(this.eventParam);
            }
        }

        /// <summary>
        /// 异步事件结束后的回调方法
        /// </summary>
        public void DoCallBack(params object[] param)
        {
            uint nReqID;

            // 处理各种事件
            switch (this.eventParam.CurOpt)
            {
                case CurOpt.LoginEvent:
                    if (this.eventParam.IsSuccess)
                    {
                        this.GetMoneyInfo();
                    }
                    break;

                // 委托交易成功
                case CurOpt.OrderOKEvent:
                    nReqID = (uint) param[0];
                    ITradeRecord OrderRecord = (ITradeRecord)param[1];
                    this.AfterBuySellStock(OrderRecord, nReqID.ToString());

                    this.GetMoneyInfo();
                    break;

                // 订单成功
                case CurOpt.OrderSuccessEvent:
                    string orderId = (string)param[0];
                    string successJson = (string)param[1];
                    this.OrderSuccess(orderId, successJson);

                    this.GetMoneyInfo();
                    break;

                // 订单错误
                case CurOpt.OrderErrEvent:
                    nReqID = (uint)param[0];
                    string errInfo = (string)param[1];
                    this.OrderError(nReqID.ToString(), errInfo);
                    break;
            }

            // 前台页面处理相关
            if (this.callBackF != null)
            {
                this.callBackF(this.eventParam);
            }
        }

        /// <summary>
        /// 取得当前最新的金额信息
        /// </summary>
        public void GetCurrentMoneyInfo()
        {
            ITradeRecord StockRecord = null;

            try
            {
                StockRecord = this.GetStockInfo("000001");
                if (this.eventParam.IsSuccess)
                {
                    this.GetMoneyInfo();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                if (StockRecord != null)
                {
                    StockRecord.Clear();
                    StockRecord = null;
                }
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 取得金额信息
        /// </summary>
        /// <returns></returns>
        private void GetMoneyInfo()
        { 
            ITradeRecord stockRecord = m_StockTrade.CapitalInfo;
            //MessageBox.Show(stockRecord.GetJsonString());
            this.eventParam.TotalMoney = Convert.ToDecimal(stockRecord.GetValueByName(0, "资金余额"));
            this.eventParam.GuPiaoMoney = Convert.ToDecimal(stockRecord.GetValueByName(0, "最新市值"));
            this.eventParam.CanUseMoney = Convert.ToDecimal(stockRecord.GetValueByName(0, "可用资金"));
            this.eventParam.CanGetMoney = Convert.ToDecimal(stockRecord.GetValueByName(0, "可取资金"));

            //return new object[] { stockRecord.GetValueByName(0, "资金余额")
            //                , stockRecord.GetValueByName(0, "最新市值")
            //                , stockRecord.GetValueByName(0, "可用资金")
            //                , stockRecord.GetValueByName(0, "可取资金")
            //            };
        }

        /// <summary>
        /// 对象比较
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int OrderInfoCompare(OrderInfo a, OrderInfo b)
        {
            return b.OrderDate.CompareTo(a.OrderDate);
        }

        /// <summary>
        /// 生成一个订单
        /// </summary>
        /// <param name="price"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        private OrderInfo NewOrder(string price, OrderType orderType)
        {
            OrderInfo order = new OrderInfo();
            order.OrderType = orderType;
            order.Price = price;

            order.OrderDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            order.OrderStatus = OrderStatus.None;

            return order;
        }

        /// <summary>
        /// 取得股票类型
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        private EZMExchangeType GetExchangeType(string stockCd)
        {
            EZMExchangeType eExchangeType = EZMExchangeType.EXCHANGETYPE_SH;
            string startCd = stockCd.Substring(0, 1);
            if ("0" == startCd || "1" == startCd || "3" == startCd)
            {
                eExchangeType = EZMExchangeType.EXCHANGETYPE_SZ;
            }

            return eExchangeType;
        }

        /// <summary>
        /// 初始化基础数据
        /// </summary>
        private void InitBaseData(ComboBox cmbAccountType, ComboBox cmbBrokerType)
        {
            string[] baseInfos = File.ReadAllLines(@".\ConnectInfo.txt");

            /// 初始化界面参数，模拟账号
            serverAddr = baseInfos[0];///"mock.tdx.com.cn"; //  券商的交易服务器IP，这儿默认模拟服务器
            serverPost = baseInfos[1]; //"7708"; // ;
            tradeAccount = baseInfos[2]; ///你的交易账号
            loginId = baseInfos[2]; // ///你的登录账号
            loginPw = baseInfos[3];
            deptId = baseInfos[4];
            clentVer = baseInfos[5];

            cmbAccountType.Items.Clear();
            cmbAccountType.Items.Add("模拟");
            cmbAccountType.Items.Add("资金账号");
            cmbAccountType.Items.Add("客户号");
            cmbAccountType.SelectedIndex = 1;

            cmbBrokerType.Items.Clear();
            cmbBrokerType.Items.Add("模拟测试");
            cmbBrokerType.Items.Add("平安证券");
            cmbBrokerType.Items.Add("招商证券");
            cmbBrokerType.SelectedIndex = 1;

            BrokerMap.Add(0, EZMBrokerType.BROKERTYPE_MNCS);
            BrokerMap.Add(1, EZMBrokerType.BROKERTYPE_PAZQ);
            BrokerMap.Add(2, EZMBrokerType.BROKERTYPE_ZSZQ);

            tradeData.Add("资金");
            tradeData.Add("股份");
            tradeData.Add("当日委托");
            tradeData.Add("当日成交");
            tradeData.Add("当日可撤委托");
            tradeData.Add("股东代码");
            tradeData.Add("融资余额");
            tradeData.Add("融券余额");
            tradeData.Add("可融证券");
            tradeData.Add("可申购新股");
            tradeData.Add("新股申购额度");
            tradeData.Add("配号");
            tradeData.Add("中签");

            hisData.Add("历史委托");
            hisData.Add("历史成交");
            hisData.Add("资金流水");
            hisData.Add("交割单");
        }

        /// <summary>
        /// 初始化对象，事件
        /// </summary>
        private void InitObjEvent()
        {
            /// 创建对象并初始化
            m_StockTrade = new StockTradeClass();
            if (null != m_StockTrade)
            {
                m_TradeEvent = new TradeEventSink(this);
                m_TradeEvent.SetIndex(1);

                /// 挂接事件
                m_StockTrade.InitEvent += m_TradeEvent.InitEvent;
                m_StockTrade.LoginEvent += m_TradeEvent.LoginEvent;
                m_StockTrade.OrderOKEvent += m_TradeEvent.OrderOKEvent;
                m_StockTrade.OrderErrEvent += m_TradeEvent.OrderErrEvent;
                m_StockTrade.OrderSuccessEvent += m_TradeEvent.OrderSuccessEvent;
                m_StockTrade.StockQuoteEvent += m_TradeEvent.StockQuoteEvent;

                m_StockTrade.ServerErrEvent += m_TradeEvent.ServerErrEvent;
                m_StockTrade.ServerChangedEvent += m_TradeEvent.ServerChangedEvent;

                /// 启用日志输出，便于调试程序
                m_StockTrade.EnableLog = true;

                /// 测试指定授权文件路径，否则使用默认和COM组件同目录的TradeAuth.zmd
                m_StockTrade.AuthFile = @".\TradeAuth.zmd";

                /// 设置通讯版本(请查看自己券商的TDX版本)，初始化结果异步通过事件通知
                /// 设置最大连接数，默认传1(最好跟调用登录前设置的服务器主机数量一致)
                m_StockTrade.Init(this.clentVer, 1);
            }
            else
            {
                /// 创建失败，请检查是否正常注册完成
                MessageBox.Show("创建失败，请检查是否正常注册完成");
            }
        }

        /// <summary>
        /// 订单的通常检查
        /// </summary>
        /// <param name="OrderRecord"></param>
        /// <param name="reqId"></param>
        private bool CommonCheckOrder(ITradeRecord OrderRecord, string reqId, OrderStatus orderStatus)
        {
            if (null != OrderRecord)
            {
                if (OrderRecord.RecordCount > 0)
                {
                    return true;
                }
                else
                {
                    this.OrderError(reqId, "RecordCount为0");
                }
            }
            else
            {
                this.OrderError(reqId, m_StockTrade.LastErrDesc);
            }

            return false;
        }

        /// <summary>
        /// 买卖后的操作
        /// </summary>
        /// <param name="OrderRecord"></param>
        private void AfterBuySellStock(ITradeRecord OrderRecord, string reqId)
        {
            if (this.CommonCheckOrder(OrderRecord, reqId, OrderStatus.Waiting))
            {
            }
            else
            {
                //MessageBox.Show(m_StockTrade.LastErrDesc);
            }
        }

        /// <summary>
        /// 订单成功的处理
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="successJson"></param>
        private void OrderSuccess(string reqId, string successJson)
        {
            this.eventParam.Msg = "订单成功";
            this.eventParam.IsSuccess = true;
        }

        /// <summary>
        /// 订单错误的处理
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="errInfo"></param>
        private void OrderError(string reqId, string errInfo)
        {
            this.eventParam.Msg = "订单错误：" + errInfo;
            this.eventParam.IsSuccess = false;
        }

        #endregion
    }
}
