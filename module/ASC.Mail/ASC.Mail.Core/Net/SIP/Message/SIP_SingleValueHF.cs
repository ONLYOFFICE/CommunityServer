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
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements single value header field.
    /// Used by header fields like To:,From:,CSeq:, ... .
    /// </summary>
    public class SIP_SingleValueHF<T> : SIP_HeaderField where T : SIP_t_Value
    {
        #region Members

        private T m_pValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets header field value.
        /// </summary>
        public override string Value
        {
            get { return ToStringValue(); }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Property Value value may not be null !");
                }

                Parse(new StringReader(value));
            }
        }

        /// <summary>
        /// Gets or sets header field value.
        /// </summary>
        public T ValueX
        {
            get { return m_pValue; }

            set { m_pValue = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <param name="value">Header field value.</param>
        public SIP_SingleValueHF(string name, T value) : base(name, "")
        {
            m_pValue = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses single value from specified reader.
        /// </summary>
        /// <param name="reader">Reader what contains </param>
        public void Parse(StringReader reader)
        {
            m_pValue.Parse(reader);
        }

        /// <summary>
        /// Convert this to string value.
        /// </summary>
        /// <returns>Returns this as string value.</returns>
        public string ToStringValue()
        {
            return m_pValue.ToStringValue();
        }

        #endregion

        // FIX ME: Change base class Value property or this to new name
    }
}