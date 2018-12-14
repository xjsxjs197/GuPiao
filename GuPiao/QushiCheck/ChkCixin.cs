using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuPiao
{
    /// <summary>
    /// 查看次新
    /// </summary>
    public class ChkCixin : QushiBase
    {
        #region " 子类重写父类的虚方法 "

        /// <summary>
        /// 检查当前趋势
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns>是否查找成功</returns>
        protected override bool ChkQushi(List<KeyValuePair<string, decimal>> stockInfos)
        {
            if (stockInfos.Count > 0 && stockInfos.Count < CIXIN_MIN_DAY)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
