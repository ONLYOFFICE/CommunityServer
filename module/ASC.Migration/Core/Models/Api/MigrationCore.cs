using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

                var migratorTypes = Assembly.GetExecutingAssembly()
                    .GetExportedTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface
                        && typeof(IMigration).IsAssignableFrom(t));

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
