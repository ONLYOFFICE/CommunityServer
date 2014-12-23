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

namespace ASC.Mail.Net.SIP.Message
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements SIP "hi-entry" value. Defined in RFC 4244.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 4244 Syntax:
    ///     hi-entry = hi-targeted-to-uri *( SEMI hi-param )
    ///     hi-targeted-to-uri= name-addr
    ///     hi-param = hi-index / hi-extension
    ///     hi-index = "index" EQUAL 1*DIGIT *(DOT 1*DIGIT)
    ///     hi-extension = generic-param
    /// </code>
    /// </remarks>
    public class SIP_t_HiEntry : SIP_t_ValueWithParams
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
                if (m_pAddress == null)
                {
                    throw new ArgumentNullException("m_pAddress");
                }

                m_pAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets 'index' parameter value. Value -1 means not specified.
        /// </summary>
        public double Index
        {
            get
            {
                SIP_Parameter parameter = Parameters["index"];
                if (parameter != null)
                {
                    return Convert.ToInt32(parameter.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (value == -1)
                {
                    Parameters.Remove("index");
                }
                else
                {
                    Parameters.Set("index", value.ToString());
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses "hi-entry" from specified value.
        /// </summary>
        /// <param name="value">SIP "hi-entry" value.</param>
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
        /// Parses "hi-entry" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            /*
                hi-entry = hi-targeted-to-uri *( SEMI hi-param )
                hi-targeted-to-uri= name-addr
                hi-param = hi-index / hi-extension
                hi-index = "index" EQUAL 1*DIGIT *(DOT 1*DIGIT)
                hi-extension = generic-param
            */

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // name-addr
            m_pAddress = new SIP_t_NameAddress();
            m_pAddress.Parse(reader);

            // Parse parameters
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid "hi-entry" value.
        /// </summary>
        /// <returns>Returns "hi-entry" value.</returns>
        public override string ToStringValue()
        {
            /*
                hi-entry = hi-targeted-to-uri *( SEMI hi-param )
                hi-targeted-to-uri= name-addr
                hi-param = hi-index / hi-extension
                hi-index = "index" EQUAL 1*DIGIT *(DOT 1*DIGIT)
                hi-extension = generic-param
            */

            StringBuilder retVal = new StringBuilder();

            // name-addr
            retVal.Append(m_pAddress.ToStringValue());

            // Add parameters
            retVal.Append(ParametersToString());

            return retVal.ToString();
        }

        #endregion
    }
}