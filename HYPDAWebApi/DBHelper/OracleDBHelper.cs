/************************************************************************************
 *      Copyright (C) 2011 MJ.com,All Rights Reserved
 *      File:
 *				DBHelper.cs
 *      Description:
 *				 SQL数据访问辅助类
 *      Author:
 *				
 *      Finish DateTime:
 *				2018年02月28日
 *      History:
 *      
 ***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using System.Collections;
using System.Diagnostics;

namespace HYPDAWebApi.DBHelper
{
    /// <summary>
    /// SQL数据访问辅助类
    /// </summary>
    public class OracleDBHelper
    {
        /// <summary>
        /// 主数据库
        /// </summary>
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        /// <summary>
        /// 韩华接口中间表
        /// </summary>
        public static readonly string ConnectionString2 = ConfigurationManager.ConnectionStrings["ConnectionString2"].ConnectionString;
        public static readonly string ConnectionString3 = ConfigurationManager.ConnectionStrings["ConnectionString3"].ConnectionString;
        /// <summary>
        /// 工艺数据库
        /// </summary>
        public static readonly string ConnectionString4 = ConfigurationManager.ConnectionStrings["ConnectionString4"].ConnectionString;
        /// <summary>
        /// 成品数据库
        /// </summary>
        public static readonly string ConnectionStringWMS = ConfigurationManager.ConnectionStrings["ConnectionStringWMS"].ConnectionString;
        public static readonly string ConnectionStringWMS2 = ConfigurationManager.ConnectionStrings["ConnectionStringWMS2"].ConnectionString;
        /// <summary>
        /// 模具测试数据库
        /// </summary>
        public static readonly string ConnectionStringMM = ConfigurationManager.ConnectionStrings["ConnectionStringMM"].ConnectionString;

        /// <summary>
        /// 获取连接对象
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>返回连接对象</returns>
        public static OracleConnection CreateConnection(string connectionString)
        {
            OracleConnection con = new OracleConnection(connectionString);
            con.Open();
            return con;
        }
        /// <summary>
        /// 执行增删改的辅助方法
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        /// <param name="values">SQL语句中的参数</param>
        /// <returns>返回收影响的行数</returns>
        public static int ExecuteCommand(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        {
            using (OracleConnection con = CreateConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = (OracleConnection)con;
                cmd.CommandType = cmdType;
                cmd.CommandText = cmdText;
                if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
                int result = cmd.ExecuteNonQuery();
                con.Close();
                return result;
            }
        }

        
        public static int ExecuteCommand(CommandType cmdType, string cmdText, DbParameter[] values,DbTransaction tran,DbConnection conn)
        {
            //using (DbConnection con = CreateConnection(connectionString))
            //{
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = (OracleConnection)conn;
                cmd.CommandType = cmdType;
                cmd.Transaction = (OracleTransaction)tran;
                cmd.CommandText = cmdText;
                if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
                int result = cmd.ExecuteNonQuery();
                //con.Close();
                return result;
            //}
        }
        public static int ExecuteCommand(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values,bool isClose)
        {
            using (OracleConnection con = CreateConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = (OracleConnection)con;
                cmd.CommandType = cmdType;
                cmd.CommandText = cmdText;
                if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
                int result = cmd.ExecuteNonQuery();
                if (isClose)
                {
                    con.Close();
                }
                return result;
            }
        }
        /// <summary>
        /// 执行查询的方法
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        /// <param name="values">SQL语句中的参数</param>
        /// <returns>返回数据读取器对象</returns>
        public static OracleDataReader GetReader(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        {
            OracleConnection con = CreateConnection(connectionString);
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = (OracleConnection)con;
            cmd.CommandType = cmdType;
            cmd.CommandText = cmdText;
            if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
            OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            
            return reader;
        }
        
        public static OracleDataReader GetReader(CommandType cmdType, string cmdText, DbParameter[] values,DbTransaction tran,DbConnection conn)
        {
            //OracleConnection con = CreateConnection(connectionString);
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = (OracleConnection)conn;
            cmd.CommandType = cmdType;
            cmd.Transaction = (OracleTransaction)tran;
            cmd.CommandText = cmdText;
            if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
            OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

            return reader;
        }
        /// <summary>
        /// 执行带聚合函数的查询方法
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        /// <param name="values">SQL语句中的参数</param>
        /// <returns>返回执行结果中第一行第一列的值</returns>
        public static object GetScalar(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        {
            using (DbConnection con = CreateConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = (OracleConnection)con;
                cmd.CommandType = cmdType;
                cmd.CommandText = cmdText;
                if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
                object result = cmd.ExecuteScalar();
                con.Close();
                return result;
            }
        }
        public static object GetScalar(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values,DbTransaction tran,DbConnection conn)
        {
            //using (DbConnection con = CreateConnection(connectionString))
            //{
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = (OracleConnection)conn;
                cmd.Transaction = (OracleTransaction)tran;
                cmd.CommandType = cmdType;
                cmd.CommandText = cmdText;
                if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
                object result = cmd.ExecuteScalar();
                //con.Close();
                return result;
            //}
        }
        /// <summary>
        /// 执行查询的方法，返回数据集
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        /// <param name="values">SQL语句或存储过程的参数列表</param>
        /// <returns>返回相应的数据集</returns>
        public static DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand();
                adapter.SelectCommand.Connection = con;
                adapter.SelectCommand.CommandType = cmdType;
                adapter.SelectCommand.CommandText = cmdText;
                if (values != null && values.Length > 0) adapter.SelectCommand.Parameters.AddRange(values);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }
        public static DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values,DbTransaction tran,DbConnection conn)
        {
            //using (DbConnection con = new DbConnection(connectionString))
            //{
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand();
                adapter.SelectCommand.Connection = (OracleConnection)conn;
                adapter.SelectCommand.Transaction = (OracleTransaction)tran;
                adapter.SelectCommand.CommandType = cmdType;
                adapter.SelectCommand.CommandText = cmdText;
                if (values != null && values.Length > 0) adapter.SelectCommand.Parameters.AddRange(values);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            //}
        }

        /// <summary>
        /// 事物处理ORACLE(带参数)
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static int TranNonQuery(string connectionString, ArrayList arr)
        {
            int j = 0;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                connection.Open();
                OracleTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < arr.Count; n++)
                    {
                        string strsql = arr[n].ToString();
                       
                        //保存sql方法
                        StackTrace st = new StackTrace(true);
                        Type classt = st.GetFrame(1).GetMethod().DeclaringType;
                        //得到调用此方法的命名空间
                        string fullname = classt.FullName;
                        //得到调用此方法的方法名
                        string methodname = st.GetFrame(1).GetMethod().Name;
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            int i = 0;
                            //try
                            //{
                            i = cmd.ExecuteNonQuery();
                            //}
                            //catch (Exception e)
                            //{
                            //    throw e;
                            //}
                            if (i < 0)
                            {
                                throw new Exception("Oracle返回-1,sql执行错误！");
                            }
                            j = j + i;
                            cmd.Parameters.Clear();
                        }
                    }
                    tx.Commit();
                }
                catch (OracleException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                    connection.Dispose();
                    cmd.Parameters.Clear();
                }
                return j;
            }
        }

        /// <summary>
        /// 事物处理ORACLE(带参数)
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static int TranNonQuerys(string connectionString, ArrayList arr)
        {
            int j = 0;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = connection;
                connection.Open();
                OracleTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    object[] sql_par = new object[2];
                    for (int n = 0; n < arr.Count; n++)
                    {
                        sql_par = (object[])arr[n];
                        string strsql = sql_par[0].ToString();
                        OracleParameter[] pars = (OracleParameter[])sql_par[1];
                        //保存sql方法
                        StackTrace st = new StackTrace(true);
                        Type classt = st.GetFrame(1).GetMethod().DeclaringType;
                        //得到调用此方法的命名空间
                        string fullname = classt.FullName;
                        //得到调用此方法的方法名
                        string methodname = st.GetFrame(1).GetMethod().Name;
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            if (pars != null)
                            {
                                // cmd.Parameters.AddRange(pars);
                                cmd.Parameters.Clear();
                                foreach (OracleParameter parm in pars)
                                {
                                    OracleParameter pp = (OracleParameter)((ICloneable)parm).Clone();
                                    cmd.Parameters.Add(pp);
                                }
                            }
                            int i = 0;
                            //try
                            //{
                            i = cmd.ExecuteNonQuery();
                            //}
                            //catch (Exception e)
                            //{
                            //    throw e;
                            //}
                            if (i < 0)
                            {
                                throw new Exception("Oracle返回-1,sql执行错误！");
                            }
                            j = j + i;
                            cmd.Parameters.Clear();
                        }
                    }
                    tx.Commit();
                }
                catch (OracleException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                    connection.Dispose();
                    cmd.Parameters.Clear();
                }
                return j;
            }
        }
    }
}
