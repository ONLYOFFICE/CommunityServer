/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.Log
{
    #region usings

    using System;
    using System.Net;
    using System.Security.Principal;

    #endregion

    /// <summary>
    /// General logging module.
    /// </summary>
    public class Logger : IDisposable
    {
        #region Events

        /// <summary>
        /// Is raised when new log entry is available.
        /// </summary>
        public event EventHandler<WriteLogEventArgs> WriteLog = null;

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose() {}

        /// <summary>
        /// Adds read log entry.
        /// </summary>
        /// <param name="size">Readed data size in bytes.</param>
        /// <param name="text">Log text.</param>
        public void AddRead(long size, string text)
        {
            OnWriteLog(new LogEntry(LogEntryType.Read, "", size, text));
        }

        /// <summary>
        /// Adds read log entry.
        /// </summary>
        /// <param name="id">Log entry ID.</param>
        /// <param name="size">Readed data size in bytes.</param>
        /// <param name="text">Log text.</param>
        /// <param name="userIdentity">Authenticated user identity.</param>
        /// <param name="localEP">Local IP endpoint.</param>
        /// <param name="remoteEP">Remote IP endpoint.</param>
        public void AddRead(string id,
                            GenericIdentity userIdentity,
                            long size,
                            string text,
                            IPEndPoint localEP,
                            IPEndPoint remoteEP)
        {
            OnWriteLog(new LogEntry(LogEntryType.Read,
                                    id,
                                    userIdentity,
                                    size,
                                    text,
                                    localEP,
                                    remoteEP,
                                    (byte[]) null));
        }

        /// <summary>
        /// Adds read log entry.
        /// </summary>
        /// <param name="id">Log entry ID.</param>
        /// <param name="size">Readed data size in bytes.</param>
        /// <param name="text">Log text.</param>
        /// <param name="userIdentity">Authenticated user identity.</param>
        /// <param name="localEP">Local IP endpoint.</param>
        /// <param name="remoteEP">Remote IP endpoint.</param>
        /// <param name="data">Log data.</param>
        public void AddRead(string id,
                            GenericIdentity userIdentity,
                            long size,
                            string text,
                            IPEndPoint localEP,
                            IPEndPoint remoteEP,
                            byte[] data)
        {
            OnWriteLog(new LogEntry(LogEntryType.Read, id, userIdentity, size, text, localEP, remoteEP, data));
        }

        /// <summary>
        /// Add write log entry.
        /// </summary>
        /// <param name="size">Written data size in bytes.</param>
        /// <param name="text">Log text.</param>
        public void AddWrite(long size, string text)
        {
            OnWriteLog(new LogEntry(LogEntryType.Write, "", size, text));
        }

        /// <summary>
        /// Add write log entry.
        /// </summary>
        /// <param name="id">Log entry ID.</param>
        /// <param name="size">Written data size in bytes.</param>
        /// <param name="text">Log text.</param>
        /// <param name="userIdentity">Authenticated user identity.</param>
        /// <param name="localEP">Local IP endpoint.</param>
        /// <param name="remoteEP">Remote IP endpoint.</param>
        public void AddWrite(string id,
                             GenericIdentity userIdentity,
                             long size,
                             string text,
                             IPEndPoint localEP,
                             IPEndPoint remoteEP)
        {
            OnWriteLog(new LogEntry(LogEntryType.Write,
                                    id,
                                    userIdentity,
                                    size,
                                    text,
                                    localEP,
                                    remoteEP,
                                    (byte[]) null));
        }

        /// <summary>
        /// Add write log entry.
        /// </summary>
        /// <param name="id">Log entry ID.</param>
        /// <param name="size">Written data size in bytes.</param>
        /// <param name="text">Log text.</param>
        /// <param name="userIdentity">Authenticated user identity.</param>
        /// <param name="localEP">Local IP endpoint.</param>
        /// <param name="remoteEP">Remote IP endpoint.</param>
        /// <param name="data">Log data.</param>
        public void AddWrite(string id,
                             GenericIdentity userIdentity,
                             long size,
                             string text,
                             IPEndPoint localEP,
                             IPEndPoint remoteEP,
                             byte[] data)
        {
            OnWriteLog(new LogEntry(LogEntryType.Write, id, userIdentity, size, text, localEP, remoteEP, data));
        }

        /// <summary>
        /// Adds text entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        public void AddText(string text)
        {
            OnWriteLog(new LogEntry(LogEntryType.Text, "", 0, text));
        }

        /// <summary>
        /// Adds text entry.
        /// </summary>
        /// <param name="id">Log entry ID.</param>
        /// <param name="text">Log text.</param>
        public void AddText(string id, string text)
        {
            OnWriteLog(new LogEntry(LogEntryType.Text, id, 0, text));
        }

        /// <summary>
        /// Adds text entry.
        /// </summary>
        /// <param name="id">Log entry ID.</param>
        /// <param name="text">Log text.</param>
        /// <param name="userIdentity">Authenticated user identity.</param>
        /// <param name="localEP">Local IP endpoint.</param>
        /// <param name="remoteEP">Remote IP endpoint.</param>
        public void AddText(string id,
                            GenericIdentity userIdentity,
                            string text,
                            IPEndPoint localEP,
                            IPEndPoint remoteEP)
        {
            OnWriteLog(new LogEntry(LogEntryType.Read,
                                    id,
                                    userIdentity,
                                    0,
                                    text,
                                    localEP,
                                    remoteEP,
                                    (byte[]) null));
        }

        /// <summary>
        /// Adds exception entry.
        /// </summary>
        /// <param name="id">Log entry ID.</param>
        /// <param name="text">Log text.</param>
        /// <param name="userIdentity">Authenticated user identity.</param>
        /// <param name="localEP">Local IP endpoint.</param>
        /// <param name="remoteEP">Remote IP endpoint.</param>
        /// <param name="exception">Exception happened.</param>
        public void AddException(string id,
                                 GenericIdentity userIdentity,
                                 string text,
                                 IPEndPoint localEP,
                                 IPEndPoint remoteEP,
                                 Exception exception)
        {
            OnWriteLog(new LogEntry(LogEntryType.Exception,
                                    id,
                                    userIdentity,
                                    0,
                                    text,
                                    localEP,
                                    remoteEP,
                                    exception));
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises WriteLog event.
        /// </summary>
        /// <param name="entry">Log entry.</param>
        private void OnWriteLog(LogEntry entry)
        {
            if (WriteLog != null)
            {
                WriteLog(this, new WriteLogEventArgs(entry));
            }
        }

        #endregion
    }
}