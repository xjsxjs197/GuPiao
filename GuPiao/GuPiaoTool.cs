using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using GuPiao;

namespace GuPiaoTool
{
    public partial class GuPiaoTool : Form
    {
        #region 全局变量

        List<string> noList = new List<string>();
        List<string> nameList = new List<string>();
        List<GuPiaoInfo> guPiaoInfo = null;
        System.Timers.Timer timersTimer = null;
        bool isRuning = false;
        TradeUtil tradeUtil = new TradeUtil();
        string selStockCd = string.Empty;
        Dictionary<string, GuPiaoInfo> guPiaoBaseInfo = new Dictionary<string,GuPiaoInfo>();
        List<OrderInfo> todayGuPiao = new List<OrderInfo>();

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public GuPiaoTool()
        {
            InitializeComponent();

            this.rdoSync.CheckedChanged += new EventHandler(this.rdoSync_CheckedChanged);
            this.FormClosing += new FormClosingEventHandler(this.GuPiaoTool_FormClosing);
            this.grdGuPiao.SelectionChanged += new EventHandler(this.grdGuPiao_SelectionChanged);

            // 设置信息
            this.tradeUtil.SetCallBack(this.AsyncCallBack);
            this.tradeUtil.SetSync(this.rdoSync.Checked);
            this.tradeUtil.SetGuPiaoInfo(this.guPiaoBaseInfo);

            // 初始化基本信息
            this.InitBaseInfo();
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
            this.tradeUtil.SetSync(this.rdoSync.Checked);
        }

        /// <summary>
        /// 买
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBuy_Click(object sender, EventArgs e)
        {
            if (!this.CheckInput(this.cmbCountBuy.SelectedItem, this.txtPriceBuy.Text))
            {
                return;
            }

            this.tradeUtil.BuyStock(this.selStockCd, this.GetCount(this.cmbCountBuy.SelectedItem), this.GetPrice(this.txtPriceBuy.Text), false);
        }

        /// <summary>
        /// 闪买
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuictBuy_Click(object sender, EventArgs e)
        {
            if (!this.CheckInput(this.cmbCountBuy.SelectedItem, this.txtPriceBuy.Text))
            {
                return;
            }

            this.tradeUtil.BuyStock(this.selStockCd, this.GetCount(this.cmbCountBuy.SelectedItem), this.GetPrice(this.txtPriceBuy.Text), true);
        }

        /// <summary>
        /// 卖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSell_Click(object sender, EventArgs e)
        {
            if (!this.CheckInput(this.cmbCountSell.SelectedItem, this.txtPriceSell.Text))
            {
                return;
            }

            this.tradeUtil.SellStock(this.selStockCd, this.GetCount(this.cmbCountSell.SelectedItem), this.GetPrice(this.txtPriceSell.Text), false);
        }

        /// <summary>
        /// 闪卖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuickSell_Click(object sender, EventArgs e)
        {
            if (!this.CheckInput(this.cmbCountSell.SelectedItem, this.txtPriceSell.Text))
            {
                return;
            }

            this.tradeUtil.SellStock(this.selStockCd, this.GetCount(this.cmbCountSell.SelectedItem), this.GetPrice(this.txtPriceSell.Text), true);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GuPiaoTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.tradeUtil.TradeRelease();
        }

        /// <summary>
        /// 切换选中股票
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdGuPiao_SelectionChanged(object sender, EventArgs e)
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

                // 设置最新价格
                string curPrice = selectedRow.Cells[2].Value as string;
                this.txtPriceBuy.Text = curPrice;
                this.txtPriceSell.Text = curPrice;

                // 根据可用余额，当前价格，设置买的按钮状态
                this.SetBuyInfo(this.lblCanUseMoney.Text, curPrice);

                // 根据可用股数，设置卖的按钮的状态
                this.SetSellInfo(Convert.ToInt32(selectedRow.Cells[5].Value));
            }
        }

        #endregion

        #region 私有方法

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
                this.cmbCountSell.Enabled = false;
                this.btnSell.Enabled = false;
                this.btnQuickSell.Enabled = false;
            }
            else
            {
                this.cmbCountSell.Enabled = true;
                this.btnSell.Enabled = true;
                this.btnQuickSell.Enabled = true;

                // 设置最大可以卖多少
                int lastCount = 0;
                if (this.cmbCountSell.Items.Count > 0)
                {
                    lastCount = Convert.ToInt32(this.cmbCountSell.Items[this.cmbCountSell.Items.Count - 1]);
                }
                if (lastCount == canUserCount)
                {
                    // 如果设置过了，不用每次设置
                    this.cmbCountSell.SelectedIndex = 0;
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
            double nowMoney = Convert.ToDouble(canUseMoney);
            double nowPrice = Convert.ToDouble(curPrice) * 100;
            if (nowMoney < nowPrice + 5)
            {
                this.cmbCountBuy.SelectedIndex = -1;
                this.cmbCountBuy.Enabled = false;
                this.btnBuy.Enabled = false;
                this.btnQuictBuy.Enabled = false;
            }
            else
            {
                this.cmbCountBuy.Enabled = true;
                this.btnBuy.Enabled = true;
                this.btnQuictBuy.Enabled = true;

                // 设置最大可以买多少
                int maxNum = (int)((nowMoney - 5) / nowPrice);
                if (maxNum == this.cmbCountBuy.Items.Count)
                {
                    // 如果设置过了，不用每次设置
                    this.cmbCountSell.SelectedIndex = 0;
                    return;
                }

                this.cmbCountBuy.Items.Clear();
                for (int i = 1; i <= maxNum; i++)
                {
                    this.cmbCountBuy.Items.Add(i * 100);
                }

                this.cmbCountBuy.SelectedIndex = 0;
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
                this.grdGuPiao.Rows[0].Selected = false;
                this.grdGuPiao.Rows[1].Selected = true;

                // 启动定时器
                timersTimer = new System.Timers.Timer();
                timersTimer.Enabled = true;
                timersTimer.Interval = 1500;
                timersTimer.AutoReset = true;
                timersTimer.Elapsed += new System.Timers.ElapsedEventHandler(timersTimer_Elapsed);
                timersTimer.SynchronizingObject = this;

                // 按钮控制
                this.btnRun.Text = "刷  新";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
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
            }
        }

        /// <summary>
        /// 读取基础数据
        /// </summary>
        private bool LoadBaseData()
        {
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
            // 从Sina取得基础数据
            string url = "http://hq.sinajs.cn/list=" + string.Join(",", noList.ToArray());
            string data = "";
            string result = HttpGet(url, data);
            if (!string.IsNullOrEmpty(result) && result.Length > 20)
            {
                guPiaoInfo = GetGuPiaoInfo(noList, nameList, result);
                this.DisplayData(guPiaoInfo);
            }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="guPiaoInfo"></param>
        private void DisplayData(List<GuPiaoInfo> guPiaoInfo)
        {
            int newRow;
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
                
                this.SetYinkuiPer(guPiaoInfo[i], lineCollection[3]);

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
                    lineCollection[4].Value = 0;
                    lineCollection[5].Value = 0;
                }
            }

            // 更新当前选中信息
            this.grdGuPiao_SelectionChanged(null, null);
        }

        /// <summary>
        /// 取得盈亏比
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string SetYinkuiPer(GuPiaoInfo item, DataGridViewCell cellItem)
        {
            decimal currentVal = decimal.Parse(item.currentVal);
            decimal zuoriShoupanVal = decimal.Parse(item.zuoriShoupanVal);
            decimal yingkuiPer = (currentVal - zuoriShoupanVal) / zuoriShoupanVal * 100;

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

            return yingkuiPer.ToString("00.00");
        }

        /// <summary>
        /// 取得股票信息
        /// </summary>
        /// <param name="noList"></param>
        /// <param name="nameList"></param>
        /// <param name="guPiaoJsInfo"></param>
        /// <returns></returns>
        private List<GuPiaoInfo> GetGuPiaoInfo(List<string> noList, List<string> nameList, string guPiaoJsInfo)
        {
            List<GuPiaoInfo> infoList = new List<GuPiaoInfo>();

            string[] guPiao = guPiaoJsInfo.Split(';');
            for (int i = 0; i < guPiao.Length; i++)
            {
                if (string.IsNullOrEmpty(guPiao[i]) || guPiao[i].Length < 10)
                {
                    break;
                }

                GuPiaoInfo item = new GuPiaoInfo();
                infoList.Add(item);
                item.fundcode = noList[i];
                item.name = nameList[i];

                string[] lines = guPiao[i].Split('=');
                string[] details = lines[1].Split(',');
                item.jinriKaipanVal  = details[1];
                item.zuoriShoupanVal = details[2];
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
            }

            return infoList;
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
            this.btnSell.Enabled = enable;
            this.btnQuickSell.Enabled = enable;
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
        /// 显示各种操作信息
        /// </summary>
        /// <param name="retMsg"></param>
        private void DispMsg(string retMsg)
        {
            this.Text = retMsg;
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
            }
        }

        /// <summary>
        /// 异步的回调方法
        /// </summary>
        /// <param name="param"></param>
        private void AsyncCallBack(params object[] param)
        {
            switch (this.tradeUtil.CurOpt)
            {
                case CurOpt.InitEvent:
                    if (this.tradeUtil.IsSuccess)
                    {
                        this.DispMsg("初始化成功");

                        this.btnRun.Enabled = true;
                    }
                    else
                    {
                        this.DispMsg(this.tradeUtil.RetMsg);
                    }
                    break;

                case CurOpt.ConnServer:
                case CurOpt.LoginEvent:
                    this.DispMsg(this.tradeUtil.RetMsg);
                    if (!this.tradeUtil.isLoginOk)
                    {
                        this.btnRun.Enabled = false;
                        this.EnableItems(false);
                    }
                    else
                    {
                        this.btnRun.Enabled = true;
                        this.EnableItems(true);

                        // 从付费接口取得可用股数
                        this.tradeUtil.GetGuPiaoInfo(this.guPiaoBaseInfo);

                        // 取得当天委托信息
                        this.tradeUtil.GetTodayPiaoInfo(this.todayGuPiao);
                        // 显示当前委托信息
                        this.DispTodayInfo();

                        // 设置金额基本信息
                        //MessageBox.Show(param[0] + " : " + param[1] + " : " + param[2] + " : " + param[3]);
                        if (param != null && param.Length >= 4)
                        {
                            this.lblTotal.Text = Convert.ToString(param[0]);
                            this.lblGuPiaoMoney.Text = Convert.ToString(param[1]);
                            this.lblCanUseMoney.Text = Convert.ToString(param[2]);
                            this.lblCanGetMoney.Text = Convert.ToString(param[3]);
                        }
                    }
                    break;

                case CurOpt.GetGuPiaoInfo:
                    this.DispMsg(this.tradeUtil.RetMsg);
                    break;

                case CurOpt.OrderOKEvent:
                case CurOpt.OrderSuccessEvent:
                    // 订单成功，需要刷新页面数据
                    this.DispMsg(this.tradeUtil.RetMsg);
                    break;
            }
        }

        #endregion
    }
}
