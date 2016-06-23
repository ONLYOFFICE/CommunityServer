/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


namespace ASC.Mail.Net.TCP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data to .... .
    /// </summary>
    public class TCP_ServerSessionEventArgs<T> : EventArgs where T : TCP_ServerSession, new()
    {
        #region Members

        private readonly TCP_Server<T> m_pServer;
        private readonly T m_pSession;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="server">TCP server.</param>
        /// <param name="session">TCP server session.</param>
        internal TCP_ServerSessionEventArgs(TCP_Server<T> server, T session)
        {
            m_pServer = server;
            m_pSession = session;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets TCP server.
        /// </summary>
        public TCP_Server<T> Server
        {
            get { return m_pServer; }
        }

        /// <summary>
        /// Gets TCP server session.
        /// </summary>
        public T Session
        {
            get { return m_pSession; }
        }

        #endregion
    }
}