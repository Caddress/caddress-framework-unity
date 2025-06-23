using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Caddress.Template.Adapter {
    public class MySQLConnector : IDataBase {

        private string _connectionURI;
        private IDbConnection _connection;

        public MySQLConnector(string connectionString) {
            _connectionURI = connectionString;
        }

        public void Connect() {
            _connection = new MySqlConnection(_connectionURI);
            _connection.Open();
        }

        public void Close() {
            _connection?.Close();
            _connection = null;
        }

        public void Insert(string table, Dictionary<string, object> data) {
            var keys = string.Join(", ", data.Keys);
            var values = string.Join(", ", data.Keys.Select(k => $"@{k}"));
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"INSERT INTO {table} ({keys}) VALUES ({values})";

            foreach (var kv in data)
                AddParameter(cmd, $"@{kv.Key}", kv.Value);

            cmd.ExecuteNonQuery();
        }

        public void Update(string table, Dictionary<string, object> data, string whereClause) {
            var setClause = string.Join(", ", data.Keys.Select(k => $"{k} = @{k}"));
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"UPDATE {table} SET {setClause} WHERE {whereClause}";

            foreach (var kv in data)
                AddParameter(cmd, $"@{kv.Key}", kv.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(string table, string whereClause) {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = $"DELETE FROM {table} WHERE {whereClause}";
            cmd.ExecuteNonQuery();
        }

        public List<Dictionary<string, object>> Query(string queryString) {
            var result = new List<Dictionary<string, object>>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = queryString;

            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[reader.GetName(i)] = reader.GetValue(i);

                    result.Add(row);
                }
            }

            return result;
        }

        private void AddParameter(IDbCommand cmd, string name, object value) {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }
    }
}
