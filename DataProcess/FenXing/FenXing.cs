using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace DataProcess.FenXing
{
    /// <summary>
    /// 分型处理
    /// </summary>
    public class FenXing
    {
        #region " 全局变量 "

        /// <summary>
        /// 历史数据信息
        /// </summary>
        private List<BaseDataInfo> hstData = new List<BaseDataInfo>();

        /// <summary>
        /// 是否需要重置数据
        /// </summary>
        public bool needResetData = true;

        /// <summary>
        /// 保存到目前为止的低点（最多三个）
        /// </summary>
        private List<decimal> bottomPoints = new List<decimal>();

        #endregion

        #region " 公有方法 "

        /// <summary>
        /// 实时分型处理(共通)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int DoRealTimeFenXingComn(BaseDataInfo data)
        {
            hstData.Add(data);

            this.DoFenXingComn();

            return data.BuySellFlg;
        }

        /// <summary>
        /// 实时分型处理(30分钟)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int DoRealTimeFenXingSp(BaseDataInfo data, BuySellSetting emuInfo, string startTime)
        {
            hstData.Add(data);

            decimal[] minMaxInfo = Util.GetMaxMinStock(this.hstData);

            this.DoFenXingSp(emuInfo, startTime, minMaxInfo);

            return data.BuySellFlg;
        }

        /// <summary>
        /// 分型处理(共通)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<BaseDataInfo> DoFenXingComn(List<BaseDataInfo> data)
        {
            hstData.Clear();
            hstData.AddRange(data);

            return this.DoFenXingComn();
        }

        /// <summary>
        /// 分型处理(30分钟)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<BaseDataInfo> DoFenXingSp(List<BaseDataInfo> data, BuySellSetting emuInfo, string startTime, decimal[] minMaxInfo)
        {
            hstData.Clear();
            hstData.AddRange(data);

            if (minMaxInfo == null)
            {
                minMaxInfo = Util.GetMaxMinStock(this.hstData);
            }

            return this.DoFenXingSp(emuInfo, startTime, minMaxInfo);
        }

        #endregion

        #region " 私有方法 "

        /// <summary>
        /// 分型处理(共通)
        /// </summary>
        /// <returns></returns>
        private List<BaseDataInfo> DoFenXingComn()
        {
            if (this.hstData.Count < 3)
            {
                return this.hstData;
            }

            // 设置第一个点
            BaseDataInfo lastPoint = this.hstData[this.hstData.Count - 1];
            BaseDataInfo tmpPoint = new BaseDataInfo();
            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
            tmpPoint.DayMinVal = lastPoint.DayMinVal;
            int chkVal = 0;
            int lastChkVal = 0;
            int lastTopPos = -1;
            int lastBottomPos = -1;

            int maxCnt = this.hstData.Count - 2;
            int lastIdx = maxCnt + 1;

            for (int i = maxCnt; i >= 0; i--)
            {
                // 判断两个点的大小关系
                chkVal = this.ChkPointsVal(this.hstData[i], tmpPoint);

                if (chkVal > 0 && lastChkVal < 0)
                {
                    // 当前上升，前面是下降，说明前一个点是低点
                    lastPoint.CurPointType = PointType.Bottom;

                    // 判断是否是第二类买点
                    lastBottomPos = this.GeBefBottomPos(lastIdx, maxCnt);
                    if (lastBottomPos > 0 && lastPoint.DayMinVal > this.hstData[lastBottomPos].DayMinVal * Consts.LIMIT_VAL)
                    {
                        // 当前低点高于上一个低点，设置第二类买点
                        this.hstData[i].BuySellFlg = 2;
                    }
                }
                else if (chkVal < 0 && lastChkVal > 0)
                {
                    // 当前下降，前面是上升，说明前一个点是高点
                    lastPoint.CurPointType = PointType.Top;

                    // 设置第一类卖点
                    this.hstData[i].BuySellFlg = -1;
                }

                // 更新当前的点
                if (chkVal != 0)
                {
                    lastChkVal = chkVal;
                    lastPoint = this.hstData[i];
                    lastIdx = i;
                    tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
                    tmpPoint.DayMinVal = lastPoint.DayMinVal;
                }
            }

            return this.hstData;
        }

        /// <summary>
        /// 分型处理(30分钟)
        /// </summary>
        /// <returns></returns>
        private List<BaseDataInfo> DoFenXingSp(BuySellSetting emuInfo, string startTime, decimal[] minMaxInfo)
        {
            this.bottomPoints.Clear();

            // 设置第一个点
            BaseDataInfo lastPoint = new BaseDataInfo();
            while (this.hstData.Count > 0)
            {
                lastPoint = this.hstData[this.hstData.Count - 1];
                if (lastPoint.Day.EndsWith(startTime))
                {
                    break;
                }
                else
                {
                    this.hstData.Remove(lastPoint);
                }
            }

            if (this.hstData.Count < 3)
            {
                return this.hstData;
            }

            int avgDataLen = emuInfo.AvgDataLen;
            int buyStrongth = emuInfo.ButStrongth;

            // 特殊处理分钟数据
            BaseDataInfo tmpPoint = new BaseDataInfo();
            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
            tmpPoint.DayMinVal = lastPoint.DayMinVal;
            int maxCnt = this.hstData.Count - 2;
            int lastIdx = maxCnt + 1;
            if (this.needResetData)
            {
                for (int i = maxCnt; i >= 0; i--)
                {
                    if (this.hstData[i].DayMaxVal > tmpPoint.DayMaxVal)
                    {
                        tmpPoint.DayMaxVal = this.hstData[i].DayMaxVal;
                    }
                    if (this.hstData[i].DayMinVal < tmpPoint.DayMinVal)
                    {
                        tmpPoint.DayMinVal = this.hstData[i].DayMinVal;
                    }

                    if (this.hstData[i].Day.EndsWith(startTime))
                    {
                        tmpPoint.DayMaxVal = this.hstData[i].DayMaxVal;
                        tmpPoint.DayMinVal = this.hstData[i].DayMinVal;
                    }
                    else
                    {
                        this.hstData[i].DayMaxVal = tmpPoint.DayMaxVal;
                        this.hstData[i].DayMinVal = tmpPoint.DayMinVal;
                    }
                }
            }

            // 设置日均线数据
            this.SetDayAverageLineInfo(avgDataLen);

            // 开始比较
            int chkVal = 0;
            int lastChkVal = 0;
            int lastTopPos = -1;
            int lastBottomPos = -1;
            bool buyed = false;
            bool startingSell = false;
            string buyDate = string.Empty;
            decimal buyPrice = 0;
            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
            tmpPoint.DayMinVal = lastPoint.DayMinVal;
            for (int i = maxCnt; i >= 0; i--)
            {
                // 判断两个点的大小关系
                chkVal = this.ChkPointsVal(this.hstData[i], tmpPoint);

                if (chkVal > 0 && lastChkVal < 0)
                {
                    // 当前上升，前面是下降，说明前一个点是低点
                    lastPoint.CurPointType = PointType.Bottom;

                    // 判断当前的低点是否是第一类买点
                    if (!buyed && this.IsBuyPointOne(lastPoint, this.hstData[i].DayVal)
                        && !this.hstData[i].Day.EndsWith(startTime) && !this.hstData[i].Day.EndsWith("150000"))
                    {
                        this.bottomPoints.RemoveRange(0, 2);
                        this.hstData[i].BuySellFlg = 1;
                        buyed = true;
                        buyPrice = this.hstData[i].DayVal;
                        buyDate = this.hstData[i].Day.Substring(0, 8);
                    }

                    /*if (!buyed && this.hstData[i].DayVal > this.hstData[i].DayAvgVal
                        && !this.hstData[i].Day.EndsWith(startTime) && !this.hstData[i].Day.EndsWith("150000"))
                    {
                        // 大于日线，并且未买过，判断是否是第二类买点
                        lastBottomPos = this.GeBefBottomPos(lastIdx, maxCnt);
                        if (lastBottomPos > 0 && lastPoint.DayMinVal > this.hstData[lastBottomPos].DayMinVal * Consts.LIMIT_VAL
                            && this.GetQushiStrongth(lastBottomPos, lastIdx, maxCnt, minMaxInfo) >= buyStrongth)
                        {
                            // 当前低点高于上一个低点，设置第二类买点
                            this.hstData[i].BuySellFlg = 2;
                            buyed = true;
                            buyPrice = this.hstData[i].DayVal;
                            buyDate = this.hstData[i].Day.Substring(0, 8);
                        }
                    }*/
                }
                else if (chkVal < 0)
                {
                    if (lastChkVal > 0)
                    {
                        // 当前下降，前面是上升，说明前一个点是高点
                        lastPoint.CurPointType = PointType.Top;

                        if (!startingSell && buyed && !this.hstData[i].Day.StartsWith(buyDate))
                        {
                            // 已经买过，并且不是当天，判断是否设置第一类卖点
                            lastTopPos = this.GeBefTopVal(lastIdx, maxCnt);
                            if (lastTopPos > 0 && this.hstData[i].DayVal * Consts.LIMIT_VAL < this.hstData[lastTopPos].DayMaxVal)
                            {
                                // 当前高点比前一个高点底，设置第一类卖点
                                //this.hstData[i].BuySellFlg = -1;
                                //buyed = false;
                                startingSell = true;
                            }
                        }
                        else if (startingSell)
                        {
                            // 开始卖的准备，并且趋势开始上升
                            this.hstData[i].BuySellFlg = -1;
                            buyed = false;
                            startingSell = false;
                        }
                    }

                    // 已经买过，只要下降到买入价，或者下降到日线一下，并且不是当天，开始卖的准备
                    if (buyed && (this.hstData[i].DayVal < buyPrice || this.hstData[i].DayVal < this.hstData[i].DayAvgVal)
                        && !this.hstData[i].Day.StartsWith(buyDate))
                    {
                        startingSell = true;
                    }
                }
                else if (chkVal > 0)
                {
                    // 判断当前的低点是否是第一类买点
                    if (!buyed && this.IsBuyPointOne(this.hstData[i].DayVal)
                        && !this.hstData[i].Day.EndsWith(startTime) && !this.hstData[i].Day.EndsWith("150000"))
                    {
                        this.bottomPoints.RemoveRange(0, 2);
                        this.hstData[i].BuySellFlg = 1;
                        buyed = true;
                        buyPrice = this.hstData[i].DayVal;
                        buyDate = this.hstData[i].Day.Substring(0, 8);
                    }
                }

                // 更新当前的点
                if (chkVal != 0)
                {
                    lastChkVal = chkVal;
                    lastPoint = this.hstData[i];
                    lastIdx = i;
                    tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
                    tmpPoint.DayMinVal = lastPoint.DayMinVal;
                }
            }

            return this.hstData;
        }

        /// <summary>
        /// 取得前一个顶分型的位置
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="idx"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        private int GeBefTopVal(int idx, int maxCnt)
        {
            for (int i = idx + 1; i < maxCnt; i++)
            {
                if (this.hstData[i].CurPointType == PointType.Top)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 取得前一个底分型的位置
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="idx"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        private int GeBefBottomPos(int idx, int maxCnt)
        {
            for (int i = idx + 1; i < maxCnt; i++)
            {
                if (this.hstData[i].CurPointType == PointType.Bottom)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 判断两个点的关系
        /// </summary>
        /// <param name="point2"></param>
        /// <param name="point1"></param>
        /// <returns></returns>
        private int ChkPointsVal(BaseDataInfo point2, BaseDataInfo point1)
        {
            // 判断是否变化
            if (point2.DayMaxVal > point1.DayMaxVal * Consts.LIMIT_VAL
                && point2.DayMinVal > point1.DayMinVal * Consts.LIMIT_VAL)
            {
                return 1;
            }
            else if (point1.DayMaxVal > point2.DayMaxVal * Consts.LIMIT_VAL
                && point1.DayMinVal > point2.DayMinVal * Consts.LIMIT_VAL)
            {
                return -1;
            }
            else
            {
                // 前后有包含关系，或者没有变化
                point1.DayMaxVal = point2.DayMaxVal;
                return 0;
            }
        }

        /// <summary>
        /// 查找更低的地点
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="lastBottomPos"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        private bool hasMoreLowBottom(int lastBottomPos, int maxCnt)
        {
            for (int i = lastBottomPos + 1; i < maxCnt; i++)
            {
                if (this.hstData[i].CurPointType == PointType.Bottom)
                {
                    if (this.hstData[i].DayMinVal * Consts.LIMIT_VAL < this.hstData[lastBottomPos].DayMinVal)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 设置日均线数据
        /// </summary>
        /// <param name="avgDataLen">均值的长度（1为半小时）</param>
        private void SetDayAverageLineInfo(int avgDataLen)
        {
            int index = 0;
            int jibieCount = avgDataLen - 1;
            int maxCount = this.hstData.Count - avgDataLen;
            decimal total = 0;

            while (index <= maxCount)
            {
                total = 0;
                for (int i = 0; i <= jibieCount; i++)
                {
                    total += this.hstData[index + i].DayVal;
                }

                this.hstData[index].DayAvgVal = total / avgDataLen;

                index++;
            }
        }

        /// <summary>
        /// 取得当前点时，趋势的强度（角度）
        /// </summary>
        /// <param name="fenXingInfo"></param>
        /// <param name="lastBottomPos"></param>
        /// <param name="idx"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        private int GetQushiStrongth(int lastBottomPos, int idx, int maxCnt, decimal[] minMaxInfo)
        {
            int lastTopPos = this.GeBefTopVal(idx, maxCnt);
            if (lastBottomPos == -1 || lastTopPos == -1)
            {
                return 0;
            }

            int imgWidth = (this.hstData.Count + 2) * Consts.IMG_X_STEP;
            int x1 = imgWidth - (lastBottomPos * Consts.IMG_X_STEP + Consts.IMG_X_STEP);
            int x2 = imgWidth - (lastTopPos * Consts.IMG_X_STEP + Consts.IMG_X_STEP);
            decimal step = Util.GetYstep(minMaxInfo);
            int y1 = (int)((this.hstData[lastBottomPos].DayMaxVal - minMaxInfo[0]) * step);
            int y2 = (int)((this.hstData[lastTopPos].DayMinVal - minMaxInfo[0]) * step);

            int retVal = (int)(Math.Atan2((y2 - y1), (x2 - x1)) * 180 / Math.PI);

            return retVal;
        }

        /// <summary>
        /// 判断当前的低点是否是第一类买点
        /// </summary>
        /// <param name="lastBottomPoint"></param>
        /// <param name="curVal"></param>
        /// <returns></returns>
        private bool IsBuyPointOne(BaseDataInfo lastBottomPoint, decimal curVal)
        { 
            if (this.bottomPoints.Count == 0)
            {
                this.bottomPoints.Add(lastBottomPoint.DayMinVal);
            }
            else if (this.bottomPoints.Count == 1)
            {
                this.bottomPoints.Add(lastBottomPoint.DayMinVal);
                if (!(this.bottomPoints[1] * Consts.LIMIT_VAL < this.bottomPoints[0]))
                {
                    this.bottomPoints.RemoveAt(0);
                }
            }
            else if (this.bottomPoints.Count == 2)
            {
                this.bottomPoints.Add(lastBottomPoint.DayMinVal);
                if (!(this.bottomPoints[2] * Consts.LIMIT_VAL < this.bottomPoints[1]))
                {
                    this.bottomPoints.RemoveRange(0, 2);
                }
            }
            else
            {
                if (lastBottomPoint.DayMinVal * Consts.LIMIT_VAL < this.bottomPoints[this.bottomPoints.Count - 1])
                {
                    this.bottomPoints.Add(lastBottomPoint.DayMinVal);
                    this.bottomPoints.RemoveAt(0);
                }
                else
                {
                    this.bottomPoints.Clear();
                    this.bottomPoints.Add(lastBottomPoint.DayMinVal);
                }
            }

            return this.IsBuyPointOne(curVal);
        }

        /// <summary>
        /// 判断当前的低点是否是第一类买点
        /// </summary>
        /// <param name="lastBottomPoint"></param>
        /// <param name="curVal"></param>
        /// <returns></returns>
        private bool IsBuyPointOne(decimal curVal)
        {
            if (this.bottomPoints.Count == 3 && curVal > this.bottomPoints[1])
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
