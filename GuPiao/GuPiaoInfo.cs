
using System.Collections.Generic;
namespace GuPiaoTool
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
        /// 记录秒的数据
        /// </summary>
        public List<float> hisVal { get; set; }

        /// <summary>
        /// 记录多秒的均值数据
        /// 3秒，5秒，10秒等，每个数据两个值，当前值和上一个值
        /// </summary>
        public List<float[]> secondsPoints { get; set; }

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
        /// 高位卖点
        /// </summary>
        public float topSellPoint { get; set; }

        /// <summary>
        /// 低位卖点
        /// </summary>
        public float bottomSellPoint { get; set; }

        /// <summary>
        /// 犹豫的点（上下浮动的点）
        /// </summary>
        public float waitPoint { get; set; }

        /// <summary>
        /// 正在等待卖
        /// </summary>
        public bool isWaitingSell { get; set; }

        /// <summary>
        /// 正在等待买
        /// </summary>
        public bool isWaitingBuy { get; set; }

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
