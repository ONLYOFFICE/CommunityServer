/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using ASC.Xmpp.Core.utils.Xml;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Utils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace ASC.Xmpp.Server.Gateway
{
    class TcpXmppConnection : IXmppConnection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TcpXmppConnection));

        private readonly StreamParser streamParser;
        private readonly byte[] buffer;

        private readonly long maxPacket;
        private long packetSize;

        private int streamEndFired;
        private int closed;

        private readonly EndPoint remoteEndPoint;

        private readonly ConcurrentQueue<Tuple<byte[], Node>> sendingQueue = new ConcurrentQueue<Tuple<byte[], Node>>();

        private int sending;

        protected Stream sendStream;

        protected Stream receiveStream;

        public string Id
        {
            get;
            private set;
        }

        public bool TlsStarted
        {
            get
            {
                return receiveStream is SslStream;
            }
        }

        public TcpXmppConnection(Socket socket, long maxPacket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            Id = UniqueId.CreateNewId();

            streamParser = new StreamParser();
            streamParser.Reset();
            streamParser.OnStreamStart += StreamParserOnStreamStart;
            streamParser.OnStreamElement += StreamParserOnStreamElement;
            streamParser.OnStreamEnd += StreamParserOnStreamEnd;
            streamParser.OnStreamError += StreamParserOnStreamError;
            streamParser.OnError += StreamParserOnError;

            buffer = new byte[socket.ReceiveBufferSize];
            remoteEndPoint = socket.RemoteEndPoint;

            sendStream = receiveStream = new NetworkStream(socket, true);
            this.maxPacket = maxPacket;

            log.DebugFormat("Create new connection {0} with {1}", Id, remoteEndPoint);
        }

        public void StartTls(X509Certificate2 certificate)
        {
            receiveStream.Flush();
            sendStream = receiveStream = new SslStream(receiveStream, false);
            ((SslStream)receiveStream).AuthenticateAsServer(certificate, false,
                SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, false);
            Reset();
        }

        public void Reset()
        {
            streamParser.Reset();
        }

        public void Close()
        {
            if (Interlocked.CompareExchange(ref closed, 1, 0) == 0)
            {
                try
                {
                    OnStreamEnd();
                }
                catch (Exception error)
                {
                    LogError(error, "Close");
                }
                try
                {
                    var handler = Closed;
                    if (handler != null) handler(this, new XmppConnectionCloseEventArgs());
                }
                catch (Exception error)
                {
                    LogError(error, "Close");
                }

                try
                {
                    sendStream.Close();
                    sendStream = null;
                    receiveStream.Close();
                    receiveStream = null;
                }
                catch (Exception error)
                {
                    LogError(error, "Close");
                }

                log.DebugFormat("Close connection {0} with {1}", Id, remoteEndPoint);
            }
        }

        public void Send(Node node, Encoding encoding)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (encoding == null) throw new ArgumentNullException("encoding");

            Send(encoding.GetBytes(node.ToString(encoding)), node);
        }

        public void Send(string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            if (encoding == null) throw new ArgumentNullException("encoding");

            Send(encoding.GetBytes(text), null);
        }

        public void BeginReceive()
        {
            try
            {
                if (receiveStream != null && receiveStream.CanRead)
                {
                    receiveStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, receiveStream);
                }
            }
            catch (ObjectDisposedException) { }
        }

        public event EventHandler<XmppStreamStartEventArgs> XmppStreamStart;

        public event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd;

        public event EventHandler<XmppStreamEventArgs> XmppStreamElement;

        public event EventHandler<XmppConnectionCloseEventArgs> Closed;
        
        private void ReadCallback(IAsyncResult asyncResult)
        {
            try
            {
                var stream = (Stream)asyncResult.AsyncState;
                int readed = stream.EndRead(asyncResult);
                
                if (0 < readed)
                {
                    streamParser.Push(buffer, 0, readed);

                    BeginReceive();

                    packetSize += readed;
                    if (packetSize > maxPacket)
                    {
                        throw new ArgumentException("request-too-large");
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                LogErrorAndCloseConnection(e, "ReadCallback");
            }
        }

        private void Send(byte[] buffer, Node node)
        {
            if (buffer == null)
            {
                return;
            }

            sendingQueue.Enqueue(Tuple.Create(buffer, node));
            if (sendStream != null)
            {
                Send(sendStream);
            }
        }

        private void Send(Stream stream)
        {
            if (Interlocked.CompareExchange(ref sending, 1, 0) == 0)
            {
                Tuple<byte[], Node> item;
                try
                {
                    if (sendingQueue.TryDequeue(out item))
                    {
                        stream.BeginWrite(item.Item1, 0, item.Item1.Length, WriteCallback, stream);
                    }
                    else
                    {
                        Volatile.Write(ref sending, 0);
                    }
                }
                catch (Exception e)
                {
                    Volatile.Write(ref sending, 0);
                    LogErrorAndCloseConnection(e, "Send");
                }
            }
        }

        private void WriteCallback(IAsyncResult asyncResult)
        {
            var stream = (Stream)asyncResult.AsyncState;
            try
            {
                stream.EndWrite(asyncResult);
            }
            catch (Exception e)
            {
                LogErrorAndCloseConnection(e, "WriteCallback");
            }
            finally
            {
                Volatile.Write(ref sending, 0);
            }
            Send(stream);
        }

        private void StreamParserOnStreamStart(object sender, Node e, string streamNamespace)
        {
            packetSize = 0;
            var handler = XmppStreamStart;
            if (handler != null) handler(this, new XmppStreamStartEventArgs(Id, e, streamNamespace));
        }

        private void StreamParserOnStreamElement(object sender, Node e)
        {
            packetSize = 0;
            var handler = XmppStreamElement;
            if (handler != null) handler(this, new XmppStreamEventArgs(Id, e));
        }

        private void StreamParserOnStreamEnd(object sender, Node e)
        {
            packetSize = 0;
            OnStreamEnd();
        }

        private void StreamParserOnStreamError(object sender, Exception ex)
        {
            var streamParser = (StreamParser)sender;
            log.ErrorFormat("StreamParserOnStreamError {0}, streamParser.Current = {1}", ex, streamParser.Current);
        }

        private void StreamParserOnError(object sender, Exception ex)
        {
            var streamParser = (StreamParser)sender;
            log.ErrorFormat("StreamParserOnError {0}, streamParser.Current = {1}", ex, streamParser.Current);
        }

        private void OnStreamEnd()
        {
            if (Interlocked.CompareExchange(ref streamEndFired, 1, 0) == 0)
            {
                var handler = XmppStreamEnd;
                if (handler != null)
                {
                    List<Node> notSended = new List<Node>();
                    Tuple<byte[], Node> tuple;
                    while (!sendingQueue.IsEmpty)
                    {
                        if (sendingQueue.TryDequeue(out tuple))
                        {
                            if (tuple.Item2 != null)
                            {
                                notSended.Add(tuple.Item2);
                            }
                        }
                    }
                    handler(this, new XmppStreamEndEventArgs(Id, notSended));
                }
            }
        }

        private void LogErrorAndCloseConnection(Exception error, string method)
        {
            LogError(error, method);
            Close();
        }

        private void LogError(Exception error, string method)
        {
            if (error is ObjectDisposedException ||
                error.InnerException is ObjectDisposedException ||
                error is IOException)
            {
                //ignore
            }
            else
            {
                log.ErrorFormat("Error {0} connection {1} with {2}: {3}", method, Id, remoteEndPoint, error);
            }
        }
    }
}