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
using DataProcess.GetData;
using GuPiao.Common;

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

            this.oldPnlBodyHeight = this.pnlBody.Height;
            this.Height -= this.oldPnlBodyHeight;

            this.cmbCon.SelectedIndexChanged += new EventHandler(this.cmbCon_SelectedIndexChanged);

            // 绑定子菜单事件
            this.SetSubMenuEvent();

            //获取UI线程同步上下文
            this.mSyncContext = SynchronizationContext.Current;

            // 设置当前的数据时间
            this.SetDataDate();

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
            this.DisplayCurDayInfo(e.X, imgBk.Width);
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

                default:
                    // 显示所有信息
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
                    this.Do(this.ThreadGetMinuteData, TimeRange.M5);
                    break;

                case "get15M":
                    this.Do(this.ThreadGetMinuteData, TimeRange.M15);
                    break;

                case "get30M":
                    this.Do(this.ThreadGetMinuteData, TimeRange.M30);
                    break;

                case "getDay":
                    this.Do(this.ThreadGetAllDayData);
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
                    this.Do(this.ThreadDrawMinuteQushi, TimeRange.M5);
                    break;

                case "drawM15":
                    this.Do(this.ThreadDrawMinuteQushi, TimeRange.M15);
                    break;

                case "drawM30":
                    this.Do(this.ThreadDrawMinuteQushi, TimeRange.M30);
                    break;

                case "drawDay":
                    this.Do(this.ThreadDrawDayQushi);
                    break;
            }
        }

        #endregion

        #region " 公有方法 "
        #endregion

        #region " 私有方法 "

        #region " 各种数据处理 "

        /// <summary>
        /// 多线程取得所有分钟级别的数据
        /// </summary>
        private void ThreadGetMinuteData(params object[] param)
        {
            TimeRange timeRange = (TimeRange)param[0];

            // 设定结束日期
            DateTime now = DateTime.Now;
            string endDay;
            if (DateTime.Now.Hour > 15)
            {
                endDay = now.ToString("yyyy-MM-dd 15:00:00");
            }
            else if (DateTime.Now.Hour < 9)
            {
                endDay = now.AddDays(-1).ToString("yyyy-MM-dd 15:00:00");
            }
            else
            {
                endDay = now.ToString("yyyy-MM-dd HH:mm:00");
            }

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + timeRange.ToString() + "/");

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFromSina(CSV_FOLDER, endDay, timeRange);
            List<string> allStock = getData.Before();

            // 设置进度条
            this.ResetProcessBar(allStock.Count);

            // 循环取得所有的数据
            foreach (string stockCd in allStock)
            {
                // 取得当前Stock数据
                getData.Start(stockCd, allCsv);

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();

            // 获取数据后的相关处理
            getData.After();

            // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, this.btnGetAllStock);

            // 重新设置结束时间
            this.dataDate = endDay.Replace("-", "").Replace(" ", "").Replace(":", "");
        }

        /// <summary>
        /// 多线程取得所有整天的数据
        /// </summary>
        private void ThreadGetAllDayData()
        {
            // 设定结束日期
            string endDay = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + DAY_FOLDER);

            // 获取所有的代码信息
            GetDataBase getData = new GetDataFrom163(CSV_FOLDER + DAY_FOLDER, endDay);
            List<string> allStock = getData.Before();

            // 设置进度条
            this.ResetProcessBar(allStock.Count);

            // 循环取得所有的数据
            foreach (string stockCd in allStock)
            {
                // 取得当前Stock数据
                getData.Start(stockCd, allCsv);
                
                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();

            // 获取数据后的相关处理
            getData.After();

             // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, this.btnGetAllStock);

            // 重新设置结束时间
            this.dataDate = endDay;
        }

        /// <summary>
        /// 画天级别趋势图
        /// </summary>
        private void ThreadDrawDayQushi()
        {
            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + DAY_FOLDER);
            this.hasBuyPointsStock.Clear();

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                base.baseFile = fileItem.File;

                this.CreateDayQushiImg(Util.GetShortNameWithoutType(fileItem.File));

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();

            // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, this.btnCreate);

            // 保存包括买点信息的数据
            this.SaveHasBuyPointsInfo();

            // 显示所有信息
            this.mSyncContext.Post(this.DisplayAllStockPng, null);
        }

        /// <summary>
        /// 画分钟级别趋势图
        /// </summary>
        private void ThreadDrawMinuteQushi(params object[] param)
        {
            TimeRange timeRange = (TimeRange)param[0];
            string minuteFolder = timeRange.ToString() + "/";

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + minuteFolder);
            //this.hasBuyPointsStock.Clear();

            // 设置进度条
            this.ResetProcessBar(allCsv.Count);

            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                base.baseFile = fileItem.File;

                this.CreateMinuteQushiImg(Util.GetShortNameWithoutType(fileItem.File), minuteFolder);

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();

            // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, this.btnCreate);

            //// 保存包括买点信息的数据
            //this.SaveHasBuyPointsInfo();

            // 显示所有信息
            this.mSyncContext.Post(this.DisplayAllStockPng, null);
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
            Dictionary<string, object> dataInfo = this.GetStockInfo(stockCdData);
            if (dataInfo == null)
            {
                return false;
            }

            return chkQushi.StartCheck((List<BaseDataInfo>)dataInfo["stockInfos"]);
        }

        /// <summary>
        /// 根据StockCd取得相关数据信息
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetStockInfo(string stockCdData)
        {
            // 读取所有信息
            List<BaseDataInfo> stockInfos = this.GetStockHistoryInfo(CSV_FOLDER + this.subFolder + stockCdData + ".csv");
            if (stockInfos.Count == 0)
            {
                return null;
            }

            // 取得最大、最小值
            // 期间还处理了一下0，等于前一天的值
            decimal[] minMaxInfo = this.GetMaxMinStock(stockInfos);
            if (minMaxInfo[0] == 0 || minMaxInfo[1] == 0 || (minMaxInfo[1] - minMaxInfo[0]) == 0 || stockInfos.Count == 0)
            {
                return null;
            }

            Dictionary<string, object> dicRet = new Dictionary<string, object>();
            dicRet.Add("stockInfos", stockInfos);
            dicRet.Add("minMaxInfo", minMaxInfo);

            return dicRet;
        }

        /// <summary>
        /// 画天级别趋势图
        /// </summary>
        /// <param name="stockCdData"></param>
        private void CreateDayQushiImg(string stockCdData)
        {
            // 获得数据信息
            this.subFolder = DAY_FOLDER;
            Dictionary<string, object> dataInfo = this.GetStockInfo(stockCdData);
            if (dataInfo == null)
            {
                return;
            }

            // 基础数据信息
            List<BaseDataInfo> stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];

            // 最大、最小值信息
            decimal[] minMaxInfo = (decimal[])dataInfo["minMaxInfo"];
            decimal step = 370 / (minMaxInfo[1] - minMaxInfo[0]);

            // 设定图片
            Bitmap imgQushi = new Bitmap(600, 400);
            Graphics grp = Graphics.FromImage(imgQushi);
            grp.SmoothingMode = SmoothingMode.AntiAlias;
            grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 开始画日线
            this.DrawStockQushi(stockInfos, step, minMaxInfo[0], imgQushi, new Pen(Color.Black, 1F), grp);

            // 取得5日级别信息
            List<BaseDataInfo> stockInfo5Jibie = this.GetJibieStockInfo(stockInfos, 5);

            // 开始画5日线
            if (stockInfo5Jibie.Count > 0)
            {
                this.DrawStockQushi(stockInfo5Jibie, step, minMaxInfo[0], imgQushi, new Pen(Color.Green, 1F), grp);
            }

            /*
            // 取得10日级别信息
            List<BaseDataInfo> stockInfo10Jibie = this.GetJibieStockInfo(stockInfos, 10);

            // 开始画10日线
            if (stockInfo10Jibie.Count > 0)
            {
                this.DrawStockQushi(stockInfo10Jibie, step, minMaxInfo[0], imgQushi, new Pen(Color.Red, 1F), grp);
            }

            // 趋势图上画买卖点
            string tmpDate = this.dataDate;
            bool hasBuyPoint = this.DrawStockBuySellPoint(stockInfos, stockInfo5Jibie, step, minMaxInfo[0], imgQushi, grp);
            if (hasBuyPoint)
            {
                this.hasBuyPointsStock.Add(new KeyValuePair<string, string>(stockCdData.Substring(0, 6), this.dataDate));
                this.dataDate = tmpDate;
            }*/

            // 开始分型笔的线段
            List<BaseDataInfo> fenXingInfo = this.GetFenxingPenInfo(stockInfos);
            this.ResetFenxingPenInfo(fenXingInfo);
            List<BaseDataInfo> fenXingInfo5 = this.GetFenxingPenInfo(stockInfo5Jibie);
            this.ResetFenxingPenInfo(fenXingInfo5);
            fenXingInfo.Reverse();
            fenXingInfo5.Reverse();
            string tmpDate = this.dataDate;
            bool hasBuyPoint = this.DrawFenxingPen(fenXingInfo, fenXingInfo5, step, minMaxInfo[0], imgQushi, new Pen(Color.DarkOrange, 1F), grp);
            if (hasBuyPoint)
            {
                this.hasBuyPointsStock.Add(new KeyValuePair<string, string>(stockCdData.Substring(0, 6), this.dataDate));
                this.dataDate = tmpDate;
            }

            // 保存图片
            imgQushi.Save(IMG_FOLDER + this.subFolder + stockCdData.Substring(0, 6) + ".png");
        }

        /// <summary>
        /// 画分钟级别趋势图
        /// </summary>
        /// <param name="stockCdData"></param>
        private void CreateMinuteQushiImg(string stockCdData, string minuteFolder)
        {
            // 获得数据信息
            this.subFolder = minuteFolder;
            Dictionary<string, object> dataInfo = this.GetStockInfo(stockCdData);
            if (dataInfo == null)
            {
                return;
            }

            // 基础数据信息
            List<BaseDataInfo> stockInfos = (List<BaseDataInfo>)dataInfo["stockInfos"];

            // 最大、最小值信息
            decimal[] minMaxInfo = (decimal[])dataInfo["minMaxInfo"];
            decimal step = 370 / (minMaxInfo[1] - minMaxInfo[0]);

            // 设定图片
            Bitmap imgQushi = new Bitmap(600, 400);
            Graphics grp = Graphics.FromImage(imgQushi);
            grp.SmoothingMode = SmoothingMode.AntiAlias;
            grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 开始画分钟线
            this.DrawStockQushi(stockInfos, step, minMaxInfo[0], imgQushi, new Pen(Color.Black, 1F), grp);

            // 开始分型笔的线段
            List<BaseDataInfo> fenXingInfo = this.GetFenxingPenInfo(stockInfos);
            fenXingInfo.Reverse();
            this.ResetFenxingPenInfo(fenXingInfo);
            string tmpDate = this.dataDate;
            bool hasBuyPoint = this.DrawFenxingPen(fenXingInfo, null, step, minMaxInfo[0], imgQushi, new Pen(Color.DarkOrange, 1F), grp);
            if (hasBuyPoint)
            {
                this.dataDate = tmpDate;
            }

            // 保存图片
            imgQushi.Save(IMG_FOLDER + minuteFolder + stockCdData.Substring(0, 6) + ".png");
        }

        /// <summary>
        /// 取得股票历史数据
        /// </summary>
        /// <param name="stockFile"></param>
        /// <returns></returns>
        private List<BaseDataInfo> GetStockHistoryInfo(string stockFile)
        {
            List<BaseDataInfo> stockInfo = new List<BaseDataInfo>();
            if (string.IsNullOrEmpty(stockFile) || !File.Exists(stockFile))
            {
                return stockInfo;
            }

            string[] allLine = File.ReadAllLines(stockFile, Encoding.UTF8);
            for (int i = 1; i < allLine.Length && i <= MAX_DAYS; i++)
            {
                if (allLine[i].IndexOf("Error") > 0)
                {
                    stockInfo.Clear();
                    return stockInfo;
                }

                string[] allItems = allLine[i].Split(',');
                if (allItems.Length > 3)
                {
                    BaseDataInfo dayInfo = new BaseDataInfo();
                    dayInfo.Day = allItems[0].Replace("-", "").Replace(" ", "").Replace(":", "");
                    if (i == 1 && !this.IsValidStock(dayInfo.Day))
                    {
                        stockInfo.Clear();
                        return stockInfo;
                    }
                    dayInfo.DayVal = decimal.Parse(allItems[3]);
                    dayInfo.DayMaxVal = decimal.Parse(allItems[4]);
                    dayInfo.DayMinVal = decimal.Parse(allItems[5]);

                    stockInfo.Add(dayInfo);
                }
            }

            return stockInfo;
        }

        /// <summary>
        /// 取得当前级别的信息
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <param name="jibie">5、10、30等级别</param>
        /// <returns></returns>
        private List<BaseDataInfo> GetJibieStockInfo(List<BaseDataInfo> stockInfos, int jibie)
        {
            List<BaseDataInfo> stockInfo = new List<BaseDataInfo>();

            int index = 0;
            int jibieCount = jibie - 1;
            int maxCount = stockInfos.Count - jibie;
            decimal total = 0;
            decimal minVal = decimal.MaxValue;
            decimal maxVal = 0;

            while (index <= maxCount)
            {
                total = 0;
                minVal = decimal.MaxValue;
                maxVal = 0;
                for (int i = 0; i <= jibieCount; i++)
                {
                    total += stockInfos[index + i].DayVal;
                    maxVal = Math.Max(stockInfos[index + i].DayVal, maxVal);
                    minVal = Math.Max(stockInfos[index + i].DayVal, minVal);
                }

                BaseDataInfo item = new BaseDataInfo();
                item.Day = stockInfos[index].Day;
                item.DayVal = total / jibie;
                item.DayMaxVal = maxVal;
                item.DayMinVal = minVal;

                stockInfo.Add(item);

                index++;
            }

            if (stockInfo.Count > 0)
            {
                while (stockInfo.Count < stockInfos.Count)
                {
                    stockInfo.Add(stockInfo[stockInfo.Count - 1]);
                }
            }

            return stockInfo;
        }

        /// <summary>
        /// 开始画趋势图
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private void DrawStockQushi(List<BaseDataInfo> stockInfo, decimal step, decimal minVal, Bitmap img, Pen pen, Graphics grp)
        {
            int startX = img.Width - IMG_X_STEP;
            int x1 = startX;
            int y1 = this.GetYPos(img.Height, stockInfo[0].DayVal, minVal, step);
            int x2 = 0;
            int y2 = 0;
            int index = 1;
            while (index <= stockInfo.Count - 1)
            {
                x2 = startX - IMG_X_STEP;
                y2 = this.GetYPos(img.Height, stockInfo[index].DayVal, minVal, step);

                grp.DrawLine(pen, x1, y1, x2, y2);
                x1 = x2;
                y1 = y2;

                index++;
                startX -= IMG_X_STEP;
            }
        }

        /// <summary>
        /// 趋势图上画买卖点
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private bool DrawStockBuySellPoint(List<BaseDataInfo> stockInfo, List<BaseDataInfo> jibie5StockInfo, decimal step, decimal minVal, Bitmap img, Graphics grp)
        {
            if (jibie5StockInfo.Count == 0 || jibie5StockInfo.Count != stockInfo.Count)
            {
                return false;
            }

            int curQushi = 0; // 当前趋势，负数：下跌，0：转折，正数：上涨
            Brush buyBush = new SolidBrush(Color.Red);
            Brush sellBush = new SolidBrush(Color.Green);
            Font font = new Font(new FontFamily("Microsoft YaHei"), 6, FontStyle.Bold);
            bool hasBuyPoint = false;
            bool buyed = false;

            for (int i = stockInfo.Count - 2; i >= 0; i--)
            {
                // 当前上涨
                if (stockInfo[i].DayVal > stockInfo[i + 1].DayVal * Consts.LIMIT_VAL)
                {
                    if (curQushi < 0)
                    {
                        // 原来是下跌，如果满足转折条件（连续N天，并且超过5日线）
                        if (-curQushi > Consts.QUSHI_CONTINUE_DAYS && stockInfo[i].DayVal >= jibie5StockInfo[i].DayVal)
                        {
                            // 画买点
                            grp.FillEllipse(buyBush, img.Width - IMG_X_STEP * (i + 3), this.GetYPos(img.Height, stockInfo[i + 1].DayVal, minVal, step), 7, 7);
                            hasBuyPoint = true;
                            buyed = true;
                            this.dataDate = stockInfo[i + 1].Day;
                        }

                        // 趋势标志恢复
                        curQushi = 0;
                    }
                    else
                    {
                        // 原来就是上涨，趋势的标志增大
                        curQushi++;
                    }
                }
                // 当前是下跌
                else if (stockInfo[i].DayVal * Consts.LIMIT_VAL < stockInfo[i + 1].DayVal)
                {
                    if (curQushi > 0)
                    {
                        if (buyed)
                        {
                            // 原来是上涨,并且已经有买点时才画卖点
                            grp.FillEllipse(sellBush, img.Width - IMG_X_STEP * (i + 3), this.GetYPos(img.Height, stockInfo[i + 1].DayVal, minVal, step), 7, 7);
                            buyed = false;
                        }

                        // 趋势标志恢复
                        curQushi = 0;
                    }
                    else
                    {
                        // 原来就是下跌，趋势的标志增大
                        curQushi--;
                    }
                }
            }

            return hasBuyPoint;
        }

        #endregion

        #region " 分型处理相关 "

        /// <summary>
        /// 开始分型笔的线段
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private bool DrawFenxingPen(List<BaseDataInfo> fenXingInfo, List<BaseDataInfo> fenXingInfo5, decimal step, decimal minVal, Bitmap img, Pen pen, Graphics grp)
        {
            if (fenXingInfo.Count == 0)
            {
                return false;
            }

            int x1 = img.Width - (fenXingInfo.Count * IMG_X_STEP);
            int y1 = this.GetYPos(img.Height, fenXingInfo[0].DayVal, minVal, step);
            int x2 = 0;
            int y2 = 0;
            decimal curVal;
            int maxCnt = fenXingInfo.Count - 2;
            Brush buyBush = new SolidBrush(Color.Red);
            Brush sellBush = new SolidBrush(Color.Green);
            Font font = new Font(new FontFamily("Microsoft YaHei"), 6, FontStyle.Bold);
            bool hasBuyPoint = false;
            bool buyed = false;
            decimal buyPrice = 0;

            for (int index = maxCnt; index >= 0; index--)
            {
                if (fenXingInfo[index].CurPointType != PointType.Changing || index == 0)
                {
                    x2 = img.Width - (index + 1) * IMG_X_STEP;
                    //curVal = stockInfo[index].CurPen == PenStatus.DownPen ? stockInfo[index].DayMinVal : stockInfo[index].DayMaxVal;
                    curVal = fenXingInfo[index].DayVal;
                    y2 = this.GetYPos(img.Height, curVal, minVal, step);

                    //if (fenXingInfo[index].CurPen == PenStatus.UpPen)
                    //{
                    //    if (!buyed)
                    //    {
                    //        // 没有买点时画买点
                    //        grp.FillEllipse(buyBush, x2, y2, 7, 7);
                    //        hasBuyPoint = true;
                    //        buyed = true;
                    //        buyPrice = fenXingInfo[index].DayVal;
                    //        this.dataDate = fenXingInfo[index].Day;
                    //    }
                    //}
                    //else if (buyed)
                    //{
                    //    if (fenXingInfo[index].CurPen == PenStatus.DownPen 
                    //        || fenXingInfo[index].DayVal < buyPrice * Consts.SELL_VAL
                    //        || fenXingInfo[index].DayVal < fenXingInfo5[index].DayVal)
                    //    {
                    //        // 已经有买点时才画卖点
                    //        grp.FillEllipse(sellBush, x2, y2, 7, 7);
                    //        buyed = false;
                    //    }
                    //}

                    // 写字用做标识
                    if (fenXingInfo[index].CurPointType == PointType.Top)
                    {
                        grp.DrawString("T", font, sellBush, x2, y2);
                    }
                    else
                    {
                        grp.DrawString("B", font, buyBush, x2, y2);
                    }

                    grp.DrawLine(pen, x1, y1, x2, y2);
                    x1 = x2;
                    y1 = y2;
                }
            }

            return hasBuyPoint;
        }

        /// <summary>
        /// 重新设置分型、笔信息
        /// </summary>
        /// <param name="stockInfo"></param>
        private void ResetFenxingPenInfo(List<BaseDataInfo> fenxingPenInfo)
        {
            // 分型的特殊处理（必须是顶、低交替显示）
            BaseDataInfo lastData = null;
            for (int i = fenxingPenInfo.Count - 1; i >= 0; i--)
            {
                if (fenxingPenInfo[i].CurPointType == PointType.Bottom)
                {
                    if (lastData == null || lastData.CurPointType == PointType.Top)
                    {
                        lastData = fenxingPenInfo[i];
                    }
                    else
                    {
                        // 连续两个底分型，取后一个底，取消前一个底
                        lastData.CurPointType = PointType.Changing;
                        lastData = fenxingPenInfo[i];
                    }
                }
                else if (fenxingPenInfo[i].CurPointType == PointType.Top)
                {
                    if (lastData == null || lastData.CurPointType == PointType.Bottom)
                    {
                        lastData = fenxingPenInfo[i];
                    }
                    else
                    {
                        // 连续两个顶分型，取后一个顶，取消前一个顶
                        lastData.CurPointType = PointType.Changing;
                        lastData = fenxingPenInfo[i];
                    }
                }

                //if (fenxingPenInfo[i].CurPointType == PointType.Bottom
                //    && fenxingPenInfo[i].NextPointType == PointType.Bottom)
                //{
                //    if (lastData == null || lastData.CurPointType == PointType.Top)
                //    {
                //        lastData = fenxingPenInfo[i];
                //    }
                //    else
                //    {
                //        // 连续两个底分型，取后一个底，取消前一个底
                //        lastData.NextPointType = PointType.Changing;
                //    }
                //}
                //else if (fenxingPenInfo[i].CurPointType == PointType.Top
                //    && fenxingPenInfo[i].NextPointType == PointType.Top)
                //{
                //    if (lastData == null || lastData.CurPointType == PointType.Bottom)
                //    {
                //        lastData = fenxingPenInfo[i];
                //    }
                //    else
                //    {
                //        // 连续两个顶分型，取后一个顶，取消前一个顶
                //        lastData.NextPointType = PointType.Changing;
                //    }
                //}
            }
        }

        /// <summary>
        /// 取得分型、笔信息
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <returns></returns>
        private List<BaseDataInfo> GetFenxingPenInfo(List<BaseDataInfo> stockInfo)
        {
            List<BaseDataInfo> ret = new List<BaseDataInfo>();
            if (stockInfo.Count < 6)
            {
                return ret;
            }

            PointType pointType;
            PointType lastType;

            // 循环查找分型情报
            for (int i = stockInfo.Count - 1; i >= 2;)
            {
                // 查找分型情报
                int[] fenxing = this.ChkFenxing(stockInfo, i);
                pointType = (PointType)fenxing[0];

                // 将分型情报位置之前的分型形成中情报保存
                this.SetRangeFenxing(ret, stockInfo, i, fenxing[1] + 2, pointType);

                if (pointType == PointType.Changing)
                {
                    // 将最后的分型形成中情报保存
                    this.SetRangeFenxing(ret, stockInfo, fenxing[1] + 1, 0, pointType);
                    return ret;
                }

                // 重置位置信息
                i = fenxing[1];

                // 保存当前分型信息
                BaseDataInfo curFenxin = new BaseDataInfo();
                curFenxin.Day = stockInfo[i + 1].Day;
                curFenxin.DayVal = stockInfo[i + 1].DayVal;
                curFenxin.CurPointType = PointType.Changing;
                ret.Add(curFenxin);

                //if (pointType == PointType.Top) // 出现了顶分型
                //{
                //    if (befFenxin.CurPointType == PointType.Changing)
                //    {
                //        // 第一次分型以前的点的初始化(-1,0)
                //        befFenxin.CurPointType = PointType.Bottom;
                //        befFenxin.NextPointType = PointType.Changing;
                //    }

                //    if (befFenxin.CurPointType == PointType.Top)
                //    {
                //        if (befFenxin.NextPointType == PointType.Changing)
                //        {
                //            // -1,0 => 1,1
                //            curFenxin.CurPointType = PointType.Bottom;
                //            curFenxin.NextPointType = PointType.Bottom;
                //        }
                //        else
                //        {
                //            // -1,-1 => -1,0
                //            curFenxin.CurPointType = PointType.Top;
                //            curFenxin.NextPointType = PointType.Changing;
                //        }
                //    }
                //    else
                //    {
                //        if (befFenxin.NextPointType == PointType.Changing)
                //        {
                //            // 1,0 => 1,1
                //            curFenxin.CurPointType = PointType.Bottom;
                //            curFenxin.NextPointType = PointType.Bottom;
                //        }
                //        else
                //        {
                //            // 1,1 => 1,1
                //            curFenxin.CurPointType = PointType.Bottom;
                //            curFenxin.NextPointType = PointType.Bottom;
                //        }
                //    }

                //    //curFenxin.DayMaxVal = stockInfo[i + 1].DayMaxVal;
                //    curFenxin.DayVal = stockInfo[i + 1].DayVal;
                //}
                //else if (pointType == PointType.Bottom) // 出现了底分型
                //{
                //    if (befFenxin.CurPointType == PointType.Changing)
                //    {
                //        // 第一次分型的初始化(-1,0)
                //        befFenxin.CurPointType = PointType.Top;
                //        befFenxin.NextPointType = PointType.Changing;
                //    }

                //    if (befFenxin.CurPointType == PointType.Bottom)
                //    {
                //        if (befFenxin.NextPointType == PointType.Changing)
                //        {
                //            // 1,0 => -1,-1
                //            curFenxin.CurPointType = PointType.Top;
                //            curFenxin.NextPointType = PointType.Top;
                //        }
                //        else
                //        {
                //            // 1,1 => 1,0
                //            curFenxin.CurPointType = PointType.Bottom;
                //            curFenxin.NextPointType = PointType.Changing;
                //        }
                //    }
                //    else
                //    {
                //        if (befFenxin.NextPointType == PointType.Changing)
                //        {
                //            // -1,0 => -1,-1
                //            curFenxin.CurPointType = PointType.Top;
                //            curFenxin.NextPointType = PointType.Top;
                //        }
                //        else
                //        {
                //            // -1,-1 => -1,-1
                //            curFenxin.CurPointType = PointType.Top;
                //            curFenxin.NextPointType = PointType.Top;
                //        }
                //    }

                //    //curFenxin.DayMinVal = stockInfo[i + 1].DayMinVal;
                //    curFenxin.DayVal = stockInfo[i + 1].DayVal;
                //}

                if (i <= 2)
                {
                    // 将最后的分型形成中情报保存
                    this.SetRangeFenxing(ret, stockInfo, i, 0, PointType.Changing);
                    return ret;
                }
            }

            return ret;
        }

        /// <summary>
        /// 复制区间内的分型情报
        /// </summary>
        /// <param name="fenXing"></param>
        /// <param name="stockInfo"></param>
        /// <param name="idxStart"></param>
        /// <param name="idxEnd"></param>
        private void SetRangeFenxing(List<BaseDataInfo> fenXing, List<BaseDataInfo> stockInfo, int idxStart, int idxEnd, PointType pointType)
        {
            for (int i = idxStart; i >= idxEnd; i--)
            {
                BaseDataInfo item = new BaseDataInfo();
                item.Day = stockInfo[i].Day;
                item.CurPointType = (i > idxEnd ? PointType.Changing : pointType);
                //item.NextPointType = (i > idxEnd ? PointType.Changing : pointType);
                item.DayVal = stockInfo[i].DayVal;
                item.DayMinVal = item.DayVal;
                item.DayMaxVal = item.DayVal;

                fenXing.Add(item);
            }
        }

        /// <summary>
        /// 检查当前分型
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <returns>int.max：顶分型，int.min：底分型，正常：需要向下判断</returns>
        private int[] ChkFenxing(List<BaseDataInfo> stockInfo, int idx)
        {
            // 第一个：分型，第二个：下一个数据位置
            int[] fenxing = new int[2];

            if (idx < 2)
            {
                fenxing[0] = (int)PointType.Changing;
                fenxing[1] = idx;
                return fenxing;
            }

            decimal[] minMaxVal = new decimal[2];
            decimal[] minMaxVal2 = new decimal[2];

            while (idx >= 2)
            {
                // 取得没有包含关系的位置
                idx = this.GetNotIncludeInfo(stockInfo, idx, minMaxVal);
                if (idx < 2)
                {
                    fenxing[0] = (int)PointType.Changing;
                    fenxing[1] = idx;
                    return fenxing;
                }

                // 取得下一个没有包含关系的位置
                idx--;
                idx = this.GetNotIncludeInfo(stockInfo, idx, minMaxVal2);
                if (idx == 0)
                {
                    if (minMaxVal2[0] > minMaxVal[0])
                    {
                        fenxing[0] = (int)PointType.Top;
                    }
                    else if (minMaxVal2[0] < minMaxVal[0])
                    {
                        fenxing[0] = (int)PointType.Bottom;
                    }
                    else
                    {
                        fenxing[0] = (int)PointType.Changing;
                    }

                    fenxing[1] = 0;
                    return fenxing;
                }

                // 判断当前三个点的关系
                BaseDataInfo lastInfo = stockInfo[idx - 1];
                if (minMaxVal[0] * Consts.LIMIT_VAL < minMaxVal2[0] && minMaxVal2[0] > lastInfo.DayVal * Consts.LIMIT_VAL)
                {
                    fenxing[0] = (int)PointType.Top;
                    fenxing[1] = idx - 2;
                    return fenxing;
                }
                else if (minMaxVal[0] > minMaxVal2[0] * Consts.LIMIT_VAL && minMaxVal2[0] * Consts.LIMIT_VAL < lastInfo.DayVal)
                {
                    fenxing[0] = (int)PointType.Bottom;
                    fenxing[1] = idx - 2;
                    return fenxing;
                }

                /*if (idx == 0)
                {
                    if (minMaxVal2[1] > minMaxVal[1])
                    {
                        fenxing[0] = Consts.TOP_TYPE;
                    }
                    else if (minMaxVal2[0] < minMaxVal[0])
                    {
                        fenxing[0] = Consts.BOTTOM_TYPE;
                    }
                    else
                    {
                        fenxing[0] = Consts.NONE_TYPE;
                    }

                    fenxing[1] = 0;
                    return fenxing;
                }

                // 判断当前三个K线的关系
                BaseDataInfo lastInfo = stockInfo[idx - 1];
                if (minMaxVal[1] < minMaxVal2[1] && minMaxVal2[1] > lastInfo.DayMaxVal)
                {
                    fenxing[0] = Consts.TOP_TYPE;
                    fenxing[1] = idx - 2;
                    return fenxing;
                }
                else if (minMaxVal[0] > minMaxVal2[0] && minMaxVal2[0] < lastInfo.DayMinVal)
                {
                    fenxing[0] = Consts.BOTTOM_TYPE;
                    fenxing[1] = idx - 2;
                    return fenxing;
                }*/

                idx--;
            }

            fenxing[0] = (int)PointType.Changing;
            fenxing[1] = idx;
            return fenxing;
        }

        /// <summary>
        /// 取得没有包括关系的值的位置
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="idx"></param>
        /// <param name="minMaxVal"></param>
        /// <returns></returns>
        private int GetNotIncludeInfo(List<BaseDataInfo> stockInfo, int idx, decimal[] minMaxVal)
        {
            minMaxVal[0] = stockInfo[idx].DayVal;

            while (idx >= 1)
            {
                BaseDataInfo nextInfo = stockInfo[idx - 1];

                // 判断是否变化
                if (Math.Abs(minMaxVal[0] - nextInfo.DayVal) < minMaxVal[0] * (Consts.LIMIT_VAL - 1))
                {
                    // 前后没有变化
                    minMaxVal[0] = nextInfo.DayVal;
                    idx--;
                    continue;
                }

                break;
            }

            /*minMaxVal[0] = stockInfo[idx].DayMinVal;
            minMaxVal[1] = stockInfo[idx].DayMaxVal;

            while (idx >= 1)
            {
                BaseDataInfo nextInfo = stockInfo[idx - 1];

                // 判断包含关系
                if (minMaxVal[1] > nextInfo.DayMaxVal * Consts.LIMIT_VAL && minMaxVal[0] * Consts.LIMIT_VAL < nextInfo.DayMinVal)
                {
                    // 前面的包括后面的
                    minMaxVal[1] = nextInfo.DayMaxVal;
                    idx--;
                    continue;
                }
                else if (minMaxVal[1] * Consts.LIMIT_VAL < nextInfo.DayMaxVal && minMaxVal[0] > nextInfo.DayMinVal * Consts.LIMIT_VAL)
                {
                    // 后面的包括前面的
                    minMaxVal[1] = nextInfo.DayMaxVal;
                    idx--;
                    continue;
                }
                else
                {
                    if (minMaxVal[1] > nextInfo.DayMaxVal * Consts.LIMIT_VAL && minMaxVal[0] * Consts.LIMIT_VAL < nextInfo.DayMinVal)
                    {
                        // 特殊处理1
                        minMaxVal[1] = nextInfo.DayMaxVal;
                        idx--;
                        continue;
                    }
                    else if (minMaxVal[1] * Consts.LIMIT_VAL < nextInfo.DayMaxVal && minMaxVal[0] > nextInfo.DayMinVal * Consts.LIMIT_VAL)
                    {
                        // 特殊处理2
                        minMaxVal[1] = nextInfo.DayMaxVal;
                        idx--;
                        continue;
                    }
                }

                break;
            }*/

            return idx;
        }

        #endregion

        #region " 画面显示相关 "

        /// <summary>
        /// 显示当前位置的数据信息
        /// </summary>
        /// <param name="x"></param>
        /// <param name="imgWidth"></param>
        private void DisplayCurDayInfo(int x, int imgWidth)
        {
            x -= x % IMG_X_STEP;
            int pos = (int)((imgWidth - x - IMG_X_STEP) / IMG_X_STEP);
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

            if (!string.IsNullOrEmpty(this.curStockData[idx].Day))
            {
                sb.Append(this.curStockData[idx].Day.Substring(0, 4)).Append("年");
                sb.Append(this.curStockData[idx].Day.Substring(4, 2)).Append("月");
                sb.Append(this.curStockData[idx].Day.Substring(6, 2)).Append("日");
                sb.Append(" ");
            }

            if (this.curStockData.Count > 0)
            {
                sb.Append(" ");
                sb.Append("1:").Append(this.GetStockValInfo(this.curStockData, idx));
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
            // 取得已经存在的所有趋势图
            List<FilePosInfo> allImg = Util.GetAllFiles(IMG_FOLDER);

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
                string stockImg = IMG_FOLDER + this.allStock[this.curIdx] + ".png";
                if (File.Exists(stockImg))
                {
                    this.imgBody.Image = Image.FromFile(stockImg);
                }

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

            string[] allLine = this.GetStockFileContent(stockCd);
            if (allLine != null && allLine.Length > 2)
            {
                string[] lineData = allLine[1].Split(',');
                this.curStockName = lineData[2];
            }
        }

        /// <summary>
        /// 设置当前数据
        /// </summary>
        /// <param name="stockCdData"></param>
        private void SetCurStockData(string stockCdData)
        {
            // 获得数据信息
            this.subFolder = DAY_FOLDER;
            Dictionary<string, object> dataInfo = this.GetStockInfo(stockCdData);
            if (dataInfo == null)
            {
                return;
            }

            // 基础数据信息
            this.curStockData = (List<BaseDataInfo>)dataInfo["stockInfos"];

            // 取得5日级别信息
            this.curStockJibie5Data = this.GetJibieStockInfo(this.curStockData, 5);

            // 取得10日级别信息
            this.curStockJibie10Data = this.GetJibieStockInfo(this.curStockData, 10);
        }

        /// <summary>
        /// 设置当前的数据时间
        /// </summary>
        private void SetDataDate()
        {
            FilePosInfo fileInfo = null;

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER + DAY_FOLDER).Where(p => !p.IsFolder).ToList();
            if (allCsv.Count > 0)
            {
                fileInfo = allCsv[0];
            }

            if (fileInfo != null)
            {
                this.dataDate = Util.GetShortNameWithoutType(fileInfo.File).Substring(7);
                this.cmbCon.Enabled = true;
            }
            else
            {
                this.dataDate = string.Empty;
                this.btnAft.Enabled = false;
                this.btnBef.Enabled = false;
                this.cmbCon.Enabled = false;
            }
        }

        #endregion

        #region " 各种基本处理 "

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

            if (DateTime.Compare(chkDt, maxDt.AddDays(3)) <= 0)
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
        /// 取得最大、最小值
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns></returns>
        private decimal[] GetMaxMinStock(List<BaseDataInfo> stockInfos)
        {
            decimal[] minMaxInfo = new decimal[2];
            decimal minVal = decimal.MaxValue;
            decimal maxVal = 0;

            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                if (stockInfos[i].DayVal == 0)
                {
                    stockInfos.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                decimal curVal = stockInfos[i].DayVal;
                if (curVal > maxVal)
                {
                    maxVal = curVal;
                }

                if (curVal > 0 && curVal < minVal)
                {
                    minVal = curVal;
                }

                if (curVal == 0)
                {
                    BaseDataInfo item = new BaseDataInfo();
                    item.Day = stockInfos[i].Day;
                    item.DayVal = stockInfos[i + 1].DayVal;
                    item.DayMaxVal = stockInfos[i + 1].DayMaxVal;
                    item.DayMinVal = stockInfos[i + 1].DayMinVal;

                    stockInfos[i] = item;
                }
            }

            minMaxInfo[0] = minVal;
            minMaxInfo[1] = maxVal;

            return minMaxInfo;
        }

        /// <summary>
        /// 取得Y坐标的位置
        /// </summary>
        /// <param name="imgH"></param>
        /// <param name="pointVal"></param>
        /// <param name="minVal"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private int GetYPos(int imgH, decimal pointVal, decimal minVal, decimal step)
        {
            return imgH - ((int)((pointVal - minVal) * step)) - 10;
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
            if (idx > 0)
            {
                sb.Append("(");
                decimal tmp = (stockInfos[idx - 1].DayVal - stockInfos[idx].DayVal) * 100 / stockInfos[idx].DayVal;
                sb.Append(tmp.ToString("0.00"));
                sb.Append(")");
            }

            return sb.ToString();
        }

        #endregion

        #endregion
    }
}
