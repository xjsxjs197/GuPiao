﻿using System;
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
        /// 不作为变化的判断范围
        /// </summary>
        private const decimal LIMIT_VAL = (decimal)1.005;

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
        public int DoRealTimeFenXingM30(BaseDataInfo data, int avgDataLen)
        {
            hstData.Add(data);

            this.DoFenXingM30(avgDataLen);

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
        public List<BaseDataInfo> DoFenXingM30(List<BaseDataInfo> data, int avgDataLen)
        {
            hstData.Clear();
            hstData.AddRange(data);

            return this.DoFenXingM30(avgDataLen);
        }

        #endregion

        #region " 公有方法 "

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
                    lastBottomPos = this.GeBefBottomPos(this.hstData, lastIdx, maxCnt);
                    if (lastBottomPos > 0 && lastPoint.DayMinVal > this.hstData[lastBottomPos].DayMinVal * LIMIT_VAL)
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
        private List<BaseDataInfo> DoFenXingM30(int avgDataLen)
        {
            // 设置第一个点
            BaseDataInfo lastPoint = new BaseDataInfo();
            while (this.hstData.Count > 0)
            {
                lastPoint = this.hstData[this.hstData.Count - 1];
                if (lastPoint.Day.IndexOf("100000") > 0)
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

            // 特殊处理30分钟数据
            BaseDataInfo tmpPoint = new BaseDataInfo();
            tmpPoint.DayMaxVal = lastPoint.DayMaxVal;
            tmpPoint.DayMinVal = lastPoint.DayMinVal;
            int maxCnt = this.hstData.Count - 2;
            int lastIdx = maxCnt + 1;
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

                if (this.hstData[i].Day.IndexOf("100000") > 0)
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

                    if (!buyed && this.hstData[i].DayVal > this.hstData[i].DayAvgVal
                        && !this.hstData[i].Day.EndsWith("100000") && !this.hstData[i].Day.EndsWith("150000"))
                    {
                        // 大于日线，并且未买过，判断是否是第二类买点
                        lastBottomPos = this.GeBefBottomPos(this.hstData, lastIdx, maxCnt);
                        if (lastBottomPos > 0 && lastPoint.DayMinVal > this.hstData[lastBottomPos].DayMinVal * LIMIT_VAL)
                        {
                            // 当前低点高于上一个低点，设置第二类买点
                            this.hstData[i].BuySellFlg = 2;
                            buyed = true;
                            buyPrice = this.hstData[i].DayVal;
                            buyDate = this.hstData[i].Day.Substring(0, 8);
                        }
                    }
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
                            lastTopPos = this.GeBefTopVal(this.hstData, lastIdx, maxCnt);
                            if (lastTopPos > 0 && this.hstData[i].DayVal * LIMIT_VAL < this.hstData[lastTopPos].DayMaxVal)
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
                    if (buyed && (this.hstData[i].DayVal < buyPrice * LIMIT_VAL || this.hstData[i].DayVal < this.hstData[i].DayAvgVal)
                        && !this.hstData[i].Day.StartsWith(buyDate))
                    {
                        startingSell = true;
                    }
                }

                //if (chkVal >= 0 && startingSell && (this.hstData[i].DayVal > buyPrice * LIMIT_VAL || this.hstData[i].DayVal > this.hstData[i].DayAvgVal))
                //{
                //    // 已经开始卖的准备，但是趋势开始上升，清除标志位
                //    startingSell = false;
                //}

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
        private int GeBefTopVal(List<BaseDataInfo> fenXingInfo, int idx, int maxCnt)
        {
            for (int i = idx + 1; i < maxCnt; i++)
            {
                if (fenXingInfo[i].CurPointType == PointType.Top)
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
        private int GeBefBottomPos(List<BaseDataInfo> fenXingInfo, int idx, int maxCnt)
        {
            for (int i = idx + 1; i < maxCnt; i++)
            {
                if (fenXingInfo[i].CurPointType == PointType.Bottom)
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
            if (point2.DayMaxVal > point1.DayMaxVal * LIMIT_VAL
                && point2.DayMinVal > point1.DayMinVal * LIMIT_VAL)
            {
                return 1;
            }
            else if (point1.DayMaxVal > point2.DayMaxVal * LIMIT_VAL
                && point1.DayMinVal > point2.DayMinVal * LIMIT_VAL)
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
        private bool hasMoreLowBottom(List<BaseDataInfo> fenXingInfo, int lastBottomPos, int maxCnt)
        {
            for (int i = lastBottomPos + 1; i < maxCnt; i++)
            {
                if (fenXingInfo[i].CurPointType == PointType.Bottom)
                {
                    if (fenXingInfo[i].DayMinVal < fenXingInfo[lastBottomPos].DayMinVal)
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

        #endregion
    }
}