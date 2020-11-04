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
    using System.Collections;

    #endregion

    /// <summary>
    /// Parametized header field. 
    /// <p/>
    /// Syntax: value;parameterName=parameterValue;parameterName=parameterValue;... .
    /// Example: (Content-Type:) text/html; charset="ascii".
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class ParametizedHeaderField
    {
        #region Members

        private readonly HeaderField m_pHeaderField;
        private readonly HeaderFieldParameterCollection m_pParameters;

        #endregion

        #region Properties

        /// <summary>
        /// Gets header field name.
        /// </summary>
        public string Name
        {
            get { return m_pHeaderField.Name; }
        }

        /// <summary>
        /// Gets or sets header field value.
        /// </summary>
        public string Value
        {
            get
            {
                // Syntax: value;parameterName=parameterValue;parameterName=parameterValue;... ;
                // First item is value
                return TextUtils.SplitQuotedString(m_pHeaderField.Value, ';')[0];
            }

            set { StoreParameters(value, ParseParameters()); }
        }

        /// <summary>
        /// Gets header field parameters.
        /// </summary>
        public HeaderFieldParameterCollection Parameters
        {
            get { return m_pParameters; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="headerField">Source header field.</param>
        public ParametizedHeaderField(HeaderField headerField)
        {
            m_pHeaderField = headerField;

            m_pParameters = new HeaderFieldParameterCollection(this);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses parameters from header field.
        /// </summary>
        /// <returns></returns>
        internal Hashtable ParseParameters()
        {
            // Syntax: value;parameterName=parameterValue;parameterName=parameterValue;... 
            string[] paramNameValues = TextUtils.SplitQuotedString(m_pHeaderField.EncodedValue, ';');

            Hashtable retVal = new Hashtable();
            // Skip value, other entries are parameters
            for (int i = 1; i < paramNameValues.Length; i++)
            {
                string[] paramNameValue = paramNameValues[i].Trim().Split(new[] {'='}, 2);
                if (!retVal.ContainsKey(paramNameValue[0].ToLower()))
                {
                    if (paramNameValue.Length == 2)
                    {
                        string value = paramNameValue[1];

                        // Quotes-string, unqoute.
                        if (value.StartsWith("\""))
                        {
                            value = TextUtils.UnQuoteString(paramNameValue[1]);
                        }

                        retVal.Add(paramNameValue[0].ToLower(), value);
                    }
                    else
                    {
                        retVal.Add(paramNameValue[0].ToLower(), "");
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Stores parameters to header field Value property.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameters"></param>
        internal void StoreParameters(string value, Hashtable parameters)
        {
            string retVal = value;
            foreach (DictionaryEntry entry in parameters)
            {
                retVal += ";\t" + entry.Key + "=\"" + entry.Value + "\"";
            }

            // Syntax: value;parameterName=parameterValue;parameterName=parameterValue;... ;
            m_pHeaderField.Value = retVal;
        }

        #endregion
    }
}