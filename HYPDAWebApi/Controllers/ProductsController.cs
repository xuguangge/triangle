using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using HYPDAWebApi.Models;
using HYPDAWebApi.App_Data;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using log4net.Util;
using System.IO;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Threading.Tasks;
using HYPDAWebApi.DBHelper;
using HYPDAWebApi.Models.ViewModel;

namespace HYPDAWebApi.Controllers
{
    public class ProductsController : ApiController
    {
        string NowTime = "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')";
        string FAC = "07";
        MouldController mouldController= new MouldController();
        QualityTestingController qualityTestingController = new QualityTestingController();
        Product[] products = new Product[]
        {
            new Product { Id = 1, Name ="1",Category ="1" },
            new Product { Id = 2, Name ="2", Category ="2" },
            new Product{ Id = 3, Name ="3", Category ="3" }
        };

        ////[HttpPost]
        ///测试链接是否通畅
        [Route("api/GetProduct")]
        public Product[] GetProduct(int id)
        {
            var product = products.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                return null;
            }
            return products;
        }

        /// <summary>
        /// 登录接口 带更新检查
        /// </summary>
        /// <param name="LoginID">工号</param>
        /// <param name="pwd">密码</param>
        /// <param name="version">PDA当前版本</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ReponseData> Login(string LoginID, string pwd, string version,string Mac)
        {
            return await Task.Run(() =>
            {
                String password = Md5GetString(pwd);
                string sql = "SELECT ID,Name, LoginName FROM LSFW_EMPLOYEE WHERE LoginName= '" + LoginID.ToUpper() + "' AND Password= '" + password + "'  AND LEAYN = 'N' ";
                DataTable dtM = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (dtM.Rows.Count > 0)
                {
                    //查询此设备是否启用状态
                    string selectMac = "SELECT * FROM pda_device where MACADDRESS = '"+ Mac + "' and useyn = '启用'";
                    DataTable dtMac = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, selectMac, null).Tables[0];
                    if (dtMac != null && dtMac.Rows.Count > 0)
                    {

                    }
                    else
                    {
                        return ReponseHelper.ReponesInfo("true", "401", "此设备为未启用状态或无此设备记录，请联系大数据处理！", dtMac);
                    }
                  
                    //查询拥有的菜单权限
                    string selec = @"select* from pda_permission where id in (
                           select distinct permission_id from pda_role_permission where role_id in (
                           select  id from PDA_ROLE where id in" +
                        "(select role_id from PDA_USER_ROLE t where user_id = '" + dtM.Rows[0]["ID"].ToString() + "'))) and del_flag = '0'";
                    DataTable perDt = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, selec, null).Tables[0];
                    if (perDt != null && perDt.Rows.Count > 0)
                    {
                        string sele = @"select max(vers) vers from (
                            select * from pda_permission where id in (
                            select distinct permission_id from pda_role_permission where role_id in (
                            select  id from PDA_ROLE where id in" +
                            "(select role_id from PDA_USER_ROLE t where user_id = '" + dtM.Rows[0]["ID"].ToString() + "'))) and del_flag = '0'" +
                            @") a
                            left join （--获取各菜单的最大版本，有可能存在菜单未设置版本的情况
                            select b.id,max(replace(c.version,'.','')) vers from pda_handheld_permission a
                            left join pda_permission b
                            on a.permission_id = b.id
                            left join pda_handheld_update c
                            on a.handheld_id = c.id 
                             where b.id is not null 
                             group by b.id）b
                             on a.id = b.id
                             where vers is not null";
                        DataTable vers = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sele, null).Tables[0];

                        if (vers.Rows[0]["VERS"].ToString() != null && vers.Rows[0]["VERS"].ToString() != "")
                        {
                            int vesbb = 0;
                            try
                            {
                                vesbb = Convert.ToInt32(version.Replace(".", ""));
                            }
                            catch (Exception ex)
                            {
                                return ReponseHelper.ReponesInfo("true", "401", "PDA版本异常！", dtM);
                            }

                            //菜单更新的版本大于当前PDA的版本
                            if (Convert.ToInt32(vers.Rows[0]["VERS"].ToString()) > vesbb)
                            {
                                return ReponseHelper.ReponesInfo("true", "402", "版本更新！", dtM);
                            }
                            else
                            {
                                //全部菜单
                                List<MenuModel> iDCards = DataTableToList<MenuModel>.ConvertToModel(perDt);
                                //菜单组
                                List<MenuModel> cdList = iDCards.FindAll(m => m.PARENT_ID is null).ToList();

                                List<MenuModel> returnList = insertChild(iDCards, cdList);
                                return ReponseHelper.ReponesInfo("true", "200", dtM.Rows[0]["NAME"].ToString(), returnList);

                            }

                        }
                        else
                        {
                            //全部菜单
                            List<MenuModel> iDCards = DataTableToList<MenuModel>.ConvertToModel(perDt);
                            //菜单组
                            List<MenuModel> cdList = iDCards.FindAll(m => m.PARENT_ID is null).ToList();

                            List<MenuModel> returnList = insertChild(iDCards, cdList);
                            return ReponseHelper.ReponesInfo("true", "200", dtM.Rows[0]["NAME"].ToString(), returnList);
                        }
                    }
                    else
                    {
                        return ReponseHelper.ReponesInfo("true", "401", "您未维护PDA操作权限！", dtM);
                    }
                }
                else
                {
                    return ReponseHelper.ReponesInfo("true", "401", "账号密码错误！", dtM);
                }

            });
            //else return null;
        }

        /// <summary>
        /// 获取子菜单
        /// </summary>
        /// <param name="iDCards">全部菜单</param>
        /// <param name="cdList">组菜单</param>
        /// <returns></returns>
        private List<MenuModel> insertChild(List<MenuModel> iDCards, List<MenuModel> cdList)
        {
            for (int i = 0; i < cdList.Count; i++)
            {
                List<MenuModel> ChildList = iDCards.FindAll(m => m.PARENT_ID == cdList[i].ID).ToList();
                if (ChildList.Count > 0)
                {
                    cdList[i].children.AddRange(ChildList);
                }
            }
            return cdList;
        }

        /// <summary>
        /// 角色信息查询
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Route("api/Login")]
        public async Task<User> SelUser(User user)
        {
            return await Task.Run(() =>
            {
                string sql = @"SELECT a.*,(select VERSION from （
                select* from pda_handheld_update order by create_time desc）a where rownum <= 1) VERSION FROM LSFW_EMPLOYEE a 
                WHERE LoginName= '" + user.LoginID.ToUpper() + "' AND LEAYN = 'N' ";
                DataTable uses = OracleDBHelper.GetDataSet(OracleDBHelper.ConnectionString, CommandType.Text, sql, null).Tables[0];
                if (uses != null && uses.Rows.Count > 0)
                {
                    List<User> users = DataTableToList<User>.ConvertToModel(uses);
                    return users[0];
                }
                else return null;
            });
        }




        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="Pwd">密码</param>
        /// <returns>加密过的密码</returns>
        private string Md5GetString(string Pwd)
        {
            MD5 md5s = new MD5CryptoServiceProvider();
            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(Pwd);
            byte[] bufferMd5 = md5s.ComputeHash(buffer);
            StringBuilder sbMd5 = new StringBuilder();
            for (int i = 0; i < bufferMd5.Length; i++)
            {
                sbMd5.Append(bufferMd5[i].ToString("X2"));
            }
            return sbMd5.ToString();
        }



        #region 查询工装班在职人员

        ///// <summary>
        ///// 查询工装班在职人员2
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("api/GetpeopleGZB2")]
        //public DataTable GetpeopleGZB2()
        //{
        //    IDataBase db = new OracleBase();
        //    try
        //    {
        //        return db.GetDataTable(@"SELECT LOGINNAME,NAME
        //                              FROM LSFW_EMPLOYEE
        //                             WHERE POSNAM = '工装更换'
        //                               AND LEAYN = 'N'
        //                             ORDER BY LOGINNAME ASC");
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error("ERROR", ex);
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// 查询工装班在职人员3
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("api/GetpeopleGZB3")]
        //public DataTable GetpeopleGZB3()
        //{
        //    IDataBase db = new OracleBase();
        //    try
        //    {
        //        return db.GetDataTable(@"SELECT LOGINNAME,NAME
        //                              FROM LSFW_EMPLOYEE
        //                             WHERE POSNAM = '工装更换'
        //                               AND LEAYN = 'N'
        //                             ORDER BY LOGINNAME ASC");
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error("ERROR", ex);
        //        return null;
        //    }
        //}


        #endregion



       
        [HttpGet]
        [Route("api/dtNow")]
        public DateTime dtNow()
        {
            return DateTime.Now;
        }

//        [HttpGet]
//        [Route("api/GetSTA0002")]
//        public DataTable GetSTA0002(string[] param)
//        {
//            string sql = "SELECT CARTID,CARTNAM FROM STA0002 WHERE DIV= :DIV ORDER BY CARTID";
//            IDataBase db = new OracleBase();
//            DataTable dtM = db.GetDataTable(sql, new OracleParameter("DIV", param[0]));
//            if (dtM.Rows.Count > 0)
//            {
//                return dtM;
//            }
//            else return null;
//        }

       
   
      

//        [HttpGet]
//        [Route("api/DeleteDate")]
//        public int DeleteDate(string table, string[] ColNam, string[] content)
//        {
//            IDataBase db = new OracleBase();
//            string sql = string.Format("DELECT {0} WHERE 1=1", table);
//            if (ColNam.Length > 0)
//            {
//                for (int i = 1; i < ColNam.Length; i++)
//                {
//                    sql += string.Format("AND {0} = '{1}'", ColNam[i], content[i]);
//                }
//            }
//            int j = 0;
//            j = db.ExecuteNonQuery(sql);
//            return j;
//        }


//        #endregion

//        #region 余量改写（不用）

//        [HttpGet]
//        [Route("api/UpDateQTY")]
//        public int UpDateQTY(string[] str)
//        {
//            //EDA0001 设备
//            //LS_EMPLOYEE 职工
//            //WIP0001 在制品库存
//            //LTC0001 半成品追踪
//            //QMC0101 半成品不良登记

//            //0、RFID写入、不良
//            //1、LOTID
//            //2、剩余量
//            //3、设备号
//            //4、员工号
//            try
//            {
//                IDataBase db = new OracleBase();
//                DataRow ROW = SHIFT.GetShift(DateTime.Now);
//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[4] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);
//                if (dtEMP.Rows.Count == 0)
//                    return 0;
//                string sqlWIP = "SELECT * FROM WIP0001 WHERE LOTID='" + str[1] + "' AND DIV='02' AND FAC='" + FAC + "'";
//                DataTable dtWIP = db.GetDataTable(sqlWIP);
//                if (dtWIP.Rows.Count == 0)
//                    return 0;
//                string sqlEDA = "SELECT * FROM EDA0001 WHERE MCHID='" + str[3] + "' AND FAC='" + FAC + "'";
//                DataTable dtEDA = db.GetDataTable(sqlEDA);
//                if (dtEDA.Rows.Count == 0)
//                    return 0;
//                List<string> sqlList = new List<string>();
//                if (str[0] == "不良")
//                {
//                    string sqlQMC = "SELECT * FROM QMC0101 WHERE LOTID='" + str[1] + "' AND DIV='02'";
//                    DataTable dtQMC = db.GetDataTable(sqlQMC);
//                    if (dtQMC.Rows.Count > 0)
//                    {
//                        string UpdateQMC = @"UPDATE QMC0101 SET AYN='N' , BQTY='" + str[2] + "' , CNAM ='" + dtEMP.Rows[0]["NAME"].ToString() + "' , " +
//                            @" CDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), CTIM=TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), CSHT ='" + ROW["WSHT"].ToString() + "' ," +
//                            @" CBAN='" + ROW["WBAN"].ToString() + "' WHERE LOTID='" + str[1] + "' AND DIV='02'";
//                        sqlList.Add(UpdateQMC);
//                    }
//                    else
//                    {
//                        string InsertQMC = "INSERT INTO QMC0101 (ID,FAC,DIV,AYN,LOTID,ITNBR,ITDSC,BQTY,MCHID,CNAM,CDAT,CTIM,CSHT,CBAN) " +
//                            @" VALUES('" + Guid.NewGuid() + "','" + FAC + "','02','Y','" + str[1] + "','" + dtWIP.Rows[0]["ITNBR"].ToString() + "', " +
//                            @" '" + dtWIP.Rows[0]["ITDSC"].ToString() + "', '" + str[2] + "','" + str[3] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "' , " +
//                            @" to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), '" + ROW["WSHT"].ToString() + "' , '" + ROW["WBAN"].ToString() + "')";
//                        sqlList.Add(InsertQMC);
//                    }
//                    string UpdateLTC = "UPDATE LTC0001 SET BQTY='" + str[2] + "', STYN='2' WHERE LOTID='" + str[1] + "' AND DIV='02'";
//                    sqlList.Add(UpdateLTC);
//                }
//                //余量
//                string UpdateWIP = "UPDATE WIP0001 SET LTHEN='" + Convert.ToInt32(str[2]) + "' WHERE LOTID='" + str[1] + "' AND DIV='02'";
//                sqlList.Add(UpdateWIP);
//                int f = db.TranNonQuery(sqlList.ToArray());
//                if (f > 0)
//                    return 1;
//                else return 0;
//            }
//            catch
//            {
//                return 0;
//            }
//        }

//        [HttpGet]
//        [Route("api/InsIFHalb10")]
//        public int InsIFHalb10(string lotid, string qty, string mchid, string bid, string userid)
//        {
//            IDataBase db = new OracleBase();
//            IDataBase db2 = new OracleBase("");
//            int i = new int();
//            string sql_ltc = "SELECT * FROM LTC0001 WHERE LOTID = '" + lotid + "'";
//            DataTable dt_ltc = db.GetDataTable(sql_ltc);
//            if (dt_ltc.Rows.Count > 0)
//            {
//                string sql = string.Format(@"INSERT INTO IF_WMS_HALB_10(LOTNO,UNLOADDAT,MCHID,ITNBR,ITDSC,WDATE,WTIME,LOTSTATUS,REMQTY,UNIT,ITGRPCOD3,
//                                                  CARTID,CARTSTATE,USERID,RCV_FLAG,RCV_DT,ENT_USER_ID,ENT_DT,UPD_USER_ID,UPD_DT)
//                                            VALUES('{0}',{1},'{2}','{3}','{4}',{5},{6},'{7}','{8}','{9}','{10}','{11}',
//                                                    '{12}','{13}','{14}','{15}','{16}',{17},'{18}',{19})",
//                                                lotid, "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')", mchid,
//                                                dt_ltc.Rows[0]["AITNBR"].ToString(), dt_ltc.Rows[0]["AITDSC"].ToString(),
//                                                "TO_DATE('" + dt_ltc.Rows[0]["AUDAT"].ToString() + "','yyyy-MM-dd hh24:mi:ss')",
//                                                "TO_DATE('" + dt_ltc.Rows[0]["AUTIM"].ToString() + "','yyyy-MM-dd hh24:mi:ss')",
//                                                dt_ltc.Rows[0]["STYN"].ToString(), qty, null, dt_ltc.Rows[0]["DIV"].ToString(), bid, null,
//                                                dt_ltc.Rows[0]["AUNAM"].ToString(), null, null, userid,
//                                                "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')", userid,
//                                                "TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss')");
//                i = db2.ExecuteNonQuery(sql);
//            }

//            return i;
//        }

//        #endregion

//        #region 暂时不用

//        [HttpGet]
//        [Route("api/GetVersion")]
//        public string GetVersion()
//        {
//            if (!string.IsNullOrEmpty(File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Version.txt"))))
//            {
//                string ver = File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Version.txt"));
//                return ver;
//            }
//            else
//                return "";
//        }

//        #region PDA更新包下载 (暂时不用)

//        const string GS_SN = "YIDA20130925@SH";
//        //private static NLog.Logger logger = NLog.LogManager.GetLogger("AppUpdate");

//        /// <summary>
//        /// 程序更新,首先检查，如果需要更新，则下载文件..
//        /// </summary>
//        [HttpGet]
//        [Route("api/UpdateApp")]
//        public byte[] UpdateApp(string as_sn, string ClientVer, ref string sErr)
//        {
//            if (GS_SN != as_sn)
//            {
//                return null;
//            }
//            byte[] bsZIP = null;
//            try
//            {
//                string newVerStr = null;
//                string fileName = CheckNewAppVersion(out newVerStr);
//                if (String.IsNullOrEmpty(fileName))
//                {
//                    sErr = "当前程序已经是最新版本，不需要更新！";
//                    return null;
//                }
//                Version oldVer = new Version(ClientVer);
//                Version newVer = new Version(newVerStr);
//                if (oldVer.CompareTo(newVer) >= 0)
//                {
//                    sErr = "当前程序已经是最新版本，不需要更新！";
//                    return null;
//                }
//                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
//                {
//                    using (BinaryReader reader = new BinaryReader(fs))
//                    {
//                        using (MemoryStream ms = new MemoryStream())
//                        {
//                            byte[] buffer = new byte[1024];
//                            int count = 0;
//                            while ((count = reader.Read(buffer, 0, 1024)) > 0)
//                            {
//                                ms.Write(buffer, 0, count);
//                            }
//                            // 压缩返回的文件
//                            byte[] bs = ms.ToArray();
//                            bsZIP = ConvertZip1.ConvertZip.Compress(bs);
//                            ms.Close();
//                        }
//                        reader.Close();
//                    }
//                    fs.Close();
//                }
//                //File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Version.txt"))
//                if (!string.IsNullOrEmpty(newVer.ToString()))
//                {
//                    File.WriteAllText(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Version.txt"), newVer.ToString());
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//            return bsZIP;
//        }

//        [HttpGet]
//        [Route("api/CheckAppVersion")]
//        public string CheckAppVersion(string as_sn, string oldVersion)
//        {
//            if (GS_SN != as_sn)
//            {
//                return "";
//            }
//            string newVerStr = null;
//            string fileName = CheckNewAppVersion(out newVerStr);
//            if (String.IsNullOrEmpty(fileName))
//            {
//                LogHelper.Debug( "CheckAppVersion:无需更新");
//                return "";
//            }
//            Version oldVer = new Version(oldVersion);
//            Version newVer = new Version(newVerStr);
//            if (oldVer.CompareTo(newVer) >= 0)
//            {
//                LogHelper.Debug( "当前程序已经是最新版本,不需要更新！oldVersion=" + oldVersion + ";newVerStr=" + newVerStr);
//                return "";
//            }
//            return newVerStr; // 需要更新到的新版本号
//        }

//        /// <summary>
//        /// 检查服务器端是否有新更新包_
//        /// </summary>
//        [HttpGet]
//        [Route("api/CheckNewAppVersion")]
//        public string CheckNewAppVersion(out string newVersion)
//        {
//            newVersion = "1.0";
//            string patchDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update");
//            string[] patchFile = null;
//            if (Directory.Exists(patchDir))
//            {
//                patchFile = Directory.GetFiles(patchDir, "Patch_*.CAB");
//            }

//            if (patchFile == null || patchFile.Length == 0)
//            {
//                return null;
//            }

//            // Get the last one
//            string fileName = patchFile[0];
//            FileInfo fi0 = new FileInfo(patchFile[0]);
//            FileInfo fii = null;
//            for (int i = 1; i < patchFile.Length; i++)
//            {
//                fii = new FileInfo(patchFile[i]);
//                if (fi0.CreationTime < fii.CreationTime)
//                {
//                    fileName = patchFile[i];
//                    fi0 = fii;
//                }
//            }
//            newVersion = Path.GetFileNameWithoutExtension(fileName);
//            newVersion = newVersion.Replace("Patch_", "");
//            return fileName;
//        }

//        #endregion

//        #endregion

//        #region 成品出库


 
      
//        

      
//        [HttpGet]
//        [Route("api/CheckCanXAll")]
//        public bool CheckCanXAll(string sBill)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                bool bCanXAll = true;
//                string sSql = "SELECT * FROM SDA0002 WHERE BILLNO = '" + sBill + "'";
//                DataTable dt = db.GetDataTable(sSql);
//                if (dt != null && dt.Rows.Count > 0)
//                {
//                    if (dt.Rows[0]["STATE"].ToString() == "6")//若工单已出库
//                        bCanXAll = false;
//                }
//                else
//                {
//                    bCanXAll = false;
//                }
//                return bCanXAll;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return false;
//            }
//        }

      
      
      
        

        

      
//        #endregion

//        #region 硫化扫码

        
//        [HttpGet]
//        [Route("api/GetVulMchid")]
//        public DataTable GetVulMchid()
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string strSql = "SELECT * FROM EDA0001 WHERE SUBSTR(MCHID,3,1)='D' AND MCHTCOD='D02' ORDER BY MCHID ";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        #region 转移


//        //(PAD0401或STX0004数据和LTA0001数据不一致)
//        //0处理异常、1转移成功、2转移失败
//        [HttpGet]
//        [Route("api/GetMoveContinue_1")]
//        public int GetMoveContinue_1(string[] str)
//        {
//            //0、BARCODE
//            //1、机台
//            //2、左右区分
//            //3、目标机台
//            //4、目标区分
//            //5、登录人工号
//            try
//            {
//                IDataBase db = new OracleBase();
//                //
//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[5] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);

//                //
//                string strSql = "SELECT * FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
//                DataTable dtSTX0002 = db.GetDataTable(strSql);
//                string wrginflag = dtSTX0002.Rows[0]["WRGINFLAG"].ToString();

//                //
//                string gtitnbr = "";
//                string gtitdsc = "";
//                string itnbr = "";
//                string itdsc = "";
//                string strPAD0401 = @"SELECT * FROM PAD0401 WHERE DIV='1' AND MCHID='" + str[1] + "' AND LR='" + str[2] + "'";
//                DataTable dtPAD0401 = db.GetDataTable(strPAD0401);
//                if (dtPAD0401.Rows.Count == 0)
//                {
//                    //string strSTX0004 = @"SELECT ID,FAC,MCHID,LR,CUITNBR,'' AS CUITDSC,BUITNBR,'' AS BUITDSC,WTIM FROM STX0004 WHERE MCHID='" + str[1] + "' AND LR='" + str[2] + "' ORDER BY WTIM DESC";
//                    //上面=狗屎
//                    string strSTX = "SELECT BUITNBR,BOMID CUITNBR,'' AS CUITDSC,'' AS BUITDSC FROM LTA0001 A,EDC0003 B WHERE A.BUITNBR = B.ITNBR AND BARCODE  ='" + str[0] + "' AND B.USEYN = 'Y' AND B.LEVL = '1'";
//                    DataTable dtSTX = db.GetDataTable(strSTX);
//                    if (dtSTX != null && dtSTX.Rows.Count > 0)
//                    {
//                        string strEDB0010 = @"SELECT *
//                                                          FROM (SELECT A.*,
//                                                                       ROW_NUMBER ()
//                                                                          OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC)
//                                                                          RN
//                                                                  FROM EDB0010 A)
//                                                         WHERE RN = 1";
//                        DataTable dtEDB0010 = db.GetDataTable(strEDB0010);
//                        if (dtEDB0010 != null && dtEDB0010.Rows.Count > 0)
//                        {
//                            //胎胚
//                            DataRow[] Brow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["BUITNBR"] + "'");
//                            if (Brow.Length > 0)
//                                dtSTX.Rows[0]["BUITDSC"] = Brow[0]["ITDSC"].ToString();
//                            //硫化
//                            DataRow[] Crow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["CUITNBR"] + "'");
//                            if (Crow.Length > 0)
//                                dtSTX.Rows[0]["CUITDSC"] = Crow[0]["ITDSC"].ToString();
//                        }
//                        gtitnbr = dtSTX.Rows[0]["BUITNBR"].ToString();
//                        gtitdsc = dtSTX.Rows[0]["BUITDSC"].ToString();
//                        itnbr = dtSTX.Rows[0]["CUITNBR"].ToString();
//                        itdsc = dtSTX.Rows[0]["CUITDSC"].ToString();
//                    }
//                }
//                else
//                {
//                    gtitnbr = dtPAD0401.Rows[0]["BUITNBR"].ToString();
//                    gtitdsc = dtPAD0401.Rows[0]["BUITDSC"].ToString();
//                    itnbr = dtPAD0401.Rows[0]["CUITNBR"].ToString();
//                    itdsc = dtPAD0401.Rows[0]["CUITDSC"].ToString();
//                }
//                //
//                string strDelete = "DELETE FROM STX0002 WHERE BARCODE='" + str[0] + "' AND MCHID='" + str[1] + "' AND LRFLAG='" + str[2] + "' ";
//                //
//                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
//                                   @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[3] + "','" + str[4] + "','" + str[0] + "', " +
//                                   @" '" + gtitnbr + "','" + itnbr + "','" + wrginflag + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                                   @" '2')";
//                //
//                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
//                    @"EVNDAT, " +
//                    @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
//                    @"ENT_USER_ID,ENT_DT, " +
//                    @"UPD_USER_ID,UPD_DT, " +
//                    @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
//                    @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
//                    @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                    @"'" + str[3] + "','" + str[4] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
//                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                    @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                    @"'N','2' )";
//                //
//                string desc = "由" + str[1] + "-" + str[2] + "转移到" + str[3] + "-" + str[4];
//                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC) VALUES( " +
//                                @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[3] + "', '" + str[4] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
//                                @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'1','" + desc + "')";
//                //
//                List<string> list = new List<string>();
//                list.Add(strDelete);
//                list.Add(strInsert);
//                list.Add(strSTG0005);
//                list.Add(sql_Up);
//                int j = db.TranNonQuery(list.ToArray());
//                if (j > 0)
//                    return 1;
//                else
//                    return 2;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return 0;
//            }
//        }

   

//        #endregion

//        #region 新增



//        //(PAD0401或STX0004数据和LTA0001数据不一致)
//        //0处理异常、1新增成功、2新增失败
//        [HttpGet]
//        [Route("api/GetNewContinue_1")]
//        public int GetNewContinue_1(string[] str)
//        {
//            //0、BARCODE
//            //1、机台
//            //2、左右区分
//            //3、登录人工号
//            //4、新增原因
//            try
//            {
//                IDataBase db = new OracleBase();

//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[3] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);

//                //
//                string gtitnbr = "";
//                string gtitdsc = "";
//                string itnbr = "";
//                string itdsc = "";
//                string strPAD0401 = @"SELECT * FROM PAD0401 WHERE DIV='1' AND MCHID='" + str[1] + "' AND LR='" + str[2] + "'";
//                DataTable dtPAD0401 = db.GetDataTable(strPAD0401);
//                if (dtPAD0401.Rows.Count == 0)
//                {
//                    //string strSTX0004 = @"SELECT ID,FAC,MCHID,LR,CUITNBR,'' AS CUITDSC,BUITNBR,'' AS BUITDSC,WTIM FROM STX0004 WHERE MCHID='" + str[1] + "' AND LR='" + str[2] + "' ORDER BY WTIM DESC";
//                    string strSTX = "SELECT BUITNBR,BOMID CUITNBR,'' AS CUITDSC,'' AS BUITDSC FROM LTA0001 A,EDC0003 B WHERE A.BUITNBR = B.ITNBR AND BARCODE  ='" + str[0] + "' AND B.USEYN = 'Y' AND B.LEVL = '1'";
//                    DataTable dtSTX = db.GetDataTable(strSTX);
//                    if (dtSTX != null && dtSTX.Rows.Count > 0)
//                    {
//                        string strEDB0010 = @"SELECT *
//                                                      FROM (SELECT A.*,
//                                                                   ROW_NUMBER ()
//                                                                      OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC)
//                                                                      RN
//                                                              FROM EDB0010 A)
//                                                     WHERE RN = 1";
//                        DataTable dtEDB0010 = db.GetDataTable(strEDB0010);
//                        if (dtEDB0010 != null && dtEDB0010.Rows.Count > 0)
//                        {
//                            //胎胚
//                            DataRow[] Brow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["BUITNBR"] + "'");
//                            if (Brow.Length > 0)
//                                dtSTX.Rows[0]["BUITDSC"] = Brow[0]["ITDSC"].ToString();
//                            //硫化
//                            DataRow[] Crow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["CUITNBR"] + "'");
//                            if (Crow.Length > 0)
//                                dtSTX.Rows[0]["CUITDSC"] = Crow[0]["ITDSC"].ToString();
//                        }
//                        gtitnbr = dtSTX.Rows[0]["BUITNBR"].ToString();
//                        gtitdsc = dtSTX.Rows[0]["BUITDSC"].ToString();
//                        itnbr = dtSTX.Rows[0]["CUITNBR"].ToString();
//                        itdsc = dtSTX.Rows[0]["CUITDSC"].ToString();
//                    }
//                }
//                else
//                {
//                    gtitnbr = dtPAD0401.Rows[0]["BUITNBR"].ToString();
//                    gtitdsc = dtPAD0401.Rows[0]["BUITDSC"].ToString();
//                    itnbr = dtPAD0401.Rows[0]["CUITNBR"].ToString();
//                    itdsc = dtPAD0401.Rows[0]["CUITDSC"].ToString();
//                }
//                //
//                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
//                                       @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[1] + "','" + str[2] + "','" + str[0] + "', " +
//                                       @" '" + gtitnbr + "','" + itnbr + "','0',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                                       @" '2' )";
//                //
//                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
//                       @"EVNDAT, " +
//                       @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
//                       @"ENT_USER_ID,ENT_DT, " +
//                       @"UPD_USER_ID,UPD_DT, " +
//                       @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
//                       @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
//                       @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'" + str[1] + "','" + str[2] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
//                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'N','2' )";
//                //
//                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC,REMARK) VALUES( " +
//                                     @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[1] + "', '" + str[2] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
//                                     @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'2','新增','" + str[4] + "')";
//                //
//                List<string> list = new List<string>();
//                list.Add(strInsert);
//                list.Add(strSTG0005);
//                list.Add(sql_Up);
//                int j = db.TranNonQuery(list.ToArray());
//                if (j > 0)
//                    return 1;
//                else
//                    return 2;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return 0;
//            }
//        }

//        //返回值：0员工不存在、 1新增成功、 2成型规格不一致、 3当前机台区分没有正在生产的计划、 
//        //       4生产追溯没有此条码信息、 5新增失败、 6处理异常 7、此条码没有硫化规格信息 
//        //       8、条码在胎胚不良已存在、81报废、9条码在胎胚保留未处理
//        [HttpGet]
//        [Route("api/GetNewContinue_2")]
//        public string GetNewContinue_2(string[] str)
//        {
//            //0、BARCODE
//            //1、机台
//            //2、左右区分
//            //3、登录人工号
//            try
//            {
//                IDataBase db = new OracleBase();

//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[3] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);
//                //
//                string gtitnbr = "";
//                string gtitdsc = "";
//                string itnbr = "";
//                string itdsc = "";
//                string strPAD0401 = @"SELECT * FROM PAD0401 WHERE DIV='1' AND MCHID='" + str[1] + "' AND LR='" + str[2] + "'";
//                DataTable dtPAD0401 = db.GetDataTable(strPAD0401);
//                if (dtPAD0401.Rows.Count == 0)
//                {
//                    string strSTX = "SELECT BUITNBR,BOMID CUITNBR,'' AS CUITDSC,'' AS BUITDSC FROM LTA0001 A,EDC0003 B WHERE A.BUITNBR = B.ITNBR AND BARCODE  ='" + str[0] + "' AND B.USEYN = 'Y' AND B.LEVL = '1'";
//                    DataTable dtSTX = db.GetDataTable(strSTX);
//                    if (dtSTX != null && dtSTX.Rows.Count > 0)
//                    {
//                        string strEDB0010 = @"SELECT *
//                                                          FROM (SELECT A.*,
//                                                                       ROW_NUMBER ()
//                                                                          OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC)
//                                                                          RN
//                                                                  FROM EDB0010 A)
//                                                         WHERE RN = 1";
//                        DataTable dtEDB0010 = db.GetDataTable(strEDB0010);
//                        if (dtEDB0010 != null && dtEDB0010.Rows.Count > 0)
//                        {
//                            //胎胚
//                            DataRow[] Brow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["BUITNBR"] + "'");
//                            if (Brow.Length > 0)
//                                dtSTX.Rows[0]["BUITDSC"] = Brow[0]["ITDSC"].ToString();
//                            //硫化
//                            DataRow[] Crow = dtEDB0010.Select("ITNBR='" + dtSTX.Rows[0]["CUITNBR"] + "'");
//                            if (Crow.Length > 0)
//                                dtSTX.Rows[0]["CUITDSC"] = Crow[0]["ITDSC"].ToString();
//                        }
//                        gtitnbr = dtSTX.Rows[0]["BUITNBR"].ToString();
//                        gtitdsc = dtSTX.Rows[0]["BUITDSC"].ToString();
//                        itnbr = dtSTX.Rows[0]["CUITNBR"].ToString();
//                        itdsc = dtSTX.Rows[0]["CUITDSC"].ToString();
//                    }
//                    else
//                        return "7";
//                }
//                else
//                {
//                    gtitnbr = dtPAD0401.Rows[0]["BUITNBR"].ToString();
//                    gtitdsc = dtPAD0401.Rows[0]["BUITDSC"].ToString();
//                    itnbr = dtPAD0401.Rows[0]["CUITNBR"].ToString();
//                    itdsc = dtPAD0401.Rows[0]["CUITDSC"].ToString();
//                }
//                //
//                string strLTA0001 = @" SELECT * FROM LTA0001 " +
//                                @" WHERE BARCODE='" + str[0] + "' ";
//                DataTable dtLTA0001 = db.GetDataTable(strLTA0001);
//                if (dtLTA0001.Rows.Count == 0)
//                    return "4";
//                string gtitnbrL = dtLTA0001.Rows[0]["BUITNBR"].ToString();
//                if (gtitnbr != gtitnbrL)
//                    return "Err^2^" + gtitnbr + "^" + gtitnbrL;
//                //
//                string strInsert = @"INSERT INTO STX0002(ID,FAC,MCHID,LRFLAG,BARCODE,GTITNBR,ITNBR,WRGINFLAG,CTIM,MOVDIV) VALUES ( " +
//                                       @" '" + Guid.NewGuid() + "','" + FAC + "','" + str[1] + "','" + str[2] + "','" + str[0] + "', " +
//                                       @" '" + gtitnbr + "','" + itnbr + "','0',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                                       @" '2' )";
//                //
//                string strSTG0005 = @"INSERT INTO STG0005(ID,FAC,BARCODE, " +
//                       @"EVNDAT, " +
//                       @"VCMCHID,LRFLAG,ITNBR,ITDSC,GTITNBR,GTITDSC,GIPFLAG, " +
//                       @"ENT_USER_ID,ENT_DT, " +
//                       @"UPD_USER_ID,UPD_DT, " +
//                       @"SEND_ROUTEDATA_FLAG,MOVDIV) VALUES ( " +
//                       @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
//                       @"TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'" + str[1] + "','" + str[2] + "','" + itnbr + "','" + itdsc + "','" + gtitnbr + "','" + gtitdsc + "','Y', " +
//                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'" + dtEMP.Rows[0]["NAME"].ToString() + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'N','2' )";
//                //
//                string sql_Up = @"INSERT INTO STX0001(ID,FAC,MCHID,LRFLAG,BARCODE,EID,EDATE,OPDIV,OPDESC) VALUES( " +
//                                     @" '" + Guid.NewGuid() + "', '" + FAC + "', '" + str[1] + "', '" + str[2] + "', '" + str[0] + "', '" + dtEMP.Rows[0]["NAME"].ToString() + "'," +
//                                     @" TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'2','新增')";
//                //
//                List<string> list = new List<string>();
//                list.Add(strInsert);
//                list.Add(strSTG0005);
//                list.Add(sql_Up);
//                int j = db.TranNonQuery(list.ToArray());
//                if (j > 0)
//                    return "1";
//                else
//                    return "5";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "6";
//            }
//        }

//        #endregion

       
   

    

//        #region 停机


//        //停机、开机
//        [HttpGet]
//        [Route("api/GetNOCOD")]
//        public DataTable GetNOCOD(string cod)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string strSql = @"SELECT A.NOSTRTCOD,A.NOSTRTNAM,A.SUBTNAM,A.SUBTCOD FROM EQA0003 A LEFT JOIN EQA0004 B ON A.NOSTRTCOD=B.NOSTRTCOD 
//                          WHERE B.FAC='" + FAC + "' AND A.USEYN='Y' AND B.PROCCOD='D2' AND A.SUBTCOD='" + cod + "'";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }


//        //停机、开机
//        [HttpGet]
//        [Route("api/GetSUBNOCOD")]
//        public DataTable GetSUBNOCOD()
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string strSql = @"SELECT A.SUBTNAM,A.SUBTCOD FROM EQA0003 A LEFT JOIN EQA0004 B ON A.NOSTRTCOD=B.NOSTRTCOD 
//                          WHERE B.FAC='" + FAC + "' AND A.USEYN='Y' AND B.PROCCOD='D2' GROUP BY A.SUBTNAM,A.SUBTCOD";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }
//        //返回值：0员工不存在、 1停机成功、 2不启动代码不存在、 3停机失败、 4处理异常
//        [HttpGet]
//        [Route("api/GetVulStop")]
//        public int GetVulStop(string[] str)
//        {
//            //0、机台
//            //1、左右区分
//            //2、停机代码
//            //3、停机原因
//            //4、停机人工号
//            //5、停机人名称
//            try
//            {
//                IDataBase db = new OracleBase();
//                DataRow row = SHIFT.GetShift(DateTime.Now);
//                string sITNBR = string.Empty;
//                string sITDSC = string.Empty;

//                //string sSqlSel = "SELECT CUITNBR,CUITDSC FROM PAD0401 WHERE DIV = '1'  AND WDATE =TO_DATE('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','YYYY-MM-DD') AND MCHID = '" + str[0] + "' AND LR=  '" + str[1] + "'";
//                string sSqlSel = "SELECT ITNBR,ITDSC FROM PAG0001 WHERE  MCHID = '" + str[0] + str[1] + "'";
//                DataTable dtSel = db.GetDataTable(sSqlSel);
//                if (dtSel.Rows.Count > 0 && dtSel != null)
//                {
//                    sITNBR = dtSel.Rows[0]["ITNBR"].ToString();
//                    sITDSC = dtSel.Rows[0]["ITDSC"].ToString();
//                }

//                //判断员工是否存在
//                string sqllogin = "SELECT * FROM LSFW_EMPLOYEE WHERE  FAC='" + FAC + "' AND LOGINNAME= '" + str[4] + "' AND NAME='" + str[5] + "' ";
//                DataTable dtemp = db.GetDataTable(sqllogin);
//                if (dtemp.Rows.Count == 0)
//                    return 0;

//                //判断不启动代码是否正确
//                string sqlpsd = "SELECT * FROM EQA0003 WHERE FAC='" + FAC + "' AND NOSTRTCOD='" + str[2] + "' AND NOSTRTNAM='" + str[3] + "'";
//                DataTable dteda = db.GetDataTable(sqlpsd);
//                if (dteda.Rows.Count == 0)
//                    return 2;

//                //更新防止MES系统停机未解除，再次申请停机的
//                string sqlstr = @"UPDATE EQB0003 SET 
//			                        RSTRDATE=TO_DATE('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                                    RSTRTTIM=TO_DATE('" + DateTime.Now.ToString() + @"','yyyy-MM-dd hh24:mi:ss'),
//			                        RSTRTNAM='" + str[5] + @"'
//                             WHERE FAC='" + FAC + "' AND MCHID='" + str[0] + "' AND LRFLAG='" + str[1] + "' AND NOSTRTCOD= '" + str[2] + "' AND (RSTRTNAM IS NULL OR RSTRTNAM ='')";
//                int r = db.ExecuteNonQuery(sqlstr);

//                //插入不启动履历
//                string sqlInsert = @"INSERT INTO EQB0003(ID,FAC,WDATE,WSHT,WBAN,MCHID,LRFLAG,NOSTRTCOD,NOSTRTRSN,NOSTRTNAM,NOSTRTTIM,RSTRDATE,RSTRTTIM,RSTRTNAM,REMARK,ITNBR,ITDSC)
//            VALUES
//            ('" + Guid.NewGuid() + "','" + FAC + "',TO_DATE('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),'" + row["WSHT"].ToString() + "','" + row["WBAN"].ToString() + "'," +
//                @" '" + str[0] + "','" + str[1] + "','" + str[2] + "','" + str[3] + "'," +
//                @" '" + str[5] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'),'','','','','" + sITNBR + "','" + sITDSC + "')";

//                //更新设备运行状态
//                string sqlEquip = "UPDATE EDA0007 SET EQPSTATE='" + dteda.Rows[0]["SUBTCOD"].ToString() + "',ENAM='" + str[5] + "',ETIM=TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss') WHERE FAC='" + FAC + "' AND MCHID='" + str[0] + "' AND LRFLAG='" + str[1] + "'";
//                List<string> list = new List<string>();
//                //list.Add(sqlstr);
//                list.Add(sqlInsert);
//                list.Add(sqlEquip);
//                int i = db.TranNonQuery(list.ToArray());
//                if (i > 0)
//                    return 1;
//                else
//                    return 3;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return 4;
//            }
//        }

     
  
//        [HttpGet]
//        [Route("api/GetIFStop")]
//        public string GetIFStop(string[] str)
//        {
//            //0机台号
//            //1区分

//            IDataBase db = new OracleBase();
//            try
//            {
//                string strSql = @"SELECT * FROM EQB0003 WHERE MCHID='" + str[0] + "' AND LRFLAG='" + str[1] + "' AND (RSTRTNAM IS NULL OR RSTRTNAM ='')";
//                DataTable dt = db.GetDataTable(strSql);
//                if (dt.Rows.Count > 0) //正在停机
//                    return dt.Rows[0]["NOSTRTRSN"] + "&" + dt.Rows[0]["NOSTRTTIM"];
//                else
//                    return "";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "";
//            }
//        }

//        #endregion



      


//        [HttpGet]
//        [Route("api/GetXXXXByBarcode")]
//        public string GetXXXXByBarcode(string sBarcode)
//        {
//            IDataBase db = new OracleBase();
//            string sResult = string.Empty;
//            string wgjg = string.Empty;
//            string SDSjg = string.Empty;
//            string Xgjg = string.Empty;
//            string UFjg = string.Empty;
//            string DBjg = string.Empty;

//            try
//            {
//                string sql = @" 
//SELECT
//       CASE
//          WHEN     VI.WGRES IS NULL
//               AND CUITNBR IS NOT NULL
//               AND VIBTN.VIBTNRES = '合格'
//          THEN
//             '合格'
//          WHEN VI.WGRES = 'A'
//          THEN
//             '合格'
//          WHEN VI.WGRES = 'B'
//          THEN
//             '不良'
//          WHEN VI.WGRES = 'C'
//          THEN
//             '报废'
//       END
//          外观检查结果,
//       UF.UFGRADE AS UF检查结果,
//       DB.OVERALLGRADE AS DB检查结果,
//       XR.GR AS X光检查结果,
//       SDS.SDSRES AS SDS检查结果,
//       DECODE (CUGRADE,
//               '604', '华阳一级',
//               '605', '华阳二级',
//               '606', '华阳')
//          AS 判级结果,
//       DECODE (INSTO.DIV,
//               '0', '正品入库',
//               '1', '废品入库',
//               '2', '实验胎(出)',
//               '3', '例查胎(出)')
//          入库结果
//  FROM LTA0001 A
//       LEFT JOIN LSFW_EMPLOYEE B ON A.CUNAM = B.LOGINNAME
//       LEFT JOIN
//       (SELECT *
//          FROM (SELECT PN,
//                       CASE
//                          WHEN GR = '01' THEN '合格'
//                          WHEN GR = '03' THEN '不合格'
//                          WHEN GR = '07' THEN '未知状态'
//                       END
//                          GR,
//                       WTIME,
//                       ROW_NUMBER ()
//                          OVER (PARTITION BY PN ORDER BY WTIME DESC)
//                          RN
//                  FROM QMX0001
//                 WHERE PN = '" + sBarcode + @"')
//         WHERE RN = '1') XR
//          ON A.BARCODE = XR.PN
//       LEFT JOIN
//       (SELECT *
//          FROM (SELECT BARCODE,
//                       CASE
//                          WHEN VIRESULT = '1' THEN '合格'
//                          WHEN VIRESULT = '2' THEN '报废'
//                          WHEN VIRESULT = '3' THEN '人工打磨'
//                          WHEN VIRESULT = '4' THEN '外观终审'
//                          WHEN VIRESULT = '5' THEN '条码读取异常'
//                       END
//                          VIBTNRES,
//                       VITIM,
//                       ROW_NUMBER ()
//                          OVER (PARTITION BY BARCODE ORDER BY VITIM DESC)
//                          RN
//                  FROM CKA0007
//                 WHERE BARCODE = '" + sBarcode + @"')
//         WHERE RN = '1') VIBTN
//          ON A.BARCODE = VIBTN.BARCODE
//       LEFT JOIN (SELECT BARCODE, AYN WGRES
//                    FROM QMA0101
//                   WHERE BARCODE = '" + sBarcode + @"' AND DIV = '1') VI
//          ON A.BARCODE = VI.BARCODE
//       LEFT JOIN
//       (SELECT *
//          FROM (SELECT BARCODE,
//                       CASE
//                          WHEN SDSRES = 'A' THEN '合格'
//                          WHEN SDSRES = 'C' THEN '报废'
//                       END
//                          SDSRES,
//                       WTIME,
//                       ROW_NUMBER ()
//                          OVER (PARTITION BY BARCODE ORDER BY WTIME DESC)
//                          RN
//                  FROM QMA0002
//                 WHERE BARCODE = '" + sBarcode + @"')
//         WHERE RN = '1') SDS
//          ON A.BARCODE = SDS.BARCODE
//       LEFT JOIN
//       (SELECT *
//          FROM (SELECT BARCODE,
//                       UFGRADE,
//                       WTIME,
//                       ROW_NUMBER ()
//                          OVER (PARTITION BY BARCODE ORDER BY WTIME DESC)
//                          RN
//                  FROM QMA0201
//                 WHERE BARCODE = '" + sBarcode + @"')
//         WHERE RN = '1') UF
//          ON A.BARCODE = UF.BARCODE
//       LEFT JOIN
//       (SELECT *
//          FROM (SELECT BARCODE,
//                       OVERALLGRADE,
//                       WTIME,
//                       ROW_NUMBER ()
//                          OVER (PARTITION BY BARCODE ORDER BY WTIME DESC)
//                          RN
//                  FROM QMA0301
//                 WHERE BARCODE = '" + sBarcode + @"')
//         WHERE RN = '1') DB
//          ON A.BARCODE = DB.BARCODE
//       LEFT JOIN (SELECT COUNT (*) UFCNT
//                    FROM QMA0201
//                   WHERE BARCODE = '" + sBarcode + @"') UFCNT
//          ON 1 = 1
//       LEFT JOIN (SELECT COUNT (*) DBCNT
//                    FROM QMA0301
//                   WHERE BARCODE = '" + sBarcode + @"') DBCNT
//          ON 1 = 1
//       LEFT JOIN
//       (SELECT *
//          FROM (SELECT BARCODE,
//                       DIV,
//                       ROW_NUMBER ()
//                          OVER (PARTITION BY BARCODE ORDER BY OUTDATE DESC)
//                          RN
//                  FROM SDA0001
//                 WHERE BARCODE = '" + sBarcode + @"' AND XYN = '0')
//         WHERE RN = '1') INSTO
//          ON A.BARCODE = INSTO.BARCODE
// WHERE A.BARCODE = '" + sBarcode + "' ";

//                DataTable dt = db.GetDataTable(sql);

//                if (dt.Rows.Count > 0)
//                {

//                    wgjg = dt.Rows[0]["外观检查结果"].ToString();
//                    SDSjg = dt.Rows[0]["SDS检查结果"].ToString();
//                    Xgjg = dt.Rows[0]["X光检查结果"].ToString();
//                    UFjg = dt.Rows[0]["UF检查结果"].ToString();
//                    DBjg = dt.Rows[0]["DB检查结果"].ToString();
//                    sResult = wgjg + "^" + SDSjg + "^" + Xgjg + "^" + UFjg + "^" + DBjg;
//                    return sResult;
//                }
//                else
//                    return "";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "Err-" + ex.Message;
//            }
//        }


     
  
//        #endregion

//        /// <summary>
//        /// 取得服务器时间
//        /// </summary>
//        /// <returns>服务器时间</returns>
//        [HttpGet]
//        [Route("api/GetSystemDateTime")]
//        public DateTime GetSystemDateTime()
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                DataTable dt = db.GetDataTable("select getdate() as [DateTime]");
//                if (dt != null)
//                {
//                    return (DateTime)dt.Rows[0][0];
//                }
//                return DateTime.MinValue;
//            }
//            catch (Exception ex)
//            {

//                LogHelper.Error("ERROR", ex);
//                return DateTime.Now;
//            }

//        }


    


//        #region 硫化成品胎盘点
     

     
      
//        [HttpGet]
//        [Route("api/CheckCureTakStkDupOrNot")]
//        public bool CheckCureTakStkDupOrNot(string sBarcode)
//        {
//            IDataBase db = new OracleBase();
//            bool bDup = false;
//            try
//            {
//                string strSql = "SELECT * FROM SJE0010 WHERE TO_CHAR(SCANTIM,'YYYY-MM') =TO_CHAR(SYSDATE,'YYYY-MM') AND BARCODE ='" + sBarcode + "'";
//                DataTable dt = db.GetDataTable(strSql);
//                if (dt.Rows.Count > 0)
//                    bDup = true;

//                return bDup;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return true;
//            }
//        }


  
       
//        #endregion

//        #region 硫化废品确认
    


//        [HttpGet]
//        [Route("api/GetBillNoF")]
//        public DataTable GetBillNoF(string sBarcode, string sBILLNO)
//        {
//            IDataBase db = new OracleBase();

//            try
//            {
//                string strSql = "SELECT * FROM SJE0011 WHERE BARCODE ='" + sBarcode + "' AND BILLNO='" + sBILLNO + "'";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetBillNoF_NO")]
//        public DataTable GetBillNoF_NO()
//        {
//            IDataBase db = new OracleBase();

//            try
//            {
//                string strSql = "SELECT DISTINCT BILLNO  FROM(SELECT BILLNO FROM  SJE0011  WHERE BILLNO IS NOT NULL ORDER BY SCANTIM DESC) WHERE ROWNUM<4";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }
//        #endregion

//        #region 成型半成品

//        //半成品邀请
//        [HttpGet]
//        [Route("api/GetMchid")]
//        public DataTable GetMchid()
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = "SELECT MCHID FROM EDA0001 WHERE PROC ='C' AND STEP ='C1' ORDER BY MCHID";
//                DataTable DT = db.GetDataTable(sql);
//                if (DT.Rows.Count > 0)
//                {
//                    return DT;
//                }
//                else return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetDiv")]
//        public DataTable GetDiv(string mchid)
//        {
//            IDataBase db = new OracleBase();
//            DataRow ROW = SHIFT.GetShift(DateTime.Now);
//            try
//            {
//                string wdate = "TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd')";
//                string wsht = ROW["WSHT"].ToString();
//                string Sql_div = @"SELECT SBNBR1,SBDSC1, SBNBR2,SBDSC2,CCNBR1, CCDSC1,CCNBR2,CCDSC2, CPNBR, CPDSC,BDNBR, BDDSC,TRNBR, TRDSC,SWNBR, SWDSC,ILNBR,ILDSC
//                           FROM PAD0307";
//                Sql_div += @" WHERE WDATE = " + wdate + " AND MCHID ='" + mchid + "' AND DIV='1'";//AND ITNBR = '" + dt_nbr.Rows[0]["ITNBR"].ToString() + "' AND WSHT= '" + wsht + "'
//                DataTable dt = db.GetDataTable(Sql_div);

//                if (dt.Rows.Count > 0)
//                {
//                    return dt;
//                }
//                else
//                    return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }

//        }

//        [HttpGet]
//        [Route("api/InsIfHalb07")]
//        public int InsIfHalb07(string mchid, string div, string itnbr, string itdsc, string userid)
//        {
//            IDataBase db = new OracleBase();
//            IDataBase db2 = new OracleBase("");
//            int i = 0;
//            try
//            {
//                string sql_cod3 = "SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1 AND ITNBR = '" + itnbr + "'";
//                string cod3 = "";
//                DataTable dt_cod = db.GetDataTable(sql_cod3);
//                if (dt_cod.Rows.Count == 1)
//                {
//                    cod3 = dt_cod.Rows[0]["ITTYPECOD"].ToString();
//                    string sql = string.Format(@"INSERT INTO IF_WMS_HALB_07(INVTIM,MCHID,POSITION,ITGRPCOD3,ITNBR,ITDSC,INVQTY,PRIORITY,
//                                        RCV_FLAG,RCV_DT,ENT_USER_ID,ENT_DT,UPD_USER_ID,UPD_DT)
//                                         VALUES({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}',{11},'{12}',{13})",
//                                            NowTime, mchid, div, cod3, itnbr, itdsc, "1", null, null, null, userid, NowTime, userid, NowTime);
//                    i = db2.ExecuteNonQuery(sql);
//                }
//                if (dt_cod.Rows.Count >= 1) i = dt_cod.Rows.Count;
//                return i;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return 0;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetSIDWMS_HALB")]
//        public DataTable GetSIDWMS_HALB(string div)
//        {
//            IDataBase db3 = new OracleBase("", "");
//            try
//            {
//                if (string.IsNullOrEmpty(div)) return null;
//                string Sql_div = @"SELECT ITNBR,ITDSC,COUNT(LOTNO) AS LOTNUM
//                           FROM IF_WMS_HALB_INV_VIEW
//                           WHERE STS='0' AND USE='O' ";
//                //只有胎面会有两个区分，其余的都是一个
//                if (div.Contains(","))
//                {
//                    string[] arr = div.Split(',');
//                    string divNew = "'" + arr[0] + "','" + arr[1] + "'";
//                    Sql_div += " AND STODIV IN (" + divNew + ") ";
//                }
//                else
//                    Sql_div += " AND STODIV ='" + div + "'";

//                Sql_div += @" GROUP BY ITNBR,ITDSC
//                      ORDER BY ITNBR";
//                DataTable dt = db3.GetDataTable(Sql_div);

//                if (dt.Rows.Count > 0)
//                {
//                    return dt;
//                }
//                else
//                    return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        //半成品回收
//        [HttpGet]
//        [Route("api/Get_HalfBack")]
//        public string Get_HalfBack(string[] str)
//        {
//            //IF_WMS_HALB_08 ,IF_WMS_HALB_10中间表

//            //4 该工装位置已回收
//            //3 库存不存在该工装号
//            //0 Web异常
//            //1成功
//            //2 输入的余量大于在制品库存数

//            //0成型机号
//            //1空工装位置(9)
//            //2搬运工具号
//            //3工装状态（2余量、4不良、3空工装、1满工装）
//            //4产品小分类
//            //5规格代码
//            //6规格名称
//            //7操作工号
//            //8半成品状态
//            //9LOTID
//            //10余量
//            try
//            {
//                IDataBase db = new OracleBase();
//                IDataBase dbif = new OracleBase("");
//                IDataBase dbsid = new OracleBase("", "");
//                DataRow ROW = SHIFT.GetShift(DateTime.Now);
//                //

//                if (str[1].Contains("BP"))
//                    str[1] = str[1].Replace("BP", "CC");
//                //
//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[7] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);
//                if (dtEMP.Rows.Count == 0)
//                    return "0";
//                //
//                string issql = @"SELECT INVTIM FROM IF_WMS_HALB_08 
//                             WHERE MCHID='" + str[0] + "' AND POSITION='" + str[1] + "' " +
//                                @" AND INVTIM>TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd") + "','yyyy-MM-dd') " +
//                                @" ORDER BY INVTIM DESC";
//                DataTable dt = dbif.GetDataTable(issql);
//                if (dt.Rows.Count > 0)
//                {
//                    DateTime invagotim = Convert.ToDateTime(dt.Rows[0]["INVTIM"]);
//                    TimeSpan ts = DateTime.Now - invagotim;
//                    if (ts.TotalMinutes < 10)
//                    {
//                        return "4";
//                    }
//                }
//                //
//                string LOTID = str[9];
//                string ITDSC = str[6];
//                string ITNBR = str[5];
//                string SL = str[10];
//                string wdate = DateTime.Now.ToString("yyyy-MM-dd");
//                string wtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

//                if (LOTID == "")
//                    LOTID = "LOT" + DateTime.Now.ToString("yyyyMMddhh24missfff");
//                if (ITNBR == "")
//                    ITNBR = "EmptyCart";
//                if (ITDSC == "")
//                    ITDSC = "空车回收";

//                int NOW = 0;
//                string dw = "";
//                DataTable WIPDT = db.GetDataTable("SELECT * FROM WIP0001 WHERE TOOLNO='" + str[2].ToUpper() + "' AND WTIME IS NOT NULL ORDER BY WTIME DESC");
//                if (WIPDT.Rows.Count != 0)
//                {
//                    wdate = Convert.ToDateTime(WIPDT.Rows[0]["WDATE"]).ToString("yyyy-MM-dd");
//                    wtime = Convert.ToDateTime(WIPDT.Rows[0]["WTIME"]).ToString("yyyy-MM-dd HH:mm:ss");
//                    if (str[1] != "BD1")
//                    {
//                        NOW = Convert.ToInt16(WIPDT.Rows[0]["LTHEN"]);
//                        dw = "M";
//                        SL = WIPDT.Rows[0]["LTHEN"].ToString();
//                    }
//                    else
//                    {
//                        NOW = Convert.ToInt16(WIPDT.Rows[0]["QTY"]);
//                        SL = WIPDT.Rows[0]["QTY"].ToString();
//                        dw = "EA";
//                    }
//                    if (Convert.ToInt16(str[10]) > NOW)
//                        return "2";
//                    ITNBR = WIPDT.Rows[0]["ITNBR"].ToString();
//                    ITDSC = WIPDT.Rows[0]["ITDSC"].ToString();
//                    LOTID = WIPDT.Rows[0]["LOTID"].ToString();
//                }
//                else
//                {
//                    //空的工装不会判断在制品库存里有没有这玩意
//                    if (str[3] != "3")
//                        return "3";
//                    if (str[1] != "BD1")
//                        dw = "M";
//                    else
//                        dw = "EA";
//                }
//                DataTable hhdt = dbsid.GetDataTable("SELECT * FROM IF_WMS_HALB_INV_VIEW WHERE LOTNO='" + LOTID + "'");
//                if (hhdt.Rows.Count > 0)
//                    LOTID = "LOT" + DateTime.Now.ToString("yyyyMMddhh24missfff");
//                //半成品工装返回邀请信息(必须插入)
//                string sqlstr = @"insert into IF_WMS_HALB_08 (
//                INVTIM,MCHID,POSITION,CARTID,CARTSTATE,
//                ITGRPCOD3,ITNBR,ITDSC,RCV_FLAG,
//                RCV_DT,ENT_USER_ID,ENT_DT
//               ,UPD_USER_ID
//               ,UPD_DT)	    
//                VALUES
//                (SYSDATE,'" + str[0] + "','" + str[1] + "','" + str[2] + "','" + str[3] + "', " +
//                   @"'" + str[4] + "','" + ITNBR + "','" + ITDSC + "','N', " +
//                   @"'','" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                   @"'" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'))";
//                List<string> list = new List<string>();
//                list.Add(sqlstr);
//                //空工装
//                if (str[3] == "3")
//                {
//                    string sql = "DELETE WIP0001 WHERE LOTID='" + LOTID + "'";
//                    db.ExecuteNonQuery(sql);
//                    //半成品生产余量信息()
//                    string sqlstr2 = @"insert into IF_WMS_HALB_10 ( " +
//                        @"LOTNO,UNLOADDAT,MCHID,ITNBR,ITDSC, " +
//                        @"WDATE,WTIME,LOTSTATUS,REMQTY,　" +
//                        @"UNIT,ITGRPCOD3,CARTID,CARTSTATE,USERID,RCV_FLAG,RCV_DT
//                   ,ENT_USER_ID,ENT_DT
//                   ,UPD_USER_ID,UPD_DT) 
//                   VALUES
//                   ('" + LOTID + "',sysdate,'" + str[0] + "','" + ITNBR + "','" + ITDSC + "', " +
//                       @"to_date('" + DateTime.Now.ToShortDateString() + "','yyyy-MM-dd'),sysdate,'" + str[8] + "','0', " +
//                       @"'" + dw + "','" + str[4] + "','" + str[2] + "','" + str[3] + "','" + str[7] + "','N','', " +
//                       @"'" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'))";
//                    list.Add(sqlstr2);
//                }
//                //满工装
//                else if (str[3] == "1")
//                {
//                    //半成品生产余量信息()
//                    string sqlstr2 = @"insert into IF_WMS_HALB_10 (
//                LOTNO,UNLOADDAT,MCHID,ITNBR,ITDSC,
//                WDATE,WTIME,LOTSTATUS,REMQTY,
//                UNIT,ITGRPCOD3,CARTID,CARTSTATE,
//                USERID,RCV_FLAG,RCV_DT
//               ,ENT_USER_ID,ENT_DT
//               ,UPD_USER_ID,UPD_DT) 
//               VALUES
//               ('" + LOTID + "',sysdate,'" + str[0] + "','" + ITNBR + "','" + ITDSC + "', " +
//                   @"TO_DATE('" + DateTime.Now.ToShortDateString() + "','yyyy-MM-dd'),sysdate,'" + str[8] + "','" + SL + "', " +
//                   @"'" + dw + "','" + str[4] + "','" + str[2] + "','" + str[3] + "', " +
//                   @"'" + str[7] + "','N','', " +
//                   @"'" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                   @"'" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'))";
//                    list.Add(sqlstr2);
//                }
//                //不良余量工装
//                else
//                {
//                    //半成品生产余量信息
//                    string sqlstr2 = @"insert into IF_WMS_HALB_10 (
//                    LOTNO,UNLOADDAT,MCHID,ITNBR,ITDSC,
//                    WDATE,WTIME,LOTSTATUS,REMQTY,
//                    UNIT,ITGRPCOD3,CARTID,CARTSTATE,
//                    USERID,RCV_FLAG,RCV_DT
//                   ,ENT_USER_ID,ENT_DT
//                   ,UPD_USER_ID,UPD_DT) 
//                    VALUES
//                   ('" + LOTID + "',sysdate,'" + str[0] + "','" + ITNBR + "','" + ITDSC + "', " +
//                       @"TO_DATE('" + wdate + "','yyyy-MM-dd'),TO_DATE('" + wtime + "','yyyy-MM-dd hh24:mi:ss'),'" + str[8] + "','" + str[10] + "', " +
//                       @"'" + dw + "','" + str[4] + "','" + str[2] + "','2', " +
//                       @"'" + str[7] + "','N','', " +                 //" + str[3] + "
//                       @"'" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), " +
//                       @"'" + str[7] + "',TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'))";
//                    list.Add(sqlstr2);

//                    List<string> sqlList = new List<string>();
//                    //余量
//                    if (str[3] == "2")
//                    {

//                        string UpdateWIP = "UPDATE WIP0001 SET LTHEN='" + str[10] + "' WHERE LOTID='" + LOTID + "' AND DIV='02'";
//                        string UpdateLTC = "UPDATE LTC0001 SET QTYY='" + str[10] + "' WHERE LOTID='" + LOTID + "' AND DIV='02'";
//                        sqlList.Add(UpdateWIP);
//                        sqlList.Add(UpdateLTC);
//                        db.TranNonQuery(sqlList.ToArray());
//                    }
//                    //不良
//                    else if (str[3] == "4")
//                    {
//                        string UpdateWIP = "UPDATE WIP0001 SET LTHEN='" + str[10] + "',STA='2' WHERE LOTID='" + LOTID + "' AND DIV='02'";
//                        string UpdateLTC = "UPDATE LTC0001 SET QTYY='" + str[10] + "',STYN='2' WHERE LOTID='" + LOTID + "' AND DIV='02'";
//                        sqlList.Add(UpdateWIP);
//                        sqlList.Add(UpdateLTC);

//                        string sqlQMC = "SELECT * FROM QMC0101 WHERE LOTID='" + LOTID + "' AND DIV='02'";
//                        DataTable dtQMC = db.GetDataTable(sqlQMC);
//                        if (dtQMC.Rows.Count > 0)
//                        {
//                            string UpdateQMC = @"UPDATE QMC0101 SET AYN='N' , BQTY='" + str[10] + "' , CNAM ='" + dtEMP.Rows[0]["NAME"].ToString() + "' , " +
//                                @" CDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), CTIM=TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), CSHT ='" + ROW["WSHT"].ToString() + "' ," +
//                                @" CBAN='" + ROW["WBAN"].ToString() + "' WHERE LOTID='" + LOTID + "' AND DIV='02'";
//                            sqlList.Add(UpdateQMC);
//                        }
//                        else
//                        {
//                            string InsertQMC = "INSERT INTO QMC0101 (ID,FAC,DIV,AYN,LOTID,ITNBR,ITDSC,BQTY,MCHID,CNAM,CDAT,CTIM,CSHT,CBAN) " +
//                                @" VALUES('" + Guid.NewGuid() + "','" + FAC + "','02','Y','" + LOTID + "','" + ITNBR + "', " +
//                                @" '" + ITDSC + "', '" + str[10] + "','" + str[1] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "' , " +
//                                @" to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), TO_DATE('" + DateTime.Now.ToString() + "','yyyy-MM-dd hh24:mi:ss'), '" + ROW["WSHT"].ToString() + "' , '" + ROW["WBAN"].ToString() + "')";
//                            sqlList.Add(InsertQMC);
//                        }
//                        db.TranNonQuery(sqlList.ToArray());
//                    }

//                    //BD1 胎圈 CC1 CC2 帘布1、2  CP1 冠带条 SW1 胎侧 TD1 胎面 SB1 SB2 带束1、2 IL1 内衬
//                    if (str[1] != "BD1")//非胎圈 
//                    {
//                        string sql = "UPDATE WIP0001 SET LTHEN='" + str[10] + "', STA='" + str[8] + "' WHERE LOTID='" + LOTID + "'";
//                        db.ExecuteNonQuery(sql);
//                    }
//                    else //胎圈 
//                    {
//                        string sql = "UPDATE WIP0001 SET QTY='" + str[10] + "', STA='" + str[8] + "' WHERE LOTID='" + LOTID + "'";
//                        db.ExecuteNonQuery(sql);
//                    }
//                }
//                dbif.TranNonQuery(list.ToArray());
//                db.ExecuteNonQuery("DELETE FROM STF0011 WHERE MCHID='" + str[0] + "' AND POSITION='" + str[1] + "' AND CARTID='" + str[2] + "'");
//                return "1";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "0";
//            }
//        }




//        [HttpGet]
//        [Route("api/GetSTF0010")]
//        public List<string> GetSTF0010(string mchid, string[] str)
//        {
//            //0机台
//            //1 BD1,CC1,CC2,CP1,IL1,SB1,SB2,SW1,TD1

//            IDataBase db = new OracleBase();
//            List<string> mess = new List<string>();
//            try
//            {
//                string strSql = @"SELECT * FROM STF0010 WHERE MCHID='" + mchid + "' AND RFIDFLAG='1' ORDER BY ETIM DESC ";
//                DataTable dt = db.GetDataTable(strSql);
//                if (dt != null && dt.Rows.Count > 0)
//                {
//                    string fl = "";
//                    for (int i = 0; i < str.Length; i++)
//                    {
//                        DataRow[] dr = dt.Select("POSITION='" + str[i] + "'");
//                        if (dr.Length > 0)
//                            fl += "1,";
//                        else
//                            fl += "0,";
//                    }
//                    fl = fl.Substring(0, fl.Length - 1);
//                    mess.Add(fl);
//                }
//                else
//                    mess.Add("0,0,0,0,0,0,0,0,0");
//                return mess;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return mess;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetSTF0010_1")]
//        public string GetSTF0010_1(string mchid, string pro)
//        {
//            //0机台
//            //1半成品位置 BD1,CC1,CC2,CP1,IL1,SB1,SB2,SW1,TD1

//            IDataBase db = new OracleBase();
//            string mess = "";
//            try
//            {
//                string strSql = @"SELECT * FROM STF0010 WHERE MCHID='" + mchid + "'  AND POSITION='" + pro + "' AND RFIDFLAG='1' ORDER BY ETIM DESC ";
//                DataTable dt = db.GetDataTable(strSql);
//                if (dt != null && dt.Rows.Count > 0)
//                    mess = "1";
//                else
//                    mess = "0";
//                return mess;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return mess;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetSTF0011")]
//        public DataTable GetSTF0011(string[] str)
//        {
//            //0机台
//            //1半成品区分
//            //2工装号

//            IDataBase db = new OracleBase();
//            try
//            {
//                string Sql_div = @"SELECT * FROM STF0011
//                           WHERE MCHID='" + str[0] + "' AND POSITION='" + str[1] + "' AND CARTID='" + str[2] + "'";
//                DataTable dt = db.GetDataTable(Sql_div);
//                if (dt.Rows.Count > 0)
//                    return dt;
//                else
//                    return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetCARTID")]
//        public DataTable GetCARTID(string[] str)
//        {
//            //0机台
//            //1半成品区分

//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = "SELECT DISTINCT CARTID FROM STF0011 WHERE MCHID='" + str[0] + "' AND POSITION='" + str[1] + "' AND CARTID IS NOT NULL ORDER BY CARTID";
//                DataTable DT = db.GetDataTable(sql);
//                if (DT.Rows.Count > 0)
//                    return DT;
//                else
//                    return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetLTC0001")]
//        public DataTable GetLTC0001(string[] str)
//        {
//            //0工装号

//            IDataBase db = new OracleBase();
//            try
//            {
//                string Sql_div = @"SELECT * FROM LTC0001
//                           WHERE BID='" + str[0] + "' ORDER BY AUTIM DESC ";
//                DataTable dt = db.GetDataTable(Sql_div);
//                if (dt.Rows.Count > 0)
//                    return dt;
//                else
//                    return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetSTA0002_1")]
//        public DataTable GetSTA0002_1(string[] str)
//        {
//            //0 工装号
//            //1 区分（BD、CP） 

//            IDataBase db = new OracleBase();
//            try
//            {
//                string strSTA0002 = @"SELECT * FROM STA0002 " +
//                                    @" WHERE DIV='" + str[1] + "' " +
//                                    @" AND CARTID='" + str[0] + "' " +
//                                    @" AND RFTAGYN='Y' AND USEYN='Y' AND FAC='" + FAC + "'";
//                DataTable dtSTA0002 = db.GetDataTable(strSTA0002);
//                if (dtSTA0002.Rows.Count > 0)
//                    return dtSTA0002;
//                else
//                    return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }

//        #endregion

       

  

 
//        #region 修边

//        [HttpGet]
//        [Route("api/GetTRIMDetail")]
//        public string GetTRIMDetail(string[] str)
//        {
//            //0 条码
//            //1 员工号
//            //2 班组

//            string sTRIMDetail = string.Empty;
//            IDataBase db = new OracleBase();
//            DataRow ROW = SHIFT.GetShift(DateTime.Now);
//            try
//            {
//                //
//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[1] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);
//                if (dtEMP.Rows.Count == 0)
//                    return "Err#员工不存在！";
//                //
//                string sqlCKA = "SELECT * FROM CKA0020 WHERE BARCODE='" + str[0] + "' AND FAC='" + FAC + "'";
//                DataTable dtCKA = db.GetDataTable(sqlCKA);
//                if (dtCKA.Rows.Count > 0)
//                    return "Err#此条码已登记！";
//                //
//                DataTable dtLTA0001 = db.GetDataTable("SELECT * FROM LTA0001 WHERE BARCODE='" + str[0] + "' AND FAC='" + FAC + "'");
//                string date = DateTime.Now.ToString();
//                if (dtLTA0001 != null && dtLTA0001.Rows.Count > 0)
//                {
//                    string sInsert = @"INSERT INTO CKA0020(ID,FAC,BARCODE, " +
//                         @"TRIMDAT, " +
//                         @"TRIMTIM, " +
//                         @"TRIMSHT,TRIMNAM, " +
//                         @"TRIMNUM,TRIMBAN) VALUES( " +
//                         @"'" + Guid.NewGuid() + "','" + FAC + "','" + str[0] + "', " +
//                         @"TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'), " +
//                         @"TO_DATE('" + Convert.ToDateTime(date) + "','yyyy-MM-dd hh24:mi:ss'), " +
//                         @"'" + ROW["WSHT"].ToString() + "','" + dtEMP.Rows[0]["NAME"].ToString() + "', " +
//                         @"'1','" + str[2] + "')";
//                    int i = db.ExecuteNonQuery(sInsert);
//                    string wsht = "";
//                    if (ROW["WSHT"].ToString() == "1") wsht = "早班";
//                    else if (ROW["WSHT"].ToString() == "2") wsht = "中班";
//                    else if (ROW["WSHT"].ToString() == "3") wsht = "夜班";
//                    if (i == 1)
//                    {
//                        sTRIMDetail += "" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "";
//                        sTRIMDetail += "^";
//                        sTRIMDetail += "" + Convert.ToDateTime(date).ToString("yyyy-MM-dd HH:mm:ss") + "";
//                        sTRIMDetail += "^";
//                        sTRIMDetail += "" + dtEMP.Rows[0]["NAME"].ToString() + "";
//                        sTRIMDetail += "^";
//                        sTRIMDetail += "" + wsht + "";
//                        return "OK#" + sTRIMDetail;
//                    }
//                    else
//                        return "Err#登记失败！";
//                }
//                else
//                    return "Err#追溯记录没有此条码信息！";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "Err#" + ex.Message;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetTRIMNum")]
//        public string GetTRIMNum(string[] str)
//        {
//            //0员工号

//            IDataBase db = new OracleBase();
//            DataRow ROW = SHIFT.GetShift(DateTime.Now);
//            try
//            {
//                //
//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[0] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);
//                if (dtEMP.Rows.Count == 0)
//                    return "员工不存在！";
//                //
//                string strSql = @"SELECT * FROM CKA0020 WHERE TRIMSHT='" + ROW["WSHT"] + "' " +
//                    @" AND TRIMDAT=TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd') " +
//                    @" AND TRIMNAM='" + dtEMP.Rows[0]["NAME"].ToString() + "'";
//                DataTable dt = db.GetDataTable(strSql);
//                if (dt != null && dt.Rows.Count > 0)
//                    return dt.Rows.Count.ToString();
//                else
//                    return "0";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return ex.Message;
//            }
//        }

//        #endregion

 

//        #region SDS检测

//        /// <summary>
//        /// SDS判定，告知检测线流向数据(SDS判定)
//        /// </summary>
//        /// <param name="BARCODE">条码</param>
//        /// <param name="USENAME">判定人</param>
//        /// <param name="AYN">1合格  2报废</param>
//        /// <param name="CODE">报废CODE</param>
//        /// <returns>1 成功 2条码不存在  3未知错误 4已被判废品 5已被登记</returns>
//        [HttpGet]
//        [Route("api/A_SDS_CHECK")]
//        public string A_SDS_CHECK(string BARCODE, string USENAME, string AYN, string CODE)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                //SHIFT.GetSht(db);
//                DataRow ROW = SHIFT.GetShift(DateTime.Now);
//                string dbdt = @"SELECT 
//                          A.BARCODE,CUDAT ,CUTIM, CUSHT , CUBAN, CUMCH , LR, C.ITNBR AS CUITNBR , C.ITDSC AS CUITDSC, STATE, CUSTATE,
//                          MODCOD, SLECOD, WGRES , UFRES, DBRES, XRES, SDSRES, UFCNT, DBCNT, CUSQTY, 
//                          CUDQTY , CUBQTY, CUIQTY , CUSMYN , BUITNBR, BUITDSC ,BUDAT , BUTIM, BUSHT , BUBAN , 
//                          BUMCH , BUNAM , WYYN , SEWT , REWT, BUSQTY , BUDQTY , BUBQTY , BUIQTY , BUSMYN ,
//                          TRLOTID , SWLOTID , BDLOTID, SBLOTID1, SBLOTID2, CCLOTID1 , CCLOTID2, ILLOTID, CPLOTID, 
//                          BARLAT , BARANG, BARLR, CUCOTCOD, BUCOTCOD , BOMVER, NORMALYN
//                          FROM 
//                          LTA0001 A
//                          LEFT  JOIN STG0005 B
//                          ON A.BARCODE=B.BARCODE
//                          LEFT JOIN (SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) C
//                          ON B.ITNBR = C.ITNBR
//                          WHERE A.FAC='" + FAC + "' AND A.BARCODE='" + BARCODE + "'";
//                DataTable dtbarmes = db.GetDataTable(dbdt);
//                if (dtbarmes.Rows.Count > 0)
//                {
//                    string sqlm = "SELECT * FROM QMA0101 WHERE  DIV='5' AND BARCODE='" + BARCODE + "'";
//                    DataTable dtm = db.GetDataTable(sqlm);
//                    if (dtm.Rows.Count > 0)
//                        return "4";
//                    sqlm = "SELECT * FROM QMA0002 WHERE  DIV='2' AND BARCODE='" + BARCODE + "'";
//                    dtm = db.GetDataTable(sqlm);
//                    if (dtm.Rows.Count > 0)
//                        return "5";
//                    string CUTIM = "";
//                    string CUDAT = "";
//                    if (string.IsNullOrEmpty(dtbarmes.Rows[0]["CUTIM"].ToString()))
//                        CUTIM = "NULL";
//                    else
//                        CUTIM = "to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["CUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";

//                    if (string.IsNullOrEmpty(dtbarmes.Rows[0]["CUDAT"].ToString()))
//                        CUDAT = "NULL";
//                    else
//                        CUDAT = "to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["CUDAT"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";

//                    List<string> list = new List<string>();
//                    List<string> listIF = new List<string>();
//                    //SDS检测结果履历
//                    //string Update_CKA0008 = @" UPDATE CKA0008 SET SDSRESULT='" + AYN + "' ,SDSTIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss') WHERE FAC='" + FAC + "' AND BARCODE='" + BARCODE + "'";
//                    string Update_CKA0008 = @"INSERT INTO CKA0008 " +
//                       @"(ID,FAC,BARCODE, " +
//                       @"SDSSTATION,SDSRESULT, " +
//                       @"SDSTIM " +
//                       @") VALUES( " +
//                       @"SYS_GUID(),'" + FAC + "','" + BARCODE + "', " +
//                       @"'SDS01','" + AYN + "'," +
//                       @"TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss') )";
//                    if (AYN == "2")
//                    {
//                        string dbsql = @"INSERT INTO QMA0101(
//                    ID,FAC,DIV,IDAT,IBAN, 
//                    ISHT,INAM,ITIM,COD,CIDAT,
//                    CIBAN,CISHT,CINAM,CITIM,BUITNBR,
//                    BUITDSC,CUITNBR,CUITDSC,CCOD,BUMCH,
//                    BUDAT,BUTIM,
//                    BUSHT,BUBAN,BUNAM,CUMCH,
//                    CUDAT,CUTIM,
//                    CUSHT,CUBAN, LR,BARCODE,AYN, 
//                    STWT, REWT,MODCOD, SLECOD,STATE)
//                    VALUES 
//                    (SYS_GUID(),'" + FAC + "','5',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),'" + ROW["WBAN"] + @"',
//                    '" + ROW["WSHT"] + "','" + USENAME + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + CODE + "',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                    '" + ROW["WBAN"] + "','" + ROW["WSHT"] + "','" + USENAME + "',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss'),'" + dtbarmes.Rows[0]["BUITNBR"] + @"',
//                    '" + dtbarmes.Rows[0]["BUITDSC"] + "', '" + dtbarmes.Rows[0]["CUITNBR"] + "','" + dtbarmes.Rows[0]["CUITDSC"] + "', '" + CODE + "','" + dtbarmes.Rows[0]["BUMCH"] + @"',
//                    to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),to_date('" + Convert.ToDateTime(dtbarmes.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                    '" + dtbarmes.Rows[0]["BUSHT"] + "',  '" + dtbarmes.Rows[0]["BUBAN"] + "',  '" + dtbarmes.Rows[0]["BUNAM"] + "', '" + dtbarmes.Rows[0]["CUMCH"] + @"',
//                    " + CUDAT + "," + CUTIM + @",
//                    '" + dtbarmes.Rows[0]["CUSHT"] + "',  '" + dtbarmes.Rows[0]["CUBAN"] + "', '" + dtbarmes.Rows[0]["LR"] + "','" + BARCODE + @"','C',
//                    '" + dtbarmes.Rows[0]["SEWT"] + "',  '" + dtbarmes.Rows[0]["REWT"] + "', '" + dtbarmes.Rows[0]["MODCOD"] + "',  '" + dtbarmes.Rows[0]["SLECOD"] + @"', '" + dtbarmes.Rows[0]["CUSTATE"] + "')";
//                        string dbsql1 = @"INSERT INTO QMA0002
//                        (ID,FAC, DIV, WDATE, WTIME, 
//                         WBAN, WSHT, WNAM, BARCODE, ITNBR,ITDSC,
//                         MCHID, SDSRES)
//                         VALUES
//                        (SYS_GUID(), '" + FAC + "', '2',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        '" + ROW["WBAN"] + "','" + ROW["WSHT"] + "','" + USENAME + "','" + BARCODE + "', '" + dtbarmes.Rows[0]["CUITNBR"] + "','" + dtbarmes.Rows[0]["CUITDSC"] + @"',
//                         '" + dtbarmes.Rows[0]["CUMCH"] + "','C' )";

//                        list.Add(dbsql);
//                        list.Add(dbsql1);
//                        list.Add(Update_CKA0008);
//                        //db.TranNonQuery(list.ToArray());
//                    }
//                    else
//                    {
//                        string dbsql1 = @"INSERT INTO QMA0002
//                        (ID,FAC, DIV, WDATE, WTIME, 
//                         WBAN, WSHT, WNAM, BARCODE, ITNBR,ITDSC,
//                         MCHID, SDSRES)
//                         VALUES
//                        (SYS_GUID(), '" + FAC + "', '2',TO_DATE('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        '" + ROW["WBAN"] + "','" + ROW["WSHT"] + "','" + USENAME + "','" + BARCODE + "', '" + dtbarmes.Rows[0]["CUITNBR"] + "','" + dtbarmes.Rows[0]["CUITDSC"] + @"',
//                         '" + dtbarmes.Rows[0]["CUMCH"] + "','A' )";

//                        list.Add(dbsql1);
//                        list.Add(Update_CKA0008);
//                        //db.TranNonQuery(list.ToArray());
//                    }
//                    IDataBase middb = new OracleBase("07");
//                    string mesid = string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now);
//                    //告知VR数据流向(合格)
//                    string sql = @"INSERT INTO SDSRESULT( MESSAGEID,STATUS,DATETIME1,DATETIME2,BARCODE,STATION,RESULT)
//                            VALUES 
//					        ('" + mesid + "','1',to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),NULL,'" + BARCODE + "','SDS01','" + AYN + "')";
//                    //middb.ExecuteNonQuery(sql);
//                    listIF.Add(sql);
//                    int i = RollBackOracle(list.ToArray(), listIF.ToArray());
//                    if (i > 0)
//                        return "1";
//                    else
//                        return "3";
//                }
//                else
//                {
//                    return "2";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "3";
//            }
//        }

//        [HttpGet]
//        [Route("api/RollBackOracle")]
//        public int RollBackOracle(string[] SQL1, string[] SQL2)
//        {
//            OracleConnection mescon = new OracleConnection(ConfigurationManager.AppSettings["ConnectionString"]);
//            OracleConnection ifcon = new OracleConnection(ConfigurationManager.AppSettings["ConnectionString2"]);
//            using (OracleCommand mescmd = new OracleCommand())//using (OracleCommand mescmd = new OracleCommand(SQL1, mescon))
//            {
//                int i = 0;
//                try
//                {
//                    if (mescon.State != ConnectionState.Open)
//                        mescon.Open();
//                    //mescmd.Transaction = mescon.BeginTransaction();
//                    //mescmd.ExecuteNonQuery();

//                    mescmd.Connection = mescon;
//                    mescmd.Transaction = mescon.BeginTransaction();
//                    mescmd.CommandText = "BEGIN \n ";
//                    foreach (string s in SQL1)
//                    {
//                        mescmd.CommandText += s + ";\n";
//                        i++;
//                    }
//                    mescmd.CommandText += "\nEND;";
//                    mescmd.ExecuteNonQuery();
//                    //mescmd.Transaction.Commit();
//                    #region
//                    using (OracleCommand ifcmd = new OracleCommand())//using (OracleCommand ifcmd = new OracleCommand(SQL2, ifcon))
//                    {
//                        try
//                        {
//                            if (ifcon.State != ConnectionState.Open)
//                                ifcon.Open();
//                            //ifcmd.ExecuteNonQuery();
//                            //ifcmd.Transaction.Commit();

//                            ifcmd.Connection = ifcon;
//                            ifcmd.Transaction = ifcon.BeginTransaction();
//                            ifcmd.CommandText = "BEGIN \n ";
//                            foreach (string s in SQL2)
//                            {
//                                ifcmd.CommandText += s + ";\n";
//                                i++;
//                            }
//                            ifcmd.CommandText += "\nEND;";
//                            ifcmd.ExecuteNonQuery();
//                            ifcmd.Transaction.Commit();
//                        }
//                        catch (Exception ex)
//                        {
//                            mescmd.Transaction.Rollback();
//                            ifcmd.Transaction.Rollback();
//                            //config.Log.ErrLog(ex);
//                            LogHelper.Error("ERROR", ex);





//                        }
//                    }
//                    #endregion
//                    mescmd.Transaction.Commit();
//                }
//                catch (Exception ex)
//                {
//                    mescmd.Transaction.Rollback();
//                    LogHelper.Error("ERROR", ex);





//                }
//                return i;
//            }
//        }

//        #endregion

//        #region 药粉投入
//        [HttpGet]
//        [Route("api/GetYaoITNBR")]
//        public DataTable GetYaoITNBR(string[] str)
//        {
//            //0 罐号
//            //1 Lineno

//            IDataBase db = new OracleBase();

//            try
//            {
//                string strSql = "SELECT * FROM TDB0431 WHERE BIN_NO ='" + str[0] + "' AND LINE_NO ='" + str[1] + "'";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }





//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sBinNo">罐号</param>
//        /// <param name="sCtlDiv">控制区分 0-关 1-开</param>
//        /// <returns></returns>

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sDiv">1-黑炭黑 2-白炭黑 3-碳酸钙</param>
//        /// <param name="sLineNo">机组号</param>
//        /// <param name="sGuanNo">罐号</param>
//        /// <param name="sLotid">批次号</param>
//        /// <param name="sUser">投入人</param>
//        /// <returns>结果</returns>

//        /// <summary>
//        /// 补录小料库存
//        /// </summary>
//        /// <param name="sLotid">要补录的批次号</param>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("api/BLWip")]
//        public string BLWip(string sLotid)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sSql = "INSERT INTO WIP0003 SELECT SYS_GUID(),'07',DIV,LOTID,RFIDNO,ITNBR,ITDSC,TOOLNO,LTHEN,WT,STA,STIME,ETIME,WDATE,WTIME,LOCKYN,STKSTS,CUSTBATCHLOT FROM WIP0003_LOG WHERE LOTID = '" + sLotid + "' AND STKSTS = '12' AND ROWNUM=1";
//                int i = db.ExecuteNonQuery(sSql);
//                if (i > 0)
//                    return "1";
//                else
//                    return "0";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "Err-" + ex.Message;
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sDiv">0-油料 1-黑炭黑 9-辅料</param>
//        /// <param name="sGuanNo">月罐，油大罐，辅料默认"-1"</param>
//        /// <param name="sLotid"></param>
//        /// <param name="sUser"></param>
//        /// <returns></returns>

//        #endregion

//        #region 原材料领用


     
//        [HttpGet]
//        [Route("api/GetDept")]
//        public DataTable GetDept()
//        {
//            IDataBase db = new OracleBase();

//            try
//            {
//                string strSql = "SELECT * FROM EDA0005 WHERE USEYN='Y'";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }

//        [HttpGet]
//        [Route("api/GetEmpNam")]
//        public DataTable GetEmpNam(string[] str)
//        {
//            //部门名称

//            IDataBase db = new OracleBase();

//            try
//            {
//                string strSql = "SELECT * FROM LSFW_EMPLOYEE WHERE LEAYN='N' AND DEPNAM='" + str[0] + "'";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }


//        [HttpGet]
//        [Route("api/RecordScanLot")]
//        public string RecordScanLot(string sLotId, string sUserid)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string strInsert = "INSERT INTO RAW_LOT_RECORD (LOTNO,RNAM,RTIM) VALUES ('" + sLotId + "','" + sUserid + "',sysdate)";
//                int i = db.ExecuteNonQuery(strInsert);
//                if (i > 0)
//                    return "OK";
//                else
//                    return "ERR";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }



   





       




        



//        [HttpGet]
//        [Route("api/GetDingScanOrNot")]
//        public DataTable GetDingScanOrNot(string sLotid)
//        {
//            IDataBase db = new OracleBase();

//            try
//            {
//                string strSql = "SELECT * FROM STI0001 WHERE LOTNO ='" + sLotid + "' AND RMVDAT IS NULL    AND  INDAT IS NOT NULL  ";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }


  

//        /// <summary>
//        /// 原材料事务
//        /// </summary>
//        /// <param name="sTxnDiv">事务区分</param>
//        /// <param name="sLotID">批次号</param>
//        /// <param name="sITNBR">规格代码</param>
//        /// <param name="sITDSC">规格名称</param>
//        /// <param name="sTxnEmpNo">事务人</param>
//        /// <param name="sTxnQty">事务数量</param>
//        /// <param name="sFacDiv">工厂区分</param>
//        /// <param name="sBackEmpNo">退回人</param>
//        /// <param name="sBackDepNo">退回部门</param>
//        /// <param name="bAllQty">是否全部数量txn</param>
//        /// <returns></returns>


//        [HttpGet]
//        [Route("api/CheckBatchLot")]
//        public string CheckBatchLot(string sLotNo, string sBatchLot)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                string sResult = "OK";
//                string sSql = string.Empty;
//                sSql = "SELECT * FROM STB0002 WHERE CUSTBATCHLOT = '" + sBatchLot + "'";
//                DataTable dt1 = db.GetDataTable(sSql);
//                if (dt1.Rows.Count > 0 && dt1 != null)
//                    sResult = "Err - [重复]该供应商条码已被绑定！";

//                sSql = "SELECT * FROM STB0002 WHERE LOTNO = '" + sLotNo + "'";
//                DataTable dt2 = db.GetDataTable(sSql);
//                if (dt2.Rows.Count > 0 && dt1 != null)
//                {
//                    if (!string.IsNullOrEmpty(dt2.Rows[0]["CUSTBATCHLOT"].ToString()))
//                    {
//                        sResult = "Ask - 该内部条码已绑定其他供应商条码，确定要重新绑定？";
//                    }
//                }
//                return sResult;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return ex.ToString();
//            }
//        }

//        [HttpGet]
//        [Route("api/BindBatchLot")]
//        public string BindBatchLot(string sLotNo, string sBatchLot, string sBindNam)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                string sSql = "UPDATE STB0002 SET CUSTBATCHLOT = '" + sBatchLot + "',BATCHBINDTIM = SYSDATE,BATCHBINDNAM = '" + sBindNam + "' WHERE LOTNO = '" + sLotNo + "' ";
//                int iRes = db.ExecuteNonQuery(sSql);
//                if (iRes > 0)
//                    return "OK - 绑定成功！";
//                else
//                    return "Err - 绑定失败！";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return ex.ToString();

//            }
//        }
//        #endregion

//        #region 外观不良、外观修理

//        /// <summary>
//        /// 获取不良CODE
//        /// </summary>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("api/A_Appear_GETCODE")]
//        public DataSet A_Appear_GETCODE(string[] str)
//        {
//            //0 DIV
//            //1 OTHDIV

//            IDataBase db = new OracleBase();
//            try
//            {
//                DataSet ds = new DataSet();
//                string sql = string.Format(@"SELECT * FROM QMA0001 WHERE FAC='" + FAC + "'AND USEYN='Y' AND DIV LIKE '%" + str[0] + "%' AND OTHDIV LIKE '%" + str[1] + "%' ORDER BY DIV,BCOD ");
//                ds = db.GetDataSet(sql);
//                return ds;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }

//        /// <summary>
//        /// 外观不良
//        /// </summary>
//        /// <returns>1成功 2失败：条码不存在 3.未知错误 4.员工不存在 5.此条码已判废，不可再登记修理 6.登记失败</returns>
//        [HttpGet]
//        [Route("api/B_BAD_RESULT")]
//        public string B_BAD_RESULT(string[] str)
//        {
//            //0 条码
//            //1 外观 CODE
//            //2 修理人工号
//            //3 1.热补 2.打磨(默认) 3热补+打磨
//            //4 1不良 2合格 3报废

//            IDataBase db = new OracleBase();
//            DataRow ROW = SHIFT.GetShift(DateTime.Now);
//            DataTable dt1 = new DataTable();
//            string sql = "";
//            string sql1 = "";
//            string PITIM = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//            try
//            {
//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[2] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);
//                if (dtEMP.Rows.Count == 0)
//                    return "4";

//                string dbdt = @"SELECT 
//                          A.BARCODE,CUDAT ,CUTIM, CUSHT , CUBAN, CUMCH , LR, C.ITNBR AS CUITNBR , C.ITDSC AS CUITDSC, STATE, CUSTATE,
//                          MODCOD, SLECOD, WGRES , UFRES, DBRES, XRES, SDSRES, UFCNT, DBCNT, CUSQTY, 
//                          CUDQTY , CUBQTY, CUIQTY , CUSMYN , BUITNBR, BUITDSC ,BUDAT , BUTIM, BUSHT , BUBAN , 
//                          BUMCH , BUNAM , WYYN , SEWT , REWT, BUSQTY , BUDQTY , BUBQTY , BUIQTY , BUSMYN ,
//                          TRLOTID , SWLOTID , BDLOTID, SBLOTID1, SBLOTID2, CCLOTID1 , CCLOTID2, ILLOTID, CPLOTID, 
//                          BARLAT , BARANG, BARLR, CUCOTCOD, BUCOTCOD , BOMVER, NORMALYN
//                          FROM 
//                          LTA0001 A
//                          LEFT  JOIN STG0005 B
//                          ON A.BARCODE=B.BARCODE
//                          LEFT JOIN (SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) C
//                          ON B.ITNBR = C.ITNBR
//                          WHERE A.FAC='" + FAC + "' AND A.BARCODE='" + str[0] + "'";
//                DataTable dt = db.GetDataTable(dbdt);
//                if (dt.Rows.Count > 0)
//                {
//                    string CUTIM = "";
//                    string CUDAT = "";
//                    if (string.IsNullOrEmpty(dt.Rows[0]["CUTIM"].ToString()))
//                        CUTIM = "NULL";
//                    else
//                        CUTIM = "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";

//                    if (string.IsNullOrEmpty(dt.Rows[0]["CUDAT"].ToString()))
//                        CUDAT = "NULL";
//                    else
//                        CUDAT = "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd')";
//                    string sqlm = "SELECT * FROM QMA0101 WHERE DIV='1' AND BARCODE='" + str[0] + "'";
//                    dt1 = db.GetDataTable(sqlm);
//                    //有不良，更新 无不良新增
//                    if (dt1.Rows.Count > 0)
//                    {
//                        //DataRow[] row = dt1.Select("AYN='C'");
//                        //if (row.Length > 0)
//                        //    return "5";
//                        //修改轮胎的修理病象 
//                        if (str[4] == "1")//不良
//                        {
//                            sql = @"UPDATE QMA0101 
//                        SET IDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        IBAN='" + ROW["WBAN"] + "',ISHT='" + ROW["WSHT"] + "',INAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                        ITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        BIDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        BIBAN='" + ROW["WBAN"] + "',BISHT='" + ROW["WSHT"] + "',BINAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                        BITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        ACOD='" + str[1] + "',AYN='B',COD='" + str[1] + @"'
//				        WHERE DIV='1' AND FAC='" + FAC + "' AND BARCODE='" + str[0] + "'";
//                        }
//                        else if (str[4] == "2")//合格
//                        {
//                            sql = @"UPDATE QMA0101 
//                        SET IDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        IBAN='" + ROW["WBAN"] + "',ISHT='" + ROW["WSHT"] + "',INAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                        ITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        AYN='A',
//				        WHERE DIV='1' AND FAC='" + FAC + "' AND BARCODE='" + str[0] + "'";
//                        }
//                        else if (str[4] == "3")//报废
//                        {
//                            sql = @"UPDATE QMA0101 
//                        SET IDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        IBAN='" + ROW["WBAN"] + "',ISHT='" + ROW["WSHT"] + "',INAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                        ITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        CIDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        CIBAN='" + ROW["WBAN"] + "',CISHT='" + ROW["WSHT"] + "',CINAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                        CITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        CCOD='" + str[1] + "',AYN='C',COD='" + str[1] + @"'
//				        WHERE DIV='1' AND FAC='" + FAC + "' AND BARCODE='" + str[0] + "'";
//                        }
//                        //PIDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),PIBAN='" + ROW["WBAN"] + "',PISHT='" + ROW["WSHT"] + "',PINAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',PITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                    }
//                    else
//                    {
//                        //登记修理病相
//                        if (str[4] == "1")//不良
//                        {
//                            sql = @"INSERT INTO QMA0101(
//                            ID,FAC,DIV,IBAN, 
//                            IDAT,
//                            ISHT,INAM,COD,
//                            BIDAT,
//                            ITIM,
//                            BIBAN,BISHT,BINAM,BUITNBR,
//                            BITIM,
//                            BUITDSC,CUITNBR,CUITDSC,ACOD,BUMCH,
//                            BUDAT,
//                            BUTIM,
//                            BUSHT,BUBAN,BUNAM,CUMCH,
//                            CUDAT,CUTIM,PYN,
//                            CUSHT,CUBAN, LR,BARCODE,AYN, 
//                            STWT, REWT,MODCOD, SLECOD,STATE,
//                            PIDAT,
//                            PIBAN,PISHT,PINAM,PITIM)
//                            VALUES 
//                            (SYS_GUID(),'" + FAC + "','1','" + ROW["WBAN"] + @"',
//                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "','" + str[1] + @"',
//                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            '" + ROW["WBAN"] + "','" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "','" + dt.Rows[0]["BUITNBR"] + @"',
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            '" + dt.Rows[0]["BUITDSC"] + "', '" + dt.Rows[0]["CUITNBR"] + "','" + dt.Rows[0]["CUITDSC"] + "', '" + str[1] + "','" + dt.Rows[0]["BUMCH"] + @"',
//                            to_date('" + Convert.ToDateTime(dt.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            to_date('" + Convert.ToDateTime(dt.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            '" + dt.Rows[0]["BUSHT"] + "',  '" + dt.Rows[0]["BUBAN"] + "',  '" + dt.Rows[0]["BUNAM"] + "', '" + dt.Rows[0]["CUMCH"] + @"',
//                            " + CUDAT + "," + CUTIM + @",'N',
//                            '" + dt.Rows[0]["CUSHT"] + "',  '" + dt.Rows[0]["CUBAN"] + "', '" + dt.Rows[0]["LR"] + "','" + str[0] + @"','B',
//                            '" + dt.Rows[0]["SEWT"] + "',  '" + dt.Rows[0]["REWT"] + "', '" + dt.Rows[0]["MODCOD"] + "',  '" + dt.Rows[0]["SLECOD"] + @"', '" + dt.Rows[0]["CUSTATE"] + @"',
//                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            '" + ROW["WBAN"] + "', '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'))";
//                        }
//                        else if (str[4] == "2")//合格
//                        {
//                        }
//                        else if (str[4] == "3")//报废
//                        {
//                            sql = @"INSERT INTO QMA0101(
//                            ID,FAC,DIV,IBAN, 
//                            IDAT,
//                            ISHT,INAM,COD,
//                            BIDAT,
//                            ITIM,
//                            BIBAN,BISHT,BINAM,BUITNBR,
//                            BITIM,
//                            BUITDSC,CUITNBR,CUITDSC,CCOD,BUMCH,
//                            BUDAT,
//                            BUTIM,
//                            BUSHT,BUBAN,BUNAM,CUMCH,
//                            CUDAT,CUTIM,PYN,
//                            CUSHT,CUBAN, LR,BARCODE,AYN, 
//                            STWT, REWT,MODCOD, SLECOD,STATE,
//                            PIDAT,
//                            PIBAN,PISHT,PINAM,PITIM)
//                            VALUES 
//                            (SYS_GUID(),'" + FAC + "','1','" + ROW["WBAN"] + @"',
//                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "','" + str[1] + @"',
//                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            '" + ROW["WBAN"] + "','" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + "','" + dt.Rows[0]["BUITNBR"] + @"',
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            '" + dt.Rows[0]["BUITDSC"] + "', '" + dt.Rows[0]["CUITNBR"] + "','" + dt.Rows[0]["CUITDSC"] + "', '" + str[1] + "','" + dt.Rows[0]["BUMCH"] + @"',
//                            to_date('" + Convert.ToDateTime(dt.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            to_date('" + Convert.ToDateTime(dt.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                            '" + dt.Rows[0]["BUSHT"] + "',  '" + dt.Rows[0]["BUBAN"] + "',  '" + dt.Rows[0]["BUNAM"] + "', '" + dt.Rows[0]["CUMCH"] + @"',
//                            " + CUDAT + "," + CUTIM + @",'N',
//                            '" + dt.Rows[0]["CUSHT"] + "',  '" + dt.Rows[0]["CUBAN"] + "', '" + dt.Rows[0]["LR"] + "','" + str[0] + @"','C',
//                            '" + dt.Rows[0]["SEWT"] + "',  '" + dt.Rows[0]["REWT"] + "', '" + dt.Rows[0]["MODCOD"] + "',  '" + dt.Rows[0]["SLECOD"] + @"', '" + dt.Rows[0]["CUSTATE"] + @"',
//                            to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                            '" + ROW["WBAN"] + "', '" + ROW["WSHT"] + "','" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                            to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'))";
//                        }
//                    }
//                    //LTA0001
//                    if (str[4] == "1")//不良
//                        sql1 = "UPDATE LTA0001 SET WGRES='B' WHERE BARCODE='" + str[0] + "'";
//                    else if (str[4] == "2")//合格
//                        sql1 = "UPDATE LTA0001 SET WGRES='A' WHERE BARCODE='" + str[0] + "'";
//                    else if (str[4] == "3")//报废
//                        sql1 = "UPDATE LTA0001 SET WGRES='C' WHERE BARCODE='" + str[0] + "'";
//                    List<string> list = new List<string>();
//                    if (!string.IsNullOrEmpty(sql))
//                        list.Add(sql);
//                    if (!string.IsNullOrEmpty(sql1))
//                        list.Add(sql1);
//                    //更新MES表
//                    int i = db.TranNonQuery(list.ToArray());
//                    if (i > 0)
//                        return "1";
//                    else
//                        return "6";
//                }
//                else
//                {
//                    return "2";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "3";
//            }
//        }

//        [HttpGet]
//        [Route("api/B_Juade_BAD")]
//        public DataTable B_Juade_BAD(string[] str)
//        {
//            //0 条码

//            IDataBase db = new OracleBase();
//            //DataRow ROW = SHIFT.GetShift(DateTime.Now);

//            try
//            {
//                string strSql = @"SELECT A.*,B.BNAM,B.OTHDIV  
//                          FROM QMA0101 A LEFT JOIN QMA0001 B ON A.ACOD=B.BCOD
//                          WHERE A.DIV='1' AND B.DIV='1' AND A.AYN='B' AND A.PYN='N' 
//                          AND A.BARCODE='" + str[0] + "'";
//                DataTable dt = db.GetDataTable(strSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }

//        /// <summary>
//        /// 外观修理
//        /// </summary>
//        /// <returns>1成功 2失败：条码不存在 3.未知错误 4.员工不存在 5.此条码已判废，不可再登记修理 6.登记失败</returns>
//        [HttpGet]
//        [Route("api/B_REPAIR_RESULT")]
//        public string B_REPAIR_RESULT(string[] str)
//        {
//            //0 条码
//            //1 外观 CODE
//            //2 修理人工号
//            //3 1.热补 2.打磨(默认) 3热补+打磨

//            IDataBase db = new OracleBase();
//            DataRow ROW = SHIFT.GetShift(DateTime.Now);
//            DataTable dt1 = new DataTable();
//            string sql = "";
//            string sql1 = "";
//            string PITIM = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//            try
//            {
//                string sqlEMP = "SELECT * FROM LSFW_EMPLOYEE WHERE LOGINNAME='" + str[2] + "' AND FAC='" + FAC + "'";
//                DataTable dtEMP = db.GetDataTable(sqlEMP);
//                if (dtEMP.Rows.Count == 0)
//                    return "4";

//                string dbdt = @"SELECT 
//                          A.BARCODE,CUDAT ,CUTIM, CUSHT , CUBAN, CUMCH , LR, C.ITNBR AS CUITNBR , C.ITDSC AS CUITDSC, STATE, CUSTATE,
//                          MODCOD, SLECOD, WGRES , UFRES, DBRES, XRES, SDSRES, UFCNT, DBCNT, CUSQTY, 
//                          CUDQTY , CUBQTY, CUIQTY , CUSMYN , BUITNBR, BUITDSC ,BUDAT , BUTIM, BUSHT , BUBAN , 
//                          BUMCH , BUNAM , WYYN , SEWT , REWT, BUSQTY , BUDQTY , BUBQTY , BUIQTY , BUSMYN ,
//                          TRLOTID , SWLOTID , BDLOTID, SBLOTID1, SBLOTID2, CCLOTID1 , CCLOTID2, ILLOTID, CPLOTID, 
//                          BARLAT , BARANG, BARLR, CUCOTCOD, BUCOTCOD , BOMVER, NORMALYN
//                          FROM 
//                          LTA0001 A
//                          LEFT JOIN STG0005 B
//                          ON A.BARCODE=B.BARCODE
//                          LEFT JOIN (SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1) C
//                          ON B.ITNBR = C.ITNBR
//                          WHERE A.FAC='" + FAC + "' AND A.BARCODE='" + str[0] + "'";
//                DataTable dt = db.GetDataTable(dbdt);
//                if (dt.Rows.Count > 0)
//                {
//                    string CUTIM = "";
//                    string CUDAT = "";
//                    if (string.IsNullOrEmpty(dt.Rows[0]["CUTIM"].ToString()))
//                        CUTIM = "NULL";
//                    else
//                        CUTIM = "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-MM-dd hh24:mi:ss')";

//                    if (string.IsNullOrEmpty(dt.Rows[0]["CUDAT"].ToString()))
//                        CUDAT = "NULL";
//                    else
//                        CUDAT = "to_date('" + Convert.ToDateTime(dt.Rows[0]["CUDAT"]).ToString("yyyy-MM-dd") + "','yyyy-MM-dd')";

//                    string pirepdiv = "";
//                    DataTable dtt = db.GetDataTable("SELECT * FROM QMA0103 WHERE DIV='1' AND BARCODE='" + str[0] + "'");
//                    int pirepd = dtt.Rows.Count + 1;
//                    pirepdiv = pirepd.ToString();

//                    sql = @"UPDATE QMA0101 
//                        SET IDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        IBAN='" + ROW["WBAN"] + "',ISHT='" + ROW["WSHT"] + "',INAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                        ITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        PIDAT=to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        PIBAN='" + ROW["WBAN"] + "',PISHT='" + ROW["WSHT"] + "',PINAM='" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                        PITIM=to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        BCOD='" + str[1] + "',AYN='B',COD='" + str[1] + @"'
//				        WHERE DIV='1' AND FAC='" + FAC + "' AND BARCODE='" + str[0] + "'";

//                    //插入一条修理履历
//                    sql1 = @"INSERT INTO QMA0103( ID,FAC,DIV,PIBAN,PISHT,
//                        PIDAT,
//                        PITIM,
//                        PINAM,BUITNBR,
//                        PITIMEND,
//                        BUITDSC,CUITNBR,CUITDSC,BCOD,
//                        BUMCH,
//                        BUDAT,
//                        BUTIM,
//                        BUSHT,BUBAN,BUNAM,
//                        CUMCH,CUDAT,CUTIM,CUSHT,
//                        BARCODE,AYN,PYN,RWAY,STWT,REWT,
//                        MODCOD,
//                        SLECOD,DEPCOD,ENAM,
//                        ETIM,
//                        STATE,PIREPDIV)
//                        VALUES 
//                        ('" + Guid.NewGuid() + "','" + FAC + "', '1','" + ROW["WBAN"] + "','" + ROW["WSHT"] + @"', 
//                        to_date('" + Convert.ToDateTime(ROW["WDATE"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        to_date('" + PITIM + @"','yyyy-MM-dd hh24:mi:ss') ,
//                         '" + dtEMP.Rows[0]["NAME"].ToString() + @"','" + dt.Rows[0]["BUITNBR"].ToString() + @"',
//                        to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                         '" + dt.Rows[0]["BUITDSC"].ToString() + "','" + dt.Rows[0]["CUITNBR"].ToString() + "' ,'" + dt.Rows[0]["CUITDSC"].ToString() + "','" + str[1] + @"', 
//                         '" + dt.Rows[0]["BUMCH"].ToString() + @"',
//                        to_date('" + Convert.ToDateTime(dt.Rows[0]["BUDAT"]).ToString("yyyy-MM-dd") + @"','yyyy-MM-dd'),
//                        to_date('" + Convert.ToDateTime(dt.Rows[0]["BUTIM"]).ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'),
//                        '" + dt.Rows[0]["BUSHT"].ToString() + @"','" + dt.Rows[0]["BUBAN"].ToString() + "','" + dt.Rows[0]["BUNAM"].ToString() + @"',
//                         '" + dt.Rows[0]["CUMCH"].ToString() + "'," + CUDAT + "," + CUTIM + ",'" + dt.Rows[0]["CUSHT"].ToString() + @"',
//                         '" + str[0] + "','B','Y','" + str[3] + "','" + dt.Rows[0]["SEWT"].ToString() + "','" + dt.Rows[0]["REWT"].ToString() + @"',
//                         '" + dt.Rows[0]["MODCOD"].ToString() + @"',
//                         '" + dt.Rows[0]["SLECOD"].ToString() + @"','','" + dtEMP.Rows[0]["NAME"].ToString() + @"',
//                         to_date('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"','yyyy-MM-dd hh24:mi:ss'), 
//                         '" + dt.Rows[0]["CUSTATE"].ToString() + "','" + pirepdiv + "' )";
//                    List<string> list = new List<string>();
//                    list.Add(sql);
//                    list.Add(sql1);
//                    //更新MES表
//                    int i = db.TranNonQuery(list.ToArray());
//                    if (i > 0)
//                        return "1";
//                    else
//                        return "6";
//                }
//                else
//                {
//                    return "2";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "3";
//            }
//        }
//        [HttpGet]
//        [Route("api/GetProductInfoByLotid")]
//        public string GetProductInfoByLotid(string sLotid)
//        {
//            IDataBase db = new OracleBase();
//            string sProdInfo = string.Empty;
//            try
//            {
//                string sSql = "select ITNBR,ITDSC from STB0002 where LOTNO = '" + sLotid + "'";
//                DataTable dtProd = db.GetDataTable(sSql);
//                if (dtProd.Rows.Count > 0)
//                {
//                    sProdInfo = dtProd.Rows[0]["ITNBR"].ToString() + "^" + dtProd.Rows[0]["ITDSC"].ToString();
//                }
//                return sProdInfo;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "Err-" + ex;
//            }
//        }
//        #endregion

//        #region 外检判定查询
     

    

//        [HttpGet]
//        [Route("api/GetDBSSJInfo")]
//        public DataTable GetDBSSJInfo(string sBarcode)
//        {
//            IDataBase db = new OracleBase();
//            //DataRow ROW = SHIFT.GetShift(DateTime.Now);
//            DataTable dt = null;
//            try
//            {
//                string sSql = string.Empty;
//                sSql = "SELECT A.BARCODE, ";
//                sSql += "               A.DIV, ";
//                sSql += "               A.CUITNBR, ";
//                sSql += "               A.CUITDSC, ";
//                sSql += "               A.BIDAT, ";
//                sSql += "               A.BITIM, ";
//                sSql += "               A.ACOD, ";
//                sSql += "              B.BNAM ";
//                sSql += "          FROM QMA0101 A, QMA0001 B ";
//                sSql += "        WHERE     A.ACOD = B.BCOD(+) ";
//                sSql += "              AND A.AYN = 'B' ";
//                sSql += "              AND A.DIV IN ('2', '3') ";
//                sSql += "             AND BARCODE = '" + sBarcode + "'";
//                sSql += "             AND A.ACOD IN ('57-1')";//技术部提供了1个不良代码，只有这1个需要刷胶浆
//                dt = db.GetDataTable(sSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }

//        //txtBarcode1.Text, txtITNBR.Text, txtITDSC.Text, txtUFPTim.Text, txtUFBadCod.Text, txtUFBadNam.Text, txtDBPTim.Text, txtDBBadCod.Text, txtDBBadNam.Text
   

   
//        [HttpGet]
//        [Route("api/Get_BADYN")]
//        public string Get_BADYN(string code)
//        {
//            string sql = "";

//            try
//            {
//                IDataBase db = new OracleBase();
//                SHIFT.GetSht();
//                sql = "select * from QMB0102 where BARCODE ='" + code + "'";
//                DataTable dt = db.GetDataTable(sql);
//                if (dt != null && dt.Rows.Count > 0)
//                {
//                    return "1";
//                }
//                else
//                {
//                    return "0";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return sql;
//            }
//        }


//        [HttpGet]
//        [Route("api/Get_BADTEXT")]
//        public string Get_BADTEXT(string code)
//        {
//            string sql = "";
//            try
//            {
//                IDataBase db = new OracleBase();
//                SHIFT.GetSht();
//                sql = "select * from STG0003 where BARCODE ='" + code + "' order by wtdat ";
//                DataTable dt = db.GetDataTable(sql);
//                if (dt != null && dt.Rows.Count > 0)
//                {
//                    return dt.Rows[0]["STATE"].ToString();
//                }
//                else
//                {
//                    return "0";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return sql;
//            }
//        }



//        #endregion




//        #region 首模登记/判定

//        [HttpGet]
//        [Route("api/GetModPlanCodInst")]
//        public DataTable GetModPlanCodInst(string sMchid, string sLR)
//        {
//            IDataBase db = new OracleBase();
//            DataRow row = SHIFT.GetShift(DateTime.Now);
//            try
//            {
//                //string sql = "select PLANID from MDD0003 where PLANSTATUS ='2' and wdate = to_date('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD')  and mchid = '" + sMchid + "' and LRFLAG = '" + sLR + "'order by PLANID";
//                string sql = "select PLANID from MDD0003 where PLANSTATUS ='2'  and mchid = '" + sMchid + "' and LRFLAG = '" + sLR + "'order by PLANID";
//                DataTable dt = db.GetDataTable(sql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }

     
    
   
  
    
    
   
   

//        [HttpGet]
//        [Route("api/GetProdInfoByProdCod")]
//        public DataTable GetProdInfoByProdCod(string sITNBR)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sSql = "SELECT * FROM (SELECT A.*,ROW_NUMBER () OVER (PARTITION BY ITNBR ORDER BY TO_NUMBER (VERSION) DESC) RN FROM EDB0010 A) WHERE RN = 1 AND ITNBR = '" + sITNBR + "'";
//                DataTable dt = db.GetDataTable(sSql);
//                return dt;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }

//        /// <summary>
//        /// 计划上机
//        /// </summary>
//        /// <param name="sPlanCod">计划号</param>
//        /// <param name="sUserNam">用户名</param>
//        /// <param name="sDIV">计划类别</param>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("api/InstModToMch")]
//        public string InstModToMch(string sPlanCod, string sUserNam, string sDIV)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sSts = string.Empty;
//                sSts = sDIV == "3" ? "5" : "3";//若模具计划为：3-干冰清洗 则直接完成
//                string sSql = "UPDATE MDD0003 SET PLANSTATUS = '" + sSts + "',STIM = SYSDATE,SNAM = '" + sUserNam + "'   WHERE PLANID = '" + sPlanCod + "'";
//                int iRes = db.ExecuteNonQuery(sSql);
//                if (iRes > 0)
//                    return "OK";
//                else
//                    return "ERR";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "ERR";
//            }
//        }

//        #endregion



//        #region 模具上机
     
    
   
       
   
//        [HttpGet]
//        [Route("api/Send_SGRES")]
//        public string Send_SGRES(string PLANID, string RESULT)
//        {
//            IDataBase db = new OracleBase(0, 0, 0);
//            try
//            {
//                // 查询组模ID
//                DataTable dtZUMUID = mouldController.GetZUMUID(PLANID);
//                string ZMID = "0";
//                if (dtZUMUID != null && dtZUMUID.Rows.Count > 0)
//                {
//                    ZMID = dtZUMUID.Rows[0]["ZMID"].ToString();
//                }
//                int iZMID = Convert.ToInt32(ZMID);
//                string sSql = @"INSERT INTO MES_FistCheck (ID,PlanID,ZMID,Result,createTime,IS_Read,SOURCES) VALUES
//                (SEQ_MES_FistCheck.Nextval,'" + PLANID + "'," + iZMID + ",'" + RESULT + "',SYSDATE,'N','HY')";

//                int iResult = db.ExecuteNonQuery(sSql);
//                if (iResult > 0)
//                    return "OK";
//                else
//                    return "发送MM系统首罐信息失败！";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "EX";
//            }
//        }

//        [HttpGet]
//        [Route("api/Send_SGRESXX")]
//        public string Send_SGRESXX(string PLANID, int DICCODE, string DICVALUE, string RESULT)
//        {
//            IDataBase db = new OracleBase(0, 0, 0);
//            try
//            {
//                // 查询组模ID
//                DataTable dtZUMUID = mouldController.GetZUMUID(PLANID);

//                string sql = "SELECT * FROM MES_FistCheck WHERE  PlanID = '" + PLANID + "' ORDER BY createTime DESC";
//                DataTable dtFistCheck = db.GetDataTable(sql);

//                if (dtFistCheck != null && dtFistCheck.Rows.Count > 0)
//                {
//                    int iFID = Convert.ToInt32(dtFistCheck.Rows[0]["ID"].ToString());
//                    string ZMID = "0";
//                    if (dtZUMUID != null && dtZUMUID.Rows.Count > 0)
//                    {
//                        ZMID = dtZUMUID.Rows[0]["ZMID"].ToString();
//                    }
//                    int iZMID = Convert.ToInt32(ZMID);
//                    string sSql = @"INSERT INTO MES_CheckResult (FistID,ID,PlanID,ZMID,DicCode,DicValue,Result,createTime,IS_Read,SOURCES) VALUES
//                (" + iFID + ",SEQ_MES_CheckResult.Nextval,'" + PLANID + "'," + iZMID + "," + DICCODE + ",'" + DICVALUE + "','" + RESULT + "',SYSDATE,'N','HY')";

//                    int iResult = db.ExecuteNonQuery(sSql);
//                    if (iResult > 0)
//                        return "OK";
//                    else
//                        return "发送MM系统首罐详细信息失败！";
//                }
//                else
//                {
//                    return "";
//                }


//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "EX";
//            }
//        }




//        #endregion

//        #region 胶囊管理
//        [HttpGet]
//        [Route("api/GetBldLotInfo")]
//        public DataTable GetBldLotInfo(string sLotid)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                return db.GetDataTable("SELECT * FROM PPE0007 WHERE LOTNO = '" + sLotid + "'");
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }

//        [HttpGet]
//        [Route("api/BladderOut")]
//        public string BladderOut(string sLotid, string sITNBR, string sQTY, string sName, string sPlanId)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                DataTable dt = db.GetDataTable(@"SELECT * FROM PPE0010 WHERE PLANSTATUS='1' AND PLANID='" + sPlanId + "'");
//                if (dt.Rows.Count > 0)
//                {
//                    DataRow row = SHIFT.GetShift(DateTime.Now);
//                    List<string> sqllist = new List<string>();
//                    //录入出库信息
//                    string sInsSql = "INSERT INTO PPE0008 (ID,FAC,LOTNO,WDATE,WSHT,ECODE,QTY,ONAM,OTIM,PLANID) VALUES (";
//                    sInsSql += "sys_guid(),";
//                    sInsSql += "'07',";
//                    sInsSql += "'" + sLotid + "',";
//                    sInsSql += "to_date('" + Convert.ToDateTime(row["WDATE"]).ToString("yyyy-MM-dd") + "','YYYY-MM-DD'),";
//                    sInsSql += "'" + row["WSHT"].ToString() + "',";
//                    sInsSql += "'" + sITNBR + "',";
//                    sInsSql += "'" + sQTY + "',";
//                    sInsSql += "'" + sName + "',";
//                    sInsSql += "sysdate,";
//                    sInsSql += "'" + sPlanId + "'";
//                    sInsSql += ")";
//                    sqllist.Add(sInsSql);

//                    //该批次库存数量中减掉已出库的数量
//                    string sUpdSql = "UPDATE PPE0007 SET INVQTY = INVQTY- " + sQTY + " WHERE LOTNO = '" + sLotid + "'";
//                    sqllist.Add(sUpdSql);

//                    //将该计划状态变更为已出库
//                    string sUpdSql1 = "UPDATE PPE0010 SET PLANSTATUS = '2' WHERE PLANID = '" + sPlanId + "'";
//                    sqllist.Add(sUpdSql1);

//                    int iRes = db.TranNonQuery(sqllist.ToArray());
//                    if (iRes > 0)
//                        return "OK";
//                    else
//                        return "NG";
//                }
//                else
//                {
//                    return "FH";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return "EX";
//            }

//        }

//        [HttpGet]
//        [Route("api/GetBldPlanInfo")]
//        public DataTable GetBldPlanInfo(string sPLANID)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                return db.GetDataTable("SELECT * FROM PPE0010 WHERE PLANID = '" + sPLANID + "'");
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);





//                return null;
//            }
//        }



      
  
//        [HttpGet]
//        [Route("api/GetBARFG")]
//        public DataTable GetBARFG(string barcode)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                //string strSQL = @"SELECT * FROM SDA0025 WHERE ITNBR = '" + sERPITNBR + "' AND PDDJ = '" + sGRD + "' AND DBRESULT = '" + dbGrad + "' AND UFRESULT = '" + ufGrad + "'";
//                //LogLog.Warn(GetType(),strSQL);
//                string sqlQMA0103 = @"select COUNT(*) XLQTY from (
//                                                SELECT * FROM qma0103 WHERE DIV IN ('1') AND RNAM IS NOT NULL AND BARCODE = '" + barcode + @"'
//                                                UNION ALL
//                                                SELECT * FROM qma0103 WHERE DIV IN ('2','3') AND ZJQRNAM IS NOT NULL AND BARCODE = '" + barcode + @"'
//                                                )";
//                return db.GetDataTable(sqlQMA0103);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }


//        [HttpGet]
//        [Route("api/GetBldPlanList")]
//        public DataTable GetBldPlanList()
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                return db.GetDataTable("SELECT * FROM PPE0010 WHERE PLANSTATUS='1'");
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);

//                return null;
//            }
//        }



//        #endregion
//        #region 成品胎物流处理

     
//        [HttpGet]
//        [Route("api/ReSendJDXX")]
//        public string ReSendJDXX(string barcode)
//        {
//            string sInsWms = "";
//            IDataBase db = new OracleBase();
//            IDataBase WMSDB = new OracleBase(1, 2);
//            DataTable DT = new DataTable();
//            try
//            {
//                string sLotInfoSql = "SELECT AAA.*,BBB.RFIDTAG  FROM ( ";
//                sLotInfoSql = "SELECT * ";
//                sLotInfoSql += "  FROM (SELECT AB.*, CD.ATTRVAL, ROWNUM RN ";
//                sLotInfoSql += "          FROM (SELECT B.BARCODE, ";
//                sLotInfoSql += "                       B.CUITNBR, ";
//                sLotInfoSql += "                       B.CUITDSC, ";
//                sLotInfoSql += "                       NVL (B.CUGRADE, '606') CUGRADE, ";
//                sLotInfoSql += "                       CUSMYN, ";
//                sLotInfoSql += "                       B.CUTIM, ";
//                sLotInfoSql += "                       C.SPEC, ";
//                sLotInfoSql += "                       B.BUITNBR ITNBR, ";
//                sLotInfoSql += "                       ERPITNBR ";
//                sLotInfoSql += "                  FROM  LTA0001 B, ";
//                sLotInfoSql += "                       (SELECT * ";
//                sLotInfoSql += "                          FROM (SELECT A.*, ";
//                sLotInfoSql += "                                       ROW_NUMBER () ";
//                sLotInfoSql += "                                       OVER ( ";
//                sLotInfoSql += "                                          PARTITION BY ITNBR ";
//                sLotInfoSql += "                                          ORDER BY TO_NUMBER (VERSION) DESC) ";
//                sLotInfoSql += "                                          RN ";
//                sLotInfoSql += "                                  FROM EDB0010 A ";
//                sLotInfoSql += "                                 WHERE ITGRPCOD = 'FERT') ";
//                sLotInfoSql += "                         WHERE RN = 1) C ";
//                sLotInfoSql += "                 WHERE  B.CUITNBR = C.ITNBR(+) ";
//                sLotInfoSql += "                 AND B.BARCODE = '" + barcode + "') AB, ";
//                sLotInfoSql += "               (SELECT ITNBR, ATTRVAL ";
//                sLotInfoSql += "                  FROM (SELECT * ";
//                sLotInfoSql += "                          FROM (SELECT A.*, ";
//                sLotInfoSql += "                                       ROW_NUMBER () ";
//                sLotInfoSql += "                                       OVER ( ";
//                sLotInfoSql += "                                          PARTITION BY ITNBR ";
//                sLotInfoSql += "                                          ORDER BY TO_NUMBER (VERSION) DESC) ";
//                sLotInfoSql += "                                          RN ";
//                sLotInfoSql += "                                  FROM EDB0010 A ";
//                sLotInfoSql += "                                 WHERE ITGRPCOD = 'HALB') ";
//                sLotInfoSql += "                         WHERE RN = 1) A, ";
//                sLotInfoSql += "                       EDB0015 B ";
//                sLotInfoSql += "                 WHERE     A.ID = B.ITEMID(+) ";
//                sLotInfoSql += "                       AND A.ITTYPECOD = 'GT' ";
//                sLotInfoSql += "                       AND B.ATTRCOD(+) = 'M18') CD ";
//                sLotInfoSql += "         WHERE AB.ITNBR = CD.ITNBR(+))   )AAA  ";
//                sLotInfoSql += "  LEFT JOIN (SELECT BARCODE,RFIDTAG FROM  LTA0006 ) BBB   ";
//                sLotInfoSql += "  ON AAA.BARCODE =BBB.BARCODE";
//                /*	LEFT JOIN (SELECT BARCODE,RFIDTAG FROM  LTA0006 ) BBB
//                                                ON AAA.BARCODE =BBB.BARCODE */

//                DT = db.GetDataTable(sLotInfoSql);
//                if (DT != null && DT.Rows.Count > 0)
//                {
//                    string sORGANIZATION_ID = string.Empty;//组织ID 默认83
//                    string sTIRE_BARCODE = string.Empty;//条码号
//                    string sTIRE_NUMBER = string.Empty;//胎号
//                    string sITEM_NUM = string.Empty;//物料代码
//                    string sITEM_DESC = string.Empty;//物料描述
//                    string sWEEK_NO = string.Empty;//周牌号
//                    string sOFFLINE_BASE = string.Empty;//下线基地
//                    string sPRODUCTION_DATE = string.Empty;//生产日期
//                    string sSPECI_MODELS = string.Empty;//规格型号
//                    string sWORKSHOP_CODE = string.Empty;//生产车间
//                    string sPRODUCTION_TYPE = string.Empty;//产品类别
//                    string sACTUAL_WEIGHT = string.Empty;//胎胚实绩重量
//                    string sTIRE_STATUS = string.Empty;//DI-正品 FP:废次品 BL:冻结
//                    string sKIND = string.Empty;//轮胎性质，华阳一级，二级
//                    string sREWORK_FLAG = string.Empty;//返修标志  默认1-正常
//                    string sSTOCK_DIV = string.Empty;//仓库类别，默认空
//                    string sRFIDTAG = string.Empty;//轮胎芯片

//                    int iRst = 0;
//                    foreach (DataRow dr in DT.Rows)
//                    {
//                        sORGANIZATION_ID = "83";
//                        sTIRE_BARCODE = dr["BARCODE"].ToString();
//                        sTIRE_NUMBER = "N/A";
//                        //由于EDB0010.ITNBR字段属于华阳MES内部管理的物料代码，这个地方需要把ERP代码接口到物流园
//                        sITEM_NUM = dr["ERPITNBR"].ToString();
//                        if (!string.IsNullOrEmpty(dr["CUITDSC"].ToString()) && dr["CUITDSC"].ToString().Contains("试验胎"))//若包含“试验胎”，则去除掉
//                            sITEM_DESC = dr["CUITDSC"].ToString().Substring(0, dr["CUITDSC"].ToString().IndexOf("试验胎") - 1);
//                        else
//                            sITEM_DESC = dr["CUITDSC"].ToString();
//                        if (!string.IsNullOrEmpty(dr["CUTIM"].ToString()))
//                        {
//                            DateTime dt1 = Convert.ToDateTime(dr["CUTIM"].ToString());
//                            sWEEK_NO = qualityTestingController.weekno(dt1);
//                        }
//                        else
//                            sWEEK_NO = "0000";
//                        sOFFLINE_BASE = "07";
//                        if (!string.IsNullOrEmpty(dr["CUTIM"].ToString()))
//                        {
//                            sPRODUCTION_DATE = Convert.ToDateTime(dr["CUTIM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
//                        }
//                        else
//                            sPRODUCTION_DATE = "2000-01-01 01:01:01";

//                        sSPECI_MODELS = dr["SPEC"].ToString();
//                        sWORKSHOP_CODE = "HYBG";
//                        sPRODUCTION_TYPE = "CB";//CB-半钢胎
//                        sACTUAL_WEIGHT = dr["ATTRVAL"].ToString();
//                        //sTIRE_STATUS = "合格";
//                        //DI-合格 FP-报废 BL-封存
//                        sTIRE_STATUS = "DI";
//                        sKIND = dr["CUGRADE"].ToString();
//                        sREWORK_FLAG = "1";
//                        sSTOCK_DIV = "";
//                        sRFIDTAG = dr["RFIDTAG"].ToString();
//                        sInsWms = "INSERT INTO OFFLINE_BASE_INFO_TO_WMS(OFFLINE_ID,ORGANIZATION_ID,TYRE_BARCODE,TYRE_NUM,ITEM_NUM,ITEM_DESC,DOT,OFFLINE_BASE,PRODUCTION_DATE,SPECI_MODELS,";
//                        sInsWms += "WORKSHOP_CODE,PRODUCTION_TYPE,ACTUAL_WEIGHT,ATTRIBUTE_CATEGORY,KIND,REWORK_FLAG,STOCK_DIV,MES_CREATE_TIME,WMS_HANDLE_TIME,WMS_HANDLE_STATE,WMS_HANDLE_MSG,TYRE_RFID) VALUES (";
//                        sInsWms += "SYS_GUID()," + sORGANIZATION_ID + ",'" + sTIRE_BARCODE + "','" + sTIRE_NUMBER + "','" + sITEM_NUM + "','" + sITEM_DESC + "','" + sWEEK_NO + "','" + sOFFLINE_BASE + "',TO_DATE('" + sPRODUCTION_DATE + "','YYYY-MM-DD HH24:MI:SS'),";
//                        sInsWms += "'" + sSPECI_MODELS + "','" + sWORKSHOP_CODE + "','" + sPRODUCTION_TYPE + "'," + sACTUAL_WEIGHT + ",'" + sTIRE_STATUS + "','" + sKIND + "','" + sREWORK_FLAG + "','" + sSTOCK_DIV + "',SYSDATE,'','1','','" + sRFIDTAG + "')";

//                        iRst = WMSDB.ExecuteNonQuery(sInsWms);



//                        #region 待注释
//                        //List<string> ListSendInfo = new List<string>();
//                        //ListSendInfo.Add("N/A" + "," + sTIRE_BARCODE + "," + sTIRE_NUMBER + "," + sITEM_NUM + "," + sWEEK_NO + "," + sOFFLINE_BASE + "," + sPRODUCTION_DATE + "," + sSPECI_MODELS + "," + sWORKSHOP_CODE + "," + sPRODUCTION_TYPE + "," + sACTUAL_WEIGHT + "," + sTIRE_STATUS);
//                        //WebServiceHelper web = new WebServiceHelper();
//                        //string[] strBack = web.QueryGetWebService(ListSendInfo.ToArray());
//                        #endregion

//                    }
//                    if (iRst > 0)
//                        return "OK";
//                    else
//                        return "Err:发送基地下线表失败！";
//                }
//                else
//                {
//                    return "Err:条码无相关生产信息，不能重新上线";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "Err:" + ex;
//            }
//        }


   
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sBarcode"></param>
//        /// <param name="sITNBR"></param>
//        /// <param name="sITDSC"></param>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("api/FTTireWLOnline")]
//        public string FTTireWLOnline(string barcode, string sITNBR, string sITDSC, string sNam, string sReason)
//        {
//            IDataBase db = new OracleBase();
//            IDataBase IFDB = new OracleBase("");
//            IDataBase WMSDB = new OracleBase(1, 2);
//            DataTable DT = new DataTable();
//            string sInsWms = "";
//            try
//            {
//                DateTime dtNow = DateTime.Now;
//                DataRow row = SHIFT.GetShift(dtNow);
//                string sts = "A";
//                bool b = true;
//                string sHold = string.Empty;
//                string s = "no read";
//                string itnbr = "no read";
//                string itdsc = "no read";
//                string sGrd = string.Empty;
//                string sERPITNBR = string.Empty;
//                if (barcode.Contains("no read"))
//                    b = false;
//                if (b)
//                {
//                    //DataTable DT = MESDB.GetTable("SELECT * FROM LTA0001 WHERE BARCODE='" + barcode + "' AND CUITNBR IS NOT NULL");
//                    //string sLotInfoSql = " SELECT A.*,B.ERPITNBR FROM LTA0001 A, (SELECT * ";
//                    //sLotInfoSql += "                                     FROM (SELECT A.*,";
//                    //sLotInfoSql += "                                                  ROW_NUMBER ()";
//                    //sLotInfoSql += "                                                  OVER (";
//                    //sLotInfoSql += "                                                     PARTITION BY ITNBR";
//                    //sLotInfoSql += "                                                     ORDER BY";
//                    //sLotInfoSql += "                                                        TO_NUMBER (VERSION) DESC)";
//                    //sLotInfoSql += "                                                     RN";
//                    //sLotInfoSql += "                                             FROM EDB0010 A WHERE ITGRPCOD = 'FERT')";
//                    //sLotInfoSql += "                                    WHERE RN = 1) B";
//                    //sLotInfoSql += " WHERE A.CUITNBR = B.ITNBR(+) AND BARCODE = '" + barcode + "'";
//                    string sLotInfoSql = "SELECT AAA.*, BBB.RFIDTAG FROM  (   ";
//                    sLotInfoSql = "SELECT B.ERPITNBR, ";
//                    sLotInfoSql += "       A.BARCODE, ";
//                    sLotInfoSql += "       A.CUITNBR, ";
//                    sLotInfoSql += "       A.CUITDSC, ";
//                    sLotInfoSql += "       A.CUTIM, ";
//                    sLotInfoSql += "       A.CUGRADE, ";
//                    sLotInfoSql += "       A.CUCOTCOD, ";
//                    sLotInfoSql += "       B.SPEC, ";
//                    sLotInfoSql += "       C.ITNBR, ";
//                    sLotInfoSql += "       D.ATTRVAL ";
//                    sLotInfoSql += "  FROM LTA0001 A, ";
//                    sLotInfoSql += "       (SELECT * ";
//                    sLotInfoSql += "          FROM (SELECT A.*, ";
//                    sLotInfoSql += "                       ROW_NUMBER () ";
//                    sLotInfoSql += "                       OVER (PARTITION BY ITNBR ";
//                    sLotInfoSql += "                             ORDER BY TO_NUMBER (VERSION) DESC) ";
//                    sLotInfoSql += "                          RN ";
//                    sLotInfoSql += "                  FROM EDB0010 A ";
//                    sLotInfoSql += "                 WHERE ITGRPCOD = 'FERT') ";
//                    sLotInfoSql += "         WHERE RN = 1) B, ";
//                    sLotInfoSql += "       EDC0003 C, ";
//                    sLotInfoSql += "       (SELECT ITNBR, ATTRVAL ";
//                    sLotInfoSql += "          FROM EDB0010 A, EDB0015 B ";
//                    sLotInfoSql += "         WHERE     A.ID = B.ITEMID(+) ";
//                    sLotInfoSql += "               AND A.USEYN = 'Y' ";
//                    sLotInfoSql += "               AND A.ITTYPECOD = 'GT' ";
//                    sLotInfoSql += "               AND B.ATTRCOD(+) = 'M18') D ";
//                    sLotInfoSql += " WHERE     A.CUITNBR = B.ITNBR(+) ";
//                    sLotInfoSql += "       AND C.ITNBR = D.ITNBR(+) ";
//                    sLotInfoSql += "       AND B.ITNBR = C.PARNTITNBR(+) ";
//                    sLotInfoSql += "       AND C.USEYN(+) = 'Y' ";
//                    sLotInfoSql += "       AND BARCODE = '" + barcode + "')AAA  ";
//                    sLotInfoSql += " LEFT JOIN (SELECT BARCODE ,RFIDTAG FROM  LTA0006 )	BBB ";
//                    sLotInfoSql += " ON AAA.BARCODE=BBB.BARCODE ";
//                    /*	 LEFT JOIN (SELECT BARCODE ,RFIDTAG FROM  LTA0006 )	BBB
//                                                                     ON AAA.BARCODE=BBB.BARCODE*/


//                    DT = db.GetDataTable(sLotInfoSql);
//                    if (DT != null && DT.Rows.Count > 0)
//                    {
//                        itnbr = DT.Rows[0]["CUITNBR"].ToString();
//                        itdsc = DT.Rows[0]["CUITDSC"].ToString();
//                        sGrd = DT.Rows[0]["CUGRADE"].ToString();
//                        sERPITNBR = DT.Rows[0]["ERPITNBR"].ToString();
//                        sHold = DT.Rows[0]["CUCOTCOD"].ToString();
//                    }
//                    else
//                    {
//                        b = false;
//                        s = itnbr = "NO ITNBR";
//                        itdsc = "NO ITDSC";
//                        return "Err:条码无相关生产信息，不能重新上线";
//                    }
//                }

//                //增加判断是否有外检
//                if (b)
//                {
//                    DataTable dtVI = db.GetDataTable("SELECT COUNT(*) CNT FROM CKA0007 WHERE BARCODE='" + barcode + "' AND VIRESULT = '1'");
//                    if (dtVI.Rows[0]["CNT"].ToString() == "0")//若外检没有合格检测结果
//                    {
//                        //接着判断手持外检
//                        DataTable dtVI1 = db.GetDataTable("SELECT COUNT(*) CNT FROM QMA0003 WHERE BARCODE='" + barcode + "' AND VIRES = '1'");
//                        if (dtVI1.Rows[0]["CNT"].ToString() == "0")//若也没有合格检测结果
//                        {
//                            b = false;
//                            sts = "EW";
//                        }
//                    }
//                }
//                //增加判断有没有均匀检测
//                if (b)
//                {
//                    DataTable dtUF = db.GetDataTable("SELECT COUNT(*) CNT FROM QMA0201 WHERE BARCODE='" + barcode + "'");
//                    if (dtUF.Rows[0]["CNT"].ToString() == "0")//若均匀性没有检测结果
//                    {
//                        b = false;
//                        sts = "EJ";
//                    }
//                }
//                //增加判断有没有动平衡检测
//                if (b)
//                {
//                    DataTable dtDB = db.GetDataTable("SELECT COUNT(*) CNT FROM QMA0301 WHERE BARCODE='" + barcode + "'");
//                    if (dtDB.Rows[0]["CNT"].ToString() == "0")//若均匀性没有检测结果
//                    {
//                        b = false;
//                        sts = "ED";
//                    }
//                }


//                if (b)
//                {
//                    foreach (DataRow dr in db.GetDataTable("SELECT AYN,DIV FROM QMA0101 WHERE AYN<>'A' AND BARCODE='" + barcode + "'").Rows)
//                    {
//                        b = false;
//                        sts = dr["AYN"].ToString();
//                        //bdiv = dr["DIV"].ToString();
//                        s = barcode;
//                    }
//                }

//                if (sHold == "HOLD")
//                    sts = "H";

//                //给益联发送入库路线
//                string sSts = string.Empty;
//                if (sts == "A")
//                {
//                    sSts = "OK";
//                }
//                else if (sts == "EW")
//                {
//                    sSts = "NGEW";
//                }
//                else if (sts == "EJ")
//                {
//                    sSts = "NGEJ";
//                }
//                else if (sts == "ED")
//                {
//                    sSts = "NGED";
//                }
//                else
//                {
//                    sSts = "NG";
//                }

//                if (sSts == "NG")
//                {
//                    return "Err:轮胎不良或保留，不能重新上线";
//                }
//                if (sSts == "NGEW")
//                {
//                    return "Err:缺少外检检测信息，不能重新上线";
//                }
//                if (sSts == "NGEJ")
//                {
//                    return "Err:缺少均匀性检测信息，不能重新上线";
//                }
//                if (sSts == "NGED")
//                {
//                    return "Err:缺少动平衡检测信息，不能重新上线";
//                }
//                // 获取DB信息
//                DataTable dtDBINFO = qualityTestingController.GetDB(barcode);
//                // 获取UF信息
//                DataTable dtUFINFO = qualityTestingController.GetUF(barcode);
//                string strDBDJ = "";
//                string strUFDJ = "";
//                if (dtDBINFO != null && dtDBINFO.Rows.Count > 0)
//                {
//                    strDBDJ = dtDBINFO.Rows[0]["OVERALLGRADE"].ToString();
//                }
//                if (dtUFINFO != null && dtUFINFO.Rows.Count > 0)
//                {
//                    strUFDJ = dtUFINFO.Rows[0]["UFGRADE"].ToString();
//                }
//                string sRoutStockNo = GetRoutStockNo(sERPITNBR, sGrd, strDBDJ, strUFDJ);
//                if (string.IsNullOrEmpty(sRoutStockNo))
//                    return "Err:规格：" + sERPITNBR + "没有维护入库路线，请先维护";

//                //判断华阳入库扫描是否有相同条码正常扫描，若有则不允许上线,，防止两个表有重复
//                string sSqlChk = "SELECT BARCODE FROM SDA0013 WHERE BARCODE ='" + barcode + "' AND STS = 'A'";
//                DataTable dtchk = db.GetDataTable(sSqlChk);
//                if (dtchk != null && dtchk.Rows.Count > 0)
//                {
//                    return "Err:已正常发送给益联路线，请尝试直接上线重新分拣";
//                }

//                #region 重新发送基地下线表
//                //===============重新发送基地下线表======================================
//                List<string> ListSendInfo = new List<string>();
//                //string sWIP_NO = string.Empty;
//                //string sTIRE_BARCODE = string.Empty;
//                //string sTIRE_NUMBER = string.Empty;
//                //string sITEM_NUM = string.Empty;
//                //string sWEEK_NO = string.Empty;
//                //string sOFFLINE_BASE = string.Empty;
//                //string sPRODUCTION_DATE = string.Empty;
//                //string sSPECI_MODELS = string.Empty;
//                //string sWORKSHOP_CODE = string.Empty;
//                //string sPRODUCTION_TYPE = string.Empty;
//                //string sACTUAL_WEIGHT = string.Empty;
//                //string sTIRE_STATUS = string.Empty;

//                //sWIP_NO = "N/A";
//                //sTIRE_BARCODE = DT.Rows[0]["BARCODE"].ToString();
//                //sTIRE_NUMBER = "N/A";
//                ////sITEM_NUM = orcReader["CUITNBR"].ToString();
//                ////由于EDB0010.ITNBR字段属于华阳MES内部管理的物料代码，这个地方需要把ERP代码接口到物流园
//                //sITEM_NUM = DT.Rows[0]["ERPITNBR"].ToString();
//                //if (!string.IsNullOrEmpty(DT.Rows[0]["CUTIM"].ToString()))
//                //{
//                //    DateTime dt1 = Convert.ToDateTime(DT.Rows[0]["CUTIM"].ToString());
//                //    sWEEK_NO = weekno(dt1);
//                //}
//                //else
//                //    sWEEK_NO = "0000";
//                //sOFFLINE_BASE = "07";
//                //if (!string.IsNullOrEmpty(DT.Rows[0]["CUTIM"].ToString()))
//                //{
//                //    sPRODUCTION_DATE = Convert.ToDateTime(DT.Rows[0]["CUTIM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
//                //}
//                //else
//                //    sPRODUCTION_DATE = "0001-01-01 01:01:01";

//                //sSPECI_MODELS = DT.Rows[0]["SPEC"].ToString();
//                //sWORKSHOP_CODE = "HYBG";
//                //sPRODUCTION_TYPE = "半钢胎";
//                //sACTUAL_WEIGHT = DT.Rows[0]["ATTRVAL"].ToString();
//                ////sTIRE_STATUS = "合格";
//                ////DI-合格 FP-报废 BL-封存
//                //sTIRE_STATUS = "DI";

//                //ListSendInfo.Add(sWIP_NO + "," + sTIRE_BARCODE + "," + sTIRE_NUMBER + "," + sITEM_NUM + "," + sWEEK_NO + "," + sOFFLINE_BASE + "," + sPRODUCTION_DATE + "," + sSPECI_MODELS + "," + sWORKSHOP_CODE + "," + sPRODUCTION_TYPE + "," + sACTUAL_WEIGHT + "," + sTIRE_STATUS);
//                //WebServiceHelper web = new WebServiceHelper();
//                //string[] strBack = web.QueryGetWebService(ListSendInfo.ToArray());

//                string sORGANIZATION_ID = string.Empty;//组织ID 默认83
//                string sTIRE_BARCODE = string.Empty;//条码号
//                string sTIRE_NUMBER = string.Empty;//胎号
//                string sITEM_NUM = string.Empty;//物料代码
//                string sITEM_DESC = string.Empty;//物料描述
//                string sWEEK_NO = string.Empty;//周牌号
//                string sOFFLINE_BASE = string.Empty;//下线基地
//                string sPRODUCTION_DATE = string.Empty;//生产日期
//                string sSPECI_MODELS = string.Empty;//规格型号
//                string sWORKSHOP_CODE = string.Empty;//生产车间
//                string sPRODUCTION_TYPE = string.Empty;//产品类别
//                string sACTUAL_WEIGHT = string.Empty;//胎胚实绩重量
//                string sTIRE_STATUS = string.Empty;//DI-正品 FP:废次品 BL:冻结
//                string sKIND = string.Empty;//轮胎性质，华阳一级，二级
//                string sREWORK_FLAG = string.Empty;//返修标志  默认1-正常
//                string sSTOCK_DIV = string.Empty;//仓库类别，默认空
//                string sRFIDTAG = string.Empty;//轮胎RFID

//                foreach (DataRow dr in DT.Rows)
//                {
//                    sORGANIZATION_ID = "83";
//                    sTIRE_BARCODE = dr["BARCODE"].ToString();
//                    sTIRE_NUMBER = "N/A";
//                    //由于EDB0010.ITNBR字段属于华阳MES内部管理的物料代码，这个地方需要把ERP代码接口到物流园
//                    sITEM_NUM = dr["ERPITNBR"].ToString();
//                    if (!string.IsNullOrEmpty(dr["CUITDSC"].ToString()) && dr["CUITDSC"].ToString().Contains("试验胎"))//若包含“试验胎”，则去除掉
//                        sITEM_DESC = dr["CUITDSC"].ToString().Substring(0, dr["CUITDSC"].ToString().IndexOf("试验胎") - 1);
//                    else
//                        sITEM_DESC = dr["CUITDSC"].ToString();
//                    if (!string.IsNullOrEmpty(dr["CUTIM"].ToString()))
//                    {
//                        DateTime dt1 = Convert.ToDateTime(dr["CUTIM"].ToString());
//                        sWEEK_NO = qualityTestingController.weekno(dt1);
//                    }
//                    else
//                        sWEEK_NO = "0000";
//                    sOFFLINE_BASE = "07";
//                    if (!string.IsNullOrEmpty(dr["CUTIM"].ToString()))
//                    {
//                        sPRODUCTION_DATE = Convert.ToDateTime(dr["CUTIM"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
//                    }
//                    else
//                        sPRODUCTION_DATE = "2000-01-01 01:01:01";

//                    sSPECI_MODELS = dr["SPEC"].ToString();
//                    sWORKSHOP_CODE = "HYBG";
//                    sPRODUCTION_TYPE = "CB";//CB-半钢胎
//                    sACTUAL_WEIGHT = dr["ATTRVAL"].ToString();
//                    //sTIRE_STATUS = "合格";
//                    //DI-合格 FP-报废 BL-封存
//                    sTIRE_STATUS = "DI";
//                    sKIND = dr["CUGRADE"].ToString();
//                    sREWORK_FLAG = "1";
//                    sSTOCK_DIV = sRoutStockNo;

//                    sInsWms = "INSERT INTO OFFLINE_BASE_INFO_TO_WMS(OFFLINE_ID,ORGANIZATION_ID,TYRE_BARCODE,TYRE_NUM,ITEM_NUM,ITEM_DESC,DOT,OFFLINE_BASE,PRODUCTION_DATE,SPECI_MODELS,";
//                    sInsWms += "WORKSHOP_CODE,PRODUCTION_TYPE,ACTUAL_WEIGHT,ATTRIBUTE_CATEGORY,KIND,REWORK_FLAG,STOCK_DIV,MES_CREATE_TIME,WMS_HANDLE_TIME,WMS_HANDLE_STATE,WMS_HANDLE_MSG,TYRE_RFID) VALUES (";
//                    sInsWms += "SYS_GUID()," + sORGANIZATION_ID + ",'" + sTIRE_BARCODE + "','" + sTIRE_NUMBER + "','" + sITEM_NUM + "','" + sITEM_DESC + "','" + sWEEK_NO + "','" + sOFFLINE_BASE + "',TO_DATE('" + sPRODUCTION_DATE + "','YYYY-MM-DD HH24:MI:SS'),";
//                    sInsWms += "'" + sSPECI_MODELS + "','" + sWORKSHOP_CODE + "','" + sPRODUCTION_TYPE + "','" + sACTUAL_WEIGHT + "','" + sTIRE_STATUS + "','" + sKIND + "','" + sREWORK_FLAG + "','" + sSTOCK_DIV + "',SYSDATE,'','1','','" + sRFIDTAG + "')";

//                    string KJ_LongMenSql = string.Format(@"INSERT INTO KJ_MID_INTIREDETAIL(ID,BARCODE,TIRECOD,TIREDESC,STOCKDIV,STS,STATUS,DATETIME1,KIND,WEEKNO,PRODATE,FAC,GRADE) 
//                                            VALUES (SYS_GUID(),'{0}','{1}','{2}','{3}','{4}',1,SYSDATE,'{5}','{6}',TO_DATE('{7}','YYYY-MM-DD HH24:MI:SS'),'07','{8}')", sTIRE_BARCODE, sITEM_NUM, sITEM_DESC, sSTOCK_DIV, sts == "A" ? "1" : "0", sTIRE_STATUS, sWEEK_NO, sPRODUCTION_DATE, sKIND);


//                    int iRst = WMSDB.ExecuteNonQuery(sInsWms);


//                    //                string  sqlSDA0013 = @"INSERT INTO SDA0013(ID,FAC,BARCODE,ITNBR,ITDSC,
//                    //                                                    SCANDAT,SCANTIM,SCANSHT,STS,IF_FLAG,STOCKNO) " +
//                    //                                  "VALUES(SYS_GUID(),'07','" + sTIRE_BARCODE + "','" +
//                    //                                  sTIRE_NUMBER + "','" + sITEM_NUM + "',TRUNC(SYSDATE+9/24),SYSDATE,'" +
//                    //                                  tireInfo.Sht + "','" + tireInfo.Sts + "','N','" + tireInfo.SRoutStockNo + "')";


//                    int iRst1 = IFDB.ExecuteNonQuery(KJ_LongMenSql);
//                    if (iRst == 0 || iRst1 == 0)
//                        throw new Exception("发送基地下线表失败！" + sInsWms + "----****----" + KJ_LongMenSql);

//                    #region 旧物流接口逻辑，上新后注释掉
//                    //ListSendInfo.Add("N/A" + "," + sTIRE_BARCODE + "," + sTIRE_NUMBER + "," + sITEM_NUM + "," + sWEEK_NO + "," + sOFFLINE_BASE + "," + sPRODUCTION_DATE + "," + sSPECI_MODELS + "," + sWORKSHOP_CODE + "," + sPRODUCTION_TYPE + "," + sACTUAL_WEIGHT + "," + sTIRE_STATUS);
//                    //WebServiceHelper web = new WebServiceHelper();
//                    //string[] strBack = web.QueryGetWebService(ListSendInfo.ToArray());
//                    #endregion
//                }
//                //==================END============================================
//                #endregion

//                //插入履历表
//                string sInsSql = "INSERT INTO SDA0014 (ID,FAC,BARCODE,ITNBR,ITDSC,SCANDAT,SCANTIM,SCANSHT,SCANNAM,ONLREASON) VALUES (";
//                sInsSql += " sys_guid(),'07','" + barcode + "','" + sITNBR + "','" + sITDSC + "',to_date('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
//                sInsSql += " to_date('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),'" + row["WSHT"].ToString() + "','" + sNam + "','" + sReason + "')";
//                db.ExecuteNonQuery(sInsSql);
//                // 插入计数表
//                string sInsSqlDEL = "DELETE SDA0016 WHERE BARCODE = '" + barcode + "'";
//                db.ExecuteNonQuery(sInsSqlDEL);

//                string sInsSqlJS = "INSERT INTO SDA0016 (ID,FAC,BARCODE,ITNBR,ITDSC,SCANDAT,SCANTIM,SCANSHT,STS,IF_FLAG,STOCKNO) VALUES (";
//                sInsSqlJS += " sys_guid(),'07','" + barcode + "','" + sITNBR + "','" + sITDSC + "',to_date('" + Convert.ToDateTime(row["WDATE"].ToString()).ToString("yyyy-MM-dd") + "','yyyy-MM-dd'),";
//                sInsSqlJS += " to_date('" + dtNow.ToString("yyyy-MM-dd HH:mm:ss") + "','YYYY-MM-DD HH24:MI:SS'),'" + row["WSHT"].ToString() + "','A','N','" + sRoutStockNo + "')";
//                int aa = db.ExecuteNonQuery(sInsSqlJS);

//                LogHelper.Debug("插入0016语句：" + sInsSqlJS + "结果：" + aa);


//                string sRoutStkSql = "INSERT INTO PHS2_INSTOCKROUTDATA(MESSAGEID,STATUS,DATETIME1,DATETIME2,BARCODE,TIRECOD,TIREDESC,STOCKDIV,STS) VALUES (";
//                sRoutStkSql += "'','1',SYSDATE,'','" + barcode + "','" + itnbr + "','" + itdsc + "','" + sRoutStockNo + "','" + sSts + "')";


//                IFDB.ExecuteNonQuery(sRoutStkSql);

//                return "OK";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "Err:" + ex;
//            }
//        }

       


//        /// <summary>
//        /// 获取条码入库号
//        /// </summary>
//        /// <param name="sBarcode"></param>
//        /// <returns></returns>
//        private string GetRoutStockNo(string sERPITNBR, string sGRD, string dbGrad, string ufGrad)
//        {
//            IDataBase db = new OracleBase();
//            //========================旧规则=================================
//            //配套一级(604)==>二号库
//            //配套二级(605)+规格==>一号库
//            //非配套(606)-->则按照仓库规格维护的即可
//            string sStockNo = "1";//默认1

//            ////首先考虑等级
//            //if (sGRD == "604")//配套一级都入二号库
//            //{
//            //    sStockNo = "2";
//            //    return sStockNo;
//            //}
//            //else if (sGRD == "605")//配套二级都入一号库
//            //{
//            //    sStockNo = "1";
//            //    return sStockNo;
//            //}
//            string strItnbr = sERPITNBR;
//            DataTable dtPJXX = new DataTable();
//            if (!string.IsNullOrEmpty(strItnbr))
//            {
//                //string sqlSDA0025 = "SELECT * FROM SDA0025 WHERE ITNBR = '" + strItnbr + "' AND PDDJ = '" + sGRD + "' AND DBRESULT = '" + dbGrad + "' AND UFRESULT = '" + ufGrad + "'";
//                dtPJXX = qualityTestingController.GetPTGGINFO(strItnbr, sGRD, dbGrad, ufGrad);
//            }
//            if (dtPJXX != null && dtPJXX.Rows.Count > 0)
//            {
//                sStockNo = dtPJXX.Rows[0]["CKH"].ToString();
//            }
//            else
//            {
//                //其次考虑对应关系
//                string sSql = "SELECT * FROM SDA0005 WHERE ITNBR= '" + sERPITNBR + "'";
//                DataTable dtStockNo = db.GetDataTable(sSql);
//                if (dtStockNo != null && dtStockNo.Rows.Count > 0)
//                {
//                    sStockNo = dtStockNo.Rows[0]["STODIV"].ToString();
//                }
//                else
//                {
//                    return "";
//                }
//            }

//            return sStockNo;

//        }

//        [HttpGet]
//        [Route("api/GetBarcodeErpInfo")]
//        public DataTable GetBarcodeErpInfo(string sBarcode)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                string sSqlInfo = "SELECT A.BARCODE,A.CUITNBR,A.CUITDSC,B.ERPITNBR from LTA0001 A, (SELECT ITNBR, ERPITNBR,ITDSC";
//                sSqlInfo += "              FROM (SELECT ITNBR,ITDSC,";
//                sSqlInfo += "                               ERPITNBR,";
//                sSqlInfo += "                               VERSION,";
//                sSqlInfo += "                              ROW_NUMBER ()";
//                sSqlInfo += "                             OVER (PARTITION BY ITNBR";
//                sSqlInfo += "                                     ORDER BY TO_NUMBER (VERSION) DESC)";
//                sSqlInfo += "                                  RN";
//                sSqlInfo += "                         FROM EDB0010)";
//                sSqlInfo += "                 WHERE RN = 1) B WHERE A.CUITNBR = B.ITNBR AND A.BARCODE = '" + sBarcode + "'";
//                DataTable dtResult = db.GetDataTable(sSqlInfo);
//                return dtResult;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return null;
//            }
//        }


   

  
       
//        [HttpGet]
//        [Route("api/SendSecChkInfoToYL")]
//        public string SendSecChkInfoToYL(string sBarcode, string sITNBR, string sITDSC, string sStation, string sSortReason1)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                IDataBase IFDB = new OracleBase("");
//                string sSecVI = "INSERT INTO PHS2_LASTVIROUTDATA (MESSAGEID,STATUS,DATETIME1,DATETIME2,BARCODE,TIRECOD,TIREDESC,LASTVI,SORTREASON) VALUES (";
//                sSecVI += "'',1,sysdate,'','" + sBarcode + "','" + sITNBR + "','" + sITDSC + "','" + sStation + "','" + sSortReason1 + "')";
//                IFDB.ExecuteNonQuery(sSecVI);
//                string sUpdCKA = "UPDATE CKA0003 SET LASTVI = '" + sStation + "',LASTVIREMARK = '" + sSortReason1 + "',TIMSTAMP = SYSDATE WHERE BARCODE = '" + sBarcode + "'";
//                db.ExecuteNonQuery(sUpdCKA);
//                return "OK";
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "Err:" + ex;
//            }
//        }

//        [HttpGet]
//        [Route("api/CheckSecInspection")]
//        public string CheckSecInspection(string sBarcode, string sERPITNBR)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                string bdiv = "";
//                string sts = "A";
//                bool b = true;
//                string itnbr = "";
//                string itdsc = "";
//                string state = "";
//                int ufcnt = 0, dbcnt = 0;
//                string sHold = string.Empty;

//                //二检分拣规则
//                int iSumQty = 0;
//                string sSql = "SELECT ITNBR,SECINSP,RULE5,RULEVAL5";
//                sSql += "  FROM CKA0001  WHERE ITNBR = '" + sERPITNBR + "'";
//                DataTable dtRule = db.GetDataTable(sSql);
//                if (dtRule != null && dtRule.Rows.Count > 0)
//                {
//                    switch (dtRule.Rows[0]["RULE5"].ToString())
//                    {
//                        case "-1"://不检
//                            bdiv = "IN-直接入库";
//                            break;
//                        case "0"://全检
//                            bdiv = "RVI-需二检";
//                            break;
//                        case "1"://抽检-规格百分比
//                            string SecRuleVal = dtRule.Rows[0]["RULEVAL5"].ToString();
//                            if (SecRuleVal.Trim() == "0" || string.IsNullOrEmpty(SecRuleVal) || SecRuleVal.Trim() == "0.0" || SecRuleVal.Trim() == "0.00")
//                            {
//                                bdiv = "RVI-需二检";//填写数值异常的话，也都全部二检
//                                break;
//                            }
//                            string sSqlLJ = "select ITNBR,SECSUMQTY from CKA0053 where itnbr = '" + sERPITNBR + "'";
//                            DataTable dtLJ = db.GetDataTable(sSqlLJ);
//                            if (dtLJ != null && dtLJ.Rows.Count > 0)
//                            {
//                                string sUpdSql = "update CKA0053 set SECSUMQTY=SECSUMQTY+1 where itnbr ='" + sERPITNBR + "'";
//                                db.ExecuteNonQuery(sUpdSql);
//                                decimal dCnt = 100 / int.Parse(SecRuleVal);
//                                int iCnt = (int)Math.Round(dCnt);//获取每多少个检测一个，按照二检策略值计算
//                                iSumQty = int.Parse(dtLJ.Rows[0]["SECSUMQTY"].ToString());
//                                if (iSumQty % iCnt == 0)
//                                {
//                                    bdiv = "RVI-需二检";
//                                    break;
//                                }
//                                else
//                                {
//                                    bdiv = "IN-直接入库";
//                                    break;
//                                }
//                            }
//                            else
//                            {
//                                string sInsSql = "insert into CKA0053 values(sys_guid(),'07','" + sERPITNBR + "',1)";
//                                db.ExecuteNonQuery(sInsSql);
//                                bdiv = "RVI-需二检";
//                            }
//                            break;
//                        default:
//                            bdiv = "RVI-需二检";
//                            break;
//                    }
//                }
//                else//若规格没维护，则都二检
//                {
//                    bdiv = "RVI-需二检";
//                }
//                //

//                //判断是否异常拨胎
//                if (b)
//                {
//                    //DataTable DT = MESDB.GetTable("SELECT * FROM LTA0001 WHERE BARCODE='" + sBarcode + "' AND CUITNBR IS NOT NULL");
//                    DataTable DT = db.GetDataTable("SELECT * FROM LTA0001 WHERE BARCODE='" + sBarcode + "' AND CUITNBR IS NOT NULL");
//                    if (DT != null && DT.Rows.Count > 0)
//                    {
//                        itnbr = DT.Rows[0]["CUITNBR"].ToString();
//                        itdsc = DT.Rows[0]["CUITDSC"].ToString();
//                        state = DT.Rows[0]["CUSTATE"].ToString();
//                        if (string.IsNullOrEmpty(DT.Rows[0]["UFCNT"].ToString()))
//                            ufcnt = 1;
//                        else
//                            ufcnt = int.Parse(DT.Rows[0]["UFCNT"].ToString());
//                        if (string.IsNullOrEmpty(DT.Rows[0]["DBCNT"].ToString()))
//                            dbcnt = 1;
//                        else
//                            dbcnt = int.Parse(DT.Rows[0]["DBCNT"].ToString());
//                        sHold = DT.Rows[0]["CUCOTCOD"].ToString();
//                        if (string.IsNullOrEmpty(itnbr.Trim()))
//                        {
//                            b = false;
//                            bdiv = "ABN-条码无硫化信息";
//                        }
//                        //JOE 2020-07-24 技术部张华提出增加 2-试制 5-对策试制 在二检拨胎
//                        if (DT.Rows[0]["CUSTATE"].ToString() == "2" || DT.Rows[0]["CUSTATE"].ToString() == "5")
//                        {
//                            b = false;
//                            bdiv = "ABN-试制胎拨下";
//                        }
//                    }
//                    else
//                    {
//                        b = false;
//                        bdiv = "ABN-条码无生产信息";
//                    }
//                }
//                //增加判断有没有外检
//                if (b)
//                {
//                    DataTable dtVI = db.GetDataTable("SELECT COUNT(*) CNT FROM CKA0007 WHERE BARCODE='" + sBarcode + "' AND VIRESULT = '1'");
//                    if (dtVI.Rows[0]["CNT"].ToString() == "0")//若外检没有合格检测结果
//                    {
//                        //接着判断手持外检
//                        DataTable dtVI1 = db.GetDataTable("SELECT COUNT(*) CNT FROM QMA0003 WHERE BARCODE='" + sBarcode + "' AND VIRES = '1'");
//                        if (dtVI1.Rows[0]["CNT"].ToString() == "0")//若也没有合格检测结果
//                        {
//                            b = false;
//                            bdiv = "ABN-外观检测没有合格检测结果";
//                        }
//                    }
//                }
//                //增加判断有没有均匀性检测，动平衡不判断，因为动平衡若没有信息则MES也不知道条码也就无法判断
//                if (b)
//                {
//                    DataTable dtUF = db.GetDataTable("SELECT COUNT(*) CNT FROM QMA0201 WHERE BARCODE='" + sBarcode + "'");
//                    if (dtUF.Rows[0]["CNT"].ToString() == "0")//若均匀性没有检测结果
//                    {
//                        b = false;
//                        bdiv = "ABN-UF没有检测结果";
//                    }
//                }
//                if (b)
//                {
//                    DataTable dt1 = db.GetDataTable("SELECT AYN,DIV FROM QMA0101 WHERE AYN<>'A' AND BARCODE='" + sBarcode + "'");
//                    foreach (DataRow dr in dt1.Rows)
//                    {
//                        b = false;
//                        sts = dr["AYN"].ToString();
//                        bdiv = "ABN-轮胎不良或报废";
//                    }
//                }
//                if (b)
//                {
//                    if (ufcnt > 1 || dbcnt > 1)//若UFDB复检则报警
//                    {
//                        b = false;
//                        bdiv = "ABN-UFDB复检";
//                    }
//                }
//                if (b)
//                {
//                    if (sHold == "HOLD")
//                    {
//                        b = false;
//                        bdiv = "ABN-保留";
//                    }
//                }

//                string sInstSql = @"INSERT INTO SDA0010(ID,FAC,BARCODE,ITNBR,ITDSC,
//                            SCANDAT,SCANTIM,SCANSHT,STS,STATE,BDIV) " +
//                 "VALUES(SYS_GUID(),'07','" + sBarcode + "','" +
//                 itnbr + "','" + itdsc + "',TRUNC(SYSDATE+9/24),SYSDATE,'" +
//                 qualityTestingController.GetFacSht() + "','" + sts + "','" + state +
//                 "','" + bdiv + "')";
//                db.ExecuteNonQuery(sInstSql);

//                return bdiv;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "Err:" + ex;
//            }
//        }

//        /// <summary>
//        /// 查询人员工号
//        /// </summary>
//        /// <returns></returns>
//        [HttpGet]
//        [Route("api/GET_YGGH")]
//        public string GET_YGGH(string str)
//        {
//            try
//            {
//                IDataBase db = new OracleBase();
//                string sql = @"SELECT * FROM LSFW_EMPLOYEE WHERE NAME = '" + str + "'";
//                DataTable dtYGGH = db.GetDataTable(sql);
//                if (dtYGGH != null && dtYGGH.Rows.Count > 0)
//                {
//                    return dtYGGH.Rows[0]["LOGINNAME"].ToString();
//                }
//                else
//                {
//                    return "";
//                }
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR", ex);
//                return "Err:" + ex;
//            }
//        }

  
     
//        #endregion








//        public DataTable GET_HGPLX(string BARCODE)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = "SELECT * FROM SDA0025 WHERE ITNBR = '" + BARCODE + "' AND PDDJ = '163'";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }

       
//        public DataTable GET_TPGQJCYN(string str)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = "select * from qmb0102 where BARCODE = '" + str + "' and  div = '3'";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }

//        public DataTable GET_TPCZJCYN(string str)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = "select * from qmb0102 where BARCODE = '" + str + "' and  div = '2' AND CNNAM  <> 'MESADMIN'";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }


   
//        public DataTable GET_DS1JCYN(string str)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTSB1 FROM (SELECT * FROM (select barcode,butim,sblotid1 from LTA0001 WHERE BARCODE = '" + str + @"') M
//                    LEFT JOIN 
//                    (SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//                    WHERE RN = 1
//                    )N
//                    ON M.SBLOTID1 = N.LOTID)
//                    WHERE QXTIM IS NOT NULL
//                    AND BUTIM > QXTIM ";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }

       
//        public DataTable GET_DS2JCYN(string str)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTSB2 FROM (SELECT * FROM (select barcode,butim,sblotid2 from LTA0001 WHERE BARCODE = '" + str + @"') M
//                    LEFT JOIN 
//                    (SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//                    WHERE RN = 1
//                    )N
//                    ON M.SBLOTID2 = N.LOTID)
//                    WHERE QXTIM IS NOT NULL
//                    AND BUTIM > QXTIM ";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }



//        public DataTable GET_TT1JCYN(string str)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTBP1 FROM (SELECT * FROM (select barcode,butim,CCLOTID1 from LTA0001 WHERE BARCODE = '" + str + @"') M
//                    LEFT JOIN 
//                    (SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//                    WHERE RN = 1
//                    )N
//                    ON M.CCLOTID1 = N.LOTID)
//                    WHERE QXTIM IS NOT NULL
//                    AND BUTIM > QXTIM";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }

     
//        public DataTable GET_TT2JCYN(string str)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTBP2 FROM (SELECT * FROM (select barcode,butim,CCLOTID2 from LTA0001 WHERE BARCODE = '" + str + @"') M
//                    LEFT JOIN 
//                    (SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//                    WHERE RN = 1
//                    )N
//                    ON M.CCLOTID2 = N.LOTID)
//                    WHERE QXTIM IS NOT NULL
//                    AND BUTIM > QXTIM";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }

       
//        public DataTable GET_TQJCYN(string str)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTBD FROM (SELECT * FROM (select barcode,butim,BDLOTID from LTA0001 WHERE BARCODE = '" + str + @"') M
//                    LEFT JOIN 
//                    (SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//                    WHERE RN = 1
//                    )N
//                    ON M.BDLOTID = N.LOTID)
//                    WHERE QXTIM IS NOT NULL
//                    AND BUTIM > QXTIM ";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }


       
//        public DataTable GET_SCJCYN(string sBarcode)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTSC FROM (SELECT * FROM (SELECT * FROM (select P.*,Q.AUTIM,Q.LOTID1 from (select barcode,butim,sblotid1 from LTA0001 WHERE BARCODE =  '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '02') Q
//ON P.SBLOTID1 = Q.LOTID
//UNION ALL
//(select P.*,Q.AUTIM,Q.LOTID2 from (select barcode,butim,sblotid1 from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '02') Q
//ON P.SBLOTID1 = Q.LOTID))
//UNION ALL
//(
//select P.*,Q.AUTIM,Q.LOTID1 from (select barcode,butim,sblotid2 from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '02') Q
//ON P.SBLOTID2 = Q.LOTID
//UNION ALL
//(select P.*,Q.AUTIM,Q.LOTID2 from (select barcode,butim,sblotid2 from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '02') Q
//ON P.SBLOTID2 = Q.LOTID)
//)
//) M
//LEFT JOIN 
//(SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//WHERE RN = 1
//)N
//ON M.LOTID1 = N.LOTID)
//WHERE QXTIM IS NOT NULL
//AND BUTIM > AUTIM
//AND AUTIM > QXTIM";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }



       
//        public DataTable GET_FCJCYN(string sBarcode)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTFC FROM (SELECT * FROM (SELECT * FROM (select P.*,Q.AUTIM,Q.LOTID1 from (select barcode,butim,CClotid1 from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '03') Q
//ON P.CClotid1 = Q.LOTID
//UNION ALL
//(select P.*,Q.AUTIM,Q.JLOTID1 from (select barcode,butim,CClotid1 from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '03') Q
//ON P.CClotid1 = Q.LOTID))
//UNION ALL
//(
//select P.*,Q.AUTIM,Q.LOTID1 from (select barcode,butim,CClotid2 from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '03') Q
//ON P.CClotid2 = Q.LOTID
//UNION ALL
//(select P.*,Q.AUTIM,Q.JLOTID1 from (select barcode,butim,CClotid2 from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '03') Q
//ON P.CClotid2 = Q.LOTID)
//)
//) M
//LEFT JOIN 
//(SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//WHERE RN = 1
//)N
//ON M.LOTID1 = N.LOTID)
//WHERE QXTIM IS NOT NULL
//AND BUTIM > AUTIM
//AND AUTIM > QXTIM ";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }


       
//        public DataTable GET_GSQJCYN(string sBarcode)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = @"SELECT COUNT(*) CNTGSQ FROM (SELECT * FROM (select P.*,Q.AUTIM,Q.LOTID1 from (select barcode,butim,BDLOTID from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '04') Q
//ON P.BDLOTID = Q.LOTID
//UNION ALL
//(select P.*,Q.AUTIM,Q.LOTID2 from (select barcode,butim,BDLOTID from LTA0001 WHERE BARCODE = '" + sBarcode + @"') P
//left join 
//(SELECT * FROM LTC0001 WHERE DIV = '04') Q
//ON P.BDLOTID = Q.LOTID)
//) M
//LEFT JOIN 
//(SELECT LOTID,QXTIM FROM (select A.*,ROW_NUMBER() OVER(PARTITION BY A.LOTID ORDER BY A.QXTIM) RN  from qmc0102 A WHERE A.BCOD = 'NM-EXP' )
//WHERE RN = 1
//)N
//ON M.LOTID1 = N.LOTID)
//WHERE QXTIM IS NOT NULL
//AND BUTIM > AUTIM
//AND AUTIM > QXTIM ";
//                return db.GetDataTable(sql);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return null;
//            }
//        }



      
//        public int INSERT_SDA0029(string sBarcode, string dbGrad, string ufGrad, string sStockNo, string strMess)
//        {
//            IDataBase db = new OracleBase();
//            try
//            {
//                string sql = "INSERT INTO SDA0029 (ID,FAC,BARCODE,DBRESULT,UFRESULT,CKH,FKYY,WTIM) VALUES(SYS_GUID(),'07','" + sBarcode + "','" + dbGrad + "','" + ufGrad + "','" + sStockNo + "','" + strMess + "',SYSDATE)";
//                int iRes = db.ExecuteNonQuery(sql);
//                return iRes;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.Error("ERROR",ex);
//                return -1;
//            }
//        }

     

    }

}
