using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ConnectionFactory
{
<<<<<<< HEAD
    public partial class CfCommand
    {
        #region ExecuteNonQuery

=======
    public partial class CfCommand : IDisposable
    {
        #region ExecuteNonQuery

        //[System.Diagnostics.DebuggerStepThrough]
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
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

<<<<<<< HEAD
        public int ExecuteNonQuery(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return ExecuteNonQuery(cmdType, cmdText, cfParams);
        }

=======
>>>>>>> ba793c7ae09df0e7280087f86968eb9435428a79
        #endregion
    }
}
