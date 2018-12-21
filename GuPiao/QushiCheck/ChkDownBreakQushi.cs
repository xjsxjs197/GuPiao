using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GuPiao.Common;

namespace GuPiao
{
    /// <summary>
    /// 下跌转折趋势检查
    /// </summary>
    public class ChkDownBreakQushi : QushiBase
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

            // 最后一天开始向上走
            if (stockInfos[0].DayVal > stockInfos[1].DayVal * Consts.LIMIT_VAL)
            {
                // 以前都是向下走
                this.qushiDays = 0;
                int index = 1;
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
            else
            {
                return false;
            }
        }

        #endregion
    }
}
