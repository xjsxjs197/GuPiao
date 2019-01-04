using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 下跌转折趋势检查（转折了多少天）
    /// </summary>
    public class ChkDownBreakDaysQushi : QushiBase
    {
        #region " 子类重写父类的虚方法 "

        /// <summary>
        /// 检查当前趋势
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns>是否查找成功</returns>
        protected override bool ChkQushi(List<BaseDataInfo> stockInfos)
        {
            if (stockInfos.Count < BREAK_CHK_MIN_LEN)
            {
                return false;
            }

            // 最后N天开始向上走
            for (int i = 0; i < base.checkDays; i++)
            {
                if (stockInfos[i].DayVal > stockInfos[i + 1].DayVal * Consts.LIMIT_VAL)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }

            // 以前都是向下走
            this.qushiDays = 0;
            int index = base.checkDays;
            int maxCnt = stockInfos.Count - 1;
            while (index < maxCnt)
            {
                if (stockInfos[index].DayVal * Consts.LIMIT_VAL < stockInfos[index + 1].DayVal)
                {
                    this.qushiDays++;
                    index++;
                    continue;
                }
                else
                {
                    break;
                }
            }

            return base.IsContinueQushi();
        }

        #endregion
    }
}
