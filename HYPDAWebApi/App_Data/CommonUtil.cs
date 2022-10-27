/************************************************************************************
 *      Copyright (C) 2011 MJ.com,All Rights Reserved					
 *      File:																
 *				CommonUtil.cs	                                            	
 *      Description:															
 *				 通用类        												
 *      Author:																									
 *				http://www.supesoft.com										
 *      Finish DateTime:														
 *				2011年6月20日													
 *      History:																
 *              2011年6月21日
 ***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Globalization;
using System.Web.SessionState;
using System.Security.Cryptography;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Microsoft.Win32;

namespace HYPDAWebApi.App_Data
{
    using System.Data;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    /// 通用类
    /// </summary>
    public class CommonUtil
    {
        //private static ISYS_USERManager sys_UserManager = new MJ.Business.Implements.SYS_USERManager();

        #region "返回字符串在字符串中出现的次数"
        /// <summary>
        /// 返回字符串在字符串中出现的次数
        /// </summary>
        /// <param name="Char">要检测出现的字符</param>
        /// <param name="String">要检测的字符串</param>
        /// <returns>出现次数</returns>
        public static int GetCharInStringCount(string Char, string String)
        {
            string str = String.Replace(Char, "");
            return (String.Length - str.Length) / Char.Length;

        }
        #endregion

        #region "获得物理路径"
        /// <summary>
        /// 获得物理路径
        /// </summary>
        /// <param name="a">路径</param>
        /// <returns>物理路径</returns>
        public static string GetFullPath(string a)
        {
            string AppDir = System.AppDomain.CurrentDomain.BaseDirectory;
            if (a.IndexOf(":") < 0)
            {
                string str = a.Replace("..\\", "");
                if (str != a)
                {
                    int Num = (a.Length - str.Length) / ("..\\").Length + 1;
                    for (int i = 0; i < Num; i++)
                    {
                        AppDir = AppDir.Substring(0, AppDir.LastIndexOf("\\"));
                    }
                    str = "\\" + str;

                }
                a = AppDir + str;
            }
            return a;
        }
        #endregion

        #region "获得日志文件存放目录"
        /// <summary>
        /// 获得日志文件存放目录
        /// </summary>
        public static string LogDir
        {
            get
            {
                string LogFilePath = GetFullPath(ConfigurationManager.AppSettings["LogDir"]);
                if (!Directory.Exists(LogFilePath))
                    Directory.CreateDirectory(LogFilePath);
                return LogFilePath;
            }
        }
        #endregion

        


        #region "js信息提示框"
        /// <summary>
        /// js信息提示框
        /// </summary>
        /// <param name="Message">提示信息文字</param>
        /// <param name="ReturnUrl">返回地址</param>
        /// <param name="rq"></param>
        public static void MessBox(string Message, string ReturnUrl, HttpContext rq)
        {
            System.Text.StringBuilder msgScript = new System.Text.StringBuilder();
            msgScript.Append("<script language=JavaScript>\n");
            msgScript.Append("alert(\"" + Message + "\");\n");
            msgScript.Append("parent.location.href='" + ReturnUrl + "';\n");
            msgScript.Append("</script>\n");
            rq.Response.Write(msgScript.ToString());
            rq.Response.End();
        }

        /// <summary>
        /// 弹出Alert信息窗
        /// </summary>
        /// <param name="Message">信息内容</param>
        public static void MessBox(string Message)
        {
            System.Text.StringBuilder msgScript = new System.Text.StringBuilder();
            msgScript.Append("<script language=JavaScript>\n");
            msgScript.Append("alert(\"" + Message + "\");\n");
            msgScript.Append("</script>\n");
            HttpContext.Current.Response.Write(msgScript.ToString());
        }

        #endregion

        #region 格式化字符串,符合SQL语句
        /// <summary>
        /// 格式化字符串,符合SQL语句
        /// </summary>
        /// <param name="formatStr">需要格式化的字符串</param>
        /// <returns>字符串</returns>
        public static string inSQL(string formatStr)
        {
            string rStr = formatStr;
            if (formatStr != null && formatStr != string.Empty)
            {
                rStr = rStr.Replace("'", "''");
                //rStr = rStr.Replace("\"", "\"\"");
            }
            return rStr;
        }
        /// <summary>
        /// 格式化字符串,是inSQL的反向
        /// </summary>
        /// <param name="formatStr"></param>
        /// <returns></returns>
        public static string outSQL(string formatStr)
        {
            string rStr = formatStr;
            if (rStr != null)
            {
                rStr = rStr.Replace("''", "'");
                rStr = rStr.Replace("\"\"", "\"");
            }
            return rStr;
        }

        /// <summary>
        /// 查询SQL语句,删除一些SQL注入问题
        /// </summary>
        /// <param name="formatStr">需要格式化的字符串</param>
        /// <returns></returns>
        public static string querySQL(string formatStr)
        {
            string rStr = formatStr;
            if (rStr != null && rStr != "")
            {
                rStr = rStr.Replace("'", "");
            }
            return rStr;
        }
        #endregion

        #region 截取字符串
        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="str_value"></param>
        /// <param name="str_len"></param>
        /// <returns></returns>
        public static string leftx(string str_value, int str_len)
        {
            int p_num = 0;
            int i;
            string New_Str_value = "";

            if (str_value == "")
            {
                New_Str_value = "";
            }
            else
            {
                int Len_Num = str_value.Length;
                for (i = 0; i <= Len_Num - 1; i++)
                {
                    if (i > Len_Num) break;
                    char c = Convert.ToChar(str_value.Substring(i, 1));
                    if (((int)c > 255) || ((int)c < 0))
                        p_num = p_num + 2;
                    else
                        p_num = p_num + 1;



                    if (p_num >= str_len)
                    {

                        New_Str_value = str_value.Substring(0, i + 1);
                        break;
                    }
                    else
                    {
                        New_Str_value = str_value;
                    }

                }

            }
            return New_Str_value;
        }
        #endregion

        #region 检测用户提交页面
        /// <summary>
        /// 检测用户提交页面
        /// </summary>
        /// <param name="rq"></param>
        public static void Check_Post_Url(HttpContext rq)
        {
            string WebHost = "";
            if (rq.Request.ServerVariables["SERVER_NAME"] != null)
            {
                WebHost = rq.Request.ServerVariables["SERVER_NAME"].ToString();
            }

            string From_Url = "";
            if (rq.Request.UrlReferrer != null)
            {
                From_Url = rq.Request.UrlReferrer.ToString();
            }

            if (From_Url == "" || WebHost == "")
            {
                rq.Response.Write("禁止外部提交数据!");
                rq.Response.End();
            }
            else
            {
                WebHost = "HTTP://" + WebHost.ToUpper();
                From_Url = From_Url.ToUpper();
                int a = From_Url.IndexOf(WebHost);
                if (From_Url.IndexOf(WebHost) < 0)
                {
                    rq.Response.Write("禁止外部提交数据!");
                    rq.Response.End();
                }
            }

        }
        #endregion

        #region 日期处理
        /// <summary>
        /// 格式化日期为2006-12-22
        /// </summary>
        /// <param name="dTime"></param>
        /// <returns></returns>
        public static string formatDate(DateTime dTime)
        {
            string rStr;
            rStr = dTime.Year + "-" + dTime.Month + "-" + dTime.Day;
            return rStr;
        }

        /// <summary>
        /// 获取日期
        /// </summary>
        /// <param name="sDate"></param>
        /// <returns></returns>
        public static string getWeek(DateTime sDate)
        {
            Calendar myCal = CultureInfo.InvariantCulture.Calendar;


            string rStr = "";
            switch (myCal.GetDayOfWeek(sDate).ToString())
            {
                case "Sunday":
                    rStr = "星期日";
                    break;
                case "Monday":
                    rStr = "星期一";
                    break;
                case "Tuesday":
                    rStr = "星期二";
                    break;
                case "Wednesday":
                    rStr = "星期三";
                    break;
                case "Thursday":
                    rStr = "星期四";
                    break;
                case "Friday":
                    rStr = "星期五";
                    break;
                case "Saturday":
                    rStr = "星期六";
                    break;
            }
            return rStr;
        }
        /// <summary>
        /// 获取某年某月的总天数
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>总天数</returns>
        public static int GetMonthDays(int year, int month)
        {
            if (month >= 1 && month <= 12)
            {
                int[] days1 = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                int[] days2 = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                if ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)
                {
                    return days2[month - 1];
                }
                else
                {
                    return days1[month - 1];
                }
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region 随机颜色数据

        /// <summary>
        /// 随机颜色数据
        /// </summary>
        /// <returns></returns>
        public static string getStrColor()
        {
            int length = 6;
            byte[] random = new Byte[length / 2];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(random);

            StringBuilder sb = new StringBuilder(length);
            int i;
            for (i = 0; i < random.Length; i++)
            {
                sb.Append(String.Format("{0:X2}", random[i]));
            }
            return sb.ToString();
        }
        #endregion

        #region "隐藏IP地址最后一位用*号代替"
        /// <summary>
        /// 隐藏IP地址最后一位用*号代替
        /// </summary>
        /// <param name="Ipaddress">IP地址:192.168.34.23</param>
        /// <returns></returns>
        public static string HidenLastIp(string Ipaddress)
        {
            return Ipaddress.Substring(0, Ipaddress.LastIndexOf(".")) + ".*";
        }
        #endregion

        #region "防刷新检测"
        /// <summary>
        /// 防刷新检测
        /// </summary>
        /// <param name="Second">访问间隔秒</param>
        /// <param name="UserSession"></param>
        public static bool CheckRefurbish(int Second, HttpSessionState UserSession)
        {

            bool i = true;
            if (UserSession["RefTime"] != null)
            {
                DateTime d1 = Convert.ToDateTime(UserSession["RefTime"]);
                DateTime d2 = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
                TimeSpan d3 = d2.Subtract(d1);
                if (d3.Seconds < Second)
                {
                    i = false;
                }
                else
                {
                    UserSession["RefTime"] = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                }
            }
            else
            {
                UserSession["RefTime"] = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
            }

            return i;
        }
        #endregion

        #region "判断是否是Decimal类型"
        /// <summary>
        /// 判断是否是Decimal类型
        /// </summary>
        /// <param name="TBstr0">判断数据字符</param>
        /// <returns>true是false否</returns>
        public static bool IsDecimal(string TBstr0)
        {
            bool IsBool = false;
            string Intstr0 = "1234567890";
            string IntSign0, StrInt, StrDecimal;
            int IntIndex0, IntSubstr, IndexInt;
            int decimalbool = 0;
            int db = 0;
            bool Bf, Bl;
            if (TBstr0.Length > 2)
            {
                IntIndex0 = TBstr0.IndexOf(".");
                if (IntIndex0 != -1)
                {
                    string StrArr = ".";
                    char[] CharArr = StrArr.ToCharArray();
                    string[] NumArr = TBstr0.Split(CharArr);
                    IndexInt = NumArr.GetUpperBound(0);
                    if (IndexInt > 1)
                    {
                        decimalbool = 1;
                    }
                    else
                    {
                        StrInt = NumArr[0].ToString();
                        StrDecimal = NumArr[1].ToString();
                        //--- 整数部分－－－－－
                        if (StrInt.Length > 0)
                        {
                            if (StrInt.Length == 1)
                            {
                                IntSubstr = Intstr0.IndexOf(StrInt);
                                if (IntSubstr != -1)
                                {
                                    Bf = true;
                                }
                                else
                                {
                                    Bf = false;
                                }
                            }
                            else
                            {
                                for (int i = 0; i <= StrInt.Length - 1; i++)
                                {
                                    IntSign0 = StrInt.Substring(i, 1).ToString();
                                    IntSubstr = Intstr0.IndexOf(IntSign0);
                                    if (IntSubstr != -1)
                                    {
                                        db = db + 0;
                                    }
                                    else
                                    {
                                        db = i + 1;
                                        break;
                                    }
                                }

                                if (db == 0)
                                {
                                    Bf = true;
                                }
                                else
                                {
                                    Bf = false;
                                }
                            }
                        }
                        else
                        {
                            Bf = true;
                        }
                        //----小数部分－－－－
                        if (StrDecimal.Length > 0)
                        {
                            for (int j = 0; j <= StrDecimal.Length - 1; j++)
                            {
                                IntSign0 = StrDecimal.Substring(j, 1).ToString();
                                IntSubstr = Intstr0.IndexOf(IntSign0);
                                if (IntSubstr != -1)
                                {
                                    db = db + 0;
                                }
                                else
                                {
                                    db = j + 1;
                                    break;
                                }
                            }
                            if (db == 0)
                            {
                                Bl = true;
                            }
                            else
                            {
                                Bl = false;
                            }
                        }
                        else
                        {
                            Bl = false;
                        }
                        if ((Bf && Bl) == true)
                        {
                            decimalbool = 0;
                        }
                        else
                        {
                            decimalbool = 1;
                        }

                    }

                }
                else
                {
                    decimalbool = 1;
                }

            }
            else
            {
                decimalbool = 1;
            }

            if (decimalbool == 0)
            {
                IsBool = true;
            }
            else
            {
                IsBool = false;
            }

            return IsBool;
        }
        #endregion

        #region "获取随机数"
        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="length">随机数长度</param>
        /// <returns></returns>
        public static string GetRandomPassword(int length)
        {
            byte[] random = new Byte[length / 2];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(random);

            StringBuilder sb = new StringBuilder(length);
            int i;
            for (i = 0; i < random.Length; i++)
            {
                sb.Append(String.Format("{0:X2}", random[i]));
            }
            return sb.ToString();
        }
        #endregion

        #region "获取用户IP地址"
        /// <summary>
        /// 获取用户IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {

            string user_IP = string.Empty;
            HttpContext context = System.Web.HttpContext.Current;
            if (context!=null)
            {
                user_IP = context.Request.UserHostAddress;
            }
            if (string.IsNullOrEmpty(user_IP))
            {
                return ":::";
            }
            else {
                return user_IP;
            }
            
        }
        #endregion

        #region 字符串截取补字符函数
        /// <summary>
        /// 字符串截取补字符函数
        /// </summary>
        /// <param name="s">要处理的字符串</param>
        /// <param name="len">长度</param>
        /// <param name="b">补充的字符</param>
        /// <returns>处理后字符</returns>
        public static string splitStringLen(string s, int len, char b)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            if (s.Length >= len)
                return s.Substring(0, len);
            return s.PadRight(len, b);
        }
        #endregion

        #region "3des加密字符串"
        /// <summary>
        /// 3des加密函数(ECB加密模式,PaddingMode.PKCS7,无IV)
        /// </summary>
        /// <param name="encryptValue">加密字符</param>
        /// <param name="key">加密key(24字符)</param>
        /// <returns>加密后Base64字符</returns>
        public static string EncryptString(string encryptValue, string key)
        {
            string enstring = "加密出错!";
            ICryptoTransform ct; //需要此接口才能在任何服务提供程序上调用 CreateEncryptor 方法，服务提供程序将返回定义该接口的实际 encryptor 对象。
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;
            SymmetricAlgorithm des3 = SymmetricAlgorithm.Create("TripleDES");
            des3.Mode = CipherMode.ECB;
            des3.Key = Encoding.UTF8.GetBytes(splitStringLen(key,24,'0'));
            //des3.KeySize = 192;
            des3.Padding = PaddingMode.PKCS7;

            ct = des3.CreateEncryptor();

            byt = Encoding.UTF8.GetBytes(encryptValue);//将原始字符串转换成字节数组。大多数 .NET 加密算法处理的是字节数组而不是字符串。

            //创建 CryptoStream 对象 cs 后，现在使用 CryptoStream 对象的 Write 方法将数据写入到内存数据流。这就是进行实际加密的方法，加密每个数据块时，数据将被写入 MemoryStream 对象。

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            try
            {
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                enstring = Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                enstring = ex.ToString();
            }
            finally
            {
                cs.Close();
                cs.Dispose();
                ms.Close();
                ms.Dispose();
                des3.Clear();
                ct.Dispose();
            }
            enstring = Convert.ToBase64String(ms.ToArray());
            return enstring;
        }
        #endregion

        #region "3des解密字符串"
        /// <summary>
        /// 3des解密函数(ECB加密模式,PaddingMode.PKCS7,无IV)
        /// </summary>
        /// <param name="decryptString">解密字符</param>
        /// <param name="key">解密key(24字符)</param>
        /// <returns>解密后字符</returns>
        public static string DecryptString(string decryptString,string key)
        {
            string destring="解密字符失败!";
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            SymmetricAlgorithm des3 = SymmetricAlgorithm.Create("TripleDES");
            des3.Mode = CipherMode.ECB;
            des3.Key = Encoding.UTF8.GetBytes(splitStringLen(key,24,'0'));
            //des3.KeySize = 192;
            des3.Padding = PaddingMode.PKCS7;

            ct = des3.CreateDecryptor();

            byt = Convert.FromBase64String(decryptString);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            try
            {
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                destring = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                destring = ex.ToString();
            }
            finally{
                ms.Close();
                cs.Close();
                ms.Dispose();
                cs.Dispose();
                ct.Dispose();
                des3.Clear();
            }
            return destring;
        }
        #endregion

        #region "MD5加密"
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">加密字符</param>
        /// <param name="code">加密位数16/32</param>
        /// <returns></returns>
        public static string md5(string str, int code)
        {
            string strEncrypt = string.Empty;
            if (code == 16)
            {
                strEncrypt = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").Substring(8, 16);
            }

            if (code == 32)
            {
                strEncrypt = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5");
            }

            return strEncrypt;
        }
        #endregion

        #region 脚本提示信息,并且跳转到最上层框架
        /// <summary>
        /// 脚本提示信息
        /// </summary>
        /// <param name="Msg">信息内容,可以为空,为空表示不出现提示窗口</param>
        /// <param name="Url">跳转地址</param>
        public static string Hint(string Msg, string Url)
        {
            System.Text.StringBuilder rStr = new System.Text.StringBuilder();

            rStr.Append("<script language='javascript'>");
            if (Msg != "")
                rStr.Append("	alert('" + Msg + "');");

            if (Url != "")
                rStr.Append("	window.top.location.href = '" + Url + "';");

            rStr.Append("</script>");

            return rStr.ToString();
        }
        #endregion

        #region 脚本提示信息,并且跳转到当前框架内
        /// <summary>
        /// 脚本提示信息
        /// </summary>
        /// <param name="Msg">信息内容,可以为空,为空表示不出现提示窗口</param>
        /// <param name="Url">跳转地址,自已可以写入脚本</param>
        /// <returns></returns>
        public static string LocalHintJs(string Msg, string Url)
        {
            System.Text.StringBuilder rStr = new System.Text.StringBuilder();

            rStr.Append("<script language='JavaScript'>\n");
            if (Msg != "")
                rStr.AppendFormat("	alert('{0}');\n", Msg);

            if (Url != "")
                rStr.Append(Url + "\n");
            rStr.Append("</script>");

            return rStr.ToString();
        }

        #endregion

        #region 脚本提示信息,并且跳转到当前框架内,地址为空时,返回上页
        /// <summary>
        /// 脚本提示信息
        /// </summary>
        /// <param name="Msg"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string LocalHint(string Msg, string Url)
        {
            System.Text.StringBuilder rStr = new System.Text.StringBuilder();

            rStr.Append("<script language='JavaScript'>\n");
            if (Msg != "")
                rStr.AppendFormat("	alert('{0}');\n", Msg);

            if (Url != "")
                rStr.AppendFormat("	window.location.href = '" + Url + "';\n");
            else
                rStr.AppendFormat(" window.history.back();");

            rStr.Append("</script>\n");

            return rStr.ToString();
        }
        #endregion

        #region "按当前日期和时间生成随机数"
        /// <summary>
        /// 按当前日期和时间生成随机数
        /// </summary>
        /// <param name="Num">附加随机数长度</param>
        /// <returns></returns>
        public static string sRndNum(int Num)
        {
            string sTmp_Str = System.DateTime.Today.Year.ToString() + System.DateTime.Today.Month.ToString("00") + System.DateTime.Today.Day.ToString("00") + System.DateTime.Now.Hour.ToString("00") + System.DateTime.Now.Minute.ToString("00") + System.DateTime.Now.Second.ToString("00");
            return sTmp_Str + RndNum(Num);
        }
        #endregion

        #region 生成0-9随机数
        /// <summary>
        /// 生成0-9随机数
        /// </summary>
        /// <param name="VcodeNum">生成长度</param>
        /// <returns></returns>
        public static string RndNum(int VcodeNum)
        {
            StringBuilder sb = new StringBuilder(VcodeNum);
            Random rand = new Random();
            for (int i = 1; i < VcodeNum + 1; i++)
            {
                int t = rand.Next(9);
                sb.AppendFormat("{0}", t);
            }
            return sb.ToString();

        }
        #endregion

        #region "通过RNGCryptoServiceProvider 生成随机数 0-9"
        /// <summary>
        /// 通过RNGCryptoServiceProvider 生成随机数 0-9 
        /// </summary>
        /// <param name="length">随机数长度</param>
        /// <returns></returns>
        public static string RndNumRNG(int length)
        {
            byte[] bytes = new byte[16];
            RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                r.GetBytes(bytes);
                sb.AppendFormat("{0}", (int)((decimal)bytes[0] / 256 * 10));
            }
            return sb.ToString();

        }
        #endregion

        #region "在当前路径上创建日期格式目录(20110620)"
        /// <summary>
        /// 在当前路径上创建日期格式目录(20110620)
        /// </summary>
        /// <param name="sPath">返回目录名</param>
        /// <returns></returns>
        public static string CreateDir(string sPath)
        {
            string sTemp = System.DateTime.Today.Year.ToString() + System.DateTime.Today.Month.ToString("00") + System.DateTime.Today.Day.ToString("00");
            sPath += sTemp;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(@sPath); //构造函数创建目录
            if (di.Exists == false)
            {
                di.Create();
            }

            return sTemp;
        }
        #endregion

        #region "检测是否为有效邮件地址格式"
        /// <summary>
        /// 检测是否为有效邮件地址格式
        /// </summary>
        /// <param name="strIn">输入邮件地址</param>
        /// <returns></returns>
        public static bool IsValidEmail(string strIn)
        {
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
        #endregion

        #region "邮件发送"
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="strto">接收邮件地址</param>
        /// <param name="strSubject">主题</param>
        /// <param name="strBody">内容</param>
        public static void SendSMTPEMail(string strto, string strSubject, string strBody)
        {
            string SMTPHost = ConfigurationManager.AppSettings["SMTPHost"];
            string SMTPPort = ConfigurationManager.AppSettings["SMTPPort"];
            string SMTPUser = ConfigurationManager.AppSettings["SMTPUser"];
            string SMTPPassword = ConfigurationManager.AppSettings["SMTPPassword"];
            string MailFrom = ConfigurationManager.AppSettings["MailFrom"];
            string MailSubject = ConfigurationManager.AppSettings["MailSubject"];

            SmtpClient client = new SmtpClient(SMTPHost);
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(SMTPUser, SMTPPassword);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            MailMessage message = new MailMessage(SMTPUser, strto, strSubject, strBody);
            message.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
            message.IsBodyHtml = true;

            client.Send(message);
        }
        #endregion

        #region "转换编码"
        /// <summary>
        /// 转换编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Encode(string str)
        {
            if (str == null)
            {
                return "";
            }
            else
            {

                return System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding(54936).GetBytes(str));
            }
        }
        #endregion

        #region "当前显示应用模组"
        /// <summary>
        /// 显示应用模组
        /// </summary>
        public static int ApplicationID
        {
            get
            {
                try
                {
                    return Convert.ToInt32(ConfigurationManager.AppSettings["ApplicationID"]);
                }
                catch
                {
                    return 0;
                }
            }
        }
        #endregion

        

        #region "获取当前Cookies名称"
        /// <summary>
        /// "获取当前Cookies名称
        /// </summary>
        public static string Get_CookiesName
        {
            get
            {
                return "MJWork_YKS_zhlb";
            }
        }
        #endregion

        #region "获取WEBCache名称前辍"
        /// <summary>
        /// 获取WEBCache名称前辍
        /// </summary>
        public static string Get_WebCacheName
        {
            get
            {
                return "MJWork_YKS_zhlb";
            }
        }
        #endregion

        #region "设置页面不被缓存"
        /// <summary>
        /// 设置页面不被缓存
        /// </summary>
        public static void SetPageNoCache()
        {

            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.ExpiresAbsolute = System.DateTime.Now.AddSeconds(-1);
            HttpContext.Current.Response.Expires = 0;
            HttpContext.Current.Response.CacheControl = "no-cache";
            HttpContext.Current.Response.AddHeader("Pragma", "No-Cache");
        }
        #endregion

        #region "获取页面url"
        /// <summary>
        /// 获取当前访问页面地址
        /// </summary>
        public static string GetScriptName
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"].ToString();
                }
                else
                {
                 return   "";
                }
               
            }
        }

        /// <summary>
        /// 检测当前url是否包含指定的字符
        /// </summary>
        /// <param name="sChar">要检测的字符</param>
        /// <returns></returns>
        public static bool CheckScriptNameChar(string sChar)
        {
            bool rBool = false;
            if (GetScriptName.ToLower().LastIndexOf(sChar) >= 0)
                rBool = true;
            return rBool;
        }

        /// <summary>
        /// 获取当前页面的扩展名
        /// </summary>
        public static string GetScriptNameExt
        {
            get
            {
                return GetScriptName.Substring(GetScriptName.LastIndexOf(".") + 1);
            }
        }

        /// <summary>
        /// 获取当前访问页面地址参数
        /// </summary>
        public static string GetScriptNameQueryString
        {
            get
            {
                System.Threading.ThreadPool.QueueUserWorkItem((Object obj) => {
                    System.Web.HttpContext content = obj as HttpContext;
                    if (content != null)
                    {

                    }
                }, HttpContext.Current);
                return HttpContext.Current!=null?HttpContext.Current.Request.ServerVariables["QUERY_STRING"].ToString():"";
            }
        }

        /// <summary>
        /// 获得页面文件名和参数名
        /// </summary>
        public static string GetScriptNameUrl
        {
            get {
                string Script_Name = CommonUtil.GetScriptName;
                Script_Name = Script_Name.Substring(Script_Name.LastIndexOf("/") + 1);
                Script_Name += "?"+GetScriptNameQueryString;
                return Script_Name;
            }
        }

        /// <summary>
        /// 获得当前页面的文件名
        /// </summary>
        public static string GetScriptFileName
        {
            get {
                string Script_Name = CommonUtil.GetScriptName;
                Script_Name = Script_Name.Substring(Script_Name.LastIndexOf("/") + 1);
                return Script_Name;
            }
        }

        /// <summary>
        /// 获取当前访问页面Url
        /// </summary>
        public static string GetScriptUrl
        {
            get
            {
                return CommonUtil.GetScriptNameQueryString == "" ?
                    CommonUtil.GetScriptName :
                    string.Format("{0}?{1}", CommonUtil.GetScriptName, CommonUtil.GetScriptNameQueryString);
            }
        }

        /// <summary>
        /// 返回当前页面目录的url
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <returns></returns>
        public static string GetHomeBaseUrl(string FileName)
        {
            string Script_Name = CommonUtil.GetScriptName;
            return string.Format("{0}/{1}", Script_Name.Remove(Script_Name.LastIndexOf("/")), FileName);
        }

        /// <summary>
        /// 返回当前网站网址
        /// </summary>
        /// <returns></returns>
        public static string GetHomeUrl()
        {
            return HttpContext.Current.Request.Url.Authority;
        }

        /// <summary>
        /// 获取当前访问文件物理目录
        /// </summary>
        /// <returns>路径</returns>
        public static string GetScriptPath
        {
            get
            {
                string Paths = HttpContext.Current.Request.ServerVariables["PATH_TRANSLATED"].ToString();
                return Paths.Remove(Paths.LastIndexOf("\\"));
            }
        }
        #endregion

        #region "按字符串位数补0"
        /// <summary>
        /// 按字符串位数补0
        /// </summary>
        /// <param name="CharTxt">字符串</param>
        /// <param name="CharLen">字符长度</param>
        /// <returns></returns>
        public static string FillZero(string CharTxt, int CharLen)
        {
            if (CharTxt.Length < CharLen)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < CharLen - CharTxt.Length; i++)
                {
                    sb.Append("0");
                }
                sb.Append(CharTxt);
                return sb.ToString();
            }
            else
            {
                return CharTxt;
            }
        }

        #endregion

        #region "替换JS中特殊字符"
        /// <summary>
        /// 将JS中的特殊字符替换
        /// </summary>
        /// <param name="str">要替换字符</param>
        /// <returns></returns>
        public static string ReplaceJs(string str)
        {

            if (str != null)
            {
                str = str.Replace("\"", "&quot;");
                str = str.Replace("(", "&#40;");
                str = str.Replace(")", "&#41;");
                str = str.Replace("%", "&#37;");
            }

            return str;

        }
        #endregion

        #region "正式表达式验证"
        /// <summary>
        /// 正式表达式验证
        /// </summary>
        /// <param name="C_Value">验证字符</param>
        /// <param name="C_Str">正式表达式</param>
        /// <returns>符合true不符合false</returns>
        public static bool CheckRegEx(string C_Value, string C_Str)
        {
            Regex objAlphaPatt;
            objAlphaPatt = new Regex(C_Str, RegexOptions.Compiled);


            return objAlphaPatt.Match(C_Value).Success;
        }
        #endregion

        #region "检测当前字符是否在以,号分开的字符串中(xx,sss,xaf,fdsf)"
        /// <summary>
        /// 检测当前字符是否在以,号分开的字符串中(xx,sss,xaf,fdsf)
        /// </summary>
        /// <param name="TempChar">需检测字符</param>
        /// <param name="TempStr">待检测字符串</param>
        /// <returns>存在true,不存在false</returns>
        public static bool Check_Char_Is(string TempChar, string TempStr)
        {
            bool rBool = false;
            if (TempChar != null && TempStr != null)
            {
                string[] TempStrArray = TempStr.Split(',');
                for (int i = 0; i < TempStrArray.Length; i++)
                {
                    if (TempChar == TempStrArray[i].Trim())
                    {
                        rBool = true;
                        break;
                    }
                }
            }
            return rBool;
        }
        #endregion

        #region "上传文件配置"
        /// <summary>
        /// 上传目录设置
        /// </summary>
        //public static string UpLoadDir
        //{
        //    get
        //    {
        //        string uploadDir= FrameSystemInfo.GetSystemInfoTable.C_UPLOADPATH;
        //        if (uploadDir.StartsWith("/")) uploadDir = "~" + uploadDir;
        //        return uploadDir;
        //    }
        //}

        /// <summary>
        /// 图片缩图高度
        /// </summary>
        //public static int UpImgHeight
        //{
        //    get
        //    {
        //        //return Convert.ToInt32(ConfigurationManager.AppSettings["UpImgHeight"]);
        //        return FrameSystemInfo.GetSystemInfoTable.C_UPIMGHEIGHT;
        //    }
        //}
        ///// <summary>
        ///// 图片缩图宽度
        ///// </summary>
        //public static int UpImgWidth
        //{
        //    get
        //    {
        //        //return Convert.ToInt32(ConfigurationManager.AppSettings["UpImgWidth"]);
        //        return FrameSystemInfo.GetSystemInfoTable.C_UPIMGHEIGHT;
        //    }
        //}
        #endregion

        #region "前台设置"

        /// <summary>
        /// 菜单风格 default:经典
        /// </summary>
        public static string MenuStyle
        {
            get
            {
                if (HttpContext.Current.Request.Cookies["MenuStyle"] == null)
                {
                    return "default";
                }
                else
                {
                    return HttpContext.Current.Request.Cookies["MenuStyle"].Value;
                }
            }
            set
            {
                HttpContext.Current.Response.Cookies["MenuStyle"].Value = value.ToString();
            }
        }

        /// <summary>
        /// 分页每页记录数(默认10)
        /// </summary>
        public static int PageSize
        {
            get
            {
                if (HttpContext.Current.Request.Cookies["PageSize"] == null)
                {
                    return 10;
                }
                else
                {
                    return Convert.ToInt32(HttpContext.Current.Request.Cookies["PageSize"].Value);
                }
            }
            set
            {
                HttpContext.Current.Response.Cookies["PageSize"].Value = value.ToString();
            }
        }

        /// <summary>
        /// 表格样式(默认default)
        /// </summary>
        public static string TableSink
        {
            get
            {
                if (HttpContext.Current.Request.Cookies["TableSink"] == null)
                {
                    return "default";
                    //return "classical";
                }
                else
                {
                    return HttpContext.Current.Request.Cookies["TableSink"].Value.ToString();
                }
            }
            set
            {
                HttpContext.Current.Response.Cookies["TableSink"].Value = value;
            }
        }

       
        /// <summary>
        /// 用户在线过期时间 (分)默认30分 如果用户在当前设定的时间内没有任何操作,将会被系统自动退出
        /// </summary>
        public static int OnlineMinute
        {
            get
            {
                try
                {
                    int _onlineminute = Convert.ToInt32(ConfigurationManager.AppSettings["OnlineMinute"]);
                    if (_onlineminute == 0)
                        return 10000;
                    else
                        return _onlineminute;
                }
                catch
                {
                    return 30;
                }
            }
        }

        /// <summary>
        /// 是否允许清空操作日志
        /// </summary>
        public static bool AllowClearData
        {
            get {
                try
                {
                    bool _allowcleardata = Convert.ToBoolean(ConfigurationManager.AppSettings["AllowClearData"]);
                    return _allowcleardata;
                }
                catch
                {
                    return false;
                } 
            }
        }

        #endregion

        #region "产生GUID"
        /// <summary>
        /// 获取一个GUID字符串
        /// </summary>
        public static string GetGUID
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
        }
        #endregion

        #region "生成刷新部门列表js"
        /// <summary>
        /// 生成刷新部门列表js
        /// </summary>
        public static string BuildJs
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<script language=javascript>");
                sb.Append("window.parent.leftbody.location.reload();");
                sb.Append("</script>");

                return sb.ToString();
            }
        }
        #endregion

        #region "获取服务器IP"
        /// <summary>
        /// 获取服务器IP
        /// </summary>
        public static string GetServerIp
        {
            get
            {
                return HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"].ToString();
            }
        }
        #endregion

        #region "获取服务器操作系统"
        /// <summary>
        /// 获取服务器操作系统
        /// </summary>
        public static string GetServerOS
        {
            get
            {
                return Environment.OSVersion.VersionString;
            }
        }
        #endregion

        #region "获取服务器域名"
        /// <summary>
        /// 获取服务器域名
        /// </summary>
        public static string GetServerHost
        {
            get
            {
                return HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToString();
            }
        }
        #endregion

        #region "获取本站点对应的服务器端口号"
        /// <summary>
        /// 获取本站点对应的服务器端口号
        /// </summary>
        public static int GetServerPort
        {
            get
            {
                return Convert.ToInt32(HttpContext.Current.Request.ServerVariables["SERVER_PORT"]);
            }
        }
        #endregion

        #region "显示出错详细信息在用户页面(用户开发调试,在生产环境请设置为false)"
        /// <summary>
        /// 显示出错详细信息在用户页面(用户开发调试,在生产环境请设置为false)
        /// </summary>
        public static bool DispError
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["DispError"]);
            }
        }
        #endregion

        #region "根据IP获取IP查询Url地址"
        /// <summary>
        /// 根据IP获取IP查询Url地址
        /// </summary>
        /// <param name="IP">IP地址</param>
        /// <returns>查询url</returns>
        //public static string GetIPLookUrl(string IP)
        //{
        //    return string.Format("<a href='" + FrameSystemInfo.GetSystemInfoTable.C_IPLOOKURL + "' target='_blank'>{0}</a>", IP);
        //}
        #endregion

        #region "根据文件扩展名获取当前目录下的文件列表"
        /// <summary>
        /// 根据文件扩展名获取当前目录下的文件列表
        /// </summary>
        /// <param name="FileExt">文件扩展名</param>
        /// <returns>返回文件列表</returns>
        public static List<string> GetDirFileList(string FileExt)
        {
            List<string> FilesList = new List<string>();
            string[] Files = Directory.GetFiles(GetScriptPath, string.Format("*.{0}", FileExt));
            foreach (string var in Files)
            {
                FilesList.Add(System.IO.Path.GetFileName(var).ToLower());
            }
            return FilesList;
        }
        #endregion

        #region "根据文件相对路径生成下载Url地址"
        /// <summary>
        /// 根据文件相对路径生成下载Url地址
        /// </summary>
        /// <param name="FilePath">文件相对路径</param>
        /// <returns>加密后Url地址</returns>
        //public static string BuildDownFileUrl(string FilePath)
        //{
        //    string MKey = EncryptString(FilePath, FrameSystemInfo.GetSystemInfoTable.S_REGSIONGUID);
        //    MKey = HttpContext.Current.Server.UrlEncode(MKey);
        //    return string.Format("{0}?FileName={1}", new System.Web.UI.Control().ResolveUrl("~/Manager/DownLoadfile.aspx"), MKey);
        //}
        #endregion

        #region "根据文件扩展名获得文件的content-type"
        /// <summary>
        /// 根据文件扩展名获得文件的content-type
        /// </summary>
        /// <param name="fileextension">文件扩展名如.gif</param>
        /// <returns>文件对应的content-type 如:application/gif</returns>
        public static string GetFileMIME(string fileextension)
        {
            //set the default content-type
            const string DEFAULT_CONTENT_TYPE = "application/unknown";

            RegistryKey regkey, fileextkey;
            string filecontenttype;

            //the file extension to lookup


            try
            {
                //look in HKCR
                regkey = Registry.ClassesRoot;

                //look for extension
                fileextkey = regkey.OpenSubKey(fileextension);

                //retrieve Content Type value
                filecontenttype = fileextkey.GetValue("Content Type", DEFAULT_CONTENT_TYPE).ToString();

                //cleanup
                fileextkey = null;
                regkey = null;
            }
            catch
            {
                filecontenttype = DEFAULT_CONTENT_TYPE;
            }

            return filecontenttype;
        }
        #endregion

        #region "返回状态字符"
        /// <summary>
        /// 根据状态值返回状态字符
        /// </summary>
        /// <param name="i">状态值</param>
        /// <returns>返回字符</returns>
        public static string ReturnStatusInt(int i)
        {
            string rString = "未知";
            switch (i)
            {
                case 0:
                    rString = "正常";
                    break;
                case 1:
                    rString = "禁用";
                    break;
            }
            return rString;
        }
        #endregion

        #region "删除文件"
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="FilePath">删除的文件路径</param>
        /// <param name="PathType">删除文件路径类型</param>
        /// <returns>成功/失败</returns>
        public static bool DeleteFile(string FilePath, DeleteFilePathType PathType)
        {
            bool rBool = false;
            switch (PathType)
            {
                case DeleteFilePathType.DummyPath:
                    FilePath = HttpContext.Current.Server.MapPath(FilePath);
                    break;
                case DeleteFilePathType.NowDirectoryPath:
                    FilePath = HttpContext.Current.Server.MapPath(FilePath);
                    break;
                case DeleteFilePathType.PhysicsPath:
                    break;
            }
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                rBool = true;
            }
            return rBool;
        }
        #endregion

        #region "获得操作系统"
        /// <summary>
        /// 获得操作系统
        /// </summary>
        /// <returns>操作系统名称</returns>
        public static string GetSystem
        {
            get
            {
                string s = HttpContext.Current.Request.UserAgent.Trim().Replace("(", "").Replace(")", "");
                string[] sArray = s.Split(';');
                switch (sArray[2].Trim())
                {
                    case "Windows 4.10":
                        s = "Windows 98";
                        break;
                    case "Windows 4.9":
                        s = "Windows Me";
                        break;
                    case "Windows NT 5.0":
                        s = "Windows 2000";
                        break;
                    case "Windows NT 5.1":
                        s = "Windows XP";
                        break;
                    case "Windows NT 5.2":
                        s = "Windows 2003";
                        break;
                    case "Windows NT 6.0":
                        s = "Windows Vista";
                        break;
                    default:
                        s = "Other";
                        break;
                }
                return s;
            }
        }


        #endregion

        #region "获得状态文字"
        /// <summary>
        /// 获得状态文字
        /// </summary>
        /// <param name="Status">状态类型</param>
        /// <param name="AddWord">附加文字</param>
        /// <returns></returns>
        public static string GetStatTxt(int Status, string AddWord)
        {
            if (Status == 0)
                return "未" + AddWord;
            else
                return "己" + AddWord;
        }
        #endregion

        #region "获得在线统计数据保存环境"
        /// <summary>
        /// 获得在线统计数据保存环境
        /// </summary>
        //public static OnlineCountType GetOnlineCountType
        //{
        //    get
        //    {
        //        if (GetConfigOnlineCountType == 1)
        //            return OnlineCountType.DataBase;
        //        else
        //            return OnlineCountType.Cache;
        //    }
        //}

        /// <summary>
        /// 获得配置在线统计类型
        /// </summary>
        //private static int GetConfigOnlineCountType
        //{
        //    get
        //    {
        //        int rInt = 0;
        //        try
        //        {
        //            rInt = Convert.ToInt32(ConfigurationManager.AppSettings["OnlineCountType"]);
        //        }
        //        catch (Exception ex)
        //        {
        //            FileTxtLogs.WriteLog(ex);
        //        }
        //        return rInt;
        //    }
        //}
        #endregion

        #region "获得sessionid"
        /// <summary>
        /// 获得sessionid
        /// </summary>
        public static string GetSessionID
        {
            get
            {
                return HttpContext.Current.Session.SessionID;
            }
        }
        #endregion


        #region "进行base64编码"
        /// <summary>
        /// 进行base64编码
        /// </summary>
        /// <param name="s">字符</param>
        /// <returns></returns>
        public static string EnBase64(string s)
        {
            return Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(s));
        }
        #endregion

        #region "进行Base64解码"
        /// <summary>
        /// 进行Base64解码
        /// </summary>
        /// <param name="s">字符</param>
        /// <returns></returns>
        public static string DeBase64(string s)
        {
            return System.Text.Encoding.Default.GetString(Convert.FromBase64String(s));
        }
        #endregion 

        #region "获得缓存类配置(命名空间)"
        /// <summary>
        /// 获得缓存类配置(命名空间)
        /// </summary>
        public static string GetCachenamespace
        {
            get
            {
                return ConfigurationManager.AppSettings["Cachenamespace"];
            }
        }
        #endregion

        #region "获得缓存类配置(类名)"
        /// <summary>
        /// 获得缓存类配置(类名)
        /// </summary>
        public static string GetCacheclassName
        {
            get
            {
                return ConfigurationManager.AppSettings["CacheclassName"];
            }
        }
        #endregion 

        #region "将日期类型转换成字符"
        /// <summary>
        /// 将日期类型转换成字符
        /// </summary>
        /// <param name="s">日期</param>
        /// <returns>字符</returns>
        public static string ConvertDate(DateTime? s)
        {
            if (s.HasValue)
                return s.Value.ToString("yyyy/MM/dd");
            else
                return "";
        }
        #endregion 

        #region "格式化TextArea输入内容为html显示"
        /// <summary>
        /// 格式化TextArea输入内容为html显示
        /// </summary>
        /// <param name="s">要格式化内容</param>
        /// <returns>完成内容</returns>
        public static string FormatTextArea(string s)
        {
            s = s.Replace("\n", "<br>");
            s = s.Replace("\x20", "&nbsp;");
            return s;
        }
        #endregion 

        #region "检测Ip地址是否正确"
        /// <summary>
        /// 检测Ip地址是否正确
        /// </summary>
        /// <param name="ip">ip字符串</param>
        /// <returns>正确/不正确</returns>
        public static bool CheckIp(string ip)
        {
            System.Net.IPAddress ipa;
            if (System.Net.IPAddress.TryParse(ip, out ipa))
            {
                ipa = null;
                return true;
            }
            else
            {
                ipa = null;
                return false;
            }
        }
        #endregion

        #region "格式化日期24小时制为字符串如:2011/06/20 21:22:33"
        /// <summary>
        /// 格式化日期24小时制为字符串如:2011/06/20 21:22:33
        /// </summary>
        /// <param name="d">日期</param>
        /// <returns>字符</returns>
        public static string FormatDateToString(DateTime d)
        {
            return d.ToString("yyyy/MM/dd HH:mm:ss");
        }
        #endregion

        #region "格式化日期显示为字符"
        /// <summary>
        /// 格式化日期显示为字符
        /// </summary>
        /// <param name="d">日期</param>
        /// <returns></returns>
        public static string FormatDateToDispString(DateTime d)
        {
            return d.ToString("yyyy/MM/dd HH:mm:ss");    
        }
        #endregion

        #region "格式化为UTC时间"
        /// <summary>
        /// 格式化为UTC时间
        /// </summary>
        /// <param name="d">时间</param>
        /// <returns>格式化日期</returns>
        public static DateTime FormatDateToUTC(DateTime d)
        {
            return d.ToUniversalTime();
        }

        /// <summary>
        /// 格式化为UTC时间
        /// </summary>
        /// <param name="d">时间字符</param>
        /// <returns>时间</returns>
        public static DateTime FormatDateToUTC(string d)
        {
            return Convert.ToDateTime(d).ToUniversalTime();
        }
        #endregion

        #region "状态判断"
        /// <summary>
        /// 状态判断
        /// </summary>
        /// <param name="ID">状态ID</param>
        /// <returns>否,是</returns>
        public static string GetStatus(int ID)
        {
            if (ID == 0)
                return "否";
            else
                return "是";
        }
        #endregion

        #region 获得用户类型字符
        /// <summary>
        /// 获得用户类型字符
        /// </summary>
        /// <param name="typeid">类型值</param>
        /// <returns>类型字符</returns>
        public static string Sys_UserType(int typeid)
        {
            if (typeid == 0)
                return "超级用户";
            else if (typeid == 1)
                return "普通用户";
            else if (typeid == 2)
            {
                return "管理员";
            }
            else
                return "未知用户类型";
        }
        #endregion

        #region 导出Excel

        public static void ToExcel(System.Web.UI.Control ctl, string strFileName)
        {
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + strFileName + ".xls");
            HttpContext.Current.Response.Charset = "utf-8";

            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            //System.Text.Encoding.Default;	     

            HttpContext.Current.Response.ContentType = "application/ms-excel";     //设置输出流为简体中文						
            ctl.Page.EnableViewState = false;
            System.Globalization.CultureInfo myCItrad = new System.Globalization.CultureInfo("ZH-CN", true);
            System.IO.StringWriter tw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);

            ctl.RenderControl(hw);
            HttpContext.Current.Response.Write(tw.ToString());
            HttpContext.Current.Response.End();
            hw.Flush();
            hw.Close();
            tw.Flush();
            tw.Close();

        }
        //导出到Excel
        public static void ToExcel2(System.Web.UI.Control ctl, string strFileName)
        {
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + strFileName + ".xls");
            HttpContext.Current.Response.Charset = "GB2312";	//"utf-8";	

            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            //System.Text.Encoding.Default;	     

            HttpContext.Current.Response.ContentType = "application/ms-excel";     //设置输出流为简体中文						
            ctl.Page.EnableViewState = false;
            System.Globalization.CultureInfo myCItrad = new System.Globalization.CultureInfo("ZH-CN", true);
            System.IO.StringWriter tw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);

            ctl.RenderControl(hw);
            HttpContext.Current.Response.Write(tw.ToString());
            HttpContext.Current.Response.End();
            hw.Flush();
            hw.Close();
            tw.Flush();
            tw.Close();

        }
        //导出到Excel		
        //public static void ToExcel(DataTable dt)
        //{
        //    string sb = "";

        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        for (int i = 0; i < dt.Columns.Count; i++)
        //        {
        //            sb = sb + dr[i].ToString() + "\t";
        //        }
        //        sb = sb + "\n";
        //    }

        //    HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=myexcel.xls");
        //    HttpContext.Current.Response.Charset = "UTF-8";
        //    HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.Default;
        //    HttpContext.Current.Response.ContentType = "application/ms-excel";

        //    System.IO.StringWriter tw = new System.IO.StringWriter();
        //    System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);
        //    hw.WriteLine(sb.ToString());
        //    HttpContext.Current.Response.Write(tw.ToString());

        //    HttpContext.Current.Response.End();

        //    hw.Flush();
        //    hw.Close();
        //    tw.Flush();
        //    tw.Close();
        //}

        //导出到Excel
        public static void ToExcel(string sb ,string fileName)
        {
            HttpContext.Current.Response.AppendHeader("Content-Disposition", String.Format("inline;filename={0}.xls", fileName));
            HttpContext.Current.Response.Charset = "GB2312";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.Default;
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            System.Globalization.CultureInfo myCItrad = new System.Globalization.CultureInfo("ZH-CN", true);

            System.IO.StringWriter tw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);
            hw.WriteLine(sb.ToString());
            HttpContext.Current.Response.Write(tw.ToString());

            HttpContext.Current.Response.End();

            hw.Flush();
            hw.Close();
            tw.Flush();
            tw.Close();
        }

        #endregion
        #region 判断字符串是不是日期格式

        public static bool CheckDate(string str)
        {
            try
            {
              DateTime dateTime=  Convert.ToDateTime(str);
              return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        #endregion


        /// <summary>
        /// 导出数据
        /// zangj 2020-10-29
        /// 注意：这里只考虑了普通和二层复合表头，二层以上表头不适用
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public static Stream ExportDataStream(DataTable dataTableXls, List<TableModel> tableConifgList)
        //{

        //    #region 生成xls表头
        //    IWorkbook book = new HSSFWorkbook();
        //    ICellStyle cellstyle = book.CreateCellStyle();
        //    cellstyle.VerticalAlignment = VerticalAlignment.Center;
        //    cellstyle.Alignment = HorizontalAlignment.Center;
        //    //cellstyle.BorderBottom = BorderStyle.Thin;
        //    //cellstyle.BorderLeft = BorderStyle.Thin;
        //    //cellstyle.BorderRight = BorderStyle.Thin;
        //    //cellstyle.BorderTop = BorderStyle.Thin;


        //    int dataRowStart = 1;//数据起始行默认1
        //    ISheet sheet = book.CreateSheet("sheet1");
        //    IRow rowTitle = sheet.CreateRow(0);//主标题

        //    if (tableConifgList.Exists(t => t.children != null && t.children.Length > 0))
        //    {
        //        int i = 0; //列索引
        //        dataRowStart = 2;
        //        IRow rowTitleSub = sheet.CreateRow(1); //副标题
        //        foreach (TableModel item in tableConifgList)
        //        {
        //            rowTitle.CreateCell(i).SetCellValue(item.label);

        //            if (item.children != null && item.children.Length > 0)
        //            {
        //                sheet.AddMergedRegion(new CellRangeAddress(0, 0, i, i + item.children.Length - 1));
        //                sheet.GetRow(0).GetCell(i).CellStyle = cellstyle;
        //                for (int j = 0; j < item.children.Length; j++)
        //                {
        //                    rowTitleSub.CreateCell(i + j).SetCellValue(item.children[j].label);
        //                }

        //                i += item.children.Length;
        //            }
        //            else
        //            {
        //                sheet.AddMergedRegion(new CellRangeAddress(0, 1, i, i));
        //                sheet.GetRow(0).GetCell(i).CellStyle = cellstyle;
        //                i++;
        //            }
        //        }

        //    }
        //    else
        //    {
        //        for (int j = 0; j < tableConifgList.Count; j++)
        //        {
        //            ICell icell = rowTitle.CreateCell(j);
        //            icell.SetCellValue(tableConifgList[j].label);
        //            icell.CellStyle = cellstyle;
        //        }

        //    }

        //    for (int k = 0; k < dataTableXls.Rows.Count; k++)
        //    {
        //        IRow row = sheet.CreateRow(k + dataRowStart);
        //        for (int l = 0; l < dataTableXls.Columns.Count; l++)
        //        {
        //            object tmpCellValue = dataTableXls.Rows[k][dataTableXls.Columns[l]];
        //            row.CreateCell(l).SetCellValue(tmpCellValue != null ? tmpCellValue.ToString() : "");
        //        }


        //    }

        //    #endregion
        //    //for (int m = 0; m < i; m++)
        //    //{
        //    //    sheet.AutoSizeColumn(m);//m：根据标题的个数设置自动列宽
        //    //}

        //    MemoryStream ms = new MemoryStream();
        //    book.Write(ms);
        //    ms.Seek(0, SeekOrigin.Begin);
        //    return ms;
        //}

        /// <summary>
        /// 解析MENU映射--获取中文菜单
        /// </summary>
        /// <param name="comname"></param>
        /// <returns></returns>
        public static string GetMenuName(string api)
        {
            #region 获取配置文件里的菜单
            Stream s = null;
            string file= ConfigurationManager.AppSettings["menuMapper"];
            if (file.StartsWith("assembly://"))
            {
                string prefixStr = "assembly://";
                string assemblyName = file.Substring(prefixStr.Length, file.LastIndexOf("/") - prefixStr.Length);
                string fileName = file.Substring(file.LastIndexOf("/") + 1);
                //Assembly assembly= Assembly.Load(assemblyName);
                //Assembly assembly2 = Assembly.GetExecutingAssembly();
              //string[] str=  assembly.GetManifestResourceNames();
                s = Assembly.Load(assemblyName).GetManifestResourceStream(fileName);
               
            }

            string fcom = "";
            XmlDocument xml = new XmlDocument();
            xml.Load(s);
            XmlNodeList XmlNodes = xml.GetElementsByTagName("Mapper");
            for (int i = 0; i < XmlNodes.Count; i++)
            {
                for (int I = 0; I < XmlNodes[i].ChildNodes.Count; I++)
                {
                    XmlElement xl = XmlNodes[i].ChildNodes[I] as XmlElement;
                    string tbname = "";
                    if (xl.GetAttribute("url").ToString() == api)
                    {
                        tbname = xl.GetAttribute("menuname").ToString();
                        if (tbname.Trim().Length > 0)
                        {
                            fcom = tbname;
                            break;
                        }
                    }
                }
            }
            return fcom;
            #endregion
        }
    }

    #region "枚举类型"
    /// <summary>
    /// 删除文件路径类型
    /// </summary>
    public enum DeleteFilePathType
    {
        /// <summary>
        /// 物理路径
        /// </summary>
        PhysicsPath = 1,
        /// <summary>
        /// 虚拟路径
        /// </summary>
        DummyPath = 2,
        /// <summary>
        /// 当前目录
        /// </summary>
        NowDirectoryPath = 3
    }

    /// <summary>
    /// 获取方式
    /// </summary>
    public enum MethodType
    {
        /// <summary>
        /// Post方式
        /// </summary>
        Post = 1,
        /// <summary>
        /// Get方式
        /// </summary>
        Get = 2
    }
   
    /// <summary>
    /// 获取数据类型
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// 字符
        /// </summary>
        Str = 1,
        /// <summary>
        /// 日期
        /// </summary>
        Dat = 2,
        /// <summary>
        /// 整型
        /// </summary>
        Int = 3,
        /// <summary>
        /// 长整型
        /// </summary>
        Long = 4,
        /// <summary>
        /// 双精度小数
        /// </summary>
        Double = 5,
        /// <summary>
        /// 只限字符和数字
        /// </summary>
        CharAndNum = 6,
        /// <summary>
        /// 只限邮件地址
        /// </summary>
        Email = 7,
        /// <summary>
        /// 只限字符和数字和中文
        /// </summary>
        CharAndNumAndChinese = 8,
        /// <summary>
        /// 只限数字
        /// </summary>
        Number = 9,
        /// <summary>
        /// 只限数字和逗号的中英文格式
        /// </summary>
        NumAndStr = 10,
        /// <summary>
        /// 只限括号内字符^%&',;=?$\"
        /// </summary>
        CharStr = 11,
        /// <summary>
        /// 只限26个英文字母组成的字符串
        /// </summary>
        CharEnglish = 12

    }

    /// <summary>
    /// 表操作方法
    /// </summary>
    public enum DataTable_Action
    {
        /// <summary>
        /// 插入
        /// </summary>
        Insert = 0,
        /// <summary>
        /// 更新
        /// </summary>
        Update = 1,
        /// <summary>
        /// 删除
        /// </summary>
        Delete = 2
    }
    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum OnlineCountType
    {
        /// <summary>
        /// 缓存
        /// </summary>
        Cache = 0,
        /// <summary>
        /// 数据库
        /// </summary>
        DataBase = 1
    }
    #endregion

}