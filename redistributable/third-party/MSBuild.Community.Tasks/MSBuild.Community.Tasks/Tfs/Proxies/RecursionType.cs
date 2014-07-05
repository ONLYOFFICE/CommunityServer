
using System;
using System.Reflection;

namespace MSBuild.Community.Tasks.Tfs.Proxies
{
    internal class RecursionType
    {
        Assembly _assembly;
        Type _type;
        object _instance;

        public RecursionType(Assembly versionControlClientAssembly) : this(versionControlClientAssembly, null) {}
        public RecursionType(Assembly versionControlClientAssembly, object instance)
        {
            _assembly = versionControlClientAssembly;
            _type = _assembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.RecursionType");
            _instance = instance;
        }

        public Type Type { get { return _type; } }
        public object Instance { get { return _instance; } }

        public RecursionType Full
        {
            get
            {
                object fullInstance = _type.GetField("Full").GetValue(null);
                return new RecursionType(_assembly, fullInstance);
            }
        }
    }
}
