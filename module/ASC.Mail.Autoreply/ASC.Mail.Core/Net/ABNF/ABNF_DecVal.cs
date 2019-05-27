/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
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