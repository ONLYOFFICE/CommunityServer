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
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;

    #endregion

    /// <summary>
    /// Socket logger.
    /// </summary>
    public class SocketLogger
    {
#if (DEBUG)
        private const int logDumpCount = 1;
#else
        private const int logDumpCount = 100;
#endif

        #region Members

        private readonly List<SocketLogEntry> m_pEntries;
        private readonly LogEventHandler m_pLogHandler;
        private readonly Socket m_pSocket;
        private bool m_FirstLogPart = true;
        private IPEndPoint m_pLoaclEndPoint;
        private IPEndPoint m_pRemoteEndPoint;
        private string m_SessionID = "";
        private string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets session ID.
        /// </summary>
        public string SessionID
        {
            get { return m_SessionID; }

            set { m_SessionID = value; }
        }

        /// <summary>
        /// Gets or sets authenticated user name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }

            set { m_UserName = value; }
        }

        /// <summary>
        /// Gets current cached log entries.
        /// </summary>
        public SocketLogEntry[] LogEntries
        {
            get { return m_pEntries.ToArray(); }
        }

        /// <summary>
        /// Gets local endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return m_pLoaclEndPoint; }
        }

        /// <summary>
        /// Gets remote endpoint.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEndPoint; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="logHandler"></param>
        public SocketLogger(Socket socket, LogEventHandler logHandler)
        {
            m_pSocket = socket;
            m_pLogHandler = logHandler;

            m_pEntries = new List<SocketLogEntry>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts log entries to string.
        /// </summary>
        /// <param name="logger">Socket logger.</param>
        /// <param name="firstLogPart">Specifies if first log part of multipart log.</param>
        /// <param name="lastLogPart">Specifies if last log part (logging ended).</param>
        /// <returns></returns>
        public static string LogEntriesToString(SocketLogger logger, bool firstLogPart, bool lastLogPart)
        {
            string logText = "//----- Sys: 'Session:'" + logger.SessionID + " added " + DateTime.Now + "\r\n";
            if (!firstLogPart)
            {
                logText = "//----- Sys: 'Session:'" + logger.SessionID + " partial log continues " +
                          DateTime.Now + "\r\n";
            }

            foreach (SocketLogEntry entry in logger.LogEntries)
            {
                if (entry.Type == SocketLogEntryType.ReadFromRemoteEP)
                {
                    logText += CreateEntry(logger, entry.Text, ">>>");
                }
                else if (entry.Type == SocketLogEntryType.SendToRemoteEP)
                {
                    logText += CreateEntry(logger, entry.Text, "<<<");
                }
                else
                {
                    logText += CreateEntry(logger, entry.Text, "---");
                }
            }

            if (lastLogPart)
            {
                logText += "//----- Sys: 'Session:'" + logger.SessionID + " removed " + DateTime.Now + "\r\n";
            }
            else
            {
                logText += "//----- Sys: 'Session:'" + logger.SessionID + " partial log " + DateTime.Now +
                           "\r\n";
            }

            return logText;
        }

        /// <summary>
        /// Adds data read(from remoteEndpoint) entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="size">Readed text size.</param>
        public void AddReadEntry(string text, long size)
        {
            if (m_pLoaclEndPoint == null || m_pRemoteEndPoint == null)
            {
                m_pLoaclEndPoint = (IPEndPoint) m_pSocket.LocalEndPoint;
                m_pRemoteEndPoint = (IPEndPoint) m_pSocket.RemoteEndPoint;
            }

            m_pEntries.Add(new SocketLogEntry(text, size, SocketLogEntryType.ReadFromRemoteEP));

            OnEntryAdded();
        }

        /// <summary>
        /// Adds data send(to remoteEndpoint) entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="size">Sent text size.</param>
        public void AddSendEntry(string text, long size)
        {
            if (m_pLoaclEndPoint == null || m_pRemoteEndPoint == null)
            {
                m_pLoaclEndPoint = (IPEndPoint) m_pSocket.LocalEndPoint;
                m_pRemoteEndPoint = (IPEndPoint) m_pSocket.RemoteEndPoint;
            }

            m_pEntries.Add(new SocketLogEntry(text, size, SocketLogEntryType.SendToRemoteEP));

            OnEntryAdded();
        }

        /// <summary>
        /// Adds free text entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        public void AddTextEntry(string text)
        {
            m_pEntries.Add(new SocketLogEntry(text, 0, SocketLogEntryType.FreeText));

            OnEntryAdded();
        }

        /// <summary>
        /// Requests to write all in memory log entries to log log file.
        /// </summary>
        public void Flush()
        {
            if (m_pLogHandler != null)
            {
                m_pLogHandler(this, new Log_EventArgs(this, m_FirstLogPart, true));
            }
        }

        #endregion

        #region Utility methods

        private static string CreateEntry(SocketLogger logger, string text, string prefix)
        {
            string retVal = "";

            if (text.EndsWith("\r\n"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            string remIP = "xxx.xxx.xxx.xxx";
            try
            {
                if (logger.RemoteEndPoint != null)
                {
                    remIP = (logger.RemoteEndPoint).Address.ToString();
                }
            }
            catch {}

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            foreach (string line in lines)
            {
                retVal += "SessionID: " + logger.SessionID + "  RemIP: " + remIP + "  " + prefix + "  '" +
                          line + "'\r\n";
            }

            return retVal;
        }

        /// <summary>
        /// This method is called when new loge entry has added.
        /// </summary>
        private void OnEntryAdded()
        {
            // Ask to server to write partial log
            if (m_pEntries.Count > logDumpCount)
            {
                if (m_pLogHandler != null)
                {
                    m_pLogHandler(this, new Log_EventArgs(this, m_FirstLogPart, false));
                }

                m_pEntries.Clear();
                m_FirstLogPart = false;
            }
        }

        #endregion
    }
}