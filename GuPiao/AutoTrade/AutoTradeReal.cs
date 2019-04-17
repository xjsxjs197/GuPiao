using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;
using DataProcess.FenXing;

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

        #endregion

        #region 子类重写父类的虚方法

        /// <summary>
        /// 初始化其他
        /// </summary>
        protected override void InitOther()
        {
            // 修改系统时间
            this.ChangeSystemTime();

            // 设置异步关联信息
            this.tradeUtil.SetCallBack(this.callBackF);
            this.tradeUtil.SetGuPiaoInfo(this.guPiaoBaseInfo);

            // 设置交易的基础配置信息
            this.tradeUtil.Init(this.cmbAccountType, this.cmbBrokerType);
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
            // 链接服务器
            this.tradeUtil.ConnServer(this.cmbAccountType.SelectedIndex, this.cmbBrokerType.SelectedIndex);
            this.callBackF(this.tradeUtil.eventParam);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void TradeReleaseSub()
        {
            this.tradeUtil.TradeRelease();
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

                    item.fundcode = noList[i];
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
            // 判断当前的数据，是否已经追加到历史数据中
            BaseDataInfo lastItem = stockInfos[0];
            string stockCd = lastItem.Code;
            string lastTime = time.ToString().PadLeft(6, '0');
            if (lastItem.Day.EndsWith(lastTime))
            {
                return null;
            }

            // 处理最后的数据
            lastItem = new BaseDataInfo();
            lastItem.Code = stockCd;
            lastItem.Day = this.tradeDate.ToString("yyyyMMdd") + lastTime;
            lastItem.DayVal = Convert.ToDecimal(item.currentVal);
            lastItem.DayMaxVal = Convert.ToDecimal(item.zuigaoVal);
            lastItem.DayMinVal = Convert.ToDecimal(item.zuidiVal);
            stockInfos.Insert(0, lastItem);

            // 取得分型的数据
            FenXing fenXing = new FenXing();
            List<BaseDataInfo> fenxingInfo =
                fenXing.DoFenXingSp(stockInfos, this.configInfo, this.dataFilter[0].ToString().PadLeft(6, '0'), null);

            return lastItem;
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
