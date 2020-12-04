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
        public bool needResetData = false;

        /// <summary>
        /// 保存到目前为止的低点（最多三个）
        /// </summary>
        private List<decimal> bottomPoints = new List<decimal>();

        /// <summary>
        /// 是否设置检查点标识
        /// </summary>
        private bool needSetCheckPoint = false;

        #endregion

        #region " 公有方法 "

        /// <summary>
        /// 初始化
        /// </summary>
        public FenXing()
        {
            this.needSetCheckPoint = false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public FenXing(bool needSetCheckPoint)
        {
            this.needSetCheckPoint = needSetCheckPoint;
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

            if (this.hstData.Count < 3)
            {
                return this.hstData;
            }

            // 分型、画笔
            List<BaseDataInfo> fenxingData = this.DoFenXingComn();

            // 根据笔画线段
            this.DoCheckLine(fenxingData);

            return fenxingData;
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

            // 特殊处理开始的点
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

            if (minMaxInfo == null)
            {
                minMaxInfo = Util.GetMaxMinStock(this.hstData);
            }

            // 分型、画笔
            List<BaseDataInfo> fenxingData = this.DoFenXingSp(emuInfo, startTime, minMaxInfo);

            // 根据笔画线段
            this.DoCheckLine(fenxingData);

            return fenxingData;
        }

        /// <summary>
        /// 根据最后一个元素检查当前的分型
        /// </summary>
        /// <param name="data"></param>
        public List<BaseDataInfo> DoFenXingLastItem(List<BaseDataInfo> data)
        {
            BaseDataInfo curPoint = data[0];
            BaseDataInfo lastChkPoint = data[1].LastChkPoint;
            int lastIdx = data.IndexOf(lastChkPoint);
            int maxCnt = data.Count - 1;
            if (data[1].PointType == PointType.Up || data[1].PointType == PointType.Down)
            {
                lastIdx = 1;
                lastChkPoint = data[1];
                lastChkPoint.DayMaxValTmp = lastChkPoint.DayMaxVal;
                lastChkPoint.DayMinValTmp = lastChkPoint.DayMinVal;
            }
            else if (lastIdx == -1)
            {
                lastIdx = maxCnt;
            }
            curPoint.LastChkPoint = lastChkPoint;

            // 判断两个点的大小关系
            curPoint.PointType = this.ChkPointsVal(curPoint, lastChkPoint);

            if (curPoint.PointType == PointType.Up && lastChkPoint.PointType == PointType.Down)
            {
                // 当前上升，前面是下降，说明前一个点是低点
                if (this.IsRangeOk(lastIdx, maxCnt, data))
                {
                    lastChkPoint.PointType = PointType.Bottom;
                }
            }
            else if (curPoint.PointType == PointType.Down && lastChkPoint.PointType == PointType.Up)
            {
                // 当前下降，前面是上升，说明前一个点是高点
                if (this.IsRangeOk(lastIdx, maxCnt, data))
                {
                    lastChkPoint.PointType = PointType.Top;
                }
            }

            return data;
        }

        /// <summary>
        /// 在分型的数据中划分线段
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void DoCheckLine(List<BaseDataInfo> data)
        {
            List<BaseDataInfo> newData = new List<BaseDataInfo>();
            BaseDataInfo lastPen = new BaseDataInfo();
            bool isUpPen = true;
            int lastIdx = 0;

            // 取得第一个高低点信息
            int maxCnt = data.Count - 1;
            for (int i = maxCnt; i >= 0; i--)
            { 
                if (data[i].PointType == PointType.Top)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (data[j].PointType == PointType.Bottom)
                        {
                            if (data[j].DayMinVal > data[maxCnt].DayVal)
                            {
                                lastIdx = maxCnt;
                                data[lastIdx].PenType = PointType.Bottom;
                                isUpPen = true;
                            }
                            else
                            {
                                lastIdx = i - 1;
                                data[i].PenType = PointType.Top;
                                isUpPen = false;
                            }
                            break;
                        }
                    }
                    break;
                }
                else if (data[i].PointType == PointType.Bottom)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (data[j].PointType == PointType.Top)
                        {
                            if (data[j].DayMaxVal < data[maxCnt].DayVal)
                            {
                                lastIdx = maxCnt;
                                data[lastIdx].PenType = PointType.Top;
                                isUpPen = false;
                            }
                            else
                            {
                                lastIdx = i - 1;
                                data[i].PenType = PointType.Bottom;
                                isUpPen = true;
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            if (lastIdx == 0)
            {
                return;
            }

            // 设置第一个点
            lastIdx = this.GetNextPen(lastIdx, data, lastPen, isUpPen);
            if (lastIdx == -1)
            {
                return;
            }
            lastPen.LastChkPoint = null;

            for (int i = lastIdx - 1; i >= 0; i--)
            {
                BaseDataInfo curPen = new BaseDataInfo();
                curPen.LastChkPoint = lastPen;
                i = this.GetNextPen(i, data, curPen, isUpPen);
                if (i == -1)
                {
                    return;
                }

                // 判断两个点的大小关系
                curPen.PenType = this.ChkPointsVal(curPen, lastPen);

                if (curPen.PenType == PointType.Up)
                {
                    if (lastPen.PenType == PointType.Down)
                    {
                        // 当前上升，前面是下降，说明前一个点是低点
                        data[lastPen.PenBottomPos].PenType = PointType.Bottom;
                        i = lastPen.PenBottomPos;
                        isUpPen = true;

                        // 更新当前的点
                        lastPen = curPen;
                        lastPen.PenType = PointType.Changing;
                    }
                    else
                    {
                        // 更新当前的点
                        lastPen = curPen;
                    }
                }
                else if (curPen.PenType == PointType.Down)
                {
                    if (lastPen.PenType == PointType.Up)
                    {
                        // 当前下降，前面是上升，说明前一个点是高点
                        data[lastPen.PenTopPos].PenType = PointType.Top;
                        i = lastPen.PenTopPos;
                        isUpPen = false;

                        // 更新当前的点
                        lastPen = curPen;
                        lastPen.PenType = PointType.Changing;
                    }
                    else
                    {
                        // 更新当前的点
                        lastPen = curPen;
                    }
                }
                else
                {
                    PointType lastType = lastPen.PenType;
                    lastPen = curPen;
                    lastPen.PenType = lastType;
                }
            }
        }

        /// <summary>
        /// 是否设置检查点标识
        /// </summary>
        /// <param name="needSetCheckPoint"></param>
        public void SetCheckPoint(bool needSetCheckPoint)
        {
            this.needSetCheckPoint = needSetCheckPoint;
        }

        #endregion

        #region " 私有方法 "

        /// <summary>
        /// 分型处理(共通)
        /// </summary>
        /// <returns></returns>
        private List<BaseDataInfo> DoFenXingComn()
        {
            // 设置第一个点
            BaseDataInfo lastChkPoint = this.hstData[this.hstData.Count - 1];
            lastChkPoint.DayMaxValTmp = lastChkPoint.DayMaxVal;
            lastChkPoint.DayMinValTmp = lastChkPoint.DayMinVal;
            lastChkPoint.LastChkPoint = null;
            int lastIdx = this.hstData.Count - 1;

            int maxCnt = this.hstData.Count - 2;

            for (int i = maxCnt; i >= 0; i--)
            {
                BaseDataInfo curPoint = this.hstData[i];
                curPoint.LastChkPoint = lastChkPoint;
                
                // 判断两个点的大小关系
                curPoint.PointType = this.ChkPointsVal(curPoint, lastChkPoint);

                if (curPoint.PointType == PointType.Up && lastChkPoint.PointType == PointType.Down)
                {
                    // 当前上升，前面是下降，说明前一个点是低点
                    if (this.IsRangeOk(lastIdx, maxCnt, this.hstData))
                    {
                        lastChkPoint.PointType = PointType.Bottom;
                        if (this.needSetCheckPoint)
                        {
                            curPoint.CheckPoint = -1;
                        }
                    }
                }
                else if (curPoint.PointType == PointType.Down && lastChkPoint.PointType == PointType.Up)
                {
                    // 当前下降，前面是上升，说明前一个点是高点
                    if (this.IsRangeOk(lastIdx, maxCnt, this.hstData))
                    {
                        lastChkPoint.PointType = PointType.Top;
                        if (this.needSetCheckPoint)
                        {
                            curPoint.CheckPoint = 1;
                        }
                    }
                }

                // 更新当前的点
                if (curPoint.PointType == PointType.Up || curPoint.PointType == PointType.Down)
                {
                    lastIdx = i;
                    lastChkPoint = curPoint;
                    lastChkPoint.DayMaxValTmp = lastChkPoint.DayMaxVal;
                    lastChkPoint.DayMinValTmp = lastChkPoint.DayMinVal;
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
            return this.DoFenXingComn();
        }

        /// <summary>
        /// 取得下一个向下的笔数据（主要是高低点数据）
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="data"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetNextDownPen(int idx, List<BaseDataInfo> data, BaseDataInfo penInfo)
        { 
            while (idx >= 0)
            {
                if (data[idx].PointType == PointType.Top)
                {
                    penInfo.DayMaxVal = data[idx].DayMaxVal;
                    penInfo.DayMaxValTmp = data[idx].DayMaxVal;
                    penInfo.PenTopPos = idx;
                    penInfo.Day = data[idx].Day;
                }
                else if (data[idx].PointType == PointType.Bottom)
                {
                    penInfo.DayMinVal = data[idx].DayMinVal;
                    penInfo.DayMinValTmp = data[idx].DayMinVal;
                    penInfo.PenBottomPos = idx;
                    penInfo.Day = data[idx].Day;
                    break;
                }

                idx--;
            }

            return idx;
        }

        /// <summary>
        /// 取得下一个向上的笔数据（主要是高低点数据）
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="data"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetNextUpPen(int idx, List<BaseDataInfo> data, BaseDataInfo penInfo)
        {
            while (idx >= 0)
            {
                if (data[idx].PointType == PointType.Bottom)
                {
                    penInfo.DayMinVal = data[idx].DayMinVal;
                    penInfo.DayMinValTmp = data[idx].DayMinVal;
                    penInfo.PenBottomPos = idx;
                    penInfo.Day = data[idx].Day;
                }
                else if (data[idx].PointType == PointType.Top)
                {
                    penInfo.DayMaxVal = data[idx].DayMaxVal;
                    penInfo.DayMaxValTmp = data[idx].DayMaxVal;
                    penInfo.PenTopPos = idx;
                    penInfo.Day = data[idx].Day;
                    break;
                }

                idx--;
            }

            return idx;
        }

        /// <summary>
        /// 取得下一个笔信息
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="data"></param>
        /// <param name="penInfo"></param>
        /// <param name="isUpPen"></param>
        /// <returns></returns>
        private int GetNextPen(int idx, List<BaseDataInfo> data, BaseDataInfo penInfo, bool isUpPen)
        {
            if (isUpPen)
            {
                idx = this.GetNextDownPen(idx, data, penInfo);
            }
            else
            {
                idx = this.GetNextUpPen(idx, data, penInfo);
            }
            return idx;
        }

        ///// <summary>
        ///// 分型处理(30分钟)
        ///// </summary>
        ///// <returns></returns>
        //private List<BaseDataInfo> DoFenXingSp(BuySellSetting emuInfo, string startTime, decimal[] minMaxInfo)
        //{
        //    //this.bottomPoints.Clear();

        //    // 设置第一个点
        //    BaseDataInfo lastPoint = this.hstData[this.hstData.Count - 1];

        //    int avgDataLen = emuInfo.AvgDataLen;
        //    int buyStrongth = emuInfo.ButStrongth;

        //    // 特殊处理分钟数据
        //    BaseDataInfo tmpPoint = new BaseDataInfo();
        //    tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
        //    tmpPoint.DayMinVal = lastPoint.DayMinVal;
        //    int maxCnt = this.hstData.Count - 2;
        //    int lastIdx = maxCnt + 1;
        //    if (this.needResetData)
        //    {
        //        for (int i = maxCnt; i >= 0; i--)
        //        {
        //            if (this.hstData[i].DayMaxVal > tmpPoint.DayMaxVal)
        //            {
        //                tmpPoint.DayMaxVal = this.hstData[i].DayMaxVal;
        //            }
        //            if (this.hstData[i].DayMinVal < tmpPoint.DayMinVal)
        //            {
        //                tmpPoint.DayMinVal = this.hstData[i].DayMinVal;
        //            }

        //            if (this.hstData[i].Day.EndsWith(startTime))
        //            {
        //                tmpPoint.DayMaxVal = this.hstData[i].DayMaxVal;
        //                tmpPoint.DayMinVal = this.hstData[i].DayMinVal;
        //            }
        //            else
        //            {
        //                this.hstData[i].DayMaxVal = tmpPoint.DayMaxVal;
        //                this.hstData[i].DayMinVal = tmpPoint.DayMinVal;
        //            }
        //        }
        //    }

        //    //// 设置日均线数据
        //    //this.SetDayAverageLineInfo(avgDataLen);

        //    // 开始比较
        //    int chkVal = 0;
        //    int lastChkVal = 0;
        //    int lastTopPos = -1;
        //    int lastBottomPos = -1;
        //    bool buyed = false;
        //    bool startingSell = false;
        //    string buyDate = string.Empty;
        //    decimal buyPrice = 0;
        //    tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
        //    tmpPoint.DayMinVal = lastPoint.DayMinVal;
        //    for (int i = maxCnt; i >= 0; i--)
        //    {
        //        // 判断两个点的大小关系
        //        chkVal = this.ChkPointsVal(this.hstData[i], tmpPoint);

        //        if (chkVal > 0 && lastChkVal < 0)
        //        {
        //            // 当前上升，前面是下降，说明前一个点是低点
        //            lastPoint.PointType = PointType.Bottom;

        //            //// 判断当前的低点是否是第一类买点
        //            //if (!buyed && this.IsBuyPointOne(lastPoint, this.hstData[i].DayVal, lastIdx, maxCnt)
        //            //    && !this.hstData[i].Day.EndsWith(startTime) && !this.hstData[i].Day.EndsWith("150000"))
        //            //    //&& string.Compare(this.hstData[i].Day, "133000") > 0 && !this.hstData[i].Day.EndsWith("150000"))
        //            //{
        //            //    this.bottomPoints.RemoveRange(0, 2);
        //            //    this.hstData[i].BuySellFlg = 1;
        //            //    buyed = true;
        //            //    buyPrice = this.hstData[i].DayVal;
        //            //    buyDate = this.hstData[i].Day.Substring(0, 8);
        //            //}

        //            /*if (!buyed && this.hstData[i].DayVal > this.hstData[i].DayAvgVal
        //                && !this.hstData[i].Day.EndsWith(startTime) && !this.hstData[i].Day.EndsWith("150000"))
        //            {
        //                // 大于日线，并且未买过，判断是否是第二类买点
        //                lastBottomPos = this.GeBefBottomPos(lastIdx, maxCnt);
        //                if (lastBottomPos > 0 && lastPoint.DayMinVal > this.hstData[lastBottomPos].DayMinVal * Consts.LIMIT_VAL
        //                    && this.GetQushiStrongth(lastBottomPos, lastIdx, maxCnt, minMaxInfo) >= buyStrongth)
        //                {
        //                    // 当前低点高于上一个低点，设置第二类买点
        //                    this.hstData[i].BuySellFlg = 2;
        //                    buyed = true;
        //                    buyPrice = this.hstData[i].DayVal;
        //                    buyDate = this.hstData[i].Day.Substring(0, 8);
        //                }
        //            }*/
        //        }
        //        else if (chkVal < 0)
        //        {
        //            if (lastChkVal > 0)
        //            {
        //                // 当前下降，前面是上升，说明前一个点是高点
        //                lastPoint.PointType = PointType.Top;

        //                //if (buyed && !this.hstData[i].Day.StartsWith(buyDate))
        //                //{
        //                //    // 设置第一类卖点
        //                //    this.hstData[i].BuySellFlg = -1;
        //                //    buyed = false;
        //                //}

        //                //if (!startingSell && buyed && !this.hstData[i].Day.StartsWith(buyDate))
        //                //{
        //                //    // 已经买过，并且不是当天，判断是否设置第一类卖点
        //                //    lastTopPos = this.GeBefTopVal(lastIdx, maxCnt);
        //                //    if (lastTopPos > 0 && this.hstData[i].DayVal * Consts.LIMIT_VAL < this.hstData[lastTopPos].DayMaxVal)
        //                //    {
        //                //        // 当前高点比前一个高点底，设置第一类卖点
        //                //        //this.hstData[i].BuySellFlg = -1;
        //                //        //buyed = false;
        //                //        startingSell = true;
        //                //    }
        //                //}
        //                //else if (startingSell)
        //                //{
        //                //    // 开始卖的准备，并且趋势开始上升
        //                //    this.hstData[i].BuySellFlg = -1;
        //                //    buyed = false;
        //                //    startingSell = false;
        //                //}
        //            }

        //            //// 已经买过，只要下降到买入价，或者下降到日线一下，并且不是当天，开始卖的准备
        //            //if (buyed && (this.hstData[i].DayVal < buyPrice || this.hstData[i].DayVal < this.hstData[i].DayAvgVal)
        //            //    && !this.hstData[i].Day.StartsWith(buyDate))
        //            //{
        //            //    startingSell = true;
        //            //}
        //        }
        //        //else if (chkVal > 0)
        //        //{
        //        //    // 判断当前的低点是否是第一类买点
        //        //    if (!buyed && this.IsBuyPointOne(this.hstData[i].DayVal, lastIdx, maxCnt)
        //        //        && !this.hstData[i].Day.EndsWith(startTime) && !this.hstData[i].Day.EndsWith("150000"))
        //        //    {
        //        //        this.bottomPoints.RemoveRange(0, 2);
        //        //        this.hstData[i].BuySellFlg = 1;
        //        //        buyed = true;
        //        //        buyPrice = this.hstData[i].DayVal;
        //        //        buyDate = this.hstData[i].Day.Substring(0, 8);
        //        //    }
        //        //}

        //        // 更新当前的点
        //        if (chkVal != 0)
        //        {
        //            lastChkVal = chkVal;
        //            lastPoint = this.hstData[i];
        //            lastIdx = i;
        //            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
        //            tmpPoint.DayMinVal = lastPoint.DayMinVal;
        //        }
        //    }

        //    return this.hstData;
        //}

        /// <summary>
        /// 判断当前的顶或底距离上一个底或顶的距离是否合理，中间至少有一条线
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="maxCnt"></param>
        /// <returns></returns>
        private bool IsRangeOk(int idx, int maxCnt, List<BaseDataInfo> data)
        {
            int oldIdx = idx;
            PointType pt = PointType.Changing;
            while (idx <= maxCnt)
            {
                pt = data[idx].PointType;
                if (pt == PointType.Top
                    || pt == PointType.Bottom)
                {
                    if (idx >= oldIdx + 4)
                    {
                        return true;
                    }
                    else
                    {
                        data[idx].PointType = (pt == PointType.Top ? PointType.Up : PointType.Down);
                        return false;
                    }
                }

                idx++;
            }

            return true;
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
                if (this.hstData[i].PointType == PointType.Top)
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
                if (this.hstData[i].PointType == PointType.Bottom)
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
        private PointType ChkPointsVal(BaseDataInfo curPoint, BaseDataInfo lastChkPoint)
        {
            // 判断是否变化
            if (curPoint.DayMaxVal > lastChkPoint.DayMaxValTmp * Consts.LIMIT_VAL
                && curPoint.DayMinVal > lastChkPoint.DayMinValTmp * Consts.LIMIT_VAL)
            {
                return PointType.Up;
            }
            else if (curPoint.DayMaxVal * Consts.LIMIT_VAL < lastChkPoint.DayMaxValTmp
                && curPoint.DayMinVal * Consts.LIMIT_VAL < lastChkPoint.DayMinValTmp)
            {
                return PointType.Down;
            }
            else if (lastChkPoint.LastChkPoint != null)
            {
                // 前后有包含关系，或者没有变化
                if (lastChkPoint.LastChkPoint.PointType == PointType.Up)
                {
                    lastChkPoint.DayMaxValTmp = Math.Max(lastChkPoint.DayMaxValTmp, curPoint.DayMaxVal);
                    lastChkPoint.DayMinValTmp = Math.Max(lastChkPoint.DayMinValTmp, curPoint.DayMinVal);
                }
                else if (lastChkPoint.LastChkPoint.PointType == PointType.Down)
                {
                    lastChkPoint.DayMaxValTmp = Math.Min(lastChkPoint.DayMaxValTmp, curPoint.DayMaxVal);
                    lastChkPoint.DayMinValTmp = Math.Min(lastChkPoint.DayMinValTmp, curPoint.DayMinVal);
                }

                return PointType.Changing;
            }

            return PointType.Changing;
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
                if (this.hstData[i].PointType == PointType.Bottom)
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
        private bool IsBuyPointOne(BaseDataInfo lastBottomPoint, decimal curVal, int lastIdx, int maxCnt)
        { 
            if (this.bottomPoints.Count == 0)
            {
                this.bottomPoints.Add(lastBottomPoint.DayMinVal);
            }
            else if (this.bottomPoints.Count == 1)
            {
                this.bottomPoints.Add(lastBottomPoint.DayMinVal);
                if (!(this.bottomPoints[1] * Consts.DIFF_VAL < this.bottomPoints[0]))
                {
                    this.bottomPoints.RemoveAt(0);
                }
            }
            else if (this.bottomPoints.Count == 2)
            {
                this.bottomPoints.Add(lastBottomPoint.DayMinVal);
                if (!(this.bottomPoints[2] * Consts.DIFF_VAL < this.bottomPoints[1]))
                {
                    this.bottomPoints.RemoveRange(0, 2);
                }
            }
            else
            {
                if (lastBottomPoint.DayMinVal * Consts.DIFF_VAL < this.bottomPoints[this.bottomPoints.Count - 1])
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

            return this.IsBuyPointOne(curVal, lastIdx, maxCnt);
        }

        /// <summary>
        /// 判断当前的低点是否是第一类买点
        /// </summary>
        /// <param name="lastBottomPoint"></param>
        /// <param name="curVal"></param>
        /// <returns></returns>
        private bool IsBuyPointOne(decimal curVal, int lastIdx, int maxCnt)
        {
            if (this.bottomPoints.Count == 3 && curVal > this.bottomPoints[1] * Consts.LIMIT_VAL)
            //if (this.bottomPoints.Count == 3)
            {
                int lastTopPos = this.GeBefTopVal(lastIdx, maxCnt);
                if (lastTopPos > 0)
                {
                    int lastTopPos2 = this.GeBefTopVal(lastTopPos, maxCnt);
                    if (lastTopPos2 > 0 && this.hstData[lastTopPos].DayMaxVal > this.hstData[lastTopPos2].DayMaxVal * Consts.DIFF_VAL)
                    {
                        return true;
                    }
                }

            }

            return false;
        }

        #endregion
    }
}
