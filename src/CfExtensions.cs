using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace ConnectionFactory
{
    public static class CfExtensions
    {
        public static dynamic ToExpando(this IDataReader reader)
        {
            var dictionary = new ExpandoObject() as IDictionary<string, object>;
            for (int i = 0; i < reader.FieldCount; i++)
                dictionary.Add(reader.GetName(i), reader[i] == DBNull.Value ? null : reader[i]);

            return dictionary as ExpandoObject;
        }

    }
}
