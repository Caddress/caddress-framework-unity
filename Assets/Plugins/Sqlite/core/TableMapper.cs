
using System.Collections.Generic;
using System.Reflection;
using Mono.Data.Sqlite;

namespace USqlite
{ 
    public class TableMapper<T>
    {
        private readonly BaseReader m_reader = null;
        private readonly CreateInstanceDelegate m_createInstanceDelegate = null;

        public TableMapper(BaseReader reader, CreateInstanceDelegate instanceDelegate)
        {
            m_reader = reader;
            m_createInstanceDelegate = instanceDelegate;
            Orm.MappingType(typeof(T));
        }

        public List<T> ToObject()
        {
            List<T> result = new List<T>();
            var reader = m_reader.dataReader;
            while(reader.Read())
            {
                T instance = (T)m_createInstanceDelegate();
                for(int columnId = 0; columnId < reader.FieldCount; columnId++)
                {
                    var columnName = reader.GetName(columnId);
                    MemberInfo memberInfo = null;
                    if (Orm.TryGetMember(columnName, out memberInfo))
                    {
                        if (memberInfo is FieldInfo)
                        {
                            var fieldInfo = memberInfo as FieldInfo;
                            var readFunc = Orm.GetReadFunc(fieldInfo.FieldType);
                            object data = readFunc(m_reader, columnId);
                            fieldInfo.SetValue(instance,data);
                        }
                        else if ( memberInfo is PropertyInfo )
                        {
                            var propertyInfo = memberInfo as PropertyInfo;
                            var readFunc = Orm.GetReadFunc(propertyInfo.PropertyType);
                            object data = readFunc(m_reader, columnId);
                            propertyInfo.SetValue(instance,data,null);
                        }  
                    }
                }
                result.Add(instance);
            }
            m_reader?.Close();
            return result;
        }
    }
}