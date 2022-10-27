using System;
using System.Web.Http;
using System.Data;
using log4net.Util;
using HYPDAWebApi.DBHelper;
using System.Collections;
using HYPDAWebApi.App_Data;

namespace HYPDAWebApi.Controllers
{
    /// <summary>
    /// 原材料
    /// </summary>
    public class RawMaterialController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";

        [HttpGet]
        [Route("api/GetLotITNBR")]
        public DataTable GetLotITNBR(string str)
        {
            //0 Lot号
            try
            {
                string strSql = "SELECT * FROM STB0002 WHERE LOTNO ='" + str + "'";
                DataTable dt =  OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];

                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


        [HttpGet]
        [Route("api/GetEmpInfo")]
        public DataTable GetEmpInfo(string sUserID)
        {
            //部门名称
            try
            {
                string strSql = "SELECT * FROM LSFW_EMPLOYEE WHERE LEAYN='N' AND LOGINNAME='" + sUserID + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


        [HttpGet]
        [Route("api/CheckCanTransactionOrNot")]
        public string CheckCanTransactionOrNot(string sLotID, string sTxnDiv)
        {
            string sSql = string.Empty;
            string sResultStr = "OK";
            DataTable dt = new DataTable();
            try
            {
                switch (sTxnDiv)
                {
                    case "12"://采购退货
                        sSql = "SELECT COUNT(*) CNT FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                         dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                        if (dt.Rows.Count > 0 && dt != null)
                        {
                            if (dt.Rows[0]["CNT"].ToString() == "0")
                            {
                                sResultStr = "NG-该批次在库存中不存在，不可以退货！";
                            }
                        }
                        break;
                    case "13"://工厂转移
                        sSql = "SELECT COUNT(*) CNT FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];

                        if (dt.Rows.Count > 0 && dt != null)
                        {
                            if (dt.Rows[0]["CNT"].ToString() == "0")
                            {
                                sResultStr = "NG-该批次在华阳库存中不存在，无法转移！";
                            }
                        }
                        break;
                    case "21"://车间领用
                        sSql = "SELECT STA,STKSTS  FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];

                        if (dt.Rows.Count > 0 && dt != null)
                        {
                            if (dt.Rows[0]["STKSTS"].ToString() == "12")
                            {
                                sResultStr = "NG-该批次已出库，无法再次被领用！";
                            }
                            if (dt.Rows[0]["STA"].ToString() == "5")
                            {
                                sResultStr = "NG-该批次在平库过期，无法被领用！";
                            }
                            if (dt.Rows[0]["STA"].ToString() == "2")
                            {
                                sResultStr = "NG-该批次在平库不良，无法被领用！";
                            }
                            if (dt.Rows[0]["STA"].ToString() == "3")
                            {
                                sResultStr = "NG-该批次在平库保留，无法被领用！";
                            }
                        }
                        else
                        {
                            sResultStr = "NG-该批次信息被删除，无法执行操作！";
                        }
                        break;
                    case "22"://车间退回
                        sSql = "SELECT STKSTS  FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];

                        if (dt.Rows.Count > 0 && dt != null)
                        {
                            if (dt.Rows[0]["STKSTS"].ToString() == "10")
                            {
                                sResultStr = "NG-该批次已在华阳库中，无法再次被退回！";
                            }
                        }
                        else
                        {
                            //sResultStr = "NG-该批次信息被删除，无法执行操作！";
                        }
                        break;
                    case "31"://盘盈接收
                        sSql = "SELECT STKSTS  FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];

                        if (dt.Rows.Count > 0 && dt != null)
                        {
                            if (dt.Rows[0]["STKSTS"].ToString() == "10")
                            {
                                sResultStr = "NG-该批次已在华阳库中，无法盘盈接收！";
                            }
                        }
                        break;
                    case "32"://盘亏发出
                        sSql = "SELECT COUNT(*) CNT FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];

                        if (dt.Rows.Count > 0 && dt != null)
                        {
                            if (int.Parse(dt.Rows[0]["CNT"].ToString()) == 0)
                            {
                                sResultStr = "NG-该批次在原材料库存中不存在，无法盘亏发出！";
                            }
                        }
                        break;
                }
                return sResultStr;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/RawMaterialTransaction")]
        public string RawMaterialTransaction(string sTxnDiv, string sLotID, string sITNBR, string sITDSC, string sTxnEmpNo, string sTxnQty, string sFacDiv, string sBackEmpNo, string sBackDepNo, string sUnit, bool bAllQty)
        {
            DataRow ROW = SHIFT.GetShift(DateTime.Now);

            ArrayList list = new ArrayList();
            string strInsert = string.Empty;
            string strUpdate = string.Empty;
            string strDelete = string.Empty;
            string strDelete1 = string.Empty;
            try
            {
                switch (sTxnDiv)
                {
                    case "12"://采购退货
                        strInsert = @"INSERT INTO STB0003( " +
                                                    @"ID,FAC,LOTID, " +
                                                    @"ITNBR,ITDSC,TXNEMPNO, " +
                                                    @"TXNQTY,UNIT, " +
                                                    @"TXNDAT, " +
                                                    @"TXNTIM,TXNDIV) VALUES( " +
                                                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + sLotID + "', " +
                                                    @"'" + sITNBR + "','" + sITDSC + "','" + sTxnEmpNo + "', " +
                                                    @"'" + sTxnQty + "','" + sUnit + "', " +
                                                    @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                                                    @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'" + sTxnDiv + "' )";
                        //string strUpdate = "UPDATE WIP0003 SET STKSTS='12' WHERE LOTID='" + str[0] + "'";
                        strDelete = "DELETE FROM WIP0003 WHERE LOTID='" + sLotID + "'";
                        //string strDelete1 = "DELETE FROM STB0002 WHERE LOTNO='" + sLotID + "'";
                        list.Add(strInsert);
                        list.Add(strDelete);
                        //list.Add(strDelete1);
                        break;
                    case "13"://工厂转移
                        strInsert = @"INSERT INTO STB0003( " +
                                                    @"ID,FAC,LOTID, " +
                                                    @"ITNBR,ITDSC,TXNEMPNO, " +
                                                    @"TXNQTY,UNIT, " +
                                                    @"TXNDAT, " +
                                                    @"TXNTIM,TXNDIV,FACDIV) VALUES( " +
                                                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + sLotID + "', " +
                                                    @"'" + sITNBR + "','" + sITDSC + "','" + sTxnEmpNo + "', " +
                                                    @"'" + sTxnQty + "','" + sUnit + "', " +
                                                    @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                                                    @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'" + sTxnDiv + "','" + sFacDiv + "')";
                        if (bAllQty)//若转移全部数量
                            strDelete = "DELETE FROM WIP0003 WHERE LOTID='" + sLotID + "'";
                        else
                            strDelete = "UPDATE WIP0003 SET WT = WT-" + sTxnQty + " WHERE LOTID = '" + sLotID + "'";
                        list.Add(strInsert);
                        list.Add(strDelete);
                        break;
                    case "22"://车间退回
                        strInsert = @"INSERT INTO STB0003( " +
                                            @"ID,FAC,LOTID, " +
                                            @"ITNBR,ITDSC,TXNEMPNO, " +
                                            @"TXNQTY,UNIT, " +
                                            @"TXNDAT, " +
                                            @"TXNTIM,TXNDIV,BACKEMPNO,BACKDEPNO) VALUES( " +
                                            @"'" + Guid.NewGuid() + "','" + FAC + "','" + sLotID + "', " +
                                            @"'" + sITNBR + "','" + sITDSC + "','" + sTxnEmpNo + "', " +
                                            @"'" + sTxnQty + "','" + sUnit + "', " +
                                            @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                                            @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'" + sTxnDiv + "','" + sBackEmpNo + "','" + sBackDepNo + "')";
                        strDelete = "DELETE FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        strDelete1 = "INSERT INTO WIP0003 SELECT SYS_GUID(),'07','',LOTNO,'',ITNBR,ITDSC,'','','" + sTxnQty + "','1','','',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),SYSDATE,'N','10',CUSTBATCHLOT FROM STB0002 WHERE LOTNO = '" + sLotID + "'";
                        list.Add(strInsert);
                        list.Add(strDelete);
                        list.Add(strDelete1);
                        break;
                    case "31"://盘盈接收
                        strInsert = @"INSERT INTO STB0003( " +
                                            @"ID,FAC,LOTID, " +
                                            @"ITNBR,ITDSC,TXNEMPNO, " +
                                            @"TXNQTY,UNIT, " +
                                            @"TXNDAT, " +
                                            @"TXNTIM,TXNDIV) VALUES( " +
                                            @"'" + Guid.NewGuid() + "','" + FAC + "','" + sLotID + "', " +
                                            @"'" + sITNBR + "','" + sITDSC + "','" + sTxnEmpNo + "', " +
                                            @"'" + sTxnQty + "','" + sUnit + "', " +
                                            @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                                            @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'" + sTxnDiv + "')";
                        strDelete = "DELETE FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        strDelete1 = "INSERT INTO WIP0003 SELECT SYS_GUID(),'07','',LOTNO,'',ITNBR,ITDSC,'','','" + sTxnQty + "','1','','',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),SYSDATE,'N','10',CUSTBATCHLOT FROM STB0002 WHERE LOTNO = '" + sLotID + "'";
                        list.Add(strInsert);
                        list.Add(strDelete);
                        list.Add(strDelete1);
                        break;
                    case "32"://盘亏发出
                        strInsert = @"INSERT INTO STB0003( " +
                                            @"ID,FAC,LOTID, " +
                                            @"ITNBR,ITDSC,TXNEMPNO, " +
                                            @"TXNQTY,UNIT, " +
                                            @"TXNDAT, " +
                                            @"TXNTIM,TXNDIV) VALUES( " +
                                            @"'" + Guid.NewGuid() + "','" + FAC + "','" + sLotID + "', " +
                                            @"'" + sITNBR + "','" + sITDSC + "','" + sTxnEmpNo + "', " +
                                            @"'" + sTxnQty + "','" + sUnit + "', " +
                                            @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                                            @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'" + sTxnDiv + "')";
                        strDelete = "DELETE FROM WIP0003 WHERE LOTID = '" + sLotID + "'";
                        list.Add(strInsert);
                        list.Add(strDelete);
                        break;
                    default:
                        break;
                }
                int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,list);
                if (i > 0)
                    return "1";
                else
                    return "2";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetLotRaw")]
        public DataTable GetLotRaw(string str)
        {
            //0 Lot号
            try
            {
                string strSql = "SELECT * FROM WIP0003 WHERE LOTID ='" + str + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/CheckEarlyProdLot")]
        public string CheckEarlyProdLot(string sLotID, string sDIV)
        {
            //0 LOT号
            try
            {
                string sRst = string.Empty;
                string sSql = string.Empty;

                if (sDIV == "FC")
                {
                    sSql = " SELECT A.LOTNO ";
                    sSql += "  FROM STE0001 A, LTC0001 B";
                    sSql += " WHERE     A.LOTNO = B.LOTID";
                    sSql += "       AND B.AUTIM < (SELECT AUTIM";
                    sSql += "                        FROM LTC0001";
                    sSql += "                       WHERE LOTID = '" + sLotID + "')";
                    sSql += "       AND A.ITNBR = (SELECT AITNBR";
                    sSql += "                        FROM LTC0001";
                    sSql += "                       WHERE LOTID = '" + sLotID + "')";
                    //刘小辉与大建讨论后需修改只考虑正常批次
                    sSql += " AND A.STOSTS  = '1'";
                }
                else if (sDIV == "BW")
                {
                    sSql = " SELECT A.LOTID ";
                    sSql += "  FROM WIP0001 A, LTC0001 B";
                    sSql += " WHERE     A.LOTID = B.LOTID  AND A.STKSTS = '11' AND A.STA = '1' ";
                    sSql += "       AND B.AUTIM < (SELECT AUTIM";
                    sSql += "                        FROM LTC0001";
                    sSql += "                       WHERE LOTID = '" + sLotID + "')";
                    sSql += "       AND A.ITNBR = (SELECT AITNBR";
                    sSql += "                        FROM LTC0001";
                    sSql += "                       WHERE LOTID = '" + sLotID + "')";
                }
                else if (sDIV == "ZLJ")
                {
                    sSql = " SELECT A.LOTID ";
                    sSql += "  FROM WIP0004 A, LTC0001 B";
                    sSql += " WHERE     A.LOTID = B.LOTID  AND A.STKSTS = '11' AND A.STA = '1' ";
                    sSql += "       AND B.AUTIM < (SELECT AUTIM";
                    sSql += "                        FROM LTC0001";
                    sSql += "                       WHERE LOTID = '" + sLotID + "')";
                    sSql += "       AND A.ITNBR = (SELECT AITNBR";
                    sSql += "                        FROM LTC0001";
                    sSql += "                       WHERE LOTID = '" + sLotID + "')";
                }
                else
                {
                    //sSql = "  SELECT A.LOTID ";
                    //sSql += " FROM WIP0003 A, STB0002 B ";
                    //sSql += " WHERE     A.LOTID = B.LOTNO ";
                    //sSql += " AND B.ETIM < (SELECT ETIM ";
                    //sSql += " FROM STB0002 ";
                    //sSql += " WHERE LOTNO = '" + sLotID + "') ";
                    //sSql += " AND A.ITNBR = (SELECT ITNBR ";
                    //sSql += " FROM STB0002 ";
                    //sSql += " WHERE LOTNO = '" + sLotID + "') ";
                    //sSql += " AND A.STKSTS='10'";

                    //更改为通过货单的时间来判断是否有更早的
                    sSql += "SELECT A.LOTID ";
                    sSql += "  FROM WIP0003 A, ";
                    sSql += "       (SELECT A.LOTNO, ";
                    sSql += "               A.ITNBR, ";
                    sSql += "               A.ITDSC, ";
                    sSql += "               B.ETIM ";
                    sSql += "          FROM STB0002 A, STB0001 B ";
                    sSql += "         WHERE A.ORDERID = B.ID) B ";
                    sSql += " WHERE     A.LOTID = B.LOTNO ";
                    sSql += "       AND B.ETIM < (SELECT ETIM ";
                    sSql += "                       FROM (SELECT A.LOTNO, ";
                    sSql += "                                    A.ITNBR, ";
                    sSql += "                                    A.ITDSC, ";
                    sSql += "                                    B.ETIM ";
                    sSql += "                               FROM STB0002 A, STB0001 B ";
                    sSql += "                              WHERE A.ORDERID = B.ID) ";
                    sSql += "                      WHERE LOTNO = '" + sLotID + "') ";
                    sSql += "       AND A.ITNBR = (SELECT ITNBR ";
                    sSql += "                        FROM STB0002 ";
                    sSql += "                       WHERE LOTNO = '" + sLotID + "') ";
                    sSql += "       AND A.STKSTS = '10' ";
                    sSql += "       AND A.STA = '1' ";

                }
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (DT.Rows.Count > 0)
                {
                    if (sDIV == "FC")
                    {
                        for (int i = 0; i < DT.Rows.Count; i++)
                        {
                            sRst += DT.Rows[i]["LOTNO"].ToString() + "^";
                        }
                    }
                    else
                    {
                        for (int i = 0; i < DT.Rows.Count; i++)
                        {
                            sRst += DT.Rows[i]["LOTID"].ToString() + "^";
                        }
                    }
                    sRst = sRst.Substring(0, sRst.Length - 1);
                    return sRst;
                }
                return "";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpPost]
        [Route("api/GetRawRecipients")]
        public string GetRawRecipients(string[] str)
        {
            //0 Lot号
            //1 规格代码
            //2 规格名称
            //3 重量
            //4 领用部门
            //5 领用人工号
            //6 发料人工号（登录人）
            //7 单位（KG）

            DataRow ROW = SHIFT.GetShift(DateTime.Now);

            try
            {
                string strInsert = @"INSERT INTO STB0003( " +
                    @"ID,FAC,LOTID, " +
                    @"ITNBR,ITDSC,ISSUEEMPNO, " +
                    @"RECEPTDEPNO,RECEPTEMPNO, " +
                    @"RECEPTQTY,UNIT, " +
                    @"RECEPTDAT, " +
                    @"RECEPTTIM,TXNDIV) VALUES( " +
                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
                    @"'" + str[1] + "','" + str[2] + "','" + str[6] + "', " +
                    @"'" + str[4] + "','" + str[5] + "', " +
                    @"'" + str[3] + "','" + str[7] + "', " +
                    @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                    @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'21' )";
                string strUpdate = "UPDATE WIP0003 SET STKSTS='12' WHERE LOTID='" + str[0] + "'";
                ArrayList list = new ArrayList();
                list.Add(strInsert);
                list.Add(strUpdate);
                int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (i > 0)
                    return "1";
                else
                    return "2";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


    }
}
