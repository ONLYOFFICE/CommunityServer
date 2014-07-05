/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

#if !PLATFORM_COMPACTFRAMEWORK
namespace System.Data.SQLite
{
  using System.Transactions;

  internal class SQLiteEnlistment : IEnlistmentNotification, IDisposable
  {
    internal SQLiteTransaction _transaction;
    internal Transaction _scope;
    internal bool _disposeConnection;

    internal SQLiteEnlistment(SQLiteConnection cnn, Transaction scope)
    {
      _transaction = cnn.BeginTransaction();
      _scope = scope;

      _scope.EnlistVolatile(this, System.Transactions.EnlistmentOptions.None);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable Members
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable "Pattern" Members
    private bool disposed;
    private void CheckDisposed() /* throw */
    {
#if THROW_ON_DISPOSED
        if (disposed)
            throw new ObjectDisposedException(typeof(SQLiteEnlistment).Name);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                ////////////////////////////////////
                // dispose managed resources here...
                ////////////////////////////////////

                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }

                if (_scope != null)
                {
                    // _scope.Dispose(); // NOTE: Not "owned" by us.
                    _scope = null;
                }
            }

            //////////////////////////////////////
            // release unmanaged resources here...
            //////////////////////////////////////

            disposed = true;
        }
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region Destructor
    ~SQLiteEnlistment()
    {
        Dispose(false);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    private void Cleanup(SQLiteConnection cnn)
    {
      if (_disposeConnection)
        cnn.Dispose();

      _transaction = null;
      _scope = null;
    }

    #region IEnlistmentNotification Members

    public void Commit(Enlistment enlistment)
    {
      CheckDisposed();

      SQLiteConnection cnn = _transaction.Connection;
      cnn._enlistment = null;

      try
      {
        _transaction.IsValid(true);
        _transaction.Connection._transactionLevel = 1;
        _transaction.Commit();

        enlistment.Done();
      }
      finally
      {
        Cleanup(cnn);
      }
    }

    public void InDoubt(Enlistment enlistment)
    {
      CheckDisposed();
      enlistment.Done();
    }

    public void Prepare(PreparingEnlistment preparingEnlistment)
    {
      CheckDisposed();

      if (_transaction.IsValid(false) == false)
        preparingEnlistment.ForceRollback();
      else
        preparingEnlistment.Prepared();
    }

    public void Rollback(Enlistment enlistment)
    {
      CheckDisposed();

      SQLiteConnection cnn = _transaction.Connection;
      cnn._enlistment = null;

      try
      {
        _transaction.Rollback();
        enlistment.Done();
      }
      finally
      {
        Cleanup(cnn);
      }
    }

    #endregion
  }
}
#endif // !PLATFORM_COMPACT_FRAMEWORK