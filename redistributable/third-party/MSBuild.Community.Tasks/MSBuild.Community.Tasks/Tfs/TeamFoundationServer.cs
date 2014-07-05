
using System;
using System.Net;
using System.Reflection;
using System.IO;
using System.Collections;
using MSBuild.Community.Tasks.Tfs.Proxies;

namespace MSBuild.Community.Tasks.Tfs
{
    /// <summary>
    /// Handles all communication with the Team Foundation Server
    /// </summary>
    internal class TeamFoundationServer : IServer
    {
        Assembly clientAssembly;
        Assembly versionControlClientAssembly;

        /// <summary>
        /// Creates an instace of the TeamFoundationServer class
        /// </summary>
        /// <param name="clientLocation">The local file path containing the TFS libraries. null if TFS is in the GAC.</param>
        public TeamFoundationServer(string clientLocation)
        {
            if (clientLocation == null)
            {
                try
                {
                    clientAssembly =
                        Assembly.Load(
                            "Microsoft.TeamFoundation.Client, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    versionControlClientAssembly =
                        Assembly.Load(
                            "Microsoft.TeamFoundation.VersionControl.Client, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                }
                catch {}

                if (clientAssembly == null)
                {
                    clientAssembly =
                        Assembly.Load(
                            "Microsoft.TeamFoundation.Client, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    versionControlClientAssembly =
                        Assembly.Load(
                            "Microsoft.TeamFoundation.VersionControl.Client, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                }
            }
            else
            {
                string clientFilePath = Path.Combine(clientLocation, "Microsoft.TeamFoundation.Client.dll");
                string versionControlClientFilePath = Path.Combine(clientLocation, "Microsoft.TeamFoundation.VersionControl.Client.dll");

                clientAssembly = Assembly.LoadFrom(clientFilePath);
                versionControlClientAssembly = Assembly.LoadFrom(versionControlClientFilePath);
            }
        }



        /// <summary>
        /// Retrieves the latest changeset ID associated with a path
        /// </summary>
        /// <param name="localPath">A path on the local filesystem</param>
        /// <param name="credentials">Credentials used to authenticate against the serer</param>
        /// <returns></returns>
        public int GetLatestChangesetId(string localPath, ICredentials credentials)
        {
            int latestChangesetId = 0;
            string server;

            Workstation workstation = new Workstation(versionControlClientAssembly);
            WorkspaceInfo workspaceInfo = workstation.GetLocalWorkspaceInfo(localPath);
            server = workspaceInfo.ServerUri.ToString();
            VersionControlServer sourceControl = new VersionControlServer(clientAssembly, versionControlClientAssembly, server, credentials);

            Workspace workspace = sourceControl.GetWorkspace(localPath);
            WorkspaceVersionSpec workspaceVersionSpec = new WorkspaceVersionSpec(versionControlClientAssembly, workspace);

            VersionSpec versionSpec = new VersionSpec(versionControlClientAssembly);
            RecursionType recursionType = new RecursionType(versionControlClientAssembly);

            IEnumerable history = sourceControl.QueryHistory(localPath, versionSpec.Latest, recursionType.Full, workspaceVersionSpec);

            IEnumerator historyEnumerator = history.GetEnumerator();
            Changeset latestChangeset = new Changeset(versionControlClientAssembly);
            if (historyEnumerator.MoveNext())
            {
                latestChangeset = new Changeset(versionControlClientAssembly, historyEnumerator.Current);
            }

            if (latestChangeset.Instance != null)
            {
                latestChangesetId = latestChangeset.ChangesetId;
            }
            return latestChangesetId;
        }

    }
}
