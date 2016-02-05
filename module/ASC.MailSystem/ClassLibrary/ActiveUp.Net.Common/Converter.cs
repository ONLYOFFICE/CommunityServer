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
using System.Diagnostics.Contracts;
using System.Security;

namespace ActiveUp.Net.Mail
{
    public static class Converter
    {
        public static byte[] ToByteArray(int input)
        {
            byte a = (byte)(input >> 24);
            byte b = (byte)((input & 16711680) >> 16);
            byte c = (byte)((input & 65280) >> 8);
            byte d = (byte)(input & 255);
            byte[] e = { a, b, c, d };
            return e;
        }

        public static ulong ToULong(byte[] input)
        {
            ulong l = (((ulong)input[0]) << 56)
                + (((ulong)input[1]) << 48)
                + (((ulong)input[2]) << 40)
                + (((ulong)input[3]) << 32)
                + (((ulong)input[4]) << 24)
                + (((ulong)input[5]) << 16)
                + (((ulong)input[6]) << 8)
                + ((ulong)input[7]);
                return l;
        }

        public static short ToShort(byte[] input)
        {
            short l = (short)((((short)input[0]) << 8) + (short)input[1]);
            return l;
        }

        public static int ToInt(byte[] input)
        {
            int l = (((int)input[0]) << 24)
                + (((int)input[1]) << 16)
                + (((int)input[2]) << 8)
                + (int)input[3];
            return l;
        }

        public static long ToLong(byte[] input)
        {
            long l = ((long)input[0] << 56)
                | ((long)input[1] << 48)
                | ((long)input[2] << 40)
                | ((long)input[3] << 32)
                | ((long)input[4] << 24)
                | ((long)input[5] << 16)
                | ((long)input[6] << 8)
                | (long)input[7];
            return l;
        }

        public static DateTime UnixTimeStampToDateTime(int timeStamp)
        {
            return new DateTime(1970, 1, 1).AddSeconds(timeStamp);
        }

        /// <summary>
        /// Converts the specified string, which encodes binary data as Base64 digits, to the equivalent byte array.
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns>The array of bytes represented by the specifed Base64 string.</returns>
        [SecuritySafeCritical]
        public static Byte[] FromBase64String(String s, int offset, int length)
        {

            // "s" is an unfortunate parameter name, but we need to keep it for backward compat.

            if (s == null)
                throw new ArgumentNullException("s");

            unsafe
            {
                fixed (Char* sPtr = s)
                {

                    return FromBase64CharPtr(sPtr + offset, length);
                }
            }
        }

        /// <summary>
        /// Convert Base64 encoding characters to bytes:
        ///  - Compute result length exactly by actually walking the input;
        ///  - Allocate new result array based on computation;
        ///  - Decode input into the new array;
        /// </summary>
        /// <param name="inputPtr">Pointer to the first input char</param>
        /// <param name="inputLength">Number of input chars</param>
        /// <returns></returns>
        [SecurityCritical]
        private static unsafe Byte[] FromBase64CharPtr(Char* inputPtr, Int32 inputLength)
        {

            // The validity of parameters much be checked by callers, thus we are Critical here.

            Contract.Assert(0 <= inputLength);

            // We need to get rid of any trailing white spaces.
            // Otherwise we would be rejecting input such as "abc= ":
            while (inputLength > 0)
            {
                Int32 lastChar = inputPtr[inputLength - 1];
                if (lastChar != (Int32)' ' && lastChar != (Int32)'\n' && lastChar != (Int32)'\r' && lastChar != (Int32)'\t')
                    break;
                inputLength--;
            }

            // Compute the output length:
            Int32 resultLength = FromBase64_ComputeResultLength(inputPtr, inputLength);

            Contract.Assert(0 <= resultLength);

            // resultLength can be zero. We will still enter FromBase64_Decode and process the input.
            // It may either simply write no bytes (e.g. input = " ") or throw (e.g. input = "ab").

            // Create result byte blob:
            Byte[] decodedBytes = new Byte[resultLength];

            // Convert Base64 chars into bytes:
            Int32 actualResultLength;
            fixed (Byte* decodedBytesPtr = decodedBytes)
                actualResultLength = FromBase64_Decode(inputPtr, inputLength, decodedBytesPtr, resultLength);

            // Note that actualResultLength can differ from resultLength if the caller is modifying the array
            // as it is being converted. Silently ignore the failure.
            // Consider throwing exception in an non in-place release.

            // We are done:
            return decodedBytes;
        }


        /// <summary>
        /// Decode characters representing a Base64 encoding into bytes:
        /// Walk the input. Every time 4 chars are read, convert them to the 3 corresponding output bytes.
        /// This method is a bit lengthy on purpose. We are trying to avoid jumps to helpers in the loop
        /// to aid performance.
        /// </summary>
        /// <param name="startInputPtr">Pointer to first input char</param>
        /// <param name="inputLength">Number of input chars</param>
        /// <param name="startDestPtr">Pointer to location for teh first result byte</param>
        /// <param name="destLength">Max length of the preallocated result buffer</param>
        /// <returns>If the result buffer was not large enough to write all result bytes, return -1;
        /// Otherwise return the number of result bytes actually produced.</returns>
        [SecurityCritical]
        private static unsafe Int32 FromBase64_Decode(Char* startInputPtr, Int32 inputLength, Byte* startDestPtr, Int32 destLength)
        {

            // You may find this method weird to look at. Its written for performance, not aesthetics.
            // You will find unrolled loops label jumps and bit manipulations.

            const UInt32 intA = (UInt32)'A';
            const UInt32 inta = (UInt32)'a';
            const UInt32 int0 = (UInt32)'0';
            const UInt32 intEq = (UInt32)'=';
            const UInt32 intPlus = (UInt32)'+';
            const UInt32 intSlash = (UInt32)'/';
            const UInt32 intSpace = (UInt32)' ';
            const UInt32 intTab = (UInt32)'\t';
            const UInt32 intNLn = (UInt32)'\n';
            const UInt32 intCRt = (UInt32)'\r';
            const UInt32 intAtoZ = (UInt32)('Z' - 'A');  // = ('z' - 'a')
            const UInt32 int0to9 = (UInt32)('9' - '0');

            Char* inputPtr = startInputPtr;
            Byte* destPtr = startDestPtr;

            // Pointers to the end of input and output:
            Char* endInputPtr = inputPtr + inputLength;
            Byte* endDestPtr = destPtr + destLength;

            // Current char code/value:
            UInt32 currCode;

            // This 4-byte integer will contain the 4 codes of the current 4-char group.
            // Eeach char codes for 6 bits = 24 bits.
            // The remaining byte will be FF, we use it as a marker when 4 chars have been processed.            
            UInt32 currBlockCodes = 0x000000FFu;

            unchecked
            {
                while (true)
                {

                    // break when done:
                    if (inputPtr >= endInputPtr)
                        goto _AllInputConsumed;

                    // Get current char:
                    currCode = (UInt32)(*inputPtr);
                    inputPtr++;

                    // Determine current char code:

                    if (currCode - intA <= intAtoZ)
                        currCode -= intA;

                    else if (currCode - inta <= intAtoZ)
                        currCode -= (inta - 26u);

                    else if (currCode - int0 <= int0to9)
                        currCode -= (int0 - 52u);

                    else
                    {
                        // Use the slower switch for less common cases:
                        switch (currCode)
                        {

                            // Significant chars:
                            case intPlus: currCode = 62u;
                                break;

                            case intSlash: currCode = 63u;
                                break;

                            // Legal no-value chars (we ignore these):
                            case intCRt:
                            case intNLn:
                            case intSpace:
                            case intTab:
                                continue;

                            // The equality char is only legal at the end of the input.
                            // Jump after the loop to make it easier for the JIT register predictor to do a good job for the loop itself:
                            case intEq:
                                goto _EqualityCharEncountered;

                            // Other chars are illegal:
                            default:
                                throw new FormatException("Format_BadBase64Char");
                        }
                    }

                    // Ok, we got the code. Save it:
                    currBlockCodes = (currBlockCodes << 6) | currCode;

                    // Last bit in currBlockCodes will be on after in shifted right 4 times:
                    if ((currBlockCodes & 0x80000000u) != 0u)
                    {

                        if ((Int32)(endDestPtr - destPtr) < 3)
                            return -1;

                        *(destPtr) = (Byte)(currBlockCodes >> 16);
                        *(destPtr + 1) = (Byte)(currBlockCodes >> 8);
                        *(destPtr + 2) = (Byte)(currBlockCodes);
                        destPtr += 3;

                        currBlockCodes = 0x000000FFu;
                    }

                }
            }  // unchecked while

            // 'd be nice to have an assert that we never get here, but CS0162: Unreachable code detected.
        // Contract.Assert(false, "We only leave the above loop by jumping; should never get here.");

            // We jump here out of the loop if we hit an '=':
        _EqualityCharEncountered:

            Contract.Assert(currCode == intEq);

            // Recall that inputPtr is now one position past where '=' was read.
            // '=' can only be at the last input pos:
            if (inputPtr == endInputPtr)
            {

                // Code is zero for trailing '=':
                currBlockCodes <<= 6;

                // The '=' did not complete a 4-group. The input must be bad:
                if ((currBlockCodes & 0x80000000u) == 0u)
                    throw new FormatException("Format_BadBase64CharArrayLength");

                if ((int)(endDestPtr - destPtr) < 2)  // Autch! We underestimated the output length!
                    return -1;

                // We are good, store bytes form this past group. We had a single "=", so we take two bytes:
                *(destPtr++) = (Byte)(currBlockCodes >> 16);
                *(destPtr++) = (Byte)(currBlockCodes >> 8);

                currBlockCodes = 0x000000FFu;

            }
            else
            { // '=' can also be at the pre-last position iff the last is also a '=' excluding the white spaces:

                // We need to get rid of any intermediate white spaces.
                // Otherwise we would be rejecting input such as "abc= =":
                while (inputPtr < (endInputPtr - 1))
                {
                    Int32 lastChar = *(inputPtr);
                    if (lastChar != (Int32)' ' && lastChar != (Int32)'\n' && lastChar != (Int32)'\r' && lastChar != (Int32)'\t')
                        break;
                    inputPtr++;
                }

                if (inputPtr == (endInputPtr - 1) && *(inputPtr) == '=')
                {

                    // Code is zero for each of the two '=':
                    currBlockCodes <<= 12;

                    // The '=' did not complete a 4-group. The input must be bad:
                    if ((currBlockCodes & 0x80000000u) == 0u)
                        throw new FormatException("Format_BadBase64CharArrayLength");

                    if ((Int32)(endDestPtr - destPtr) < 1)  // Autch! We underestimated the output length!
                        return -1;

                    // We are good, store bytes form this past group. We had a "==", so we take only one byte:
                    *(destPtr++) = (Byte)(currBlockCodes >> 16);

                    currBlockCodes = 0x000000FFu;

                }
                else  // '=' is not ok at places other than the end:
                    throw new FormatException("Format_BadBase64Char");

            }

            // We get here either from above or by jumping out of the loop:
        _AllInputConsumed:

            // The last block of chars has less than 4 items
            if (currBlockCodes != 0x000000FFu)
                throw new FormatException("Format_BadBase64CharArrayLength");

            // Return how many bytes were actually recovered:
            return (Int32)(destPtr - startDestPtr);

        } // Int32 FromBase64_Decode(...)


        /// <summary>
        /// Compute the number of bytes encoded in the specified Base 64 char array:
        /// Walk the entire input counting white spaces and padding chars, then compute result length
        /// based on 3 bytes per 4 chars.
        /// </summary>
        [SecurityCritical]
        private static unsafe Int32 FromBase64_ComputeResultLength(Char* inputPtr, Int32 inputLength)
        {

            const UInt32 intEq = (UInt32)'=';
            const UInt32 intSpace = (UInt32)' ';

            Contract.Assert(0 <= inputLength);

            Char* inputEndPtr = inputPtr + inputLength;
            Int32 usefulInputLength = inputLength;
            Int32 padding = 0;

            while (inputPtr < inputEndPtr)
            {

                UInt32 c = (UInt32)(*inputPtr);
                inputPtr++;

                // We want to be as fast as possible and filter out spaces with as few comparisons as possible.
                // We end up accepting a number of illegal chars as legal white-space chars.
                // This is ok: as soon as we hit them during actual decode we will recognise them as illegal and throw.
                if (c <= intSpace)
                    usefulInputLength--;

                else if (c == intEq)
                {
                    usefulInputLength--;
                    padding++;
                }
            }

            Contract.Assert(0 <= usefulInputLength);

            // For legal input, we can assume that 0 <= padding < 3. But it may be more for illegal input.
            // We will notice it at decode when we see a '=' at the wrong place.
            Contract.Assert(0 <= padding);

            // Perf: reuse the variable that stored the number of '=' to store the number of bytes encoded by the
            // last group that contains the '=':
            if (padding != 0)
            {

                if (padding == 1)
                    padding = 2;
                else if (padding == 2)
                    padding = 1;
                else
                    throw new FormatException("Format_BadBase64Char");
            }

            // Done:
            return (usefulInputLength / 4) * 3 + padding;
        }

        public static byte[] FromQuotedPrintableString(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException("input");

            var arr = new List<byte>();

            input = string.Format("{0}==", input.Replace("=\r\n", ""));
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

            return arr.ToArray();
        }
    }
}
