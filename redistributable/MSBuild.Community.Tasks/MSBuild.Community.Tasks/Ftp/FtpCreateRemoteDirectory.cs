

using System;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks.Ftp
{
    /// <summary>
    /// Creates a full remote directory on the remote server if not exists using the File Transfer Protocol (FTP).
    /// This can be one directory or a full path to create.
    /// </summary>
    /// <example>Create remote directory:
    /// <code><![CDATA[
    /// <FtpCreateRemoteDirectoty 
    ///     ServerHost="ftp.myserver.com"
    ///     Port="42"
    ///     RemoteDirectory="Directory\Subdirectory\MyOtherSubdirectory"
    ///     Username="user"
    ///     Password="p@ssw0rd"
    /// />
    /// ]]></code>
    /// </example>
    /// <remarks>The full remote directory path will be created. All directories that doesn't exists on the remote server will be created.</remarks>
    public class FtpCreateRemoteDirectory : FtpClientTaskBase
    {
        /// <summary>
        /// The remote directory to create.
        /// </summary>
        private String _remoteDirectory;

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
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
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
                catch(Exception caught)
                {
                    Log.LogErrorFromException( caught, false );
                    Log.LogError( "Could not login. {0}", caught.Message );
                    return false;
                }

                // Get all directories in specified remote directory path.
                String[] directoryNames = RemoteDirectory.Split( '/', '\\' );

                foreach(String directoryName in directoryNames)
                {
                    bool directoryExistsRemotely = false;

                    try
                    {
                        // Determ is directory exists on the remote server.
                        directoryExistsRemotely = DirectoryExists( directoryName );
                    }
                    catch(FtpException caught)
                    {
                        Log.LogWarningFromException( caught );
                    }

                    if(!directoryExistsRemotely)
                    {
                        // Log debug message.
                        Log.LogMessage( MessageImportance.Low, "Directory {0} doesn't exist and will be created.",
                                       directoryName );

                        try
                        {
                            // Create directory.
                            MakeDirectory( directoryName );
                        }
                        catch(Exception caught)
                        {
                            Log.LogErrorFromException( caught );
                            Log.LogError( "Directory {0} couldn't be created remotely. {1}", directoryName,
                                         caught.Message );
                            return false;
                        }

                        // Log debug message.
                        Log.LogMessage( MessageImportance.Low, "Directory {0} created remotely.", directoryName );
                    }
                    else
                    {
                        // Log debug message.
                        Log.LogMessage( MessageImportance.Low, "Directory {0} exists and will not be created." );
                    }

                    try
                    {
                        // Change working directory to current directory.
                        ChangeWorkingDirectory( directoryName );
                    }
                    catch(FtpException caught)
                    {
                        Log.LogErrorFromException( caught );
                        Log.LogError( "Couldn't change working directory on remote server to {0}. {1}", directoryName,
                                     caught.Message );
                        return false;
                    }
                }
            }
            finally
            {
                Close();
            }

            Log.LogMessage( "Full directory path created remotely." );
            return true;
        }
    }
}
