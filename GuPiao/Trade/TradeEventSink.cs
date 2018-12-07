using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZMTradeCom;

namespace GuPiao
{
    /// <summary>
    /// 交易事件类
    /// </summary>
    public class TradeEventSink : _ITradeEvents
    {
        /// <summary>
        ///  记录事件回调对应的交易对象
        /// </summary>
        private IStockTrade m_spiTrade = null;

        /// <summary>
        /// 记录交易对象的序号，多账号下用
        /// </summary>
        private ushort m_nTradeIndex = 0;

        /// <summary>
        /// 全局交易对象
        /// </summary>
        private TradeUtil tradeUtil;

        /// <summary>
        /// 全局初始化
        /// </summary>
        /// <param name="pTradeUtil"></param>
        public TradeEventSink(TradeUtil pTradeUtil)
        {
            this.tradeUtil = pTradeUtil;
        }

        /// <summary>
        /// 初始化接口通知
        /// </summary>
        /// <param name="vTrade" Desc="交易接口对象"></param>
        /// <param name="bLoginOK" Desc="是否登录成功标记"></param>
        public void InitEvent(object vTrade, bool bLoginOK)
        {
            this.tradeUtil.CurOpt = CurOpt.InitEvent;

            /// 获得接口对象
            if (null == m_spiTrade)
            {
                m_spiTrade = (IStockTrade)vTrade;
            }

            this.tradeUtil.IsSuccess = bLoginOK;
            if (bLoginOK)
            {
                this.tradeUtil.RetMsg = "初始化成功";
            }
            else
            {
                this.tradeUtil.RetMsg = m_spiTrade.LastErrDesc;
            }
            
            this.tradeUtil.DoCallBack(null);
        }

        /// <summary>
        /// 登录服务器成功通知
        /// </summary>
        /// <param name="vTrade" Desc="交易接口对象"></param>
        /// <param name="nTradeID" Desc="交易ID标识"></param>
        /// <param name="strHost" Desc="当前登录的交易服务器主机"></param>
        /// <param name="nPort" Desc="当前登录的交易服务器端口"></param>
        /// <param name="bLoginOK" Desc="是否登录成功标记"></param>
        public void LoginEvent(object vTrade, ushort nTradeID, string strHost, ushort nPort, bool bLoginOK)
        {
            this.tradeUtil.CurOpt = CurOpt.LoginEvent;

            if (null == m_spiTrade)
            {
                m_spiTrade = (IStockTrade)vTrade;
            }

            if (bLoginOK)
            {
                /// 异步事件处理中，请尽量不要有阻塞操作，避免影响底层流程正常处理。弹出消息框仅限于调试程序
                if (1 == m_nTradeIndex)
                {
                    this.tradeUtil.isLoginOk = true;

                    this.tradeUtil.IsSuccess = true;
                    this.tradeUtil.RetMsg = "异步：连接成功！";
                    //this.tradeUtil.RetMsg = "链接速度（" + m_spiTrade.CurServerHost + "）： " + m_spiTrade.ConnSpeed.ToString();
                }
                //else if (2 == m_nTradeIndex)
                //{
                //    MessageBox.Show("异步连接2成功！下面开始获取股东代码信息！");
                //}

                ///// 可以检测连接状态有效性
                //bool bValid = m_spiTrade.ConnectValid;
                ///// 获取股东信息对象
                //ITradeRecord ShareHolder = m_spiTrade.ShareHolderCode;
                //if (null != ShareHolder)
                //{
                //    MessageBox.Show(ShareHolder.GetJsonString());
                //    ShareHolder = null;
                //}

                //MessageBox.Show("下面开始演示批量获取股票实时5档行情");
                
                ///// 异步获取多只股票的5档实时行情
                //m_spiTrade.GetStockQuotes(m_spiTrade.CurTradeID, "000001;000002;600001", EZMRunPriType.RUNPRITYPE_ABOVE_NORMAL);

            }
            else
            {
                /// 弹出登录错误提示
                this.tradeUtil.IsSuccess = false;
                this.tradeUtil.RetMsg = "异步：" + m_spiTrade.LastErrDesc;
            }

            this.tradeUtil.DoCallBack(null);
        }

        /// <summary>
        /// 委托交易提交成功通知
        /// </summary>
        /// <param name="nReqID" Desc="本组件维护的交易请求序列号"></param>
        /// <param name="eExchangeType Desc="交易市场类型""></param>
        /// <param name="vRecord" Desc="交易结果对象"></param>
        public void OrderOKEvent(uint nReqID, EZMExchangeType eExchangeType, object vRecord)
        {
            this.tradeUtil.CurOpt = CurOpt.OrderOKEvent;
            this.tradeUtil.DoCallBack(new object[] { nReqID, (ITradeRecord)vRecord });
        }

        /// <summary>
        /// 委托交易成交通知，当日提交的委托，实际成交后发送的结果通知
        /// </summary>
        /// <param name="strOrderID" Desc="券商服务器上的委托ID标识"></param>
        /// <param name="strSuccessJson" Desc="成功的JSON数据包"></param>
        public void OrderSuccessEvent(string strOrderID, string strSuccessJson)
        {
            this.tradeUtil.CurOpt = CurOpt.OrderSuccessEvent;
            this.tradeUtil.DoCallBack(new object[] { strOrderID, strSuccessJson });
        }

        /// <summary>
        /// 委托错误通知
        /// </summary>
        /// <param name="nReqID" Desc="本组件维护的交易请求序列号"></param>
        /// <param name="ErrInfo"></param>
        public void OrderErrEvent(uint nReqID, string ErrInfo)
        {
            this.tradeUtil.CurOpt = CurOpt.OrderErrEvent;
            this.tradeUtil.DoCallBack(new object[] { nReqID, ErrInfo });
        }

        /// <summary>
        /// 查询股票实时5档行情返回数据通知
        /// </summary>
        /// <param name="nReqID" Desc="本组件维护的交易请求序列号"></param>
        /// <param name="StockCode" Desc="单个股票编码"></param>
        /// <param name="vRecord" Desc="行情数据包"></param>
        public void StockQuoteEvent(uint nReqID, string StockCode, object vRecord)
        {
            this.tradeUtil.CurOpt = CurOpt.StockQuoteEvent;

            ITradeRecord TradeRecord = (ITradeRecord)vRecord;
            if (null != TradeRecord)
            {
                uint nFieldCount = TradeRecord.FieldCount;
                uint nRecordCount = TradeRecord.RecordCount;

                /// 弹出JSON格式数据包
                MessageBox.Show(TradeRecord.GetJsonString());

                for (uint i = 0; i < nRecordCount; i++)
                {
                    /// 遍历数据集合
                    for (uint j = 0; j < nFieldCount; j++)
                    {
                        /// 获取指定行和列的数据
                        var temVal = TradeRecord.GetValue(i, j);
                        var temType = TradeRecord.GetDataType(j);
                    }
                }
            }

        }

        /// <summary>
        ///  服务器产生错误通知
        /// </summary>
        /// <param name="nTradeID" Desc="交易ID标识"></param>
        /// <param name="nReqID" Desc="请求ID标识"></param>
        public void ServerErrEvent(ushort nTradeID, uint nReqID)
        {
            this.tradeUtil.CurOpt = CurOpt.ServerErrEvent;
            this.tradeUtil.RetMsg = "服务器错误：" + m_spiTrade.LastErrDesc;
            this.tradeUtil.DoCallBack(null);
        }

        /// <summary>
        ///  服务器切换通知，只有高级版多服务器主机配置才有本事件，暂未实现
        /// </summary>
        /// <param name="nPreTradeID" Desc="上一个交易ID标识"></param>
        /// <param name="nCurTradeID" Desc="当前交易ID标识"></param>
        public void ServerChangedEvent(ushort nPreTradeID, ushort nCurTradeID)
        {
        }

        public void SetIndex(ushort nIndex)
        {
            m_nTradeIndex = nIndex;
        }

        public void ReleaseTrade()
        {
            m_spiTrade = null;
        }
    }
}
