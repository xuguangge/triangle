using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using static System.Collections.Specialized.BitVector32;

namespace HYPDAWebApi.App_Data
{
    /// <summary>
    /// 返回类
    /// </summary>
    public static class ReponseHelper
    {
        /// <summary>
        /// 数据返回
        /// </summary>
        /// <param name="info">记录信息</param>
        /// <param name="code">返回状态编码</param>
        /// <param name="msg">返修信息</param>
        /// <param name="data">返回数据</param>
        /// <returns></returns>
        public static ReponseData ReponesInfo(string code, string msg, object data)
        {

            LogHelper.Info(msg);
            return new ReponseData()
            {
                code = code
            };
        }
        /// <summary>
        /// 数据返回
        /// </summary>
        /// <param name="info">记录信息</param>
        /// <param name="code">返回状态编码</param>
        /// <param name="msg">返修信息</param>
        /// <param name="data">返回数据</param>
        /// <param name="writeDb">是否写入数据库</param>
        /// <param name="actionClick">操作按钮</param>
        /// <returns></returns>
        public static ReponseData ReponesInfo(string info, string code, string msg, object data,bool writeDb=false,string actionClick="")
        {

            if (writeDb)
            {
                LogHelper.WriteInfoDb(info, actionClick);
            }
            else
            {
                LogHelper.Info(info);
            }
            return new ReponseData()
            {
                code = code,
                msg = msg,
                data = data
            };
        }
        /// <summary>
        /// 数据返回
        /// </summary>
        /// <param name="info">记录信息</param>
        /// <param name="code">返回状态编码</param>
        /// <param name="msg">返修信息</param>
        /// <param name="data">返回数据</param>
        /// <param name="ex">异常</param>
        /// <returns></returns>
        /// <returns></returns>
        public static ReponseData ReponesError(string info, string code, string msg, object data,Exception ex)
        {
            LogHelper.Error(info, ex);
            return new ReponseData()
            {
                code = code,
                msg = msg,
                data = data,
                ex= ex
            };
        }
    }
}
