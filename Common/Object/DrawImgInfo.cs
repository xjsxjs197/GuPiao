using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Common
{
    /// <summary>
    /// 画图时的信息
    /// </summary>
    public class DrawImgInfo
    {
        /// <summary>
        /// 画买点时的刷子
        /// </summary>
        public Brush BuyBush { get; set; }

        /// <summary>
        /// 画卖点时的刷子
        /// </summary>
        public Brush SellBush { get; set; }

        /// <summary>
        /// 天蓝的刷子
        /// </summary>
        public Brush BlueVioletBush { get; set; }

        /// <summary>
        /// 画买卖点时的字体
        /// </summary>
        public Font BuySellFont { get; set; }

        /// <summary>
        /// 通常文字的字体
        /// </summary>
        public Font NormalFont { get; set; }

        /// <summary>
        /// 名称文字的字体
        /// </summary>
        public Font NameFont { get; set; }

        /// <summary>
        /// 画虚线的笔
        /// </summary>
        public Pen DashLinePen { get; set; }

        /// <summary>
        /// 画通常线的笔
        /// </summary>
        public Pen NormalLinePen { get; set; }

        /// <summary>
        /// 黑色线的笔
        /// </summary>
        public Pen BlackLinePen { get; set; }

        /// <summary>
        /// 绿色线的笔
        /// </summary>
        public Pen GreenLinePen { get; set; }

        /// <summary>
        /// 红色线的笔
        /// </summary>
        public Pen RedLinePen { get; set; }

        /// <summary>
        /// 深橙色线的笔
        /// </summary>
        public Pen DarkOrangeLinePen { get; set; }

        /// <summary>
        /// 深绿色线的笔
        /// </summary>
        public Pen DarkGreenLinePen { get; set; }

        /// <summary>
        /// 深蓝色线的笔
        /// </summary>
        public Pen DarkBlueLinePen { get; set; }
    }
}
