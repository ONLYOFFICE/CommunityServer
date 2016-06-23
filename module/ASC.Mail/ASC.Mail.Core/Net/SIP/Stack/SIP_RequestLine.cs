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


namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements SIP Request-Line. Defined in RFC 3261.
    /// </summary>
    public class SIP_RequestLine
    {
        #region Members

        private string m_Method = "";
        private AbsoluteUri m_pUri;
        private string m_Version = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="method">SIP method.</param>
        /// <param name="uri">Request URI.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>method</b> or <b>uri</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public SIP_RequestLine(string method, AbsoluteUri uri)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!SIP_Utils.IsToken(method))
            {
                throw new ArgumentException("Argument 'method' value must be token.");
            }
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            m_Method = method.ToUpper();
            m_pUri = uri;
            m_Version = "SIP/2.0";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets request method. This value is always in upper-case.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> has invalid value.</exception>
        public string Method
        {
            get { return m_Method; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Method");
                }
                if (!SIP_Utils.IsToken(value))
                {
                    throw new ArgumentException("Property 'Method' value must be token.");
                }

                m_Method = value.ToUpper();
            }
        }

        /// <summary>
        /// Gets or sets request URI.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public AbsoluteUri Uri
        {
            get { return m_pUri; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Uri");
                }

                m_pUri = value;
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
        /// Returns Request-Line string.
        /// </summary>
        /// <returns>Returns Request-Line string.</returns>
        public override string ToString()
        {
            // RFC 3261 25. 
            //  Request-Line = Method SP Request-URI SP SIP-Version CRLF

            return m_Method + " " + m_pUri + " " + m_Version + "\r\n";
        }

        #endregion
    }
}