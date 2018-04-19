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

namespace GuPiao
{
    /// <summary>
    /// 交易共通类
    /// </summary>
    public class TradeUtil
    {
        #region 全局变量

        /// <summary>
        /// 具体业务操作的方法
        /// </summary>
        public delegate void DoSomethingWithP(params object[] param);

        /// <summary>
        /// 定义回调方法
        /// </summary>
        public DoSomethingWithP callBackF = null;

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
        /// 订单信息
        /// </summary>
        Dictionary<string, OrderInfo> orderInfo = new Dictionary<string,OrderInfo>();

        /// <summary>
        /// 券商信息
        /// </summary>
        Dictionary<int, EZMBrokerType> BrokerMap = new Dictionary<int, EZMBrokerType>();

        /// <summary>
        /// 当前操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 当前错误信息
        /// </summary>
        public string RetMsg { get; set; }

        /// <summary>
        /// 当前操作
        /// </summary>
        public CurOpt CurOpt { get; set; }

        List<string> tradeData = new List<string>();
        List<string> hisData = new List<string>();

        string serverAddr = "";/// 券商的交易服务器IP，这儿默认模拟服务器
        string serverPost = "";
        string tradeAccount = "";    ///你的交易账号
        string loginId = "";         ///你的登录账号
        string loginPw = "";
        string deptId = "";

        /// <summary>
        /// 是否是同步操作
        /// </summary>
        bool isSyncOpt = true;

        #endregion

        #region 初始化

        public TradeUtil()
        { 
        }

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
        public void SetCallBack(DoSomethingWithP pCallBackF)
        {
            this.callBackF = pCallBackF;
        }

        /// <summary>
        /// 退出程序时释放资源
        /// </summary>
        public void TradeRelease()
        {
            this.CurOpt = CurOpt.TradeRelease;

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
            this.CurOpt = CurOpt.Init;

            this.orderInfo.Clear();

            // 初始化基础数据
            InitBaseData(cmbAccountType, cmbBrokerType);

            // 初始化对象，事件
            InitObjEvent();
        }

        /// <summary>
        /// 设置同步、异步
        /// </summary>
        /// <param name="pSync"></param>
        public void SetSync(bool pSync)
        {
            this.isSyncOpt = pSync;
        }

        /// <summary>
        /// 链接服务器
        /// </summary>
        public void ConnServer(int accountIndex, int brokerIndex)
        {
            this.CurOpt = CurOpt.ConnServer;

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
                this.RetMsg = "请先输入登录密码！";
                this.IsSuccess = false;
                return;
            }

            /// 设置其他参数
            m_StockTrade.TradeAccount = this.tradeAccount;/// 交易账号，一般为资金账号
            m_StockTrade.DepartmentID = ushort.Parse(this.deptId);/// 营业部ID

            bool bRet = false;
            if (this.isSyncOpt)
            {
                /// 指定同步连接，直到返回结果
                bRet = m_StockTrade.LogIn(false);
                if (bRet)
                {
                    this.RetMsg = "登录成功！";
                    this.IsSuccess = true;

                    /*
                    /// 无错误，获得登录成功的交易连接标识
                    ushort nTradeID = m_StockTrade.CurTradeID;

                    MessageBox.Show("登录成功，下面开始获取股东代码信息！");

                    /// 获得股东信息
                    ITradeRecord TradeRecord = m_StockTrade.ShareHolderCode;
                    if (null != TradeRecord)
                    {
                        /// 获得记录集的列数和行数
                        uint nFieldCount = TradeRecord.FieldCount;
                        uint nRecordCount = TradeRecord.RecordCount;

                        /// 弹出JSON格式数据包
                        MessageBox.Show(TradeRecord.GetJsonString());

                        for (uint i = 0; i < nRecordCount; i++)
                        {
                            /// 根据列字段名直接取数据，获取股东代码
                            var StockCode = TradeRecord.GetValueByName(i, "股东代码");
                            /// 遍历数据集合
                            for (uint j = 0; j < nFieldCount; j++)
                            {
                                /// 获取指定行和列的数据
                                var temVal = TradeRecord.GetValue(i, j);
                                var temType = TradeRecord.GetDataType(j);
                            }
                        }
                    }*/
                }
            }
            else
            {
                /// 指定异步连接,事件回调的时候会传递自己的交易接口对象
                bRet = m_StockTrade.LogIn(true);
            }

            if (!bRet)
            {
                /// 连接失败时获取错误描述和类型
                string strErrDesc = m_StockTrade.LastErrDesc;
                //EZMTradeErrType ErrType = m_StockTrade.LastErrType;
                //MessageBox.Show(strErrDesc);
                this.RetMsg = strErrDesc;
                this.IsSuccess = false;
            }
        }

        /// <summary>
        /// 关闭链接
        /// </summary>
        public void CloseServer()
        {
            this.CurOpt = CurOpt.CloseServer;

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
            this.CurOpt = CurOpt.GetStockInfo;

            ITradeRecord StockRecord = null;
            if (stockCd.Length < 6)
            {
                this.RetMsg = "查询股票代码应该是6位！";
                this.IsSuccess = false;
                return StockRecord;/// 代码错误
            }
            if (0 == m_StockTrade.CurTradeID)
            {
                this.RetMsg = "请先登录服务器再操作！";
                this.IsSuccess = false;
                return StockRecord;/// 没有登录
            }

            StockRecord = m_StockTrade.GetStockQuote(m_StockTrade.CurTradeID, stockCd);
            if (null == StockRecord)
            {
                this.RetMsg = "获取指定股票实时5档行情失败！";
                this.IsSuccess = false;
                return StockRecord;
            }
            if (0 == StockRecord.RecordCount)
            {
                this.RetMsg = "获取指定股票实时5档行情数据无记录！";
                this.IsSuccess = false;
                return StockRecord;/// 没有记录
            }

            this.IsSuccess = true;
            this.RetMsg = "获取指定股票实时5档行情数据成功！";

            return StockRecord;
        }

        /// <summary>
        /// 开始买
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        public string BuyStock(string stockCd, uint count, float price, bool isQuick) 
        {
            this.CurOpt = CurOpt.BuyStock;
            ITradeRecord StockRecord = null;

            try
            {
                StockRecord = this.GetStockInfo(stockCd);
                if (!this.IsSuccess)
                {
                    return this.RetMsg;
                }

                /// 取当前价
                float buyPrice = price;
                if (isQuick)
                {
                    //var vBuy4 = StockRecord.GetValueFloat(0, 5);
                    var vBuy5 = StockRecord.GetValueByName(0, "买五价");
                    buyPrice = (float)vBuy5;
                }

                EZMExchangeType eExchangeType = this.GetExchangeType(stockCd);
                OrderInfo order = this.NewOrder(buyPrice.ToString(), OrderType.Buy);

                if (this.isSyncOpt)
                {
                    /// 同步提交委托，知道返回结果
                    ITradeRecord OrderRecord = m_StockTrade.SyncCommitOrder(true, EZMStockOrderType.STOCKORDERTYPE_BUY,
                        EZMOrderPriceType.ORDERPRICETYPE_LIMIT, stockCd, buyPrice, count, eExchangeType);

                    // 买后的操作
                    order.ReqId = Guid.NewGuid().ToString();
                    orderInfo.Add(order.ReqId, order);

                    this.AfterBuyStock(OrderRecord, order.ReqId);
                }
                else
                {
                    /// 通过AddOrder重复调用可以实现提交多条委托，然后调用CommitOrder一次性提交到服务器
                    /// 限价买
                    uint nReq1 = m_StockTrade.AddOrder(EZMStockOrderType.STOCKORDERTYPE_BUY, EZMOrderPriceType.ORDERPRICETYPE_LIMIT, stockCd, buyPrice, count, eExchangeType);
                    //uint nReq2 = m_StockTrade.AddOrder(EZMStockOrderType.STOCKORDERTYPE_BUY, EZMOrderPriceType.ORDERPRICETYPE_LIMIT, stockCd, (float)vBuy4, count, eExchangeType);

                    order.ReqId = nReq1.ToString();
                    orderInfo.Add(order.ReqId, order);

                    /// 真正提交委托操作，每个委托结果通过事件来通知，通过AddOrder返回的请求ID标识
                    m_StockTrade.CommitOrder(m_StockTrade.CurTradeID, true, EZMRunPriType.RUNPRITYPE_NORMAL);
                }
            }
            finally
            {
                if (StockRecord != null)
                {
                    StockRecord.Clear();
                    StockRecord = null;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 开始卖
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        public string SellStock(string stockCd, uint count, float price, bool isQuick)
        {
            this.CurOpt = CurOpt.SellStock;
            ITradeRecord StockRecord = null;

            try
            {
                StockRecord = this.GetStockInfo(stockCd);
                if (!this.IsSuccess)
                {
                    return this.RetMsg;
                }

                if (this.guPiaoBaseInfo.ContainsKey(stockCd))
                {
                    GuPiaoInfo guPiaoItem = this.guPiaoBaseInfo[stockCd];
                    if (guPiaoItem.CanUseCount == 0)
                    {
                        this.IsSuccess = false;
                        this.RetMsg = "没有可用股份";
                        return this.RetMsg;
                    }
                    else
                    {
                        float sellPrice = price;
                        if (isQuick)
                        {
                            var varVal = StockRecord.GetValueByName(0, "卖一价");
                            sellPrice = (float)varVal;
                        }

                        EZMExchangeType eExchangeType = this.GetExchangeType(stockCd);
                        OrderInfo order = this.NewOrder(sellPrice.ToString(), OrderType.Sell);

                        if (this.isSyncOpt)
                        {
                            /// 同步操作，直到提交委托服务器返回结果
                            ITradeRecord SellRecord = m_StockTrade.SyncCommitOrder(true, EZMStockOrderType.STOCKORDERTYPE_SALE,
                                EZMOrderPriceType.ORDERPRICETYPE_LIMIT, stockCd, sellPrice, count, eExchangeType);
                            
                            order.ReqId = Guid.NewGuid().ToString();
                            orderInfo.Add(order.ReqId, order);

                            this.AfterSellStock(SellRecord, order.ReqId);
                        }
                        else
                        {
                            /// 返回的请求ID，会由事件通知的时候传回，从而知道每个委托的实际结果
                            uint nReqID = m_StockTrade.AddOrder(EZMStockOrderType.STOCKORDERTYPE_SALE,
                                EZMOrderPriceType.ORDERPRICETYPE_LIMIT, stockCd, sellPrice, count, eExchangeType);

                            order.ReqId = nReqID.ToString();
                            orderInfo.Add(order.ReqId, order);

                            /// 批量提交委托，结果通过事件通知得到
                            m_StockTrade.CommitOrder(m_StockTrade.CurTradeID, true, EZMRunPriType.RUNPRITYPE_NORMAL);
                        }
                    }
                }
                else
                {
                    this.IsSuccess = false;
                    this.RetMsg = "没有持股";
                    return this.RetMsg;
                }
            }
            finally
            {
                if (StockRecord != null)
                {
                    StockRecord.Clear();
                    StockRecord = null;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得持股信息
        /// </summary>
        /// <returns></returns>
        public void GetGuPiaoInfo(Dictionary<string, GuPiaoInfo> guPiaoInfo)
        {
            this.CurOpt = CurOpt.GetGuPiaoInfo;
            ITradeRecord StockRecord = null;

            try
            {
                if (0 == m_StockTrade.CurTradeID)
                {
                    return;/// 没有登录
                }

                StockRecord = m_StockTrade.QueryTradeData(m_StockTrade.CurTradeID,
                    EZMStockQueryType.STOCKQUERYTYPE_STOCK);
                if (null == StockRecord)
                {
                    return;
                }

                uint nRecordCount = StockRecord.RecordCount;
                if (0 == nRecordCount)
                {
                    return;/// 没有持股
                }

                for (uint nIndex = 0; nIndex < nRecordCount; nIndex++)
                {
                    var varVal = StockRecord.GetValueByName(nIndex, "可用股份");
                    //string strStockName = StockRecord.GetValueByName(nIndex, "证券名称").ToString();
                    //string strHolderCode = StockRecord.GetValueByName(nIndex, "股东代码").ToString();
                    string strStockCode = StockRecord.GetValueByName(nIndex, "证券代码").ToString();

                    GuPiaoInfo item = new GuPiaoInfo();
                    item.fundcode = strStockCode;
                    item.CanUseCount = (uint)varVal;
                    item.TotalCount = 0; // TODO

                    if (guPiaoInfo.ContainsKey(strStockCode))
                    {
                        guPiaoInfo[strStockCode] = item;
                    }
                    else
                    {
                        guPiaoInfo.Add(strStockCode, item);
                    }
                }
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
            this.CurOpt = CurOpt.CancelOrder;
            if (string.IsNullOrEmpty(stockCd) || stockCd.Length != 6)
            {
                this.RetMsg = "取消订单：交易代码不对 " + stockCd;
                this.IsSuccess = false;
                return;
            }

            if (string.IsNullOrEmpty(orderID))
            {
                this.RetMsg = "取消订单：订单ID为空";
                this.IsSuccess = false;
                return;
            }

            EZMExchangeType eExchangeType = this.GetExchangeType(stockCd);
            ITradeRecord OrderRecord = m_StockTrade.CancelOrder(eExchangeType, orderID);

            // 修改订单状态
            if (this.CommonCheckOrder(OrderRecord, "", OrderStatus.OrderCancel))
            {
                OrderInfo order = this.orderInfo.FirstOrDefault(p => orderID.Equals(p.Value.OrderId)).Value;
                if (orderID.Equals(order.OrderId))
                {
                    // 修改订单状态
                    order.OrderStatus = OrderStatus.OrderCancel;
                }
                else
                {
                    this.RetMsg = "取消订单：取消失败（没有找到订单信息）";
                    this.IsSuccess = false;
                }
            }

            this.IsSuccess = true;
        }

        /// <summary>
        /// 异步调用结束后的回调方法
        /// </summary>
        /// <param name="param"></param>
        public void DoCallBack(params object[] param)
        {
            uint nReqID;

            // 处理各种事件
            switch (this.CurOpt)
            {
                // 委托交易成功
                case CurOpt.OrderOKEvent:
                    nReqID = (uint) param[0];
                    ITradeRecord OrderRecord = (ITradeRecord)param[1];
                    this.AfterBuyStock(OrderRecord, nReqID.ToString());
                    break;

                // 订单成功
                case CurOpt.OrderSuccessEvent:
                    string orderId = (string)param[0];
                    string successJson = (string)param[1];
                    this.OrderSuccess(orderId, successJson);
                    break;

                // 订单错误
                case CurOpt.OrderErrEvent:
                    nReqID = (uint)param[0];
                    string errInfo = (string)param[1];
                    this.OrderError(nReqID.ToString(), errInfo);
                    break;
            }

            // 前天页面处理相关
            if (this.callBackF != null)
            {
                this.callBackF(param);
            }
        }

        #endregion

        #region 私有方法

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
            /// 初始化界面参数，模拟账号
            serverAddr = "mock.tdx.com.cn";/// 券商的交易服务器IP，这儿默认模拟服务器
            serverPost = "7708";
            tradeAccount = "";    ///你的交易账号
            loginId = "xjsxjs197";         ///你的登录账号
            loginPw = "xjsxjs197";
            deptId = "9000";

            cmbAccountType.Items.Clear();
            cmbAccountType.Items.Add("模拟");
            cmbAccountType.Items.Add("资金账号");
            cmbAccountType.Items.Add("客户号");
            cmbAccountType.SelectedIndex = 0;

            cmbBrokerType.Items.Clear();
            cmbBrokerType.Items.Add("模拟测试");
            cmbBrokerType.Items.Add("平安证券");
            cmbBrokerType.Items.Add("招商证券");
            cmbBrokerType.SelectedIndex = 0;

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
                //m_StockTrade.AuthFile = "D:\\TradeAuth.zmd";

                /// 设置通讯版本(请查看自己券商的TDX版本)，初始化结果异步通过事件通知
                /// 设置最大连接数，默认传1(最好跟调用登录前设置的服务器主机数量一致)
                m_StockTrade.Init("8.05", 1);
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
                    if (this.orderInfo.ContainsKey(reqId))
                    {
                        OrderInfo order = this.orderInfo[reqId];
                        order.OrderStatus = orderStatus;

                        ///测试JSON格式数据包
                        //MessageBox.Show(OrderRecord.GetJsonString());

                        /// 获取前面委托成功的ID
                        /// 注意有些券商返回的委托成功记录第一个不是委托编号，需要调用GetValueByName来获取委托编号
                        order.OrderId = OrderRecord.GetValueByName(0, "委托编号").ToString();
                    }
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
        /// 买后的操作
        /// </summary>
        /// <param name="OrderRecord"></param>
        private void AfterBuyStock(ITradeRecord OrderRecord, string reqId)
        {
            if (this.CommonCheckOrder(OrderRecord, reqId, OrderStatus.Waiting))
            {
                
            }
        }

        /// <summary>
        /// 卖后的操作
        /// </summary>
        /// <param name="OrderRecord"></param>
        private void AfterSellStock(ITradeRecord OrderRecord, string reqId)
        {
            if (this.CommonCheckOrder(OrderRecord, reqId, OrderStatus.Waiting))
            {
                
            }
        }

        /// <summary>
        /// 订单成功的处理
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="successJson"></param>
        private void OrderSuccess(string orderId, string successJson)
        {
            OrderInfo order = this.orderInfo.FirstOrDefault(p => orderId.Equals(p.Value.OrderId)).Value;
            if (orderId.Equals(order.OrderId))
            {
                // 修改订单状态
                order.OrderStatus = OrderStatus.OrderOk;
                this.RetMsg = "取消订单：取消失败（没有找到订单信息）";
            }
            else
            {
                this.RetMsg = "订单成功：但是没有更新状态";
            }

            this.IsSuccess = true;
        }

        /// <summary>
        /// 订单错误的处理
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="errInfo"></param>
        private void OrderError(string reqId, string errInfo)
        {
            if (this.orderInfo.ContainsKey(reqId))
            {
                OrderInfo order = this.orderInfo[reqId];
                order.OrderStatus = OrderStatus.OrderError;
                order.RetMsg = errInfo;

                this.RetMsg = "订单错误：" + errInfo;
                this.IsSuccess = false;
            }
        }

        #endregion
    }
}
