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
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Data.Storage;


namespace ASC.Data.Backup
{
    public class DbBackupProvider : IBackupProvider
    {
        private readonly List<string> processedTables = new List<string>();


        public string Name
        {
            get { return "databases"; }
        }

        public IEnumerable<XElement> GetElements(int tenant, string[] configs, IDataWriteOperator writer)
        {
            processedTables.Clear();
            var xml = new List<XElement>();
            var connectionKeys = new Dictionary<string, string>();
            foreach (var connectionString in GetConnectionStrings(configs))
            {
                //do not save the base, having the same provider and connection string is not to duplicate
                //data, but also expose the ref attribute of repetitive bases for the correct recovery
                var node = new XElement(connectionString.Name);
                xml.Add(node);

                var connectionKey = connectionString.ProviderName + connectionString.ConnectionString;
                if (connectionKeys.ContainsKey(connectionKey))
                {
                    node.Add(new XAttribute("ref", connectionKeys[connectionKey]));
                }
                else
                {
                    connectionKeys.Add(connectionKey, connectionString.Name);
                    node.Add(BackupDatabase(tenant, connectionString, writer));
                }
            }

            return xml;
        }

        public void LoadFrom(IEnumerable<XElement> elements, int tenant, string[] configs, IDataReadOperator reader)
        {
            processedTables.Clear();
            foreach (var connectionString in GetConnectionStrings(configs))
            {
                RestoreDatabase(connectionString, elements, reader);
            }
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;


        private void OnProgressChanged(string status, int progress)
        {
            if (ProgressChanged != null) ProgressChanged(this, new ProgressChangedEventArgs(status, progress));
        }


        private Configuration GetConfiguration(string config)
        {
            if (config.Contains(Path.DirectorySeparatorChar) && !Uri.IsWellFormedUriString(config, UriKind.Relative))
            {
                var map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = string.Compare(Path.GetExtension(config), ".config", true) == 0 ?
                    config :
                    Path.Combine(config, "Web.config");
                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
            return WebConfigurationManager.OpenWebConfiguration(config);
        }

        private IEnumerable<ConnectionStringSettings> GetConnectionStrings(string[] configs)
        {
            if (configs.Length == 0)
            {
                configs = new string[] { AppDomain.CurrentDomain.SetupInformation.ConfigurationFile };
            }
            var connectionStrings = new List<ConnectionStringSettings>();
            foreach (var config in configs)
            {
                connectionStrings.AddRange(GetConnectionStrings(GetConfiguration(config)));
            }
            return connectionStrings.GroupBy(cs => cs.Name).Select(g => g.First());
        }

        private IEnumerable<ConnectionStringSettings> GetConnectionStrings(Configuration cfg)
        {
            var connectionStrings = new List<ConnectionStringSettings>();
            foreach (ConnectionStringSettings connectionString in cfg.ConnectionStrings.ConnectionStrings)
            {
                if (connectionString.Name == "LocalSqlServer" || connectionString.Name == "readonly") continue;
                connectionStrings.Add(connectionString);
                if (connectionString.ConnectionString.Contains("|DataDirectory|"))
                {
                    connectionString.ConnectionString = connectionString.ConnectionString.Replace("|DataDirectory|", Path.GetDirectoryName(cfg.FilePath) + '\\');
                }
            }
            return connectionStrings;
        }

        private List<XElement> BackupDatabase(int tenant, ConnectionStringSettings connectionString, IDataWriteOperator writer)
        {
            var xml = new List<XElement>();
            var errors = 0;
            var timeout = TimeSpan.FromSeconds(1);

            using (var dbHelper = new DbHelper(connectionString))
            {
                var tables = dbHelper.GetTables();
                for (int i = 0; i < tables.Count; i++)
                {
                    var table = tables[i];
                    OnProgressChanged(table, (int)(i / (double)tables.Count * 100));

                    if (processedTables.Contains(table, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    xml.Add(new XElement(table));
                    DataTable dataTable = null;
                    while (true)
                    {
                        try
                        {
                            dataTable = dbHelper.GetTable(table, tenant);
                            break;
                        }
                        catch
                        {
                            errors++;
                            if (20 < errors) throw;
                            Thread.Sleep(timeout);
                        }
                    }
                    foreach (DataColumn c in dataTable.Columns)
                    {
                        if (c.DataType == typeof(DateTime)) c.DateTimeMode = DataSetDateTime.Unspecified;
                    }

                    var tmp = Path.GetTempFileName();
                    using (var file = File.OpenWrite(tmp))
                    {
                        dataTable.WriteXml(file, XmlWriteMode.WriteSchema);
                    }

                    writer.WriteEntry(string.Format("{0}\\{1}\\{2}", Name, connectionString.Name, table).ToLower(), tmp);
                    File.Delete(tmp);

                    processedTables.Add(table);
                }
            }
            return xml;
        }

        private void RestoreDatabase(ConnectionStringSettings connectionString, IEnumerable<XElement> elements, IDataReadOperator reader)
        {
            var dbName = connectionString.Name;
            var dbElement = elements.SingleOrDefault(e => string.Compare(e.Name.LocalName, connectionString.Name, true) == 0);
            if (dbElement != null && dbElement.Attribute("ref") != null)
            {
                dbName = dbElement.Attribute("ref").Value;
                dbElement = elements.Single(e => string.Compare(e.Name.LocalName, dbElement.Attribute("ref").Value, true) == 0);
            }
            if (dbElement == null) return;

            using (var dbHelper = new DbHelper(connectionString))
            {
                var tables = dbHelper.GetTables();
                for (int i = 0; i < tables.Count; i++)
                {
                    var table = tables[i];
                    OnProgressChanged(table, (int)(i / (double)tables.Count * 100));

                    if (processedTables.Contains(table, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (dbElement.Element(table) != null)
                    {
                        using (var stream = reader.GetEntry(string.Format("{0}\\{1}\\{2}", Name, dbName, table).ToLower()))
                        {
                            var data = new DataTable();
                            data.ReadXml(stream);
                            dbHelper.SetTable(data);
                        }
                        processedTables.Add(table);
                    }
                }
            }
        }

        private class DbHelper : IDisposable
        {
            private readonly DbProviderFactory factory;
            private readonly DbConnection connect;
            private readonly DbCommandBuilder builder;
            private readonly DataTable columns;
            private readonly bool mysql;
            private readonly IDictionary<string, string> whereExceptions = new Dictionary<string, string>();
            private readonly static ILog log = LogManager.GetLogger("ASC.Data");

            public DbHelper(ConnectionStringSettings connectionString)
            {
                var file = connectionString.ElementInformation.Source;
                if ("web.connections.config".Equals(Path.GetFileName(file), StringComparison.InvariantCultureIgnoreCase))
                {
                    file = Path.Combine(Path.GetDirectoryName(file), "Web.config");
                }
                var xconfig = XDocument.Load(file);
                var provider = xconfig.XPathSelectElement("/configuration/system.data/DbProviderFactories/add[@invariant='" + connectionString.ProviderName + "']");
                factory = (DbProviderFactory)Activator.CreateInstance(Type.GetType(provider.Attribute("type").Value, true));
                builder = factory.CreateCommandBuilder();
                connect = factory.CreateConnection();
                connect.ConnectionString = connectionString.ConnectionString;
                connect.Open();

                mysql = connectionString.ProviderName.ToLower().Contains("mysql");
                if (mysql)
                {
                    CreateCommand("set @@session.sql_mode = concat(@@session.sql_mode, ',NO_AUTO_VALUE_ON_ZERO')").ExecuteNonQuery();
                }

                columns = connect.GetSchema("Columns");

                whereExceptions["calendar_calendar_item"] = " where calendar_id in (select id from calendar_calendars where tenant = {0}) ";
                whereExceptions["calendar_calendar_user"] = " where calendar_id in (select id from calendar_calendars where tenant = {0}) ";
                whereExceptions["calendar_event_item"] = " inner join calendar_events on calendar_event_item.event_id = calendar_events.id where calendar_events.tenant = {0} ";
                whereExceptions["calendar_event_user"] = " inner join calendar_events on calendar_event_user.event_id = calendar_events.id where calendar_events.tenant = {0} ";
                whereExceptions["crm_entity_contact"] = " inner join crm_contact on crm_entity_contact.contact_id = crm_contact.id where crm_contact.tenant_id = {0} ";
                whereExceptions["crm_entity_tag"] = " inner join crm_tag on crm_entity_tag.tag_id = crm_tag.id where crm_tag.tenant_id = {0} ";
                whereExceptions["files_folder_tree"] = " inner join files_folder on folder_id = id where tenant_id = {0} ";
                whereExceptions["forum_answer_variant"] = " where answer_id in (select id from forum_answer where tenantid = {0})";
                whereExceptions["forum_topic_tag"] = " where topic_id in (select id from forum_topic where tenantid = {0})";
                whereExceptions["forum_variant"] = " where question_id in (select id from forum_question where tenantid = {0})";
                whereExceptions["projects_project_participant"] = " inner join projects_projects on projects_project_participant.project_id = projects_projects.id where projects_projects.tenant_id = {0} ";
                whereExceptions["projects_following_project_participant"] = " inner join projects_projects on projects_following_project_participant.project_id = projects_projects.id where projects_projects.tenant_id = {0} ";
                whereExceptions["projects_project_tag"] = " inner join projects_projects on projects_project_tag.project_id = projects_projects.id where projects_projects.tenant_id = {0} ";
                whereExceptions["tenants_tenants"] = " where id = {0}";
                whereExceptions["core_acl"] = " where tenant = {0} or tenant = -1";
                whereExceptions["core_subscription"] = " where tenant = {0} or tenant = -1";
                whereExceptions["core_subscriptionmethod"] = " where tenant = {0} or tenant = -1";
            }

            public List<string> GetTables()
            {
                var allowTables = new List<string>
                {
                    "blogs_",
                    "bookmarking_",
                    "calendar_",
                    "core_",
                    "crm_",
                    "events_",
                    "files_",
                    "forum_",
                    "photo_",
                    "projects_",
                    "tenants_",
                    "webstudio_",
                    "wiki_",
                };

                var disallowTables = new List<string>
                {
                    "core_settings",
                    "webstudio_uservisit",
                    "webstudio_useractivity",
                    "tenants_tariff",
                    "tenants_forbiden",
                };

                IEnumerable<string> tables;

                if (mysql)
                {
                    tables = CreateCommand("show tables")
                        .ExecuteList()
                        .ConvertAll(r => (string)r[0]);
                }
                else
                {
                    tables = connect
                        .GetSchema("Tables")
                        .Select(@"TABLE_TYPE <> 'SYSTEM_TABLE'")
                        .Select(row => ((string)row["TABLE_NAME"]));
                }

                return tables
                    .Where(t => allowTables.Any(a => t.StartsWith(a)) && !disallowTables.Any(d => t.StartsWith(d)))
                    .ToList();
            }

            public DataTable GetTable(string table, int tenant)
            {
                try
                {
                    var dataTable = new DataTable(table);
                    var adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = CreateCommand("select " + Quote(table) + ".* from " + Quote(table) + GetWhere(table, tenant));

                    log.Debug(adapter.SelectCommand.CommandText);

                    adapter.Fill(dataTable);
                    return dataTable;
                }
                catch (Exception error)
                {
                    log.ErrorFormat("Table {0}: {1}", table, error);
                    throw;
                }
            }


            public void SetTable(DataTable table)
            {
                using (var tx = connect.BeginTransaction())
                {
                    try
                    {
                        if ("tenants_tenants".Equals(table.TableName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // remove last tenant
                            var tenantid = CreateCommand("select id from tenants_tenants order by id desc limit 1").ExecuteScalar<int>();
                            CreateCommand("delete from tenants_tenants where id = " + tenantid).ExecuteNonQuery();
                            if (table.Columns.Contains("mappeddomain"))
                            {
                                foreach (var r in table.Rows.Cast<DataRow>())
                                {
                                    r[table.Columns["mappeddomain"]] = null;
                                    if (table.Columns.Contains("id"))
                                    {
                                        CreateCommand("update tenants_tariff set tenant = " + r[table.Columns["id"]] + " where tenant = " + tenantid).ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        var sql = new StringBuilder("replace into " + Quote(table.TableName) + "(");

                        var tableColumns = GetColumnsFrom(table.TableName)
                            .Intersect(table.Columns.Cast<DataColumn>().Select(c => c.ColumnName), StringComparer.InvariantCultureIgnoreCase)
                            .ToList();

                        tableColumns.ForEach(column => sql.AppendFormat("{0}, ", Quote(column)));
                        sql.Replace(", ", ") values (", sql.Length - 2, 2);

                        var insert = connect.CreateCommand();
                        tableColumns.ForEach(column =>
                        {
                            sql.AppendFormat("@{0}, ", column);
                            var p = insert.CreateParameter();
                            p.ParameterName = "@" + column;
                            insert.Parameters.Add(p);
                        });
                        sql.Replace(", ", ")", sql.Length - 2, 2);
                        insert.CommandText = sql.ToString();

                        foreach (var r in table.Rows.Cast<DataRow>())
                        {
                            foreach (var c in tableColumns)
                            {
                                ((IDbDataParameter)insert.Parameters["@" + c]).Value = r[c];
                            }
                            insert.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Table {0}: {1}", table, e);
                    }
                }
            }

            public void Dispose()
            {
                builder.Dispose();
                connect.Dispose();
            }

            private DbCommand CreateCommand(string sql)
            {
                var command = connect.CreateCommand();
                command.CommandText = sql;
                return command;
            }

            private string Quote(string identifier)
            {
                return identifier;
            }

            private IEnumerable<string> GetColumnsFrom(string table)
            {
                if (mysql)
                {
                    return CreateCommand("show columns from " + Quote(table))
                        .ExecuteList()
                        .ConvertAll(r => (string)r[0]);
                }
                else
                {
                    return columns.Select(string.Format("TABLE_NAME = '{0}'", table))
                        .Select(r => r["COLUMN_NAME"].ToString());
                }
            }

            private string GetWhere(string tableName, int tenant)
            {
                if (tenant == -1) return string.Empty;

                if (whereExceptions.ContainsKey(tableName.ToLower()))
                {
                    return string.Format(whereExceptions[tableName.ToLower()], tenant);
                }
                var tenantColumn = GetColumnsFrom(tableName).FirstOrDefault(c => c.ToLower().StartsWith("tenant"));
                return tenantColumn != null ?
                    " where " + Quote(tenantColumn) + " = " + tenant :
                    " where 1 = 0";
            }
        }
    }
}
