
using System;
using System.Reflection;

namespace MSBuild.Community.Tasks.Tfs.Proxies
{
    internal class WorkspaceInfo
    {
        Assembly _assembly;
        Type _type;
        object _instance;

        public WorkspaceInfo(Assembly versionControlClientAssembly, object instance)
        {
            _assembly = versionControlClientAssembly;
            _type = _assembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.WorkspaceInfo");
            _instance = instance;
        }

        public Type Type { get { return _type; } }

        public object Instance { get { return _instance; } }

        public Uri ServerUri
        {
            get
            {
                PropertyInfo serverUriProperty = _type.GetProperty("ServerUri");
                object serverUri = serverUriProperty.GetValue(_instance, null);
                return (Uri)serverUri;
            }
        }
    }
}
