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


namespace ASC.Mail.Net.SMTP.Relay
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for <b>Relay_Server.SessionCompleted</b> event.
    /// </summary>
    public class Relay_SessionCompletedEventArgs
    {
        #region Members

        private readonly Exception m_pException;
        private readonly Relay_Session m_pSession;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Relay session what completed processing.</param>
        /// <param name="exception">Exception what happened or null if relay completed successfully.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>session</b> is null.</exception>
        public Relay_SessionCompletedEventArgs(Relay_Session session, Exception exception)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            m_pSession = session;
            m_pException = exception;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets Exception what happened or null if relay completed successfully.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        /// <summary>
        /// Gets relay session what completed processing.
        /// </summary>
        public Relay_Session Session
        {
            get { return m_pSession; }
        }

        #endregion
    }
}