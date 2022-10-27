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
    /// 半制品
    /// </summary>
    public class SemiFinishedProductsController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";

        [HttpGet]
        [Route("api/BeadMark")]
        public string BeadMark(string sLotId, string sUserid)
        {
           
            try
            {
                string ggsql = "select ITDSC from STB0003 where   LOTID LIKE '%" + sLotId + "%'  ";
                DataTable ggdt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, ggsql, null).Tables[0];
                if (ggdt == null || ggdt.Rows.Count == 0)
                {
                    return "Err:0";//批次号不存在
                }


                string sSql1 = "SELECT * FROM STI0001 WHERE LOTNO = '" + sLotId + "' ";
                DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                if (dt1 != null && dt1.Rows.Count > 0)
                    return "Err:1";//批次已扫描
                                   //else if (ggdt.Rows[0][0].ToString().Substring(0, 4) != "钢丝帘线")
                                   //{
                                   //    return "Err-999";
                                   //}
                string sSql = "SELECT * FROM STB0002 WHERE LOTNO = '" + sLotId + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    string sInsScan = "INSERT INTO STI0001 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,DZDAT,DZNAM) VALUES (";
                    sInsScan += "sys_guid(),'07','1','" + sLotId + "','" + dt.Rows[0]["ITNBR"].ToString() + "','" + dt.Rows[0]["ITDSC"].ToString() + "',SYSDATE,'" + sUserid + "')";
                    int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                    if (iResult > 0)
                        return "OK";
                    else
                        return "Err-NONE";
                }
                else
                    return "Err:0";//批次号不存在
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
        /// <param name="sDIV">钢丝、纤维区分</param>
        /// <param name="sLDIV">投入口区分</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ClearInput")]
        public string ClearInput(string sDIV, string sLDIV)
        {
           
            DataRow ROW = SHIFT.GetShift(DateTime.Now);
            try
            {
                if (sDIV == "09")//若是纤维
                {
                    string sSql = "DELETE FROM LTC0002 WHERE SDIV = '" + sDIV + "' AND LDIV = '" + sLDIV + "' AND MCHID = '07SF01'";
                    OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null);
                    return "OK";
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }

        /// <summary>
        /// 出厂退库
        /// </summary>
        /// <param name="sDIV">钢丝圈，压延，半成品，胶料</param>
        /// <param name="sLotid"></param>
        /// <param name="sITNBR"></param>
        /// <param name="sITDSC"></param>
        /// <param name="sQTY"></param>
        /// <param name="sCartID"></param>
        /// <param name="sINDIV">华盛，华茂，华达</param>
        /// <param name="sTo"></param>
        /// <param name="sNAM">操作人</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ExcuteBackToHuaYang")]
        public string ExcuteBackToHuaYang(string sDIV, string sLotid, string sITNBR, string sITDSC, string sQTY, string sCartID, string sINDIV, string sTo, string sNAM)
        {
            try
            {
               
                string sSql = string.Empty;
                string sSqlWIP = string.Empty;
                ArrayList sqlArr = new ArrayList();
                switch (sDIV)
                {
                    case "BD"://钢丝圈
                        sSql = "INSERT INTO STD0006(ID,FAC,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,INNAM,INTIM,INDIV) VALUES(";
                        sSql += "sys_guid(),'07','" + sLotid.ToUpper() + "','" + sITNBR + "','" + sITDSC + "','" + sCartID + "','" + sQTY + "','1','" + sNAM + "',SYSDATE,'" + sINDIV + "')";
                        sSqlWIP = "INSERT INTO WIP0001(ID,FAC,DIV,LOTID,ITNBR,ITDSC,TOOLNO,QTY,STA,STIME,ETIME,WDATE,WTIME,LOCKYN,WSHT,WBAN,MCHID,STKSTS) ";
                        sSqlWIP += " SELECT SYS_GUID(),'07',DIV,LOTID,AITNBR,AITDSC,'" + sCartID + "','" + sQTY + "','1',AUTIM,AUTIM,AUDAT,AUTIM,'N',AUSHT,AUBAN,AUMCH,'" + sTo + "' FROM LTC0001 WHERE LOTID = '" + sLotid.ToUpper() + "'";
                        sqlArr.Add(sSqlWIP);
                        sqlArr.Add(sSql);
                        break;
                    case "FS"://压延
                        string sSelSql = "SELECT DIV FROM LTC0001 WHERE LOTID = '" + sLotid.ToUpper() + "'";
                        DataTable dtSel = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSelSql, null).Tables[0];
                        if (dtSel != null && dtSel.Rows.Count > 0)
                        {
                            string divs = dtSel.Rows[0]["DIV"].ToString();
                            sSql = "INSERT INTO STE0006(ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,INNAM,INTIM,INDIV) VALUES(";
                            sSql += "sys_guid(),'07','" + divs + "','" + sLotid.ToUpper() + "','" + sITNBR + "','" + sITDSC + "','" + sCartID + "','" + sQTY + "','1','" + sNAM + "',SYSDATE,'" + sINDIV + "')";
                            sSqlWIP = "INSERT INTO WIP0001(ID,FAC,DIV,LOTID,ITNBR,ITDSC,TOOLNO,QTY,STA,STIME,ETIME,WDATE,WTIME,LOCKYN,WSHT,WBAN,MCHID,STKSTS) ";
                            sSqlWIP += " SELECT SYS_GUID(),'07',DIV,LOTID,AITNBR,AITDSC,'" + sCartID + "','" + sQTY + "','1',AUTIM,AUTIM,AUDAT,AUTIM,'N',AUSHT,AUBAN,AUMCH,'" + sTo + "' FROM LTC0001 WHERE LOTID = '" + sLotid.ToUpper() + "'";
                            sqlArr.Add(sSqlWIP);
                            sqlArr.Add(sSql);
                        }
                        break;
                    case "JB"://终炼胶
                        sSql = "INSERT INTO STC0008(ID,FAC,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,INNAM,INTIM,INDIV) VALUES(";
                        sSql += "sys_guid(),'07','" + sLotid.ToUpper() + "','" + sITNBR + "','" + sITDSC + "','" + sCartID + "','" + sQTY + "','1','" + sNAM + "',SYSDATE,'" + sINDIV + "')";
                        sSqlWIP = "INSERT INTO WIP0004(ID,FAC,DIV,LOTID,MCHID,WSHT,WBAN,ITNBR,ITDSC,TOOLNO,WT,STA,STIME,ETIME,WDATE,WTIME,LOCKYN,STKSTS) ";
                        sSqlWIP += " SELECT SYS_GUID(),'07',DECODE(DIV,1,1,2,0,DIV) DIV,LOTID,JMCH, JSHT,JBAN,ITNBR,ITDSC,'" + sCartID + "','" + sQTY + "','1',JTIM,JTIM,JDAT,JTIM,'N','" + sTo + "' FROM LTD0001 WHERE LOTID = '" + sLotid.ToUpper() + "'";
                        sqlArr.Add(sSqlWIP);
                        sqlArr.Add(sSql);
                        break;
                    case "OTH"://其他半成品
                        sSql = "INSERT INTO STF0009(ID,FAC,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,INNAM,INTIM,INDIV) VALUES(";
                        sSql += "sys_guid(),'07','" + sLotid.ToUpper() + "','" + sITNBR + "','" + sITDSC + "','" + sCartID + "','" + sQTY + "','1','" + sNAM + "',SYSDATE,'" + sINDIV + "')";
                        sSqlWIP = "INSERT INTO WIP0001(ID,FAC,DIV,LOTID,ITNBR,ITDSC,TOOLNO,QTY,STA,STIME,ETIME,WDATE,WTIME,LOCKYN,WSHT,WBAN,MCHID,STKSTS) ";
                        sSqlWIP += " SELECT SYS_GUID(),'07',DIV,LOTID,AITNBR,AITDSC,'" + sCartID + "','" + sQTY + "','1',AUTIM,AUTIM,AUDAT,AUTIM,'N',AUSHT,AUBAN,AUMCH,'" + sTo + "' FROM LTC0001 WHERE LOTID = '" + sLotid.ToUpper() + "'";
                        sqlArr.Add(sSqlWIP);
                        sqlArr.Add(sSql);
                        break;
                    default: break;
                }


                if (sqlArr.Count > 0)
                {
                    OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sqlArr);
                }
                return "OK";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                LogHelper.Debug( "ExcuteBackToHuaYang:" + sCartID + "批次号：" + sLotid.ToUpper());
                return ex.ToString();
            }
        }

        [HttpPost]
        [Route("api/ExecuteNoQueryCD")]
        public int ExecuteNoQueryCD(string[] param)
        {
            //0 ID
            //1 FAC
            //2 LOT号
            //3 规格代码
            //4 规格名称 
            //5 出库量
            //6 状态
            //7 仓库号
            //8 区域号
            //9 员工名称
            //10 工装号
            //11 出库区分
            try
            {
               
                string sdivName = string.Empty;
                switch (param[11])
                {
                    case "线边库":
                        sdivName = "12";
                        break;
                    case "华盛":
                        sdivName = "05";
                        break;
                    case "华茂":
                        sdivName = "06";
                        break;
                    case "华达":
                        sdivName = "08";
                        break;
                    case "华新":
                        sdivName = "09";
                        break;
                }

                string sChk = "SELECT COUNT(*) CNT FROM STD0001 WHERE LOTNO = '" + param[2] + "'";
                DataTable dtChk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sChk, null).Tables[0];
                if (dtChk.Rows.Count > 0)
                {
                    int iRst = 0;
                    bool bRst = int.TryParse(dtChk.Rows[0]["CNT"].ToString(), out iRst);
                    if (bRst)
                    {
                        if (iRst == 0)
                        {
                            return 999;//库存已无该批次
                        }
                    }

                }
                string sqlADD = @"INSERT INTO STD0003 (ID,FAC,LOTNO,ITNBR,ITDSC,QTY,STOSTS,STONO,STOAREA,OUTNAM,OUTTIM,CARTID,OUTDIV) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + Convert.ToDouble(param[5]) + "','" + param[6] + "','" + param[7] + "','" + param[8] + "','" + param[9] + "',sysdate,'" + param[10] + "','" + sdivName + "')";
                //string sqlDELETE = @"DELETE STD0001 WHERE ID='" + param[10] + "'";
                string sqlDELETE = "DELETE FROM STD0001 WHERE LOTNO = '" + param[2] + "'";

                string sUpdStock = string.Empty;
                if (param[11] == "华盛" || param[11] == "华茂" || param[11] == "华达" || param[11] == "华新")
                    sUpdStock = "DELETE FROM WIP0001 WHERE LOTID = '" + param[2] + "'";//删除WIP
                else
                    sUpdStock = "UPDATE WIP0001 SET STKSTS = '12' WHERE LOTID = '" + param[2] + "'";//更新为12-在线边库
                ArrayList sql = new ArrayList();
                sql.Add(sqlADD);
                sql.Add(sqlDELETE);
                sql.Add(sUpdStock);
                return OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sql);
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return 0;
            }
        }

        [HttpPost]
        [Route("api/ExecuteNoQueryCDYY")]
        public int ExecuteNoQueryCDYY(string[] param)
        {
            //0 ID
            //1 FAC
            //2 区分
            //3 LOT号
            //4 规格代码
            //5 规格名称
            //6 
            //7 出库量
            //8 状态
            //9 仓库号
            //10 区域号
            //11 排号
            //12 层号
            //13 员工名称
            //14 GetSTE0001Q 这个方法查出来的数据的ID
            //15 出库区分：线边库，华盛，华茂，华达
            try
            {
               
                string sdivName = string.Empty;
                switch (param[15])
                {
                    case "线边库":
                        sdivName = "12";
                        break;
                    case "华盛":
                        sdivName = "05";
                        break;
                    case "华茂":
                        sdivName = "06";
                        break;
                    case "华达":
                        sdivName = "08";
                        break;
                    case "华新":
                        sdivName = "09";
                        break;
                }
                string sChk = "SELECT COUNT(*) CNT FROM STE0001 WHERE LOTNO = '" + param[3] + "'";
                DataTable dtChk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sChk, null).Tables[0];
                if (dtChk.Rows.Count > 0)
                {
                    int iRst = 0;
                    bool bRst = int.TryParse(dtChk.Rows[0]["CNT"].ToString(), out iRst);
                    if (bRst)
                    {
                        if (iRst == 0)
                        {
                            string sChk11 = "SELECT COUNT(*) CNT FROM WIP0001 WHERE LOTID = '" + param[3] + "'";
                            DataTable dtChk11 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sChk11, null).Tables[0];
                            if (dtChk11.Rows.Count > 0)
                            {
                                int iRst11 = 0;
                                bool bRst11 = int.TryParse(dtChk11.Rows[0]["CNT"].ToString(), out iRst11);
                                if (iRst11 == 0)
                                {
                                    return 999;//库存已无该批次
                                }
                            }

                        }
                    }
                }

                string sChk1 = "SELECT * FROM STE0001 WHERE LOTNO = '" + param[3] + "'";
                DataTable dtChk1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sChk1, null).Tables[0];
                if (dtChk1.Rows.Count > 0)
                {
                    if (dtChk1.Rows[0]["STOSTS"].ToString() != "1")
                    {
                        return 2333;//该批次状态异常
                    }
                }

                string sChk12 = "SELECT * FROM WIP0001 WHERE LOTID = '" + param[3] + "'";
                DataTable dtChk12 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sChk12, null).Tables[0];
                if (dtChk12.Rows.Count > 0)
                {
                    if (dtChk12.Rows[0]["STA"].ToString() != "1")
                    {
                        return 2333;//该批次状态异常
                    }
                }


                string sqlADD = @"INSERT INTO STE0003 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,STONO,STOAREA,AREAROW,LAYER,OUTNAM,OUTTIM,OUTDIV) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + param[5] + "','" + param[6] + "','" + Convert.ToDouble(param[7]) + "','" + param[8] + "','" + param[9] + "','" + param[10] + "','" + param[11] + "','" + param[12] + "','" + param[13] + "',sysdate,'" + sdivName + "')";
                //string sqlDELETE = @"DELETE STE0001 WHERE ID='" + param[14] + "'";
                string sqlDELETE = @"DELETE STE0001 WHERE LOTNO='" + param[3] + "'";

                string sUpdStock = string.Empty;
                if (param[15] == "线边库")
                    sUpdStock = "UPDATE WIP0001 SET STKSTS = '12' WHERE LOTID = '" + param[3] + "'";//更新为12-在线边库
                else
                {
                    sUpdStock = "DELETE WIP0001 WHERE LOTID = '" + param[3] + "'";//删除库存
                }
                ArrayList sql = new ArrayList();
                sql.Add(sqlADD);
                sql.Add(sqlDELETE);
                sql.Add(sUpdStock);
                return OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sql);
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return 0;
            }
        }

        [HttpPost]
        [Route("api/ExecuteNoQueryRD")]
        public int ExecuteNoQueryRD(string[] param)
        {
            //0 ID
            //1 FAC
            //2 LOT号
            //3 规格代码
            //4 规格名称
            //5 入库量
            //6 状态
            //7 仓库号
            //8 区域号
            //9 员工名称
            //10 仓库区分
            //11 
            //12 IN-入库 OUTIN-出库退库
           
            try
            {
                //要入库的LOTNO，库存中是否存在
                string sSqlWIP = "SELECT * FROM WIP0001 WHERE LOTID = '" + param[2] + "'";
                DataTable dtbw = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlWIP, null).Tables[0];
                if (dtbw.Rows.Count == 0 || dtbw == null)
                    return -5;
                string sqlCC = "SELECT * FROM STD0001 WHERE LOTNO='" + param[2] + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlCC, null).Tables[0];
                if (dt.Rows.Count > 0)
                    return -1;
                else
                {
                    ArrayList sql = new ArrayList();
                    if (param[12] == "IN")//普通入库
                    {
                        string sqlX = @"INSERT INTO STD0001 (ID,FAC,LOTNO,ITNBR,ITDSC,QTY,STOSTS,STONO,STOAREA,INNAM,INTIM,CARTID) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + Convert.ToDouble(param[5]) + "','" + Convert.ToChar(param[6]) + "','" + param[7] + "','" + param[8] + "','" + param[9] + "',sysdate,'" + param[11] + "')";
                        string sqlLvLi = @"INSERT INTO STD0002 (ID,FAC,LOTNO,ITNBR,ITDSC,QTY,STOSTS,STONO,STOAREA,INNAM,INTIM,CARTID) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + Convert.ToDouble(param[5]) + "','" + Convert.ToChar(param[6]) + "','" + param[7] + "','" + param[8] + "','" + param[9] + "',sysdate,'" + param[11] + "')";
                        string sUpdStock = "UPDATE WIP0001 SET STKSTS = '11' WHERE LOTID = '" + param[2] + "'";//更新为11-在库
                        sql.Add(sqlX);
                        sql.Add(sqlLvLi);
                        sql.Add(sUpdStock);
                    }
                    else//出库退库
                    {
                        string sqlX = @"INSERT INTO STD0001 (ID,FAC,LOTNO,ITNBR,ITDSC,QTY,STOSTS,STONO,STOAREA,INNAM,INTIM,CARTID) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + Convert.ToDouble(param[5]) + "','" + Convert.ToChar(param[6]) + "','" + param[7] + "','" + param[8] + "','" + param[9] + "',sysdate,'" + param[11] + "')";
                        string sqlLvLi = @"INSERT INTO STD0006 (ID,FAC,LOTNO,ITNBR,ITDSC,QTY,STOSTS,STONO,STOAREA,CNAM,CTIM) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + Convert.ToDouble(param[5]) + "','" + Convert.ToChar(param[6]) + "','" + param[7] + "','" + param[8] + "','" + param[9] + "',sysdate)";

                        string sUpdStock = string.Empty;
                        string sCheckWip = "SELECT COUNT(*) CNT FROM WIP0001 WHERE LOTID = '" + param[2] + "'";
                        DataTable dtWIP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sCheckWip, null).Tables[0];
                        if (dtWIP.Rows.Count > 0)
                        {
                            sUpdStock = "UPDATE WIP0001 SET STKSTS = '11',QTY = '" + Convert.ToDouble(param[5]) + "' WHERE LOTID = '" + param[2] + "'";//更新为11-在库
                        }
                        else
                        {
                            sUpdStock = "INSERT INTO WIP0001 (ID,FAC,DIV,LOTID,RFIDNO,MCHID,WSHT,WBAN,ITNBR,ITDSC,TOOLNO,QTY,STA,STIME,ETIME,WDATE,WTIME,LOCKYN,STKSTS) ";
                            sUpdStock += "SELECT SYS_GUID(),'07',DIV,LOTID,'',AUMCH,AUSHT,AUBAN,AITNBR,AITDSC,BID,'" + Convert.ToDouble(param[5]) + "',STYN,'','',AUDAT,AUTIM,'N','11' FROM LTC0001 WHERE LOTID = '" + param[2] + "'";
                        }
                        sql.Add(sqlX);
                        sql.Add(sqlLvLi);
                        sql.Add(sUpdStock);
                    }
                    return OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sql);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return 0;
            }
        }


        [HttpPost]
        [Route("api/ExecuteNoQueryRDYY")]
        public int ExecuteNoQueryRDYY(string[] param)
        {
            //0 ID
            //1 FAC
            //2 区分
            //3 LOT号
            //4 规格代码
            //5 规格名称
            //6 
            //7 入库量
            //8 状态
            //9 仓库号
            //10 区域号
            //11 排号
            //12 层号
            //13 员工名称
            //14 仓库区分

           
            try
            {
                //要入库的LOTNO，库存中是否存在
                string sSqlWIP = "SELECT * FROM WIP0001 WHERE LOTID = '" + param[3] + "'";
                DataTable dtYY = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlWIP, null).Tables[0];
                if (dtYY.Rows.Count == 0 || dtYY == null)
                    return -5;
                //判断库存是否存在
                string sqlCC = @"SELECT * FROM STE0001 WHERE LOTNO='" + param[3] + "'";
                DataTable dtcount = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlCC, null).Tables[0];
                if (dtcount.Rows.Count > 0)
                {
                    return -1;
                }
                else
                {
                    string sqlC = @"SELECT COUNT(*) FROM STE0001 WHERE DIV='" + param[2] + "' AND STONO='" + param[9] + "' AND STOAREA='" + param[10] + "' AND AREAROW='" + param[11] + "' AND LAYER='" + param[12] + "' GROUP BY DIV,STONO,STOAREA,AREAROW,LAYER ";
                    DataTable dtC = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlC, null).Tables[0];
                    string sqlKC = @"SELECT MAXQTY FROM STA0001 WHERE DIV='" + param[14] + "' AND STONO='" + param[9] + "' AND STOAREA='" + param[10] + "' AND AREAROW='" + param[11] + "' AND LAYER='" + param[12] + "'  ";
                    DataTable DTKC = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlKC, null).Tables[0];

                    if (dtC.Rows.Count > 0)
                    {
                        int kc = Convert.ToInt32(DTKC.Rows[0][0]) - Convert.ToInt32(dtC.Rows[0][0]);
                        if (kc < 1)
                        {
                            return -2;
                        }
                        else
                        {
                            string sqlX = @"INSERT INTO STE0001 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,STONO,STOAREA,AREAROW,LAYER,INNAM,INTIM) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + param[5] + "','" + param[6] + "','" + Convert.ToDouble(param[7]) + "','" + param[8] + "','" + param[9] + "','" + param[10] + "','" + param[11] + "','" + param[12] + "','" + param[13] + "',sysdate)";
                            string sqlLvLi = @"INSERT INTO STE0002 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,STONO,STOAREA,AREAROW,LAYER,INNAM,INTIM) 
                         VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + param[5] + "','" + param[6] + "','" + Convert.ToDouble(param[7]) + "','" + param[8] + "','" + param[9] + "','" + param[10] + "','" + param[11] + "','" + param[12] + "','" + param[13] + "',sysdate)";
                            string sUpdStock = "UPDATE WIP0001 SET STKSTS = '11' WHERE LOTID = '" + param[3] + "'";//更新为11-在库
                            ArrayList sql = new ArrayList();
                            sql.Add(sqlX);
                            sql.Add(sqlLvLi);
                            sql.Add(sUpdStock);
                            return OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sql);
                        }
                    }
                    else
                    {

                        string sqlX = @"INSERT INTO STE0001 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,STONO,STOAREA,AREAROW,LAYER,INNAM,INTIM) 
                        VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + param[5] + "','" + param[6] + "','" + Convert.ToDouble(param[7]) + "','" + param[8] + "','" + param[9] + "','" + param[10] + "','" + param[11] + "','" + param[12] + "','" + param[13] + "',sysdate)";
                        string sqlLvLi = @"INSERT INTO STE0002 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,STONO,STOAREA,AREAROW,LAYER,INNAM,INTIM) 
                         VALUES ('" + param[0] + "','" + param[1] + "','" + param[2] + "','" + param[3] + "','" + param[4] + "','" + param[5] + "','" + param[6] + "','" + Convert.ToDouble(param[7]) + "','" + param[8] + "','" + param[9] + "','" + param[10] + "','" + param[11] + "','" + param[12] + "','" + param[13] + "',sysdate)";
                        string sUpdStock = "UPDATE WIP0001 SET STKSTS = '11' WHERE LOTID = '" + param[3] + "'";//更新为11-在库
                        ArrayList sql = new ArrayList();
                        sql.Add(sqlX);
                        sql.Add(sqlLvLi);
                        sql.Add(sUpdStock);

                        return OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sql);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return 0;
            }
        }

        [HttpPost]
        [Route("api/ExecuteOtherHalbToOtherFac")]
        public int ExecuteOtherHalbToOtherFac(string[] param)
        {
            //0 ID
            //1 FAC
            //2 LOT号
            //3 规格代码
            //4 规格名称 
            //5 出库量
            //6 状态
            //7 员工名称
            //8 工装号
            //9 出库区分
            try
            {
               
                string sdivName = string.Empty;
                switch (param[9])
                {
                    case "华盛":
                        sdivName = "05";
                        break;
                    case "华茂":
                        sdivName = "06";
                        break;
                    case "华达":
                        sdivName = "08";
                        break;
                    case "华新":
                        sdivName = "09";
                        break;
                }

                string sqlADD = "INSERT INTO STF0002 (ID,FAC,LOTNO,ITGRPCOD3,STODIV,OUTDAT,GTMCHID,WKCOD,ITNBR,ITDSC,CARTID,CARTSTATE,LOTSTATE,PRODQTY,UNIT,MCHID,WDATE,WTIME,WBAN,WSHFT,USERID,OUTDIV,OUTCELL) VALUES (";
                sqlADD += "sys_guid(),'" + param[1] + "','" + param[2] + "','','',SYSDATE,'','','" + param[3] + "','" + param[4] + "','" + param[8] + "','','" + param[6] + "','" + param[5] + "','','','','','','','" + param[7] + "','" + sdivName + "','')";
                string sUpdStock = "DELETE FROM WIP0001 WHERE LOTID = '" + param[2] + "'";//删除WIP
                ArrayList sql = new ArrayList();
                sql.Add(sqlADD);
                sql.Add(sUpdStock);
                return OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sql);
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return 0;
            }
        }

        [HttpGet]
        [Route("api/GetBIDInfo")]
        public DataTable GetBIDInfo(string str)
        {
           

            try
            {
                string strSql = "SELECT * FROM LTC0001 WHERE DIV IN ('08','09')  AND LOTID ='" + str + "'";
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
        [Route("api/GetCPTXX")]
        public DataTable GetCPTXX(string str)
        {
           
            try
            {
                if (string.IsNullOrEmpty(str)) return null;
                string Sql_div = @"SELECT * FROM EDC0003
                                          WHERE ITNBR = '" + str + "' AND USEYN = 'Y'";
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
        [Route("api/GetInPut")]
        public string GetInPut(InPut inPut)
        {
            //0投入状态，正常还是误投
            //1钢丝还是纤维
            //2投入口区分，默认5
            //3当前投入批次号
            //4班组
            //5原材料规格代码
            //6原材料规格描述 
            //7上次投入批次
            //8投入口区分，6(纤维)
            //9当前批次号2（纤维）
            //10上次投入批次2（纤维）
           
            DataRow ROW = SHIFT.GetShift(DateTime.Now);
            try
            {
               ArrayList list = new ArrayList();

                if (inPut.bClrStock)//若需要清除WIP库存
                {
                    if (inPut.str[1] == "08")//钢丝
                    {
                        string[] strLots = inPut.str[7].Split(',');
                        if (strLots.Length > 0)
                        {
                            foreach (string sVar in strLots)
                            {
                                list.Add("DELETE FROM WIP0003 WHERE LOTID = '" + sVar + "'");
                            }
                        }
                    }
                    else//纤维
                    {
                        list.Add("DELETE FROM WIP0003 WHERE LOTID = '" + inPut.str[7] + "'");
                        list.Add("DELETE FROM WIP0003 WHERE LOTID = '" + inPut.str[10] + "'");
                    }
                }

                //更新钢丝投入记录，投入时间，投入人
                if (inPut.str[1] == "08")//钢丝
                {
                    string[] strLots = inPut.str[3].Split(',');
                    if (strLots.Length > 0)
                    {
                        foreach (string sVar in strLots)
                        {
                            list.Add("UPDATE STI0001 SET USEDAT = SYSDATE, USENAM = '" + inPut.sLoginName + "' WHERE LOTNO = '" + sVar + "' AND RMVDAT IS NULL");
                        }
                    }
                }
                else
                {
                    string[] strLots = inPut.str[3].Split(',');
                    if (strLots.Length > 0)
                    {
                        foreach (string sVar in strLots)
                        {
                            string sSql1 = "SELECT * FROM STI0002 WHERE LOTNO = '" + sVar + "' AND USENAM IS NULL";
                            DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                            if (dt1 != null && dt1.Rows.Count > 0)
                            {
                                list.Add("UPDATE STI0002 SET USEDAT = SYSDATE, USENAM = '" + inPut.sLoginName + "' WHERE LOTNO = '" + sVar + "' AND RMVDAT IS NULL");
                            }
                        }
                    }


                    string[] strLots1 = inPut.str[9].Split(',');
                    if (strLots1.Length > 0)
                    {
                        foreach (string sVar in strLots1)
                        {
                            string sSql1 = "SELECT * FROM STI0002 WHERE LOTNO = '" + sVar + "' AND USENAM IS NULL";
                            DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                            if (dt1 != null && dt1.Rows.Count > 0)
                            {
                                list.Add("UPDATE STI0002 SET USEDAT = SYSDATE, USENAM = '" + inPut.sLoginName + "' WHERE LOTNO = '" + sVar + "' AND RMVDAT IS NULL");
                            }
                        }
                    }


                }

                string strDelete = "DELETE FROM LTC0002 WHERE MCHID='07SF01' AND SDIV = '" + inPut.str[1] + "' AND LDIV='" + inPut.str[2] + "'";
                string strDelete1 = string.Empty;
                if (inPut.str[1] == "09")//纤维
                    strDelete1 = "DELETE FROM LTC0002 WHERE MCHID='07SF01' AND SDIV = '" + inPut.str[1] + "' AND LDIV='" + inPut.str[8] + "'";
                string strSql = "INSERT INTO LTC0002( " +
                    @"ID,FAC,DIV,SDIV,LDIV, " +
                    @"MCHID,LOTID,NBRRE,DSCRE, " +
                    @"WTIM, " +
                    @"WDAT, " +
                    @"WSHT,WBAN,LTLOTID " +
                    @") VALUES ( " +
                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + inPut.str[0] + "','" + inPut.str[1] + "','" + inPut.str[2] + "', " +
                    @"'07SF01','" + inPut.str[3] + "','" + inPut.str[5] + "','" + inPut.str[6] + "', " +
                    @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'), " +
                    @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                    @"'" + ROW["WSHT"] + "','" + inPut.str[4] + "','" + inPut.str[7] + "'" +
                    @")";
                string strSql1 = string.Empty;
                if (inPut.str[1] == "09")//纤维
                    strSql1 = "INSERT INTO LTC0002( " +
                       @"ID,FAC,DIV,SDIV,LDIV, " +
                       @"MCHID,LOTID,NBRRE,DSCRE, " +
                       @"WTIM, " +
                       @"WDAT, " +
                       @"WSHT,WBAN,LTLOTID " +
                       @") VALUES ( " +
                       @"'" + Guid.NewGuid() + "','" + FAC + "','" + inPut.str[0] + "','" + inPut.str[1] + "','" + inPut.str[8] + "', " +
                       @"'07SF01','" + inPut.str[9] + "','" + inPut.str[5] + "','" + inPut.str[6] + "', " +
                       @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'), " +
                       @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
                       @"'" + ROW["WSHT"] + "','" + inPut.str[4] + "','" + inPut.str[10] + "'" +
                       @")";
                list.Add(strDelete);
                if (inPut.str[1] == "09")//纤维
                    list.Add(strDelete1);
                list.Add(strSql);
                if (inPut.str[1] == "09" && !string.IsNullOrEmpty(inPut.str[8]) && !string.IsNullOrEmpty(inPut.str[9]))
                    list.Add(strSql1);
                int i = OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, list);
                if (i > 0)
                    return "投入成功！";
                else
                    return "投入失败！";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }

        [HttpGet]
        [Route("api/GetInPutYN")]
        public string GetInPutYN(string sdiv)
        {
            string ycnbr = string.Empty;
           
            try
            {
                string strSql = "SELECT * FROM PAD0204 WHERE DIV='1' AND LDIV = '" + sdiv + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    ycnbr = dt.Rows[0]["YCNBR"].ToString();
                }
                return ycnbr;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }

        [HttpPost]
        [Route("api/GetLEAVE")]
        public DataTable GetLEAVE(string[] param)
        {
            //0 区分
            //1 仓库号 
            //2 区域号
            //3 排号 

            try
            {
                string sql = "SELECT LAYER FROM STA0001 WHERE DIV= '"+param[0]+"' AND USEYN='Y' AND STONO= '"+param[1]+"' AND STOAREA= '"+param[2]+"' AND AREAROW= '"+param[3]+"' ORDER BY LAYER";
               
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM.Rows.Count > 0)
                {
                    return dtM;
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
        [Route("api/GetLTA0001")]
        public DataTable GetLTA0001(string str)
        {
           
            try
            {
                if (string.IsNullOrEmpty(str)) return null;
                string Sql_div = @"SELECT * FROM LTA0001
                                          WHERE BARCODE = '" + str + "'";
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

        [HttpGet]
        [Route("api/GetSTG0002")]
        public DataTable GetSTG0002(string str)
        {
           
            try
            {
                if (string.IsNullOrEmpty(str)) return null;
                string Sql_div = @"SELECT * FROM STG0002
                                          WHERE BARCODE = '" + str + "' ORDER BY OUTDAT";
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
        [Route("api/GetLTC001")]
        public DataTable GetLTC001(string[] parm)
        {
            //0 LOT号
            //1 区分

            string[] DIV;
           
            try
            {
                string sql = "SELECT * FROM LTC0001 WHERE  LOTID= '"+parm[0]+"'";
                DataTable dtM = new DataTable();
                if (parm[1].IndexOf("|") > -1)
                {
                    DIV = parm[1].Split('|');
                    sql += "AND (DIV = '"+DIV[0]+"' OR DIV = '"+DIV[1]+"')";
                    dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                }
                else
                {
                    sql += "AND DIV = '"+parm[1]+"'";
                    dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                }
                if (dtM.Rows.Count > 0)
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

        #region 压延投入



        /// <summary>
        /// 获取当前投入的钢丝（08）/纤维批次（09）
        /// </summary>
        /// <param name="sdiv">钢丝or纤维</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetNowLotId")]
        public string GetNowLotId(string sdiv)
        {
            string lotid = string.Empty;
            string strSql = string.Empty;
           
            try
            {
                if (sdiv == "08")//钢丝
                {
                    strSql = "SELECT * FROM LTC0002 WHERE MCHID='07SF01' AND SDIV = '" + sdiv + "' AND LDIV='5'";
                    DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        lotid = dt.Rows[0]["LOTID"].ToString();
                    }
                }
                else//纤维
                {
                    strSql = "SELECT * FROM LTC0002 WHERE MCHID='07SF01' AND SDIV = '" + sdiv + "' AND LDIV in ('5','6') ORDER BY LDIV";
                    DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            lotid += dt.Rows[i]["LOTID"].ToString() + "^";
                        }
                        lotid = lotid.TrimEnd('^');
                    }
                }
                return lotid;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }
        /// <summary>
        /// 获取之前投入的钢丝/纤维批次
        /// </summary>
        /// <param name="sdiv">钢丝or纤维</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetPreviousLotId")]
        public string GetPreviousLotId(string sdiv)
        {
            string ltlotid = string.Empty;
            string strSql = string.Empty;
           
            try
            {
                if (sdiv == "08")//钢丝
                {
                    strSql = "SELECT * FROM LTC0002 WHERE MCHID='07SF01' AND SDIV = '" + sdiv + "' AND LDIV='5'";
                    DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ltlotid = dt.Rows[0]["LTLOTID"].ToString();
                    }
                }
                else
                {
                    strSql = "SELECT * FROM LTC0002 WHERE MCHID='07SF01' AND SDIV = '" + sdiv + "' AND LDIV IN ('5','6') ORDER BY LDIV";
                    DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        //ltlotid = dt.Rows[0]["LTLOTID"].ToString();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ltlotid += dt.Rows[i]["LTLOTID"].ToString() + "^";
                        }
                        ltlotid = ltlotid.TrimEnd('^');
                    }
                }
                return ltlotid;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return "Err-" + ex.Message;
            }
        }


        #endregion

        [HttpPost]
        [Route("api/GetROW")]
        public DataTable GetROW(string[] param)
        {
            //0 区分
            //1 仓库号 
            //2 区域号
            try
            {
                string sql = "SELECT DISTINCT AREAROW FROM STA0001 WHERE DIV= '"+param[0]+"' AND USEYN='Y' AND STONO= '"+param[1]+"' AND STOAREA= '"+param[2]+"' ORDER BY AREAROW";
               
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM.Rows.Count > 0)
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

        [HttpPost]
        [Route("api/GetSTD0001D")]
        public DataTable GetSTD0001D(string[] param)
        {
            //0 LOT号

            try
            {
                string sql = "SELECT * FROM STD0001 WHERE LOTNO = '"+param[0]+"'";
               
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (DT.Rows.Count > 0)
                {
                    return DT;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


        [HttpPost]
        [Route("api/GetSTE0001Q")]
        public DataTable GetSTE0001Q(string[] param)
        {
            //0 LOT号
            //1 区分

            try
            {
                string[] DIV = param[1].Split('|');
                string sql = "SELECT ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,STONO,STOAREA,AREAROW,LAYER,INNAM,INTIM FROM STE0001 WHERE (DIV= '"+DIV[0]+"' OR DIV= '"+DIV[1]+"') AND LOTNO= '"+param[0]+"'";
               
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (DT.Rows.Count > 0)
                {
                    return DT;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpPost]
        [Route("api/GetSTONONAMED")]
        public DataTable GetSTONONAMED(string[] parm)
        {
            //0 区分
            try
            {
                string sql = "SELECT DISTINCT STONO,STONAM FROM STA0001 WHERE DIV= '"+parm[0]+"' AND USEYN='Y' ORDER BY STONO";
               
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM.Rows.Count > 0)
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

        [HttpPost]
        [Route("api/GetSTOAREAD")]
        public DataTable GetSTOAREAD(string[] param)
        {
            //0 区分
            //1 仓库号
            try
            {
                string sql = "SELECT DISTINCT STOAREA,STOAREANAM FROM STA0001 WHERE DIV= '"+ param[0] +"' AND USEYN='Y' AND STONO= '"+ param[1] +"' ORDER BY STOAREA";
               
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM.Rows.Count > 0)
                {
                    return dtM;
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

        [HttpPost]
        [Route("api/GetSTOCKD")]
        public DataTable GetSTOCKD(string[] param)
        {
            //0 LOT号

            try
            {
                string sql = "SELECT LOTNO,(ITNBR||'-'||ITDSC) AS ITN,QTY,STOSTS FROM STD0001 WHERE LOTNO LIKE '"+ "%" + param[0] + "%" + "'";
                //OracleParameter[] par = new OracleParameter[] { new OracleParameter("LOTNO", ) };
               
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (DT.Rows.Count > 0)
                {
                    return DT;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpPost]
        [Route("api/GetSTOCKDYY")]
        public DataTable GetSTOCKDYY(string[] param)
        {
            //0 LOT号
            //1 区分

            try
            {
                string[] DIV = param[1].Split('|');
                string sql = "SELECT LOTNO,(ITNBR||'-'||ITDSC) AS ITN,QTY,STOSTS FROM STE0001 WHERE LOTNO LIKE '"+ "%" + param[0] + "%" + "' AND (DIV= '"+DIV[0]+ "' OR DIV= '" + DIV[1] + "')";
                //OracleParameter[] par = new OracleParameter[] { new OracleParameter("LOTNO", "%" + param[0] + "%"), new OracleParameter("DIV", DIV[0]), new OracleParameter("DIVB", DIV[1]) };
               
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (DT.Rows.Count > 0)
                {
                    return DT;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/GetSWGG")]
        public DataTable GetSWGG(string str)
        {
           
            try
            {
                DateTime dtTemp = DateTime.Now;
                //当前月初
                DateTime yc = dtTemp.AddDays(-dtTemp.Day + 1);
                //当前月末
                DateTime ym = yc.AddMonths(1).AddDays(-1);

                DateTime yd = Convert.ToDateTime(ym.ToString("yyyy-MM-dd 14:59:59"));

                DateTime st = DateTime.Now;
                if (DateTime.Now.Hour < 15)
                {
                    st = DateTime.Now;

                }
                else
                {
                    st = DateTime.Now.AddDays(1);
                }
                //工厂日期的月初
                DateTime st1 = st.AddDays(-st.Day + 1);
                //工厂日期的月末
                DateTime st2 = st;


                if (string.IsNullOrEmpty(str)) return null;
                string Sql_div = @"select
            ISLASTMID,isdate as ISLASTBY,isdateG,itnbr,ITDSC,(case when ISLAST='1' then '收尾' else '' end) ISLAST 
            from (select a.*,(case when to_char(isdate,'hh24')>=15  then isdate+1 else isdate end ) isdateG from ppa0001 a where ISLAST='1' and ISDATE between to_date('" + st1.ToString("yyyy-MM-dd") + "','yyyy-MM-dd') and  to_date('" + st2.ToString("yyyy-MM-dd") + "','yyyy-MM-dd')) WHERE ITNBR = '" + str + "' ORDER BY ISLASTMID";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, Sql_div, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
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

        /// <summary>
        /// 参数化获取表数据
        /// </summary>
        /// <returns>ColNam与content需要一一对应</returns>
        [HttpPost]
        [Route("api/GetTableData")]
        public DataTable GetTableData(TableData tableData)
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
                sql += string.Format(" ORDER BY {0} ", tableData.sort[0]);
                for (int i = 1; i < tableData.sort.Length; i++)
                {
                    sql += string.Format(" , {0}", tableData.sort[i]);
                }
            }

            DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
            
            return dtM;
           
        }



        [HttpPost]
        [Route("api/GetWIP0001Q")]
        public DataTable GetWIP0001Q(string[] param)
        {
            //0 LOT号
            //1 区分

            try
            {
                string sql = "SELECT * FROM WIP0001 WHERE (DIV= '08' OR DIV= '09')  AND (STKSTS = '10' OR STKSTS = '11')  AND LOTID= '"+param[0]+"'";
               
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (DT.Rows.Count > 0)
                {
                    return DT;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        [HttpPost]
        [Route("api/NEWGetWIP0001Q")]
        public DataTable NEWGetWIP0001Q(string[] param)
        {
            //0 LOT号
            //1 区分
            //2 卷轴号
            try
            {
                string sql = "SELECT * FROM WIP0001 WHERE (DIV= '08' OR DIV= '09')  AND (STKSTS = '10' OR STKSTS = '11')  AND TOOLNO= '" + param[2] + "'";
               
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (DT.Rows.Count > 0)
                {
                    return DT;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }

        [HttpGet]
        [Route("api/GetXianweiScanOrNot")]
        public DataTable GetXianweiScanOrNot(string sLotid)
        {
           

            try
            {
                string strSql = "SELECT * FROM STI0002 WHERE LOTNO ='" + sLotid + "' AND RMVDAT IS NULL";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        /// <summary>
        /// FRUNK·DU 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/INPUTRING")]
        public string INPUTRING(string[] strl)
        {
            try
            {
               
                //str[0] id
                //str[1] 07
                //str[2] lot
                //str[3] mchid
                //str[4] 投入口
                //str[5] 轮盘号
                //str[6] 操作人
                string sql = @"select * from RINGINPUT t where trk = '" + strl[4] + "' and mchid = '" + strl[3] + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        sql = @"delete from RINGINPUT where id = '" + dt.Rows[i]["ID"].ToString() + "'";
                        OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sql, null);
                    }
                }
                string sSecVI = @"INSERT INTO RINGINPUT  VALUES ('" + strl[0] + "','" + strl[1] + "','" + strl[2] + "','" + strl[3] + "','" + strl[4] + "','" + strl[5] + "','" + strl[6] + "',sysdate）";
                OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sSecVI, null);
                return "OK";
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return "Err:" + ex;
            }
        }

        [HttpGet]
        [Route("api/JudgeEarlyLot")]
        public string JudgeEarlyLot(string sLotid, string sNowLotid, string sNowLotid1)//当前将要投入lot，当前投入lot
        {
            //0 Lot号

           

            try
            {
                string strSql = "SELECT LOTID,ITNBR,RECEPTTIM FROM STB0003 WHERE LOTID ='" + sLotid + "' AND TXNDIV = '21' ORDER BY RECEPTTIM DESC";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, strSql, null).Tables[0];
                string sITNBR = string.Empty;
                DateTime dtRecipTim;
                string sResult = string.Empty;
                if (dt.Rows.Count > 0 && dt != null)
                {
                    sITNBR = dt.Rows[0]["ITNBR"].ToString();
                    dtRecipTim = Convert.ToDateTime(dt.Rows[0]["RECEPTTIM"].ToString());
                    string sSql1 = " SELECT A.LOTID, B.RECEPTTIM ";
                    sSql1 += "  FROM (SELECT * ";
                    sSql1 += "          FROM WIP0003 ";
                    sSql1 += "         WHERE ITNBR = '" + sITNBR + "' AND STKSTS = '12' AND STA = '1' ";
                    if (!string.IsNullOrEmpty(sNowLotid) && !string.IsNullOrEmpty(sNowLotid1))
                        sSql1 += " AND LOTID NOT IN ('" + sNowLotid + "','" + sNowLotid1 + "')";
                    else if (!string.IsNullOrEmpty(sNowLotid))
                        sSql1 += " AND LOTID !='" + sNowLotid + "'";
                    else if (!string.IsNullOrEmpty(sNowLotid1))
                        sSql1 += " AND LOTID !='" + sNowLotid1 + "'";
                    else sSql1 += "";
                    sSql1 += " ) A, ";//排除当前已经投入的批次
                    sSql1 += "       (SELECT * ";
                    sSql1 += "          FROM (SELECT A.*, ";
                    sSql1 += "                       ROW_NUMBER () OVER (PARTITION BY LOTID ORDER BY RECEPTTIM DESC) RN FROM STB0003 A WHERE TXNDIV = '21') ";
                    sSql1 += "         WHERE RN = 1) B ";
                    sSql1 += " WHERE     A.LOTID = B.LOTID ";
                    sSql1 += "       AND RECEPTTIM < TO_DATE ('" + dtRecipTim.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS')";
                    DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                    if (dt1.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            sResult += dt1.Rows[i]["LOTID"].ToString() + "^";
                        }
                        return sResult;
                    }
                    else if (dt1.Rows.Count == 0)
                        return "OK";
                    else
                        return "";
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return ex.Message;
            }
        }




        [HttpPost]
        [Route("api/NEWGetSTE0001Q")]
        public DataTable NEWGetSTE0001Q(string[] param)
        {
            //0 LOT号
            //1 区分
            //2卷轴号

            try
            {
                string[] DIV = param[1].Split('|');
                string sql = "SELECT ID,FAC,DIV,LOTNO,ITNBR,ITDSC,CARTID,QTY,STOSTS,STONO,STOAREA,AREAROW,LAYER,INNAM,INTIM FROM STE0001 WHERE 1=1 AND CARTID='" + param[2] + "'";
               
                DataTable DT = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (DT.Rows.Count > 0 && DT != null)
                {
                    return DT;
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return null;
            }
        }


        //根据卷轴号退压延大卷到华阳
        [HttpGet]
        [Route("api/newVerifyBackToHuaYang")]
        public string newVerifyBackToHuaYang(string sDIV, string sLotid, string CarId1)
        {
            try
            {
               
                string sSqlWIP = string.Empty;
                sSqlWIP = "SELECT * FROM WIP0001 WHERE LOTID = '" + sLotid + "' OR TOOLNO='" + CarId1 + "' ";
                DataTable dtWIP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlWIP, null).Tables[0];
                if (dtWIP.Rows.Count > 0 && dtWIP != null)
                {
                    return "1";
                }
                DataTable dt1 = null;
                string sResult = string.Empty;
                string sSql = string.Empty;
                sSql = "SELECT * FROM STE0003 WHERE LOTNO = '" + sLotid + "' ORDER BY OUTTIM DESC";
                dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt1.Rows.Count > 0 && dt1 != null)
                {
                    sResult = "OK" + "^" + dt1.Rows[0]["ITNBR"].ToString() + "^" + dt1.Rows[0]["ITDSC"].ToString() + "^" + dt1.Rows[0]["CARTID"].ToString() + "^" + dt1.Rows[0]["QTY"].ToString() + "^" + dt1.Rows[0]["OUTDIV"].ToString();
                }
                else
                {
                    sResult = "";
                }
                return sResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return ex.ToString();
            }
        }

        [HttpGet]
        [Route("api/RecordFiberScan")]
        public string RecordFiberScan(string sLotId, string sUserid)
        {
           
            try
            {
                string sSql = "SELECT * FROM STB0002 WHERE LOTNO = '" + sLotId + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    string sSql1 = "SELECT * FROM STI0002 WHERE LOTNO = '" + sLotId + "' ";
                    DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                    if (dt1.Rows.Count > 0 && dt1 != null)
                        return "Err:1";//批次已扫描且没有撤下

                    if (true)
                    {
                        string sInsScan = "INSERT INTO STI0002 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,INDAT,INNAM) VALUES (";
                        sInsScan += "sys_guid(),'07','1','" + sLotId + "','" + dt.Rows[0]["ITNBR"].ToString() + "','" + dt.Rows[0]["ITDSC"].ToString() + "',SYSDATE,'" + sUserid + "')";
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                        if (iResult > 0)
                            return "OK";
                        else
                            return "Err-NONE";
                    }
                    else
                    {
                        return "Err-2";//没有出库或其他异常
                    }
                }
                else
                {
                    return "Err:0";//批次号不存在
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/RecordSteelDingRmv")]
        public string RecordSteelDingRmv(string sLotId, string sUserid)
        {
           
            try
            {
                string sSql = "SELECT * FROM STB0002 WHERE LOTNO = '" + sLotId + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt.Rows.Count > 0 && dt != null)
                {
                    string sSql1 = "SELECT * FROM STI0001 WHERE LOTNO = '" + sLotId + "' AND USEDAT IS NOT NULL AND RMVDAT IS NULL";
                    DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                    if (dt1.Rows.Count == 0 || dt1 == null)
                    {
                        return "Err:1";//批次号没有投用或者已被封箱
                    }
                    else
                    {
                        string sSql2 = "UPDATE STI0001 SET RMVDAT = SYSDATE ,RMVNAM = '" + sUserid + "' WHERE LOTNO = '" + sLotId + "' AND USEDAT IS NOT NULL AND RMVDAT IS NULL";
                        int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sSql2, null);
                        if (iResult > 0)
                        {
                            string sqlFX = " SELECT * FROM STI0001 WHERE LOTNO = '" + sLotId + "' AND INDAT IS NOT NULL AND INNAM IS NOT NULL ";
                            DataTable dtFX = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sqlFX, null).Tables[0];
                            if (dtFX != null && dtFX.Rows.Count > 0)
                            {
                                string sqlInsert = @"INSERT INTO STI0003 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,INDAT,INNAM,USENAM,RMVDAT,RMVNAM,DZDAT,DZNAM,FXNAM,FXTIM)  " +
                                    @" VALUES(" +
                                    @"sys_guid(),'" + dtFX.Rows[0]["FAC"].ToString() + "','" + dtFX.Rows[0]["DIV"].ToString() + "','" + dtFX.Rows[0]["LOTNO"].ToString() + "',  " +
                                @" '" + dtFX.Rows[0]["ITNBR"].ToString() + "',  '" + dtFX.Rows[0]["ITDSC"].ToString() + "', to_date('" + dtFX.Rows[0]["INDAT"] + "','yyyy-mm-dd hh24:mi:ss'),  " +
                                @" '" + dtFX.Rows[0]["INNAM"].ToString() + "',  '" + dtFX.Rows[0]["USENAM"].ToString() + "',  to_date('" + dtFX.Rows[0]["RMVDAT"] + "','yyyy-mm-dd hh24:mi:ss'), " +
                                @" '" + dtFX.Rows[0]["RMVNAM"].ToString() + "',  to_date('" + dtFX.Rows[0]["DZDAT"] + "','yyyy-mm-dd hh24:mi:ss'),  '" + dtFX.Rows[0]["DZNAM"].ToString() + "', " +
                                 @" '" + sUserid + "',  SYSDATE ）"; //插入封箱履历
                                int iResult666 = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sqlInsert, null);
                            }

                            return "OK";

                        }

                        else
                        {
                            return "Err-NONE";
                        }
                    }
                }
                else
                {
                    return "Err:0";//批次号不存在
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }


        [HttpGet]
        [Route("api/RecordSteelDingScan")]
        public string RecordSteelDingScan(string sLotId, string sUserid)
        {
           
            try
            {
                string ggsql = "select ITDSC from STB0003 where   LOTID LIKE '%" + sLotId + "%'  ";
                string ggmsql = "select ITDSC from STB0003 where   LOTID LIKE '%" + sLotId + "%' AND ITDSC  LIKE '%钢%'";
                DataTable ggdt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, ggsql, null).Tables[0];
                DataTable ggmdt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, ggmsql, null).Tables[0];
                if (ggdt == null || ggdt.Rows.Count == 0)
                {
                    return "Err:0";//批次号不存在
                }
                //else if (ggdt.Rows[0][0].ToString().Substring(0, 4) != "钢丝帘线")
                else if (ggmdt == null || ggmdt.Rows.Count == 0)
                {
                    return "Err-999";
                }
                string sSql = "SELECT * FROM STB0002 WHERE LOTNO = '" + sLotId + "'";
                DataTable dt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    string sSql1 = "SELECT * FROM STI0001 WHERE LOTNO = '" + sLotId + "' AND INDAT IS NOT NULL AND RMVDAT IS NULL";
                    DataTable dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql1, null).Tables[0];
                    if (dt1.Rows.Count > 0 && dt1 != null)
                        return "Err:1";//批次已扫描且没有撤下

                    string sSqlChk = "SELECT ROUND ( (SYSDATE - DZDAT) * 24) HOURS, DZDAT FROM (SELECT * FROM STI0001 WHERE LOTNO = '" + sLotId + "') ";
                    DataTable dtChk = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlChk, null).Tables[0];
                    if (dtChk != null && dtChk.Rows.Count > 0)
                    {
                        if (int.Parse(dtChk.Rows[0]["HOURS"].ToString()) < 24)
                            return "Err-3";//钢丝锭从出库到现在未达到24小时
                        else
                        {
                            //string sInsScan = "INSERT INTO STI0001 (ID,FAC,DIV,LOTNO,ITNBR,ITDSC,INDAT,INNAM) VALUES (";
                            //sInsScan += "sys_guid(),'07','1','" + sLotId + "','" + dt.Rows[0]["ITNBR"].ToString() + "','" + dt.Rows[0]["ITDSC"].ToString() + "',SYSDATE,'" + sUserid + "')";

                            string sInsScan = "update STI0001 set INDAT =  SYSDATE, INNAM= '" + sUserid + "',USEDAT = NULL,USENAM = NULL,RMVDAT=NULL,RMVNAM=NULL where LOTNO = '" + sLotId + "' ";
                            // sInsScan += "sys_guid(),'07','1','" + sLotId + "','" + dt.Rows[0]["ITNBR"].ToString() + "','" + dt.Rows[0]["ITDSC"].ToString() + "',SYSDATE,'" + sUserid + "')";




                            int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sInsScan, null);
                            if (iResult > 0)
                                return "OK";
                            else
                                return "Err-NONE";
                        }
                    }
                    else
                    {
                        return "Err-2";//没有出库或其他异常
                    }
                }
                else
                {
                    return "Err:0";//批次号不存在
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);





                return null;
            }
        }

        [HttpGet]
        [Route("api/VerifyBackToHuaYang")]
        public string VerifyBackToHuaYang(string sDIV, string sLotid, string BkCart)
        {
            try
            {
               
                string sSqlWIP = string.Empty;
                if (sDIV != "JB")
                {
                    sSqlWIP = "SELECT * FROM WIP0001 WHERE LOTID = '" + sLotid + "' OR TOOLNO='" + BkCart + "' ";
                    DataTable dtWIP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlWIP, null).Tables[0];
                    if (dtWIP.Rows.Count > 0 && dtWIP != null)
                        return "1";
                }
                else
                {
                    sSqlWIP = "SELECT * FROM WIP0004 WHERE LOTID = '" + sLotid + "' OR TOOLNO='" + BkCart + "' ";
                    DataTable dtWIP = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSqlWIP, null).Tables[0];
                    if (dtWIP.Rows.Count > 0 && dtWIP != null)
                        return "1";
                }
                DataTable dt1 = null;
                string sResult = string.Empty;
                string sSql = string.Empty;
                switch (sDIV)
                {
                    case "BD"://钢丝圈
                        sSql = "SELECT * FROM STD0003 WHERE LOTNO = '" + sLotid + "' ORDER BY OUTTIM DESC";
                        dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                        if (dt1.Rows.Count > 0 && dt1 != null)
                            sResult = "OK" + "^" + dt1.Rows[0]["ITNBR"].ToString() + "^" + dt1.Rows[0]["ITDSC"].ToString() + "^" + dt1.Rows[0]["CARTID"].ToString() + "^" + dt1.Rows[0]["QTY"].ToString() + "^" + dt1.Rows[0]["OUTDIV"].ToString();
                        else
                            sResult = "";
                        break;
                    case "FS"://压延
                        sSql = "SELECT * FROM STE0003 WHERE LOTNO = '" + sLotid + "' ORDER BY OUTTIM DESC";
                        dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                        if (dt1.Rows.Count > 0 && dt1 != null)
                            sResult = "OK" + "^" + dt1.Rows[0]["ITNBR"].ToString() + "^" + dt1.Rows[0]["ITDSC"].ToString() + "^" + dt1.Rows[0]["CARTID"].ToString() + "^" + dt1.Rows[0]["QTY"].ToString() + "^" + dt1.Rows[0]["OUTDIV"].ToString();
                        else
                            sResult = "";
                        break;
                    case "JB"://终炼胶
                        sSql = "SELECT * FROM STC0002 WHERE LOTNO = '" + sLotid + "' ORDER BY INDAT DESC";
                        dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                        if (dt1.Rows.Count > 0 && dt1 != null)
                            sResult = "OK" + "^" + dt1.Rows[0]["ITNBR"].ToString() + "^" + dt1.Rows[0]["ITDSC"].ToString() + "^" + dt1.Rows[0]["CARTID"].ToString() + "^" + dt1.Rows[0]["OUTQTY"].ToString() + "^" + dt1.Rows[0]["OUTDIV"].ToString();
                        else
                            sResult = "";
                        break;
                    case "OTH"://其他半成品
                        sSql = "SELECT * FROM STF0002 WHERE LOTNO = '" + sLotid + "' ORDER BY OUTDAT DESC";
                        dt1 = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sSql, null).Tables[0];
                        if (dt1.Rows.Count > 0 && dt1 != null)
                            sResult = "OK" + "^" + dt1.Rows[0]["ITNBR"].ToString() + "^" + dt1.Rows[0]["ITDSC"].ToString() + "^" + dt1.Rows[0]["CARTID"].ToString() + "^" + dt1.Rows[0]["PRODQTY"].ToString() + "^" + dt1.Rows[0]["OUTDIV"].ToString();
                        else
                            sResult = "";
                        break;
                    default:
                        sResult = "";
                        break;
                }
                return sResult;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return ex.ToString();
            }
        }

        #region 仓库管理


        [HttpPost]
        [Route("api/ExecuteFinalGlueToOtherFac")]
        public int ExecuteFinalGlueToOtherFac(string[] param)
        {




            //0 ID
            //1 FAC
            //2 LOT号
            //3 规格代码
            //4 规格名称 
            //5 出库量
            //6 状态
            //7 员工名称
            //8 工装号
            //9 出库区分
            try
            {
               
                string sdivName = string.Empty;
                switch (param[9])
                {
                    case "华盛":
                        sdivName = "05";
                        break;
                    case "华茂":
                        sdivName = "06";
                        break;
                    case "华达":
                        sdivName = "08";
                        break;
                    case "华新":
                        sdivName = "09";
                        break;
                }

                string sqlADD = "INSERT INTO STC0002 (ID,FAC,LOTNO,INDAT,ITNBR,ITDSC,CARTID,CARTSTATE,ITEMSTATE,OUTQTY,UNIT,MCHID,WDATE,WBAN,WSHFT,USERID,OUTDIV) VALUES (";
                sqlADD += "sys_guid(),'" + param[1] + "','" + param[2] + "',SYSDATE,'" + param[3] + "','" + param[4] + "','" + param[8] + "','','" + param[6] + "','" + param[5] + "','','','','','','" + param[7] + "','" + sdivName + "')";
                string sUpdStock = "DELETE FROM WIP0004 WHERE LOTID = '" + param[2] + "'";//删除WIP
                ArrayList sql = new ArrayList();
                sql.Add(sqlADD);
                sql.Add(sUpdStock);
                return OracleDBHelper.TranNonQuery(OracleDBHelper.ConnectionString, sql);
            }
            catch (Exception ex)
            {
                LogHelper.Error("ERROR", ex);
                return 0;
            }
        }



        #endregion


        /// <summary>
        /// 压延卷曲扫描卷轴号
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="unam"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/SFOUTSCAR")]
        public string SFOUTSCAR(string tool, string unam)
        {
           
            try
            {

                string ggsql = "select * from STA0002 where   CARTID =  '" + tool + "' AND  USEYN = 'Y'";
                DataTable ggdt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, ggsql, null).Tables[0];

                if (ggdt == null || ggdt.Rows.Count == 0)
                {
                    return "Err:0";//工装号不存在
                }

                string sql1 = "INSERT INTO LTC0016 (ID,FAC,AUTIM,AUNAM,TOOLNO,IFLAG) VALUES (SYS_GUID(),'07',SYSDATE,'" + unam + "','" + tool + "','N')";
                int iResult = OracleDBHelper.ExecuteCommand(OracleDBHelper.ConnectionString, CommandType.Text, sql1, null);

                if (iResult > 0)
                    return "OK";
                else
                    return "Err-NONE";

            }
            catch (Exception ex)
            {
                LogHelper.Error("错误",ex);
                return null;
            }
        }
    }
}
