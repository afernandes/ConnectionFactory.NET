using System;
using System.Collections.Generic;
using System.Data;

namespace ConnectionFactory
{
    public partial class CfCommand
    {
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

        public DataSet DataAdapter(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return DataAdapter(cmdType, cmdText, cfParams);
        }

        #endregion
    }
}
