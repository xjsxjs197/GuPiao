﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 上涨趋势检查
    /// </summary>
    public class ChkUpQushi : QushiBase
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

            for (int i = 0; i < stockInfos.Count; i++)
            {
                if (this.qushiDays == 0)
                {
                    if (stockInfos[i].PointType == PointType.Bottom)
                    {
                        bottom1 = stockInfos[i].DayMinVal;
                        this.qushiDays++;
                    }
                    else if (stockInfos[i].PointType == PointType.Top)
                    {
                        break;
                    }
                }
                else if (this.qushiDays == 1)
                {
                    if (stockInfos[i].PointType == PointType.Bottom)
                    {
                        break;
                    }
                    else if (stockInfos[i].PointType == PointType.Top)
                    {
                        top1 = stockInfos[i].DayMaxVal;
                        this.qushiDays++;
                    }
                }
                else if (this.qushiDays == 2)
                {
                    if (stockInfos[i].PointType == PointType.Bottom)
                    {
                        bottom2 = stockInfos[i].DayMinVal;
                        this.qushiDays++;
                    }
                    else if (stockInfos[i].PointType == PointType.Top)
                    {
                        break;
                    }
                }
            }

            if (this.qushiDays == 3)
            {
                if (bottom1 > bottom2 * UP_DOWN_DIFF
                    && stockInfos[0].DayVal > top1 * UP_DOWN_DIFF)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
