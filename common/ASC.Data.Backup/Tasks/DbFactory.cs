/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Data.Backup.Utils;
using MySql.Data.MySqlClient;

namespace ASC.Data.Backup.Tasks
{
    public class DbFactory
    {
        public const string DefaultConnectionStringName = "default";

        private readonly string configPath;
        private readonly string connectionStringName;

        private ConnectionStringSettings connectionStringSettings;
        private DbProviderFactory dbProviderFactory;

        internal ConnectionStringSettings ConnectionStringSettings
        {
            get
            {
                if (connectionStringSettings == null)
                {
                    var configuration = ConfigurationProvider.Open(configPath);
                    connectionStringSettings = configuration.ConnectionStrings.ConnectionStrings[connectionStringName];
                }
                return connectionStringSettings;
            }
        }

        private DbProviderFactory DbProviderFactory
        {
            get
            {
                if (dbProviderFactory == null)
                {
                    var xconfig = XDocument.Load(configPath);
                    var providerInvariantName = ConnectionStringSettings.ProviderName;
                    var provider = xconfig.XPathSelectElement("/configuration/system.data/DbProviderFactories/add[@invariant='" + providerInvariantName + "']");

                    var type = Type.GetType(provider.Attribute("type").Value, true);
                    dbProviderFactory = (DbProviderFactory)Activator.CreateInstance(type, true);
                }
                return dbProviderFactory;
            }
        }

        public DbFactory(string configPath, string connectionStringName = DefaultConnectionStringName)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                throw new ArgumentException("Empty config path.", "configPath");
            }
            if (string.IsNullOrEmpty(connectionStringName))
            {
                connectionStringName = DefaultConnectionStringName;
            }
            this.configPath = PathHelper.ToRootedConfigPath(configPath);
            this.connectionStringName = connectionStringName;
        }

        public DbConnection OpenConnection()
        {
            var connection = DbProviderFactory.CreateConnection();
            if (connection != null)
            {
                connection.ConnectionString = EnsureConnectionTimeout(ConnectionStringSettings.ConnectionString);
                connection.Open();
            }
            return connection;
        }

        public IDbDataAdapter CreateDataAdapter()
        {
            var result = DbProviderFactory.CreateDataAdapter();
            if (result == null && DbProviderFactory is MySqlClientFactory)
            {
                result = new MySqlDataAdapter();
            }
            return result;
        }

        public DbCommand CreateLastInsertIdCommand()
        {
            var command = DbProviderFactory.CreateCommand();
            if (command != null)
                command.CommandText =
                    ConnectionStringSettings.ProviderName.IndexOf("MySql", StringComparison.OrdinalIgnoreCase) != -1
                        ? "select Last_Insert_Id();"
                        : "select last_insert_rowid();";
            return command;
        }

        public DbCommand CreateShowColumnsCommand(string tableName)
        {
            var command = DbProviderFactory.CreateCommand();
            if (command != null)
            {
                command.CommandText = "show columns from " + tableName + ";";
            }
            return command;
        }
        
        private static string EnsureConnectionTimeout(string connectionString)
        {
            if (!connectionString.Contains("Connection Timeout"))
            {
                connectionString = connectionString.TrimEnd(';') + ";Connection Timeout=90";
            }
            return connectionString;
        }
    }
}
