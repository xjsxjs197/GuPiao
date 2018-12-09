using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Hanhua.Common;

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
        /// 工具的名称
        /// </summary>
        private const string TITLE = "财富数据 ";

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
        /// </summary>
        private List<string> hasBuyPointsStock = new List<string>();

        /// <summary>
        /// 当前数据的时间
        /// </summary>
        private string dataDate = string.Empty;

        /// <summary>
        /// 当前显示的数据信息
        /// </summary>
        private List<decimal> curStockData = new List<decimal>();

        /// <summary>
        /// 当前显示的数据信息(5日数据)
        /// </summary>
        private List<decimal> curStockJibie5Data = new List<decimal>();

        /// <summary>
        /// 当前显示的数据信息(10日数据)
        /// </summary>
        private List<decimal> curStockJibie10Data = new List<decimal>();

        /// <summary>
        /// 当前数据的名称
        /// </summary>
        private string curStockName = string.Empty;

        #endregion

        #region " 初始化 "

        /// <summary>
        /// 初始化
        /// </summary>
        public CreateQushi()
        {
            InitializeComponent();

            this.cmbCon.SelectedIndex = 0;
            this.pnlBody.BackColor = Color.FromArgb(199, 237, 204);

            this.cmbCon.SelectedIndexChanged += new EventHandler(this.cmbCon_SelectedIndexChanged);

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
            this.btnCreate.Enabled = false;
            
            // 重新设置当前显示
            this.ResetDisplay();

            this.Do(this.ThreadDrawQushiImg);
        }

        /// <summary>
        /// 取得所有数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetAllStock_Click(object sender, EventArgs e)
        {
            this.btnGetAllStock.Enabled = false;

            // 重新设置当前显示
            this.ResetDisplay();

            this.Do(this.ThreadGetAllStock);
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
            this.SetTitle(this.allStock[this.curIdx], this.dataDate, 0);
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

                default:
                    // 显示所有信息
                    this.DisplayAllStockPng(null);
                    this.cmbCon.Enabled = true;
                    break;
            }
        }

        #endregion

        #region " 公有方法 "
        #endregion

        #region " 私有方法 "

        /// <summary>
        /// 多线程取得所有数据
        /// </summary>
        private void ThreadGetAllStock()
        {
            // 设定结束日期
            string endDay = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER);

            // 取得所有信息的Html页面内容
            string allInfos = Util.GetHtmlStr("http://quote.eastmoney.com/stocklist.html", "");

            // 过滤数据
            Regex reg = new Regex("<li><a target=\"_blank\" href=\"http://quote.eastmoney.com/\\S\\S(.*?).html\">");   // 定义正则表达式
            Regex regSub = new Regex(@"[0|6|3]\d{5}");   // 定义正则表达式

            string leftUrl = "http://quotes.money.163.com/service/chddata.html?fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;TURNOVER;VOTURNOVER;VATURNOVER;TCAP;MCAP";
            string rightUrl = "&end=" + endDay + "&code=";
            string result;
            string codeType;
            string tmpFile = CSV_FOLDER + "tmp.csv";
            Encoding encoding = Encoding.GetEncoding("GBK");

            MatchCollection mc = reg.Matches(allInfos);  // 在内容中匹配与正则表达式匹配的字符

            // 设置进度条
            this.ResetProcessBar(mc.Count);

            foreach (Match m in mc)     // 循环匹配到的字符
            {
                string stockCd = regSub.Match(m.Value).Value;
                if (!string.IsNullOrEmpty(stockCd))
                {
                    if (stockCd.StartsWith("6"))
                    {
                        codeType = "0";
                    }
                    else
                    {
                        codeType = "1";
                    }


                    // 取得开始时间
                    string startDay = this.GetExitsStock(allCsv, stockCd);
                    if (string.IsNullOrEmpty(startDay))
                    {
                        // 取截止今天为止的所有数据
                        result = Util.HttpGet(leftUrl + rightUrl + codeType + stockCd, "", encoding);
                        if (!string.IsNullOrEmpty(result))
                        {
                            File.WriteAllText(CSV_FOLDER + stockCd + "_" + endDay + ".csv", result, Encoding.UTF8);
                        }
                    }
                    else if (string.Compare(startDay, endDay) < 0)
                    {
                        // 取开始时间，到结束时间的数据
                        result = Util.HttpGet(leftUrl + rightUrl + codeType + stockCd + "&start=" + startDay, "", encoding);
                        if (!string.IsNullOrEmpty(result))
                        {
                            // 生成临时文件
                            File.WriteAllText(tmpFile, result, Encoding.UTF8);

                            // 将临时文件的内容，追加到既存的文件中
                            string oldFilePath = CSV_FOLDER + stockCd + "_" + startDay + ".csv";
                            string[] oldFile = File.ReadAllLines(oldFilePath, Encoding.UTF8);
                            string[] newContent = File.ReadAllLines(tmpFile, Encoding.UTF8);
                            List<string> all = new List<string>();
                            all.AddRange(newContent);
                            for (int i = 2; i < oldFile.Length; i++)
                            {
                                all.Add(oldFile[i]);
                            }

                            // 生成新的文件，删除既存的文件
                            File.WriteAllLines(CSV_FOLDER + stockCd + "_" + endDay + ".csv", all.ToArray(), Encoding.UTF8);
                            File.Delete(oldFilePath);
                        }
                    }
                }

                // 更新进度条
                this.ProcessBarStep();
            }

            // 关闭进度条
            this.CloseProcessBar();

            // 删除临时文件
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }

             // 设置按钮可用
            this.mSyncContext.Post(this.UISetBtnEnable, this.btnGetAllStock);
        }

        /// <summary>
        /// 画趋势图
        /// </summary>
        private void ThreadDrawQushiImg()
        {
            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER);
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

                this.CreateQushiImg(Util.GetShortNameWithoutType(fileItem.File));

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
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER);

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
                if (this.chkNoChuangye.Checked && this.IsChuangyeStock(shortName))
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
        /// 是否是创业板数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        private bool IsChuangyeStock(string stockCd)
        {
            return stockCd.StartsWith("300");
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
                    this.hasBuyPointsStock.AddRange(allLine);
                    if (string.IsNullOrEmpty(allLine[allLine.Length - 1]))
                    {
                        this.hasBuyPointsStock.RemoveAt(this.hasBuyPointsStock.Count - 1);
                    }
                }
            }
        }

        /// <summary>
        /// 保存包括买点信息的数据
        /// </summary>
        private void SaveHasBuyPointsInfo()
        {
            File.WriteAllLines(RESULT_FOLDER + "BuyPoints.txt", this.hasBuyPointsStock.ToArray(), Encoding.UTF8);
        }

        /// <summary>
        /// 设置当前的数据时间
        /// </summary>
        private void SetDataDate()
        {
            FilePosInfo fileInfo = null;

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(CSV_FOLDER).Where(p => !p.IsFolder).ToList();
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

        /// <summary>
        /// 根据代码取得数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        private string[] GetStockFileContent(string stockCd)
        {
            string csvFile = CSV_FOLDER + stockCd + "_" + this.dataDate + ".csv";
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
                DateTime dt = DateTime.ParseExact(this.dataDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);

                this.SetTitle(this.allStock[this.curIdx], dt.AddDays(-pos).ToString("yyyyMMdd"), pos);
            }
        }

        /// <summary>
        /// 设置当前标题
        /// </summary>
        /// <param name="stockCd"></param>
        private void SetTitle(string stockCd, string dataDate, int idx)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(stockCd).Append(" ");
            sb.Append(this.curStockName).Append(" ");

            if (!string.IsNullOrEmpty(dataDate))
            {
                sb.Append(dataDate.Substring(0, 4)).Append("年");
                sb.Append(dataDate.Substring(4, 2)).Append("月");
                sb.Append(dataDate.Substring(6, 2)).Append("日");
                sb.Append(" ");
            }

            if (this.curStockData.Count > 0)
            {
                sb.Append(" ");
                sb.Append("1:").Append(this.curStockData[idx]);
                sb.Append("   ");
            }

            if (this.curStockJibie5Data.Count > 0)
            {
                sb.Append("5:").Append(this.curStockJibie5Data[idx]);
                sb.Append("   ");
            }

            if (this.curStockJibie10Data.Count > 0)
            {
                sb.Append("10:").Append(this.curStockJibie10Data[idx]);
                sb.Append("   ");
            }

            this.Text = TITLE + sb.ToString();
        }

        /// <summary>
        /// 检查当前趋势
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <returns></returns>
        private bool ChkQushi(string stockCdData, QushiBase chkQushi)
        {
            // 获得数据信息
            Dictionary<string, object> dataInfo = this.GetStockInfo(stockCdData);
            if (dataInfo == null)
            {
                return false;
            }

            return chkQushi.StartCheck((List<decimal>)dataInfo["stockInfos"]);
        }

        /// <summary>
        /// 根据StockCd取得相关数据信息
        /// </summary>
        /// <param name="stockCdData"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetStockInfo(string stockCdData)
        {
            // 读取所有信息
            List<decimal> stockInfos = this.GetStockHistoryInfo(CSV_FOLDER + stockCdData + ".csv");
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
                if (this.chkNoChuangye.Checked && this.IsChuangyeStock(shortName))
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

            foreach (string stockCd in this.hasBuyPointsStock)
            {
                if (this.chkNoChuangye.Checked && this.IsChuangyeStock(stockCd))
                {
                    continue;
                }

                this.allStock.Add(stockCd);
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
                this.SetTitle(this.allStock[this.curIdx], this.dataDate, 0);
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
            Dictionary<string, object> dataInfo = this.GetStockInfo(stockCdData);
            if (dataInfo == null)
            {
                return;
            }

            // 基础数据信息
            this.curStockData = (List<decimal>)dataInfo["stockInfos"];

            // 取得5日级别信息
            this.curStockJibie5Data = this.GetJibieStockInfo(this.curStockData, 5);

            // 取得10日级别信息
            this.curStockJibie10Data = this.GetJibieStockInfo(this.curStockData, 10);
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
        /// 取得当前Code的数据
        /// </summary>
        /// <param name="allCsv"></param>
        /// <param name="stockCd"></param>
        /// <returns>文件名中的日期</returns>
        private string GetExitsStock(List<FilePosInfo> allCsv, string stockCd)
        {
            int pos = 0;
            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                pos = fileItem.File.IndexOf(stockCd);
                if (pos > 0)
                {
                    return fileItem.File.Substring(pos + 7, 8);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 画趋势图
        /// </summary>
        /// <param name="stockCdData"></param>
        private void CreateQushiImg(string stockCdData)
        {
            // 获得数据信息
            Dictionary<string, object> dataInfo = this.GetStockInfo(stockCdData);
            if (dataInfo == null)
            {
                return;
            }

            // 基础数据信息
            List<decimal> stockInfos = (List<decimal>)dataInfo["stockInfos"];

            // 最大、最小值信息
            decimal[] minMaxInfo = (decimal[])dataInfo["minMaxInfo"];
            decimal step = 290 / (minMaxInfo[1] - minMaxInfo[0]);

            // 设定图片
            Bitmap imgQushi = new Bitmap(600, 320);
            Graphics grp = Graphics.FromImage(imgQushi);
            grp.SmoothingMode = SmoothingMode.AntiAlias;
            grp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 开始画日线
            this.DrawStockQushi(stockInfos, step, minMaxInfo[0], imgQushi, new Pen(Color.Black, 1F), grp);

            // 取得5日级别信息
            List<decimal> stockInfo5Jibie = this.GetJibieStockInfo(stockInfos, 5);

            // 开始画5日线
            if (stockInfo5Jibie.Count > 0)
            {
                this.DrawStockQushi(stockInfo5Jibie, step, minMaxInfo[0], imgQushi, new Pen(Color.Green, 1F), grp);
            }

            // 取得10日级别信息
            List<decimal> stockInfo10Jibie = this.GetJibieStockInfo(stockInfos, 10);

            // 开始画10日线
            if (stockInfo10Jibie.Count > 0)
            {
                this.DrawStockQushi(stockInfo10Jibie, step, minMaxInfo[0], imgQushi, new Pen(Color.Red, 1F), grp);
            }

            // 趋势图上画买卖点
            bool hasBuyPoint = this.DrawStockBuySellPoint(stockInfos, stockInfo5Jibie, step, minMaxInfo[0], imgQushi, grp);
            if (hasBuyPoint)
            {
                this.hasBuyPointsStock.Add(stockCdData.Substring(0, 6));
            }

            // 保存图片
            imgQushi.Save(IMG_FOLDER + stockCdData.Substring(0, 6) + ".png");
        }

        /// <summary>
        /// 取得股票历史数据
        /// </summary>
        /// <param name="stockFile"></param>
        /// <returns></returns>
        private List<decimal> GetStockHistoryInfo(string stockFile)
        { 
            List<decimal> stockInfo = new List<decimal>();
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
                    stockInfo.Add(decimal.Parse(allItems[3]));
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
        private List<decimal> GetJibieStockInfo(List<decimal> stockInfos, int jibie)
        {
            List<decimal> stockInfo = new List<decimal>();

            int index = 0;
            int jibieCount = jibie - 1;
            int maxCount = stockInfos.Count - jibie;

            while (index <= maxCount)
            {
                decimal total = 0;
                for (int i = 0; i <= jibieCount; i++)
                {
                    total += stockInfos[index + i];
                }
                stockInfo.Add(total / jibie);

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
        /// 取得最大、最小值
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns></returns>
        private decimal[] GetMaxMinStock(List<decimal> stockInfos)
        { 
            decimal[] minMaxInfo = new decimal[2];
            decimal minVal = decimal.MaxValue;
            decimal maxVal = 0;

            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                if (stockInfos[i] == 0)
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
                decimal curVal = stockInfos[i];
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
                    stockInfos[i] = stockInfos[i + 1];
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
        /// 开始画趋势图
        /// </summary>
        /// <param name="stockInfo"></param>
        /// <param name="step"></param>
        private void DrawStockQushi(List<decimal> stockInfo, decimal step, decimal minVal, Bitmap img, Pen pen, Graphics grp)
        {
            int startX = img.Width - IMG_X_STEP;
            int x1 = startX;
            int y1 = this.GetYPos(img.Height, stockInfo[0], minVal, step);
            int x2 = 0;
            int y2 = 0;
            int index = 1;
            while (index <= stockInfo.Count - 1)
            {
                x2 = startX - IMG_X_STEP;
                y2 = this.GetYPos(img.Height, stockInfo[index], minVal, step);

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
        private bool DrawStockBuySellPoint(List<decimal> stockInfo, List<decimal> jibie5StockInfo, decimal step, decimal minVal, Bitmap img, Graphics grp)
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
                if (stockInfo[i] > stockInfo[i + 1] * Consts.LIMIT_VAL)
                {
                    if (curQushi < 0)
                    {
                        // 原来是下跌，如果满足转折条件（连续N天，并且超过5日线）
                        if (-curQushi > Consts.QUSHI_CONTINUE_DAYS && stockInfo[i] >= jibie5StockInfo[i])
                        {
                            // 画买点
                            grp.FillEllipse(buyBush, img.Width - IMG_X_STEP * (i + 3), this.GetYPos(img.Height, stockInfo[i + 1], minVal, step), 7, 7);
                            hasBuyPoint = true;
                            buyed = true;
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
                else if (stockInfo[i] * Consts.LIMIT_VAL < stockInfo[i + 1])
                {
                    if (curQushi > 0)
                    {
                        if (buyed)
                        {
                            // 原来是上涨,并且已经有买点时才画卖点
                            grp.FillEllipse(sellBush, img.Width - IMG_X_STEP * (i + 3), this.GetYPos(img.Height, stockInfo[i + 1], minVal, step), 7, 7);
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
    }
}
