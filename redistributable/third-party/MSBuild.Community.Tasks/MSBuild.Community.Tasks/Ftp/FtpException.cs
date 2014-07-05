

using System;

namespace MSBuild.Community.Tasks.Ftp
{
    /// <summary>
    /// Exception returned by FTP server.
    /// </summary>
    public class FtpException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public FtpException( String message )
            : base( message )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public FtpException( String message, Exception innerException )
            : base( message, innerException )
        {

        }
    }
}
