using System;
using System.Web.Http;
using System.Data;
using log4net.Util;
using HYPDAWebApi.DBHelper;
using System.Collections;
using HYPDAWebApi.Models.ViewModel;
using System.Globalization;
using HYPDAWebApi.App_Data;
using System.Collections.Generic;

namespace HYPDAWebApi.Controllers
{
    /// <summary>
    /// 模具
    /// </summary>
    public class MouldController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";


        [HttpGet]
        [Route("api/ChangeMod")]
        public string ChangeMod(int type, string sMCHID, string sLR, string sLogId, string sCln_Loc, string sCln_Rsn)
        {
             
            try
            {
                string sqlxk = "select   *   from  MDD0002  WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'  ";

                DataTable dtxk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlxk, null).Tables[0];

                if (dtxk.Rows[0]["ITNBR"].ToString() != "" && type == 1)
                {
                    return "XK";
                }
                else if (dtxk.Rows[0]["ITNBR"].ToString() != "" && type == 2)
                {
                    return "XK";
                }
                else if (dtxk.Rows[0]["ITNBR"].ToString() != "" && type == 4)
                {
                    return "XK";
                }

                DataRow row = SHIFT.GetShift(DateTime.Now);
                //获取机台模具现况信息 以及计划信息（排除状态为 5-已完成 6-已取消的）
                string sSql = @"SELECT A.CAVID,A.CHAID,A.GQID,A.ITNBR,A.ITDSC,A.USECOUNT,A.USECOUNT_1,A.DRYICE,B.*
                                              FROM (SELECT * FROM MDD0002 WHERE MCHID='" +
                                                 sMCHID + "' AND LRFLAG='" + sLR + @"') A,
                                              (SELECT C.*,D.ITDSC ITDSCD FROM MDD0003 C,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) D 
                                              WHERE C.ITNBRB=D.ITNBR(+) AND 
                                               C.PLANSTATUS = '2') B 
                                              WHERE A.MCHID=B.MCHID(+) AND A.LRFLAG=B.LRFLAG(+)";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                string sql = @"SELECT COUNT(1) FROM LTA0001 WHERE CUMCH = '" +
                                  sMCHID + "' AND LR = '" + sLR + @"' AND CUTIM BETWEEN 
                                TO_DATE(TO_CHAR((SELECT STATIME FROM (SELECT ROWNUM RN,STATIME FROM MDD0001 
                                WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + @"' AND ENDTIME IS NULL) X 
                                WHERE X.RN=1),'YYYY-MM-DD HH24:MI:SS'), 'YYYY-MM-DD HH24:MI:SS') AND SYSDATE";
                DataTable dcnt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
               
                string cnt = dcnt.Rows[0][0].ToString();
                ArrayList sqllist = new ArrayList();
                ArrayList sqllist1 = new ArrayList();
                if (type == 1)//模具交替
                {
                    if (dt.Rows[0]["PLANSTATUS"].ToString() != "2" || dt.Rows[0]["CLNDIV"].ToString() != "1")//无待上机的模具交替计划
                        return "FH";
                    //变更该机台的模具安装现况信息：型腔号、壳体号、钢圈号、生产规格代码、生产规格名称、使用次数
                    sqllist.Add(@"UPDATE MDD0002 SET CAVID= '" + dt.Rows[0]["MODID"] +
                                  "', CHAID='" + dt.Rows[0]["TAOID"] + "', GQID='" + dt.Rows[0]["GQCOD"] +
                                  "',ITNBR = '" + dt.Rows[0]["ITNBRB"] + "',ITDSC = '" + dt.Rows[0]["ITDSCD"] +
                                  "',USECOUNT=0,ENAM='" + sLogId + "',ETIM=SYSDATE WHERE MCHID = '" + sMCHID +
                                  "' AND LRFLAG = '" + sLR + "'");
                    ////录入该机台上一次使用模具的下机信息：下机时间、下机原因ECODE=1、使用次数(根据模具上下机时间统计硫化条码个数)
                    //sqllist.Add(@"UPDATE MDD0001 SET ENDTIME = SYSDATE, CNT = '" + cnt +
                    //            "', ECODE = '1' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR +
                    //            "' AND ENDTIME IS NULL");
                    //录入新上机模具信息：ID、FAC、日期、班次、班组、机台、左右
                    //新模具型腔号、新模具壳体号、新模具钢圈号、新硫化规格代码、新硫化规格名称
                    //上机时间、上机人、计划号

                    sqllist.Add(@"INSERT INTO MDD0001(ID, fac, wdate, wsht, wban, mchid, lr, 
                              modid, taoid, gqcod, itnbr, itdsc, statime, enam, etim, planid,cln_loc,cln_rsn)
                              VALUES(SYS_GUID(), '07', TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                                  "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" + row["WBAN"].ToString() + "', '" +
                                  sMCHID + "', '" + sLR + "', '" + dt.Rows[0]["MODID"] + "', '" + dt.Rows[0]["TAOID"] + "', '" +
                                  dt.Rows[0]["GQCOD"] + "', '" + dt.Rows[0]["ITNBRB"] + "', '" + dt.Rows[0]["ITDSCD"] +
                                    "', SYSDATE, '" + sLogId + "', SYSDATE, '" + dt.Rows[0]["PLANID"] + "','" + sCln_Loc + "','" + sCln_Rsn + "')");
                    //将该计划状态变更为已上机
                    sqllist.Add("UPDATE MDD0003 SET PLANSTATUS = '3',STIM=SYSDATE,SNAM='" + sLogId + "' WHERE PLANID = '" + dt.Rows[0]["PLANID"] + "'");

                }
                else if (type == 2)//拆卸清洗
                {
                    if (dt.Rows[0]["CLNDIV"].ToString() == "2" && (dt.Rows[0]["PLANSTATUS"].ToString() == "3" || dt.Rows[0]["PLANSTATUS"].ToString() == "4"))//有已上机或首罐待检的拆卸清洗计划
                        return "FH";
                    if (dt.Rows[0]["CLNDIV"].ToString() == "2" && dt.Rows[0]["PLANSTATUS"].ToString() != "2")//不是待上机的计划不允许上机
                        return "DSJ2";
                    if (dt.Rows[0]["CLNDIV"].ToString() != "2")//无拆卸清洗计划
                    {
                        //                    //生成一个模具拆卸清洗的计划号
                        //                    dt.Rows[0]["PLANID"] = DateTime.Now.ToString("yyyyMMdd") + sMCHID.ToString().Substring(3, 3) + sLR + NextValue("MODPLANNO");
                        //                    //自动生成一条模具拆卸清洗计划 状态 3-已上机，因为后面需要检首罐
                        //                    DataTable dtplan = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@" SELECT *FROM MDD0003 WHERE planid='" + dt.Rows[0]["PLANID"] + "'");
                        //                    if (dtplan == null || dtplan.Rows.Count == 0)
                        //                    {
                        //                        sqllist.Add(@"INSERT INTO MDD0003
                        //                              (ID, fac, wdate, wsht, wban, mchid, lrflag, statime, endtime, itnbra, itnbrb, 
                        //                               enam, etim, modid, taoid, planid, planstatus, gqcod, clndiv,itdsca,itdscb)
                        //                               VALUES
                        //                              (SYS_GUID(), '07', TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                        //               "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" + row["WBAN"].ToString() + "',  '" +
                        //               sMCHID + "', '" + sLR + "', SYSDATE-5/24, SYSDATE,  '" + dt.Rows[0]["ITNBRB"] + "',  '" +
                        //               dt.Rows[0]["ITNBRB"] + "',  '" + sLogId + "', SYSDATE, '" + dt.Rows[0]["CAVID"] + "',  '" +
                        //               dt.Rows[0]["CHAID"] + "', '" + dt.Rows[0]["PLANID"] + "','3', '" + dt.Rows[0]["GQID"] + "', '2','" + dt.Rows[0]["ITDSCD"] + "','" + dt.Rows[0]["ITDSCD"] + "')");
                        //                    }
                        return "CXQX2";

                    }


                    //变更该机台的模具安装现况信息：使用次数
                    sqllist.Add(@"UPDATE MDD0002 SET USECOUNT=0,ENAM='" + sLogId + "',ETIM=SYSDATE,ITNBR =  '" + dt.Rows[0]["ITNBRB"].ToString() + "',ITDSC = '" + dt.Rows[0]["ITDSCD"].ToString() +
                        "',CAVID= '" + dt.Rows[0]["MODID"].ToString() +
                                  "', CHAID='" + dt.Rows[0]["TAOID"].ToString() + "', GQID='" + dt.Rows[0]["GQCOD"].ToString() +
                                  "' WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'");
                    //录入该机台上一次使用模具的下机信息：下机时间、下机原因ECODE=1、使用次数(根据模具上下机时间统计硫化条码个数)
                    //sqllist.Add(@"UPDATE MDD0001 SET ENDTIME = SYSDATE, CNT = '" + cnt +
                    //    "', ECODE = '2' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR +
                    //    "' AND ENDTIME IS NULL");
                    //录入新上机模具信息：ID、FAC、日期、班次、班组、机台、左右
                    //新模具型腔号、新模具壳体号、新模具钢圈号、新硫化规格代码、新硫化规格名称
                    //上机时间、上机人、计划号
                    sqllist.Add(@"INSERT INTO MDD0001(ID, fac, wdate, wsht, wban, mchid, lr, 
                              modid, taoid, gqcod, itnbr, itdsc, statime, enam, etim, planid,CLN_LOC,CLN_RSN)
                              VALUES(SYS_GUID(), '07', TO_DATE('" +
                                  Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                                  "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" +
                                  row["WBAN"].ToString() + "', '" +
                                  sMCHID + "', '" + sLR + "', '" + dt.Rows[0]["CAVID"] + "', '" +
                                  dt.Rows[0]["CHAID"] + "', '" +
                                  dt.Rows[0]["GQID"] + "', '" + dt.Rows[0]["ITNBRB"] + "', '" + dt.Rows[0]["ITDSCD"] +
                                    "', SYSDATE, '" + sLogId + "', SYSDATE, '" + dt.Rows[0]["PLANID"] + "','" + sCln_Loc + "','" + sCln_Rsn + "')");
                    //将该计划状态变更为已上机
                    sqllist.Add("UPDATE MDD0003 SET PLANSTATUS = '3',STIM=SYSDATE,SNAM='" + sLogId +
                                "' WHERE PLANID = '" + dt.Rows[0]["PLANID"] + "'");
                }
                else if (type == 4)//拆卸修理
                {
                    if (dt.Rows[0]["CLNDIV"].ToString() == "4" && (dt.Rows[0]["PLANSTATUS"].ToString() == "3" || dt.Rows[0]["PLANSTATUS"].ToString() == "4"))//有已上机或首罐待检的拆卸修理计划
                        return "FH";
                    if (dt.Rows[0]["CLNDIV"].ToString() == "4" && dt.Rows[0]["PLANSTATUS"].ToString() != "2")//不是待上机的计划不允许上机
                        return "DSJ2";
                    if (dt.Rows[0]["CLNDIV"].ToString() != "4")//无拆卸修理计划
                    {
                        //2020.06.17 硫化表明拆卸修理计划必须手动下，不自动生成
                        return "NA";
                    }
                    //2020-06-16 JOE 硫化工段表明拆卸修理不重置使用次数，因为没有清洗
                    //sqllist.Add(@"UPDATE MDD0002 SET USECOUNT=0,ENAM='" + sLogId + "',ETIM=SYSDATE WHERE MCHID = '" + sMCHID +
                    //              "' AND LRFLAG = '" + sLR + "'");

                    sqllist.Add(@"UPDATE MDD0002 SET USECOUNT=0,ENAM='" + sLogId + "',ETIM=SYSDATE,ITNBR =  '" + dt.Rows[0]["ITNBRB"].ToString() + "',ITDSC = '" + dt.Rows[0]["ITDSCD"].ToString() +
                        "',CAVID= '" + dt.Rows[0]["MODID"].ToString() +
                                  "', CHAID='" + dt.Rows[0]["TAOID"].ToString() + "', GQID='" + dt.Rows[0]["GQCOD"].ToString() +
                                  "' WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'");

                    //录入该机台上一次使用模具的下机信息：下机时间、下机原因ECODE=6、使用次数(根据模具上下机时间统计硫化条码个数)
                    //sqllist.Add(@"UPDATE MDD0001 SET ENDTIME = SYSDATE, CNT = '" + cnt +
                    //    "', ECODE = '6' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR +
                    //    "' AND ENDTIME IS NULL");
                    //录入新上机模具信息：ID、FAC、日期、班次、班组、机台、左右
                    //新模具型腔号、新模具壳体号、新模具钢圈号、新硫化规格代码、新硫化规格名称
                    //上机时间、上机人、计划号
                    sqllist.Add(@"INSERT INTO MDD0001(ID, fac, wdate, wsht, wban, mchid, lr, 
                              modid, taoid, gqcod, itnbr, itdsc, statime, enam, etim, planid,CLN_LOC,CLN_RSN)
                              VALUES(SYS_GUID(), '07', TO_DATE('" +
                                  Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                                  "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" +
                                  row["WBAN"].ToString() + "', '" +
                                  sMCHID + "', '" + sLR + "', '" + dt.Rows[0]["CAVID"] + "', '" +
                                  dt.Rows[0]["CHAID"] + "', '" +
                                  dt.Rows[0]["GQID"] + "', '" + dt.Rows[0]["ITNBRB"] + "', '" + dt.Rows[0]["ITDSCD"] +
                                    "', SYSDATE, '" + sLogId + "', SYSDATE, '" + dt.Rows[0]["PLANID"] + "','" + sCln_Loc + "','" + sCln_Rsn + "')");
                    //将该计划状态变更为已上机
                    sqllist.Add("UPDATE MDD0003 SET PLANSTATUS = '3',STIM=SYSDATE,SNAM='" + sLogId +
                                "' WHERE PLANID = '" + dt.Rows[0]["PLANID"] + "'");
                }
                else//若是干冰清洗
                {
                    if (dt.Rows[0]["CLNDIV"].ToString() != "3")//无干冰清洗计划
                    {
                        //生成一个模具干冰清洗的计划号
                        dt.Rows[0]["PLANID"] = DateTime.Now.ToString("yyyyMMdd") + sMCHID.ToString().Substring(3, 3) + sLR + NextValue("MODPLANNO");
                        //自动生成一条已经完成的模具干冰清洗计划
                        DataTable dtplan = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@" SELECT *FROM MDD0003 WHERE planid='" + dt.Rows[0]["PLANID"] + "'" , null).Tables[0];
                        if (dtplan == null || dtplan.Rows.Count == 0)
                        {
                            sqllist.Add(@"INSERT INTO MDD0003
                              (ID, fac, wdate, wsht, wban, mchid, lrflag, statime, endtime, itnbra, itnbrb, 
                               enam, etim, modid, taoid, planid, planstatus, gqcod, snam, stim, clndiv,itdsca,itdscb)
                               VALUES
                              (SYS_GUID(), '07', TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                                      "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" + row["WBAN"].ToString() + "',  '" +
                                      sMCHID + "', '" + sLR + "', SYSDATE-1/24, SYSDATE,  '" + dt.Rows[0]["ITNBR"] + "',  '" +
                                      dt.Rows[0]["ITNBR"] + "',  '" + sLogId + "', SYSDATE, '" + dt.Rows[0]["CAVID"] + "',  '" +
                                      dt.Rows[0]["CHAID"] + "', '" + dt.Rows[0]["PLANID"] + "','5', '" + dt.Rows[0]["GQID"] +
                                      "', '" + sLogId + "', SYSDATE, '3','" + dt.Rows[0]["ITDSC"] + "','" + dt.Rows[0]["ITDSC"] + "')");
                        }

                    }
                    else
                    {
                        //有干冰清洗计划就将状态变更为已完成
                        //sqllist.Add("UPDATE MDD0003 SET PLANSTATUS = '5',STIM=SYSDATE,SNAM='" + sLogId + "' WHERE PLANID = '" + dt.Rows[0]["PLANID"] + "'");
                    }
                    //自动生成一条已经完成的模具干冰清洗履历
                    sqllist.Add(@"INSERT INTO MDD0001
                         (ID, fac, wdate, wsht, wban, mchid, lr, modid, taoid, gqcod, 
                          itnbr, itdsc, statime,  ecode, cnt, enam, etim, planid,CLN_LOC,CLN_RSN)
                          VALUES
                         (SYS_GUID(), '07', TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                                  "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" + row["WBAN"].ToString() + "', '" +
                                  sMCHID + "', '" + sLR + "', '" + dt.Rows[0]["CAVID"] + "', '" + dt.Rows[0]["CHAID"] + "', '" +
                                  dt.Rows[0]["GQID"] + "', '" + dt.Rows[0]["ITNBR"] + "', '" + dt.Rows[0]["ITDSC"] +
                                  "', SYSDATE, '" + (type + 2) + "', '" + dt.Rows[0]["USECOUNT_1"].ToString() + "','" + sLogId + "', SYSDATE, '" +//次数从现况的干冰次数获取
                                  dt.Rows[0]["PLANID"] + "','" + sCln_Loc + "','" + sCln_Rsn + "')");



                }

                //变更干冰清洗信号,当前使用次数(MES计数)置0，更新修改人，修改时间（按日期判断模具现况状态使用）
                //sqllist.Add("UPDATE MDD0002 SET DRYICE=" + (type == 3 ? 1 : 0) + ",USECOUNT_1=0,ENAM = '" + sLogId + "',ETIM = SYSDATE WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'");

                //2018.09.18 JOE 经于李磊讨论，只有拆卸清洗和模具交替重置ETIM时间，干冰清洗不重置（对模具现况 持续日期 有影响）
                //2018.10.09 和李磊张科讨论，增加干冰清洗时间，现况增加干冰清洗持续时间
                //2020.06.16 新增模具拆卸修理后，不重置任何清洗次数
                int iSCGBQXQ = 0;
                if (type != 4)//非拆卸修理
                {
                    // 记录首次干冰清洗前次数

                    DataTable dtSCGBQXQ = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, @"SELECT * FROM MDD0002 WHERE  MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'", null).Tables[0]; ;
                    iSCGBQXQ = Convert.ToInt32(dtSCGBQXQ.Rows[0]["USECOUNT"].ToString());

                    sqllist.Add("UPDATE MDD0002 SET DRYICE=" + (type == 3 ? 1 : 0) + ",USECOUNT_1=0,GTIM = SYSDATE WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'");

                }
                else
                {
                    sqllist.Add("UPDATE MDD0002 SET GTIM = SYSDATE WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'");
                }



                int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                if (iRes > 0)
                {
                    if (type == 1)
                    {
                        ////2021-11-24 增加干冰清洗计划生成   李旭日 start
                        //生成干冰清洗计划
                        DataTable dtInfoCX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, @"SELECT A.CAVID,A.CHAID,A.GQID,A.ITNBR,A.ITDSC,A.USECOUNT,A.USECOUNT_1,A.DRYICE,B.*
                                              FROM (SELECT * FROM MDD0002 WHERE MCHID='" +
                                                      sMCHID + "' AND LRFLAG='" + sLR + @"') A,
                                              (SELECT C.*,D.ITDSC ITDSCD FROM MDD0003 C,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) D 
                                              WHERE C.ITNBRB=D.ITNBR(+) AND 
                                              C.PLANSTATUS<>'6' AND C.PLANSTATUS<>'5') B 
                                              WHERE A.MCHID=B.MCHID(+) AND A.LRFLAG=B.LRFLAG(+)", null).Tables[0];
                        DataTable dtHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, @"SELECT A.ITDSC,B.*,C.* FROM EDB0010 A,mdd0006 B,MDD0007 C WHERE A.PATTERN(+) = B.HW_FLAG AND B.HW_FLAG = C.HW_FLAG AND ITCLSFYCOD = 'CR' AND A.USEYN = 'Y' AND A.PATTERN IS NOT NULL AND A.ITDSC LIKE '%欧盟%' AND A.ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "'", null).Tables[0]; 
                        DataTable dtTSHWK = new DataTable();
                        DataTable dtTHT = new DataTable();
                        // 干冰首次预警
                        int iGBCNTSCMIN = 0;
                        // 干冰首次上限
                        int iGBCNTSCMAX = 0;
                        // 干冰递增预警
                        int iGBCNTDZMIN = 0;
                        // 干冰递增上限
                        int iGBCNTDZMAX = 0;
                        //拆卸上限次数
                        int iCNTMAX = 0;
                        //干冰上限天数
                        int iGBSXTS = 0;
                        // 拆卸上限天数
                        int iCXSXTS = 0;

                        if (dtHWK != null && dtHWK.Rows.Count > 0)
                        {
                            DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString,CommandType.Text,@"SELECT * FROM MDD0006 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'" , null).Tables[0];
                            iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                            iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                            iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            dtTSHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0007 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'", null).Tables[0];
                            {
                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTSCMIN"].ToString()))
                                {
                                    iGBCNTSCMIN = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTSCMIN = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTSCMIN"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTSCMAX"].ToString()))
                                {
                                    iGBCNTSCMAX = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTSCMAX = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTSCMAX"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                            }
                        }
                        else
                        {
                            dtTHT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0004 WHERE ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "' AND MUITNBR = '" + dtInfoCX.Rows[0]["CAVID"].ToString() + "'", null).Tables[0];
                            if (dtTHT != null && dtTHT.Rows.Count > 0)
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'Y'", null).Tables[0];
                                iGBCNTSCMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTSCMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["TXDATE"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                            else
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'N'", null).Tables[0];
                                iGBCNTSCMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTSCMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                        }
                        DateTime dtNOW = DateTime.Now;
                        DateTime dtmGBMAX = dtNOW.AddDays(iGBSXTS);
                        DateTime dtmCXMAX = dtNOW.AddDays(iCXSXTS);
                        sqllist1.Add(@"UPDATE PPB0310 SET FLAG = 'N' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "'");
                        sqllist1.Add(@"INSERT INTO PPB0310(ID, MCHID,LR,ITDSC,CAVID,GBSJCNT,CNTGB,CNTGBMIN,CNTGBMAX,CNT,CNTMAX,GBDATEMAX,CXDATEMAX,FLAG,STTIM)
                              VALUES(SYS_GUID(), '" + sMCHID + "', '" + sLR + "','" + dtInfoCX.Rows[0]["ITDSC"].ToString() + "','" + dtInfoCX.Rows[0]["CAVID"].ToString() + "',0,'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "'," + iGBCNTSCMIN + "," + iGBCNTSCMAX + ",'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "', " + iCNTMAX + ",TO_DATE('" + dtmGBMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS'),TO_DATE('" +
                                     dtmCXMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS') ,'Y',sysdate)");

                        ////2021-11-24 增加干冰清洗计划生成   李旭日 end
                    }
                    if (type == 2)
                    {
                        ////2021-11-24 增加干冰清洗计划生成   李旭日 start
                        //生成干冰清洗计划
                        DataTable dtInfoCX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.CAVID,A.CHAID,A.GQID,A.ITNBR,A.ITDSC,A.USECOUNT,A.USECOUNT_1,A.DRYICE,B.*
                                              FROM (SELECT * FROM MDD0002 WHERE MCHID='" +
                                                      sMCHID + "' AND LRFLAG='" + sLR + @"') A,
                                              (SELECT C.*,D.ITDSC ITDSCD FROM MDD0003 C,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) D 
                                              WHERE C.ITNBRB=D.ITNBR(+) AND 
                                              C.PLANSTATUS<>'6' AND C.PLANSTATUS<>'5') B 
                                              WHERE A.MCHID=B.MCHID(+) AND A.LRFLAG=B.LRFLAG(+)", null).Tables[0];
                        DataTable dtHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.ITDSC,B.*,C.* FROM EDB0010 A,mdd0006 B,MDD0007 C WHERE A.PATTERN(+) = B.HW_FLAG AND B.HW_FLAG = C.HW_FLAG AND ITCLSFYCOD = 'CR' AND A.USEYN = 'Y' AND A.PATTERN IS NOT NULL AND A.ITDSC LIKE '%欧盟%' AND A.ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "'", null).Tables[0];
                        DataTable dtTSHWK = new DataTable();
                        DataTable dtTHT = new DataTable();
                        // 干冰首次预警
                        int iGBCNTSCMIN = 0;
                        // 干冰首次上限
                        int iGBCNTSCMAX = 0;
                        // 干冰递增预警
                        int iGBCNTDZMIN = 0;
                        // 干冰递增上限
                        int iGBCNTDZMAX = 0;
                        //拆卸上限次数
                        int iCNTMAX = 0;
                        //干冰上限天数
                        int iGBSXTS = 0;
                        // 拆卸上限天数
                        int iCXSXTS = 0;

                        if (dtHWK != null && dtHWK.Rows.Count > 0)
                        {
                            DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0006 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'", null).Tables[0];

                            iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                            iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                            iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            dtTSHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0007 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'", null).Tables[0];
                            if (dtTSHWK != null && dtTSHWK.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTSCMIN"].ToString()))
                                {
                                    iGBCNTSCMIN = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTSCMIN = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTSCMIN"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTSCMAX"].ToString()))
                                {
                                    iGBCNTSCMAX = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTSCMAX = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTSCMAX"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                            }

                        }
                        else
                        {
                            dtTHT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0004 WHERE ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "' AND MUITNBR = '" + dtInfoCX.Rows[0]["CAVID"].ToString() + "'", null).Tables[0];
                            if (dtTHT != null && dtTHT.Rows.Count > 0)
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'Y'", null).Tables[0];
                                iGBCNTSCMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTSCMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["TXDATE"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                            else
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'N'", null).Tables[0];
                                iGBCNTSCMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTSCMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                        }
                        DateTime dtNOW = DateTime.Now;
                        DateTime dtmGBMAX = dtNOW.AddDays(iGBSXTS);
                        DateTime dtmCXMAX = dtNOW.AddDays(iCXSXTS);
                        sqllist1.Add(@"UPDATE PPB0310 SET FLAG = 'N' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "'");
                        sqllist1.Add(@"INSERT INTO PPB0310(ID, MCHID,LR,ITDSC,CAVID,GBSJCNT,CNTGB,CNTGBMIN,CNTGBMAX,CNT,CNTMAX,GBDATEMAX,CXDATEMAX,FLAG,STTIM)
                              VALUES(SYS_GUID(), '" + sMCHID + "', '" + sLR + "','" + dtInfoCX.Rows[0]["ITDSC"].ToString() + "','" + dtInfoCX.Rows[0]["CAVID"].ToString() + "',0,'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "'," + iGBCNTSCMIN + "," + iGBCNTSCMAX + ",'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "', " + iCNTMAX + ",TO_DATE('" + dtmGBMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS'),TO_DATE('" +
                                     dtmCXMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS') ,'Y',sysdate)");
                    }
                    if (type == 3)
                    {
                        ////2021-11-24 增加干冰清洗计划生成   李旭日 start
                        //生成干冰清洗计划
                        DataTable dtInfoCX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.CAVID,A.CHAID,A.GQID,A.ITNBR,A.ITDSC,A.USECOUNT,A.USECOUNT_1,A.DRYICE,B.*
                                              FROM (SELECT * FROM MDD0002 WHERE MCHID='" +
                                                      sMCHID + "' AND LRFLAG='" + sLR + @"') A,
                                              (SELECT C.*,D.ITDSC ITDSCD FROM MDD0003 C,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) D 
                                              WHERE C.ITNBRB=D.ITNBR(+) AND 
                                              C.PLANSTATUS<>'6' AND C.PLANSTATUS<>'5') B 
                                              WHERE A.MCHID=B.MCHID(+) AND A.LRFLAG=B.LRFLAG(+)", null).Tables[0];
                        DataTable dtHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.ITDSC,B.*,C.* FROM EDB0010 A,mdd0006 B,MDD0007 C WHERE A.PATTERN(+) = B.HW_FLAG AND B.HW_FLAG = C.HW_FLAG AND ITCLSFYCOD = 'CR' AND A.USEYN = 'Y' AND A.PATTERN IS NOT NULL AND A.ITDSC LIKE '%欧盟%' AND A.ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "'", null).Tables[0];
                        DataTable dtTSHWK = new DataTable();
                        DataTable dtTHT = new DataTable();
                        // 干冰首次预警
                        int iGBCNTSCMIN = 0;
                        // 干冰首次上限
                        int iGBCNTSCMAX = 0;
                        // 干冰递增预警
                        int iGBCNTDZMIN = 0;
                        // 干冰递增上限
                        int iGBCNTDZMAX = 0;
                        //拆卸上限次数
                        int iCNTMAX = 0;
                        //干冰上限天数
                        int iGBSXTS = 0;
                        // 拆卸上限天数
                        int iCXSXTS = 0;

                        if (dtHWK != null && dtHWK.Rows.Count > 0)
                        {
                            DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0006 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'", null).Tables[0];

                            iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                            iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                            iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            dtTSHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0007 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'", null).Tables[0];
                            if (dtTSHWK != null && dtTSHWK.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTDZMIN"].ToString()))
                                {
                                    iGBCNTDZMIN = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTDZMIN = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTDZMIN"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTDZMAX"].ToString()))
                                {
                                    iGBCNTDZMAX = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTDZMAX = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTDZMAX"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                            }

                        }
                        else
                        {
                            dtTHT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0004 WHERE ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "' AND MUITNBR = '" + dtInfoCX.Rows[0]["CAVID"].ToString() + "'", null).Tables[0];
                            if (dtTHT != null && dtTHT.Rows.Count > 0)
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'Y'", null).Tables[0];
                                iGBCNTDZMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTDZMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBTXCNT"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                            else
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'N'", null).Tables[0];
                                iGBCNTDZMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTDZMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                        }
                        DateTime dtNOW = DateTime.Now;
                        DateTime dtmGBMAX = dtNOW.AddDays(iGBSXTS);
                        DateTime dtmCXMAX = dtNOW.AddDays(iCXSXTS);
                        DataTable dtPPB0310 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM PPB0310 WHERE FLAG = 'Y' AND MCHID = '" + sMCHID + "' AND LR =  '" + sLR + "'", null).Tables[0];
                        int iCNT1 = 0;
                        if (dtPPB0310 != null && dtPPB0310.Rows.Count > 0)
                        {
                            iCNT1 = Convert.ToInt32(dtPPB0310.Rows[0]["GBSJCNT"].ToString()) + 1;
                        }
                        else
                        {
                            iCNT1 = 1;
                        }
                        sqllist1.Add(@"UPDATE PPB0310 SET FLAG = 'N' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "'");
                        sqllist1.Add(@"INSERT INTO PPB0310(ID, MCHID,LR,ITDSC,CAVID,GBSJCNT,CNTGB,CNTGBMIN,CNTGBMAX,CNT,CNTMAX,GBDATEMAX,CXDATEMAX,FLAG,STTIM)
                              VALUES(SYS_GUID(), '" + sMCHID + "', '" + sLR + "','" + dtInfoCX.Rows[0]["ITDSC"].ToString() + "','" + dtInfoCX.Rows[0]["CAVID"].ToString() + "'," + iCNT1 + ",'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "'," + iGBCNTDZMIN + "," + iGBCNTDZMAX + ",'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "', " + iCNTMAX + ",TO_DATE('" + dtmGBMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS'),TO_DATE('" +
                                     dtmCXMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS') ,'Y',sysdate)");
                    }
                    else
                    {
                        ////2021-11-24 增加干冰清洗计划生成   李旭日 start
                        //生成干冰清洗计划
                        DataTable dtInfoCX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.CAVID,A.CHAID,A.GQID,A.ITNBR,A.ITDSC,A.USECOUNT,A.USECOUNT_1,A.DRYICE,B.*
                                              FROM (SELECT * FROM MDD0002 WHERE MCHID='" +
                                                      sMCHID + "' AND LRFLAG='" + sLR + @"') A,
                                              (SELECT C.*,D.ITDSC ITDSCD FROM MDD0003 C,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) D 
                                              WHERE C.ITNBRB=D.ITNBR(+) AND 
                                              C.PLANSTATUS<>'6' AND C.PLANSTATUS<>'5') B 
                                              WHERE A.MCHID=B.MCHID(+) AND A.LRFLAG=B.LRFLAG(+)", null).Tables[0];
                        DataTable dtHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.ITDSC,B.*,C.* FROM EDB0010 A,mdd0006 B,MDD0007 C WHERE A.PATTERN(+) = B.HW_FLAG AND B.HW_FLAG = C.HW_FLAG AND ITCLSFYCOD = 'CR' AND A.USEYN = 'Y' AND A.PATTERN IS NOT NULL AND A.ITDSC LIKE '%欧盟%' AND A.ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "'", null).Tables[0];
                        DataTable dtTSHWK = new DataTable();
                        DataTable dtTHT = new DataTable();
                        // 干冰首次预警
                        int iGBCNTSCMIN = 0;
                        // 干冰首次上限
                        int iGBCNTSCMAX = 0;
                        // 干冰递增预警
                        int iGBCNTDZMIN = 0;
                        // 干冰递增上限
                        int iGBCNTDZMAX = 0;
                        //拆卸上限次数
                        int iCNTMAX = 0;
                        //干冰上限天数
                        int iGBSXTS = 0;
                        // 拆卸上限天数
                        int iCXSXTS = 0;

                        if (dtHWK != null && dtHWK.Rows.Count > 0)
                        {
                            DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0006 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'", null).Tables[0];
                            iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                            iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                            iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            dtTSHWK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0007 WHERE HW_FLAG = '" + dtHWK.Rows[0]["HW_FLAG"].ToString() + "'", null).Tables[0];
                            if (dtTSHWK != null && dtTSHWK.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTSCMIN"].ToString()))
                                {
                                    iGBCNTSCMIN = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTSCMIN = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTSCMIN"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                                if (string.IsNullOrEmpty(dtTSHWK.Rows[0]["GBCNTSCMAX"].ToString()))
                                {
                                    iGBCNTSCMAX = 0 + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }
                                else
                                {
                                    iGBCNTSCMAX = Convert.ToInt32(dtTSHWK.Rows[0]["GBCNTSCMAX"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                }

                            }
                        }
                        else
                        {
                            dtTHT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0004 WHERE ITDSC = '" + dtInfoCX.Rows[0]["ITDSCD"].ToString() + "' AND MUITNBR = '" + dtInfoCX.Rows[0]["CAVID"].ToString() + "'", null).Tables[0];
                            if (dtTHT != null && dtTHT.Rows.Count > 0)
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'Y'", null).Tables[0];
                                iGBCNTSCMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTSCMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["TXDATE"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                            else
                            {
                                DataTable dtTHTINFO = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0005 WHERE AS_FLAG = 'N'", null).Tables[0];
                                iGBCNTSCMIN = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iGBCNTSCMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["GBCOUNT"].ToString()) + Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString());
                                iCNTMAX = Convert.ToInt32(dtTHTINFO.Rows[0]["CXCOUNT"].ToString());
                                iGBSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["GBDATE"].ToString());
                                iCXSXTS = Convert.ToInt32(dtTHTINFO.Rows[0]["CXDATE"].ToString());
                            }
                        }
                        DateTime dtNOW = DateTime.Now;
                        DateTime dtmGBMAX = dtNOW.AddDays(iGBSXTS);
                        DateTime dtmCXMAX = dtNOW.AddDays(iCXSXTS);
                        sqllist1.Add(@"UPDATE PPB0310 SET FLAG = 'N' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "'");
                        sqllist1.Add(@"INSERT INTO PPB0310(ID, MCHID,LR,ITDSC,CAVID,GBSJCNT,CNTGB,CNTGBMIN,CNTGBMAX,CNT,CNTMAX,GBDATEMAX,CXDATEMAX,FLAG,STTIM)
                              VALUES(SYS_GUID(), '" + sMCHID + "', '" + sLR + "','" + dtInfoCX.Rows[0]["ITDSC"].ToString() + "','" + dtInfoCX.Rows[0]["CAVID"].ToString() + "',0,'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "'," + iGBCNTSCMIN + "," + iGBCNTSCMAX + ",'" +
                                     Convert.ToInt32(dtInfoCX.Rows[0]["USECOUNT"].ToString()) + "', " + iCNTMAX + ",TO_DATE('" + dtmGBMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS'),TO_DATE('" +
                                     dtmCXMAX.ToString() + "','YYYY-MM-DD HH24:MI:SS') ,'Y',sysdate)");

                        ////2021-11-24 增加干冰清洗计划生成   李旭日 end
                    }
                    int iRes1 = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist1);
                }

                if (iRes > 0)
                    return "OK";
                else
                    return "NG";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "EX";
            }

        }

        [HttpGet]
        [Route("api/GetDSJPlanInfo")]
        public DataTable GetDSJPlanInfo(string MCHID)
        {
             
            try
            {
                string sql = "SELECT * FROM MDD0003 WHERE MCHID = '" + MCHID.Substring(0, 6) + "' AND LRFLAG = '" + MCHID.Substring(6, 1) + "' AND PLANSTATUS = '3'";
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        /// <summary>
        /// [WebMethod(Description = "根据机台左右获模具上机现况")]
        /// </summary>
        /// <param name="MCHID"></param>
        /// <param name="LR"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetMUSJXK")]
        public DataTable GetMUSJXK(string MCHID, string LR)
        {
            try
            {
                string sql = "SELECT * FROM MDD0002 WHERE MCHID = '" + MCHID + "' AND LRFLAG = '" + LR + "'";
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetZUMUID")]
        public DataTable GetZUMUID(string PLANID)
        {
             
            try
            {
                string sql = "SELECT * FROM MDD0011 WHERE PLANID = '" + PLANID + "' ORDER BY GETTIM DESC";
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/Send_MMSJInfo")]
        public string Send_MMSJInfo(string PLANID, string PLANTYPE, string MCHID, string LR, string ZMID, string REMARK)
        {

            try
            {
               
                int iZMID = Convert.ToInt32(ZMID);

                string sSql = @"INSERT INTO MES_Install (ID,PlanID,PlanType,MCHID,LR,ZMID,InDatetime,Remark,createTime,IS_Read,Sources) VALUES
                (SEQ_MES_Install.Nextval,'" + PLANID + "','" + PLANTYPE + "','" + MCHID + "','" + LR + "'," + iZMID + ",SYSDATE,'',SYSDATE,'N','HY')";

                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionStringMM,CommandType.Text,sSql,null);
                if (iResult > 0)
                    return "OK";
                else
                    return "发送MM系统上机信息失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "EX";
            }
        }

        [HttpGet]
        [Route("api/GetBLDInfoByMch")]
        public DataTable GetBLDInfoByMch(string sMCHLR)
        {
             
            try
            {
                string sql = @"SELECT * FROM PPE0003 WHERE MCHID = '" + sMCHLR.Substring(0, 6) +
                    "' AND LR = '" + sMCHLR.Substring(6, 1) + "'";
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetMoldDryIceWash")]
        public string GetMoldDryIceWash(string sMCHLR)
        {
             
            try
            {
                //            DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT CNT FROM (
                //                            SELECT CNT,ROWNUM RN FROM MDD0001 
                //                            WHERE MCHID='" + sMCHLR.Substring(0, 6) +
                //                            "' AND LR='" + sMCHLR.Substring(6, 1) +
                //                            @"' AND ENDTIME IS NOT NULL AND ECODE='5'
                //                            ORDER BY ENDTIME DESC) WHERE RN=1");
                DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT USECOUNT_1 FROM MDD0002 WHERE MCHID='" +
                                sMCHLR.Substring(0, 6) + "' AND LRFLAG='" + sMCHLR.Substring(6, 1) + "'", null).Tables[0];
                string sQTY = string.Empty;
                if (dt1.Rows.Count > 0 && dt1 != null)
                    sQTY = dt1.Rows[0]["USECOUNT_1"].ToString();
                else
                    sQTY = "0";
                return sQTY;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "2";
            }
        }

        #region
        [HttpGet]
        [Route("api/CheckTimeChangeMod")]
        public DataTable CheckTimeChangeMod(string sMCHID, string sLR, string sECOD)
        {
             
            try
            {
                string sSql = "SELECT * FROM (SELECT * FROM MDD0001 WHERE MCHID ='" + sMCHID + "' AND LR='" + sLR + "' AND ECODE = '" + sECOD + "'  ";
                sSql += " ORDER BY ETIM DESC) WHERE ROWNUM = 1";
                DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];
                return dt1;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }
        #endregion

        [HttpGet]
        [Route("api/GET_WHOutInfo")]
        public DataTable GET_WHOutInfo(string sMCHID, string sLR)
        {
            try
            {
                string strLL = @"SELECT * FROM (SELECT * FROM mdd0003 WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + @"' 
                                   AND PLANSTATUS <> '1' AND PLANSTATUS <> '6' AND PLANSTATUS <> '2' AND CLNDIV <> '3'
                                   ORDER BY ETIM desc) WHERE ROWNUM = 1";
                DataTable dtLL = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strLL, null).Tables[0];
                string strMJJH = "";
                if (dtLL != null && dtLL.Rows.Count > 0)
                {
                    strMJJH = dtLL.Rows[0]["PLANID"].ToString();
                }
                string sWHOutInfo = @"SELECT A.MCHID,A.LR,A.ZMID,B.USECOUNT_1 FROM (SELECT * FROM (SELECT * FROM MDD0011 WHERE  MCHID = '" + sMCHID + "' AND LR = '" + sLR + @"' 
                                  AND PLANID = '" + strMJJH + @"' ORDER BY GETTIM DESC) WHERE ROWNUM = 1) A
                                   LEFT JOIN MDD0002 B ON A.MCHID = B.MCHID AND A.LR = B.LRFLAG";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sWHOutInfo, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/SEND_WashOut")]
        public int SEND_WashOut(string sMCHID, string sLR, string sLogId, string name, string zmid, string usecount, string jhr, string wz, string yy)
        {
            try
            {
                
                int iZMID = Convert.ToInt32(zmid);
                int iCOUNT = Convert.ToInt32(usecount);
                string sql = @"INSERT INTO Mes_DryIceVleaningDetails(ID,MchId,LR,ClearNo,CleaningStaff,CleaningTime,ZMID,createTime,IS_Read,USAGE_COUNT,SOURCES,TUTELAGE_NO,POSITION,REASON) VALUES 
                (SEQ_Mes_DryIceVleaningDetails.Nextval,'" + sMCHID + "','" + sLR + "','" + sLogId + "','" + name + "',SYSDATE," + iZMID + ",SYSDATE,'N'," + iCOUNT + ",'HY','" + jhr + "','" + wz + "','" + yy + "')";

                int iRES = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionStringMM, CommandType.Text, sql, null);
                return iRES;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 0;
            }
        }

        [HttpGet]
        [Route("api/AutoBindBldInfoByProdInfoAndMch")]
        public DataTable AutoBindBldInfoByProdInfoAndMch(string sMCHLR)
        {
            try
            {
                 
                //获取将要交替的生产规格（模具计划里）或者正在进行的生产规格
                string itnbr = "";
                string itdsc = "";
                DataTable dtx = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.ITNBRB,B.ITDSC FROM MDD0003 A,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) B 
                                            WHERE A.ITNBRB=B.ITNBR(+) AND A.CLNDIV='1' 
                                            AND (A.PLANSTATUS='3' OR A.PLANSTATUS='4') 
                                            AND A.MCHID='" + sMCHLR.Substring(0, 6) + "' AND A.LRFLAG='" + sMCHLR.Substring(6, 1) + "'", null).Tables[0];
                if (dtx.Rows.Count > 0)
                {
                    itnbr = dtx.Rows[0]["ITNBRB"].ToString();
                    itdsc = dtx.Rows[0]["ITDSC"].ToString();
                }
                else
                {
                    DataTable dty = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT CUITNBR,CUITDSC FROM PAD0401 WHERE MCHID='" +
                                    sMCHLR.Substring(0, 6) + "' AND LR='" + sMCHLR.Substring(6, 1) + @"' AND DIV='1' 
                                UNION SELECT ITNBR,ITDSC FROM PAG0001 WHERE MCHID='" + sMCHLR + "'", null).Tables[0];
                    itnbr = dty.Rows[0]["CUITNBR"].ToString();
                    itdsc = dty.Rows[0]["CUITDSC"].ToString();
                }
                //采用ERP规格代码
                int idx = itnbr.IndexOf("_T");
                if (idx != -1)
                    itnbr = itnbr.Substring(0, idx);
                //根据获取的生产规格找到对应的胶囊型号
                string sSqlBindBld = @"SELECT ERPJN BLD_ITNBR,JNXH BLD_ITDSC FROM CURE_RECIPE_PLM  WHERE SHYN='Y' AND ITNBR = '" + itnbr + @"'
                                                UNION ALL
                                                SELECT ERPJNBX BLD_ITNBR,JNBX BLD_ITDSC FROM CURE_RECIPE_PLM  WHERE SHYN='Y' AND ITNBR = '" + itnbr + "'";
                DataTable dtBindBld = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqlBindBld, null).Tables[0];
                return dtBindBld;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetBLDInfo")]
        public DataTable GetBLDInfo(string sBCOD)
        {
             
            try
            {
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT * FROM (SELECT ERPJN BLD_ITNBR,JNXH BLD_ITDSC FROM CURE_RECIPE_PLM  WHERE SHYN='Y' " +
                                                    @"UNION ALL
                                                SELECT ERPJNBX BLD_ITNBR,JNBX BLD_ITDSC FROM CURE_RECIPE_PLM  WHERE SHYN='Y') WHERE  BLD_ITNBR LIKE '%" + sBCOD + "%' GROUP BY BLD_ITNBR,BLD_ITDSC", null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetECODE")]
        public DataTable GetECODE()
        {
             
            try
            {
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT JCODE,JDESC FROM PPE0001 WHERE USEYN='Y'", null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        /// 取得服务器时间
        /// </summary>
        /// <returns>服务器时间</returns>
        [HttpGet]
        [Route("api/GetSystemDate")]
        public string GetSystemDate()
        {
             
            try
            {
                //DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"select to_char(sysdate,'yyyy-MM-dd') as fudate from dual");
                //if (dt != null)
                //{
                //    return dt.Rows[0]["fudate"].ToString();
                //}
                return DateTime.Now.ToString("yyyy年MM月dd日", DateTimeFormatInfo.InvariantInfo);
            }
            catch (Exception ex)
            {

                LogHelper.Error("ERROR", ex);
                return DateTime.Now.ToString("yyyy年MM月dd日", DateTimeFormatInfo.InvariantInfo);
            }

        }

        [HttpGet]
        [Route("api/GetBldCount")]
        public string GetBldCount(string sMCH, string sLR)
        {
             
            try
            {
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT COUNT(1) FROM LTA0001 WHERE CUMCH = '" + sMCH +
                                                "' AND LR = '" + sLR +
                                                @"' AND CUTIM BETWEEN TO_DATE(TO_CHAR((SELECT STATIME FROM 
                                            (SELECT ROWNUM RN,STATIME FROM PPE0002  WHERE MCHID = '" + sMCH +
                                                 "' AND LR = '" + sLR + @"' AND ENDTIME IS NULL) X WHERE X.RN=1),
                                            'YYYY-MM-DD HH24:MI:SS'), 'YYYY-MM-DD HH24:MI:SS') AND SYSDATE", null).Tables[0];
                if (dt.Rows[0][0].ToString() == "0")//当胶囊使用次数=0时，判定重复提交
                    return "0";
                return "1";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "2";
            }
        }


        #region 胶囊上机

        [HttpPost]
        [Route("api/ChangeBLD")]
        public string ChangeBLD(Change change)
        {
             
            try
            {
                string lok = @"SELECT COUNT(1) FROM LTA0001 WHERE CUMCH = '" + change.sMCH + "' AND LR = '" + change.sLR +
                                               @"' AND CUTIM BETWEEN TO_DATE(TO_CHAR((SELECT STATIME FROM 
                                            (SELECT ROWNUM RN,STATIME FROM PPE0002  WHERE MCHID = '" + change.sMCH +
                                                "' AND LR = '" + change.sLR + @"' AND ENDTIME IS NULL) X WHERE X.RN=1),
                                            'YYYY-MM-DD HH24:MI:SS'), 'YYYY-MM-DD HH24:MI:SS') AND SYSDATE";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,lok, null).Tables[0];
                //if (dt.Rows[0][0].ToString() == "0")//当胶囊使用次数=0时，判定重复提交
                //    return "FH";           
                string itnbr = "";
                string itdsc = "";

                //5月31号和李磊讨论，考虑 3-已上机 4-首模待检 两种状态 （2-待上机不考虑是因为有可能会提前组装好，但原硫化规格还在正常生产）
                DataTable dtx = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.ITNBRB,B.ITDSC FROM MDD0003 A,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) B 
                                            WHERE A.ITNBRB=B.ITNBR(+) AND A.CLNDIV='1' 
                                            AND (A.PLANSTATUS='3' OR A.PLANSTATUS='4') 
                                            AND A.MCHID='" + change.sMCH + "' AND A.LRFLAG='" + change.sLR + "'", null).Tables[0];
                if (dtx.Rows.Count > 0)
                {
                    itnbr = dtx.Rows[0]["ITNBRB"].ToString();
                    itdsc = dtx.Rows[0]["ITDSC"].ToString();
                }
                else
                {
                    DataTable dty = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT CUITNBR,CUITDSC FROM PAD0401 WHERE MCHID='" +
                                    change.sMCH + "' AND LR='" + change.sLR + @"' AND DIV='1' 
                                UNION SELECT ITNBR,ITDSC FROM PAG0001 WHERE MCHID='" + change.sMCH + change.sLR + "'", null).Tables[0];
                    itnbr = dty.Rows[0]["CUITNBR"].ToString();
                    itdsc = dty.Rows[0]["CUITDSC"].ToString();
                    //2018-07-23 李磊告知胶囊现况的规格信息，不需要体现试验胎规格
                    if (itnbr.Contains("_")) itnbr = itnbr.Substring(0, itnbr.IndexOf("_"));
                    if (itdsc.Contains("_")) itdsc = itdsc.Substring(0, itdsc.IndexOf("_") - 4);
                }

                //当前胶囊现况信息
                DataTable dt2 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT A.*,B.COUNT,C.PLANID FROM (SELECT * FROM PPE0003 
                                              WHERE MCHID='" + change.sMCH + "' AND LR='" + change.sLR + @"') A,PPE0004 B,
                                              (SELECT * FROM PPE0010 WHERE PLANSTATUS = '1' OR PLANSTATUS = '2') C
                                              WHERE A.SPEC=B.ITNBR(+) AND A.MCHID=C.MCHID(+) AND A.LR=C.LRFLAG(+)", null).Tables[0];
                DataRow row = SHIFT.GetShift(DateTime.Now);
                ArrayList sqllist = new ArrayList();

                if (dt2.Rows[0]["PLANID"].ToString() != "")//有胶囊计划,执行计划
                {
                    sqllist.Add("UPDATE PPE0010 SET PLANSTATUS='3',ENAM='" + change.sLogId +
                        "',ETIM=SYSDATE WHERE PLANID='" + dt2.Rows[0]["PLANID"] + "'");
                }
                else//无胶囊计划,增加胶囊计划
                {
                    dt2.Rows[0]["PLANID"] = DateTime.Now.ToString("yyyyMMdd") + change.sMCH.Substring(3, 3) + change.sLR + NextValue("BLDPLANNO");
                    sqllist.Add(@"INSERT INTO PPE0010(ID,FAC,WDATE,WSHT,WBAN,MCHID,LRFLAG,STATIME,ENDTIME,
                                ITNBRA,ITNBRB,BLDCOD,BLDNAM,CNT,MAXCNT,REMARK,PLANID,PLANSTATUS,ENAM,ETIM) 
                                VALUES(SYS_GUID(),'07',TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                                    "','YYYY-MM-DD'),'" + row["WSHT"].ToString() + "', '" + row["WBAN"].ToString() +
                                    "','" + change.sMCH + "','" + change.sLR + "',SYSDATE-1/24,SYSDATE,'" + itnbr + "','" +
                                    itdsc + "','" + change.sBCOD + "','" + change.sBNAM + "','" + dt2.Rows[0]["COUNT1"] +
                                    "','" + dt2.Rows[0]["COUNT"] + "','" + change.sECOD + "','" + dt2.Rows[0]["PLANID"] +
                                    "','3','" + change.sLogId + "',SYSDATE)");
                }
                //录入该机台上一次使用胶囊的下机信息：下机时间、下机原因、使用次数(根据胶囊上下机时间统计硫化条码个数)
                sqllist.Add(@"UPDATE PPE0002 SET ENDTIME = SYSDATE, CNT = " + dt.Rows[0][0] + ", ECODE = '" +
                             change.sECOD + "',MCNT=(SELECT COUNT1 FROM PPE0003 WHERE MCHID='" + change.sMCH + "' AND LR='" +
                             change.sLR + "') WHERE MCHID = '" + change.sMCH + "' AND LR = '" + change.sLR + "' AND ENDTIME IS NULL");

                //变更该机台的胶囊使用现况信息：胶囊规格、生产规格代码、生产规格名称   2021-05-10  更新COUNT1默认为0--李磊
                sqllist.Add(@"UPDATE PPE0003 SET SPEC = '" + change.sBCOD + "',SDSC = '" + change.sBNAM + "',ITNBR = '" +
                                itnbr + "',ITDSC = '" + itdsc + @"',WTIME=SYSDATE,COUNT1='0'
                            WHERE MCHID = '" + change.sMCH + "' AND LR = '" + change.sLR + "'");
                //录入新上机胶囊信息：ID、FAC、日期、班次、班组、机台、左右、新胶囊规格代码、
                //新硫化规格代码、新硫化规格名称、上机时间、上机人、计划号
                //如二次使用，还将录入首次使用的时间、机台、左右、次数
                string[] snd = change.snd;
                if (snd[0] != "")//二次使用的胶囊不更新库存
                {
                    sqllist.Add(@"INSERT INTO ppe0002(ID, fac, wdate, wsht, wban, mchid, CODE, 
                            itnbr, itdsc, statime, lr, enam, etim, planid, ltim, lmchid, llr, lcnt,jnam,REMARK,YN_T,CBAN)
                            VALUES(SYS_GUID(), '07', TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                                 "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" + row["WBAN"].ToString() +
                                 "', '" + change.sMCH + "', '" + change.sBCOD + "', '" + itnbr + "', '" + itdsc + "', SYSDATE, '" + change.sLR +
                                 "', '" + change.sLogId + "', SYSDATE, '" + dt2.Rows[0]["PLANID"] + "', TO_DATE('" + snd[0] +
                                 "','YYYY-MM-DD'), '" + snd[1] + "', '" + snd[2] + "', " + snd[3] + ",'" + change.sBNAM + "','" + change.sREMARK + "','" + change.YN_T + "','" + change.CBAN + "')");
                }
                else
                {
                    sqllist.Add(@"INSERT INTO ppe0002
                        (ID, fac, wdate, wsht, wban, mchid, CODE, itnbr, itdsc, statime, lr, enam, etim, planid,jnam,REMARK,YN_T,CBAN)
                         VALUES(SYS_GUID(), '07', TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") +
                             "','YYYY-MM-DD'), '" + row["WSHT"].ToString() + "', '" + row["WBAN"].ToString() +
                             "', '" + change.sMCH + "', '" + change.sBCOD + "', '" + itnbr + "', '" + itdsc + "', SYSDATE, '" + change.sLR +
                             "', '" + change.sLogId + "', SYSDATE, '" + dt2.Rows[0]["PLANID"] + "','" + change.sBNAM + "','" + change.sREMARK + "','" + change.YN_T + "','" + change.CBAN + "')");
                    //更新胶囊库存量，数量减一
                    sqllist.Add(@"UPDATE PPE0004 SET QTY=QTY-1 WHERE ITNBR='" + change.sBCOD + "'");
                    //插入出库履历
                    string sInsSql = "INSERT INTO PPE0008 (ID,FAC,LOTNO,WDATE,WSHT,ECODE,QTY,ONAM,OTIM,PLANID) VALUES (";
                    sInsSql += "sys_guid(),";
                    sInsSql += "'07',";
                    sInsSql += "'',";
                    sInsSql += "to_date('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sInsSql += "'" + row["WSHT"].ToString() + "',";
                    sInsSql += "'" + change.sBCOD + "',";
                    sInsSql += "'1',";
                    sInsSql += "'" + change.sLogId + "',";
                    sInsSql += "sysdate,";
                    sInsSql += "'" + dt2.Rows[0]["PLANID"] + "'";
                    sInsSql += ")";
                    sqllist.Add(sInsSql);
                }
                int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                if (iRes > 0)
                    return "OK";
                else
                    return "NG";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "EX";
            }

        }

        #endregion
        [HttpGet]
        [Route("api/Kapanguige")]
        public DataTable Kapanguige(string sMCHLR)
        {
             
            //    DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {
                DataTable dty = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT CUITNBR,CUITDSC FROM PAD0401 WHERE MCHID='" +
                    sMCHLR.Substring(0, 6) + "' AND LR='" + sMCHLR.Substring(6, 1) + @"' AND DIV='1' 
                                UNION SELECT ITNBR,ITDSC FROM PAG0001 WHERE MCHID='" + sMCHLR.Substring(0, 6) + sMCHLR.Substring(6, 1) + "'", null).Tables[0];


                string ITNBR = dty.Rows[0]["CUITNBR"].ToString();
                string ITDSC = dty.Rows[0]["CUITDSC"].ToString();

                return dty;

            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/Kapan")]
        public string Kapan(string sMCHLR, string CB, string NAME)
        {
             
            //    DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {
                DataTable dty = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT CUITNBR,CUITDSC FROM PAD0401 WHERE MCHID='" +
                    sMCHLR.Substring(0, 6) + "' AND LR='" + sMCHLR.Substring(6, 1) + @"' AND DIV='1' 
                                UNION SELECT ITNBR,ITDSC FROM PAG0001 WHERE MCHID='" + sMCHLR.Substring(0, 6) + sMCHLR.Substring(6, 1) + "'", null).Tables[0];
                string sInsScan = "INSERT INTO KP0002 (ID,FAC,WDATE,WSHT,WTIM,WNAM,MCHID,LR,KPINCH) VALUES (";
                sInsScan += "sys_guid(),'07',SYSDATE,'A',TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + NAME + "','" + sMCHLR.Substring(0, 6) + "','" + sMCHLR.Substring(6, 1) + "','" + CB + "')";

                string sql = "UPDATE KP0001 SET ITNBR='" + dty.Rows[0]["CUITNBR"].ToString() + "',ITDSC='" + dty.Rows[0]["CUITDSC"].ToString() + "' , KPINCH ='" + CB + "' ,ENAM='" + NAME + "',ETIM =TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')   WHERE MCHID='" + sMCHLR.Substring(0, 6) + "' AND LR='" + sMCHLR.Substring(6, 1) + "'";

                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,sInsScan,null);

                int iResult1 = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sql, null);

                if (iResult > 0 && iResult1 > 0)
                    return "OK";
                else
                    return "Err-NONE";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/KapanOut")]
        public int KapanOut(string sMCHID, string sLR)
        {
             
            //    DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {


                string sql = "UPDATE KP0001 SET ITNBR='',ITDSC='' , KPINCH ='' ,ENAM='',ETIM=''   WHERE MCHID='" + sMCHID + "' AND LR='" + sLR + "'";


                string sql1 = "UPDATE KP0002 SET ENDTIME =TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss') WHERE MCHID='" + sMCHID + "' AND LR='" + sLR + "'";


                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sql, null);

                int iResult1 = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sql1, null);

                if (iResult > 0 && iResult1 > 0)
                    return 0;
                else
                    return 2;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return 2;
            }
        }

        [HttpGet]
        [Route("api/GetMoldPlanInfoByMch")]
        public DataTable GetMoldPlanInfoByMch(string sMCHLR, string CLNDIV)
        {
             
            try
            {
                string sql = "SELECT A.*,B.ITDSC FROM MDD0003 A,EDB0010 B WHERE A.ITNBRB=B.ITNBR(+) and A.PLANSTATUS ='2' and CLNDIV='" + CLNDIV + "'  and A.mchid = '" + sMCHLR.Substring(0, 6) + "' and A.LRFLAG = '" + sMCHLR.Substring(6, 1) + "'";
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        /// [WebMethod(Description = "模具下机取消对应计划")]
        /// </summary>
        /// <param name="sMCHID"></param>
        /// <param name="sLR"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/MoldQX")]
        public int MoldQX(string sMCHID, string sLR)
        {
            try
            {
                ArrayList sqllist = new ArrayList();
                string sqlMMInfo = @"SELECT * FROM (SELECT * FROM mdd0003 WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + @"'
                                   AND (PLANSTATUS ='3' OR PLANSTATUS = '4')
                                   ORDER BY ETIM desc) WHERE ROWNUM = 1";
                DataTable dtMMInfo = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlMMInfo, null).Tables[0];
                if (dtMMInfo != null && dtMMInfo.Rows.Count > 0)
                {
                    string strPLANID = dtMMInfo.Rows[0]["PLANID"].ToString();
                    sqllist.Add(@"UPDATE  mdd0003 SET PLANSTATUS = '6' WHERE PLANID = '" + strPLANID + "'");

                    string strDtime1 = dtMMInfo.Rows[0]["STIM"].ToString();
                    DateTime dtIME1 = DateTime.Now;
                    if (!string.IsNullOrEmpty(strDtime1))
                    {
                        dtIME1 = Convert.ToDateTime(strDtime1);
                    }

                    sqllist.Add(@"INSERT INTO MDD0012
                                    (DPLANID, MESID, mchid, LR, BEFOREITNBR, BEFOREITDSC, ITNBR, 
                                     ITDSC, DEMANDTIME,DEMANDSHIFT, PLANSTARTTIME, PLANENDTIME, DETAIL, REMARK, DEPTNO,CREATEDATE,IS_Q,IS_Read,Sources,PLANTYPE)
                                 VALUES
                                    (SYS_GUID(), '" + dtMMInfo.Rows[0]["PLANID"].ToString() + @"', 
                                     '" + dtMMInfo.Rows[0]["MCHID"].ToString() + "', '" + dtMMInfo.Rows[0]["LRFLAG"].ToString() + "', '" + dtMMInfo.Rows[0]["ITNBRA"].ToString() + @"', 
                                     '" + dtMMInfo.Rows[0]["ITDSCA"].ToString() + "', '" + dtMMInfo.Rows[0]["ITNBRB"].ToString() + @"', 
                                     '" + dtMMInfo.Rows[0]["ITDSCB"].ToString() + "',TO_DATE( '" + dtIME1.ToString("yyyy-MM-dd") + @"', 'YYYY-MM-DD'),0,
                                    TO_DATE( '" + dtMMInfo.Rows[0]["STATIME"].ToString() + @"', 'YYYY-MM-DD HH24:MI:SS'), 
                                    TO_DATE( '" + dtMMInfo.Rows[0]["ENDTIME"].ToString() + @"', 'YYYY-MM-DD HH24:MI:SS'), 
                                     '" + dtMMInfo.Rows[0]["REMARK"].ToString() + "','','PDA', SYSDATE, 10,'N','HY',10)");

                    int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sqllist);
                    if (iRes > 0)
                        return 0;
                    else
                        return 2;
                }
                else
                {
                    return 2;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("错误",ex);
                return 3;
            }
        }




        [HttpGet]
        [Route("api/MoldOut")]
        public int MoldOut(string sMCHID, string sLR)
        {
             
            try
            {

                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"select * from  MDD0001  WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "' AND ENDTIME IS NULL ", null).Tables[0];
                DataTable dtxk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"select * from  MDD0002  WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'   ", null).Tables[0];
                if (dt.Rows.Count < 1 && dtxk.Rows.Count < 1)
                {
                    return 1;
                }
                //dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"select * from  PPE0002  WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "' AND ENDTIME IS NULL ");
                //if (dt.Rows.Count < 1)
                //{
                //    return 1;
                //}
                DataTable dcnt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT COUNT(1) FROM LTA0001 WHERE CUMCH = '" +
                       sMCHID + "' AND LR = '" + sLR + @"' AND CUTIM BETWEEN 
                                TO_DATE(TO_CHAR((SELECT STATIME FROM (SELECT ROWNUM RN,STATIME FROM MDD0001 
                                WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + @"' AND ENDTIME IS NULL) X 
                                WHERE X.RN=1),'YYYY-MM-DD HH24:MI:SS'), 'YYYY-MM-DD HH24:MI:SS') AND SYSDATE", null).Tables[0];
                string cnt = dcnt.Rows[0][0].ToString();

                LogHelper.Warning("模具下机接口开始-----------------------");
                // 与MM系统接口  李旭日 2022-4-26 start
                string sqlMMInfo = @"select A.*,B.CBSWBM,B.CBGG,B.CBHW,B.CBMH,B.CKSJ,B.ZMID from (SELECT * FROM (SELECT * FROM mdd0003 WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + @"'
                                   AND PLANSTATUS <> '1' AND PLANSTATUS <> '6' AND PLANSTATUS <> '2' AND CLNDIV <> '3'
                                   ORDER BY ETIM desc) WHERE ROWNUM = 1)A,
                                   (SELECT * FROM (SELECT * FROM MDD0011 WHERE  MCHID = '" + sMCHID + "' AND LR = '" + sLR + @"' ORDER BY GETTIM DESC) ) B 
                                  WHERE A.PlanID = B.PlanID(+) 
                         
                                    AND A.MCHID = B.MCHID(+) AND A.LRFLAG = B.LR(+)
                                    AND A.MCHID = '" + sMCHID + "' AND A.LRFLAG = '" + sLR + "' AND B.CKSJ IS NOT NULL ORDER BY B.CKSJ DESC";
                DataTable dtMMInfo = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlMMInfo, null).Tables[0];
                DateTime ttoday = DateTime.Now;
                DateTime tSJSJ = DateTime.Now;
                if (!string.IsNullOrEmpty(dtxk.Rows[0]["ETIM"].ToString()))
                {
                    tSJSJ = DateTime.Parse(dtxk.Rows[0]["ETIM"].ToString());
                }



                TimeSpan ts1 = ttoday - tSJSJ;
                double dbJG = Convert.ToDouble(ts1.TotalDays.ToString());

                if (dtMMInfo != null && dtMMInfo.Rows.Count > 0)
                {
                    int iPLANTYPE = 0;
                    if (dtMMInfo.Rows[0]["CLNDIV"].ToString() == "1")
                    {
                        iPLANTYPE = 10;
                    }
                    else if (dtMMInfo.Rows[0]["CLNDIV"].ToString() == "2")
                    {
                        iPLANTYPE = 20;
                    }
                    else if (dtMMInfo.Rows[0]["CLNDIV"].ToString() == "4")
                    {
                        iPLANTYPE = 30;
                    }
                    int iZMID = 0;
                    if (!string.IsNullOrEmpty(dtMMInfo.Rows[0]["ZMID"].ToString()))
                    {
                        iZMID = Convert.ToInt32(dtMMInfo.Rows[0]["ZMID"].ToString());
                    }

                    int iSYCS = Convert.ToInt32(dtxk.Rows[0]["USECOUNT"].ToString());
                    string sqlMMXJ = @"INSERT INTO MES_Use (ID,PlanID,MCHID,LR,ZMID,YCBSWBM,YCBGG,YCBHW,YCBMH,createTime,IS_Read,Sources,BCSYCS,BCSJTS,ITNBR,PLANTYPE) VALUES 
                               (SEQ_MES_Use.Nextval,'" + dtMMInfo.Rows[0]["PLANID"].ToString() + "','" + sMCHID + "','" + sLR + "'," + iZMID + @",
               '" + dtMMInfo.Rows[0]["CBSWBM"].ToString() + "','" + dtMMInfo.Rows[0]["CBGG"].ToString() + "','" + dtMMInfo.Rows[0]["CBHW"].ToString() + @"',
                '" + dtMMInfo.Rows[0]["CBMH"].ToString() + "',SYSDATE,'N','HY'," + iSYCS + "," + dbJG + ",'" + dtMMInfo.Rows[0]["ITNBRB"].ToString() + "'," + iPLANTYPE + ")";
                    int aaa = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionStringMM, CommandType.Text, sqlMMXJ, null);
                    LogHelper.Warning("计划id： " + dtMMInfo.Rows[0]["PLANID"].ToString() + "模具下机发送数据结果：" + aaa);

                }

                // 与MM系统接口  李旭日 2022-4-26 end
                LogHelper.Debug("模具下机接口结束-----------------------");
                ArrayList sqllist = new ArrayList();

                sqllist.Add(@"UPDATE MDD0002 SET CAVID= '', CHAID='', GQID='',ITNBR = '',ITDSC = '',USECOUNT=0,USECOUNT_1=0,ENAM='',ETIM=NULL,GTIM=NULL,DRYICE=0 ,AS_FLAG='N' WHERE MCHID = '" + sMCHID + "' AND LRFLAG = '" + sLR + "'");
                //录入该机台上一次使用模具的下机信息：下机时间、下机原因ECODE=1、使用次数(根据模具上下机时间统计硫化条码个数)
                DataTable dFlag = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM MDD0003 WHERE MCHID = '" +
                      sMCHID + "' AND LRFLAG = '" + sLR + @"' AND (PLANSTATUS = '1' OR PLANSTATUS = '2')", null).Tables[0];
                if (dFlag != null && dFlag.Rows.Count > 0)
                {
                    sqllist.Add(@"UPDATE MDD0001 SET ENDTIME = SYSDATE, CNT = '" + cnt +
                                            "', ECODE = '" + dFlag.Rows[0]["CLNDIV"].ToString() + "' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR +
                                            "' AND ENDTIME IS NULL");
                }
                else
                {
                    sqllist.Add(@"UPDATE MDD0001 SET ENDTIME = SYSDATE, CNT = '" + cnt +
                                            "', ECODE = '8' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR +
                                            "' AND ENDTIME IS NULL");
                }


                DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT COUNT(1) FROM LTA0001 WHERE CUMCH = '" + sMCHID + "' AND LR = '" + sLR +
                                   @"' AND CUTIM BETWEEN TO_DATE(TO_CHAR((SELECT STATIME FROM 
                                            (SELECT ROWNUM RN,STATIME FROM PPE0002  WHERE MCHID = '" + sMCHID +
                                    "' AND LR = '" + sLR + @"' AND ENDTIME IS NULL) X WHERE X.RN=1),
                                            'YYYY-MM-DD HH24:MI:SS'), 'YYYY-MM-DD HH24:MI:SS') AND SYSDATE", null).Tables[0];
                ////变更该机台的胶囊使用现况信息：胶囊规格、生产规格代码、生产规格名称
                //sqllist.Add(@"UPDATE PPE0003 SET SPEC = '',SDSC = '',ITNBR = '',ITDSC = '',WTIME=NULL,COUNT1='0',COUNT2='0'  WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "'");
                ////录入该机台上一次使用胶囊的下机信息：下机时间、下机原因、使用次数(根据胶囊上下机时间统计硫化条码个数)
                //sqllist.Add(@"UPDATE PPE0002 SET ENDTIME = SYSDATE, CNT = " + dt1.Rows[0][0] + ", ECODE = '1',MCNT=(SELECT COUNT1 FROM PPE0003 WHERE MCHID='" + sMCHID + "' AND LR='" +
                //             sLR + "') WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "' AND ENDTIME IS NULL");

                //增加下机清除PAG0001机台对应的规格信息，2020-04-27 JOE
                sqllist.Add("UPDATE PAG0001 SET ITNBR = '',ITDSC = '' WHERE MCHID ='" + sMCHID + sLR + "'");
                int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                if (iRes > 0)
                    return 0;
                else
                    return 2;

            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return 3;
            }

        }

        [HttpGet]
        [Route("api/SEND_STTZXH")]
        public string SEND_STTZXH(string MCHID, string LRFLAG)
        {
             
            DataRow row = SHIFT.GetShift(DateTime.Now);
            ArrayList sqllist = new ArrayList();
            try
            {
                // 通过机台号和左右模查询成品规格信息和胎胚规格信息
                string sSqlchk = "SELECT * FROM PAD0401 WHERE DIV = 1  AND MCHID = '" + MCHID + "' AND LR = '" + LRFLAG + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqlchk, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    //登记完成后发送<1-供给暂停>信息
                    string sIFSql = "INSERT INTO IF_WMS_GT_11 (MCHID,ITNBR,ITDSC,GTITNBR,LRFLAG,CTLDIV,RCV_FLAG,ENT_USER_ID,ENT_DT) VALUES (";
                    sIFSql += "'" + MCHID + "',";
                    sIFSql += "'" + dt.Rows[0]["CUITNBR"].ToString() + "',";
                    sIFSql += "'" + dt.Rows[0]["CUITDSC"].ToString() + "',";
                    sIFSql += "'" + dt.Rows[0]["BUITNBR"].ToString() + "',";
                    sIFSql += "'" + LRFLAG + "',";
                    sIFSql += "'1',";//1-供给暂停
                    sIFSql += "'N',";
                    sIFSql += "'下机',";
                    sIFSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS')";
                    sIFSql += ")";
                    
                    int k = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString2,CommandType.Text, sIFSql,null);
                    if (k > 0)
                        return "OK";
                    else
                        return "-1";
                }
                else
                {
                    return "0";
                }



            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "-1";
            }
        }

        [HttpGet]
        [Route("api/WashOut")]
        public int WashOut(string sMCHID, string sLR, string sLogId, string flag)
        {
             
            string sql = @"SELECT A.CAVID,A.CHAID,A.GQID,A.ITNBR,A.ITDSC,A.USECOUNT,A.USECOUNT_1,A.DRYICE,B.*
                                              FROM (SELECT * FROM MDD0002 WHERE MCHID='" +
                                                  sMCHID + "' AND LRFLAG='" + sLR + @"') A,
                                              (SELECT C.*,D.ITDSC ITDSCD FROM MDD0003 C,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) D 
                                              WHERE C.ITNBRB=D.ITNBR(+) AND 
                                              C.PLANSTATUS<>'6' AND C.PLANSTATUS<>'5' AND C.CLNDIV = '3') B 
                                              WHERE A.MCHID=B.MCHID(+) AND A.LRFLAG=B.LRFLAG(+)  ";
            //    DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {
                //获取机台模具现况信息 以及计划信息（排除状态为 5-已完成 6-已取消的）
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
                DataTable dcnt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT COUNT(1) FROM LTA0001 WHERE CUMCH = '" +
                                   sMCHID + "' AND LR = '" + sLR + @"' AND CUTIM BETWEEN 
                                TO_DATE(TO_CHAR((SELECT STATIME FROM (SELECT ROWNUM RN,STATIME FROM MDD0001 
                                WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + @"' AND ENDTIME IS NULL) X 
                                WHERE X.RN=1),'YYYY-MM-DD HH24:MI:SS'), 'YYYY-MM-DD HH24:MI:SS') AND SYSDATE", null).Tables[0];
                string cnt = dt.Rows[0]["USECOUNT"].ToString();
                string cnt2 = dt.Rows[0]["USECOUNT_1"].ToString();

                if (flag == "1")
                {

                    ////录入该机台上一次使用模具的下机信息：下机时间、下机原因ECODE=1、使用次数(根据模具上下机时间统计硫化条码个数)
                    string sqll = (@"UPDATE MDD0001 SET ENDTIME = SYSDATE, CNT = '" + cnt +
                       "', ECODE = '2' WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR +
                       "' AND ENDTIME IS NULL");

                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sqll, null);
                    if (iResult > 0)
                        return 0;
                    else
                        return 2;
                }
                else
                {
                    ////录入该机台上一次使用模具的下机信息：下机时间、下机原因ECODE=1、使用次数(根据模具上下机时间统计硫化条码个数)
                    string sqlll = (@"UPDATE MDD0001 SET ENDTIME = SYSDATE  WHERE MCHID = '" + sMCHID + "' AND LR = '" + sLR + "' and  ECODE = '5'" +
                        "  AND ENDTIME IS NULL");

                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sqlll, null);
                    if (iResult > 0)
                        return 0;
                    else
                        return 2;
                    ////有干冰清洗计划就将状态变更为已完成
                    //string sql1 = "UPDATE MDD0003 SET PLANSTATUS = '5',STIM=SYSDATE,SNAM='" + sLogId + "' WHERE PLANID = '" + dt.Rows[0]["PLANID"] + "'";

                    //int iResult = db.ExecuteNonQuery(sql);

                    //int iResult1 = db.ExecuteNonQuery(sql1);

                    //if (iResult > 0 && iResult1 > 0)
                    //    return 0;
                    //else
                    //    return 2;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 2;
            }
        }


        [HttpGet]
        [Route("api/GetMoldPlanInfoByMch1")]
        public DataTable GetMoldPlanInfoByMch1(string sMCHLR, string CLNDIV)
        {
             
            try
            {
                string sql = "SELECT A.*,B.ITDSC FROM MDD0003 A,EDB0010 B WHERE A.ITNBRB=B.ITNBR(+) and A.PLANSTATUS <>'5' and A.PLANSTATUS<>'6' and A.CLNDIV='" + CLNDIV + "' and A.mchid = '" + sMCHLR.Substring(0, 6) + "' and A.LRFLAG = '" + sMCHLR.Substring(6, 1) + "'";
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


        /// <summary>
        /// 获取计划号
        /// </summary>
        public string NextValue(string plantype)
        {
            string sql = @"SELECT * FROM STLOTSEQ WHERE USEDIV='" + plantype + "' AND YMD='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            if (dt.Rows.Count > 0)
            {
                int currentval = 0;
                int nextval = 0;
                if (Convert.ToInt32(dt.Rows[0]["CURRENTVAL"]) == 999)
                    currentval = 1;
                else
                    currentval = Convert.ToInt32(dt.Rows[0]["NEXTVAL"]);
                if (Convert.ToInt32(dt.Rows[0]["NEXTVAL"]) == 999)
                    nextval = 1;
                else
                    nextval = currentval + 1;
                string update = @"UPDATE STLOTSEQ SET CURRENTVAL='" + currentval.ToString("D4") + "',NEXTVAL='" + nextval.ToString("D4") + "' WHERE USEDIV='" + plantype + "' AND YMD='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, update, null);
                return currentval.ToString("D4");
            }
            else
            {
                string insert = @"INSERT INTO STLOTSEQ(ID,FAC,USEDIV,YMD,CURRENTVAL,NEXTVAL) 
                            VALUES(SYS_GUID(),'07','" + plantype + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + 1.ToString("D4") + "','" + 2.ToString("D4") + "')";
                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, insert, null);
                return 1.ToString("D4");
            }
        }

    }
}
