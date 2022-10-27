using System;
using System.Data;
using HYPDAWebApi.App_Data;
using HYPDAWebApi.DBHelper;

/// <summary>
/// SHIFT 的摘要说明
/// </summary>
public class SHIFT
{

    public static DateTime STime;
    public static DateTime ETime;
    public static DateTime WDATE;
    public static string WSHT;
    public static string WBAN;
    public static string NSHT;
    public static DateTime shtETime;

    public static void GetSht()
    {
        GetShift( DateTime.Now);
    }

    public static DataRow GetShift( DateTime dTime)
    {
        //先判断实际时间对于的工厂日期
        DataTable BetTimeB = GetBetTime(dTime);
        DataTable BetTimeC = GetBetTime(dTime.AddDays(1));
        DateTime WDATE = dTime;
        DateTime STime = dTime;
        DateTime ETime = dTime;
        string sht = "1";
        string ban = "A";
        //string nsht = "2";
        //string nban = "B";
        if (BetTimeB.Rows[0]["DIV"].ToString() == "1")
        {
            if (DateTime.Parse(BetTimeB.Rows[0]["DATE2S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeB.Rows[0]["DATE1E"].ToString()))
            {
                WDATE = DateTime.Parse(BetTimeB.Rows[0]["WDATE"].ToString());
                STime = DateTime.Parse(BetTimeB.Rows[0]["DATE2S"].ToString());
                ETime = DateTime.Parse(BetTimeB.Rows[0]["DATE1E"].ToString());
                if (DateTime.Parse(BetTimeB.Rows[0]["DATE1S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeB.Rows[0]["DATE1E"].ToString()))
                {
                    sht = "1";
                    ban = BetTimeB.Rows[0]["SHT1"].ToString();
                }
                else if (DateTime.Parse(BetTimeB.Rows[0]["DATE2S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeB.Rows[0]["DATE2E"].ToString()))
                {
                    sht = "2";
                    ban = BetTimeB.Rows[0]["SHT2"].ToString();
                }
                else
                {
                    sht = "3";
                    ban = BetTimeB.Rows[0]["SHT3"].ToString();
                }
            }
        }
        else
        {
            if (DateTime.Parse(BetTimeB.Rows[0]["DATE3S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeB.Rows[0]["DATE1E"].ToString()))
            {
                WDATE = DateTime.Parse(BetTimeB.Rows[0]["WDATE"].ToString());
                STime = DateTime.Parse(BetTimeB.Rows[0]["DATE3S"].ToString());
                ETime = DateTime.Parse(BetTimeB.Rows[0]["DATE1E"].ToString());
                if (DateTime.Parse(BetTimeB.Rows[0]["DATE1S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeB.Rows[0]["DATE1E"].ToString()))
                {
                    sht = "1";
                    ban = BetTimeB.Rows[0]["SHT1"].ToString();
                }
                else
                {
                    sht = "3";
                    ban = BetTimeB.Rows[0]["SHT3"].ToString();
                }
            }
        }
        if (BetTimeC.Rows[0]["DIV"].ToString() == "1")
        {
            if (DateTime.Parse(BetTimeC.Rows[0]["DATE2S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeC.Rows[0]["DATE1E"].ToString()))
            {
                WDATE = DateTime.Parse(BetTimeC.Rows[0]["WDATE"].ToString());
                STime = DateTime.Parse(BetTimeC.Rows[0]["DATE2S"].ToString());
                ETime = DateTime.Parse(BetTimeC.Rows[0]["DATE1E"].ToString());

                if (DateTime.Parse(BetTimeC.Rows[0]["DATE1S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeC.Rows[0]["DATE1E"].ToString()))
                {
                    sht = "1";
                    ban = BetTimeC.Rows[0]["SHT1"].ToString();
                }
                else if (DateTime.Parse(BetTimeC.Rows[0]["DATE2S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeC.Rows[0]["DATE2E"].ToString()))
                {
                    sht = "2";
                    ban = BetTimeC.Rows[0]["SHT2"].ToString();
                }
                else
                {
                    sht = "3";
                    ban = BetTimeC.Rows[0]["SHT3"].ToString();
                }

            }
        }
        else
        {
            if (DateTime.Parse(BetTimeC.Rows[0]["DATE3S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeC.Rows[0]["DATE1E"].ToString()))
            {
                WDATE = DateTime.Parse(BetTimeC.Rows[0]["WDATE"].ToString());
                STime = DateTime.Parse(BetTimeC.Rows[0]["DATE3S"].ToString());
                ETime = DateTime.Parse(BetTimeC.Rows[0]["DATE1E"].ToString());
                if (DateTime.Parse(BetTimeC.Rows[0]["DATE1S"].ToString()) <= dTime && dTime <= DateTime.Parse(BetTimeC.Rows[0]["DATE1E"].ToString()))
                {
                    sht = "1";
                    ban = BetTimeC.Rows[0]["SHT1"].ToString();
                }
                else
                {
                    sht = "3";
                    ban = BetTimeC.Rows[0]["SHT3"].ToString();
                }
            }
        }
        DataTable dt = new DataTable();
        dt.Columns.Add("WDATE", typeof(DateTime));
        dt.Columns.Add("STime", typeof(DateTime));
        dt.Columns.Add("ETime", typeof(DateTime));
        dt.Columns.Add("WSHT", typeof(string));
        dt.Columns.Add("WBAN", typeof(string));
        //dt.Columns.Add("NWSHT", typeof(string));
        //dt.Columns.Add("NWBAN", typeof(string));
        dt.Rows.Add(WDATE, STime, ETime, sht, ban);
        return dt.Rows[0];

    }

    public static DataTable GetBetTime( DateTime time)
    {
        string sql = "SELECT DIV,WDATE,TIM2,TIM3,TIM1,SHT2,SHT3,SHT1 FROM EDD0004 WHERE  to_char(WDATE,'yyyy-MM-dd')='" + time.ToString("yyyy-MM-dd") + "'";
        DataTable GetEDD0004 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0]; 
        string WDATE = "2015-10-29";
        string TIM2 = "15:00:00";
        string TIM3 = "23:00:00";
        string TIM1 = "07:00:00";
        string SHT2 = "B";
        string SHT3 = "C";
        string SHT1 = "A";
        string div = "1";
        //前提条件是，早班的开始时间，结束时间都在当日

        DateTime DATE2S = time;
        DateTime DATE2E = time;
        DateTime DATE3S = time;
        DateTime DATE3E = time;
        DateTime DATE1S = time;
        DateTime DATE1E = time;

        if (GetEDD0004.Rows.Count > 0)
        {
            //三班倒
            if (GetEDD0004.Rows[0]["DIV"].ToString() == "1")
            {
                WDATE = DateTime.Parse(GetEDD0004.Rows[0]["WDATE"].ToString().Trim()).ToString("yyyy-MM-dd");
                TIM2 = DateTime.Parse(GetEDD0004.Rows[0]["TIM2"].ToString().Trim()).ToString("HH:mm:ss");
                TIM3 = DateTime.Parse(GetEDD0004.Rows[0]["TIM3"].ToString().Trim()).ToString("HH:mm:ss");
                TIM1 = DateTime.Parse(GetEDD0004.Rows[0]["TIM1"].ToString().Trim()).ToString("HH:mm:ss");
                SHT2 = GetEDD0004.Rows[0]["SHT2"].ToString().Trim();
                SHT3 = GetEDD0004.Rows[0]["SHT3"].ToString().Trim();
                SHT1 = GetEDD0004.Rows[0]["SHT1"].ToString().Trim();

                //初始化中，夜，早开始时间
                //中班开始时间  15:00:00                                       
                DATE2S = Convert.ToDateTime(WDATE + " " + TIM2);

                //夜班开始时间  23:00:00     
                DATE3S = Convert.ToDateTime(WDATE + " " + TIM3);

                //早班开始时间  07:00:00                                            
                DATE1S = Convert.ToDateTime(WDATE + " " + TIM1);
                if (DATE1S < DATE2S)
                {
                    DATE2S = DATE2S.AddDays(-1);
                }
                if (DATE1S < DATE3S)
                {
                    DATE3S = DATE3S.AddDays(-1);
                }
                //中班结束时间
                DATE2E = DATE3S.AddSeconds(-1);
                DATE3E = DATE1S.AddSeconds(-1);
                DATE1E = DATE2S.AddDays(1).AddSeconds(-1);
            }
            else
            {
                //两班到，只考虑夜班跨天
                div = "2";
                WDATE = DateTime.Parse(GetEDD0004.Rows[0]["WDATE"].ToString().Trim()).ToString("yyyy-MM-dd");
                TIM3 = DateTime.Parse(GetEDD0004.Rows[0]["TIM3"].ToString().Trim()).ToString("HH:mm:ss");
                TIM1 = DateTime.Parse(GetEDD0004.Rows[0]["TIM1"].ToString().Trim()).ToString("HH:mm:ss");
                SHT3 = GetEDD0004.Rows[0]["SHT3"].ToString().Trim();
                SHT1 = GetEDD0004.Rows[0]["SHT1"].ToString().Trim();
                //中班开始时间
                DATE3S = Convert.ToDateTime(WDATE + " " + TIM3);
                //早班开始时间
                DATE1S = Convert.ToDateTime(WDATE + " " + TIM1);
                if (DATE1S < DATE3S)
                {
                    DATE3S = DATE3S.AddDays(-1);
                }
                DATE3E = DATE1S.AddSeconds(-1);
                DATE1E = DATE3S.AddDays(1).AddSeconds(-1);
            }
        }

        DataTable dt = new DataTable();

        dt.Columns.Add("DATE2S", typeof(DateTime));
        dt.Columns.Add("DATE2E", typeof(DateTime));
        dt.Columns.Add("DATE3S", typeof(DateTime));
        dt.Columns.Add("DATE3E", typeof(DateTime));
        dt.Columns.Add("DATE1S", typeof(DateTime));
        dt.Columns.Add("DATE1E", typeof(DateTime));
        dt.Columns.Add("WDATE", typeof(DateTime));
        dt.Columns.Add("SHT2", typeof(string));
        dt.Columns.Add("SHT3", typeof(string));
        dt.Columns.Add("SHT1", typeof(string));
        dt.Columns.Add("DIV", typeof(string));
        dt.Rows.Add(DATE2S, DATE2E, DATE3S, DATE3E, DATE1S, DATE1E, time, SHT2, SHT3, SHT1, div);

        return dt;
    }
}
