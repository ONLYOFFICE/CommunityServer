using System;
using System.IO;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks.Ftp
{
    /// <summary>
    /// Uploads a full directory content to a remote directory.
    /// </summary>
    /// <example>Uploads directory content, including all subdirectories and subdirectory content:
    /// <code><![CDATA[
    /// <Target Name="DeployWebsite">
    ///     <FtpUploadDirectoryContent 
    ///         ServerHost="ftp.myserver.com"
    ///         Port="42"
    ///         Username="user"
    ///         Password="p@ssw0rd"
    ///         LocalDirectory="c:\build\mywebsite"
    ///         RemoteDirectory="root\www\mywebsite"
    ///         Recursive="true"
    ///     />
    /// ]]></code>
    /// To go a little step further. If the local directory looked like this:
    /// <code>
    /// [mywebsite]
    ///     [images]
    ///         1.gif
    ///         2.gif
    ///         3.gif
    ///     [js]
    ///         clientscript.js
    ///         nofocus.js
    ///     [css]
    ///         print.css
    ///         main.css
    ///     index.htm
    ///     contact.htm
    ///     downloads.htm
    /// </code>
    /// All directories and there content will be uploaded and a excact copy of the content of <c>mywebsite</c> directory will be created remotely.
    /// <remarks>
    /// If <see cref="Recursive"/> is set the <c>false</c>; only index.htm, contact.htm and downloads.htm will be uploaded and no subdirectories will be created remotely.
    /// </remarks>
    /// </example>
    public class FtpUploadDirectoryContent : FtpClientTaskBase
    {
        private String _localDirectory;
        private String _remoteDirectory;
        private String _filesTransferType;
        private bool _recursive;

        /// <summary>
        /// Gets or sets the local directory that contains the content to upload.
        /// </summary>
        /// <value>The local directory.</value>
        public String LocalDirectory
        {
            get
            {
                return _localDirectory;
            }
            set
            {
                _localDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets the remote directory destination for the local files.
        /// </summary>
        /// <value>The remote directory.</value>
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
        /// Gets or sets the type of files to be transferred. Accepts either 'BINARY' or 'ASCII' value.
        /// </summary>
        /// <value>Files transfer type.</value>
        public string FilesTransferType
        {
            get
            {
                return _filesTransferType;
            }
            set
            {
                _filesTransferType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the subdirectories of the local directory should be created remotely and the content of these should also be uploaded.
        /// </summary>
        /// <value><c>true</c> if recursive; otherwise, <c>false</c>.</value>
        public bool Recursive
        {
            get
            {
                return _recursive;
            }
            set
            {
                _recursive = value;
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
                    Log.LogMessage( MessageImportance.Low, "Connected to remote host." );
                }
                catch(Exception caught)
                {
                    Log.LogErrorFromException( caught, false );
                    Log.LogError( "Couldn't connect remote machine." );
                    return false;
                }

                try
                {
                    Login();
                    Log.LogMessage(MessageImportance.Low, "Login succeed.");
                }
                catch (Exception caught)
                {
                    Log.LogErrorFromException(caught, false);
                    Log.LogError("Couldn't login.");
                    return false;
                }

                try
                {
                    if (!String.IsNullOrEmpty(FilesTransferType))
                    {
                        SetFileTransferType(FilesTransferType);
                        Log.LogMessage(MessageImportance.Low, String.Format("File transfer mode set to '{0}'.",FilesTransferType));
                    }
                }
                catch (Exception caught)
                {
                    Log.LogErrorFromException(caught, false);
                    Log.LogError("Couldn't set file transfer mode.");
                    return false;
                }

                try
                {
                    if(!DirectoryExists( RemoteDirectory ))
                    {
                        Log.LogError( "Remote directory doesn't exist." );
                        return false;
                    }

                    ChangeWorkingDirectory( RemoteDirectory );
                }
                catch(FtpException caught)
                {
                    Log.LogErrorFromException( caught, false );
                    Log.LogError( "Couldn't change remote working directory." );
                    return false;
                }

                try
                {
                    UploadDirectory( LocalDirectory, "*.*", Recursive );
                }
                catch(FtpException caught)
                {
                    Log.LogErrorFromException( caught, false );
                    Log.LogError( "Couldn't upload directory." );
                    return false;
                }
            }
            finally
            {
                Close();
            }

            return true;
        }

        /// <summary>
        /// Upload a directory and its file contents.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="recurse">if set to <c>true</c> all subdurectiries will be included.</param>
        protected void UploadDirectory( String localPath, Boolean recurse )
        {
            UploadDirectory( localPath, "*.*", recurse );
        }

        /// <summary>
        /// Upload a directory and its file contents.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="mask">Only upload files that compli to the mask.</param>
        /// <param name="recursive">if set to <c>true</c> all subdurectiries will be included.</param>
        protected void UploadDirectory( String localPath, String mask, Boolean recursive )
        {
            Log.LogMessage( MessageImportance.Low, "Uploading files of local directory {0}.", localPath );


            foreach(string file in Directory.GetFiles( localPath, mask ))
            {
                String filename = Path.GetFileName( file );
                Store( file, filename );

                Log.LogMessage( MessageImportance.Low, "{0} uploaded succesfully.", localPath );
            }

            if(recursive)
            {
                foreach(String directory in Directory.GetDirectories( localPath ))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo( directory );
                    bool existsRemotely = DirectoryExists( directoryInfo.Name );

                    if(!existsRemotely)
                    {
                        MakeDirectory( directoryInfo.Name );
                    }

                    ChangeWorkingDirectory( directoryInfo.Name );

                    UploadDirectory( directory, mask, Recursive );

                    CdUp();
                }
            }
        }
    }
}