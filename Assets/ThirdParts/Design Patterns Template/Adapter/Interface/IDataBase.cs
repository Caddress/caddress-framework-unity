using System;
using System.Collections;
using System.Collections.Generic;

namespace Caddress.Template.Adapter {
    public interface IDataBase : IDataConnector {
        void Insert(string table, Dictionary<string, object> data);
        void Update(string table, Dictionary<string, object> data, string whereClause);
        void Delete(string table, string whereClause);
        List<Dictionary<string, object>> Query(string queryString);
    }
}