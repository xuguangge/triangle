using System;
using System.Web.Http;
using System.Data;
using log4net.Util;
using HYPDAWebApi.DBHelper;
using System.Collections;
using HYPDAWebApi.Models.ViewModel;
using HYPDAWebApi.App_Data;

namespace HYPDAWebApi.Controllers
{
    /// <summary>
    /// 硫化
    /// </summary>
    public class VulcanizationController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";

        [HttpGet]
        [Route("api/Get_VulInspData")]
        public string Get_VulInspData(string sMchid, string sLR)
        {
            //DataRow shfRow = SHIFT.GetShift(DateTime.Now);
            string sDate = DateTime.Now.ToShortDateString();
            string sResult = string.Empty;
            try
            {
                string sSql = "SELECT AB.MCHID,AB.DIVLR,AB.ITNBR,AB.ITDSC,AB.BARCODE,PRES,ITEMP,PTEMP,JTEMP,STEPNO,SETTM,STEPTM,SPEC,COUNT1 ";
                sSql += "  FROM (  SELECT A.*, ROWNUM RN";
                sSql += "            FROM GY" + sMchid.Substring(2, 4) + " A";
                sSql += "           WHERE     WTIME BETWEEN TO_DATE ('" + sDate + " 00:00:00','YYYY-MM-DD HH24:MI:SS')";
                sSql += "                               AND TO_DATE ('" + sDate + " 23:59:59','YYYY-MM-DD HH24:MI:SS')";
                sSql += "                 AND DIVLR = '" + sLR + "'";
                sSql += "        ORDER BY WTIME DESC) AB,";
                sSql += "       (SELECT *";
                sSql += "          FROM PPE0003@HYMES_PROD";
                sSql += "         WHERE MCHID = '" + sMchid + "' AND LR = '" + sLR + "') AC";
                sSql += " WHERE RN = '1'";
                DataTable dtgy = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString4,CommandType.Text,sSql,null).Tables[0];
                if (dtgy.Rows.Count > 0)
                {
                    sResult = dtgy.Rows[0]["MCHID"].ToString() + "^" +//机台
                                   dtgy.Rows[0]["DIVLR"].ToString() + "^" +//区分左右
                                   dtgy.Rows[0]["ITNBR"].ToString() + "^" +//规格代码
                                   dtgy.Rows[0]["ITDSC"].ToString() + "^" +//规格名称
                                   dtgy.Rows[0]["PRES"].ToString() + "^" +//内压
                                   dtgy.Rows[0]["ITEMP"].ToString() + "^" +//内温
                                   dtgy.Rows[0]["PTEMP"].ToString() + "^" +//模套温度
                                   dtgy.Rows[0]["JTEMP"].ToString() + "^" +//外温
                                   dtgy.Rows[0]["SETTM"].ToString() + "^" +//硫化总时间
                                   dtgy.Rows[0]["SPEC"].ToString() + "^" +//胶囊型号
                                   dtgy.Rows[0]["COUNT1"].ToString() + "^" +//胶囊使用次数
                                   dtgy.Rows[0]["BARCODE"].ToString();//条码
                    return sResult;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/Save_VulInspData")]
        public string Save_VulInspData(string sMchid, string sLR, string sUseid, string sITNBR, string sITDSC, string sUseCnt, string sJN, string sCuTim, string sOutTemp, string sInTemp, string sSteamPres, string sNtgPres, string sBarcode)
        {
            try
            {
                 
                DataRow rRow = SHIFT.GetShift(DateTime.Now);
                string sSql = "INSERT INTO QMA0111 (ID,FAC,INSPDAT,INSPTIM,INSPSHT,INSPUSERID,MCHID,LRFLAG,ITNBR,ITDSC,CSUSECNT,CSTYPE,CUTOTALTIM,OUTTEMP,INTEMP,STEAMPRES,NTGPRES,REMARK,BARCODE) VALUES (";
                sSql += "sys_guid(),'07',";
                sSql += "TO_DATE('" + Convert.ToDateTime(rRow["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), ";
                sSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),";
                sSql += "'" + rRow["WSHT"] + "',";
                sSql += "'" + sUseid + "',";
                sSql += "'" + sMchid + "',";
                sSql += "'" + sLR + "',";
                sSql += "'" + sITNBR + "',";
                sSql += "'" + sITDSC + "',";
                sSql += "" + sUseCnt + ",";
                sSql += "'" + sJN + "',";
                sSql += "" + sCuTim + ",";
                sSql += "" + sOutTemp + ",";
                sSql += "" + sInTemp + ",";
                sSql += "" + sSteamPres + ",";
                sSql += "" + sNtgPres + ",";
                sSql += "'',";
                sSql += "'" + sBarcode + "'";
                sSql += ")";

                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null);
                if (iResult > 0)
                    return "OK";
                else
                    return "Fail-登记失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }
       
        [HttpPost]
        [Route("api/GetNOINFO")]
        public string GetNOINFO(string[] str)
        {
            //0 机台
            //1 区分
            string mess = null;
             
            try
            {
                string strSql = @"SELECT * FROM EQB0003 WHERE MCHID='" + str[0] + "' AND LRFLAG='" + str[1] + "' AND (RSTRTNAM IS NULL OR RSTRTNAM ='') ORDER BY NOSTRTTIM DESC ";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    mess = dt.Rows[0]["LRFLAG"] + "&" + dt.Rows[0]["NOSTRTTIM"] + "&" + dt.Rows[0]["NOSTRTCOD"] + "&" + dt.Rows[0]["NOSTRTRSN"];
                }
                return mess;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return mess;
            }
        }

        //返回值：0员工不存在、 1开机成功、 2不启动代码不存在、 3开机失败、 4处理异常
        [HttpPost]
        [Route("api/GetVulOpen")]
        public int GetVulOpen(string[] str)
        {
            //EQB0002-停机履历查看
            //Employee-员工信息表
            //EQA0003-不启动代码管理
            //EDA0001-设备基本信息

            //0机台号
            //1区分
            //2不启动代码
            //3员工号
            //
            try
            {
                //1.更新设备运行状态 2.更新不启动履历
                 
                DataRow row = SHIFT.GetShift(DateTime.Now);

                //判断员工是否存在
                string sqllogin = "SELECT * FROM LSFW_EMPLOYEE  WHERE FAC='" + FAC + "' AND LOGINNAME = '" + str[3] + "' ";
                DataTable dtemp = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqllogin, null).Tables[0];
                if (dtemp.Rows.Count == 0)
                    return 0;

                //判断不启动代码是否正确
                string sqlpsd = "SELECT * FROM EQA0003 WHERE FAC='" + FAC + "' AND NOSTRTCOD='" + str[2] + "'";
                DataTable dteda = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlpsd, null).Tables[0];
                if (dteda.Rows.Count == 0)
                    return 2;

                //更新履历查看
                string sql = @"UPDATE EQB0003 SET 
			                        RSTRDATE=TO_DATE('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
                                    RSTRTTIM=TO_DATE('" + DateTime.Now.ToString() + @"','yyyy-MM-dd hh24:mi:ss'),
			                        RSTRTNAM='" + dtemp.Rows[0]["NAME"].ToString() + @"'
                           WHERE FAC='" + FAC + "' AND MCHID='" + str[0] + "' AND LRFLAG='" + str[1] + "' AND NOSTRTCOD= '" + str[2] + "' AND (RSTRTNAM IS NULL OR RSTRTNAM ='')";
                //更新设备运行状态
                string sql1 = "UPDATE EDA0007 SET EQPSTATE='0',ENAM='" + dtemp.Rows[0]["NAME"].ToString() + "',ETIM=TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss') WHERE FAC='" + FAC + "' AND MCHID='" + str[0] + "' AND LRFLAG='" + str[1] + "'";
                ArrayList list = new ArrayList();
                list.Add(sql);
                list.Add(sql1);
                int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (i > 0)
                    return 1;
                else
                    return 3;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 4;
            }
        }

        [HttpGet]
        [Route("api/Get_VulInspITNBR")]
        public string Get_VulInspITNBR(string sMchid, string sLR)
        {
             

            string sDate = DateTime.Now.ToShortDateString();
            string sResult = string.Empty;
            try
            {
                string sSql = "SELECT * FROM PAD0401 WHERE DIV = '1' AND MCHID = '" + sMchid + "' AND LR = '" + sLR + "'";

                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    sResult = dt.Rows[0]["MCHID"].ToString() + "^" +//机台
                                   dt.Rows[0]["LR"].ToString() + "^" +//区分左右
                                   dt.Rows[0]["CUITNBR"].ToString() + "^" +//规格代码
                                   dt.Rows[0]["CUITDSC"].ToString();//规格名称

                    return sResult;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err:" + ex.Message;
            }
        }

        /// <summary>
        /// 机台工艺登记数据
        /// </summary>
        /// <param name="sMchid"></param>
        /// <param name="sLR"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Select_LhGyData")]
        public string Select_LhGyData(string sMchid, string sLR)
        {
            try
            {
                 
                DataRow rRow = SHIFT.GetShift(DateTime.Now);
                string seSql = "SELECT * FROM QMA0116 WHERE MCHID = '" + sMchid + "' AND MUCHLR = '" + sLR + "' AND ZJTIME IS NULL";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, seSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["ID"].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/Insert_LhGyData")]
        public string Insert_LhGyData(string itnbr, string itdsc, string mchid, string muchlr, string lhdm, string outtem, string jnitnbr, string jnhigh, string hml, string zqp, string step3, string step8, string czk, string alltime, string gyyname)
        {
            try
            {
                 
                DateTime dtNow = DateTime.Now;
                DataRow rRow = SHIFT.GetShift(DateTime.Now);
                string sSql = "INSERT INTO QMA0116 (ID,ITNBR,ITDSC,MCHID,MUCHLR,LHDM,OUTTEM,JNITNBR,JNHIGH,HML,ZQP,STEP3,STEP8,CZK,ALLTIME,GYYNAME,GYYTIME) VALUES (";
                sSql += "sys_guid(),'" + itnbr + "','" + itdsc + "','" + mchid + "','" + muchlr + "','" + lhdm + "','" + outtem + "',  ";
                sSql += " '" + jnitnbr + "','" + jnhigh + "','" + hml + "','" + zqp + "','" + step3 + "','" + step8 + "','" + czk + "','" + alltime + "','" + gyyname + "',TO_DATE('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss') ";
                sSql += ")";

                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null);
                if (iResult > 0)
                    return "OK";
                else
                    return "Fail-登记失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }


        [HttpGet]
        [Route("api/Update_LhGyData")]
        public string Update_LhGyData(string id, string itnbr, string itdsc, string lhdm, string outtem, string jnitnbr, string jnhigh, string hml, string zqp, string step3, string step8, string czk, string alltime, string gyyname)
        {
            try
            {
                 
                DateTime dtNow = DateTime.Now;
                DataRow rRow = SHIFT.GetShift(DateTime.Now);
                string seSql = "UPDATE QMA0116 SET ITNBR = '" + itnbr + "',ITDSC = '" + itdsc + "',LHDM = '" + lhdm + "',OUTTEM = '" + outtem + "',JNITNBR = '" + jnitnbr + "',  ";
                seSql += " JNHIGH = '" + jnhigh + "',HML = '" + hml + "',ZQP = '" + zqp + "',STEP3 = '" + step3 + "',STEP8 = '" + step8 + "',CZK = '" + czk + "',ALLTIME = '" + alltime + "',GYYNAME = '" + gyyname + "',GYYTIME = TO_DATE('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";
                seSql += "    WHERE ID = '" + id + "'";

                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, seSql, null);
                if (iResult > 0)
                    return "OK";
                else
                    return "Fail-更新失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/GetRoutingDataByBarcode")]
        public string GetRoutingDataByBarcode(string sBarcode)
        {
             
            string sResult = string.Empty;
            string sITDSC = string.Empty;
            string sTRIM = string.Empty;
            string sUFDB = string.Empty;
            string sXRAY = string.Empty;
            string sSDS = string.Empty;
            try
            {
                string sql = " SELECT * ";
                sql += "   FROM (SELECT A.ID,A.FAC,A.BARCODE,A.ITNBR,A.TRIMMINGMACHINE,A.TUGBMACHINE,A.XRAYMACHINE,A.SDSMACHINE,A.TIMSTAMP,A.XRAYREMARK,A.SDSREMARK,B.ITDSC, ";
                sql += "   ROW_NUMBER () ";
                sql += "   OVER (PARTITION BY BARCODE ORDER BY TIMSTAMP DESC) RN  ";
                sql += "   FROM CKA0003 A,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) B ";
                sql += "   WHERE A.ITNBR = B.ITNBR AND BARCODE = '" + sBarcode + "' ) ";
                sql += "  WHERE RN = 1 ";

                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];

                if (dt.Rows.Count > 0)
                {
                    sITDSC = dt.Rows[0]["ITDSC"].ToString();
                    sTRIM = dt.Rows[0]["TRIMMINGMACHINE"].ToString();
                    sUFDB = dt.Rows[0]["TUGBMACHINE"].ToString();
                    sXRAY = dt.Rows[0]["XRAYMACHINE"].ToString();
                    sSDS = dt.Rows[0]["SDSMACHINE"].ToString();
                    sResult = sITDSC + "^" + sTRIM + "^" + sUFDB + "^" + sXRAY + "^" + sSDS;
                    return sResult;
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err-" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/GetCurrentMoldInfo")]
        public DataTable GetCurrentMoldInfo(string sMchid, string sLR)
        {
             
            string sSql = "SELECT CAVID,CHAID,GQCOD,GQID FROM MDD0002 WHERE MCHID = '" + sMchid + "' AND LRFLAG  = '" + sLR + "' ";
            DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
            return dt;
        }

        [HttpGet]
        [Route("api/LoadFirstBarCodeInfo")]
        public DataTable LoadFirstBarCodeInfo(string sBarcode)
        {
             
            //GYConfig gycon = new GYConfig();
            try
            {
                string sSql = "SELECT * FROM LTA0001 WHERE BARCODE = '" + sBarcode + "'";
                string sGYSql = string.Empty;
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    string sSETTM = string.Empty;
                    dt.Columns.Add("SETTM");
                    string sTrimMch = string.Empty;
                    dt.Columns.Add("PRE_TRIMMCH");
                    if (!string.IsNullOrEmpty(dt.Rows[0]["CUMCH"].ToString()))
                    {
                        string sTab = "GY" + dt.Rows[0]["CUMCH"].ToString().Substring(2, 4);
                        sGYSql = " select distinct SETTM from " + sTab + " where barcode = '" + sBarcode + "'";
                        DataTable dtgy = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString4, CommandType.Text, sGYSql, null).Tables[0];
                        if (dtgy != null && dtgy.Rows.Count > 0)
                            sSETTM = dtgy.Rows[0]["SETTM"].ToString();
                    }
                    dt.Rows[0]["SETTM"] = sSETTM;

                    string sSqlTrim = "SELECT TRIMMINGMACHINE FROM CKA0003 WHERE BARCODE = '" + sBarcode + "' ORDER BY TIMSTAMP DESC";
                    DataTable dtTrim = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlTrim, null).Tables[0];
                    if (dtTrim != null && dtTrim.Rows.Count > 0)
                        sTrimMch = dtTrim.Rows[0]["TRIMMINGMACHINE"].ToString();

                    dt.Rows[0]["PRE_TRIMMCH"] = sTrimMch;
                }
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        /// 判断是否入库
        /// </summary>
        /// <param name="sBarcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetBarCodeInfo")]
        public DataTable GetBarCodeInfo(string sBarcode)
        {
             

            try
            {
                string strSql = "SELECT * FROM LTA0001 WHERE BARCODE ='" + sBarcode + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpPost]
        [Route("api/RegistEndCheckInfo")]
        public string RegistEndCheckInfo(Regist regist)
        {
             
            DataRow row = SHIFT.GetShift(DateTime.Now);
            ArrayList sqllist = new ArrayList();
            try
            {
                string sSqlchk = "SELECT COUNT(*) CNT FROM QMA0206 WHERE BARCODE = '" + regist.sBarcode + "'";
                DataTable dtchk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlchk, null).Tables[0];
                if (dtchk.Rows.Count > 0)
                {
                    if (int.Parse(dtchk.Rows[0]["CNT"].ToString()) > 0)
                        return "-2";
                }

                string sPYN = "N";
                string sPRES = string.Empty;
                DateTime dPDAT = DateTime.MinValue;
                DateTime dPTIM = DateTime.MinValue;
                string sPNAM = string.Empty;
                string sPSHT = string.Empty;
                string sPBAN = string.Empty;
                foreach (string sItem in regist.sCheckItems)
                {

                    if (sItem.Contains("NG"))
                    {
                        sPYN = "Y";
                        sPRES = "B";
                        dPDAT = Convert.ToDateTime(row["WDATE"].ToString());
                        dPTIM = System.DateTime.Now;
                        sPNAM = regist.sDNAM;
                        sPSHT = row["WSHT"].ToString();
                        sPBAN = row["WBAN"].ToString();
                        break;
                    }
                }
                DataTable dt = this.LoadFirstBarCodeInfo(regist.sBarcode);
                if (dt.Rows.Count > 0 && dt != null)
                {
                    Guid sID = Guid.NewGuid();
                    string sSql = "INSERT INTO QMA0206 (ID,FAC,CUDAT,CUTIM,MCHID,LR,MODID,TAOID,BUITNBR,BUITDSC,CUITNBR,CUITDSC,PYN,PRES,PTIM,PDAT,PNAM,PSHT,PBAN,DIV,BARCODE,REMARK,CUTOTIM,WKCOD,HEMOLI,PLANID,DTIM,DDAT,DNAM,DSHT,DBAN,GQCOD) VALUES(";
                    sSql += "'" + sID + "',";
                    sSql += "'07',";
                    sSql += "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUDAT"].ToString()).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql += "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUTIM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql += "'" + dt.Rows[0]["CUMCH"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["LR"].ToString() + "',";
                    sSql += "'" + regist.sModID + "',";
                    sSql += "'" + regist.sTaoID + "',";
                    sSql += "'" + dt.Rows[0]["BUITNBR"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["BUITDSC"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["CUITNBR"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["CUITDSC"].ToString() + "',";
                    sSql += "'" + sPYN + "',";
                    sSql += "'" + sPRES + "',";
                    if (dPTIM == DateTime.MinValue)
                        sSql += "null,";
                    else
                        sSql += "to_date('" + dPTIM.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    if (dPDAT == DateTime.MinValue)
                        sSql += "null,";
                    else
                        sSql += "to_date('" + dPDAT.ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql += "'" + sPNAM + "',";
                    sSql += "'" + sPSHT + "',";
                    sSql += "'" + sPBAN + "',";
                    sSql += "'" + regist.sDIV + "',";
                    sSql += "'" + regist.sBarcode + "',";
                    sSql += "'" + regist.sREMARK + "',";
                    sSql += "'" + regist.sCUTOTIM + "',";
                    sSql += "'" + regist.sWKCOD + "',";
                    sSql += "'" + regist.sHEMOLI + "',";
                    sSql += "'" + regist.sPLANID + "',";
                    sSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql += "to_date('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql += "'" + regist.sDNAM + "',";
                    sSql += "'" + row["WSHT"].ToString() + "',";
                    sSql += "'" + row["WBAN"].ToString() + "',";
                    sSql += "'" + regist.sQuanID + "'";
                    sSql += ")";
                    sqllist.Add(sSql);

                    foreach (string sItem in regist.sCheckItems)
                    {
                        string[] sitem = sItem.Split('^');
                        string sql1 = "INSERT INTO QMA0207 (ID,SID,DIV,SMNAM,CHKRESULT,CHKMANDIV) VALUES (";
                        sql1 += "sys_guid(),";
                        sql1 += "'" + sID + "',";
                        sql1 += "'" + regist.sDIV + "',";
                        sql1 += "'" + sitem[0] + "',";
                        sql1 += "'" + sitem[1] + "',";
                        sql1 += "'1'";
                        sql1 += ")";
                        sqllist.Add(sql1);
                    }
                    int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sqllist);
                    if (j > 0)
                        return "OK";
                    else
                        return "-1";
                }
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "-1";
            }
        }

        /// <summary>
        /// 外检判定查询
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Get_JUDGE")]
        public DataTable Get_JUDGE(string code, string rway)
        {
             
            DataRow ROW = SHIFT.GetShift(DateTime.Now);
            try
            {
                string sql = string.Empty;
                if (!string.IsNullOrEmpty(rway))
                {
                    sql = @"SELECT A.*,QM.BCOD||'^'||QMA.BNAM BDESC
                                      FROM LTA0001 A,
                                           (SELECT *
                                              FROM (SELECT A.*,
                                                           ROW_NUMBER ()
                                                              OVER (PARTITION BY A.BARCODE ORDER BY RTIM DESC)
                                                              RN
                                                      FROM QMA0103 A
                                                     WHERE DIV = '1' AND RWAY = '" + rway + @"')
                                             WHERE RN = '1') QM,QMA0001 QMA
                                     WHERE A.BARCODE = QM.BARCODE(+)  and QM.BCOD = QMA.BCOD(+) AND A.BARCODE='" + code + "'";
                }
                else
                {
                    sql = @"SELECT A.*,QM.BCOD||'^'||QMA.BNAM BDESC
                                      FROM LTA0001 A,
                                           (SELECT *
                                              FROM (SELECT A.*,
                                                           ROW_NUMBER ()
                                                              OVER (PARTITION BY A.BARCODE ORDER BY RTIM DESC)
                                                              RN
                                                      FROM QMA0103 A
                                                     WHERE DIV = '1')
                                             WHERE RN = '1') QM,QMA0001 QMA
                                     WHERE A.BARCODE = QM.BARCODE(+)  and QM.BCOD = QMA.BCOD(+) AND A.BARCODE='" + code + "'";
                }
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return new DataTable();
            }
        }

        /// <summary>
        /// 人工打磨登记，登记修理日期、修理班组、修理班次、修理人、修理时间
        /// 2017-12-23 JOE
        /// </summary>
        /// <param name="BARCODE">打磨扫描条码</param>
        /// <param name="DIV">种类区分</param>
        /// <param name="WBAN">修理班组</param>
        /// <param name="USERNAME">修理人工号</param>
        /// <param name="USERNAME">修理深度</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/A_GrindRepair_Regist")]
        public string A_GrindRepair_Regist(string BARCODE, string DIV, string WBAN, string USERNAME)
        {
             
            DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {
                //判断成品胎不良登记表中是否存在该条码
                string QMA0101_sql = @"SELECT * FROM QMA0101 WHERE FAC='" + FAC + "' AND DIV='" + DIV + "' AND BARCODE='" + BARCODE + "'";
                DataTable dt_QMA0101 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, QMA0101_sql, null).Tables[0];
                if (dt_QMA0101.Rows.Count == 0)
                    return "1";

                //判断成品胎修理履历表中是否存在该条码,以及是否质检有最新判定（必须有质检最新判定且没被修理过）
                string QMA0103_sql = @"SELECT * FROM QMA0103 WHERE FAC='" + FAC + "' AND DIV='" + DIV + "' AND BARCODE='" + BARCODE + "' AND RWAY = '2' AND PIDAT IS NULL";//打磨区分
                DataTable dt_QMA0103 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, QMA0103_sql, null).Tables[0];
                if (dt_QMA0103.Rows.Count == 0)
                    return "2";

                ArrayList list = new ArrayList();
                //根据BARCODE更新成品胎不良登记表中修理信息
                string QMA0101_update = @"UPDATE QMA0101 SET PIDAT=TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd')," +
                                        @"                   PIBAN='" + WBAN + "'," +
                                        @"                   PISHT='" + row["WSHT"].ToString() + "'," +
                                        @"                   PINAM='" + USERNAME + "'," +
                                        @"                   PITIM=TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd HH24:mi:ss') " +
                                        @"             WHERE FAC='" + FAC + "' " +
                                        @"               AND DIV='" + DIV + "' " +
                                        @"               AND BARCODE='" + BARCODE + "'";
                list.Add(QMA0101_update);

                //根据BARCODE更新成品胎修理履历表中修理信息
                string QMA0103_update = @"UPDATE QMA0103 SET PIDAT=TO_DATE('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd')," +
                                        @"                   PIBAN='" + WBAN + "'," +
                                        @"                   PISHT='" + row["WSHT"].ToString() + "'," +
                                        @"                   PINAM='" + USERNAME + "'," +
                                        @"                   PITIM=TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd HH24:mi:ss') " +
                                        @"             WHERE FAC='" + FAC + "' " +
                                        @"               AND DIV='" + DIV + "' " +
                                        @"               AND BARCODE='" + BARCODE + "' AND RWAY = '2'";
                list.Add(QMA0103_update);

                OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                return "3";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "4";
            }
        }

        [HttpGet]
        [Route("api/GetModPlanCod")]
        public DataTable GetModPlanCod(string sMchid, string sLR)
        {
             
            DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {
                //string sql = "select PLANID from MDD0003 where PLANSTATUS in ('3','4') and wdate = to_date('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD')  and mchid = '" + sMchid + "' and LRFLAG = '" + sLR + "'order by PLANID";
                string sql = "select PLANID from MDD0003 where PLANSTATUS in ('3','4')  and mchid = '" + sMchid + "' and LRFLAG = '" + sLR + "'order by PLANID";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/UpdatePlanCodING")]
        public int UpdatePlanCodING(string sPlanCod)
        {
             
            try
            {
                string sql = "update MDD0003 set PLANSTATUS = '4' where planid = '" + sPlanCod + "'";
                int iRes = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sql, null);
                return iRes;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return -1;
            }
        }

        [HttpGet]
        [Route("api/LoadPlanInfo")]
        public string LoadPlanInfo(string sPlanCod)
        {
             
            try
            {
                string sResult = string.Empty;
                string sql = "select * from MDD0003 where planid = '" + sPlanCod + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    sResult = dt.Rows[0]["PLANSTATUS"].ToString() + "^" + dt.Rows[0]["MCHID"].ToString() + "^" + dt.Rows[0]["LRFLAG"].ToString() + "^" +
                                    dt.Rows[0]["MODID"].ToString() + "^" + dt.Rows[0]["MODNAM"].ToString() + "^" + dt.Rows[0]["MODPATTERN"].ToString() + "^" +
                                    dt.Rows[0]["TAOID"].ToString() + "^" + dt.Rows[0]["TAOCOD"].ToString() + "^" + dt.Rows[0]["REMARK"].ToString() + "^" +
                                     dt.Rows[0]["GQCOD"].ToString() + "^" + dt.Rows[0]["ITNBRB"].ToString() + "^" + dt.Rows[0]["CLNDIV"].ToString();
                    return sResult;
                }
                return "";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "";
            }
        }

        [HttpGet]
        [Route("api/BarcodeItnbrSameOrNot")]
        public bool BarcodeItnbrSameOrNot(string sBarcode, string sMCHID, string sLR)
        {
             
            DataRow ROW = SHIFT.GetShift(DateTime.Now);
            bool bSame = true;
            string sITNBR = string.Empty;
            string sITNBR1 = string.Empty;
            try
            {
                string sSql = "SELECT CUITNBR FROM LTA0001 WHERE BARCODE = '" + sBarcode + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                    sITNBR = dt.Rows[0]["CUITNBR"].ToString();

                //string sSql1 = "SELECT CUITNBR FROM PAD0401 WHERE WDATE =TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd') ";
                //sSql1 += " AND WSHT = '" + ROW["WSHT"] + "'";
                //sSql1 += " AND MCHID = '" + sMCHID + "'";
                //sSql1 += " AND LR = '" + sLR + "'";
                //sSql1 += " AND DIV = '1' ";
                //只判断当前正在生产的规格，和日期无关 2019-01-28 JOE
                string sSql1 = "SELECT CUITNBR FROM PAD0401 ";
                sSql1 += " WHERE MCHID = '" + sMCHID + "'";
                sSql1 += " AND LR = '" + sLR + "'";
                sSql1 += " AND DIV = '1' ORDER BY WDATE DESC";
                DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                if (dt1.Rows.Count > 0 && dt1 != null)
                    sITNBR1 = dt1.Rows[0]["CUITNBR"].ToString();

                if (sITNBR != sITNBR1)
                    bSame = false;

                return bSame;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return false;
            }
        }

        [HttpGet]
        [Route("api/BarcodeItnbrSameOrNotWithMoldITNBR")]
        public bool BarcodeItnbrSameOrNotWithMoldITNBR(string sCUITNBR, string sPLANID)
        {
             
            try
            {
                bool bSame = true;
                string sPlanItnbr = string.Empty;
                string sSql = "SELECT ITNBRB FROM MDD0003 WHERE PLANID = '" + sPLANID + "'";
                DataTable dtSql = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dtSql != null && dtSql.Rows.Count > 0)
                    sPlanItnbr = dtSql.Rows[0]["ITNBRB"].ToString();

                if (sCUITNBR.Contains("_"))
                    sCUITNBR = sCUITNBR.Substring(0, sCUITNBR.IndexOf('_'));

                if (sCUITNBR != sPlanItnbr)
                    bSame = false;
                return bSame;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return false;
            }
        }

        [HttpPost]
        [Route("api/RegistFirstCheckInfo")]
        public string RegistFirstCheckInfo(Regist regist)
        {
             
            DataRow row = SHIFT.GetShift(DateTime.Now);
            ArrayList sqllist = new ArrayList();
            try
            {
                string sSqlchk = "SELECT COUNT(*) CNT FROM QMA0106 WHERE BARCODE = '" + regist.sBarcode + "'";
                DataTable dtchk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlchk, null).Tables[0];
                if (dtchk.Rows.Count > 0)
                {
                    if (int.Parse(dtchk.Rows[0]["CNT"].ToString()) > 0)
                        return "-2";
                }

                string sPYN = "N";
                string sPRES = string.Empty;
                DateTime dPDAT = DateTime.MinValue;
                DateTime dPTIM = DateTime.MinValue;
                string sPNAM = string.Empty;
                string sPSHT = string.Empty;
                string sPBAN = string.Empty;
                foreach (string sItem in regist.sCheckItems)
                {

                    if (sItem.Contains("NG"))
                    {
                        sPYN = "Y";
                        sPRES = "B";
                        dPDAT = Convert.ToDateTime(row["WDATE"].ToString());
                        dPTIM = System.DateTime.Now;
                        sPNAM = regist.sDNAM;
                        sPSHT = row["WSHT"].ToString();
                        sPBAN = row["WBAN"].ToString();
                        break;
                    }
                }
                DataTable dt = this.LoadFirstBarCodeInfo(regist.sBarcode);
                if (dt.Rows.Count > 0 && dt != null)
                {
                    Guid sID = Guid.NewGuid();
                    string sSql = "INSERT INTO QMA0106 (ID,FAC,CUDAT,CUTIM,MCHID,LR,MODID,TAOID,BUITNBR,BUITDSC,CUITNBR,CUITDSC,PYN,PRES,PTIM,PDAT,PNAM,PSHT,PBAN,DIV,BARCODE,REMARK,CUTOTIM,WKCOD,HEMOLI,PLANID,DTIM,DDAT,DNAM,DSHT,DBAN,GQCOD) VALUES(";
                    sSql += "'" + sID + "',";
                    sSql += "'07',";
                    sSql += "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUDAT"].ToString()).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql += "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUTIM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql += "'" + dt.Rows[0]["CUMCH"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["LR"].ToString() + "',";
                    sSql += "'" + regist.sModID + "',";
                    sSql += "'" + regist.sTaoID + "',";
                    sSql += "'" + dt.Rows[0]["BUITNBR"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["BUITDSC"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["CUITNBR"].ToString() + "',";
                    sSql += "'" + dt.Rows[0]["CUITDSC"].ToString() + "',";
                    sSql += "'" + sPYN + "',";
                    sSql += "'" + sPRES + "',";
                    if (dPTIM == DateTime.MinValue)
                        sSql += "null,";
                    else
                        sSql += "to_date('" + dPTIM.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    if (dPDAT == DateTime.MinValue)
                        sSql += "null,";
                    else
                        sSql += "to_date('" + dPDAT.ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql += "'" + sPNAM + "',";
                    sSql += "'" + sPSHT + "',";
                    sSql += "'" + sPBAN + "',";
                    sSql += "'" + regist.sDIV + "',";
                    sSql += "'" + regist.sBarcode + "',";
                    sSql += "'" + regist.sREMARK + "',";
                    sSql += "'" + regist.sCUTOTIM + "',";
                    sSql += "'" + regist.sWKCOD + "',";
                    sSql += "'" + regist.sHEMOLI + "',";
                    sSql += "'" + regist.sPLANID + "',";
                    sSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql += "to_date('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql += "'" + regist.sDNAM + "',";
                    sSql += "'" + row["WSHT"].ToString() + "',";
                    sSql += "'" + row["WBAN"].ToString() + "',";
                    sSql += "'" + regist.sQuanID + "'";
                    sSql += ")";
                    sqllist.Add(sSql);

                    foreach (string sItem in regist.sCheckItems)
                    {
                        string[] sitem = sItem.Split('^');
                        string sql1 = "INSERT INTO QMA0107 (ID,SID,DIV,SMNAM,CHKRESULT,CHKMANDIV) VALUES (";
                        sql1 += "sys_guid(),";
                        sql1 += "'" + sID + "',";
                        sql1 += "'" + regist.sDIV + "',";
                        sql1 += "'" + sitem[0] + "',";
                        sql1 += "'" + sitem[1] + "',";
                        sql1 += "'1'";
                        sql1 += ")";
                        sqllist.Add(sql1);
                    }
                    int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sqllist);
                    if (j > 0)
                    {
                        //登记完成后发送<1-供给暂停>信息
                        string sIFSql = "INSERT INTO IF_WMS_GT_11 (MCHID,ITNBR,ITDSC,GTITNBR,LRFLAG,CTLDIV,RCV_FLAG,ENT_USER_ID,ENT_DT) VALUES (";
                        sIFSql += "'" + dt.Rows[0]["CUMCH"].ToString() + "',";
                        sIFSql += "'" + dt.Rows[0]["CUITNBR"].ToString() + "',";
                        sIFSql += "'" + dt.Rows[0]["CUITDSC"].ToString() + "',";
                        sIFSql += "'" + dt.Rows[0]["BUITNBR"].ToString() + "',";
                        sIFSql += "'" + dt.Rows[0]["LR"].ToString() + "',";
                        sIFSql += "'1',";//1-供给暂停
                        sIFSql += "'N',";
                        sIFSql += "'PDA',";
                        sIFSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS')";
                        sIFSql += ")";
                        
                        int k = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString2, CommandType.Text, sIFSql, null);
                        if (k > 0)
                            return "OK";
                        else
                            return "-1";
                    }
                    else
                        return "-1";
                }
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "-1";
            }
        }

        [HttpGet]
        [Route("api/GetGTDetial")]
        public string GetGTDetial(string sBarcode)
        {
            string sGTDetail = string.Empty;
             
            try
            {
                //string sSql = "select * from LTA0001 where barcode ='" + sBarcode + "'";
                string sSql = "SELECT * FROM LTA0001 A,STG0002 B";
                sSql += " WHERE A.BARCODE = B.BARCODE(+)";
                sSql += " AND A.BARCODE ='" + sBarcode + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    sGTDetail = dt.Rows[0]["BUITNBR"].ToString() + "^" + dt.Rows[0]["BUITDSC"].ToString() + "^" + dt.Rows[0]["BUTIM"].ToString() + "^" + dt.Rows[0]["BUSHT"].ToString() + "^" +
                                dt.Rows[0]["BUBAN"].ToString() + "^" + dt.Rows[0]["BUMCH"].ToString() + "^" + dt.Rows[0]["BUNAM"].ToString() + "^" + dt.Rows[0]["STATE"].ToString() + "^" +
                                dt.Rows[0]["OUTDIV"].ToString();
                    return "OK@" + sGTDetail;
                }
                else
                {
                    return "OK@NO DETAIL!";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err-" + ex.Message + "";
            }
        }

        [HttpGet]
        [Route("api/GetGTBadCode")]
        public DataSet GetGTBadCode()
        {
             
            try
            {
                DataSet ds = new DataSet();
                string sql = "select * from QMB0001 where useyn = 'Y' order by bcod";
                ds = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null);
                return ds;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpPost]
        [Route("api/RegistGTBad")]
        public string RegistGTBad(string[] str)
        {
            //0条码
            //1工号
            //2不良代码
            //string[] str = { s1, s2, s3 };

             
            
            DataRow ROW = SHIFT.GetShift(DateTime.Now);

            string mchid = "";//机台
            string lrflag = "";//左右

            try
            {
                //
                string strSTX0002 = "SELECT * FROM STX0002 WHERE BARCODE='" + str[0] + "'";
                DataTable dtSTX0002 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSTX0002, null).Tables[0];
                if (dtSTX0002.Rows.Count == 1)
                {
                    mchid = dtSTX0002.Rows[0]["MCHID"].ToString();
                    lrflag = dtSTX0002.Rows[0]["LRFLAG"].ToString();
                    //返回值：0员工不存在、 1移除成功、 2移除失败、 3硫化实绩不存在此机台区分的信息、 4处理异常
                    int vul = VulSTX(str[0], mchid, lrflag, str[1], "胎胚外观不良");
                    if (vul != 1)
                        return vul.ToString();
                }
                else if (dtSTX0002.Rows.Count > 1)
                    return "1";
                else
                { }
                //
                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[1] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];

                //验证条码是否存在表QMB0101
                string[] strQMB0101 = { "QMB0101", str[0], "AYN <> 'A'" };
                DataTable dtQMB0101 = GetExist(strQMB0101);
                if (dtQMB0101 != null && dtQMB0101.Rows.Count > 0)
                {
                    if (dtQMB0101.Rows[0]["AYN"].ToString() == "B")
                        return "5";//不良
                    else
                        return "51";//报废
                }
                //验证条码是否存在表QMB0104
                string[] strQMB0104 = { "QMB0104", str[0], "DIYN='N'" };
                DataTable dtQMB0104 = GetExist(strQMB0104);
                if (dtQMB0104 != null && dtQMB0104.Rows.Count > 0)
                    return "6";
                //
                string strLTA0001 = @" SELECT * FROM LTA0001 " +
                                   @" WHERE BARCODE='" + str[0] + "' ";
                DataTable dtLTA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strLTA0001, null).Tables[0];
                if (dtLTA0001.Rows.Count == 0)
                    return "7";
                //
                //string sqlQMB0104 = @"INSERT INTO QMB0104 " +
                //    @"(ID,FAC,BARCODE, " +
                //    @"HODAT, " +
                //    @"HOTIM, " +
                //    @"HOSHT,HOBAN,HONAM,BUMCH, " +
                //    @"BUDAT, " +
                //    @"BUTIM, " +
                //    @"BUSHT,BUBAN, " +
                //    @"BUNAM, " +
                //    @"BUITNBR,BUITDSC, " +
                //    @"HOREN,DIYN) VALUES(" +
                //   @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
                //   @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                //   @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'), " +
                //   @"'" + ROW["WSHT"] + "','" + ROW["WBAN"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "','" + dtLTA0001.Rows[0]["BUMCH"].ToString() + "'," +
                //   @"TO_DATE('" + Convert.ToDateTime(dtLTA0001.Rows[0]["BUDAT"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                //   @"TO_DATE('" + Convert.ToDateTime(dtLTA0001.Rows[0]["BUTIM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'), " +
                //   @"'" + dtLTA0001.Rows[0]["BUSHT"].ToString() + "','" + dtLTA0001.Rows[0]["BUBAN"].ToString() + "'," +
                //   @"'" + dtLTA0001.Rows[0]["BUNAM"].ToString() + "', " +
                //   @"'" + dtLTA0001.Rows[0]["BUITNBR"].ToString() + "','" + dtLTA0001.Rows[0]["BUITDSC"].ToString() + "', " +
                //   @"'PDA','N')";

                string sqlif = "INSERT INTO IF_WMS_GT_05 " +
                    @"(BARCODE,ITNBR,ITDSC, " +
                    @"WDATE, " +
                    @"WTIM, " +
                    @"WSHT,WBAN," +
                    @"USERID,ITMSTATUS,RCV_FLAG,JUDGUSERID," +
                    @"JUDGDT,ENT_USER_ID, " +
                    @"ENT_DT,UPD_USER_ID, " +
                    @"UPD_DT) VALUES (" +
                    @"'" + str[0] + "','" + dtLTA0001.Rows[0]["BUITNBR"].ToString() + "','" + dtLTA0001.Rows[0]["BUITDSC"].ToString() + "', " +
                    @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                    @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'" + ROW["WSHT"] + "','" + ROW["WBAN"] + "'," +
                    @"'" + dtLTA0001.Rows[0]["BUNAM"].ToString() + "','B','N','" + dtEMP.Rows[0]["NAME"].ToString() + "', " +
                    @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dtEMP.Rows[0]["NAME"].ToString() + "', " +
                    @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dtEMP.Rows[0]["NAME"].ToString() + "', " +
                    @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'))";

                string sChkQMB0101 = "SELECT COUNT(*) CNT FROM QMB0101 WHERE DIV = '1' AND BARCODE = '" + str[0] + "'";
                DataTable dtChkQMB0101 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sChkQMB0101, null).Tables[0];
                string sUpdQMB0101 = string.Empty;
                if (int.Parse(dtChkQMB0101.Rows[0]["CNT"].ToString()) > 0)//若QMB0101表中存在该barcode的外观不良记录
                {
                    sUpdQMB0101 += "UPDATE QMB0101 SET ";
                    sUpdQMB0101 += "AYN = 'B',";
                    sUpdQMB0101 += "IDAT = to_date('" + Convert.ToDateTime(ROW["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), ";
                    sUpdQMB0101 += "IBAN = '" + ROW["WBAN"].ToString() + "',";
                    sUpdQMB0101 += "ISHT = '" + ROW["WSHT"].ToString() + "',";
                    sUpdQMB0101 += "INAM = '" + str[1] + "',";
                    sUpdQMB0101 += "ITIM =  to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd HH24:MI:SS'), ";
                    sUpdQMB0101 += "COD='" + str[2] + "',";
                    sUpdQMB0101 += "PYN='N'";
                    sUpdQMB0101 += " WHERE DIV = '1' AND BARCODE = '" + str[0] + "'";
                }
                else//没有则插入
                {
                    sUpdQMB0101 += "INSERT INTO QMB0101(ID,FAC,DIV,AYN,BARCODE,IDAT,IBAN,ISHT,INAM,ITIM,COD,PYN,BUITNBR,BUITDSC,STATE,BUMCH,BUDAT,BUTIM,BUSHT,BUBAN,BUNAM) VALUES(";
                    sUpdQMB0101 += "sys_guid(),";
                    sUpdQMB0101 += "'07',";
                    sUpdQMB0101 += "'1',";
                    sUpdQMB0101 += "'B',";
                    sUpdQMB0101 += "'" + str[0] + "',";
                    sUpdQMB0101 += "to_date('" + Convert.ToDateTime(ROW["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
                    sUpdQMB0101 += "'" + ROW["WBAN"].ToString() + "',";
                    sUpdQMB0101 += "'" + ROW["WSHT"].ToString() + "',";
                    sUpdQMB0101 += "'" + str[1] + "',";
                    sUpdQMB0101 += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd HH24:MI:SS'),";
                    sUpdQMB0101 += "'" + str[2] + "',";
                    sUpdQMB0101 += "'N',";
                    sUpdQMB0101 += "'" + dtLTA0001.Rows[0]["BUITNBR"].ToString() + "',";
                    sUpdQMB0101 += "'" + dtLTA0001.Rows[0]["BUITDSC"].ToString() + "',";
                    sUpdQMB0101 += "'" + dtLTA0001.Rows[0]["STATE"].ToString() + "',";
                    sUpdQMB0101 += "'" + dtLTA0001.Rows[0]["BUMCH"].ToString() + "',";
                    sUpdQMB0101 += "TO_DATE('" + Convert.ToDateTime(dtLTA0001.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
                    sUpdQMB0101 += "TO_DATE('" + Convert.ToDateTime(dtLTA0001.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd HH24:mi:ss'),";
                    sUpdQMB0101 += "'" + dtLTA0001.Rows[0]["BUSHT"].ToString() + "',";
                    sUpdQMB0101 += "'" + dtLTA0001.Rows[0]["BUBAN"].ToString() + "',";
                    sUpdQMB0101 += "'" + dtLTA0001.Rows[0]["BUNAM"].ToString() + "'";
                    sUpdQMB0101 += ")";
                }

                ArrayList list = new ArrayList();
                list.Add(sUpdQMB0101);
                int j = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString2, CommandType.Text, sqlif, null);

                if (j > 0)
                {
                    int k = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                    string sqlQmb = "SELECT * FROM QMB0101 WHERE BARCODE='" + str[0] + "' ";
                    DataTable dtsqlQmb = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlQmb, null).Tables[0];
                    if (dtLTA0001 != null && dtLTA0001.Rows.Count > 0 && dtsqlQmb != null && dtsqlQmb.Rows.Count > 0)
                    {
                        //更新lta0001表的胎胚状态为不良状态
                        string sqlUpdate = " UPDATE LTA0001 SET GTSTA='2' WHERE BARCODE = '" + str[0] + "'";
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sqlUpdate, null);
                    }


                    return "8";
                }
                else
                    return "9";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "4";
            }
        }

        //返回值：0员工不存在、 1移除成功、 2移除失败、 3硫化实绩不存在此机台区分的信息、 4处理异常 
        [HttpGet]
        [Route("api/VulSTX")]
        public int VulSTX(string BARCODE, string MCHID, string LRFLAG, string Name, string sReason)
        {
            try
            {
                 
                
                string serno = "";//作业指示编号（工单号）
                int ctlqty = 0; //供给数量（胎胚计划量）
                int gtreqty = 0; //(胎胚已送量)
                string itnbr = ""; //硫化产品代码(硫化规格代码)
                string itdsc = ""; //硫化产品名称(硫化规格名称)
                string gtitnbr = ""; //成型(胎胚)规格代码(胎胚规格代码)

                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + Name + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return 0;

                string sql_Del = "DELETE FROM STX0002 WHERE BARCODE = '" + BARCODE + "'";
                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC,REMARK)
                             VALUES('" + System.Guid.NewGuid() + "', '" + FAC + "', '" + MCHID + "', '" + LRFLAG + "', '" + BARCODE + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'0', '移除','" + sReason + "')";
                //
                string strSqlstx3 = "SELECT * FROM STX0003 WHERE MCHID='" + MCHID + "' AND LR='" + LRFLAG + "'";
                DataTable dtSTX0003 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSqlstx3, null).Tables[0];
                if (dtSTX0003 != null && dtSTX0003.Rows.Count > 0)
                {
                    serno = dtSTX0003.Rows[0]["PLANCOD"].ToString();
                    ctlqty = Convert.ToInt32(dtSTX0003.Rows[0]["PLQTY"].ToString()) + 1;
                    if (Convert.ToInt32(dtSTX0003.Rows[0]["GTREQTY"].ToString()) > 0)
                        gtreqty = Convert.ToInt32(dtSTX0003.Rows[0]["GTREQTY"].ToString()) - 1;
                    else
                        gtreqty = 0;
                    itnbr = dtSTX0003.Rows[0]["CUITNBR"].ToString();
                    itdsc = dtSTX0003.Rows[0]["CUITDSC"].ToString();
                    gtitnbr = dtSTX0003.Rows[0]["BUITNBR"].ToString();
                    //插入中间表
                    string strInsert = @"INSERT INTO IF_WMS_GT_11 (SERNO,MCHID,ITNBR,ITDSC,GTITNBR,LRFLAG,CTLDIV,CTLQTY, " +
                                       @" RCV_FLAG,ENT_USER_ID,ENT_DT,UPD_USER_ID,UPD_DT) VALUES( " +
                                       @" '" + serno + "','" + MCHID + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + LRFLAG + "','3','" + ctlqty + "', " +
                                       @" 'N','MES',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'MES',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss') )";
                    //更新MES表
                    string strUpdate = "UPDATE STX0003 SET GTREQTY='" + gtreqty + "'  WHERE MCHID='" + MCHID + "' AND LR='" + LRFLAG + "'";
                    //
                    ArrayList list = new ArrayList();
                    list.Add(sql_Del);
                    list.Add(sql_Up);
                    list.Add(strUpdate);
                    int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                    int s = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString2, CommandType.Text, strInsert, null);
                    if (j > 0 && s > 0)
                        return 1;
                    else
                        return 2;
                }
                else
                {
                    return 3;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 4;
            }
        }


        [HttpGet]
        [Route("api/GetNum")]
        public string[] GetNum(string MCHID, string LR)
        {
             
            try
            {
                string[] str = new string[7];


                string jhsql = "select * from PAA0401 where   MCHID ='" + MCHID + "' and  LR ='" + LR + "' and  REYN = 'Y'  ORDER BY WDATE DESC ";
                DataTable jhDT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, jhsql, null).Tables[0];

                if (jhDT.Rows.Count < 1)
                {
                    str[0] = "此机台无计划规格";
                    str[1] = "此机台无计划规格";
                    str[2] = "";
                    str[3] = "";
                    str[4] = "";
                    str[5] = "";
                    str[6] = "";
                    return str;
                }




                string VALUE = jhDT.Rows[0]["ITNBRT"].ToString();

                string[] res = VALUE.Split('_');



                string ITNBR = res[0];


                string sql = "";
                sql = @"SELECT ITNBR,N'' ITDSC,
                       SUM(LOTCOUNT)  LOTCOUNT,
                       SUM(LOTCOUNTN) LOTCOUNTN,
                       SUM(LOTCOUNTB) LOTCOUNTB,
                       SUM(LOTCOUNTK) LOTCOUNTK,
                       SUM(LOTCOUNTH) LOTCOUNTH,
                       SUM(LOTCOUNTG) LOTCOUNTG,
                       SUM(LOTCOUNTE) LOTCOUNTE,
                       SUM(LOTCOUNTU) LOTCOUNTU 
                        FROM ( 
                        SELECT  ITNBR,ITEMSTS,
                                COUNT (LOTID) LOTCOUNT,
                                DECODE(ITEMSTS,'N',COUNT (LOTID),0) LOTCOUNTN,
                                DECODE(ITEMSTS,'B',COUNT (LOTID),0) LOTCOUNTB,
                                CASE WHEN ITEMSTS='N' AND USE!='X' THEN COUNT(LOTID) ELSE 0 END LOTCOUNTK,
                                DECODE(ITEMSTS,'W',COUNT (LOTID),0) LOTCOUNTH,
                                DECODE(ITEMSTS,'G',COUNT (LOTID),0) LOTCOUNTG,
                                DECODE(ITEMSTS,'E',COUNT (LOTID),0) LOTCOUNTE,
                                DECODE(USE,'X',COUNT(LOTID),0) LOTCOUNTU 
                                FROM IF_WMS_GT_INV_VIEW@GT_STOCK_DBLINK 
                                WHERE 1=1  ";
                sql += "AND ITNBR = '" + ITNBR + "' ";
                sql += @" GROUP BY  ITNBR,ITEMSTS,USE 
                                ORDER BY  ITNBR  ) GROUP BY ITNBR 
                                ORDER BY  ITNBR ";

                DataTable kcDT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];


                if (jhDT.Rows.Count > 0)
                {
                    str[0] = jhDT.Rows[0]["ITNBR"].ToString();
                    str[1] = jhDT.Rows[0]["ITDSC"].ToString();

                }
                else
                {
                    str[0] = "无数据";
                    str[1] = "无数据";
                }
                if (kcDT.Rows.Count > 0)
                {
                    str[2] = kcDT.Rows[0]["LOTCOUNT"].ToString();
                    str[3] = kcDT.Rows[0]["LOTCOUNTK"].ToString();
                    str[4] = kcDT.Rows[0]["LOTCOUNTH"].ToString();
                    str[5] = kcDT.Rows[0]["LOTCOUNTE"].ToString();
                    str[6] = kcDT.Rows[0]["LOTCOUNTB"].ToString();


                }
                else
                {
                    str[2] = "无库存";
                    str[3] = "无库存";
                    str[4] = "无库存";
                    str[5] = "无库存";
                    str[6] = "无库存";
                }

                return str;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        #region 脱模剂/硅油登记
        [HttpGet]
        [Route("api/Get_VulTGRegData")]
        public string Get_VulTGRegData(string sMchid, string sLR)
        {
            //GYConfig gycon = new GYConfig();
            //DataRow shfRow = SHIFT.GetShift(DateTime.Now);
            //string sDate = DateTime.Now.ToShortDateString();
            string sResult = string.Empty;
            try
            {
                //string sSql = "SELECT * FROM (SELECT A.*, ROWNUM RN";
                //sSql += "            FROM GY" + sMchid.Substring(2, 4) + " A";
                //sSql += "           WHERE     WTIME BETWEEN TO_DATE ('" + sDate + " 00:00:00','YYYY-MM-DD HH24:MI:SS')";
                //sSql += "                               AND TO_DATE ('" + sDate + " 23:59:59','YYYY-MM-DD HH24:MI:SS')";
                //sSql += "                 AND DIVLR = '" + sLR + "'";
                //sSql += "        ORDER BY WTIME DESC)B";
                //sSql += " WHERE RN = '1'";
                string sSql = "SELECT *";
                sSql += "                         FROM (SELECT A.*, ROWNUM RN";
                sSql += "                                FROM (  SELECT *";
                sSql += "                                          FROM GY" + sMchid.Substring(2, 4) + " A";
                sSql += "                                         WHERE DIVLR = '" + sLR + "' AND WTIME > SYSDATE - 1";
                sSql += "                                     ORDER BY WTIME DESC) A) B";
                sSql += "                        WHERE RN = '1'";
                DataTable dtgy = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString4, CommandType.Text, sSql, null).Tables[0];
                //gycon.oracleBase.GetDataTable(sSql);
                if (dtgy.Rows.Count > 0)
                {
                    sResult = dtgy.Rows[0]["STEPNO"].ToString();
                    return sResult;
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/Save_VulTGRegData")]
        public string Save_VulTGRegData(string sMchid, string sLR, string sUseid, string sPTITEM)
        {
            try
            {
                 
                DataRow rRow = SHIFT.GetShift(DateTime.Now);
                string sSql = "INSERT INTO QMA0112 (ID,FAC,SCANDAT,SCANTIM,SCANNAM,SCANSHT,MCHID,LR,PTITEM) VALUES (";
                sSql += "sys_guid(),'07',";
                sSql += "TO_DATE('" + Convert.ToDateTime(rRow["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), ";
                sSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),";
                sSql += "'" + sUseid + "',";
                sSql += "'" + rRow["WSHT"] + "',";
                sSql += "'" + sMchid + "',";
                sSql += "'" + sLR + "',";
                sSql += "'" + sPTITEM + "'";
                sSql += ")";

                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null);
                if (iResult > 0)
                    return "OK";
                else
                    return "Fail-登记失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }
        #endregion

        [HttpGet]
        [Route("api/GetVULDetial")]
        public string GetVULDetial(string sBarcode)
        {
            string sGTDetail = string.Empty;
             
            try
            {
                string sSql = "SELECT * FROM LTA0001 C,STG0002 B";
                sSql += " WHERE  C.BARCODE = B.BARCODE(+)";
                sSql += " AND C.BARCODE ='" + sBarcode + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                string cunam = "";
                if (dt.Rows.Count > 0)
                {
                    string sqlEMP = @"SELECT * FROM LSFW_EMPLOYEE 
                                  WHERE (LOGINNAME = '" + dt.Rows[0]["CUNAM"].ToString() + "' OR NAME = '" + dt.Rows[0]["CUNAM"].ToString() + "' )" +
                                    @"AND FAC='" + FAC + "'";
                    DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];
                    if (dtEMP.Rows.Count > 0)
                        cunam = dtEMP.Rows[0]["NAME"].ToString();

                    sGTDetail = dt.Rows[0]["CUITNBR"].ToString() + "^"
                                + dt.Rows[0]["CUITDSC"].ToString() + "^"
                                + dt.Rows[0]["CUTIM"].ToString() + "^"
                                + dt.Rows[0]["CUSHT"].ToString() + "^"
                                + dt.Rows[0]["CUBAN"].ToString() + "^"
                                + dt.Rows[0]["CUMCH"].ToString() + "^"
                                + dt.Rows[0]["LR"].ToString() + "^"
                                + dt.Rows[0]["CUSTATE"].ToString() + "^"
                                + dt.Rows[0]["OUTDIV"].ToString() + "^"
                                + cunam + "^"
                                + dt.Rows[0]["CUBQTY"].ToString();
                    return "OK@" + sGTDetail;
                }
                else
                {
                    return "OK@NO DETAIL!";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err-" + ex.Message + "";
            }
        }

        [HttpPost]
        [Route("api/GetExist")]
        public DataTable GetExist(string[] str)
        {
            //0表名
            //1条码
            //2条件 AYN <> 'A'

            ////验证条码是否存在表QMB0101
            //string[] strQMB0101 ={ "QMB0101", str[0], "AYN <> 'A'" };
            //if (GetExist(strQMB0101))
            //    return "5";
            ////验证条码是否存在表QMB0104
            //string[] strQMB0104 ={ "QMB0104", str[0], "DIYN='N'" };
            //if (GetExist(strQMB0104))
            //    return "6";

             
            try
            {
                string sql = "SELECT * FROM " + str[0] + " WHERE BARCODE='" + str[1] + "' AND " + str[2] + " AND FAC='" + FAC + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
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



    }
}
