
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.SqlServer
{
	/// <summary>
	/// MSBuild task to execute DDL and SQL statements.
	/// </summary>
	/// <remarks>Requires the the SQL Server libraries and dynamically loads the 
	/// required Microsoft.SqlServer.ConnectionInfo assembly.</remarks>
	/// <example>
	/// <code><![CDATA[
	/// <PropertyGroup>
	///		<ConnectionString>Server=localhost;Integrated Security=True</ConnectionString>
	///	</PropertyGroup>
	///
	/// <Target Name="ExecuteDDL">
	///		<ExecuteDDL ConnectionString="$(ConnectionString)" Files="SqlBatchScript.sql" ContinueOnError="false" />
	/// </Target>
	/// ]]></code>
	/// </example>
	public class ExecuteDDL : Task
	{
		private string _batchSeparator = "GO";
		private int _statementTimeout = 30;
		private ServerConnectionWrapper _conn = null;
		private string _connStr = null;
		private ITaskItem[] _files;
		private List<int> _results= new List<int>();

		#region properties
		
		/// <summary>
		/// The connection string
		/// </summary>
		[Required]
		public string ConnectionString
		{
			get { return _connStr; }
			set { _connStr = value; }
		}

		/// <summary>
		/// Gets or sets the DDL/SQL files.
		/// </summary>
		/// <value>The assemblies.</value>
		[Required]
		public ITaskItem[] Files
		{
			get { return _files; }
			set { _files = value; }
		}

		/// <summary>
		/// Output the return count/values
		/// </summary>
		[Output]
		public int[] Results
		{
			get { return _results.ToArray(); }
		}

		/// <summary>
		/// Timeout to execute a DDL statement.
		/// </summary>
		/// <remarks>Defaults to 30 seconds. Set to 0 for an infinite timeout period.</remarks>
		public int StatementTimeout
		{
			get { return _statementTimeout; }
			set { _statementTimeout = value; }
		}
		
		#endregion
		
		private ServerConnectionWrapper Connection
		{
			get
			{
				if (_conn == null)
				{
					_conn = new ServerConnectionWrapper();
					_conn.ConnectionString =_connStr;
					_conn.BatchSeparator = _batchSeparator;
					_conn.InfoMessage += InfoMessage;
					_conn.StatementTimeout = _statementTimeout;
					_conn.Connect();
				}
				return _conn;
			}
		}
		
		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns>
		/// true if the task successfully executed; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			bool result = false;
			try
			{
				foreach (ITaskItem item in _files)
				{
					Log.LogMessage("Executing DDL file '{0}'", item.ItemSpec);
					result = ExecuteSql(ReadFile(item.ItemSpec));
					if (!(result || BuildEngine.ContinueOnError))
					{
						result = false;
						break;
					}
				}
			}
			finally
			{
				if (Connection != null)
				{
					Connection.Disconnect();
				}
			}
			return result;
		}

		private bool ExecuteSql(string sql)
		{
			int sqlResult = -1;
			bool result = false;
			if (!string.IsNullOrEmpty(sql))
			{
				try
				{
					sqlResult = Connection.ExecuteNonQuery(sql);
					Log.LogMessage("\tSuccessfully executed SQL command with result = {0}", sqlResult);
					result = true;
				}
				catch (Exception ex)
				{
					Log.LogError("Unexpected {2} error executing SQL command: {0}\n{1}", sql, ex.Message, ex.GetType());
					Exception inner = ex.InnerException;
					while (inner != null)
					{
						Log.LogError("Error {0}", inner.Message);
						if (inner is SqlException)
						{
							foreach (SqlError error in ((SqlException)inner).Errors)
							{
								Log.LogError("{1}: Error # {0} on Line {2}: {3}", error.Number, error.Server, error.LineNumber, error.Message);
							}
						}
						inner = inner.InnerException;
					}
				}
				_results.Add(sqlResult);
			}
			else
			{
				Log.LogWarning("Empty SQL command");
				result = true;
			}
			return result;
		}

		private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			Log.LogMessage("\t" + e.Message);
		}

		private string ReadFile(string path)
		{
			using (StreamReader reader = new StreamReader(path))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Gets or sets the batch delimter string.
		/// </summary>
		/// <remarks>Default is "GO" for T-SQL.</remarks>
		public string BatchSeparator
		{
			get { return _batchSeparator; }
			set
			{
				if (string.IsNullOrEmpty(value) || (value.Trim().Length == 0))
				{
					throw new ArgumentException("BatchSeparator cannot be null or empty", "value");
				}
				_batchSeparator = value;
			}
		}
	}
}
