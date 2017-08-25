using System;
using System.Collections.Generic;
using System.Data;

namespace ConnectionFactory
{
<<<<<<< HEAD
    public partial class CfCommand
=======
    public partial class CfCommand : IDisposable
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
    {
        #region QueryForList

        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>list of entities</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> QueryForList<T>(
            CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null) where T : new()
        {
            return QueryForList<T>(ExecuteReader(cmdType, cmdText, cmdParms));
        }

<<<<<<< HEAD
        public IList<T> QueryForList<T>(
            CfCommandType cmdType, string cmdText, object cmdParms) where T : new()
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return QueryForList<T>(cmdType, cmdText, cfParams);
        }

=======
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="dr">datareader loaded</param>
        /// <returns>list of entities</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static IList<T> QueryForList<T>(IDataReader dr) where T : new()
        {
            Logger.Debug("Begin method");

            Type entityType = typeof(T);
            var entitys = new List<T>();
            try
            {
                var properties = CfMapCache.GetInstance(entityType);
                while (dr.Read())
                {
                    var newObject = new T();
                    for (var index = 0; index < dr.FieldCount; index++)
                    {
                        if (properties.ContainsKey(dr.GetName(index).ToUpper()))
                        {
                            var info = properties[dr.GetName(index).ToUpper()];
                            if ((info != null) && info.CanWrite && !dr.IsDBNull(index))
                            {
                                try
                                {
                                    info.SetValue(newObject, dr.GetValue(index), null);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                    throw new CfException(String.Format("Conversion error in {0}.{1} - (Connection Factory: QueryForList) > {2}", entityType.Name, dr.GetName(index), ex.Message), ex);
                                }
                            }
                        }
                        else
                        {
                            Logger.Debug("Property not exist: " + entityType.FullName + dr.GetName(index));
                        }
                    }
                    entitys.Add(newObject);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new CfException("Unknown Error (Connection Factory: QueryForList) " + ex.Message, ex);
            }
            finally
            {
                dr.Close();
            }
            Logger.Debug("End method");

            return (entitys.Count > 0) ? entitys : default(List<T>);

        }

        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>list of entities (ExpandoObject)</returns>
        public IList<dynamic> QueryForList(
            CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            return QueryForList(ExecuteReader(cmdType, cmdText, cmdParms));
        }

<<<<<<< HEAD
        public IList<dynamic> QueryForList(
            CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return QueryForList(cmdType, cmdText, cfParams);
        }

=======
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <param name="dr">datareader loaded</param>
        /// <returns>list of entities (ExpandoObject)</returns>
        public static IList<dynamic> QueryForList(IDataReader dr)
        {
            Logger.Debug("Begin method");

            var returnValue = new List<dynamic>();

            while (dr.Read())
            {
                returnValue.Add(dr.ToExpando());
            }

            Logger.Debug("End method");
            return (returnValue.Count > 0) ? returnValue : default(List<dynamic>); ;

        }

        #endregion
    }
}
