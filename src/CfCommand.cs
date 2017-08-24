using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ConnectionFactory
{
    public partial class CfCommand : IDisposable
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
