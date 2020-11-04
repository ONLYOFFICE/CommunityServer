/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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