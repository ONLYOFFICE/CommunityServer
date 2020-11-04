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


using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    public enum ModuleName
    {
        Audit,
        Calendar,
        Community,
        Core,
        Crm,
        Crm2,
        CrmInvoice,
        Files,
        Files2,
        Mail,
        Projects,
        Tenants,
        WebStudio
    }

    internal interface IModuleSpecifics
    {
        string ConnectionStringName { get; }
        ModuleName ModuleName { get; }

        IEnumerable<TableInfo> Tables { get; }
        IEnumerable<RelationInfo> TableRelations { get; }

        IEnumerable<TableInfo> GetTablesOrdered(); 
            
        DbCommand CreateSelectCommand(DbConnection connection, int tenantId, TableInfo table, int limit, int offset);
        DbCommand CreateDeleteCommand(DbConnection connection, int tenantId, TableInfo table);
        DbCommand CreateInsertCommand(bool dump, DbConnection connection, ColumnMapper columnMapper, TableInfo table, DataRowInfo row);

        bool TryAdjustFilePath(bool dump, ColumnMapper columnMapper, ref string filePath);

        void PrepareData(DataTable data);
        Stream PrepareData(string key, Stream data, ColumnMapper columnMapper);
    }
}
