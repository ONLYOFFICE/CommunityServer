/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Data.Backup.Extensions;

namespace ASC.Data.Backup.Tasks
{
    public class DbFactory
    {
        private readonly Dictionary<string, ConnectionStringSettings> _connectionStrings = new Dictionary<string, ConnectionStringSettings>();
        private readonly Dictionary<string, DbProviderFactory> _providerFactories = new Dictionary<string, DbProviderFactory>();
        private readonly string _configPath;

        public DbFactory(string configPath)
        {
            if (!configPath.EndsWith(".config"))
                configPath = Path.Combine(configPath, "web.config");

            _configPath = configPath;
            LoadConnectionStrings();
        }

        public IDbConnection OpenConnection(string connectionStringName)
        {
            DbConnection connection = GetDbProviderFactory(GetProviderName(connectionStringName)).CreateConnection();
            if (connection != null)
            {
                connection.ConnectionString = GetConnectionString(connectionStringName);
                connection.Open();
            }
            return connection;
        }

        public IDbDataAdapter CreateDataAdapter(string connectionStringName)
        {
            return GetDbProviderFactory(GetProviderName(connectionStringName)).CreateDataAdapter();
        }

        public IDbCommand CreateLastInsertIdCommand(string connectionStringName)
        {
            var command = GetDbProviderFactory(GetProviderName(connectionStringName)).CreateCommand();
            if (command != null)
                command.CommandText =
                    GetProviderName(connectionStringName).IndexOf("MySql", StringComparison.OrdinalIgnoreCase) != -1
                        ? "select Last_Insert_Id();"
                        : "select last_insert_rowid();";
            return command;
        }

        private string GetProviderName(string connectionStringName)
        {
            return _connectionStrings[connectionStringName].ProviderName;
        }

        private string GetConnectionString(string connectionStringName)
        {
            string connectionString = _connectionStrings[connectionStringName].ConnectionString;
            if (!connectionString.Contains("Connection Timeout"))
            {
                connectionString = connectionString.TrimEnd(';') + ";Connection Timeout=90";
            }
            return connectionString;
        }

        private DbProviderFactory GetDbProviderFactory(string invariantName)
        {
            if (!_providerFactories.ContainsKey(invariantName))
            {
                var xconfig = XDocument.Load(_configPath);
                var provider = xconfig.XPathSelectElement("/configuration/system.data/DbProviderFactories/add[@invariant='" + invariantName + "']");

                var typeName = provider.Attribute("type").ValueOrDefault();

                if (!string.IsNullOrEmpty(typeName))
                {
                    var type = Type.GetType(typeName, false);
                    if (type != null)
                    {
                        var factory = (DbProviderFactory)Activator.CreateInstance(type, true);
                        _providerFactories.Add(invariantName, factory);
                    }
                }
            }

            return _providerFactories[invariantName];
        }

        private void LoadConnectionStrings()
        {
            var config = FileUtility.OpenConfigurationFile(_configPath);

            foreach (var connectionString in config.ConnectionStrings.ConnectionStrings.Cast<ConnectionStringSettings>().Where(x => x.Name != "readonly"))
            {
                if (connectionString.ConnectionString.Contains("|DataDirectory|"))
                    connectionString.ConnectionString = connectionString.ConnectionString.Replace("|DataDirectory|", Path.GetDirectoryName(config.FilePath) + '\\');

                _connectionStrings.Add(connectionString.Name, connectionString);
            }
        }
    }
}
