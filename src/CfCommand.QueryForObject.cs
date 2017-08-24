using System;
using System.Collections.Generic;
using System.Data;

namespace ConnectionFactory
{
    public partial class CfCommand
    {
        #region QueryForObject

        /// <summary>
        /// Performs datareader and returns loaded entity
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>Entity</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public T QueryForObject<T>(CfCommandType cmdType, string cmdText,
            IEnumerable<CfParameter> cmdParms = null) where T : new()
        {
            return QueryForObject<T>(ExecuteReader(cmdType, cmdText, cmdParms));
        }

        public T QueryForObject<T>(CfCommandType cmdType, string cmdText,
            object cmdParms) where T : new()
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return QueryForObject<T>(cmdType, cmdText, cfParams);
        }

        /// <summary>
        /// Performs datareader and returns loaded entity
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="dr">datareader loaded</param>
        /// <returns>Entity</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static T QueryForObject<T>(IDataReader dr) where T : new()
        {
            Logger.Debug("Begin method");

            var entityType = typeof(T);
            var entity = default(T);

            try
            {
                var properties = CfMapCache.GetInstance(entityType);
                if (dr.Read())
                {
                    entity = new T();

                    for (var index = 0; index < dr.FieldCount; index++)
                    {
                        if (properties.ContainsKey(dr.GetName(index).ToUpper()))
                        {
                            var info = properties[dr.GetName(index).ToUpper()];
                            if (info != null && info.CanWrite && !dr.IsDBNull(index))
                            {
                                try
                                {
                                    info.SetValue(entity, dr.GetValue(index), null);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                    throw new CfException(String.Format("Conversion error in {0}.{1} - (Connection Factory: QueryForObject) > {2}", entityType.Name, dr.GetName(index), ex.Message), ex);
                                }
                            }
                        }
                        else
                        {
                            Logger.Debug("Property not exist: " + entityType.FullName + "." + dr.GetName(index));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new CfException("Unknown Error (Connection Factory: QueryForObject) " + ex.Message, ex);
            }
            finally
            {
                dr.Close();
            }

            Logger.Debug("End method");
            return entity;
        }

        public dynamic QueryForObject(
            CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            return QueryForObject(ExecuteReader(cmdType, cmdText, cmdParms));
        }

        public dynamic QueryForObject(
            CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return QueryForObject(cmdType, cmdText, cfParams);
        }

        public static dynamic QueryForObject(IDataReader dr)
        {
            return dr.Read() ? dr.ToExpando() : null;
        }

        #endregion
    }
}
