/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.Log
{
    #region usings

    using System;
    using System.Net;
    using System.Security.Principal;

    #endregion

    /// <summary>
    /// Implements log entry.
    /// </summary>
    public class LogEntry
    {
        #region Members

        private readonly string m_ID = "";
        private readonly byte[] m_pData;
        private readonly Exception m_pException;
        private readonly IPEndPoint m_pLocalEP;
        private readonly IPEndPoint m_pRemoteEP;
        private readonly GenericIdentity m_pUserIdentity;
        private readonly long m_Size;
        private readonly string m_Text = "";
        private readonly DateTime m_Time;
        private readonly LogEntryType m_Type = LogEntryType.Text;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Log entry type.</param>
        /// <param name="id">Log entry ID.</param>
        /// <param name="size">Specified how much data was readed or written.</param>
        /// <param name="text">Description text.</param>
        public LogEntry(LogEntryType type, string id, long size, string text)
        {
            m_Type = type;
            m_ID = id;
            m_Size = size;
            m_Text = text;

            m_Time = DateTime.Now;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Log entry type.</param>
        /// <param name="id">Log entry ID.</param>
        /// <param name="userIdentity">Log entry owner user or null if none.</param>
        /// <param name="size">Log entry read/write size in bytes.</param>
        /// <param name="text">Log text.</param>
        /// <param name="localEP">Local IP end point.</param>
        /// <param name="remoteEP">Remote IP end point.</param>
        /// <param name="data">Log data.</param>
        public LogEntry(LogEntryType type,
                        string id,
                        GenericIdentity userIdentity,
                        long size,
                        string text,
                        IPEndPoint localEP,
                        IPEndPoint remoteEP,
                        byte[] data)
        {
            m_Type = type;
            m_ID = id;
            m_pUserIdentity = userIdentity;
            m_Size = size;
            m_Text = text;
            m_pLocalEP = localEP;
            m_pRemoteEP = remoteEP;
            m_pData = data;

            m_Time = DateTime.Now;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Log entry type.</param>
        /// <param name="id">Log entry ID.</param>
        /// <param name="userIdentity">Log entry owner user or null if none.</param>
        /// <param name="size">Log entry read/write size in bytes.</param>
        /// <param name="text">Log text.</param>
        /// <param name="localEP">Local IP end point.</param>
        /// <param name="remoteEP">Remote IP end point.</param>
        /// <param name="exception">Exception happened. Can be null.</param>
        public LogEntry(LogEntryType type,
                        string id,
                        GenericIdentity userIdentity,
                        long size,
                        string text,
                        IPEndPoint localEP,
                        IPEndPoint remoteEP,
                        Exception exception)
        {
            m_Type = type;
            m_ID = id;
            m_pUserIdentity = userIdentity;
            m_Size = size;
            m_Text = text;
            m_pLocalEP = localEP;
            m_pRemoteEP = remoteEP;
            m_pException = exception;

            m_Time = DateTime.Now;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gest log data. Value null means no log data.
        /// </summary>
        public byte[] Data
        {
            get { return m_pData; }
        }

        /// <summary>
        /// Gets log entry type.
        /// </summary>
        public LogEntryType EntryType
        {
            get { return m_Type; }
        }

        /// <summary>
        /// Gets exception happened. This property is available only if LogEntryType.Exception.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        /// <summary>
        /// Gets log entry ID.
        /// </summary>
        public string ID
        {
            get { return m_ID; }
        }

        /// <summary>
        /// Gets local IP end point. Value null means no local end point.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return m_pLocalEP; }
        }

        /// <summary>
        /// Gets remote IP end point. Value null means no remote end point.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEP; }
        }

        /// <summary>
        /// Gets how much data was readed or written, depends on <b>LogEntryType</b>.
        /// </summary>
        public long Size
        {
            get { return m_Size; }
        }

        /// <summary>
        /// Gets describing text.
        /// </summary>
        public string Text
        {
            get { return m_Text; }
        }

        /// <summary>
        /// Gets time when log entry was created.
        /// </summary>
        public DateTime Time
        {
            get { return m_Time; }
        }

        /// <summary>
        /// Gets log entry related user identity.
        /// </summary>
        public GenericIdentity UserIdentity
        {
            get { return m_pUserIdentity; }
        }

        #endregion
    }
}