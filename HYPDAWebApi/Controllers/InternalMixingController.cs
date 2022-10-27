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
    /// 密炼
    /// </summary>
    public class InternalMixingController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";
        RawMaterialController RawMaterial = new RawMaterialController();

        [HttpGet]
        [Route("api/CheckRawTDL")]
        public DataTable CheckRawTDL(string sITNBR)
        {
            
            try
            {
                string sSql = "SELECT * FROM LTD0022 WHERE ITNBR = '" + sITNBR + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/Get_RAWNAME")]
        public DataTable Get_RAWNAME(string LabITNBR)
        {
            
            try
            {
                string strSql = "SELECT *  FROM MX0065 WHERE  RAW_CODE ='" + LabITNBR + "'";
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
        [Route("api/GetBinNoByITNBR")]
        public DataTable GetBinNoByITNBR(string sITNBR, string sLineNo)
        {
            

            try
            {
                string sSql = "SELECT * ";
                sSql += "  FROM TDB0431 ";
                sSql += "   WHERE (   RAW_CODE = '" + sITNBR + "' ";
                sSql += "        OR RAW_CODE_1 = '" + sITNBR + "'";
                sSql += "        OR RAW_CODE_2 = '" + sITNBR + "'";
                sSql += "        OR RAW_CODE_3 = '" + sITNBR + "'";
                sSql += "        OR RAW_CODE_4 = '" + sITNBR + "'";
                sSql += "        OR RAW_CODE_5 = '" + sITNBR + "'";
                sSql += ") AND LINE_NO = '" + sLineNo + "'";

                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetMixBinInfo")]
        public DataTable GetMixBinInfo(string sDIV, string sBinNo)
        {
            

            try
            {
                //  string strSql = "SELECT DISTINCT RAW_NAME,RAW_CODE  FROM MX0060 WHERE DIV = '" + sDIV + "'  AND B_BIN_NO ='" + sBinNo + "' AND RAW_CODE IS NOT NULL";

                string strSql = "SELECT *  FROM MX0065 WHERE     B_BIN_NO ='" + sBinNo + "' AND RAW_CODE IS NOT NULL";


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
        [Route("api/GetPrepare")]
        public string GetPrepare(string[] str)
        {
            //0 罐号
            //1 药粉规格代码
            //2 药粉规格名称
            //3 药粉lot
            //4 投入人
            //5 line_no

            

            try
            {
                string sewip0003 = " SELECT * FROM WIP0003 WHERE LOTID='" + str[3] + "'";
                DataTable dtwip0003 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sewip0003, null).Tables[0];
                if (dtwip0003 != null && dtwip0003.Rows.Count > 0)
                {
                    if (dtwip0003.Rows[0]["STKSTS"].ToString() != "12")
                    {
                        return "12";
                    }
                }

                ArrayList sSqls = new ArrayList();
                string sBins = string.Empty;
                sSqls.Add("UPDATE TDB0431 SET CURRLOTID = '" + str[3] + "',INNAM = '" + str[4] + "',INTIM = TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')  WHERE LINE_NO = '" + str[5] + "' AND BIN_NO = '" + str[0] + "'");
                sSqls.Add("UPDATE MX0050 SET OPEN_FLAG = 'Y' WHERE LINE_NO = '" + str[5] + "' AND BIN_NO = '" + str[0] + "'");
                sSqls.Add("UPDATE MX0050 SET CLOSE_FLAG = 'Y' WHERE LINE_NO = '" + str[5] + "'  AND BIN_NO <>'" + str[0] + "'");
                //删除前一个批次的库存
                string sSql = "SELECT * FROM TDB0431 WHERE LINE_NO = '" + str[5] + "' AND BIN_NO = '" + str[0] + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    //若当前投入的批次和之前的批次不一样，则删除之前的批次库存
                    if (dt.Rows[0]["CURRLOTID"].ToString() != str[3])
                    {
                        sSqls.Add("DELETE FROM WIP0003 WHERE LOTID = '" + dt.Rows[0]["CURRLOTID"].ToString() + "' ");
                    }
                }
                ////投入则清除
                //sSqls[3] = "DELETE FROM WIP0003 WHERE LOTID = '" + str[3] + "' ";//由于需要控制开舱门 不可以投入就清除
                int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sSqls);
                if (i > 0)
                    return "1";
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/InputBWC")]
        public string InputBWC(string sDiv, string sLineNo, string sGuanNo, string sLotid, string sUser)
        {

            
            try
            {
                ArrayList sSqls = new ArrayList();
                string sq = "UPDATE MX0060 set CURRLOTID = '" + sLotid + "', INNAM = '" + sUser + "', INTIM = TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')  ";
                 sq += " WHERE DIV = '" + sDiv + "' and LINE_NO='" + sLineNo + "' AND BIN_NO = '" + sGuanNo + "' ";
                sSqls.Add (sq);
                //删除前一个批次的库存
                //string sSql = "SELECT * FROM MX0060 WHERE DIV = '" + sDiv + "' and LINE_NO='" + sLineNo + "' AND BIN_NO = '" + sGuanNo + "' ";
                //DataTable dt = db.GetDataTable(sSql);
                //if (dt.Rows.Count > 0 && dt != null)
                //{
                //    //若当前投入的批次和之前的批次不一样，则删除之前的批次库存
                //    if (dt.Rows[0]["CURRLOTID"].ToString() != sLotid)
                //    {
                //        sSqls[1] = "DELETE FROM WIP0003 WHERE LOTID = '" + dt.Rows[0]["CURRLOTID"].ToString() + "' ";
                //    }
                //}
                sSqls.Add("DELETE FROM WIP0003 WHERE LOTID = '" + sLotid + "' ");

                int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sSqls);
                if (i > 0)
                    return "1";
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/InputCarbonOil")]
        public string InputCarbonOil(string sDiv, string sGuanNo, string sLotid, string sUser)
        {
            
            try
            {
                ArrayList sSqls = new ArrayList();
                string sITNBR = "", sITDSC = "", sITWGT = "", sUNIT = "";
                string sSel = "SELECT * FROM STB0002 WHERE LOTNO = '" + sLotid + "'";
                DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSel, null).Tables[0];
                if (dt1.Rows.Count > 0)
                {
                    sITNBR = dt1.Rows[0]["ITNBR"].ToString();
                    sITDSC = dt1.Rows[0]["ITDSC"].ToString();
                    sITWGT = dt1.Rows[0]["LOTWGT"].ToString();
                    sUNIT = dt1.Rows[0]["UNIT"].ToString();
                }
                if (sDiv == "1")//若是黑炭黑，由于其月日罐对应的特殊性，只更新有标准规格的
                {
                    //sSqls[0] = "UPDATE MX0060 set CURRLOTID = '" + sLotid + "', INNAM = '" + sUser + "', INTIM = TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')  ";
                    //sSqls[0] += " WHERE DIV = '" + sDiv + "' AND B_BIN_NO = '" + sGuanNo + "' AND RAW_CODE IS NOT NULL";
                    sSqls.Add("  INSERT INTO MX0075(ID,FAC,LOTID,ITNBR,ITDSC,TNAM,TTIM,ETIM,B_BIN_NO,WT,STA)  VALUES(SYS_GUID(),'07','" + sLotid + "','" + sITNBR + "','" + sITDSC + "','" + sUser + "',SYSDATE,SYSDATE,'" + sGuanNo + "','" + sITWGT + "','0')");
                    LogHelper.Debug("插入月罐队列：" + sSqls[0]);
                }
                else
                {
                    string sq = "UPDATE MX0060 set CURRLOTID = '" + sLotid + "', INNAM = '" + sUser + "', INTIM = TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')  ";
                    sq += " WHERE DIV = '" + sDiv + "' AND B_BIN_NO = '" + sGuanNo + "' ";
                    sSqls.Add(sq);
                }

                ////删除前一个批次的库存
                //string sSql = "SELECT * FROM MX0060 WHERE DIV = '" + sDiv + "' AND B_BIN_NO = '" + sGuanNo + "' ";
                //DataTable dt = db.GetDataTable(sSql);
                //if (dt.Rows.Count > 0 && dt != null)
                //{
                //    //若当前投入的批次和之前的批次不一样，则删除之前的批次库存
                //    if (dt.Rows[0]["CURRLOTID"].ToString() != sLotid)
                //    {
                //        sSqls[1] = "DELETE FROM WIP0003 WHERE LOTID = '" + dt.Rows[0]["CURRLOTID"].ToString() + "' ";
                //    }
                //}

                //原材料投入则清除 JOE 2019-03-21
                sSqls.Add("DELETE FROM WIP0003 WHERE LOTID = '" + sLotid + "' ");



                string[] str = { sLotid, sITNBR, sITDSC, sITWGT, "生产部-密炼工段", "", sUser, sUNIT };

                string sRc = RawMaterial.GetRawRecipients(str);

                int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString,sSqls);
                if (i > 0 && sRc == "1")
                    return "1";
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/SendCtlDoor")]
        public string SendCtlDoor(string sLineNo, string sBinNo, string sCtlDiv)
        {
            

            try
            {
                string sSql = string.Empty;
                if (sCtlDiv == "0")
                    sSql = "UPDATE MX0050 SET CLOSE_FLAG = 'Y' WHERE LINE_NO = '" + sLineNo + "' AND BIN_NO = '" + sBinNo + "'";
                else
                    sSql = "UPDATE MX0050 SET OPEN_FLAG = 'Y' WHERE LINE_NO = '" + sLineNo + "' AND BIN_NO = '" + sBinNo + "'";

                int i = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text,sSql,null);
                if (i > 0)
                    return "1";
                else
                    return "0";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }

    }
}
