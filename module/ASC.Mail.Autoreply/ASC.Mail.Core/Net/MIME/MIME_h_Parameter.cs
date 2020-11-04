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