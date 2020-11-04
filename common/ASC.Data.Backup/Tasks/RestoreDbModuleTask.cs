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
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Tasks.Modules;

namespace ASC.Data.Backup.Tasks
{
    internal class RestoreDbModuleTask : PortalTaskBase
    {
        private const int TransactionLength = 10000;

        private readonly IDataReadOperator _reader;
        private readonly IModuleSpecifics _module;
        private readonly ColumnMapper _columnMapper;
        private readonly DbFactory _factory;
        private readonly bool _replaceDate;
        private readonly bool dump;

        public RestoreDbModuleTask(ILog logger, IModuleSpecifics module, IDataReadOperator reader, ColumnMapper columnMapper, DbFactory factory, bool replaceDate, bool dump)
            : base(logger, -1, null)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (columnMapper == null)
                throw new ArgumentNullException("columnMapper");

            if (factory == null)
                throw new ArgumentNullException("factory");

            _module = module;
            _reader = reader;
            _columnMapper = columnMapper;
            _factory = factory;
            _replaceDate = replaceDate;
            this.dump = dump;
        }

        public override void RunJob()
        {
            Logger.DebugFormat("begin restore data for module {0}", _module.ModuleName);
            SetStepsCount(_module.Tables.Count(t => !IgnoredTables.Contains(t.Name)));

            using (var connection = _factory.OpenConnection())
            {
                foreach (var table in _module.GetTablesOrdered().Where(t => !IgnoredTables.Contains(t.Name) && t.InsertMethod != InsertMethod.None))
                {
                    Logger.DebugFormat("begin restore table {0}", table.Name);

                    var transactionsCommited = 0;
                    var rowsInserted = 0;
                    ActionInvoker.Try(
                        state =>
                            RestoreTable(connection.Fix(), (TableInfo) state, ref transactionsCommited,
                                ref rowsInserted), table, 5,
                        onAttemptFailure: error => _columnMapper.Rollback(),
                        onFailure: error => { throw ThrowHelper.CantRestoreTable(table.Name, error); });

                    SetStepCompleted();
                    Logger.DebugFormat("{0} rows inserted for table {1}", rowsInserted, table.Name);
                }
            }

            Logger.DebugFormat("end restore data for module {0}", _module.ModuleName);
        }

        private void RestoreTable(DbConnection connection, TableInfo tableInfo, ref int transactionsCommited, ref int rowsInserted)
        {
            SetColumns(connection, tableInfo);

            using (var stream = _reader.GetEntry(KeyHelper.GetTableZipKey(_module, tableInfo.Name)))
            {
                var lowImportanceRelations = _module
                    .TableRelations
                    .Where(
                        r =>
                            string.Equals(r.ParentTable, tableInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    .Where(r => r.Importance == RelationImportance.Low && !r.IsSelfRelation())
                    .Select(r => Tuple.Create(r, _module.Tables.Single(t => t.Name == r.ChildTable)))
                    .ToList();

                foreach (
                    IEnumerable<DataRowInfo> rows in
                        GetRows(tableInfo, stream)
                            .Skip(transactionsCommited*TransactionLength)
                            .MakeParts(TransactionLength))
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        int rowsSuccess = 0;
                        foreach (DataRowInfo row in rows)
                        {
                            if (_replaceDate)
                            {
                                foreach (var column in tableInfo.DateColumns)
                                {
                                    _columnMapper.SetDateMapping(tableInfo.Name, column, row[column.Key]);
                                }
                            }

                            object oldIdValue = null;
                            object newIdValue = null;

                            if (tableInfo.HasIdColumn())
                            {
                                oldIdValue = row[tableInfo.IdColumn];
                                newIdValue = _columnMapper.GetMapping(tableInfo.Name, tableInfo.IdColumn, oldIdValue);
                                if (newIdValue == null)
                                {
                                    if (tableInfo.IdType == IdType.Guid)
                                    {
                                        newIdValue = Guid.NewGuid().ToString("D");
                                    }
                                    else if (tableInfo.IdType == IdType.Integer)
                                    {
                                        newIdValue = connection
                                            .CreateCommand(string.Format("select max({0}) from {1};",
                                                tableInfo.IdColumn, tableInfo.Name))
                                            .WithTimeout(120)
                                            .ExecuteScalar<int>() + 1;
                                    }
                                }
                                if (newIdValue != null)
                                {
                                    _columnMapper.SetMapping(tableInfo.Name, tableInfo.IdColumn, oldIdValue,
                                        newIdValue);
                                }
                            }

                            var insertCommand = _module.CreateInsertCommand(dump, connection, _columnMapper, tableInfo,
                                row);
                            if (insertCommand == null)
                            {
                                Logger.WarnFormat("Can't create command to insert row to {0} with values [{1}]", tableInfo,
                                    row);
                                _columnMapper.Rollback();
                                continue;
                            }
                            insertCommand.WithTimeout(120).ExecuteNonQuery();
                            rowsSuccess++;

                            if (tableInfo.HasIdColumn() && tableInfo.IdType == IdType.Autoincrement)
                            {
                                var lastIdCommand = _factory.CreateLastInsertIdCommand();
                                lastIdCommand.Connection = connection;
                                newIdValue = Convert.ToInt32(lastIdCommand.ExecuteScalar());
                                _columnMapper.SetMapping(tableInfo.Name, tableInfo.IdColumn, oldIdValue, newIdValue);
                            }

                            _columnMapper.Commit();

                            foreach (Tuple<RelationInfo, TableInfo> relation in lowImportanceRelations)
                            {
                                if (!relation.Item2.HasTenantColumn())
                                {
                                    Logger.WarnFormat(
                                        "Table {0} does not contain tenant id column. Can't apply low importance relations on such tables.",
                                        relation.Item2.Name);
                                    continue;
                                }

                                object oldValue = row[relation.Item1.ParentColumn];
                                object newValue = _columnMapper.GetMapping(relation.Item1.ParentTable,
                                    relation.Item1.ParentColumn, oldValue);

                                connection.CreateCommand(
                                    string.Format("update {0} set {1} = {2} where {1} = {3} and {4} = {5}",
                                        relation.Item1.ChildTable,
                                        relation.Item1.ChildColumn,
                                        newValue is string ? "'" + newValue + "'" : newValue,
                                        oldValue is string ? "'" + oldValue + "'" : oldValue,
                                        relation.Item2.TenantColumn,
                                        _columnMapper.GetTenantMapping())).WithTimeout(120).ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        transactionsCommited++;
                        rowsInserted += rowsSuccess;
                    }
                }
            }
        }

        private IEnumerable<DataRowInfo> GetRows(TableInfo table, Stream xmlStream)
        {
            if (xmlStream == null)
                return Enumerable.Empty<DataRowInfo>();

            var rows = DataRowInfoReader.ReadFromStream(xmlStream);

            var selfRelation = _module.TableRelations.SingleOrDefault(x => x.ChildTable == table.Name && x.IsSelfRelation());
            if (selfRelation != null)
            {
                rows = rows
                    .ToTree(x => x[selfRelation.ParentColumn], x => x[selfRelation.ChildColumn])
                    .SelectMany(x => OrderNode(x));
            }

            return rows;
        }

        private static IEnumerable<DataRowInfo> OrderNode(TreeNode<DataRowInfo> node)
        {
            var result = new List<DataRowInfo> {node.Entry};
            result.AddRange(node.Children.SelectMany(x => OrderNode(x)));
            return result;
        }

        private void SetColumns(DbConnection connection, TableInfo table)
        {
            var showColumnsCommand = _factory.CreateShowColumnsCommand(table.Name);
            showColumnsCommand.Connection = connection;
            table.Columns = showColumnsCommand.ExecuteList().Select(x => Convert.ToString(x[0])).ToArray();
        }
    }
}
