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


namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for <b>SIP_ServerTransaction.ResponseSent</b> method.
    /// </summary>
    public class SIP_ResponseSentEventArgs : EventArgs
    {
        #region Members

        private readonly SIP_Response m_pResponse;
        private readonly SIP_ServerTransaction m_pTransaction;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transaction">Server transaction.</param>
        /// <param name="response">SIP response.</param>
        /// <exception cref="ArgumentNullException">Is raised when any of the arguments is null.</exception>
        public SIP_ResponseSentEventArgs(SIP_ServerTransaction transaction, SIP_Response response)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            m_pTransaction = transaction;
            m_pResponse = response;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets response which was sent.
        /// </summary>
        public SIP_Response Response
        {
            get { return m_pResponse; }
        }

        /// <summary>
        /// Gets server transaction which sent response.
        /// </summary>
        public SIP_ServerTransaction ServerTransaction
        {
            get { return m_pTransaction; }
        }

        #endregion
    }
}