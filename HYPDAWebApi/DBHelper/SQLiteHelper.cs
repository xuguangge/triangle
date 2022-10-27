
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

using System.Linq;
using System.Text;

namespace HYPDAWebApi.DBHelper
{
    public static class SQLiteHelper
    {
        //读取配置文件中的连接字符串
        public static string CONSTR = ConfigurationManager.ConnectionStrings["sqlite"].ConnectionString ;


        /// <summary>
        /// 获取连接对象
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>返回连接对象</returns>
        //public static SQLiteConnection CreateConnection(string connectionString)
        //{
        //    SQLiteConnection con = new SQLiteConnection(connectionString);
        //    con.Open();
        //    return con;
        //}
        ///// <summary>
        ///// 执行增删改的辅助方法
        ///// </summary>
        ///// <param name="connectionString">连接字符串</param>
        ///// <param name="cmdType">命令类型</param>
        ///// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        ///// <param name="values">SQL语句中的参数</param>
        ///// <returns>返回收影响的行数</returns>
        //public static int ExecuteCommand(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        //{
        //    using (SQLiteConnection con = CreateConnection(connectionString))
        //    {
        //        SQLiteCommand cmd = new SQLiteCommand();
        //        cmd.Connection = (SQLiteConnection)con;
        //        cmd.CommandType = cmdType;
        //        cmd.CommandText = cmdText;
        //        if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
        //        int result = cmd.ExecuteNonQuery();
        //        con.Close();
        //        return result;
        //    }
        //}
        //public static int ExecuteCommand(CommandType cmdType, string cmdText, DbParameter[] values, DbTransaction tran, DbConnection conn)
        //{
        //    //using (DbConnection con = CreateConnection(connectionString))
        //    //{
        //    SQLiteCommand cmd = new SQLiteCommand();
        //    cmd.Connection = (SQLiteConnection)conn;
        //    cmd.CommandType = cmdType;
        //    cmd.Transaction = (SQLiteTransaction)tran;
        //    cmd.CommandText = cmdText;
        //    if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
        //    int result = cmd.ExecuteNonQuery();
        //    //con.Close();
        //    return result;
        //    //}
        //}
        //public static int ExecuteCommand(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values, bool isClose)
        //{
        //    using (SQLiteConnection con = CreateConnection(connectionString))
        //    {
        //        SQLiteCommand cmd = new SQLiteCommand();
        //        cmd.Connection = (SQLiteConnection)con;
        //        cmd.CommandType = cmdType;
        //        cmd.CommandText = cmdText;
        //        if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
        //        int result = cmd.ExecuteNonQuery();
        //        if (isClose)
        //        {
        //            con.Close();
        //        }
        //        return result;
        //    }
        //}
        ///// <summary>
        ///// 执行查询的方法
        ///// </summary>
        ///// <param name="connectionString">连接字符串</param>
        ///// <param name="cmdType">命令类型</param>
        ///// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        ///// <param name="values">SQL语句中的参数</param>
        ///// <returns>返回数据读取器对象</returns>
        //public static SQLiteDataReader GetReader(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        //{
        //    SQLiteConnection con = CreateConnection(connectionString);
        //    SQLiteCommand cmd = new SQLiteCommand();
        //    cmd.Connection = (SQLiteConnection)con;
        //    cmd.CommandType = cmdType;
        //    cmd.CommandText = cmdText;
        //    if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
        //    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

        //    return reader;
        //}

        //public static SQLiteDataReader GetReader(CommandType cmdType, string cmdText, DbParameter[] values, DbTransaction tran, DbConnection conn)
        //{
        //    //SQLiteConnection con = CreateConnection(connectionString);
        //    SQLiteCommand cmd = new SQLiteCommand();
        //    cmd.Connection = (SQLiteConnection)conn;
        //    cmd.CommandType = cmdType;
        //    cmd.Transaction = (SQLiteTransaction)tran;
        //    cmd.CommandText = cmdText;
        //    if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
        //    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

        //    return reader;
        //}
        ///// <summary>
        ///// 执行带聚合函数的查询方法
        ///// </summary>
        ///// <param name="connectionString">连接字符串</param>
        ///// <param name="cmdType">命令类型</param>
        ///// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        ///// <param name="values">SQL语句中的参数</param>
        ///// <returns>返回执行结果中第一行第一列的值</returns>
        //public static object GetScalar(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        //{
        //    using (DbConnection con = CreateConnection(connectionString))
        //    {
        //        SQLiteCommand cmd = new SQLiteCommand();
        //        cmd.Connection = (SQLiteConnection)con;
        //        cmd.CommandType = cmdType;
        //        cmd.CommandText = cmdText;
        //        if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
        //        object result = cmd.ExecuteScalar();
        //        con.Close();
        //        return result;
        //    }
        //}
        //public static object GetScalar(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values, DbTransaction tran, DbConnection conn)
        //{
        //    //using (DbConnection con = CreateConnection(connectionString))
        //    //{
        //    SQLiteCommand cmd = new SQLiteCommand();
        //    cmd.Connection = (SQLiteConnection)conn;
        //    cmd.Transaction = (SQLiteTransaction)tran;
        //    cmd.CommandType = cmdType;
        //    cmd.CommandText = cmdText;
        //    if (values != null && values.Length > 0) cmd.Parameters.AddRange(values);
        //    object result = cmd.ExecuteScalar();
        //    //con.Close();
        //    return result;
        //    //}
        //}
        ///// <summary>
        ///// 执行查询的方法，返回数据集
        ///// </summary>
        ///// <param name="connectionString">数据库连接字符串</param>
        ///// <param name="cmdType">命令类型</param>
        ///// <param name="cmdText">要执行的SQL语句或存储过程名</param>
        ///// <param name="values">SQL语句或存储过程的参数列表</param>
        ///// <returns>返回相应的数据集</returns>
        //public static DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values)
        //{
        //    using (SQLiteConnection con = new SQLiteConnection(connectionString))
        //    {
        //        SQLiteDataAdapter adapter = new SQLiteDataAdapter();
        //        adapter.SelectCommand = new SQLiteCommand();
        //        adapter.SelectCommand.Connection = con;
        //        adapter.SelectCommand.CommandType = cmdType;
        //        adapter.SelectCommand.CommandText = cmdText;
        //        if (values != null && values.Length > 0) adapter.SelectCommand.Parameters.AddRange(values);
        //        DataSet ds = new DataSet();
        //        adapter.Fill(ds);
        //        return ds;
        //    }
        //}
        //public static DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText, DbParameter[] values, DbTransaction tran, DbConnection conn)
        //{
        //    //using (DbConnection con = new DbConnection(connectionString))
        //    //{
        //    SQLiteDataAdapter adapter = new SQLiteDataAdapter();
        //    adapter.SelectCommand = new SQLiteCommand();
        //    adapter.SelectCommand.Connection = (SQLiteConnection)conn;
        //    adapter.SelectCommand.Transaction = (SQLiteTransaction)tran;
        //    adapter.SelectCommand.CommandType = cmdType;
        //    adapter.SelectCommand.CommandText = cmdText;
        //    if (values != null && values.Length > 0) adapter.SelectCommand.Parameters.AddRange(values);
        //    DataSet ds = new DataSet();
        //    adapter.Fill(ds);
        //    return ds;
        //    //}
        //}
    }
}
