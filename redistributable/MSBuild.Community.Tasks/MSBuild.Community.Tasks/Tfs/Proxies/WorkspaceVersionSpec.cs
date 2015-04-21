
using System;
using System.Reflection;

namespace MSBuild.Community.Tasks.Tfs.Proxies
{
    internal class WorkspaceVersionSpec
    {
        Assembly _assembly;
        Type _type;
        object _instance;

        public WorkspaceVersionSpec(Assembly versionControlClientAssembly, Workspace workspace)
        {
            _assembly = versionControlClientAssembly;
            _type = _assembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.WorkspaceVersionSpec");
            _instance = createInstance(workspace);
        }

        public Type Type { get { return _type; } }
        public object Instance { get { return _instance; } }

        private object createInstance(Workspace workspace)
        {
            ConstructorInfo workspaceVersionSpecTypeContstructor = _type.GetConstructor(new Type[] { workspace.Type });
            object workspaceVersionSpec = workspaceVersionSpecTypeContstructor.Invoke(new object[] { workspace.Instance });
            return workspaceVersionSpec;
        }

        public object Latest
        {
            get
            {
                PropertyInfo versionSpecTypeLatestProperty = _type.GetProperty("Latest");
                object latestVersionSpec = versionSpecTypeLatestProperty.GetValue(null, null);
                return latestVersionSpec;
            }
        }

    }
}
