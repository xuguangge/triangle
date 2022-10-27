using System;
using System.Collections.Generic;
using System.Web.Http;
using HYPDAWebApi.App_Data;
using System.Data;
using log4net.Util;
using System.Threading.Tasks;
using HYPDAWebApi.DBHelper;
using HYPDAWebApi.Models.ViewModel;
using System.Collections;

namespace HYPDAWebApi.Controllers
{
    /// <summary>
    /// 成型半成品
    /// </summary>
    public class FormedSemiController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";

        #region 成型鼓交替原因
        /// <summary>
        /// 成型鼓上机
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetCCODE")]
        public DataTable GetCCODE()
        {
          
            try
            {
                LogHelper.Debug("测试" + "sss");
                DataTable dataTable = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, "SELECT CCODE,CDESC FROM MDE0002 WHERE USEYN='Y'", null).Tables[0];
                LogHelper.Debug("测试" + "sss");
                return dataTable;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        #endregion

        #region 成型鼓上机
        /// <summary>
        /// 成型鼓上机
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/CODINGUP")]
        public string CODINGUP(string MCHID, string LR, string CODING, string CDESC, string INCH, string SBAN, string NAME)
        {
           
            string sInsScan = "";
            string TS = "";
            try
            {
                if (LR == "主鼓左")
                {

                    if (CODING.EndsWith("R"))
                    {
                        return "所上机鼓侧与鼓编号不一致，请确认后上机！！！";
                    }

                    //ygjy 校验右鼓是否有上机，是否和左鼓配套
                    string ygjy = @"SELECT CODING3 FROM (select * from  mde0003 WHERE MCHID='" + MCHID + "' and LR<>'副鼓'AND ETIME IS NULL  AND CODING1 IS NULL ORDER BY STIME DESC)WHERE ROWNUM=1 ";
                    DataTable dtjy = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, ygjy, null).Tables[0];
                    if (dtjy != null && dtjy.Rows.Count > 0)
                    {
                        if (CODING.Substring(0, CODING.Length - 1) != dtjy.Rows[0]["CODING3"].ToString().Substring(0, dtjy.Rows[0]["CODING3"].ToString().Length - 1))
                        {

                            LogHelper.Debug( "CODING:" + CODING + "-------" + dtjy.Rows[0]["CODING3"].ToString());
                            TS = "当前机台使用的非配套成型鼓";
                            //////////////////////////////////提示非配套鼓，然后继续往下走

                            string xgg = @"select *
                                          from (select *
                                                  from mde0004
                                                 where coding = '" + CODING + @"'
                                                   and div is not null
                                                   and etime is null
                                                 order by stime desc)
                                         where rownum = 1";
                            DataTable dtgg = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, xgg, null).Tables[0];
                           
                            if (dtgg != null && dtgg.Rows.Count > 0)
                            {
                                string sql1 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "'  AND LR='主鼓左' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                            DataTable dtM1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql1, null).Tables[0];

                                if (dtM1 != null && dtM1.Rows.Count > 0)
                                {
                                    return "该机台存在未下机成型鼓，请勿重复上机！！！";
                                }
                                else
                                {
                                    sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING1,CDESC,INCH,SBAN,STATE,DIV,SNAME,STIME,ENAME,ETIME,CCNT1) VALUES (";
                                    sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','" + SBAN + "','1','1','" + NAME + "',SYSDATE,NULL,NULL,'0')";
                                    string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '在线' WHERE CODING = '" + CODING + "'";
                                    //db.ExecuteNonQuery(sUpMde);
                                    OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                                    int iResult =  OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                                    if (iResult > 0)
                                    {
                                        return "OK" + TS;
                                    }
                                    else
                                    {
                                        return "Err-NONE" + sInsScan;
                                    }
                                }
                            }
                            else
                            {
                                return "该成型鼓未进行胶囊上机操作，不允许进行鼓上机！！！";
                            }


                        }
                        else
                        {


                            string xgg = @"select *
                                              from (select *
                                                      from mde0004
                                                     where coding = '" + CODING + @"'
                                                       and div is not null
                                                       and etime is null
                                                     order by stime desc)
                                             where rownum = 1";
                            DataTable dtgg = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, xgg, null).Tables[0];
                            if (dtgg != null && dtgg.Rows.Count > 0)
                            {
                                string sql1 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "'  AND LR='主鼓左' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                                DataTable dtM1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql1, null).Tables[0];
                                if (dtM1 != null && dtM1.Rows.Count > 0)
                                {
                                    return "该机台存在未下机成型鼓，请勿重复上机！！！";
                                }
                                else
                                {
                                    sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING1,CDESC,INCH,SBAN,STATE,DIV,SNAME,STIME,ENAME,ETIME,CCNT1) VALUES (";
                                    sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','" + SBAN + "','1','1','" + NAME + "',SYSDATE,NULL,NULL,'0')";
                                    string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '在线' WHERE CODING = '" + CODING + "'";
                                    //db.ExecuteNonQuery(sUpMde);
                                    OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                                    if (iResult > 0)
                                    {
                                        return "OK" + TS;
                                    }
                                    else
                                    {
                                        return "Err-NONE" + sInsScan;
                                    }
                                }
                            }
                            else
                            {
                                return "该成型鼓未进行胶囊上机操作，不允许进行鼓上机！！！";
                            }


                        }
                    }
                    else
                    {
                        string xgg = @"select *
                              from (select *
                                      from mde0004
                                     where coding = '" + CODING + @"'
                                       and div is not null
                                       and etime is null
                                     order by stime desc)
                             where rownum = 1";
                        DataTable dtgg = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, xgg, null).Tables[0];
                        if (dtgg != null && dtgg.Rows.Count > 0)
                        {
                            string sql1 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "'  AND LR='主鼓左' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                            DataTable dtM1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql1, null).Tables[0];
                            if (dtM1 != null && dtM1.Rows.Count > 0)
                            {
                                return "该机台存在未下机成型鼓，请勿重复上机！！！";
                            }
                            else
                            {
                                sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING1,CDESC,INCH,SBAN,STATE,DIV,SNAME,STIME,ENAME,ETIME,CCNT1) VALUES (";
                                sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','" + SBAN + "','1','1','" + NAME + "',SYSDATE,NULL,NULL,'0')";
                                string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '在线' WHERE CODING = '" + CODING + "'";
                                //db.ExecuteNonQuery(sUpMde);
                                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                               
                                if (iResult > 0)
                                {
                                    return "OK" + TS;
                                }
                                else
                                {
                                    return "Err-NONE" + sInsScan;
                                }
                            }
                        }
                        else
                        {
                            return "该成型鼓未进行胶囊上机操作，不允许进行鼓上机！！！";
                        }



                        //    string sql1 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "'  AND LR='主鼓' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                        //    DataTable dtM1 = db.GetDataTable(sql1);
                        //    if (dtM1 != null && dtM1.Rows.Count > 0)
                        //    {
                        //        return "该机台存在未下机成型鼓，请勿重复上机！！！";
                        //    }
                        //    else 
                        //    { 
                        //    sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING1,CDESC,INCH,STATE,DIV,SNAME,STIME,ENAME,ETIME) VALUES (";
                        //    sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','1','1','" + NAME + "',SYSDATE,NULL,NULL)";
                        //    int iResult = db.ExecuteNonQuery(sInsScan);
                        //    if (iResult > 0)
                        //    {
                        //        return "OK";
                        //    }
                        //    else
                        //    {
                        //        return "Err-NONE" + sInsScan;
                        //    }
                        //}
                    }

                }
                else if (LR == "主鼓右")   //主鼓又不分左右了.注释掉  ........//又分了 mmp 2021.08.03
                {




                    if (CODING.EndsWith("L"))
                    {

                        return "所上机鼓侧与鼓编号不一致,请确认后上机！！！";
                    }

                    //zgjy 校验右鼓是否有上机，是否和左鼓配套
                    string zgjy = @"SELECT CODING1 FROM (select * from  mde0003 WHERE MCHID='" + MCHID + "' and LR<>'副鼓'AND ETIME IS NULL AND CODING3 IS NULL  ORDER BY STIME DESC)WHERE ROWNUM=1 ";
                    DataTable dtjy2 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, zgjy, null).Tables[0];
                    if (dtjy2 != null && dtjy2.Rows.Count > 0)
                    {
                        LogHelper.Debug("CODING:" + CODING + "-------" + dtjy2.Rows[0]["CODING1"].ToString());

                        if (CODING.Substring(0, CODING.Length - 1) != dtjy2.Rows[0]["CODING1"].ToString().Substring(0, dtjy2.Rows[0]["CODING1"].ToString().Length - 1))
                        {

                            LogHelper.Debug("aaaaaCODING:" + CODING + "-------" + dtjy2.Rows[0]["CODING1"].ToString());

                            TS = "当前机台使用的非配套成型鼓";



                            string xgg1 = @"select *
                                          from (select *
                                                  from mde0004
                                                 where coding = '" + CODING + @"'
                                                   and div is not null
                                                   and etime is null
                                                 order by stime desc)
                                         where rownum = 1";
                            DataTable dtgg1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, xgg1, null).Tables[0];
                            if (dtgg1 != null && dtgg1.Rows.Count > 0)
                            {

                                string sql2 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "' AND LR='主鼓右' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                                DataTable dtM2 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql2, null).Tables[0];
                                if (dtM2 != null && dtM2.Rows.Count > 0)
                                {
                                    return "该机台存在未下机成型鼓，请勿重复上机！！！";
                                }
                                else
                                {
                                    sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING3,CDESC,INCH,SBAN,STATE,DIV,SNAME,STIME,ENAME,ETIME,CCNT3) VALUES (";
                                    sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','" + SBAN + "','1','1','" + NAME + "',SYSDATE,NULL,NULL,'0')";
                                    string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '在线' WHERE CODING = '" + CODING + "'";
                                    //db.ExecuteNonQuery(sUpMde);
                                    OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                                    
                                    if (iResult > 0)
                                    {
                                        return "OK";
                                    }
                                    else
                                    {
                                        return "Err-NONE" + sInsScan;
                                    }
                                }
                            }
                            else
                            {
                                return "该成型鼓未进行胶囊上机操作，不允许进行鼓上机！！！";
                            }


                        }
                        else
                        {


                            ////////////////////////////////////提示非配套鼓，然后继续往下走
                            string xgg1 = @"select *
                                          from (select *
                                                  from mde0004
                                                 where coding = '" + CODING + @"'
                                                   and div is not null
                                                   and etime is null
                                                 order by stime desc)
                                         where rownum = 1";
                            DataTable dtgg1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, xgg1, null).Tables[0];
                            if (dtgg1 != null && dtgg1.Rows.Count > 0)
                            {

                                string sql2 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "' AND LR='主鼓右' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                                DataTable dtM2 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql2, null).Tables[0];
                                if (dtM2 != null && dtM2.Rows.Count > 0)
                                {
                                    return "该机台存在未下机成型鼓，请勿重复上机！！！";
                                }
                                else
                                {
                                    sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING3,CDESC,INCH,SBAN,STATE,DIV,SNAME,STIME,ENAME,ETIME,CCNT3) VALUES (";
                                    sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','" + SBAN + "','1','1','" + NAME + "',SYSDATE,NULL,NULL,'0')";
                                    string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '在线' WHERE CODING = '" + CODING + "'";
                                    //db.ExecuteNonQuery(sUpMde);
                                    OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                                   
                                    if (iResult > 0)
                                    {
                                        return "OK";
                                    }
                                    else
                                    {
                                        return "Err-NONE" + sInsScan;
                                    }
                                }
                            }
                            else
                            {
                                return "该成型鼓未进行胶囊上机操作，不允许进行鼓上机！！！";
                            }


                        }
                    }
                    else
                    {
                        string xgg1 = @"select *
                                      from (select *
                                              from mde0004
                                             where coding = '" + CODING + @"'
                                               and div is not null
                                               and etime is null
                                             order by stime desc)
                                     where rownum = 1";
                        DataTable dtgg1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, xgg1, null).Tables[0];
                        if (dtgg1 != null && dtgg1.Rows.Count > 0)
                        {

                            string sql2 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "' AND LR='主鼓右' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                            DataTable dtM2 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql2, null).Tables[0];
                            if (dtM2 != null && dtM2.Rows.Count > 0)
                            {
                                return "该机台存在未下机成型鼓，请勿重复上机！！！";
                            }
                            else
                            {
                                sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING3,CDESC,INCH,SBAN,STATE,DIV,SNAME,STIME,ENAME,ETIME,CCNT3) VALUES (";
                                sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','" + SBAN + "','1','1','" + NAME + "',SYSDATE,NULL,NULL,'0')";
                                string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '在线' WHERE CODING = '" + CODING + "'";
                                //db.ExecuteNonQuery(sUpMde);
                                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                                if (iResult > 0)
                                {
                                    return "OK";
                                }
                                else
                                {
                                    return "Err-NONE" + sInsScan;
                                }
                            }
                        }
                        else
                        {
                            return "该成型鼓未进行胶囊上机操作，不允许进行鼓上机！！！";
                        }
                    }
                }
                else if (LR == "副鼓")
                {
                    string sql3 = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "' AND LR='副鼓' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                    DataTable dtM3 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql3, null).Tables[0];
                    if (dtM3 != null && dtM3.Rows.Count > 0)
                    {
                        return "该机台存在未下机成型鼓，请勿重复上机！！！";
                    }
                    else
                    {
                        sInsScan = "INSERT INTO MDE0003 (ID,FAC,MCHID,LR,CODING2,CDESC,INCH,SBAN,STATE,DIV,SNAME,STIME,ENAME,ETIME,CCNT2) VALUES (";
                        sInsScan += "sys_guid(),'07','" + MCHID + "','" + LR + "','" + CODING + "','" + CDESC + "','" + INCH + "','" + SBAN + "','1','2','" + NAME + "',SYSDATE,NULL,NULL,'0')";
                        string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '在线' WHERE CODING = '" + CODING + "'";
                        //db.ExecuteNonQuery(sUpMde);
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                        if (iResult > 0)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Err-NONE" + sInsScan;
                        }
                    }
                }

                return "xuguangge";

            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "1" + sInsScan;
            }


        }


        #endregion

        #region 成型鼓下机
        /// <summary>
        /// 成型鼓下机
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/CODINGDOWN")]
        public string CODINGDOWN(string MCHID, string LR, string CODING, string INCH, string XBAN, string NAME)
        {
            

            try
            {


                if (LR == "主鼓左")
                {
                    string sql = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "'  AND CODING1='" + CODING + "' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                    DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                    if (dtM != null && dtM.Rows.Count > 0)
                    {
                        string sInsScan = "UPDATE MDE0003 SET ETIME=SYSDATE ,XBAN='" + XBAN + "',ENAME='" + NAME + "',STATE='2'WHERE  ID='" + dtM.Rows[0]["ID"].ToString() + "'";
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                        string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '不在线' WHERE CODING = '" + CODING + "'";
                        //db.ExecuteNonQuery(sUpMde);
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                        if (iResult > 0)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Err-NONE" + sInsScan;
                        }
                    }
                    else
                    {
                        return "未查询到当前机台有上机记录";
                    }
                }
                else if (LR == "主鼓右") //主鼓又不分左右了.注释掉  ........//又分了mmp2021.08.03
                {
                    string sql = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "'  AND CODING3='" + CODING + "' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                    DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                    if (dtM != null && dtM.Rows.Count > 0)
                    {
                        string sInsScan = "UPDATE MDE0003 SET ETIME=SYSDATE ,XBAN='" + XBAN + "',ENAME='" + NAME + "',STATE='2'WHERE  ID='" + dtM.Rows[0]["ID"].ToString() + "'";
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                        string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '不在线' WHERE CODING = '" + CODING + "'";
                        //db.ExecuteNonQuery(sUpMde);
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                        if (iResult > 0)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Err-NONE" + sInsScan;
                        }
                    }
                    else
                    {
                        return "未查询到当前机台有上机记录";
                    }
                }
                else if (LR == "副鼓")
                {
                    string sql = "  SELECT * FROM (SELECT * FROM MDE0003 WHERE  MCHID='" + MCHID + "'   AND CODING2='" + CODING + "' AND ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                    DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                    if (dtM != null && dtM.Rows.Count > 0)
                    {
                        string sInsScan = "UPDATE MDE0003 SET ETIME=SYSDATE ,XBAN='" + XBAN + "',ENAME='" + NAME + "',STATE='2'WHERE  ID='" + dtM.Rows[0]["ID"].ToString() + "'";
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                        string sUpMde = "UPDATE MDE0001 SET ONLINEYN = '不在线' WHERE CODING = '" + CODING + "'";
                        //db.ExecuteNonQuery(sUpMde);
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sUpMde, null);
                        if (iResult > 0)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Err-NONE" + sInsScan;
                        }
                    }
                    else
                    {
                        return "未查询到当前机台有上机记录";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
            return "1";
        }

        #endregion


        /// <summary>
        /// 查询当前机台，上个uid是否绑定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/selUid")]
        public async Task<DataTable> selUid(string MCHID)
        {
            return await Task.Run(() =>
            {

                DataRow row = SHIFT.GetShift(DateTime.Now);
                string sql = @"select * from LTA0006 where state = '0' and BUMCH = '" + MCHID +"'";
                DataTable uses = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
              
                    //List<Plan> users = DataTableToList<Plan>.ConvertToModel(uses);
                    return uses;
                
            });
        }


        [HttpGet]
        [Route("api/InsertLta0006")]
        public string InsertLta0006(string BUMCH, string PLANCOD, string RFIDTAG, string BARCODE, string BUNAM,string STATE)
        {
            try
            {
               
                DataRow row = SHIFT.GetShift(DateTime.Now);
                if (STATE.Equals("1"))
                {
                    string del = "delete from lta0006 where state = '0' and BUMCH = '" + BUMCH + "'";
                    OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, del, null);
                }
                string sel = "select * from LTA0006 where RFIDTAG='"+ RFIDTAG +"'";
                DataTable uses = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sel, null).Tables[0];
                if (uses != null && uses.Rows.Count > 0)
                {
                    return "Err";
                }
                string ins = "insert into LTA0006 (ID,FAC,BUMCH,PLANCOD,RFIDTAG,BARCODE,BUNAM,BUTIM,BUSHT,STATE) values( ";
                ins += "sys_guid(),'07','" + BUMCH + "','" + PLANCOD + "','" + RFIDTAG + "','" + BARCODE + "','" + BUNAM + "',SYSDATE,'" + row["WSHT"].ToString() + "','" + STATE + "')";
                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, ins, null);
                if (iResult > 0)
                {//
                    return "OK";
                }
                else
                {
                    return "Err-NONE" + ins;
                }


            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return ex.ToString();
            }
        }

        #region 成型更换工装确认

        /// <summary>
        /// 成型更换工装确认
        /// </summary>
        /// <returns></returns>
        //这个方法用来计算上机时间
        [HttpGet]
        [Route("api/Gzqr")]
        public string Gzqr(string MCHID, string NAME)
        {
            
            string sInsScan = "";
            try
            {
                string SELSQL = @"SELECT * FROM  MDE0008 WHERE MCHID ='" + MCHID + "' AND ENDT IS NULL ";
                DataTable dtSELSQL = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, SELSQL, null).Tables[0];
                if (dtSELSQL != null && dtSELSQL.Rows.Count > 0)
                {
                    return "当前机台存在未确认工装记录，请在MES上端确认后进行工装更换确认！";
                    //功能优化 增加校验，防止机台存在未确认工装更换记录，存在重复数据！2022.3.29Xugg
                }
                else
                {
                    //查规格交替履历 找出上个规格，当前规格及工单号
                    string cjh = "SELECT * FROM (select AITNBR,AITDSC,BITNBR,BITDSC,PLANCODNEW from  pad0002  WHERE  MCHID='" + MCHID + "' order by  wtime  desc ) WHERE ROWNUM=1 ";
                DataTable dtCXJH = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, cjh, null).Tables[0];
                    if (dtCXJH != null && dtCXJH.Rows.Count > 0)
                    {//将数据插入mde0008 
                        string ins008 = "insert into mde0008 (ID,FAC,MCHID,ITDSCA,ITNBRA,ITDSCB,ITNBRB,STARTT,PLANCODNEW,NAME) values( ";
                        ins008 += "sys_guid(),'07','" + MCHID + "','" + dtCXJH.Rows[0]["AITDSC"].ToString() + "','" + dtCXJH.Rows[0]["AITNBR"].ToString() + "','" + dtCXJH.Rows[0]["BITDSC"].ToString() + "','" + dtCXJH.Rows[0]["BITNBR"].ToString() + "',SYSDATE,'" + dtCXJH.Rows[0]["PLANCODNEW"].ToString() + "','" + NAME + "')";
                        //  LogBLL.error("xugggggggg " + ins008);
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, ins008, null);
                        if (iResult > 0)
                        {
                            return "OK" + "_" + dtCXJH.Rows[0]["AITDSC"].ToString() + "_" + dtCXJH.Rows[0]["BITDSC"].ToString();
                        }
                        else
                        {
                            return "Err-NONE" + ins008;
                        }
                    }
                    else
                    {
                        return "未查询到上个交替规格！";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "1" + sInsScan;
            }
            
        }
        /// <summary>
        /// 这个方法用来计算下机时间
        /// </summary>
        /// <param name="MCHID"></param>
        /// <param name="NAME"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GXSJ")]
        public string GXSJ(string MCHID, string NAME, string CBAN, string NAME1, string NAME2, string NAME3)
        {
           
            string sInsScan = "";
            try
            {  //查当前机台最新上机记录
                string csj = "  select * from (select * from  mde0008 where mchid='" + MCHID + "'  and endt  is null order by  startt desc) where rownum=1";//查上机
                DataTable dtCXSJ = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, csj, null).Tables[0];
                if (dtCXSJ != null && dtCXSJ.Rows.Count > 0)
                {//更新数据表mde0008
                 //考虑到现场WiFi强度不一致，有时候工装班来不及选择其他换工装的人，所以在确认按钮上增加更新换工装的人，毕竟可能涉及到他们工资 22.3.07
                    string upd008 = "update mde0008 set name='" + NAME + "',CBAN='" + CBAN + "',NAME1='" + NAME1 + "',NAME2='" + NAME2 + "',NAME3='" + NAME3 + "' where  id='" + dtCXSJ.Rows[0]["ID"].ToString() + "'";
                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, upd008, null);
                    if (iResult > 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "Err-NONE" + upd008;
                    }
                }
                else
                {
                    return "未查询到当前机台有开始更换工装记录";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "1" + sInsScan;
            }

        }

        #endregion

        /// <summary>
        /// 查询工装班在职人员
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetpeopleGZB")]
        public DataTable GetpeopleGZB()
        {
            
            try
            {
                string sql = @"SELECT LOGINNAME,NAME
                                      FROM LSFW_EMPLOYEE
                                     WHERE POSNAM = '工装更换'
                                       AND LEAYN = 'N'
                                     ORDER BY LOGINNAME ASC";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        #region 成型鼓胶囊交替原因
        /// <summary>
        /// 成型鼓胶囊交替原因
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetJNCCODE")]
        public DataTable GetJNCCODE()
        {
           
            try
            {
                DataTable dtCXSJ = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, "SELECT CCODE,CDESC FROM MDE0006 WHERE USEYN='Y'", null).Tables[0];

                return dtCXSJ;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        #endregion

        #region 成型胶囊上机
        /// <summary>
        /// 成型胶囊上机
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/CHANGECS")]
        public string CHANGECS(string MCHID, string CODING, string LR, string CSINCH, string DIV, string SBAN, string NAME)
        {
            
            string sInsScan = "";
            try
            {
                //if (DIV == "胶囊换新")
                //   {
                if (LR == "主鼓左")
                {

                    if (CODING.EndsWith("R"))
                    {
                        return "所上胶囊与鼓选择不一致，不允许上胶囊！！！";
                    }

                    string sql1 = "SELECT * FROM ( SELECT * FROM MDE0004 WHERE LR='主鼓左'  AND MCHID='" + MCHID + "' and coding='" + CODING + "' AND ETIME IS NULL order BY STIME DESC )WHERE  ROWNUM=1";
                    DataTable dtM1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql1, null).Tables[0];
                    if (dtM1 != null && dtM1.Rows.Count > 0)
                    {
                        return "当前机台存在未下机胶囊！！！";
                    }
                    else
                    {
                        sInsScan = "INSERT INTO MDE0004 (ID,FAC,MCHID,CODING,LR,CSINCH,DIV,SBAN,SNAME,STIME,ENAME,ETIME,CCNT) VALUES (";
                        sInsScan += "sys_guid(),'07','" + MCHID + "','" + CODING + "','" + LR + "','" + CSINCH + "','" + DIV + "','" + SBAN + "','" + NAME + "',SYSDATE,NULL,NULL,'1')";
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                        if (iResult > 0)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Err-NONE" + sInsScan;
                        }
                    }
                }

                else if (LR == "主鼓右")
                {

                    if (CODING.EndsWith("L"))
                    {
                        return "所上胶囊与鼓选择不一致，不允许上胶囊！！！";
                    }

                    string sql1 = "SELECT * FROM ( SELECT * FROM MDE0004 WHERE LR='主鼓右'  AND  MCHID='" + MCHID + "'  and coding='" + CODING + "' AND ETIME IS NULL order BY STIME DESC )WHERE  ROWNUM=1";
                    DataTable dtM1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql1, null).Tables[0];
                    if (dtM1 != null && dtM1.Rows.Count > 0)
                    {
                        return "当前机台存在未下机胶囊！！！";
                    }
                    else
                    {
                        sInsScan = "INSERT INTO MDE0004 (ID,FAC,MCHID,CODING,LR,CSINCH,DIV,SBAN,SNAME,STIME,ENAME,ETIME,CCNT) VALUES (";
                        sInsScan += "sys_guid(),'07','" + MCHID + "','" + CODING + "','" + LR + "','" + CSINCH + "','" + DIV + "','" + SBAN + "','" + NAME + "',SYSDATE,NULL,NULL,'1')";
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                        if (iResult > 0)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Err-NONE" + sInsScan;
                        }
                    }
                }
                //else
                //{
                //    string sql1 = "SELECT * FROM ( SELECT * FROM MDE0004 WHERE LR='副鼓'  AND MCHID='" + MCHID + "' AND ETIME IS NULL order BY STIME DESC )WHERE  ROWNUM=1";
                //    DataTable dtM1 = db.GetDataTable(sql1);
                //    if (dtM1 != null && dtM1.Rows.Count > 0)
                //    {
                //        return "当前机台存在未下机胶囊！！！";
                //    }
                //    else
                //    {
                //        sInsScan = "INSERT INTO MDE0004 (ID,FAC,MCHID,CODING,LR,CSINCH,DIV,SNAME,STIME,ENAME,ETIME) VALUES (";
                //        sInsScan += "sys_guid(),'07','" + MCHID + "','" + CODING + "','" + LR + "','" + CSINCH + "','" + DIV + "','" + NAME + "',SYSDATE,NULL,NULL)";
                //        int iResult = db.ExecuteNonQuery(sInsScan);
                //        if (iResult > 0)
                //        {
                //            return "OK";
                //        }
                //        else
                //        {
                //            return "Err-NONE" + sInsScan;
                //        }
                //    }
                //}


                //  }
                //else ///下机 要分开做
                //{
                //    string sql = "SELECT * FROM (SELECT * FROM MDE0004 WHERE CODING='" + CODING + "' and ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                //    DataTable dtM = db.GetDataTable(sql);
                //    if (dtM != null && dtM.Rows.Count > 0)
                //    {
                //        string sInsScan1 = "UPDATE MDE0004 SET ETIME=SYSDATE,ENAME='" + NAME + "',div='胶囊报废'WHERE  ID='" + dtM.Rows[0]["ID"].ToString() + "'";
                //        int iResult = db.ExecuteNonQuery(sInsScan1);
                //        if (iResult > 0)
                //        {
                //            return "OK";
                //        }
                //        else
                //        {
                //            return "Err-NONE" + sInsScan1;
                //        }
                //    }
                //} 
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "1" + sInsScan;
            }

            return "XGGG";
        }
        #endregion

        #region 成型胶囊下机
        /// <summary>
        /// 成型胶囊下机
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/CHANGECSDOWN")]
        public string CHANGECSDOWN(string MCHID, string CODING, string LR, string CSINCH, string XBAN, string NAME)
        {
            
            string sInsScan = "";
            try
            {
                string sql = "SELECT * FROM (SELECT * FROM MDE0004 WHERE CODING='" + CODING + "' and ename IS NULL  and etime IS NULL  ORDER BY STIME DESC) WHERE  ROWNUM=1";
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM != null && dtM.Rows.Count > 0)
                {
                    string sInsScan1 = "UPDATE MDE0004 SET ETIME=SYSDATE,XBAN='" + XBAN + "',ENAME='" + NAME + "'WHERE  ID='" + dtM.Rows[0]["ID"].ToString() + "'";
                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan1, null);
                    if (iResult > 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "Err-NONE" + sInsScan1;
                    }
                }
                else
                {
                    return "当前成型鼓无待下机胶囊！";
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "1" + sInsScan;
            }

            
        }
        #endregion

        [HttpGet]
        [Route("api/GetBarcodeGTWgt")]
        public DataTable GetBarcodeGTWgt(string sBarcode)
        {
           
            try
            {
                string sSql = "SELECT A.BUITNBR,";
                sSql += "       A.BUITDSC,";
                sSql += "       B.ATTRVAL,";
                sSql += "       B.ATTRVAL * 1.025 UPP,";
                sSql += "       B.ATTRVAL * 0.975 LOW,";
                sSql += "       B.ATTRVAL * 1.010 UPP1,";
                sSql += "       B.ATTRVAL * 0.990 LOW1";
                sSql += "  FROM LTA0001 A,";
                sSql += "       (SELECT ITNBR, ATTRVAL";
                sSql += "          FROM EDB0010 A, EDB0015 B";
                sSql += "         WHERE     A.ID = B.ITEMID";
                sSql += "               AND B.ATTRCOD = 'M18'";
                sSql += "               AND A.USEYN = 'Y'";
                sSql += "               AND A.ITTYPECOD = 'GT') B";
                sSql += " WHERE A.BUITNBR = B.ITNBR(+) AND A.BARCODE = '" + sBarcode + "'";

                DataTable dtM1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];

                return dtM1;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sBarcode"></param>
        /// <param name="sITNBR"></param>
        /// <param name="sITDSC"></param>
        /// <param name="sStdWT"></param>
        /// <param name="sWT"></param>
        /// <param name="sUPP"></param>
        /// <param name="sLOW"></param>
        /// <param name="sUPP1"></param>
        /// <param name="sLOW1"></param>
        /// <param name="sState">重量不良区分 1,4,5</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/BarcodeGTBuLu")]
        public string BarcodeGTBuLu(string sBarcode, string sITNBR, string sITDSC, string sStdWT, string sWT, string sUPP, string sLOW, string sUPP1, string sLOW1, string sState, string sNam)
        {
            try
            {
                ArrayList sqllist = new ArrayList();
                
              
                DataRow row = SHIFT.GetShift( DateTime.Now);
                string sNG = "N";
                string sSql = "SELECT * FROM STG0003 WHERE BARCODE = '" + sBarcode + "'";
                DataTable dtSql = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dtSql != null && dtSql.Rows.Count > 0)
                    return "1";//已经有称重信息
                               //
                string sINSsql = "insert into STG0003 (ID,FAC,BARCODE,ITNBR,ITDSC,STATE,WGT,UPLIMIT,LWLIMIT,WTDAT,WTLOC,ENT_USER_ID,ENT_DT,UPD_USER_ID,UPD_DT,UPLIMIT_Q,LWLIMIT_Q,STDWGT,STODIV,BDIV) values(";
                sINSsql += "'" + System.Guid.NewGuid().ToString() + "',";
                sINSsql += "'" + FAC + "',";
                sINSsql += "'" + sBarcode + "',";
                sINSsql += "'" + sITNBR + "',";
                sINSsql += "'" + sITDSC + "',";//orcReader["ITDSC"].ToString()
                sINSsql += "'" + sState + "',";
                sINSsql += "'" + sWT + "',";
                sINSsql += "'" + sUPP + "',";
                sINSsql += "'" + sLOW + "',";
                sINSsql += "SYSDATE,";
                sINSsql += "'',";
                sINSsql += "'" + sNam + "',";
                sINSsql += "SYSDATE,";
                sINSsql += "'" + sNam + "',";
                sINSsql += "SYSDATE,";
                sINSsql += "'" + sUPP1 + "',";
                sINSsql += "'" + sLOW1 + "',";
                sINSsql += "'" + sStdWT + "',";
                sINSsql += "'','B'";
                sINSsql += ")";
                sqllist.Add(sINSsql);
                //
                if (sState != "1")//若不良
                {
                    string sINSsql2 = "insert into QMB0101 (ID,FAC,DIV,AYN,BARCODE,IDAT,IBAN,ISHT,INAM,ITIM,COD,PYN,BUITNBR,BUITDSC,STATE,BUMCH,BUDAT,BUTIM,BUSHT,BUBAN,BUNAM,STWT,REWT) values(";
                    sINSsql2 += "'" + System.Guid.NewGuid().ToString() + "',";
                    sINSsql2 += "'" + FAC + "',";
                    sINSsql2 += "'2',";//2->重量检查
                    sINSsql2 += "'B',";//检查不合格，默认插入B->修理(不良)
                    sINSsql2 += "'" + sBarcode + "',";
                    sINSsql2 += "to_date('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
                    sINSsql2 += "'" + row["WBAN"].ToString() + "',";
                    sINSsql2 += "'" + row["WSHT"].ToString() + "',";
                    sINSsql2 += "'" + sNam + "',"; //判定人INAM
                    sINSsql2 += "to_date('" + DateTime.Now.ToString() + "','yyyy-MM-dd HH24:mi:ss'),";
                    sINSsql2 += "'GT-21',";//GT-21：称重不良
                    sINSsql2 += "'N',";
                    sINSsql2 += "'" + sITNBR + "',";
                    sINSsql2 += "'" + sITDSC + "',";
                    sINSsql2 += "'',";
                    sINSsql2 += "'',";
                    sINSsql2 += "null,";
                    sINSsql2 += "null,";
                    sINSsql2 += "'',";
                    sINSsql2 += "'',";
                    sINSsql2 += "'',";
                    sINSsql2 += "'" + sStdWT + "',";//标准重量
                    sINSsql2 += "'" + sWT + "'";//实测重量
                    sINSsql2 += ")";
                   
                    sqllist.Add(sINSsql2);
                   

                    sNG = "G";
                }

                //发送给韩华胎胚状态
                string sInsSql1 = "insert into IF_WMS_GT_05 (BARCODE,ITNBR,ITDSC,WDATE,WTIM,WSHT,WBAN,USERID,NOOUTRSN,ITMSTATUS,JUDGUSERID,JUDGDT,RCV_FLAG,RCV_DT,ENT_USER_ID,ENT_DT,UPD_USER_ID,UPD_DT) values(";
                sInsSql1 += "'" + sBarcode + "',";
                sInsSql1 += "'" + sITNBR + "',";
                sInsSql1 += "'" + sITDSC + "',";
                sInsSql1 += "to_date('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
                sInsSql1 += "sysdate,";
                sInsSql1 += "'',";
                sInsSql1 += "'',";
                sInsSql1 += "'',";
                sInsSql1 += "'',";
                sInsSql1 += "'" + sNG + "',";
                sInsSql1 += "'',";
                sInsSql1 += "sysdate,";
                sInsSql1 += "'N',";
                sInsSql1 += "'',";
                sInsSql1 += "'MES',";
                sInsSql1 += "sysdate,";
                sInsSql1 += "'MES',";
                sInsSql1 += "sysdate";
                sInsSql1 += ")";
                //正式打开
                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString2, CommandType.Text, sInsSql1, null);
                //ifdb.ExecuteNonQuery(sInsSql1);

                string sUPDsql2 = "UPDATE LTA0001 SET WYYN='Y',SEWT='" + sStdWT + "',REWT='" + sWT + "' WHERE BARCODE='" + sBarcode + "'";
                sqllist.Add(sUPDsql2);
                int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sqllist);
                if (iRes > 0)
                    return "OK";
                else
                    return "NG";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }

        #region 裁断扫描投料
        /// <summary>
        /// 裁断扫描投料 斜裁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/sbPutIn")]
        public string sbPutIn(string MCHID, string CODING, string CBAN, string FPDIV, string CNAME)
        {//MCHID.Text.Trim(), CODING.Text.Trim(),CBAN.Text.Trim(),FPDIV.Text.Trim(), comn.sLoginName
            
            string sInsScan = "";
            try
            {
                sInsScan = " INSERT INTO  MDE0014(ID,FAC,MCHID,CBAN,CODING,FPDIV,CDIV,CNAME,CTIM) VALUES( ";
                sInsScan += " sys_guid(),'07','" + MCHID + "','" + CBAN + "','" + CODING + "','" + FPDIV + "','1','" + CNAME + "',SYSDATE )";
                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                if (iResult > 0)
                {
                    return "OK";
                }
                else
                {
                    return "Err-NONE" + sInsScan;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "1" + sInsScan;
            }

            return "XUGG";
        }
        #endregion

        [HttpGet]
        [Route("api/GetWIP0001")]
        public DataTable GetWIP0001(string lotno)
        {
            
            try
            {
                if (string.IsNullOrEmpty(lotno)) return null;
                string Sql_div = @"SELECT C.*, A.BCOD, B.BNAM
                                          FROM WIP0001 C, QMC0101 A, QMC0001 B
                                         WHERE     C.LOTID = A.LOTID(+)
                                               AND A.BCOD = B.BCOD(+)
                                               AND  C.TOOLNO='" + lotno + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, Sql_div, null).Tables[0];
                if (dt.Rows.Count > 0)
                    return dt;
                else
                    return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpPost]
        [Route("api/GetLTC002")]
        public DataTable GetLTC002(string[] parm)
        {
            //0 LOT号
            //1 区分


            
            try
            {
                string sql = "SELECT * from qmc0102 A WHERE A.BCOD = 'NM-EXP'  AND LOTID = '" + parm[0] + "' AND DIV = '" + parm[1] + "'";
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM != null && dtM.Rows.Count > 0)
                {
                    return dtM;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpPost]
        [Route("api/NEWGetLTC001")]
        public DataTable NEWGetLTC001(string[] parm)
        {
            //0 LOT号
            //1 区分
            //2卷轴号
           
            try
            {
                string sql = "SELECT * FROM WIP0001  WHERE  TOOLNO= '" + parm[2] + "' ";
                LogHelper.Debug("Xugggggg:" + sql);
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM != null && dtM.Rows.Count > 0)
                {
                    return dtM;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        /// <summary>
        /// 查询绑定RFID成品胎的基本信息
        /// </summary>
        /// <param name="rfid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetGTBasic")]
        public DataTable GetGTBasic(string rfid)
        {
            //RFID信息
            try
            {
                string strSql = @"SELECT
	* 
FROM
	(
		(
		SELECT
			* 
		FROM
			(
				(
				SELECT
					* 
				FROM
					(
						( SELECT RFIDTAG, BARCODE FROM LTA0006 WHERE RFIDTAG = '" + rfid + @"' ) A
						LEFT JOIN (
						SELECT
							BARCODE BARCODE1,
							CUITNBR,
							CUITDSC,
							CUMCH,
							CUBAN,
							CUTIM,
							CUNAM CUNAME1,
							BUITNBR,
							BUITDSC,
							BUTIM,
							BUMCH,
							BUNAM 
						FROM
							LTA0001 
						) B ON A.BARCODE = B.BARCODE1 
					) 
				) C
				LEFT JOIN (
				SELECT
					X.ITNBR,
					X.RULEVAL1 
				FROM
					CKA0015 X
					LEFT JOIN EDB0010 Y ON X.ITNBR = Y.ITNBR 
				WHERE
					Y.USEYN = 'Y' 
				ORDER BY
					X.ITNBR 
				) D ON C.CUITNBR = D.ITNBR 
			) 
		) E
	LEFT JOIN ( SELECT LOGINNAME, NAME CUNAM FROM LSFW_EMPLOYEE ) F ON E.CUNAME1 = F.LOGINNAME 
	)";

                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                //if (GTBasic != null && GTBasic.Rows.Count > 0)
                //{
                //    return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                //}
                //else
                //{
                //    return null;//PDA提示  未能通过当前RFID查询到该成品胎的基本信息！
                //}

            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }




        //这个只是加了个div=1的判断

        /// <summary>
        /// 查询当前机台，当前班次在干的计划
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/selPlan")]
        public async Task<List<Plan>> selPlan(string MCHID)
        {
            return await Task.Run(() =>
            {

                DataRow row = SHIFT.GetShift(DateTime.Now);
                string sql = @"select key,value from (select PLANCOD as Key,ITDSC as Value,min(div) div from PAD0307 t 
                    where MCHID = '" + MCHID + "' and wdate = to_date('" + Convert.ToDateTime(row["WDATE"]).ToShortDateString() + "','YYYY-MM-DD') " +
                    "and wsht = '" + row["WSHT"].ToString() + "'  and div='1' group by PLANCOD,ITDSC) order by div";
                DataTable uses = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (uses != null && uses.Rows.Count > 0)
                {
                    List<Plan> users = DataTableToList<Plan>.ConvertToModel(uses);
                    return users;
                }
                else return null;
            });
        }

    }
}
