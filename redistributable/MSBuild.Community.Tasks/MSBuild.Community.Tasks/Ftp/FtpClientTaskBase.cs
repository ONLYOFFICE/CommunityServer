#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;


using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuild.Community.Tasks.Properties;

namespace MSBuild.Community.Tasks.Ftp
{
    /// <summary>
    /// Ftp client base class.
    /// </summary>
    public abstract class FtpClientTaskBase : Task
    {
        /// <summary>
        /// The socket that will connect to the FTP server.
        /// </summary>
        private Socket _clientSocket;

        /// <summary>
        /// The size of the data buffer.
        /// </summary>
        private int _bufferSize;

        /// <summary>
        /// The last recieved FTP response over the client socket.
        /// </summary>
        private FtpReply _lastReply;

        private bool _logFtpMessageConversation;

        /// <summary>
        /// The password to use to login.
        /// </summary>
        private String _password;

        /// <summary>
        /// The port number of the FTP server.
        /// </summary>
        private int _port;

        /// <summary>
        /// The hostname of the FTP server.
        /// </summary>
        private String _serverhost;

        /// <summary>
        /// The username to use to login.
        /// </summary>
        private String _username;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientTaskBase"/> class.
        /// </summary>
        public FtpClientTaskBase()
        {
            ServerHost = "localhost";
            Port = 21;
            LogFtpMessageConversation = false;
            BufferSize = 8196;
        }

        /// <summary>
        /// Gets or sets the server host.
        /// </summary>
        /// <value>The server host.</value>
        /// <exception cref="ArgumentOutOfRangeException">The lenght of the given value is greater then 126 characters.</exception>
        [Required]
        public String ServerHost
        {
            get
            {
                return _serverhost;
            }
            set
            {
                if(value.Length > 126)
                {
                    throw new ArgumentOutOfRangeException( "ServerHost", value,
                                                          "The lenght of the server host value cannot be greater then 126 characters." );
                }

                _serverhost = value;
            }
        }

        /// <summary>
        /// Gets or sets the port number.
        /// </summary>
        /// <value>The port numer.</value>
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        /// <summary>
        /// Gets or sets the client socket.
        /// </summary>
        /// <value>The client socket.</value>
        protected Socket ClientSocket
        {
            get
            {
                return _clientSocket;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="FtpClientTaskBase"/> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        protected bool Connected
        {
            get
            {
                return ClientSocket != null && ClientSocket.Connected;
            }
        }

        /// <summary>
        /// Gets the encoding to use with communication with the server.
        /// </summary>
        /// <value>The encoding.</value>
        private static Encoding ConversationEncoding
        {
            get
            {
                return Encoding.ASCII;
            }
        }

        /// <summary>
        /// Gets the last recieved FTP response over the client socket.
        /// </summary>
        /// <value>The last recieved FTP response over the client socket.</value>
        public FtpReply LastReply
        {
            get
            {
                return _lastReply;
            }
            set
            {
                _lastReply = value;
            }
        }

        /// <summary>
        /// Gets or sets the username to login.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }

        /// <summary>
        /// Gets or sets the password to login.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the FTP message conversation should be logged.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the FTP message conversation should be logged; otherwise, <c>false</c>.
        /// </value>
        private bool LogFtpMessageConversation
        {
            get
            {
                return _logFtpMessageConversation;
            }
            set
            {
                _logFtpMessageConversation = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the data buffer.
        /// </summary>
        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }
            set
            {
                _bufferSize = value;
            }
        }

        /// <summary>
        /// Connects this FTP server socket.
        /// </summary>
        /// <exception cref="FtpException">Thrown when unable to connect.</exception>
        public void Connect()
        {
            // Make sure the client is not connected.
            if(Connected)
            {
                Close();
            }

            // Create socket to server.
            IPAddress serverAddress = null;
            _clientSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

            try
            {
                // Resolve server address from server host to connect.
                IPAddress[] serverAddresses = Dns.GetHostAddresses( ServerHost );
                serverAddress = serverAddresses[0];
            }
            catch(SocketException caught)
            {
                String message = String.Format( Resources.CouldNotResolveServerHostName, ServerHost );
                CloseAndTrowException( new FtpException( message, caught ) );
            }

            try
            {
                // Connect to endpoint.
                _clientSocket.Connect( serverAddress, Port );
            }
            catch(SecurityException caught)
            {
                CloseAndTrowException( new FtpException( Resources.CouldNotConnectToRemoteServer, caught ) );
            }
            catch(SocketException caught)
            {
                CloseAndTrowException( new FtpException( Resources.CouldNotConnectToRemoteServer, caught ) );
            }

            // Read first reply of the server.
            FtpReply reply = ReadResponse();

            if(reply.ResultCode != 220)
            {
                // No OK message received, throw exception because this indicates an error.
                CloseAndTrowException( new FtpException( reply.Message ) );
            }
        }

        /// <summary>
        /// Login to the FTP server with the specified credentials.
        /// </summary>
        public void Login()
        {
            // Send user command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "USER", Username );

            // If 'login ok, password needed' result is received.
            if(reply.ResultCode == 331)
            {
                // Send password and read reply.
                reply = SendCommandAndReadResponse( "PASS", Password );
            }

            // If not 'login ok' nor 'not implemented' is received, throw exception.
            if(reply.ResultCode != 230 && reply.ResultCode != 202)
            {
                CloseAndTrowException( new FtpException( reply.Message ) );
            }
        }

        /// <summary>
        /// Sets the type of file to be transferred.
        /// </summary>
        /// <param name="mode">File transfer type: BINARY or ASCII</param>
        public void SetFileTransferType( String mode )
        {
            if (!mode.Equals("binary", StringComparison.InvariantCultureIgnoreCase)
                && !mode.Equals("ascii", StringComparison.InvariantCultureIgnoreCase))
            {
                CloseAndTrowException(new FtpException("Set file transfer type accepts only following values: 'BINARY' or 'ASCII'."));
            }

            String rawCommand = mode.Equals("binary", StringComparison.InvariantCultureIgnoreCase)
                ? "TYPE I"
                : "TYPE A";

            // Send user command and read reply.
            FtpReply reply = SendCommandAndReadResponse(rawCommand);

            // If setting file transfer mode was not accepted.
            if (reply.ResultCode != 200)
            {
                CloseAndTrowException(new FtpException(reply.Message));
            }
        }

        /// <summary>
        /// Changes the working directory.
        /// </summary>
        /// <param name="remoteDirectory">The remote directory.</param>
        /// <exception cref="FtpException">Occurs if there where connection problems during the process or the FTP server doesn't support the CWD command. See the Message of the exception for details.</exception>
        /// <remarks>Sends the CWD command.</remarks>
        public void ChangeWorkingDirectory( String remoteDirectory )
        {
            // Send change working directory command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "CWD", remoteDirectory );

            // If no 'okay' reply received, throw exception.
            if(reply.ResultCode != 250)
            {
                throw new FtpException( reply.Message );
            }
        }

        /// <summary>
        /// Gets the working directory.
        /// </summary>
        /// <returns>The current working directory.</returns>
        public String GetWorkingDirectory()
        {
            FtpReply reply = SendCommandAndReadResponse( "PWD" );

            if(reply.ResultCode != 257)
            {
                throw new FtpException( reply.Message );
            }

            Match directoryMath = Regex.Match( reply.Message, "\"(?<directory>[^\"]+)\"" );
            if(!directoryMath.Success)
            {
                throw new FtpException( "Couldn't find directory in response from server." );
            }

            return directoryMath.Groups["directory"].Value;
        }

        /// <summary>
        /// Change to the parent of the current working directory.
        /// </summary>
        /// <exception cref="FtpException">Occurs if there where connection problems during the process or the FTP server doesn't support the CDUP command. See the Message of the exception for details.</exception>
        /// <remarks>Sends the CDUP command.</remarks>
        public void CdUp()
        {
            // Send change directory up command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "CDUP" );

            // If no 'okay' reply received, throw exception.
            if(reply.ResultCode != 250 && reply.ResultCode != 200)
            {
                throw new FtpException( reply.Message );
            }
        }

        /// <summary>
        /// Determs whether a remote file exists.
        /// </summary>
        /// <param name="remoteFile">The remote file.</param>
        /// <returns></returns>
        /// <exception cref="FtpException">Occurs if there where connection problems during the operation or if the FTP server doesn't support the SIZE command. See the Message of the exception for details.</exception>
        public bool FileExists( String remoteFile )
        {
            // Send size command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "SIZE ", remoteFile );

            // 213, is the filesize reply.
            if(reply.ResultCode == 213)
            {
                return true;
            }

            // 550, is the No such file reply.
            if(reply.ResultCode == 550)
            {
                return false;
            }

            throw new FtpException( reply.Message );
        }

        /// <summary>
        /// Determs whether a remote directory exists.
        /// </summary>
        /// <param name="remoteDirectory">The remote directory.</param>
        /// <remarks>
        /// This method is based on the succeedness of a CWD command, this can give wrong indication at a rare number of FTP server!
        /// </remarks>
        /// <exception cref="FtpException">Thrown if the opperation couldn't be executed.</exception>
        /// <returns><c>true</c> if the directory exists remotely; otherwise <c>false</c></returns>
        public bool DirectoryExists( String remoteDirectory )
        {
            bool exists = false;
            String workingDirectory = GetWorkingDirectory();


            // Send change working directory command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "CWD", remoteDirectory );

            // If 'okay'
            if(reply.ResultCode == 250)
            {
                exists = true;
                ChangeWorkingDirectory( workingDirectory );
            }

            return exists;
        }

        /// <summary>
        /// Removes a remote directory.
        /// </summary>
        /// <param name="directoryName">The remote directory name.</param>
        /// <exception cref="FtpException">Occurs if there where connection problems during the process or the FTP server doesn't support the RMD command. See the Message of the exception for details.</exception>
        /// <remarks>Sends the RMD command.</remarks>
        public void RemoveDirectory( String directoryName )
        {
            // Send remove directory command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "RMD", directoryName );

            // If not 'Pathname created', throw exception.
            if(reply.ResultCode != 257)
            {
                throw new FtpException( reply.Message );
            }
        }

        /// <summary>
        /// Creates a remote directory in the current working folder.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <exception cref="FtpException">Occurs if there where connection problems during the process or the FTP server doesn't support the MMD command. See the Message of the exception for details.</exception>
        public void MakeDirectory( String directoryName )
        {
            // Send make directory command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "MKD", directoryName );

            // If not 'Pathname created', throw exception.
            if(reply.ResultCode != 257)
            {
                throw new FtpException( reply.Message );
            }
        }

        /// <summary>
        /// Closes the connection to the server.
        /// </summary>
        public void Close()
        {
            // Only close if connected.
            if(Connected)
            {
                try
                {
                    // Close client socket.
                    ClientSocket.Close();
                }
                catch(Exception caught)
                {
                    Log.LogWarningFromException( caught );
                }
            }

            if(ClientSocket != null)
            {
                // Nullefy client socket.
                _clientSocket = null;
            }
        }

        /// <summary>
        /// Stores the specified localFile.
        /// </summary>
        /// <param name="localFile">The localfile.</param>
        /// <param name="remoteFileName">The remotefile.</param>
        public void Store( String localFile, String remoteFileName )
        {
            // Make sure the file exists.
            if(!File.Exists( localFile ))
            {
                throw new FileNotFoundException( "Couldn't find local file.", localFile );
            }

            // Create local file stream and data stream to remote server.
            using(FileStream localFileStream = File.OpenRead( localFile ))
            {
                // Send STOR command and create data stream.
                using(Stream remoteDataStream = CreateDataStreamAndSendCommand( "STOR " + remoteFileName ))
                {
                    // Create and buffer and read the first chunk of the local file.
                    byte[] buffer = new byte[BufferSize];
                    int bytesReaded = localFileStream.Read( buffer, 0, buffer.Length );

                    // While there a bytes readed from the local file.
                    while(bytesReaded > 0)
                    {
                        // Write readed bytes to the remote data stream.
                        remoteDataStream.Write( buffer, 0, bytesReaded );

                        if(bytesReaded == buffer.Length)
                        {
                            // Read next available bytes from the local file.
                            bytesReaded = localFileStream.Read( buffer, 0, buffer.Length );
                        }
                        else
                        {
                            // No more bytes available, so break.
                            break;
                        }
                    }
                }
            }

            // Read response from the server.
            FtpReply reply = ReadResponse();

            // If not 'Requested file action successful', throw exception.
            if(reply.ResultCode != 226)
            {
                throw new FtpException( reply.Message );
            }
        }

        /// <summary>
        /// Send a command to the FTP server.
        /// </summary>
        /// <param name="command">The command, for example PWD.</param>
        /// <param name="value">The value.</param>
        protected void SendCommand( String command, String value )
        {
            String rawCommand = String.Format( "{0} {1}", command, value );
            SendCommand( rawCommand );
        }

        /// <summary>
        /// Send a command to the FTP server.
        /// </summary>
        /// <param name="rawCommand">The full command to send.</param>
        protected void SendCommand( String rawCommand )
        {
            // Add \r\n to command if it doesn't contain it.
            if(!rawCommand.EndsWith( Environment.NewLine ))
            {
                rawCommand = String.Concat( rawCommand, Environment.NewLine );
            }

            // Get byte representation of the command and send it.
            Byte[] buffer = ConversationEncoding.GetBytes( rawCommand );
            ClientSocket.Send( buffer );

            if(LogFtpMessageConversation)
            {
                // Log conversation.
                Log.LogMessage( MessageImportance.Low, "FTP > {0}", rawCommand.Trim( '\n', '\r' ) );
            }
        }

        /// <summary>
        /// Send a command to the FTP server and returns the response.
        /// </summary>
        /// <param name="command">The command, for example PWD.</param>
        /// <param name="value">The value</param>
        /// <returns>The reply of the FTP server for this command.</returns>
        protected FtpReply SendCommandAndReadResponse( String command, String value )
        {
            String rawCommand = String.Format( "{0} {1}", command, value );
            return SendCommandAndReadResponse( rawCommand );
        }

        /// <summary>
        /// Send a command to the FTP server and returns the response.
        /// </summary>
        /// <param name="rawCommand">The raw command to send.</param>
        /// <returns>The reply of the FTP server for this command.</returns>
        protected FtpReply SendCommandAndReadResponse( String rawCommand )
        {
            SendCommand( rawCommand );
            return ReadResponse();
        }

        /// <summary>
        /// Get the full directory details of the current directory.
        /// </summary>
        /// <returns>A array that contains all the FTP files located in the currenct directory.</returns>
        public FtpEntry[] GetDirectoryDetails()
        {
            return GetDirectoryDetails( null );
        }

        /// <summary>
        /// Create a data stream and send a raw command.
        /// </summary>
        /// <param name="rawCommand">The raw command to send.</param>
        /// <returns>The data stream that was created.</returns>
        public Stream CreateDataStreamAndSendCommand( String rawCommand )
        {
            // Send PASV command and read reply.
            FtpReply reply = SendCommandAndReadResponse( "PASV" );

            // If not 'Entering PASV mode' response received;
            if(reply.ResultCode != 227)
            {
                // Throw exception.
                throw new FtpException( reply.Message );
            }

            // Get the ip end point from the response message.
            IPEndPoint endPoint = ParseDataEndPointFromMessage( reply.Message );

            // Create data socket and connect it.
            Socket dataSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            dataSocket.Connect( endPoint );

            // Send provided command and read reply.
            reply = SendCommandAndReadResponse( rawCommand );

            // If not in the 1xx range, throw exception.
            if (reply.ResultCode < 100 || reply.ResultCode > 199)
            {
                // throw exception.
                throw new FtpException( reply.Message );
            }

            // Return data stream.
            return new NetworkStream( dataSocket, true );
        }

        /// <summary>
        /// Parses the data IP end point from datarequest message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static IPEndPoint ParseDataEndPointFromMessage( String message )
        {
            String ipAddressAsString = String.Empty;

            int ipStartIndex = message.IndexOf( "(" ) + 1;
            int ipEndIndex = message.IndexOf( ")" );

            String ipAddressFromMessage = message.Substring( ipStartIndex, ipEndIndex - ipStartIndex );
            String[] ipAddressChunks = ipAddressFromMessage.Split( ',' );


            for(int i = 0; i < 3; i++)
            {
                ipAddressAsString += ipAddressChunks[i];
                ipAddressAsString += ".";
            }

            ipAddressAsString += ipAddressChunks[3];

            int port = Int32.Parse( ipAddressChunks[4] ) * 256 + Int32.Parse( ipAddressChunks[5] );
            IPAddress ipAddress = IPAddress.Parse( ipAddressAsString );
            return new IPEndPoint( ipAddress, port );
        }

        /// <summary>
        /// Get the full directory details of the current directory.
        /// </summary>
        /// <param name="remoteDirectory">The remove directory, emtpy or <c>null</c> will get the details of the current directory.</param>
        /// <returns>A array that contains all the FTP files located in the currenct directory.</returns>
        public FtpEntry[] GetDirectoryDetails( String remoteDirectory )
        {
            String listCommand = "LIST";

            if(!String.IsNullOrEmpty( remoteDirectory ))
            {
                listCommand += " " + remoteDirectory;
            }

            using(Stream stream = CreateDataStreamAndSendCommand( listCommand ))
            {
                byte[] buffer = new byte[8192];
                int bytesReceived = stream.Read( buffer, 0, buffer.Length );
                String dirEntryList = String.Empty;

                while(bytesReceived > 0)
                {
                    dirEntryList += ConversationEncoding.GetString( buffer, 0, bytesReceived );
                    bytesReceived = stream.Read( buffer, 0, buffer.Length );
                }

                String[] dirEntries = dirEntryList.Split( '\n' );
                return FtpEntry.ParseDirList( dirEntries );
            }
        }

        /// <summary>
        /// Reads the ftp response from the client socket.
        /// </summary>
        /// <returns>The response of the FTP server.</returns>
        protected FtpReply ReadResponse()
        {
            // Read next available reply string from the client socket and
            // get the result code piece of the reply.
            String responseString = ReadResponseString();
            String resultCodePiece = responseString.Substring( 0, 3 );

            // Parse result code and get message piece of the reply.
            Int32 resultCode = Int32.Parse( resultCodePiece );
            String message = responseString.Substring( 4 );

            // Create FTP reply and set last reply.
            FtpReply reply = new FtpReply( resultCode, message );
            _lastReply = reply;

            // Return result.
            return reply;
        }

        /// <summary>
        /// Reads the response string from the client socket.
        /// </summary>
        /// <returns>The response of the client socket.</returns>
        private String ReadResponseString()
        {
            Byte[] buffer = new Byte[8192];
            String responseString = String.Empty;

            int bytesReceived = ClientSocket.Receive( buffer, 0, buffer.Length, SocketFlags.None );

            while(bytesReceived > 0)
            {
                responseString += ConversationEncoding.GetString( buffer, 0, bytesReceived );

                if(bytesReceived == buffer.Length)
                {
                    bytesReceived = ClientSocket.Receive( buffer, 0, buffer.Length, SocketFlags.None );
                }
                else
                {
                    break;
                }
            }

            if(LogFtpMessageConversation)
            {
                Log.LogMessage( MessageImportance.Low, "FTP < {0}", responseString.Trim( '\n', '\r' ) );
            }

            return responseString;
        }

        /// <summary>
        /// Make sure the connections are closed and trow the specified exception.
        /// </summary>
        /// <param name="exception">The exception to throw.</param>
        private void CloseAndTrowException( Exception exception )
        {
            Close();
            throw exception;
        }
    }
}