using System;
using System.Collections.Generic;
using System.Data;

namespace ConnectionFactory
{
    public partial class CfCommand
    {
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

        public IEnumerable<IDataReader> LazyLoad(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return LazyLoad(cmdType, cmdText, cfParams);
        }

        #endregion
    }
}
