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

namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for ResponseReceived events.
    /// </summary>
    public class SIP_ResponseReceivedEventArgs : EventArgs
    {
        #region Members

        private readonly SIP_Response m_pResponse;
        private readonly SIP_Stack m_pStack;
        private readonly SIP_ClientTransaction m_pTransaction;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="transaction">Client transaction what response it is. This value can be null if no matching client response.</param>
        /// <param name="response">Received response.</param>
        internal SIP_ResponseReceivedEventArgs(SIP_Stack stack,
                                               SIP_ClientTransaction transaction,
                                               SIP_Response response)
        {
            m_pStack = stack;
            m_pResponse = response;
            m_pTransaction = transaction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets client transaction which response it is. This value is null if no matching client transaction.
        /// If this core is staless proxy then it's allowed, otherwise core MUST discard that response.
        /// </summary>
        public SIP_ClientTransaction ClientTransaction
        {
            get { return m_pTransaction; }
        }

        /// <summary>
        /// Gets SIP dialog where Response belongs to. Returns null if Response doesn't belong any dialog.
        /// </summary>
        public SIP_Dialog Dialog
        {
            get { return m_pStack.TransactionLayer.MatchDialog(m_pResponse); }
        }

        /// <summary>
        /// Gets response received by SIP stack.
        /// </summary>
        public SIP_Response Response
        {
            get { return m_pResponse; }
        }

        #endregion
    }
}