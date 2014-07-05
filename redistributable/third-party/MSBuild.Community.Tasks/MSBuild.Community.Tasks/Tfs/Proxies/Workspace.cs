
using System;
using System.Reflection;

namespace MSBuild.Community.Tasks.Tfs.Proxies
{
    internal class Workspace
    {
        Assembly _assembly;
        Type _type;
        object _instance;

        public Workspace(Assembly versionControlClientAssembly, object instance)
        {
            _assembly = versionControlClientAssembly;
            _type = _assembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.Workspace");
            _instance = instance;
        }

        public Type Type { get { return _type; } }

        public object Instance { get { return _instance; } }
    }
}
