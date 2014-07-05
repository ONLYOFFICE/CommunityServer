// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ude;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Contains several static methods providing encoding/decoding in various formats.
    /// </summary>
    public abstract class Codec
    {
        /// <summary>
        /// Detect whitespace between encoded words as stated by RFC2047
        /// </summary>
        private static Regex whiteSpace = new Regex(@"(\?=)(\s*)(=\?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        /// <summary>
        /// Detect encoded words as stated by RFC2047
        /// </summary>
        private static Regex encodedWord = new Regex(@"(=\?)(?<charset>[^\?]*)(\?)(?<encoding>[BbQq])(\?)(?<message>[^\?]*)(\?=)", RegexOptions.CultureInvariant);
        
        public static string GetUniqueString()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id.ToString()+System.DateTime.Now.ToString("yyMMddhhmmss")+System.DateTime.Now.Millisecond.ToString()+(new Random().GetHashCode());
        }
        /// <summary>
        /// Encodes the text in quoted-printable format conforming to the RFC 2045 and RFC 2046.
        /// </summary>
        /// <param name="fromCharset">The charset of input.</param>
        /// <param name="input">Data to be encoded.</param>
        /// <remarks>The lines are wrapped to a length of max 78 characters to ensure data integrity across some gateways.</remarks>
        /// <returns>The input encoded as 7bit quoted-printable data, in the form of max 78 characters lines.</returns>
        /// <example>
        /// The example below illustrates the encoding of a string in quoted-printable.
        /// <code>
        /// C#
        ///
        /// string input = "ActiveMail rocks ! Here are some non-ASCII characters =ç.";
        /// string output = Codec.ToQuotedPrintable(input,"iso-8859-1");
        /// </code>
        /// output returns A= ctiveMail rocks ! Here are some weird characters =3D=E7.
        ///
        /// Non ASCII characters have been encoded (=3D represents = and =E7 represents ç).
        /// </example>
        public static string ToQuotedPrintable(string input, string fromCharset)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Added this verification, there was an error here.
            if (input != null)
            {
                byte[] body = GetEncoding(fromCharset).GetBytes(input);
                int index = 0, wrap = 0, check = 0;

                byte decim = 0;

                for (index = 0; index < body.Length; index++)
                {
                    if (wrap == 0 && index + 73 - check < body.Length)
                    {
                        // it's a new line. Let's check if there is a bot in the next line.
                        while (body[index + 73 - check] == 46)
                        {
                            wrap++;
                            check++;
                            if (wrap == 72 || index + 73 - check >= body.Length)
                                break;
                        }
                        check = 0;
                    }

                    decim = body[index];
                    if ((decim < 33 || decim == 61 || decim > 126) && decim != 32)
                        sb.Append("=" + decim.ToString("X").PadLeft(2, '0'));
                    else
                        sb.Append((char)decim);

                    if (wrap >= 72)
                    {
                        sb.Append("=\r\n");
                        wrap = 0;
                    }
                    else
                        wrap++;

                    //if((sb.Length/72d)==System.Math.Round(sb.Length/72d) || ((sb.Length-1)/72d)==System.Math.Round((sb.Length-1)/72d) || ((sb.Length-2)/72d)==System.Math.Round((sb.Length-2)/72d) || ((sb.Length-3)/72d)==System.Math.Round((sb.Length-3)/72d))

            }
        }
            //sb.Append("=\r\n");
            return sb.ToString();
        }
        /// <summary>
        /// Encodes the given string in a format (specified in RFC 2047) that can be used in RFC 2822 headers to represent non-ASCII textual data.
        /// </summary>
        /// <param name="input">The string to be encoded (the Header field's value).</param>
        /// <param name="charset">The charset of the Header field's value.</param>
        /// <returns>The encoded string with only 7bit characters.</returns>
        /// <remarks>ActiveMail only encodes in this format using Base64, but the RFC2047Decode method also decodes string encoded in this format with quoted-printable.</remarks>
        /// <example>
        /// The example below illustrates the encoding of a string.
        /// <code>
        /// C#
        ///
        /// string input = "ActiveMail rocks ! Here are some non-ASCII characters =ç.";
        /// string output = Codec.RFC2047Encode(input,"iso-8859-1");
        /// </code>
        ///
        /// output returns =?iso-8859-1?B?QWN0aXZlTWFpbCByb2NrcyAhIEhlcmUgYXJlIHNvbWUgd2VpcmQgY2hhcmFjdGVycyA95y4=?=
        ///
        /// This value can be used as for example the subject of a message.
        /// If you suspect the text to contain non ASCII characters, do message.Subject = Codec.RFC2047Encode(yourRawValue);.
        /// </example>
        public static string RFC2047Encode(string input)
        {
            var emcoding = System.Text.Encoding.UTF8;
            return "=?" + emcoding.HeaderName + "?B?" + System.Convert.ToBase64String(emcoding.GetBytes(input)) + "?=";
        }
        /// <summary>
        /// Return encoding based on encoding name
        /// Tries to resolve some wrong-formatted encoding names
        /// </summary>
        /// <param name="encodingName"></param>
        /// <returns></returns>
        public static System.Text.Encoding GetEncoding(string encodingName)
        {
            System.Text.Encoding encoding = null;
            try
            {
                encoding = System.Text.Encoding.GetEncoding(encodingName);
            }
            catch
            {
                encoding = EncodingTools.GetEncodingByCodepageName(encodingName);

                if (encoding == null)
                    encoding = System.Text.Encoding.GetEncoding("iso-8859-1");
            }
            if (encoding == null)
            {
                try
                {
                    if (encodingName.ToUpper() == "UTF8")
                        encodingName = "UTF-8";
                    else if (encodingName.StartsWith("ISO") && char.IsDigit(encodingName, 3))
                        encodingName = encodingName.Insert(3, "-");
                    encodingName = encodingName.Replace("_", "-").ToUpper();
                    encoding = System.Text.Encoding.GetEncoding(encodingName);
                }
                catch
                {
                    encoding = System.Text.Encoding.GetEncoding("iso-8859-1");
                }
            }

            return encoding;
        }

        /// <summary>
        /// Decodes the given string from the format specified in RFC 2047 (=?charset?value?=).
        /// </summary>
        /// <param name="input">The string to be decoded.</param>
        /// <returns>The decoded string.</returns>
        /// <example>
        /// The example below illustrates the decoding of a string.
        /// <code>
        /// C#
        ///
        /// string input = "I once wrote that =?iso-8859-1?B?QWN0aXZlTWFpbCByb2NrcyAhIEhlcmUgYXJlIHNvbWUgd2VpcmQgY2hhcmFjdGVycyA95y4=?=";
        /// string output = Codec.RFC2047Decode(input);
        /// </code>
        ///
        /// output returns I once wrote that ActiveMail rocks ! Here are some weird characters =ç.
        /// </example>
        public static string RFC2047Decode(string input)
        {
            // Remove whitespaces
            string text = whiteSpace.Replace(
                input,
                delegate(Match a)
                {
                    return "?==?";
                });

            //SUPPORT_CODE_BEGIN
            //Todo: Code below added for automated charset detection
            //This code not part of RFC 2084
            var m = encodedWord.Match(text);
            if (m.Success)
            {
            //SUPPORT_CODE_END

                text = DecodeSameEncodedParts(text);
                // Decode encoded words
                text = encodedWord.Replace(
                    text,
                    delegate(Match curRes)
                        {
                            if (curRes.Groups["encoding"].Value.Equals("B", StringComparison.OrdinalIgnoreCase))
                            {
                                var message = curRes.Groups["message"].Value.Replace(" ", "");

                                var encoder = GetEncoding(curRes.Groups["charset"].Value);

                                try
                                {
                                    return encoder.GetString(Convert.FromBase64String(message));
                                }
                                catch
                                {
                                    int index = message.LastIndexOf("=");

                                    while (index != -1)
                                    {
                                        // remove the extra character

                                        message = message.Remove(index);
                                        try
                                        {
                                            return encoder.GetString(Convert.FromBase64String(message));
                                        }
                                        catch
                                        {
                                            index = message.LastIndexOf("=");
                                        }
                                    }

                                    throw;
                                }
                            }
                            else
                            {
                                string tmpbuffer = curRes.Groups["message"].Value.Replace("_", " ");
                                return Codec.FromQuotedPrintable(tmpbuffer, curRes.Groups["charset"].Value);
                            }
                        });
                //SUPPORT_CODE_BEGIN
            }
            else
            {
                var encoder = GetEncoding("");
                byte[] text_in_bytes = encoder.GetBytes(text);
                var charset_detector = new CharsetDetector();
                charset_detector.Feed(text_in_bytes, 0, text_in_bytes.Length);
                charset_detector.DataEnd();
                if (charset_detector.Charset != null)
                {
                    text = GetEncoding(charset_detector.Charset).GetString(text_in_bytes);
                }
            }
            //SUPPORT_CODE_END
            return text;
        }
        /// <summary>
        /// Decode same encoded parts of string in one
        /// </summary>
        /// <param name="input">encoded string</param>
        /// <returns>conmbined encoded string</returns>
        private static string DecodeSameEncodedParts(string input)
        {
            var decoded_bytes = new Dictionary<string, List<byte>>();

            string decoded_result = CombineSameEncodedParts(input, decoded_bytes);

            var i = 0;

            foreach (var item in decoded_bytes)
            {
                string encoding = item.Key.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries)[1];

                var encoder = GetEncoding(encoding);

                var decoded_string = encoder.GetString(item.Value.ToArray());

                decoded_result = decoded_result.Replace(string.Format("{0}%{1}%?=", item.Key, i), decoded_string);

                i++;
            }

            return (decoded_bytes.Count > 0 && !decoded_result.Contains("?=") && !decoded_result.Contains("=?")) ? decoded_result.Replace("=", "").Replace("_", " ") : decoded_result;
        }
        /// <summary>
        /// Combine same encoded parts of string in one
        /// </summary>
        /// <param name="input">encoded string</param>
        /// <param name="decoded_bytes">dictionary with key = content_type and value = byte array</param>
        /// <returns>conmbined encoded string</returns>
        private static string CombineSameEncodedParts(string input, IDictionary<string, List<byte>> decoded_bytes)
        {
            while (input.Contains("?==?"))
            {
                var matches = encodedWord.Matches(input);

                if (matches.Count > 1)
                {
                    var last_encoding = matches[0].Groups["encoding"].Value.ToLower();
                    var last_content_type = string.Format("=?{0}?{1}?", matches[0].Groups["charset"].Value.ToLower(), last_encoding);

                    var continue_cycle = false;

                    for (var i = 1; i < matches.Count; i++)
                    {
                        var cur_encoding = matches[i].Groups["encoding"].Value.ToLower();
                        var cur_content_type = string.Format("=?{0}?{1}?", matches[i].Groups["charset"].Value.ToLower(), cur_encoding);

                        if (last_content_type != cur_content_type)
                        {
                            last_encoding = cur_encoding;
                            last_content_type = cur_content_type;
                        }
                        else if (input[matches[i].Index - 1] == '=' && input[matches[i].Index - 2] == '?')
                        {
                            var last_message = matches[i - 1].Groups["message"];
                            var cur_message = matches[i].Groups["message"];

                            if (!last_message.Value.StartsWith("%"))
                            {
                                var tmp_array = String2DecodedArray(last_message.Value, last_encoding);

                                if (decoded_bytes.ContainsKey(last_content_type))
                                {
                                    decoded_bytes[last_content_type].AddRange(tmp_array);
                                }
                                else
                                    decoded_bytes.Add(last_content_type, new List<byte>(tmp_array));
                            }

                            if (decoded_bytes.ContainsKey(cur_content_type))
                                decoded_bytes[cur_content_type].AddRange(String2DecodedArray(cur_message.Value, cur_encoding));
                            else
                                throw new Exception("Smth wrong! Bad algorithm!");

                            input = input.Remove(matches[i - 1].Index, matches[i - 1].Length + matches[i].Length);
                            input = input.Insert(matches[i - 1].Index, string.Format("{0}%{1}%?=", last_content_type, decoded_bytes.Count - 1));

                            continue_cycle = true;
                            break;
                        }
                    }

                    if (continue_cycle)
                        continue;
                }

                break;
            }
            return input;
        }
        
        /// <summary>
        /// Convert string to base64 or quoted printable byte array
        /// </summary>
        /// <param name="input">encoded string</param>
        /// <param name="encoding">b = base64; q = quoted printable</param>
        /// <returns></returns>
        private static byte[] String2DecodedArray(string input, string encoding)
        {
            input = input.Replace(" ", "");
            var tmpArray = encoding == "b" ?
                Convert.FromBase64String(input) :
                ConvertFromQuotedPrintableString(input);
            return tmpArray;
        }
        /// <summary>
        /// Decodes text from quoted-printable format defined in RFC 2045 and RFC 2046.
        /// </summary>
        /// <param name="toCharset">The original charset of input.</param>
        /// <param name="input">Data to be decoded.</param>
        /// <returns>The decoded data.</returns>
        /// <example>
        /// The example below illustrates the decoding of a string from quoted-printable.
        /// <code>
        /// C#
        ///
        /// string input = "A=\r\nctiveMail rocks ! Here are some weird characters =3D=E7.";
        /// string output = Codec.FromQuotedPrintable(input,"iso-8859-1");
        /// </code>
        ///
        /// output returns ActiveMail rocks ! Here are some weird characters =ç.
        /// </example>
        public static string FromQuotedPrintable(string input, string toCharset)
        {
            try
            {
                var decoded = ConvertFromQuotedPrintableString(input);

                if (decoded == null)
                    throw new Exception("Illigal characters found");

                var result = GetEncoding(toCharset).GetString(decoded).TrimEnd('=');

                return result;
            }
            catch (Exception)
            {
                return input;
            }
        }
        /// <summary>
        /// Decodes text from quoted-printable format defined in RFC 2045 and RFC 2046.
        /// </summary>
        /// <param name="toCharset">The original charset of input.</param>
        /// <param name="input">Data to be decoded.</param>
        /// <returns>The decoded data.</returns>
        /// <example>
        /// The example below illustrates the decoding of a string from quoted-printable.
        /// <code>
        /// C#
        ///
        /// string input = "A=\r\nctiveMail rocks ! Here are some weird characters =3D=E7.";
        /// string output = Codec.FromQuotedPrintable(input,"iso-8859-1");
        /// </code>
        ///
        /// output returns ActiveMail rocks ! Here are some weird characters =ç.
        /// </example>
        public static byte[] ConvertFromQuotedPrintableString(string input)
        {
            var arr = new List<byte>();

            try
            {
                input = input.Replace("=\r\n", "") + "==";
                int i = 0;

                while (true)
                {
                    if (i <= (input.Length) - 3)
                    {
                        if (input[i] == '=' && input[i + 1] != '=')
                        {
                            try
                            {
                                arr.Add(System.Convert.ToByte(
                                    System.Int32.Parse(
                                        String.Concat((char)input[i + 1], 
                                        (char)input[i + 2]), 
                                        System.Globalization.NumberStyles.HexNumber)));

                                i += 3;
                            }
                            catch (Exception)
                            {
                                arr.Add((byte)input[i]);
                                i++;
                            }
                        }
                        else
                        {
                            arr.Add((byte)input[i]);
                            i++;
                        }
                    }
                    else break;
                }
            }
            catch (Exception)
            {
                /* BAD QUOTED PRINTABLE STRING */
            }

            return arr.ToArray();
        }

        private static string fileNameCleanerExpression = "[" + string.Join("", Array.ConvertAll(System.IO.Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))) + "]";
        private static Regex fileNameCleaner = new Regex(fileNameCleanerExpression, RegexOptions.Compiled);

        public static string CleanFileName(string fileName)
        {
            return fileNameCleaner.Replace(fileName, "");
        }

        public static string GetFieldName(string input)
        {
            switch (input)
            {
                case "content-id": return "Content-ID";
                case "message-id": return "Message-ID";
                case "content-md5": return "Content-HexMD5Digest";
                case "mime-version": return "MIME-Version";
                default: return Codec.Capitalize(input);
            }
        }
        internal static string Capitalize(string input)
        {
            string output = string.Empty;
            string[] parts = input.Split('-');
            foreach (string str in parts) output += str[0].ToString().ToUpper() + str.Substring(1) + "-";
            return output.TrimEnd('-');
        }
        /// <summary>
        /// Wraps the given string to a set of lines of a maximum given length.
        /// </summary>
        /// <param name="input">Data to be wrapped.</param>
        /// <param name="totalchars">The maximum length for each line.</param>
        /// <returns>The data as a set of lines of a maximum length.</returns>
        /// <remarks>This can be used to wrap lines to a maximum length of 78 characters to ensure data integrity across some gateways.</remarks>
        public static string Wrap(string input, int totalchars)
        {
            totalchars -= 3;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int i = 0;
            for (i = 0; (i + totalchars) < input.Length; i += totalchars)
            {
                sb.Append(string.Concat("\r\n", input.Substring(i, totalchars)));
            }
            return (string.Concat(sb.ToString(),"\r\n",input.Substring(i,input.Length-i))).TrimStart(new char[] {'\r','\n'});

            //totalchars -= 3; // NOT HERE: this value can be 78 or 77, so 78-3=75 or 77-3=74
            //System.Text.StringBuilder sb = new System.Text.StringBuilder(); // NOT HERE
            //int i = 0; // NOT HERE
            //// NOT HERE: The for increment is right, one possible problem is with the increment for i
            //// was zero, but there was not this possibility, since the possible values for totalchars
            //// are 75 or 74 maybe the input.length is too large, but I don't believe in this
            //// possibility, since second ansuman the problem is random.
            //for (i = 0; (i + totalchars) < input.Length; i += totalchars) // NOT HERE
            //{
            //    // NOT HERE: the possibility to got out of memory here is if the message have about
            //    // 1.2 GB - 1.6Gb (or ~600K - 800K characters), second answman this isn't.
            //    sb.Append("\r\n" + input.Substring(i, totalchars));
            //}
            //// MAYBE HERE... changed the string concatenation by + to string.Concat...
            //string ret = (sb.ToString() + "\r\n" + input.Substring(i, input.Length - i));
            //ret = ret.TrimStart(new char[] { '\r', '\n' });
            //return ret;
        }
        public static string GetCRCBase64(string base64input)
        {
            byte[] binput = Convert.FromBase64String(base64input);
            const long CRC24_INIT = 0xb704ceL;
            const long CRC24_POLY = 0x1864cfbL;
            long crc = CRC24_INIT;
            for (int i = 0; i < binput.Length; i++)
            {
                crc ^= (((long)binput[i]) << 16);
                for (int j = 0; j < 8; j++)
                {
                    crc <<= 1;
                    if ((crc & 0x1000000) == 0x1000000)
                        crc ^= CRC24_POLY;
                }
            }
            byte a = (byte)(crc >> 16);
            byte b = (byte)((crc & 65280) >> 8);
            byte c = (byte)(crc & 255);
            byte[] d = { a, b, c };
            return Convert.ToBase64String(d);
        }
        public static string GetCRCBase64(byte[] input)
        {
            return GetCRCBase64(Convert.ToBase64String(input));
        }
        public static string ToRadix64(byte[] input)
        {
            return Convert.ToBase64String(input) + "\r\n=" + GetCRCBase64(input);
        }
        public static byte[] FromRadix64(string input)
        {
            string radix64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            input = input.TrimEnd('=');
            int length = input.Length - (input.Length % 4);
            byte[] inbytes = new byte[input.Length];
            for (int j = 0; j < inbytes.Length; j++)
            {
                inbytes[j] = (byte)radix64Alphabet.IndexOf(input[j]);
            }
            List<Byte> outbytes = new List<Byte>();
            for (int i = 0; i < length / 4; i++)
            {
                outbytes.Add(((byte)((inbytes[i * 4] << 2) + (inbytes[i * 4 + 1] >> 4))));
                outbytes.Add(((byte)(((inbytes[i * 4 + 1] & 15) << 4) + (inbytes[i * 4 + 2] >> 2))));
                outbytes.Add(((byte)(((inbytes[i * 4 + 2] & 3) << 6) + inbytes[i * 4 + 3])));
            }
            if ((input.Length % 4) == 3)
            {
                outbytes.Add(((byte)((inbytes[inbytes.Length - 3] << 2) + (inbytes[inbytes.Length - 3 + 1] >> 4))));
                outbytes.Add(((byte)(((inbytes[inbytes.Length - 3 + 1] & 15) << 4) + (inbytes[inbytes.Length - 3 + 2] >> 2))));
            }
            if ((input.Length % 4) == 2)
            {
                outbytes.Add(((byte)((inbytes[inbytes.Length - 2] << 2) + (inbytes[inbytes.Length - 2 + 1] >> 4))));
            }

            byte[] result = new byte[outbytes.Count];
            outbytes.CopyTo(result);
            return result;
        }
        public static string ToBitString(byte input)
        {
            string output = string.Empty;
            output += ((input & 128) == 128) ? "1" : "0";
            output += ((input & 64) == 64) ? "1" : "0";
            output += ((input & 32) == 32) ? "1" : "0";
            output += ((input & 16) == 16) ? "1" : "0";
            output += ((input & 8) == 8) ? "1" : "0";
            output += ((input & 4) == 4) ? "1" : "0";
            output += ((input & 2) == 2) ? "1" : "0";
            output += ((input & 1) == 1) ? "1" : "0";
            return output;
        }
        public static string ToBitString(short input)
        {
            string output = string.Empty;
            output += ((input & 32768) == 32768) ? "1" : "0";
            output += ((input & 16384) == 16384) ? "1" : "0";
            output += ((input & 8192) == 8192) ? "1" : "0";
            output += ((input & 4096) == 4096) ? "1" : "0";
            output += ((input & 2048) == 2048) ? "1" : "0";
            output += ((input & 1024) == 1024) ? "1" : "0";
            output += ((input & 512) == 512) ? "1" : "0";
            output += ((input & 256) == 256) ? "1" : "0";
            output += ((input & 128) == 128) ? "1" : "0";
            output += ((input & 64) == 64) ? "1" : "0";
            output += ((input & 32) == 32) ? "1" : "0";
            output += ((input & 16) == 16) ? "1" : "0";
            output += ((input & 8) == 8) ? "1" : "0";
            output += ((input & 4) == 4) ? "1" : "0";
            output += ((input & 2) == 2) ? "1" : "0";
            output += ((input & 1) == 1) ? "1" : "0";
            return output;
        }
        internal static byte FromBitString(string input)
        {
            byte output = 0;
            if (input[7].Equals('1')) output++;
            if (input[6].Equals('1')) output += 2;
            if (input[5].Equals('1')) output += 4;
            if (input[4].Equals('1')) output += 8;
            if (input[3].Equals('1')) output += 16;
            if (input[2].Equals('1')) output += 32;
            if (input[1].Equals('1')) output += 64;
            if (input[0].Equals('1')) output += 128;
            return output;
        }
    }
}