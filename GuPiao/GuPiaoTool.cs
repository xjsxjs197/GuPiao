using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GuPiaoTool
{
    public partial class GuPiaoTool : Form
    {
        #region 全局变量

        List<string> noList = new List<string>();
        List<string> nameList = new List<string>();
        List<GuPiaoInfo> guPiaoInfo = null;
        System.Timers.Timer timersTimer = null;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public GuPiaoTool()
        {
            InitializeComponent();
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
            try
            {
                // 读取基础数据
                if (!this.LoadBaseData())
                {
                    return;
                }

                // 刷新页面
                RefreshPage();

                // 启动定时器
                timersTimer = new System.Timers.Timer();
                timersTimer.Enabled = true;
                timersTimer.Interval = 2000;
                timersTimer.AutoReset = true;
                timersTimer.Elapsed += new System.Timers.ElapsedEventHandler(timersTimer_Elapsed);
                timersTimer.SynchronizingObject = this;

                // 按钮控制
                this.btnRun.Enabled = false;
                this.btnRefresh.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        /// <summary>
        /// 刷新页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
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
        /// 定时事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timersTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 刷新页面
            RefreshPage();
        }

        #endregion

        #region 私有方法

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
            this.grdGuPiao.Rows.Clear();
            for (int i = 0; i < guPiaoInfo.Count; i++)
            {
                int newRow = this.grdGuPiao.Rows.Add();
                DataGridViewCellCollection lineCollection = this.grdGuPiao.Rows[newRow].Cells;
                lineCollection[0].Value = guPiaoInfo[i].fundcode + "(" + guPiaoInfo[i].name + ")";
                lineCollection[1].Value = guPiaoInfo[i].zuoriShoupanVal;
                lineCollection[2].Value = guPiaoInfo[i].currentVal;
                
                this.SetYinkuiPer(guPiaoInfo[i], lineCollection[3]);
            }
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

        #endregion
    }
}
