using System.Configuration;
using HYPDAWebApi.App_Data;

/// <summary>
/// IFConfig 的摘要说明
/// </summary>
public class GYConfig
{
    private static GYConfig instance;

    public static GYConfig Instance()
    {
        if (instance == null) instance = new GYConfig();
        return instance;
    }

    public OracleBase oracleBase;
    public static string connectionString4 = ConfigurationManager.AppSettings["ConnectionString4"];    

    public GYConfig()
    {
        oracleBase = new OracleBase(connectionString4, "GY");
    }
}
