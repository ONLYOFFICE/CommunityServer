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
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;

using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Dialects;

namespace ASC.Common.Data
{
    public static class DbRegistry
    {
        private const string DEFAULT = "DEFAULT";
        private static readonly object syncRoot = new object();
        private static readonly IDictionary<string, DbProviderFactory> providers = new Dictionary<string, DbProviderFactory>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly IDictionary<string, string> connnectionStrings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly IDictionary<string, ISqlDialect> dialects = new Dictionary<string, ISqlDialect>(StringComparer.InvariantCultureIgnoreCase);
        private static volatile bool configured = false;

        static DbRegistry()
        {
            dialects["MySql.Data.MySqlClient.MySqlClientFactory"] = new MySQLDialect();
            dialects["System.Data.SQLite.SQLiteFactory"] = new SQLiteDialect();
        }

        internal static void RegisterDatabase(string databaseId, DbProviderFactory providerFactory, string connectionString)
        {
            if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException("databaseId");
            if (providerFactory == null) throw new ArgumentNullException("providerFactory");

            if (!providers.ContainsKey(databaseId))
            {
                lock (syncRoot)
                {
                    if (!providers.ContainsKey(databaseId))
                    {
                        providers.Add(databaseId, providerFactory);
                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            connnectionStrings.Add(databaseId, connectionString);
                        }
                    }
                }
            }
        }

        internal static void RegisterDatabase(string databaseId, string providerInvariantName, string connectionString)
        {
            RegisterDatabase(databaseId, DbProviderFactories.GetFactory(providerInvariantName), connectionString);
        }

        public static void RegisterDatabase(string databaseId, ConnectionStringSettings connectionString)
        {
            RegisterDatabase(databaseId, connectionString.ProviderName, connectionString.ConnectionString);
        }

        public static void UnRegisterDatabase(string databaseId)
        {
            if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException("databaseId");

            if (providers.ContainsKey(databaseId))
            {
                lock (syncRoot)
                {
                    if (providers.ContainsKey(databaseId))
                    {
                        providers.Remove(databaseId);
                    }
                }
            }

            if (connnectionStrings.ContainsKey(databaseId))
            {
                lock (syncRoot)
                {
                    if (connnectionStrings.ContainsKey(databaseId))
                    {
                        connnectionStrings.Remove(databaseId);
                    }
                }
            }
        }

        public static bool IsDatabaseRegistered(string databaseId)
        {
            lock (syncRoot)
            {
                return providers.ContainsKey(databaseId);
            }
        }

        public static DbConnection CreateDbConnection(string databaseId)
        {
            Configure();

            if (!providers.ContainsKey(databaseId))
            {
                databaseId = DEFAULT;
            }

            var connection = providers[databaseId].CreateConnection();
            if (connnectionStrings.ContainsKey(databaseId))
            {
                connection.ConnectionString = connnectionStrings[databaseId];
            }
            return connection;
        }

        public static DbProviderFactory GetDbProviderFactory(string databaseId)
        {
            Configure();

            if (!providers.ContainsKey(databaseId))
            {
                databaseId = DEFAULT;
            }
            return providers.ContainsKey(databaseId) ? providers[databaseId] : null;
        }

        public static ConnectionStringSettings GetConnectionString(string databaseId)
        {
            Configure();

            if (!connnectionStrings.ContainsKey(databaseId))
            {
                databaseId = DEFAULT;
            }
            return connnectionStrings.ContainsKey(databaseId) ? new ConnectionStringSettings(databaseId, connnectionStrings[databaseId], providers[databaseId].GetType().Name) : null;
        }

        public static ISqlDialect GetSqlDialect(string databaseId)
        {
            var provider = GetDbProviderFactory(databaseId);
            if (provider != null && dialects.ContainsKey(provider.GetType().FullName))
            {
                return dialects[provider.GetType().FullName];
            }
            return SqlDialect.Default;
        }


        public static void Configure()
        {
            if (!configured)
            {
                lock (syncRoot)
                {
                    if (!configured)
                    {
                        var factories = DbProviderFactories.GetFactoryClasses();
                        AppDomain.CurrentDomain.SetData("DataDirectory", Environment.CurrentDirectory); //SQLite
                        foreach (ConnectionStringSettings cs in ConfigurationManagerExtension.ConnectionStrings)
                        {
                            var factory = factories.Rows.Find(cs.ProviderName);
                            if (factory == null)
                            {
                                throw new ConfigurationErrorsException("Db factory " + cs.ProviderName + " not found.");
                            }
                            RegisterDatabase(cs.Name, cs);
                        }
                        configured = true;
                    }
                }
            }
        }
    }
}