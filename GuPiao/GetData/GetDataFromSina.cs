using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanhua.Common;
using System.Text.RegularExpressions;
using System.IO;
using GuPiao.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GuPiao.GetData
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
            this.endDayForFile = endDay.Replace("-", "").Replace(" ", "").Replace(":", "");
        }
        
        #endregion

        #region " 子类重写父类的虚方法 "

        /// <summary>
        /// 开始获取数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        protected override void StartGetData(string stockCd, List<FilePosInfo> allCsv)
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
            sb.Append(DATA_URL).Append(codeType).Append(stockCd);
            sb.Append("&scale=").Append((int)this.timeRange);
            sb.Append("&datalen=").Append(DATA_LEN_MAX);

            // 取截止今天为止的所有数据
            string result = Util.HttpGet(sb.Append(DATA_LEN_MAX).ToString(), "", encoding);
            if (!string.IsNullOrEmpty(result))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(result);
                if (jArray != null)
                {
                    List<string> allMinuteData = new List<string>();

                    // 取得开始时间
                    string startDay = this.GetExitsStock(allCsv, stockCd);
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
                    else if (string.Compare(startDay, endDay) < 0)
                    {
                        // 读取既存文件的内容
                        sb.Length = 0;
                        sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                        sb.Append(stockCd).Append("_").Append(startDay).Append(".csv");
                        string[] oldFile = File.ReadAllLines(sb.ToString(), Encoding.UTF8);

                        // 取得分钟级别数据
                        string lastDay = this.GetMinuteData(jArray, sb, stockCd, allMinuteData);

                        // 最新数据和旧数据结合
                        bool canMerge = false;
                        for (int i = 1; i < oldFile.Length; i++)
                        {
                            if (!canMerge && oldFile[i].IndexOf(lastDay) >= 0)
                            {
                                canMerge = true;
                                continue;
                            }

                            if (canMerge)
                            {
                                allMinuteData.Add(oldFile[i]);
                            }
                        }

                        // 保存数据文件
                        sb.Length = 0;
                        sb.Append(base.csvFolder).Append(this.timeRange.ToString()).Append("/");
                        sb.Append(stockCd).Append("_").Append(this.endDayForFile).Append(".csv");
                        File.WriteAllLines(sb.ToString(), allMinuteData.ToArray(), Encoding.UTF8);
                    }
                }
            }
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
            allMinuteData.Add(",,,,,,");

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

                if (i == jArray.Count - 1)
                {
                    lastDay = jArray[i]["day"].ToString();
                }
            }

            return lastDay;
        }

        #endregion
    }
}
