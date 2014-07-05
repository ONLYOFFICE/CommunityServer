

using System;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks.Ftp
{
    /// <summary>
    /// Determ if a remote directory exists on a FTP server or not.
    /// </summary>
    /// <example>Determ of Directory\1 exists:
    /// <code><![CDATA[
    /// <Target Name="CheckIfDirectoryExists">
    ///     <FtpDirectoryExists 
    ///         ServerHost="ftp.myserver.com"
    ///         Port="42"
    ///         RemoteDirectory="1\2\3"
    ///         Username="user"
    ///         Password="p@ssw0rd"
    ///     >
    ///         <Output TaskParameter="Exists" PropertyName="Exists" /> 
    ///     </FtpDirectoryExists>
    ///     <Message Text="Directory '1\2\3' exists: $(Exists)"/>
    /// ]]></code>
    /// If the directory exists on the server you should see the following output in the console:
    /// <c>Directory '1\2\3' exists: true</c>
    /// </example>
    /// <remarks>The full remote directory path will be created. All directories that doesn't exists on the remote server will be created.</remarks>
    public class FtpDirectoryExists : FtpClientTaskBase
    {
        /// <summary>
        /// The remote directory to create.
        /// </summary>
        private String _remoteDirectory;

        /// <summary>
        /// Flag that indicates whether the directory exists on the server.
        /// </summary>
        private bool _exists;

        /// <summary>
        /// Gets or sets the remote directory to create.
        /// </summary>
        /// <value>The remote directory.</value>
        /// <example>This can be one directory name, like <c>"Directory"</c>, or a directory path, like <c>"Directory\Subdirectoy"</c>.
        /// </example>
        [Required]
        public string RemoteDirectory
        {
            get
            {
                return _remoteDirectory;
            }
            set
            {
                _remoteDirectory = value;
            }
        }

        /// <summary>
        /// Gets an indication whether the directory exists on the server.
        /// </summary>
        /// <value><c>true</c> when the directory exists on the server; otherwise <c>false</c>.</value>
        [Output]
        public bool Exists
        {
            get
            {
                return _exists;
            }
        }

        ///<summary>
        /// Executes the current task.
        ///</summary>
        ///<returns>
        ///true if the task successfully executed; otherwise, false.
        ///</returns>
        public override bool Execute()
        {
            try
            {
                try
                {
                    Connect();
                    Log.LogMessage( MessageImportance.Low, "Connected to remote server." );
                }
                catch(FtpException caught)
                {
                    Log.LogErrorFromException( caught, false );
                    Log.LogError( "Could not connect to remote server. {0}", caught.Message );
                    return false;
                }

                try
                {
                    Login();
                    Log.LogMessage( MessageImportance.Low, "Login succesfully." );
                }
                catch(FtpException caught)
                {
                    Log.LogErrorFromException( caught, false );
                    Log.LogError( "Could not login. {0}", caught.Message );
                    return false;
                }

                try
                {
                    _exists = DirectoryExists( RemoteDirectory );
                }
                catch(FtpException caught)
                {
                    Log.LogErrorFromException( caught, false );
                    Log.LogError( "Could determ whether the remote directory exists or not. {0}", caught.Message );
                    return false;
                }
            }
            finally
            {
                Close();
            }

            return true;
        }
    }
}
