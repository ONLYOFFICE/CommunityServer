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


namespace ASC.Mail.Net.SMTP.Server
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// This class implements SMTP server reply.
    /// </summary>
    public class SMTP_Reply
    {
        #region Members

        private readonly string[] m_pReplyLines;
        private readonly int m_ReplyCode;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyCode">SMTP server reply code.</param>
        /// <param name="replyLine">SMTP server reply line.</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>replyLine</b> is null reference.</exception>
        public SMTP_Reply(int replyCode, string replyLine) : this(replyCode, new[] {replyLine})
        {
            if (replyLine == null)
            {
                throw new ArgumentNullException("replyLine");
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyCode">SMTP server reply code.</param>
        /// <param name="replyLines">SMTP server reply line(s).</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>replyLines</b> is null reference.</exception>
        public SMTP_Reply(int replyCode, string[] replyLines)
        {
            if (replyCode < 200 || replyCode > 599)
            {
                throw new ArgumentException("Argument 'replyCode' value must be >= 200 and <= 599.",
                                            "replyCode");
            }
            if (replyLines == null)
            {
                throw new ArgumentNullException("replyLines");
            }
            if (replyLines.Length == 0)
            {
                throw new ArgumentException("Argument 'replyLines' must conatin at least one line.",
                                            "replyLines");
            }

            m_ReplyCode = replyCode;
            m_pReplyLines = replyLines;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets SMTP server reply code.
        /// </summary>
        public int ReplyCode
        {
            get { return m_ReplyCode; }
        }

        /// <summary>
        /// Gets SMTP server reply lines.
        /// </summary>
        public string[] ReplyLines
        {
            get { return m_pReplyLines; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns SMTP server reply as string.
        /// </summary>
        /// <returns>Returns SMTP server reply as string.</returns>
        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder();
            for (int i = 0; i < m_pReplyLines.Length; i++)
            {
                // Last line.
                if (i == (m_pReplyLines.Length - 1))
                {
                    retVal.Append(m_ReplyCode + " " + m_pReplyLines[i] + "\r\n");
                }
                else
                {
                    retVal.Append(m_ReplyCode + "-" + m_pReplyLines[i] + "\r\n");
                }
            }

            return retVal.ToString();
        }

        #endregion
    }
}