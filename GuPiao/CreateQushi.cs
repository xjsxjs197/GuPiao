using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Common;
using DayBatch;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        /// 趋势图X轴间隔的像素
        /// </summary>
        private const int IMG_X_STEP = 5;

        /// <summary>
        /// 图片的高度
        /// </summary>
        private const int IMG_H = 400;

        /// <summary>
        /// 数据路径信息
        /// </summary>
        private const string CSV_FOLDER = @"./Data/";

        /// <summary>
        /// 图片路径信息
        /// </summary>
        private const string IMG_FOLDER = @"./PngImg/";

        /// <summary>
        /// 趋势过滤结果路径信息
        /// </summary>
        private const string RESULT_FOLDER = @"./ChkResult/";

        /// <summary>
        /// 买卖点目录
        /// </summary>
        private const string BUY_SELL_POINT = @"./BuySellPoint/";

        /// <summary>
        /// 天数据的目录
        /// </summary>
        private const string DAY_FOLDER = @"Day/";

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
        /// 所有数据信息
        /// </summary>
        private List<BaseDataInfo> allStockCdName = new List<BaseDataInfo>();

        /// <summary>
        /// 所有数据时间（天，5分钟，15分钟，30分钟）
        /// </summary>
        private List<string> allDataDate = new List<string>();

        /// <summary>
        /// 是否触发事件
        /// </summary>
        private bool needRaiseEvent = true;

        /// <summary>
        /// 是否正在拖拽图片
        /// </summary>
        private bool dragImg = false;

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

        #endregion

        #region " 初始化 "

        /// <summary>
        /// 初始化
        /// </summary>
        public CreateQushi()
        {
            InitializeComponent();

            this.subFolder = DAY_FOLDER;
            this.cmbCon.SelectedIndex = 0;
            this.pnlBody.BackColor = Color.FromArgb(199, 237, 204);

            // 重新设置页面高度
            this.ResetPageHeight();

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

            // 实例化一个和窗口一样大的位图
            Bitmap imgBk = new Bitmap(this.imgBody.Image.Width, this.imgBody.Image.Height);
            // 创建位图的gdi对象
            Graphics g = Graphics.FromImage(imgBk);
            // 创建画笔
            Pen p = new Pen(Color.Blue, 1.0f);
            // 指定线条的样式为划线段
            p.DashStyle = DashStyle.Dash;
            // 根据当前位置，画横线和竖线
            g.DrawLine(p, 0, e.Y, imgBk.Width - 1, e.Y);
            g.DrawLine(p, e.X, 0, e.X, imgBk.Height - 1);

            // 将位图贴到窗口上
            this.imgBody.BackgroundImage = imgBk;
            // 释放gid和pen资源
            g.Dispose();
            p.Dispose();

            // 显示当前位置的数据信息
            this.DisplayCurDayInfo(e.X);

            // 拖动图片处理
            if (this.dragImg)
            {
                this.DragImg(e.X - e.X % IMG_X_STEP);
            }
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

            // 重新设置当前显示
            this.ResetDisplay();

            QushiBase chkQushi = null;
            string selectedText = this.cmbCon.SelectedItem as string;
            switch (selectedText)
            {
                case "下跌递减":
                    this.Do(this.ThreadChkQushi, new ChkDownDecreaseQushi(), this.cmbCon, selectedText);
                    break;

                case "下跌转折":
                    this.Do(this.ThreadChkQushi, new ChkDownBreakQushi(), this.cmbCon, selectedText);
                    break;

                case "下跌转折两天":
                    chkQushi = new ChkDownBreakDaysQushi();
                    chkQushi.SetCheckDays(2);
                    this.Do(this.ThreadChkQushi, chkQushi, this.cmbCon, selectedText);
                    break;

                case "下跌转折三天":
                    chkQushi = new ChkDownBreakDaysQushi();
                    chkQushi.SetCheckDays(3);
                    this.Do(this.ThreadChkQushi, chkQushi, this.cmbCon, selectedText);
                    break;

                case "上涨转折":
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

                case "显示日线":
                    this.subFolder = DAY_FOLDER;
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                case "显示5分钟线":
                    this.subFolder = TimeRange.M5.ToString() + "/";
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                case "显示15分钟线":
                    this.subFolder = TimeRange.M15.ToString() + "/";
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                case "显示30分钟线":
                    this.subFolder = TimeRange.M30.ToString() + "/";
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;

                default:
                    // 显示所有信息
                    this.subFolder = DAY_FOLDER;
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
            if (e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
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
                        timeRange = TimeRange.M15;
                        break;

                    case TimeRange.M15:
                        timeRange = TimeRange.M5;
                        break;

                    case TimeRange.M5:
                        timeRange = TimeRange.Day;
                        break;
                }

                this.btnChgTime.Text = timeRange.ToString();

                // 切换趋势图级别
                if (this.imgBody.Image != null)
                {
                    this.imgBody.Image.Dispose();
                    this.imgBody.Image = null;
                }
                this.subFolder = timeRange.ToString() + "/";
                this.posFromRight = 0;
                this.RedrawQushiImg(IMG_FOLDER + this.subFolder + this.allStock[this.curIdx] + ".png");

                // 设置当前的数据时间
                this.dataDate = this.GetDataDate(this.subFolder);

                // 设置当前数据
                this.SetCurStockData(this.allStock[this.curIdx] + "_" + this.dataDate);
            }
        }

        /// <summary>
        /// 处理图片拖拽开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgBody_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.dragImg = true;
                this.oldImgX = e.X - e.X % IMG_X_STEP;
            }
        }

        /// <summary>
        /// 处理图片拖拽结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgBody_MouseUp(object sender, MouseEventArgs e)
        {
            this.dragImg = false;
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// 试运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestRun_Click(object sender, EventArgs e)
        {
            this.Do(this.StartTestRun);
            //this.Do(this.CheckData);
            //this.Do(this.CheckAllCd);
            //this.Do(this.CheckRightCd);
            //this.Do(this.ReplaceDayData);
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

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + DAY_FOLDER);

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
                if (NO_CHUANGYE && this.IsChuangyeStock(shortName))
                {
                    continue;
                }

                if (this.ChkQushi(shortName, chkQushi))
                {
                    this.allStock.Add(shortName.Substring(0, 6));
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
        private bool ChkQushi(string stockCdData, QushiBase chkQushi)
        {
            // 获得数据信息
            this.subFolder = DAY_FOLDER;
            Dictionary<string, object> dataInfo = DayBatchProcess.GetStockInfo(stockCdData, this.subFolder, "./");
            if (dataInfo == null)
            {
                return false;
            }

            return chkQushi.StartCheck((List<BaseDataInfo>)dataInfo["stockInfos"]);
        }

        #endregion

        #region " 画面显示相关 "

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
        private void DisplayCurDayInfo(int x)
        {
            x -= x % IMG_X_STEP;
            int pos = (int)((this.posFromRight + (this.imgBody.Image.Width - x - IMG_X_STEP)) / IMG_X_STEP);
            if (pos >= 0 && pos <= this.curStockData.Count - 1)
            {
                this.SetTitle(this.allStock[this.curIdx], pos);
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
            List<FilePosInfo> allImg = Util.GetAllFiles(IMG_FOLDER + this.subFolder);

            this.allStock.Clear();

            foreach (FilePosInfo fileItem in allImg)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                string shortName = Util.GetShortNameWithoutType(fileItem.File);
                if (NO_CHUANGYE && this.IsChuangyeStock(shortName))
                {
                    continue;
                }

                this.allStock.Add(shortName);
            }

            // 重新显示当前信息
            this.ReDisplayStockInfo(0);

            // 重置下拉框选中项
            this.needRaiseEvent = false;
            if (this.subFolder.Equals(DAY_FOLDER))
            {
                this.cmbCon.SelectedIndex = 0;
            }
            else if (this.subFolder.Equals(TimeRange.M5.ToString() + "/"))
            {
                this.cmbCon.SelectedIndex = 1;
            }
            else if (this.subFolder.Equals(TimeRange.M15.ToString() + "/"))
            {
                this.cmbCon.SelectedIndex = 2;
            }
            else if (this.subFolder.Equals(TimeRange.M30.ToString() + "/"))
            {
                this.cmbCon.SelectedIndex = 3;
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
                if (NO_CHUANGYE && this.IsChuangyeStock(stockDate.Key))
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
                string stockImg = IMG_FOLDER + this.subFolder + this.allStock[this.curIdx] + ".png";
                this.posFromRight = 0;
                this.RedrawQushiImg(stockImg);

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
            BaseDataInfo findItem = this.allStockCdName.FirstOrDefault(p => p.Code.Equals(stockCd));
            if (findItem != null && findItem.Code.Equals(stockCd))
            {
                this.curStockName = findItem.Name;
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

            if (this.subFolder == DAY_FOLDER)
            {
                // 取得5日级别信息
                this.curStockJibie5Data = DayBatchProcess.GetJibieStockInfo(this.curStockData, 5);

                // 取得10日级别信息
                this.curStockJibie10Data = DayBatchProcess.GetJibieStockInfo(this.curStockData, 10);
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
            if (this.imgBody.Height != IMG_H)
            {
                int diff = this.imgBody.Height - IMG_H;
                this.Height -= diff;
            }
        }

        /// <summary>
        /// 重新设置显示的趋势图
        /// </summary>
        /// <param name="imgFile"></param>
        private void RedrawQushiImg(string imgFile)
        {
            if (!File.Exists(imgFile))
            {
                return;
            }

            Image imgFrom = Image.FromFile(imgFile);
            Bitmap imgTo = new Bitmap(this.imgBody.Width, IMG_H);
            Graphics grp = Graphics.FromImage(imgTo);
            this.oldImgWidth = imgFrom.Width;
            int srcImgX = imgFrom.Width - this.posFromRight;
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

            if (this.posFromRight % IMG_X_STEP == 0)
            {
                this.RedrawQushiImg(IMG_FOLDER + this.subFolder + this.allStock[this.curIdx] + ".png");
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
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + subFolder).Where(p => !p.IsFolder).ToList();
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
            string csvFile = CSV_FOLDER + DAY_FOLDER + stockCd + "_" + this.dataDate + ".csv";
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

            StringBuilder sb = new StringBuilder();
            foreach (string shortName in this.allStock)
            {
                string[] allLine = this.GetStockFileContent(shortName);
                if (allLine != null && allLine.Length > 2)
                {
                    sb.Append(shortName).Append(" ");

                    string[] lineData = allLine[1].Split(',');
                    sb.Append(lineData[2]).Append(" ").Append(lineData[3]).Append("\r\n");
                }
            }

            File.WriteAllText(RESULT_FOLDER + fileName + ".txt", sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 读取包括买点信息的数据
        /// </summary>
        private void LoadHasBuyPointsInfo()
        {
            string file = RESULT_FOLDER + "BuyPoints.txt";
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

            File.WriteAllLines(RESULT_FOLDER + "BuyPoints.txt", tmpList.ToArray(), Encoding.UTF8);
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
        /// 是否是创业板数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        private bool IsChuangyeStock(string stockCd)
        {
            return stockCd.StartsWith("300");
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

            string[] allLine = File.ReadAllLines(CSV_FOLDER + "AllStockInfo.txt", Encoding.UTF8);
            if (allLine != null && allLine.Length > 0)
            {
                foreach (string codeName in allLine)
                {
                    if (string.IsNullOrEmpty(codeName))
                    {
                        continue;
                    }

                    BaseDataInfo item = new BaseDataInfo();
                    item.Code = codeName.Substring(0, 6);
                    item.Name = codeName.Substring(7);
                    this.allStockCdName.Add(item);

                    allCd.Add(item.Code);
                }
            }

            return allCd;
        }

        #endregion

        #region " 测试模块 "

        /// <summary>
        /// 修正天的数据
        /// </summary>
        private void ReplaceDayData()
        {
            // 取得已经存在的所有数据信息
            this.subFolder = TimeRange.Day.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + this.subFolder);

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

            File.WriteAllLines(CSV_FOLDER + "ErrorCdFile.txt", errorInfo.ToArray(), Encoding.UTF8);
        }

        /// <summary>
        /// 检查数据的正确性
        /// </summary>
        private void CheckRightCd(TimeRange timeRange, List<string> errorInfo)
        {
            // 取得已经存在的所有数据信息
            this.subFolder = timeRange.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + this.subFolder);

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

        /// <summary>
        /// 检查所有可用的代码
        /// </summary>
        private void CheckAllCd()
        {
            List<string> allAvailableCd = new List<string>();
            DateTime dt = Util.GetAvailableDt();
            string endDay = dt.AddDays(-1).ToString("yyyyMMdd");

            for (int i = 1; i <= 3000; i++)
            {
                //this.CheckAvailableCdSina(i, allAvailableCd, endDay);
                this.CheckAvailableCd163(i, allAvailableCd, endDay);
            }

            for (int i = 300001; i <= 300999; i++)
            {
                //this.CheckAvailableCdSina(i, allAvailableCd, endDay);
                this.CheckAvailableCd163(i, allAvailableCd, endDay);
            }

            for (int i = 600000; i <= 603999; i++)
            {
                //this.CheckAvailableCdSina(i, allAvailableCd, endDay);
                this.CheckAvailableCd163(i, allAvailableCd, endDay);
            }

            File.WriteAllLines(CSV_FOLDER + "AllStockInfo.txt", allAvailableCd.ToArray(), Encoding.UTF8);
        }

        /// <summary>
        /// 检查可用的代码
        /// </summary>
        /// <param name="stockCd"></param>
        private string CheckAvailableCd163(int stockCd, List<string> allAvailableCd, string endDay)
        {
            string strCd = stockCd.ToString().PadLeft(6, '0');
            // 判断类型
            if (strCd.StartsWith("6"))
            {
                strCd = "0" + strCd;
            }
            else
            {
                strCd = "1" + strCd;
            }

            Encoding encoding = Encoding.GetEncoding("GBK");
            string result = string.Empty;

            try
            {
                result = Util.HttpGet(@"http://quotes.money.163.com/service/chddata.html?fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;&end=" + endDay + "&code="
                    + strCd + "&start=20190101", "", encoding);
            }
            catch (Exception e)
            {
                return "取得 " + strCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace;
            }

            if (!string.IsNullOrEmpty(result) && !"null".Equals(result, System.StringComparison.OrdinalIgnoreCase))
            {
                string[] lines = null;
                string[] lastRow = null;
                try
                {
                    lines = result.Split('\n');
                    if (lines.Length > 2)
                    {
                        lastRow = lines[1].Split(',');
                    }
                }
                catch (Exception e)
                {
                    return "处理Json " + strCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace;
                }

                if (lastRow != null && lastRow.Length > 2)
                {
                    // 取得最新的一条数据
                    string lastDay = lastRow[0].Replace("-", "").Replace("/", "");

                    if (endDay.Equals(lastDay))
                    {
                        allAvailableCd.Add(stockCd.ToString().PadLeft(6, '0') + " " + lastRow[2]);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 检查可用的代码
        /// </summary>
        /// <param name="stockCd"></param>
        private string CheckAvailableCdSina(int stockCd, List<string> allAvailableCd, string endDay)
        {
            string strCd = stockCd.ToString().PadLeft(6, '0');
            // 判断类型
            if (strCd.StartsWith("6"))
            {
                strCd = "sh" + strCd;
            }
            else
            {
                strCd = "sz" + strCd;
            }

            Encoding encoding = Encoding.GetEncoding("GBK");
            string result = string.Empty;

            try
            {
                result = Util.HttpGet(@"http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol="
                    + strCd + "&scale=240", "", encoding);
            }
            catch (Exception e)
            {
                return "取得 " + strCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace;
            }

            if (!string.IsNullOrEmpty(result) && !"null".Equals(result, System.StringComparison.OrdinalIgnoreCase))
            {
                JArray jArray = null;
                try
                {
                    jArray = (JArray)JsonConvert.DeserializeObject(result);
                }
                catch (Exception e)
                {
                    return "处理Json " + strCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace;
                }

                if (jArray != null && jArray.Count > 0)
                {
                    // 取得最新的一条数据
                    string lastDay = jArray[jArray.Count - 1]["day"].ToString().Replace("-", "").Replace(" ", "").Replace(":", "");
                    if (endDay.Equals(lastDay))
                    {
                        allAvailableCd.Add(stockCd.ToString().PadLeft(6, '0'));
                    }
                }
            }

            Thread.Sleep(1000);

            return string.Empty;
        }

        /// <summary>
        /// 检查数据的正确性
        /// </summary>
        private void CheckData()
        {
            DateTime dt = Util.GetAvailableDt();
            List<string> allCd = GetAllStockBaseInfo();

            //this.CheckCsvData(TimeRange.M15, dt.ToString("yyyyMMdd150000"), allCd);

            this.CheckCsvData(TimeRange.Day, dt.ToString("yyyyMMdd"), allCd);
            this.CheckCsvData(TimeRange.M30, dt.ToString("yyyyMMdd150000"), allCd);
            this.CheckCsvData(TimeRange.M5, dt.ToString("yyyyMMdd150000"), allCd);
        }

        /// <summary>
        /// 检查数据
        /// </summary>
        /// <param name="timeRange"></param>
        private void CheckCsvData(TimeRange timeRange, string dt, List<string> allCd)
        {
            // 取得已经存在的所有数据信息
            this.subFolder = timeRange.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + this.subFolder);

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
                if (!allCd.Contains(stockCd))
                {
                    File.Delete(fileItem.File);
                }

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();
        }

        /// <summary>
        /// 试运行
        /// </summary>
        private void StartTestRun()
        {
            // 取得已经存在的所有数据信息
            this.subFolder = TimeRange.M30.ToString() + "/";
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + this.subFolder);
            Dictionary<string, List<string>[]> buySellInfo = new Dictionary<string, List<string>[]>();
            StringBuilder notGoodSb = new StringBuilder();
            StringBuilder goodSb = new StringBuilder();

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                base.baseFile = fileItem.File;
                string stockCdDate = Util.GetShortNameWithoutType(fileItem.File);
                if (NO_CHUANGYE && this.IsChuangyeStock(stockCdDate))
                {
                    continue;
                }

                // 测试BuySell的逻辑
                this.CheckBuySellPoint(stockCdDate, buySellInfo, notGoodSb, goodSb);

                // 更新进度条
                this.ProcessBarStep();
            }
            
            // 关闭进度条
            this.CloseProcessBar();

            this.SaveTotalBuySellInfo(allCsv, buySellInfo, notGoodSb, goodSb);
        }

        /// <summary>
        /// 测试买卖点的逻辑
        /// </summary>
        /// <param name="stockCdDate"></param>
        private void CheckBuySellPoint(string stockCdDate, Dictionary<string, List<string>[]> buySellInfo,
            StringBuilder notGoodSb, StringBuilder goodSb)
        {
            // 获得数据信息
            Dictionary<string, object> dataInfo = DayBatchProcess.GetStockInfo(stockCdDate, this.subFolder, "./");
            if (dataInfo == null)
            {
                return;
            }

            List<BaseDataInfo> stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
            if (stockInfos.Count == 0)
            {
                return;
            }

            // 设置测试的开始时间
            string startDate;
            int startIdx = 0;
            if (this.subFolder.Equals(DAY_FOLDER))
            {
                startDate = DateTime.Now.AddDays(-60).ToString("yyyyMMdd");
            }
            else
            {
                startDate = DateTime.Now.AddDays(-60).ToString("yyyyMMdd090000");
            }

            // 取得分型的数据
            List<BaseDataInfo> fenxingInfo = DayBatchProcess.SetFenxingInfoDayM30(stockInfos);
            for (int i = 0; i < fenxingInfo.Count; i++)
            {
                if (string.Compare(fenxingInfo[i].Day, startDate) < 0)
                {
                    startIdx = i;
                    break;
                }
            }

            if (startIdx == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            string stockCd = stockCdDate.Substring(0, 6);
            decimal buyPrice = 0;
            bool buyed = false;
            List<string> buyInfo;
            List<string> sellInfo;
            for (int i = startIdx; i >= 0; i--)
            {
                if (fenxingInfo[i].BuySellFlg < 0 && buyed)
                {
                    buyed = false;
                    sb.Append(fenxingInfo[i].Day).Append(" ");
                    sb.Append(fenxingInfo[i].DayVal.ToString().PadLeft(8, ' ')).Append(" ");
                    decimal diff = ((fenxingInfo[i].DayVal / buyPrice) - 1) * 100;
                    sb.Append(diff.ToString("0.00").PadLeft(7, ' ')).Append("%\r\n");

                    if (diff < -1)
                    {
                        string[] tmp = sb.ToString().Split('\r');
                        notGoodSb.Append(tmp[tmp.Length - 2]).Append("\r\n");
                    }
                    else if (diff > 1)
                    {
                        string[] tmp = sb.ToString().Split('\r');
                        goodSb.Append(tmp[tmp.Length - 2]).Append("\r\n");
                    }

                    if (!buySellInfo.ContainsKey(fenxingInfo[i].Day))
                    {
                        buyInfo = new List<string>();
                        sellInfo = new List<string>();
                        List<string>[] listBuySell = new List<string>[2];
                        listBuySell[0] = buyInfo;
                        listBuySell[1] = sellInfo;
                        buySellInfo.Add(fenxingInfo[i].Day, listBuySell);
                    }
                    else
                    {
                        List<string>[] listBuySell = buySellInfo[fenxingInfo[i].Day];
                        buyInfo = listBuySell[0];
                        sellInfo = listBuySell[1];
                    }
                    //sellInfo.Add(stockCd + "(" + fenxingInfo[i].DayVal + "(" + diff.ToString("0.00") + "%))");
                    sellInfo.Add(stockCd + " " + fenxingInfo[i].DayVal);
                }
                else if (fenxingInfo[i].BuySellFlg > 0 && !buyed)
                {
                    //decimal befBottomVal = DayBatchProcess.GeBefBottomVal(fenxingInfo, i, startIdx);
                    //if (!buyed && befBottomVal != 0 && fenxingInfo[i].DayVal > befBottomVal * Consts.LIMIT_VAL)
                    {
                        buyed = true;
                        buyPrice = fenxingInfo[i].DayVal;
                        sb.Append(stockCd).Append(" ");
                        sb.Append(fenxingInfo[i].Day).Append(" ");
                        sb.Append(buyPrice.ToString().PadLeft(8, ' ')).Append(" ");

                        if (!buySellInfo.ContainsKey(fenxingInfo[i].Day))
                        {
                            buyInfo = new List<string>();
                            sellInfo = new List<string>();
                            List<string>[] listBuySell = new List<string>[2];
                            listBuySell[0] = buyInfo;
                            listBuySell[1] = sellInfo;
                            buySellInfo.Add(fenxingInfo[i].Day, listBuySell);
                        }
                        else
                        {
                            List<string>[] listBuySell = buySellInfo[fenxingInfo[i].Day];
                            buyInfo = listBuySell[0];
                            sellInfo = listBuySell[1];
                        }
                        //buyInfo.Add(stockCd + "(" + buyPrice + ")");
                        buyInfo.Add(stockCd + " " + buyPrice);
                    }
                }
            }

            if (buyed)
            {
                sb.Append("\r\n");
            }

            if (sb.Length > 0)
            {
                File.WriteAllText(BUY_SELL_POINT + stockCd + ".txt", sb.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// 保存买卖信息
        /// </summary>
        /// <param name="buySellInfo"></param>
        private void SaveTotalBuySellInfo(List<FilePosInfo> allCsv, Dictionary<string, List<string>[]> buySellInfo,
            StringBuilder notGoodSb, StringBuilder goodSb)
        {
            List<string> dayList = new List<string>(buySellInfo.Keys);
            dayList.Sort();

            int buyThread = 10;
            decimal threadMoney = 1000;
            List<string> buyedStock = new List<string>();
            List<Dictionary<string, object>> buySellHst = new List<Dictionary<string, object>>();
            Dictionary<string, object> buySellItem;
            StringBuilder sb;
            decimal diff;
            while (buyThread-- > 0)
            {
                buySellItem = new Dictionary<string, object>();
                buySellItem.Add("stockCd", string.Empty);
                buySellItem.Add("status", string.Empty);
                buySellItem.Add("price", (decimal)0);
                buySellItem.Add("buyCount", (decimal)0);
                buySellItem.Add("buyMoney", (decimal)0);
                buySellItem.Add("TotalMoney", threadMoney);

                sb = new StringBuilder();
                sb.Append("Thread").Append(buyThread).Append(" ");
                sb.Append("Start Total: " + threadMoney).Append("\r\n");
                buySellItem.Add("logBuf", sb);

                buySellHst.Add(buySellItem);
            }

            foreach (string day in dayList)
            {
                List<string> buyInfo = buySellInfo[day][0];
                List<string> sellInfo = buySellInfo[day][1];
                buyInfo.Reverse();
                buyedStock.Clear();

                foreach (Dictionary<string, object> bsp in buySellHst)
                {
                    if (!"B".Equals(bsp["status"] as string))
                    {
                        foreach (string bp in buyInfo)
                        {
                            string[] tmp = bp.Split(' ');
                            decimal price = decimal.Parse(tmp[1]);

                            if (!buyedStock.Contains(tmp[0]))
                            {
                                decimal totalMoney = (decimal)bsp["TotalMoney"];
                                int canBuyCnt = this.CanBuyCount(totalMoney, price);
                                if (canBuyCnt > 0)
                                {
                                    buyedStock.Add(tmp[0]);

                                    bsp["stockCd"] = tmp[0];
                                    bsp["status"] = "B";
                                    bsp["price"] = price;
                                    bsp["buyCount"] = (decimal)(canBuyCnt * 100);
                                    bsp["buyMoney"] = (decimal)bsp["buyCount"] * price + 5;
                                    bsp["TotalMoney"] = (decimal)bsp["TotalMoney"] - (decimal)bsp["buyMoney"];

                                    sb = (StringBuilder)bsp["logBuf"];
                                    sb.Append(day).Append(" B ").Append(tmp[0]).Append(" ");
                                    sb.Append(price.ToString().PadLeft(8, ' ')).Append(" ");
                                    sb.Append(bsp["buyCount"].ToString().PadLeft(4, ' ')).Append("\r\n");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (string sp in sellInfo)
                        {
                            if (sp.StartsWith(bsp["stockCd"] as string, StringComparison.OrdinalIgnoreCase))
                            {
                                string[] tmp = sp.Split(' ');
                                decimal price = decimal.Parse(tmp[1]);
                                decimal sellMoney = (decimal)bsp["buyCount"] * price;

                                buyedStock.Remove(tmp[0]);

                                bsp["status"] = "S";
                                bsp["TotalMoney"] = (decimal)bsp["TotalMoney"] + sellMoney;

                                diff = ((sellMoney / (decimal)bsp["buyMoney"]) - 1) * 100;
                                sb = (StringBuilder)bsp["logBuf"];
                                sb.Append(day).Append(" S ").Append(bsp["stockCd"]).Append(" ");
                                sb.Append(price.ToString().PadLeft(8, ' ')).Append(" ");
                                sb.Append(bsp["buyCount"].ToString().PadLeft(4, ' ')).Append(" ");
                                sb.Append(diff.ToString("0.00")).Append(" ").Append(bsp["TotalMoney"]).Append("(");
                                diff = ((decimal)bsp["TotalMoney"] / threadMoney - 1) * 100;
                                sb.Append(diff.ToString("0.00")).Append(")");
                                sb.Append("\r\n");
                                break;
                            }
                        }
                    }
                }
            }

            StringBuilder sbAll = new StringBuilder();
            decimal totalAll = 0;

            foreach (Dictionary<string, object> bsp in buySellHst)
            {
                sb = (StringBuilder)bsp["logBuf"];
                decimal total = (decimal)bsp["TotalMoney"];
                sb.Append("End Total: ");

                if ("B".Equals(bsp["status"]))
                {
                    FilePosInfo lastInfo = allCsv.FirstOrDefault(p => p.File.IndexOf(bsp["stockCd"] as string) > 0);
                    // 获得数据信息
                    Dictionary<string, object> dataInfo = DayBatchProcess.GetStockInfo(
                        Util.GetShortNameWithoutType(lastInfo.File), this.subFolder, "./");
                    if (dataInfo != null)
                    {
                        List<BaseDataInfo> stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];
                        if (stockInfos.Count > 0)
                        {
                            total += (decimal)bsp["buyCount"] * stockInfos[0].DayVal;
                        }
                    }
                }
                totalAll += total;
                sb.Append(total).Append(" ");
                diff = (total / threadMoney - 1) * 100;
                sb.Append(diff.ToString("0.00")).Append("%");

                sbAll.Append(sb.ToString()).Append("\r\n\r\n");
            }
            sbAll.Append("Total : ").Append(threadMoney * buySellHst.Count).Append(" ");
            diff = (totalAll / (threadMoney * buySellHst.Count) - 1) * 100;
            sbAll.Append(totalAll).Append(" ").Append(diff.ToString("0.00")).Append("%\r\n\r\n");

            File.WriteAllText(BUY_SELL_POINT + "TotalBuySellInfo.txt", sbAll.ToString(), Encoding.UTF8);

            File.WriteAllText(BUY_SELL_POINT + "BadBuySellPoint.txt", notGoodSb.ToString(), Encoding.UTF8);

            File.WriteAllText(BUY_SELL_POINT + "GoodBuySellPoint.txt", goodSb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 可以买多少数量的取得
        /// </summary>
        /// <param name="money"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private int CanBuyCount(decimal money, decimal price)
        {
            return (int)((money - 5) / (price * 100));
        }

        #endregion

        #endregion

    }
}
