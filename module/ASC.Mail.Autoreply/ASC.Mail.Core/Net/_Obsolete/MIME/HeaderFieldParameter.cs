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


namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Header field parameter.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class HeaderFieldParameter
    {
        #region Members

        private readonly string m_Name = "";
        private readonly string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets header field parameter name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets header field parameter name.
        /// </summary>
        public string Value
        {
            get { return m_Value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parameterName">Header field parameter name.</param>
        /// <param name="parameterValue">Header field parameter value.</param>
        public HeaderFieldParameter(string parameterName, string parameterValue)
        {
            m_Name = parameterName;
            m_Value = parameterValue;
        }

        #endregion
    }
}