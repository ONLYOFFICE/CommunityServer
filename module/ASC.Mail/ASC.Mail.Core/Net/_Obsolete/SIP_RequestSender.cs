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
