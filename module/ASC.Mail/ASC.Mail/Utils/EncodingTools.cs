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


using System;
using System.Collections.Generic;
using System.Text;
using Ude;

namespace ASC.Mail.Utils
{
    public static class EncodingTools
    {
        // this only contains ascii, default windows code page and unicode
        public static int[] preferedEncodingsForStream;

        // this contains all codepages, sorted by preference and byte usage 
        public static int[] preferedEncodings;

        // this contains all codepages, sorted by preference and byte usage 
        public static int[] allEncodings;

        public static Dictionary<string, string> encodingAliases;

        /// <summary>
        /// Static constructor that fills the default preferred codepages
        /// </summary>
        static EncodingTools()
        {
            var streamEcodings = new List<int>();
            var encodings = new List<int>();
            var mimeEcodings = new List<int>();

            // asscii - most simple so put it in first place...
            streamEcodings.Add(Encoding.ASCII.CodePage);
            mimeEcodings.Add(Encoding.ASCII.CodePage);
            encodings.Add(Encoding.ASCII.CodePage);


            // add default 2nd for all encodings
            encodings.Add(Encoding.Default.CodePage);
            // default is single byte?
            if (Encoding.Default.IsSingleByte)
            {
                // put it in second place
                streamEcodings.Add(Encoding.Default.CodePage);
                mimeEcodings.Add(Encoding.Default.CodePage);
            }

            // prefer JIS over JIS-SHIFT (JIS is detected better than JIS-SHIFT)
            // this one does include cyrilic (strange but true)
            encodings.Add(50220);
            mimeEcodings.Add(50220);


            // always allow unicode flavours for streams (they all have a preamble)
            streamEcodings.Add(Encoding.Unicode.CodePage);
            foreach (EncodingInfo enc in Encoding.GetEncodings())
            {
                if (!streamEcodings.Contains(enc.CodePage))
                {
                    Encoding encoding = Encoding.GetEncoding(enc.CodePage);
                    if (encoding.GetPreamble().Length > 0)
                        streamEcodings.Add(enc.CodePage);
                }
            }

            // stream is done here
            preferedEncodingsForStream = streamEcodings.ToArray();

            // all singlebyte encodings
            foreach (EncodingInfo enc in Encoding.GetEncodings())
            {
                if (!enc.GetEncoding().IsSingleByte)
                    continue;

                if (!encodings.Contains(enc.CodePage))
                    encodings.Add(enc.CodePage);

                // only add iso and IBM encodings to mime encodings 
                if (enc.CodePage <= 1258)
                {
                    mimeEcodings.Add(enc.CodePage);
                }
            }

            // add the rest (multibyte)
            foreach (EncodingInfo enc in Encoding.GetEncodings())
            {
                if (!enc.GetEncoding().IsSingleByte)
                {
                    if (!encodings.Contains(enc.CodePage))
                        encodings.Add(enc.CodePage);

                    // only add iso and IBM encodings to mime encodings 
                    if (enc.CodePage <= 1258)
                    {
                        mimeEcodings.Add(enc.CodePage);
                    }
                }
            }

            // add unicodes
            mimeEcodings.Add(Encoding.Unicode.CodePage);

            preferedEncodings = mimeEcodings.ToArray();
            allEncodings = encodings.ToArray();

            #region Fill in codepage aliases map
            encodingAliases = new Dictionary<string, string>();

            var knownAliases = new[]
            {
                // first name is an alias. The second one is a registered in IANA name
                // All aliases (first column) must be in lower case
                // Please don't append aliases to aliases. This will lead to exception in GetEncodingByCodepageName()

                // It was investigated that only Java aliases are not present in Encoding class internal lists.
                // All windows aliases may be found here: http://www.lingoes.net/en/translator/codepage.htm
                // Java aliases have been obtained from here: http://docs.oracle.com/javase/1.4.2/docs/guide/intl/encoding.doc.html

                "cp1250",       "windows-1250", // Windows Eastern European 
                "cp-1250",      "windows-1250",
                "cp1251",       "windows-1251", // Windows Cyrillic 
                "cp-1251",      "windows-1251",
                "cp1252",       "windows-1252", // Windows Latin-1 
                "cp-1252",      "windows-1252",
                "cp1253",       "windows-1253", // Windows Greek 
                "cp-1253",      "windows-1253",
                "cp1254",       "windows-1254", // Windows Turkish 
                "cp-1254",      "windows-1254",
                "cp1257",       "windows-1257", // Windows Baltic 
                "cp-1257",      "windows-1257",
                "iso8859_1",    "iso-8859-1",   // ISO 8859-1, Latin Alphabet No. 1 
                "iso8859_2",    "iso-8859-2",   // Latin Alphabet No. 2
                "iso8859_4",    "iso-8859-4",   // Latin Alphabet No. 4
                "iso8859_5",    "iso-8859-5",   // Latin/Cyrillic Alphabet 
                "iso8859_7",    "iso-8859-7",   // Latin/Greek Alphabet  
                "iso8859_9",    "iso-8859-9",   // Latin Alphabet No. 9
                "iso8859_13",   "iso-8859-13",  // Latin Alphabet No. 13
                "iso8859_15",   "iso-8859-15",  // Latin Alphabet No. 15
                "koi8_r",       "koi8-r",       // KOI8-R, Russian 
                "utf8",         "utf-8",        // Eight-bit UCS Transformation Format 
                "utf16",        "utf-16",       // Sixteen-bit UCS Transformation Format, byte order identified by an optional byte-order mark
                "unicodebigunmarked",       "utf-16be", // Sixteen-bit Unicode Transformation Format, big-endian byte order
                "unicodelittleunmarked",    "utf-16le", // Sixteen-bit Unicode Transformation Format, little-endian byte order
                
                "cp1255",       "windows-1255", // Windows Hebrew 
                "cp-1255",      "windows-1255",
                "cp1256",       "windows-1256", // Windows Arabic 
                "cp-1256",      "windows-1256",
                "cp1258",       "windows-1258", // Windows Vietnamese 
                "cp-1258",      "windows-1258",
                "iso8859_3",    "iso-8859-3",   // Latin Alphabet No. 3
                "iso8859_6",    "iso-8859-6",   // Latin/Arabic Alphabet 
                "iso8859_8",    "iso-8859-8",   // Latin/Hebrew Alphabet  
                "ms932",        "shift_jis",    // Windows Japanese   
                "windows-31j",  "shift_jis",    // Windows Japanese   
                "euc_jp",       "euc-jp",       // JISX 0201, 0208 and 0212, EUC encoding Japanese   
                "euc_jp_linux", "x-euc-jp-linux", // JISX 0201, 0208 , EUC encoding Japanese   
                "iso2022jp",    "iso-2022-jp",  // JIS X 0201, 0208, in ISO 2022 form, Japanese
                "ms936",        "x-mswin-936",  // Windows Simplified Chinese   
                "euc_cn",       "x-euc-cn",     // GB2312, EUC encoding, Simplified Chinese   
                "iscii91",      "iscii91",      // Windows Japanese   
                "ms949",        "x-windows-949",  // Windows Korean   
                "iso2022kr",    "iso-2022-kr",  // ISO 2022 KR, Korean   
                "ms950",        "x-windows-950",  // Windows Traditional Chinese   
                "ms950_hkscs",  "x-ms950-hkscs",  // Windows Traditional Chinese with Hong Kong extensions   
                "euc-tw",       "x-euc-tw",     // CNS11643 (Plane 1-3), EUC encoding, Traditional Chinese   
                "tis620",       "tis-620",      // TIS620, Thai   

            };

            for (int i = 0; i < knownAliases.Length; i += 2)
            {
                encodingAliases[knownAliases[i]] = knownAliases[i + 1];
            }

            #endregion
        }

        /// <summary>
        /// Create an Encoding instance by codepage name using additional jdk aliases
        /// Doesn't throw if codepage name is not supported
        /// </summary>
        /// <param name="codepageName">codepage name</param>
        /// <returns>Created Encoding instance or null if codepage name is not supported</returns>
        public static Encoding GetEncodingByCodepageName(string codepageName)
        {
            try
            {
                return GetEncodingByCodepageName_Throws(codepageName.ToLower());
            }
            catch (ArgumentException)
            {
                //Logger.AddEntry(string.Format("GetEncodingByCodepageName Error : {0}", ex));
                return null;
            }
        }

        /// <summary>
        /// Create an Encoding instance by codepage name using additional jdk aliases
        /// Throws if codepage name is not supported
        /// </summary>
        /// <param name="codepageName">codepage name</param>
        /// <returns>Created Encoding instance</returns>
        /// <exception cref="System.ArgumentException">Throws if codepage name is not supported</exception>
        public static Encoding GetEncodingByCodepageName_Throws(string codepageName)
        {
            string dealiasedName;
            if (!encodingAliases.TryGetValue(codepageName.ToLower(), out dealiasedName))
            {
                dealiasedName = codepageName;
            }

            return Encoding.GetEncoding(dealiasedName);
        }

        public static string DetectCharset(byte[] bytes)
        {
            try
            {
                var charsetDetector = new CharsetDetector();

                charsetDetector.Feed(bytes, 0, bytes.Length);

                charsetDetector.DataEnd();

                return charsetDetector.Charset == null ? null : charsetDetector.Charset.ToLowerInvariant();
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
