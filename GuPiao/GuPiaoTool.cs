using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using GuPiao;
using System.Threading;

namespace GuPiaoTool
{
    /// <summary>
    /// 财富雪球
    /// 自动化交易平台，带来财富，想雪球一样越贵越大
    /// </summary>
    public partial class GuPiaoTool : Form
    {
        #region 全局变量

        List<string> noList = new List<string>();
        List<string> nameList = new List<string>();
        List<BuySellPoint> buySellPoints = new List<BuySellPoint>();
        BuySellPoint defaultBuySellPoint = new BuySellPoint();
        System.Timers.Timer timersTimer = null;
        bool isRuning = false;
        TradeUtil tradeUtil = new TradeUtil();
        string selStockCd = string.Empty;

        string toolsTitle = "财富雪球V4.0";

        /// <summary>
        /// 每N秒检查一次
        /// </summary>
        const int VAL_CHK_RANGE = 5;

        /// <summary>
        /// 最大记录的数据块数（10分钟=600秒=5秒*120）
        /// </summary>
        const int MAX_BLOCK = 120;

        /// <summary>
        /// 判断趋势用的最小值
        /// </summary>
        const float CHK_MIN_VAL = 0.1f;

        /// <summary>
        /// 记录Log信息
        /// </summary>
        List<string> logInfo = new List<string>();

        /// <summary>
        /// 记录错误Log信息
        /// </summary>
        List<string> logError = new List<string>();

        /// <summary>
        /// 从Sina取得的数据
        /// </summary>
        List<GuPiaoInfo> guPiaoInfo = new List<GuPiaoInfo>();
        
        /// <summary>
        /// 股票的基本信息
        /// </summary>
        Dictionary<string, GuPiaoInfo> guPiaoBaseInfo = new Dictionary<string, GuPiaoInfo>();

        /// <summary>
        /// 当天的交易信息
        /// </summary>
        List<OrderInfo> todayGuPiao = new List<OrderInfo>();

        /// <summary>
        /// UI线程的同步上下文
        /// </summary>
        SynchronizationContext mSyncContext = null;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public GuPiaoTool()
        {
            InitializeComponent();

            this.Text = this.toolsTitle;

            //获取UI线程同步上下文
            this.mSyncContext = SynchronizationContext.Current;

            this.rdoSync.CheckedChanged += new EventHandler(this.rdoSync_CheckedChanged);
            this.FormClosing += new FormClosingEventHandler(this.GuPiaoTool_FormClosing);
            this.grdGuPiao.SelectionChanged += new EventHandler(this.grdGuPiao_SelectionChanged);
            this.grdGuPiao.CellContentClick += new DataGridViewCellEventHandler(this.grdGuPiao_CellContentClick);
            this.grdHis.CellContentClick += new DataGridViewCellEventHandler(this.grdHis_CellContentClick);

            // 修改系统时间
            this.ChangeSystemTime();

            // 设置信息
            this.tradeUtil.SetCallBack(this.ThreadAsyncCallBack);
            this.tradeUtil.SetGuPiaoInfo(this.guPiaoBaseInfo);

            // 初始化基本信息
            this.InitBaseInfo();

            this.cmbCountBuy.SelectedIndex = 2;
            this.cmbCountSell.SelectedIndex = 3;
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
            if (!this.isRuning)
            {
                // 链接服务器
                this.tradeUtil.ConnServer(this.cmbAccountType.SelectedIndex, this.cmbBrokerType.SelectedIndex);
                if (!this.tradeUtil.IsSuccess)
                {
                    this.DispMsg(this.tradeUtil.RetMsg);
                }

                this.isRuning = true;

                // 开始运行
                this.StartRun();
            }
            else
            {
                // 刷新状态
                this.StartRefresh();
            }
        }

        /// <summary>
        /// 定时事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timersTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 刷新页面
            RefreshPage();
        }

        /// <summary>
        /// 同步异步切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoSync_CheckedChanged(object sender, EventArgs e)
        {
            // 设置同步、异步
        }

        /// <summary>
        /// 买
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBuy_Click(object sender, EventArgs e)
        {
            this.BuyStock(BuySellType.Buy);
        }

        /// <summary>
        /// 闪买
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuictBuy_Click(object sender, EventArgs e)
        {
            this.BuyStock(BuySellType.QuickBuy);
        }

        /// <summary>
        /// 卖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSell_Click(object sender, EventArgs e)
        {
            this.SellStock(BuySellType.Sell);
        }

        /// <summary>
        /// 闪卖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuickSell_Click(object sender, EventArgs e)
        {
            this.SellStock(BuySellType.QuickSell);
        }

        /// <summary>
        /// 超级快买
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sqBuy_Click(object sender, EventArgs e)
        {
            this.BuyStock(BuySellType.SuperQuickBuy);
        }

        /// <summary>
        /// 超级快卖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sqSell_Click(object sender, EventArgs e)
        {
            this.SellStock(BuySellType.SuperQuickSell);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuPiaoTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.tradeUtil.TradeRelease();

            // 记录Log信息
            if (this.logInfo.Count > 0)
            {
                File.WriteAllLines(@"./logs/" + DateTime.Now.ToString("yyyyMMdd") + "PointTrend.txt", this.logInfo.ToArray(), Encoding.UTF8);
            }

            //// 测试用，保存所有数据
            //StringBuilder sb = new StringBuilder();
            //foreach (GuPiaoInfo item in this.guPiaoInfo)
            //{
            //    sb.Append(item.fundcode).Append("\r\n");
            //    item.allPointsVal[0].ForEach(p => sb.Append(p.ToString("00.00")).Append(" "));
            //    sb.Append("\r\n");
            //    item.allPointsVal[1].ForEach(p => sb.Append(p.ToString("00.00")).Append(" "));
            //    sb.Append("\r\n");
            //    item.allPointsVal[2].ForEach(p => sb.Append(p.ToString("00.00")).Append(" "));
            //    sb.Append("\r\n");
            //}
            //File.WriteAllText(@"./logs/" + DateTime.Now.ToString("yyyyMMdd") + "PointVal.txt", sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 切换选中股票
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdGuPiao_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // 选中某一条信息
                this.SelectGuPiao();

                // 设置最新价格
                this.SetInputPrice();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), ex);
            }
        }

        /// <summary>
        /// 取消操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdHis_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 5 || string.IsNullOrEmpty(this.grdHis[e.ColumnIndex, e.RowIndex].Value as string))
            {
                return;
            }

            DataGridViewCellCollection lineCollection = this.grdHis.Rows[e.RowIndex].Cells;
            this.tradeUtil.CancelOrder(lineCollection[0].Value as string, lineCollection[6].Value as string);
        }

        /// <summary>
        /// 设置自动买卖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdGuPiao_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 1 || (e.ColumnIndex != 7 && e.ColumnIndex != 8))
            {
                return;
            }

            object oldVal = this.grdGuPiao.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (oldVal == null || (bool)oldVal == false)
            {
                oldVal = true;
            }
            else
            {
                oldVal = false;
            }

            this.grdGuPiao.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldVal;

            // 更新自动买卖标志
            if (e.ColumnIndex == 7)
            {
                this.guPiaoInfo[e.RowIndex].isAutoBuy = (bool)oldVal;
            }
            else
            {
                this.guPiaoInfo[e.RowIndex].isAutoSell = (bool)oldVal;
            }
        }

        /*
        /// <summary>
        /// 特殊功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdGuPiao_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 设置开板买入
            this.SetKaibanMairu(e);

            // 设置最新价格
            this.SetInputPrice();
        }*/

        #endregion

        #region 私有方法

        /// <summary>
        /// 修改系统时间
        /// </summary>
        private void ChangeSystemTime()
        {
            // 取得当前系统时间
            DateTime t = DateTime.Now;
            // 修改为2018/4/23
            DateTime newTm = new DateTime(2018, 4, 23, t.Hour, t.Minute, t.Second, t.Millisecond);

            // 转换System.DateTime到SYSTEMTIME
            SYSTEMTIME st = new SYSTEMTIME();
            st.FromDateTime(newTm);
            // 调用Win32 API设置系统时间
            Win32API.SetLocalTime(ref st);
        }

        /// <summary>
        /// 买的共通
        /// </summary>
        /// <param name="buySellType"></param>
        private void BuyStock(BuySellType buySellType)
        {
            if (!this.CheckInput(this.cmbCountBuy.Text, this.txtPriceBuy.Text))
            {
                return;
            }

            string retMsg = this.tradeUtil.BuyStock(this.selStockCd, this.GetCount(this.cmbCountBuy.Text), this.GetPrice(this.txtPriceBuy.Text), buySellType);
            this.DispMsg(retMsg);
        }

        /// <summary>
        /// 卖的共通
        /// </summary>
        /// <param name="buySellType"></param>
        private void SellStock(BuySellType buySellType)
        {
            if (!this.CheckInput(this.cmbCountSell.Text, this.txtPriceSell.Text))
            {
                return;
            }

            string retMsg = this.tradeUtil.SellStock(this.selStockCd, this.GetCount(this.cmbCountSell.Text), this.GetPrice(this.txtPriceSell.Text), buySellType);
            this.DispMsg(retMsg);
        }

        /// <summary>
        /// 设置开板买入
        /// </summary>
        private void SetKaibanMairu(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 1 || e.ColumnIndex != 6)
            {
                return;
            }

            object oldVal = this.grdGuPiao.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (oldVal == null || (bool)oldVal == false)
            {
                oldVal = true;
            }
            else
            {
                oldVal = false;
            }

            this.grdGuPiao.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldVal;
        }

        /// <summary>
        /// 设置卖相关按钮状态
        /// </summary>
        /// <param name="enabled"></param>
        private void SetSellAreaEnabled(bool enabled)
        {
            this.cmbCountSell.Enabled = enabled;
            this.btnSell.Enabled = enabled;
            this.btnQuickSell.Enabled = enabled;
            this.btnSqSell.Enabled = enabled;
        }

        /// <summary>
        /// 设置买相关按钮状态
        /// </summary>
        /// <param name="enabled"></param>
        private void SetBuyAreaEnabled(bool enabled)
        {
            this.cmbCountBuy.Enabled = enabled;
            this.btnBuy.Enabled = enabled;
            this.btnQuictBuy.Enabled = enabled;
            this.btnSqBuy.Enabled = enabled;
        }

        /// <summary>
        /// 设置卖的按钮状态
        /// </summary>
        /// <param name="canUserCount"></param>
        private void SetSellInfo(int canUserCount)
        {
            int maxCount = Convert.ToInt32(canUserCount);
            if (maxCount < 100)
            {
                this.cmbCountSell.SelectedIndex = -1;
                this.SetSellAreaEnabled(false);
            }
            else
            {
                this.SetSellAreaEnabled(true);

                // 设置最大可以卖多少
                int lastCount = 0;
                if (this.cmbCountSell.Items.Count > 0)
                {
                    lastCount = Convert.ToInt32(this.cmbCountSell.Items[this.cmbCountSell.Items.Count - 1]);
                }
                if (lastCount == canUserCount)
                {
                    // 如果设置过了，不用每次设置
                    if (this.cmbCountSell.SelectedIndex == -1)
                    {
                        this.cmbCountSell.SelectedIndex = 0;
                    }
                    return;
                }
                int maxNum = (int)(maxCount / 100);
                this.cmbCountSell.Items.Clear();
                for (int i = 1; i <= maxNum; i++)
                {
                    this.cmbCountSell.Items.Add(i * 100);
                }

                this.cmbCountSell.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 设置买的按钮状态
        /// </summary>
        /// <param name="canUseMoney"></param>
        /// <param name="curPrice"></param>
        private void SetBuyInfo(string canUseMoney, string curPrice)
        {
            float nowMoney = (float)Convert.ToDouble(canUseMoney);
            float nowPrice = (float)Convert.ToDouble(curPrice) * 100;
            if (nowMoney < nowPrice + 5)
            {
                this.cmbCountBuy.SelectedIndex = -1;
                this.SetBuyAreaEnabled(false);
            }
            else
            {
                this.SetBuyAreaEnabled(true);

                // 设置最大可以买多少
                int maxNum = 0;
                if (nowPrice > 0)
                {
                    maxNum = (int)((nowMoney - 5) / nowPrice);
                }
                
                if (maxNum == this.cmbCountBuy.Items.Count)
                {
                    // 如果设置过了，不用每次设置
                    if (this.cmbCountSell.SelectedIndex == -1)
                    {
                        this.cmbCountSell.SelectedIndex = 0;
                    }
                    return;
                }

                this.cmbCountBuy.Items.Clear();
                for (int i = 1; i <= maxNum; i++)
                {
                    this.cmbCountBuy.Items.Add(i * 100);
                }

                if (maxNum > 0)
                {
                    this.cmbCountBuy.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        private void StartRun()
        {
            try
            {
                // 读取基础数据
                if (!this.LoadBaseData())
                {
                    return;
                }

                // 刷新页面
                RefreshPage();

                // 默认选中第二条记录（第一条是大盘）
                if (this.grdGuPiao.Rows.Count > 2)
                {
                    this.grdGuPiao.Rows[0].Selected = false;
                    this.grdGuPiao.Rows[1].Selected = true;
                }

                // 按钮控制
                this.btnRun.Text = "刷  新";
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.StackTrace);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), ex);
            }
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitTimer()
        {
            timersTimer = new System.Timers.Timer();
            timersTimer.Enabled = true;
            timersTimer.Interval = 1000; // 每1秒更新数据
            timersTimer.AutoReset = true;
            timersTimer.Elapsed += new System.Timers.ElapsedEventHandler(timersTimer_Elapsed);
            timersTimer.SynchronizingObject = this;
        }

        /// <summary>
        /// 检查是否可以自动卖
        /// </summary>
        private void CheckAutoSell(object state)
        {
            try
            {
                foreach (GuPiaoInfo guPiaoItem in this.guPiaoInfo)
                {
                    if (this.CanAutoSell(guPiaoItem))
                    {
                        // 可以自动卖
                        string retMsg = this.tradeUtil.SellStock(guPiaoItem.fundcode.Substring(2, 6), guPiaoItem.CanUseCount, 999.0f, BuySellType.QuickSell);

                        // 在线程中更新UI（通过UI线程同步上下文mSyncContext）
                        mSyncContext.Post(this.ThreadDispMsg, retMsg);
                    }
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message + "\n" + e.StackTrace);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), e);
            }
        }

        /// <summary>
        /// 检查是否可以自动买
        /// </summary>
        private void CheckAutoBuy(object state)
        {
            Dictionary<string, string> buyInfo = new Dictionary<string, string>();
            try
            {
                foreach (GuPiaoInfo guPiaoItem in this.guPiaoInfo)
                {
                    string trendTxt = string.Empty;
                    string stockInfo = guPiaoItem.fundcode + "(" + guPiaoItem.name + ")";
                    
                    // 判断当前走势
                    this.CheckValTrend(guPiaoItem);

                    buyInfo.Add(stockInfo, guPiaoItem.curTrend.ToString());
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message + "\n" + e.StackTrace);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), e);
            }

            // 在线程中更新UI（通过UI线程同步上下文mSyncContext）
            mSyncContext.Post(this.DispTrendInfo, buyInfo);
        }

        /// <summary>
        /// 设置页面的趋势
        /// </summary>
        /// <param name="state"></param>
        private void DispTrendInfo(object state)
        {
            try
            {
                Dictionary<string, string> buyInfo = (Dictionary<string, string>)state;
                for (int i = 1; i < this.grdGuPiao.Rows.Count; i++)
                {
                    DataGridViewCellCollection lineCollection = this.grdGuPiao.Rows[i].Cells;
                    string stockCd = lineCollection[0].Value.ToString();
                    if (buyInfo.ContainsKey(stockCd))
                    {
                        lineCollection["buyFlg"].Value = buyInfo[stockCd];
                    }
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message + "\n" + e.StackTrace);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), e);
            }
        }

        /// <summary>
        /// 检查是否可以自动卖
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool CanAutoSell(GuPiaoInfo item)
        {
            if (item.CanUseCount == 0)
            {
                return false;
            }

            float yingkuiPer = this.SetYinkuiPer(item, null);

            //// 使用趋势判断
            //// 判断高点卖
            //if (yingkuiPer > item.topSellPoint || yingkuiPer < item.bottomSellPoint)
            //{
            //    // 大于高位卖点或低于低位卖点时
            //    // 并且趋势向下，卖出
            //    if (this.CheckValTrend(item) <= -5)
            //    {
            //        // 5秒趋势向下
            //        return true;
            //    }
            //}
            //else
            //{
            //    // 正常情况下，如果10秒趋势向下，卖出
            //    if (this.CheckValTrend(item) <= -10)
            //    {
            //        // 10秒趋势向下
            //        return true;
            //    }
            //}

            // 使用设定的卖点判断
            // 判断高点卖
            if (yingkuiPer > 0 && this.CanAutoTopSell(item, yingkuiPer))
            {
                return true;
            }

            // 判断低点卖
            if (yingkuiPer < 0 && this.CanAutoBottomSell(item, yingkuiPer))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否可以自动高点卖
        /// </summary>
        /// <param name="item"></param>
        /// <param name="yingkuiPer"></param>
        /// <returns></returns>
        private bool CanAutoTopSell(GuPiaoInfo item, float yingkuiPer)
        {
            // 判断高点卖
            if (item.isWaitingSell)
            {
                // 在犹豫时间内判断
                if (yingkuiPer > item.topSellPoint)
                {
                    // 如果有升高的趋势，重新设置高点，重新开始犹豫等待
                    item.topSellPoint = yingkuiPer;
                    item.curSellWaitTime = item.sellWaitTime;
                }
                else
                {
                    // 如果开始下降
                    if (yingkuiPer < (item.topSellPoint - item.waitPoint))
                    {
                        // 低于了最高点 - 犹豫点，开始自动卖
                        item.isWaitingSell = false;
                        return true;
                    }
                }
            }
            else if (yingkuiPer > item.topSellPoint)
            {
                // 到达设置的卖点，开始犹豫等待
                item.isWaitingSell = true;
                item.curSellWaitTime = item.sellWaitTime;
            }

            return false;
        }

        /// <summary>
        /// 检查是否可以自动低点卖
        /// </summary>
        /// <param name="item"></param>
        /// <param name="yingkuiPer"></param>
        /// <returns></returns>
        private bool CanAutoBottomSell(GuPiaoInfo item, float yingkuiPer)
        {
            //// 判断低点卖
            //if (item.isWaitingSell)
            //{
            //    // 如果开始下降
            //    if (yingkuiPer < (item.bottomSellPoint - item.waitPoint))
            //    {
            //        // 低于了最低点 - 犹豫点，开始自动卖
            //        item.isWaitingSell = false;
            //        return true;
            //    }
            //    else if (yingkuiPer > item.bottomSellPoint)
            //    {
            //        // 已经升高到卖点以上了，取消自动卖的等待
            //        item.isWaitingSell = false;
            //        item.curSellWaitTime = item.sellWaitTime;
            //    }
            //}
            //else if (yingkuiPer < item.bottomSellPoint)
            //{
            //    // 到达设置的卖点，开始犹豫等待
            //    item.isWaitingSell = true;
            //    item.curSellWaitTime = item.sellWaitTime;
            //}

            if (yingkuiPer < item.bottomSellPoint)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断当前走势
        /// </summary>
        /// <param name="state"></param>
        private void CheckValTrend(object state)
        { 
            GuPiaoInfo item = (GuPiaoInfo)state;
            List<float> valsList = item.valsList;
            if (valsList.Count < 2)
            {
                return;
            }

            lock (valsList)
            {
                int index = valsList.Count - 1;
                if (valsList[index] > CHK_MIN_VAL && valsList[index - 1] > CHK_MIN_VAL)
                {
                    // 当前是上升趋势
                    item.curTrend.Append("B");

                    // 判断以前的趋势，如果是下降或水平，则说明到底部了，上升趋势可能会很强烈
                    index -= 2;
                    while (index >= 1)
                    {
                        if (valsList[index] <= CHK_MIN_VAL && valsList[index - 1] <= CHK_MIN_VAL)
                        {
                            if (item.curTrend.Length < 3)
                            {
                                item.curTrend.Append("B");
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                        index -= 2;
                    }
                }
                else if (valsList[index] <= -CHK_MIN_VAL && valsList[index - 1] <= -CHK_MIN_VAL)
                {
                    // 当前是下降趋势
                    item.curTrend.Append("S");

                    // 判断以前的趋势，如果是上升或水平，则说明到顶部了，下降趋势可能会很强烈
                    index -= 2;
                    while (index >= 1)
                    {
                        if (valsList[index] > -CHK_MIN_VAL && valsList[index - 1] > -CHK_MIN_VAL)
                        {
                            if (item.curTrend.Length < 3)
                            {
                                item.curTrend.Append("S");
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                        index -= 2;
                    }
                }
                else
                { 
                    // 清空当前趋势
                    item.curTrend.Length = 0;
                }
            }
        }

        /// <summary>
        /// 刷新状态
        /// </summary>
        private void StartRefresh()
        {
            if (timersTimer != null)
            {
                timersTimer.Stop();

                // 读取基础数据
                if (!this.LoadBaseData())
                {
                    return;
                }

                timersTimer.Start();

                // 获取最新的金额信息
                object[] moneyInfo = this.tradeUtil.GetCurrentMoneyInfo();
                this.RefreshMoneyInfo(moneyInfo);
            }
        }

        /// <summary>
        /// 读取基础数据
        /// </summary>
        private bool LoadBaseData()
        {
            // 读取基础数据
            string[] baseInfo = File.ReadAllLines(@"./基础数据.txt", Encoding.UTF8);
            noList = new List<string>();
            nameList = new List<string>();
            for (int i = 1; i < baseInfo.Length; i++)
            {
                string[] fundInfos = baseInfo[i].Split(' ');
                noList.Add(fundInfos[0]);
                nameList.Add(fundInfos[2]);
            }

            if (noList.Count == 0)
            {
                MessageBox.Show("基础信息有误！");
                return false;
            }

            // 读取买卖点配置信息
            buySellPoints.Clear();
            baseInfo = File.ReadAllLines(@"./BuyCellPointInfo.txt", Encoding.UTF8);
            for (int i = 1; i < baseInfo.Length; i++)
            {
                string[] buyCellInfos = baseInfo[i].Split(' ');
                BuySellPoint buyCellItem = new BuySellPoint();
                buyCellItem.StockCd = buyCellInfos[0];
                buyCellItem.TopSellPoint = Convert.ToInt32(buyCellInfos[1]);
                buyCellItem.BottomSellPoint = Convert.ToInt32(buyCellInfos[2]);
                buyCellItem.SellWaitTime = Convert.ToInt32(buyCellInfos[3]);
                buyCellItem.WaitPoint = Convert.ToInt32(buyCellInfos[4]);
                buySellPoints.Add(buyCellItem);
            }

            if (buySellPoints.Count == 0)
            {
                MessageBox.Show("买卖点信息有误！");
                return false;
            }

            defaultBuySellPoint.TopSellPoint = buySellPoints[0].TopSellPoint;
            defaultBuySellPoint.BottomSellPoint = buySellPoints[0].BottomSellPoint;
            defaultBuySellPoint.SellWaitTime = buySellPoints[0].SellWaitTime;
            defaultBuySellPoint.WaitPoint = buySellPoints[0].WaitPoint;

            return true;
        }

        /// <summary>
        /// 刷新页面
        /// </summary>
        private void RefreshPage()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.MultThreadRefreshPage));
        }

        /// <summary>
        /// 多线程刷新页面
        /// </summary>
        private void MultThreadRefreshPage(object state)
        {
            try
            {
                // 从Sina取得基础数据
                string url = "http://hq.sinajs.cn/list=" + string.Join(",", noList.ToArray());
                string data = "";
                string result = HttpGet(url, data);
                if (!string.IsNullOrEmpty(result) && result.Length > 20)
                {
                    this.GetGuPiaoInfo(noList, nameList, result);

                    // 在线程中更新UI（通过UI线程同步上下文mSyncContext）
                    mSyncContext.Post(this.DisplayData, null);

                    // 检查是否可以自动卖
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(this.CheckAutoSell));

                    // 检查是否可以自动买
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.CheckAutoBuy));
                }
            }
            catch (Exception e)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), e);
            }
        }

        /// <summary>
        /// 选中某一条信息
        /// </summary>
        private void SelectGuPiao()
        {
            if (this.grdGuPiao.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = this.grdGuPiao.SelectedRows[0];
                // 设置选中的代码
                string cell0 = selectedRow.Cells[0].Value as string;
                if (!string.IsNullOrEmpty(cell0))
                {
                    this.selStockCd = cell0.Substring(2, 6);
                }

                // 取得最新价格
                string curPrice = selectedRow.Cells[2].Value as string;

                // 根据可用余额，当前价格，设置买的按钮状态
                this.SetBuyInfo(this.lblCanUseMoney.Text, curPrice);

                // 根据可用股数，设置卖的按钮的状态
                this.SetSellInfo(Convert.ToInt32(selectedRow.Cells[5].Value));
            }
        }

        /// <summary>
        /// 设置最新价格
        /// </summary>
        private void SetInputPrice()
        {
            if (this.grdGuPiao.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = this.grdGuPiao.SelectedRows[0];
                // 设置最新价格
                string curPrice = selectedRow.Cells[2].Value as string;
                this.txtPriceBuy.Text = curPrice;
                this.txtPriceSell.Text = curPrice;
            }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        private void DisplayData(object param)
        {
            int newRow;
            float yingkuiPer;
            for (int i = 0; i < guPiaoInfo.Count; i++)
            {
                newRow = i;
                if (i == this.grdGuPiao.Rows.Count)
                {
                    newRow = this.grdGuPiao.Rows.Add();
                }

                DataGridViewCellCollection lineCollection = this.grdGuPiao.Rows[newRow].Cells;
                lineCollection[0].Value = guPiaoInfo[i].fundcode + "(" + guPiaoInfo[i].name + ")";
                lineCollection[1].Value = guPiaoInfo[i].zuoriShoupanVal;
                lineCollection[2].Value = guPiaoInfo[i].currentVal;

                yingkuiPer = this.SetYinkuiPer(guPiaoInfo[i], lineCollection[3]);

                // 设置股数信息
                string stockCd = guPiaoInfo[i].fundcode.Substring(2, 6);
                if (this.guPiaoBaseInfo.ContainsKey(stockCd))
                {
                    GuPiaoInfo item = this.guPiaoBaseInfo[stockCd];
                    lineCollection[4].Value = item.TotalCount;
                    lineCollection[5].Value = item.CanUseCount;
                }
                else
                {
                    lineCollection[4].Value = (uint)0;
                    lineCollection[5].Value = (uint)0;
                }

                // 更新股数信息
                guPiaoInfo[i].TotalCount = (uint)lineCollection[4].Value;
                guPiaoInfo[i].CanUseCount = (uint)lineCollection[5].Value;

                /*
                // 判断开板买入
                if (lineCollection[6].Value != null && (bool)(lineCollection[6].Value))
                {
                    if (this.KaibanMairu(stockCd, guPiaoInfo[i].currentVal, yingkuiPer))
                    {
                        // 如果买了，不管成功失败，下次取消买入，如果想再买，再点击即可
                        lineCollection[6].Value = false;
                    }
                }*/
            }

            // 更新当前选中信息
            this.SelectGuPiao();
        }

        /// <summary>
        /// 开板买入
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="curPrice"></param>
        private bool KaibanMairu(string stockCd, string curPrice, float yingkuiPer)
        {
            float nowMoney = float.Parse(this.lblCanUseMoney.Text);
            float nowPrice = float.Parse(curPrice) * 100;
            if (yingkuiPer < 9.5 && nowMoney > (nowPrice + 5))
            {
                // 未涨停，并且有钱
                // 设置最大可以买多少
                int maxNum = 0;
                if (nowPrice > 0)
                {
                    maxNum = (int)((nowMoney - 5) / nowPrice);
                }

                if (maxNum > 0)
                {
                    // 买一半
                    int buyCount = maxNum / 2;
                    if (buyCount * 2 < maxNum)
                    {
                        buyCount++;
                    }

                    //MessageBox.Show(stockCd + " " + this.GetCount(buyCount * 100) + " " + (float)nowPrice);
                    this.tradeUtil.BuyStock(stockCd, this.GetCount(buyCount * 100), (float)nowPrice, BuySellType.SuperQuickSell);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 取得盈亏比
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private float SetYinkuiPer(GuPiaoInfo item, DataGridViewCell cellItem)
        {
            decimal currentVal = decimal.Parse(item.currentVal);
            decimal zuoriShoupanVal = decimal.Parse(item.zuoriShoupanVal);
            float yingkuiPer = (float)((currentVal - zuoriShoupanVal) / zuoriShoupanVal * 100);

            if (cellItem != null)
            {
                cellItem.Value = yingkuiPer.ToString("00.00");

                if (yingkuiPer > 0)
                {
                    cellItem.Style.ForeColor = Color.Red;
                }
                else if (yingkuiPer < 0)
                {
                    cellItem.Style.ForeColor = Color.Green;
                }
                else
                {
                    cellItem.Style.ForeColor = Color.Black;
                }
            }

            return yingkuiPer;
        }

        /// <summary>
        /// 取得股票信息
        /// </summary>
        /// <param name="noList"></param>
        /// <param name="nameList"></param>
        /// <param name="guPiaoJsInfo"></param>
        private void GetGuPiaoInfo(List<string> noList, List<string> nameList, string guPiaoJsInfo)
        {
            string[] guPiao = guPiaoJsInfo.Split(';');
            for (int i = 0; i < guPiao.Length; i++)
            {
                if (string.IsNullOrEmpty(guPiao[i]) || guPiao[i].Length < 10)
                {
                    break;
                }

                string[] lines = guPiao[i].Split('=');
                string[] details = lines[1].Split(',');

                GuPiaoInfo item = this.guPiaoInfo.FirstOrDefault(p => p.fundcode.Equals(noList[i]));
                if (item == null)
                {
                    item = new GuPiaoInfo();
                    item.fundcode = noList[i];
                    item.name = nameList[i];
                    item.jinriKaipanVal = details[1];
                    item.zuoriShoupanVal = details[2];

                    item.secCounter = 1;
                    item.valsList = new List<float>();
                    item.lastVal = float.Parse(details[3]);
                    item.curTrend = new StringBuilder();

                    // 取得自动的卖点
                    BuySellPoint pointInfo = this.buySellPoints.FirstOrDefault(p => p.StockCd.Equals(item.fundcode));
                    if (pointInfo == null)
                    {
                        item.topSellPoint = this.defaultBuySellPoint.TopSellPoint;
                        item.bottomSellPoint = -this.defaultBuySellPoint.BottomSellPoint;
                        item.sellWaitTime = this.defaultBuySellPoint.SellWaitTime;
                        item.waitPoint = this.defaultBuySellPoint.WaitPoint;
                    }
                    else
                    {
                        item.topSellPoint = pointInfo.TopSellPoint;
                        item.bottomSellPoint = -pointInfo.BottomSellPoint;
                        item.sellWaitTime = pointInfo.SellWaitTime;
                        item.waitPoint = pointInfo.WaitPoint;
                    }

                    this.guPiaoInfo.Add(item);
                }
                
                item.currentVal      = details[3];
                item.zuigaoVal       = details[4];
                item.zuidiVal        = details[5];
                item.jingmaiInVal    = details[6];
                item.jingmaiOutVal   = details[7];
                item.chengjiaoShu    = details[8];
                item.chengjiaoJine   = details[9];
                item.gushuIn1        = details[10];
                item.valIn1          = details[11];
                item.gushuIn2        = details[12];
                item.valIn2          = details[13];
                item.gushuIn3        = details[14];
                item.valIn3          = details[15];
                item.gushuIn4        = details[16];
                item.valIn4          = details[17];
                item.gushuIn5        = details[18];
                item.valIn5          = details[19];
                item.gushuOut1       = details[20];
                item.valOut1         = details[21];
                item.gushuOut2       = details[22];
                item.valOut2         = details[23];
                item.gushuOut3       = details[24];
                item.valOut3         = details[25];
                item.gushuOut4       = details[26];
                item.valOut4         = details[27];
                item.gushuOut5       = details[28];
                item.valOut5         = details[29];
                item.date            = details[30];
                item.time            = details[31];

                // 每个时间段检查一次数据
                if (item.secCounter < VAL_CHK_RANGE)
                {
                    item.secCounter++;
                }
                else
                {
                    item.secCounter = 1;
                    float curVal = float.Parse(item.currentVal);
                    item.valsList.Add(curVal - item.lastVal);
                    item.lastVal = curVal;

                    // 判断是否达到最大记录数
                    if (item.valsList.Count > MAX_BLOCK)
                    {
                        item.valsList.RemoveAt(0);
                    }
                }

                // 判断当前走势
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.CheckValTrend), item);
            }
        }

        /// <summary>
        /// Http发送Post请求方法
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        private static string HttpPost(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataStr.Length;
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            writer.Write(postDataStr);
            writer.Flush();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码  
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            return retString;
        }

        /// <summary>
        /// Http发送Get请求方法
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        private static string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary>
        /// 初始化基本信息
        /// </summary>
        private void InitBaseInfo()
        {
            this.cmbCountBuy.SelectedIndex = 0;

            tradeUtil.Init(this.cmbAccountType, this.cmbBrokerType);
        }

        /// <summary>
        /// 页面项目控制
        /// </summary>
        /// <param name="enable"></param>
        private void EnableItems(bool enable)
        {
            this.cmbCountBuy.Enabled = enable;
            this.cmbCountSell.Enabled = enable;
            this.txtPriceBuy.ReadOnly = !enable;
            this.txtPriceSell.ReadOnly = !enable;
            this.btnBuy.Enabled = enable;
            this.btnQuictBuy.Enabled = enable;
            this.btnSqBuy.Enabled = enable;
            this.btnSell.Enabled = enable;
            this.btnQuickSell.Enabled = enable;
            this.btnSqSell.Enabled = enable;
        }

        /// <summary>
        /// 检查输入信息
        /// </summary>
        /// <returns></returns>
        private bool CheckInput(object objCount, string txtPrice)
        {
            if (string.IsNullOrEmpty(this.selStockCd))
            {
                this.DispMsg("交易代码不对");
                return false;
            }

            uint count = this.GetCount(objCount);
            if (count == 0 || (count % 100) > 0)
            {
                this.DispMsg("交易数量不对，必须大于0并且是100的倍数");
                return false;
            }

            if (!(this.GetPrice(txtPrice) > 0))
            {
                this.DispMsg("交易金额不对");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 取得价格
        /// </summary>
        /// <returns></returns>
        private float GetPrice(string txtPrice)
        {
            float fPrice = 0.0f;
            string price = txtPrice;
            if (string.IsNullOrEmpty(price))
            {
                return 0.0f;
            }

            if (float.TryParse(price, out fPrice))
            {
                return fPrice;
            }

            return 0.0f;
        }

        /// <summary>
        /// 取得数量
        /// </summary>
        /// <returns></returns>
        private uint GetCount(object count)
        {
            if (count != null)
            {
                return Convert.ToUInt32(count);
            }

            return 0;
        }

        /// <summary>
        /// 显示各种操作信息(多线程内部调用)
        /// </summary>
        /// <param name="retMsg"></param>
        private void ThreadDispMsg(object retMsg)
        {
            this.DispMsg(retMsg.ToString());
        }

        /// <summary>
        /// 显示各种操作信息
        /// </summary>
        /// <param name="retMsg"></param>
        private void DispMsg(string retMsg)
        {
            this.Text = this.toolsTitle + " " + retMsg;
        }

        /// <summary>
        /// 显示当前委托信息
        /// </summary>
        private void DispTodayInfo()
        {
            this.grdHis.Rows.Clear();
            for (int i = 0; i < this.todayGuPiao.Count; i++)
            {
                int index = this.grdHis.Rows.Add();
                DataGridViewCellCollection lineCollection = this.grdHis.Rows[index].Cells;
                lineCollection[0].Value = this.todayGuPiao[i].StockCd;
                lineCollection[1].Value = this.todayGuPiao[i].OrderType == OrderType.Buy ? "Buy" : "Sell";
                lineCollection[2].Value = this.todayGuPiao[i].Count;
                lineCollection[3].Value = this.todayGuPiao[i].Price;
                lineCollection[4].Value = this.todayGuPiao[i].OrderStatus == OrderStatus.Waiting ? "Waiting" :
                    (this.todayGuPiao[i].OrderStatus == OrderStatus.OrderOk ? "Ok" : "Cancel");

                if (this.todayGuPiao[i].OrderStatus == OrderStatus.Waiting)
                {
                    //lineCollection[5].Visible = true;
                    lineCollection[5].Value = "Cancel";
                }
                else
                {
                    //lineCollection[5].Visible = false;
                    lineCollection[5].Value = "";
                }

                lineCollection[6].Value = this.todayGuPiao[i].OrderId;
                lineCollection[7].Value = this.todayGuPiao[i].OrderDate;
            }
        }

        /// <summary>
        /// 设置剩余金额信息
        /// </summary>
        /// <param name="param"></param>
        private void SetMoneyInfo(params object[] param)
        {
            //MessageBox.Show(param[0] + " : " + param[1] + " : " + param[2] + " : " + param[3]);
            if (param != null && param.Length >= 4)
            {
                this.lblTotal.Text = Convert.ToString(param[0]);
                this.lblGuPiaoMoney.Text = Convert.ToString(param[1]);
                this.lblCanUseMoney.Text = Convert.ToString(param[2]);
                this.lblCanGetMoney.Text = Convert.ToString(param[3]);
            }
        }

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
                    if (!this.tradeUtil.isLoginOk)
                    {
                        this.btnRun.Enabled = false;
                        this.EnableItems(false);
                    }
                    else
                    {
                        this.btnRun.Enabled = true;
                        this.EnableItems(true);

                        // 刷新金额、当天委托信息
                        this.RefreshMoneyInfo(param);

                        // 启动定时器
                        this.InitTimer();
                    }
                    break;

                case CurOpt.OrderOKEvent:
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.DelayOrderOKEvent), param);
                    break;

                case CurOpt.OrderSuccessEvent:
                case CurOpt.CancelOrder:
                    // 刷新金额、当天委托信息
                    this.RefreshMoneyInfo(param);
                    break;
            }
        }

        /// <summary>
        /// 延时执行OrderOKEvent后面的处理
        /// </summary>
        private void DelayOrderOKEvent(object state)
        {
            // 休眠时间
            Thread.Sleep(1000);

            // 刷新金额、当天委托信息
            // 在线程中更新UI（通过UI线程同步上下文mSyncContext）
            mSyncContext.Post(this.ThreadRefreshMoneyInfo, state);
        }

        /// <summary>
        /// 刷新金额、当天委托信息(多线程中调用)
        /// </summary>
        /// <param name="param"></param>
        private void ThreadRefreshMoneyInfo(object param)
        {
            this.RefreshMoneyInfo((object[])param);
        }

        /// <summary>
        /// 刷新金额、当天委托信息
        /// </summary>
        /// <param name="param"></param>
        private void RefreshMoneyInfo(object[] param)
        {
            try
            {
                // 从付费接口取得可用股数
                this.tradeUtil.GetGuPiaoInfo(this.guPiaoBaseInfo);

                // 取得当天委托信息
                this.tradeUtil.GetTodayPiaoInfo(this.todayGuPiao);
                // 显示当前委托信息
                this.DispTodayInfo();

                // 设置金额基本信息
                this.SetMoneyInfo(param);
            }
            catch (Exception e)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), e);
            }
        }

        /// <summary>
        /// 写入错误Log
        /// </summary>
        /// <param name="state"></param>
        private void AppendErrorLog(object state)
        {
            string errorLogFile = @".\logs\" + DateTime.Now.ToString("yyyyMMdd") + "_err.txt";
            List<string> errorInfo = new List<string>();
            if (File.Exists(errorLogFile))
            {
                errorInfo.AddRange(File.ReadAllLines(errorLogFile, Encoding.UTF8));
                if (errorInfo.Count > 0 && string.IsNullOrEmpty(errorInfo[errorInfo.Count - 1]))
                {
                    errorInfo.RemoveAt(errorInfo.Count - 1);
                }
            }

            Exception e = (Exception)state;
            errorInfo.Add((string)(DateTime.Now.ToString("HH:mm:ss") + " " + e.Message + "\n" + e.StackTrace));

            File.WriteAllLines(errorLogFile, errorInfo.ToArray(), Encoding.UTF8);

            // 在线程中更新UI（通过UI线程同步上下文mSyncContext）
            mSyncContext.Post(this.ThreadDispMsg, errorInfo[errorInfo.Count - 1]);
        }

        #endregion
    }
}
