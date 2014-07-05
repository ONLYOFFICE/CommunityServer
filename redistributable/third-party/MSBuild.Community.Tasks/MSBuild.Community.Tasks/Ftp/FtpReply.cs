

using System;

namespace MSBuild.Community.Tasks.Ftp
{
    /// <summary>
    /// Represenatation of a FTP reply message.
    /// </summary>
    public class FtpReply
    {
        /// <summary>
        /// The result code of the FTP response.
        /// </summary>
        private int _resultCode;

        /// <summary>
        /// The response message.
        /// </summary>
        private String _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpReply"/> class.
        /// </summary>
        /// <param name="resultCode">The result code of the FTP response.</param>
        /// <param name="message">The response message.</param>
        public FtpReply( int resultCode, string message )
        {
            _resultCode = resultCode;
            _message = message;
        }

        /// <summary>
        /// Gets or sets the result code.
        /// </summary>
        /// <value>The result code.</value>
        public int ResultCode
        {
            get
            {
                return _resultCode;
            }
            set
            {
                _resultCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
    }
}