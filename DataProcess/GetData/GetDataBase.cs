using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Common;

namespace DataProcess.GetData
{
    /// <summary>
    /// 取得数据的基类
    /// </summary>
    public abstract class GetDataBase
    {
        #region " 全局变量 "

        /// <summary>
        /// 数据路径信息
        /// </summary>
        protected string csvFolder;

        /// <summary>
        /// 最新数据时间信息
        /// </summary>
        protected string endDay;

        #endregion

        #region " 初始化 "

        protected GetDataBase(string csvFolder, string endDay)
        {
            this.csvFolder = csvFolder;
            this.endDay = endDay;
        }

        #endregion

        #region " 公共方法 "

        /// <summary>
        /// 获取数据前的准备
        /// </summary>
        public List<string> Before()
        {
            // 设定结束日期
            string endDay = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            // 取得已经存在的所有数据信息
            List<FilePosInfo> allCsv = Util.GetAllFiles(this.csvFolder);

            // 取得所有信息的Html页面内容
            string allInfos = Util.GetHtmlStr("http://quote.eastmoney.com/stocklist.html", "");

            // 定义正则表达式过滤数据
            Regex reg = new Regex("<li><a target=\"_blank\" href=\"http://quote.eastmoney.com/\\S\\S(.*?).html\">");
            Regex regSub = new Regex(@"[0|6|3]\d{5}");

            // 在内容中匹配与正则表达式匹配的字符
            MatchCollection mc = reg.Matches(allInfos);

            // 循环匹配到的字符
            List<string> allStock = new List<string>();
            foreach (Match m in mc)
            {
                string stockCd = regSub.Match(m.Value).Value;
                if (string.IsNullOrEmpty(stockCd))
                {
                    continue;
                }

                allStock.Add(stockCd);
            }

            return allStock;
        }

        /// <summary>
        /// 开始获取数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        public string Start(string stockCd, List<FilePosInfo> allCsv)
        {
            return this.StartGetData(stockCd, allCsv);
        }

        /// <summary>
        /// 开始获取数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        public string CopyM5(string stockCd, List<FilePosInfo> allCsv)
        {
            return this.StartCopyData(stockCd, allCsv);
        }

        /// <summary>
        /// 取得所有不是最新数据的Code
        /// </summary>
        /// <param name="allStockCd"></param>
        /// <param name="?"></param>
        /// <param name="allCsv"></param>
        /// <returns></returns>
        public List<string> GetAllNeedCd(List<string> allStockCd, List<FilePosInfo> allCsv, string endDay)
        {
            List<string> needGetAllCd = new List<string>();

            foreach (string stockCd in allStockCd)
            {
                string startDay = this.GetExitsStock(allCsv, stockCd);
                if (endDay.CompareTo(startDay) > 0)
                {
                    needGetAllCd.Add(stockCd);
                }
            }

            return needGetAllCd;
        }

        /// <summary>
        /// 取得数据的后处理
        /// </summary>
        public void After()
        {
            this.AfterSub();
        }

        #endregion

        #region " 子类可以重写的虚方法 "

        /// <summary>
        /// 开始获取数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        protected virtual string StartGetData(string stockCd, List<FilePosInfo> allCsv)
        {
            return string.Empty;
        }

        /// <summary>
        /// 开始获取数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <param name="allCsv"></param>
        protected virtual string StartCopyData(string stockCd, List<FilePosInfo> allCsv)
        {
            return string.Empty;
        }

        /// <summary>
        /// 取得当前Code的数据
        /// </summary>
        /// <param name="allCsv"></param>
        /// <param name="stockCd"></param>
        /// <returns>文件名中的日期</returns>
        protected virtual string GetExitsStock(List<FilePosInfo> allCsv, string stockCd)
        {
            int pos = 0;
            string shortName;
            foreach (FilePosInfo fileItem in allCsv)
            {
                if (fileItem.IsFolder)
                {
                    continue;
                }

                shortName = Util.GetShortNameWithoutType(fileItem.File);
                if (shortName.StartsWith(stockCd))
                {
                    return shortName.Substring(pos + 7);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得数据的后处理
        /// </summary>
        protected virtual void AfterSub()
        { 
        }

        #endregion
    }
}
