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


using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Stack
{
    /// <summary>
    /// This class is responsible for sending <b>request</b> to one of the <b>hops</b>.
    /// Hops are tried one by one until a server is contacted.
    /// </summary>
    public class SIP_RequestSender
    {
        private bool                  m_IsStarted    = false;
        private SIP_Stack             m_pStack       = null;
        private SIP_Request           m_pRequest     = null;
        private Queue<SIP_Hop>        m_pHops        = null;
        private int                   m_Timeout      = 15;
        private SIP_ClientTransaction m_pTransaction = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">SIP stack.</param>
        /// <param name="request">SIP request.</param>
        /// <param name="hops">Priority ordered "hops" where to send request.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b>,<b>request</b> or <b>hops</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public SIP_RequestSender(SIP_Stack stack,SIP_Request request,Queue<SIP_Hop> hops)
        {
            if(stack == null){
                throw new ArgumentNullException("stack");
            }
            if(request == null){
                throw new ArgumentNullException("request");
            }
            if(hops == null){
                throw new ArgumentNullException("hops");
            }
            if(hops.Count == 0){
                throw new ArgumentException("There must be at least 1 hop in 'hops' queue.");
            }
            
            m_pStack   = stack;
            m_pRequest = request;
            m_pHops    = hops;
        }


        #region method Start

        /// <summary>
        /// Starts sending request.
        /// </summary>
        public void Start()
        {
            if(m_IsStarted){
                throw new InvalidOperationException("Request sending is already started.");
            }
            m_IsStarted = true;

            // RFC 3261 8.1.2 and . 16.6. We need to create new client transaction for each attempt.
        }

        #endregion

        #region method Cancel

        /// <summary>
        /// Cancels request sending.
        /// </summary>
        public void Cancel()
        {
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets SIP request what this sender is responsible for.
        /// </summary>
        public SIP_Request Request
        {
            get{ return m_pRequest; }
        }

        #endregion

        #region Events handling

        // public event EventHandler NewTransaction
        // public event EventHandler 

        #endregion

    }
}
