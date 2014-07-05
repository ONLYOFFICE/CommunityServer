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
    /// Represents a single SQL backup in SQLite.
    /// </summary>
    internal sealed class SQLiteBackup : IDisposable
    {
        /// <summary>
        /// The underlying SQLite object this backup is bound to.
        /// </summary>
        internal SQLiteBase _sql;

        /// <summary>
        /// The actual backup handle.
        /// </summary>
        internal SQLiteBackupHandle _sqlite_backup;

        /// <summary>
        /// The destination database for the backup.
        /// </summary>
        internal IntPtr _destDb;

        /// <summary>
        /// The destination database name for the backup.
        /// </summary>
        internal byte[] _zDestName;

        /// <summary>
        /// The source database for the backup.
        /// </summary>
        internal IntPtr _sourceDb;

        /// <summary>
        /// The source database name for the backup.
        /// </summary>
        internal byte[] _zSourceName;

        /// <summary>
        /// The last result from the StepBackup method of the SQLite3 class.
        /// This is used to determine if the call to the FinishBackup method of
        /// the SQLite3 class should throw an exception when it receives a non-Ok
        /// return code from the core SQLite library.
        /// </summary>
        internal int _stepResult;

        /// <summary>
        /// Initializes the backup.
        /// </summary>
        /// <param name="sqlbase">The base SQLite object.</param>
        /// <param name="backup">The backup handle.</param>
        /// <param name="destDb">The destination database for the backup.</param>
        /// <param name="zDestName">The destination database name for the backup.</param>
        /// <param name="sourceDb">The source database for the backup.</param>
        /// <param name="zSourceName">The source database name for the backup.</param>
        internal SQLiteBackup(
            SQLiteBase sqlbase,
            SQLiteBackupHandle backup,
            IntPtr destDb,
            byte[] zDestName,
            IntPtr sourceDb,
            byte[] zSourceName
            )
        {
            _sql = sqlbase;
            _sqlite_backup = backup;
            _destDb = destDb;
            _zDestName = zDestName;
            _sourceDb = sourceDb;
            _zSourceName = zSourceName;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes and finalizes the backup.
        /// </summary>
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
            throw new ObjectDisposedException(typeof(SQLiteBackup).Name);
#endif
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ////////////////////////////////////
                    // dispose managed resources here...
                    ////////////////////////////////////

                    if (_sqlite_backup != null)
                    {
                        _sqlite_backup.Dispose();
                        _sqlite_backup = null;
                    }

                    _zSourceName = null;
                    _sourceDb = IntPtr.Zero;
                    _zDestName = null;
                    _destDb = IntPtr.Zero;
                    _sql = null;
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
        ~SQLiteBackup()
        {
            Dispose(false);
        }
        #endregion
    }
}
