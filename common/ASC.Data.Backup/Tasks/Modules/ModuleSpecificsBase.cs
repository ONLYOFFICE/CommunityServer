/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using ASC.Common.Data;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal abstract class ModuleSpecificsBase : IModuleSpecifics
    {
        public abstract ModuleName ModuleName { get; }

        private string _connectionStringName;
        public virtual string ConnectionStringName
        {
            get { return _connectionStringName ?? (_connectionStringName = ModuleName.ToString().ToLower()); }
        }

        public abstract IEnumerable<TableInfo> Tables { get; }
        public abstract IEnumerable<RelationInfo> TableRelations { get; }

        public IEnumerable<TableInfo> GetTablesOrdered()
        {
            var notOrderedTables = new List<TableInfo>(Tables);

            int totalTablesCount = notOrderedTables.Count;
            int orderedTablesCount = 0;
            while (orderedTablesCount < totalTablesCount)
            {
                int orderedTablesCountBeforeIter = orderedTablesCount; // ensure we not in infinite loop...

                int i = 0;
                while (i < notOrderedTables.Count)
                {
                    var table = notOrderedTables[i];

                    var parentTables = TableRelations
                        .Where(x => x.FitsForTable(table.Name) && !x.IsExternal() && !x.IsSelfRelation() && x.Importance != RelationImportance.Low)
                        .Select(x => x.ParentTable);

                    if (parentTables.All(x => notOrderedTables.All(y => !string.Equals(y.Name, x, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        notOrderedTables.RemoveAt(i);
                        orderedTablesCount++;
                        yield return table;
                    }
                    else
                    {
                        i++;
                    }
                }

                if (orderedTablesCountBeforeIter == orderedTablesCount) // ensure we not in infinite loop...
                    throw ThrowHelper.CantOrderTables(notOrderedTables.Select(x => x.Name));
            }
        }

        public DbCommand CreateSelectCommand(DbConnection connection, int tenantId, TableInfo table, int limit, int offset)
        {
            return connection.CreateCommand(string.Format("select t.* from {0} as t {1} limit {2},{3};", table.Name, GetSelectCommandConditionText(tenantId, table), offset, limit));
        }

        public DbCommand CreateDeleteCommand(DbConnection connection, int tenantId, TableInfo table)
        {
            var commandText = string.Format("delete t.* from {0} as t {1};", table.Name, GetDeleteCommandConditionText(tenantId, table));
            return connection.CreateCommand(commandText);
        }

        public DbCommand CreateInsertCommand(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row)
        {
            if (table.InsertMethod == InsertMethod.None)
                return null;

            Dictionary<string, object> valuesForInsert;
            if (!TryPrepareRow(dump, connection, columnMapper, table, row, out valuesForInsert))
                return null;

            var columns = valuesForInsert.Keys.Intersect(table.Columns).ToArray();

            var insertCommantText = string.Format("{0} into {1}({2}) values({3});",
                                                  table.InsertMethod != InsertMethod.Ignore
                                                      ? table.InsertMethod.ToString().ToLower()
                                                      : "insert ignore",
                                                  table.Name,
                                                  string.Join(",", columns),
                                                  string.Join(",", columns.Select(c => "@" + c)));

            var command = connection.CreateCommand(insertCommantText);
            foreach (var parameter in valuesForInsert)
            {
                command.AddParameter(parameter.Key, parameter.Value);
            }
            return command;
        }

        public virtual bool TryAdjustFilePath(bool dump, ColumnMapper columnMapper, ref string filePath)
        {
            return true;
        }

        protected virtual string GetSelectCommandConditionText(int tenantId, TableInfo table)
        {
            if (!table.HasTenantColumn())
                throw ThrowHelper.CantDetectTenant(table.Name);

            return string.Format("where t.{0} = {1}", table.TenantColumn, tenantId);
        }

        protected virtual string GetDeleteCommandConditionText(int tenantId, TableInfo table)
        {
            return GetSelectCommandConditionText(tenantId, table);
        }

        protected virtual bool TryPrepareRow(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row, out Dictionary<string, object> preparedRow)
        {
            preparedRow = new Dictionary<string, object>();

            var parentRelations = TableRelations
                .Where(x => x.FitsForRow(row) && x.Importance != RelationImportance.Low)
                .GroupBy(x => x.ChildColumn)
                .ToDictionary(x => x.Key);

            foreach (var columnName in row.ColumnNames)
            {
                if (table.IdType == IdType.Autoincrement && columnName.Equals(table.IdColumn, StringComparison.OrdinalIgnoreCase))
                    continue;

                var val = row[columnName];
                if (!parentRelations.ContainsKey(columnName))
                {
                    if (!TryPrepareValue(connection, columnMapper, table, columnName, ref val))
                        return false;
                }
                else
                {
                    if (!TryPrepareValue(dump, connection, columnMapper, table, columnName, parentRelations[columnName], ref val))
                        return false;

                    if (!table.HasIdColumn() && !table.HasTenantColumn() && val == row[columnName])
                        return false;
                }

                preparedRow.Add(columnName, val);
            }

            return true;
        }

        protected virtual bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
        {
            if (columnName.Equals(table.TenantColumn, StringComparison.OrdinalIgnoreCase))
            {
                int tenantMapping = columnMapper.GetTenantMapping();
                if (tenantMapping < 1)
                    return false;
                value = tenantMapping;
                return true;
            }

            if (table.UserIDColumns.Any(x => columnName.Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                var strVal = Convert.ToString(value);
                string userMapping = columnMapper.GetUserMapping(strVal);
                if (userMapping == null)
                    return Helpers.IsEmptyOrSystemUser(strVal);
                value = userMapping;
                return true;
            }

            var mapping = columnMapper.GetMapping(table.Name, columnName, value);
            if (mapping != null)
                value = mapping;

            return true;
        }

        protected virtual bool TryPrepareValue(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
        {
            return TryPrepareValue(connection, columnMapper, relations.Single(), ref value);
        }

        protected virtual bool TryPrepareValue(DbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
        {
            var mappedValue = columnMapper.GetMapping(relation.ParentTable, relation.ParentColumn, value);
            if (mappedValue != null)
            {
                value = mappedValue;
                return true;
            }

            Guid guidVal;
            int intVal;
            return value == null ||
                Guid.TryParse(Convert.ToString(value), out guidVal) ||
                int.TryParse(Convert.ToString(value), out intVal);
        }

        public virtual void PrepareData(DataTable data)
        {
            // nothing to do
        }

        public virtual Stream PrepareData(string key, Stream stream, ColumnMapper columnMapper)
        {
            return stream;
        }
    }
}
