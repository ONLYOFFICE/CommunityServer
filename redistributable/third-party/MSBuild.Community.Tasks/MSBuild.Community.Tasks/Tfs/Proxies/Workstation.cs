
using System;
using System.Reflection;

namespace MSBuild.Community.Tasks.Tfs.Proxies
{
    internal class Workstation
    {
        Assembly _assembly;
        Type _type;
        object _instance;

        public Workstation(Assembly versionControlClientAssembly)
        {
            _assembly = versionControlClientAssembly;
            _type = _assembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.Workstation");
            _instance = createInstance();
        }

        private object createInstance()
        {
            PropertyInfo currentProperty = _type.GetProperty("Current");
            object currentWorkstation = currentProperty.GetValue(null, null);
            return currentWorkstation;
        }

        internal WorkspaceInfo GetLocalWorkspaceInfo(string localPath)
        {
            MethodInfo getLocalWorkspaceInfoMethod = _type.GetMethod("GetLocalWorkspaceInfo", new Type[] { typeof(string) });
            object workspaceInfoInstance = getLocalWorkspaceInfoMethod.Invoke(_instance, new object[] { localPath });
            if (workspaceInfoInstance == null)
            {
                throw new TeamFoundationServerException(String.Format("The local path {0} is not associated with a TFS Workspace.", localPath));
            }
            WorkspaceInfo workspaceInfo = new WorkspaceInfo(_assembly, workspaceInfoInstance);
            return workspaceInfo;
        }
    }
}
