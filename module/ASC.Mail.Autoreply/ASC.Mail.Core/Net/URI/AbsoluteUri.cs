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


namespace ASC.Mail.Net
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements absolute-URI. Defined in RFC 3986.4.3.
    /// </summary>
    public class AbsoluteUri
    {
        #region Members

        private string m_Scheme = "";
        private string m_Value = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal AbsoluteUri() {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets URI scheme.
        /// </summary>
        public virtual string Scheme
        {
            get { return m_Scheme; }
        }

        /// <summary>
        /// Gets URI value after scheme.
        /// </summary>
        public string Value
        {
            get { return ToString().Split(new[] {':'}, 2)[1]; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parse URI from string value.
        /// </summary>
        /// <param name="value">String URI value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>value</b> has invalid URI value.</exception>
        public static AbsoluteUri Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value == "")
            {
                throw new ArgumentException("Argument 'value' value must be specified.");
            }

            string[] scheme_value = value.Split(new[] {':'}, 2);
            if (scheme_value[0].ToLower() == UriSchemes.sip || scheme_value[0].ToLower() == UriSchemes.sips)
            {
                SIP_Uri uri = new SIP_Uri();
                uri.ParseInternal(value);

                return uri;
            }
            else
            {
                AbsoluteUri uri = new AbsoluteUri();
                uri.ParseInternal(value);

                return uri;
            }
        }

        /// <summary>
        /// Converts URI to string.
        /// </summary>
        /// <returns>Returns URI as string.</returns>
        public override string ToString()
        {
            return m_Scheme + ":" + m_Value;
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Parses URI from the specified string.
        /// </summary>
        /// <param name="value">URI string.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        protected virtual void ParseInternal(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string[] scheme_value = value.Split(new[] {':'}, 1);
            m_Scheme = scheme_value[0].ToLower();
            if (scheme_value.Length == 2)
            {
                m_Value = scheme_value[1];
            }
        }

        #endregion
    }
}