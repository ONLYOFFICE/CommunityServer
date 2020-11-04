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


using System.Text.RegularExpressions;

namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// Implements 'encoded-word' encoding. Defined in RFC 2047.
    /// </summary>
    public class MIME_Encoding_EncodedWord
    {
        #region Members

        private readonly MIME_EncodedWordEncoding m_Encoding;
        private readonly Encoding m_pCharset;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="encoding">Encoding to use to encode text.</param>
        /// <param name="charset">Charset to use for encoding. If not sure UTF-8 is strongly recommended.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>charset</b> is null reference.</exception>
        public MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding encoding, Encoding charset)
        {
            if (charset == null)
            {
                throw new ArgumentNullException("charset");
            }

            m_Encoding = encoding;
            m_pCharset = charset;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if specified text must be encoded.
        /// </summary>
        /// <param name="text">Text to encode.</param>
        /// <returns>Returns true if specified text must be encoded, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
        public static bool MustEncode(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            // Encoding is needed only for non-ASCII chars.

            foreach (char c in text)
            {
                if (c > 127)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Encodes specified text if it contains 8-bit chars, otherwise text won't be encoded.
        /// </summary>
        /// <param name="encoding">Encoding to use to encode text.</param>
        /// <param name="charset">Charset to use for encoding. If not sure UTF-8 is strongly recommended.</param>
        /// <param name="text">Text to encode.</param>
        /// <returns>Returns encoded text.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>charset</b> or <b>text</b> is null reference.</exception>
        public static string EncodeS(MIME_EncodedWordEncoding encoding, Encoding charset, string text)
        {
            if (charset == null)
            {
                throw new ArgumentNullException("charset");
            }
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            /* RFC 2047 2.
                encoded-word = "=?" charset "?" encoding "?" encoded-text "?="
             
                An 'encoded-word' may not be more than 75 characters long, including
                'charset', 'encoding', 'encoded-text', and delimiters.  If it is
                desirable to encode more text than will fit in an 'encoded-word' of
                75 characters, multiple 'encoded-word's (separated by CRLF SPACE) may
                be used.
             
               RFC 2231 (updates syntax)
                encoded-word := "=?" charset ["*" language] "?" encoded-text "?="
            */

            if (MustEncode(text))
            {
                StringBuilder retVal = new StringBuilder();
                byte[] data = charset.GetBytes(text);
                int maxEncodedTextSize = 75 - (("=?" + charset.WebName + "?" + encoding + "?" + "?=")).Length;

                #region B encode

                if (encoding == MIME_EncodedWordEncoding.B)
                {
                    retVal.Append("=?" + charset.WebName + "?B?");
                    int stored = 0;
                    string base64 = Convert.ToBase64String(data);
                    for (int i = 0; i < base64.Length; i += 4)
                    {
                        // Encoding buffer full, create new encoded-word.
                        if (stored + 4 > maxEncodedTextSize)
                        {
                            retVal.Append("?=\r\n =?" + charset.WebName + "?B?");
                            stored = 0;
                        }

                        retVal.Append(base64, i, 4);
                        stored += 4;
                    }
                    retVal.Append("?=");
                }

                #endregion

                #region Q encode

                else
                {
                    retVal.Append("=?" + charset.WebName + "?Q?");
                    int stored = 0;
                    foreach (byte b in data)
                    {
                        string val = null;
                        // We need to encode byte. Defined in RFC 2047 4.2.
                        if (b > 127 || b == '=' || b == '?' || b == '_' || b == ' ')
                        {
                            val = "=" + b.ToString("X2");
                        }
                        else
                        {
                            val = ((char)b).ToString();
                        }

                        // Encoding buffer full, create new encoded-word.
                        if (stored + val.Length > maxEncodedTextSize)
                        {
                            retVal.Append("?=\r\n =?" + charset.WebName + "?Q?");
                            stored = 0;
                        }

                        retVal.Append(val);
                        stored += val.Length;
                    }
                    retVal.Append("?=");
                }

                #endregion

                return retVal.ToString();
            }
            else
            {
                return text;
            }
        }

        public static string DecodeSubject(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            if (ParseRegex.IsMatch(text))
            {
                //Kill spaces
                text = Regex.Replace(text,@"(\s+|\t+)","");

                string[] separator = { @"?==?" };
                string[] splitStr = text.Split(separator, StringSplitOptions.None);
                for (int i = 1; i < splitStr.Length; i++)
                {
                    if (splitStr[i - 1][splitStr[i - 1].Length - 1] != '=')
                        splitStr[i] = Regex.Replace(splitStr[i], @"(?<charset>.*?)\?(?<encoding>[qQbB])\?", "");
                    else splitStr[i] = @"?==?" + splitStr[i];

                }
                if(splitStr.Length >1) text = string.Join("", splitStr);

            }
            return ParseRegex.Replace(text, new MatchEvaluator(Evalute));
        }


        private static readonly Regex ParseRegex = new Regex(@"=\?(?<charset>.*?)\?(?<encoding>[qQbB])\?(?<value>.*?)\?=", RegexOptions.Multiline|RegexOptions.Compiled);

        public static string DecodeAll(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return ParseRegex.Replace(text, new MatchEvaluator(Evalute));


            //StringBuilder builder = new StringBuilder();
            //StringBuilder notDecoded = new StringBuilder();
            //builder.Append(text);
            ////Try decode as single
            //var fullWordParts = text.Split('?');


            //try
            //{
            //    if (fullWordParts.Length == 5)
            //    {
            //        if (fullWordParts[2].Equals("B", StringComparison.OrdinalIgnoreCase))
            //        {
            //            //do processing
            //            using (var ms = new MemoryStream())
            //            {
            //                byte[] bytes = Convert.FromBase64String(fullWordParts[3]);
            //                        ms.Write(bytes, 0, bytes.Length);
            //                notDecoded.Append(
            //                    Encoding.GetEncoding(fullWordParts[1].Split('*')[0]).GetString(ms.ToArray()));
            //                builder = notDecoded;
            //            }
            //        }
            //        else
            //        {
            //            notDecoded.Append(DecodeS(builder.ToString()));
            //            builder = notDecoded;
            //        }
                    
            //    }
            //    else
            //    {


            //        var words = text.Split(' ', '\t');
            //        //Get encoded word start
            //        int decodedWordIndex = 0;
            //        while (decodedWordIndex < words.Length)
            //        {
            //            string[] wordparts = words[decodedWordIndex].Split('?');

            //            if (wordparts.Length == 5)
            //            {
            //                if (wordparts[2].Equals("B", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    //do processing
            //                    using (MemoryStream ms = new MemoryStream())
            //                    {
            //                        for (int i = decodedWordIndex; i < words.Length; i++)
            //                        {
            //                            string[] parts = words[i].Split('?');
            //                            // Not encoded-word.
            //                            if (parts.Length == 5)
            //                            {
            //                                byte[] bytes = Convert.FromBase64String(parts[3]);
            //                                ms.Write(bytes, 0, bytes.Length);
            //                            }
            //                        }
            //                        notDecoded.Append(
            //                            Encoding.GetEncoding(wordparts[1].Split('*')[0]).GetString(ms.ToArray()));
            //                        builder = notDecoded;
            //                        break;

            //                    }
            //                }
            //                else
            //                {


            //                    //Found decoded
            //                    //encoded
            //                    builder = new StringBuilder();
            //                    //Get the first part
            //                    builder.Append(words[decodedWordIndex].Substring(0,
            //                                                                     words[decodedWordIndex].
            //                                                                         LastIndexOf('?')));
            //                    //extract data from others
            //                    for (int i = decodedWordIndex + 1; i < words.Length; i++)
            //                    {
            //                        string[] parts = words[i].Split('?');
            //                        // Not encoded-word.
            //                        if (parts.Length == 5)
            //                        {
            //                            builder.Append(parts[3]);
            //                        }
            //                    }
            //                    builder.Append("?=");
            //                    notDecoded.Append(DecodeS(builder.ToString()));
            //                    builder = notDecoded;
            //                    break;
            //                }
            //            }
            //            else
            //            {
            //                notDecoded.Append(words[decodedWordIndex] + " ");
            //                decodedWordIndex++;
            //            }
            //        }

            //    }
            //}
            //catch
            //{
            //    return text;
            //}
            //return builder.ToString();
        }

        private static string Evalute(Match match)
        {
            try
            {
                if (match.Success)
                {
                    string charSet = match.Groups["charset"].Value;
                    string encoding = match.Groups["encoding"].Value.ToUpper();
                    string value = match.Groups["value"].Value;

                    Encoding enc = EncodingTools.GetEncodingByCodepageName(charSet) ??
                        ("utf-8".Equals(encoding,StringComparison.OrdinalIgnoreCase) ? Encoding.UTF8 : Encoding.Default);

                    if (encoding.ToLower().Equals("b"))
                        return enc.GetString(Convert.FromBase64String(value));
                    return Core.QDecode(enc, value);
                }
            }
            catch (Exception)
            {
                
            }
            return match.Value;
        }

        /// <summary>
        /// Decodes non-ascii word with MIME <b>encoded-word</b> method. Defined in RFC 2047 2.
        /// </summary>
        /// <param name="word">MIME encoded-word value.</param>
        /// <returns>Returns decoded word.</returns>
        /// <remarks>If <b>word</b> is not encoded-word or has invalid syntax, <b>word</b> is leaved as is.</remarks>
        /// <exception cref="ArgumentNullException">Is raised when <b>word</b> is null reference.</exception>
        public static string DecodeS(string word)
        {
            if (word == null)
            {
                throw new ArgumentNullException("word");
            }

            /* RFC 2047 2.
                encoded-word = "=?" charset "?" encoding "?" encoded-text "?="
             
               RFC 2231.
                encoded-word := "=?" charset ["*" language] "?" encoded-text "?="
            */

            try
            {
                return ParseRegex.Replace(word, new MatchEvaluator(Evalute));
            }
            catch
            {
                // Failed to parse encoded-word, leave it as is. RFC 2047 6.3.
                return word;
            }
        }

        /// <summary>
        /// Encodes specified text if it contains 8-bit chars, otherwise text won't be encoded.
        /// </summary>
        /// <param name="text">Text to encode.</param>
        /// <returns>Returns encoded text.</returns>
        public string Encode(string text)
        {
            if (MustEncode(text))
            {
                return EncodeS(m_Encoding, m_pCharset, text);
            }
            else
            {
                return text;
            }
        }

        /// <summary>
        /// Decodes specified encoded-word.
        /// </summary>
        /// <param name="text">Encoded-word value.</param>
        /// <returns>Returns decoded text.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
        public string Decode(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            return DecodeS(text);
        }

        #endregion
    }
}