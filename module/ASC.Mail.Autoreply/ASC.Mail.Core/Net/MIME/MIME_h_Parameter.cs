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


namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Represents MIME header field parameter.
    /// </summary>
    public class MIME_h_Parameter
    {
        #region Members

        private readonly string m_Name = "";
        private bool m_IsModified;
        private string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this header field parameter is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields parameters has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public bool IsModified
        {
            get { return m_IsModified; }
        }

        /// <summary>
        /// Gets parameter name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets or sets parameter value. Value null means not specified.
        /// </summary>
        public string Value
        {
            get { return m_Value; }

            set
            {
                m_Value = value;
                m_IsModified = true;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value. Value null means not specified.</param>
        public MIME_h_Parameter(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            m_Name = name;
            m_Value = value;
        }

        #endregion
    }
}