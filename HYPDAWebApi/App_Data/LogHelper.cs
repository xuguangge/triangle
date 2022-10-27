using System;
using log4net;
using System.Web;
using static HYPDAWebApi.App_Data.Logenum;

namespace HYPDAWebApi.App_Data
{

    public static class LogHelper
    {
        //private static log4net.ILog m_Log;

        //public LogHelper(LogType _LogType)
        //{
        //    string s = _LogType.ToString();
        //    m_Log = log4net.LogManager.GetLogger(s);
        //}
        
        private static ILog error_Log = log4net.LogManager.GetLogger("SysLog");
        private static ILog info_Log = log4net.LogManager.GetLogger("InfoLog");
        private static ILog debug_Log = log4net.LogManager.GetLogger("SysLog");
        private static ILog warm_Log = log4net.LogManager.GetLogger("PersistenceLog");
        private static ILog dao_Log = log4net.LogManager.GetLogger("DbLoger");

        public static void WriteInfoDb(string message, string actionClick)
        {
            string userName = string.Empty;
            if (HttpContext.Current != null)
            {
                userName = HttpContext.Current.User.Identity.Name;
            }
            if (!actionClick.Contains("登录"))
            {
                LogicalThreadContext.Properties["URL"] = CommonUtil.GetScriptUrl;
                LogicalThreadContext.Properties["CHNMENU"] = CommonUtil.GetMenuName(CommonUtil.GetScriptUrl.Split('/')[2]);
                //从系统缓存获取用户信息
                string CacheKey = string.Format("{0}-UserInfo-{1}", CommonUtil.Get_WebCacheName, userName);

                //SYS_USER _sYS_USER=  RedisHelper.Instance().GetHashValue<SYS_USER>(CacheKey);
                //SYS_USER sYS_USER = RedisHelper.RedisHelper.Instance().GetHashValue<SYS_USER>(CacheKey);

                //LogicalThreadContext.Properties["USERNAME"] = sYS_USER != null ? sYS_USER.U_CNAME : userName;
                // LogicalThreadContext.Properties["USERNAME"] = HttpContext.Current.User.Identity.Name;
                LogicalThreadContext.Properties["USERIP"] = CommonUtil.GetIPAddress();
                LogicalThreadContext.Properties["ACTIONCLICK"] = actionClick;
            }
            else
            {
                LogicalThreadContext.Properties["URL"] = "/api/logion";
                LogicalThreadContext.Properties["CHNMENU"] = "登录页";
                LogicalThreadContext.Properties["USERNAME"] = actionClick.Split('|')[1];
                LogicalThreadContext.Properties["USERIP"] = actionClick.Split('|')[2];
                LogicalThreadContext.Properties["ACTIONCLICK"] = actionClick.Split('|')[0]; ;
            }

            dao_Log.Info(message);
            //全局的
            // GlobalContext.Properties["CustomColumn"] = "Custom value";
        }

        /// <summary>
        /// 输出错误级别日志
        /// </summary>
        /// <param name="message">输出的消息</param>
        public static void Error(string message, Exception ex)
        {
            string err = BeautyErrorMsg(ex);
            string userName = string.Empty;
            if (HttpContext.Current != null)
            {
                userName = HttpContext.Current.User.Identity.Name;
            }
            message = string.Format("{0} | {1} | {2} | {3} | {4} ", CommonUtil.GetIPAddress(), CommonUtil.GetScriptUrl, userName,
                ex.Source, message + ":" + err);
            //记录日志
            WriteLog(LogLevel.Error, message, ex);
        }
        public static void BackgroudError(string message, Exception ex)
        {
            string err = BeautyErrorMsg(ex);
            message = string.Format("{0} | {1} | {2}| {3} | {4} ", message + ":" + err, "0.0.0.0", "后台数据服务", "", "");
            //记录日志
            WriteLog(LogLevel.Error, message, ex);
        }
        /// <summary>
        /// 输出警告级别日志
        /// </summary>
        /// <param name="message">输出的消息</param>
        public static void Warning(string message)
        {
            string userName = string.Empty;
            if (HttpContext.Current != null)
            {
                userName = HttpContext.Current.User.Identity.Name;
            }
            message = string.Format("{0} | {1} | {2}| {3} | {4} ", message, CommonUtil.GetIPAddress(), CommonUtil.GetScriptUrl, userName,
                "");
            //记录日志
            WriteLog(LogLevel.Warning, message);
        }
        public static void BackgroudWarning(string message)
        {
            message = string.Format("{0} | {1} | {2}| {3}| {4} ", message, "0.0.0.0", "后台数据服务", "", "");
            //记录日志
            WriteLog(LogLevel.Warning, message);
        }
        /// <summary>
        /// 输出信息级别日志
        /// </summary>
        /// <param name="message">输出的消息</param>
        public static void Info(string message)
        {
            message = string.Format("{0} | {1} | {2} | {3} | {4} ", CommonUtil.GetIPAddress(), CommonUtil.GetScriptUrl,
                 HttpContext.Current != null ? HttpContext.Current.User.Identity.Name : "", "", message);
            //记录日志
            WriteLog(LogLevel.Info, message);
        }
        public static void BackgroudInfo(string message)
        {
            message = string.Format("{0} | {1} | {2} | {3} | {4} ", message, "0.0.0.0", "后台数据服务", "", "");
            //记录日志
            WriteLog(LogLevel.Info, message);
        }
        /// <summary>
        /// 输出调试级别日志
        /// </summary>
        /// <param name="message">输出的消息</param>
        public static void Debug(string message)
        {
            message = string.Format("{0} | {1} | {2} | {3} | {4} ", message, CommonUtil.GetIPAddress(), CommonUtil.GetScriptUrl, HttpContext.Current != null ? HttpContext.Current.User.Identity.Name : "", "");
            //记录日志
            WriteLog(LogLevel.Debug, message);
        }
        public static void BackgroudDebug(string message)
        {
            message = string.Format("{0} | {1} | {2} | {3} | {4}", message, "0.0.0.0", "后台数据服务", "", "");
            //记录日志
            WriteLog(LogLevel.Debug, message);
        }
        /// <summary>
        /// 记录系统日志
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <param name="message">输出的消息</param>
        private static void WriteLog(LogLevel logLevel, string message, Exception ex = null)
        {
            try
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        debug_Log.Debug(message);
                        break;
                    case LogLevel.Error:
                        error_Log.Error(message);
                        break;
                    case LogLevel.Info:
                        info_Log.Info(message);
                        //dao_Log.Info(message);
                        break;
                    case LogLevel.Warning:
                        warm_Log.Warn(message);
                        break;
                }
            }
            catch (Exception excc)
            {

                
            }
           

        }
        /// <summary>
        /// 错误记录
        /// </summary>
        /// <param name="info">附加信息</param>
        /// <param name="ex">错误</param>
        private static void ErrorLog(string info, Exception ex)
        {
            if (!string.IsNullOrEmpty(info) && ex == null)
            {
                error_Log.ErrorFormat("【附加信息】:{0}；", new object[] { info });
            }
            else if (!string.IsNullOrEmpty(info) && ex != null)
            {
                string errorMsg = BeautyErrorMsg(ex);
                error_Log.ErrorFormat("【附加信息】:{0}；{1}", new object[] { info, errorMsg });
            }
            else if (string.IsNullOrEmpty(info) && ex != null)
            {
                string errorMsg = BeautyErrorMsg(ex);
                error_Log.Error(errorMsg);
            }
        }
        /// <summary>
        /// 美化错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns>错误信息</returns>
        private static string BeautyErrorMsg(Exception ex)
        {
            string errorMsg = string.Format("【异常类型】:{0}；【异常信息】:{1}", new object[] { ex.GetType().Name.Replace("\r", "").Replace("\n", "<br />"), ex.Message.Replace("\r", "").Replace("\n", "<br />") });
            // errorMsg += "\r\n";
            return errorMsg;
        }

    }
}
