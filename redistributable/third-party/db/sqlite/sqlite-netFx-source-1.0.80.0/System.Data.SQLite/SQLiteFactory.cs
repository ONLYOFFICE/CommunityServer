/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Data.Common;

#if !PLATFORM_COMPACTFRAMEWORK
  /// <summary>
  /// SQLite implementation of DbProviderFactory.
  /// </summary>
  public sealed partial class SQLiteFactory : DbProviderFactory, IDisposable
  {
    /// <overloads>
    /// Constructs a new SQLiteFactory object
    /// </overloads>
    /// <summary>
    /// Default constructor
    /// </summary>
    public SQLiteFactory()
    {
        //
        // NOTE: Do nothing here now.  All the logging setup related code has
        //       been moved to the new SQLiteLog static class.
        //
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
            throw new ObjectDisposedException(typeof(SQLiteFactory).Name);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            //if (disposing)
            //{
            //    ////////////////////////////////////
            //    // dispose managed resources here...
            //    ////////////////////////////////////
            //}

            //////////////////////////////////////
            // release unmanaged resources here...
            //////////////////////////////////////

            disposed = true;
        }
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region Destructor
    ~SQLiteFactory()
    {
        Dispose(false);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This event is raised whenever SQLite raises a logging event.
    /// Note that this should be set as one of the first things in the
    /// application.  This event is provided for backward compatibility only.
    /// New code should use the SQLiteLog class instead.
    /// </summary>
    public event SQLiteLogEventHandler Log
    {
      add { CheckDisposed(); SQLiteLog.Log += value; }
      remove { CheckDisposed(); SQLiteLog.Log -= value; }
    }

    /// <summary>
    /// Static instance member which returns an instanced SQLiteFactory class.
    /// </summary>
    public static readonly SQLiteFactory Instance = new SQLiteFactory();

    /// <summary>
    /// Returns a new SQLiteCommand object.
    /// </summary>
    /// <returns>A SQLiteCommand object.</returns>
    public override DbCommand CreateCommand()
    {
      CheckDisposed();
      return new SQLiteCommand();
    }

    /// <summary>
    /// Returns a new SQLiteCommandBuilder object.
    /// </summary>
    /// <returns>A SQLiteCommandBuilder object.</returns>
    public override DbCommandBuilder CreateCommandBuilder()
    {
      CheckDisposed();
      return new SQLiteCommandBuilder();
    }

    /// <summary>
    /// Creates a new SQLiteConnection.
    /// </summary>
    /// <returns>A SQLiteConnection object.</returns>
    public override DbConnection CreateConnection()
    {
      CheckDisposed();
      return new SQLiteConnection();
    }

    /// <summary>
    /// Creates a new SQLiteConnectionStringBuilder.
    /// </summary>
    /// <returns>A SQLiteConnectionStringBuilder object.</returns>
    public override DbConnectionStringBuilder CreateConnectionStringBuilder()
    {
      CheckDisposed();
      return new SQLiteConnectionStringBuilder();
    }

    /// <summary>
    /// Creates a new SQLiteDataAdapter.
    /// </summary>
    /// <returns>A SQLiteDataAdapter object.</returns>
    public override DbDataAdapter CreateDataAdapter()
    {
      CheckDisposed();
      return new SQLiteDataAdapter();
    }

    /// <summary>
    /// Creates a new SQLiteParameter.
    /// </summary>
    /// <returns>A SQLiteParameter object.</returns>
    public override DbParameter CreateParameter()
    {
      CheckDisposed();
      return new SQLiteParameter();
    }
  }
#endif
}
