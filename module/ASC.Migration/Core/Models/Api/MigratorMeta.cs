using System;
using System.Reflection;

namespace ASC.Migration.Core.Models.Api
{
    public class MigratorMeta
    {
        public Type MigratorType { get; private set; }

        public ApiMigratorAttribute MigratorInfo { get => MigratorType.GetCustomAttribute<ApiMigratorAttribute>(); }

        public MigratorMeta(Type type)
        {
            MigratorType = type;
        }
    }
}
