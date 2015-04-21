
using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Net;

namespace MSBuild.Community.Tasks.Tfs
{
    /// <summary>
    /// Determines the changeset in a local Team Foundation Server workspace
    /// </summary>
    public class TfsVersion : Task
    {
        private string _localPath;

        private string _username;

        /// <summary>
        /// The user to authenticate on the server
        /// </summary>
        /// <remarks>Leave empty to use the credentials of the current user.</remarks>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password;

        /// <summary>
        /// The password for the user to authenticate on the server
        /// </summary>
        /// <remarks>Leave empty to use the credentials of the current user.</remarks>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _domain;

        /// <summary>
        /// The domain of the user to authenticate on the server
        /// </summary>
        /// <remarks>Leave empty to use the credentials of the current user.</remarks>
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }


        /// <summary>Path to local working copy.</summary>
        [Required]
        public string LocalPath
        {
            get { return _localPath; }
            set { _localPath = value; }
        }

        private int _changeset;
        /// <summary>
        /// The latest changeset ID in the local path
        /// </summary>
        [Output]
        public int Changeset
        {
            get { return _changeset; }
        }

        private string _tfsLibraryLocation;

        /// <summary>
        /// The location of the Team Foundation Server client assemblies. Leave empty when the client is installed in the Global Assembly Cache.
        /// </summary>
        public string TfsLibraryLocation
        {
            get { return _tfsLibraryLocation; }
            set { _tfsLibraryLocation = value; }
        }


        /// <summary>
        /// Runs the exectuable file with the specified task parameters.
        /// </summary>
        /// <returns>
        /// true if the task runs successfully; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            IServer tfsServer = CreateServer();
            ICredentials credentials = BuildCredentials();
            try
            {
                _changeset = tfsServer.GetLatestChangesetId(_localPath, credentials);
                Log.LogMessage("Latest changset in {0} is {1}", _localPath, _changeset);
            }
            catch (TeamFoundationServerException tfsException)
            {
                Log.LogErrorFromException(tfsException, false);
                return false;
            }
            return true;
        }

        private IServer CreateServer()
        {
            return new TeamFoundationServer(_tfsLibraryLocation);
        }

        private ICredentials BuildCredentials()
        {
            ICredentials networkCredential = null;
            if (_username != null && _password != null)
            {
                if (_domain != null)
                    networkCredential = new NetworkCredential(_username, _password, _domain);
                else
                    networkCredential = new NetworkCredential(_username, _password);
            }
            else
            {
                networkCredential = CredentialCache.DefaultNetworkCredentials;
            }
            return networkCredential;
        }
    }
}
