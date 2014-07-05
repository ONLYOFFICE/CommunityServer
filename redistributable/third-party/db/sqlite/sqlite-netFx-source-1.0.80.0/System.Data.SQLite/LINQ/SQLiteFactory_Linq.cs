/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Reflection;
  using System.Security.Permissions;

  /// <summary>
  /// SQLite implementation of DbProviderFactory.
  /// </summary>
  public sealed partial class SQLiteFactory : IServiceProvider
  {
    private static Type _dbProviderServicesType;
    private static object _sqliteServices;

    static SQLiteFactory()
    {
#if (SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK) && PRELOAD_NATIVE_LIBRARY
        UnsafeNativeMethods.Initialize();
#endif

#if !PLATFORM_COMPACTFRAMEWORK
        SQLiteLog.Initialize();
#endif

        string version =
#if NET_20
            "3.5.0.0";
#else
            "4.0.0.0";
#endif

        _dbProviderServicesType = Type.GetType(String.Format("System.Data.Common.DbProviderServices, System.Data.Entity, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089", version), false);
    }

    /// <summary>
    /// Will provide a DbProviderServices object in .NET 3.5
    /// </summary>
    /// <param name="serviceType">The class or interface type to query for</param>
    /// <returns></returns>
    object IServiceProvider.GetService(Type serviceType)
    {
      if (serviceType == typeof(ISQLiteSchemaExtensions) ||
        (_dbProviderServicesType != null && serviceType == _dbProviderServicesType))
      {
        return GetSQLiteProviderServicesInstance();
      }
      return null;
    }

    [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
    private object GetSQLiteProviderServicesInstance()
    {
        if (_sqliteServices == null)
        {
            Version version = this.GetType().Assembly.GetName().Version;
            Type type = Type.GetType(String.Format("System.Data.SQLite.SQLiteProviderServices, System.Data.SQLite.Linq, Version={0}, Culture=neutral, PublicKeyToken=db937bc2d44ff139", version), false);

            if (type != null)
            {
                FieldInfo field = type.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                _sqliteServices = field.GetValue(null);
            }
        }
        return _sqliteServices;
    }
  }
}
