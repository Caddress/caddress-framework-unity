﻿using System;
using System.IO;
using System.Text;

namespace Caddress.Tools {
    public class FileEncoding {
        /// <summary> 
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型 
        /// </summary> 
        /// <param name=“FILE_NAME“>文件路径</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetType(string FILE_NAME) {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// <summary> 
        /// 通过给定的文件流，判断文件的编码类型 
        /// </summary> 
        /// <param name=“fs“>文件流</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetType(FileStream fs) {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.Default;//Encoding.GetEncoding(936);

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.GetEncoding(936));
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (ss[0] >= 0xEF) {
                if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)) {
                    reVal = Encoding.UTF8;
                }
                else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00) {
                    reVal = Encoding.BigEndianUnicode;
                }
                else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41) {
                    reVal = Encoding.Unicode;
                }
                else {
                    reVal = Encoding.Default;//Encoding.GetEncoding(936);
                }
            }

            r.Close();
            return reVal;

        }

        /// <summary> 
        /// 判断是否是不带 BOM 的 UTF8 格式 
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        private static bool IsUTF8Bytes(byte[] data) {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++) {
                curByte = data[i];
                if (charByteCounter == 1) {
                    if (curByte >= 0x80) {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0) {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6) {
                            return false;
                        }
                    }
                }
                else {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80) {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1) {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }

        /// <summary> 
        /// UTF-8转为GB2312
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        public static string Utf8ToGb(string text) {
            byte[] bs = Encoding.GetEncoding("UTF-8").GetBytes(text);
            bs = Encoding.Convert(Encoding.GetEncoding("UTF-8"), Encoding.GetEncoding("GB2312"), bs);
            return Encoding.GetEncoding("GB2312").GetString(bs);
        }
        /// <summary> 
        /// GB2312转为UTF-8
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        public static string GbToUtf8(string text) {
            Encoding utf8, gb23;
            utf8 = Encoding.GetEncoding("UTF-8");
            gb23 = Encoding.GetEncoding("GB2312");
            byte[] bt = gb23.GetBytes(text);
            bt = Encoding.Convert(gb23, utf8, bt);
            return utf8.GetString(bt);
        }
        /// <summary> 
        /// 字符串转为Unicode
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        public static string StringToUnicode(string value) {
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 2) {
                // 取两个字符，每个字符都是右对齐。
                stringBuilder.AppendFormat("u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }
        /// <summary> 
        /// 检测是否为中午
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        public static bool HasChinese(string text) {
            bool result = false;
            for (int i = 0; i < text.Length; i++)
                if ((int)text[i] > 127)
                    result = true;
            return result;
        }

    }
}