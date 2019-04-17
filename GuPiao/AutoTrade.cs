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
        /// 计时器
        /// </summary>
        private System.Timers.Timer timer = null;

        /// <summary>
        /// UI线程的同步上下文
        /// </summary>
        private SynchronizationContext mSyncContext = null;

        /// <summary>
        /// 设定信息
        /// </summary>
        private BuySellSetting configInfo = new BuySellSetting();

        /// <summary>
        /// 系统时间
        /// </summary>
        private DateTime sysDate;

        /// <summary>
        /// 自动交易共通
        /// </summary>
        private AutoTradeBase autoTradeUtil = null;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public AutoTrade()
        {
            InitializeComponent();

            this.ResetHeight();

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
            if (this.rdoEmu.Checked)
            {
                // 历史模拟交易共通
                this.autoTradeUtil = new AutoTradeEmu();

                // 交易共通初始化
                this.Do(this.AutoTraceUtilInit
                    , new object[] { (GuPiao.AutoTradeBase.DoSomethingWithP)this.ThreadAsyncCallBack
                    , this.chkChuangYe.Checked
                    , this.chkLiangRong.Checked
                    , this.dtEmu.Value
                    , false }
                );
            }
            else
            {
                // 实时交易共通
                this.autoTradeUtil = new AutoTradeReal();

                // 交易共通初始化
                this.Do(this.AutoTraceUtilInit
                    , new object[] {  (GuPiao.AutoTradeBase.DoSomethingWithP)this.ThreadAsyncCallBack
                    , this.chkChuangYe.Checked
                    , this.chkLiangRong.Checked
                    , this.sysDate
                    , this.rdoRealEmu.Checked }
                );
            }

            this.btnRun.Enabled = false;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoTrade_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopAutoTrade();
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

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            this.timer = new System.Timers.Timer();
            this.timer.Enabled = true;
            this.timer.Interval = this.autoTradeUtil.GetTimerInterval();
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            this.timer.SynchronizingObject = this;
        }

        /// <summary>
        /// 交易共通初始化
        /// </summary>
        /// <param name="param"></param>
        private void AutoTraceUtilInit(params object[] param)
        {
            // 交易共通初始化
            this.autoTradeUtil.Init((GuPiao.AutoTradeBase.DoSomethingWithP)param[0], (bool)param[1],
                (bool)param[2], (DateTime)param[3], (bool)param[4]);
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
        /// 停止程序
        /// </summary>
        private void StopAutoTrade()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
            }

            if (this.autoTradeUtil != null)
            {
                this.autoTradeUtil.TradeRelease();
            }

            this.btnRun.Enabled = true;
            this.btnRun.Text = "重新开始";
        }

        #region 定时交易相关

        /// <summary>
        /// 定时处理的逻辑
        /// </summary>
        private void TimerProcess()
        {
            // 开线程，处理实时数据
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.ThreadCheckRealTimeData));
        }

        /// <summary>
        /// 处理实时数据
        /// </summary>
        /// <param name="data"></param>
        private void ThreadCheckRealTimeData(object data)
        {
            // 定时取得数据
            List<GuPiaoInfo> dataLst = this.autoTradeUtil.TimerGetData();
            if (dataLst == null || dataLst.Count == 0)
            {
                return;
            }

            foreach (GuPiaoInfo item in dataLst)
            {
                // 检查当前数据的买卖点信息
                int buySellFlg = this.autoTradeUtil.CheckDataBuySellFlg(item);
                if (buySellFlg != 0)
                {
                    // 开始自动买卖
                    this.autoTradeUtil.ThreadAutoTrade(item, buySellFlg);
                }
            }
        }

        #endregion

        #region 异步交易接口的事件处理

        /// <summary>
        /// 异步的回调方法（多线程内调用）
        /// </summary>
        /// <param name="param"></param>
        private void ThreadAsyncCallBack(TradeEventParam param)
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
            try
            {
                TradeEventParam param = (TradeEventParam)threadParam;

                // 第一步都是显示返回的信息
                this.DispMsg(param.Msg);

                switch (param.CurOpt)
                {
                    case CurOpt.InitEvent:
                        if (param.IsSuccess)
                        {
                            // 取得配置信息
                            this.configInfo = this.autoTradeUtil.GetConfigInfo();

                            // 初始化完成后，登陆服务器
                            this.autoTradeUtil.LoginServer();
                        }
                        break;

                    case CurOpt.LoginEvent:
                        if (param.IsSuccess)
                        {
                            this.btnRun.Text = "运行中...";

                            // 登陆服务器完成后，初始化定时器
                            this.InitTimer();
                        }
                        break;

                    case CurOpt.OrderOKEvent:
                        this.autoTradeUtil.TradeAfter("订单成功");
                        break;

                    case CurOpt.OrderSuccessEvent:
                        this.autoTradeUtil.TradeAfter("交易成功");
                        break;

                    case CurOpt.CancelOrder:
                        this.autoTradeUtil.TradeAfter("交易取消");
                        break;

                    case CurOpt.ResetProcessBar:
                        this.ResetProcessBar(param.HstDataCount);
                        break;

                    case CurOpt.ProcessBarStep:
                        this.ProcessBarStep();
                        break;

                    case CurOpt.CloseProcessBar:
                        this.CloseProcessBar();
                        break;

                    case CurOpt.EmuTradeEnd:
                        this.StopAutoTrade();
                        break;
                }
            }
            catch (Exception e)
            {
                this.autoTradeUtil.WriteComnLog(e.Message + "\r\n" + e.StackTrace);
            }
        }

        #endregion

        #endregion
    }
}
