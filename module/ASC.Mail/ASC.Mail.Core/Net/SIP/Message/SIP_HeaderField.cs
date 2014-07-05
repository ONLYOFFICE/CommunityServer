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

namespace ASC.Mail.Net.SIP.Message
{
    /// <summary>
    /// Represents SIP message header field.
    /// </summary>
    public class SIP_HeaderField
    {
        #region Members

        private readonly string m_Name = "";
        private bool m_IsMultiValue;
        private string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets header field name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets or sets header field value.
        /// </summary>
        public virtual string Value
        {
            get { return m_Value; }

            set { m_Value = value; }
        }

        /// <summary>
        /// Gets if header field is multi value header field.
        /// </summary>
        public bool IsMultiValue
        {
            get { return m_IsMultiValue; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <param name="value">Header field value.</param>
        internal SIP_HeaderField(string name, string value)
        {
            m_Name = name;
            m_Value = value;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Sets property IsMultiValue value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        internal void SetMultiValue(bool value)
        {
            m_IsMultiValue = value;
        }

        #endregion
    }
}