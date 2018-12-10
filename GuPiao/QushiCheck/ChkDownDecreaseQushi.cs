using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuPiao
{
    /// <summary>
    /// 下跌递减趋势检查
    /// </summary>
    public class ChkDownDecreaseQushi : QushiBase
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
            decimal diffVal = 0;
            int maxCnt = stockInfos.Count - 1;
            while (index < maxCnt)
            {
                if (stockInfos[index].Value * Consts.LIMIT_VAL < stockInfos[index + 1].Value)
                {
                    decimal tmp = stockInfos[index + 1].Value - (stockInfos[index].Value * Consts.LIMIT_VAL);
                    if (tmp >= diffVal)
                    {
                        diffVal = tmp;
                        this.qushiDays++;
                        index++;
                        continue;
                    }
                    else
                    {
                        break;
                    }
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
