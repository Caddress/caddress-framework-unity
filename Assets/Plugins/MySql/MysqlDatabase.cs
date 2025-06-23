using UnityEngine;
using MySql.Data.MySqlClient;

namespace UMySql
{
    public class MysqlDatabase
    {
        private MysqlConnectData connectData;
        private MySqlConnection m_connection;
        private MySqlDataReader dataReader;
        private MySqlCommand command;

        public MySqlConnection connection { get => m_connection; }

        public MysqlDatabase(MysqlConnectData data)
        {
            this.connectData = data;
            OpenConnection();
        }

        ~MysqlDatabase()
        {
            CloseConnection();
        }

        //public void Release()
        //{
        //    CloseConnection();
        //}

        private void OpenConnection()
        {
            try
            {
                var connectionStrBuilder = new MySqlConnectionStringBuilder();
                connectionStrBuilder.Server = connectData.host;
                connectionStrBuilder.Password = connectData.password;
                connectionStrBuilder.Port = connectData.port;
                connectionStrBuilder.UserID = connectData.user;
                connectionStrBuilder.Database = connectData.db;
                connectionStrBuilder.CharacterSet = "utf8";
                connectionStrBuilder.AllowUserVariables = true;
                connectionStrBuilder.PersistSecurityInfo = true;
                string connectionStr = connectionStrBuilder.ToString();
                this.m_connection = new MySqlConnection(connectionStr);
                this.m_connection.Open();
                Debug.Log("<color=green>MySql--连接成功</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Mysql Error : {e.Message.ToString()}");
                throw e;
            }
        }

        private void CloseConnection()
        {
            if (m_connection != null) m_connection.Close();
            if (command != null) command.Cancel();
            if (dataReader != null) dataReader.Close();
            m_connection = null;
            command = null;
            dataReader = null;
            Debug.Log("<color=yellow>MySql--连接关闭</color>");
        }
    }

    public class MysqlConnectData
    {
        public string host;
        public uint port;
        public string user;
        public string password;
        public string db;
    }
}


