using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace ConnectionFactory
{
    public class CfConnection : IDisposable
    {
        private readonly DbProviderFactory _dbProvider;
        private DbConnection _conn;
        private DbTransaction _tran;
        private long _tranOpenCount;

        internal enum TransactionType
        {
            TransactionOpen = 1,
            TransactionCommit = 2,
            TransactionRollback = 3
        }

        [System.Diagnostics.DebuggerStepThrough]
        public CfConnection(string connectionName)
        {
            try
            {
                Configuration = ConfigurationManager.ConnectionStrings[connectionName];

                if (Configuration == null)
                {
                    throw new CfException(String.Format("{0} connection not defined in the settings.", connectionName));
                }

                _dbProvider = DbProviderFactories.GetFactory(Configuration.ProviderName);
            }
            catch (CfException)
            {
                throw;
            }
            catch (ConfigurationException cex)
            {
                throw new CfException(String.Format("Error getting the provider to {0} connection. {1}", connectionName, cex.Message), cex);
            }
            catch (Exception ex)
            {
                throw new CfException(String.Format("We could not identify the provider for {0} connection.", connectionName), ex);
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public CfConnection(DbConnection connection)
        {
            if (connection == null)            
                throw new ArgumentException("connection not defined", "connection");
            
            _conn = connection;
#if NET45
            _dbProvider = DbProviderFactories.GetFactory(connection);
#endif
        }


        [System.Diagnostics.DebuggerStepThrough]
        public void Dispose()
        {
            CloseFactoryConnection();
            //_dbProvider = null;
            if (_tran != null) _tran.Dispose();
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal void EstablishFactoryConnection()
        {
            try
            {
                if (null == _conn)
                    _conn = _dbProvider?.CreateConnection();

                if (_conn == null || !_conn.State.Equals(ConnectionState.Closed)) return;

                _conn.ConnectionString = Configuration.ConnectionString;

                if (Transaction.Current == null)
                {
                    _conn.Open();
                }

                _tranOpenCount = 0;
            }
            catch (DbException dbe)
            {
                throw new CfException("Não foi possível se conectar ao banco de dados", dbe);
            }
            catch (Exception ex)
            {
                throw new CfException("Não foi possível se conectar ao banco de dados", ex);
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal void CloseFactoryConnection()
        {
            if (_conn == null) return;

            if (!_conn.State.Equals(ConnectionState.Closed))
            {
                if (_tranOpenCount > 0)
                {
                    TransactionHandler(TransactionType.TransactionRollback);
                }
                _conn.Close();
            }
            _tranOpenCount = 0;
            //_conn.Dispose();
            //_conn = null;

        }

        [System.Diagnostics.DebuggerStepThrough]
        internal void TransactionHandler(TransactionType transactionType, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            EstablishFactoryConnection();

            switch (transactionType)
            {
                case TransactionType.TransactionOpen:
                    _tran = _conn.BeginTransaction(isolationLevel);
                    _tranOpenCount++;
                    break;
                case TransactionType.TransactionCommit:
                    _tran.Commit();
                    _tranOpenCount--;
                    break;
                case TransactionType.TransactionRollback:
                    _tran.Rollback();
                    _tranOpenCount--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("transactionType");
            }
        }

        public ConnectionStringSettings Configuration { get; private set; }

        public ConnectionState State
        {
            get { return _conn?.State ?? ConnectionState.Closed; }
        }

        internal DbCommand CreateDbCommand()
        {
            var cmd = _conn.CreateCommand();
            if (cmd.Connection.State == ConnectionState.Closed)
            {
                cmd.Connection.Open();
            }
            //var cmd =_dbProvider.CreateCommand();
            //cmd.Connection = _conn;
            if (_tranOpenCount > 0)
            {
                cmd.Transaction = _tran;
            }
            return cmd;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public CfCommand CreateCfCommand(int commandTimeout = -1)
        {
            var cfConnection = this;
            return new CfCommand(ref cfConnection, commandTimeout);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public DbDataAdapter CreateDataAdapter()
        {
            if (_dbProvider == null)
                throw new ArgumentException("dbProvider not defined for connection");

            var dataAdp = _dbProvider?.CreateDataAdapter();
            return dataAdp;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public CfTransaction CreateTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var cfConnection = this;
            return new CfTransaction(ref cfConnection, isolationLevel);
        }
    }
}
