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
