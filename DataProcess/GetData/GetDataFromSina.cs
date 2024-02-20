using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DataProcess.GetData
{
    /// <summary>
    /// 从新浪取得数据
    /// </summary>
    public class GetDataFromSina : GetDataBase
    {
        #region " 全局变量 "

        /// <summary>
        /// 时间级别
        /// </summary>
        private TimeRange timeRange;

        /// <summary>
        /// 数据源
        /// </summary>
        private const string DATA_URL = @"http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData?symbol=";

        /// <summary>
        /// 取得数据的最大长度
        /// </summary>
        private const int DATA_LEN_MAX = 1023;

        /// <summary>
        /// 文件名使用的日期
        /// </summary>
        private string endDayForFile;

        /// <summary>
        /// 文件名使用的日期
        /// </summary>
        private string endDayForFileCopy;

        /// <summary>
        /// 检查数据时，过滤的时间
        /// </summary>
        private List<string> chkTime = new List<string>();

        #endregion

        #region " 初始化 "

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="csvFolder"></param>
        /// <param name="endDay"></param>
        public GetDataFromSina(string csvFolder, string endDay, TimeRange timeRange)
            : base(csvFolder, endDay)
        {
            this.timeRange = timeRange;
            this.endDayForFile = endDay;
            this.endDayForFileCopy = endDay;
            switch (timeRange)
            {
                case TimeRange.M15:
                    this.chkTime.AddRange(new string[] { "1500", "3000", "4500", "0000" });
                    break;

                case TimeRange.M30:
                    this.chkTime.AddRange(new string[] { "3000", "0000" });
                    break;

                case TimeRange.Day:
                    this.chkTime.AddRange(new string[] { "150000" });
                    this.endDayForFileCopy += "150000";
                    break;
            }
        }
        
        #endregion

        #region " 子类重写父类的虚方法 "

        /// <summary>
        /// 检查股票代码是否存在，如果存在返回代码、名称的数组
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        protected override string StartCheckStockCd(string stockCd)
        {
            stockCd = stockCd.PadLeft(6, '0');

            Encoding encoding = Encoding.GetEncoding("GBK");
            StringBuilder sb = new StringBuilder();

            sb.Append("http://suggest3.sinajs.cn/suggest/type=11&key=").Append(stockCd);

            // 取截止今天为止的所有数据
            string result = Util.HttpGet(sb.ToString(), "", encoding);
            if (!string.IsNullOrEmpty(result) && !"null".Equals(result, System.StringComparison.OrdinalIgnoreCase))
            {
                string codeInfo;
                // 判断类型
                if (stockCd.StartsWith("6"))
                {
                    codeInfo = "sh" + stockCd;
                }
                else
                {
                    codeInfo = "sz" + stockCd;
                }

                try
                {
                    int idx = result.LastIndexOf(codeInfo);
                    if (idx > 0)
                    {
                        // sh000002,A股指数,
                        int nameStartIdx = idx + 9;
                        int nameEndIdx = result.IndexOf(",", nameStartIdx);
                        return stockCd + " " + result.Substring(nameStartIdx, nameEndIdx - nameStartIdx);
                    }
                }
                catch (Exception e)
                {
                    return "处理Json " + stockCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace;
                }
            }

            return null;
        }

        /// <summary>
        /// 开始获取数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        protected override string StartGetData(string stockCd, List<FilePosInfo> allCsv)
        {
            Encoding encoding = Encoding.GetEncoding("GBK");
            string codeType;
            StringBuilder sb = new StringBuilder();

            // 判断类型
            if (stockCd.StartsWith("6"))
            {
                codeType = "sh";
            }
            else
            {
                codeType = "sz";
            }

            // 参数设置
            int maxLen = DATA_LEN_MAX;
            sb.Append(DATA_URL).Append(codeType).Append(stockCd);
            sb.Append("&scale=").Append((int)this.timeRange);
            sb.Append("&datalen=");

            // 取得开始时间
            string startDay = this.GetExitsStock(allCsv, stockCd);
            if (this.endDayForFile.Equals(startDay))
            {
                // 最新文件已经存在
                return "NO_NEED_DATA";
            }
            else if (!string.IsNullOrEmpty(startDay))
            {
                // 设置取得数据的最大长度
                if (this.timeRange == TimeRange.M5)
                {
                    maxLen = DATA_LEN_MAX / 2;
                }
                else if (this.timeRange == TimeRange.M15)
                {
                    maxLen = DATA_LEN_MAX / 5;
                }
                else if (this.timeRange == TimeRange.M30)
                {
                    maxLen = DATA_LEN_MAX / 10;
                }
                else if (this.timeRange == TimeRange.Day)
                {
                    maxLen = DATA_LEN_MAX / 20;
                }
            }


            // 取截止今天为止的所有数据
            string result = Util.HttpGet(sb.Append(maxLen).ToString(), "", encoding);
            if (!string.IsNullOrEmpty(result) && !"null".Equals(result, System.StringComparison.OrdinalIgnoreCase))
            {
                JArray jArray = null;
                try
                {
                    jArray = (JArray)JsonConvert.DeserializeObject(result);
                }
                catch (Exception e)
                {
                    return "处理Json " + stockCd + " 数据时发生异常：\r\n" + result + "\r\n" + e.Message + "\r\n" + e.StackTrace;
                }

                if (jArray != null)
                {
                    List<string> allMinuteData = new List<string>();

                    if (string.IsNullOrEmpty(startDay))
                    {
                        // 取得分钟级别数据
                        this.GetMinuteData(jArray, sb, stockCd, allMinuteData);

                        // 保存数据文件
                        sb.Length = 0;
                        sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                        sb.Append(stockCd).Append("_").Append(this.endDayForFile).Append(".csv");
                        File.WriteAllLines(sb.ToString(), allMinuteData.ToArray(), Encoding.UTF8);
                    }
                    else if (string.Compare(startDay, this.endDayForFile) < 0)
                    {
                        // 读取既存文件的内容
                        sb.Length = 0;
                        sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                        sb.Append(stockCd).Append("_").Append(startDay).Append(".csv");
                        string oldFilePath = sb.ToString();
                        string[] oldFile = File.ReadAllLines(oldFilePath, Encoding.UTF8);

                        // 取得分钟级别数据
                        this.GetMinuteData(jArray, sb, stockCd, allMinuteData);

                        // 最新数据和旧数据结合
                        List<string> newMinuteData = new List<string>();
                        newMinuteData.AddRange(oldFile);
                        if (newMinuteData.Count <= 1)
                        {
                            newMinuteData.Clear();
                            newMinuteData.AddRange(allMinuteData.ToArray());
                        }
                        else
                        {
                            string lastDay = newMinuteData[1];
                            lastDay = lastDay.Substring(0, lastDay.IndexOf(","));
                            int mergeIdx = -1;
                            for (int i = 1; i < allMinuteData.Count; i++)
                            {
                                if (allMinuteData[i].IndexOf(lastDay) >= 0)
                                {
                                    mergeIdx = i;
                                    break;
                                }
                            }
                            if (mergeIdx > 0)
                            {
                                mergeIdx--;
                                while (mergeIdx > 0)
                                {
                                    newMinuteData.Insert(1, allMinuteData[mergeIdx]);
                                    mergeIdx--;
                                }
                            }
                        }

                        // 保存数据文件
                        sb.Length = 0;
                        sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                        sb.Append(stockCd).Append("_").Append(this.endDayForFile).Append(".csv");
                        File.WriteAllLines(sb.ToString(), newMinuteData.ToArray(), Encoding.UTF8);

                        // 删除旧的文件
                        File.Delete(oldFilePath);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 开始获取数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        protected override string StartCopyData(string stockCd, List<FilePosInfo> allCsv)
        {
            StringBuilder sb = new StringBuilder();

            // 取得开始时间
            string startDay = this.GetExitsStock(allCsv, stockCd);
            if (this.endDayForFile.Equals(startDay))
            {
                // 最新文件已经存在
                return "NO_NEED_DATA";
            }

            // 从5分钟数据，取得当前数据
            List<string> mCurData = this.GetCurData(this.GetM5Data(stockCd));

            if (string.IsNullOrEmpty(startDay))
            {
                // 保存数据文件
                sb.Length = 0;
                sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                sb.Append(stockCd).Append("_").Append(this.endDayForFile).Append(".csv");
                File.WriteAllLines(sb.ToString(), mCurData.ToArray(), Encoding.UTF8);
            }
            else if (string.Compare(startDay, this.endDayForFile) < 0)
            {
                // 读取既存文件的内容
                sb.Length = 0;
                sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                sb.Append(stockCd).Append("_").Append(startDay).Append(".csv");
                string oldFilePath = sb.ToString();
                string[] oldFile = File.ReadAllLines(oldFilePath, Encoding.UTF8);

                // 最新数据和旧数据结合
                List<string> newMinuteData = new List<string>();
                newMinuteData.AddRange(oldFile);
                if (newMinuteData.Count <= 1)
                {
                    newMinuteData.Clear();
                    newMinuteData.AddRange(mCurData.ToArray());
                }
                else
                {
                    string lastDay = newMinuteData[1];
                    lastDay = lastDay.Substring(0, lastDay.IndexOf(","));
                    int mergeIdx = -1;
                    for (int i = 1; i < mCurData.Count; i++)
                    {
                        if (mCurData[i].IndexOf(lastDay) >= 0)
                        {
                            mergeIdx = i;
                            break;
                        }
                    }
                    if (mergeIdx > 0)
                    {
                        mergeIdx--;
                        while (mergeIdx > 0)
                        {
                            newMinuteData.Insert(1, mCurData[mergeIdx]);
                            mergeIdx--;
                        }
                    }
                }

                // 保存数据文件
                sb.Length = 0;
                sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                sb.Append(stockCd).Append("_").Append(this.endDayForFile).Append(".csv");
                File.WriteAllLines(sb.ToString(), newMinuteData.ToArray(), Encoding.UTF8);

                // 删除旧的文件
                File.Delete(oldFilePath);
            }

            return string.Empty;
        }

        #endregion

        #region " 私有方法 "

        /// <summary>
        /// 取得分钟级别数据
        /// </summary>
        /// <param name="jArray"></param>
        /// <returns></returns>
        private string GetMinuteData(JArray jArray, StringBuilder sb, string stockCd, List<string> allMinuteData)
        {
            string lastDay = string.Empty;

            allMinuteData.Clear();
            allMinuteData.Add("日期,代码,名称,收盘价,最高价,最低价,开盘价");

            for (int i = jArray.Count - 1; i >= 0; i--)
            {
                sb.Length = 0;

                sb.Append(jArray[i]["day"]).Append(",");
                sb.Append(stockCd).Append(",");
                sb.Append("").Append(",");
                sb.Append(jArray[i]["close"]).Append(",");
                sb.Append(jArray[i]["high"]).Append(",");
                sb.Append(jArray[i]["low"]).Append(",");
                sb.Append(jArray[i]["open"]);

                allMinuteData.Add(sb.ToString());

                if (i == 0)
                {
                    lastDay = jArray[i]["day"].ToString();
                }
            }

            return lastDay;
        }

        /// <summary>
        /// 取得5分钟数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        private List<string> GetM5Data(string stockCd)
        {
            List<string> m5Data = new List<string>();

            string[] allLine = File.ReadAllLines(base.csvFolder + TimeRange.M5.ToString() + "/" + stockCd
                + "_" + this.endDayForFileCopy + ".csv");

            if (allLine.Length > 1)
            {
                m5Data.AddRange(allLine);
                m5Data.RemoveAt(0);

                // 最后一条数据必须是09：35的数据
                int index = m5Data.Count - 1;
                while (index >= 0)
                {
                    string[] lastLine = m5Data[index].Split(',');
                    string lastDay = lastLine[0].Replace("-", "").Replace(" ", "").Replace(":", "");
                    if (lastDay.EndsWith("093500"))
                    {
                        break;
                    }
                    else
                    {
                        m5Data.RemoveAt(index);
                        index = m5Data.Count - 1;
                    }
                }
            }

            if (m5Data.Count <= 1)
            {
                throw new Exception("5分钟数据不存在！");
            }

            return m5Data;
        }

        /// <summary>
        /// 从5分钟数据中，计算出当前分钟的数据
        /// </summary>
        /// <param name="m5Data"></param>
        /// <returns></returns>
        private List<string> GetCurData(List<string> m5Data)
        {
            List<string> curData = new List<string>();
            string[] curLine = m5Data[m5Data.Count - 1].Split(',');
            string stockCd = curLine[1];
            string openVal = curLine[6];
            decimal topVal = decimal.Parse(curLine[4]);
            decimal bottomVal = decimal.Parse(curLine[5]);
            decimal tmpVal = 0;
            StringBuilder sb = new StringBuilder();

            for (int i = m5Data.Count - 2; i >= 0; i--)
            {
                curLine = m5Data[i].Split(',');

                // 更新最大值
                tmpVal = decimal.Parse(curLine[4]);
                if (tmpVal > topVal)
                {
                    topVal = tmpVal;
                }

                // 更新最小值
                tmpVal = decimal.Parse(curLine[5]);
                if (tmpVal < bottomVal)
                {
                    bottomVal = tmpVal;
                }

                // 判断是否到了需要保存的点
                if (this.IsCurData(curLine[0].Replace(":", "")))
                {
                    sb.Length = 0;
                    sb.Append(curLine[0]).Append(",");
                    sb.Append(stockCd).Append(",");
                    sb.Append("").Append(",");
                    sb.Append(curLine[3]).Append(",");
                    sb.Append(topVal.ToString()).Append(",");
                    sb.Append(bottomVal.ToString()).Append(",");
                    sb.Append(openVal);

                    curData.Add(sb.ToString());

                    // 取下一条数据
                    if (i > 0)
                    {
                        i--;
                        curLine = m5Data[i].Split(',');
                        openVal = curLine[6];
                        topVal = decimal.Parse(curLine[4]);
                        bottomVal = decimal.Parse(curLine[5]);
                    }
                }
            }

            curData.Reverse();

            // 天数据特殊处理（取得分秒信息）
            if (this.timeRange == TimeRange.Day)
            {
                for (int i = 0; i < curData.Count; i++)
                {
                    curData[i] = curData[i].Replace(" 15:00:00", "");
                }
            }

            curData.Insert(0, ",,,,,,");

            return curData;
        }

        /// <summary>
        /// 当前数据是否需要保存
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private bool IsCurData(string dt)
        {
            foreach (string chkDt in this.chkTime)
            {
                if (dt.EndsWith(chkDt))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
