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