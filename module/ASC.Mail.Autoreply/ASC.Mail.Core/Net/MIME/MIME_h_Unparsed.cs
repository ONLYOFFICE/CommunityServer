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
    using System.Text;

    #endregion

    /// <summary>
    /// This class represent header field what parsing has failed.
    /// </summary>
    public class MIME_h_Unparsed : MIME_h
    {
        #region Members

        private readonly string m_Name;
        private readonly string m_ParseValue;
        private readonly Exception m_pException;
        private readonly string m_Value;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public override bool IsModified
        {
            get { return false; }
        }

        /// <summary>
        /// Gets header field name.
        /// </summary>
        public override string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets header field value.
        /// </summary>
        public string Value
        {
            get { return m_Value; }
        }

        /// <summary>
        /// Gets error happened during parse.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Content-Type: text/plain'.</param>
        /// <param name="exception">Parsing error.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        internal MIME_h_Unparsed(string value, Exception exception)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string[] name_value = value.Split(new[] {':'}, 2);
            if (name_value.Length != 2)
            {
                throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
            }

            m_Name = name_value[0];
            m_Value = name_value[1].Trim();
            m_ParseValue = value;
            m_pException = exception;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Content-Type: text/plain'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="InvalidOperationException">Is alwyas raised when this mewthod is accsessed.</exception>
        public static MIME_h_Unparsed Parse(string value)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.</param>
        /// <returns>Returns header field as string.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            return m_ParseValue;
        }

        #endregion
    }
}