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

using System;

namespace ASC.Xmpp.Core.IO.Compression
{
    /// <summary>
    ///   This class is general purpose class for writing data to a buffer. It allows you to write bits as well as bytes Based on DeflaterPending.java author of the original java version : Jochen Hoenicke
    /// </summary>
    public class PendingBuffer
    {
        #region Members

        /// <summary>
        /// </summary>
        private int bitCount;

        /// <summary>
        /// </summary>
        private uint bits;

        /// <summary>
        ///   Internal work buffer
        /// </summary>
        protected byte[] buf;

        /// <summary>
        /// </summary>
        private int end;

        /// <summary>
        /// </summary>
        private int start;

        #endregion

        #region Constructor

        /// <summary>
        ///   construct instance using default buffer size of 4096
        /// </summary>
        public PendingBuffer() : this(4096)
        {
        }

        /// <summary>
        ///   construct instance using specified buffer size
        /// </summary>
        /// <param name="bufsize"> size to use for internal buffer </param>
        public PendingBuffer(int bufsize)
        {
            buf = new byte[bufsize];
        }

        #endregion

        #region Properties

        /// <summary>
        ///   The number of bits written to the buffer
        /// </summary>
        public int BitCount
        {
            get { return bitCount; }
        }

        /// <summary>
        ///   Indicates if buffer has been flushed
        /// </summary>
        public bool IsFlushed
        {
            get { return end == 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Clear internal state/buffers
        /// </summary>
        public void Reset()
        {
            start = end = bitCount = 0;
        }

        /// <summary>
        ///   write a byte to buffer
        /// </summary>
        /// <param name="b"> value to write </param>
        public void WriteByte(int b)
        {
            buf[end++] = (byte) b;
        }

        /// <summary>
        ///   Write a short value to buffer LSB first
        /// </summary>
        /// <param name="s"> value to write </param>
        public void WriteShort(int s)
        {
            buf[end++] = (byte) s;
            buf[end++] = (byte) (s >> 8);
        }

        /// <summary>
        ///   write an integer LSB first
        /// </summary>
        /// <param name="s"> value to write </param>
        public void WriteInt(int s)
        {
            buf[end++] = (byte) s;
            buf[end++] = (byte) (s >> 8);
            buf[end++] = (byte) (s >> 16);
            buf[end++] = (byte) (s >> 24);
        }

        /// <summary>
        ///   Write a block of data to buffer
        /// </summary>
        /// <param name="block"> data to write </param>
        /// <param name="offset"> offset of first byte to write </param>
        /// <param name="len"> number of bytes to write </param>
        public void WriteBlock(byte[] block, int offset, int len)
        {
            Array.Copy(block, offset, buf, end, len);
            end += len;
        }

        /// <summary>
        ///   Align internal buffer on a byte boundary
        /// </summary>
        public void AlignToByte()
        {
            if (bitCount > 0)
            {
                buf[end++] = (byte) bits;
                if (bitCount > 8)
                {
                    buf[end++] = (byte) (bits >> 8);
                }
            }

            bits = 0;
            bitCount = 0;
        }

        /// <summary>
        ///   Write bits to internal buffer
        /// </summary>
        /// <param name="b"> source of bits </param>
        /// <param name="count"> number of bits to write </param>
        public void WriteBits(int b, int count)
        {
            // 			if (DeflaterConstants.DEBUGGING) {
            // 				//Console.WriteLine("writeBits("+b+","+count+")");
            // 			}
            bits |= (uint) (b << bitCount);
            bitCount += count;
            if (bitCount >= 16)
            {
                buf[end++] = (byte) bits;
                buf[end++] = (byte) (bits >> 8);
                bits >>= 16;
                bitCount -= 16;
            }
        }

        /// <summary>
        ///   Write a short value to internal buffer most significant byte first
        /// </summary>
        /// <param name="s"> value to write </param>
        public void WriteShortMSB(int s)
        {
            buf[end++] = (byte) (s >> 8);
            buf[end++] = (byte) s;
        }

        /// <summary>
        ///   Flushes the pending buffer into the given output array. If the output array is to small, only a partial flush is done.
        /// </summary>
        /// <param name="output"> the output array; </param>
        /// <param name="offset"> the offset into output array; </param>
        /// <param name="length"> length the maximum number of bytes to store; </param>
        /// <exception name="ArgumentOutOfRangeException">IndexOutOfBoundsException if offset or length are invalid.</exception>
        /// <returns> </returns>
        public int Flush(byte[] output, int offset, int length)
        {
            if (bitCount >= 8)
            {
                buf[end++] = (byte) bits;
                bits >>= 8;
                bitCount -= 8;
            }

            if (length > end - start)
            {
                length = end - start;
                Array.Copy(buf, start, output, offset, length);
                start = 0;
                end = 0;
            }
            else
            {
                Array.Copy(buf, start, output, offset, length);
                start += length;
            }

            return length;
        }

        /// <summary>
        ///   Convert internal buffer to byte array. Buffer is empty on completion
        /// </summary>
        /// <returns> converted buffer contents contents </returns>
        public byte[] ToByteArray()
        {
            var ret = new byte[end - start];
            Array.Copy(buf, start, ret, 0, ret.Length);
            start = 0;
            end = 0;
            return ret;
        }

        #endregion
    }
}