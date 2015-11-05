using System;
using System.Data;

namespace ConnectionFactory
{
   public class CfTransaction : IDisposable
   {
      private bool _tranIsOpen;
      private readonly CfConnection _conn;

      [System.Diagnostics.DebuggerStepThrough]
      internal CfTransaction(ref CfConnection cfConn, IsolationLevel isolationLevel)
      {
         _conn = cfConn;
         _conn.TransactionHandler(CfConnection.TransactionType.TransactionOpen, isolationLevel);
         _tranIsOpen = true;
      }

      [System.Diagnostics.DebuggerStepThrough]
      public void Commit()
      {
         if (_tranIsOpen)
         {
            _conn.TransactionHandler(CfConnection.TransactionType.TransactionCommit);
            _tranIsOpen = false;
         }
      }

      [System.Diagnostics.DebuggerStepThrough]
      public void Dispose()
      {
         if (_tranIsOpen)
         {
            _conn.TransactionHandler(CfConnection.TransactionType.TransactionRollback);
            _tranIsOpen = false;
         }
      }
   }
}
