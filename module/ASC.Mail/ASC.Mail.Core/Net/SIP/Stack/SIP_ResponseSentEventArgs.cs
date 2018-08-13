/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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