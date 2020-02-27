/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "Referred-By" value. Defined in RFC 3892.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3892 Syntax:
    ///     Referred-By         =  referrer-uri *( SEMI (referredby-id-param / generic-param) )
    ///     referrer-uri        = ( name-addr / addr-spec )
    ///     referredby-id-param = "cid" EQUAL sip-clean-msg-id
    ///     sip-clean-msg-id    = LDQUOT dot-atom "@" (dot-atom / host) RDQUOT
    /// </code>
    /// </remarks>
    public class SIP_t_ReferredBy : SIP_t_ValueWithParams
    {
        #region Members

        private SIP_t_NameAddress m_pAddress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets address.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        public SIP_t_NameAddress Address
        {
            get { return m_pAddress; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Address");
                }

                m_pAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets 'cid' parameter value. Value null means not specified.
        /// </summary>
        public string CID
        {
            get
            {
                SIP_Parameter parameter = Parameters["cid"];
                if (parameter != null)
                {
                    return parameter.Value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Parameters.Remove("cid");
                }
                else
                {
                    Parameters.Set("cid", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP 'Referred-By' value.</param>
        public SIP_t_ReferredBy(string value)
        {
            m_pAddress = new SIP_t_NameAddress();

            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "Referred-By" from specified value.
        /// </summary>
        /// <param name="value">SIP "Referred-By" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "Referred-By" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                Referred-By         =  referrer-uri *( SEMI (referredby-id-param / generic-param) )
                referrer-uri        = ( name-addr / addr-spec )
                referredby-id-param = "cid" EQUAL sip-clean-msg-id
                sip-clean-msg-id    = LDQUOT dot-atom "@" (dot-atom / host) RDQUOT
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // referrer-uri
            m_pAddress.Parse(reader);

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "Referred-By" value.
        /// </summary>
        /// <returns>Returns "Referred-By" value.</returns>
        public override string ToStringValue()
        {
            /*
                Referred-By         =  referrer-uri *( SEMI (referredby-id-param / generic-param) )
                referrer-uri        = ( name-addr / addr-spec )
                referredby-id-param = "cid" EQUAL sip-clean-msg-id
                sip-clean-msg-id    = LDQUOT dot-atom "@" (dot-atom / host) RDQUOT
            */

            StringBuilder retVal = new StringBuilder();

            // referrer-uri
            retVal.Append(m_pAddress.ToStringValue());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}