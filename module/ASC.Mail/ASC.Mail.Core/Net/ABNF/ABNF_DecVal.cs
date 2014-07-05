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
    using System.Text;

    #endregion

    /// <summary>
    /// This class represent ABNF "dec-val". Defined in RFC 5234 4.
    /// </summary>
    public class ABNF_DecVal : ABNF_Element
    {
        #region Nested type: ValueType

        private enum ValueType
        {
            Single = 0,
            Concated = 1,
            Range = 2,
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default 'range' value constructor.
        /// </summary>
        /// <param name="start">Range start value.</param>
        /// <param name="end">Range end value.</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public ABNF_DecVal(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentException("Argument 'start' value must be >= 0.");
            }
            if (end < 0)
            {
                throw new ArgumentException("Argument 'end' value must be >= 0.");
            }

            // TODO:
        }

        /// <summary>
        /// Default 'concated' value constructor.
        /// </summary>
        /// <param name="values">Concated values.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>values</b> is null reference value.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public ABNF_DecVal(int[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Length < 1)
            {
                throw new ArgumentException("Argument 'values' must contain at least 1 value.");
            }

            // TODO:
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ABNF_DecVal Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // dec-val =  "d" 1*DIGIT [ 1*("." 1*DIGIT) / ("-" 1*DIGIT) ]

            if (reader.Peek() != 'd')
            {
                throw new ParseException("Invalid ABNF 'dec-val' value '" + reader.ReadToEnd() + "'.");
            }

            // Eat 'd'.
            reader.Read();

            if (!char.IsNumber((char) reader.Peek()))
            {
                throw new ParseException("Invalid ABNF 'dec-val' value '" + reader.ReadToEnd() + "'.");
            }

            ValueType valueType = ValueType.Single;
            List<int> values = new List<int>();
            StringBuilder b = new StringBuilder();
            while (true)
            {
                // We reached end of string.
                if (reader.Peek() == -1)
                {
                    // - or . without required 1 DIGIT.
                    if (b.Length == 0)
                    {
                        throw new ParseException("Invalid ABNF 'dec-val' value '" + reader.ReadToEnd() + "'.");
                    }
                    break;
                }
                else if (char.IsNumber((char) reader.Peek()))
                {
                    b.Append((char) reader.Read());
                }
                    // Concated value.
                else if (reader.Peek() == '.')
                {
                    // Range and conacted is not allowed to mix.
                    if (valueType == ValueType.Range)
                    {
                        throw new ParseException("Invalid ABNF 'dec-val' value '" + reader.ReadToEnd() + "'.");
                    }
                    if (b.Length == 0)
                    {
                        throw new ParseException("Invalid ABNF 'dec-val' value '" + reader.ReadToEnd() + "'.");
                    }

                    values.Add(Convert.ToInt32(b.ToString()));
                    b = new StringBuilder();
                    valueType = ValueType.Concated;

                    // Eat '.'.
                    reader.Read();
                }
                    // Value range.
                else if (reader.Peek() == '-')
                {
                    // Range and conacted is not allowed to mix. Also multiple ranges not allowed.
                    if (valueType != ValueType.Single)
                    {
                        throw new ParseException("Invalid ABNF 'dec-val' value '" + reader.ReadToEnd() + "'.");
                    }
                    values.Add(Convert.ToInt32(b.ToString()));
                    b = new StringBuilder();
                    valueType = ValueType.Range;

                    // Eat '-'.
                    reader.Read();
                }
                    // Not dec-val char, value reading completed.
                else
                {
                    // - or . without required 1 DIGIT.
                    if (b.Length == 0)
                    {
                        throw new ParseException("Invalid ABNF 'dec-val' value '" + reader.ReadToEnd() + "'.");
                    }
                    break;
                }
            }
            values.Add(Convert.ToInt32(b.ToString()));

            //Console.WriteLine(valueType.ToString());
            //foreach(int v in values){
            //    Console.WriteLine(v);
            // }

            if (valueType == ValueType.Single)
            {
                return new ABNF_DecVal(values[0], values[0]);
            }
            else if (valueType == ValueType.Concated)
            {
                return new ABNF_DecVal(values.ToArray());
            }
            else
            {
                return new ABNF_DecVal(values[0], values[1]);
            }
        }

        #endregion
    }
}