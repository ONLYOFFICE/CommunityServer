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
    using System.Text;

    #endregion

    /// <summary>
    /// This class represent ABNF "repetition". Defined in RFC 5234 4.
    /// </summary>
    public class ABNF_Repetition
    {
        #region Members

        private readonly int m_Max = int.MaxValue;
        private readonly int m_Min;
        private readonly ABNF_Element m_pElement;

        #endregion

        #region Properties

        /// <summary>
        /// Gets minimum repetitions.
        /// </summary>
        public int Min
        {
            get { return m_Min; }
        }

        /// <summary>
        /// Gets maximum repetitions.
        /// </summary>
        public int Max
        {
            get { return m_Max; }
        }

        /// <summary>
        /// Gets repeated element.
        /// </summary>
        public ABNF_Element Element
        {
            get { return m_pElement; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="min">Minimum repetitions.</param>
        /// <param name="max">Maximum repetitions.</param>
        /// <param name="element">Repeated element.</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>element</b> is null reference.</exception>
        public ABNF_Repetition(int min, int max, ABNF_Element element)
        {
            if (min < 0)
            {
                throw new ArgumentException("Argument 'min' value must be >= 0.");
            }
            if (max < 0)
            {
                throw new ArgumentException("Argument 'max' value must be >= 0.");
            }
            if (min > max)
            {
                throw new ArgumentException("Argument 'min' value must be <= argument 'max' value.");
            }
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            m_Min = min;
            m_Max = max;
            m_pElement = element;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ABNF_Repetition Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            /*
                repetition     =  [repeat] element
                repeat         =  1*DIGIT / (*DIGIT "*" *DIGIT)
                element        =  rulename / group / option / char-val / num-val / prose-val
            */

            int min = 0;
            int max = int.MaxValue;

            // --- range ------------------------------------
            if (char.IsDigit((char) reader.Peek()))
            {
                StringBuilder minString = new StringBuilder();
                while (char.IsDigit((char) reader.Peek()))
                {
                    minString.Append((char) reader.Read());
                }
                min = Convert.ToInt32(minString.ToString());
            }
            if (reader.Peek() == '*')
            {
                reader.Read();
            }
            if (char.IsDigit((char) reader.Peek()))
            {
                StringBuilder maxString = new StringBuilder();
                while (char.IsDigit((char) reader.Peek()))
                {
                    maxString.Append((char) reader.Read());
                }
                max = Convert.ToInt32(maxString.ToString());
            }
            //-----------------------------------------------

            // End of stream reached.
            if (reader.Peek() == -1)
            {
                return null;
            }
                // We have rulename.
            else if (char.IsLetter((char) reader.Peek()))
            {
                return new ABNF_Repetition(min, max, ABNF_RuleName.Parse(reader));
            }
                // We have group.
            else if (reader.Peek() == '(')
            {
                return new ABNF_Repetition(min, max, ABFN_Group.Parse(reader));
            }
                // We have option.
            else if (reader.Peek() == '[')
            {
                return new ABNF_Repetition(min, max, ABNF_Option.Parse(reader));
            }
                // We have char-val.
            else if (reader.Peek() == '\"')
            {
                return new ABNF_Repetition(min, max, ABNF_CharVal.Parse(reader));
            }
                // We have num-val.
            else if (reader.Peek() == '%')
            {
                // Eat '%'.
                reader.Read();

                if (reader.Peek() == 'd')
                {
                    return new ABNF_Repetition(min, max, ABNF_DecVal.Parse(reader));
                }
                else
                {
                    throw new ParseException("Invalid 'num-val' value '" + reader.ReadToEnd() + "'.");
                }
            }
                // We have prose-val.
            else if (reader.Peek() == '<')
            {
                return new ABNF_Repetition(min, max, ABNF_ProseVal.Parse(reader));
            }

            return null;
        }

        #endregion
    }
}