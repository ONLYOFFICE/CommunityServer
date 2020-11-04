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


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    #endregion

    /// <summary>
    /// This class implements extended socket, provides usefull methods for reading and writing data to socket.
    /// </summary>
    public class SocketEx : IDisposable
    {
        #region Nested type: _BeginWritePeriodTerminated_State

        /// <summary>
        /// BeginWritePeriodTerminated state obejct.
        /// </summary>
        private struct _BeginWritePeriodTerminated_State
        {
            #region Members

            private readonly SocketCallBack m_Callback;
            private readonly bool m_CloseStream;
            private readonly Stream m_Stream;
            private readonly object m_Tag;
            private int m_CountSent;
            private bool m_HasCRLF;
            private int m_LastByte;

            #endregion

            #region Properties

            /// <summary>
            /// Gets source stream.
            /// </summary>
            public Stream Stream
            {
                get { return m_Stream; }
            }

            /// <summary>
            /// Gets if stream must be closed if reading completed.
            /// </summary>
            public bool CloseStream
            {
                get { return m_CloseStream; }
            }

            /// <summary>
            /// Gets user data.
            /// </summary>
            public object Tag
            {
                get { return m_Tag; }
            }

            /// <summary>
            /// Gets callback what must be called if asynchronous write ends.
            /// </summary>
            public SocketCallBack Callback
            {
                get { return m_Callback; }
            }

            /// <summary>
            /// Gets or sets if last sent data ends with CRLF.
            /// </summary>
            public bool HasCRLF
            {
                get { return m_HasCRLF; }

                set { m_HasCRLF = value; }
            }

            /// <summary>
            /// Gets or sets what is last sent byte.
            /// </summary>
            public int LastByte
            {
                get { return m_LastByte; }

                set { m_LastByte = value; }
            }

            /// <summary>
            /// Gets or sets how many bytes has written to socket.
            /// </summary>
            public int CountSent
            {
                get { return m_CountSent; }

                set { m_CountSent = value; }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="stream">Source stream.</param>
            /// <param name="closeStream">Specifies if stream must be closed after reading is completed.</param>
            /// <param name="tag">User data.</param>
            /// <param name="callback">Callback what to call if asynchronous data writing completes.</param>
            public _BeginWritePeriodTerminated_State(Stream stream,
                                                     bool closeStream,
                                                     object tag,
                                                     SocketCallBack callback)
            {
                m_Stream = stream;
                m_CloseStream = closeStream;
                m_Tag = tag;
                m_Callback = callback;
                m_HasCRLF = false;
                m_LastByte = -1;
                m_CountSent = 0;
            }

            #endregion
        }

        #endregion

        #region Nested type: BufferDataBlockCompleted

        private delegate void BufferDataBlockCompleted(Exception x, object tag);

        #endregion

        #region Members

        private readonly byte[] m_Buffer;
        private int m_AvailableInBuffer;
        private string m_Host = "";
        private DateTime m_LastActivityDate;
        private int m_OffsetInBuffer;
        private Encoding m_pEncoding;
        private SocketLogger m_pLogger;
        private Socket m_pSocket;
        private NetworkStream m_pSocketStream;
        private SslStream m_pSslStream;
        private long m_ReadedCount;
        private bool m_SSL;
        private long m_WrittenCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets socket default encoding. 
        /// </summary>
        public Encoding Encoding
        {
            get { return m_pEncoding; }

            set
            {
                if (m_pEncoding == null)
                {
                    throw new ArgumentNullException("Encoding");
                }

                m_pEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets logging source. If this is setted, reads/writes are logged to it.
        /// </summary>
        public SocketLogger Logger
        {
            get { return m_pLogger; }

            set { m_pLogger = value; }
        }

        /// <summary>
        /// Gets raw uderlaying socket.
        /// </summary>
        public Socket RawSocket
        {
            get { return m_pSocket; }
        }

        /// <summary>
        /// Gets if socket is connected.
        /// </summary>
        public bool Connected
        {
            get { return m_pSocket != null && m_pSocket.Connected; }
        }

        /// <summary>
        /// Gets the local endpoint.
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                if (m_pSocket == null)
                {
                    return null;
                }
                else
                {
                    return m_pSocket.LocalEndPoint;
                }
            }
        }

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                if (m_pSocket == null)
                {
                    return null;
                }
                else
                {
                    return m_pSocket.RemoteEndPoint;
                }
            }
        }

        /// <summary>
        /// Gets if socket is connected via SSL.
        /// </summary>
        public bool SSL
        {
            get { return m_SSL; }
        }

        /// <summary>
        /// Gets how many bytes are readed through this socket.
        /// </summary>
        public long ReadedCount
        {
            get { return m_ReadedCount; }
        }

        /// <summary>
        /// Gets how many bytes are written through this socket.
        /// </summary>
        public long WrittenCount
        {
            get { return m_WrittenCount; }
        }

        /// <summary>
        /// Gets when was last socket(read or write) activity.
        /// </summary>
        public DateTime LastActivity
        {
            get { return m_LastActivityDate; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SocketEx()
        {
            m_Buffer = new byte[8000];
            m_pEncoding = Encoding.UTF8;
            m_LastActivityDate = DateTime.Now;

            m_pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_pSocket.ReceiveTimeout = 60000;
            m_pSocket.SendTimeout = 60000;
        }

        /// <summary>
        /// Socket wrapper. NOTE: You must pass connected socket here !
        /// </summary>
        /// <param name="socket">Socket.</param>
        public SocketEx(Socket socket)
        {
            m_Buffer = new byte[8000];
            m_pEncoding = Encoding.UTF8;
            m_LastActivityDate = DateTime.Now;

            m_pSocket = socket;

            if (socket.ProtocolType == ProtocolType.Tcp)
            {
                m_pSocketStream = new NetworkStream(socket, false);
            }
            m_pSocket.ReceiveTimeout = 60000;
            m_pSocket.SendTimeout = 60000;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up any resouces being used.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Connects to the specified host.
        /// </summary>
        /// <param name="endpoint">IP endpoint where to connect.</param>
        public void Connect(IPEndPoint endpoint)
        {
            Connect(endpoint.Address.ToString(), endpoint.Port, false);
        }

        /// <summary>
        /// Connects to the specified host.
        /// </summary>
        /// <param name="endpoint">IP endpoint where to connect.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
        public void Connect(IPEndPoint endpoint, bool ssl)
        {
            Connect(endpoint.Address.ToString(), endpoint.Port, ssl);
        }

        /// <summary>
        /// Connects to the specified host.
        /// </summary>
        /// <param name="host">Host name or IP where to connect.</param>
        /// <param name="port">TCP port number where to connect.</param>
        public void Connect(string host, int port)
        {
            Connect(host, port, false);
        }

        /// <summary>
        /// Connects to the specified host.
        /// </summary>
        /// <param name="host">Host name or IP where to connect.</param>
        /// <param name="port">TCP port number where to connect.</param>
        /// <param name="ssl">Specifies if to connected via SSL.</param>
        public void Connect(string host, int port, bool ssl)
        {
            m_pSocket.Connect(new IPEndPoint(System.Net.Dns.GetHostAddresses(host)[0], port));
            // mono won't support it
            //m_pSocket.Connect(host,port);

            m_Host = host;
            m_pSocketStream = new NetworkStream(m_pSocket, false);

            if (ssl)
            {
                SwitchToSSL_AsClient();
            }
        }

        /// <summary>
        /// Disconnects socket.
        /// </summary>
        public void Disconnect()
        {
            lock (this)
            {
                if (m_pSocket != null)
                {
                    m_pSocket.Close();
                }

                m_SSL = false;
                m_pSocketStream = null;
                m_pSslStream = null;
                m_pSocket = null;
                m_OffsetInBuffer = 0;
                m_AvailableInBuffer = 0;
                m_Host = "";
                m_ReadedCount = 0;
                m_WrittenCount = 0;
            }
        }

        /// <summary>
        /// Shutdowns socket.
        /// </summary>
        /// <param name="how"></param>
        public void Shutdown(SocketShutdown how)
        {
            m_pSocket.Shutdown(how);
        }

        /// <summary>
        /// Associates a Socket with a local endpoint.
        /// </summary>
        /// <param name="loaclEP"></param>
        public void Bind(EndPoint loaclEP)
        {
            m_pSocket.Bind(loaclEP);
        }

        /// <summary>
        /// Places a Socket in a listening state.
        /// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue. </param>
        public void Listen(int backlog)
        {
            m_pSocket.Listen(backlog);
        }

        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="ssl"></param>
        /// <returns></returns>
        public SocketEx Accept(bool ssl)
        {
            Socket s = m_pSocket.Accept();
            return new SocketEx(s);
        }

        /// <summary>
        /// Switches socket to SSL mode. Throws excpetion is socket is already in SSL mode.
        /// </summary>
        /// <param name="certificate">Certificate to use for SSL.</param>
        public void SwitchToSSL(X509Certificate certificate)
        {
            if (m_SSL)
            {
                throw new Exception("Error can't switch to SSL, socket is already in SSL mode !");
            }

            SslStream sslStream = new SslStream(m_pSocketStream);
            sslStream.AuthenticateAsServer(certificate);

            m_SSL = true;
            m_pSslStream = sslStream;
        }

        /// <summary>
        /// Switches socket to SSL mode. Throws excpetion is socket is already in SSL mode.
        /// </summary>
        public void SwitchToSSL_AsClient()
        {
            if (m_SSL)
            {
                throw new Exception("Error can't switch to SSL, socket is already in SSL mode !");
            }

            SslStream sslStream = new SslStream(m_pSocketStream, true, RemoteCertificateValidationCallback);
            sslStream.AuthenticateAsClient(m_Host);

            m_SSL = true;
            m_pSslStream = sslStream;
        }

        /// <summary>
        /// Reads byte from socket. Returns readed byte or -1 if socket is shutdown and tehre is no more data available.
        /// </summary>
        /// <returns>Returns readed byte or -1 if socket is shutdown and tehre is no more data available.</returns>
        public int ReadByte()
        {
            BufferDataBlock();
            // Socket is shutdown
            if (m_AvailableInBuffer == 0)
            {
                m_OffsetInBuffer = 0;
                m_AvailableInBuffer = 0;
                return -1;
            }

            m_OffsetInBuffer++;
            m_AvailableInBuffer--;

            return m_Buffer[m_OffsetInBuffer - 1];
        }

        /// <summary>
        /// Reads line from socket. Maximum line length is 4000 bytes. NOTE: CRLF isn't written to destination stream.
        /// If maximum allowed line length is exceeded line is read to end, but isn't stored to buffer and exception
        /// is thrown after line reading.
        /// </summary>
        /// <returns>Returns readed line.</returns>
        public string ReadLine()
        {
            return ReadLine(4000);
        }

        /// <summary>
        /// Reads line from socket.NOTE: CRLF isn't written to destination stream.
        /// If maximum allowed line length is exceeded line is read to end, but isn't stored to buffer and exception
        /// is thrown after line reading.
        /// </summary>
        /// <param name="maxLineLength">Maximum line length in bytes.</param>
        /// <returns>Returns readed line.</returns>
        public string ReadLine(int maxLineLength)
        {
            return m_pEncoding.GetString(ReadLineByte(maxLineLength));
        }

        /// <summary>
        /// Reads line from socket.NOTE: CRLF isn't written to destination stream.
        /// If maximum allowed line length is exceeded line is read to end, but isn't stored to buffer and exception
        /// is thrown after line reading.
        /// </summary>
        /// <param name="maxLineLength">Maximum line length in bytes.</param>
        /// <returns>Returns readed line.</returns>
        public byte[] ReadLineByte(int maxLineLength)
        {
            MemoryStream strmLineBuf = new MemoryStream();
            ReadLine(strmLineBuf, maxLineLength);

            return strmLineBuf.ToArray();
        }

        /// <summary>
        /// Reads line from socket and stores it to specified stream. NOTE: CRLF isn't written to destination stream.
        /// If maximum allowed line length is exceeded line is read to end, but isn't stored to buffer and exception
        /// is thrown after line reading.
        /// </summary>
        /// <param name="stream">Stream where to store readed line.</param>
        /// <param name="maxLineLength">Maximum line length in bytes.</param>
        public void ReadLine(Stream stream, int maxLineLength)
        {
            // Delay last byte writing, this is because CR, if next is LF, then skip CRLF and terminate reading.

            int lastByte = ReadByte();
            int currentByte = ReadByte();
            int readedCount = 2;
            while (currentByte > -1)
            {
                // We got line
                if (lastByte == (byte) '\r' && currentByte == (byte) '\n')
                {
                    // Logging stuff
                    if (m_pLogger != null)
                    {
                        if (stream.CanSeek && stream.Length < 200)
                        {
                            byte[] readedData = new byte[stream.Length];
                            stream.Position = 0;
                            stream.Read(readedData, 0, readedData.Length);
                            m_pLogger.AddReadEntry(m_pEncoding.GetString(readedData), readedCount);
                        }
                        else
                        {
                            m_pLogger.AddReadEntry("Big binary line, readed " + readedCount + " bytes.",
                                                   readedCount);
                        }
                    }

                    stream.Flush();

                    // Maximum allowed length exceeded
                    if (readedCount > maxLineLength)
                    {
                        throw new ReadException(ReadReplyCode.LengthExceeded,
                                                "Maximum allowed line length exceeded !");
                    }

                    return;
                }
                else
                {
                    // Maximum allowed length exceeded, just don't store data.
                    if (readedCount < maxLineLength)
                    {
                        stream.WriteByte((byte) lastByte);
                    }
                    lastByte = currentByte;
                }

                // Read next byte
                currentByte = ReadByte();
                readedCount++;
            }

            // We should not reach there, if so then socket closed
            // Logging stuff
            if (m_pLogger != null)
            {
                m_pLogger.AddTextEntry("Remote host closed socket !");
            }
            throw new ReadException(ReadReplyCode.SocketClosed,
                                    "Connected host closed socket, read line terminated unexpectedly !");
        }

        /// <summary>
        /// Reads specified length of data from socket and store to specified stream.
        /// </summary>
        /// <param name="lengthToRead">Specifies how much data to read from socket.</param>
        /// <param name="storeStream">Stream where to store data.</param>
        public void ReadSpecifiedLength(int lengthToRead, Stream storeStream)
        {
            while (lengthToRead > 0)
            {
                BufferDataBlock();
                // Socket is shutdown
                if (m_AvailableInBuffer == 0)
                {
                    m_OffsetInBuffer = 0;
                    m_AvailableInBuffer = 0;
                    // Logging stuff
                    if (m_pLogger != null)
                    {
                        m_pLogger.AddTextEntry("Remote host closed socket, all data wans't readed !");
                    }
                    throw new Exception("Remote host closed socket, all data wans't readed !");
                }

                // We have all data in buffer what we need.
                if (m_AvailableInBuffer >= lengthToRead)
                {
                    storeStream.Write(m_Buffer, m_OffsetInBuffer, lengthToRead);
                    storeStream.Flush();

                    m_OffsetInBuffer += lengthToRead;
                    m_AvailableInBuffer -= lengthToRead;
                    lengthToRead = 0;

                    // Logging stuff
                    if (m_pLogger != null)
                    {
                        if (storeStream.CanSeek && storeStream.Length < 200)
                        {
                            byte[] readedData = new byte[storeStream.Length];
                            storeStream.Position = 0;
                            storeStream.Read(readedData, 0, readedData.Length);
                            m_pLogger.AddReadEntry(m_pEncoding.GetString(readedData), lengthToRead);
                        }
                        else
                        {
                            m_pLogger.AddReadEntry("Big binary data, readed " + lengthToRead + " bytes.",
                                                   lengthToRead);
                        }
                    }
                }
                    // We need more data than buffer has,read all buffer data.
                else
                {
                    storeStream.Write(m_Buffer, m_OffsetInBuffer, m_AvailableInBuffer);
                    storeStream.Flush();

                    lengthToRead -= m_AvailableInBuffer;
                    m_OffsetInBuffer = 0;
                    m_AvailableInBuffer = 0;
                }
            }
        }

        /// <summary>
        /// Reads period terminated string. The data is terminated by a line containing only a period, that is,
        /// the character sequence "&lt;CRLF&gt;.&lt;CRLF&gt;".
        /// When a line of text is received, it checks the line. If the line is composed of a single period,
        /// it is treated as the end of data indicator.  If the first character is a period and there are 
        /// other characters on the line, the first character is deleted.
        /// If maximum allowed data length is exceeded data is read to end, but isn't stored to buffer and exception
        /// is thrown after data reading.
        /// </summary>
        /// <param name="maxLength">Maximum data length in bytes.</param>
        /// <returns></returns>
        public string ReadPeriodTerminated(int maxLength)
        {
            MemoryStream ms = new MemoryStream();
            ReadPeriodTerminated(ms, maxLength);

            return m_pEncoding.GetString(ms.ToArray());
        }

        /// <summary>
        /// Reads period terminated data. The data is terminated by a line containing only a period, that is,
        /// the character sequence "&lt;CRLF&gt;.&lt;CRLF&gt;".
        /// When a line of text is received, it checks the line. If the line is composed of a single period,
        /// it is treated as the end of data indicator.  If the first character is a period and there are 
        /// other characters on the line, the first character is deleted.
        /// If maximum allowed data length is exceeded data is read to end, but isn't stored to stream and exception
        /// is thrown after data reading.
        /// </summary>
        /// <param name="stream">Stream where to store readed data.</param>
        /// <param name="maxLength">Maximum data length in bytes.</param>
        public void ReadPeriodTerminated(Stream stream, int maxLength)
        {
            /* When a line of text is received by the server, it checks the line.
               If the line is composed of a single period, it is treated as the 
               end of data indicator.  If the first character is a period and 
               there are other characters on the line, the first character is deleted.
            */

            // Delay last byte writing, this is because CR, if next is LF, then last byte isn't written.

            byte[] buffer = new byte[8000];
            int positionInBuffer = 0;

            int lastByte = ReadByte();
            int currentByte = ReadByte();
            int readedCount = 2;
            bool lineBreak = false;
            bool expectCRLF = false;
            while (currentByte > -1)
            {
                // We got <CRLF> + 1 char, we must skip that char if it is '.'.
                if (lineBreak)
                {
                    lineBreak = false;

                    // We must skip that char if it is '.'
                    if (currentByte == '.')
                    {
                        expectCRLF = true;

                        currentByte = ReadByte();
                    }
                }
                    // We got <CRLF>
                else if (lastByte == (byte) '\r' && currentByte == (byte) '\n')
                {
                    lineBreak = true;

                    // We have <CRLF>.<CRLF>, skip last <CRLF>.
                    if (expectCRLF)
                    {
                        // There is data in buffer, flush it
                        if (positionInBuffer > 0)
                        {
                            stream.Write(buffer, 0, positionInBuffer);
                            positionInBuffer = 0;
                        }

                        // Logging stuff
                        if (m_pLogger != null)
                        {
                            if (stream.CanSeek && stream.Length < 200)
                            {
                                byte[] readedData = new byte[stream.Length];
                                stream.Position = 0;
                                stream.Read(readedData, 0, readedData.Length);
                                m_pLogger.AddReadEntry(m_pEncoding.GetString(readedData), readedCount);
                            }
                            else
                            {
                                m_pLogger.AddReadEntry("Big binary data, readed " + readedCount + " bytes.",
                                                       readedCount);
                            }
                        }

                        // Maximum allowed length exceeded
                        if (readedCount > maxLength)
                        {
                            throw new ReadException(ReadReplyCode.LengthExceeded,
                                                    "Maximum allowed line length exceeded !");
                        }

                        return;
                    }
                }

                // current char isn't CRLF part, so it isn't <CRLF>.<CRLF> terminator.
                if (expectCRLF && !(currentByte == (byte) '\r' || currentByte == (byte) '\n'))
                {
                    expectCRLF = false;
                }

                // Maximum allowed length exceeded, just don't store data.
                if (readedCount < maxLength)
                {
                    // Buffer is filled up, write buffer to stream
                    if (positionInBuffer > (buffer.Length - 2))
                    {
                        stream.Write(buffer, 0, positionInBuffer);
                        positionInBuffer = 0;
                    }

                    buffer[positionInBuffer] = (byte) lastByte;
                    positionInBuffer++;
                }

                // Read next byte
                lastByte = currentByte;
                currentByte = ReadByte();
                readedCount++;
            }

            // We never should reach there, only if data isn't <CRLF>.<CRLF> terminated.
            // Logging stuff
            if (m_pLogger != null)
            {
                m_pLogger.AddTextEntry("Remote host closed socket and data wasn't <CRLF>.<CRLF> terminated !");
            }
            throw new Exception("Remote host closed socket and data wasn't <CRLF>.<CRLF> terminated !");
        }

        /// <summary>
        /// Writes specified data to socket.
        /// </summary>
        /// <param name="data">Data to write to socket.</param>
        public void Write(string data)
        {
            if (Logger != null)
            {
                Logger.AddSendEntry(data, data.Length);
            }
            Write(new MemoryStream(m_pEncoding.GetBytes(data)));
        }

        /// <summary>
        /// Writes specified data to socket.
        /// </summary>
        /// <param name="data">Data to to wite to socket.</param>
        public void Write(byte[] data)
        {
            Write(new MemoryStream(data));
        }

        /// <summary>
        /// Writes specified data to socket.
        /// </summary>
        /// <param name="data">Data to to wite to socket.</param>
        /// <param name="offset">Offset in data from where to start sending data.</param>
        /// <param name="length">Lengh of data to send.</param>
        public void Write(byte[] data, int offset, int length)
        {
            MemoryStream ms = new MemoryStream(data);
            ms.Position = offset;
            Write(ms, length);
        }

        /// <summary>
        /// Writes specified data to socket.
        /// </summary>
        /// <param name="stream">Stream which data to write to socket. Reading starts from stream current position and will be readed to EOS.</param>
        public void Write(Stream stream)
        {
            m_pSocket.NoDelay = false;

            byte[] buffer = new byte[4000];
            int sentCount = 0;
            int readedCount = stream.Read(buffer, 0, buffer.Length);
            while (readedCount > 0)
            {
                if (m_SSL)
                {
                    m_pSslStream.Write(buffer, 0, readedCount);
                    m_pSslStream.Flush();
                }
                else
                {
                    m_pSocketStream.Write(buffer, 0, readedCount);
                }

                sentCount += readedCount;
                m_WrittenCount += readedCount;
                m_LastActivityDate = DateTime.Now;

                readedCount = stream.Read(buffer, 0, buffer.Length);
            }

            // Logging stuff
            if (m_pLogger != null)
            {
                if (sentCount < 200)
                {
                    m_pLogger.AddSendEntry(m_pEncoding.GetString(buffer, 0, sentCount), sentCount);
                }
                else
                {
                    m_pLogger.AddSendEntry("Big binary data, sent " + sentCount + " bytes.", sentCount);
                }
            }
        }

        /// <summary>
        /// Writes specified data to socket.
        /// </summary>
        /// <param name="stream">Stream which data to write to socket. Reading starts from stream current position and specified count will be readed.</param>
        /// <param name="count">Number of bytes to read from stream and write to socket.</param>
        public void Write(Stream stream, long count)
        {
            m_pSocket.NoDelay = false;

            byte[] buffer = new byte[4000];
            int sentCount = 0;
            int readedCount = 0;
            if ((count - sentCount) > buffer.Length)
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
            }
            else
            {
                readedCount = stream.Read(buffer, 0, (int) (count - sentCount));
            }
            while (sentCount < count)
            {
                if (m_SSL)
                {
                    m_pSslStream.Write(buffer, 0, readedCount);
                    m_pSslStream.Flush();
                }
                else
                {
                    m_pSocketStream.Write(buffer, 0, readedCount);
                }

                sentCount += readedCount;
                m_WrittenCount += readedCount;
                m_LastActivityDate = DateTime.Now;

                if ((count - sentCount) > buffer.Length)
                {
                    readedCount = stream.Read(buffer, 0, buffer.Length);
                }
                else
                {
                    readedCount = stream.Read(buffer, 0, (int) (count - sentCount));
                }
            }

            // Logging stuff
            if (m_pLogger != null)
            {
                if (sentCount < 200)
                {
                    m_pLogger.AddSendEntry(m_pEncoding.GetString(buffer, 0, sentCount), sentCount);
                }
                else
                {
                    m_pLogger.AddSendEntry("Big binary data, sent " + sentCount + " bytes.", sentCount);
                }
            }
        }

        /// <summary>
        /// Writes specified line to socket. If line isn't CRLF terminated, CRLF is added automatically.
        /// </summary>
        /// <param name="line">Line to write to socket.</param>
        public void WriteLine(string line)
        {
            if (Logger != null)
            {
                Logger.AddSendEntry(line, line.Length);
            }
            WriteLine(m_pEncoding.GetBytes(line));
        }

        /// <summary>
        /// Writes specified line to socket. If line isn't CRLF terminated, CRLF is added automatically.
        /// </summary>
        /// <param name="line">Line to write to socket.</param>
        public void WriteLine(byte[] line)
        {
            // Don't allow to wait after we send data, because there won't no more data
            m_pSocket.NoDelay = true;

            // <CRF> is missing, add it
            if (line.Length < 2 ||
                (line[line.Length - 2] != (byte) '\r' && line[line.Length - 1] != (byte) '\n'))
            {
                byte[] newLine = new byte[line.Length + 2];
                Array.Copy(line, newLine, line.Length);
                newLine[newLine.Length - 2] = (byte) '\r';
                newLine[newLine.Length - 1] = (byte) '\n';

                line = newLine;
            }

            if (m_SSL)
            {
                m_pSslStream.Write(line);
            }
            else
            {
                m_pSocketStream.Write(line, 0, line.Length);
            }

            m_WrittenCount += line.Length;
            m_LastActivityDate = DateTime.Now;

            // Logging stuff
            if (m_pLogger != null)
            {
                if (line.Length < 200)
                {
                    m_pLogger.AddSendEntry(m_pEncoding.GetString(line), line.Length);
                }
                else
                {
                    m_pLogger.AddSendEntry("Big binary line, sent " + line.Length + " bytes.", line.Length);
                }
            }
        }

        /// <summary>
        /// Writes period terminated string to socket. The data is terminated by a line containing only a period, that is,
        /// the character sequence "&lt;CRLF&gt;.&lt;CRLF&gt;". Before sending a line of text, check the first
        /// character of the line.If it is a period, one additional period is inserted at the beginning of the line.
        /// </summary>
        /// <param name="data">String data to write.</param>
        public void WritePeriodTerminated(string data)
        {
            WritePeriodTerminated(new MemoryStream(m_pEncoding.GetBytes(data)));
        }

        /// <summary>
        /// Writes period terminated data to socket. The data is terminated by a line containing only a period, that is,
        /// the character sequence "&lt;CRLF&gt;.&lt;CRLF&gt;". Before sending a line of text, check the first
        /// character of the line.If it is a period, one additional period is inserted at the beginning of the line.
        /// </summary>
        /// <param name="stream">Stream which data to write. Reading begins from stream current position and is readed to EOS.</param>
        public void WritePeriodTerminated(Stream stream)
        {
            /* Before sending a line of text, check the first character of the line.
               If it is a period, one additional period is inserted at the beginning of the line.
            */

            int countSent = 0;
            byte[] buffer = new byte[4000];
            int positionInBuffer = 0;
            bool CRLF = false;
            int lastByte = -1;
            int currentByte = stream.ReadByte();
            while (currentByte > -1)
            {
                // We have CRLF, mark it up
                if (lastByte == '\r' && currentByte == '\n')
                {
                    CRLF = true;
                }
                    // There is CRLF + current byte
                else if (CRLF)
                {
                    // If it is a period, one additional period is inserted at the beginning of the line.
                    if (currentByte == '.')
                    {
                        buffer[positionInBuffer] = (byte) '.';
                        positionInBuffer++;
                    }

                    // CRLF handled, reset it
                    CRLF = false;
                }

                buffer[positionInBuffer] = (byte) currentByte;
                positionInBuffer++;

                lastByte = currentByte;

                // Buffer is filled up, write buffer to socket.
                if (positionInBuffer > (4000 - 10))
                {
                    if (m_SSL)
                    {
                        m_pSslStream.Write(buffer, 0, positionInBuffer);
                    }
                    else
                    {
                        m_pSocketStream.Write(buffer, 0, positionInBuffer);
                    }
                    countSent += positionInBuffer;
                    m_WrittenCount += positionInBuffer;
                    m_LastActivityDate = DateTime.Now;
                    positionInBuffer = 0;
                }

                currentByte = stream.ReadByte();
            }

            // We have readed all data, write budder data + .<CRLF> or <CRLF>.<CRLF> if data not <CRLF> terminated.
            if (!CRLF)
            {
                buffer[positionInBuffer] = (byte) '\r';
                positionInBuffer++;
                buffer[positionInBuffer] = (byte) '\n';
                positionInBuffer++;
            }

            buffer[positionInBuffer] = (byte) '.';
            positionInBuffer++;
            buffer[positionInBuffer] = (byte) '\r';
            positionInBuffer++;
            buffer[positionInBuffer] = (byte) '\n';
            positionInBuffer++;

            if (m_SSL)
            {
                m_pSslStream.Write(buffer, 0, positionInBuffer);
            }
            else
            {
                m_pSocketStream.Write(buffer, 0, positionInBuffer);
            }
            countSent += positionInBuffer;
            m_WrittenCount += positionInBuffer;
            m_LastActivityDate = DateTime.Now;
            //-------------------------------------------------------------------------------------//

            // Logging stuff
            if (m_pLogger != null)
            {
                if (countSent < 200)
                {
                    m_pLogger.AddSendEntry(m_pEncoding.GetString(buffer), buffer.Length);
                }
                else
                {
                    m_pLogger.AddSendEntry("Binary data, sent " + countSent + " bytes.", countSent);
                }
            }
        }

        /// <summary>
        /// Begins reading line from socket asynchrounously.
        /// If maximum allowed line length is exceeded line is read to end, but isn't stored to buffer and exception
        /// is thrown after line reading.
        /// </summary>
        /// <param name="stream">Stream where to store readed line.</param>
        /// <param name="maxLineLength">Maximum line length in bytes.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous line read operation is completed.</param>
        public void BeginReadLine(Stream stream, int maxLineLength, object tag, SocketCallBack callback)
        {
            TryToReadLine(callback, tag, stream, maxLineLength, -1, 0);
        }

        /// <summary>
        /// Begins reading specified amount of data from socket asynchronously.
        /// </summary>
        /// <param name="stream">Stream where to store readed data.</param>
        /// <param name="lengthToRead">Specifies number of bytes to read from socket.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous read operation is completed.</param>
        public void BeginReadSpecifiedLength(Stream stream,
                                             int lengthToRead,
                                             object tag,
                                             SocketCallBack callback)
        {
            TryToReadReadSpecifiedLength(stream, lengthToRead, tag, callback, 0);
        }

        /// <summary>
        /// Begins reading period terminated data. The data is terminated by a line containing only a period, that is,
        /// the character sequence "&lt;CRLF&gt;.&lt;CRLF&gt;".
        /// When a line of text is received, it checks the line. If the line is composed of a single period,
        /// it is treated as the end of data indicator.  If the first character is a period and there are 
        /// other characters on the line, the first character is deleted.
        /// If maximum allowed data length is exceeded data is read to end, but isn't stored to stream and exception
        /// is thrown after data reading.
        /// </summary>
        /// <param name="stream">Stream where to store readed data.</param>
        /// <param name="maxLength">Maximum data length in bytes.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous read operation is completed.</param>
        public void BeginReadPeriodTerminated(Stream stream,
                                              int maxLength,
                                              object tag,
                                              SocketCallBack callback)
        {
            TryToReadPeriodTerminated(callback, tag, stream, maxLength, -1, 0, false, false);
        }

        /// <summary>
        /// Begins writing specified data to socket.
        /// </summary>
        /// <param name="stream">Stream which data to write to socket. Reading starts from stream current position and will be readed to EOS.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous write operation is completed.</param>
        public void BeginWrite(Stream stream, object tag, SocketCallBack callback)
        {
            // Allow socket to optimise sends
            m_pSocket.NoDelay = false;

            BeginProcessingWrite(stream, tag, callback, 0);
        }

        /// <summary>
        /// Begins specified line sending to socket asynchronously.
        /// </summary>
        /// <param name="line">Line to send.</param>
        /// <param name="callback">The method to be called when the asynchronous line write operation is completed.</param>
        public void BeginWriteLine(string line, SocketCallBack callback)
        {
            // Don't allow to wait after we send data, because there won't no more data
            m_pSocket.NoDelay = true;

            BeginWriteLine(line, null, callback);
        }

        /// <summary>
        /// Begins specified line sending to socket asynchronously.
        /// </summary>
        /// <param name="line">Line to send.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous line write operation is completed.</param>
        public void BeginWriteLine(string line, object tag, SocketCallBack callback)
        {
            // Don't allow to wait after we send data, because there won't no more data
            m_pSocket.NoDelay = true;

            if (!line.EndsWith("\r\n"))
            {
                line += "\r\n";
            }

            byte[] lineBytes = m_pEncoding.GetBytes(line);
            if (m_SSL)
            {
                m_pSslStream.BeginWrite(lineBytes,
                                        0,
                                        lineBytes.Length,
                                        OnBeginWriteLineCallback,
                                        new[] {tag, callback, lineBytes});
            }
            else
            {
                m_pSocketStream.BeginWrite(lineBytes,
                                           0,
                                           lineBytes.Length,
                                           OnBeginWriteLineCallback,
                                           new[] {tag, callback, lineBytes});
            }
        }

        /// <summary>
        /// Begins writing period terminated data to socket. The data is terminated by a line containing only a period, that is,
        /// the character sequence "&lt;CRLF&gt;.&lt;CRLF&gt;". Before sending a line of text, check the first
        /// character of the line.If it is a period, one additional period is inserted at the beginning of the line.
        /// </summary>
        /// <param name="stream">Stream which data to write. Reading begins from stream current position and is readed to EOS.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous write operation is completed.</param>
        public void BeginWritePeriodTerminated(Stream stream, object tag, SocketCallBack callback)
        {
            BeginWritePeriodTerminated(stream, false, tag, callback);
        }

        /// <summary>
        /// Begins writing period terminated data to socket. The data is terminated by a line containing only a period, that is,
        /// the character sequence "&lt;CRLF&gt;.&lt;CRLF&gt;". Before sending a line of text, check the first
        /// character of the line.If it is a period, one additional period is inserted at the beginning of the line.
        /// </summary>
        /// <param name="stream">Stream which data to write. Reading begins from stream current position and is readed to EOS.</param>
        /// <param name="closeStream">Specifies if stream is closed after write operation has completed.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous write operation is completed.</param>
        public void BeginWritePeriodTerminated(Stream stream,
                                               bool closeStream,
                                               object tag,
                                               SocketCallBack callback)
        {
            // Allow socket to optimise sends
            m_pSocket.NoDelay = false;

            _BeginWritePeriodTerminated_State state = new _BeginWritePeriodTerminated_State(stream,
                                                                                            closeStream,
                                                                                            tag,
                                                                                            callback);
            BeginProcessingWritePeriodTerminated(state);
        }

        /// <summary>
        /// Sends data to the specified end point.
        /// </summary>
        /// <param name="data">Data to send.</param>
        /// <param name="remoteEP">Remote endpoint where to send data.</param>
        /// <returns>Returns number of bytes actualy sent.</returns>
        public int SendTo(byte[] data, EndPoint remoteEP)
        {
            return m_pSocket.SendTo(data, remoteEP);
        }

        #endregion

        #region Utility methods

        private bool RemoteCertificateValidationCallback(Object sender,
                                                         X509Certificate certificate,
                                                         X509Chain chain,
                                                         SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None ||
                sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                return true;
            }

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        /// <summary>
        /// Tries to read line from socket data buffer. If buffer doesn't contain line, 
        /// next buffer data block is getted asynchronously and this method is called again.
        /// </summary>
        /// <param name="callback">The method to be called when the asynchronous line read operation is completed.</param>
        /// <param name="tag">User data.</param>
        /// <param name="stream">Stream where to store readed data.</param>
        /// <param name="maxLineLength">Specifies maximum line legth.</param>
        /// <param name="lastByte">Last byte what was readed pevious method call or -1 if first method call.</param>
        /// <param name="readedCount">Specifies count of bytes readed.</param>
        private void TryToReadLine(SocketCallBack callback,
                                   object tag,
                                   Stream stream,
                                   int maxLineLength,
                                   int lastByte,
                                   int readedCount)
        {
            // There is no data in buffer, buffer next block asynchronously.
            if (m_AvailableInBuffer == 0)
            {
                BeginBufferDataBlock(OnBeginReadLineBufferingCompleted,
                                     new[] {callback, tag, stream, maxLineLength, lastByte, readedCount});
                return;
            }

            // Delay last byte writing, this is because CR, if next is LF, then skip CRLF and terminate reading.

            // This is first method call, buffer 1 byte
            if (lastByte == -1)
            {
                lastByte = ReadByte();
                readedCount++;

                // We use last byte, buffer next block asynchronously.
                if (m_AvailableInBuffer == 0)
                {
                    BeginBufferDataBlock(OnBeginReadLineBufferingCompleted,
                                         new[] {callback, tag, stream, maxLineLength, lastByte, readedCount});
                    return;
                }
            }

            int currentByte = ReadByte();
            readedCount++;
            while (currentByte > -1)
            {
                // We got line
                if (lastByte == (byte) '\r' && currentByte == (byte) '\n')
                {
                    // Logging stuff
                    if (m_pLogger != null)
                    {
                        if (stream.CanSeek && stream.Length < 200)
                        {
                            byte[] readedData = new byte[stream.Length];
                            stream.Position = 0;
                            stream.Read(readedData, 0, readedData.Length);
                            m_pLogger.AddReadEntry(m_pEncoding.GetString(readedData), readedCount);
                        }
                        else
                        {
                            m_pLogger.AddReadEntry("Big binary line, readed " + readedCount + " bytes.",
                                                   readedCount);
                        }
                    }

                    // Maximum allowed length exceeded
                    if (readedCount > maxLineLength)
                    {
                        if (callback != null)
                        {
                            callback(SocketCallBackResult.LengthExceeded,
                                     0,
                                     new ReadException(ReadReplyCode.LengthExceeded,
                                                       "Maximum allowed data length exceeded !"),
                                     tag);
                        }
                    }

                    // Line readed ok, call callback.
                    if (callback != null)
                    {
                        callback(SocketCallBackResult.Ok, readedCount, null, tag);
                    }

                    return;
                }
                else
                {
                    // Maximum allowed length exceeded, just don't store data.
                    if (readedCount < maxLineLength)
                    {
                        stream.WriteByte((byte) lastByte);
                    }
                }

                // Read next byte
                lastByte = currentByte;
                if (m_AvailableInBuffer > 0)
                {
                    currentByte = ReadByte();
                    readedCount++;
                }
                    // We have use all data in the buffer, buffer next block asynchronously.
                else
                {
                    BeginBufferDataBlock(OnBeginReadLineBufferingCompleted,
                                         new[] {callback, tag, stream, maxLineLength, lastByte, readedCount});
                    return;
                }
            }

            // We should not reach there, if so then socket closed
            // Logging stuff
            if (m_pLogger != null)
            {
                m_pLogger.AddTextEntry("Remote host closed socket !");
            }
            if (callback != null)
            {
                callback(SocketCallBackResult.SocketClosed,
                         0,
                         new ReadException(ReadReplyCode.SocketClosed,
                                           "Connected host closed socket, read line terminated unexpectedly !"),
                         tag);
            }
        }

        /// <summary>
        /// This method is called after asynchronous data buffering is completed.
        /// </summary>
        /// <param name="x">Exception what happened on method execution or null, if operation completed sucessfully.</param>
        /// <param name="tag">User data.</param>
        private void OnBeginReadLineBufferingCompleted(Exception x, object tag)
        {
            object[] param = (object[]) tag;
            SocketCallBack callback = (SocketCallBack) param[0];
            object callbackTag = param[1];
            Stream stream = (Stream) param[2];
            int maxLineLength = (int) param[3];
            int lastByte = (int) param[4];
            int readedCount = (int) param[5];

            if (x == null)
            {
                // We didn't get data, this can only happen if socket closed.
                if (m_AvailableInBuffer == 0)
                {
                    // Logging stuff
                    if (m_pLogger != null)
                    {
                        m_pLogger.AddTextEntry("Remote host closed socket !");
                    }

                    callback(SocketCallBackResult.SocketClosed, 0, null, callbackTag);
                }
                else
                {
                    TryToReadLine(callback, callbackTag, stream, maxLineLength, lastByte, readedCount);
                }
            }
            else
            {
                callback(SocketCallBackResult.Exception, 0, x, callbackTag);
            }
        }

        /// <summary>
        /// Tries to read specified length of data from socket data buffer. If buffer doesn't contain data, 
        /// next buffer data block is getted asynchronously and this method is called again.
        /// </summary>
        /// <param name="stream">Stream where to store readed data.</param>
        /// <param name="lengthToRead">Specifies number of bytes to read from socket.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous read operation is completed.</param>
        /// <param name="readedCount">Specifies count of bytes readed.</param>
        private void TryToReadReadSpecifiedLength(Stream stream,
                                                  int lengthToRead,
                                                  object tag,
                                                  SocketCallBack callback,
                                                  int readedCount)
        {
            if (lengthToRead == 0)
            {
                // Data readed ok, call callback.
                if (callback != null)
                {
                    callback(SocketCallBackResult.Ok, readedCount, null, tag);
                }
                return;
            }

            // There is no data in buffer, buffer next block asynchronously.
            if (m_AvailableInBuffer == 0)
            {
                BeginBufferDataBlock(OnBeginReadSpecifiedLengthBufferingCompleted,
                                     new[] {callback, tag, stream, lengthToRead, readedCount});
                return;
            }

            // Buffer has less data than that we need
            int lengthLeftForReading = lengthToRead - readedCount;
            if (lengthLeftForReading > m_AvailableInBuffer)
            {
                stream.Write(m_Buffer, m_OffsetInBuffer, m_AvailableInBuffer);
                stream.Flush();

                readedCount += m_AvailableInBuffer;
                // We used buffer directly, sync buffer info !!!
                m_OffsetInBuffer = 0;
                m_AvailableInBuffer = 0;

                BeginBufferDataBlock(OnBeginReadSpecifiedLengthBufferingCompleted,
                                     new[] {callback, tag, stream, lengthToRead, readedCount});
            }
                // Buffer contains all data we need
            else
            {
                stream.Write(m_Buffer, m_OffsetInBuffer, lengthLeftForReading);
                stream.Flush();

                readedCount += lengthLeftForReading;
                // We used buffer directly, sync buffer info !!!
                m_OffsetInBuffer += lengthLeftForReading;
                m_AvailableInBuffer -= lengthLeftForReading;

                // Logging stuff
                if (m_pLogger != null)
                {
                    if (stream.CanSeek && stream.Length < 200)
                    {
                        byte[] readedData = new byte[stream.Length];
                        stream.Position = 0;
                        stream.Read(readedData, 0, readedData.Length);
                        m_pLogger.AddReadEntry(m_pEncoding.GetString(readedData), lengthToRead);
                    }
                    else
                    {
                        m_pLogger.AddReadEntry("Big binary data, readed " + readedCount + " bytes.",
                                               readedCount);
                    }
                }

                // Data readed ok, call callback.
                if (callback != null)
                {
                    callback(SocketCallBackResult.Ok, readedCount, null, tag);
                }
            }
        }

        /// <summary>
        /// This method is called after asynchronous data buffering is completed.
        /// </summary>
        /// <param name="x">Exception what happened on method execution or null, if operation completed sucessfully.</param>
        /// <param name="tag">User data.</param>
        private void OnBeginReadSpecifiedLengthBufferingCompleted(Exception x, object tag)
        {
            object[] param = (object[]) tag;
            SocketCallBack callback = (SocketCallBack) param[0];
            object callbackTag = param[1];
            Stream stream = (Stream) param[2];
            int lengthToRead = (int) param[3];
            int readedCount = (int) param[4];

            if (x == null)
            {
                // We didn't get data, this can only happen if socket closed.
                if (m_AvailableInBuffer == 0)
                {
                    // Logging stuff
                    if (m_pLogger != null)
                    {
                        m_pLogger.AddTextEntry("Remote host closed socket !");
                    }

                    callback(SocketCallBackResult.SocketClosed, 0, null, callbackTag);
                }
                else
                {
                    TryToReadReadSpecifiedLength(stream, lengthToRead, callbackTag, callback, readedCount);
                }
            }
            else
            {
                callback(SocketCallBackResult.Exception, 0, x, callbackTag);
            }
        }

        /// <summary>
        /// Tries to read period terminated data from socket data buffer. If buffer doesn't contain 
        /// period terminated data,next buffer data block is getted asynchronously and this method is called again.
        /// </summary>
        /// <param name="callback">The method to be called when the asynchronous period terminated read operation is completed.</param>
        /// <param name="tag">User data.</param>
        /// <param name="stream">Stream where to store readed data.</param>
        /// <param name="maxLength">Specifies maximum data legth in bytes.</param>
        /// <param name="readedCount">Specifies count of bytes readed.</param>
        /// <param name="lastByte">Last byte what was readed pevious method call or -1 if first method call.</param>
        /// <param name="lineBreak">Specifies if there is active line break.</param>
        /// <param name="expectCRLF">Specifies if terminating CRLF is expected.</param>
        private void TryToReadPeriodTerminated(SocketCallBack callback,
                                               object tag,
                                               Stream stream,
                                               int maxLength,
                                               int lastByte,
                                               int readedCount,
                                               bool lineBreak,
                                               bool expectCRLF)
        {
            /* When a line of text is received by the server, it checks the line.
               If the line is composed of a single period, it is treated as the 
               end of data indicator.  If the first character is a period and 
               there are other characters on the line, the first character is deleted.
            */

            // There is no data in buffer, buffer next block asynchronously.
            if (m_AvailableInBuffer == 0)
            {
                BeginBufferDataBlock(OnBeginReadPeriodTerminatedBufferingCompleted,
                                     new[]
                                         {
                                             callback, tag, stream, maxLength, lastByte, readedCount, lineBreak,
                                             expectCRLF
                                         });
                return;
            }

            // Delay last byte writing, this is because CR, if next is LF, then last byte isn't written.

            // This is first method call, buffer 1 byte
            if (lastByte == -1)
            {
                lastByte = ReadByte();
                readedCount++;

                // We used last byte, buffer next block asynchronously.
                if (m_AvailableInBuffer == 0)
                {
                    BeginBufferDataBlock(OnBeginReadPeriodTerminatedBufferingCompleted,
                                         new[]
                                             {
                                                 callback, tag, stream, maxLength, lastByte, readedCount,
                                                 lineBreak, expectCRLF
                                             });
                    return;
                }
            }

            byte[] buffer = new byte[8000];
            int positionInBuffer = 0;

            int currentByte = ReadByte();
            readedCount++;
            while (currentByte > -1)
            {
                // We got <CRLF> + 1 char, we must skip that char if it is '.'.
                if (lineBreak)
                {
                    lineBreak = false;

                    // We must skip this char if it is '.'
                    if (currentByte == '.')
                    {
                        expectCRLF = true;

                        // Read next byte
                        if (m_AvailableInBuffer > 0)
                        {
                            currentByte = ReadByte();
                            readedCount++;
                        }
                            // We have use all data in the buffer, buffer next block asynchronously.
                        else
                        {
                            // There is data in buffer, flush it
                            if (positionInBuffer > 0)
                            {
                                stream.Write(buffer, 0, positionInBuffer);
                                positionInBuffer = 0;
                            }

                            BeginBufferDataBlock(OnBeginReadPeriodTerminatedBufferingCompleted,
                                                 new[]
                                                     {
                                                         callback, tag, stream, maxLength, lastByte, readedCount,
                                                         lineBreak, expectCRLF
                                                     });
                            return;
                        }
                    }
                }
                    // We got <CRLF>
                else if (lastByte == (byte) '\r' && currentByte == (byte) '\n')
                {
                    lineBreak = true;

                    // We have <CRLF>.<CRLF>, skip last <CRLF>.
                    if (expectCRLF)
                    {
                        // There is data in buffer, flush it
                        if (positionInBuffer > 0)
                        {
                            stream.Write(buffer, 0, positionInBuffer);
                            positionInBuffer = 0;
                        }

                        // Logging stuff
                        if (m_pLogger != null)
                        {
                            if (stream.CanSeek && stream.Length < 200)
                            {
                                byte[] readedData = new byte[stream.Length];
                                stream.Position = 0;
                                stream.Read(readedData, 0, readedData.Length);
                                m_pLogger.AddReadEntry(m_pEncoding.GetString(readedData), readedCount);
                            }
                            else
                            {
                                m_pLogger.AddReadEntry("Big binary data, readed " + readedCount + " bytes.",
                                                       readedCount);
                            }
                        }

                        // Maximum allowed length exceeded
                        if (readedCount > maxLength)
                        {
                            if (callback != null)
                            {
                                callback(SocketCallBackResult.LengthExceeded,
                                         0,
                                         new ReadException(ReadReplyCode.LengthExceeded,
                                                           "Maximum allowed data length exceeded !"),
                                         tag);
                            }
                            return;
                        }

                        // Data readed ok, call callback.
                        if (callback != null)
                        {
                            callback(SocketCallBackResult.Ok, readedCount, null, tag);
                        }
                        return;
                    }
                }

                // current char isn't CRLF part, so it isn't <CRLF>.<CRLF> terminator.
                if (expectCRLF && !(currentByte == (byte) '\r' || currentByte == (byte) '\n'))
                {
                    expectCRLF = false;
                }

                // Maximum allowed length exceeded, just don't store data.
                if (readedCount < maxLength)
                {
                    // Buffer is filled up, write buffer to stream
                    if (positionInBuffer > (buffer.Length - 2))
                    {
                        stream.Write(buffer, 0, positionInBuffer);
                        positionInBuffer = 0;
                    }

                    buffer[positionInBuffer] = (byte) lastByte;
                    positionInBuffer++;
                }

                // Read next byte
                lastByte = currentByte;
                if (m_AvailableInBuffer > 0)
                {
                    currentByte = ReadByte();
                    readedCount++;
                }
                    // We have use all data in the buffer, buffer next block asynchronously.
                else
                {
                    // There is data in buffer, flush it
                    if (positionInBuffer > 0)
                    {
                        stream.Write(buffer, 0, positionInBuffer);
                        positionInBuffer = 0;
                    }

                    BeginBufferDataBlock(OnBeginReadPeriodTerminatedBufferingCompleted,
                                         new[]
                                             {
                                                 callback, tag, stream, maxLength, lastByte, readedCount,
                                                 lineBreak, expectCRLF
                                             });
                    return;
                }
            }

            // We should never reach here.
            if (callback != null)
            {
                callback(SocketCallBackResult.Exception,
                         0,
                         new Exception(
                             "Never should reach there ! method TryToReadPeriodTerminated out of while loop."),
                         tag);
            }
        }

        /// <summary>
        /// This method is called after asynchronous data buffering is completed.
        /// </summary>
        /// <param name="x">Exception what happened on method execution or null, if operation completed sucessfully.</param>
        /// <param name="tag">User data.</param>
        private void OnBeginReadPeriodTerminatedBufferingCompleted(Exception x, object tag)
        {
            object[] param = (object[]) tag;
            SocketCallBack callback = (SocketCallBack) param[0];
            object callbackTag = param[1];
            Stream stream = (Stream) param[2];
            int maxLength = (int) param[3];
            int lastByte = (int) param[4];
            int readedCount = (int) param[5];
            bool lineBreak = (bool) param[6];
            bool expectCRLF = (bool) param[7];

            if (x == null)
            {
                // We didn't get data, this can only happen if socket closed.
                if (m_AvailableInBuffer == 0)
                {
                    // Logging stuff
                    if (m_pLogger != null)
                    {
                        m_pLogger.AddTextEntry("Remote host closed socket !");
                    }

                    callback(SocketCallBackResult.SocketClosed, 0, null, callbackTag);
                }
                else
                {
                    TryToReadPeriodTerminated(callback,
                                              callbackTag,
                                              stream,
                                              maxLength,
                                              lastByte,
                                              readedCount,
                                              lineBreak,
                                              expectCRLF);
                }
            }
            else
            {
                callback(SocketCallBackResult.Exception, 0, x, callbackTag);
            }
        }

        /// <summary>
        /// Starts sending data block to socket.
        /// </summary>
        /// <param name="stream">Stream which data to write.</param>
        /// <param name="tag">User data.</param>
        /// <param name="callback">The method to be called when the asynchronous write operation is completed</param>
        /// <param name="countSent">Specifies how many data is sent.</param>
        private void BeginProcessingWrite(Stream stream, object tag, SocketCallBack callback, int countSent)
        {
            byte[] buffer = new byte[4000];
            int readedCount = stream.Read(buffer, 0, buffer.Length);
            // There data to send
            if (readedCount > 0)
            {
                countSent += readedCount;
                m_WrittenCount += readedCount;

                if (m_SSL)
                {
                    m_pSslStream.BeginWrite(buffer,
                                            0,
                                            readedCount,
                                            OnBeginWriteCallback,
                                            new[] {stream, tag, callback, countSent});
                }
                else
                {
                    m_pSocketStream.BeginWrite(buffer,
                                               0,
                                               readedCount,
                                               OnBeginWriteCallback,
                                               new[] {stream, tag, callback, countSent});
                }
            }
                // We have sent all data
            else
            {
                // Logging stuff
                if (m_pLogger != null)
                {
                    if (stream.CanSeek && stream.Length < 200)
                    {
                        byte[] sentData = new byte[stream.Length];
                        stream.Position = 0;
                        stream.Read(sentData, 0, sentData.Length);
                        m_pLogger.AddSendEntry(m_pEncoding.GetString(sentData), countSent);
                    }
                    else
                    {
                        m_pLogger.AddSendEntry("Big binary data, sent " + countSent + " bytes.", countSent);
                    }
                }

                // Line sent ok, call callback.
                if (callback != null)
                {
                    callback(SocketCallBackResult.Ok, countSent, null, tag);
                }
            }
        }

        /// <summary>
        /// This method is called after asynchronous datablock send is completed.
        /// </summary>
        /// <param name="ar"></param>
        private void OnBeginWriteCallback(IAsyncResult ar)
        {
            object[] param = (object[]) ar.AsyncState;
            Stream stream = (Stream) param[0];
            object tag = param[1];
            SocketCallBack callBack = (SocketCallBack) param[2];
            int countSent = (int) param[3];

            try
            {
                if (m_SSL)
                {
                    m_pSslStream.EndWrite(ar);
                }
                else
                {
                    m_pSocketStream.EndWrite(ar);
                }

                m_LastActivityDate = DateTime.Now;

                BeginProcessingWrite(stream, tag, callBack, countSent);
            }
            catch (Exception x)
            {
                if (callBack != null)
                {
                    callBack(SocketCallBackResult.Exception, 0, x, tag);
                }
            }
        }

        /// <summary>
        /// This method is called after asynchronous WriteLine is completed.
        /// </summary>
        /// <param name="ar"></param>
        private void OnBeginWriteLineCallback(IAsyncResult ar)
        {
            object[] param = (object[]) ar.AsyncState;
            object tag = param[0];
            SocketCallBack callBack = (SocketCallBack) param[1];
            byte[] lineBytes = (byte[]) param[2];

            try
            {
                if (m_SSL)
                {
                    m_pSslStream.EndWrite(ar);
                }
                else
                {
                    m_pSocketStream.EndWrite(ar);
                }

                m_WrittenCount += lineBytes.Length;
                m_LastActivityDate = DateTime.Now;

                // Logging stuff
                if (m_pLogger != null)
                {
                    if (lineBytes.Length < 200)
                    {
                        m_pLogger.AddSendEntry(m_pEncoding.GetString(lineBytes), lineBytes.Length);
                    }
                    else
                    {
                        m_pLogger.AddSendEntry("Big binary line, sent " + lineBytes.Length + " bytes.",
                                               lineBytes.Length);
                    }
                }

                // Line sent ok, call callback.
                if (callBack != null)
                {
                    callBack(SocketCallBackResult.Ok, lineBytes.Length, null, tag);
                }
            }
            catch (Exception x)
            {
                if (callBack != null)
                {
                    callBack(SocketCallBackResult.Exception, 0, x, tag);
                }
            }
        }

        /// <summary>
        /// Reads data block from state.Stream and begins writing it to socket.
        /// This method is looped while all data has been readed from state.Stream, then sate.Callback is called.
        /// </summary>
        /// <param name="state">State info.</param>        
        private void BeginProcessingWritePeriodTerminated(_BeginWritePeriodTerminated_State state)
        {
            /* Before sending a line of text, check the first character of the line.
               If it is a period, one additional period is inserted at the beginning of the line.
            */

            byte[] buffer = new byte[4000];
            int positionInBuffer = 0;
            int currentByte = state.Stream.ReadByte();
            while (currentByte > -1)
            {
                // We have CRLF, mark it up
                if (state.LastByte == '\r' && currentByte == '\n')
                {
                    state.HasCRLF = true;
                }
                    // There is CRLF + current byte
                else if (state.HasCRLF)
                {
                    // If it is a period, one additional period is inserted at the beginning of the line.
                    if (currentByte == '.')
                    {
                        buffer[positionInBuffer] = (byte) '.';
                        positionInBuffer++;
                    }

                    // CRLF handled, reset it
                    state.HasCRLF = false;
                }

                buffer[positionInBuffer] = (byte) currentByte;
                positionInBuffer++;

                state.LastByte = currentByte;

                // Buffer is filled up, begin writing buffer to socket.
                if (positionInBuffer > (4000 - 10))
                {
                    state.CountSent += positionInBuffer;
                    m_WrittenCount += positionInBuffer;

                    if (m_SSL)
                    {
                        m_pSslStream.BeginWrite(buffer,
                                                0,
                                                positionInBuffer,
                                                OnBeginWritePeriodTerminatedCallback,
                                                state);
                    }
                    else
                    {
                        m_pSocketStream.BeginWrite(buffer,
                                                   0,
                                                   positionInBuffer,
                                                   OnBeginWritePeriodTerminatedCallback,
                                                   state);
                    }
                    return;
                }

                currentByte = state.Stream.ReadByte();
            }

            // We have readed all data, write .<CRLF> or <CRLF>.<CRLF> if data not <CRLF> terminated.
            if (!state.HasCRLF)
            {
                buffer[positionInBuffer] = (byte) '\r';
                positionInBuffer++;
                buffer[positionInBuffer] = (byte) '\n';
                positionInBuffer++;
            }

            buffer[positionInBuffer] = (byte) '.';
            positionInBuffer++;
            buffer[positionInBuffer] = (byte) '\r';
            positionInBuffer++;
            buffer[positionInBuffer] = (byte) '\n';
            positionInBuffer++;

            if (m_SSL)
            {
                m_pSslStream.Write(buffer, 0, positionInBuffer);
            }
            else
            {
                m_pSocketStream.Write(buffer, 0, positionInBuffer);
            }
            state.CountSent += positionInBuffer;
            m_WrittenCount += positionInBuffer;
            m_LastActivityDate = DateTime.Now;
            //-------------------------------------------------------------------------------------//

            // Logging stuff
            if (m_pLogger != null)
            {
                if (state.CountSent < 200)
                {
                    m_pLogger.AddSendEntry(m_pEncoding.GetString(buffer), buffer.Length);
                }
                else
                {
                    m_pLogger.AddSendEntry("Binary data, sent " + state.CountSent + " bytes.", state.CountSent);
                }
            }

            // We don't need stream any more, close it
            if (state.CloseStream)
            {
                try
                {
                    state.Stream.Close();
                }
                catch {}
            }

            // Data sent ok, call callback.
            if (state.Callback != null)
            {
                state.Callback(SocketCallBackResult.Ok, state.CountSent, null, state.Tag);
            }
        }

        /// <summary>
        /// This method is called after asynchronous datablock send is completed.
        /// </summary>
        /// <param name="ar"></param>
        private void OnBeginWritePeriodTerminatedCallback(IAsyncResult ar)
        {
            _BeginWritePeriodTerminated_State state = (_BeginWritePeriodTerminated_State) ar.AsyncState;

            try
            {
                if (m_SSL)
                {
                    m_pSslStream.EndWrite(ar);
                }
                else
                {
                    m_pSocketStream.EndWrite(ar);
                }

                m_LastActivityDate = DateTime.Now;

                BeginProcessingWritePeriodTerminated(state);
            }
            catch (Exception x)
            {
                // We don't need stream any more, close it
                if (state.CloseStream)
                {
                    try
                    {
                        state.Stream.Close();
                    }
                    catch {}
                }

                if (state.Callback != null)
                {
                    state.Callback(SocketCallBackResult.Exception, 0, x, state.Tag);
                }
            }
        }

        /// <summary>
        /// Buffers data from socket if needed. If there is data in buffer, no buffering is done.
        /// </summary>
        private void BufferDataBlock()
        {
            lock (this)
            {
                // There is no data in buffer, buffer next data block
                if (m_AvailableInBuffer == 0)
                {
                    m_OffsetInBuffer = 0;

                    if (m_SSL)
                    {
                        m_AvailableInBuffer = m_pSslStream.Read(m_Buffer, 0, m_Buffer.Length);
                    }
                    else
                    {
                        m_AvailableInBuffer = m_pSocket.Receive(m_Buffer);
                    }
                    m_ReadedCount += m_AvailableInBuffer;
                    m_LastActivityDate = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Start buffering data from socket asynchronously.
        /// </summary>
        /// <param name="callback">The method to be called when the asynchronous data buffering operation is completed.</param>
        /// <param name="tag">User data.</param>
        private void BeginBufferDataBlock(BufferDataBlockCompleted callback, object tag)
        {
            if (m_AvailableInBuffer == 0)
            {
                m_OffsetInBuffer = 0;

                if (m_SSL)
                {
                    m_pSslStream.BeginRead(m_Buffer,
                                           0,
                                           m_Buffer.Length,
                                           OnBeginBufferDataBlockCallback,
                                           new[] {callback, tag});
                }
                else
                {
                    m_pSocket.BeginReceive(m_Buffer,
                                           0,
                                           m_Buffer.Length,
                                           SocketFlags.None,
                                           OnBeginBufferDataBlockCallback,
                                           new[] {callback, tag});
                }
            }
        }

        /// <summary>
        /// This method is called after asynchronous BeginBufferDataBlock is completed.
        /// </summary>
        /// <param name="ar"></param>
        private void OnBeginBufferDataBlockCallback(IAsyncResult ar)
        {
            object[] param = (object[]) ar.AsyncState;
            BufferDataBlockCompleted callback = (BufferDataBlockCompleted) param[0];
            object tag = param[1];

            try
            {
                // Socket closed by this.Disconnect() or closed by remote host.
                if (m_pSocket == null || !m_pSocket.Connected)
                {
                    m_AvailableInBuffer = 0;
                }
                else
                {
                    if (m_SSL)
                    {
                        m_AvailableInBuffer = m_pSslStream.EndRead(ar);
                    }
                    else
                    {
                        m_AvailableInBuffer = m_pSocket.EndReceive(ar);
                    }
                }
                m_ReadedCount += m_AvailableInBuffer;
                m_LastActivityDate = DateTime.Now;

                if (callback != null)
                {
                    callback(null, tag);
                }
            }
            catch (Exception x)
            {
                if (callback != null)
                {
                    callback(x, tag);
                }
            }
        }

        #endregion
    }
}