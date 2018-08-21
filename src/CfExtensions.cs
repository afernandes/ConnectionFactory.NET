using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

        #region QueryForList
        public static IList<T> QueryForList<T>(this DbConnection db,
            string cmdText, object cmdParms) where T : new()
        {
            using(var conn = new CfConnection(db))
            {
                var cmd = conn.CreateCfCommand();
                return cmd.QueryForList<T>(cmdText, cmdParms);
            }            
        }

        public static IList<dynamic> QueryForList(this DbConnection db,
            string cmdText, object cmdParms)
        {
            using (var conn = new CfConnection(db))
            {
                var cmd = conn.CreateCfCommand();
                return cmd.QueryForList(cmdText, cmdParms);
            }
        }
        #endregion

        #region QueryForObject
        public static T QueryForObject<T>(this DbConnection db, 
            string cmdText, object cmdParms) where T : new()
        {
            using (var conn = new CfConnection(db))
            {
                var cmd = conn.CreateCfCommand();
                return cmd.QueryForObject<T>(cmdText, cmdParms);
            }
        }

        public static dynamic QueryForObject(this DbConnection db,
            string cmdText, object cmdParms)
        {
            using (var conn = new CfConnection(db))
            {
                var cmd = conn.CreateCfCommand();
                return cmd.QueryForObject(cmdText, cmdParms);
            }
        }
        #endregion
    }
}
