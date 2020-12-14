using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 连续跌停趋势检查
    /// </summary>
    public class ChkContinueBottomQushi : QushiBase
    {
        #region " 子类重写父类的虚方法 "

        /// <summary>
        /// 检查当前趋势
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns>是否查找成功</returns>
        protected override bool ChkQushi(List<BaseDataInfo> stockInfos)
        {
            int continueDays = 0;
            int maxCnt = stockInfos.Count - 2;
            bool ret = false;
            decimal dif = (decimal)1.09;

            for (int i = 0; i < maxCnt; i++)
            {
                if (stockInfos[i].DayVal * dif < stockInfos[i + 1].DayVal)
                {
                    continueDays++;
                }
                else
                {
                    continueDays = 0;
                }

                if (continueDays >= 3)
                {
                    stockInfos[i + 2].BuySellFlg = -1;
                    continueDays = 0;
                    ret = true;
                }
            }

            return ret;
        }

        #endregion
    }
}
