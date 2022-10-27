using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
namespace HYPDAWebApi.App_Data
{
    public class Logenum
    {
        /// <summary>
        /// 日志等级
        /// </summary>
       public  enum LogLevel
        {
            Error,
            Debug,
            Warning,
            Info
        }
        /// <summary>
        /// 日志类型
        /// </summary>
      public  enum LogType
        {
            InfoLog,
            SysLog,
            CommLog,
            ThirdLog,
            PersistenceLog
        }

    
    }
}
