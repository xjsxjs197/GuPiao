using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        protected override bool ChkQushi(List<KeyValuePair<string, decimal>> stockInfos)
        {
            this.qushiDays = 0;
            int index = 0;
            int maxCnt = stockInfos.Count - 1;
            while (index < maxCnt)
            {
                if (stockInfos[index].Value > stockInfos[index + 1].Value * Consts.LIMIT_VAL)
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
