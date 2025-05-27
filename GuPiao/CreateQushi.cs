using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Common;
using DayBatch;
using DataProcess.FenXing;
using System.Globalization;
using System.Data.OracleClient;


namespace GuPiao
{
    /// <summary>
    /// 根据原始数据做成趋势图
    /// </summary>
    public partial class CreateQushi : BaseForm
    {
        #region " 全局变量 "

        /// <summary>
        /// 取最多多少天数据
        /// </summary>
        private const int MAX_DAYS = 110;

        /// <summary>
        /// 工具的名称
        /// </summary>
        private const string TITLE = "财富数据 ";

        /// <summary>
        /// 不要创业数据
        /// </summary>
        private const bool NO_CHUANGYE = true;

        /// <summary>
        /// 打开状态时的按钮名称
        /// </summary>
        private const string OPEN_BTN_TEXT = "收 起";

        /// <summary>
        /// 收起状态时的按钮名称
        /// </summary>
        private const string CLOSE_BTN_TEXT = "展 开";

        /// <summary>
        /// UI线程的同步上下文
        /// </summary>
        SynchronizationContext mSyncContext = null;

        /// <summary>
        /// 当前显示位置
        /// </summary>
        private int curIdx = 0;

        /// <summary>
        /// 所有数据的名称信息
        /// </summary>
        private List<string> allStock = new List<string>();

        /// <summary>
        /// 有买点的数据信息
        /// Key：Stock代码
        /// Value：最后的买点日期
        /// </summary>
        private List<KeyValuePair<string, string>> hasBuyPointsStock = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// 当前数据的时间
        /// </summary>
        private string dataDate = string.Empty;

        /// <summary>
        /// 当前显示的数据信息
        /// </summary>
        private List<BaseDataInfo> curStockData = new List<BaseDataInfo>();

        /// <summary>
        /// 当前显示的数据信息(5日数据)
        /// </summary>
        private List<BaseDataInfo> curStockJibie5Data = new List<BaseDataInfo>();

        /// <summary>
        /// 当前显示的数据信息(10日数据)
        /// </summary>
        private List<BaseDataInfo> curStockJibie10Data = new List<BaseDataInfo>();

        /// <summary>
        /// 当前数据的名称
        /// </summary>
        private string curStockName = string.Empty;

        /// <summary>
        /// 取数据的弹出菜单
        /// </summary>
        private ContextMenuStrip getDataSubMenu = new ContextMenuStrip();

        /// <summary>
        /// 趋势图的弹出菜单
        /// </summary>
        private ContextMenuStrip qushiSubMenu = new ContextMenuStrip();

        /// <summary>
        /// 保持隐藏项目的高度
        /// </summary>
        private int oldPnlBodyHeight;

        /// <summary>
        /// 子目录
        /// </summary>
        private string subFolder;

        /// <summary>
        /// 保存所有的代码、名称映射信息
        /// </summary>
        private Dictionary<string, string> allStockCdName = new Dictionary<string, string>();

        /// <summary>
        /// 所有数据时间（天，5分钟，15分钟，30分钟）
        /// </summary>
        private List<string> allDataDate = new List<string>();

        /// <summary>
        /// 是否触发事件
        /// </summary>
        private bool needRaiseEvent = true;

        /// <summary>
        /// 当前显示图片的位置信息
        /// （向右移动了多少，IMG_X_STEP的整倍数）
        /// </summary>
        private int posFromRight = 0;

        /// <summary>
        /// 记录鼠标点击时X的坐标
        /// </summary>
        private int oldImgX;

        /// <summary>
        /// 记录趋势图原始的宽度
        /// </summary>
        private int oldImgWidth;

        /// <summary>
        /// 当前数据最右边的位置
        /// </summary>
        private int dataEndIdx;

        /// <summary>
        /// 当前趋势图文件路径
        /// </summary>
        private string stockImg;

        /// <summary>
        /// 是否触发日期下拉框事件
        /// </summary>
        private bool raiseDateChgEvent;

        /// <summary>
        /// 日期和位置的映射
        /// </summary>
        private Dictionary<string, int> dateIdxMap = new Dictionary<string, int>();

        /// <summary>
        /// 记录虚线框信息
        /// </summary>
        private List<int> posInfo = new List<int>();

        /// <summary>
        /// 数据库连接
        /// </summary>
        private OracleConnection conn = null;

        private string connStr = string.Empty;

        private const int MaxThreads = 3; // 最大并发线程数
        private static readonly object LockObj = new object();
        private static int _completedTasks = 0;
        private static int _totalTasks = 0;
        private static int _activeThreads = 0;
        private bool hasError = false;

        /// <summary>
        /// 输出Log的文件
        /// </summary>
        private string logFile = System.AppDomain.CurrentDomain.BaseDirectory + @"Log/GetDataBatLog.txt";

        #endregion

        #region " 初始化 "

        /// <summary>
        /// 初始化
        /// </summary>
        public CreateQushi()
        {
            InitializeComponent();

            this.subFolder = TimeRange.M30.ToString() + "/";
            this.btnChgTime.Text = TimeRange.M30.ToString();
            this.cmbCon.SelectedIndex = 0;
            this.pnlBody.BackColor = Color.FromArgb(199, 237, 204);
            this.raiseDateChgEvent = false;

            // 重新设置页面高度
            this.ResetPageHeight();

            // 设置日期控件
            this.SetDateValue();

            this.oldPnlBodyHeight = this.pnlBody.Height;
            this.Height -= this.oldPnlBodyHeight;

            this.cmbCon.SelectedIndexChanged += new EventHandler(this.cmbCon_SelectedIndexChanged);

            // 绑定子菜单事件
            this.SetSubMenuEvent();

            //获取UI线程同步上下文
            this.mSyncContext = SynchronizationContext.Current;

            // 取得所有数据的基本信息（代码+名称）
            this.GetAllStockBaseInfo();

            // 读取包括买点信息的数据
            this.LoadHasBuyPointsInfo();

            // 显示所有信息
            this.DisplayAllStockPng(null);

            this.raiseDateChgEvent = true;
        }

        #endregion

        #region " 页面事件 "

        /// <summary>
        /// 开始画趋势图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreate_Click(object sender, EventArgs e)
        {
            this.qushiSubMenu.Show(Control.MousePosition);
        }

        /// <summary>
        /// 取得所有数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetAllStock_Click(object sender, EventArgs e)
        {
            this.getDataSubMenu.Show(Control.MousePosition);
        }

        /// <summary>
        /// 向前翻页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBef_Click(object sender, EventArgs e)
        {
            // 重新显示当前信息
            this.ReDisplayStockInfo(this.curIdx - 1);
        }

        /// <summary>
        /// 向后翻页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAft_Click(object sender, EventArgs e)
        {
            // 重新显示当前信息
            this.ReDisplayStockInfo(this.curIdx + 1);
        }

        /// <summary>
        /// 鼠标移动时画十字线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgBody_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.imgBody.Image == null)
            {
                return;
            }

            if (this.posInfo.Count > 2)
            {
                return;
            }

            // 实例化一个和窗口一样大的位图
            Bitmap imgBk = new Bitmap(this.imgBody.Image.Width, this.imgBody.Image.Height);
            // 创建位图的gdi对象
            Graphics g = Graphics.FromImage(imgBk);
            // 创建画笔
            Pen p = new Pen(Color.Blue, 1.0f);
            // 指定线条的样式为划线段
            p.DashStyle = DashStyle.Dash;

            if (this.posInfo.Count == 0)
            {
                // 显示当前位置的数据信息，并且取得当前点的坐标
                int posX = e.X - (e.X % Consts.IMG_X_STEP);
                int posY = this.DisplayCurDayInfo(posX);

                // 根据当前位置，画横线和竖线
                if (posY >= 0)
                {
                    g.DrawLine(p, 0, posY, imgBk.Width - 1, posY);
                    g.DrawLine(p, posX, 0, posX, imgBk.Height - 1);
                }
            }
            else if (this.posInfo.Count == 2)
            {
                int posX1 = this.posInfo[0];
                int posY1 = this.posInfo[1];

                int posX2 = e.X - (e.X % Consts.IMG_X_STEP);
                int xVal = (int)((this.posFromRight + (this.imgBody.Image.Width - posX2 - Consts.IMG_X_STEP)) / Consts.IMG_X_STEP);
                int posY2 = Util.GetPosYByX(this.curStockData, xVal, this.imgBody.Image.Height);
                this.DisplayDiffInfo(posX1, posX2);

                // 根据当前位置，画横线和竖线
                if (posY1 >= 0 && posY2 >= 0)
                {
                    g.DrawLine(p, posX1, posY1, posX2, posY1);
                    g.DrawLine(p, posX1, posY2, posX2, posY2);
                    g.DrawLine(p, posX1, posY1, posX1, posY2);
                    g.DrawLine(p, posX2, posY1, posX2, posY2);
                }
            }

            // 将位图贴到窗口上
            this.imgBody.BackgroundImage = imgBk;
            // 释放gid和pen资源
            g.Dispose();
            p.Dispose();
        }

        /// <summary>
        /// 鼠标离开时，取消十字线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgBody_MouseLeave(object sender, EventArgs e)
        {
            if (this.imgBody.Image == null)
            {
                return;
            }

            // 清除图像
            Bitmap imgBk = new Bitmap(this.imgBody.Image.Width, this.imgBody.Image.Height);
            Graphics g = Graphics.FromImage(imgBk);
            g.Clear(Color.Transparent);
            this.imgBody.BackgroundImage = imgBk;
            g.Dispose();

            // 显示当前位置的数据信息
            this.SetTitle(this.allStock[this.curIdx], 0);
        }

        /// <summary>
        /// 单击鼠标，选中范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgBody_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.imgBody.Image == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (this.posInfo.Count < 4)
                {
                    int posX = e.X - (e.X % Consts.IMG_X_STEP);
                    int xVal = (int)((this.posFromRight + (this.imgBody.Image.Width - posX - Consts.IMG_X_STEP)) / Consts.IMG_X_STEP);
                    int posY = Util.GetPosYByX(this.curStockData, xVal, this.imgBody.Image.Height);
                    this.posInfo.Add(posX);
                    this.posInfo.Add(posY);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.posInfo.Clear();
            }
        }

        /// <summary>
        /// 过滤条件变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbCon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.needRaiseEvent)
            {
                return;
            }

            this.cmbCon.Enabled = false;
            this.raiseDateChgEvent = false;

            // 重新设置当前显示
            this.ResetDisplay();

            QushiBase chkQushi = null;
            string selectedText = this.cmbCon.SelectedItem as string;
            switch (selectedText)
            {
                case "查看成功买点":
                    this.ViewBuySellPoints("GoodBuySellPoint.txt");
                    break;

                case "查看失败买点":
                    this.ViewBuySellPoints("BadBuySellPoint.txt");
                    break;

                case "下跌递减":
                    this.Do(this.ThreadChkQushi, new ChkDownDecreaseQushi(), this.cmbCon, selectedText);
                    break;

                case "最后是底分型":
                    this.Do(this.ThreadChkQushi, new ChkDownBreakQushi(), this.cmbCon, selectedText);
                    break;

                case "查看龙头股":
                    this.ViewLongtou();
                    break;

                case "下跌转折两天":
                    chkQushi = new ChkDownBreakDaysQushi();
                    chkQushi.SetCheckDays(2);
                    this.Do(this.ThreadChkQushi, chkQushi, this.cmbCon, selectedText);
                    break;

                case "第一类买点":
                    chkQushi = new ChkDownBreakDaysQushi();
                    chkQushi.SetCheckDays(3);
                    this.Do(this.ThreadChkQushi, chkQushi, this.cmbCon, selectedText);
                    break;

                case "最后是顶分型":
                    this.Do(this.ThreadChkQushi, new ChkUpBreakQushi(), this.cmbCon, selectedText);
                    break;

                case "一路下跌":
                    this.Do(this.ThreadChkQushi, new ChkDownQushi(), this.cmbCon, selectedText);
                    break;

                case "一路上涨":
                    this.Do(this.ThreadChkQushi, new ChkUpQushi(), this.cmbCon, selectedText);
                    break;

                case "查看买点":
                    this.DisplayHasBuyPointsStock();
                    this.cmbCon.Enabled = true;
                    break;

                case "查看次新":
                    this.Do(this.ThreadChkQushi, new ChkCixin(), this.cmbCon, selectedText);
                    break;

                case "所有天数据":
                    this.subFolder = Consts.DAY_FOLDER;
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                case "所有5分钟数据":
                    this.subFolder = TimeRange.M5.ToString() + "/";
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                case "所有15分钟数据":
                    this.subFolder = TimeRange.M15.ToString() + "/";
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                case "所有30分钟数据":
                    this.subFolder = TimeRange.M30.ToString() + "/";
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                case "连续涨停":
                    this.Do(this.ThreadChkQushi, new ChkContinueTopQushi(), this.cmbCon, selectedText);
                    break;

                case "连续跌停":
                    this.Do(this.ThreadChkQushi, new ChkContinueBottomQushi(), this.cmbCon, selectedText);
                    break;

                default:
                    // 显示所有信息
                    this.subFolder = Consts.DAY_FOLDER;
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;
            }
        }

        /// <summary>
        /// 只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCdSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                if (string.IsNullOrEmpty(this.txtCdSearch.Text))
                {
                    e.Handled = true;
                    return;
                }

                string searchTxt = this.txtCdSearch.Text.Trim();
                int tmpCd;
                if (int.TryParse(searchTxt, out tmpCd))
                {
                    searchTxt.PadLeft(6, '0');
                    if (this.allStock.Contains(searchTxt))
                    {
                        int idx = this.allStock.IndexOf(searchTxt);
                        if (idx >= 0)
                        {
                            // 重新显示当前信息
                            this.ReDisplayStockInfo(idx);
                        }
                    }
                }
                else
                {
                    this.allStock.Clear();

                    foreach (KeyValuePair<string, string> cdName in this.allStockCdName)
                    {
                        if (cdName.Value.IndexOf(searchTxt) >= 0)
                        {
                            this.allStock.Add(cdName.Key);
                        }
                    }

                    // 重新显示当前信息
                    this.ReDisplayStockInfo(0);

                    this.cmbCon.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 展开、收起显示的区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChgDisp_Click(object sender, EventArgs e)
        {
            if (OPEN_BTN_TEXT.Equals(this.btnChgDisp.Text))
            {
                this.Height -= this.oldPnlBodyHeight;
                this.btnChgDisp.Text = CLOSE_BTN_TEXT;
            }
            else
            {
                this.Height += this.oldPnlBodyHeight;
                this.btnChgDisp.Text = OPEN_BTN_TEXT;
            }
        }

        /// <summary>
        /// 子菜单点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getDataSubMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.getDataSubMenu.Visible = false;
            this.btnGetAllStock.Enabled = false;

            // 重新设置当前显示
            this.ResetDisplay();

            switch (e.ClickedItem.Name)
            {
                case "get5M":
                    this.Do(this.ThreadGetData, TimeRange.M5);
                    break;

                case "get15M":
                    this.Do(this.ThreadGetData, TimeRange.M15);
                    break;

                case "get30M":
                    this.Do(this.ThreadGetData, TimeRange.M30);
                    break;

                case "getDay":
                    this.Do(this.ThreadGetData, TimeRange.Day);
                    break;
            }
        }

        /// <summary>
        /// 画趋势图子菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void qushiSubMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.getDataSubMenu.Visible = false;
            this.btnCreate.Enabled = false;

             // 重新设置当前显示
            this.ResetDisplay();

            switch (e.ClickedItem.Name)
            {
                case "drawM5":
                    this.Do(this.ThreadDrawQushiImg, TimeRange.M5);
                    break;

                case "drawM15":
                    this.Do(this.ThreadDrawQushiImg, TimeRange.M15);
                    break;

                case "drawM30":
                    this.Do(this.ThreadDrawQushiImg, TimeRange.M30);
                    break;

                case "drawDay":
                    this.Do(this.ThreadDrawQushiImg, TimeRange.Day);
                    break;
            }
        }

        /// <summary>
        /// 切换时间级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChgTime_Click(object sender, EventArgs e)
        {
            if (this.subFolder.EndsWith("/"))
            {
                // 切换时间级别
                TimeRange timeRange = (TimeRange)Enum.Parse(typeof(TimeRange), this.subFolder.Substring(0, this.subFolder.Length - 1));
                switch (timeRange)
                {
                    case TimeRange.Day:
                        timeRange = TimeRange.M30;
                        break;

                    case TimeRange.M30:
                        timeRange = TimeRange.Day;
                        break;

                    //case TimeRange.M15:
                    //    timeRange = TimeRange.M5;
                    //    break;

                    //case TimeRange.M5:
                    //    timeRange = TimeRange.Day;
                    //    break;
                }

                this.btnChgTime.Text = timeRange.ToString();

                // 切换趋势图级别
                if (this.imgBody.Image != null)
                {
                    this.imgBody.Image.Dispose();
                    this.imgBody.Image = null;
                }
                this.subFolder = timeRange.ToString() + "/";

                // 设置当前的数据时间
                this.dataDate = this.GetDataDate(this.subFolder);

                // 设置当前数据
                this.SetCurStockData(this.allStock[this.curIdx] + "_" + this.dataDate);

                // 重新设置显示的趋势图
                this.stockImg = Consts.BASE_PATH + Consts.IMG_FOLDER + this.subFolder + this.allStock[this.curIdx] + Consts.IMG_TYPE;
                this.RedrawQushiImg();
            }
        }

        /// <summary>
        /// 当前日期减一天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDateBef_Click(object sender, EventArgs e)
        {
            // 设置当前的数据位置
            this.ReSetCurEndDataIdx(this.dataEndIdx - 1);

            // 重新设置显示的趋势图
            this.RedrawQushiImg();
        }

        /// <summary>
        /// 当前日期加一天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDateAft_Click(object sender, EventArgs e)
        {
            // 设置当前的数据位置
            this.ReSetCurEndDataIdx(this.dataEndIdx + 1);

            // 重新设置显示的趋势图
            this.RedrawQushiImg();
        }

        /// <summary>
        /// 当前日期变更到指定的天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbData_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.raiseDateChgEvent)
            {
                return;
            }

            this.dataEndIdx = this.dateIdxMap[this.cmbData.Items[this.cmbData.SelectedIndex].ToString()];

            // 设置日期按钮状态
            this.SetDatetimeBtnStatus();

            // 重新设置显示的趋势图
            this.RedrawQushiImg();
        }

        /// <summary>
        /// 跟踪当前数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveData_Click(object sender, EventArgs e)
        {
            this.SaveStockImg();
        }

        /// <summary>
        /// 试运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestRun_Click(object sender, EventArgs e)
        {
            //this.Do(this.CheckRightCd);
            //this.Do(this.ReplaceDayData);
            //this.Do(this.SetRongziRongYuan);
            this.Do(this.ImportCsvToDb);
            //this.ImportCsvToDb();
            //this.Do(this.CheckM5Data);
            //this.Do(this.CheckHolidayDate);
            //this.ChangeErrStockCsvName();
        }

        /// <summary>
        /// 对比两点趋势
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewDiff_Click(object sender, EventArgs e)
        {
            // 查看两点的趋势差异
            this.ViewTwoPointDiff();
        }

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateQushi_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.CloseDbConn();
        }

        #endregion

        #region " 私有方法 "

        #region " 各种数据处理 "

        /// <summary>
        /// 多线程取得所有分钟级别的数据
        /// </summary>
        private void ThreadGetData(params object[] param)
        {
            TimeRange timeRange = (TimeRange)param[0];

            DayBatchProcess batch = new DayBatchProcess(this.ResetProcessBar, this.AfterGetData, this.ProcessBarStep);
            batch.StartGetData(timeRange);
        }

        /// <summary>
        /// 画分钟级别趋势图
        /// </summary>
        private void ThreadDrawQushiImg(params object[] param)
        {
            TimeRange timeRange = (TimeRange)param[0];
            this.subFolder = timeRange.ToString() + "/";

            DayBatchProcess batch = new DayBatchProcess(this.ResetProcessBar, this.AfterDrawQushiImg, this.ProcessBarStep);
            batch.StartDrawQushiImg(timeRange);
        }

        /// <summary>
        /// 过滤当前趋势
        /// </summary>
        /// <param name="param"></param>
        private void ThreadChkQushi(params object[] param)
        { 
            if (param == null || param.Length < 2)
            {
                return;
            }

            QushiBase chkQushi = (QushiBase)param[0];
            FenXing fenXing = new FenXing(true);

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + Consts.DAY_FOLDER);

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            this.allStock.Clear();

            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                base.baseFile = fileItem.File;
                string shortName = Util.GetShortNameWithoutType(fileItem.File);
                if (NO_CHUANGYE && Util.IsChuangyeStock(shortName))
                {
                    continue;
                }

                string code = shortName.Substring(0, 6);
                if (this.allStockCdName.ContainsKey(code))
                {
                    string name = this.allStockCdName[code];
                    if (name.StartsWith("ST") || name.StartsWith("*ST"))
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                if (this.ChkQushi(shortName, chkQushi, fenXing))
                {
                    this.allStock.Add(code);
                }

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();

            // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, (Control)param[1]);

            // 刷新页面
            this.mSyncContext.Post(this.ReDisplayStockInfo, 0);

            // 保存趋势过滤的结果
            this.SaveQushiResult((string)param[2]);
        }

        /// <summary>
        /// 检查当前趋势
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <returns></returns>
        private bool ChkQushi(string stockCdData, QushiBase chkQushi, FenXing fenXing)
        {
            // 获得数据信息
            this.subFolder = Consts.DAY_FOLDER;
            Dictionary<string, object> dataInfo = DayBatchProcess.GetStockInfo(stockCdData, this.subFolder, "./");
            if (dataInfo == null)
            {
                return false;
            }

            List<BaseDataInfo> stockInfo = (List<BaseDataInfo>)dataInfo["stockInfos"];

            if (chkQushi is ChkDownBreakDaysQushi)
            {
                decimal[]  minMaxInfo = (decimal[])dataInfo["minMaxInfo"];
                return DayBatchProcess.ChkAllFirstBuyPoints(fenXing.DoFenXingComn(stockInfo), minMaxInfo);
            }
            else
            {
                return chkQushi.StartCheck(fenXing.DoFenXingComn(stockInfo));
            }
        }

        /// <summary>
        /// 查看两点的趋势差异
        /// </summary>
        private void ViewTwoPointDiff()
        {
            if (this.IsRangDateRight() == false)
            {
                return;
            }

            // 重新画趋势图
            DayBatchProcess dbp = new DayBatchProcess();
            string img = dbp.ViewTwoPointQushiImg(this.allStock[this.curIdx] + "_" + this.dataDate, this.dtStart.Value.ToString("yyyy/MM/dd"), this.dtEnd.Value.ToString("yyyy/MM/dd"));

            if (string.IsNullOrEmpty(img))
            {
                MessageBox.Show("重新画趋势图发生错误");
            }
            else
            {
                // 重新显示图片
                this.stockImg = img;
                this.ResetImg(0);
            }
        }

        /// <summary>
        /// 跟踪当前数据
        /// </summary>
        private void SaveStockImg()
        {
            if (this.IsRangDateRight() == false)
            {
                return;
            }

            // 重新画趋势图
            DayBatchProcess dbp = new DayBatchProcess();
            dbp.SaveM30QushiImg(this.allStock[this.curIdx] + "_" + this.dataDate, this.dtStart.Value.ToString("yyyy/MM/dd"), this.dtEnd.Value.ToString("yyyy/MM/dd"));

            MessageBox.Show("设定范围内的数据已经保存");
        }

        /// <summary>
        /// 检查日期范围是否正确
        /// </summary>
        /// <returns></returns>
        private bool IsRangDateRight()
        {
            if (this.dtStart.Value == null)
            {
                MessageBox.Show("开始日期不能为空");
                return false;
            }

            if (this.dtStart.Value > this.dtEnd.Value)
            {
                MessageBox.Show("开始日期不能超过结束日期");
                return false;
            }

            return true;
        }

        #endregion

        #region " 画面显示相关 "

        /// <summary>
        /// 设置日期控件
        /// </summary>
        private void SetDateValue()
        {
            DateTime now = DateTime.Now;
            this.dtEnd.Value = now;
            this.dtStart.Value = now.AddDays(-7);
        }

        /// <summary>
        /// 查看模拟的买卖点
        /// </summary>
        /// <param name="file"></param>
        private void ViewBuySellPoints(string file)
        {
            this.subFolder = TimeRange.M30.ToString() + "/";
            string filePath = Consts.BASE_PATH + Consts.BUY_SELL_POINT_HST + file;
            string[] allPoints = File.ReadAllLines(filePath);
            
            // 设置当前的数据时间
            this.dataDate = this.GetDataDate(this.subFolder);

            this.allStock.Clear();

            foreach (string line in allPoints)
            {
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                this.allStock.Add(line.Substring(0, 6));
            }

            // 重新显示当前信息
            this.ReDisplayStockInfo(0);

            this.cmbCon.Enabled = true;
        }

        /// <summary>
        /// 查看龙头股
        /// </summary>
        private void ViewLongtou()
        {
            this.subFolder = TimeRange.M30.ToString() + "/";
            string tmpFolder = TimeRange.Day.ToString() + "/";
            string filePath = Consts.BASE_PATH + Consts.CSV_FOLDER + @"龙头股.txt";
            string[] allPoints = File.ReadAllLines(filePath);

            // 设置当前的数据时间
            this.dataDate = this.GetDataDate(tmpFolder);

            this.allStock.Clear();

            // 将图片Copy过去
            string stockCd = string.Empty;
            string folder = Consts.BASE_PATH + Consts.RESULT_FOLDER + "龙头股";
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);

            foreach (string line in allPoints)
            {
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                stockCd = line.Substring(0, 6);
                this.allStock.Add(stockCd);

                // Copy图片
                string pngFile = @"\" + stockCd + Consts.IMG_TYPE;
                File.Copy(Consts.BASE_PATH + Consts.IMG_FOLDER + TimeRange.M30.ToString() + pngFile, folder + pngFile, true);
            }

            // 重新显示当前信息
            this.ReDisplayStockInfo(0);

            this.cmbCon.Enabled = true;
        }

        /// <summary>
        /// 取得数据后的处理
        /// </summary>
        private void AfterGetData()
        {
            // 关闭进度条
            this.CloseProcessBar();

            // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, this.btnGetAllStock);
        }

        /// <summary>
        /// 画完趋势图后的处理
        /// </summary>
        private void AfterDrawQushiImg()
        {
            // 关闭进度条
            this.CloseProcessBar();

            // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, this.btnCreate);

            // 显示所有信息
            this.mSyncContext.Post(this.DisplayAllStockPng, null);
        }

        /// <summary>
        /// 显示当前位置的数据信息
        /// </summary>
        /// <param name="x"></param>
        private int DisplayCurDayInfo(int x)
        {
            int pos = (int)((this.posFromRight + (this.imgBody.Image.Width - x - Consts.IMG_X_STEP)) / Consts.IMG_X_STEP);
            if (pos >= 0 && pos <= this.curStockData.Count - 1)
            {
                this.SetTitle(this.allStock[this.curIdx], pos);
            }

            return Util.GetPosYByX(this.curStockData, pos, this.imgBody.Image.Height);
        }

        /// <summary>
        /// 显示连个位置间的数据增量信息
        /// </summary>
        /// <param name="x"></param>
        private void DisplayDiffInfo(int x1, int x2)
        {
            int pos1 = (int)((this.posFromRight + (this.imgBody.Image.Width - x1 - Consts.IMG_X_STEP)) / Consts.IMG_X_STEP);
            int pos2 = (int)((this.posFromRight + (this.imgBody.Image.Width - x2 - Consts.IMG_X_STEP)) / Consts.IMG_X_STEP);
            if (pos1 >= 0 && pos1 <= this.curStockData.Count - 1
                && pos2 >= 0 && pos2 <= this.curStockData.Count - 1)
            {

                decimal value1 = this.curStockData[pos1].DayVal;
                decimal value2 = this.curStockData[pos2].DayVal;
                decimal tmp, lastVal;
                int idx;
                if (pos1 < pos2)
                {
                    tmp = (value1 - value2) * 100 / value2;
                    idx = pos1;
                    lastVal = value1;
                }
                else
                {
                    tmp = (value2 - value1) * 100 / value1;
                    idx = pos2;
                    lastVal = value2;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(this.allStock[this.curIdx]).Append(" ");
                sb.Append(this.curStockName).Append(" ");

                DateTime dt = this.GetDateFromString(this.curStockData[idx].Day);
                if (this.curStockData[idx].Day.Length == 8)
                {
                    sb.Append(dt.ToString("yy年MM月dd日  "));
                }
                else
                {
                    sb.Append(dt.ToString("yy年MM月dd日 HH:mm  "));
                }
                sb.Append(lastVal).Append("(");
                sb.Append(tmp.ToString("0.00")).Append(")");

                this.Text = TITLE + sb.ToString();
            }
        }

        /// <summary>
        /// 设置当前标题
        /// </summary>
        /// <param name="stockCd"></param>
        private void SetTitle(string stockCd, int idx)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(stockCd).Append(" ");
            sb.Append(this.curStockName).Append(" ");

            if (this.curStockData.Count > 0 && !string.IsNullOrEmpty(this.curStockData[idx].Day))
            {
                DateTime dt = this.GetDateFromString(this.curStockData[idx].Day);
                if (this.curStockData[idx].Day.Length == 8)
                {
                    sb.Append(dt.ToString("yy年MM月dd日  "));
                    sb.Append("1:").Append(this.GetStockValInfo(this.curStockData, idx));
                }
                else
                {
                    sb.Append(dt.ToString("yy年MM月dd日 HH:mm  "));
                    sb.Append(this.GetStockValInfo(this.curStockData, idx));
                }
                sb.Append("   ");
            }

            if (this.curStockJibie5Data.Count > 0)
            {
                sb.Append("5:").Append(this.GetStockValInfo(this.curStockJibie5Data, idx));
                sb.Append("   ");
            }

            if (this.curStockJibie10Data.Count > 0)
            {
                sb.Append("10:").Append(this.GetStockValInfo(this.curStockJibie10Data, idx));
                sb.Append("   ");
            }

            this.Text = TITLE + sb.ToString();
        }

        /// <summary>
        /// 显示所有信息
        /// </summary>
        private void DisplayAllStockPng(object param)
        {
            // 设置当前的数据时间
            this.dataDate = this.GetDataDate(this.subFolder);

            // 取得已经存在的所有趋势图
            List<FilePosInfo> allImg = Util.GetAllFiles(Consts.BASE_PATH + Consts.IMG_FOLDER + this.subFolder);

            this.allStock.Clear();

            foreach (FilePosInfo fileItem in allImg)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                string shortName = Util.GetShortNameWithoutType(fileItem.File);
                if (NO_CHUANGYE && Util.IsChuangyeStock(shortName))
                {
                    continue;
                }

                this.allStock.Add(shortName);
            }

            // 重新显示当前信息
            this.ReDisplayStockInfo(0);

            // 重置下拉框选中项
            this.needRaiseEvent = false;
            if (this.subFolder.Equals(Consts.DAY_FOLDER))
            {
                this.cmbCon.SelectedItem = "所有天数据";
            }
            else if (this.subFolder.Equals(TimeRange.M5.ToString() + "/"))
            {
                this.cmbCon.SelectedItem = "所有5分钟数据";
            }
            else if (this.subFolder.Equals(TimeRange.M30.ToString() + "/"))
            {
                this.cmbCon.SelectedItem = "所有30分钟数据";
            }
            this.needRaiseEvent = true;
        }

        /// <summary>
        /// 显示存在买点的数据
        /// </summary>
        private void DisplayHasBuyPointsStock()
        {
            this.allStock.Clear();

            foreach (KeyValuePair<string, string> stockDate in this.hasBuyPointsStock)
            {
                if (NO_CHUANGYE && Util.IsChuangyeStock(stockDate.Key))
                {
                    continue;
                }

                this.allStock.Add(stockDate.Key);
            }

            // 重新显示当前信息
            this.ReDisplayStockInfo(0);
        }

        /// <summary>
        /// 重新设置当前显示
        /// </summary>
        private void ResetDisplay()
        {
            if (this.imgBody.Image != null)
            {
                this.imgBody.Image.Dispose();
                this.imgBody.Image = null;
            }
            this.allStock.Clear();
            this.btnBef.Enabled = false;
            this.btnAft.Enabled = false;
            this.lblCntInfo.Text = "0/0";
        }

        /// <summary>
        /// 重新显示当前信息
        /// </summary>
        /// <param name="index"></param>
        private void ReDisplayStockInfo(object objIndex)
        {
            int index = (int)objIndex;

            // 设置当前位置
            if (index < 0)
            {
                this.curIdx = 0;
            }
            else if (index >= this.allStock.Count)
            {
                this.curIdx = this.allStock.Count > 0 ? this.allStock.Count - 1 : 0;
            }
            else
            {
                this.curIdx = index;
            }

            // 显示当前位置的走势图片
            if (this.curIdx >= 0 && this.curIdx <= this.allStock.Count - 1)
            {
                // 设置按钮可用与否
                if (this.curIdx == 0)
                {
                    this.btnBef.Enabled = false;
                }
                else
                {
                    this.btnBef.Enabled = true;
                }

                if (this.curIdx == this.allStock.Count - 1 || this.allStock.Count == 0)
                {
                    this.btnAft.Enabled = false;
                }
                else
                {
                    this.btnAft.Enabled = true;
                }

                // 设置当前数据
                this.SetCurStockData(this.allStock[this.curIdx] + "_" + this.dataDate);

                this.stockImg = Consts.BASE_PATH + Consts.IMG_FOLDER + this.subFolder + this.allStock[this.curIdx] + Consts.IMG_TYPE;
                this.posFromRight = 0;
                this.RedrawQushiImg();

                // 重新设置日期列表
                this.ReSetCmbData();

                // 设置当前的数据位置
                this.ReSetCurEndDataIdx(0);

                // 设置当前数据名称
                this.SetCurStockName(this.allStock[this.curIdx]);

                // 显示标题
                this.SetTitle(this.allStock[this.curIdx], 0);
            }

            // 显示数量信息
            this.lblCntInfo.Text = (this.curIdx + 1) + "/" + this.allStock.Count;
        }

        /// <summary>
        /// 设置当前数据名称
        /// </summary>
        /// <param name="stockCd"></param>
        private void SetCurStockName(string stockCd)
        {
            this.curStockName = string.Empty;
            if (this.allStockCdName.ContainsKey(stockCd))
            {
                this.curStockName = this.allStockCdName[stockCd];
            }
        }

        /// <summary>
        /// 重新设置日期列表
        /// </summary>
        private void ReSetCmbData()
        {
            this.raiseDateChgEvent = false;

            this.dateIdxMap.Clear();
            this.cmbData.Items.Clear();
            List<string> dateLst = new List<string>();
            for (int i = 0; i < this.curStockData.Count; i++)
            {
                string tmpDay = this.GetDateFromString(this.curStockData[i].Day.Substring(0, 8)).ToString("yyyy-MM-dd");
                if (!dateLst.Contains(tmpDay))
                {
                    dateLst.Add(tmpDay);
                    this.dateIdxMap.Add(tmpDay, i);
                }
            }

            this.cmbData.Items.AddRange(dateLst.ToArray());
            if (this.cmbData.Items.Count > 0)
            {
                this.cmbData.SelectedIndex = 0;
            }
            else
            {
                this.cmbData.SelectedIndex = -1;
            }

            this.raiseDateChgEvent = true;
        }

        /// <summary>
        /// 设置当前的数据位置
        /// </summary>
        private void ReSetCurEndDataIdx(int idx)
        {
            if (idx < this.cmbData.Items.Count)
            {
                this.dataEndIdx = idx;

                this.raiseDateChgEvent = false;
                this.cmbData.SelectedIndex = idx;
                this.raiseDateChgEvent = true;

                // 设置日期按钮状态
                this.SetDatetimeBtnStatus();
            }
        }

        /// <summary>
        /// 设置日期按钮状态
        /// </summary>
        private void SetDatetimeBtnStatus()
        {
            if (this.dataEndIdx > 0)
            {
                this.btnDateBef.Enabled = true;
            }
            else
            {
                this.btnDateBef.Enabled = false;
            }

            if (this.dataEndIdx < this.curStockData.Count - 1)
            {
                this.btnDateAft.Enabled = true;
            }
            else
            {
                this.btnDateAft.Enabled = false;
            }
        }

        /// <summary>
        /// 设置当前数据
        /// </summary>
        /// <param name="stockCdDate"></param>
        private void SetCurStockData(string stockCdDate)
        {
            // 获得数据信息
            Dictionary<string, object> dataInfo = DayBatchProcess.GetStockInfo(stockCdDate, this.subFolder, "./");
            if (dataInfo == null)
            {
                this.curStockData.Clear();
                this.curStockJibie5Data.Clear();
                this.curStockJibie10Data.Clear();
                return;
            }

            // 基础数据信息
            this.curStockData = (List<BaseDataInfo>)dataInfo["stockInfos"];

            if (this.subFolder == Consts.DAY_FOLDER)
            {
                // 取得5日级别信息
                this.curStockJibie5Data = DayBatchProcess.GetAverageLineInfo(this.curStockData, 5);

                // 取得10日级别信息
                this.curStockJibie10Data = DayBatchProcess.GetAverageLineInfo(this.curStockData, 10);
            }
            else
            {
                this.curStockJibie5Data.Clear();
                this.curStockJibie10Data.Clear();
            }
        }

        /// <summary>
        /// 重新设置页面高度
        /// </summary>
        private void ResetPageHeight()
        {
            if (this.imgBody.Height != Consts.IMG_H)
            {
                int diff = this.imgBody.Height - Consts.IMG_H;
                this.Height -= diff;
            }
        }

        /// <summary>
        /// 重新计算图片结束位置
        /// </summary>
        private TimeRange ResetImgPosRight()
        {
            // 判断时间级别
            TimeRange timeRange = (TimeRange)Enum.Parse(typeof(TimeRange), this.subFolder.Substring(0, this.subFolder.Length - 1));

            switch (timeRange)
            {
                case TimeRange.Day:
                    this.posFromRight = this.dataEndIdx * Consts.IMG_X_STEP;
                    break;

                case TimeRange.M30:
                    this.posFromRight = this.dataEndIdx * Consts.IMG_X_STEP * 8;
                    break;

                case TimeRange.M5:
                    this.posFromRight = this.dataEndIdx * Consts.IMG_X_STEP * 48;
                    break;
            }

            return timeRange;
        }

        /// <summary>
        /// 重新设置显示的趋势图
        /// </summary>
        /// <param name="imgFile"></param>
        private void RedrawQushiImg()
        {
            // 重新计算图片结束位置
            TimeRange timeRange = this.ResetImgPosRight();

            // 重新画趋势图
            DayBatchProcess dbp = new DayBatchProcess();
            dbp.CreateQushiImg(this.allStock[this.curIdx] + "_" + this.dataDate, timeRange);

            if (!File.Exists(this.stockImg))
            {
                return;
            }

            // 重新显示图片
            this.ResetImg(this.posFromRight);
        }

        /// <summary>
        /// 重新显示图片
        /// </summary>
        /// <param name="posFromRight"></param>
        private void ResetImg(int posFromRight)
        {
            Image imgFrom = Image.FromFile(this.stockImg);
            Bitmap imgTo = new Bitmap(this.imgBody.Width, Consts.IMG_H);
            Graphics grp = Graphics.FromImage(imgTo);
            this.oldImgWidth = imgFrom.Width;
            int srcImgX = imgFrom.Width - posFromRight;
            int toImgX = 0;
            Rectangle srcRect;
            if (srcImgX <= imgTo.Width)
            {
                srcImgX = 0;
                srcRect = new Rectangle(srcImgX, 0, imgFrom.Width, imgFrom.Height);
                toImgX = imgTo.Width - imgFrom.Width;
            }
            else
            {
                srcImgX -= imgTo.Width;
                srcRect = new Rectangle(srcImgX, 0, imgTo.Width, imgFrom.Height);
                toImgX = 0;
            }
            grp.DrawImage(imgFrom, toImgX, 0, srcRect, GraphicsUnit.Pixel);

            if (this.imgBody.Image != null)
            {
                this.imgBody.Image.Dispose();
                this.imgBody.Image = null;
            }
            this.imgBody.Image = imgTo;

            // 释放Graphics和图片资源
            grp.Dispose();
            imgFrom.Dispose();
        }

        /// <summary>
        /// 拖动图片处理
        /// </summary>
        private void DragImg(int newX)
        {
            if (this.oldImgWidth <= this.imgBody.Image.Width)
            {
                // 无法移动时，直接返回
                this.Cursor = Cursors.No;
                return;
            }

            if (newX < this.oldImgX && this.posFromRight <= 0)
            {
                // 向左移动，但是已经到最右端了，直接返回
                this.Cursor = Cursors.No;
                return;
            }

            if (newX > this.oldImgX && (this.oldImgWidth - this.posFromRight) <= this.imgBody.Image.Width)
            {
                // 向右移动，但是已经到最左端了，直接返回
                this.Cursor = Cursors.No;
                return;
            }

            this.Cursor = Cursors.Hand;
            this.posFromRight += newX - this.oldImgX;
            this.oldImgX = newX;

            if (this.posFromRight % Consts.IMG_X_STEP == 0)
            {
                this.stockImg = Consts.BASE_PATH + Consts.IMG_FOLDER + this.subFolder + this.allStock[this.curIdx] + Consts.IMG_TYPE;
                this.RedrawQushiImg();
            }
        }

        #endregion

        #region " 各种基本处理 "

        /// <summary>
        /// 取得当前数据的最新时间
        /// </summary>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        private string GetDataDate(string subFolder)
        {
            FilePosInfo fileInfo = null;

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + subFolder).Where(p => !p.IsFolder).ToList();
            if (allCsv.Count > 0)
            {
                fileInfo = allCsv[0];
            }

            if (fileInfo != null)
            {
                return Util.GetShortNameWithoutType(fileInfo.File).Substring(7);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 绑定子菜单事件
        /// </summary>
        private void SetSubMenuEvent()
        {
            this.getDataSubMenu.ItemClicked += new ToolStripItemClickedEventHandler(this.getDataSubMenu_ItemClicked);
            this.qushiSubMenu.ItemClicked += new ToolStripItemClickedEventHandler(this.qushiSubMenu_ItemClicked);

            // 取数据的子菜单
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Name = "get5M";
            item.Text = "5分钟";
            this.getDataSubMenu.Items.Add(item);

            item = new ToolStripMenuItem();
            item.Name = "get15M";
            item.Text = "15分钟";
            this.getDataSubMenu.Items.Add(item);

            item = new ToolStripMenuItem();
            item.Name = "get30M";
            item.Text = "30分钟";
            this.getDataSubMenu.Items.Add(item);

            this.getDataSubMenu.Items.Add(new ToolStripSeparator());

            item = new ToolStripMenuItem();
            item.Name = "getDay";
            item.Text = "天";
            this.getDataSubMenu.Items.Add(item);

            // 画趋势图的子菜单
            item = new ToolStripMenuItem();
            item.Name = "drawM5";
            item.Text = "5分钟级别";
            this.qushiSubMenu.Items.Add(item);

            item = new ToolStripMenuItem();
            item.Name = "drawM15";
            item.Text = "15分钟级别";
            this.qushiSubMenu.Items.Add(item);

            item = new ToolStripMenuItem();
            item.Name = "drawM30";
            item.Text = "30分钟级别";
            this.qushiSubMenu.Items.Add(item);

            this.qushiSubMenu.Items.Add(new ToolStripSeparator());

            item = new ToolStripMenuItem();
            item.Name = "drawDay";
            item.Text = "天级别";
            this.qushiSubMenu.Items.Add(item);
        }

        /// <summary>
        /// 根据最后一天的日期信息判断是否是合理的数据
        /// </summary>
        /// <param name="maxDate"></param>
        /// <returns></returns>
        private bool IsValidStock(string maxDate)
        {
            DateTime maxDt = this.GetDateFromString(maxDate);
            DateTime chkDt = this.GetDateFromString(this.dataDate);

            if (DateTime.Compare(chkDt, maxDt.AddDays(30)) <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 日期类型转换
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime GetDateFromString(string date)
        {
            string dateFormat = "yyyyMMdd";
            if (date.Length > 8)
            {
                dateFormat = "yyyyMMddHHmmss";
            }

            return DateTime.ParseExact(date, dateFormat, System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// 根据代码取得数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        private string[] GetStockFileContent(string stockCd)
        {
            string csvFile = Consts.BASE_PATH + Consts.CSV_FOLDER + Consts.DAY_FOLDER + stockCd + "_" + this.dataDate + ".csv";
            if (File.Exists(csvFile))
            {
                return File.ReadAllLines(csvFile, Encoding.UTF8);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 保存趋势过滤的结果
        /// </summary>
        private void SaveQushiResult(string fileName)
        {
            if (this.allStock.Count == 0 || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            // 将图片Copy过去
            string folder = Consts.BASE_PATH + Consts.RESULT_FOLDER + fileName;
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);

            StringBuilder sb = new StringBuilder();
            foreach (string stockCd in this.allStock)
            {
                string[] allLine = this.GetStockFileContent(stockCd);
                if (allLine != null && allLine.Length > 2)
                {
                    sb.Append(stockCd).Append(" ");

                    string[] lineData = allLine[1].Split(',');
                    sb.Append(lineData[2]).Append(" ").Append(lineData[3]).Append("\r\n");
                }

                // Copy图片
                string pngFile = @"\" + stockCd + Consts.IMG_TYPE;
                File.Copy(Consts.BASE_PATH + Consts.IMG_FOLDER + TimeRange.M30.ToString() + pngFile, folder + pngFile, true);
            }

            File.WriteAllText(Consts.BASE_PATH + Consts.RESULT_FOLDER + fileName + ".txt", sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 读取包括买点信息的数据
        /// </summary>
        private void LoadHasBuyPointsInfo()
        {
            string file = Consts.BASE_PATH + Consts.RESULT_FOLDER + "BuyPoints.txt";
            this.hasBuyPointsStock.Clear();
            if (File.Exists(file))
            {
                string[] allLine = File.ReadAllLines(file, Encoding.UTF8);
                if (allLine != null && allLine.Length > 0)
                {
                    foreach (string line in allLine)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        string[] lineTxt = line.Split(' ');
                        this.hasBuyPointsStock.Add(new KeyValuePair<string, string>(lineTxt[0], lineTxt[1]));
                    }
                }
            }
        }

        /// <summary>
        /// 保存包括买点信息的数据
        /// </summary>
        private void SaveHasBuyPointsInfo()
        {
            // 买点排序
            this.hasBuyPointsStock.Sort(this.BuyPointCompare);

            List<string> tmpList = new List<string>();
            this.hasBuyPointsStock.ForEach(p => tmpList.Add(p.Key + " " + p.Value));

            File.WriteAllLines(Consts.BASE_PATH + Consts.RESULT_FOLDER + "BuyPoints.txt", tmpList.ToArray(), Encoding.UTF8);
        }

        /// <summary>
        /// 对象比较
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int BuyPointCompare(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
        {
            return string.Compare(b.Value, a.Value);
        }

        /// <summary>
        /// 设置UI的按钮可用
        /// </summary>
        /// <param name="btn"></param>
        private void UISetBtnEnable(object btn)
        {
            if (btn != null)
            {
                ((Control)btn).Enabled = true;
            }
        }

        /// <summary>
        /// 取得当前价位和变动比率
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        private string GetStockValInfo(List<BaseDataInfo> stockInfos, int idx)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(stockInfos[idx].DayVal);
            if (idx >= 0 && idx < stockInfos.Count - 1)
            {
                sb.Append("(");
                decimal tmp = (stockInfos[idx].DayVal - stockInfos[idx + 1].DayVal) * 100 / stockInfos[idx + 1].DayVal;
                sb.Append(tmp.ToString("0.00"));
                sb.Append(")");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 取得所有数据的基本信息（代码+名称）
        /// </summary>
        private List<string> GetAllStockBaseInfo()
        {
            List<string> allCd = new List<string>();
            this.allStockCdName.Clear();

            string[] allLine = File.ReadAllLines(Consts.BASE_PATH + Consts.CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
            if (allLine != null && allLine.Length > 0)
            {
                foreach (string codeName in allLine)
                {
                    if (string.IsNullOrEmpty(codeName))
                    {
                        continue;
                    }

                    string code = codeName.Substring(0, 6);
                    this.allStockCdName.Add(code, codeName.Substring(7));

                    allCd.Add(code);
                }
            }

            return allCd;
        }

        #endregion

        #region " 测试模块 "

        /// <summary>
        /// 节假日检查
        /// </summary>
        private void CheckHolidayDate()
        {
            DateTime dt = DateTime.Now;
            DateTime minDt = Convert.ToDateTime("1990-01-01 09:00:00"); 
            StringBuilder sb = new StringBuilder();

            while (dt.CompareTo(minDt) > 0)
            {
                int isHoliday = Util.IsHolidayByDate(dt);
                if (isHoliday > 0)
                {
                    sb.Append(dt.ToString("yyyy-MM-dd ")).Append(isHoliday).Append("\r\n");
                }

                dt = dt.AddDays(-1);
            }
            File.WriteAllText(@"./Data/Holiday" + DateTime.Now.ToString("yyyyMMdd") + ".txt", sb.ToString(), Encoding.UTF8);
        }

        private void CheckM5Data()
        {
            // 取得已经存在的所有数据信息
            this.subFolder = TimeRange.M5.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder);
            List<string> errData = new List<string>();

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (FilePosInfo fileItem in allCsv)
                {
                    if (fileItem.IsFolder)
                    {
                        continue;
                    }

                    base.baseFile = fileItem.File;
                    string[] allLine = File.ReadAllLines(fileItem.File);
                    if (allLine.Length > 1)
                    {
                        if (!allLine[1].StartsWith("2020-04-01"))
                        {
                            //File.Delete(fileItem.File);
                            errData.Add(Util.GetShortNameWithoutType(fileItem.File) + " " + allLine[1]);
                        }
                    }
                    else
                    {
                        //File.Delete(fileItem.File);
                        errData.Add(Util.GetShortNameWithoutType(fileItem.File) + " " + allLine[1]);
                    }

                    // 更新进度条
                    this.ProcessBarStep();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\n" + e.StackTrace);
            }

            File.WriteAllLines(Consts.BASE_PATH + Consts.CSV_FOLDER + "dataErrChkM5" + DateTime.Now.ToString("yyyyMMdd") + ".txt", errData.ToArray(), Encoding.UTF8);

            // 关闭进度条
            this.CloseProcessBar();
        }

        /// <summary>
        /// Csv数据导入到MOracle
        /// </summary>
        private void ImportCsvToDb()
        {
            // 取得配置的DB信息
            string[] dbAddrInfo = File.ReadAllLines(@".\DbAddrInfo.txt");
            this.connStr = dbAddrInfo[2];

            // 关闭数据库连接
            this.CloseDbConn();

            // 创建数据连接
            if (!this.CreateDbConn(this.connStr))
            {
                return;
            }

            hasError = false;

            // 取得已经存在的所有数据信息
            this.subFolder = TimeRange.M5.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder).Where(p => !p.IsFolder).ToList();
            Dictionary<string, string> stockMaxDate = new Dictionary<string, string>();
            foreach (FilePosInfo fileItem in allCsv)
            {
                stockMaxDate.Add(Util.GetShortNameWithoutType(fileItem.File).Substring(0, 6),
                    Util.GetShortNameWithoutType(fileItem.File).Substring(7));
            }

            // 开始导入代码名称数据
            StringBuilder sb = new StringBuilder();
            string dt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            string[] allLine = File.ReadAllLines(@".\data\AllStockInfo.txt", Encoding.UTF8);

            // 设置进度条
            this.ResetProcessBar(allLine.Length);

            OracleTransaction transaction = this.conn.BeginTransaction();
            try
            {
                OracleCommand cmd = this.conn.CreateCommand();
                cmd.Transaction = transaction;

                for (int i = 0; i < allLine.Length; i++)
                {
                    sb.Length = 0;
                    sb.Append("insert into STOCK_INFO (stock_code, max_trade_date, stock_name, memo) VALUES ");
                    sb.Append("('").Append(allLine[i].Substring(0, 6)).Append("'");
                    if (stockMaxDate.ContainsKey(allLine[i].Substring(0, 6)))
                    {
                        sb.Append(",TO_DATE('").Append(stockMaxDate[allLine[i].Substring(0, 6)]).Append("', 'YYYYMMDDHH24MISS') ");
                    }
                    else
                    {
                        sb.Append(",TO_DATE('19700101000000', 'YYYYMMDDHH24MISS') ");
                    }
                    sb.Append(",'").Append(allLine[i].Substring(7)).Append("'");
                    sb.Append(",''").Append(")");


                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();

                    if (i > 0 && i % 1000 == 0)
                    {
                        transaction.Commit();
                        transaction = this.conn.BeginTransaction();
                        cmd.Transaction = transaction;
                    }
                    else if (i == allLine.Length - 1)
                    {
                        transaction.Commit();
                    }

                    // 更新进度条
                    this.ProcessBarStep();
                }
            }
            catch (Exception e)
            {
                hasError = true;
                transaction.Rollback();
                this.WriteSqlLog(e.Message + "\n" + e.StackTrace);
            }

            //// 关闭进度条
            //this.CloseProcessBar();

            // 导入天的数据
            //this.ImportCsvToMySql(mySQLConn, TimeRange.Day, "data_day", dt);

            // 导入M5的数据
            //this.ImportCsvToDb(TimeRange.M5, "m5");

            // 导入M15的数据
            //this.ImportCsvToMySql(mySQLConn, TimeRange.M15, "data_m15", dt);

            // 导入M30的数据
            //this.ImportCsvToMySql(mySQLConn, TimeRange.M30, "data_m30", dt);

            // 关闭数据库连接
            //this.CloseDbConn();

            if (hasError)
            {
                MessageBox.Show("出现错误 \n具体信息参考LOG表");
            }
            else
            {
                MessageBox.Show("成功导入数据");
            }
        }

        /// <summary>
        /// 写Log到数据库
        /// </summary>
        /// <param name="message"></param>
        private void WriteSqlLog(string message)
        {
            try
            {
                OracleCommand cmd = this.conn.CreateCommand();
                StringBuilder sb = new StringBuilder();

                sb.Length = 0;
                sb.Append("insert into LOG (log_date, log_info) VALUES (SYSDATE, '");
                sb.Append(message).Append("')");

                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(message + "\r\n" + e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 创建数据连接
        /// </summary>
        private bool CreateDbConn(string connString)
        {
            try
            {
                conn = new OracleConnection(connString);
                conn.Open();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        private void CloseDbConn()
        {
            try
            {
                if (this.conn != null)
                {
                    this.conn.Close();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\n" + e.StackTrace);
            }
        }

        private void ChangeErrStockCsvName()
        {
            this.subFolder = TimeRange.Day.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder).Where(p => !p.IsFolder).ToList();

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            foreach (FilePosInfo fileItem in allCsv)
            {
                string fileDate = Util.GetShortNameWithoutType(fileItem.File).Substring(7);
                string[] allLine = File.ReadAllLines(fileItem.File, Encoding.UTF8);
                if (allLine.Length > 2)
                {
                    string[] maxDateLine = allLine[1].Split(',');
                    string maxDate = Util.TrimDate(maxDateLine[0]);
                    if (maxDate.CompareTo(fileDate) < 0)
                    {
                        File.Move(fileItem.File, fileItem.File.Replace(fileDate, maxDate));
                    }
                }

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();
        }

        /// <summary>
        /// 等待空闲线程
        /// </summary>
        private void WaitForThreadSlot()
        {
            while (true)
            {
                lock (LockObj)
                {
                    if (_activeThreads < MaxThreads)
                    {
                        _activeThreads++;
                        return;
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 导入特定类型数据到Oracle
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="dbName"></param>
        private void ImportCsvToDb(TimeRange timeRange, string dbName)
        {
            // 取得已经存在的所有数据信息
            this.subFolder = timeRange.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder).Where(p => !p.IsFolder).ToList();

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            _totalTasks = allCsv.Count;

            // 创建线程池控制器
            using (var countdown = new ManualResetEvent(false))
            {
                //Dictionary<string, string> allllStockMaxDate = GetAllStockMaxDate(dbName);
                foreach (FilePosInfo fileItem in allCsv)
                {
                    //if (Util.GetShortNameWithoutType(fileItem.File).Substring(0, 6).CompareTo("600095") < 0)
                    //{
                    //    continue;
                    //}

                    //if (Util.GetShortNameWithoutType(fileItem.File).Substring(0, 6).CompareTo("600096") > 0)
                    //{
                    //    countdown.Set();
                    //    break;
                    //}
                    //string curStockCd = Util.GetShortNameWithoutType(fileItem.File).Substring(0, 6);
                    //string curMaxDt = Util.GetShortNameWithoutType(fileItem.File).Substring(7);
                    //// 比较当然CSV的数据，和数据库的最大时间数据
                    //if (allllStockMaxDate.ContainsKey(curStockCd))
                    //{
                    //    if (curMaxDt.Equals(Util.TrimDate(allllStockMaxDate[curStockCd])))
                    //    {
                    //        continue;
                    //    }
                    //}

                    // 等待可用线程
                    WaitForThreadSlot();

                    // 启动线程
                    base.baseFile = fileItem.File;
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var csvFile = (string)state;
                        InsertStockData(csvFile, dbName, countdown);
                    }, fileItem.File);

                    // 更新进度条
                    this.ProcessBarStep();
                }

                // 等待所有线程完成
                countdown.WaitOne();
                
                // 关闭进度条
                this.CloseProcessBar();

                if (hasError)
                {
                    MessageBox.Show("出现错误 \n具体信息参考LOG表");
                }
                else
                {
                    MessageBox.Show("成功导入数据");
                }
            }
        }

        /// <summary>
        /// 取得所有数据的最大日期
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllStockMaxDate(string dbName)
        {
            Dictionary<string, string> allllStockMaxDate = new Dictionary<string, string>();
            try
            {
                OracleCommand insCmd = this.conn.CreateCommand();
                StringBuilder sb = new StringBuilder();

                sb.Length = 0;
                sb.Append("select to_char(max(trade_date), 'yyyy-MM-dd HH24:mi:ss'), stock_code from ").Append(dbName);
                sb.Append(" Group by stock_code ");
                insCmd.CommandText = sb.ToString();

                OracleDataReader dbResult = insCmd.ExecuteReader();
                if (dbResult != null)
                {
                    while (dbResult.Read())
                    {
                        allllStockMaxDate.Add(dbResult.GetString(1).ToString(), dbResult.GetString(0).ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteSqlLog("取得所有数据的最大日期时错误" + "\r\n" + ex.Message + "\n" + ex.StackTrace);
            }

            return allllStockMaxDate;
        }

        /// <summary>
        /// CSV的数据插入到数据库
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="dbName"></param>
        /// <param name="countdown"></param>
        private void InsertStockData(string csvFile, string dbName, ManualResetEvent countdown)
        {
            OracleConnection conn = null;
            //OracleTransaction transaction = null;
            StringBuilder sb = new StringBuilder();
            string stockCode = Util.GetShortNameWithoutType(csvFile).Substring(0, 6);

            try
            {
                if (!File.Exists(csvFile))
                {
                    return;
                }

                // 连接配置
                conn = new OracleConnection(this.connStr);
                conn.Open();
                
                OracleCommand insCmd = conn.CreateCommand();
                string maxDt = string.Empty;
                string maxDtChk = string.Empty;

                sb.Length = 0;
                sb.Append("select to_char(max(trade_date), 'yyyy-MM-dd HH24:mi:ss'), to_char(max(trade_date), 'yyyyMMddHH24miss') from ").Append(dbName);
                sb.Append(" Where stock_code = '").Append(stockCode).Append("'");
                insCmd.CommandText = sb.ToString();

                maxDt = string.Empty;
                OracleDataReader dbResult = insCmd.ExecuteReader();
                if (dbResult != null && dbResult.Read())
                {
                    if (!dbResult.GetOracleString(0).IsNull)
                    {
                        maxDt = dbResult.GetString(0).ToString();
                        maxDtChk = dbResult.GetString(1).ToString();
                    }
                }

                // 判断是否当前CSV已经导入了
                if (!maxDtChk.Equals(string.Empty) && Util.GetShortNameWithoutType(csvFile).EndsWith(maxDtChk))
                {
                    return;
                }

                this.WriteSqlLog("Start Import " + stockCode);
                sb.Length = 0;
                int lineCnt = 0;

                //// 启动事务
                //transaction = conn.BeginTransaction();
                //insCmd.Transaction = transaction;

                // 读取CSV数据
                string[] allLine = File.ReadAllLines(csvFile);
                int maxLen = allLine.Length - 1;
                sb.Append(" BEGIN ");
                for (int i = 1; i <= maxLen; i++)
                {
                    // 2020-03-18 15:00:00,000001,,12.710,12.740,12.650,12.720
                    // datetime,code,name,close_val,max_val,min_val,open_val
                    string[] curLine = allLine[i].Split(',');
                    if (string.Compare(curLine[0], maxDt) > 0)
                    {
                        lineCnt++;

                        //sb.Append(" into ").Append(dbName).Append(" (");
                        sb.Append(" INSERT into ").Append(dbName).Append(" (");
                        sb.Append(" trade_date, stock_code, close_price, high_price, low_price, open_price) VALUES (");
                        sb.Append(" to_date('").Append(curLine[0]).Append("', 'yyyy-MM-dd HH24:mi:ss') ");
                        sb.Append(",'").Append(curLine[1].Replace("'", "")).Append("'");
                        sb.Append(",").Append(curLine[3]);
                        sb.Append(",").Append(curLine[4]);
                        sb.Append(",").Append(curLine[5]);
                        sb.Append(",").Append(curLine[6]);
                        sb.Append(") ;\r\n ");

                        //insCmd.CommandText = sb.ToString();
                        //sb.Length = 0;
                        //insCmd.ExecuteNonQuery();

                        if (lineCnt % 1000 == 0 && i < maxLen)
                        {
                            sb.Append(" COMMIT; END; ");
                            insCmd.CommandText = sb.ToString();
                            insCmd.ExecuteNonQuery();
                            sb.Length = 0;
                            sb.Append(" BEGIN ");

                            //transaction.Commit();
                            //transaction = conn.BeginTransaction();
                            //insCmd.Transaction = transaction;
                            lineCnt = 0;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (lineCnt > 0)
                {
                    sb.Append(" COMMIT; END; ");
                    insCmd.CommandText = sb.ToString();
                    insCmd.ExecuteNonQuery();
                    //transaction.Commit();
                }
                //transaction.Dispose();
                //transaction = null;

                this.WriteSqlLog("End Import " + stockCode);
            }
            catch (Exception ex)
            {
                hasError = true;
                // 事务回滚（自动回滚未提交数据）
                //if (transaction != null)
                //{
                //    transaction.Rollback();
                //}
                File.AppendAllText(logFile, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + ex.Message + "\r\n" + ex.StackTrace + "\r\n", Encoding.UTF8);
                this.WriteSqlLog(stockCode + "\r\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                // 资源清理
                //if (transaction != null)
                //{
                //    transaction.Dispose();
                //}
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
                // 任务完成标记
                lock (LockObj)
                {
                    _activeThreads--;
                    if (++_completedTasks >= _totalTasks)
                        countdown.Set();
                }
            }
        }

        /// <summary>
        /// 融资融券设置
        /// </summary>
        private void SetRongziRongYuan()
        {
            string[] allLine = File.ReadAllLines(@"./Data/RongZiRongYuan.txt");
            StringBuilder sb = new StringBuilder();

            foreach (string line in allLine)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] tmp = line.Split(' ');
                if (tmp.Length >= 3)
                {
                    sb.Append(tmp[0]).Append(" ");
                    sb.Append(tmp[1]).Append(" ");
                    sb.Append(tmp[2]).Append("\r\n");
                }
            }

            File.WriteAllText(@"./Data/RongZiRongYuan1.txt", sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 修正天的数据
        /// </summary>
        private void ReplaceDayData()
        {
            // 取得已经存在的所有数据信息
            this.subFolder = TimeRange.Day.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder);

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                base.baseFile = fileItem.File;
                string[] allLine = File.ReadAllLines(fileItem.File);
                for (int i = 1; i < allLine.Length; i++)
                {
                    allLine[i] = allLine[i].Replace(" 15:00:00", "");
                }

                File.WriteAllLines(fileItem.File, allLine, Encoding.UTF8);

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();
        }

        /// <summary>
        /// 检查数据的正确性
        /// </summary>
        private void CheckRightCd()
        {
            List<string> errorInfo = new List<string>();
            this.CheckRightCd(TimeRange.M5, errorInfo);

            this.CheckRightCd(TimeRange.M15, errorInfo);

            this.CheckRightCd(TimeRange.M30, errorInfo);

            this.CheckRightCd(TimeRange.Day, errorInfo);

            File.WriteAllLines(Consts.BASE_PATH + Consts.CSV_FOLDER + "ErrorCdFile.txt", errorInfo.ToArray(), Encoding.UTF8);
        }

        /// <summary>
        /// 检查数据的正确性
        /// </summary>
        private void CheckRightCd(TimeRange timeRange, List<string> errorInfo)
        {
            // 取得已经存在的所有数据信息
            this.subFolder = timeRange.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(Consts.BASE_PATH + Consts.CSV_FOLDER + this.subFolder);

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                base.baseFile = fileItem.File;
                string stockCd = Util.GetShortNameWithoutType(fileItem.File).Substring(0, 6);
                string[] allLine = File.ReadAllLines(fileItem.File);
                for (int i = 1; i < allLine.Length; i++)
                {
                    if (allLine[i].IndexOf(stockCd) < 0)
                    {
                        errorInfo.Add(fileItem.File);
                        errorInfo.Add(allLine[i]);
                        break;
                    }
                }

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();
        }

        #endregion

        #endregion
    }
}
