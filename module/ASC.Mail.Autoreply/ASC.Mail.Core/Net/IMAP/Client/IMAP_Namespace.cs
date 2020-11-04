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


namespace ASC.Mail.Net.IMAP.Client
{
    /// <summary>
    /// IMAP namespace. Defined in RFC 2342.
    /// </summary>
    public class IMAP_Namespace
    {
        #region Members

        private readonly string m_Delimiter = "";
        private readonly string m_Name = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets namespace name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets namespace hierarchy delimiter.
        /// </summary>
        public string Delimiter
        {
            get { return m_Delimiter; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Namespace name.</param>
        /// <param name="delimiter">Namespace hierarchy delimiter.</param>
        public IMAP_Namespace(string name, string delimiter)
        {
            m_Name = name;
            m_Delimiter = delimiter;
        }

        #endregion
    }
}