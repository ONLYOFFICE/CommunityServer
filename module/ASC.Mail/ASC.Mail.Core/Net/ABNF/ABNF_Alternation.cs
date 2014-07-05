/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.ABNF
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;

    #endregion

    /// <summary>
    /// This class represent ABNF "alternation". Defined in RFC 5234 4.
    /// </summary>
    public class ABNF_Alternation
    {
        #region Members

        private readonly List<ABNF_Concatenation> m_pItems;

        #endregion

        #region Properties

        /// <summary>
        /// Gets alternation items.
        /// </summary>
        public List<ABNF_Concatenation> Items
        {
            get { return m_pItems; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ABNF_Alternation()
        {
            m_pItems = new List<ABNF_Concatenation>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ABNF_Alternation Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // alternation = concatenation *(*c-wsp "/" *c-wsp concatenation)

            ABNF_Alternation retVal = new ABNF_Alternation();

            while (true)
            {
                ABNF_Concatenation item = ABNF_Concatenation.Parse(reader);
                if (item != null)
                {
                    retVal.m_pItems.Add(item);
                }

                // We reached end of string.
                if (reader.Peek() == -1)
                {
                    break;
                }
                    // We have next alternation item.
                else if (reader.Peek() == '/')
                {
                    reader.Read();
                }
                    // We have unexpected value, probably alternation ends.
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