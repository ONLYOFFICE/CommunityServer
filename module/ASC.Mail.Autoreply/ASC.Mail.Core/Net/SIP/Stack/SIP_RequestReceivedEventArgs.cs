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
    /// This class provides data for <b>SIP_Dialog.RequestReceived</b> event and <b>SIP_Core.OnRequestReceived</b>> method.
    /// </summary>
    public class SIP_RequestReceivedEventArgs : EventArgs
    {
        #region Members

        private readonly SIP_Dialog m_pDialog;
        private readonly SIP_Flow m_pFlow;
        private readonly SIP_Request m_pRequest;
        private readonly SIP_Stack m_pStack;
        private SIP_ServerTransaction m_pTransaction;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="flow">SIP data flow.</param>
        /// <param name="request">Recieved request.</param>
        internal SIP_RequestReceivedEventArgs(SIP_Stack stack, SIP_Flow flow, SIP_Request request)
            : this(stack, flow, request, null, null) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="flow">SIP data flow.</param>
        /// <param name="request">Recieved request.</param>
        /// <param name="dialog">SIP dialog which received request.</param>
        /// <param name="transaction">SIP server transaction which must be used to send response back to request maker.</param>
        internal SIP_RequestReceivedEventArgs(SIP_Stack stack,
                                              SIP_Flow flow,
                                              SIP_Request request,
                                              SIP_Dialog dialog,
                                              SIP_ServerTransaction transaction)
        {
            m_pStack = stack;
            m_pFlow = flow;
            m_pRequest = request;
            m_pDialog = dialog;
            m_pTransaction = transaction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets SIP dialog where Request belongs to. Returns null if Request doesn't belong any dialog.
        /// </summary>
        public SIP_Dialog Dialog
        {
            get { return m_pDialog; }
        }

        /// <summary>
        /// Gets request received by SIP stack.
        /// </summary>
        public SIP_Request Request
        {
            get { return m_pRequest; }
        }

        /// <summary>
        /// Gets server transaction for that request. Server transaction is created when this property is 
        /// first accessed. If you don't need server transaction for that request, for example statless proxy, 
        /// just don't access this property. For ACK method, this method always return null, because ACK 
        /// doesn't create transaction !
        /// </summary>
        public SIP_ServerTransaction ServerTransaction
        {
            get
            {
                // ACK never creates transaction.
                if (m_pRequest.RequestLine.Method == SIP_Methods.ACK)
                {
                    return null;
                }

                // Create server transaction for that request.
                if (m_pTransaction == null)
                {
                    m_pTransaction = m_pStack.TransactionLayer.EnsureServerTransaction(m_pFlow, m_pRequest);
                }

                return m_pTransaction;
            }
        }

        #endregion
    }
}