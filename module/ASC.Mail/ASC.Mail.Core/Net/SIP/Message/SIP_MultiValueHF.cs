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
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements generic multi value SIP header field.
    /// This is used by header fields like Via,Contact, ... .
    /// </summary>
    public class SIP_MultiValueHF<T> : SIP_HeaderField where T : SIP_t_Value, new()
    {
        #region Members

        private readonly List<T> m_pValues;

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
                if (value != null)
                {
                    throw new ArgumentNullException("Property Value value may not be null !");
                }

                Parse(value);

                base.Value = value;
            }
        }

        /// <summary>
        /// Gets header field values.
        /// </summary>
        public List<T> Values
        {
            get { return m_pValues; }
        }

        /// <summary>
        /// Gets values count.
        /// </summary>
        public int Count
        {
            get { return m_pValues.Count; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Header field name.</param>
        /// <param name="value">Header field value.</param>
        public SIP_MultiValueHF(string name, string value) : base(name, value)
        {
            m_pValues = new List<T>();

            SetMultiValue(true);

            Parse(value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets header field values.
        /// </summary>
        /// <returns></returns>
        public object[] GetValues()
        {
            return m_pValues.ToArray();
        }

        /// <summary>
        /// Removes value from specified index.
        /// </summary>
        /// <param name="index">Index of value to remove.</param>
        public void Remove(int index)
        {
            if (index > -1 && index < m_pValues.Count)
            {
                m_pValues.RemoveAt(index);
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Parses multi value header field values.
        /// </summary>
        /// <param name="value">Header field value.</param>
        private void Parse(string value)
        {
            m_pValues.Clear();

            StringReader r = new StringReader(value);
            while (r.Available > 0)
            {
                r.ReadToFirstChar();
                // If we have COMMA, just consume it, it last value end.
                if (r.StartsWith(","))
                {
                    r.ReadSpecifiedLength(1);
                }

                // Allow xxx-param to pasre 1 value from reader.
                T param = new T();
                param.Parse(r);
                m_pValues.Add(param);
            }
        }

        /// <summary>
        /// Converts to valid mutli value header field value.
        /// </summary>
        /// <returns></returns>
        private string ToStringValue()
        {
            StringBuilder retVal = new StringBuilder();
            // Syntax: xxx-parm *(COMMA xxx-parm)
            for (int i = 0; i < m_pValues.Count; i++)
            {
                retVal.Append(m_pValues[i].ToStringValue());

                // Don't add comma for last item.
                if (i < m_pValues.Count - 1)
                {
                    retVal.Append(',');
                }
            }

            return retVal.ToString();
        }

        #endregion
    }
}