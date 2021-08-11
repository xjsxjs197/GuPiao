using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GuPiao;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 实时信息
    /// </summary>
    public partial class ReadTimeInfo : Form
    {
        #region 全局变量

        List<string> noList = new List<string>();
        List<string> nameList = new List<string>();
        System.Timers.Timer timersTimer = null;
        string selStockCd = string.Empty;

        string toolsTitle = "实时信息";

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
        public ReadTimeInfo()
        {
            InitializeComponent();

            this.Text = this.toolsTitle;

            //获取UI线程同步上下文
            this.mSyncContext = SynchronizationContext.Current;

            this.FormClosing += new FormClosingEventHandler(this.GuPiaoTool_FormClosing);
        }

        #endregion

        #region 页面事件

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
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuPiaoTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.timersTimer != null)
            {
                this.timersTimer.Stop();
            }

            // 记录Log信息
            if (this.logInfo.Count > 0)
            {
                File.WriteAllLines(@"./Log/" + DateTime.Now.ToString("yyyyMMdd") + "PointTrend.txt", this.logInfo.ToArray(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// 画面显示时的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuPiaoTool_Shown(object sender, EventArgs e)
        {
            this.StartRun();
        }

        #endregion

        #region 私有方法


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

                // 初始化定时器
                this.InitTimer();
            }
            catch (Exception ex)
            {
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
                string result = Util.HttpGet(url, data, Encoding.UTF8);
                if (!string.IsNullOrEmpty(result) && result.Length > 20)
                {
                    this.GetGuPiaoInfo(noList, nameList, result);

                    // 在线程中更新UI（通过UI线程同步上下文mSyncContext）
                    mSyncContext.Post(this.DisplayData, null);
                }
            }
            catch (Exception e)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppendErrorLog), e);
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
            }
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
