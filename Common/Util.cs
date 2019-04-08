using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Net;

namespace Common
{
    /// <summary>
    /// 共通方法
    /// </summary>
    public class Util
    {
        /// <summary>
        /// 日中字符对照表文件名
        /// </summary>
        public static string jpCnCharMapFileName = "./JpCnCharMap.txt";

        /// <summary>
        /// 要过滤的文件（不需要查询）的后缀名
        /// </summary>
        public static Dictionary<string, string> notSearchFile = Util.GetNotSearchFile();

        /// <summary>
        /// 是否需要检查Tpl文件
        /// </summary>
        public static bool NeedCheckTpl = false;

        /// <summary>
        /// 是否需要取得所有文件
        /// </summary>
        public static bool IsGetAllFile = false;

        /// <summary>
        /// 每次复制的字节数
        /// </summary>
        private const int COPY_BLOCK = 1024 * 1024 * 15;

        #region " 文件处理共通 "

        /// <summary>
        /// 取得文件名
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="fileNameOffset"></param>
        /// <returns></returns>
        public static string GetFileNameFromStringTable(byte[] byData, int fileNameOffset)
        {
            int fileNameStartPos = fileNameOffset;
            while (byData[fileNameOffset] != 0)
            {
                fileNameOffset++;
            }
            fileNameOffset--;

            return Util.GetHeaderString(byData, fileNameStartPos, fileNameOffset);
        }

        /// <summary>
        /// 取得文件名
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="fileNameOffset"></param>
        /// <returns></returns>
        public static string GetFileNameFromStringTable(byte[] byData, int fileNameOffset, Encoding encoding)
        {
            int fileNameStartPos = fileNameOffset;
            while (byData[fileNameOffset] != 0)
            {
                fileNameOffset++;
            }
            fileNameOffset--;

            return Util.GetHeaderString(byData, fileNameStartPos, fileNameOffset, encoding);
        }

        /// <summary>
        /// 取得文件名
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="fileNameOffset"></param>
        /// <returns></returns>
        public static string GetFileNameFromStringTable(byte[] byData, int fileNameOffset, int num, Encoding encoding)
        {
            int fileNameStartPos = fileNameOffset;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < num; i++)
            {
                while (byData[fileNameOffset] != 0)
                {
                    fileNameOffset++;
                }
                fileNameOffset--;

                sb.Append(Util.GetHeaderString(byData, fileNameStartPos, fileNameOffset, encoding));
                if (i < (num - 1))
                {
                    fileNameOffset += 2;
                    fileNameStartPos = fileNameOffset;
                    sb.Append("\n");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 将字节数组，通过特定的Encoder转换成字符串
        /// 里面的结束符转换成自定义字符
        /// </summary>
        /// <param name="byData"></param>
        /// <param name="encoder"></param>
        /// <returns></returns>
        public static string GetStringFromByte(byte[] byData, Encoding encoder)
        {
            string strCurrentLine = encoder.GetString(byData);
            int endCharIndex = strCurrentLine.IndexOf('\0');
            if (endCharIndex == -1)
            {
                return strCurrentLine;
            }

            string strRet = encoder.GetString(byData);
            return strRet.Replace("\0", "\n");

            //StringBuilder sb = new StringBuilder();
            //int intEndCharNum = 0;

            //// 将结束符变成自定义的字符串，以便于编辑
            //while (endCharIndex != -1)
            //{
            //    // 找到挨着的所有结束符
            //    intEndCharNum++;
            //    while ((endCharIndex + intEndCharNum) < strCurrentLine.Length
            //        && "\0".Equals(strCurrentLine.Substring(endCharIndex + intEndCharNum, 1)))
            //    {
            //        intEndCharNum++;
            //    }

            //    // 重新拼字符串
            //    sb.Append(strCurrentLine.Substring(0, endCharIndex));
            //    sb.Append("[E*" + intEndCharNum.ToString().PadLeft(2, '0') + "]\n");
            //    strCurrentLine = strCurrentLine.Substring(endCharIndex + intEndCharNum);

            //    intEndCharNum = 0;
            //    endCharIndex = strCurrentLine.IndexOf('\0');
            //}
            //sb.Append(strCurrentLine);

            //return sb.ToString();
        }

        /// <summary>
        /// 取得不需要检索的文件列表
        /// </summary>
        /// <returns>不需要检索的文件列表</returns>
        public static Dictionary<string, string> GetNotSearchFile()
        {
            Dictionary<string, string> notSearchFile = new Dictionary<string, string>();

            notSearchFile.Add("BRSAR", "WII音频文件");
            notSearchFile.Add("BRSTM", "WII音频文件");
            notSearchFile.Add("STR", "PS上使用的视频压缩格式");
            notSearchFile.Add("PSS", "PS2上动画文件");
            notSearchFile.Add("SFD", "WII动画文件");
            notSearchFile.Add("EDH", "和声音文件放在一起，不知道作用");
            notSearchFile.Add("TPL", "WII图片文件");
            notSearchFile.Add("GPL", "WII几何画板文件");
            notSearchFile.Add("ANM", "WII Animation Bank文件");
            notSearchFile.Add("ACT", "WII Actor Hierarchy文件");
            notSearchFile.Add("BRFNT", "WII 字库文件");
            notSearchFile.Add("THP", "WII 动画文件");
            notSearchFile.Add("AUD", "WII 声音文件");
            notSearchFile.Add("ADP", "WII 声音文件");
            notSearchFile.Add("AVI", "动画文件");
            notSearchFile.Add("H4M", "Ngc 动画文件");

            return notSearchFile;
        }

        /// <summary>
        /// 得到目录的所有文件
        /// </summary>
        /// <param name="strFolder">目录</param>
        /// <returns>目录的所有文件</returns>
        public static List<FilePosInfo> GetAllFiles(string strFolder)
        {
            List<FilePosInfo> fileNameInfo = new List<FilePosInfo>();
            if (Directory.Exists(strFolder))
            {
                Util.GetAllFilesInfo(strFolder, fileNameInfo, 0);
            }

            return fileNameInfo;
        }

        /// <summary>
        /// 设置打开文件对话框的Filter
        /// </summary>
        /// <param name="filter"></param>
        public static string SetOpenDailog(string filter, string defaultFile)
        {
            // 打开要分析的文件
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = filter;
            if (string.IsNullOrEmpty(defaultFile))
            {
                openFile.FileName = System.IO.Path.GetFullPath(@"..\..\");
            }
            else 
            {
                openFile.FileName = defaultFile;
            }

            DialogResult rs = openFile.ShowDialog();
            if (rs == DialogResult.Cancel || string.IsNullOrEmpty(openFile.FileName))
            {
                return string.Empty;
            }

            return openFile.FileName;
        }

        /// <summary>
        /// 设置保存文件对话框的Filter
        /// </summary>
        /// <param name="filter"></param>
        public static string SetSaveDailog(string filter, string defaultFile)
        {
            // 打开要分析的文件
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;
            if (string.IsNullOrEmpty(defaultFile))
            {
                saveFileDialog.FileName = System.IO.Path.GetFullPath(@"..\..\"); 
            }
            else 
            {
                saveFileDialog.FileName = defaultFile;
            }

            DialogResult rs = saveFileDialog.ShowDialog();
            if (rs == DialogResult.Cancel || string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                return string.Empty;
            }

            return saveFileDialog.FileName;
        }

        /// <summary>
        /// 取得目录信息
        /// </summary>
        /// <returns>目录信息</returns>
        public static string OpenFolder(string defaultPath)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            if (string.IsNullOrEmpty(defaultPath))
            {
                folderDlg.SelectedPath = System.IO.Path.GetFullPath(@"..\..\");
            }
            else
            {
                folderDlg.SelectedPath = defaultPath;
            }
            DialogResult dr = folderDlg.ShowDialog();

            if (dr == DialogResult.Cancel || string.IsNullOrEmpty(folderDlg.SelectedPath))
            {
                return string.Empty;
            }
            else
            {
                return folderDlg.SelectedPath;
            }
        }

        /// <summary>
        /// 取得短文件名（带文件类型）
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public static string GetShortName(string fileFullName)
        {
            string[] names = fileFullName.Split('\\');
            return names[names.Length - 1];
        }

        /// <summary>
        /// 取得短文件名（不文件类型）
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public static string GetShortNameWithoutType(string fileFullName)
        {
            string[] names = Util.GetShortName(fileFullName).Split('.');
            return names[0];
        }

        /// <summary>
        /// Bmg类型文件解码
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string BmgDecode(string file)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                return string.Empty;
            }

            return Util.BmgDecode(File.ReadAllBytes(file));
        }

        /// <summary>
        /// Bmg类型文件解码
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string BmgDecode(byte[] byBmgData)
        {
            // MESGbmg1开头的判断
            if (byBmgData[0] == 0x4d
                && byBmgData[1] == 0x45
                && byBmgData[2] == 0x53
                && byBmgData[3] == 0x47
                && byBmgData[4] == 0x62
                && byBmgData[5] == 0x6d
                && byBmgData[6] == 0x67
                && byBmgData[7] == 0x31)
            {
                // 取得有多少句文本
                int txtCount = Util.GetOffset(byBmgData, 0x28, 0x29);
                int txtStep = Util.GetOffset(byBmgData, 0x2a, 0x2b);
                List<int> txtOffsets = new List<int>();
                for (int i = 0; i < txtCount; i++)
                {
                    txtOffsets.Add(Util.GetOffset(byBmgData, i * txtStep + 0x30, i * txtStep + 3 + 0x30));
                }

                // 计算文本位置
                int headerLen = 0x20;
                int infLen = Util.GetOffset(byBmgData, 0x24, 0x27);
                int datStart = headerLen + infLen;
                int datEnd = datStart + Util.GetOffset(byBmgData, datStart + 4, datStart + 7);

                // 循环取得文本
                StringBuilder sb = new StringBuilder();
                Encoding shiftJis = Encoding.GetEncoding(932);
                for (int i = 0; i < txtCount; i++)
                {
                    int txtOffset = txtOffsets[i];
                    int txtStart = datStart + 8 + txtOffset;
                    int nextTxtStart = datEnd;
                    if (i < txtCount - 1)
                    {
                        nextTxtStart = datStart + 8 + txtOffsets[i + 1];
                    }

                    if (txtStep == 4)
                    {
                        sb.Append(Util.GetHeaderString(byBmgData, txtStart, nextTxtStart - 1, shiftJis));
                        sb.Append("<BR>\n");
                    }
                    else if (txtStep == 8)
                    {
                        // 特殊处理里面的关键字
                        for (int j = txtStart + 1; j < nextTxtStart; j++)
                        {
                            if (byBmgData[j] == 0x1a)
                            {
                                // 追加前面正常的文本
                                sb.Append(Util.GetHeaderString(byBmgData, txtStart, j - 1, shiftJis));
                                
                                // 追加后面的关键字
                                sb.Append("^");
                                while ((byBmgData[j] & 0x80) != 0x80)
                                {
                                    sb.Append(byBmgData[j].ToString("x") + " ");
                                    j++;
                                }
                                sb.Append("^");

                                txtStart = j;
                            }
                        }

                        sb.Append("<BR>\n");
                    }
                }

                return sb.ToString().Replace("\0", string.Empty);
            }

            return string.Empty;
        }

        #endregion

        #region " 通常共通 "

        /// <summary>
        /// 改变目录
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public static string ChgToGitHubPath(string baseFolder, string gameName)
        {
            return (baseFolder.Replace(gameName, "") + @"HanhuaProject\" + gameName);
        }

        /// <summary>
        /// 判断两个文件是否相同
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <returns></returns>
        public static int isFilesSame(string fileA, string fileB)
        {
            FileStream fsA = File.OpenRead(fileA);
            FileStream fsB = File.OpenRead(fileB);
            BufferedStream fsBufA = new BufferedStream(fsA, COPY_BLOCK);
            BufferedStream fsBufB = new BufferedStream(fsB, COPY_BLOCK);
            int sameLen = 0;

            using (fsA)
            {
                using (fsB)
                {
                    using (fsBufA)
                    {
                        using (fsBufB)
                        {
                            if (fsA.Length != fsB.Length)
                            {
                                return 0;
                            }

                            long len = fsA.Length;
                            fsA.Seek(0, SeekOrigin.Begin);
                            fsB.Seek(0, SeekOrigin.Begin);

                            while (len-- > 0)
                            {
                                if (fsBufA.ReadByte() != fsBufB.ReadByte())
                                {
                                    return sameLen;
                                }
                                sameLen++;
                            }
                        }
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// 从包括路径的文件名中取得文件名
        /// </summary>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        public static string GetShortFileName(string fullFileName)
        {
            if (string.IsNullOrEmpty(fullFileName))
            {
                return string.Empty;
            }

            string[] strNames = fullFileName.Split('\\');
            return strNames[strNames.Length - 1];
        }

        /// <summary>
        /// 生成一级汉字
        /// </summary>
        /// <returns></returns>
        public static string CreateOneLevelHanzi()
        {
            List<byte> hanziByteList = new List<byte>();
            // 国标一级字(共3755个): 区:16-55, 位:01-94, 55区最后5位为空位
            for (int x = 16; x <= 55; x++)
            {
                for (int y = 1; y <= 94; y++)
                {
                    if (x == 55 && y >= 89)
                    {
                        break;
                    }
                    hanziByteList.Add((byte)(x + 0xA0));
                    hanziByteList.Add((byte)(y + 0xA0));
                }
            }

            //return Encoding.GetEncoding("GB2312").GetString(hanziByteList.ToArray()) + "弩驽浣蝙蝠圣阱悚蚯蚓骼蜷鳄桥蟑螂蜻蜓骼魅";
            return Encoding.GetEncoding("GB2312").GetString(hanziByteList.ToArray());
        }

        /// <summary>
        /// 生成二级汉字
        /// </summary>
        /// <returns></returns>
        public static string CreateTwoLevelHanzi()
        {
            List<byte> hanziByteList = new List<byte>();
            // 国标二级汉字(共3008个): 区:56-87, 位:01-94
            for (int x = 56; x <= 87; x++)
            {
                for (int y = 1; y <= 94; y++)
                {
                    hanziByteList.Add((byte)(x + 0xA0));
                    hanziByteList.Add((byte)(y + 0xA0));
                }
            }

            return Encoding.GetEncoding("GB2312").GetString(hanziByteList.ToArray());
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="cmap1"></param>
        /// <param name="cmap2"></param>
        /// <returns></returns>
        public static int Comparison(KeyValuePair<int, int> cmap1, KeyValuePair<int, int> cmap2)
        {
            return cmap1.Key - cmap2.Key;
        }

        /// <summary>
        /// 根据Utf8字符的编码数字取得相应的字符
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetUtf8StrFromNumber(int number)
        {
            byte[] charByte = new byte[1];

            if (number <= 127)
            {
                // 1字节 0xxxxxxx
                charByte = new byte[] { (byte)number };
            }
            else if (number > 127 && number <= 2047)
            {
                // 2字节 110xxxxx 10xxxxxx
                charByte = new byte[] { (byte)(((number >> 6) & 31) + 192), (byte)((number & 63) + 128) };
            }
            else if (number > 2047 && number <= 65535)
            {
                // 3字节 1110xxxx 10xxxxxx 10xxxxxx
                charByte = new byte[] { (byte)(((number >> 12) & 15) + 224), (byte)(((number >> 6) & 63) + 128), (byte)((number & 63) + 128) };
            }

            return Encoding.UTF8.GetString(charByte);
        }

        /// <summary>
        /// 根据Shift-jis字符的编码数字取得相应的字符
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetShiftJisStrFromNumber(int number)
        {
            byte[] charByte;
            if (number <= 0xFF)
            {
                charByte = new byte[] { (byte)(number & 0xFF) };
            }
            else
            {
                charByte = new byte[] { (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF) };
            }

            return Encoding.GetEncoding("Shift-Jis").GetString(charByte);
        }

        /// <summary>
        /// 根据Utf16字符的编码数字取得相应的字符
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetUtf16StrFromNumber(int number, string endianess)
        {
            byte[] charByte;
            if (number <= 0xFF)
            {
                charByte = new byte[] { (byte)(number & 0xFF) };
            }
            else
            {
                charByte = new byte[] { (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF) };
            }

            if ("FFFE".Equals(endianess.ToUpper()))
            {
                return Encoding.BigEndianUnicode.GetString(charByte);
            }
            else
            {
                return Encoding.Unicode.GetString(charByte);
            }
        }

        /// <summary>
        /// 根据字符的编码数字取得相应的字符
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetStrFromNumber(int number, int encoding, string endianess)
        {
            switch (encoding)
            {
                case 0:
                    return Util.GetUtf8StrFromNumber(number);

                case 1:
                    return Util.GetUtf16StrFromNumber(number, endianess);

                case 2:
                    return Util.GetShiftJisStrFromNumber(number);

                case 3:
                    return Encoding.ASCII.GetString(new byte[] { (byte)number} );

                default:
                    return Util.GetUtf8StrFromNumber(number);
            }
        }

        /// <summary>
        /// 根据汉字取得当前汉字的Unicode编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetUnicodeFromStr(string str)
        {
            byte[] hanziByte = Encoding.UTF8.GetBytes(str);
            
            switch (hanziByte.Length)
            {
                case 1:
                    hanziByte[0] = (byte)(hanziByte[0] & 0x7F);
                    return hanziByte[0];

                case 2:
                    hanziByte[0] = (byte)(hanziByte[0] >> 2 & 0x07);
                    hanziByte[1] = (byte)((hanziByte[0] & 0x3) << 6 | (hanziByte[1] & 0x3F));
                    return Util.GetOffset(hanziByte, 0, 1);

                case 3:
                    hanziByte[0] = (byte)((hanziByte[0] & 0xF) << 4| (hanziByte[1] >> 2 & 0x0F));
                    hanziByte[1] = (byte)((hanziByte[1] & 0x3) << 6 | (hanziByte[2] & 0x3F));
                    return Util.GetOffset(hanziByte, 0, 1);
            }

            return Util.GetOffset(hanziByte, 0, hanziByte.Length - 1);
        }

        /// <summary>
        /// 3位的字节数据变成8位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert3To8(byte value)
        {
            // Swizzle bits: 00000123 -> 12312312
            return (byte)((value << 5) | (value << 2) | (value >> 1));
        }
        
        /// <summary>
        /// 8位的字节数据变成3位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert8To3(byte value)
        {
            // Swizzle bits: 12312312 -> 00000123
            return (byte)(value >> 5);
        }

        /// <summary>
        /// 4位的字节数据变成8位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert4To8(byte value)
        {
            // Swizzle bits: 00001234 -> 12341234
            return (byte)((value << 4) | value);
        }

        /// <summary>
        /// 8位的字节数据变成4位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert8To4(byte value)
        {
            // Swizzle bits: 12341234 -> 00001234
            return (byte)(value >> 4);
        }

        /// <summary>
        /// 5位的字节数据变成8位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert5To8(byte value)
        {
            // Swizzle bits: 00012345 -> 12345123
            return (byte)((value << 3) | (value >> 2));
        }

        /// <summary>
        /// 8位的字节数据变成5位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert8To5(byte value)
        {
            // Swizzle bits: 12345123 -> 00012345
            return (byte)(value >> 3);
        }

        /// <summary>
        /// 6位的字节数据变成8位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert6To8(byte value)
        {
            // Swizzle bits: 00123456 -> 12345612
            return (byte)((value << 2) | (value >> 4));
        }

        /// <summary>
        /// 8位的字节数据变成6位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte Convert8To6(byte value)
        {
            // Swizzle bits: 12345612 -> 00123456
            return (byte)(value >> 2);
        }

        /// <summary>
        /// 根据开始、结束位置取得字节数组中的offset
        /// </summary>
        /// <param name="byData">字节数组</param>
        /// <param name="startPos">开始字节位置</param>
        /// <param name="endPos">结束字节位置</param>
        /// <returns></returns>
        public static int GetOffset(byte[] byData, int startPos, int endPos)
        {
            int intRetValue = 0;
            int intBytePos = endPos - startPos;

            for (int i = startPos; i <= endPos; i++)
            {
                intRetValue += (int)((uint)(byData[i]) << (intBytePos * 8));
                intBytePos--;
            }

            return intRetValue;
        }

        /// <summary>
        /// 取得字库编码格式
        /// </summary>
        /// <param name="encodeing"></param>
        /// <returns></returns>
        public static string GetFontEncodingStr(int encodeing)
        {
            switch (encodeing)
            {
                case 1:
                    return "UTF-16BE";

                case 2:
                    return "SJIS";

                case 3:
                    return "windows-1252";

                case 4:
                    return "hex";
            }

            return "UTF-8";
        }

        /// <summary>
        /// 取得字库编码器
        /// </summary>
        /// <param name="encodeing"></param>
        /// <returns></returns>
        public static Encoding GetFontEncoding(int encodeing, string endianess)
        {
            switch (encodeing)
            {
                case 1:
                    if ("FFFE".Equals(endianess.ToUpper()))
                    {
                        return Encoding.BigEndianUnicode;
                    }
                    else
                    {
                        return Encoding.Unicode;
                    }

                case 2:
                    return Encoding.GetEncoding("Shift-Jis");

                case 3:
                    return Encoding.ASCII;

                case 4:
                    MessageBox.Show("不支持这种格式编码！\n暂且使用Utf8编码.");
                    return Encoding.UTF8;
            }

            return Encoding.UTF8;
        }

        /// <summary>
        /// 根据开始、结束位置取得字节数组中的字符串
        /// </summary>
        /// <param name="byData">字节数组</param>
        /// <param name="startPos">开始字节位置</param>
        /// <param name="endPos">结束字节位置</param>
        /// <returns></returns>
        public static string GetBytesString(byte[] byData, int startPos, int endPos)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = startPos; i <= endPos; i++)
            {
                sb.Append(Convert.ToString(byData[i], 16));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 根据开始、结束位置取得字节数组中的字符串
        /// </summary>
        /// <param name="byData">字节数组</param>
        /// <param name="startPos">开始字节位置</param>
        /// <param name="endPos">结束字节位置</param>
        /// <returns></returns>
        public static string GetHeaderString(byte[] byData, int startPos, int endPos)
        {
            return Encoding.GetEncoding(932).GetString(byData, startPos, endPos - startPos + 1);
        }

        /// <summary>
        /// 根据开始、结束位置取得字节数组中的字符串
        /// </summary>
        /// <param name="byData">字节数组</param>
        /// <param name="startPos">开始字节位置</param>
        /// <param name="endPos">结束字节位置</param>
        /// <returns></returns>
        public static string GetHeaderString(byte[] byData, int startPos, int endPos, Encoding encoding)
        {
            return encoding.GetString(byData, startPos, endPos - startPos + 1);
            //byte[] byTxt = new byte[endPos - startPos + 1];
            //Array.Copy(byData, startPos, byTxt, 0, byTxt.Length);
            //return encoding.GetString(byTxt);
        }

        /// <summary>
        /// 将字节数据解码成字符串
        /// </summary>
        /// <param name="byData"></param>
        /// <returns></returns>
        public static string DecodeByteArray(byte[] byData, Decoder decoder)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder endCharSb = new StringBuilder();

            // 将当前文件解码成字符串
            char[] charData = new char[decoder.GetCharCount(byData, 0, byData.Length, true)];
            decoder.GetChars(byData, 0, byData.Length, charData, 0);

            foreach (char itemChar in charData)
            {
                if (itemChar != '\0')
                {
                    if (endCharSb.Length > 0)
                    {
                        sb.Append("^" + endCharSb.ToString().Trim() + "^");
                        endCharSb.Length = 0;
                    }
                    sb.Append(itemChar);
                }
                else
                {
                    endCharSb.Append("0 ");
                }
            }

            if (endCharSb.Length > 0)
            {
                sb.Append("^" + endCharSb.ToString().Trim() + "^");
                endCharSb.Length = 0;
            }

            return sb.ToString().Replace("\n", "^0a^\n").Replace("\r", "^0d^\n");
        }

        /// <summary>
        /// 将字节数据解码成字符串
        /// </summary>
        /// <param name="byData"></param>
        /// <returns></returns>
        public static string EncodeByteArray(byte[] byData, Encoding encoding)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder endCharSb = new StringBuilder();

            // 将当前文件解码成字符串
            byte byCur;
            for (int i = 0; i < byData.Length; i++)
            {
                byCur = byData[i];
                if (byCur == 0)
                {
                    sb.Append("^0^");
                }
                else if (byCur >= 0x20 && byCur <= 0x7e)
                {
                    sb.Append(encoding.GetString(new byte[] { byCur }));
                }
                else if ((byCur >= 0x81 && byCur <= 0x9f)
                    || (byCur >= 0xe0 && byCur <= 0xef))
                {
                    sb.Append(encoding.GetString(new byte[] { byCur, byData[i + 1] }));
                    i++;
                }
                else
                {
                    sb.Append("^").Append(byCur.ToString("x").PadLeft(2, '0')).Append("^");
                }
            }

            return sb.ToString().Replace("^0a^", "^0a^\n");
        }

        /// <summary>
        /// 判断字符串是否是数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            try
            {
                Convert.ToDecimal(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 去掉文件名中的后缀数字(file_01 -> file)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string TrimFileNo(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            return Regex.Replace(fileName, @"_\d+$", string.Empty);
        }

        /// <summary>
        /// 根据固定长度，重新设置位置
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="fixSize"></param>
        /// <returns></returns>
        public static int ResetPos(int pos, int fixSize)
        {
            int temp = pos % fixSize;
            if (temp > 0)
            {
                return pos + (fixSize - temp);
            }
            else
            {
                return pos;
            }
        }

        #endregion

        #region " 网页相关共通 "

        /// <summary>  
        /// 获取网页的HTML码  
        /// </summary>  
        /// <param name="url">链接地址</param>  
        /// <param name="encoding">编码类型</param>  
        /// <returns></returns>  
        public static string GetHtmlStr(string url, string encoding)
        {
            string htmlStr = "";
            try
            {
                if (!String.IsNullOrEmpty(url))
                {
                    WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
                    WebResponse response = request.GetResponse();           //创建WebResponse对象  
                    Stream datastream = response.GetResponseStream();       //创建流对象  
                    Encoding ec = Encoding.Default;
                    if (encoding == "UTF8")
                    {
                        ec = Encoding.UTF8;
                    }
                    else if (encoding == "Default")
                    {
                        ec = Encoding.Default;
                    }
                    else if (!string.IsNullOrEmpty(encoding))
                    {
                        ec = Encoding.GetEncoding("GBK");
                    }
                    StreamReader reader = new StreamReader(datastream, ec);
                    htmlStr = reader.ReadToEnd();                  //读取网页内容  
                    reader.Close();
                    datastream.Close();
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }

            return htmlStr;
        }

        /// <summary>
        /// Http发送Post请求方法
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static string HttpPost(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataStr.Length;
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            writer.Write(postDataStr);
            writer.Flush();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码  
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            return retString;
        }

        /// <summary>
        /// Http发送Get请求方法
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static string HttpGet(string Url, string postDataStr, Encoding encoding)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, encoding);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        #endregion

        #region " 数据处理相关共通 "

        /// <summary>
        /// 取得非节假日的日期
        /// </summary>
        /// <returns></returns>
        public static DateTime GetAvailableDt()
        {
            DateTime dt = DateTime.Now;

            if (dt.Hour < 15)
            {
                dt = dt.AddDays(-1);
            }

            while (Util.IsHolidayByDate(dt))
            {
                dt = dt.AddDays(-1);
            }

            return dt;
        }

        /// <summary>
        /// 判断是不是周末/节假日
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>周末和节假日返回true，工作日返回false</returns>
        public static bool IsHolidayByDate(DateTime date)
        {
            var isHoliday = false;
            try
            {
                var day = date.DayOfWeek;

                // 判断是否为周末
                if (day == DayOfWeek.Sunday || day == DayOfWeek.Saturday)
                {
                    return true;
                }

                // 0为工作日，1为周末，2为法定节假日
                var result = Util.HttpPost("http://tool.bitefu.net/jiari/", "d=" + date.ToString("yyyyMMdd"));
                if (result == "1" || result == "2")
                {
                    isHoliday = true;
                }
            }
            catch
            {
                isHoliday = false;
            }

            return isHoliday;
        }

        /// <summary>
        /// 取得运行时可变的配置参数
        /// </summary>
        /// <returns></returns>
        public static BuySellSetting GetBuyCellSettingInfo()
        {
            BuySellSetting emuInfo = new BuySellSetting();

            try
            {
                string[] emuSetting = File.ReadAllLines(Consts.BASE_PATH + "BuyCellSetting.txt");
                emuInfo.BefDay = Convert.ToInt32(emuSetting[1]);
                emuInfo.ThreadCnt = Convert.ToInt32(emuSetting[3]);
                emuInfo.ThreadMoney = Convert.ToDecimal(emuSetting[5]);
                emuInfo.IsReverse = Convert.ToBoolean(emuSetting[7]);
                emuInfo.AvgDataLen = Convert.ToInt32(emuSetting[9]);
                emuInfo.NeedChuangYe = Convert.ToBoolean(emuSetting[11]);
                emuInfo.NeedRongZiRongQuan = Convert.ToBoolean(emuSetting[13]);
                emuInfo.DataCntPerSecond = Convert.ToInt32(emuSetting[15]);
                emuInfo.SystemTitle = emuSetting[17];
                emuInfo.ButStrongth = Convert.ToInt32(emuSetting[19]);
                emuInfo.AutoTradeLevel = emuSetting[21];
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.WriteLine();
                throw new Exception(e.Message);
            }

            return emuInfo;
        }

        /// <summary>
        /// 是否是创业板数据
        /// </summary>
        /// <param name="stockCd"></param>
        /// <returns></returns>
        public static bool IsChuangyeStock(string stockCd)
        {
            return stockCd.StartsWith("300");
        }

        /// <summary>
        /// 取得融资融券信息
        /// </summary>
        /// <returns></returns>
        public static List<string> GetRongZiRongQuan()
        {
            List<string> allRongzi = new List<string>();
            string[] rongziRongQuan = File.ReadAllLines(Consts.BASE_PATH + Consts.CSV_FOLDER + "RongZiRongYuan.txt");
            foreach (string line in rongziRongQuan)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] tmp = line.Split(' ');
                if (tmp.Length >= 3)
                {
                    allRongzi.Add(tmp[1]);
                }
            }

            return allRongzi;
        }

        /// <summary>
        /// 取得最大、最小值
        /// </summary>
        /// <param name="stockInfos"></param>
        /// <returns></returns>
        public static decimal[] GetMaxMinStock(List<BaseDataInfo> stockInfos)
        {
            decimal[] minMaxInfo = new decimal[2];
            decimal minVal = decimal.MaxValue;
            decimal maxVal = 0;

            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                if (stockInfos[i].DayVal == 0)
                {
                    stockInfos.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            for (int i = stockInfos.Count - 1; i >= 0; i--)
            {
                decimal curVal = stockInfos[i].DayVal;
                if (curVal > maxVal)
                {
                    maxVal = curVal;
                }

                if (curVal > 0 && curVal < minVal)
                {
                    minVal = curVal;
                }

                if (curVal == 0)
                {
                    BaseDataInfo item = new BaseDataInfo();
                    item.Day = stockInfos[i].Day;
                    item.DayVal = stockInfos[i + 1].DayVal;
                    item.DayMaxVal = stockInfos[i + 1].DayMaxVal;
                    item.DayMinVal = stockInfos[i + 1].DayMinVal;

                    stockInfos[i] = item;
                }
            }

            minMaxInfo[0] = minVal;
            minMaxInfo[1] = maxVal;

            return minMaxInfo;
        }

        /// <summary>
        /// 取得画图Y轴的差值
        /// </summary>
        /// <param name="minMaxInfo"></param>
        /// <returns></returns>
        public static decimal GetYstep(decimal[] minMaxInfo)
        {
            return 370 / (minMaxInfo[1] - minMaxInfo[0]);
        }

        /// <summary>
        /// 可以买多少数量的取得
        /// </summary>
        /// <param name="money"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public static int CanBuyCount(decimal money, decimal price)
        {
            return (int)((money - 5) / (price * 100));
        }

        #endregion

        #region " 私有方法 "

        /// <summary>
        /// 分析目录
        /// </summary>
        /// <param name="folder">目录</param>
        /// <param name="fileNameInfo">文件信息列表</param>
        /// <param name="folderIndex">目录的级别</param>
        private static void GetAllFilesInfo(string folder, List<FilePosInfo> fileNameInfo, int folderIndex)
        {
            // 追加当前目录
            fileNameInfo.Add(new FilePosInfo(folder, true, folderIndex));
            folderIndex++;

            DirectoryInfo di = new DirectoryInfo(folder);
            FileInfo[] fiList = di.GetFiles(); // 取得当前目录下所有文件
            DirectoryInfo[] diA = di.GetDirectories(); // 取得当前目录下所有目录

            // 追加当前目录的文件
            foreach (FileInfo fi in fiList)
            {
                if (Util.IsGetAllFile || Util.NeedCheckFile(fi.FullName))
                {
                    fileNameInfo.Add(new FilePosInfo(fi.FullName, false, folderIndex));
                }
            }

            // 递归分析当前目录下子目录的文件
            foreach (DirectoryInfo childDi in diA)
            {
                Util.GetAllFilesInfo(childDi.FullName, fileNameInfo, folderIndex);
            }
        }

        /// <summary>
        /// 根据后缀名判断是否需要检查
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>是否需要检查</returns>
        private static bool NeedCheckFile(string fileName)
        {
            string[] paths = fileName.Split('.');
            string endName = paths[paths.Length - 1].ToUpper();

            if (endName == "TPL")
            {
                return Util.NeedCheckTpl;
            }
            else if (Util.notSearchFile.ContainsKey(endName)
                || fileName.IndexOf(@"\audio\") != -1
                || fileName.IndexOf(@"\door\") != -1
                || fileName.IndexOf(@"\movie\") != -1
                || fileName.IndexOf(@"\sound\") != -1
                || fileName.IndexOf(@"\bgm\") != -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion
    }
}