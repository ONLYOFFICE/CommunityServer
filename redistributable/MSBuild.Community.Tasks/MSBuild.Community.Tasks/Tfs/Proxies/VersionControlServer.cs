
using System;
using System.Reflection;
using System.Net;
using System.Collections;

namespace MSBuild.Community.Tasks.Tfs.Proxies
{
    internal class VersionControlServer
    {
        Assembly _assembly;
        Type _type;
        object _instance;

        public VersionControlServer(Assembly clientAssembly, Assembly versionControlClientAssembly, string server, ICredentials credentials)
        {
            _assembly = versionControlClientAssembly;
            _type = _assembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer");
            _instance = createInstance(clientAssembly, server, credentials);
        }

        private object createInstance(Assembly clientAssembly, string server, ICredentials credentials)
        {
            Type teamFoundationServerType = clientAssembly.GetType("Microsoft.TeamFoundation.Client.TeamFoundationServer");
            ConstructorInfo teamFoundationServerConstructor = teamFoundationServerType.GetConstructor(new Type[] { typeof(string), typeof(ICredentials) });
            object teamFoundationServer = teamFoundationServerConstructor.Invoke(new object[] { server, credentials });

            MethodInfo getServiceMethod = teamFoundationServerType.GetMethod("GetService", new Type[] { typeof(Type) });
            object versionControlServer = getServiceMethod.Invoke(teamFoundationServer, new object[] { _type });
            return versionControlServer;
        }

        public Workspace GetWorkspace(string localPath)
        {
            Type itemNotMappedException = _assembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.ItemNotMappedException");
            MethodInfo getWorkspaceMethod = _type.GetMethod("GetWorkspace", new Type[] { typeof(string) });
            object workspace = null;
            try
            {
                workspace = getWorkspaceMethod.Invoke(_instance, new object[] { localPath });
            }
            catch (Exception e)
            {
                Exception actualException;
                if (e is TargetInvocationException)
                {
                    actualException = e.InnerException;
                }
                else
                {
                    actualException = e;
                }

                if (actualException != null)
                {
                    if (actualException.GetType() == itemNotMappedException)
                    {
                        throw new TeamFoundationServerException(actualException.Message);
                    }
                }
                throw;
            }
            return new Workspace(_assembly, workspace);
        }

        public IEnumerable QueryHistory(string localPath, VersionSpec version, RecursionType recursion, WorkspaceVersionSpec toVersion)
        {
            Type[] parameterTypes = new Type[] {
                        typeof(string), version.Type, typeof(int), recursion.Type, typeof(string), toVersion.Type, toVersion.Type, typeof(int), typeof(bool), typeof(bool) 
                    };

            MethodInfo queryHistoryMethod = _type.GetMethod("QueryHistory", parameterTypes);

            IEnumerable history = (IEnumerable)queryHistoryMethod.Invoke(_instance, 
                new object[] {
                    localPath, version.Instance, 0, recursion.Instance, null, null, toVersion.Instance, int.MaxValue, false, false
                });
            return history;
        }
    }
}
