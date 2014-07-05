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
using System.Data;
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

        public IDbCommand CreateSelectCommand(IDbConnection connection, int tenantId, TableInfo table)
        {
            return connection.CreateCommand(string.Format("select t.* from {0} as t {1};", table.Name, GetSelectCommandConditionText(tenantId, table)));
        }

        public IDbCommand CreateDeleteCommand(IDbConnection connection, int tenantId, TableInfo table)
        {
            var commandText = string.Format("delete t.* from {0} as t {1};", table.Name, GetDeleteCommandConditionText(tenantId, table));
            return connection.CreateCommand(commandText);
        }

        public IDbCommand CreateInsertCommand(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row)
        {
            if (table.InsertMethod == InsertMethod.None)
                return null;

            Dictionary<string, object> valuesForInsert;
            if (!TryPrepareRow(connection, columnMapper, table, row, out valuesForInsert))
                return null;

            var insertCommantText = string.Format("{0} into {1}({2}) values({3});",
                                                  table.InsertMethod != InsertMethod.Ignore
                                                      ? table.InsertMethod.ToString().ToLower()
                                                      : "insert ignore",
                                                  table.Name,
                                                  string.Join(",", valuesForInsert.Keys),
                                                  string.Join(",", valuesForInsert.Keys.Select(x => "@" + x)));

            IDbCommand command = connection.CreateCommand(insertCommantText);
            foreach (var parameter in valuesForInsert)
            {
                command.AddParameter(parameter.Key, parameter.Value);
            }
            return command;
        }

        public virtual bool TryAdjustFilePath(ColumnMapper columnMapper, ref string filePath)
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

        protected virtual bool TryPrepareRow(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row, out Dictionary<string, object> preparedRow)
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
                    if (!TryPrepareValue(connection, columnMapper, table, columnName, parentRelations[columnName], ref val))
                        return false;
                }

                preparedRow.Add(columnName, val);
            }

            return true;
        }

        protected virtual bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, ref object value)
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

        protected virtual bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, TableInfo table, string columnName, IEnumerable<RelationInfo> relations, ref object value)
        {
            return TryPrepareValue(connection, columnMapper, relations.Single(), ref value);
        }

        protected virtual bool TryPrepareValue(IDbConnection connection, ColumnMapper columnMapper, RelationInfo relation, ref object value)
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
                   (Guid.TryParse(Convert.ToString(value), out guidVal) && guidVal == Guid.Empty) ||
                   (int.TryParse(Convert.ToString(value), out intVal) && intVal <= 0);
        }
    }
}
