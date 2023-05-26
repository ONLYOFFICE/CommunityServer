/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;
using System.Reflection;

using ASC.Migration.GoogleWorkspace;
using ASC.Migration.NextcloudWorkspace;
using ASC.Migration.OwnCloud;

namespace ASC.Migration.Core.Models.Api
{
    public static class MigrationCore
    {

        private static Dictionary<string, MigratorMeta> _migrators = null;
        private static Dictionary<string, MigratorMeta> Migrators
        {
            get
            {
                if (_migrators != null) return _migrators;

                _migrators = new Dictionary<string, MigratorMeta>(StringComparer.OrdinalIgnoreCase);

                var migratorTypes = new List<Type>()
                {
                    typeof(NextcloudWorkspaceMigration),
                    typeof(GoogleWorkspaceMigration),
                    typeof (OwnCloudMigration)
                };

                foreach (var type in migratorTypes)
                {
                    var attr = type.GetCustomAttribute<ApiMigratorAttribute>();
                    if (attr == null) continue;

                    _migrators.Add(attr.Name, new MigratorMeta(type));
                }

                return _migrators;
            }
        }

        public static string[] GetAvailableMigrations() => Migrators.Keys.ToArray();

        public static MigratorMeta GetMigrator(string migrator) => Migrators.TryGetValue(migrator, out var meta) ? meta : null;
    }
}
