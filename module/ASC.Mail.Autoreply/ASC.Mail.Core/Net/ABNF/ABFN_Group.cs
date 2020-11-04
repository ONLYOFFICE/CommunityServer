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


namespace ASC.Mail.Net.ABNF
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// This class represent ABNF "group". Defined in RFC 5234 4.
    /// </summary>
    public class ABFN_Group : ABNF_Element
    {
        #region Members

        private ABNF_Alternation m_pAlternation;

        #endregion

        #region Properties

        /// <summary>
        /// Gets option alternation elements.
        /// </summary>
        public ABNF_Alternation Alternation
        {
            get { return m_pAlternation; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ABFN_Group Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // group = "(" *c-wsp alternation *c-wsp ")"

            if (reader.Peek() != '(')
            {
                throw new ParseException("Invalid ABNF 'group' value '" + reader.ReadToEnd() + "'.");
            }

            // Eat "(".
            reader.Read();

            // TODO: *c-wsp

            ABFN_Group retVal = new ABFN_Group();

            // We reached end of stream, no closing ")".
            if (reader.Peek() == -1)
            {
                throw new ParseException("Invalid ABNF 'group' value '" + reader.ReadToEnd() + "'.");
            }

            retVal.m_pAlternation = ABNF_Alternation.Parse(reader);

            // We don't have closing ")".
            if (reader.Peek() != ')')
            {
                throw new ParseException("Invalid ABNF 'group' value '" + reader.ReadToEnd() + "'.");
            }
            else
            {
                reader.Read();
            }

            return retVal;
        }

        #endregion
    }
}