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
        #region LazyLoadForObjects

        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>list of entities</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerable<T> LazyLoadForObjects<T>(
            CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null) where T : new()
        {
            return LazyLoadForObjects<T>(LazyLoad(cmdType, cmdText, cmdParms));
        }

<<<<<<< HEAD
        public IEnumerable<T> LazyLoadForObjects<T>(
            CfCommandType cmdType, string cmdText, object cmdParms) where T : new()
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return LazyLoadForObjects<T>(cmdType, cmdText, cfParams);
        }

=======
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
        /// <summary>
        /// LazyLoadForObjects Performs and returns the list loaded entities
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="dr">datareader loaded</param>
        /// <returns>list of entities</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static IEnumerable<T> LazyLoadForObjects<T>(IEnumerable<IDataReader> dr) where T : new()
        {
            Logger.Debug("Begin method");

            Type entityType = typeof(T);
            var properties = CfMapCache.GetInstance(entityType);

            using (var enumerator = dr.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var reader = enumerator.Current;

                    if (reader != null)
                    {
                        var newObject = new T();
                        for (var index = 0; index < reader.FieldCount; index++)
                        {
                            if (properties.ContainsKey(reader.GetName(index).ToUpper()))
                            {
                                var info = properties[reader.GetName(index).ToUpper()];
                                if ((info != null) && info.CanWrite && !reader.IsDBNull(index))
                                {
                                    try
                                    {
                                        info.SetValue(newObject, reader.GetValue(index), null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex);
                                        throw new CfException(String.Format("Conversion error in {0}.{1} - (Connection Factory: LazyLoadForObjects) > {2}", entityType.Name, reader.GetName(index), ex.Message), ex);
                                    }
                                }
                            }
                            else
                            {
                                Logger.Debug("Property not exist: " + entityType.FullName + reader.GetName(index));
                            }
                        }
                        yield return newObject;
                    }
                }
            }
            Logger.Debug("End method");
        }

        public IEnumerable<dynamic> LazyLoadForObjects(
            CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            return LazyLoadForObjects(LazyLoad(cmdType, cmdText, cmdParms));
        }

<<<<<<< HEAD
        public IEnumerable<dynamic> LazyLoadForObjects(
            CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return LazyLoadForObjects(cmdType, cmdText, cfParams);
        }

=======
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
        public static IEnumerable<dynamic> LazyLoadForObjects(IEnumerable<IDataReader> dr)
        {
            using (var enumerator = dr.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var reader = enumerator.Current;

                    if (reader != null)
                    {
                        yield return reader.ToExpando();
                    }
                }
            }
        }

        #endregion
    }
}
