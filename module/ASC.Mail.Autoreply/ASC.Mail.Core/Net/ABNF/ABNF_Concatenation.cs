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
    using System.Collections.Generic;
    using System.IO;

    #endregion

    /// <summary>
    /// This class represent ABNF "concatenation". Defined in RFC 5234 4.
    /// </summary>
    public class ABNF_Concatenation
    {
        #region Members

        private readonly List<ABNF_Repetition> m_pItems;

        #endregion

        #region Properties

        /// <summary>
        /// Gets concatenation items.
        /// </summary>
        public List<ABNF_Repetition> Items
        {
            get { return m_pItems; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ABNF_Concatenation()
        {
            m_pItems = new List<ABNF_Repetition>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ABNF_Concatenation Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // concatenation  =  repetition *(1*c-wsp repetition)
            // repetition     =  [repeat] element

            ABNF_Concatenation retVal = new ABNF_Concatenation();

            while (true)
            {
                ABNF_Repetition item = ABNF_Repetition.Parse(reader);
                if (item != null)
                {
                    retVal.m_pItems.Add(item);
                }
                    // We reached end of string.
                else if (reader.Peek() == -1)
                {
                    break;
                }
                    // We have next concatenation item.
                else if (reader.Peek() == ' ')
                {
                    reader.Read();
                }
                    // We have unexpected value, probably concatenation ends.
                else
                {
                    break;
                }
            }

            return retVal;
        }

        #endregion
    }
}