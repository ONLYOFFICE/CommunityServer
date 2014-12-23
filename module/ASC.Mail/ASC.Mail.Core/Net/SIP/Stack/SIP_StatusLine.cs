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
    /// Implements SIP Status-Line. Defined in RFC 3261.
    /// </summary>
    public class SIP_StatusLine
    {
        #region Members

        private string m_Reason = "";
        private int m_StatusCode;
        private string m_Version = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="statusCode">Status code.</param>
        /// <param name="reason">Reason text.</param>
        /// <exception cref="ArgumentException">Is raised when </exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>reason</b> is null reference.</exception>
        public SIP_StatusLine(int statusCode, string reason)
        {
            if (statusCode < 100 || statusCode > 699)
            {
                throw new ArgumentException("Argument 'statusCode' value must be >= 100 and <= 699.");
            }
            if (reason == null)
            {
                throw new ArgumentNullException("reason");
            }

            m_Version = "SIP/2.0";
            m_StatusCode = statusCode;
            m_Reason = reason;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets reason phrase.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public string Reason
        {
            get { return m_Reason; }

            set
            {
                if (Reason == null)
                {
                    throw new ArgumentNullException("Reason");
                }

                m_Reason = value;
            }
        }

        /// <summary>
        /// Gets or sets status code.
        /// </summary>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> has invalid value.</exception>
        public int StatusCode
        {
            get { return m_StatusCode; }

            set
            {
                if (value < 100 || value > 699)
                {
                    throw new ArgumentException("Argument 'statusCode' value must be >= 100 and <= 699.");
                }

                m_StatusCode = value;
            }
        }

        /// <summary>
        /// Gets or sets SIP version.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> has invalid value.</exception>
        public string Version
        {
            get { return m_Version; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Version");
                }
                if (value == "")
                {
                    throw new ArgumentException("Property 'Version' value must be specified.");
                }

                m_Version = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns Status-Line string.
        /// </summary>
        /// <returns>Returns Status-Line string.</returns>
        public override string ToString()
        {
            // RFC 3261 25. 
            //  Status-Line = SIP-Version SP Status-Code SP Reason-Phrase CRLF

            return m_Version + " " + m_StatusCode + " " + m_Reason + "\r\n";
        }

        #endregion
    }
}