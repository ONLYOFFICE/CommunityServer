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