using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class GuPiaoInfo
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string fundcode { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 今日开盘价
        /// </summary>
        public string jinriKaipanVal { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public string zuoriShoupanVal { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public string currentVal { get; set; }

        /// <summary>
        /// 当前秒数
        /// </summary>
        public int secCounter { get; set; }

        /// <summary>
        /// 记录上一个最小时间段的数据
        /// </summary>
        public float lastVal { get; set; }

        /// <summary>
        /// 记录多个时间段的走势信息
        /// </summary>
        public List<float> valsList { get; set; }

        /// <summary>
        /// 当前趋势（B开头为买，S开头为卖，连续的相同字母越多，越强烈）
        /// </summary>
        public StringBuilder curTrend { get; set; }

        /// <summary>
        /// 自动卖的等待时间
        /// </summary>
        public int sellWaitTime { get; set; }

        /// <summary>
        /// 当前的等待时间
        /// </summary>
        public int curSellWaitTime { get; set; }

        /// <summary>
        /// 自动买的等待时间
        /// </summary>
        public int buyWaitTime { get; set; }

        /// <summary>
        /// 今日最高价
        /// </summary>
        public string zuigaoVal { get; set; }

        /// <summary>
        /// 今日最低价
        /// </summary>
        public string zuidiVal { get; set; }

        /// <summary>
        /// 竞买价（买一报价）
        /// </summary>
        public string jingmaiInVal { get; set; }

        /// <summary>
        /// 竞卖价（卖一报价）
        /// </summary>
        public string jingmaiOutVal { get; set; }

        /// <summary>
        /// 成交股票数（通常要除一百）
        /// </summary>
        public string chengjiaoShu { get; set; }

        /// <summary>
        /// 成交金额（元为单位）
        /// </summary>
        public string chengjiaoJine { get; set; }

        /// <summary>
        /// 申请股数 买一
        /// </summary>
        public string gushuIn1 { get; set; }

        /// <summary>
        /// 申请报价 买一
        /// </summary>
        public string valIn1 { get; set; }

        /// <summary>
        /// 申请股数 买二
        /// </summary>
        public string gushuIn2 { get; set; }

        /// <summary>
        /// 申请报价 买二
        /// </summary>
        public string valIn2 { get; set; }

        /// <summary>
        /// 申请股数 买三
        /// </summary>
        public string gushuIn3 { get; set; }

        /// <summary>
        /// 申请报价 买三
        /// </summary>
        public string valIn3 { get; set; }

        /// <summary>
        /// 申请股数 买四
        /// </summary>
        public string gushuIn4 { get; set; }

        /// <summary>
        /// 申请报价 买四
        /// </summary>
        public string valIn4 { get; set; }

        /// <summary>
        /// 申请股数 买五
        /// </summary>
        public string gushuIn5 { get; set; }

        /// <summary>
        /// 申请报价 买五
        /// </summary>
        public string valIn5 { get; set; }

        /// <summary>
        /// 申请股数 卖一
        /// </summary>
        public string gushuOut1 { get; set; }

        /// <summary>
        /// 申请报价 卖一
        /// </summary>
        public string valOut1 { get; set; }

        /// <summary>
        /// 申请股数 卖二
        /// </summary>
        public string gushuOut2 { get; set; }

        /// <summary>
        /// 申请报价 卖二
        /// </summary>
        public string valOut2 { get; set; }

        /// <summary>
        /// 申请股数 卖三
        /// </summary>
        public string gushuOut3 { get; set; }

        /// <summary>
        /// 申请报价 卖三
        /// </summary>
        public string valOut3 { get; set; }

        /// <summary>
        /// 申请股数 卖四
        /// </summary>
        public string gushuOut4 { get; set; }

        /// <summary>
        /// 申请报价 卖四
        /// </summary>
        public string valOut4 { get; set; }

        /// <summary>
        /// 申请股数 卖五
        /// </summary>
        public string gushuOut5 { get; set; }

        /// <summary>
        /// 申请报价 卖五
        /// </summary>
        public string valOut5 { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string time { get; set; }

        /// <summary>
        /// 可用股数
        /// </summary>
        public uint CanUseCount { get; set; }

        /// <summary>
        /// 总股数
        /// </summary>
        public uint TotalCount { get; set; }
    }
}
