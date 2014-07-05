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

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="StreamManipulator.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using System;

namespace ASC.Xmpp.Core.IO.Compression.Streams
{

    #region usings

    #endregion

    /// <summary>
    ///   This class allows us to retrieve a specified number of bits from the input buffer, as well as copy big byte blocks. It uses an int buffer to store up to 31 bits for direct manipulation. This guarantees that we can get at least 16 bits, but we only need at most 15, so this is all safe. There are some optimizations in this class, for example, you must never peek more than 8 bits more than needed, and you must first peek bits before you may drop them. This is not a general purpose class but optimized for the behaviour of the Inflater. authors of the original java version : John Leuner, Jochen Hoenicke
    /// </summary>
    public class StreamManipulator
    {
        #region Members

        /// <summary>
        /// </summary>
        private int bits_in_buffer;

        /// <summary>
        /// </summary>
        private uint buffer;

        /// <summary>
        /// </summary>
        private byte[] window;

        /// <summary>
        /// </summary>
        private int window_end;

        /// <summary>
        /// </summary>
        private int window_start;

        #endregion

        #region Properties

        /// <summary>
        ///   Gets the number of bits available in the bit buffer. This must be only called when a previous PeekBits() returned -1.
        /// </summary>
        /// <returns> the number of bits available. </returns>
        public int AvailableBits
        {
            get { return bits_in_buffer; }
        }

        /// <summary>
        ///   Gets the number of bytes available.
        /// </summary>
        /// <returns> The number of bytes available. </returns>
        public int AvailableBytes
        {
            get { return window_end - window_start + (bits_in_buffer >> 3); }
        }

        /// <summary>
        ///   Returns true when SetInput can be called
        /// </summary>
        public bool IsNeedingInput
        {
            get { return window_start == window_end; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Get the next n bits but don't increase input pointer. n must be less or equal 16 and if this call succeeds, you must drop at least n - 8 bits in the next call.
        /// </summary>
        /// <param name="n"> </param>
        /// <returns> the value of the bits, or -1 if not enough bits available. */ </returns>
        public int PeekBits(int n)
        {
            if (bits_in_buffer < n)
            {
                if (window_start == window_end)
                {
                    return -1; // ok
                }

                buffer |=
                    (uint)
                    ((window[window_start++] & 0xff | (window[window_start++] & 0xff) << 8) << bits_in_buffer);
                bits_in_buffer += 16;
            }

            return (int) (buffer & ((1 << n) - 1));
        }

        /// <summary>
        ///   Drops the next n bits from the input. You should have called PeekBits with a bigger or equal n before, to make sure that enough bits are in the bit buffer.
        /// </summary>
        /// <param name="n"> </param>
        public void DropBits(int n)
        {
            buffer >>= n;
            bits_in_buffer -= n;
        }

        /// <summary>
        ///   Gets the next n bits and increases input pointer. This is equivalent to PeekBits followed by dropBits, except for correct error handling.
        /// </summary>
        /// <param name="n"> </param>
        /// <returns> the value of the bits, or -1 if not enough bits available. </returns>
        public int GetBits(int n)
        {
            int bits = PeekBits(n);
            if (bits >= 0)
            {
                DropBits(n);
            }

            return bits;
        }

        /// <summary>
        ///   Skips to the next byte boundary.
        /// </summary>
        public void SkipToByteBoundary()
        {
            buffer >>= bits_in_buffer & 7;
            bits_in_buffer &= ~7;
        }

        /// <summary>
        ///   Copies length bytes from input buffer to output buffer starting at output[offset]. You have to make sure, that the buffer is byte aligned. If not enough bytes are available, copies fewer bytes.
        /// </summary>
        /// <param name="output"> The buffer to copy bytes to. </param>
        /// <param name="offset"> The offset in the buffer at which copying starts </param>
        /// <param name="length"> The length to copy, 0 is allowed. </param>
        /// <returns> The number of bytes copied, 0 if no bytes were available. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Length is less than zero</exception>
        /// <exception cref="InvalidOperationException">Bit buffer isnt byte aligned</exception>
        public int CopyBytes(byte[] output, int offset, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if ((bits_in_buffer & 7) != 0)
            {
                /* bits_in_buffer may only be 0 or a multiple of 8 */
                throw new InvalidOperationException("Bit buffer is not byte aligned!");
            }

            int count = 0;
            while (bits_in_buffer > 0 && length > 0)
            {
                output[offset++] = (byte) buffer;
                buffer >>= 8;
                bits_in_buffer -= 8;
                length--;
                count++;
            }

            if (length == 0)
            {
                return count;
            }

            int avail = window_end - window_start;
            if (length > avail)
            {
                length = avail;
            }

            Array.Copy(window, window_start, output, offset, length);
            window_start += length;

            if (((window_start - window_end) & 1) != 0)
            {
                /* We always want an even number of bytes in input, see peekBits */
                buffer = (uint) (window[window_start++] & 0xff);
                bits_in_buffer = 8;
            }

            return count + length;
        }

        /// <summary>
        ///   resets state and empties internal buffers
        /// </summary>
        public void Reset()
        {
            buffer = (uint) (window_start = window_end = bits_in_buffer = 0);
        }

        /// <summary>
        ///   Add more input for consumption. Only call when IsNeedingInput returns true
        /// </summary>
        /// <param name="buf"> data to be input </param>
        /// <param name="off"> offset of first byte of input </param>
        /// <param name="len"> length of input </param>
        public void SetInput(byte[] buf, int off, int len)
        {
            if (window_start < window_end)
            {
                throw new InvalidOperationException("Old input was not completely processed");
            }

            int end = off + len;

            /* We want to throw an ArrayIndexOutOfBoundsException early.  The
			* check is very tricky: it also handles integer wrap around.
			*/
            if (0 > off || off > end || end > buf.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((len & 1) != 0)
            {
                /* We always want an even number of bytes in input, see peekBits */
                buffer |= (uint) ((buf[off++] & 0xff) << bits_in_buffer);
                bits_in_buffer += 8;
            }

            window = buf;
            window_start = off;
            window_end = end;
        }

        #endregion
    }
}