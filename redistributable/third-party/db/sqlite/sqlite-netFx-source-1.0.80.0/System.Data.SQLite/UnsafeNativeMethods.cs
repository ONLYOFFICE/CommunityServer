/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
    using System;
#if PRELOAD_NATIVE_LIBRARY || DEBUG
    using System.Diagnostics;
#endif

#if PRELOAD_NATIVE_LIBRARY
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
#endif

#if !PLATFORM_COMPACTFRAMEWORK && !DEBUG
  using System.Security;
#endif

    using System.Runtime.InteropServices;

#if !PLATFORM_COMPACTFRAMEWORK && !DEBUG
  [SuppressUnmanagedCodeSecurity]
#endif
    internal static class UnsafeNativeMethods
    {
        #region Optional Native SQLite Library Pre-Loading Code
        //
        // NOTE: If we are looking for the standard SQLite DLL ("sqlite3.dll"),
        //       the interop DLL ("SQLite.Interop.dll"), or we are running on the
        //       .NET Compact Framework, we should include this code (only if the
        //       feature has actually been enabled).  This code would be totally
        //       redundant if this module has been bundled into the mixed-mode
        //       assembly.
        //
#if SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK

        //
        // NOTE: Only compile in the native library pre-load code if the feature
        //       has been enabled for this build.
        //
#if PRELOAD_NATIVE_LIBRARY
#if !PLATFORM_COMPACTFRAMEWORK
        /// <summary>
        /// The name of the environment variable containing the processor
        /// architecture of the current process.
        /// </summary>
        private static readonly string PROCESSOR_ARCHITECTURE =
            "PROCESSOR_ARCHITECTURE";
#endif

        /////////////////////////////////////////////////////////////////////////

        private static readonly string DllFileExtension = ".dll";

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// This is the P/Invoke method that wraps the native Win32 LoadLibrary
        /// function.  See the MSDN documentation for full details on what it
        /// does.
        /// </summary>
        /// <param name="fileName">
        /// The name of the executable library.
        /// </param>
        /// <returns>
        /// The native module handle upon success -OR- IntPtr.Zero on failure.
        /// </returns>
        [DllImport("kernel32",
            CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto,
#if !PLATFORM_COMPACTFRAMEWORK
 BestFitMapping = false, ThrowOnUnmappableChar = true,
#endif
 SetLastError = true)]
        private static extern IntPtr LoadLibrary(string fileName);

        /// <summary>
        /// This lock is used to protect the static _SQLiteModule and
        /// processorArchitecturePlatforms fields, below.
        /// </summary>
        private static readonly object staticSyncRoot = new object();

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Stores the mappings between processor architecture names and platform
        /// names.
        /// </summary>
        private static Dictionary<string, string> processorArchitecturePlatforms;

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The native module handle for the native SQLite library or the value
        /// IntPtr.Zero.
        /// </summary>
        private static IntPtr _SQLiteModule = IntPtr.Zero;

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// For now, this method simply calls the Initialize method.
        /// </summary>
        static UnsafeNativeMethods()
        {
            Initialize();
        }

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Attempts to initialize this class by pre-loading the native SQLite
        /// library for the processor architecture of the current process.
        /// </summary>
        internal static void Initialize()
        {
#if !PLATFORM_COMPACTFRAMEWORK
            //
            // NOTE: If the "NoPreLoadSQLite" environment variable is set, skip
            //       all our special code and simply return.
            //
            if (Environment.GetEnvironmentVariable("No_PreLoadSQLite") != null)
                return;
#endif

            lock (staticSyncRoot)
            {
                //
                // TODO: Make sure this list is updated if the supported
                //       processor architecture names and/or platform names
                //       changes.
                //
                if (processorArchitecturePlatforms == null)
                {
                    processorArchitecturePlatforms =
                        new Dictionary<string, string>();

                    processorArchitecturePlatforms.Add("X86", "Win32");
                    processorArchitecturePlatforms.Add("AMD64", "x64");
                    processorArchitecturePlatforms.Add("IA64", "Itanium");
                }

                //
                // BUGBUG: What about other application domains?
                //
                if (_SQLiteModule == IntPtr.Zero)
                    _SQLiteModule = PreLoadSQLiteDll(null, null);
            }
        }

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Queries and returns the base directory of the current application
        /// domain.
        /// </summary>
        /// <returns>
        /// The base directory for the current application domain -OR- null if it
        /// cannot be determined.
        /// </returns>
        private static string GetBaseDirectory()
        {
#if !PLATFORM_COMPACTFRAMEWORK
            //
            // NOTE: If the "PreLoadSQLite_BaseDirectory" environment variable
            //       is set, use it verbatim for the base directory.
            //
            string directory = Environment.GetEnvironmentVariable(
                "PreLoadSQLite_BaseDirectory");

            if (directory != null)
                return directory;
#endif
            //
            // NOTE: Otherwise, fallback on using the base directory of the
            //       current application domain.
            //
            var codeBaseUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            if (codeBaseUri.Scheme == Uri.UriSchemeFile)
            {
                return Path.GetDirectoryName(codeBaseUri.LocalPath);
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Determines if the dynamic link library file name requires a suffix
        /// and adds it if necessary.
        /// </summary>
        /// <param name="fileName">
        /// The original dynamic link library file name to inspect.
        /// </param>
        /// <returns>
        /// The dynamic link library file name, possibly modified to include an
        /// extension.
        /// </returns>
        private static string FixUpDllFileName(
            string fileName
            )
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                PlatformID platformId = Environment.OSVersion.Platform;

                if ((platformId == PlatformID.Win32S) ||
                    (platformId == PlatformID.Win32Windows) ||
                    (platformId == PlatformID.Win32NT) ||
                    (platformId == PlatformID.WinCE))
                {
                    if (!fileName.EndsWith(DllFileExtension,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return fileName + DllFileExtension;
                    }
                }
            }

            return fileName;
        }

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Queries and returns the processor architecture of the current
        /// process.
        /// </summary>
        /// <returns>
        /// The processor architecture of the current process -OR- null if it
        /// cannot be determined.  Always returns an empty string when running on
        /// the .NET Compact Framework.
        /// </returns>
        private static string GetProcessorArchitecture()
        {
#if !PLATFORM_COMPACTFRAMEWORK
            //
            // BUGBUG: Will this always be reliable?
            //
            return Environment.GetEnvironmentVariable(PROCESSOR_ARCHITECTURE);
#else
          //
          // BUGBUG: No way to determine this value on the .NET Compact
          //         Framework (running on Windows CE, etc).
          //
          return String.Empty;
#endif
        }

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Given the processor architecture, returns the name of the platform.
        /// </summary>
        /// <param name="processorArchitecture">
        /// The processor architecture to be translated to a platform name.
        /// </param>
        /// <returns>
        /// The platform name for the specified processor architecture -OR- null
        /// if it cannot be determined.
        /// </returns>
        private static string GetPlatformName(
            string processorArchitecture
            )
        {
            if (String.IsNullOrEmpty(processorArchitecture))
                return null;

            lock (staticSyncRoot)
            {
                if (processorArchitecturePlatforms == null)
                    return null;

                string platformName;

                if (processorArchitecturePlatforms.TryGetValue(
                        processorArchitecture, out platformName))
                {
                    return platformName;
                }

                if (processorArchitecturePlatforms.TryGetValue(
#if !PLATFORM_COMPACTFRAMEWORK
processorArchitecture.ToUpperInvariant(),
#else
                      processorArchitecture.ToUpper(),
#endif
 out platformName))
                {
                    return platformName;
                }
            }

            return null;
        }

        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Attempts to load the native SQLite library based on the specified
        /// directory and processor architecture.
        /// </summary>
        /// <param name="directory">
        /// The base directory to use, null for default (the base directory of
        /// the current application domain).  This directory should contain the
        /// processor architecture specific sub-directories.
        /// </param>
        /// <param name="processorArchitecture">
        /// The requested processor architecture, null for default (the
        /// processor architecture of the current process).  This caller should
        /// almost always specify null for this parameter.
        /// </param>
        /// <returns>
        /// The native module handle as returned by LoadLibrary -OR- IntPtr.Zero
        /// if the loading fails for any reason.
        /// </returns>
        private static IntPtr PreLoadSQLiteDll(
            string directory,
            string processorArchitecture
            )
        {
            //
            // NOTE: If the specified base directory is null, use the default.
            //
            if (directory == null)
                directory = GetBaseDirectory();

            //
            // NOTE: If we failed to query the base directory, stop now.
            //
            if (directory == null)
                return IntPtr.Zero;

            //
            // NOTE: If the native SQLite library exists in the base directory
            //       itself, stop now.
            //
            string fileName = FixUpDllFileName(Path.Combine(directory,
                SQLITE_DLL));

            if (File.Exists(fileName))
                return IntPtr.Zero;

            //
            // NOTE: If the specified processor architecture is null, use the
            //       default.
            //
            if (processorArchitecture == null)
                processorArchitecture = GetProcessorArchitecture();

            //
            // NOTE: If we failed to query the processor architecture, stop now.
            //
            if (processorArchitecture == null)
                return IntPtr.Zero;

            //
            // NOTE: Build the full path and file name for the native SQLite
            //       library using the processor architecture name.
            //
            fileName = FixUpDllFileName(Path.Combine(Path.Combine(directory,
                processorArchitecture), SQLITE_DLL));

            //
            // NOTE: If the file name based on the processor architecture name
            // is not found, try using the associated platform name.
            //
            if (!File.Exists(fileName))
            {
                //
                // NOTE: Attempt to translate the processor architecture to a
                //       platform name.
                //
                string platformName = GetPlatformName(processorArchitecture);

                //
                // NOTE: If we failed to translate the platform name, stop now.
                //
                if (platformName == null)
                    return IntPtr.Zero;

                //
                // NOTE: Build the full path and file name for the native SQLite
                //       library using the platform name.
                //
                fileName = FixUpDllFileName(Path.Combine(Path.Combine(directory,
                    platformName), SQLITE_DLL));

                //
                // NOTE: If the file does not exist, skip trying to load it.
                //
                if (!File.Exists(fileName))
                    return IntPtr.Zero;
            }

            try
            {
                //
                // NOTE: Show exactly where we are trying to load the native
                //       SQLite library from.
                //
                Trace.WriteLine(String.Format(
                    "Trying to load native SQLite library \"{0}\"...",
                    fileName));

                //
                // NOTE: Attempt to load the native library.  This will either
                //       return a valid native module handle, return IntPtr.Zero,
                //       or throw an exception.
                //
                return LoadLibrary(fileName);
            }
            catch (Exception e)
            {
                try
                {
                    //
                    // NOTE: First, grab the last Win32 error number.
                    //
                    int lastError = Marshal.GetLastWin32Error();

                    //
                    // NOTE: Show where we failed to load the native SQLite
                    //       library from along with the Win32 error code and
                    //       exception information.
                    //
                    Trace.WriteLine(String.Format(
                        "Failed to load native SQLite library \"{0}\" " +
                        "(getLastError = {1}): {2}",
                        fileName, lastError, e)); /* throw */
                }
                catch
                {
                    // do nothing.
                }
            }

            return IntPtr.Zero;
        }
#endif
#endif
        #endregion

        /////////////////////////////////////////////////////////////////////////

#if !SQLITE_STANDARD

#if !USE_INTEROP_DLL

#if !PLATFORM_COMPACTFRAMEWORK
    private const string SQLITE_DLL = "System.Data.SQLite.dll";
#else
    internal const string SQLITE_DLL = "SQLite.Interop.080.dll";
#endif // PLATFORM_COMPACTFRAMEWORK

#else
        private const string SQLITE_DLL = "SQLite.Interop.dll";
#endif // USE_INTEROP_DLL

#else
    private const string SQLITE_DLL = "sqlite3";
#endif

        // This section uses interop calls that also fetch text length to optimize conversion.  
        // When using the standard dll, we can replace these calls with normal sqlite calls and 
        // do unoptimized conversions instead afterwards
        #region interop added textlength calls

#if !SQLITE_STANDARD

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_bind_parameter_name_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_database_name_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_database_name16_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_decltype_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_decltype16_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_name_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_name16_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_origin_name_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_origin_name16_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_table_name_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_table_name16_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_text_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_column_text16_interop(IntPtr stmt, int index, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_errmsg_interop(IntPtr db, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_prepare_interop(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain, out int nRemain);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_table_column_metadata_interop(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, out IntPtr ptrDataType, out IntPtr ptrCollSeq, out int notNull, out int primaryKey, out int autoInc, out int dtLen, out int csLen);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_value_text_interop(IntPtr p, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_value_text16_interop(IntPtr p, out int len);

#endif
        // !SQLITE_STANDARD

        #endregion

        // These functions add existing functionality on top of SQLite and require a little effort to
        // get working when using the standard SQLite library.
        #region interop added functionality

#if !SQLITE_STANDARD

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_close_interop(IntPtr db);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_create_function_interop(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal, int needCollSeq);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_finalize_interop(IntPtr stmt);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_open_interop(byte[] utf8Filename, int flags, out IntPtr db);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_open16_interop(byte[] utf8Filename, int flags, out IntPtr db);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_reset_interop(IntPtr stmt);

#endif
        // !SQLITE_STANDARD

        #endregion

        // The standard api call equivalents of the above interop calls
        #region standard versions of interop functions

#if SQLITE_STANDARD

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_close(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_create_function(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_finalize(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_open_v2(byte[] utf8Filename, out IntPtr db, int flags, IntPtr vfs);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern int sqlite3_open16(string fileName, out IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_reset(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_bind_parameter_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_database_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_database_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_decltype(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_decltype16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_origin_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_origin_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_table_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_table_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_text(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_text16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_errmsg(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_prepare(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_table_column_metadata(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, out IntPtr ptrDataType, out IntPtr ptrCollSeq, out int notNull, out int primaryKey, out int autoInc);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_text(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_text16(IntPtr p);

#endif
        // SQLITE_STANDARD

        #endregion

        // These functions are custom and have no equivalent standard library method.
        // All of them are "nice to haves" and not necessarily "need to haves".
        #region no equivalent standard method

#if !SQLITE_STANDARD

        [DllImport(SQLITE_DLL)]
        internal static extern IntPtr sqlite3_context_collseq(IntPtr context, out int type, out int enc, out int len);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_context_collcompare(IntPtr context, byte[] p1, int p1len, byte[] p2, int p2len);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_cursor_rowid(IntPtr stmt, int cursor, out long rowid);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_index_column_info_interop(IntPtr db, byte[] catalog, byte[] IndexName, byte[] ColumnName, out int sortOrder, out int onError, out IntPtr Collation, out int colllen);

        [DllImport(SQLITE_DLL)]
        internal static extern void sqlite3_resetall_interop(IntPtr db);

        [DllImport(SQLITE_DLL)]
        internal static extern int sqlite3_table_cursor(IntPtr stmt, int db, int tableRootPage);

#endif
        // !SQLITE_STANDARD

        #endregion

        // Standard API calls global across versions.  There are a few instances of interop calls
        // scattered in here, but they are only active when PLATFORM_COMPACTFRAMEWORK is declared.
        #region standard sqlite api calls

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_libversion();

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_sourceid();

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern void sqlite3_interrupt(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern long sqlite3_last_insert_rowid(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_changes(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern long sqlite3_memory_used();

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern long sqlite3_memory_highwater(int resetFlag);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_shutdown();

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_busy_timeout(IntPtr db, int ms);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_bind_blob(IntPtr stmt, int index, Byte[] value, int nSize, IntPtr nTransient);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_double_interop(IntPtr stmt, int index, ref double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_bind_int(IntPtr stmt, int index, int value);

        //
        // NOTE: This really just calls "sqlite3_bind_int"; however, it has the
        //       correct type signature for an unsigned (32-bit) integer.
        //
#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int")]
#endif
        internal static extern int sqlite3_bind_uint(IntPtr stmt, int index, uint value);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_int64_interop(IntPtr stmt, int index, ref long value);
#endif

        //
        // NOTE: This really just calls "sqlite3_bind_int64"; however, it has the
        //       correct type signature for an unsigned long (64-bit) integer.
        //
#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_uint64(IntPtr stmt, int index, ulong value);
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int64_interop")]
    internal static extern int sqlite3_bind_uint64_interop(IntPtr stmt, int index, ref ulong value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_bind_null(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_bind_parameter_count(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_bind_parameter_index(IntPtr stmt, byte[] strName);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_column_count(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_step(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double sqlite3_column_double(IntPtr stmt, int index);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_column_double_interop(IntPtr stmt, int index, out double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_column_int(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long sqlite3_column_int64(IntPtr stmt, int index);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_column_int64_interop(IntPtr stmt, int index, out long value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_column_bytes(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern TypeAffinity sqlite3_column_type(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_create_collation(IntPtr db, byte[] strName, int nType, IntPtr pvUser, SQLiteCollation func);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_aggregate_count(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_value_blob(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_value_bytes(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double sqlite3_value_double(IntPtr p);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_value_double_interop(IntPtr p, out double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_value_int(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long sqlite3_value_int64(IntPtr p);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_value_int64_interop(IntPtr p, out Int64 value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern TypeAffinity sqlite3_value_type(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int nSize, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_double(IntPtr context, double value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_double_interop(IntPtr context, ref double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern void sqlite3_result_int(IntPtr context, int value);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_int64(IntPtr context, long value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_int64_interop(IntPtr context, ref Int64 value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern void sqlite3_result_null(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern void sqlite3_result_text(IntPtr context, byte[] value, int nLen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int nBytes);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
        internal static extern int sqlite3_bind_text16(IntPtr stmt, int index, string value, int nlen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
        internal static extern void sqlite3_result_error16(IntPtr context, string strName, int nLen);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
        internal static extern void sqlite3_result_text16(IntPtr context, string strName, int nLen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_key(IntPtr db, byte[] key, int keylen);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_rekey(IntPtr db, byte[] key, int keylen);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_update_hook(IntPtr db, SQLiteUpdateCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_commit_hook(IntPtr db, SQLiteCommitCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_trace(IntPtr db, SQLiteTraceCallback func, IntPtr pvUser);

        // Since sqlite3_config() takes a variable argument list, we have to overload declarations
        // for all possible calls.  For now, we are only exposing the SQLITE_CONFIG_LOG call.
#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_config(int op, SQLiteLogCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_rollback_hook(IntPtr db, SQLiteRollbackCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_db_handle(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_next_stmt(IntPtr db, IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_exec(IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam, out IntPtr errMsg);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_get_autocommit(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_extended_result_codes(IntPtr db, int onoff);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_errcode(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_extended_errcode(IntPtr db);

        // Since sqlite3_log() takes a variable argument list, we have to overload declarations
        // for all possible calls.  For now, we are only exposing a single string, and 
        // depend on the caller to format the string.
#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern void sqlite3_log(int iErrCode, byte[] zFormat);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_file_control(IntPtr db, byte[] zDbName, int op, IntPtr pArg);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern IntPtr sqlite3_backup_init(IntPtr destDb, byte[] zDestName, IntPtr sourceDb, byte[] zSourceName);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_backup_step(IntPtr backup, int nPage);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_backup_finish(IntPtr backup);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_backup_remaining(IntPtr backup);

#if !PLATFORM_COMPACTFRAMEWORK
        [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
        internal static extern int sqlite3_backup_pagecount(IntPtr backup);
        #endregion
    }

#if PLATFORM_COMPACTFRAMEWORK
  internal abstract class CriticalHandle : IDisposable
  {
    private bool _isClosed;
    protected IntPtr handle;
    
    protected CriticalHandle(IntPtr invalidHandleValue)
    {
      handle = invalidHandleValue;
      _isClosed = false;
    }

    ~CriticalHandle()
    {
      Dispose(false);
    }

    private void Cleanup()
    {
      if (!IsClosed)
      {
        this._isClosed = true;
        if (!IsInvalid)
        {
          ReleaseHandle();
          GC.SuppressFinalize(this);
        }
      }
    }

    public void Close()
    {
      Dispose(true);
    }

    public void Dispose()
    {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      Cleanup();
    }

    protected abstract bool ReleaseHandle();

    protected void SetHandle(IntPtr value)
    {
      handle = value;
    }

    public void SetHandleAsInvalid()
    {
      _isClosed = true;
      GC.SuppressFinalize(this);
    }

    public bool IsClosed
    {
      get { return _isClosed; }
    }

    public abstract bool IsInvalid
    {
      get;
    }

  }

#endif

    // Handles the unmanaged database pointer, and provides finalization support for it.
    internal class SQLiteConnectionHandle : CriticalHandle
    {
        public static implicit operator IntPtr(SQLiteConnectionHandle db)
        {
            return (db != null) ? db.handle : IntPtr.Zero;
        }

        public static implicit operator SQLiteConnectionHandle(IntPtr db)
        {
            return new SQLiteConnectionHandle(db);
        }

        private SQLiteConnectionHandle(IntPtr db)
            : this()
        {
            SetHandle(db);
        }

        internal SQLiteConnectionHandle()
            : base(IntPtr.Zero)
        {
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                SQLiteBase.CloseConnection(this);

#if DEBUG && !NET_COMPACT_20
                try
                {
                    Trace.WriteLine(String.Format(
                        "CloseConnection: {0}", handle));
                }
                catch
                {
                }
#endif

#if DEBUG
                return true;
#endif
            }
#if DEBUG && !NET_COMPACT_20
            catch (SQLiteException e)
#else
      catch (SQLiteException)
#endif
            {
#if DEBUG && !NET_COMPACT_20
                try
                {
                    Trace.WriteLine(String.Format(
                        "CloseConnection: {0}, exception: {1}",
                        handle, e));
                }
                catch
                {
                }
#endif
            }
#if DEBUG
            return false;
#else
      return true;
#endif
        }

        public override bool IsInvalid
        {
            get { return (handle == IntPtr.Zero); }
        }
    }

    // Provides finalization support for unmanaged SQLite statements.
    internal class SQLiteStatementHandle : CriticalHandle
    {
        public static implicit operator IntPtr(SQLiteStatementHandle stmt)
        {
            return (stmt != null) ? stmt.handle : IntPtr.Zero;
        }

        public static implicit operator SQLiteStatementHandle(IntPtr stmt)
        {
            return new SQLiteStatementHandle(stmt);
        }

        private SQLiteStatementHandle(IntPtr stmt)
            : this()
        {
            SetHandle(stmt);
        }

        internal SQLiteStatementHandle()
            : base(IntPtr.Zero)
        {
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                SQLiteBase.FinalizeStatement(this);

#if DEBUG && !NET_COMPACT_20
                try
                {
                    Trace.WriteLine(String.Format(
                        "FinalizeStatement: {0}", handle));
                }
                catch
                {
                }
#endif

#if DEBUG
                return true;
#endif
            }
#if DEBUG && !NET_COMPACT_20
            catch (SQLiteException e)
#else
      catch (SQLiteException)
#endif
            {
#if DEBUG && !NET_COMPACT_20
                try
                {
                    Trace.WriteLine(String.Format(
                        "FinalizeStatement: {0}, exception: {1}",
                        handle, e));
                }
                catch
                {
                }
#endif
            }
#if DEBUG
            return false;
#else
      return true;
#endif
        }

        public override bool IsInvalid
        {
            get { return (handle == IntPtr.Zero); }
        }
    }

    // Provides finalization support for unmanaged SQLite backup objects.
    internal class SQLiteBackupHandle : CriticalHandle
    {
        public static implicit operator IntPtr(SQLiteBackupHandle backup)
        {
            return (backup != null) ? backup.handle : IntPtr.Zero;
        }

        public static implicit operator SQLiteBackupHandle(IntPtr backup)
        {
            return new SQLiteBackupHandle(backup);
        }

        private SQLiteBackupHandle(IntPtr backup)
            : this()
        {
            SetHandle(backup);
        }

        internal SQLiteBackupHandle()
            : base(IntPtr.Zero)
        {
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                SQLiteBase.FinishBackup(this);

#if DEBUG && !NET_COMPACT_20
                try
                {
                    Trace.WriteLine(String.Format(
                        "FinishBackup: {0}", handle));
                }
                catch
                {
                }
#endif

#if DEBUG
                return true;
#endif
            }
#if DEBUG && !NET_COMPACT_20
            catch (SQLiteException e)
#else
          catch (SQLiteException)
#endif
            {
#if DEBUG && !NET_COMPACT_20
                try
                {
                    Trace.WriteLine(String.Format(
                        "FinishBackup: {0}, exception: {1}",
                        handle, e));
                }
                catch
                {
                }
#endif
            }
#if DEBUG
            return false;
#else
          return true;
#endif
        }

        public override bool IsInvalid
        {
            get { return (handle == IntPtr.Zero); }
        }
    }
}
