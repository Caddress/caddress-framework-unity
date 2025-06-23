using LitJson;
using MySql.Data.MySqlClient;

namespace UMySql
{
    public class Mysql
    {

        private static MysqlDatabase m_database = null;

        public static void Open(MysqlConnectData connectData)
        {
            m_database = new MysqlDatabase(connectData);
        }


        public static void Close()
        {
            //m_database.Release();
            m_database = null;
            System.GC.Collect();
        }


        #region TODO 直接获取数据

        public static string Select(string sql)
        {
            if (m_database == null) return null;
            if (string.IsNullOrEmpty(m_database.connection.Database)) return null;
            MySqlCommand cmd = new MySqlCommand(sql, m_database.connection);
            cmd.CommandTimeout = 0;
            MySqlDataReader reader = cmd.ExecuteReader(System.Data.CommandBehavior.Default);
            try
            {
                string result = string.Empty;
                var writer = new JsonWriter();
                writer.WriteArrayStart();
                while (reader.Read())
                {
                    writer.WriteObjectStart();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        writer.WritePropertyName(reader.GetName(i));
                        writer.Write(reader.GetValue(i).ToString());
                    }
                    writer.WriteObjectEnd();
                }
                writer.WriteArrayEnd();
                result = $"{{\"data\":{writer.ToString()}}}";
                reader.Close();
                reader.Dispose();
                reader = null;
                cmd.Dispose();
                cmd = null;
                return result;
            }
            catch (System.Exception e)
            {
                if(reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                if(cmd != null)
                {
                    cmd.Dispose();
                }
                throw new System.Exception(e.Message.ToString());
            }
        }

        public static void Excute(string sql, System.Action<string> callback = null)
        {
            if (m_database == null) return;
            if (m_database.connection != null && string.IsNullOrEmpty(m_database.connection.Database)) return;
            MySqlCommand cmd = null;
            try
            {
                cmd = m_database.connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                callback?.Invoke("true");
            }
            catch (System.Exception e)
            {
                if(cmd != null)
                    cmd.Dispose();
                throw new System.Exception(e.Message.ToString());
            }
        }

        #endregion

    }


}
