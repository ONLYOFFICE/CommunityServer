/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;

  /// <summary>
  /// This internal class provides the foundation of SQLite support.  It defines all the abstract members needed to implement
  /// a SQLite data provider, and inherits from SQLiteConvert which allows for simple translations of string to and from SQLite.
  /// </summary>
  internal abstract class SQLiteBase : SQLiteConvert, IDisposable
  {
    internal SQLiteBase(SQLiteDateFormats fmt, DateTimeKind kind)
      : base(fmt, kind) { }

    static internal object _lock = new object();

    /// <summary>
    /// Returns a string representing the active version of SQLite
    /// </summary>
    internal abstract string Version { get; }
    /// <summary>
    /// Returns the rowid of the most recent successful INSERT into the database from this connection.
    /// </summary>
    internal abstract long LastInsertRowId { get; }
    /// <summary>
    /// Returns the number of changes the last executing insert/update caused.
    /// </summary>
    internal abstract int Changes { get; }
    /// <summary>
    /// Returns the amount of memory (in bytes) currently in use by the SQLite core library.
    /// </summary>
    internal abstract long MemoryUsed { get; }
    /// <summary>
    /// Returns the maximum amount of memory (in bytes) used by the SQLite core library since the high-water mark was last reset.
    /// </summary>
    internal abstract long MemoryHighwater { get; }
    /// <summary>
    /// Shutdown the SQLite engine so that it can be restarted with different config options.
    /// We depend on auto initialization to recover.
    /// </summary>
    internal abstract int Shutdown();
    /// <summary>
    /// Returns non-zero if a database connection is open.
    /// </summary>
    /// <returns></returns>
    internal abstract bool IsOpen();
    /// <summary>
    /// Opens a database.
    /// </summary>
    /// <remarks>
    /// Implementers should call SQLiteFunction.BindFunctions() and save the array after opening a connection
    /// to bind all attributed user-defined functions and collating sequences to the new connection.
    /// </remarks>
    /// <param name="strFilename">The filename of the database to open.  SQLite automatically creates it if it doesn't exist.</param>
    /// <param name="connectionFlags">The flags associated with the parent connection object</param>
    /// <param name="openFlags">The open flags to use when creating the connection</param>
    /// <param name="maxPoolSize">The maximum size of the pool for the given filename</param>
    /// <param name="usePool">If true, the connection can be pulled from the connection pool</param>
    internal abstract void Open(string strFilename, SQLiteConnectionFlags connectionFlags, SQLiteOpenFlagsEnum openFlags, int maxPoolSize, bool usePool);
    /// <summary>
    /// Closes the currently-open database.
    /// </summary>
    /// <remarks>
    /// After the database has been closed implemeters should call SQLiteFunction.UnbindFunctions() to deallocate all interop allocated
    /// memory associated with the user-defined functions and collating sequences tied to the closed connection.
    /// </remarks>
    internal abstract void Close();
    /// <summary>
    /// Sets the busy timeout on the connection.  SQLiteCommand will call this before executing any command.
    /// </summary>
    /// <param name="nTimeoutMS">The number of milliseconds to wait before returning SQLITE_BUSY</param>
    internal abstract void SetTimeout(int nTimeoutMS);
    /// <summary>
    /// Returns the text of the last error issued by SQLite
    /// </summary>
    /// <returns></returns>
    internal abstract string SQLiteLastError();

    /// <summary>
    /// When pooling is enabled, force this connection to be disposed rather than returned to the pool
    /// </summary>
    internal abstract void ClearPool();

    /// <summary>
    /// Prepares a SQL statement for execution.
    /// </summary>
    /// <param name="cnn">The source connection preparing the command.  Can be null for any caller except LINQ</param>
    /// <param name="strSql">The SQL command text to prepare</param>
    /// <param name="previous">The previous statement in a multi-statement command, or null if no previous statement exists</param>
    /// <param name="timeoutMS">The timeout to wait before aborting the prepare</param>
    /// <param name="strRemain">The remainder of the statement that was not processed.  Each call to prepare parses the
    /// SQL up to to either the end of the text or to the first semi-colon delimiter.  The remaining text is returned
    /// here for a subsequent call to Prepare() until all the text has been processed.</param>
    /// <returns>Returns an initialized SQLiteStatement.</returns>
    internal abstract SQLiteStatement Prepare(SQLiteConnection cnn, string strSql, SQLiteStatement previous, uint timeoutMS, out string strRemain);
    /// <summary>
    /// Steps through a prepared statement.
    /// </summary>
    /// <param name="stmt">The SQLiteStatement to step through</param>
    /// <returns>True if a row was returned, False if not.</returns>
    internal abstract bool Step(SQLiteStatement stmt);
    /// <summary>
    /// Resets a prepared statement so it can be executed again.  If the error returned is SQLITE_SCHEMA, 
    /// transparently attempt to rebuild the SQL statement and throw an error if that was not possible.
    /// </summary>
    /// <param name="stmt">The statement to reset</param>
    /// <returns>Returns -1 if the schema changed while resetting, 0 if the reset was sucessful or 6 (SQLITE_LOCKED) if the reset failed due to a lock</returns>
    internal abstract int Reset(SQLiteStatement stmt);
    internal abstract void Cancel();

    internal abstract void Bind_Double(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, double value);
    internal abstract void Bind_Int32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, Int32 value);
    internal abstract void Bind_UInt32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, UInt32 value);
    internal abstract void Bind_Int64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, Int64 value);
    internal abstract void Bind_UInt64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, UInt64 value);
    internal abstract void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value);
    internal abstract void Bind_Blob(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, byte[] blobData);
    internal abstract void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt);
    internal abstract void Bind_Null(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index);

    internal abstract int Bind_ParamCount(SQLiteStatement stmt, SQLiteConnectionFlags flags);
    internal abstract string Bind_ParamName(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index);
    internal abstract int Bind_ParamIndex(SQLiteStatement stmt, SQLiteConnectionFlags flags, string paramName);

    internal abstract int ColumnCount(SQLiteStatement stmt);
    internal abstract string ColumnName(SQLiteStatement stmt, int index);
    internal abstract TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index);
    internal abstract string ColumnType(SQLiteStatement stmt, int index, out TypeAffinity nAffinity);
    internal abstract int ColumnIndex(SQLiteStatement stmt, string columnName);
    internal abstract string ColumnOriginalName(SQLiteStatement stmt, int index);
    internal abstract string ColumnDatabaseName(SQLiteStatement stmt, int index);
    internal abstract string ColumnTableName(SQLiteStatement stmt, int index);
    internal abstract void ColumnMetaData(string dataBase, string table, string column, out string dataType, out string collateSequence, out bool notNull, out bool primaryKey, out bool autoIncrement);
    internal abstract void GetIndexColumnExtendedInfo(string database, string index, string column, out int sortMode, out int onError, out string collationSequence);

    internal abstract double GetDouble(SQLiteStatement stmt, int index);
    internal abstract Int32 GetInt32(SQLiteStatement stmt, int index);
    internal abstract Int64 GetInt64(SQLiteStatement stmt, int index);
    internal abstract string GetText(SQLiteStatement stmt, int index);
    internal abstract long GetBytes(SQLiteStatement stmt, int index, int nDataoffset, byte[] bDest, int nStart, int nLength);
    internal abstract long GetChars(SQLiteStatement stmt, int index, int nDataoffset, char[] bDest, int nStart, int nLength);
    internal abstract DateTime GetDateTime(SQLiteStatement stmt, int index);
    internal abstract bool IsNull(SQLiteStatement stmt, int index);

    internal abstract void CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16);
    internal abstract void CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal);
    internal abstract CollationSequence GetCollationSequence(SQLiteFunction func, IntPtr context);
    internal abstract int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2);
    internal abstract int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2);

    internal abstract int AggregateCount(IntPtr context);
    internal abstract IntPtr AggregateContext(IntPtr context);

    internal abstract long GetParamValueBytes(IntPtr ptr, int nDataOffset, byte[] bDest, int nStart, int nLength);
    internal abstract double GetParamValueDouble(IntPtr ptr);
    internal abstract int GetParamValueInt32(IntPtr ptr);
    internal abstract Int64 GetParamValueInt64(IntPtr ptr);
    internal abstract string GetParamValueText(IntPtr ptr);
    internal abstract TypeAffinity GetParamValueType(IntPtr ptr);

    internal abstract void ReturnBlob(IntPtr context, byte[] value);
    internal abstract void ReturnDouble(IntPtr context, double value);
    internal abstract void ReturnError(IntPtr context, string value);
    internal abstract void ReturnInt32(IntPtr context, Int32 value);
    internal abstract void ReturnInt64(IntPtr context, Int64 value);
    internal abstract void ReturnNull(IntPtr context);
    internal abstract void ReturnText(IntPtr context, string value);

    /// <summary>
    /// Enables or disabled extened result codes returned by SQLite
    /// </summary>
    /// <param name="bOnOff">true to enable extended result codes, false to disable.</param>
    /// <returns></returns>
    internal abstract void SetExtendedResultCodes(bool bOnOff);
    /// <summary>
    /// Returns the numeric result code for the most recent failed SQLite API call 
    /// associated with the database connection. 
    /// </summary>
    /// <returns>Result code</returns>
    internal abstract int ResultCode();
    /// <summary>
    /// Returns the extended numeric result code for the most recent failed SQLite API call 
    /// associated with the database connection. 
    /// </summary>
    /// <returns>Extended result code</returns>
    internal abstract int ExtendedResultCode();

    /// <summary>
    /// Add a log message via the SQLite sqlite3_log interface.
    /// </summary>
    /// <param name="iErrCode">Error code to be logged with the message.</param>
    /// <param name="zMessage">String to be logged.  Unlike the SQLite sqlite3_log() 
    /// interface, this should be pre-formatted.  Consider using the 
    /// String.Format() function.</param>
    /// <returns></returns>
    internal abstract void LogMessage(int iErrCode, string zMessage);

    internal abstract void SetPassword(byte[] passwordBytes);
    internal abstract void ChangePassword(byte[] newPasswordBytes);

    internal abstract void SetUpdateHook(SQLiteUpdateCallback func);
    internal abstract void SetCommitHook(SQLiteCommitCallback func);
    internal abstract void SetTraceCallback(SQLiteTraceCallback func);
    internal abstract void SetRollbackHook(SQLiteRollbackCallback func);
    internal abstract int SetLogCallback(SQLiteLogCallback func);
    internal abstract bool IsInitialized();

    internal abstract int GetCursorForTable(SQLiteStatement stmt, int database, int rootPage);
    internal abstract long GetRowIdForCursor(SQLiteStatement stmt, int cursor);

    internal abstract object GetValue(SQLiteStatement stmt, int index, SQLiteType typ);

    internal abstract bool AutoCommit
    {
      get;
    }

    internal abstract int FileControl(string zDbName, int op, IntPtr pArg);

    /// <summary>
    /// Creates a new SQLite backup object based on the provided destination
    /// database connection.  The source database connection is the one
    /// associated with this object.  The source and destination database
    /// connections cannot be the same.
    /// </summary>
    /// <param name="destCnn">The destination database connection.</param>
    /// <param name="destName">The destination database name.</param>
    /// <param name="sourceName">The source database name.</param>
    /// <returns>The newly created backup object.</returns>
    internal abstract SQLiteBackup InitializeBackup(
        SQLiteConnection destCnn, string destName,
        string sourceName);

    /// <summary>
    /// Copies up to N pages from the source database to the destination
    /// database associated with the specified backup object.
    /// </summary>
    /// <param name="backup">The backup object to use.</param>
    /// <param name="nPage">
    /// The number of pages to copy or negative to copy all remaining pages.
    /// </param>
    /// <param name="retry">
    /// Set to true if the operation needs to be retried due to database
    /// locking issues.
    /// </param>
    /// <returns>
    /// True if there are more pages to be copied, false otherwise.
    /// </returns>
    internal abstract bool StepBackup(SQLiteBackup backup, int nPage, out bool retry);

    /// <summary>
    /// Returns the number of pages remaining to be copied from the source
    /// database to the destination database associated with the specified
    /// backup object.
    /// </summary>
    /// <param name="backup">The backup object to check.</param>
    /// <returns>The number of pages remaining to be copied.</returns>
    internal abstract int RemainingBackup(SQLiteBackup backup);

    /// <summary>
    /// Returns the total number of pages in the source database associated
    /// with the specified backup object.
    /// </summary>
    /// <param name="backup">The backup object to check.</param>
    /// <returns>The total number of pages in the source database.</returns>
    internal abstract int PageCountBackup(SQLiteBackup backup);

    /// <summary>
    /// Destroys the backup object, rolling back any backup that may be in
    /// progess.
    /// </summary>
    /// <param name="backup">The backup object to destroy.</param>
    internal abstract void FinishBackup(SQLiteBackup backup);

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
            throw new ObjectDisposedException(typeof(SQLiteBase).Name);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    protected virtual void Dispose(bool disposing)
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
    ~SQLiteBase()
    {
        Dispose(false);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    // These statics are here for lack of a better place to put them.
    // They exist here because they are called during the finalization of
    // a SQLiteStatementHandle, SQLiteConnectionHandle, and SQLiteFunctionCookieHandle.
    // Therefore these functions have to be static, and have to be low-level.

    internal static string SQLiteLastError(SQLiteConnectionHandle db)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_errmsg_interop(db, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_errmsg(db), -1);
#endif
    }

    internal static void FinishBackup(SQLiteBackupHandle backup)
    {
        lock (_lock)
        {
            int n = UnsafeNativeMethods.sqlite3_backup_finish(backup);
            backup.SetHandleAsInvalid();
            if (n > 0) throw new SQLiteException(n, null);
        }
    }

    internal static void FinalizeStatement(SQLiteStatementHandle stmt)
    {
      lock (_lock)
      {
#if !SQLITE_STANDARD
        int n = UnsafeNativeMethods.sqlite3_finalize_interop(stmt);
#else
      int n = UnsafeNativeMethods.sqlite3_finalize(stmt);
#endif
        if (n > 0) throw new SQLiteException(n, null);
      }
    }

    internal static void CloseConnection(SQLiteConnectionHandle db)
    {
      lock (_lock)
      {
#if !SQLITE_STANDARD
        int n = UnsafeNativeMethods.sqlite3_close_interop(db);
#else
      ResetConnection(db);
      int n = UnsafeNativeMethods.sqlite3_close(db);
#endif
        if (n > 0) throw new SQLiteException(n, SQLiteLastError(db));
      }
    }

    internal static void ResetConnection(SQLiteConnectionHandle db)
    {
      lock (_lock)
      {
        IntPtr stmt = IntPtr.Zero;
        int n;
        do
        {
          stmt = UnsafeNativeMethods.sqlite3_next_stmt(db, stmt);
          if (stmt != IntPtr.Zero)
          {
#if !SQLITE_STANDARD
            n = UnsafeNativeMethods.sqlite3_reset_interop(stmt);
#else
            n = UnsafeNativeMethods.sqlite3_reset(stmt);
#endif
          }
        } while (stmt != IntPtr.Zero);

        if (IsAutocommit(db) == false) // a transaction is pending on the connection
        {
          n = UnsafeNativeMethods.sqlite3_exec(db, ToUTF8("ROLLBACK"), IntPtr.Zero, IntPtr.Zero, out stmt);
          if (n > 0) throw new SQLiteException(n, SQLiteLastError(db));
        }
      }
    }

    internal static bool IsAutocommit(SQLiteConnectionHandle hdl)
    {
      return (UnsafeNativeMethods.sqlite3_get_autocommit(hdl) == 1);
    }

  }

  internal interface ISQLiteSchemaExtensions
  {
    void BuildTempSchema(SQLiteConnection cnn);
  }

  [Flags]
  internal enum SQLiteOpenFlagsEnum
  {
    None = 0,
    ReadOnly = 0x01,
    ReadWrite = 0x02,
    Create = 0x04,
    SharedCache = 0x01000000,
    Default = 0x06,
  }

  /// <summary>
  /// The extra behavioral flags that can be applied to a connection.
  /// </summary>
  [Flags()]
  public enum SQLiteConnectionFlags
  {
      /// <summary>
      /// No extra flags.
      /// </summary>
      None = 0x0,

      /// <summary>
      /// Enable logging of all SQL statements to be prepared.
      /// </summary>
      LogPrepare = 0x1,

      /// <summary>
      /// Enable logging of all bound parameter types and raw values.
      /// </summary>
      LogPreBind = 0x2,

      /// <summary>
      /// Enable logging of all bound parameter strongly typed values.
      /// </summary>
      LogBind = 0x4,

      /// <summary>
      /// Enable logging of all exceptions caught from user-provided
      /// managed code called from native code via delegates.
      /// </summary>
      LogCallbackException = 0x8,

      /// <summary>
      /// Enable logging of backup API errors.
      /// </summary>
      LogBackup = 0x10,

      /// <summary>
      /// Enable all logging.
      /// </summary>
      LogAll = LogPrepare | LogPreBind | LogBind |
               LogCallbackException | LogBackup,

      /// <summary>
      /// The default extra flags for new connections.
      /// </summary>
      Default = LogCallbackException
  }

  // These are the options to the internal sqlite3_config call.
  internal enum SQLiteConfigOpsEnum
  {
    SQLITE_CONFIG_NONE = 0, // nil 
    SQLITE_CONFIG_SINGLETHREAD = 1, // nil 
    SQLITE_CONFIG_MULTITHREAD = 2, // nil 
    SQLITE_CONFIG_SERIALIZED = 3, // nil 
    SQLITE_CONFIG_MALLOC = 4, // sqlite3_mem_methods* 
    SQLITE_CONFIG_GETMALLOC = 5, // sqlite3_mem_methods* 
    SQLITE_CONFIG_SCRATCH = 6, // void*, int sz, int N 
    SQLITE_CONFIG_PAGECACHE = 7, // void*, int sz, int N 
    SQLITE_CONFIG_HEAP = 8, // void*, int nByte, int min 
    SQLITE_CONFIG_MEMSTATUS = 9, // boolean 
    SQLITE_CONFIG_MUTEX = 10, // sqlite3_mutex_methods* 
    SQLITE_CONFIG_GETMUTEX = 11, // sqlite3_mutex_methods* 
    // previously SQLITE_CONFIG_CHUNKALLOC 12 which is now unused
    SQLITE_CONFIG_LOOKASIDE = 13, // int int 
    SQLITE_CONFIG_PCACHE = 14, // sqlite3_pcache_methods* 
    SQLITE_CONFIG_GETPCACHE = 15, // sqlite3_pcache_methods* 
    SQLITE_CONFIG_LOG = 16, // xFunc, void* 
  }

}
