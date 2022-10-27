using System;
using System.Data;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;

namespace HYPDAWebApi.App_Data
{
    /// <summary>
    /// OracleBase 的摘要说明
    /// </summary>
    public class OracleBase 
    {
        private OracleConnection Connection;

        public OracleBase()
        {
            Connection = new OracleConnection(ConfigurationManager.AppSettings["ConnectionString"]);
        }
        public OracleBase(string con)
        {
            Connection = new OracleConnection(ConfigurationManager.AppSettings["ConnectionString2"]);
        }
        public OracleBase(string con, string str)
        {
            if (str == "GY")
                Connection = new OracleConnection(ConfigurationManager.AppSettings["ConnectionString4"]);
            else
                Connection = new OracleConnection(ConfigurationManager.AppSettings["ConnectionString3"]);
        }
        public OracleBase(int con)
        {
            Connection = new OracleConnection(ConfigurationManager.AppSettings["ConnectionStringWMS"]);
        }
        public OracleBase(int con, int can)
        {
            Connection = new OracleConnection(ConfigurationManager.AppSettings["ConnectionStringWMS2"]);
        }
        public OracleBase(int con, int can, int san)
        {
            Connection = new OracleConnection(ConfigurationManager.AppSettings["ConnectionStringMM"]);
        }
        /// <summary>
        /// 执行无返回值的SQL语句
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <returns>影响的行数</returns>
        public int ExecuteNonQuery(string strSQL)
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();
            OracleCommand command = new OracleCommand(strSQL, Connection);
            command.Transaction = Connection.BeginTransaction();
            try
            {
                int i = command.ExecuteNonQuery();
                command.Transaction.Commit();
                return i;
            }
            catch //(Exception ex)
            {
                command.Transaction.Rollback();
                //LogBase.WriteLog("OracleBase-ExecuteNonQuery", ex);
                return 0;
            }
            finally
            {
                CloseConn();
            }
        }

        /// <summary>
        /// 执行有返回值的SQL语句并返回DataSet
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(string strSQL)
        {
            try
            {
                DataSet dataSet = new DataSet();
                OracleDataAdapter SqlDA = new OracleDataAdapter(strSQL, Connection);
                SqlDA.Fill(dataSet);
                return dataSet;
            }
            catch //(Exception ex)
            {
                //LogBase.WriteLog("OracleBase-ReturnDataSet", ex);
                return null;
            }
            finally
            {
                CloseConn();
            }
        }

        /// <summary>
        /// 执行有返回值的SQL语句并返回首个表
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>表</returns>
        public DataTable GetDataTable(string sql, params DbParameter[] param)
        {
            DataSet ds = GetDataSet(sql, param);
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        /// <summary>
        /// 执行有返回值的SQL语句并返回DataSet
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(string sql, params DbParameter[] param)
        {
            DataSet ds = null;
            try
            {
                OracleCommand sqlCmd = new OracleCommand(sql, Connection);
                if (param != null)
                {
                    for (int i = 0; i < param.Length; i++)
                        //sqlCmd.Parameters.Add(param[i]);
                        sqlCmd.Parameters.Add(new OracleParameter(param[i].ParameterName, param[i].Value));
                }
                OracleDataAdapter da = new OracleDataAdapter(sqlCmd);
                ds = new DataSet();
                da.Fill(ds);
            }
            catch //(Exception ex)
            {
                ds = null;
                //LogBase.WriteLog("OracleBase-ReturnDataSet", ex);
            }
            finally
            {
                CloseConn();
            }
            return ds;
        }

        /// <summary>
        /// 执行有返回值的SQL语句并返回首个表的首行
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>行</returns>
        public DataRow GetTableRow(string sql, params DbParameter[] param)
        {
            DataTable dt = GetDataTable(sql, param);
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }

        /// <summary>
        /// 执行无返回值的SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns>影响的行数</returns>
        public int ExecuteNonQuery(string sql, params DbParameter[] param)
        {
            int ret = 0;
            OracleCommand cmd = null;
            try
            {
                cmd = new OracleCommand(sql, Connection);
                if (sql.StartsWith("usp_"))
                    cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                if (param != null)
                {
                    for (int i = 0; i < param.Length; i++)
                        //cmd.Parameters.Add(param[i]);
                        cmd.Parameters.Add(new OracleParameter(param[i].ParameterName, param[i].Value));
                }
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
                cmd.Transaction = Connection.BeginTransaction();
                int t = cmd.ExecuteNonQuery();
                if (t > 0)
                    ret = t;
                else
                    ret = 0;
                cmd.Transaction.Commit();
            }
            catch //(Exception ex)
            {
                //LogBase.WriteLog("OracleBase-ExecuteNonQuery", ex);
            }
            finally
            {
                CloseConn();
            }
            return ret;
        }

        /// <summary>
        /// 增加行
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="row"></param>
        /// <param name="notIn"></param>
        public void InsertRow(string tableName, DataRow row, params string[] notIn)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO " + tableName);
            List<string> nlist = new List<string>(notIn);
            List<string> list = new List<string>();
            List<string> list1 = new List<string>();
            List<OracleParameter> listP = new List<OracleParameter>();
            foreach (DataColumn col in row.Table.Columns)
            {
                if (nlist.Contains(col.ColumnName)) continue;
                list.Add("[" + col.ColumnName + "]");
                list1.Add("@" + col.ColumnName);
                listP.Add(new OracleParameter(col.ColumnName, row[col.ColumnName]));
            }
            sb.AppendLine("(" + string.Join(", ", list.ToArray()) + ")");
            sb.AppendLine("SELECT " + string.Join(", ", list1.ToArray()));
            ExecuteNonQuery(sb.ToString(), listP.ToArray());
        }

        /// <summary>
        /// 修改行
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="row"></param>
        /// <param name="notIn"></param>
        public void UpdateRow(string tableName, DataRow row, params string[] notIn)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UPDATE " + tableName);
            sb.AppendLine("SET");
            List<string> nlist = new List<string>(notIn);
            List<string> list = new List<string>();
            List<OracleParameter> listP = new List<OracleParameter>();
            foreach (DataColumn col in row.Table.Columns)
            {
                if (nlist.Contains(col.ColumnName)) continue;
                listP.Add(new OracleParameter(col.ColumnName, row[col.ColumnName]));
                if (col.ColumnName == "ID") continue;
                list.Add("[" + col.ColumnName + "]" + "=@" + col.ColumnName);
            }
            sb.AppendLine(string.Join(", ", list.ToArray()));
            sb.AppendLine("WHERE ID=@ID");
            ExecuteNonQuery(sb.ToString(), listP.ToArray());
        }

        /// <summary>
        /// 删除行
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        public void DeleteRow(string tableName, DataRow row, params string[] keys)
        {
            List<string> listkey = new List<string>();
            List<OracleParameter> listP = new List<OracleParameter>();
            foreach (string key in keys)
            {
                listkey.Add("[" + key + "]" + "=@" + key);
                listP.Add(new OracleParameter(key, row[key]));
            }
            if (listkey.Count == 0)
            {
                listkey.Add("ID=@ID");
                listP.Add(new OracleParameter("ID", row["ID"]));
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DELETE FROM " + tableName);
            sb.AppendLine("WHERE " + string.Join(" AND ", listkey.ToArray()));
            ExecuteNonQuery(sb.ToString(), listP.ToArray());
        }

        /// <summary>
        /// 事物，执行SQL语句
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <returns>影响的行数</returns>
        public int TranNonQuery(string[] SQL)
        {
            //if (Connection.State == ConnectionState.Closed)
            //    Connection.Open();
            //int i = 0;
            //using (OracleCommand orcomm = Connection.CreateCommand())
            //{
            //    using (OracleTransaction tran = Connection.BeginTransaction())
            //    {
            //        try
            //        {
            //            orcomm.Transaction = tran;
            //            foreach (string s in SQL)
            //            {
            //                orcomm.CommandText = s;
            //                i += orcomm.ExecuteNonQuery();

            //            }
            //            if (i > 1)
            //            {
            //                tran.Commit();
            //            }
            //            //i = orcomm.ExecuteNonQuery();
            //            return i;
            //        }
            //        catch (Exception ex)
            //        {
            //            tran.Rollback();
            //            CloseConn();
            //            throw ex;
            //        }
            //    }
            //}
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
            using (OracleCommand command = new OracleCommand())
            {
                command.Connection = Connection;
                command.Transaction = Connection.BeginTransaction();
                int i = 0;
                try
                {
                    command.CommandText = "BEGIN \n ";
                    foreach (string s in SQL)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            command.CommandText += s + ";\n";
                            i++;
                        }
                    }
                    command.CommandText += "\nEND;";
                    command.ExecuteNonQuery();
                    command.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    command.Transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    CloseConn();
                }
                return i;
            }
        }

        /// <summary>
        /// 事物，执行SQL语句
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <returns>影响的行数</returns>
        public int TranNonQuery1(string SQL)
        {
            //if (Connection.State == ConnectionState.Closed)
            //    Connection.Open();
            //int i = 0;
            //using (OracleCommand orcomm = Connection.CreateCommand())
            //{
            //    using (OracleTransaction tran = Connection.BeginTransaction())
            //    {
            //        try
            //        {
            //            orcomm.Transaction = tran;
            //                orcomm.CommandText = SQL;
            //                i= orcomm.ExecuteNonQuery();
            //            //i = orcomm.ExecuteNonQuery();
            //            return i;
            //        }
            //        catch (Exception ex)
            //        {
            //            tran.Rollback();
            //            CloseConn();
            //            throw ex;
            //        }
            //    }
            //}
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
            using (OracleCommand command = new OracleCommand(SQL, Connection))
            {
                command.Transaction = Connection.BeginTransaction();
                try
                {
                    int i = command.ExecuteNonQuery();
                    command.Transaction.Commit();
                    CloseConn();
                    return i;
                }
                catch (Exception ex)
                {
                    command.Transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    CloseConn();
                }
            }
        }

        private void CloseConn()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }
    }
}
