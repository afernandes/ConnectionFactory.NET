using System;
using System.Collections.Generic;

namespace ConnectionFactory
{
    public partial class CfCommand
    {
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
        public T ExecuteScalar<T>(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return ExecuteScalar<T>(cmdType, cmdText, cfParams);
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
        public dynamic ExecuteScalar(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return ExecuteScalar(cmdType, cmdText, cfParams);
        }


        #endregion
    }
}
