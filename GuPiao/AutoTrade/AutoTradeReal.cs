using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;
using DataProcess.FenXing;
using System.Text.RegularExpressions;

namespace GuPiao
{
    /// <summary>
    /// 实时自动交易
    /// </summary>
    public class AutoTradeReal : AutoTradeBase
    {
        #region 全局变量

        /// <summary>
        /// 交易的共通处理对象
        /// </summary>
        private TradeUtil tradeUtil = new TradeUtil();

        private ComboBox cmbAccountType = new ComboBox();
        private ComboBox cmbBrokerType = new ComboBox();

        private string curDay = string.Empty;

        #endregion

        #region 子类重写父类的虚方法

        /// <summary>
        /// 初始化其他
        /// </summary>
        protected override void InitOther()
        {
            if (this.isRealEmu)
            {
                TradeEventParam param = new TradeEventParam();
                param.CurOpt = CurOpt.InitEvent;
                param.IsSuccess = true;
                param.Msg = "实时模拟处理：初始化成功";
                this.callBackF(param);
            }
            else
            {
                // 修改系统时间，证书的有效时间是从2018/4/23开始的一周
                this.ChangeSystemTime();

                // 设置异步关联信息
                this.tradeUtil.SetCallBack(this.callBackF);
                this.tradeUtil.SetGuPiaoInfo(this.guPiaoBaseInfo);

                // 设置交易的基础配置信息
                this.tradeUtil.Init(this.cmbAccountType, this.cmbBrokerType);
            }

            this.curDay = this.tradeDate.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 获取定时器间隔
        /// 实时交易的间隔是1秒
        /// </summary>
        /// <returns></returns>
        protected override double GetTradeTimer()
        {
            return 1000;
        }

        /// <summary>
        /// 登陆服务器
        /// </summary>
        protected override void LoginServerSub()
        {
            if (this.isRealEmu)
            {
                TradeEventParam param = new TradeEventParam();
                param.CurOpt = CurOpt.LoginEvent;
                param.IsSuccess = true;
                param.Msg = "实时模拟处理：登陆服务器成功";
                this.callBackF(param);
            }
            else
            {
                // 链接服务器
                this.tradeUtil.ConnServer(this.cmbAccountType.SelectedIndex, this.cmbBrokerType.SelectedIndex);
                this.callBackF(this.tradeUtil.eventParam);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void TradeReleaseSub()
        {
            if (!this.isRealEmu)
            {
                // 实时真实处理时，需要释放链接资源
                this.tradeUtil.TradeRelease();
            }
        }

        /// <summary>
        /// 取得代码前面的前缀
        /// </summary>
        /// <returns></returns>
        protected override string GetStockCdBefChar(string stockCd)
        {
            if (stockCd.StartsWith("6"))
            {
                return "sh";
            }
            else
            {
                return "sz";
            }
        }

        /// <summary>
        /// 通过代码列表取得数据
        /// </summary>
        /// <param name="noList"></param>
        /// <returns></returns>
        protected override void GetDataByCdList(List<string> noList, List<GuPiaoInfo> dataLst)
        {
            // 从Sina取得基础数据
            string url = "http://hq.sinajs.cn/list=" + string.Join(",", noList.ToArray());
            string result = Util.HttpGet(url, string.Empty, Encoding.UTF8);
            if (!string.IsNullOrEmpty(result) && result.Length > 20)
            {
                string[] guPiao = result.Split(';');
                for (int i = 0; i < guPiao.Length; i++)
                {
                    if (string.IsNullOrEmpty(guPiao[i]) || guPiao[i].Length < 10)
                    {
                        break;
                    }

                    string[] lines = guPiao[i].Split('=');
                    string[] details = lines[1].Split(',');

                    GuPiaoInfo item = new GuPiaoInfo();
                    dataLst.Add(item);

                    item.fundcode = noList[i].Substring(2, 6);
                    item.jinriKaipanVal = details[1];
                    item.zuoriShoupanVal = details[2];
                    item.currentVal = details[3];
                    item.zuigaoVal = details[4];
                    item.zuidiVal = details[5];
                    item.jingmaiInVal = details[6];
                    item.jingmaiOutVal = details[7];
                    item.chengjiaoShu = details[8];
                    item.chengjiaoJine = details[9];
                    item.gushuIn1 = details[10];
                    item.valIn1 = details[11];
                    item.gushuIn2 = details[12];
                    item.valIn2 = details[13];
                    item.gushuIn3 = details[14];
                    item.valIn3 = details[15];
                    item.gushuIn4 = details[16];
                    item.valIn4 = details[17];
                    item.gushuIn5 = details[18];
                    item.valIn5 = details[19];
                    item.gushuOut1 = details[20];
                    item.valOut1 = details[21];
                    item.gushuOut2 = details[22];
                    item.valOut2 = details[23];
                    item.gushuOut3 = details[24];
                    item.valOut3 = details[25];
                    item.gushuOut4 = details[26];
                    item.valOut4 = details[27];
                    item.gushuOut5 = details[28];
                    item.valOut5 = details[29];
                    item.date = details[30];
                    item.time = details[31].Replace(":", "");
                }
            }
        }

        /// <summary>
        /// 检查当前数据的买卖点标志
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override BaseDataInfo CheckCurDataBuySellFlg(List<BaseDataInfo> stockInfos, GuPiaoInfo item, int time)
        {
            // 取得分型的数据
            FenXing fenXing = new FenXing();
            fenXing.needResetData = false;
            List<BaseDataInfo> fenxingInfo =
                fenXing.DoFenXingSp(stockInfos, this.configInfo, this.dataFilter[0].ToString().PadLeft(6, '0'), null);

            return fenxingInfo[0];
        }

        /// <summary>
        /// 买操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="buyCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        protected override bool BuyStock(string cd, int buyCnt, decimal price)
        {
            if (this.isRealEmu)
            {
                base.BuyStock(cd, buyCnt, price);
            }
            else
            {
                this.tradeUtil.BuyStock(cd, (uint)buyCnt, (float)price, BuySellType.QuickBuy);
            }

            return true;
        }

        /// <summary>
        /// 卖操作
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="buyCnt"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        protected override bool SellStock(string cd, int sellCnt, decimal price)
        {
            if (this.isRealEmu)
            {
                base.SellStock(cd, sellCnt, price);
            }
            else
            {
                this.tradeUtil.SellStock(cd, (uint)sellCnt, (float)price, BuySellType.QuickSell);
            }

            return true;
        }

        /// <summary>
        /// 取得当天交易信息
        /// </summary>
        /// <param name="todayGuPiao"></param>
        protected override void GetTodayTradeInfo(List<OrderInfo> todayGuPiao)
        {
            if (!this.isRealEmu)
            {
                this.tradeUtil.GetTodayPiaoInfo(this.todayGuPiao);
            }
        }

        /// <summary>
        /// 是否可以开始取数据
        /// </summary>
        /// <returns></returns>
        protected override bool CanGetData()
        {
            int time = this.CheckTime(DateTime.Now.ToString("HHmmss"));
            if (this.dataFilter.Contains(time))
            {
                if (!this.curRoundDataEnd)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this.curRoundDataEnd = false;
                return false;
            }
        }

        /// <summary>
        /// 过滤停牌的数据
        /// </summary>
        protected override void CheckTingPaiData()
        {
            string url = "http://data.eastmoney.com/tfpxx/";
            string result = Util.HttpGet(url, string.Empty, Encoding.GetEncoding("GB2312"));
            if (!string.IsNullOrEmpty(result))
            {
                int idx = result.IndexOf("defjson:  {pages:1,data:[\"");
                if (idx <= 0)
                {
                    return;
                }

                string tingpaiInfo = result.Substring(idx + 26, result.IndexOf("]},") - idx - 26);
                if (string.IsNullOrEmpty(tingpaiInfo))
                {
                    return;
                }

                string[] tingpaiData = Regex.Split(tingpaiInfo, "\",\"", RegexOptions.IgnoreCase);
                for (int i = 0; i < tingpaiData.Length; i++)
                {
                    this.allStockCd.Remove(tingpaiData[i].Substring(0, 6));
                }
            }
        }

        /// <summary>
        /// 处理当前取得的数据
        /// 设置当前区间的高低点
        /// </summary>
        protected override bool AddRealTimeData(List<GuPiaoInfo> data)
        {
            int time = this.CheckTime(DateTime.Now.ToString("HHmmss"));
            bool isRangeTime = this.dataFilter.Contains(time);
            foreach (GuPiaoInfo item in data)
            {
                if (this.stockCdData.ContainsKey(item.fundcode))
                {
                    List<BaseDataInfo> hstData = this.stockCdData[item.fundcode];
                    BaseDataInfo lastItem = hstData[hstData.Count - 1];
                    lastItem.DayVal = Convert.ToDecimal(item.currentVal);
                    if (lastItem.DayVal > lastItem.DayMaxVal)
                    {
                        lastItem.DayMaxVal = lastItem.DayVal;
                    }
                    if (lastItem.DayVal < lastItem.DayMinVal)
                    {
                        lastItem.DayMinVal = lastItem.DayVal;
                    }

                    if (isRangeTime && !this.curRoundDataEnd)
                    {
                        lastItem = new BaseDataInfo();
                        lastItem.Code = item.fundcode;
                        lastItem.DayVal = Convert.ToDecimal(item.currentVal);
                        lastItem.DayMaxVal = lastItem.DayVal;
                        lastItem.DayMinVal = lastItem.DayVal;
                        hstData.Insert(0, lastItem);
                    }
                }
            }

            if (isRangeTime)
            {
                if (this.curRoundDataEnd)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                this.curRoundDataEnd = false;

                return false;
            }
        }

        #endregion

        #region 私有方法

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

        #endregion
    }
}
