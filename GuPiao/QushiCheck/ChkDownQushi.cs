using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 下跌趋势检查
    /// </summary>
    public class ChkDownQushi : QushiBase
    {
        #region " 子类重写父类的虚方法 "

        /// <summary>
        /// 检查当前趋势
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns>是否查找成功</returns>
        protected override bool ChkQushi(List<BaseDataInfo> stockInfos)
        {
            this.qushiDays = 0;
            decimal bottom1 = 0;
            decimal bottom2 = 0;
            decimal top1 = 0;
            decimal top2 = 0;

            for (int i = 0; i < stockInfos.Count; i++)
            {
                if (stockInfos[i].PointType == PointType.Bottom)
                {
                    if (bottom1 == 0)
                    {
                        bottom1 = stockInfos[i].DayMinVal;
                        this.qushiDays++;
                    }
                    else if (bottom2 == 0)
                    {
                        bottom2 = stockInfos[i].DayMinVal;
                        this.qushiDays++;
                    }
                }
                else if (stockInfos[i].PointType == PointType.Top)
                {
                    if (top1 == 0)
                    {
                        top1 = stockInfos[i].DayMaxVal;
                        this.qushiDays++;
                    }
                    else if (top2 == 0)
                    {
                        top2 = stockInfos[i].DayMaxVal;
                        this.qushiDays++;
                    }
                }

                if (this.qushiDays == 4)
                {
                    break;
                }
            }

            if (this.qushiDays == 4)
            {
                if (bottom1 < bottom2 && top1 < top2)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
