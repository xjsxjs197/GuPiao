using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Common;

namespace DataProcess.GetData
{
    /// <summary>
    /// 从网易取得数据
    /// </summary>
    public class GetDataFrom163 : GetDataBase
    {
        #region " 初始化 "

        public GetDataFrom163(string csvFolder, string endDay)
            : base(csvFolder, endDay)
        { 
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
            // 定义正则表达式过滤数据
            Regex regSub = new Regex(@"[0|6|3]\d{5}");

            string leftUrl = "http://quotes.money.163.com/service/chddata.html?fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;TURNOVER;VOTURNOVER;VATURNOVER;TCAP;MCAP";
            string rightUrl = "&end=" + this.endDay + "&code=";
            string tmpFile = this.csvFolder + "tmp.csv";
            Encoding encoding = Encoding.GetEncoding("GBK");
            string result;
            string codeType;

            // 判断类型
            if (stockCd.StartsWith("6"))
            {
                codeType = "0";
            }
            else
            {
                codeType = "1";
            }

            // 取得开始时间
            string startDay = this.GetExitsStock(allCsv, stockCd);
            if (string.IsNullOrEmpty(startDay))
            {
                // 取截止今天为止的所有数据
                result = Util.HttpGet(leftUrl + rightUrl + codeType + stockCd, "", encoding);
                if (!string.IsNullOrEmpty(result))
                {
                    File.WriteAllText(this.csvFolder + stockCd + "_" + endDay + ".csv", result, Encoding.UTF8);
                }
            }
            else if (string.Compare(startDay, endDay) < 0)
            {
                // 取开始时间，到结束时间的数据
                result = Util.HttpGet(leftUrl + rightUrl + codeType + stockCd + "&start=" + startDay, "", encoding);
                if (!string.IsNullOrEmpty(result))
                {
                    // 生成临时文件
                    File.WriteAllText(tmpFile, result, Encoding.UTF8);

                    // 将临时文件的内容，追加到既存的文件中
                    string oldFilePath = this.csvFolder + stockCd + "_" + startDay + ".csv";
                    string[] oldFile = File.ReadAllLines(oldFilePath, Encoding.UTF8);
                    string[] newContent = File.ReadAllLines(tmpFile, Encoding.UTF8);
                    List<string> all = new List<string>();
                    all.AddRange(newContent);
                    for (int i = 2; i < oldFile.Length; i++)
                    {
                        all.Add(oldFile[i]);
                    }

                    // 生成新的文件，删除既存的文件
                    File.WriteAllLines(this.csvFolder + stockCd + "_" + endDay + ".csv", all.ToArray(), Encoding.UTF8);
                    File.Delete(oldFilePath);
                }
            }
        }

        /// <summary>
        /// 取得数据的后处理
        /// </summary>
        protected override void AfterSub()
        {
            string tmpFile = this.csvFolder + "tmp.csv";
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }

        #endregion
    }
}
