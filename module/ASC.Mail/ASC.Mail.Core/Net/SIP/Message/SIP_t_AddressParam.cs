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
    /// Implements SIP_t_NameAddress + parameters value.
    /// </summary>
    public class SIP_t_AddressParam : SIP_t_ValueWithParams
    {
        #region Members

        private SIP_t_NameAddress m_pAddress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets address.
        /// </summary>
        public SIP_t_NameAddress Address
        {
            get { return m_pAddress; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_AddressParam() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP_t_NameAddress + parameters value.</param>
        public SIP_t_AddressParam(string value)
        {
            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses this from specified value.
        /// </summary>
        /// <param name="value">Address + params value.</param>
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
        /// Parses this from address param string.
        /// </summary>
        /// <param name="reader">Reader what contains address param string.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Parse address
            SIP_t_NameAddress address = new SIP_t_NameAddress();
            address.Parse(reader);
            m_pAddress = address;

            // Parse parameters.
            ParseParameters(reader);
        }

        /// <summary>
        /// Converts this to valid value string.
        /// </summary>
        /// <returns></returns>
        public override string ToStringValue()
        {
            StringBuilder retVal = new StringBuilder();

            // Add address
            retVal.Append(m_pAddress.ToStringValue());

            // Add parameters
            foreach (SIP_Parameter parameter in Parameters)
            {
                if (parameter.Value != null)
                {
                    retVal.Append(";" + parameter.Name + "=" + parameter.Value);
                }
                else
                {
                    retVal.Append(";" + parameter.Name);
                }
            }

            return retVal.ToString();
        }

        #endregion
    }
}