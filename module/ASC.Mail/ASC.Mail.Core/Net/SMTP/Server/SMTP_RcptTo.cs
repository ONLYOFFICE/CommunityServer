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

namespace ASC.Mail.Net.SMTP.Server
{
    #region usings

    using System;

    #endregion

    /// <summary>
    ///  This class holds RCPT TO: command value.
    /// </summary>
    public class SMTP_RcptTo
    {
        #region Members

        private readonly string m_Mailbox = "";
        private readonly SMTP_Notify m_Notify = SMTP_Notify.NotSpecified;
        private readonly string m_ORCPT = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mailbox">Mailbox value.</param>
        /// <param name="notify">DSN NOTIFY parameter value.</param>
        /// <param name="orcpt">DSN ORCPT parameter value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>mailbox</b> is null reference.</exception>
        public SMTP_RcptTo(string mailbox, SMTP_Notify notify, string orcpt)
        {
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox");
            }

            m_Mailbox = mailbox;
            m_Notify = notify;
            m_ORCPT = orcpt;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets SMTP "mailbox" value. Actually this is just email address.
        /// </summary>
        public string Mailbox
        {
            get { return m_Mailbox; }
        }

        /// <summary>
        /// Gets DSN NOTIFY parameter value.
        /// This value specified when SMTP server should send delivery status notification.
        /// Defined in RFC 1891.
        /// </summary>
        public SMTP_Notify Notify
        {
            get { return m_Notify; }
        }

        /// <summary>
        /// Gets DSN ORCPT parameter value. Value null means not specified.
        /// This value specifies "original" recipient address where message is sent (has point only when message forwarded).
        /// Defined in RFC 1891.
        /// </summary>
        public string ORCPT
        {
            get { return m_ORCPT; }
        }

        #endregion
    }
}