/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.Mail.Net.FTP.Client
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using IO;
    using TCP;
    using StringReader=StringReader;

    #endregion

    #region enum TransferType

    /// <summary>
    /// Transfer type.
    /// </summary>
    internal enum TransferType
    {
        /// <summary>
        /// ASCII transfer data.
        /// </summary>
        Ascii = 0,
        /// <summary>
        /// Binary transfer data. 
        /// </summary>
        Binary = 1,
    }

    #endregion

    /// <summary>
    /// This class implements FTP client. Defined in RFC 959.
    /// </summary>
    public class FTP_Client : TCP_Client
    {
        #region Nested type: DataConnection

        /// <summary>
        /// This class implements FTP client data connection.
        /// </summary>
        private class DataConnection : IDisposable
        {
            #region Members

            private int m_ActivePort = -1;
            private bool m_IsActive;
            private DateTime m_LastActivity;
            private FTP_Client m_pOwner;
            private Socket m_pSocket;
            private FTP_TransferMode m_TransferMode = FTP_TransferMode.Active;

            #endregion

            #region Properties

            /// <summary>
            /// Gets data connection local IP end point.
            /// </summary>
            public IPEndPoint LocalEndPoint
            {
                get { return (IPEndPoint) m_pSocket.LocalEndPoint; }
            }

            /// <summary>
            /// Gets last time when data connection has read or written data.
            /// </summary>
            public DateTime LastActivity
            {
                get { return m_LastActivity; }
            }

            /// <summary>
            /// Gets if there is active read or write job in data stream.
            /// </summary>
            public bool IsActive
            {
                get { return m_IsActive; }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="owner">Data connection owner FTP client.</param>
            public DataConnection(FTP_Client owner)
            {
                m_pOwner = owner;

                CreateSocket();
            }

            #endregion

            #region Methods

            /// <summary>
            /// Cleans up any resources being used.
            /// </summary>
            public void Dispose()
            {
                if (m_pSocket != null)
                {
                    m_pSocket.Close();
                    m_pSocket = null;
                }
                m_pOwner = null;
            }

            /// <summary>
            /// Swtiches FTP data connection to active mode.
            /// </summary>
            public void SwitchToActive()
            {
                // In acvtive mode we must start listening incoming FTP server connection.
                m_pSocket.Listen(1);

                m_TransferMode = FTP_TransferMode.Active;

                m_pOwner.LogAddText(
                    "FTP data channel switched to Active mode, listening FTP server connect to '" +
                    m_pSocket.LocalEndPoint + "'.");
            }

            /// <summary>
            /// Swtiches FTP data connection to passive mode and connects to the sepcified FTP server.
            /// </summary>
            /// <param name="remoteEP">FTP server IP end point.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
            public void SwitchToPassive(IPEndPoint remoteEP)
            {
                if (remoteEP == null)
                {
                    throw new ArgumentNullException("remoteEP");
                }

                m_pOwner.LogAddText("FTP data channel switched to Passive mode, connecting to FTP server '" +
                                    remoteEP + "'.");

                // In passive mode we just need to connect to the specified FTP host.
                m_pSocket.Connect(remoteEP);

                m_TransferMode = FTP_TransferMode.Passive;

                m_pOwner.LogAddText("FTP Passive data channel established, localEP='" +
                                    m_pSocket.LocalEndPoint + "' remoteEP='" + m_pSocket.RemoteEndPoint + "'.");
            }

            /// <summary>
            /// Reads all data from FTP data connection and stores to the specified stream.
            /// </summary>
            /// <param name="stream">Stream where to store data.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
            public void ReadAll(Stream stream)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }

                m_IsActive = true;
                try
                {
                    if (m_TransferMode == FTP_TransferMode.Active)
                    {
                        using (NetworkStream dataStream = WaitFtpServerToConnect(20))
                        {
                            long bytesReaded = TransferStream(dataStream, stream);
                            m_pOwner.LogAddRead(bytesReaded,
                                                "Data connection readed " + bytesReaded + " bytes.");
                        }
                    }
                    else if (m_TransferMode == FTP_TransferMode.Passive)
                    {
                        using (NetworkStream dataStream = new NetworkStream(m_pSocket, true))
                        {
                            long bytesReaded = TransferStream(dataStream, stream);
                            m_pOwner.LogAddRead(bytesReaded,
                                                "Data connection readed " + bytesReaded + " bytes.");
                        }
                    }
                }
                finally
                {
                    m_IsActive = false;
                    CleanUpSocket();
                }
            }

            /// <summary>
            /// Writes all data from the specified stream to FTP data connection.
            /// </summary>
            /// <param name="stream">Stream which data to write.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
            public void WriteAll(Stream stream)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }

                try
                {
                    if (m_TransferMode == FTP_TransferMode.Active)
                    {
                        using (NetworkStream dataStream = WaitFtpServerToConnect(20))
                        {
                            long bytesWritten = TransferStream(stream, dataStream);
                            m_pOwner.LogAddWrite(bytesWritten,
                                                 "Data connection wrote " + bytesWritten + " bytes.");
                        }
                    }
                    else if (m_TransferMode == FTP_TransferMode.Passive)
                    {
                        using (NetworkStream dataStream = new NetworkStream(m_pSocket, true))
                        {
                            long bytesWritten = TransferStream(stream, dataStream);
                            m_pOwner.LogAddWrite(bytesWritten,
                                                 "Data connection wrote " + bytesWritten + " bytes.");
                        }
                    }
                }
                finally
                {
                    m_IsActive = false;
                    CleanUpSocket();
                }
            }

            /// <summary>
            /// Cleans up socket for reuse.
            /// </summary>
            public void CleanUpSocket()
            {
                if (m_pSocket != null)
                {
                    m_pSocket.Close();
                }

                // We can't reuse socket, so we need to recreate new one for each transfer.
                CreateSocket();
            }

            #endregion

            #region Utility methods

            /// <summary>
            /// Waits FTP server to connect to this data connection.
            /// </summary>
            /// <param name="waitTime">Wait time out in seconds.</param>
            /// <returns>Returns connected network stream.</returns>
            private NetworkStream WaitFtpServerToConnect(int waitTime)
            {
                try
                {
                    m_pOwner.LogAddText("FTP Active data channel waiting FTP server connect to '" +
                                        m_pSocket.LocalEndPoint + "'.");

                    //--- Wait ftp server connection -----------------------------//
                    DateTime startTime = DateTime.Now;
                    while (!m_pSocket.Poll(0, SelectMode.SelectRead))
                    {
                        Thread.Sleep(50);

                        if (startTime.AddSeconds(waitTime) < DateTime.Now)
                        {
                            m_pOwner.LogAddText("FTP server didn't connect during expected time.");

                            throw new IOException("FTP server didn't connect during expected time.");
                        }
                    }
                    //-----------------------------------------------------------//

                    // Accpet FTP server connection.
                    Socket socket = m_pSocket.Accept();

                    m_pOwner.LogAddText("FTP Active data channel established, localEP='" +
                                        socket.LocalEndPoint + "' remoteEP='" + socket.RemoteEndPoint + "'.");

                    return new NetworkStream(socket, true);
                }
                finally
                {
                    CleanUpSocket();
                }
            }

            /// <summary>
            /// Creates new socket for data connection.
            /// </summary>
            private void CreateSocket()
            {
                // IPv4
                if (m_pOwner.LocalEndPoint.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    m_pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                    // IPv6
                else
                {
                    m_pSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }

                int port = 0;
                // Data connection port range not specified, let system to allocate port for us.
                if (m_pOwner.DataPortRange == null)
                {
                    port = 0;
                }
                    // Data connection port specified, use next port from range.
                else
                {
                    // There is no acitve port or we have reached end of range, just reset range.
                    if (m_ActivePort == -1 || (m_ActivePort + 1) > m_pOwner.DataPortRange.End)
                    {
                        m_ActivePort = m_pOwner.DataPortRange.Start;
                    }
                    else
                    {
                        m_ActivePort++;
                    }
                    port = m_ActivePort;
                }

                // Data connection IP address not specified, use default.
                if (m_pOwner.DataIP == null || m_pOwner.DataIP == IPAddress.Any)
                {
                    m_pSocket.Bind(new IPEndPoint(m_pOwner.LocalEndPoint.Address, port));
                }
                    // Data connection IP specified, use it.
                else
                {
                    m_pSocket.Bind(new IPEndPoint(m_pOwner.DataIP, port));
                }
                m_pSocket.SendTimeout = 30000;
                m_pSocket.ReceiveTimeout = 30000;
            }

            /// <summary>
            /// Copies all source stream data to the specified target stream.
            /// </summary>
            /// <param name="source">Source stream.</param>
            /// <param name="target">Target stream.</param>
            private long TransferStream(Stream source, Stream target)
            {
                long totalReadedCount = 0;
                byte[] buffer = new byte[Workaround.Definitions.MaxStreamLineLength];
                while (true)
                {
                    int readedCount = source.Read(buffer, 0, buffer.Length);
                    // End of stream reached, we readed all data sucessfully.
                    if (readedCount == 0)
                    {
                        return totalReadedCount;
                    }
                    else
                    {
                        target.Write(buffer, 0, readedCount);
                        totalReadedCount += readedCount;
                        m_LastActivity = DateTime.Now;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Members

        private string m_GreetingText = "";
        private GenericIdentity m_pAuthdUserIdentity;
        private DataConnection m_pDataConnection;
        private IPAddress m_pDataConnectionIP;
        private PortRange m_pDataPortRange;
        private List<string> m_pExtCapabilities;
        private FTP_TransferMode m_TransferMode = FTP_TransferMode.Passive;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets data connection establish mode.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public FTP_TransferMode TransferMode
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_TransferMode;
            }

            set
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                m_TransferMode = value;
            }
        }

        /// <summary>
        /// Gets or sets local IP address to use for data connection. Value null means that system will allocate it.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IPAddress DataIP
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pDataConnectionIP;
            }

            set
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                m_pDataConnectionIP = value;

                // If We are connected, we need to reset data connection.
                if (IsConnected)
                {
                    m_pDataConnection.CleanUpSocket();
                }
            }
        }

        /// <summary>
        /// Gets or sets ports what data connection may use. Value null means that system will allocate it.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public PortRange DataPortRange
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pDataPortRange;
            }

            set
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                m_pDataPortRange = value;

                // If We are connected, we need to reset data connection.
                if (IsConnected)
                {
                    m_pDataConnection.CleanUpSocket();
                }
            }
        }

        /// <summary>
        /// Gets greeting text which was sent by FTP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and FTP client is not connected.</exception>
        public string GreetingText
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_GreetingText;
            }
        }

        /// <summary>
        /// Gets FTP exteneded capabilities supported by FTP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and FTP client is not connected.</exception>
        public string[] ExtenededCapabilities
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pExtCapabilities.ToArray();
            }
        }

        /// <summary>
        /// Gets session authenticated user identity, returns null if not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and FTP client is not connected.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pAuthdUserIdentity;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up any resources being used. This method is thread-safe.
        /// </summary>
        public override void Dispose()
        {
            lock (this)
            {
                base.Dispose();

                m_pDataConnectionIP = null;
            }
        }

        /// <summary>
        /// Closes connection to FTP server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        public override void Disconnect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("FTP client is not connected.");
            }

            try
            {
                // Send QUIT command to server.                
                WriteLine("QUIT");
            }
            catch {}

            try
            {
                base.Disconnect();
            }
            catch {}

            m_pExtCapabilities = null;
            m_pAuthdUserIdentity = null;
            if (m_pDataConnection != null)
            {
                m_pDataConnection.Dispose();
                m_pDataConnection = null;
            }
        }

        /// <summary>
        /// Terminates the user and flushes all state information on the server. The connection is left open. 
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void Reinitialize()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            WriteLine("REIN");

            string[] response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Authenticates user. Authenticate method chooses strongest possible authentication method supported by server.
        /// </summary>
        /// <param name="userName">User login name.</param>
        /// <param name="password">Password.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected or is already authenticated.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>userName</b> is null.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void Authenticate(string userName, string password)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException("Session is already authenticated.");
            }
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }
            if (password == null)
            {
                password = "";
            }

            WriteLine("USER " + userName);

            string[] response = ReadResponse();
            if (response[0].StartsWith("331"))
            {
                WriteLine("PASS " + password);

                /* FTP server may give multiline reply here
				   For example:
					230-User someuser has group access to:  someuser
			    	230 OK. Current restricted directory is /
				*/
                response = ReadResponse();
                if (!response[0].StartsWith("230"))
                {
                    throw new FTP_ClientException(response[0]);
                }

                m_pAuthdUserIdentity = new GenericIdentity(userName, "ftp-user/pass");
            }
            else
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Send NOOP command to server. This method can be used for keeping connection alive(not timing out).
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void Noop()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            WriteLine("NOOP");

            string[] response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Aborts an active file transfer.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void Abort()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            WriteLine("ABOR");

            string line = ReadLine();
            if (!line.StartsWith("2"))
            {
                throw new FTP_ClientException(line);
            }
        }

        /// <summary>
        /// Gets current working directory in the sFTP server.
        /// </summary>
        /// <returns>Returns current working directory.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public string GetCurrentDir()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            WriteLine("PWD");

            string[] response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }

            StringReader r = new StringReader(response[0]);
            // Skip status code.
            r.ReadWord();

            return r.ReadWord();
        }

        /// <summary>
        /// Changes the current working directory on the server.
        /// </summary>
        /// <param name="path">Directory absolute or relative path to the current working directory.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void SetCurrentDir(string path)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argumnet 'path' must be specified.");
            }

            WriteLine("CWD " + path);

            string[] response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Gets files and directories in the current server directory.
        /// </summary>
        /// <returns>Returns current working directory listing.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public FTP_ListItem[] GetList()
        {
            return GetList(null);
        }

        /// <summary>
        /// Gets files and directories in the current server directory.
        /// </summary>
        /// <param name="path">Directory or file name which listing to get. Value null means current directory will be listed.</param>
        /// <returns>Returns current working directory listing.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected or FTP data connection has active read/write operation.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public FTP_ListItem[] GetList(string path)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (m_pDataConnection.IsActive)
            {
                throw new InvalidOperationException(
                    "There is already active read/write operation on data connection.");
            }

            List<FTP_ListItem> retVal = new List<FTP_ListItem>();

            // Set transfer mode
            SetTransferType(TransferType.Binary);

            if (m_TransferMode == FTP_TransferMode.Passive)
            {
                Pasv();
            }
            else
            {
                Port();
            }

            // If FTP server supports MLSD command, use it to get directory listing.
            // MLSD is standard way to get dir listing, while LIST command isn't any strict standard.
            bool mlsdSupported = false;
            foreach (string feature in m_pExtCapabilities)
            {
                if (feature.ToLower().StartsWith("mlsd"))
                {
                    mlsdSupported = true;
                    break;
                }
            }

            #region MLSD

            if (mlsdSupported)
            {
                if (string.IsNullOrEmpty(path))
                {
                    WriteLine("MLSD");
                }
                else
                {
                    WriteLine("MLSD " + path);
                }

                string[] response = ReadResponse();
                if (!response[0].StartsWith("1"))
                {
                    throw new FTP_ClientException(response[0]);
                }

                MemoryStream ms = new MemoryStream();
                m_pDataConnection.ReadAll(ms);

                response = ReadResponse();
                if (!response[0].StartsWith("2"))
                {
                    throw new FTP_ClientException(response[0]);
                }

                byte[] lineBuffer = new byte[8000];
                ms.Position = 0;
                SmartStream mlsdStream = new SmartStream(ms, true);
                while (true)
                {
                    SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(lineBuffer,
                                                                                       SizeExceededAction.
                                                                                           JunkAndThrowException);
                    mlsdStream.ReadLine(args, false);
                    if (args.Error != null)
                    {
                        throw args.Error;
                    }
                    string line = args.LineUtf8;

                    // We reached end of stream, we readed whole list sucessfully.
                    if (line == null)
                    {
                        break;
                    }
                    else
                    {
                        string[] parameters = line.Substring(0, line.LastIndexOf(';')).Split(';');
                        string name = line.Substring(line.LastIndexOf(';') + 1).Trim();

                        string type = "";
                        long size = 0;
                        DateTime modified = DateTime.MinValue;
                        foreach (string parameter in parameters)
                        {
                            string[] name_value = parameter.Split('=');
                            if (name_value[0].ToLower() == "type")
                            {
                                type = name_value[1].ToLower();
                            }
                            else if (name_value[0].ToLower() == "size")
                            {
                                size = Convert.ToInt32(name_value[1]);
                            }
                            else if (name_value[0].ToLower() == "modify")
                            {
                                modified = DateTime.ParseExact(name_value[1],
                                                               "yyyyMMddHHmmss",
                                                               DateTimeFormatInfo.InvariantInfo);
                            }
                            else
                            {
                                // Other options won't interest us, skip them.
                            }
                        }

                        if (type == "dir")
                        {
                            retVal.Add(new FTP_ListItem(name, 0, modified, true));
                        }
                        else if (type == "file")
                        {
                            retVal.Add(new FTP_ListItem(name, size, modified, false));
                        }
                    }
                }
            }

                #endregion

                #region LIST

            else
            {
                if (string.IsNullOrEmpty(path))
                {
                    WriteLine("LIST");
                }
                else
                {
                    WriteLine("LIST " + path);
                }

                string[] response = ReadResponse();
                if (!response[0].StartsWith("1"))
                {
                    throw new FTP_ClientException(response[0]);
                }

                MemoryStream ms = new MemoryStream();
                m_pDataConnection.ReadAll(ms);

                response = ReadResponse();
                if (!response[0].StartsWith("2"))
                {
                    throw new FTP_ClientException(response[0]);
                }

                ms.Position = 0;
                SmartStream listStream = new SmartStream(ms, true);
                SmartStream.ReadLineAsyncOP args = new SmartStream.ReadLineAsyncOP(new byte[8000],
                                                                                   SizeExceededAction.
                                                                                       JunkAndThrowException);
                listStream.ReadLine(args, false);
                if (args.Error != null)
                {
                    throw args.Error;
                }
                string line = args.LineUtf8;

                string listingType = "unix";
                // Dedect listing.
                if (line != null)
                {
                    try
                    {
                        StringReader r = new StringReader(line);
                        DateTime modified = DateTime.ParseExact(r.ReadWord() + " " + r.ReadWord(),
                                                                new[] {"MM-dd-yy hh:mmtt"},
                                                                DateTimeFormatInfo.InvariantInfo,
                                                                DateTimeStyles.None);
                        listingType = "win";
                    }
                    catch {}
                }

                string[] winDateFormats = new[] {"M-d-yy h:mmtt"};
                string[] unixFormats = new[] {"MMM d H:mm", "MMM d yyyy"};

                byte[] lineBuffer = new byte[8000];
                while (line != null)
                {
                    // Windows listing.                 
                    if (listingType == "win")
                    {
                        // MM-dd-yy hh:mm <DIR> directoryName
                        // MM-dd-yy hh:mm size  fileName

                        StringReader r = new StringReader(line);
                        // Read date
                        DateTime modified = DateTime.ParseExact(r.ReadWord() + " " + r.ReadWord(),
                                                                winDateFormats,
                                                                DateTimeFormatInfo.InvariantInfo,
                                                                DateTimeStyles.None);

                        r.ReadToFirstChar();
                        // We have directory.
                        if (r.StartsWith("<dir>", false))
                        {
                            r.ReadSpecifiedLength(5);
                            r.ReadToFirstChar();

                            retVal.Add(new FTP_ListItem(r.ReadToEnd(), 0, modified, true));
                        }
                            // We have file
                        else
                        {
                            // Read file size
                            long size = Convert.ToInt64(r.ReadWord());
                            r.ReadToFirstChar();

                            retVal.Add(new FTP_ListItem(r.ReadToEnd(), size, modified, false));
                        }
                    }
                        // Unix listing
                    else
                    {
                        // "d"directoryAtttributes xx xx xx 0 MMM d HH:mm/yyyy directoryName
                        // fileAtttributes xx xx xx fileSize MMM d HH:mm/yyyy fileName

                        StringReader r = new StringReader(line);
                        string attributes = r.ReadWord();
                        r.ReadWord();
                        r.ReadWord();
                        r.ReadWord();
                        long size = Convert.ToInt64(r.ReadWord());
                        DateTime modified =
                            DateTime.ParseExact(r.ReadWord() + " " + r.ReadWord() + " " + r.ReadWord(),
                                                unixFormats,
                                                DateTimeFormatInfo.InvariantInfo,
                                                DateTimeStyles.None);
                        r.ReadToFirstChar();
                        string name = r.ReadToEnd();
                        if (name != "." && name != "..")
                        {
                            if (attributes.StartsWith("d"))
                            {
                                retVal.Add(new FTP_ListItem(name, 0, modified, true));
                            }
                            else
                            {
                                retVal.Add(new FTP_ListItem(name, size, modified, false));
                            }
                        }
                    }

                    listStream.ReadLine(args, false);
                    if (args.Error != null)
                    {
                        throw args.Error;
                    }
                    line = args.LineUtf8;
                }
            }

            #endregion

            return retVal.ToArray();
        }

        /// <summary>
        /// Gets specified file from FTP server.
        /// </summary>
        /// <param name="path">File absolute or relative path to the current working directory.</param>
        /// <param name="storePath">Local file path where to store received file.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected or FTP data connection has active read/write operation.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> or <b>storePath</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void GetFile(string path, string storePath)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (m_pDataConnection.IsActive)
            {
                throw new InvalidOperationException(
                    "There is already active read/write operation on data connection.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }
            if (storePath == null)
            {
                throw new ArgumentNullException("storePath");
            }
            if (storePath == "")
            {
                throw new ArgumentException("Argument 'storePath' value must be specified.");
            }

            using (FileStream fs = File.Create(storePath))
            {
                GetFile(path, fs);
            }
        }

        /// <summary>
        /// Gets specified file from FTP server.
        /// </summary>
        /// <param name="path">File absolute or relative path to the current working directory.</param>
        /// <param name="stream">Stream where to store received file.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected or FTP data connection has active read/write operation.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void GetFile(string path, Stream stream)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (m_pDataConnection.IsActive)
            {
                throw new InvalidOperationException(
                    "There is already active read/write operation on data connection.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Set transfer mode
            SetTransferType(TransferType.Binary);

            if (m_TransferMode == FTP_TransferMode.Passive)
            {
                Pasv();
            }
            else
            {
                Port();
            }

            // Send RETR command
            WriteLine("RETR " + path);

            string[] response = ReadResponse();
            if (!response[0].StartsWith("1"))
            {
                throw new FTP_ClientException(response[0]);
            }

            m_pDataConnection.ReadAll(stream);

            /* FTP server may give multiline reply here
			/  For example:
			/	226-File successfully transferred
			/	226 0.002 seconds (measured here), 199.65 Mbytes per second 339163 bytes received in 00:00 (8.11 MB/s)
			*/
            response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Appends specified data to the existing file. If existing file doesn't exist, it will be created.
        /// </summary>
        /// <param name="path">FTP server file absolute or relative path to the current working directory.</param>
        /// <param name="stream">Stream which data append to the specified FTP server file.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected or FTP data connection has active read/write operation.</exception>
        /// <exception cref="ArgumentNullException">Is raied when <b>file</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void AppendToFile(string path, Stream stream)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (m_pDataConnection.IsActive)
            {
                throw new InvalidOperationException(
                    "There is already active read/write operation on data connection.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Set transfer mode
            SetTransferType(TransferType.Binary);

            if (m_TransferMode == FTP_TransferMode.Passive)
            {
                Pasv();
            }
            else
            {
                Port();
            }

            // Send APPE command
            WriteLine("APPE " + path);

            string[] response = ReadResponse();
            if (!response[0].StartsWith("1"))
            {
                throw new FTP_ClientException(response[0]);
            }

            m_pDataConnection.WriteAll(stream);

            /* FTP server may give multiline reply here
			/  For example:
			/	226-File successfully transferred
			/	226 0.002 seconds (measured here), 199.65 Mbytes per second 339163 bytes received in 00:00 (8.11 MB/s)
			*/
            response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Stores specified file to FTP server.
        /// </summary>
        /// <param name="path">File absolute or relative path to the current working directory.</param>
        /// <param name="sourcePath">File path which to store to FTP server.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected or FTP data connection has active read/write operation.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> or <b>sourcePath</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void StoreFile(string path, string sourcePath)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (m_pDataConnection.IsActive)
            {
                throw new InvalidOperationException(
                    "There is already active read/write operation on data connection.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }
            if (sourcePath == null)
            {
                throw new ArgumentNullException("sourcePath");
            }
            if (sourcePath == "")
            {
                throw new ArgumentException("Argument 'sourcePath' value must be specified.");
            }

            using (FileStream fs = File.OpenRead(sourcePath))
            {
                StoreFile(path, fs);
            }
        }

        /// <summary>
        /// Stores specified file to FTP server.
        /// </summary>
        /// <param name="path">File absolute or relative path to the current working directory.</param>
        /// <param name="stream">Stream which data to store to FTP server.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected or FTP data connection has active read/write operation.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void StoreFile(string path, Stream stream)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (m_pDataConnection.IsActive)
            {
                throw new InvalidOperationException(
                    "There is already active read/write operation on data connection.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Set transfer mode
            SetTransferType(TransferType.Binary);

            if (m_TransferMode == FTP_TransferMode.Passive)
            {
                Pasv();
            }
            else
            {
                Port();
            }

            // Send STOR command
            WriteLine("STOR " + path);

            string[] response = ReadResponse();
            if (!response[0].StartsWith("1"))
            {
                throw new FTP_ClientException(response[0]);
            }

            m_pDataConnection.WriteAll(stream);

            /* FTP server may give multiline reply here
			/  For example:
			/	226-File successfully transferred
			/	226 0.002 seconds (measured here), 199.65 Mbytes per second 339163 bytes received in 00:00 (8.11 MB/s)
			*/
            response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Deletes specified file from ftp server.
        /// </summary>
        /// <param name="path">File absolute or relative path to the current working directory.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void DeleteFile(string path)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }

            WriteLine("DELE " + path);

            string reply = ReadLine();
            if (!reply.StartsWith("250"))
            {
                throw new FTP_ClientException(reply);
            }
        }

        /// <summary>
        /// Renames file or directory to the new specified name.
        /// </summary>
        /// <param name="fromPath">Exisitng file or directory absolute or relative path to the current working directory.</param>
        /// <param name="toPath">New file or directory absolute or relative path to the current working directory.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>fromPath</b> or <b>toPath</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void Rename(string fromPath, string toPath)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (fromPath == null)
            {
                throw new ArgumentNullException("fromPath");
            }
            if (fromPath == "")
            {
                throw new ArgumentException("Argument 'fromPath' value must be specified.");
            }
            if (toPath == null)
            {
                throw new ArgumentNullException("toPath");
            }
            if (toPath == "")
            {
                throw new ArgumentException("Argument 'toPath' value must be specified.");
            }

            WriteLine("RNFR " + fromPath);

            string reply = ReadLine();
            if (!reply.StartsWith("350"))
            {
                throw new FTP_ClientException(reply);
            }

            WriteLine("RNTO " + toPath);

            reply = ReadLine();
            if (!reply.StartsWith("250"))
            {
                throw new FTP_ClientException(reply);
            }
        }

        /// <summary>
        /// Creates a directory on the FTP server.
        /// </summary>
        /// <param name="path">Directory absolute or relative path to the current working directory.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void CreateDirectory(string path)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }

            WriteLine("MKD " + path);

            string reply = ReadLine();
            if (!reply.StartsWith("257"))
            {
                throw new FTP_ClientException(reply);
            }
        }

        /// <summary>
        /// Deletes specified directory from FTP server.
        /// </summary>
        /// <param name="path">Directory absolute or relative path to the current working directory.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when FTP client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>path</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="FTP_ClientException">Is raised when FTP server returns error.</exception>
        public void DeleteDirectory(string path)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == "")
            {
                throw new ArgumentException("Argument 'path' value must be specified.");
            }

            WriteLine("RMD " + path);

            string reply = ReadLine();
            if (!reply.StartsWith("250"))
            {
                throw new FTP_ClientException(reply);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// This method is called after TCP client has sucessfully connected.
        /// </summary>
        protected override void OnConnected()
        {
            m_pDataConnection = new DataConnection(this);

            /*
			  Notes: Greeting may be single or multiline response.
			 		
			  Examples:
			 	220<SP>FTP server ready<CRLF> 
			  
			 	220-FTP server ready<CRLF>
			 	220-Addtitional text<CRLF>
			 	220<SP>final row<CRLF>			  
			*/

            string line = ReadLine();
            if (line.StartsWith("220"))
            {
                StringBuilder greetText = new StringBuilder();
                greetText.Append(line.Substring(4));

                // Read multiline greet text.
                while (line.StartsWith("220-"))
                {
                    line = ReadLine();

                    greetText.AppendLine(line.Substring(4));
                }

                m_GreetingText = greetText.ToString();
            }
            else
            {
                throw new FTP_ClientException(line);
            }

            #region FEAT

            /* Try to get FTP server supported capabilities, if command not supported, just skip tat command.
                RFC 2389 3.
                    
                Examples:
                    C: FEAT
                    S: 211-Extensions supported:
                    S:  MLST size*;create;modify*;perm;media-type
                    S:  SIZE
                    S:  COMPRESSION
                    S:  MDTM
                    S: 211 END

            */

            WriteLine("FEAT");

            line = ReadLine();
            m_pExtCapabilities = new List<string>();
            if (line.StartsWith("211"))
            {
                line = ReadLine();
                while (line.StartsWith(" "))
                {
                    m_pExtCapabilities.Add(line.Trim());

                    line = ReadLine();
                }
            }

            #endregion
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Sets transfer typr.
        /// </summary>
        /// <param name="type">Transfer type.</param>
        private void SetTransferType(TransferType type)
        {
            if (type == TransferType.Ascii)
            {
                WriteLine("TYPE A");
            }
            else if (type == TransferType.Binary)
            {
                WriteLine("TYPE I");
            }
            else
            {
                throw new ArgumentException("Not supported argument 'type' value '" + type + "'.");
            }

            string[] response = ReadResponse();
            if (!response[0].StartsWith("2"))
            {
                throw new FTP_ClientException(response[0]);
            }
        }

        /// <summary>
        /// Sends PORT command to server.
        /// </summary>
        private void Port()
        {
            string[] response = null;
            // We will try all IP addresses assigned to this machine, the first one that the remote machine likes will be chosen.
            foreach (IPAddress ip in Dns.GetHostAddresses(""))
            {
                if (ip.AddressFamily == m_pDataConnection.LocalEndPoint.AddressFamily)
                {
                    WriteLine("PORT " + ip.ToString().Replace(".", ",") + "," +
                              (m_pDataConnection.LocalEndPoint.Port >> 8) + "," +
                              (m_pDataConnection.LocalEndPoint.Port & 0xFF));

                    response = ReadResponse();
                    if (response[0].StartsWith("2"))
                    {
                        m_pDataConnection.SwitchToActive();
                        return;
                    }
                }
            }

            // If we reach so far PORT commant didn't suceed.
            throw new FTP_ClientException(response[0]);
        }

        /// <summary>
        /// Sends PASV command to server.
        /// </summary>
        private void Pasv()
        {
            WriteLine("PASV");

            string[] response = ReadResponse();
            if (!response[0].StartsWith("227"))
            {
                throw new FTP_ClientException(response[0]);
            }

            // Parse IP:port from 227 Entering Passive Mode (192,168,1,10,1,10).
            string[] parts =
                response[0].Substring(response[0].IndexOf("(") + 1,
                                      response[0].IndexOf(")") - response[0].IndexOf("(") - 1).Split(',');

            m_pDataConnection.SwitchToPassive(
                new IPEndPoint(IPAddress.Parse(parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3]),
                               (Convert.ToInt32(parts[4]) << 8) | Convert.ToInt32(parts[5])));
        }

        /// <summary>
        /// Reads FTP server response line(s).
        /// </summary>
        /// <returns>Returns FTP server response.</returns>
        private string[] ReadResponse()
        {
            /*
                There can be single or multiline response.
             
                Examples:
                    226 File successfully transferred
              
			    	226-File successfully transferred
			        226 0.002 seconds (measured here), 199.65 Mbytes per second 339163 bytes received in 00:00 (8.11 MB/s)
            */

            List<string> retVal = new List<string>();
            while (true)
            {
                string response = ReadLine();
                // Server closed connection for some reason.
                if (response == null)
                {
                    throw new Exception("Remote host disconnected connection unexpectedly.");
                }
                retVal.Add(response);
                // Multiline response.
                if (response.Length >= 4 && response[3] == '-')
                {
                    // Fall to next loop cycle.
                }
                    // Single line response.
                else
                {
                    break;
                }
            }

            return retVal.ToArray();
        }

        #endregion
    }
}