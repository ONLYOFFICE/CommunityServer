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