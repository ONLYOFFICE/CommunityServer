using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

using ASC.Common.Data;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class MailTableSpecifics
    {
        public Dictionary<string, List<object>> findValues = new Dictionary<string, List<object>>()
        {
            {
                "domain", new List<object>()
            },
            {
                "id", new List<object>()
            },
            {
                "email", new List<object>()
            },
            {
                "reversedomain", new List<object>()
            },
        };

        private readonly MailTableInfo[] _tables = new[]
            {
                new MailTableInfo("maddr", "domain", "reversedomain"),
                new MailTableInfo("dkim", "domain_name", "domain"),
                new MailTableInfo("domain", "domain"),

                new MailTableInfo("mailbox", "username", "email"),
                new MailTableInfo("alias", "address", "email"),
                new MailTableInfo("used_quota", "username", "email"),

                new MailTableInfo("msgs", "sid", "id"),
                new MailTableInfo("msgrcpt", "rid", "id"),

                new MailTableInfo("bayes_seen"),
                new MailTableInfo("bayes_token")
            };

        public IEnumerable<MailTableInfo> Tables => _tables;

        public DbCommand CreateSelectCommand(DbConnection connection, MailTableInfo table, int limit, int offset)
        {
            return connection.CreateCommand(string.Format("select t.* from {0} as t {1} limit {2},{3};", table.Name, GetSelectCommandConditionText(table), offset, limit));
        }

        protected string GetSelectCommandConditionText(MailTableInfo table)
        {
            return table.existFindColumn ? string.Format("where t.{0} in ({1})", table.FindColumn, string.Join(",", findValues[table.FindColumnKey])) : string.Empty;
        }


        public DbCommand CreateInsertCommand(DbConnection connection, MailTableInfo table, DataRowInfo row)
        {
            Dictionary<string, object> valuesForInsert;
            if (!TryPrepareRow(connection, table, row, out valuesForInsert))
                return null;

            var columns = valuesForInsert.Keys.Intersect(table.Columns).ToArray();

            var insertCommantText = string.Format("{0} into {1}({2}) values({3});",
                                                  "insert ignore",
                                                  table.Name,
                                                  string.Join(",", columns.Select(c =>$"`{c}`")),
                                                  string.Join(",", columns.Select(c =>$"@{c}".Replace('-','_'))));

            var command = connection.CreateCommand(insertCommantText);
            foreach (var parameter in valuesForInsert)
            {
                command.AddParameter(parameter.Key.Replace('-', '_'), parameter.Value);
            }
            return command;
        }

        protected virtual bool TryPrepareRow(DbConnection connection, MailTableInfo table, DataRowInfo row, out Dictionary<string, object> preparedRow)
        {
            preparedRow = new Dictionary<string, object>();

            foreach (var columnName in row.ColumnNames)
            {
                preparedRow.Add(columnName, row[columnName]);
            }

            return true;
        }
    }

    internal class MailTableInfo
    {
        public string Name { get; private set; }
        public string[] Columns { get; set; }
        public string FindColumn { get; private set; }
        public string FindColumnKey { get; private set; }
        public bool existFindColumn { get; private set; }

        public MailTableInfo(string name)
        {
            Name = name;
            existFindColumn = false;
        }

        public MailTableInfo(string name, string findColumn, string findColumnKey = null)
        {
            Name = name;
            FindColumn = findColumn;
            FindColumnKey = findColumnKey ?? findColumn;
            existFindColumn = true;
        }
    }
}
