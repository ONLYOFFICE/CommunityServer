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
using System.Runtime.InteropServices;
using System.IO;

namespace ASC.Mail.Net
{
    public static class EncodingTools
    {
        // this only contains ascii, default windows code page and unicode
        public static int[] PreferedEncodingsForStream;

        // this contains all codepages, sorted by preference and byte usage 
        public static int[] PreferedEncodings;

        // this contains all codepages, sorted by preference and byte usage 
        public static int[] AllEncodings;


        public static Dictionary<string, string> encoding_aliases;



        /// <summary>
        /// Static constructor that fills the default preferred codepages
        /// </summary>
        static EncodingTools()
        {

            List<int> streamEcodings = new List<int>();
            List<int> allEncodings = new List<int>();
            List<int> mimeEcodings = new List<int>();

            // asscii - most simple so put it in first place...
            streamEcodings.Add(Encoding.ASCII.CodePage);
            mimeEcodings.Add(Encoding.ASCII.CodePage);
            allEncodings.Add(Encoding.ASCII.CodePage);


            // add default 2nd for all encodings
            allEncodings.Add(Encoding.Default.CodePage);
            // default is single byte?
            if (Encoding.Default.IsSingleByte)
            {
                // put it in second place
                streamEcodings.Add(Encoding.Default.CodePage);
                mimeEcodings.Add(Encoding.Default.CodePage);
            }



            // prefer JIS over JIS-SHIFT (JIS is detected better than JIS-SHIFT)
            // this one does include cyrilic (strange but true)
            allEncodings.Add(50220);
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
            PreferedEncodingsForStream = streamEcodings.ToArray();


            // all singlebyte encodings
            foreach (EncodingInfo enc in Encoding.GetEncodings())
            {


                if (!enc.GetEncoding().IsSingleByte)
                    continue;

                if (!allEncodings.Contains(enc.CodePage))
                    allEncodings.Add(enc.CodePage);

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
                    if (!allEncodings.Contains(enc.CodePage))
                        allEncodings.Add(enc.CodePage);

                    // only add iso and IBM encodings to mime encodings 
                    if (enc.CodePage <= 1258)
                    {
                        mimeEcodings.Add(enc.CodePage);
                    }
                }
            }

            // add unicodes
            mimeEcodings.Add(Encoding.Unicode.CodePage);


            PreferedEncodings = mimeEcodings.ToArray();
            AllEncodings = allEncodings.ToArray();

            #region Fill in codepage aliases map
            encoding_aliases = new Dictionary<string, string>();

            string[] known_aliases = new string[] 
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

            for (int i = 0; i < known_aliases.Length; i += 2)
            {
                encoding_aliases[known_aliases[i]] = known_aliases[i + 1];
            }

            #endregion
        }


        /// <summary>
        /// Checks if specified string data is acii data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsAscii(string data)
        {
            // assume empty string to be ascii
            if ((data == null) || (data.Length == 0))
                return true;
            foreach (char c in data)
            {
                if ((int)c > 127)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the best Encoding for usage in mime encodings
        /// </summary>
        /// <param name="input">text to detect</param>
        /// <returns>the suggested encoding</returns>
        public static Encoding GetMostEfficientEncoding(string input)
        {
            return GetMostEfficientEncoding(input, PreferedEncodings);
        }

        /// <summary>
        /// Gets the best ISO Encoding for usage in a stream
        /// </summary>
        /// <param name="input">text to detect</param>
        /// <returns>the suggested encoding</returns>
        public static Encoding GetMostEfficientEncodingForStream(string input)
        {
            return GetMostEfficientEncoding(input, PreferedEncodingsForStream);
        }

        /// <summary>
        /// Gets the best fitting encoding from a list of possible encodings
        /// </summary>
        /// <param name="input">text to detect</param>
        /// <param name="preferedEncodings">an array of codepages</param>
        /// <returns>the suggested encoding</returns>
        public static Encoding GetMostEfficientEncoding(string input, int[] preferedEncodings)
        {
            Encoding enc = DetectOutgoingEncoding(input, preferedEncodings, true);
            // unicode.. hmmm... check for smallest encoding
            if (enc.CodePage == Encoding.Unicode.CodePage)
            {
                int byteCount = Encoding.UTF7.GetByteCount(input);
                enc = Encoding.UTF7;
                int bestByteCount = byteCount;

                // utf8 smaller?
                byteCount = Encoding.UTF8.GetByteCount(input);
                if (byteCount < bestByteCount)
                {
                    enc = Encoding.UTF8;
                    bestByteCount = byteCount;
                }

                // unicode smaller?
                byteCount = Encoding.Unicode.GetByteCount(input);
                if (byteCount < bestByteCount)
                {
                    enc = Encoding.Unicode;
                    bestByteCount = byteCount;
                }
            }
            else
            {

            }
            return enc;
        }

        public static Encoding DetectOutgoingEncoding(string input)
        {
            return DetectOutgoingEncoding(input, PreferedEncodings, true);
        }

        public static Encoding DetectOutgoingStreamEncoding(string input)
        {
            return DetectOutgoingEncoding(input, PreferedEncodingsForStream, true);
        }

        public static Encoding[] DetectOutgoingEncodings(string input)
        {
            return DetectOutgoingEncodings(input, PreferedEncodings, true);
        }

        public static Encoding[] DetectOutgoingStreamEncodings(string input)
        {
            return DetectOutgoingEncodings(input, PreferedEncodingsForStream, true);
        }

        private static Encoding DetectOutgoingEncoding(string input, int[] preferedEncodings, bool preserveOrder)
        {

            if (input == null)
                throw new ArgumentNullException("input");

            // empty strings can always be encoded as ASCII
            if (input.Length == 0)
                return Encoding.ASCII;

            Encoding result = Encoding.ASCII;

            // get the IMultiLanguage3 interface
            MultiLanguage.IMultiLanguage3 multilang3 = new MultiLanguage.CMultiLanguageClass();
            if (multilang3 == null)
                throw new System.Runtime.InteropServices.COMException("Failed to get IMultilang3");
            try
            {
                int[] resultCodePages = new int[preferedEncodings != null ? preferedEncodings.Length : Encoding.GetEncodings().Length];
                uint detectedCodepages = (uint)resultCodePages.Length;
                ushort specialChar = (ushort)'?';


                // get unmanaged arrays
                IntPtr pPrefEncs = preferedEncodings == null ? IntPtr.Zero : Marshal.AllocCoTaskMem(sizeof(uint) * preferedEncodings.Length);
                IntPtr pDetectedEncs = Marshal.AllocCoTaskMem(sizeof(uint) * resultCodePages.Length);

                try
                {
                    if (preferedEncodings != null)
                        Marshal.Copy(preferedEncodings, 0, pPrefEncs, preferedEncodings.Length);

                    Marshal.Copy(resultCodePages, 0, pDetectedEncs, resultCodePages.Length);

                    MultiLanguage.MLCPF options = MultiLanguage.MLCPF.MLDETECTF_VALID_NLS;
                    if (preserveOrder)
                        options |= MultiLanguage.MLCPF.MLDETECTF_PRESERVE_ORDER;

                    if (preferedEncodings != null)
                        options |= MultiLanguage.MLCPF.MLDETECTF_PREFERRED_ONLY;

                    multilang3.DetectOutboundCodePage(options,
                        input, (uint)input.Length,
                        pPrefEncs, (uint)(preferedEncodings == null ? 0 : preferedEncodings.Length),

                        pDetectedEncs, ref detectedCodepages,
                        ref specialChar);

                    // get result
                    if (detectedCodepages > 0)
                    {
                        int[] theResult = new int[detectedCodepages];
                        Marshal.Copy(pDetectedEncs, theResult, 0, theResult.Length);
                        result = Encoding.GetEncoding(theResult[0]);
                    }

                }
                finally
                {
                    if (pPrefEncs != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(pPrefEncs);
                    Marshal.FreeCoTaskMem(pDetectedEncs);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(multilang3);
            }
            return result;
        }

        public static Encoding[] DetectOutgoingEncodings(string input, int[] preferedEncodings, bool preserveOrder)
        {

            if (input == null)
                throw new ArgumentNullException("input");

            // empty strings can always be encoded as ASCII
            if (input.Length == 0)
                return new Encoding[] { Encoding.ASCII };

            List<Encoding> result = new List<Encoding>();

            // get the IMultiLanguage3 interface
            MultiLanguage.IMultiLanguage3 multilang3 = new MultiLanguage.CMultiLanguageClass();
            if (multilang3 == null)
                throw new System.Runtime.InteropServices.COMException("Failed to get IMultilang3");
            try
            {
                int[] resultCodePages = new int[preferedEncodings.Length];
                uint detectedCodepages = (uint)resultCodePages.Length;
                ushort specialChar = (ushort)'?';


                // get unmanaged arrays
                IntPtr pPrefEncs = Marshal.AllocCoTaskMem(sizeof(uint) * preferedEncodings.Length);
                IntPtr pDetectedEncs = preferedEncodings == null ? IntPtr.Zero : Marshal.AllocCoTaskMem(sizeof(uint) * resultCodePages.Length);

                try
                {
                    if (preferedEncodings != null)
                        Marshal.Copy(preferedEncodings, 0, pPrefEncs, preferedEncodings.Length);

                    Marshal.Copy(resultCodePages, 0, pDetectedEncs, resultCodePages.Length);

                    MultiLanguage.MLCPF options = MultiLanguage.MLCPF.MLDETECTF_VALID_NLS | MultiLanguage.MLCPF.MLDETECTF_PREFERRED_ONLY;
                    if (preserveOrder)
                        options |= MultiLanguage.MLCPF.MLDETECTF_PRESERVE_ORDER;

                    if (preferedEncodings != null)
                        options |= MultiLanguage.MLCPF.MLDETECTF_PREFERRED_ONLY;

                    // finally... call to DetectOutboundCodePage
                    multilang3.DetectOutboundCodePage(options,
                        input, (uint)input.Length,
                        pPrefEncs, (uint)(preferedEncodings == null ? 0 : preferedEncodings.Length),
                        pDetectedEncs, ref detectedCodepages,
                        ref specialChar);

                    // get result
                    if (detectedCodepages > 0)
                    {
                        int[] theResult = new int[detectedCodepages];
                        Marshal.Copy(pDetectedEncs, theResult, 0, theResult.Length);


                        // get the encodings for the codepages
                        for (int i = 0; i < detectedCodepages; i++)
                            result.Add(Encoding.GetEncoding(theResult[i]));

                    }

                }
                finally
                {
                    if (pPrefEncs != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(pPrefEncs);
                    Marshal.FreeCoTaskMem(pDetectedEncs);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(multilang3);
            }
            // nothing found
            return result.ToArray();
        }


        /// <summary>
        /// Detect the most probable codepage from an byte array
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <returns>the detected encoding or the default encoding if the detection failed</returns>
        public static Encoding DetectInputCodepage(byte[] input)
        {
            try
            {
                Encoding[] detected = DetectInputCodepages(input, 1);
                if (detected.Length > 0)
                    return detected[0];
                return Encoding.Default;
            }
            catch (COMException)
            {
                // return default codepage on error
                return Encoding.Default;
            }
        }

        /// <summary>
        /// Rerurns up to maxEncodings codpages that are assumed to be apropriate
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <param name="maxEncodings">maxiumum number of encodings to detect</param>
        /// <returns>an array of Encoding with assumed encodings</returns>
        public static Encoding[] DetectInputCodepages(byte[] input, int maxEncodings)
        {
            if (Path.DirectorySeparatorChar == '/')
            {
                // unix
                return new Encoding[0];
            }

            if (maxEncodings < 1)
                throw new ArgumentOutOfRangeException("at least one encoding must be returend", "maxEncodings");

            if (input == null)
                throw new ArgumentNullException("input");

            // empty strings can always be encoded as ASCII
            if (input.Length == 0)
                return new Encoding[] { Encoding.ASCII };

            // expand the string to be at least 256 bytes
            if (input.Length < 256)
            {
                byte[] newInput = new byte[256];
                int steps = 256 / input.Length;
                for (int i = 0; i < steps; i++)
                    Array.Copy(input, 0, newInput, input.Length * i, input.Length);

                int rest = 256 % input.Length;
                if (rest > 0)
                    Array.Copy(input, 0, newInput, steps * input.Length, rest);
                input = newInput;
            }

            List<Encoding> result = new List<Encoding>();

            // get the IMultiLanguage" interface
            MultiLanguage.IMultiLanguage2 multilang2 = new MultiLanguage.CMultiLanguageClass();
            if (multilang2 == null)
                throw new System.Runtime.InteropServices.COMException("Failed to get IMultilang2");
            try
            {
                MultiLanguage.DetectEncodingInfo[] detectedEncdings = new MultiLanguage.DetectEncodingInfo[maxEncodings];

                int scores = detectedEncdings.Length;
                int srcLen = input.Length;

                // setup options (none)   
                MultiLanguage.MLDETECTCP options = MultiLanguage.MLDETECTCP.MLDETECTCP_NONE;

                // finally... call to DetectInputCodepage
                multilang2.DetectInputCodepage(options, 0,
                    ref input[0], ref srcLen, ref detectedEncdings[0], ref scores);

                // get result
                if (scores > 0)
                {
                    for (int i = 0; i < scores; i++)
                    {
                        // add the result
                        result.Add(Encoding.GetEncoding((int)detectedEncdings[i].nCodePage));
                    }
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(multilang2);
            }
            // nothing found
            return result.ToArray();
        }


        /// <summary>
        /// Opens a text file and returns the content 
        /// encoded in the most probable encoding
        /// </summary>
        /// <param name="path">path to the souce file</param>
        /// <returns>the text content of the file</returns>
        public static string ReadTextFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            using (Stream fs = File.Open(path, FileMode.Open))
            {
                byte[] rawData = new byte[fs.Length];
                Encoding enc = DetectInputCodepage(rawData);
                return enc.GetString(rawData);
            }
        }

        /// <summary>
        /// Returns a stream reader for the given
        /// text file with the best encoding applied
        /// </summary>
        /// <param name="path">path to the file</param>
        /// <returns>a StreamReader for the file</returns>
        public static StreamReader OpenTextFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            return OpenTextStream(File.Open(path, FileMode.Open));
        }

        /// <summary>
        /// Creates a stream reader from a stream and detects
        /// the encoding form the first bytes in the stream
        /// </summary>
        /// <param name="stream">a stream to wrap</param>
        /// <returns>the newly created StreamReader</returns>
        public static StreamReader OpenTextStream(Stream stream)
        {
            // check stream parameter
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (!stream.CanSeek)
                throw new ArgumentException("the stream must support seek operations", "stream");

            // assume default encoding at first place
            Encoding detectedEncoding = Encoding.Default;

            // seek to stream start
            stream.Seek(0, SeekOrigin.Begin);

            // buffer for preamble and up to 512b sample text for dection
            byte[] buf = new byte[System.Math.Min(stream.Length, 512)];

            stream.Read(buf, 0, buf.Length);
            detectedEncoding = DetectInputCodepage(buf);
            // seek back to stream start
            stream.Seek(0, SeekOrigin.Begin);


            return new StreamReader(stream, detectedEncoding);

        }


        /// <summary>
        /// Create an Encoding instance by codepage name using additional jdk aliases
        /// Doesn't throw if codepage name is not supported
        /// </summary>
        /// <param name="codepage_name">codepage name</param>
        /// <returns>Created Encoding instance or null if codepage name is not supported</returns>
        public static Encoding GetEncodingByCodepageName(string codepage_name)
        {
            try
            {
                return GetEncodingByCodepageName_Throws(codepage_name);
            }
            catch (System.ArgumentException)
            {
                return null;
            }
        }


        /// <summary>
        /// Create an Encoding instance by codepage name using additional jdk aliases
        /// Throws if codepage name is not supported
        /// </summary>
        /// <param name="codepage_name">codepage name</param>
        /// <returns>Created Encoding instance</returns>
        /// <exception cref="System.ArgumentException">Throws if codepage name is not supported</exception>
        public static Encoding GetEncodingByCodepageName_Throws(string codepage_name)
        {
            string dealiased_name;
            if (!encoding_aliases.TryGetValue(codepage_name, out dealiased_name))
            {
                dealiased_name = codepage_name;
            }

            return System.Text.Encoding.GetEncoding(dealiased_name);
        }

    }


}
