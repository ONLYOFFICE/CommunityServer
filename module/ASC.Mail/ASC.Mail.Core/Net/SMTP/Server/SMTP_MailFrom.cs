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
    /// This class holds MAIL FROM: command value.
    /// </summary>
    public class SMTP_MailFrom
    {
        #region Members

        private readonly string m_Body;
        private readonly string m_ENVID;
        private readonly string m_Mailbox = "";
        private readonly string m_RET;
        private readonly int m_Size = -1;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mailbox">Mailbox value.</param>
        /// <param name="size">SIZE parameter value.</param>
        /// <param name="body">BODY parameter value.</param>
        /// <param name="ret">DSN RET parameter value.</param>
        /// <param name="envid">DSN ENVID parameter value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>mailbox</b> is null reference.</exception>
        public SMTP_MailFrom(string mailbox, int size, string body, string ret, string envid)
        {
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox");
            }

            m_Mailbox = mailbox;
            m_Size = size;
            m_Body = body;
            m_RET = ret;
            m_ENVID = envid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets MAIL FROM: BODY parameter value. Value null means not specified.
        /// Defined in RFC 1652.
        /// </summary>
        public string Body
        {
            get { return m_Body; }
        }

        /// <summary>
        /// Gets DSN ENVID parameter value. Value null means not specified.
        /// Defined in RFC 1891.
        /// </summary>
        public string ENVID
        {
            get { return m_ENVID; }
        }

        /// <summary>
        /// Gets SMTP "mailbox" value. Actually this is just email address.
        /// This value can be "" if "null reverse-path".
        /// </summary>
        public string Mailbox
        {
            get { return m_Mailbox; }
        }

        /// <summary>
        /// Gets DSN RET parameter value. Value null means not specified.
        /// RET specifies whether message or headers should be included in any failed DSN issued for message transmission.
        /// Defined in RFC 1891.
        /// </summary>
        public string RET
        {
            get { return m_RET; }
        }

        /// <summary>
        /// Gets MAIL FROM: SIZE parameter value. Value -1 means not specified.
        /// Defined in RFC 1870.
        /// </summary>
        public int Size
        {
            get { return m_Size; }
        }

        #endregion
    }
}