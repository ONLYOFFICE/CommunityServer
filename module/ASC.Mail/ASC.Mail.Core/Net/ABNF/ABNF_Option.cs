/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.ABNF
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// This class represent ABNF "option". Defined in RFC 5234 4.
    /// </summary>
    public class ABNF_Option : ABNF_Element
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
        public static ABNF_Option Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // option = "[" *c-wsp alternation *c-wsp "]"

            if (reader.Peek() != '[')
            {
                throw new ParseException("Invalid ABNF 'option' value '" + reader.ReadToEnd() + "'.");
            }

            // Eat "[".
            reader.Read();

            // TODO: *c-wsp

            ABNF_Option retVal = new ABNF_Option();

            // We reached end of stream, no closing "]".
            if (reader.Peek() == -1)
            {
                throw new ParseException("Invalid ABNF 'option' value '" + reader.ReadToEnd() + "'.");
            }

            retVal.m_pAlternation = ABNF_Alternation.Parse(reader);

            // We don't have closing ")".
            if (reader.Peek() != ']')
            {
                throw new ParseException("Invalid ABNF 'option' value '" + reader.ReadToEnd() + "'.");
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