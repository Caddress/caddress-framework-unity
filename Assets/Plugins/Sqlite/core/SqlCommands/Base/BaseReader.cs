using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace USqlite
{
    public class BaseReader
    {
        private SqliteDataReader m_sqliteDataReader;
        private SqliteCommand m_command;
        public SqliteDataReader dataReader { get => m_sqliteDataReader; }

        public BaseReader(SqliteCommand command)
        {
            this.m_command = command;
            this.m_sqliteDataReader = this.m_command.ExecuteReader();
        }

        public void Close()
        {
            this.m_sqliteDataReader?.Close();
            this.m_sqliteDataReader?.Dispose();
            this.m_sqliteDataReader = null;
            this.m_command?.Cancel();
            this.m_command?.Dispose();
            this.m_command = null;
        }
    }
}
