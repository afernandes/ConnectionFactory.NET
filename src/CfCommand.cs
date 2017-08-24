using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using Microsoft.CSharp.RuntimeBinder;

namespace ConnectionFactory
{
    public class CfCommand : IDisposable
    {
        /// <summary>
        /// Logger object.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CfCommand));

        private readonly CfConnection _conn;
        private DbCommand _cmd;

        [System.Diagnostics.DebuggerStepThrough]
        internal CfCommand(ref CfConnection conn)
        {
            _conn = conn;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public void Dispose()
        {
            if (_cmd == null) return;
            _cmd.Dispose();
            _cmd = null;
        }

        #region ExecuteNonQuery

        //[System.Diagnostics.DebuggerStepThrough]
        public int ExecuteNonQuery(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            Logger.Debug("Begin Method");
            try
            {
                Logger.InfoFormat("ExecuteNonQuery: {0}", cmdText);
                Logger.Info(cmdParms);

                _conn.EstablishFactoryConnection();
                PrepareCommand(cmdType, cmdText, cmdParms);

                Logger.Debug("End Method");
                return _cmd.ExecuteNonQuery();
            }
            catch (DbException dbe)
            {
                Logger.Error(dbe);
                throw new CfException("Unknown error in database: " + dbe.Message, dbe);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new CfException("Error on Connection Factory Mechanism: " + ex.Message, ex);
            }
        }

        #endregion

        #region LazyLoad

        /// <summary>
        /// Performs LazyLoad
        /// </summary>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>IEnumerable IDataRecord</returns> 
        [System.Diagnostics.DebuggerStepThrough]
        public IEnumerable<IDataReader> LazyLoad(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            Logger.InfoFormat("LazyLoad: {0}", cmdText);
            Logger.Info(cmdParms);

            _conn.EstablishFactoryConnection();
            PrepareCommand(cmdType, cmdText, cmdParms);

            using (var reader = _cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                {
                    yield return reader;
                }
            }
        }

        #endregion

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

        #region ExecuteReader

        /// <summary>
        /// Performs datareader
        /// </summary>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>Data Reader</returns> 
        [System.Diagnostics.DebuggerStepThrough]
        public IDataReader ExecuteReader(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            try
            {
                Logger.InfoFormat("ExecuteReader: {0}", cmdText);
                Logger.Info(cmdParms);

                _conn.EstablishFactoryConnection();
                PrepareCommand(cmdType, cmdText, cmdParms);
                return _cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new CfException("Unknown Error (Connection Factory: ExecuteReader) " + ex.Message, ex);
            }
        }

        public IDataReader ExecuteReader(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            try
            {
                IList<CfParameter> cfParams = null;
                if (cmdParms != null)
                {
                    if (!(cmdParms is IEnumerable<CfParameter>))
                    {

                        var props = cmdParms as ExpandoObject as IDictionary<string, object>;
                        if (props != null)
                        {
                            cfParams = new List<CfParameter>(props.Count());
                            foreach (var p in props)
                            {
                                cfParams.Add(new CfParameter(p.Key, p.Value));
                            }
                        }
                        else
                        {
                            var properties = cmdParms.GetType().GetProperties();
                            if (properties.Any())
                            {
                                cfParams = new List<CfParameter>(properties.Count());
                                foreach (var property in properties)
                                {
                                    cfParams.Add(new CfParameter(property.Name, property.GetValue(cmdParms, null)));
                                }
                            }
                        }
                    }
                }

                _conn.EstablishFactoryConnection();
                PrepareCommand(cmdType, cmdText, cfParams);
                return _cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new CfException("Unknown Error (Connection Factory: ExecuteReader) " + ex.Message, ex);
            }
        }





        #endregion

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

        public static dynamic QueryForObject(IDataReader dr)
        {
            return dr.Read() ? dr.ToExpando() : null;
        }

        #endregion

        #region ExecuteScalar

        [System.Diagnostics.DebuggerStepThrough]
        public T ExecuteScalar<T>(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            try
            {
                Logger.InfoFormat("ExecuteScalar: {0}", cmdText);
                Logger.Info(cmdParms);

                _conn.EstablishFactoryConnection();
                PrepareCommand(cmdType, cmdText, cmdParms);
                var returnValue = _cmd.ExecuteScalar();
                _cmd.Dispose();
                if (returnValue is DBNull || returnValue == null)
                {
                    return default(T);
                }
                try
                {
                    return (T)returnValue;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    throw new CfException(String.Format("Conversion error in ({0})\"{1}\" - (Connection Factory: ExecuteScalar) > {2}", typeof(T).Name, returnValue, ex.Message), ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new CfException("Unknown Error (Connection Factory: ExecuteScalar) " + ex.Message, ex);
            }
        }

        public dynamic ExecuteScalar(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            _conn.EstablishFactoryConnection();
            PrepareCommand(cmdType, cmdText, cmdParms);
            dynamic returnValue = _cmd.ExecuteScalar();
            _cmd.Dispose();

            if (returnValue is DBNull || returnValue == null)
            {
                return default(dynamic);
            }

            return returnValue;
        }

        #endregion

        #region DataAdapter

        /// <summary>
        /// DataAdapter Performs and returns the DataSet
        /// </summary>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>DataSet</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public DataSet DataAdapter(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                Logger.InfoFormat("DataAdapter: {0}", cmdText);
                Logger.Info(cmdParms);

                _conn.EstablishFactoryConnection();
                var dda = _conn.CreateDataAdapter();
                PrepareCommand(cmdType, cmdText, cmdParms);

                dda.SelectCommand = _cmd;
                var ds = new DataSet();
                dda.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new CfException("Unknown Error (Connection Factory: DataAdapter) " + ex.Message, ex);
            }
        }

        #endregion

        #region PREPARE COMMAND AND CREATE PARAMETERS (Private Methods)

        [System.Diagnostics.DebuggerStepThrough]
        private void PrepareCommand(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            _conn.EstablishFactoryConnection();

            if (null != _cmd)
                _cmd.Dispose();

            _cmd = _conn.CreateDbCommand();

            _cmd.CommandText = cmdText;
            _cmd.CommandType = (CommandType)cmdType;

            CreateDbParameters(cmdParms);
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void CreateDbParameters(IEnumerable<CfParameter> colParameters)
        {
            _cmd.Parameters.Clear();

            if (colParameters == null) return;

            foreach (var param in colParameters)
            {
                _cmd.Parameters.Add(CreateDbParameter(param.ParamName, param.ParamValue, param.ParamDbType, param.ParamDirection));
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private DbParameter CreateDbParameter(string name, object value, DbType dbType = DbType.AnsiString, ParameterDirection direction = ParameterDirection.Input, bool isNullable = true)
        {
            var param = _cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            param.DbType = dbType;
            param.Direction = direction;
            param.IsNullable = isNullable;
            return param;
        }

        #endregion

    }

    public enum CfCommandType
    {
        Text = 1,
        StoredProcedure = 4,
        TableDirect = 512,
    }

}
