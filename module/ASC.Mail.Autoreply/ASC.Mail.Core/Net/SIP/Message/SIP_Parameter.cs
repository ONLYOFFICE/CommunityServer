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
    /// This class represents SIP value parameter.
    /// </summary>
    public class SIP_Parameter
    {
        #region Members

        private readonly string m_Name = "";
        private string m_Value = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        public SIP_Parameter(string name) : this(name, "") {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        public SIP_Parameter(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == "")
            {
                throw new ArgumentException("Parameter 'name' value may no be empty string !");
            }

            m_Name = name;
            m_Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets parameter name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets or sets parameter name. Value null means value less tag prameter.
        /// </summary>
        public string Value
        {
            get { return m_Value; }

            set { m_Value = value; }
        }

        #endregion
    }
}