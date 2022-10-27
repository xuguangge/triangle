using System;
using System.Data;
using System.Data.Common;
namespace HYPDAWebApi.App_Data
{
    public interface IDataBase
    {
        ////增删改
        //void DeleteRow(string tableName, DataRow row, params string[] keys);
        //void InsertRow(string tableName, DataRow row, params string[] notIn);
        //void UpdateRow(string tableName, DataRow row, params string[] notIn);
        ////执行单条sql语句
        //int ExecuteNonQuery(string sql, params DbParameter[] param);
        //int ExecuteNonQuery(string sql);
        ////执行多条sql语句
        //int TranNonQuery(string[] SQL);
        //////查询,返回值
        //DataSet GetDataSet(string sql, params DbParameter[] param);
        //DataSet GetDataSet(string sql);

        //DataTable GetTable(string sql);
        //DataTable GetTable(string sql, params DbParameter[] param);
        ////获取服务器时间
        //DateTime GetSystemDateTime();

        int ExecuteNonQuery(string strSQL);
        DataSet GetDataSet(string strSQL);
        DataTable GetDataTable(string sql, params DbParameter[] param);
        DataSet GetDataSet(string sql, params DbParameter[] param);
        DataRow GetTableRow(string sql, params DbParameter[] param);
        int ExecuteNonQuery(string sql, params DbParameter[] param);
        void InsertRow(string tableName, DataRow row, params string[] notIn);
        void UpdateRow(string tableName, DataRow row, params string[] notIn);
        void DeleteRow(string tableName, DataRow row, params string[] keys);
        int TranNonQuery(string[] SQL);
        int TranNonQuery1(string SQL);



    }
}