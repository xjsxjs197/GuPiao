using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GuPiao
{
    /// <summary>
    /// 趋势检查基础类
    /// </summary>
    public abstract class QushiBase
    {
        #region " 私有变量 "

        /// <summary>
        /// 查找转折趋势时最小的数据
        /// </summary>
        protected const int BREAK_CHK_MIN_LEN = 20;

        /// <summary>
        /// 查看次新时的天数限定
        /// </summary>
        protected const int CIXIN_MIN_DAY = 25;

        /// <summary>
        /// 判断趋势时，上涨或下跌的力度
        /// </summary>
        protected const decimal UP_DOWN_DIFF = 1.15M;

        /// <summary>
        /// 查找趋势时，转折点离现在的最小的长度
        /// </summary>
        protected const int QUSHI_MIN_LEN = 3;

        /// <summary>
        /// 检查的天数
        /// </summary>
        protected int checkDays;

        /// <summary>
        /// 当前趋势的天数
        /// </summary>
        protected int qushiDays = 0;

        #endregion

        #region " 初始化 "

        /// <summary>
        /// 初始化
        /// </summary>
        public QushiBase()
        {
        }

        #endregion

        #region " 公共方法 "

        /// <summary>
        /// 开始查找
        /// </summary>
        public bool StartCheck(List<BaseDataInfo> stockInfos)
        {
            return this.ChkQushi(stockInfos);
        }

        /// <summary>
        /// 设置检查的天数
        /// </summary>
        /// <param name="days"></param>
        public void SetCheckDays(int days)
        {
            this.checkDays = days;
        }

        #endregion

        #region " 子类需要重写的虚方法 "

        /// <summary>
        /// 检查当前趋势
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns>是否查找成功</returns>
        protected virtual bool ChkQushi(List<BaseDataInfo> stockInfos)
        {
            return false;
        }

        /// <summary>
        /// 判断是否是连续的趋势
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsContinueQushi()
        {
            if (this.qushiDays >= Consts.QUSHI_CONTINUE_DAYS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
