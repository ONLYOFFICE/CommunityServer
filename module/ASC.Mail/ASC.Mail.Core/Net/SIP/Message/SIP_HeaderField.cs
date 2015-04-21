/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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