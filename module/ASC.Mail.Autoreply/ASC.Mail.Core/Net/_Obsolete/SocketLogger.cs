/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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