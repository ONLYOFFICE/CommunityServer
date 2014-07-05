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
using System.IO;
using System.Linq;
using System.Text;
using ASC.Common.Data;
using ASC.Data.Backup.Exceptions;
using ASC.Data.Backup.Extensions;
using ASC.Data.Backup.Tasks.Data;
using ASC.Data.Backup.Tasks.Modules;

namespace ASC.Data.Backup.Tasks
{
    internal class RestoreDbModuleTask : ProgressTask
    {
        private const int TransactionLength = 10000;

        private static readonly List<string> IgnoredTables = new List<string>(); 

        private readonly IDataReadOperator _reader;
        private readonly IModuleSpecifics _module;
        private readonly ColumnMapper _columnMapper;
        private readonly DbFactory _factory;
        private readonly bool _replaceDate;

        public RestoreDbModuleTask(IModuleSpecifics module, IDataReadOperator reader, ColumnMapper columnMapper, DbFactory factory, bool replaceDate)
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
        }

        public void IgnoreTable(string tableName)
        {
            if (!IgnoredTables.Contains(tableName))
                IgnoredTables.Add(tableName);
        }

        public override void Run()
        {
            InvokeInfo("begin restore data for module {0}", _module.ModuleName);
            InitProgress(_module.Tables.Count(t => !IgnoredTables.Contains(t.Name)));
            using (var connection = _factory.OpenConnection(_module.ConnectionStringName))
            {
                foreach (var table in _module.GetTablesOrdered().Where(t => !IgnoredTables.Contains(t.Name) && t.InsertMethod != InsertMethod.None))
                {
                    InvokeInfo("begin restore table {0}", table.Name);

                    var transactionsCommited = 0;
                    var rowsInserted = 0;
                    ActionInvoker.Try(state => RestoreTable(connection.Fix(), (TableInfo)state, ref transactionsCommited, ref rowsInserted), table, 5,
                                      onAttemptFailure: error => _columnMapper.Rollback(),
                                      onFailure: error => { throw ThrowHelper.CantRestoreTable(table.Name, error); });

                    SetStepCompleted();
                    InvokeInfo("{0} rows inserted for table {1}", rowsInserted, table.Name);
                }
            }
            InvokeInfo("end restore data for module {0}", _module.ModuleName);
        }

        private void RestoreTable(IDbConnection connection, TableInfo tableInfo, ref int transactionsCommited, ref int rowsInserted)
        {
            using (var stream = _reader.GetEntry(KeyHelper.GetTableZipKey(_module, tableInfo.Name)))
            {
                var lowImportanceRelations = _module
                    .TableRelations
                    .Where(r => string.Equals(r.ParentTable, tableInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    .Where(r => r.Importance == RelationImportance.Low && !r.IsSelfRelation())
                    .Select(r => Tuple.Create(r, _module.Tables.Single(t => t.Name == r.ChildTable)))
                    .ToList();

                foreach (IEnumerable<DataRowInfo> rows in GetRows(tableInfo, stream).Skip(transactionsCommited * TransactionLength).MakeParts(TransactionLength))
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
                                        newIdValue =  Guid.NewGuid().ToString("D");
                                    }
                                    else if (tableInfo.IdType == IdType.Integer)
                                    {
                                        newIdValue = connection
                                                         .CreateCommand(string.Format("select max({0}) from {1};", tableInfo.IdColumn, tableInfo.Name))
                                                         .WithTimeout(120)
                                                         .ExecuteScalar<int>() + 1;
                                    }
                                }
                                if (newIdValue != null)
                                {
                                    _columnMapper.SetMapping(tableInfo.Name, tableInfo.IdColumn, oldIdValue, newIdValue);
                                }
                            }

                            var insertCommand = _module.CreateInsertCommand(connection, _columnMapper, tableInfo, row);
                            if (insertCommand == null)
                            {
                                WarnCantInsertRow(row);
                                _columnMapper.Rollback();
                                continue;
                            }
                            insertCommand.WithTimeout(120).ExecuteNonQuery();
                            rowsSuccess++;

                            if (tableInfo.HasIdColumn() && tableInfo.IdType == IdType.Autoincrement)
                            {
                                var lastIdCommand = _factory.CreateLastInsertIdCommand(_module.ConnectionStringName);
                                lastIdCommand.Connection = connection;
                                newIdValue = Convert.ToInt32(lastIdCommand.ExecuteScalar());
                                _columnMapper.SetMapping(tableInfo.Name, tableInfo.IdColumn, oldIdValue, newIdValue);
                            }

                            _columnMapper.Commit();

                            foreach (Tuple<RelationInfo, TableInfo> relation in lowImportanceRelations)
                            {
                                if (!relation.Item2.HasTenantColumn())
                                {
                                    InvokeWarning("Table {0} does not contain tenant id column. Can't apply low importance relations on such tables.", relation.Item2.Name);
                                    continue;
                                }

                                object oldValue = row[relation.Item1.ParentColumn];
                                object newValue = _columnMapper.GetMapping(relation.Item1.ParentTable, relation.Item1.ParentColumn, oldValue);

                                connection.CreateCommand(string.Format("update {0} set {1} = {2} where {1} = {3} and {4} = {5}",
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

        private void WarnCantInsertRow(DataRowInfo row)
        {
            InvokeWarning(string.Format("Can't create command to insert row with values [{0}]", row));
        }
    }
}
