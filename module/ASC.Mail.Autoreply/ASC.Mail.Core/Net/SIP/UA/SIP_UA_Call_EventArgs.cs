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


namespace ASC.Mail.Net.SIP.UA
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for <b>SIP_UA.IncomingCall</b> event.
    /// </summary>
    public class SIP_UA_Call_EventArgs : EventArgs
    {
        #region Members

        private readonly SIP_UA_Call m_pCall;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="call">SIP UA call.</param>
        /// <exception cref="ArgumentNullException">Is called when <b>call</b> is null reference.</exception>
        public SIP_UA_Call_EventArgs(SIP_UA_Call call)
        {
            if (call == null)
            {
                throw new ArgumentNullException("call");
            }

            m_pCall = call;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets call.
        /// </summary>
        public SIP_UA_Call Call
        {
            get { return m_pCall; }
        }

        #endregion
    }
}