using System;
using System.Web.Http;
using System.Data;
using log4net.Util;
using HYPDAWebApi.DBHelper;
using System.Collections;
using HYPDAWebApi.Models.ViewModel;
using HYPDAWebApi.App_Data;
using System.Text;

namespace HYPDAWebApi.Controllers
{
    /// <summary>
    /// 成品胎
    /// </summary>
    public class FinishedProductController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";
        QualityTestingController qualityTestingController = new QualityTestingController();

        [HttpGet]
        [Route("api/AlreadyComp")]
        public string AlreadyComp(string BILLNO)
        {
            try
            {
                //判断是否已完成扫描
                string strComp = "SELECT * FROM SDA0002 WHERE BILLNO='" + BILLNO + "' ";
                DataTable dtComp = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strComp, null).Tables[0];
                //OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strComp, null).Tables[0];
                if (dtComp != null && dtComp.Rows.Count > 0)
                {
                    if (dtComp.Rows[0]["STATE"].ToString() == "1")
                        return "1";//是-已完成
                    else if (dtComp.Rows[0]["STATE"].ToString() == "2")
                        return "2";//是-追加中
                    else if (dtComp.Rows[0]["STATE"].ToString() == "3")
                        return "3";//是-已打印
                    else
                        return "0";
                }
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "0";
            }
        }

        [HttpGet]
        [Route("api/CheckCureBadConfDupOrNot")]
        public bool CheckCureBadConfDupOrNot(string sBarcode)
        {
           
            bool bDup = false;
            try
            {
                string strSql = "SELECT * FROM SJE0011  WHERE TO_CHAR(SCANTIM,'YYYY-MM') =TO_CHAR(SYSDATE,'YYYY-MM') AND BARCODE ='" + sBarcode + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null).Tables[0];
                if (dt.Rows.Count > 0)
                    bDup = true;

                return bDup;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return true;
            }
        }

        [HttpGet]
        [Route("api/Comp")]
        public string Comp(string BILLNO, string flag, string sLogId)
        {
            //flag 0装车2追加
            try
            {
                string strSDA0001 = " SELECT * FROM SDA0002 WHERE BILLNO='" + BILLNO + "' ";
                DataTable dtSDA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSDA0001, null).Tables[0];
                if (dtSDA0001 != null && dtSDA0001.Rows.Count > 0)
                {
                    string strSql = " UPDATE SDA0002 SET STATE='1',HDFLAG = 'N',UPDATEDATE = TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),UPDATEUSERID = '" + sLogId + "' WHERE BILLNO='" + BILLNO + "' AND (HDFLAG <>'Y' OR HDFLAG IS NULL)";
                    int i = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null);

                    string mess = "";
                    if (i > 0)
                    {
                        if (flag == "0")
                            mess = "已完成扫描";
                        if (flag == "2")
                            mess = "已完成追加";
                        return mess;
                    }
                    else return "";
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "";
            }
        }

        /// <summary>
        /// 创建单号
        /// </summary>
        /// <param name="sDiv">0-正品，1-废品区分 2-实验胎</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/CreBillNo")]
        public string CreBillNo(string sDiv)
        {
            try
            {
                
                StringBuilder sb = new StringBuilder();
                string sETCDIV = "9";

                if (sDiv == "0")
                {
                    sb.Append("HYG" + DateTime.Today.ToString("yyMMdd") + NextValueSort("SORT"));
                }
                else
                {
                    string date_Temp = DateTime.Now.ToString("yyyyMMdd");
                    string date = date_Temp.Substring(date_Temp.Length - 6);
                    string strBillNo = "";
                    int billNo = 0;
                    DataTable dtMax = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT MAX(BILLNO) AS BILLNUMBER FROM SDA0003 WHERE SUBSTR(BILLNO,3,6) = '" + date + "' AND ETCDIV='9' ", null).Tables[0];
                    DataTable dtCount = null;
                    if (dtMax != null && dtMax.Rows.Count > 0 && !string.IsNullOrEmpty(dtMax.Rows[0]["BILLNUMBER"].ToString()))
                    {
                        strBillNo = dtMax.Rows[0]["BILLNUMBER"].ToString();
                        billNo = Convert.ToInt32(strBillNo.Substring(8, 3)) + 1;
                    }
                    else
                    {
                        dtCount = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT COUNT(ID) AS BILLNUMBER FROM SDA0003 WHERE SUBSTR(BILLNO,3,6) = '" + date + "' AND ETCDIV='9' ", null).Tables[0];
                        billNo = Convert.ToInt32(dtCount.Rows[0]["BILLNUMBER"]) + 1;
                    }
                    sb.Append("HY" + date);
                    if (billNo <= 9) { sb.Append("00" + billNo); }
                    if (billNo <= 99 && billNo >= 10) { sb.Append("0" + billNo); }
                    if (billNo <= 999 && billNo >= 100) { sb.Append(billNo); }
                }

                string strSql = "INSERT INTO SDA0003(ID,BILLNO,DIV,ETCDIV) VALUES('" + Guid.NewGuid() + "','" + sb.ToString() + "','" + sDiv + "','" + sETCDIV + "')";
                int i = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strSql,null);
                if (i == 1) return "创建成功";
                else return "创建失败";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "";
            }
        }

        public string NextValueSort(string plantype)
        {
            string sql = @"SELECT * FROM STLOTSEQ WHERE USEDIV='" + plantype + "' AND YMD='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString,CommandType.Text,sql,null).Tables[0];
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
                string update = @"UPDATE STLOTSEQ SET CURRENTVAL='" + currentval.ToString("D3") + "',NEXTVAL='" + nextval.ToString("D3") + "' WHERE USEDIV='" + plantype + "' AND YMD='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, update,null);
                return currentval.ToString("D3");
            }
            else
            {
                string insert = @"INSERT INTO STLOTSEQ(ID,FAC,USEDIV,YMD,CURRENTVAL,NEXTVAL) 
                            VALUES(SYS_GUID(),'07','" + plantype + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + 1.ToString("D3") + "','" + 2.ToString("D3") + "')";
                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, insert, null);
                return 1.ToString("D3");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sBarcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/FTTireBackHY")]
        public string FTTireBackHY(string sBarcode, string sITNBR, string sITDSC, string sNam, string sReason)
        {
            
            try
            {
                DateTime dtNow = DateTime.Now;
                DataRow row = SHIFT.GetShift(dtNow);
                ArrayList sqllist = new ArrayList();
                // 查询计数表sda0016是否存在该条码
                DataTable dtjstm = null;
                string strJSTM = "SELECT * FROM SDA0016 WHERE BARCODE = '" + sBarcode + "'";
                dtjstm = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strJSTM, null).Tables[0];
                if (dtjstm != null && dtjstm.Rows.Count > 0)
                {
                    //更新MES扫描计数THYNAM,THYTIM,代表已退回
                    string sUpdSql = "UPDATE SDA0016 ";
                    sUpdSql += "   SET THYNAM = '" + sNam + "', THYTIM = SYSDATE ";
                    sUpdSql += "   WHERE BARCODE = '" + sBarcode + "' ";

                    sqllist.Add(sUpdSql);
                }

                //2021-09-2 查询sda0013 如果有状态，将sda0013状态插入到sda0015
                string CX13 = "select sts from (select * from  SDA0018 where barcode='" + sBarcode + "' order by  SCANTIM desc)where rownum=1 ";
                DataTable dtcx0013 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,CX13, null).Tables[0];
                if (dtcx0013 != null && dtcx0013.Rows.Count > 0)
                {

                    //插入履历表
                    string sInsSql = "INSERT INTO SDA0015 (ID,FAC,BARCODE,ITNBR,ITDSC,SCANDAT,SCANTIM,SCANSHT,SCANNAM,THYREASON,STS) VALUES (";
                    sInsSql += " sys_guid(),'07','" + sBarcode + "','" + sITNBR + "','" + sITDSC + "',to_date('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
                    sInsSql += " to_date('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),'" + row["WSHT"].ToString() + "','" + sNam + "','" + sReason + "','" + dtcx0013.Rows[0]["STS"].ToString() + "')";
                    sqllist.Add(sInsSql);
                    int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                    
                    return "OK";

                }
                else
                {
                    //插入履历表
                    string sInsSql = "INSERT INTO SDA0015 (ID,FAC,BARCODE,ITNBR,ITDSC,SCANDAT,SCANTIM,SCANSHT,SCANNAM,THYREASON) VALUES (";
                    sInsSql += " sys_guid(),'07','" + sBarcode + "','" + sITNBR + "','" + sITDSC + "',to_date('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
                    sInsSql += " to_date('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),'" + row["WSHT"].ToString() + "','" + sNam + "','" + sReason + "')";
                    sqllist.Add(sInsSql);
                    int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                    return "OK";
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);

                return "Err:" + ex;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sBarcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/FTTireBackJS")]
        public string FTTireBackJS(string sBarcode, string sITNBR, string sITDSC, string sMESSMTIM, string sWMSRKTIM, string sTXNDIV, string sWMSTKTIM, string sNam)
        {
            
            try
            {
                DateTime dtNow = DateTime.Now;
                DataRow row = SHIFT.GetShift(dtNow);
                ArrayList sqllist = new ArrayList();
                //List<string> sqllist = new List<string>();
                string strMESIN = "";
                if (string.IsNullOrEmpty(sMESSMTIM))
                {
                    strMESIN = "null";
                }
                else
                {
                    strMESIN = "to_date('" + sMESSMTIM + "','yyyy-MM-dd HH24:MI:SS')";
                }

                string strWMSIN = "";

                if (string.IsNullOrEmpty(sWMSRKTIM))
                {
                    strWMSIN = "null";
                }
                else
                {
                    strWMSIN = "to_date('" + sWMSRKTIM + "','yyyy-MM-dd HH24:MI:SS')";
                }


                string sInsSql = "INSERT INTO SDA0052 (ID,BARCODE,ITNBR,ITDSC,MESINTIM,WMSINTIM,TXNDIV,WMSOUTTIM,MESJSTIM,MESJSNAM) VALUES (";
                sInsSql += " sys_guid(),'" + sBarcode + "','" + sITNBR + "','" + sITDSC + "'," + strMESIN + ",";
                sInsSql += " " + strWMSIN + ",'" + sTXNDIV + "',to_date('" + sWMSTKTIM + "','YYYY-MM-DD HH24:MI:SS'),SYSDATE,'" + sNam + "')";
                sqllist.Add(sInsSql);
                LogHelper.Debug(sInsSql);
                int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                return "OK";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);

                return "Err:" + ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sBarcode"></param>
        /// <param name="sITNBR"></param>
        /// <param name="sITDSC"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/FTTireWLOnlineNew")]
        public string FTTireWLOnlineNew(string barcode, string sITNBR, string sITDSC, string sNam, string sReason)
        {
            try
            {
                
                DataTable dtVI1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"select * from SDA0018   WHERE BARCODE='" + barcode + "' order by Scantim desc", null).Tables[0];
                if (dtVI1 != null && dtVI1.Rows.Count > 0)
                {
                    if (dtVI1.Rows[0]["Sts"].ToString() == "A")
                    {
                        string sql1 = @"INSERT INTO SDA0013(ID,FAC,BARCODE,ITNBR,ITDSC,
                                                    SCANDAT,SCANTIM,SCANSHT,STS,IF_FLAG,STOCKNO) " +
                                          "VALUES(SYS_GUID(),'07','" + dtVI1.Rows[0]["BARCODE"].ToString() + "','" +
                                          dtVI1.Rows[0]["ITNBR"].ToString() + "','" + dtVI1.Rows[0]["ITDSC"].ToString() + "',TRUNC(SYSDATE+9/24),SYSDATE,'" +
                                          qualityTestingController.GetFacSht() + "','" + dtVI1.Rows[0]["STS"].ToString() + "','N','" + dtVI1.Rows[0]["STOCKNO"].ToString() + "')";
                        int iRst = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,sql1, null);
                        if (iRst == 0)
                            throw new Exception("发送SDA0013表失败！" + sql1 + "----****----");

                        //插入履历表
                        string sInsSql = "INSERT INTO SDA0014 (ID,FAC,BARCODE,ITNBR,ITDSC,SCANDAT,SCANTIM,SCANSHT,SCANNAM,ONLREASON) VALUES (";
                        sInsSql += " sys_guid(),'07','" + barcode + "','" + sITNBR + "','" + sITDSC + "',TRUNC(SYSDATE+9/24),SYSDATE,";
                        sInsSql += " '" + qualityTestingController.GetFacSht() + "','" + sNam + "','" + sReason + "')";
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,sInsSql, null);
                        // 插入计数表
                        string sInsSqlDEL = "DELETE SDA0016 WHERE BARCODE = '" + barcode + "'";
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,sInsSqlDEL, null);

                        string sInsSqlJS = "INSERT INTO SDA0016 (ID,FAC,BARCODE,ITNBR,ITDSC,SCANDAT,SCANTIM,SCANSHT,STS,IF_FLAG,STOCKNO) VALUES (";
                        sInsSqlJS += " sys_guid(),'07','" + barcode + "','" + sITNBR + "','" + sITDSC + "',TRUNC(SYSDATE+9/24),SYSDATE,";
                        sInsSqlJS += "'" + qualityTestingController.GetFacSht() + "','" + dtVI1.Rows[0]["STS"].ToString() + "','N','" + dtVI1.Rows[0]["STOCKNO"].ToString() + "')";
                        int aa = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,sInsSqlJS, null);


                        string sRoutStkSql = "INSERT INTO PHS2_INSTOCKROUTDATA(MESSAGEID,STATUS,DATETIME1,DATETIME2,BARCODE,TIRECOD,TIREDESC,STOCKDIV,STS) VALUES (";
                        sRoutStkSql += "'','1',SYSDATE,'','" + barcode + "','" + sITNBR + "','" + sITDSC + "','" + dtVI1.Rows[0]["STOCKNO"].ToString() + "','" + dtVI1.Rows[0]["STS"].ToString() + "')";


                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString2, CommandType.Text,sRoutStkSql,null);

                        return "OK";



                    }
                    else
                    {
                        return "当前条码存在不良或保留信息";
                    }
                }
                else
                {
                    return "所选条码无动均判定信息";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err:" + ex;
            }

        }

        [HttpGet]
        [Route("api/GetBarCodeBadInfo")]
        public DataTable GetBarCodeBadInfo(string sBarcode)
        {
            

            try
            {
                string strSql = "SELECT * FROM QMA0101 WHERE BARCODE ='" + sBarcode + "' AND AYN  = 'C' ";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetBillNo")]
        public DataTable GetBillNo(string sBarcode, string sBILLNO)
        {
            

            try
            {
                string strSql = "SELECT * FROM SJE0010 WHERE BARCODE ='" + sBarcode + "' AND BILLNO='" + sBILLNO + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetBillNo_NO")]
        public DataTable GetBillNo_NO()
        {
            

            try
            {
                string strSql = "SELECT DISTINCT BILLNO  FROM(SELECT BILLNO FROM  SJE0010 WHERE  BILLNO IS NOT NULL ORDER BY SCANTIM DESC) WHERE ROWNUM<4";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetDetails")]
        public DataTable GetDetails(string BILLNO, string XYN)
        {
            if (XYN == "Y")
                XYN = "1";
            else if (XYN == "N")
                XYN = "0";
            try
            {
                
                string strWhere = "";
                if (!string.IsNullOrEmpty(BILLNO))
                {
                    strWhere += " AND BILLNO = '" + BILLNO + "' ";
                }
                string strSql = @"SELECT '0' AS TAG, ITNBR,ITDSC,SUM(OUTNUM) AS OUTNUM " +
                       @" FROM SDA0001 WHERE 1=1 AND XYN='" + XYN + "' ";
                if (!string.IsNullOrEmpty(strWhere))
                {
                    strSql += strWhere;
                }
                strSql += " GROUP BY ITNBR,ITDSC ";
                strSql += " UNION ALL ";
                strSql += " SELECT '1' AS TAG, N'合计',N'',SUM(OUTNUM) " +
                    @" FROM SDA0001 WHERE 1=1 AND XYN='" + XYN + "' ";
                if (!string.IsNullOrEmpty(strWhere))
                {
                    strSql += strWhere;
                }
                strSql += " ORDER BY TAG ";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GETReturnInfo")]
        public DataTable GETReturnInfo(string str)
        {
            try
            {
                
                string strSDA0001 = "SELECT * FROM SDA0001 WHERE BARCODE ='" + str + "' AND DIV = '2' AND XYN = '0'";
                DataTable dtSDA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSDA0001, null).Tables[0];
                if (dtSDA0001 != null && dtSDA0001.Rows.Count > 0)
                {
                    return dtSDA0001;
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

        [HttpGet]
        [Route("api/GetSDA0051")]
        public DataTable GetSDA0051(string BARCODE)
        {
            string sql = "SELECT * FROM SDA0051 WHERE BARCODE= '"+BARCODE+"' AND (TXNDIV = '4' OR TXNDIV = '5') ORDER BY WMSINDAT DESC";
            
            DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            if (dtM.Rows.Count > 0)
            {
                return dtM;
            }
            else return null;
        }

        [HttpGet]
        [Route("api/GetSDA0051IN")]
        public DataTable GetSDA0051IN(string BARCODE)
        {
            string sql = "SELECT * FROM SDA0051 WHERE BARCODE= '"+ BARCODE + "' AND TXNDIV = '1' ORDER BY WMSINDAT DESC";
            
            DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql,null).Tables[0];
            if (dtM.Rows.Count > 0)
            {
                return dtM;
            }
            else return null;
        }

        [HttpGet]
        [Route("api/GetTxtNO")]
        public string GetTxtNO()
        {
            
            try
            {
                string txtNO = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
                return txtNO;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/InsertCureBadConf")]
        public string InsertCureBadConf(string sBarcode, string sMCHID, string sLR, string sITNBR, string sITDSC, string sScanMan, string sDIV, string sCCOD)
        {
            

            try
            {
                string strSql = "INSERT INTO SJE0011 (ID,FAC,BARCODE,MCHID,LR,ITNBR,ITDSC,DIV,CCOD,SCANTIM,SCANNAM) VALUES (";
                strSql += "sys_guid(),'07','" + sBarcode + "','" + sMCHID + "','" + sLR + "','" + sITNBR + "','" + sITDSC + "','" + sDIV + "','" + sCCOD + "',sysdate,'" + sScanMan + "')";
                int iRes = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null);
                if (iRes > 0)
                    return "OK";
                else
                    return "Err";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return ex.ToString();
            }
        }

        [HttpGet]
        [Route("api/InsertCureTakStk")]
        public string InsertCureTakStk(string sBarcode, string sMCHID, string sLR, string sITNBR, string sITDSC, string sScanMan, string sBILLNO)
        {
            

            try
            {
                string strSql = "INSERT INTO SJE0010 (ID,FAC,BARCODE,MCHID,LR,ITNBR,ITDSC,SCANTIM,SCANNAM,BILLNO) VALUES (";
                strSql += "sys_guid(),'07','" + sBarcode + "','" + sMCHID + "','" + sLR + "','" + sITNBR + "','" + sITDSC + "',sysdate,'" + sScanMan + "','" + sBILLNO + "')";
                int iRes = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null);
                if (iRes > 0)
                    return "OK";
                else
                    return "Err";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return ex.ToString();
            }
        }

        [HttpGet]
        [Route("api/OKZhuiJia")]
        public string OKZhuiJia(string BILLNO)
        {
            try
            {
                
                //判断是否已完成扫描
                string strComp = "UPDATE SDA0002 SET STATE='2' WHERE BILLNO='" + BILLNO + "' AND (STATE='1' OR STATE='2') AND HDFLAG <>'Y'";
                int i = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strComp, null);
                if (i > 0)
                    return "1";
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "1";
            }
        }

        [HttpPost]
        [Route("api/ProOutBound")]
        public string[] ProOutBound(string[] str)
        {
            //LS_EMPLOYEE 职工
            //LTA0001 成型硫化生产追踪
            //LTP0001 出库履历

            //0、条码
            //1、规格代码
            //2、规格名称
            //3、员工号
            //4、出库区分  0-正品 1-废品
            //5、工单号
            //6、0装车2追加
            //7、配套/非配套区分
            try
            {
                bool bExcute = true;//执行sql语句标志位
                
                string[] list = new string[10];//0是否已完成扫描 1是否出库 2规格代码 3规格名称 4总计数 5是否出库成功 6硫化机 7左右 8正/废品是否扫描错误 9配套是否对应

                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[3] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return new string[6];

                //判断是否已完成扫描
                string strComp = "SELECT * FROM SDA0002 WHERE BILLNO='" + str[5] + "' ";
                DataTable dtComp = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strComp, null).Tables[0];
                if (dtComp != null && dtComp.Rows.Count > 0)
                {
                    if (dtComp.Rows[0]["STATE"].ToString() == str[6])
                        list[0] = "0";//否
                    else
                    {
                        list[0] = dtComp.Rows[0]["STATE"].ToString();
                        bExcute = false;
                    }
                }
                else
                    list[0] = "0";

                if (list[0] == "0")
                {
                    //判断是否已出库
                    string strExist = "SELECT * FROM SDA0001 WHERE BARCODE='" + str[0] + "' AND (XYN='N' OR XYN='0') AND FAC='" + FAC + "'";//BILLNO='" + str[5] + "' AND 
                    DataTable dtLTP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strExist, null).Tables[0];
                    if (dtLTP != null && dtLTP.Rows.Count > 0)
                    {
                        list[1] = "1";
                        bExcute = false;
                    }
                    else
                        list[1] = "0";

                    string strBadOK = string.Empty;
                    string strPT = string.Empty;
                    string strHold = string.Empty;
                    list[8] = "0";
                    //判断扫描的条码是否正品或报废
                    if (str[4] == "0")//若区分是 正品
                    {

                        //增加判断正品是否配套/非配套等
                        strPT = "SELECT A.CUITNBR,A.CUITDSC,A.CUGRADE FROM LTA0001 A,EDB0010 B WHERE A.CUITNBR=B.ITNBR(+) AND BARCODE='" + str[0] + "'";
                        DataTable dtPT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strPT, null).Tables[0];

                        bool bBIS = false;
                        //判断是否有BIS属性
                        string strBISsql = "SELECT COUNT(*) CNT FROM EDB0010 A,EDB0015 B";
                        strBISsql += " WHERE A.ID = B.ITEMID";
                        strBISsql += " AND B.ATTRCOD='M69'";
                        strBISsql += " AND A.USEYN = 'Y'";
                        strBISsql += " AND ITNBR = '" + dtPT.Rows[0]["CUITNBR"].ToString() + "'";
                        DataTable dtBIS = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strBISsql, null).Tables[0];
                        if (dtBIS != null && dtBIS.Rows.Count > 0)
                        {
                            if (int.Parse(dtBIS.Rows[0]["CNT"].ToString()) > 0)
                                bBIS = true;
                        }

                        switch (str[7])
                        {
                            case "不限":
                                break;
                            case "配套一级":
                                if (dtPT.Rows[0]["CUGRADE"].ToString() != "604")
                                {
                                    list[9] = "-1";//配套一级入库检测到非一级品拦截
                                    bExcute = false;
                                }
                                if (bBIS)
                                {
                                    list[9] = "-3";//配套一级入库检测到拥有BIS属性
                                    bExcute = false;
                                }
                                break;
                            case "配套非一级":
                                if (dtPT.Rows[0]["CUGRADE"].ToString() == "604")
                                {
                                    list[9] = "-2";//配套非一级入库检测到一级品拦截
                                    bExcute = false;
                                }
                                if (bBIS)
                                {
                                    list[9] = "-4";//配套非一级入库检测到拥有BIS属性
                                    bExcute = false;
                                }
                                break;
                            case "非配套":
                                if (bBIS)
                                {
                                    list[9] = "-5";//非配套入库检测到拥有BIS属性
                                    bExcute = false;
                                }
                                break;
                            case "印度认证BIS":
                                if (!bBIS)
                                {
                                    list[9] = "-6";//印度认证BIS检测到没有BIS属性
                                    bExcute = false;
                                }
                                break;
                            default:
                                break;
                        }

                        //strBadOK = "SELECT AYN FROM QMA0101 WHERE BARCODE = '" + str[0] + "'";
                        strBadOK = "SELECT DIV FROM QMA0101 WHERE AYN<>'A' AND BARCODE='" + str[0] + "'";
                        DataTable dtBadOK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strBadOK, null).Tables[0];
                        if (dtBadOK != null && dtBadOK.Rows.Count > 0)
                        {
                            list[8] = "-1";//发现不良胎
                            bExcute = false;
                        }

                        strHold = "SELECT CUCOTCOD,CUSMYN FROM LTA0001 WHERE BARCODE='" + str[0] + "'";
                        DataTable dtHold = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strHold, null).Tables[0];
                        if (dtHold != null && dtHold.Rows.Count > 0)
                        {
                            if (dtHold.Rows[0]["CUCOTCOD"].ToString() == "HOLD")
                            {
                                list[8] = "-9";//发现保留胎
                                bExcute = false;
                            }
                            if(dtHold.Rows[0]["CUSMYN"].ToString() == "D" || dtHold.Rows[0]["CUSMYN"].ToString() == "O" || dtHold.Rows[0]["CUSMYN"].ToString() == "I")
                            {
                                list[1] = "2";
                                bExcute = false;
                            }
                        }
                    }
                    else if (str[4] == "1")//若区分是废品出库
                    {
                        strBadOK = "SELECT AYN FROM QMA0101 WHERE BARCODE = '" + str[0] + "'";
                        DataTable dtBadOK = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strBadOK, null).Tables[0];
                        bool bBad = false;
                        if (dtBadOK != null && dtBadOK.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtBadOK.Rows.Count; i++)
                            {
                                if (dtBadOK.Rows[i]["AYN"].ToString() == "C")
                                {
                                    bBad = true;
                                    break;
                                }
                            }
                            if (!bBad)
                            {
                                list[8] = "-2";//发现不是报废胎
                                bExcute = false;
                            }
                        }
                        else
                        {
                            list[8] = "-2";//发现不是报废胎
                            bExcute = false;
                        }
                    }

                    //判断条码
                    string strSpec = "SELECT * FROM LTA0001 WHERE BARCODE='" + str[0] + "' AND FAC='" + FAC + "'";
                    DataTable dtLTA = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSpec, null).Tables[0];
                    if (dtLTA != null && dtLTA.Rows.Count > 0)
                    {
                        if (dtLTA.Rows[0]["CUSMYN"].ToString() == "D" || dtLTA.Rows[0]["CUSMYN"].ToString() == "O" || dtLTA.Rows[0]["CUSMYN"].ToString() == "I")
                        {
                            list[1] = "2";
                            bExcute = false;
                        }
                        list[2] = dtLTA.Rows[0]["CUITNBR"].ToString();
                        list[3] = dtLTA.Rows[0]["CUITDSC"].ToString();
                        list[6] = "";
                        list[7] = "";
                    }
                    else
                    {
                        list[2] = "";
                        list[3] = "";
                        list[6] = "";
                        list[7] = "";
                    }
                    DataRow ROW = SHIFT.GetShift(DateTime.Now);
                    ArrayList sqlt = new ArrayList();
                    //string strDelete = "DELETE FROM SDA0001 WHERE BARCODE='" + str[0] + "' AND XYN='1'";
                    string strInsert = @"INSERT INTO SDA0001( " +
                        @" ID,FAC,BILLNO,ITNBR,ITDSC,BARCODE,DIV,OUTDATE,OUTMAN,WTIME,OUTNUM,XYN,PTDIV) VALUES ( " +
                        @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[5] + "', " +
                        @" '" + list[2] + "','" + list[3] + "','" + str[0] + "', " +
                        @" '" + str[4] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), '" + dtEMP.Rows[0]["NAME"].ToString() + "' , " +
                        @" TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),'1','0','" + str[7] + "' " +
                        @" )";
                    //sqlt.Add(strDelete);
                    sqlt.Add(strInsert);
                    //if (list[1] == "0")
                    if (bExcute == true)
                    {
                        int f = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strInsert, null);
                        //  int f = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionStringsqlt.ToArray());
                        if (f > 0)
                        {
                            f = 1;

                            //出库计数
                            int countP = 0;
                            string strSql = "";
                            DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT * FROM SDA0002 WHERE BILLNO ='" + str[5] + "'", null).Tables[0];
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                countP = Convert.ToInt32(dt.Rows[0]["OUTNUM"].ToString());
                                countP += 1;
                                strSql = "UPDATE SDA0002 SET OUTNUM='" + countP + "',UPDATEDATE = TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),UPDATEUSERID = '" + str[3] + "',SCANTYPE='PDA' WHERE BILLNO='" + str[5] + "' ";
                            }
                            else
                            {
                                countP = 1;
                                strSql = "INSERT INTO SDA0002(ID,BILLNO,OUTNUM,STATE,CARNO,TYRETYPE,TYRESTATUS,CREATEDATE,CREATEUSERID) ";
                                strSql += "VALUES ('" + Guid.NewGuid() + "','" + str[5] + "','" + countP + "','0','','B','" + str[4] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'" + str[3] + "')";
                            }
                            int i = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null);
                            if (i == 1)
                                list[4] = countP.ToString();
                            else
                                list[4] = "0";
                        }
                        else
                        {
                            f = 0;
                            list[4] = "0";
                        }
                        list[5] = f.ToString();
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return new string[6];
            }
        }

        [HttpGet]
        [Route("api/RefreshBillNo")]
        public DataTable RefreshBillNo(string sDiv)
        {
            try
            {
                

                string strSql = string.Empty;
                string date_Temp = DateTime.Now.ToString("yyyyMMdd");
                string date_Temp1 = DateTime.Now.AddDays(-10).ToString("yyyyMMdd");
                string date = date_Temp.Substring(date_Temp.Length - 6);
                string date1 = date_Temp1.Substring(date_Temp.Length - 6);
                if (sDiv == "0")//若是正品
                {
                    strSql = "SELECT * FROM SDA0003 WHERE SUBSTR(BILLNO,4,6) between '" + date1 + "' AND '" + date + "' AND DIV = '" + sDiv + "' AND ETCDIV = '9' ORDER BY BILLNO ";
                }
                else
                {
                    strSql = "SELECT * FROM SDA0003 WHERE SUBSTR(BILLNO,3,6) between '" + date1 + "' AND '" + date + "' AND DIV = '" + sDiv + "' AND ETCDIV = '9' ORDER BY BILLNO ";
                }
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpPost]
        [Route("api/RemoveLun")]
        public DataTable RemoveLun(string[] str)
        {
            //0、工单号
            //1、条码
            //2、员工号
            //3、单个0或全部1
            try
            {
                

                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[2] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return null;

                int sda0002 = 0;
                string strSDA0002 = "SELECT * FROM SDA0002 WHERE BILLNO='" + str[0] + "' ";
                DataTable dtSDA0002 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSDA0002, null).Tables[0];
                if (dtSDA0002 != null && dtSDA0002.Rows.Count > 0)
                    sda0002 = Convert.ToInt32(dtSDA0002.Rows[0]["OUTNUM"].ToString());

                string strSql = "";
                if (str[3] == "0")
                {
                    strSql += "UPDATE SDA0001 SET XYN='1' WHERE BILLNO='" + str[0] + "' AND BARCODE='" + str[1] + "'";
                }
                else if (str[3] == "1")
                {
                    strSql += "UPDATE SDA0001 SET XYN='1' WHERE BILLNO='" + str[0] + "' ";
                }
                int i = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strSql, null);
                DataTable dtXiec = null;
                if (i > 0)
                {
                    dtXiec = GetDetails(str[0], "1");
                    sda0002 -= i;
                    if (sda0002 < 0) sda0002 = 0;
                    string strSDA00022 = "UPDATE SDA0002 SET OUTNUM='" + sda0002.ToString() + "',UPDATEDATE = TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),UPDATEUSERID = '" + str[2] + "' WHERE BILLNO='" + str[0] + "' ";
                    OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,strSDA00022, null);
                }
                return dtXiec;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpPost]
        [Route("api/ReturnInfo")]
        public DataTable ReturnInfo(string[] str)
        {
            try
            {
                
                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[2] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return null;

                string strSql0001 = "UPDATE SDA0001 SET XYN='2' WHERE BILLNO='" + str[0] + "' AND BARCODE='" + str[1] + "'";
                string strSql0004 = "INSERT INTO SDA0004 (ID,FAC,BILLNO,BARCODE,ITNBR,ITDSC,OUTDATE,OUTMAN,WTIME) VALUES (SYS_GUID(),'07','" + str[0] + "','" + str[1] + "','" + str[4] + "','" + str[5] + "',sysdate,'" + dtEMP.Rows[0]["NAME"].ToString() + "',sysdate)";

                //
                ArrayList list = new ArrayList();
                list.Add(strSql0001);
                list.Add(strSql0004);

                int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,list);


                string strSDA0001 = "SELECT * FROM SDA0001 WHERE BARCODE ='" + str[1] + "' AND DIV = '2' AND XYN = '2'";
                DataTable dtSDA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSDA0001, null).Tables[0];
                if (dtSDA0001 != null && dtSDA0001.Rows.Count > 0)
                {
                    return dtSDA0001;
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

        [HttpGet]
        [Route("api/SumRefresh")]
        public string[] SumRefresh(string BILLNO, string LOGINNAME)
        {
            try
            {
                
                string[] list = new string[2];

                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + LOGINNAME + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return new string[2];

                int countP = 0;
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT * FROM SDA0002 WHERE BILLNO ='" + BILLNO + "'", null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                    countP = Convert.ToInt32(dt.Rows[0]["OUTNUM"].ToString());
                list[0] = countP.ToString();

                DataTable dtgr = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT COUNT(*) AS GEREN FROM SDA0001 WHERE BILLNO ='" + BILLNO + "'  AND XYN='0' AND OUTMAN='" + dtEMP.Rows[0]["NAME"].ToString() + "'", null).Tables[0];
                if (dtgr != null && dtgr.Rows.Count > 0)
                    list[1] = dtgr.Rows[0]["GEREN"].ToString();
                else
                    list[1] = "0";
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return new string[2];
            }
        }

        [HttpPost]
        [Route("api/XCExist")]
        public string XCExist(string[] str)
        {
            //0、工单号
            //1、条码

            try
            {
                
                DataRow row = SHIFT.GetShift(DateTime.Now);
                string sda0001 = "";
                string strSDA0001 = "SELECT * FROM SDA0001 WHERE BILLNO='" + str[0] + "' AND BARCODE='" + str[1] + "' AND XYN = '0'";
                DataTable dtSDA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,strSDA0001, null).Tables[0];
                if (dtSDA0001 == null || dtSDA0001.Rows.Count == 0)
                    sda0001 = "当前工单下不存在此条码或者该条码还没有装车入库！";

                if (dtSDA0001 != null && dtSDA0001.Rows.Count > 0)
                {
                    if (Convert.ToDateTime(dtSDA0001.Rows[0]["WTIME"].ToString()).ToString("YYYY-MM-DD") != Convert.ToDateTime(row["WDATE"].ToString()).ToString("YYYY-MM-DD"))
                        sda0001 = "无法卸车已结算的条码！";
                }
                return sda0001;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "处理异常！";
            }
        }

    }
}
