using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace ConnectionFactory
{
<<<<<<< HEAD
    public partial class CfCommand
=======
    public partial class CfCommand : IDisposable
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
    {
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
<<<<<<< HEAD
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return ExecuteReader(cmdType, cmdText, cfParams);
=======
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
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
        }

        #endregion
    }
}
