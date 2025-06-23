// ****************************************************************************
// 文件名称(File Name):			SQLite.cs
//
// 功能描述(Description):		
//
// 作者(Author): 				梁景裕
//
// 日期(Create Date): 			2018.7
//
// 修改记录(Revision History):	
// ****************************************************************************  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using LitJson;
using Mono.Data.Sqlite;
using UnityEngine;

public class SQLite
{

    public SqliteConnection connection;
    private SqliteCommand command;


    public SQLite(string path)
    {
        connection = new SqliteConnection(path);    // 创建SQLite对象的同时，创建SqliteConnection对象
        connection.Open();                         // 打开数据库链接
        Command();
    }


    public SqliteCommand Command()
    {
        command = connection.CreateCommand();
        return command;
    }

    // 【增加数据】
    public SqliteDataReader InsertData(string table_name, string[] fieldNames, object[] values)
    {
        // 如果字段的个数，和数据的个数不相等，无法执行插入的语句，所以返回一个null
        if (fieldNames.Length != values.Length)
        {
            return null;
        }

        command.CommandText = "replace into " + table_name + "('";

        for (int i = 0; i < fieldNames.Length; i++)
        {
            command.CommandText += fieldNames[i];
            if (i < fieldNames.Length - 1)
            {
                command.CommandText += "','";
            }
        }

        command.CommandText += "')" + "values ('";

        for (int i = 0; i < values.Length; i++)
        {
            command.CommandText += values[i];

            if (i < values.Length - 1)
            {
                command.CommandText += "','";
            }
        }

        command.CommandText += "')";

        return command.ExecuteReader();

    }

    // 【批量增加数据】
    //TODO：自适应所有栏位的表
    public SqliteDataReader InsertDatas(string tableName, List<List<object>> values)
    {
        // 如果字段的个数，和数据的个数不相等，无法执行插入的语句，所以返回一个null
        if (values[0].Count != 4)
        {
            return null;
        }
        Debug.Log(tableName + " "+values.Count);
        var transaction = connection.BeginTransaction();
        try
        {
            for (int i = 0; i < values.Count; i++)
            {
                SqliteCommand command = connection.CreateCommand();
                command = connection.CreateCommand();
                command.CommandText = "replace into " + tableName + " VALUES(@id,@din,@json,@date)";
                command.Parameters.Add("@id", DbType.String).Value = values[i][0];
                command.Parameters.Add("@din", DbType.String).Value = values[i][1];
                command.Parameters.Add("@json", DbType.String).Value = values[i][2];
                command.Parameters.Add("@date", DbType.String).Value = values[i][3];
                command.ExecuteNonQuery();
                //Debug.Log("插入监控数据 -----" + values[i][0]);
            }
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Debug.Log("Error : " + e.Message);
            throw e;
        }

        return null;
    }

    // 【删除数据】
    public SqliteDataReader DeleteData(string table_name, string sql)
    {
        if (string.IsNullOrEmpty(table_name))
        {
            return null;
        }
        if (string.IsNullOrEmpty(sql))
        {
            return null;
        }
        command.CommandText = "delete from " + table_name + " " + sql;
        var reader = command.ExecuteReader(System.Data.CommandBehavior.Default);
        return reader;
    }

    // 【修改数据】
    public SqliteDataReader UpdateData(string table_name, string[] values, string[] conditions)
    {
        command.CommandText = "update " + table_name + " set " + values[0];

        for (int i = 1; i < values.Length; i++)
        {
            command.CommandText += "," + values[i];
        }

        command.CommandText += " where " + conditions[0];
        for (int i = 1; i < conditions.Length; i++)
        {
            command.CommandText += " or " + conditions[i];
        }

        return command.ExecuteReader();
    }

    // 【查询数据】
    public SqliteDataReader SelectData(string table_name, string[] fields)
    {
        command.CommandText = "select " + fields[0];
        for (int i = 1; i < fields.Length; i++)
        {
            command.CommandText += "," + fields[i];
        }
        command.CommandText += " from " + table_name;

        return command.ExecuteReader();
    }

    // 【按条件查询数据】
    public SqliteDataReader SelectTableDataByCondition(string table_name, string condition)
    {
        command.CommandText = "select * from " + table_name + condition;
        var reader = command.ExecuteReader(System.Data.CommandBehavior.Default);
        return reader;
    }

    // 【按条件查询数据数量】
    public SqliteDataReader SelectCountByCondition(string table_name, string condition)
    {
        command.CommandText = "select count(*) from " + table_name + condition;
        var reader = command.ExecuteReader(System.Data.CommandBehavior.Default);
        return reader;
    }

    // 【执行任意SQL语句】
    public SqliteDataReader ExecuteAnyCommand(string commandText)
    {
        command.CommandText = commandText;
        var reader = command.ExecuteReader(System.Data.CommandBehavior.Default);
        return reader;
    }

    // 【查询整张表的数据】
    public SqliteDataReader SelectFullTableData(string table_name)
    {
        command.CommandText = "select * from " + table_name;
        var reader = command.ExecuteReader(System.Data.CommandBehavior.Default);
        return reader;
    }


    // 【关闭数据库】
    public void CloseDataBase()
    {
        connection.Close();
        //SqliteConnection.ClearPool(connection);
        connection.Dispose();
        command.Cancel();
        command.Dispose();
        connection = null;
        command = null;
    }

}
