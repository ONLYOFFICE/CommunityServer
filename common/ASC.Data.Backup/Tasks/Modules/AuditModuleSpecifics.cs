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
using System.Linq;
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class AuditModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("audit_events", "tenant_id", "id") {UserIDColumns = new[] {"user_id"}},
                new TableInfo("login_events", "tenant_id", "id") {UserIDColumns = new[] {"user_id"}}
            };

        public override string ConnectionStringName
        {
            get { return "core"; }
        }

        public override ModuleName ModuleName
        {
            get { return ModuleName.Audit; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return Enumerable.Empty<RelationInfo>(); }
        }
    }
}
