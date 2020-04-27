using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 最后是顶分型趋势检查
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

            for (int i = 0; i < stockInfos.Count; i++)
            {
                if (stockInfos[i].PointType == PointType.Top)
                {
                    if (i <= 3)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (stockInfos[i].PointType == PointType.Bottom)
                {
                    return false;
                }
            }

            return false;
        }

        #endregion
    }
}
