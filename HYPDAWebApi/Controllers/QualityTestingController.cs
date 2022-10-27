using System;
using System.Web.Http;
using System.Data;
using log4net.Util;
using HYPDAWebApi.DBHelper;
using System.Collections;
using HYPDAWebApi.App_Data;
using HYPDAWebApi.Models.ViewModel;
using System.Globalization;

namespace HYPDAWebApi.Controllers
{
    /// <summary>
    /// 质检
    /// </summary>
    public class QualityTestingController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";

        /// <summary>
        /// 获取不良CODE
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/A_FINID_GETCODE")]
        public DataSet A_FINID_GETCODE(string DIV)
        {
            
            try
            {
                DataSet ds = new DataSet();
                string sql = string.Format(@"SELECT * FROM QMA0001 WHERE FAC='" + FAC + "'AND USEYN='Y' AND DIV LIKE '%" + DIV + "%'  ORDER BY  DIV,BCOD ");
                ds = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql,null);
                return ds;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        /// <summary>
        /// 获取不良CODE，根据修理方式1热补2打磨筛选，1时筛选1,3，，，2时筛选2,3
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/A_FINID_GETCODE1")]
        public DataSet A_FINID_GETCODE1(string DIV, string OTHDIV)
        {
            
            try
            {
                DataSet ds = new DataSet();
                string sql = string.Format(@"SELECT * FROM QMA0001 WHERE FAC='" + FAC + "'AND USEYN='Y' AND DIV = '" + DIV + "' AND ( OTHDIV ='" + OTHDIV + "' OR OTHDIV ='3' OR OTHDIV ='5') ORDER BY  DIV,BCOD ");
                ds = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql,null);
                return ds;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        /// 获取报废CODE
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/A_FINID_GETCODE_SCRAP")]
        public DataSet A_FINID_GETCODE_SCRAP(string DIV)
        {
            
            try
            {
                DataSet ds = new DataSet();
                string sql = string.Format(@"SELECT * FROM QMA0001 WHERE FAC='" + FAC + "'AND USEYN='Y' AND DIV LIKE '%" + DIV + "%' AND (OTHDIV = '4' OR OTHDIV = '5' OR OTHDIV = '7') ORDER BY  DIV,BCOD ");
                ds = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null);
                return ds;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        /// <summary>
        /// 热补自检
        /// </summary>
        /// <param name="BARCODE">热补扫描条码</param>
        /// <param name="DIV">种类区分</param>
        /// <param name="WBAN">修理班组</param>
        /// <param name="USERNAME">修理人工号</param>
        /// <param name="USERNAME">修理深度</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/A_HotRepair_Check")]
        public string A_HotRepair_Check(string BARCODE, string ITNBR, string ITDSC, string HOT, string TEMP, string MPA, string USERNAME, string CmbDIV, string mchid)
        {
            
            DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {
                //判断热补自检是否存在该条码
                string QMA0115_sql = @"SELECT * FROM QMA0115 WHERE BARCODE='" + BARCODE + "'";
                DataSet dts1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,QMA0115_sql, null);
                if (dts1 != null && dts1.Tables.Count > 0)
                {
                    if (dts1.Tables[0].Rows.Count > 0)
                    {
                        return "1";
                    }

                }



                ArrayList list = new ArrayList();
                //根据BARCODE更新成品胎不良登记表中修理信息
                string QMA0115_insert = @"INSERT INTO QMA0115 (ID,FAC,BARCODE,MCHID,ITNBR,ITDSC,RBTIM,RBTEMP,RBMPA,RBDIV,ETIM,ENAME) VALUES (" +
                                        @"                   sys_guid(),'07','" + BARCODE + "','" + mchid + "','" + ITNBR + "','" + ITDSC + "'," +
                                        @"                   '" + HOT + "','" + TEMP + "','" + MPA + "','" + CmbDIV + "'," +
                                        @"                   TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd HH24:mi:ss'),'" + USERNAME + "')";

                list.Add(QMA0115_insert);




                OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,list);
                return "3";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "4";
            }
        }

        /// <summary>
        /// 热补登记，登记修理日期、修理班组、修理班次、修理人、修理时间
        /// </summary>
        /// <param name="BARCODE">热补扫描条码</param>
        /// <param name="DIV">种类区分</param>
        /// <param name="WBAN">修理班组</param>
        /// <param name="USERNAME">修理人工号</param>
        /// <param name="USERNAME">修理深度</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/A_HotRepair_Regist")]
        public string A_HotRepair_Regist(string BARCODE, string DIV, string WBAN, string USERNAME, string sPdeep)
        {
            
            DataRow row = SHIFT.GetShift(DateTime.Now);
            try
            {
                //判断成品胎不良登记表中是否存在该条码
                string QMA0101_sql = @"SELECT * FROM QMA0101 WHERE FAC='" + FAC + "' AND DIV='" + DIV + "' AND BARCODE='" + BARCODE + "'";
                DataTable dt_QMA0101 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,QMA0101_sql, null).Tables[0];
                if (dt_QMA0101.Rows.Count == 0)
                    return "1";

                //判断成品胎修理履历表中是否存在该条码,以及是否质检有最新判定（必须有质检最新判定且没被修理过）
                string QMA0103_sql = @"SELECT * FROM QMA0103 WHERE FAC='" + FAC + "' AND DIV='" + DIV + "' AND BARCODE='" + BARCODE + "' AND RWAY = '1' AND PIDAT IS NULL";//热补区分
                DataTable dt_QMA0103 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,QMA0103_sql, null).Tables[0];
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
                                        @"               AND BARCODE='" + BARCODE + "' AND RWAY = '1'";
                list.Add(QMA0103_update);

                OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,list);
                return "3";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "4";
            }
        }

        /// <summary>
        ///完成品获取轮胎记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/A_SDS_BARINFO")]
        public DataTable A_SDS_BARINFO(string BARCODE)
        {
            DataTable dt = null;
            try
            {
                
                dt = new DataTable();
                string sql = @"SELECT 
                      BARCODE,CUDAT ,CUTIM, CUSHT , CUBAN, CUMCH ,CUNAM, LR, CUITNBR ,  CUITDSC, STATE, CUSTATE,
                      MODCOD, SLECOD, WGRES , UFRES, DBRES, XRES, SDSRES, UFCNT, DBCNT, CUSQTY, 
                      CUDQTY , CUBQTY, CUIQTY , CUSMYN , BUITNBR, BUITDSC ,BUDAT , BUTIM, BUSHT , BUBAN , 
                      BUMCH , BUNAM , WYYN , SEWT , REWT, BUSQTY , BUDQTY , BUBQTY , BUIQTY , BUSMYN ,
                      TRLOTID , SWLOTID , BDLOTID, SBLOTID1, SBLOTID2, CCLOTID1 , CCLOTID2, ILLOTID, CPLOTID, 
                      BARLAT , BARANG, BARLR, CUCOTCOD, BUCOTCOD , BOMVER, NORMALYN,TRIMMCH
                      FROM 
                      LTA0001  WHERE  BARCODE='" + BARCODE + "'";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql,null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


        /// <summary>
        /// 直接废品判定
        /// </summary>
        /// <param name="BARCODE">条码</param>
        /// <param name="USENAME">操作人</param>
        /// <param name="CODE">报废CODE</param>
        /// <param name="DIV">1外观报废  2UF报废 3DB报废 4X光报废 5全息报废</param>
        /// <returns>0条码不存在于系统  1成功  2失败,已被判定  3未知错误 </returns>
        [HttpGet]
        [Route("api/B_FINAL_RESULT")]
        public string B_FINAL_RESULT(string BARCODE, string USENAME, string CODE, string DIV, string WBAN)
        {
            try
            {
                if (USENAME.Trim() == "" || USENAME.Trim().Length < 1)
                {
                    return "判定失败！判定人不可为空。";
                }
                string sORGANIZATION_ID = string.Empty;//组织ID 默认83
                string sTIRE_BARCODE = string.Empty;//条码号
                string sTIRE_NUMBER = string.Empty;//胎号
                string sITEM_NUM = string.Empty;//物料代码
                string sITEM_DESC = string.Empty;//物料描述
                string sWEEK_NO = string.Empty;//周牌号
                string sOFFLINE_BASE = string.Empty;//下线基地
                string sPRODUCTION_DATE = string.Empty;//生产日期
                string sSPECI_MODELS = string.Empty;//规格型号
                string sWORKSHOP_CODE = string.Empty;//生产车间
                string sPRODUCTION_TYPE = string.Empty;//产品类别
                string sACTUAL_WEIGHT = string.Empty;//胎胚实绩重量
                string sTIRE_STATUS = string.Empty;//DI-正品 FP:废次品 BL:冻结
                string sKIND = string.Empty;//轮胎性质，华阳一级，二级
                string sREWORK_FLAG = string.Empty;//返修标志  默认1-正常
                string sSTOCK_DIV = string.Empty;//仓库类别，默认空
                string sRFIDTAG = string.Empty;//轮胎RFID

                DataRow ROW = SHIFT.GetShift(DateTime.Now);
                DataTable dt = new DataTable();
                ArrayList sqlList = new ArrayList();
                
                //条码不能重复判废，即使不同的div也不能。2017-10-27 李磊说明
                string sChkSql = "SELECT * FROM QMA0101 WHERE BARCODE='" + BARCODE + "' AND AYN = 'C'";
                DataTable dtchk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sChkSql, null).Tables[0];
                if (dtchk.Rows.Count > 0)
                {
                    return "失败,已被判废！";
                }
                string sql = "";
                string dbdt = @"SELECT BARCODE,
                                       CUDAT,
                                       CUTIM,
                                       CUSHT,
                                       CUBAN,
                                       CUMCH,
                                       CUNAM,
                                       LR,
                                       CUITNBR,
                                       CUITDSC,
                                       STATE,
                                       CUSTATE,
                                       MODCOD,
                                       SLECOD,
                                       WGRES,
                                       UFRES,
                                       DBRES,
                                       XRES,
                                       SDSRES,
                                       UFCNT,
                                       DBCNT,
                                       CUSQTY,
                                       CUDQTY,
                                       CUBQTY,
                                       CUIQTY,
                                       CUSMYN,
                                       BUITNBR,
                                       BUITDSC,
                                       BUDAT,
                                       BUTIM,
                                       BUSHT,
                                       BUBAN,
                                       BUMCH,
                                       BUNAM,
                                       WYYN,
                                       SEWT,
                                       REWT,
                                       BUSQTY,
                                       BUDQTY,
                                       BUBQTY,
                                       BUIQTY,
                                       BUSMYN,
                                       TRLOTID,
                                       SWLOTID,
                                       BDLOTID,
                                       SBLOTID1,
                                       SBLOTID2,
                                       CCLOTID1,
                                       CCLOTID2,
                                       ILLOTID,
                                       CPLOTID,
                                       BARLAT,
                                       BARANG,
                                       BARLR,
                                       CUCOTCOD,
                                       BUCOTCOD,
                                       BOMVER,
                                       NORMALYN
                                  FROM LTA0001 
                          WHERE BARCODE='" + BARCODE + "' ";
                DataTable dtbarmes = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,dbdt, null).Tables[0];


                if (dtbarmes.Rows.Count > 0)
                {
                    //2020-08-24 JOE 物流以及大数据部要求如果轮胎已入新WMS库，则不允许报废
                    if (dtbarmes.Rows[0]["CUSMYN"].ToString() == "I")
                        return "轮胎已入库，无法判废，是否保留？";
                    //2020-09-10 JOE 若轮胎从物流出库，则不允许报废
                    if (dtbarmes.Rows[0]["CUSMYN"].ToString() == "O")
                        return "轮胎已出库，判废失败！";
                    //string sqlm = "SELECT * FROM QMA0101 WHERE BARCODE='" + BARCODE + "'";// DIV='" + DIV + "' AND  AND AYN='C' 
                    string sqlm = "SELECT * FROM QMA0101 WHERE BARCODE='" + BARCODE + "' AND DIV = '" + DIV + "'";// DIV='" + DIV + "' AND  AND AYN='C' 
                    LogHelper.Debug("B_FINAL_RESULT:" + sqlm);//2021-10-25
                    DataTable dtm = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlm, null).Tables[0];
                    if (dtm.Rows.Count == 0)
                    {
                        string CUTIM = "";
                        string CUDAT = "";
                        if (string.IsNullOrEmpty(dtbarmes.Rows[0]["CUTIM"].ToString()))
                            CUTIM = "NULL";
                        else
                            CUTIM = "to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["CUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";

                        if (string.IsNullOrEmpty(dtbarmes.Rows[0]["CUDAT"].ToString()))
                            CUDAT = "NULL";
                        else
                            CUDAT = "to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["CUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd')";
                        //插入报废记录
                        sql = @"INSERT INTO QMA0101(
                            ID,FAC,DIV,IDAT,IBAN, 
                            ISHT,INAM,ITIM,COD,CIDAT,
                            CIBAN,CISHT,CINAM,CITIM,BUITNBR,
                            BUITDSC,CUITNBR,CUITDSC,CCOD,BUMCH,
                            BUDAT,BUTIM,
                            BUSHT,BUBAN,BUNAM,CUMCH,
                            CUDAT,CUTIM,
                            CUSHT,CUBAN, LR,BARCODE,AYN, 
                            STWT, REWT,MODCOD, SLECOD,STATE)
                            VALUES 
                            (SYS_GUID(),'" + FAC + "','" + DIV + "',to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),'" + WBAN + @"',
                            '" + ROW["WSHT"] + "','" + USENAME + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + CODE + "',to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
                            '" + WBAN + "','" + ROW["WSHT"] + "','" + USENAME + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dtbarmes.Rows[0]["BUITNBR"] + @"',
                            '" + dtbarmes.Rows[0]["BUITDSC"] + "', '" + dtbarmes.Rows[0]["CUITNBR"] + "','" + dtbarmes.Rows[0]["CUITDSC"] + "', '" + CODE + "','" + dtbarmes.Rows[0]["BUMCH"] + @"',
                            to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
                            '" + dtbarmes.Rows[0]["BUSHT"] + "',  '" + dtbarmes.Rows[0]["BUBAN"] + "',  '" + dtbarmes.Rows[0]["BUNAM"] + "', '" + dtbarmes.Rows[0]["CUMCH"] + @"',
                            " + CUDAT + "," + CUTIM + @",
                            '" + dtbarmes.Rows[0]["CUSHT"] + "',  '" + dtbarmes.Rows[0]["CUBAN"] + "', '" + dtbarmes.Rows[0]["LR"] + "','" + BARCODE + @"','C',
                            '" + dtbarmes.Rows[0]["SEWT"] + "',  '" + dtbarmes.Rows[0]["REWT"] + "', '" + dtbarmes.Rows[0]["MODCOD"] + "',  '" + dtbarmes.Rows[0]["SLECOD"] + @"', '" + dtbarmes.Rows[0]["CUSTATE"] + "')";
                        sqlList.Add(sql);




                        //2021-01-08 将报废物料插入基地下线表  xuejl

                        ////////                    string sqlSDA = @"INSERT INTO SDA0013(ID,FAC,BARCODE,ITNBR,ITDSC,
                        ////////                                                    SCANDAT,SCANTIM,SCANSHT,STS,IF_FLAG,STOCKNO) " +
                        ////////                                  "VALUES(SYS_GUID(),'" + FAC + "','" + BARCODE + "','" +
                        ////////                                  dtbarmes.Rows[0]["CUITNBR"] + "','" + dtbarmes.Rows[0]["CUITDSC"] + "',TRUNC(SYSDATE+9/24),SYSDATE,'" +
                        ////////                                  ROW["WSHT"] + "','C','N','NG')";
                        ////////                    sqlList.Add(sqlSDA);
                        string sSql = string.Empty;
                        sSql += "SELECT *  FROM ( ";
                        sSql += "SELECT  AAA.*,BBB.RFIDTAG  FROM (";
                        sSql += " SELECT AB.*, CD.ATTRVAL, ROWNUM RN ";
                        sSql += "          FROM (SELECT A.ID, ";
                        sSql += "                       A.BARCODE, ";
                        sSql += "                       A.STS,A.STOCKNO, ";
                        sSql += "                       B.CUITNBR, B.CUITDSC,NVL(B.CUGRADE,'606') CUGRADE,CUSMYN,";
                        sSql += "                       B.CUTIM, ";
                        sSql += "                       C.SPEC, ";
                        sSql += "                       B.BUITNBR ITNBR, ";
                        sSql += "                       ERPITNBR ";
                        sSql += "                  FROM SDA0013 A, ";
                        sSql += "                       LTA0001 B, ";
                        sSql += "                       (SELECT * ";
                        sSql += "                          FROM (SELECT A.*, ";
                        sSql += "                                       ROW_NUMBER () ";
                        sSql += "                                       OVER ( ";
                        sSql += "                                          PARTITION BY ITNBR ";
                        sSql += "                                          ORDER BY TO_NUMBER (VERSION) DESC) ";
                        sSql += "                                          RN ";
                        sSql += "                                  FROM EDB0010 A ";
                        sSql += "                                 WHERE ITGRPCOD = 'FERT') ";
                        sSql += "                         WHERE RN = 1) C ";
                        sSql += "                 WHERE     A.BARCODE = B.BARCODE ";
                        sSql += "                       AND B.CUITNBR = C.ITNBR(+) ";
                        sSql += "                       AND A.IF_FLAG = 'N') AB, ";
                        sSql += "               (SELECT ITNBR, ATTRVAL ";
                        sSql += "                  FROM (SELECT * ";
                        sSql += "                          FROM (SELECT A.*, ";
                        sSql += "                                       ROW_NUMBER () ";
                        sSql += "                                       OVER ( ";
                        sSql += "                                          PARTITION BY ITNBR ";
                        sSql += "                                          ORDER BY TO_NUMBER (VERSION) DESC) ";
                        sSql += "                                          RN ";
                        sSql += "                                  FROM EDB0010 A ";
                        sSql += "                                 WHERE ITGRPCOD = 'HALB') ";
                        sSql += "                         WHERE RN = 1) A, ";
                        sSql += "                       EDB0015 B ";
                        sSql += "                 WHERE     A.ID = B.ITEMID(+) ";
                        sSql += "                       AND A.ITTYPECOD = 'GT' ";
                        sSql += "                       AND B.ATTRCOD(+) = 'M18') CD ";
                        sSql += "         WHERE AB.ITNBR = CD.ITNBR(+))AAA ";
                        sSql += " LEFT JOIN (SELECT  BARCODE,RFIDTAG FROM LTA0006)BBB";
                        sSql += " ON AAA.BARCODE=BBB.BARCODE ";
                        sSql += "  ) WHERE RN < 2000 AND BARCODE='" + BARCODE + "'";
                        DataTable dtERP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];

                        foreach (DataRow dr in dtERP.Rows)
                        {
                            sORGANIZATION_ID = "83";
                            sTIRE_BARCODE = BARCODE;
                            sTIRE_NUMBER = "N/A";
                            //由于EDB0010.ITNBR字段属于华阳MES内部管理的物料代码，这个地方需要把ERP代码接口到物流园
                            sITEM_NUM = dr["ERPITNBR"].ToString();
                            if (!string.IsNullOrEmpty(dr["CUITDSC"].ToString()) && dr["CUITDSC"].ToString().Contains("试验胎"))//若包含“试验胎”，则去除掉
                                sITEM_DESC = dr["CUITDSC"].ToString().Substring(0, dr["CUITDSC"].ToString().IndexOf("试验胎") - 1);
                            else
                                sITEM_DESC = dr["CUITDSC"].ToString();
                            if (!string.IsNullOrEmpty(dr["CUTIM"].ToString()))
                            {
                                DateTime dt1 = Convert.ToDateTime(dr["CUTIM"].ToString());
                                sWEEK_NO = weekno(dt1);
                            }
                            else
                                sWEEK_NO = "0000";
                            sOFFLINE_BASE = "07";
                            if (!string.IsNullOrEmpty(dr["CUTIM"].ToString()))
                            {
                                sPRODUCTION_DATE = Convert.ToDateTime(dr["CUTIM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                                sPRODUCTION_DATE = "2000-01-01 01:01:01";

                            sSPECI_MODELS = dr["SPEC"].ToString();
                            sWORKSHOP_CODE = "HYBG";
                            sPRODUCTION_TYPE = "CB";//CB-半钢胎
                            sACTUAL_WEIGHT = string.IsNullOrEmpty(dr["ATTRVAL"].ToString()) ? "0" : dr["ATTRVAL"].ToString();
                            //sTIRE_STATUS = "合格";
                            // FP-报废
                            sTIRE_STATUS = "FP";
                            sKIND = dr["CUGRADE"].ToString();
                            sREWORK_FLAG = "1";
                            sSTOCK_DIV = "";
                            sRFIDTAG = dr["RFIDTAG"].ToString();//轮胎rfid
                            string sInsWms = "INSERT INTO OFFLINE_BASE_INFO_TO_WMS(OFFLINE_ID,ORGANIZATION_ID,TYRE_BARCODE,TYRE_NUM,ITEM_NUM,ITEM_DESC,DOT,OFFLINE_BASE,PRODUCTION_DATE,SPECI_MODELS,";
                            sInsWms += "WORKSHOP_CODE,PRODUCTION_TYPE,ACTUAL_WEIGHT,ATTRIBUTE_CATEGORY,KIND,REWORK_FLAG,STOCK_DIV,MES_CREATE_TIME,WMS_HANDLE_TIME,WMS_HANDLE_STATE,WMS_HANDLE_MSG,TYRE_RFID) VALUES (";
                            sInsWms += "SYS_GUID()," + sORGANIZATION_ID + ",'" + sTIRE_BARCODE + "','" + sTIRE_NUMBER + "','" + sITEM_NUM + "','" + sITEM_DESC + "','" + sWEEK_NO + "','" + sOFFLINE_BASE + "',TO_DATE('" + sPRODUCTION_DATE + "','YYYY-MM-DD HH24:MI:SS'),";
                            sInsWms += "'" + sSPECI_MODELS + "','" + sWORKSHOP_CODE + "','" + sPRODUCTION_TYPE + "'," + sACTUAL_WEIGHT + ",'" + sTIRE_STATUS + "','" + sKIND + "','" + sREWORK_FLAG + "','" + sSTOCK_DIV + "',SYSDATE,'','1','','" + sRFIDTAG + "')";
                            int iRst = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionStringWMS2,CommandType.Text, sInsWms,null);


                        }

                        //如果当前轮胎是已入库状态则不再插入中间表
                        //int i =  OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,sql);
                        int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqlList);
                        if (i > 0)
                        {
                            string updateSql = @"update pac0401 set bdqty = bdqty+1 where wdate=to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["CUDAT"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd')
                          and mchid='" + dtbarmes.Rows[0]["CUMCH"].ToString() + "' and itnbr = '" + dtbarmes.Rows[0]["CUITNBR"].ToString() + "' and lr = '" + dtbarmes.Rows[0]["LR"].ToString() +
                           "' and sht = '" + dtbarmes.Rows[0]["CUSHT"].ToString() + "' and wnam='" + dtbarmes.Rows[0]["CUNAM"].ToString() + "'";
                             OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,updateSql,null);
                           

                            string sqlJBL = @"UPDATE QMA0104 SET DIYN = 'Y',DINAM = '" + USENAME + "',DITIM = SYSDATE,DIDAT = SYSDATE,RESULT = '3' WHERE BARCODE = '" + BARCODE + "' AND DIYN = 'N'";
                             OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,sqlJBL, null);
                            string sqlLTA0001UP = @"UPDATE LTA0001 SET CUCOTCOD = '' WHERE BARCODE = '" + BARCODE + "'";
                            OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sqlLTA0001UP, null);
                            return "判废成功！";
                        }
                        else
                            return "判废失败！";
                    }
                    else
                    {
                        DataRow[] row = dtm.Select("AYN='C'", "CITIM DESC");
                        if (row.Length > 0)
                        {
                            string div = GetDIVName(row[0]["DIV"].ToString());
                            return "失败,已被" + div + "判废！";
                        }
                        else
                        {
                            DataRow[] row1 = dtm.Select("1=1", "CITIM DESC");
                            sql = @"UPDATE QMA0101 
                        SET IDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),IBAN='" + WBAN + "',ISHT='" + ROW["WSHT"] + "',INAM='" + USENAME + @"',ITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
                           CIDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),CIBAN='" + WBAN + "',CISHT='" + ROW["WSHT"] + "',CINAM='" + USENAME + @"',CITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
                            AYN='C',CCOD='" + CODE + "',DIV='" + DIV + "',COD='" + CODE + @"'
				        WHERE BARCODE='" + BARCODE + "' AND ID='" + row1[0]["ID"] + "'";
                            int i =  OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,sql, null);
                            if (i > 0)
                            {
                                string sqlJBL1 = @"UPDATE QMA0104 SET DIYN = 'Y',DINAM = '" + USENAME + "',DITIM = SYSDATE,DIDAT = SYSDATE,RESULT = '3' WHERE BARCODE = '" + BARCODE + "' AND DIYN = 'N'";
                                 OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,sqlJBL1, null);
                                string sqlLTA0001UP = @"UPDATE LTA0001 SET CUCOTCOD = '' WHERE BARCODE = '" + BARCODE + "'";
                                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sqlLTA0001UP, null);
                                return "判废成功！";
                            }

                            else
                                return "判废失败！";
                        }
                    }
                }
                else
                {
                    return "条码不存在于系统！";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "未知错误";
            }
        }
        #region 不良



        [HttpGet]
        [Route("api/GetDIVName")]
        public string GetDIVName(string DIV)
        {
            string divName = string.Empty;
            switch (DIV)
            {
                case "1":
                    divName = "外观";
                    break;
                case "2":
                    divName = "UF";
                    break;
                case "3":
                    divName = "DB";
                    break;
                case "4":
                    divName = "X光";
                    break;
                case "5":
                    divName = "全息";
                    break;
                default:
                    divName = "外观";
                    break;
            }
            return divName;
        }



        #endregion
        public string weekno(DateTime dt)
        {
            //int iWeekStarted = 1;
            //GregorianCalendar gc = new GregorianCalendar();
            //iWeekStarted = gc.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
            //if (iWeekStarted < 10)
            //    return "0" + iWeekStarted.ToString() + DateTime.Now.Year.ToString().Substring(2, 2);
            //else
            //    return iWeekStarted.ToString() + DateTime.Now.Year.ToString().Substring(2, 2);
            int iWeekStarted = 1;
            GregorianCalendar gc = new GregorianCalendar();
            iWeekStarted = gc.GetWeekOfYear(dt, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);
            if (iWeekStarted < 10)
                return "0" + iWeekStarted.ToString() + dt.Year.ToString().Substring(2, 2);
            else
            {
                if (iWeekStarted > 50 && dt.Month == 1)//若获取到的周大于50周，但月份却是1月，则代表是上一年的最后一周，年份减一年
                    return iWeekStarted.ToString() + dt.AddYears(-1).Year.ToString().Substring(2, 2);
                else
                    return iWeekStarted.ToString() + dt.Year.ToString().Substring(2, 2);
            }
        }

        /// <summary>
        /// 外观修理，登记修理CODE，更新维修结束时间，结束时间（MES中登记修理结果）
        /// 外观人工打磨返工结果(合格 or 报废)告知物流线流向(外观人工打磨或者热补)
        /// </summary>
        /// <param name="BARCODE">条码</param>
        /// <param name="CODE">外观 CODE</param>
        /// <param name="USERNAME">修理人工号</param>
        /// <param name="RWAY">修理方式 1.热补 2.打磨</param>
        /// <param name="STATION">修理工位</param>
        /// <returns>1成功 2失败：条码不存在 3.未知错误 4.员工不存在 5.此条码已判废，不可再登记修理 6.登记失败</returns>
        [HttpGet]
        [Route("api/B_RGDM_REWORK_RESULT")]
        public string B_RGDM_REWORK_RESULT(string BARCODE, string CODE, string LOGINNAME, string RWAY)
        {
            
            DataRow ROW = SHIFT.GetShift(DateTime.Now);
            DataTable dt1 = new DataTable();
            DataTable dtn = new DataTable();
            string sql = "";
            string sql1 = "";
            string PITIM = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + LOGINNAME + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlEMP, null).Tables[0];
                if (dtEMP.Rows.Count == 0)
                    return "4";

                string dbdt = @"SELECT 
                          A.BARCODE,CUDAT ,CUTIM, CUSHT , CUBAN, CUMCH , LR, C.ITNBR AS CUITNBR , C.ITDSC AS CUITDSC, CUSTATE, 
                          MODCOD, SLECOD, WGRES , UFRES, DBRES, XRES, SDSRES, UFCNT, DBCNT, CUSQTY, 
                          CUDQTY , CUBQTY, CUIQTY , CUSMYN , BUITNBR, BUITDSC ,BUDAT , BUTIM, BUSHT , BUBAN , 
                          BUMCH , BUNAM , WYYN , SEWT , REWT, BUSQTY , BUDQTY , BUBQTY , BUIQTY , BUSMYN ,
                          TRLOTID , SWLOTID , BDLOTID, SBLOTID1, SBLOTID2, CCLOTID1 , CCLOTID2, ILLOTID, CPLOTID, 
                          BARLAT , BARANG, BARLR, CUCOTCOD, BUCOTCOD , BOMVER, NORMALYN
                          FROM 
                          LTA0001 A
                          LEFT JOIN (SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) C
                          ON A.CUITNBR = C.ITNBR
                          WHERE A.FAC='" + FAC + "' AND A.BARCODE='" + BARCODE + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,dbdt, null).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    string CUTIM = "";
                    string CUDAT = "";
                    if (string.IsNullOrEmpty(dt.Rows[0]["CUTIM"].ToString()))
                        CUTIM = "NULL";
                    else
                        CUTIM = "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";

                    if (string.IsNullOrEmpty(dt.Rows[0]["CUDAT"].ToString()))
                        CUDAT = "NULL";
                    else
                        CUDAT = "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUDAT"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";

                    //质检要求人工打磨判定不允许超过3次，热补判定不允许超过1次
                    //2019-01-08 JOE 现场要求 更改为热补判定 相同缺陷只能一次，不同缺陷≤3次
                    string sSqln = "SELECT * FROM QMA0103 WHERE BARCODE = '" + BARCODE + "' AND RWAY = '" + RWAY + "'";
                    dtn = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqln, null).Tables[0];
                    if (dtn != null && dtn.Rows.Count > 0)
                    {
                        if ((RWAY == "2" && dtn.Rows.Count == 3))//打磨
                            return "7";
                        if (RWAY == "1")//热补
                        {
                            //int iRbcnt = 0;
                            for (int j = 0; j < dtn.Rows.Count; j++)
                            {
                                if (dtn.Rows[j]["BCOD"].ToString() == CODE)//若已存在相同缺陷
                                    return "7";
                            }
                            DataTable dtRb = new DataTable();
                            DataView dvRb = new DataView(dtn);
                            dtRb = dvRb.ToTable(true, "BCOD");
                            if (dtRb.Rows.Count > 3 && dtRb != null)
                                return "7";
                        }
                    }

                    string sqlm = "SELECT * FROM QMA0101 WHERE BARCODE='" + BARCODE + "'";// AND DIV='1'
                    dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlm, null).Tables[0];
                    //有不良，更新 无不良新增
                    if (dt1.Rows.Count > 0)
                    {
                        DataRow[] row = dt1.Select("AYN='C'");
                        if (row.Length > 0)
                            return "5";

                        DataRow[] rowB = dt1.Select("AYN='B' AND DIV = '1'");//
                        if (rowB.Length > 0 && rowB[0]["ACOD"].ToString() == CODE)//若轮胎不良且不良码一样，则不允许再判定
                            return "6";

                        DataRow[] rowD = dt1.Select("DIV='1'");
                        if (rowD.Length > 0)
                        {
                            //重新判定不良，更新相关信息
                            sql = @"UPDATE QMA0101 
                        SET IDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),IBAN='" + ROW["WBAN"] + "',ISHT='" + ROW["WSHT"] + "',INAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',ITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
                        RDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),RBAN='" + ROW["WBAN"] + "',RSHT='" + ROW["WSHT"] + "',RNAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',RTIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
                        AYN='B',PYN='N',BCOD='" + CODE + "',ACOD='" + CODE + "',COD='" + CODE + @"'
				        WHERE DIV='1' AND FAC='" + FAC + "' AND BARCODE='" + BARCODE + "'";
                        }
                        else
                        {
                            //登记修理病相
                            sql = @"INSERT INTO QMA0101(
                            ID,FAC,DIV,IDAT,IBAN, 
                            ISHT,INAM,ITIM,COD,BIDAT,
                            BIBAN,BISHT,BINAM,BITIM,BUITNBR,
                            BUITDSC,CUITNBR,CUITDSC,BCOD,BUMCH,
                            BUDAT,BUTIM,
                            BUSHT,BUBAN,BUNAM,CUMCH,
                            CUDAT,CUTIM,PYN,ACOD,
                            CUSHT,CUBAN, LR,BARCODE,AYN, 
                            STWT, REWT,MODCOD, SLECOD,STATE,
                            RDAT,RBAN,RSHT,RNAM,RTIM)
                            VALUES 
                            (SYS_GUID(),'" + FAC + "','1',to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),'" + ROW["WBAN"] + @"',
                            '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + CODE + "',to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
                            '" + ROW["WBAN"] + "','" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dt.Rows[0]["BUITNBR"] + @"',
                            '" + dt.Rows[0]["BUITDSC"] + "', '" + dt.Rows[0]["CUITNBR"] + "','" + dt.Rows[0]["CUITDSC"] + "', '" + CODE + "','" + dt.Rows[0]["BUMCH"] + @"',
                            to_date('" + Convert.ToDateTime(dt.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),to_date('" + Convert.ToDateTime(dt.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
                            '" + dt.Rows[0]["BUSHT"] + "',  '" + dt.Rows[0]["BUBAN"] + "',  '" + dt.Rows[0]["BUNAM"] + "', '" + dt.Rows[0]["CUMCH"] + @"',
                            " + CUDAT + "," + CUTIM + @",'N','" + CODE + @"',
                            '" + dt.Rows[0]["CUSHT"] + "',  '" + dt.Rows[0]["CUBAN"] + "', '" + dt.Rows[0]["LR"] + "','" + BARCODE + @"','B',
                            '" + dt.Rows[0]["SEWT"] + "',  '" + dt.Rows[0]["REWT"] + "', '" + dt.Rows[0]["MODCOD"] + "',  '" + dt.Rows[0]["SLECOD"] + @"', '" + dt.Rows[0]["CUSTATE"] + @"',
                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),'" + ROW["WBAN"] + "', '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'))";
                        }
                    }
                    else
                    {
                        //登记修理病相
                        sql = @"INSERT INTO QMA0101(
                            ID,FAC,DIV,IDAT,IBAN, 
                            ISHT,INAM,ITIM,COD,BIDAT,
                            BIBAN,BISHT,BINAM,BITIM,BUITNBR,
                            BUITDSC,CUITNBR,CUITDSC,BCOD,BUMCH,
                            BUDAT,BUTIM,
                            BUSHT,BUBAN,BUNAM,CUMCH,
                            CUDAT,CUTIM,PYN,ACOD,
                            CUSHT,CUBAN, LR,BARCODE,AYN, 
                            STWT, REWT,MODCOD, SLECOD,STATE,
                            RDAT,RBAN,RSHT,RNAM,RTIM)
                            VALUES 
                            (SYS_GUID(),'" + FAC + "','1',to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),'" + ROW["WBAN"] + @"',
                            '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + CODE + "',to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
                            '" + ROW["WBAN"] + "','" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dt.Rows[0]["BUITNBR"] + @"',
                            '" + dt.Rows[0]["BUITDSC"] + "', '" + dt.Rows[0]["CUITNBR"] + "','" + dt.Rows[0]["CUITDSC"] + "', '" + CODE + "','" + dt.Rows[0]["BUMCH"] + @"',
                            to_date('" + Convert.ToDateTime(dt.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),to_date('" + Convert.ToDateTime(dt.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
                            '" + dt.Rows[0]["BUSHT"] + "',  '" + dt.Rows[0]["BUBAN"] + "',  '" + dt.Rows[0]["BUNAM"] + "', '" + dt.Rows[0]["CUMCH"] + @"',
                            " + CUDAT + "," + CUTIM + @",'N','" + CODE + @"',
                            '" + dt.Rows[0]["CUSHT"] + "',  '" + dt.Rows[0]["CUBAN"] + "', '" + dt.Rows[0]["LR"] + "','" + BARCODE + @"','B',
                            '" + dt.Rows[0]["SEWT"] + "',  '" + dt.Rows[0]["REWT"] + "', '" + dt.Rows[0]["MODCOD"] + "',  '" + dt.Rows[0]["SLECOD"] + @"', '" + dt.Rows[0]["CUSTATE"] + @"',
                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),'" + ROW["WBAN"] + "', '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'))";
                    }
                    //插入一条修理履历
                    if (RWAY == "1")//若是质检判定热补不良
                    {
                        sql1 = @"INSERT INTO QMA0103( ID,FAC,DIV,RDAT,RBAN,RSHT,
                        RNAM,RTIM,BUITNBR,
                        BUITDSC,CUITNBR,CUITDSC,BCOD,CCOD,
                        BUMCH,BUDAT,BUTIM,BUSHT,
                        BUBAN,BUNAM,CUMCH,CUDAT,CUTIM,CUSHT,
                        BARCODE,AYN,PYN,RWAY,PINFO,STWT,REWT,UFGRD,
                        UFCNT,DBGRD,DBCNT,MODCOD,
                        SLECOD,DEPCOD,ENAM,ETIM,REMARK,STATE)
                        VALUES 
                        ('" + Guid.NewGuid() + "','" + FAC + "', '1', to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),'" + ROW["WBAN"] + "','" + ROW["WSHT"] + @"', 
                         '" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + PITIM + "','yyyy-MM-dd hh24:mi:ss') ,'" + dt.Rows[0]["BUITNBR"].ToString() + @"', 
                         '" + dt.Rows[0]["BUITDSC"].ToString() + "','" + dt.Rows[0]["CUITNBR"].ToString() + "' ,'" + dt.Rows[0]["CUITDSC"].ToString() + "','" + CODE + @"','', 
                         '" + dt.Rows[0]["BUMCH"].ToString() + "',to_date('" + Convert.ToDateTime(dt.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),to_date('" + Convert.ToDateTime(dt.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dt.Rows[0]["BUSHT"].ToString() + @"',
                         '" + dt.Rows[0]["BUBAN"].ToString() + "','" + dt.Rows[0]["BUNAM"].ToString() + "','" + dt.Rows[0]["CUMCH"].ToString() + "'," + CUDAT + "," + CUTIM + ",'" + dt.Rows[0]["CUSHT"].ToString() + @"',
                         '" + BARCODE + "','B','N','" + RWAY + "','','" + dt.Rows[0]["SEWT"].ToString() + "','" + dt.Rows[0]["REWT"].ToString() + @"','',
                         '','','','" + dt.Rows[0]["MODCOD"].ToString() + @"',
                         '" + dt.Rows[0]["SLECOD"].ToString() + @"','','',
                         '', '','" + dt.Rows[0]["CUSTATE"].ToString() + "' )";
                    }
                    else//若是质检判定人工打磨
                    {
                        sql1 = @"INSERT INTO QMA0103( ID,FAC,DIV,RDAT,RBAN,RSHT,
                        RNAM,RTIM,BUITNBR,
                        BUITDSC,CUITNBR,CUITDSC,BCOD,CCOD,
                        BUMCH,BUDAT,BUTIM,BUSHT,
                        BUBAN,BUNAM,CUMCH,CUDAT,CUTIM,CUSHT,
                        BARCODE,AYN,PYN,RWAY,PINFO,STWT,REWT,UFGRD,
                        UFCNT,DBGRD,DBCNT,MODCOD,
                        SLECOD,DEPCOD,ENAM,ETIM,REMARK,STATE)
                        VALUES 
                        ('" + Guid.NewGuid() + "','" + FAC + "', '1', to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),'" + ROW["WBAN"] + "','" + ROW["WSHT"] + @"', 
                         '" + dtEMP.Rows[0]["NAME"].ToString() + "',to_date('" + PITIM + "','yyyy-MM-dd hh24:mi:ss') ,'" + dt.Rows[0]["BUITNBR"].ToString() + @"', 
                         '" + dt.Rows[0]["BUITDSC"].ToString() + "','" + dt.Rows[0]["CUITNBR"].ToString() + "' ,'" + dt.Rows[0]["CUITDSC"].ToString() + "','" + CODE + @"','', 
                         '" + dt.Rows[0]["BUMCH"].ToString() + "',to_date('" + Convert.ToDateTime(dt.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),to_date('" + Convert.ToDateTime(dt.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dt.Rows[0]["BUSHT"].ToString() + @"',
                         '" + dt.Rows[0]["BUBAN"].ToString() + "','" + dt.Rows[0]["BUNAM"].ToString() + "','" + dt.Rows[0]["CUMCH"].ToString() + "'," + CUDAT + "," + CUTIM + ",'" + dt.Rows[0]["CUSHT"].ToString() + @"',
                         '" + BARCODE + "','B','N','" + RWAY + "','','" + dt.Rows[0]["SEWT"].ToString() + "','" + dt.Rows[0]["REWT"].ToString() + @"','',
                         '','','','" + dt.Rows[0]["MODCOD"].ToString() + @"',
                         '" + dt.Rows[0]["SLECOD"].ToString() + @"','','',
                         '', '','" + dt.Rows[0]["CUSTATE"].ToString() + "' )";
                    }
                    ArrayList list = new ArrayList();
                    list.Add(sql);
                    list.Add(sql1);
                    //更新MES表
                    int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,list);
                    if (i > 0)
                        return "1";
                    else
                        return "9";
                }
                else
                {
                    return "2";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "3";
            }
        }


        [HttpGet]
        [Route("api/ConfirmDBSSJBad")]
        public string ConfirmDBSSJBad(string sBarcode, string sITNBR, string sITDSC, string sUFPTIM, string sUFBADCOD, string sUFBADNAM, string sDBPTIM, string sDBBADCOD, string sDBBADNAM, string sUSERID)
        {
            
            DateTime dtNow = DateTime.Now;
            DataRow Row = SHIFT.GetShift(dtNow);
            DataTable dtRP = null;
            ArrayList sqllist = new ArrayList();
            try
            {
                string sSqlRP = "SELECT * FROM  QMA0113 WHERE BARCODE = '" + sBarcode + "' AND RPDAT IS NULL";
                dtRP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqlRP, null).Tables[0];
                if (dtRP != null && dtRP.Rows.Count > 0)
                {
                    return "1";
                }


                if (!string.IsNullOrEmpty(sDBPTIM))//若有DB不良
                {
                    string sSql1 = "INSERT INTO QMA0113(ID,FAC,BARCODE,DIV,MPDAT,MPTIM,MPBCOD,MPBNAM,RWAY,QANAM,QADAT,QASHT,QABAN,QATIM) VALUES (";
                    sSql1 += "sys_guid(),'07',";
                    sSql1 += "'" + sBarcode + "',";
                    sSql1 += "'3',";//区分
                    sSql1 += "'',";//设备判定日期暂时为空
                    sSql1 += "TO_DATE('" + Convert.ToDateTime(sDBPTIM).ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql1 += "'" + sDBBADCOD + "',";
                    sSql1 += "'" + sDBBADNAM + "',";
                    sSql1 += "'7',";//RWAY
                    sSql1 += "'" + sUSERID + "',";
                    sSql1 += "TO_DATE('" + Convert.ToDateTime(Row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql1 += "'" + Row["WSHT"] + "',";
                    sSql1 += "'',";//班组
                    sSql1 += "TO_DATE('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";
                    sSql1 += ")";
                    sqllist.Add(sSql1);
                }
                int i = 0;
                if (sqllist.Count > 0)
                {
                    i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                }
                if (i > 0)
                    return "OK";
                else
                    return "Err";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err";
            }
        }

        [HttpGet]
        [Route("api/ConfirmUBBad")]
        public string ConfirmUBBad(string sBarcode, string sITNBR, string sITDSC, string sUFPTIM, string sUFBADCOD, string sUFBADNAM, string sDBPTIM, string sDBBADCOD, string sDBBADNAM, string sUSERID)
        {
            
            DateTime dtNow = DateTime.Now;
            DataRow Row = SHIFT.GetShift(dtNow);
            DataTable dtRP = null;
            ArrayList sqllist = new ArrayList();
            try
            {

                string sSqlRP = "SELECT * FROM  QMA0113 WHERE BARCODE = '" + sBarcode + "' AND RPDAT IS NULL";
                dtRP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqlRP, null).Tables[0];
                if (dtRP != null && dtRP.Rows.Count > 0)
                {
                    return "1";
                }


                if (!string.IsNullOrEmpty(sUFPTIM))//若有UF不良
                {
                    string sSql = "INSERT INTO QMA0113(ID,FAC,BARCODE,DIV,MPDAT,MPTIM,MPBCOD,MPBNAM,RWAY,QANAM,QADAT,QASHT,QABAN,QATIM) VALUES (";
                    sSql += "sys_guid(),'07',";
                    sSql += "'" + sBarcode + "',";
                    sSql += "'2',";//区分
                    sSql += "'',";//设备判定日期暂时为空
                    sSql += "TO_DATE('" + Convert.ToDateTime(sUFPTIM).ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql += "'" + sUFBADCOD + "',";
                    sSql += "'" + sUFBADNAM + "',";
                    sSql += "'2',";//RWAY
                    sSql += "'" + sUSERID + "',";
                    sSql += "TO_DATE('" + Convert.ToDateTime(Row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql += "'" + Row["WSHT"] + "',";
                    sSql += "'',";//班组
                    sSql += "TO_DATE('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";
                    sSql += ")";
                    sqllist.Add(sSql);
                }
                if (!string.IsNullOrEmpty(sDBPTIM))//若有DB不良
                {
                    string sSql1 = "INSERT INTO QMA0113(ID,FAC,BARCODE,DIV,MPDAT,MPTIM,MPBCOD,MPBNAM,RWAY,QANAM,QADAT,QASHT,QABAN,QATIM) VALUES (";
                    sSql1 += "sys_guid(),'07',";
                    sSql1 += "'" + sBarcode + "',";
                    sSql1 += "'3',";//区分
                    sSql1 += "'',";//设备判定日期暂时为空
                    sSql1 += "TO_DATE('" + Convert.ToDateTime(sDBPTIM).ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql1 += "'" + sDBBADCOD + "',";
                    sSql1 += "'" + sDBBADNAM + "',";
                    sSql1 += "'2',";//RWAY
                    sSql1 += "'" + sUSERID + "',";
                    sSql1 += "TO_DATE('" + Convert.ToDateTime(Row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
                    sSql1 += "'" + Row["WSHT"] + "',";
                    sSql1 += "'',";//班组
                    sSql1 += "TO_DATE('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";
                    sSql1 += ")";
                    sqllist.Add(sSql1);
                }
                int i = 0;
                if (sqllist.Count > 0)
                {
                    i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                }
                if (i > 0)
                    return "OK";
                else
                    return "Err";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err";
            }
        }

        [HttpGet]
        [Route("api/FTWLOnline")]
        public string FTWLOnline(string sBarcode, string sITNBR, string sITDSC, string STS, string sRoutStockNo, string name)
        {
            try
            {
                
                string sql = @"INSERT INTO SDA0018(ID,FAC,BARCODE,ITNBR,ITDSC,
                                                    SCANDAT,SCANTIM,SCANSHT,STS,IF_FLAG,STOCKNO,THYNAM,THYTIM) " +
                                      "VALUES(SYS_GUID(),'07','" + sBarcode + "','" +
                                      sITNBR + "','" + sITDSC + "',TRUNC(SYSDATE+9/24),SYSDATE,'" +
                                      GetFacSht() + "','" + STS + "','N','" + sRoutStockNo + "','" + name + "',SYSDATE)";


                 OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,sql, null);
                return "OK";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err:" + ex;
            }
        }

        /// <summary>
        /// 判断班次
        /// </summary>
        /// <returns>班次</returns>
        public string GetFacSht()
        {
            int i = DateTime.Now.Hour;
            return i > 22 ? "3" : i > 14 ? "2" : i > 6 ? "1" : "3";
        }


        /// <summary>
        /// 外检最终判定
        /// </summary>
        /// <param name="ban">班组</param>
        /// <param name="name">用户</param>
        /// <param name="code">code</param>
        /// <param name="code">不良描述</param>
        /// <param name="code">合格，报废区分</param>
        /// <returns>是否成功</returns>
        [HttpGet]
        [Route("api/Get_JUDGE_OK")]
        public string Get_JUDGE_OK(string ban, string name, string code, string bdesc, string div)
        {
            if (name.Trim() == "" || name.Trim().Length < 1)
            {
                return "判定失败！判定人不可为空。";
            }
            
            DataRow ROW = SHIFT.GetShift(DateTime.Now);
            try
            {
                ArrayList list = new ArrayList();
                if (div == "BAOLIU")
                {
                    DataTable dtHold = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT BARCODE FROM QMA0104 WHERE FAC='07' AND DIYN = 'N' AND BARCODE = '" + code + "'", null).Tables[0];
                    if (dtHold != null && dtHold.Rows.Count > 0)
                    {
                        return "判定失败！该条码已经被保留。";
                    }

                    string Sqllta0001 = @"select * from LTA0001 where BARCODE = '" + code + "'";
                    DataTable dtlta0001 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,Sqllta0001, null).Tables[0];
                    string sql1 = "INSERT INTO QMA0104(ID,FAC,BARCODE,HODAT,HOTIM,HOSHT,HOBAN,HONAM,BUMCH,BUDAT,BUTIM,BUSHT,BUBAN,BUNAM,CUMCH,CUDAT,CUSHT,CUITNBR,CUITDSC,HOREN,DIYN)" +
                                          " VALUES('" + Guid.NewGuid().ToString() + "','07','" + code + "',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),TO_DATE('" +
                                          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + ROW["WSHT"].ToString() + "','" + ban + "','" + name + "','" +
                                          dtlta0001.Rows[0]["BUMCH"].ToString() + "',TO_DATE('" +
                                          Convert.ToDateTime(dtlta0001.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),TO_DATE('" +
                                          Convert.ToDateTime(dtlta0001.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" +
                                          dtlta0001.Rows[0]["BUSHT"].ToString() + "','" +
                                          dtlta0001.Rows[0]["BUBAN"].ToString() + "','" +
                                          dtlta0001.Rows[0]["BUNAM"].ToString() + "','" +
                                          dtlta0001.Rows[0]["CUMCH"].ToString() + "',TO_DATE('" +
                                          Convert.ToDateTime(dtlta0001.Rows[0]["CUDAT"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" +
                                          dtlta0001.Rows[0]["CUSHT"].ToString() + "','" +
                                          dtlta0001.Rows[0]["CUITNBR"].ToString() + "','" +
                                          dtlta0001.Rows[0]["CUITDSC"].ToString() + "','" +
                                          bdesc.Split('^')[0].ToString() + "','N')";
                    list.Add(sql1);
                    //查询SDA0013表中是否存在该条码，若不存在不给QMA0104_1发送数据
                    DataTable SDA0013_DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT * FROM SDA0013 WHERE BARCODE='" + code + "'", null).Tables[0];
                    if (SDA0013_DT != null && SDA0013_DT.Rows.Count > 0)
                    {
                        DataTable LTA0001_DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,"SELECT * FROM LTA0001 WHERE BARCODE='" + code + "'", null).Tables[0];
                        if (LTA0001_DT != null && LTA0001_DT.Rows.Count > 0)
                        {
                            // 若未退库则发送，已退库不发送
                            if (LTA0001_DT.Rows[0]["CUSMYN"].ToString() != "B" && LTA0001_DT.Rows[0]["CUSMYN"].ToString() != "O")
                            {
                                list.Add("INSERT INTO QMA0104_1(ID,FAC,BARCODE,DIV,TIMSTAMP,HSFLAG,REMARK) VALUES ('" + Guid.NewGuid() + "','07','" + code + "','H',SYSDATE,'N','" + bdesc + "')");
                            }
                        }

                    }

                    list.Add("UPDATE LTA0001 SET CUCOTCOD = 'HOLD' WHERE BARCODE ='" + code + "'");
                }
                else
                {

                    //判断热补有没有修理工确认
                    string sChkSql = "SELECT COUNT(*) CNT FROM QMA0103 WHERE BARCODE = '" + code + "' AND RWAY = '1' AND PIDAT IS NULL";
                    DataTable dtChk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sChkSql, null).Tables[0];
                    if (dtChk.Rows.Count > 0)
                    {
                        if (int.Parse(dtChk.Rows[0]["CNT"].ToString()) > 0)
                        {
                            return "[判定失败!]热补未确认,请修理工确认";
                        }
                    }

                    //判断人工打磨有没有修理工确认
                    string sChkSql1 = "SELECT COUNT(*) CNT FROM QMA0103 WHERE BARCODE = '" + code + "' AND RWAY = '2' AND PIDAT IS NULL";
                    DataTable dtChk1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sChkSql1, null).Tables[0];
                    if (dtChk1.Rows.Count > 0)
                    {
                        if (int.Parse(dtChk1.Rows[0]["CNT"].ToString()) > 0)
                        {
                            return "[判定失败!]人工打磨未确认,请修理工确认";
                        }
                    }

                    //判断是否已报废
                    string sChkScrap = "SELECT COUNT(*) CNT FROM QMA0101 WHERE BARCODE = '" + code + "' AND AYN = 'C'";
                    DataTable dtChkScrap = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sChkScrap, null).Tables[0];
                    if (dtChkScrap.Rows.Count > 0)
                    {
                        if (int.Parse(dtChkScrap.Rows[0]["CNT"].ToString()) > 0)
                        {
                            return "[判定失败!]该条码已报废，无法进行操作！";
                        }
                    }


                    string sqldt = "SELECT * FROM LTA0001 WHERE BARCODE='" + code + "'";
                    DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqldt, null).Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0]["CUSMYN"].ToString() == "I")//若已入库
                            return "[判定失败!]该条码已入库，无法进行操作！";
                        if (dt.Rows[0]["CUSMYN"].ToString() == "O")//若已出库
                            return "[判定失败!]该条码已出库，无法进行操作！";
                    }
                    if (div == "OK")//合格
                    {
                        string sql = "UPDATE QMA0101 SET AYN='A',IDAT=TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),IBAN='" + ban +
                        "',ISHT='" + ROW["WSHT"].ToString() + "',INAM='" + name + "',ITIM=TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                        "','yyyy-MM-dd hh24:mi:ss')  WHERE  DIV='1' AND BARCODE='" + code + "'";
                        list.Add(sql);

                        string sql1 = "INSERT INTO QMA0003(ID,FAC,WDATE,WTIME,WBAN,WSHT,WNAM,BARCODE,ITNBR,ITDSC,VIRES)" +
                                      " VALUES('" + Guid.NewGuid().ToString() + "','07',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),TO_DATE('" +
                                      DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + ban + "','" + ROW["WSHT"].ToString() + "','" + name + "','" + code
                                      + "','" + dt.Rows[0]["CUITNBR"].ToString() + "','" + dt.Rows[0]["CUITDSC"].ToString() + "','1')";
                        list.Add(sql1);
                    }
                    else//报废
                    {
                        string sql = "UPDATE QMA0101 SET AYN='C',IDAT=TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),IBAN='" + ban +
                     "',ISHT='" + ROW["WSHT"].ToString() + "',INAM='" + name + "',ITIM=TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                     "','yyyy-MM-dd hh24:mi:ss'),COD = '" + bdesc.Split('^')[0].ToString() + "',CIDAT=TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),CIBAN='" + ban +
                     "',CISHT='" + ROW["WSHT"].ToString() + "',CINAM='" + name + "',CITIM=TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                     "','yyyy-MM-dd hh24:mi:ss'),CCOD = '" + bdesc.Split('^')[0].ToString() + "' WHERE  DIV='1' AND BARCODE='" + code + "'";
                        list.Add(sql);

                        string sql1 = "INSERT INTO QMA0003(ID,FAC,WDATE,WTIME,WBAN,WSHT,WNAM,BARCODE,ITNBR,ITDSC,VIRES)" +
                                      " VALUES('" + Guid.NewGuid().ToString() + "','07',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),TO_DATE('" +
                                      DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + ban + "','" + ROW["WSHT"].ToString() + "','" + name + "','" + code
                                      + "','" + dt.Rows[0]["CUITNBR"].ToString() + "','" + dt.Rows[0]["CUITDSC"].ToString() + "','3')";
                        list.Add(sql1);
                    }
                }

                int a = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,list);
                if (a == 0)
                    return "判定失败！";
                return "判定成功！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return ex.Message.ToString();
            }
        }

        [HttpGet]
        [Route("api/GetBarcodeAYN")]
        public DataTable GetBarcodeAYN(string sBarcode)
        {
            try
            {
                
                string sLotAYN = "SELECT AYN,DIV FROM QMA0101 WHERE  BARCODE='" + sBarcode + "'";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotAYN, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetBarcodeUF")]
        public DataTable GetBarcodeUF(string sBarcode)
        {
            try
            {
                
                string sLotUF = " SELECT COUNT(*) CNT FROM QMA0201 WHERE BARCODE='" + sBarcode + "' ";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotUF, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetBarcodeDB")]
        public DataTable GetBarcodeDB(string sBarcode)
        {
            try
            {
                
                string sLotDB = " SELECT COUNT(*) CNT FROM QMA0301 WHERE BARCODE='" + sBarcode + "'";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotDB, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetBarcodeDMSJJ")]
        public DataTable GetBarcodeDMSJJ(string sBarcode)
        {
            try
            {
                
                string sLotDMSJJ = " select * from qma0103 where barcode = '" + sBarcode + "' order by rtim desc";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotDMSJJ, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpGet]
        [Route("api/GetBarcodeStockNo")]
        public DataTable GetBarcodeStockNo(string sERPITNBR)
        {
            try
            {
                
                string sLotStockNo = " SELECT * FROM SDA0005 WHERE ITNBR= '" + sERPITNBR + "'";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotStockNo, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpGet]
        [Route("api/GetSDA0017")]
        public DataTable GetSDA0017(string sBarcode)
        {
            try
            {
                
                string sLotStockNo = " SELECT * FROM SDA0017 WHERE BARCODE='" + sBarcode + "' ORDER BY SCANTIM DESC";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotStockNo, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpGet]
        [Route("api/GetBarcodeLotInfo")]
        public DataTable GetBarcodeLotInfo(string sBarcode)
        {
            try
            {
                
                string sLotInfoSql = " SELECT A.*,B.ERPITNBR FROM LTA0001 A, (SELECT * ";
                sLotInfoSql += "                                     FROM (SELECT A.*,";
                sLotInfoSql += "                                                  ROW_NUMBER ()";
                sLotInfoSql += "                                                  OVER (";
                sLotInfoSql += "                                                     PARTITION BY ITNBR";
                sLotInfoSql += "                                                     ORDER BY";
                sLotInfoSql += "                                                        TO_NUMBER (VERSION) DESC)";
                sLotInfoSql += "                                                     RN";
                sLotInfoSql += "                                             FROM EDB0010 A WHERE ITGRPCOD = 'FERT')";
                sLotInfoSql += "                                    WHERE RN = 1) B";
                sLotInfoSql += " WHERE A.CUITNBR = B.ITNBR(+) AND BARCODE = '" + sBarcode + "'";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotInfoSql, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }
        [HttpGet]
        [Route("api/GetBarcodeVI")]
        public DataTable GetBarcodeVI(string sBarcode)
        {
            try
            {
                
                string sLotVI = " SELECT COUNT(*) CNT FROM CKA0007 WHERE BARCODE='" + sBarcode + "' AND VIRESULT = '1' ";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotVI, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpGet]
        [Route("api/GetBarcodeVI1")]
        public DataTable GetBarcodeVI1(string sBarcode)
        {
            try
            {
                
                string sLotVI1 = " SELECT COUNT(*) CNT FROM QMA0003 WHERE BARCODE='" + sBarcode + "' AND VIRES = '1' ";
                DataTable dtResult = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sLotVI1, null).Tables[0];
                return dtResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpGet]
        [Route("api/GetUF")]
        public DataTable GetUF(string BARCODE)
        {
            
            try
            {
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"select * from  (SELECT BARCODE,UFGRADE,WTIME,ROW_NUMBER () OVER (PARTITION BY BARCODE ORDER BY WTIME DESC) RN FROM QMA0201
               WHERE BARCODE = '" + BARCODE + "') where RN = 1", null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetDB")]
        public DataTable GetDB(string BARCODE)
        {
            
            try
            {
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"select * from  (SELECT BARCODE,OVERALLGRADE,WTIME,ROW_NUMBER ()  OVER (PARTITION BY BARCODE ORDER BY WTIME DESC) RN FROM QMA0301
               WHERE BARCODE = '" + BARCODE + "') where RN = 1", null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        /// 深度登记查询
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetIfDeepReg")]
        public DataTable GetIfDeepReg(string barcode)
        {
            
            try
            {
                string sql = @"SELECT *
                                      FROM (SELECT A.*,
                                                   ROW_NUMBER () OVER (PARTITION BY A.BARCODE ORDER BY RTIM DESC)
                                                      RN
                                              FROM QMA0103 A
                                             WHERE DIV = '1' AND RWAY = '1' AND BARCODE = '" + barcode + "') WHERE RN = '1'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return new DataTable();
            }
        }

        [HttpGet]
        [Route("api/GetPTGGINFO")]
        public DataTable GetPTGGINFO(string sERPITNBR, string sGRD, string dbGrad, string ufGrad)
        {
            
            try
            {
                //string strSQL = @"SELECT * FROM SDA0025 WHERE ITNBR = '" + sERPITNBR + "' AND PDDJ = '" + sGRD + "' AND DBRESULT = '" + dbGrad + "' AND UFRESULT = '" + ufGrad + "'";
                //LogLog.Warn(GetType(),strSQL);
                return OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,@"SELECT * FROM SDA0025 WHERE ITNBR = '" + sERPITNBR + "'  AND DBRESULT = '" + dbGrad.Trim() + "' AND UFRESULT = '" + ufGrad.Trim() + "'", null).Tables[0];
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetRegistBarcodeEndInfo")]
        public RegistBarcodeInfo GetRegistBarcodeEndInfo(string sBarcode)
        {
            
            try
            {
                RegistBarcodeInfo rBarInfo = new RegistBarcodeInfo();
                //string sSql = "SELECT A.*,B.*,C.ITEMPATTERN MODPATTERN,C.ITEMNAM MODNAM,D.ITEMNAM TAONAM FROM QMA0106 A,QMA0107 B,MDA0004 C,MDA0004 D WHERE A.ID = B.SID ";
                //sSql+=" AND A.MODID = C.ITEMID AND A.TAOID = D.ITEMID AND B.CHKMANDIV = '1'  AND A.BARCODE = '" + sBarcode + "' ORDER BY SMNAM";
                string sSql = "  SELECT A.*,";
                sSql += "          B.*,";
                sSql += "         '' MODPATTERN,";
                sSql += "         '' MODNAM,";
                sSql += "         '' TAONAM";
                sSql += "    FROM QMA0206 A,";
                sSql += "         QMA0207 B";
                sSql += "   WHERE     A.ID = B.SID";
                sSql += "         AND B.CHKMANDIV = '1'";
                sSql += "         AND A.BARCODE = '" + sBarcode + "'";
                sSql += "ORDER BY SMNAM";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    rBarInfo.Mchid1 = dt.Rows[0]["MCHID"].ToString();
                    rBarInfo.Lr1 = dt.Rows[0]["LR"].ToString();
                    rBarInfo.Itnbr = dt.Rows[0]["CUITNBR"].ToString();
                    rBarInfo.Itdsc = dt.Rows[0]["CUITDSC"].ToString();
                    rBarInfo.Tim = dt.Rows[0]["CUTOTIM"].ToString();
                    rBarInfo.Wcod = dt.Rows[0]["WKCOD"].ToString();
                    rBarInfo.Hemoli = dt.Rows[0]["HEMOLI"].ToString();
                    rBarInfo.Remark = dt.Rows[0]["REMARK"].ToString();
                    rBarInfo.Planid = dt.Rows[0]["PLANID"].ToString();
                    rBarInfo.Planid = dt.Rows[0]["PLANID"].ToString();
                    rBarInfo.Mchid = dt.Rows[0]["MCHID"].ToString();
                    rBarInfo.Lr = dt.Rows[0]["LR"].ToString();
                    rBarInfo.Modid = dt.Rows[0]["MODID"].ToString();
                    rBarInfo.Modnam = dt.Rows[0]["MODNAM"].ToString();
                    rBarInfo.Modpattern = dt.Rows[0]["MODPATTERN"].ToString();
                    rBarInfo.Taoid = dt.Rows[0]["TAOID"].ToString();
                    rBarInfo.Taonam = dt.Rows[0]["TAONAM"].ToString();
                    rBarInfo.Quanid = dt.Rows[0]["GQCOD"].ToString();
                    rBarInfo.Sdiv = dt.Rows[0]["DIV"].ToString();

                    string[] rBarInfos = new string[19];
                    for (int i = 0; i < rBarInfos.Length; i++)
                    {
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            if ((i + 1) == int.Parse(dt.Rows[j]["SMNAM"].ToString().Split('-')[0]))
                            {
                                rBarInfos[i] = dt.Rows[j]["CHKRESULT"].ToString();
                            }
                        }
                    }

                    rBarInfo.On1 = rBarInfos[0];
                    rBarInfo.On2 = rBarInfos[1];
                    rBarInfo.On3 = rBarInfos[2];
                    rBarInfo.On4 = rBarInfos[3];
                    rBarInfo.On5 = rBarInfos[4];
                    rBarInfo.On6 = rBarInfos[5];
                    rBarInfo.On7 = rBarInfos[6];
                    rBarInfo.On8 = rBarInfos[7];
                    rBarInfo.On9 = rBarInfos[8];
                    rBarInfo.On10 = rBarInfos[9];
                    rBarInfo.On11 = rBarInfos[10];
                    rBarInfo.On12 = rBarInfos[11];
                    rBarInfo.On13 = rBarInfos[12];
                    rBarInfo.On14 = rBarInfos[13];
                    rBarInfo.On15 = rBarInfos[14];
                    rBarInfo.On16 = rBarInfos[15];
                    rBarInfo.On17 = rBarInfos[16];
                    rBarInfo.On18 = rBarInfos[17];
                    rBarInfo.On19 = rBarInfos[18];


                    rBarInfo.Pyn = dt.Rows[0]["PYN"].ToString();
                    rBarInfo.Pres = dt.Rows[0]["PRES"].ToString();

                    return rBarInfo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetRegistBarcodeInfo")]
        public RegistBarcodeInfo GetRegistBarcodeInfo(string sBarcode)
        {
            
            try
            {
                RegistBarcodeInfo rBarInfo = new RegistBarcodeInfo();
                //string sSql = "SELECT A.*,B.*,C.ITEMPATTERN MODPATTERN,C.ITEMNAM MODNAM,D.ITEMNAM TAONAM FROM QMA0106 A,QMA0107 B,MDA0004 C,MDA0004 D WHERE A.ID = B.SID ";
                //sSql+=" AND A.MODID = C.ITEMID AND A.TAOID = D.ITEMID AND B.CHKMANDIV = '1'  AND A.BARCODE = '" + sBarcode + "' ORDER BY SMNAM";
                string sSql = "  SELECT A.*,";
                sSql += "          B.*,";
                sSql += "         '' MODPATTERN,";
                sSql += "         '' MODNAM,";
                sSql += "         '' TAONAM";
                sSql += "    FROM QMA0106 A,";
                sSql += "         QMA0107 B";
                sSql += "   WHERE     A.ID = B.SID";
                sSql += "         AND B.CHKMANDIV = '1'";
                sSql += "         AND A.BARCODE = '" + sBarcode + "'";
                sSql += "ORDER BY SMNAM";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    rBarInfo.Mchid1 = dt.Rows[0]["MCHID"].ToString();
                    rBarInfo.Lr1 = dt.Rows[0]["LR"].ToString();
                    rBarInfo.Itnbr = dt.Rows[0]["CUITNBR"].ToString();
                    rBarInfo.Itdsc = dt.Rows[0]["CUITDSC"].ToString();
                    rBarInfo.Tim = dt.Rows[0]["CUTOTIM"].ToString();
                    rBarInfo.Wcod = dt.Rows[0]["WKCOD"].ToString();
                    rBarInfo.Hemoli = dt.Rows[0]["HEMOLI"].ToString();
                    rBarInfo.Remark = dt.Rows[0]["REMARK"].ToString();
                    rBarInfo.Planid = dt.Rows[0]["PLANID"].ToString();
                    rBarInfo.Planid = dt.Rows[0]["PLANID"].ToString();
                    rBarInfo.Mchid = dt.Rows[0]["MCHID"].ToString();
                    rBarInfo.Lr = dt.Rows[0]["LR"].ToString();
                    rBarInfo.Modid = dt.Rows[0]["MODID"].ToString();
                    rBarInfo.Modnam = dt.Rows[0]["MODNAM"].ToString();
                    rBarInfo.Modpattern = dt.Rows[0]["MODPATTERN"].ToString();
                    rBarInfo.Taoid = dt.Rows[0]["TAOID"].ToString();
                    rBarInfo.Taonam = dt.Rows[0]["TAONAM"].ToString();
                    rBarInfo.Quanid = dt.Rows[0]["GQCOD"].ToString();
                    rBarInfo.Sdiv = dt.Rows[0]["DIV"].ToString();

                    string[] rBarInfos = new string[19];
                    for (int i = 0; i < rBarInfos.Length; i++)
                    {
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            if ((i + 1) == int.Parse(dt.Rows[j]["SMNAM"].ToString().Split('-')[0]))
                            {
                                rBarInfos[i] = dt.Rows[j]["CHKRESULT"].ToString();
                            }
                        }
                    }

                    rBarInfo.On1 = rBarInfos[0];
                    rBarInfo.On2 = rBarInfos[1];
                    rBarInfo.On3 = rBarInfos[2];
                    rBarInfo.On4 = rBarInfos[3];
                    rBarInfo.On5 = rBarInfos[4];
                    rBarInfo.On6 = rBarInfos[5];
                    rBarInfo.On7 = rBarInfos[6];
                    rBarInfo.On8 = rBarInfos[7];
                    rBarInfo.On9 = rBarInfos[8];
                    rBarInfo.On10 = rBarInfos[9];
                    rBarInfo.On11 = rBarInfos[10];
                    rBarInfo.On12 = rBarInfos[11];
                    rBarInfo.On13 = rBarInfos[12];
                    rBarInfo.On14 = rBarInfos[13];
                    rBarInfo.On15 = rBarInfos[14];
                    rBarInfo.On16 = rBarInfos[15];
                    rBarInfo.On17 = rBarInfos[16];
                    rBarInfo.On18 = rBarInfos[17];
                    rBarInfo.On19 = rBarInfos[18];

                    rBarInfo.Pyn = dt.Rows[0]["PYN"].ToString();
                    rBarInfo.Pres = dt.Rows[0]["PRES"].ToString();

                    return rBarInfo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        ///完成品获取维修方式
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getRepairMeth")]
        public DataTable getRepairMeth(string BARCODE)
        {
            DataTable dt = null;
            try
            {
                
                dt = new DataTable();
                string sql = @"SELECT M.*,N.RWAY AS RWAY1 FROM QMA0103 M,QMA0113 N  WHERE M.BARCODE = N.BARCODE(+) AND M.DIV IN('3','2')  AND  M.BARCODE='" + BARCODE + "' AND M.RWAY IN ('2','7') AND N.QATIM IS NOT NULL AND N.RPTIM IS NOT NULL ORDER BY N.QATIM DESC";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;

            }
        }

        /// <summary>
        /// 参数化获取表数据
        /// </summary>
        /// <param name="intent">需要获取的表字段名</param>
        /// <param name="table">表名</param>
        /// <param name="ColNam">WHERE条件的字段名</param>
        /// <param name="content">WHERE对应的字段内容</param>
        /// <param name="sort">需要ORDER BY的字段(降序)</param>
        /// <returns>ColNam与content需要一一对应</returns>
        [HttpPost]
        [Route("api/GetTableData1")]
        public DataTable GetTableData1(TableData tableData)
        {
            
            string sql = string.Format("SELECT {0} ", tableData.intent[0]);
            for (int i = 1; i < tableData.intent.Length; i++)
            {
                sql += string.Format(", {0} ", tableData.intent[i]);
            }
            sql += " FROM " + tableData.table + " WHERE 1=1 ";
            for (int i = 0; i < tableData.ColNam.Length; i++)
            {
                DataRow ROW = SHIFT.GetShift(DateTime.Now);
                if (tableData.content[i] == "shift_wdate")
                {
                    string time = "TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToShortDateString() + "','YYYY/MM/DD')";
                    sql += string.Format("AND {0} = {1} ", tableData.ColNam[i], time);
                }
                if (tableData.content[i] == "shift_wsht")
                {
                    sql += string.Format("AND {0} = '{1}' ", tableData.ColNam[i], ROW["WSHT"]);
                }
                else
                {
                    sql += string.Format("AND {0} = '{1}' ", tableData.ColNam[i], tableData.content[i]);
                }
            }
            if (tableData.sort[0] != null)
            {
                sql += string.Format(" ORDER BY {0} DESC", tableData.sort[0]);
                for (int i = 1; i < tableData.sort.Length; i++)
                {
                    sql += string.Format(" , {0} DESC", tableData.sort[i]);
                }
            }

            DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sql, null).Tables[0];
            if (dtM.Rows.Count > 0)
            {
                return dtM;
            }
            else return null;
        }

        [HttpGet]
        [Route("api/GetUFDBBadInfo")]
        public DataTable GetUFDBBadInfo(string sBarcode)
        {
            
            //DataRow ROW = SHIFT.GetShift(DateTime.Now);
            DataTable dt = null;
            try
            {
                string sSql = string.Empty;
                sSql = "SELECT A.BARCODE, ";
                sSql += "               A.DIV, ";
                sSql += "               A.CUITNBR, ";
                sSql += "               A.CUITDSC, ";
                sSql += "               A.BIDAT, ";
                sSql += "               A.BITIM, ";
                sSql += "               A.ACOD, ";
                sSql += "              B.BNAM ";
                sSql += "          FROM QMA0101 A, QMA0001 B ";
                sSql += "        WHERE     A.ACOD = B.BCOD(+) ";
                sSql += "              AND A.AYN = 'B' ";
                sSql += "              AND A.DIV IN ('2', '3') ";
                sSql += "             AND BARCODE = '" + sBarcode + "'";
                sSql += "             AND A.ACOD IN ('50-1','51-1','53-1','54-1','57-1')";//技术部提供了5个不良代码，只有这5个需要打磨
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


        [HttpGet]
        [Route("api/GetUFDBINFO")]
        public string GetUFDBINFO(string sBarcode)
        {

            
            DataRow ROW = SHIFT.GetShift(DateTime.Now);
            try
            {
                string sITNBR = "";
                string sITDSC = "";
                string sRecp = "";
                string sUFMCH = "", sUFGRADE = "", sUFTIME = "", sDBMCH = "", sDBGRADE = "", sDBTIME = "";
                string sSql = string.Empty;
                DataTable dt = null;
                string sResult = string.Empty;

                sSql = "SELECT BARCODE,CUITNBR,CUITDSC,ATTRVAL FROM LTA0001 A,(SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) B,EDB0015 C WHERE A.CUITNBR =B.ITNBR AND B.ID = C.ITEMID AND A.BARCODE = '" + sBarcode + "' AND C.ATTRCOD = 'M35'";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    sITNBR = dt.Rows[0]["CUITNBR"].ToString();
                    sITDSC = dt.Rows[0]["CUITDSC"].ToString();
                    sRecp = dt.Rows[0]["ATTRVAL"].ToString();
                }

                sSql = "select wtime,mchid,ufgrade from QMA0201 where barcode = '" + sBarcode + "' order by wtime desc";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    sUFTIME = dt.Rows[0]["WTIME"].ToString();
                    sUFGRADE = dt.Rows[0]["UFGRADE"].ToString();
                    switch (dt.Rows[0]["MCHID"].ToString())
                    {
                        case "32-0016":
                            sUFMCH = "07UF02";
                            break;
                        case "32-0017":
                            sUFMCH = "07UF01";
                            break;
                        case "32-0018":
                            sUFMCH = "07UF03";
                            break;
                        case "32-0019":
                            sUFMCH = "07UF04";
                            break;
                        case "30-18005":
                            sUFMCH = "07UF05";
                            break;
                        case "30-18007":
                            sUFMCH = "07UF06";
                            break;
                        case "30-18003":
                            sUFMCH = "07UF07";
                            break;
                        case "30-18011":
                            sUFMCH = "07UF08";
                            break;
                        case "30-18013":
                            sUFMCH = "07UF09";
                            break;
                        case "30-18015":
                            sUFMCH = "07UF10";
                            break;
                    }
                }

                sSql = "SELECT WTIME,MCHID,OVERALLGRADE FROM QMA0301 WHERE BARCODE = '" + sBarcode + "' ORDER BY WTIME DESC";
                dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSql, null).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    sDBTIME = dt.Rows[0]["WTIME"].ToString();
                    sDBGRADE = dt.Rows[0]["OVERALLGRADE"].ToString();
                    switch (dt.Rows[0]["MCHID"].ToString())
                    {
                        case "4515003":
                            sDBMCH = "07MB01";
                            break;
                        case "4515002":
                            sDBMCH = "07MB02";
                            break;
                        case "4515004":
                            sDBMCH = "07MB03";
                            break;
                        case "4515005":
                            sDBMCH = "07MB04";
                            break;
                        case "42-18006":
                            sDBMCH = "07MB05";
                            break;
                        case "42-18008":
                            sDBMCH = "07MB06";
                            break;
                        case "42-18004":
                            sDBMCH = "07MB07";
                            break;
                        case "42-18010":
                            sDBMCH = "07MB08";
                            break;
                        case "42-18012":
                            sDBMCH = "07MB09";
                            break;
                        case "42-18014":
                            sDBMCH = "07MB10";
                            break;
                    }
                }

                sResult = sITNBR + "^" + sITDSC + "^" + sRecp + "^" + sUFMCH + "^" + sUFGRADE + "^" + sUFTIME + "^" + sDBMCH + "^" + sDBGRADE + "^" + sDBTIME + "^";
                return sResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return ex.Message;
            }
        }

        #region 末罐登记/判定



        [HttpPost]
        [Route("api/JugEndCheckInfo")]
        public string JugEndCheckInfo(CheckInfo checkInfo)
        {
            
            DataRow row = SHIFT.GetShift(DateTime.Now);
            ArrayList sqllist = new ArrayList();
            try
            {
                string sPYN = "Y";
                string sPRES = "A";
                DateTime dPDAT = Convert.ToDateTime(row["WDATE"].ToString());
                DateTime dPTIM = DateTime.Now;
                string sPNAM = checkInfo.sNAM;
                string sPSHT = row["WSHT"].ToString();
                string sPBAN = row["WBAN"].ToString();

                foreach (string sItem in checkInfo.sCheckItems)
                {
                    if (sItem.Contains("NG"))
                    {
                        sPRES = "B";
                        break;
                    }
                }

                //string sSqlSel = "SELECT * FROM QMA0206 ";
                //sSqlSel += " WHERE DIV = '" + sDiv + "' ";
                //sSqlSel += " AND PLANID = '" + sPLANID + "' ";
                //sSqlSel += " AND BARCODE = '" + sBarcode + "'";
                string sSqlSel = "SELECT * FROM QMA0206 ";
                sSqlSel += " WHERE BARCODE = '" + checkInfo.sBarcode + "'";

                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqlSel, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    string sSql = "UPDATE QMA0206 SET PYN = '" + sPYN + "',PRES = '" + sPRES + "',PTIM = to_date('" + dPTIM.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql += " PDAT = to_date('" + dPDAT.ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),PNAM = '" + sPNAM + "',PSHT = '" + sPSHT + "',PBAN = '" + sPBAN + "' ";
                    sSql += " WHERE ID = '" + dt.Rows[0]["ID"].ToString() + "' ";
                    sqllist.Add(sSql);

                    foreach (string sItem in checkInfo.sCheckItems)
                    {
                        string[] sitem = sItem.Split('^');
                        string sql1 = "INSERT INTO QMA0207 (ID,SID,DIV,SMNAM,CHKRESULT,CHKMANDIV) VALUES (";
                        sql1 += "sys_guid(),";
                        sql1 += "'" + dt.Rows[0]["ID"].ToString() + "',";
                        sql1 += "'" + checkInfo.sDiv + "',";
                        sql1 += "'" + sitem[0] + "',";
                        sql1 += "'" + sitem[1] + "',";
                        sql1 += "'2'";
                        sql1 += ")";
                        sqllist.Add(sql1);
                    }
                    //if (sPRES == "A")
                    //{
                    //    string sMddSql = "UPDATE MDD0003 SET PLANSTATUS = '5' WHERE PLANID = '" + sPLANID + "'";
                    //    sqllist.Add(sMddSql);
                    //}
                    int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                    if (iRes > 0)
                    {
                        //if (sPRES == "A")//若判定合格
                        //{
                        //    //判定完成后发送<2-供给开始>信息
                        //    string sIFSql = "INSERT INTO IF_WMS_GT_11 (MCHID,ITNBR,ITDSC,GTITNBR,LRFLAG,CTLDIV,RCV_FLAG,ENT_USER_ID,ENT_DT) VALUES (";
                        //    sIFSql += "'" + dt.Rows[0]["MCHID"].ToString() + "',";
                        //    sIFSql += "'" + dt.Rows[0]["CUITNBR"].ToString() + "',";
                        //    sIFSql += "'" + dt.Rows[0]["CUITDSC"].ToString() + "',";
                        //    sIFSql += "'" + dt.Rows[0]["BUITNBR"].ToString() + "',";
                        //    sIFSql += "'" + dt.Rows[0]["LR"].ToString() + "',";
                        //    sIFSql += "'2',";//1-供给开始
                        //    sIFSql += "'N',";
                        //    sIFSql += "'PDA',";
                        //    sIFSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS')";
                        //    sIFSql += ")";
                        //    int k = ifconfig.oracleBase.ExecuteNonQuery(sIFSql);
                        //    if (k <= 0)
                        //        return "-1";
                        //}
                        return "OK";
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

        #endregion

        [HttpPost]
        [Route("api/JugFirstCheckInfo")]
        public string JugFirstCheckInfo(CheckInfo checkInfo)
        {
            
            DataRow row = SHIFT.GetShift(DateTime.Now);
            ArrayList sqllist = new ArrayList();
            try
            {
                string sPYN = "Y";
                string sPRES = "A";
                DateTime dPDAT = Convert.ToDateTime(row["WDATE"].ToString());
                DateTime dPTIM = DateTime.Now;
                string sPNAM = checkInfo.sNAM;
                string sPSHT = row["WSHT"].ToString();
                string sPBAN = row["WBAN"].ToString();

                foreach (string sItem in checkInfo.sCheckItems)
                {
                    if (sItem.Contains("NG"))
                    {
                        sPRES = "B";
                        break;
                    }
                }

                string sSqlSel = "SELECT * FROM QMA0106 ";
                sSqlSel += " WHERE DIV = '" + checkInfo.sDiv + "' ";
                sSqlSel += " AND PLANID = '" + checkInfo.sPLANID + "' ";
                sSqlSel += " AND BARCODE = '" + checkInfo.sBarcode + "'";

                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqlSel, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    string sSql = "UPDATE QMA0106 SET PYN = '" + sPYN + "',PRES = '" + sPRES + "',PTIM = to_date('" + dPTIM.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),";
                    sSql += " PDAT = to_date('" + dPDAT.ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),PNAM = '" + sPNAM + "',PSHT = '" + sPSHT + "',PBAN = '" + sPBAN + "' ";
                    sSql += " WHERE ID = '" + dt.Rows[0]["ID"].ToString() + "' ";
                    sqllist.Add(sSql);

                    foreach (string sItem in checkInfo.sCheckItems)
                    {
                        string[] sitem = sItem.Split('^');
                        string sql1 = "INSERT INTO QMA0107 (ID,SID,DIV,SMNAM,CHKRESULT,CHKMANDIV) VALUES (";
                        sql1 += "sys_guid(),";
                        sql1 += "'" + dt.Rows[0]["ID"].ToString() + "',";
                        sql1 += "'" + checkInfo.sDiv + "',";
                        sql1 += "'" + sitem[0] + "',";
                        sql1 += "'" + sitem[1] + "',";
                        sql1 += "'2'";
                        sql1 += ")";
                        sqllist.Add(sql1);
                    }
                    if (sPRES == "A")
                    {
                        string sMddSql = "UPDATE MDD0003 SET PLANSTATUS = '5' WHERE PLANID = '" + checkInfo.sPLANID + "'";//首罐判定完后更新为5-已完成
                        sqllist.Add(sMddSql);
                        //经与李磊讨论，模具现况中的模具代码在质检判定后进行更新，原因是：拆卸洗模可能会更换模具，而拆卸洗模计划是上机时自动生产，模具代码是旧的。
                        //首先获取机台的状态为4-首模待检的模具信息（确保模具信息已被更新为最新）
                        string sSqlModSql = "SELECT * FROM MDD0003 WHERE PLANSTATUS = '4' AND MCHID ='" + dt.Rows[0]["MCHID"].ToString() + "' AND LRFLAG ='" + dt.Rows[0]["LR"].ToString() + "' ORDER BY ETIM DESC";
                        DataTable dtModSql = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sSqlModSql, null).Tables[0];
                        string sCAVID = string.Empty;//型腔号
                        string sCHAID = string.Empty;//壳体号
                        string sGQCOD = string.Empty;//钢圈号
                        string sUpdMDD0002 = string.Empty;
                        if (dtModSql.Rows.Count > 0 && dtModSql != null)
                        {
                            sCAVID = string.IsNullOrEmpty(dtModSql.Rows[0]["MODID"].ToString().Trim()) ? "N/A" : dtModSql.Rows[0]["MODID"].ToString().Trim();
                            sCHAID = string.IsNullOrEmpty(dtModSql.Rows[0]["TAOID"].ToString().Trim()) ? "N/A" : dtModSql.Rows[0]["TAOID"].ToString().Trim();
                            sGQCOD = string.IsNullOrEmpty(dtModSql.Rows[0]["GQCOD"].ToString().Trim()) ? "N/A" : dtModSql.Rows[0]["GQCOD"].ToString().Trim();
                            sUpdMDD0002 = "UPDATE MDD0002 SET CAVID = '" + sCAVID + "',CHAID='" + sCHAID + "',GQCOD='" + sGQCOD + "' WHERE MCHID = '" + dt.Rows[0]["MCHID"].ToString() + "' AND LRFLAG = '" + dt.Rows[0]["LR"].ToString() + "'";
                            sqllist.Add(sUpdMDD0002);
                        }
                    }
                    int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sqllist);
                    if (iRes > 0)
                    {
                        if (sPRES == "A")//若判定合格
                        {
                            //判定完成后发送<2-供给开始>信息
                            string sIFSql = "INSERT INTO IF_WMS_GT_11 (MCHID,ITNBR,ITDSC,GTITNBR,LRFLAG,CTLDIV,RCV_FLAG,ENT_USER_ID,ENT_DT) VALUES (";
                            sIFSql += "'" + dt.Rows[0]["MCHID"].ToString() + "',";
                            sIFSql += "'" + dt.Rows[0]["CUITNBR"].ToString() + "',";
                            sIFSql += "'" + dt.Rows[0]["CUITDSC"].ToString() + "',";
                            sIFSql += "'" + dt.Rows[0]["BUITNBR"].ToString() + "',";
                            sIFSql += "'" + dt.Rows[0]["LR"].ToString() + "',";
                            sIFSql += "'2',";//1-供给开始
                            sIFSql += "'N',";
                            sIFSql += "'PDA',";
                            sIFSql += "to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS')";
                            sIFSql += ")";
                            
                            int k = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString2, CommandType.Text, sIFSql, null);
                            if (k <= 0)
                                return "-1";
                        }
                        return "OK";
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

        #region 外观修理





        /// <summary>
        /// 热补深度登记
        /// </summary>
        /// <param name="BARCODE"></param>
        /// <param name="sPdeep"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/RepairDeep_Regist")]
        public string RepairDeep_Regist(string BARCODE, string sPdeep)
        {
            
            string sqslsq = "SELECT * FROM RBDJ0001 WHERE BARCODE = '" + BARCODE + "'";
            DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqslsq, null).Tables[0];


            ArrayList list = new ArrayList();
            string sSql = "UPDATE QMA0103 SET PDEEP = '" + sPdeep + "' WHERE RWAY = '1' AND DIV = '1' AND BARCODE = '" + BARCODE + "'";
            list.Add(sSql);
            string sqlRB = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                sqlRB = "UPDATE RBDJ0001 SET RBTIM = SYSDATE,RBDJNR = '" + sPdeep + "' WHERE BARCODE = '" + BARCODE + "'";
            }
            else
            {
                sqlRB = "INSERT INTO RBDJ0001(ID,BARCODE,RBDJNR,RBTIM)  VALUES('" + Guid.NewGuid().ToString() + "','" + BARCODE + "','" + sPdeep + "',SYSDATE)";
            }
            list.Add(sqlRB);

            int iRes = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,list);

            return iRes.ToString();
        }

        #endregion

        [HttpGet]
        [Route("api/Select_LhZJData")]
        public DataTable Select_LhZJData(string sMchid, string sLR)
        {
            try
            {
                
                DataRow rRow = SHIFT.GetShift(DateTime.Now);
                string seSql = "SELECT * FROM QMA0116 WHERE MCHID = '" + sMchid + "' AND MUCHLR = '" + sLR + "' AND ZJTIME IS NULL  ORDER BY GYYTIME DESC";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,seSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt;
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


        /// <summary>
        ///质检确认
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/setZJQRMeth")]
        public string setZJQRMeth(string BARCODE, string userId)
        {
            DataTable dt = null;
            try
            {

                
                dt = new DataTable();

                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + userId + "' AND FAC='" + FAC + "'";
                DataTable dtEMP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text,sqlEMP, null).Tables[0];
                string sql = @"UPDATE QMA0103 SET ZJQRTIM = SYSDATE,ZJQRNAM = '" + dtEMP.Rows[0]["NAME"].ToString() + "' WHERE BARCODE='" + BARCODE + "' ";
                int i =  OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,sql, null);
                if (i > 0)
                    return "确认成功！";
                else
                    return "确认失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "";




            }
        }


        #region 硫化质量检查信息获取


        [HttpGet]
        [Route("api/Update_LhZJData")]
        public string Update_LhZJData(string id, string div, string remark, string zjname)
        {
            try
            {
                
                DateTime dtNow = DateTime.Now;
                DataRow rRow = SHIFT.GetShift(DateTime.Now);
                string seSql = "UPDATE QMA0116 SET ZJNAME = '" + zjname + "',STA = '" + div + "',REMARK = '" + remark + "',ZJTIME = TO_DATE('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";
                seSql += "    WHERE ID = '" + id + "'";

                int iResult =  OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString,CommandType.Text,seSql, null);
                if (iResult > 0)
                    return "OK";
                else
                    return "Fail-判定失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err:" + ex.Message;
            }
        }



        #endregion
    }
}
