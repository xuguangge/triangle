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
    /// 胎胚队列
    /// </summary>
    public class EmbryoController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";
        VulcanizationController vulcanizationController = new VulcanizationController();

        /// <summary>
        /// 查询机台硫化队列
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/GetVulShow")]
        public DataTable GetVulShow(string[] str)
        {
            //0、机台

            try
            {
                string strSql = "SELECT * FROM STX0002 WHERE MCHID='" + str[0] + "' ORDER BY CTIM ASC  ";
                DataTable dtSTX0002 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                if (dtSTX0002.Rows.Count == 0)
                    return null;
                else
                    return dtSTX0002;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        /// <summary>
        /// 保存数据信息
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/RecordCompareQueue")]
        public string RecordCompareQueue(Record record)
        {
            ArrayList sqllist = new ArrayList();
             
            try
            {
                #region 不用
                //获取系统左侧队列
                //string sSqlL = "SELECT * FROM STX0002 WHERE MCHID  ='" + sMchid + "' AND LRFLAG = 'L' ORDER BY CTIM";
                //DataTable dtL = db.GetDataTable(sSqlL);
                ////获取系统右侧队列
                //string sSqlR = "SELECT * FROM STX0002 WHERE MCHID  ='" + sMchid + "' AND LRFLAG = 'R' ORDER BY CTIM";
                //DataTable dtR = db.GetDataTable(sSqlR);

                ////L
                //string sResultL = string.Empty;
                //bool bOKL = true;
                //if (sLBarcodes.Length == dtL.Rows.Count)
                //{
                //    if (sLBarcodes.Length == 1)
                //    {
                //        if (sLBarcodes[0] != dtL.Rows[0]["BARCODE"].ToString())
                //            bOKL = false;
                //    }
                //    else if (sLBarcodes.Length == 2)
                //    {
                //        if (sLBarcodes[0] != dtL.Rows[0]["BARCODE"].ToString() || sLBarcodes[1] != dtL.Rows[1]["BARCODE"].ToString())
                //            bOKL = false;
                //    }
                //    else if (sLBarcodes.Length == 3)
                //    {
                //        if (sLBarcodes[0] != dtL.Rows[0]["BARCODE"].ToString() || sLBarcodes[1] != dtL.Rows[1]["BARCODE"].ToString() || sLBarcodes[2] != dtL.Rows[2]["BARCODE"].ToString())
                //            bOKL = false;
                //    }
                //    else
                //    { }
                //}
                //else
                //    bOKL = false;

                //if (bOKL) sResultL = "匹配"; else sResultL = "不匹配";

                //string sScanBarsL = string.Empty;
                //if (sLBarcodes != null && sLBarcodes.Length > 0)
                //{
                //    foreach (string var in sLBarcodes)
                //    {
                //        sScanBarsL += var + ";";
                //    }
                //    sScanBarsL = sScanBarsL.TrimEnd(';');
                //}
                //string sSysBarsL = string.Empty;
                //if (dtL != null && dtL.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtL.Rows.Count; i++)
                //    {
                //        sSysBarsL += dtL.Rows[i]["BARCODE"].ToString() + ";";
                //    }
                //    sSysBarsL = sSysBarsL.TrimEnd(';');
                //}

                ////R
                //string sResultR = string.Empty;
                //bool bOKR = true;
                //if (sRBarcodes.Length == dtR.Rows.Count)
                //{
                //    if (sRBarcodes.Length == 1)
                //    {
                //        if (sRBarcodes[0] != dtR.Rows[0]["BARCODE"].ToString())
                //            bOKR = false;
                //    }
                //    else if (sRBarcodes.Length == 2)
                //    {
                //        if (sRBarcodes[0] != dtR.Rows[0]["BARCODE"].ToString() || sRBarcodes[1] != dtR.Rows[1]["BARCODE"].ToString())
                //            bOKR = false;
                //    }
                //    else if (sRBarcodes.Length == 3)
                //    {
                //        if (sRBarcodes[0] != dtR.Rows[0]["BARCODE"].ToString() || sRBarcodes[1] != dtR.Rows[1]["BARCODE"].ToString() || sRBarcodes[2] != dtR.Rows[2]["BARCODE"].ToString())
                //            bOKR = false;
                //    }
                //    else
                //    { }
                //}
                //else
                //    bOKR = false;

                //if (bOKR) sResultR = "匹配"; else sResultR = "不匹配";

                //string sScanBarsR = string.Empty;
                //if (sRBarcodes != null && sRBarcodes.Length > 0)
                //{
                //    foreach (string var in sRBarcodes)
                //    {
                //        sScanBarsR += var + ";";
                //    }
                //    sScanBarsR = sScanBarsR.TrimEnd(';');
                //}
                //string sSysBarsR = string.Empty;
                //if (dtR != null && dtR.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtR.Rows.Count; i++)
                //    {
                //        sSysBarsR += dtR.Rows[i]["BARCODE"].ToString() + ";";
                //    }
                //    sSysBarsR = sSysBarsR.TrimEnd(';');
                //}

                ////记录
                //string sSqlInL = "INSERT INTO STX0020(ID,FAC,MCHID,LR,SYSBARS,SCANBARS,RESULTS,ENAM,ETIM) VALUES (";
                //sSqlInL += "sys_guid(),'07','" + sMchid + "','L','" + sSysBarsL + "','" + sScanBarsL + "','" + sResultL + "','" + sNam + "',SYSDATE)";
                //sqllist.Add(sSqlInL);
                //string sSqlInR = "INSERT INTO STX0020(ID,FAC,MCHID,LR,SYSBARS,SCANBARS,RESULTS,ENAM,ETIM) VALUES (";
                //sSqlInR += "sys_guid(),'07','" + sMchid + "','R','" + sSysBarsR + "','" + sScanBarsR + "','" + sResultR + "','" + sNam + "',SYSDATE)";
                //sqllist.Add(sSqlInR);
                //db.TranNonQuery(sqllist.ToArray());
                //return "OK";            
                #endregion
                string sScanBarsL = string.Empty;
                if (record.sLBarcodes != null && record.sLBarcodes.Length > 0)
                {
                    foreach (string var in record.sLBarcodes)
                    {
                        sScanBarsL += var + ";";
                    }
                    sScanBarsL = sScanBarsL.TrimEnd(';');
                }

                string sScanBarsR = string.Empty;
                if (record.sRBarcodes != null && record.sRBarcodes.Length > 0)
                {
                    foreach (string var in record.sRBarcodes)
                    {
                        sScanBarsR += var + ";";
                    }
                    sScanBarsR = sScanBarsR.TrimEnd(';');
                }

                string sSqlInL = "INSERT INTO STX0020(ID,FAC,MCHID,LR,SYSBARS,SCANBARS,RESULTS,ENAM,ETIM) VALUES (";
                sSqlInL += "sys_guid(),'07','" + record.sMchid + "','L','','" + sScanBarsL + "','" + record.sDIV + "','" + record.sNam + "',SYSDATE)";
                sqllist.Add(sSqlInL);
                string sSqlInR = "INSERT INTO STX0020(ID,FAC,MCHID,LR,SYSBARS,SCANBARS,RESULTS,ENAM,ETIM) VALUES (";
                sSqlInR += "sys_guid(),'07','" + record.sMchid + "','R','','" + sScanBarsR + "','" + record.sDIV + "','" + record.sNam + "',SYSDATE)";
                sqllist.Add(sSqlInR);
                OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sqllist);
                return "OK";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err:" + ex.Message;
            }
        }

        /// <summary>
        /// 校验机台队列中是否存在该条码
        /// </summary>
        /// <param name="sMchid"></param>
        /// <param name="sLR"></param>
        /// <param name="sBarcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/QueueIsRepeatOrNot")]
        public bool QueueIsRepeatOrNot(string sMchid, string sLR, string sBarcode)
        {
             
            try
            {
                bool bRepeat = false;
                DataTable dt = new DataTable();
                string sql = "select count(*) CNT from STX0002 where MCHID = '" + sMchid + "' and lrflag = '" + sLR + "' and barcode = '" + sBarcode + "'";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    int icnt = 0;
                    int.TryParse(dt.Rows[0]["CNT"].ToString(), out icnt);
                    if (icnt > 0)
                    {
                        bRepeat = true;
                    }
                }
                return bRepeat;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return true;
            }
        }

        /// <summary>
        /// 是否存在重复条码
        /// </summary>
        /// <param name="sBarcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/QueueIsRepeatOrNot_All")]
        public string QueueIsRepeatOrNot_All(string sBarcode)
        {
             
            try
            {
                string sRepeatInfo = string.Empty;
                string sMchid = string.Empty;
                string sLR = string.Empty;
                DataTable dt = new DataTable();
                string sql = "SELECT MCHID,LRFLAG,BARCODE FROM STX0002 WHERE BARCODE = '" + sBarcode + "'";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    sMchid = dt.Rows[0]["MCHID"].ToString();
                    sLR = dt.Rows[0]["LRFLAG"].ToString();
                    sRepeatInfo = sMchid + "^" + sLR + "^" + sBarcode;
                }
                return sRepeatInfo;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// 是否存在重复条码
        /// </summary>
        /// <param name="sBarcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/QueueIsRepeatOrNot_New")]
        public string QueueIsRepeatOrNot_New(string sMchid, string sLR, string sBarcode)
        {

            try
            {
                string sRepeatInfo = string.Empty;
                DataTable dt = new DataTable();
                string sql = "select MCHID,LRFLAG,BARCODE from STX0002 where MCHID = '" + sMchid + "' and lrflag = '" + sLR + "' and barcode = '" + sBarcode + "'";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    sMchid = dt.Rows[0]["MCHID"].ToString();
                    sLR = dt.Rows[0]["LRFLAG"].ToString();
                    sRepeatInfo = sMchid + "^" + sLR + "^" + sBarcode;
                }
                return sRepeatInfo;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return ex.Message;
            }
        }

        //返回值：0员工不存在、 1新增成功、 2成型规格不一致、 3当前机台区分没有正在生产的计划、 
        //       4生产追溯没有此条码信息、 5新增失败、 6处理异常 7、此条码没有硫化规格信息 
        //       8、条码在胎胚不良已存在、81报废、9条码在胎胚保留未处理 15该条码已被硫化
        /// <summary>
        /// 保存胎胚队列新增重排信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/GetVulNew")]
        public string GetVulNew(string[] str)
        {
            //0、BARCODE
            //1、机台
            //2、左右区分
            //3、登录人工号
            //4、新增原因
            try
            {
                 
               

                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[3] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return "0";
                //
                //验证条码是否存在表QMB0104
                string[] strQMB0104 = { "QMB0104", str[0], "DIYN='N'" };
                DataTable dtQMB0104 = vulcanizationController.GetExist(strQMB0104);
                if (dtQMB0104 != null && dtQMB0104.Rows.Count > 0)
                    return "9";
                //验证条码是否存在表QMB0101
                string[] strQMB0101 = { "QMB0101", str[0], "AYN <> 'A'" };
                DataTable dtQMB0101 = vulcanizationController.GetExist(strQMB0101);
                if (dtQMB0101 != null && dtQMB0101.Rows.Count > 0)
                {
                    if (dtQMB0101.Rows[0]["AYN"].ToString() == "B")
                        return "8";//不良
                    else
                        return "81";//报废
                }

                //是否条码已被硫化
                string sCrITNBR = string.Empty;
                string sCrMCH = string.Empty;
                string sCrLR = string.Empty;
                string sCrTim = string.Empty;
                string sSqlCr = "SELECT * FROM LTA0001 WHERE BARCODE = '" + str[0] + "'";
                DataTable dtCr = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlCr, null).Tables[0];
                if (dtCr.Rows.Count > 0)
                {
                    sCrITNBR = dtCr.Rows[0]["CUITNBR"].ToString();
                    sCrMCH = dtCr.Rows[0]["CUMCH"].ToString();
                    sCrLR = dtCr.Rows[0]["LR"].ToString();
                    sCrTim = dtCr.Rows[0]["CUTIM"].ToString();
                    if (!string.IsNullOrEmpty(sCrITNBR))
                        return "Err^15^" + sCrMCH + "^" + sCrLR + "^" + sCrTim + "";
                }

                //
                //string strPAD0401 = @"SELECT * FROM PAD0401 WHERE DIV='1' AND MCHID='" + str[1] + "' AND LR='" + str[2] + "'";
                //DataTable dtPAD0401 = db.GetDataTable(strPAD0401);
                //if (dtPAD0401.Rows.Count == 0)
                //    return 3;
                //string gtitnbr = dtPAD0401.Rows[0]["BUITNBR"].ToString();
                //string itnbr = dtPAD0401.Rows[0]["CUITNBR"].ToString();
                //string gtitdsc = dtPAD0401.Rows[0]["BUITDSC"].ToString();
                //string itdsc = dtPAD0401.Rows[0]["CUITDSC"].ToString();
                //
                string gtitnbr = "";
                string gtitdsc = "";
                string itnbr = "";
                string itdsc = "";
                string strPAD0401 = @"SELECT * FROM PAD0401 WHERE DIV='1' AND MCHID='" + str[1] + "' AND LR='" + str[2] + "'";
                DataTable dtPAD0401 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strPAD0401, null).Tables[0];
                if (dtPAD0401.Rows.Count == 0)
                {
                    //string strSTX0004 = @"SELECT ID,FAC,MCHID,LR,CUITNBR,'' AS CUITDSC,BUITNBR,'' AS BUITDSC,WTIM FROM STX0004 WHERE MCHID='" + str[1] + "' AND LR='" + str[2] + "' ORDER BY WTIM DESC";
                    //上面语句驴唇不对马嘴，STX0004就是废表 JOE 2019-03-29
                    string strSTX = "SELECT BUITNBR,BOMID CUITNBR,'' AS CUITDSC,'' AS BUITDSC FROM LTA0001 A,EDC0003 B WHERE A.BUITNBR = B.ITNBR AND BARCODE  ='" + str[0] + "' AND B.USEYN = 'Y' AND B.LEVL = '1'";
                    DataTable dtSTX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSTX, null).Tables[0];
                    if (dtSTX != null && dtSTX.Rows.Count > 0)
                    {
                        string strEDB0010 = @"SELECT *
                                                          FROM (SELECT A.*,
                                                                       ROW_NUMBER ()
                                                                          OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC)
                                                                          RN
                                                                  FROM EDB0010 A)
                                                         WHERE RN = 1";
                        DataTable dtEDB0010 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strEDB0010, null).Tables[0];
                        if (dtEDB0010 != null && dtEDB0010.Rows.Count > 0)
                        {
                            //胎胚
                            DataRow[] Brow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["BUITNBR"] + "'");
                            if (Brow.Length > 0)
                                dtSTX.Rows[0]["BUITDSC"] = Brow[0]["ITDSC"].ToString();
                            //硫化
                            DataRow[] Crow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["CUITNBR"] + "'");
                            if (Crow.Length > 0)
                                dtSTX.Rows[0]["CUITDSC"] = Crow[0]["ITDSC"].ToString();
                        }
                        gtitnbr = dtSTX.Rows[0]["BUITNBR"].ToString();
                        gtitdsc = dtSTX.Rows[0]["BUITDSC"].ToString();
                        itnbr = dtSTX.Rows[0]["CUITNBR"].ToString();
                        itdsc = dtSTX.Rows[0]["CUITDSC"].ToString();
                    }
                    else
                        return "7";
                }
                //return "3";
                else
                {
                    gtitnbr = dtPAD0401.Rows[0]["BUITNBR"].ToString();
                    gtitdsc = dtPAD0401.Rows[0]["BUITDSC"].ToString();
                    itnbr = dtPAD0401.Rows[0]["CUITNBR"].ToString();
                    itdsc = dtPAD0401.Rows[0]["CUITDSC"].ToString();
                }
                //
                string strLTA0001 = @" SELECT * FROM LTA0001 " +
                                @" WHERE BARCODE='" + str[0] + "' ";
                DataTable dtLTA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strLTA0001, null).Tables[0];
                if (dtLTA0001.Rows.Count == 0)
                    return "4";
                string gtitnbrL = dtLTA0001.Rows[0]["BUITNBR"].ToString();
                if (gtitnbr != gtitnbrL)
                    return "Err^2^" + gtitnbr + "^" + gtitnbrL;
                //
                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
                                       @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[1] + "','" + str[2] + "','" + str[0] + "', " +
                                       @" '" + gtitnbr + "','" + itnbr + "','0',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                                       @" '2' )";
                //
                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
                       @"EVNDAT, " +
                       @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
                       @"ENT_USER_ID,ENT_DT, " +
                       @"UPD_USER_ID,UPD_DT, " +
                       @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
                       @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
                       @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                       @"'" + str[1] + "','" + str[2] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                       @"'N','2' )";
                //
                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC,REMARK) VALUES( " +
                                     @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[1] + "', '" + str[2] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
                                     @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'2','" + str[5] + "','" + str[4] + "')";
                //
                ArrayList list = new ArrayList();
                list.Add(strInsert);
                list.Add(strSTG0005);
                list.Add(sql_Up);
                int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (j > 0)
                {
                    ArrayList lists = new ArrayList();

                    string sql = "select * from if_wms_gt_10 where barcode = '" + str[0] + "' and RCV_FLAG = 'N'";
                    DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString2, CommandType.Text, sql, null).Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            sql = "update if_wms_gt_10 set RCV_FLAG = 'Y' where barcode = '" + dt.Rows[i]["BARCODE"].ToString() + "' and mchid = '" + dt.Rows[i]["MCHID"].ToString() + "'  and lrflag = '" + dt.Rows[i]["LRFLAG"].ToString() + "'";
                            lists.Add(sql);
                        }
                        OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString2, lists);
                    }
                    return "1";
                }
                else
                {
                    return "5";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "6";
            }
        }

        //0处理异常、1新增成功、2新增失败
        /// <summary>
        /// 保存胎胚队列新增重排信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/GetNewContinue")]
        public int GetNewContinue(string[] str)
        {
            //0、BARCODE
            //1、机台
            //2、左右区分
            //3、登录人工号
            //4、新增原因
            try
            {
                 

                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[3] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];

                //
                string gtitnbr = "NO ITNBR";
                string gtitdsc = "";
                string itnbr = "NO ITNBR";
                string itdsc = "";
                //
                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
                                       @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[1] + "','" + str[2] + "','" + str[0] + "', " +
                                       @" '" + gtitnbr + "','" + itnbr + "','0',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                                       @" '2' )";
                //
                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
                       @"EVNDAT, " +
                       @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
                       @"ENT_USER_ID,ENT_DT, " +
                       @"UPD_USER_ID,UPD_DT, " +
                       @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
                       @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
                       @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                       @"'" + str[1] + "','" + str[2] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                       @"'N','2' )";
                //
                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC,REMARK) VALUES( " +
                                     @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[1] + "', '" + str[2] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
                                     @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'2','新增','" + str[4] + "')";
                //
                ArrayList list = new ArrayList();
                list.Add(strInsert);
                list.Add(strSTG0005);
                list.Add(sql_Up);
                int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (j > 0)
                    return 1;
                else
                    return 2;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 0;
            }
        }

        /// <summary>
        /// 校验是否可以重排
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/QueInital")]
        public int QueInital(string[] str)
        {
            //0、机台
            //1、左右区分

             
            try
            {
                string strSql = "DELETE FROM STX0002 WHERE MCHID='" + str[0] + "' AND LRFLAG='" + str[1] + "' ";
                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC) VALUES( " +
                                    @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[0] + "', '" + str[1] + "', '', 'MES'," +
                                    @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'3','重排')";
                ArrayList list = new ArrayList();
                list.Add(strSql);
                list.Add(sql_Up);
                int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (j > 0)
                    return 1;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 0;
            }
        }


        //返回值：0员工不存在、1转移成功、2成型规格不一致、3无此条码信息、4当前机台区分没有正在生产的计划、 
        //        5生产追溯没有此条码信息、6转移失败、7处理异常、8此条码没有硫化规格信息 
        //        9条码在胎胚不良已存在、91报废、10条码在胎胚保留未处理、15条码已被硫化
        /// <summary>
        /// 保存胎胚队列转移信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/GetVulMove")]
        public string GetVulMove(string[] str)
        {
            //0、BARCODE
            //1、机台
            //2、左右区分
            //3、目标机台
            //4、目标区分
            //5、登录人工号
            try
            {
                 
                //
                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[5] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return "0";
                //
                //验证条码是否存在表QMB0104
                string[] strQMB0104 = { "QMB0104", str[0], "DIYN='N'" };
                DataTable dtQMB0104 = vulcanizationController.GetExist(strQMB0104);
                if (dtQMB0104 != null && dtQMB0104.Rows.Count > 0)
                    return "10";
                //验证条码是否存在表QMB0101
                string[] strQMB0101 = { "QMB0101", str[0], "AYN <> 'A'" };
                DataTable dtQMB0101 = vulcanizationController.GetExist(strQMB0101);
                if (dtQMB0101 != null && dtQMB0101.Rows.Count > 0)
                {
                    if (dtQMB0101.Rows[0]["AYN"].ToString() == "B")
                        return "9";//不良
                    else
                        return "91";//报废
                }

                //是否条码已被硫化
                string sCrITNBR = string.Empty;
                string sCrMCH = string.Empty;
                string sCrLR = string.Empty;
                string sCrTim = string.Empty;
                string sSqlCr = "SELECT * FROM LTA0001 WHERE BARCODE = '" + str[0] + "'";
                DataTable dtCr = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlCr, null).Tables[0];
                if (dtCr.Rows.Count > 0)
                {
                    sCrITNBR = dtCr.Rows[0]["CUITNBR"].ToString();
                    sCrMCH = dtCr.Rows[0]["CUMCH"].ToString();
                    sCrLR = dtCr.Rows[0]["LR"].ToString();
                    sCrTim = dtCr.Rows[0]["CUTIM"].ToString();
                    if (!string.IsNullOrEmpty(sCrITNBR))
                        return "Err^15^" + sCrMCH + "^" + sCrLR + "^" + sCrTim + "";
                }
                //
                string strSql = "SELECT * FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
                DataTable dtSTX0002 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                if (dtSTX0002.Rows.Count == 0)
                    return "3";
                string wrginflag = dtSTX0002.Rows[0]["WRGINFLAG"].ToString();
                //
                string gtitnbr = "";
                string gtitdsc = "";
                string itnbr = "";
                string itdsc = "";
                string strPAD0401 = @"SELECT * FROM PAD0401 WHERE DIV='1' AND MCHID='" + str[1] + "' AND LR='" + str[2] + "'";
                DataTable dtPAD0401 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strPAD0401, null).Tables[0];
                if (dtPAD0401.Rows.Count == 0)
                {
                    //string strSTX0004 = @"SELECT ID,FAC,MCHID,LR,CUITNBR,'' AS CUITDSC,BUITNBR,'' AS BUITDSC,WTIM FROM STX0004 WHERE MCHID='" + str[1] + "' AND LR='" + str[2] + "' ORDER BY WTIM DESC";
                    //上面=狗屎
                    string strSTX = "SELECT BUITNBR,BOMID CUITNBR,'' AS CUITDSC,'' AS BUITDSC FROM LTA0001 A,EDC0003 B WHERE A.BUITNBR = B.ITNBR AND BARCODE  ='" + str[0] + "' AND B.USEYN = 'Y' AND B.LEVL = '1'";
                    DataTable dtSTX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSTX, null).Tables[0];
                    if (dtSTX != null && dtSTX.Rows.Count > 0)
                    {
                        string strEDB0010 = @"SELECT *
                                                          FROM (SELECT A.*,
                                                                       ROW_NUMBER ()
                                                                          OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC)
                                                                          RN
                                                                  FROM EDB0010 A)
                                                         WHERE RN = 1";
                        DataTable dtEDB0010 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strEDB0010, null).Tables[0];
                        if (dtEDB0010 != null && dtEDB0010.Rows.Count > 0)
                        {
                            //胎胚
                            DataRow[] Brow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["BUITNBR"] + "'");
                            if (Brow.Length > 0)
                                dtSTX.Rows[0]["BUITDSC"] = Brow[0]["ITDSC"].ToString();
                            //硫化
                            DataRow[] Crow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["CUITNBR"] + "'");
                            if (Crow.Length > 0)
                                dtSTX.Rows[0]["CUITDSC"] = Crow[0]["ITDSC"].ToString();
                        }
                        gtitnbr = dtSTX.Rows[0]["BUITNBR"].ToString();
                        gtitdsc = dtSTX.Rows[0]["BUITDSC"].ToString();
                        itnbr = dtSTX.Rows[0]["CUITNBR"].ToString();
                        itdsc = dtSTX.Rows[0]["CUITDSC"].ToString();
                    }
                    else
                        return "8";
                }
                //return "4";
                else
                {
                    gtitnbr = dtPAD0401.Rows[0]["BUITNBR"].ToString();
                    gtitdsc = dtPAD0401.Rows[0]["BUITDSC"].ToString();
                    itnbr = dtPAD0401.Rows[0]["CUITNBR"].ToString();
                    itdsc = dtPAD0401.Rows[0]["CUITDSC"].ToString();
                }
                //
                string strLTA0001 = @" SELECT * FROM LTA0001 " +
                                @" WHERE BARCODE='" + str[0] + "' ";
                DataTable dtLTA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strLTA0001, null).Tables[0];
                if (dtLTA0001.Rows.Count == 0)
                    return "5";
                string gtitnbrL = dtLTA0001.Rows[0]["BUITNBR"].ToString();
                if (gtitnbr != gtitnbrL)
                    return "Err^2^" + gtitnbr + "^" + gtitnbrL;
                //
                string strDelete = "DELETE FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
                //
                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
                                   @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[3] + "','" + str[4] + "','" + str[0] + "', " +
                                   @" '" + gtitnbr + "','" + itnbr + "','" + wrginflag + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                                   @" '2')";
                //
                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
                    @"EVNDAT, " +
                    @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
                    @"ENT_USER_ID,ENT_DT, " +
                    @"UPD_USER_ID,UPD_DT, " +
                    @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
                    @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'" + str[3] + "','" + str[4] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'N','2' )";
                //
                string desc = "由" + str[1] + "-" + str[2] + "转移到" + str[3] + "-" + str[4];
                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC) VALUES( " +
                                @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[3] + "', '" + str[4] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
                                @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'1','" + desc + "')";
                //
                ArrayList list = new ArrayList();
                list.Add(strDelete);
                list.Add(strInsert);
                list.Add(strSTG0005);
                list.Add(sql_Up);
                int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (j > 0)
                    return "1";
                else
                    return "6";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "7";
            }
        }

        //0处理异常、1转移成功、2转移失败
        /// <summary>
        /// 保存胎胚队列转移信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/GetMoveContinue")]
        public int GetMoveContinue(string[] str)
        {
            //0、BARCODE
            //1、机台
            //2、左右区分
            //3、目标机台
            //4、目标区分
            //5、登录人工号
            try
            {
                 
                //
                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[5] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];

                //
                string strSql = "SELECT * FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
                DataTable dtSTX0002 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                string wrginflag = dtSTX0002.Rows[0]["WRGINFLAG"].ToString();
                //
                string gtitnbr = "NO ITNBR";
                string gtitdsc = "";
                string itnbr = "NO ITNBR";
                string itdsc = "";
                //
                string strDelete = "DELETE FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
                //
                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
                                   @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[3] + "','" + str[4] + "','" + str[0] + "', " +
                                   @" '" + gtitnbr + "','" + itnbr + "','" + wrginflag + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                                   @" '2')";
                //
                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
                    @"EVNDAT, " +
                    @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
                    @"ENT_USER_ID,ENT_DT, " +
                    @"UPD_USER_ID,UPD_DT, " +
                    @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
                    @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'" + str[3] + "','" + str[4] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'N','2' )";
                //
                string desc = "由" + str[1] + "-" + str[2] + "转移到" + str[3] + "-" + str[4];
                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC) VALUES( " +
                                @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[3] + "', '" + str[4] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
                                @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'1','" + desc + "')";
                //
                ArrayList list = new ArrayList();
                list.Add(strDelete);
                list.Add(strInsert);
                list.Add(strSTG0005);
                list.Add(sql_Up);
                int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (j > 0)
                    return 1;
                else
                    return 2;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 0;
            }
        }

        //返回值：0员工不存在、1转移成功、2成型规格不一致、3无此条码信息、4当前机台区分没有正在生产的计划、 
        //        5生产追溯没有此条码信息、6转移失败、7处理异常、8此条码没有硫化规格信息 
        //        9条码在胎胚不良已存在、91报废、10条码在胎胚保留未处理
        /// <summary>
        /// 保存胎胚队列转移信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/GetMoveContinue_2")]
        public string GetMoveContinue_2(string[] str)
        {
            //0、BARCODE
            //1、机台
            //2、左右区分
            //3、目标机台
            //4、目标区分
            //5、登录人工号
            try
            {
                 
                //
                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[5] + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlEMP, null).Tables[0];
                //
                string strSql = "SELECT * FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
                DataTable dtSTX0002 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                if (dtSTX0002.Rows.Count == 0)
                    return "3";
                string wrginflag = dtSTX0002.Rows[0]["WRGINFLAG"].ToString();
                //
                string gtitnbr = "";
                string gtitdsc = "";
                string itnbr = "";
                string itdsc = "";
                string strPAD0401 = @"SELECT * FROM PAD0401 WHERE DIV='1' AND MCHID='" + str[1] + "' AND LR='" + str[2] + "'";
                DataTable dtPAD0401 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strPAD0401, null).Tables[0];
                if (dtPAD0401.Rows.Count == 0)
                {
                    //string strSTX0004 = @"SELECT ID,FAC,MCHID,LR,CUITNBR,'' AS CUITDSC,BUITNBR,'' AS BUITDSC,WTIM FROM STX0004 WHERE MCHID='" + str[1] + "' AND LR='" + str[2] + "' ORDER BY WTIM DESC";
                    //上面=狗屎
                    string strSTX = "SELECT BUITNBR,BOMID CUITNBR,'' AS CUITDSC,'' AS BUITDSC FROM LTA0001 A,EDC0003 B WHERE A.BUITNBR = B.ITNBR AND BARCODE  ='" + str[0] + "' AND B.USEYN = 'Y' AND B.LEVL = '1'";
                    DataTable dtSTX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSTX, null).Tables[0];
                    if (dtSTX != null && dtSTX.Rows.Count > 0)
                    {
                        string strEDB0010 = @"SELECT *
                                                          FROM (SELECT A.*,
                                                                       ROW_NUMBER ()
                                                                          OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC)
                                                                          RN
                                                                  FROM EDB0010 A)
                                                         WHERE RN = 1";
                        DataTable dtEDB0010 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strEDB0010, null).Tables[0];
                        if (dtEDB0010 != null && dtEDB0010.Rows.Count > 0)
                        {
                            //胎胚
                            DataRow[] Brow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["BUITNBR"] + "'");
                            if (Brow.Length > 0)
                                dtSTX.Rows[0]["BUITDSC"] = Brow[0]["ITDSC"].ToString();
                            //硫化
                            DataRow[] Crow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["CUITNBR"] + "'");
                            if (Crow.Length > 0)
                                dtSTX.Rows[0]["CUITDSC"] = Crow[0]["ITDSC"].ToString();
                        }
                        gtitnbr = dtSTX.Rows[0]["BUITNBR"].ToString();
                        gtitdsc = dtSTX.Rows[0]["BUITDSC"].ToString();
                        itnbr = dtSTX.Rows[0]["CUITNBR"].ToString();
                        itdsc = dtSTX.Rows[0]["CUITDSC"].ToString();
                    }
                    else
                        return "8";
                }
                else
                {
                    gtitnbr = dtPAD0401.Rows[0]["BUITNBR"].ToString();
                    gtitdsc = dtPAD0401.Rows[0]["BUITDSC"].ToString();
                    itnbr = dtPAD0401.Rows[0]["CUITNBR"].ToString();
                    itdsc = dtPAD0401.Rows[0]["CUITDSC"].ToString();
                }
                //
                string strLTA0001 = @" SELECT * FROM LTA0001 " +
                                @" WHERE BARCODE='" + str[0] + "' ";
                DataTable dtLTA0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strLTA0001, null).Tables[0];
                if (dtLTA0001.Rows.Count == 0)
                    return "5";
                string gtitnbrL = dtLTA0001.Rows[0]["BUITNBR"].ToString();
                if (gtitnbr != gtitnbrL)
                    return "Err^2^" + gtitnbr + "^" + gtitnbrL;
                //
                string strDelete = "DELETE FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
                //
                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
                                   @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[3] + "','" + str[4] + "','" + str[0] + "', " +
                                   @" '" + gtitnbr + "','" + itnbr + "','" + wrginflag + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                                   @" '2')";
                //
                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
                    @"EVNDAT, " +
                    @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
                    @"ENT_USER_ID,ENT_DT, " +
                    @"UPD_USER_ID,UPD_DT, " +
                    @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
                    @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'" + str[3] + "','" + str[4] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"'N','2' )";
                //
                string desc = "由" + str[1] + "-" + str[2] + "转移到" + str[3] + "-" + str[4];
                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC) VALUES( " +
                                @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[3] + "', '" + str[4] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
                                @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'1','" + desc + "')";
                //
                ArrayList list = new ArrayList();
                list.Add(strDelete);
                list.Add(strInsert);
                list.Add(strSTG0005);
                list.Add(sql_Up);
                int j = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (j > 0)
                    return "1";
                else
                    return "6";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "7";
            }
        }
    }
}
