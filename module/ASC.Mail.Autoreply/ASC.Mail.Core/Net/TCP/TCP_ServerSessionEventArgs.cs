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