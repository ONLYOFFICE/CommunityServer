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


namespace ASC.Mail.Net.SDP
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Implements SDP attribute.
    /// </summary>
    public class SDP_Attribute
    {
        #region Members

        private readonly string m_Name = "";
        private string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets attribute name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets or sets attribute value.
        /// </summary>
        public string Value
        {
            get { return m_Value; }

            set { m_Value = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        public SDP_Attribute(string name, string value)
        {
            m_Name = name;
            Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses media from "a" SDP message field.
        /// </summary>
        /// <param name="aValue">"a" SDP message field.</param>
        /// <returns></returns>
        public static SDP_Attribute Parse(string aValue)
        {
            // a=<attribute>
            // a=<attribute>:<value>

            // Remove a=
            StringReader r = new StringReader(aValue);
            r.QuotedReadToDelimiter('=');

            //--- <attribute> ------------------------------------------------------------
            string name = "";
            string word = r.QuotedReadToDelimiter(':');
            if (word == null)
            {
                throw new Exception("SDP message \"a\" field <attribute> name is missing !");
            }
            name = word;

            //--- <value> ----------------------------------------------------------------
            string value = "";
            word = r.ReadToEnd();
            if (word != null)
            {
                value = word;
            }

            return new SDP_Attribute(name, value);
        }

        /// <summary>
        /// Converts this to valid "a" string.
        /// </summary>
        /// <returns></returns>
        public string ToValue()
        {
            // a=<attribute>
            // a=<attribute>:<value>

            // a=<attribute>
            if (string.IsNullOrEmpty(m_Value))
            {
                return "a=" + m_Name + "\r\n";
            }
                // a=<attribute>:<value>
            else
            {
                return "a=" + m_Name + ":" + m_Value + "\r\n";
            }
        }

        #endregion
    }
}