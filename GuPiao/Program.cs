using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using GuPiaoTool;

namespace GuPiaoTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GuPiaoTool());

            /*try
            {
                if (args.Length < 2)
                {
                    throw new Exception("参数不正确，{0} Excel，{1} Log");
                }

                if (!File.Exists(args[0]))
                {
                    throw new Exception("Excel文件不存在");
                }

                if (!File.Exists(args[1]))
                {
                    throw new Exception("Log文件不存在");
                }

                string[] logInfo = File.ReadAllLines(args[1]);
                List<string> lstLogInfo = new List<string>();
                lstLogInfo.AddRange(logInfo);
                if (lstLogInfo.Count > 0 && string.IsNullOrEmpty(lstLogInfo[lstLogInfo.Count - 1]))
                {
                    lstLogInfo.RemoveAt(lstLogInfo.Count - 1);
                }


                WriteJijinInfo writeJijinInfo = new WriteJijinInfo();
                string retMsg = writeJijinInfo.StartRun(args[0]);

                if (string.IsNullOrEmpty(retMsg))
                {
                    lstLogInfo.Add(DateTime.Now.ToString("yyyy/MM/dd") + " 写入成功");
                }
                else
                {
                    lstLogInfo.Add(DateTime.Now.ToString("yyyy/MM/dd") + " " + retMsg);
                }

                File.WriteAllLines(args[1], lstLogInfo.ToArray(), System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.Write(e.Message + "\n" + e.StackTrace);
            }*/
            
        }
    }
}
