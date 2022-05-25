using System;

namespace ASC.Migration.Core.Models.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ApiMigratorAttribute : Attribute
    {
        public string Name { get; private set; }
        public bool RequiresFolder { get; private set; }

        public string[] RequiredFileTypes { get; private set; }

        public ApiMigratorAttribute(string name, string[] fileTypes)
        {
            Name = name;
            RequiredFileTypes = fileTypes;
        }

        public ApiMigratorAttribute(string name)
        {
            Name = name;
        }
    }
}
