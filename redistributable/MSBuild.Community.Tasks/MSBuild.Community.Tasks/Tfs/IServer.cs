
using System;
namespace MSBuild.Community.Tasks.Tfs
{
    /// <summary>
    /// Describes the behavior of a Team Foundation Server
    /// </summary>
    /// <exclude />
    public interface IServer
    {
        /// <summary>
        /// Retrieves the latest changeset ID associated with a path
        /// </summary>
        /// <param name="localPath">A path on the local filesystem</param>
        /// <param name="credentials">Credentials used to authenticate against the serer</param>
        /// <returns></returns>
        int GetLatestChangesetId(string localPath, System.Net.ICredentials credentials);
    }
}
