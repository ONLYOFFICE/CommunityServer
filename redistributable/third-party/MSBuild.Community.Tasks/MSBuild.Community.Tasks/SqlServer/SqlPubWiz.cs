#region Copyright © 2008 Paul Welter. All rights reserved.
/*
Copyright © 2008 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;



namespace MSBuild.Community.Tasks.SqlServer
{
    /// <summary>
    /// The SqlPubWiz commands
    /// </summary>
    public enum SqlPubCommands
    {
        /// <summary>
        /// Scripts a local database to one or more files
        /// </summary>
        Script,
        /// <summary>
        /// Publishes a local database to a web service provided by a hoster.
        /// </summary>
        Publish
    }

    /// <summary>
    /// The Database Publishing Wizard enables the deployment of
    /// SQL Server databases (both schema and data) into a shared
    /// hosting environment.
    /// </summary>
    /// <example>Generate the database script for Northwind on localhost.
    /// <code><![CDATA[
    /// <SqlPubWiz 
    ///     Database="Northwind" 
    ///     Output="Northwind.sql" 
    ///     SchemaOnly="true" />
    /// ]]></code>
    /// </example>
    public class SqlPubWiz : ToolTask
    {
        private const string InstallPathKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\90\Tools\PublishingWizard\1.2";

        #region Properties
        private SqlPubCommands _command = SqlPubCommands.Script;

        /// <summary>
        /// Gets or sets the database publishing command.
        /// </summary>
        /// <value>The database publishing command.</value>
        /// <remarks>
        /// Use either script or publish. Use script to script a local
        /// database to a file on the local computer. Use publish to 
        /// transfer database objects directly to a hosted database via
        /// a web service. script and publish enable some of the same 
        /// options and switches. Some options and switches are only 
        /// enabled only by one of these verbs.
        /// </remarks>
        public string Command
        {
            get { return _command.ToString(); }
            set { _command = (SqlPubCommands)Enum.Parse(typeof(SqlPubCommands), value, true); }
        }

        private string _connectionString;

        /// <summary>
        /// Gets or sets the full connection string to the local database.
        /// </summary>
        /// <value>The connection string.</value>
        /// <remarks>
        /// Provides a full connection string to connect to the local 
        /// database. This connection string encapsulates all connection 
        /// options to the server. Incompatible with <see cref="Username"/>, 
        /// <see cref="Password"/> or, <see cref="Server"/>.
        /// </remarks>
        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        private string _database;

        /// <summary>
        /// Gets or sets the name of the local database to be scripted.
        /// </summary>
        /// <value>The name of the database to be scripted.</value>
        public string Database
        {
            get { return _database; }
            set { _database = value; }
        }

        private string _username;

        /// <summary>
        /// Gets or sets the SQL Server user name to use for connection
        /// to the source database. 
        /// </summary>
        /// <value>The SQL Server user name.</value>
        /// <remarks>
        /// Specifies the SQL Server User name to use for connection
        /// to the source database. Requires <see cref="Password"/>.
        /// Incompatible with <see cref="ConnectionString"/>.
        /// </remarks>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password;

        /// <summary>
        /// Gets or sets the password to use for connection
        /// to the source database.
        /// </summary>
        /// <value>The password.</value>
        /// <remarks>
        /// Specifies the password to use for connection to the source
        /// database. Requires <see cref="Username"/>. 
        /// Incompatible with <see cref="ConnectionString"/>.
        /// </remarks>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _server;

        /// <summary>
        /// Gets or sets the name or IP address for the local database connection.
        /// </summary>
        /// <value>The name or IP address of server.</value>
        /// <remarks>
        /// Specifies the name or IP address for the local database connection.
        /// The default is localhost. Incompatible with <see cref="ConnectionString"/>.
        /// </remarks>
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        private ITaskItem _output;

        /// <summary>
        /// Gets or sets the full file path for the script file generated.
        /// </summary>
        /// <value>The full file path.</value>
        /// <remarks>
        /// Use only with script. Specifies the full file path for the script
        /// file generated by the sqlpubwiz. 
        /// </remarks>
        public ITaskItem Output
        {
            get { return _output; }
            set { _output = value; }
        }

        private bool _schemaOnly;

        /// <summary>
        /// Gets or sets a value indicating whether the schema, 
        /// but not the data, should be scripted.
        /// </summary>
        /// <value>
        /// <c>true</c> if only the schema should be scripted; 
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Specifies that the schema, but not the data, should be
        /// scripted. Incompatible with <see cref="DataOnly"/>.
        /// If neither <see cref="SchemaOnly"/> nor <see cref="DataOnly"/>
        /// are specified, both the schema and data are scripted.
        /// </remarks>
        public bool SchemaOnly
        {
            get { return _schemaOnly; }
            set { _schemaOnly = value; }
        }

        private bool _dataOnly;

        /// <summary>
        /// Gets or sets a value indicating whether the data but 
        /// not the schema should be scripted.
        /// </summary>
        /// <value>
        /// <c>true</c> if only the data is scripted; 
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Specifies that the data but not the schema should be 
        /// scripted. Incompatible with <see cref="NoDropExisting"/> 
        /// or <see cref="SchemaOnly"/>. If neither <see cref="SchemaOnly"/>
        /// nor <see cref="DataOnly"/> are specified, both the schema
        /// and data are scripted.
        /// </remarks>
        public bool DataOnly
        {
            get { return _dataOnly; }
            set { _dataOnly = value; }
        }

        private string _targetServer;

        /// <summary>
        /// Gets or sets the target server the script should target.
        /// </summary>
        /// <value>The target server the script should target.</value>
        /// <remarks>
        /// Specifies that the script to be generated should target
        /// a SQL Server 2000 or SQL Server 2005 instance. 
        /// The default is SQL Server 2005.
        /// </remarks>
        public string TargetServer
        {
            get { return _targetServer; }
            set { _targetServer = value; }
        }

        private bool _noSchemaQualify;

        /// <summary>
        /// Gets or sets a value indicating whether objects will
        /// not be qualified with a schema.
        /// </summary>
        /// <value>
        /// <c>true</c> if objects will not be schema qualified;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool NoSchemaQualify
        {
            get { return _noSchemaQualify; }
            set { _noSchemaQualify = value; }
        }

        private bool _noDropExisting;

        /// <summary>
        /// Gets or sets a value indicating whether the produced 
        /// script should not drop pre-existing objects.
        /// </summary>
        /// <value>
        /// <c>true</c> if pre-existing objects should not be dropped;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool NoDropExisting
        {
            get { return _noDropExisting; }
            set { _noDropExisting = value; }
        }

        private bool _quiet;

        /// <summary>
        /// Gets or sets a value indicating output message suppression.
        /// </summary>
        /// <value><c>true</c> to suppress messages; otherwise, <c>false</c>.</value>
        public bool Quiet
        {
            get { return _quiet; }
            set { _quiet = value; }
        }

        private string _hosterName;

        /// <summary>
        /// Gets or sets the friendly name of previously
        /// configured hosting Web service.
        /// </summary>
        /// <value>The name of the hoster.</value>
        public string HosterName
        {
            get { return _hosterName; }
            set { _hosterName = value; }
        }

        private string _webServiceAddress;

        /// <summary>
        /// Gets or sets the configuration of the hosting Web service endpoint.
        /// </summary>
        /// <value>The web service address.</value>
        public string WebServiceAddress
        {
            get { return _webServiceAddress; }
            set { _webServiceAddress = value; }
        }

        private string _serviceUsername;

        /// <summary>
        /// Gets or sets the username on the hosting Web service endpoint.
        /// </summary>
        /// <value>The service username.</value>
        public string ServiceUsername
        {
            get { return _serviceUsername; }
            set { _serviceUsername = value; }
        }

        private string _servicePassword;

        /// <summary>
        /// Gets or sets the password for the remote Web service endpoint.
        /// </summary>
        /// <value>The service password.</value>
        public string ServicePassword
        {
            get { return _servicePassword; }
            set { _servicePassword = value; }
        }

        private string _serviceDatabaseServer;

        /// <summary>
        /// Gets or sets the database name to publish to on the remote server.
        /// </summary>
        /// <value>The service database server.</value>
        public string ServiceDatabaseServer
        {
            get { return _serviceDatabaseServer; }
            set { _serviceDatabaseServer = value; }
        }

        private string _serviceDatabase;

        /// <summary>
        /// Gets or sets the remote database server name.
        /// </summary>
        /// <value>The service database.</value>
        public string ServiceDatabase
        {
            get { return _serviceDatabase; }
            set { _serviceDatabase = value; }
        }

        private bool _noTransaction;

        /// <summary>
        /// Gets or sets a value indicating whether the publish operation
        /// should not be executed within a single transaction.
        /// </summary>
        /// <value><c>true</c> to not publish in a single transaction; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Specifies that the publish operation should not be executed
        /// within a single transaction.  This reduces logging overhead
        /// on the target server, but if the publish is interrupted, the
        /// target database may be left in a partially populated state.
        /// </remarks>
        public bool NoTransaction
        {
            get { return _noTransaction; }
            set { _noTransaction = value; }
        }
        #endregion

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            if (string.IsNullOrEmpty(ToolPath))
            {
                string path = Registry.GetValue(InstallPathKey, "Path", ToolName) as string;
                // default key contains the full path, no need to combine tool name.
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return Path.Combine(ToolPath, ToolName);
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get { return "SqlPubWiz.exe"; }
        }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            if (_command == SqlPubCommands.Publish)
                AppendPublishCommands(builder);
            else
                AppendScriptCommands(builder);

            builder.AppendSwitchIfNotNull("-C ", _connectionString);
            builder.AppendSwitchIfNotNull("-d ", _database);
            builder.AppendSwitchIfNotNull("-U ", _username);
            builder.AppendSwitchIfNotNull("-P ", _password);
            builder.AppendSwitchIfNotNull("-S ", _server);

            if (_schemaOnly)
                builder.AppendSwitch("-schemaonly");
            if (_dataOnly)
                builder.AppendSwitch("-dataonly");
            if (_noSchemaQualify)
                builder.AppendSwitch("-noschemaqualify");
            if (_noDropExisting)
                builder.AppendSwitch("-nodropexisting");
            if (_quiet)
                builder.AppendSwitch("-q");

            builder.AppendSwitchIfNotNull("-targetserver ", _targetServer);
            
            return builder.ToString();
        }

        /// <summary>
        /// Logs the starting point of the run to all registered loggers.
        /// </summary>
        /// <param name="message">A descriptive message to provide loggers, usually the command line and switches.</param>
        protected override void LogToolCommand(string message)
        {
            Log.LogCommandLine(MessageImportance.Low, message);
        }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardOutputLoggingImportance
        {
            get { return MessageImportance.Normal; }
        }

        private void AppendPublishCommands(CommandLineBuilder builder)
        {
            builder.AppendSwitch("publish");
            builder.AppendSwitchIfNotNull("-R ", _hosterName);
            builder.AppendSwitchIfNotNull("-RW ", _webServiceAddress);
            builder.AppendSwitchIfNotNull("-RWU ", _serviceUsername);
            builder.AppendSwitchIfNotNull("-RWP ", _servicePassword);
            builder.AppendSwitchIfNotNull("-RS ", _serviceDatabaseServer);
            builder.AppendSwitchIfNotNull("-RD ", _serviceDatabase);
            if (_noTransaction)
                builder.AppendSwitch("-notransaction");

        }

        private void AppendScriptCommands(CommandLineBuilder builder)
        {
            builder.AppendSwitch("script");
            builder.AppendFileNameIfNotNull(_output);
            builder.AppendSwitch("-f");            
        }
    }
}
