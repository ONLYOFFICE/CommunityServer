/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Globalization;

  /// <summary>
  /// Represents a single SQL statement in SQLite.
  /// </summary>
  internal sealed class SQLiteStatement : IDisposable
  {
    /// <summary>
    /// The underlying SQLite object this statement is bound to
    /// </summary>
    internal SQLiteBase        _sql;
    /// <summary>
    /// The command text of this SQL statement
    /// </summary>
    internal string            _sqlStatement;
    /// <summary>
    /// The actual statement pointer
    /// </summary>
    internal SQLiteStatementHandle  _sqlite_stmt;
    /// <summary>
    /// An index from which unnamed parameters begin
    /// </summary>
    internal int               _unnamedParameters;
    /// <summary>
    /// Names of the parameters as SQLite understands them to be
    /// </summary>
    internal string[]          _paramNames;
    /// <summary>
    /// Parameters for this statement
    /// </summary>
    internal SQLiteParameter[] _paramValues;
    /// <summary>
    /// Command this statement belongs to (if any)
    /// </summary>
    internal SQLiteCommand     _command;

    /// <summary>
    /// The flags associated with the parent connection object.
    /// </summary>
    private SQLiteConnectionFlags _flags;

    private string[] _types;

    /// <summary>
    /// Initializes the statement and attempts to get all information about parameters in the statement
    /// </summary>
    /// <param name="sqlbase">The base SQLite object</param>
    /// <param name="flags">The flags associated with the parent connection object</param>
    /// <param name="stmt">The statement</param>
    /// <param name="strCommand">The command text for this statement</param>
    /// <param name="previous">The previous command in a multi-statement command</param>
    internal SQLiteStatement(SQLiteBase sqlbase, SQLiteConnectionFlags flags, SQLiteStatementHandle stmt, string strCommand, SQLiteStatement previous)
    {
      _sql     = sqlbase;
      _sqlite_stmt = stmt;
      _sqlStatement  = strCommand;
      _flags = flags;

      // Determine parameters for this statement (if any) and prepare space for them.
      int nCmdStart = 0;
      int n = _sql.Bind_ParamCount(this, _flags);
      int x;
      string s;

      if (n > 0)
      {
        if (previous != null)
          nCmdStart = previous._unnamedParameters;

        _paramNames = new string[n];
        _paramValues = new SQLiteParameter[n];

        for (x = 0; x < n; x++)
        {
          s = _sql.Bind_ParamName(this, _flags, x + 1);
          if (String.IsNullOrEmpty(s))
          {
            s = String.Format(CultureInfo.InvariantCulture, ";{0}", nCmdStart);
            nCmdStart++;
            _unnamedParameters++;
          }
          _paramNames[x] = s;
          _paramValues[x] = null;
        }
      }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable Members
    /// <summary>
    /// Disposes and finalizes the statement
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
            throw new ObjectDisposedException(typeof(SQLiteStatement).Name);
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

                if (_sqlite_stmt != null)
                {
                    _sqlite_stmt.Dispose();
                    _sqlite_stmt = null;
                }

                _paramNames = null;
                _paramValues = null;
                _sql = null;
                _sqlStatement = null;
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
    ~SQLiteStatement()
    {
        Dispose(false);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Called by SQLiteParameterCollection, this function determines if the specified parameter name belongs to
    /// this statement, and if so, keeps a reference to the parameter so it can be bound later.
    /// </summary>
    /// <param name="s">The parameter name to map</param>
    /// <param name="p">The parameter to assign it</param>
    internal bool MapParameter(string s, SQLiteParameter p)
    {
      if (_paramNames == null) return false;
      
      int startAt = 0;
      if (s.Length > 0)
      {
        if (":$@;".IndexOf(s[0]) == -1)
          startAt = 1;
      }

      int x = _paramNames.Length;
      for (int n = 0; n < x; n++)
      {
        if (String.Compare(_paramNames[n], startAt, s, 0, Math.Max(_paramNames[n].Length - startAt, s.Length), StringComparison.OrdinalIgnoreCase) == 0)
        {
          _paramValues[n] = p;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    ///  Bind all parameters, making sure the caller didn't miss any
    /// </summary>
    internal void BindParameters()
    {
      if (_paramNames == null) return;

      int x = _paramNames.Length;
      for (int n = 0; n < x; n++)
      {
        BindParameter(n + 1, _paramValues[n]);
      }
    }

    /// <summary>
    /// Attempts to convert an arbitrary object to the Boolean data type.
    /// Null object values are converted to false.  Throws a SQLiteException
    /// upon failure.
    /// </summary>
    /// <param name="obj">The object value to convert.</param>
    /// <param name="provider">The format provider to use.</param>
    /// <returns>The converted boolean value.</returns>
    private static bool ToBoolean(object obj, IFormatProvider provider)
    {
        if (obj == null)
            return false;

        TypeCode typeCode = Type.GetTypeCode(obj.GetType());

        switch (typeCode)
        {
            case TypeCode.Empty:
            case TypeCode.DBNull:
                return false;
            case TypeCode.Boolean:
                return (bool)obj;
            case TypeCode.Char:
                return ((char)obj) != (char)0 ? true : false;
            case TypeCode.SByte:
                return ((sbyte)obj) != (sbyte)0 ? true : false;
            case TypeCode.Byte:
                return ((byte)obj) != (byte)0 ? true : false;
            case TypeCode.Int16:
                return ((short)obj) != (short)0 ? true : false;
            case TypeCode.UInt16:
                return ((ushort)obj) != (ushort)0 ? true : false;
            case TypeCode.Int32:
                return ((int)obj) != (int)0 ? true : false;
            case TypeCode.UInt32:
                return ((uint)obj) != (uint)0 ? true : false;
            case TypeCode.Int64:
                return ((long)obj) != (long)0 ? true : false;
            case TypeCode.UInt64:
                return ((ulong)obj) != (ulong)0 ? true : false;
            case TypeCode.Single:
                return ((float)obj) != (float)0.0 ? true : false;
            case TypeCode.Double:
                return ((double)obj) != (double)0.0 ? true : false;
            case TypeCode.Decimal:
                return ((decimal)obj) != Decimal.Zero ? true : false;
            case TypeCode.String:
                return Convert.ToBoolean(obj, provider);
            default:
                throw new SQLiteException((int)SQLiteErrorCode.Error,
                    String.Format("Cannot convert type {0} to boolean",
                    typeCode));
        }
    }

    /// <summary>
    /// Perform the bind operation for an individual parameter
    /// </summary>
    /// <param name="index">The index of the parameter to bind</param>
    /// <param name="param">The parameter we're binding</param>
    private void BindParameter(int index, SQLiteParameter param)
    {
      if (param == null)
        throw new SQLiteException((int)SQLiteErrorCode.Error, "Insufficient parameters supplied to the command");

      object obj = param.Value;
      DbType objType = param.DbType;

      if ((obj != null) && (objType == DbType.Object))
          objType = SQLiteConvert.TypeToDbType(obj.GetType());

#if !PLATFORM_COMPACTFRAMEWORK
      if ((_flags & SQLiteConnectionFlags.LogPreBind) == SQLiteConnectionFlags.LogPreBind)
      {
          IntPtr handle = _sqlite_stmt;

          SQLiteLog.LogMessage(0, String.Format(
              "Binding statement {0} paramter #{1} with database type {2} and raw value {{{3}}}...",
              handle, index, objType, obj));
      }
#endif

      if ((obj == null) || Convert.IsDBNull(obj))
      {
          _sql.Bind_Null(this, _flags, index);
        return;
      }

      switch (objType)
      {
        case DbType.Date:
        case DbType.Time:
        case DbType.DateTime:
          //
          // NOTE: The old method (commented below) does not honor the selected date format
          //       for the connection.
          // _sql.Bind_DateTime(this, index, Convert.ToDateTime(obj, CultureInfo.CurrentCulture));
            _sql.Bind_DateTime(this, _flags, index, (obj is string) ?
              _sql.ToDateTime((string)obj) : Convert.ToDateTime(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.Boolean:
          _sql.Bind_Int32(this, _flags, index, ToBoolean(obj, CultureInfo.CurrentCulture) ? 1 : 0);
          break;
        case DbType.SByte:
          _sql.Bind_Int32(this, _flags, index, Convert.ToSByte(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.Int16:
          _sql.Bind_Int32(this, _flags, index, Convert.ToInt16(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.Int32:
          _sql.Bind_Int32(this, _flags, index, Convert.ToInt32(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.Int64:
          _sql.Bind_Int64(this, _flags, index, Convert.ToInt64(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.Byte:
          _sql.Bind_UInt32(this, _flags, index, Convert.ToByte(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.UInt16:
          _sql.Bind_UInt32(this, _flags, index, Convert.ToUInt16(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.UInt32:
          _sql.Bind_UInt32(this, _flags, index, Convert.ToUInt32(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.UInt64:
          _sql.Bind_UInt64(this, _flags, index, Convert.ToUInt64(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.Single:
        case DbType.Double:
        case DbType.Currency:
        //case DbType.Decimal: // Dont store decimal as double ... loses precision
          _sql.Bind_Double(this, _flags, index, Convert.ToDouble(obj, CultureInfo.CurrentCulture));
          break;
        case DbType.Binary:
          _sql.Bind_Blob(this, _flags, index, (byte[])obj);
          break;
        case DbType.Guid:
          if (_command.Connection._binaryGuid == true)
            _sql.Bind_Blob(this, _flags, index, ((Guid)obj).ToByteArray());
          else
            _sql.Bind_Text(this, _flags, index, obj.ToString());

          break;
        case DbType.Decimal: // Dont store decimal as double ... loses precision
          _sql.Bind_Text(this, _flags, index, Convert.ToDecimal(obj, CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture));
          break;
        default:
          _sql.Bind_Text(this, _flags, index, obj.ToString());
          break;
      }
    }

    internal string[] TypeDefinitions
    {
      get { return _types; }
    }

    internal void SetTypes(string typedefs)
    {
      int pos = typedefs.IndexOf("TYPES", 0, StringComparison.OrdinalIgnoreCase);
      if (pos == -1) throw new ArgumentOutOfRangeException();

      string[] types = typedefs.Substring(pos + 6).Replace(" ", "").Replace(";", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("`","").Split(',', '\r', '\n', '\t');

      int n;
      for (n = 0; n < types.Length; n++)
      {
        if (String.IsNullOrEmpty(types[n]) == true)
          types[n] = null;
      }
      _types = types;
    }
  }
}
