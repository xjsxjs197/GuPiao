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
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 画面初始化
        /// </summary>
        private void Init()
        {
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
                    break;

                case CurOpt.OrderSuccessEvent:
                case CurOpt.CancelOrder:
                    break;
            }
        }

        #endregion

        #endregion
    }
}
