using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HYPDAWebApi.App_Data;

/// <summary>
/// IFConfig 的摘要说明
/// </summary>
public class IFConfig
{
        private static IFConfig instance;

        public static IFConfig Instance()
        {
            if (instance == null) instance = new IFConfig();
            return instance;
        }
        
        public OracleBase oracleBase;
        public static string connectionString2 = ConfigurationManager.AppSettings["ConnectionString2"];
        //string ora_config = "Data Source=IFHYMES;Persist Security Info=True;User ID=ifhymes;Password=sql;";
        
        public IFConfig()
        {
            oracleBase = new OracleBase(connectionString2);
        }
}
