using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 上涨转折趋势检查
    /// </summary>
    public class ChkUpBreakQushi : QushiBase
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

            // 最后一天开始向下走
            if (stockInfos[0].DayVal * Consts.LIMIT_VAL < stockInfos[1].DayVal)
            {
                // 以前都是向上走
                this.qushiDays = 0;
                int index = 1;
                int maxCnt = stockInfos.Count - 1;
                while (index < maxCnt)
                {
                    if (stockInfos[index].DayVal > stockInfos[index + 1].DayVal * Consts.LIMIT_VAL)
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
            else
            {
                return false;
            }
        }

        #endregion
    }
}
