/************************************************************************************
 *      Copyright (C) 2011 MJ.com,All Rights Reserved
 *      File:
 *				DBHelper.cs
 *      Description:
 *				 SQL���ݷ��ʸ�����
 *      Author:
 *				
 *      Finish DateTime:
 *				2018��02��28��
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
    /// SQL���ݷ��ʸ�����
    /// </summary>
    public class OracleDBHelper
    {
        /// <summary>
        /// �����ݿ�
        /// </summary>
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        /// <summary>
        /// �����ӿ��м��
        /// </summary>
        public static readonly string ConnectionString2 = ConfigurationManager.ConnectionStrings["ConnectionString2"].ConnectionString;
        public static readonly string ConnectionString3 = ConfigurationManager.ConnectionStrings["ConnectionString3"].ConnectionString;
        /// <summary>
        /// �������ݿ�
        /// </summary>
        public static readonly string ConnectionString4 = ConfigurationManager.ConnectionStrings["ConnectionString4"].ConnectionString;
        /// <summary>
        /// ��Ʒ���ݿ�
        /// </summary>
        public static readonly string ConnectionStringWMS = ConfigurationManager.ConnectionStrings["ConnectionStringWMS"].ConnectionString;
        public static readonly string ConnectionStringWMS2 = ConfigurationManager.ConnectionStrings["ConnectionStringWMS2"].ConnectionString;
        /// <summary>
        /// ģ�߲������ݿ�
        /// </summary>
        public static readonly string ConnectionStringMM = ConfigurationManager.ConnectionStrings["ConnectionStringMM"].ConnectionString;

        /// <summary>
        /// ��ȡ���Ӷ���
        /// </summary>
        /// <param name="connectionString">�����ַ���</param>
        /// <returns>�������Ӷ���</returns>
        public static OracleConnection CreateConnection(string connectionString)
        {
            OracleConnection con = new OracleConnection(connectionString);
            con.Open();
            return con;
        }
        /// <summary>
        /// ִ����ɾ�ĵĸ�������
        /// </summary>
        /// <param name="connectionString">�����ַ���</param>
        /// <param name="cmdType">��������</param>
        /// <param name="cmdText">Ҫִ�е�SQL����洢������</param>
        /// <param name="values">SQL����еĲ���</param>
        /// <returns>������Ӱ�������</returns>
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
        /// ִ�в�ѯ�ķ���
        /// </summary>
        /// <param name="connectionString">�����ַ���</param>
        /// <param name="cmdType">��������</param>
        /// <param name="cmdText">Ҫִ�е�SQL����洢������</param>
        /// <param name="values">SQL����еĲ���</param>
        /// <returns>�������ݶ�ȡ������</returns>
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
        /// ִ�д��ۺϺ����Ĳ�ѯ����
        /// </summary>
        /// <param name="connectionString">�����ַ���</param>
        /// <param name="cmdType">��������</param>
        /// <param name="cmdText">Ҫִ�е�SQL����洢������</param>
        /// <param name="values">SQL����еĲ���</param>
        /// <returns>����ִ�н���е�һ�е�һ�е�ֵ</returns>
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
        /// ִ�в�ѯ�ķ������������ݼ�
        /// </summary>
        /// <param name="connectionString">���ݿ������ַ���</param>
        /// <param name="cmdType">��������</param>
        /// <param name="cmdText">Ҫִ�е�SQL����洢������</param>
        /// <param name="values">SQL����洢���̵Ĳ����б�</param>
        /// <returns>������Ӧ�����ݼ�</returns>
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
        /// ���ﴦ��ORACLE(������)
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
                       
                        //����sql����
                        StackTrace st = new StackTrace(true);
                        Type classt = st.GetFrame(1).GetMethod().DeclaringType;
                        //�õ����ô˷����������ռ�
                        string fullname = classt.FullName;
                        //�õ����ô˷����ķ�����
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
                                throw new Exception("Oracle����-1,sqlִ�д���");
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
        /// ���ﴦ��ORACLE(������)
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
                        //����sql����
                        StackTrace st = new StackTrace(true);
                        Type classt = st.GetFrame(1).GetMethod().DeclaringType;
                        //�õ����ô˷����������ռ�
                        string fullname = classt.FullName;
                        //�õ����ô˷����ķ�����
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
                                throw new Exception("Oracle����-1,sqlִ�д���");
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
