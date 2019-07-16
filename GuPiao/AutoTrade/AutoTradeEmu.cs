using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using DataProcess.FenXing;

namespace GuPiao
{
    /// <summary>
    /// 模拟自动交易
    /// </summary>
    public class AutoTradeEmu : AutoTradeBase
    {
        #region 全局变量

        #endregion

        #region 子类重写父类的虚方法

        /// <summary>
        /// 初始化其他
        /// </summary>
        protected override void InitOther()
        {
            this.dataFilterIdx = 0;

            // 模拟时，需要设置初始化结果
            TradeEventParam eventParam = new TradeEventParam();
            eventParam.IsSuccess = true;
            eventParam.Msg = "模拟：初始化成功";
            eventParam.CurOpt = CurOpt.InitEvent;

            this.callBackF(eventParam);
        }

        /// <summary>
        /// 获取定时器间隔
        /// 模拟交易的间隔是0.1秒
        /// </summary>
        /// <returns></returns>
        protected override double GetTradeTimer()
        {
            return 1;
        }

        /// <summary>
        /// 登陆服务器
        /// </summary>
        protected override void LoginServerSub()
        {
            // 模拟时，需要设置登陆结果
            TradeEventParam eventParam = new TradeEventParam();
            eventParam.IsSuccess = true;
            eventParam.Msg = "模拟：登陆服务器成功";
            eventParam.CurOpt = CurOpt.LoginEvent;

            this.callBackF(eventParam);
        }

        /// <summary>
        /// 通过代码列表取得数据
        /// </summary>
        /// <param name="noList"></param>
        /// <returns></returns>
        protected override void GetDataByCdList(List<string> noList, List<GuPiaoInfo> dataLst)
        {
            // 设置取当前数据的时间
            string curData = this.tradeDate.ToString("yyyyMMdd");
            string curTime = string.Empty;
            if (this.dataFilterIdx < this.dataFilter.Count)
            {
                curTime = this.dataFilter[this.dataFilterIdx].ToString().PadLeft(6, '0');
                curData += curTime;
            }

            foreach (string stockCd in noList)
            {
                if (this.stockCdData.ContainsKey(stockCd))
                {
                    List<BaseDataInfo> stockInfo = this.stockCdData[stockCd];
                    for (int i = 0; i < stockInfo.Count; i++)
                    {
                        if (stockInfo[i].Day.Equals(curData))
                        {
                            GuPiaoInfo item = new GuPiaoInfo();
                            item.fundcode = stockCd;
                            item.time = curTime;
                            item.currentVal = stockInfo[i].DayVal.ToString();
                            item.zuidiVal = stockInfo[i].DayMinVal.ToString();
                            item.zuigaoVal = stockInfo[i].DayMaxVal.ToString();
                            item.valIn1 = item.currentVal;
                            item.valIn2 = item.currentVal;
                            item.valIn3 = item.currentVal;
                            item.valOut1 = item.currentVal;
                            item.valOut2 = item.currentVal;
                            item.valOut3 = item.currentVal;

                            dataLst.Add(item);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查当前数据的买卖点标志
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override BaseDataInfo CheckCurDataBuySellFlg(List<BaseDataInfo> stockInfos, GuPiaoInfo item)
        {
            List<BaseDataInfo> nowEndStockInfo = new List<BaseDataInfo>();
            string curDateTime = this.tradeDate.ToString("yyyyMMdd") + this.CheckTime(item.time).ToString().PadLeft(6, '0');
            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                if (string.Compare(stockInfos[i].Day, curDateTime) <= 0)
                {
                    nowEndStockInfo.Add(stockInfos[i]);
                }
                else
                {
                    break;
                }
            }

            nowEndStockInfo.Reverse();

            // 取得分型的数据
            FenXing fenXing = new FenXing();
            List<BaseDataInfo> fenxingInfo =
                fenXing.DoFenXingSp(nowEndStockInfo, this.configInfo, this.dataFilter[0].ToString().PadLeft(6, '0'), null);

            return fenxingInfo[0];
        }

        #endregion

        #region 私有方法

        #endregion
    }
}
