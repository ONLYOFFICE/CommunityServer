/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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