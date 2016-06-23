/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Data.Backup.Utils;

namespace ASC.Data.Backup.Tasks
{
    public class DbFactory
    {
        public const string DefaultConnectionStringName = "default";

        private readonly string configPath;
        private readonly string connectionStringName;

        private ConnectionStringSettings connectionStringSettings;
        private DbProviderFactory dbProviderFactory;

        private ConnectionStringSettings ConnectionStringSettings
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

        public IDbConnection OpenConnection()
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
            return DbProviderFactory.CreateDataAdapter();
        }

        public IDbCommand CreateLastInsertIdCommand()
        {
            var command = DbProviderFactory.CreateCommand();
            if (command != null)
                command.CommandText =
                    ConnectionStringSettings.ProviderName.IndexOf("MySql", StringComparison.OrdinalIgnoreCase) != -1
                        ? "select Last_Insert_Id();"
                        : "select last_insert_rowid();";
            return command;
        }

        public IDbCommand CreateShowColumnsCommand(string tableName)
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
