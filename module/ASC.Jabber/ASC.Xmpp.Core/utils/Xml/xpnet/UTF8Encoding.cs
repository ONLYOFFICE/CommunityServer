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


#region using

using System;

#endregion

namespace ASC.Xmpp.Core.utils.Xml.xpnet
{

    #region usings

    #endregion

    /// <summary>
    ///   UTF-8 specific tokenizer.
    /// </summary>
    public class UTF8Encoding : Encoding
    {
        #region Members

        /// <summary>
        /// </summary>
        private static readonly int[] utf8HiTypeTable = new[]
                                                            {
                                                                /* 0x80 */ BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, /* 0x84 */ BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           /* 0x88 */ BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, /* 0x8C */
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, /* 0x90 */ BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           /* 0x94 */ BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, /* 0x98 */
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, /* 0x9C */ BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           /* 0xA0 */ BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, /* 0xA4 */
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, /* 0xA8 */ BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           /* 0xAC */ BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, /* 0xB0 */
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, /* 0xB4 */ BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           /* 0xB8 */ BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, BT_MALFORM, /* 0xBC */
                                                                           BT_MALFORM, BT_MALFORM, BT_MALFORM,
                                                                           BT_MALFORM, /* 0xC0 */ BT_LEAD2,
                                                                           BT_LEAD2, BT_LEAD2, BT_LEAD2,
                                                                           /* 0xC4 */ BT_LEAD2, BT_LEAD2,
                                                                           BT_LEAD2, BT_LEAD2, /* 0xC8 */
                                                                           BT_LEAD2, BT_LEAD2, BT_LEAD2, BT_LEAD2
                                                                           , /* 0xCC */ BT_LEAD2, BT_LEAD2,
                                                                           BT_LEAD2, BT_LEAD2, /* 0xD0 */
                                                                           BT_LEAD2, BT_LEAD2, BT_LEAD2, BT_LEAD2
                                                                           , /* 0xD4 */ BT_LEAD2, BT_LEAD2,
                                                                           BT_LEAD2, BT_LEAD2, /* 0xD8 */
                                                                           BT_LEAD2, BT_LEAD2, BT_LEAD2, BT_LEAD2
                                                                           , /* 0xDC */ BT_LEAD2, BT_LEAD2,
                                                                           BT_LEAD2, BT_LEAD2, /* 0xE0 */
                                                                           BT_LEAD3, BT_LEAD3, BT_LEAD3, BT_LEAD3
                                                                           , /* 0xE4 */ BT_LEAD3, BT_LEAD3,
                                                                           BT_LEAD3, BT_LEAD3, /* 0xE8 */
                                                                           BT_LEAD3, BT_LEAD3, BT_LEAD3, BT_LEAD3
                                                                           , /* 0xEC */ BT_LEAD3, BT_LEAD3,
                                                                           BT_LEAD3, BT_LEAD3, /* 0xF0 */
                                                                           BT_LEAD4, BT_LEAD4, BT_LEAD4, BT_LEAD4
                                                                           , /* 0xF4 */ BT_LEAD4, BT_LEAD4,
                                                                           BT_LEAD4, BT_LEAD4, /* 0xF8 */
                                                                           BT_NONXML, BT_NONXML, BT_NONXML,
                                                                           BT_NONXML, /* 0xFC */ BT_NONXML,
                                                                           BT_NONXML, BT_MALFORM, BT_MALFORM
                                                            };

        /// <summary>
        /// </summary>
        private static readonly int[] utf8TypeTable = new int[256];

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        static UTF8Encoding()
        {
            Array.Copy(asciiTypeTable, 0, utf8TypeTable, 0, 128);
            Array.Copy(utf8HiTypeTable, 0, utf8TypeTable, 128, 128);
        }

        /// <summary>
        ///   New tokenizer
        /// </summary>
        public UTF8Encoding() : base(1)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        ///   What is the type of the current byte?
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <returns> </returns>
        protected override int byteType(byte[] buf, int off)
        {
            return utf8TypeTable[buf[off] & 0xFF];
        }

        /// <summary>
        ///   Current byte to ASCII char
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <returns> </returns>
        protected override char byteToAscii(byte[] buf, int off)
        {
            return (char) buf[off];
        }

        /// <summary>
        ///   c is a significant ASCII character
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <param name="c"> </param>
        /// <returns> </returns>
        protected override bool charMatches(byte[] buf, int off, char c)
        {
            return ((char) buf[off]) == c;
        }

        /// <summary>
        ///   A 2 byte UTF-8 representation splits the characters 11 bits between the bottom 5 and 6 bits of the bytes.
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <returns> </returns>
        protected override int byteType2(byte[] buf, int off)
        {
            int[] page = charTypeTable[(buf[off] >> 2) & 0x7];
            return page[((buf[off] & 3) << 6) | (buf[off + 1] & 0x3F)];
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceBuf"> </param>
        /// <param name="sourceStart"> </param>
        /// <param name="sourceEnd"> </param>
        /// <param name="targetBuf"> </param>
        /// <param name="targetStart"> </param>
        /// <returns> </returns>
        protected override int convert(byte[] sourceBuf,
                                       int sourceStart,
                                       int sourceEnd,
                                       char[] targetBuf,
                                       int targetStart)
        {
            int initTargetStart = targetStart;
            int c;
            while (sourceStart != sourceEnd)
            {
                byte b = sourceBuf[sourceStart++];
                if (b >= 0)
                {
                    targetBuf[targetStart++] = (char) b;
                }
                else
                {
                    switch (utf8TypeTable[b & 0xFF])
                    {
                        case BT_LEAD2:

                            /* 5, 6 */
                            targetBuf[targetStart++] =
                                (char) (((b & 0x1F) << 6) | (sourceBuf[sourceStart++] & 0x3F));
                            break;
                        case BT_LEAD3:

                            /* 4, 6, 6 */
                            c = (b & 0xF) << 12;
                            c |= (sourceBuf[sourceStart++] & 0x3F) << 6;
                            c |= sourceBuf[sourceStart++] & 0x3F;
                            targetBuf[targetStart++] = (char) c;
                            break;
                        case BT_LEAD4:

                            /* 3, 6, 6, 6 */
                            c = (b & 0x7) << 18;
                            c |= (sourceBuf[sourceStart++] & 0x3F) << 12;
                            c |= (sourceBuf[sourceStart++] & 0x3F) << 6;
                            c |= sourceBuf[sourceStart++] & 0x3F;
                            c -= 0x10000;
                            targetBuf[targetStart++] = (char) ((c >> 10) | 0xD800);
                            targetBuf[targetStart++] = (char) ((c & ((1 << 10) - 1)) | 0xDC00);
                            break;
                    }
                }
            }

            return targetStart - initTargetStart;
        }

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <param name="end"> </param>
        /// <param name="pos"> </param>
        protected override void movePosition(byte[] buf, int off, int end, Position pos)
        {
            /* Maintain the invariant: off - colDiff == colNumber. */
            int colDiff = off - pos.ColumnNumber;
            int lineNumber = pos.LineNumber;
            while (off != end)
            {
                byte b = buf[off];
                if (b >= 0)
                {
                    ++off;
                    switch (b)
                    {
                        case (byte) '\n':
                            lineNumber += 1;
                            colDiff = off;
                            break;
                        case (byte) '\r':
                            lineNumber += 1;
                            if (off != end && buf[off] == '\n')
                            {
                                off++;
                            }

                            colDiff = off;
                            break;
                    }
                }
                else
                {
                    switch (utf8TypeTable[b & 0xFF])
                    {
                        default:
                            off += 1;
                            break;
                        case BT_LEAD2:
                            off += 2;
                            colDiff++;
                            break;
                        case BT_LEAD3:
                            off += 3;
                            colDiff += 2;
                            break;
                        case BT_LEAD4:
                            off += 4;
                            colDiff += 3;
                            break;
                    }
                }
            }

            pos.ColumnNumber = off - colDiff;
            pos.LineNumber = lineNumber;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <returns> </returns>
        private int byteType3(byte[] buf, int off)
        {
            int[] page = charTypeTable[((buf[off] & 0xF) << 4) | ((buf[off + 1] >> 2) & 0xF)];
            return page[((buf[off + 1] & 3) << 6) | (buf[off + 2] & 0x3F)];
        }

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <exception cref="InvalidTokenException"></exception>
        private void check3(byte[] buf, int off)
        {
            switch (buf[off])
            {
                case 0xEF:

                    /* 0xFFFF 0xFFFE */
                    if ((buf[off + 1] == 0xBF) && ((buf[off + 2] == 0xBF) || (buf[off + 2] == 0xBE)))
                    {
                        throw new InvalidTokenException(off);
                    }

                    return;
                case 0xED:

                    /* 0xD800..0xDFFF <=> top 5 bits are 11011 */
                    if ((buf[off + 1] & 0x20) != 0)
                    {
                        throw new InvalidTokenException(off);
                    }

                    return;
                default:
                    return;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <exception cref="InvalidTokenException"></exception>
        private void check4(byte[] buf, int off)
        {
            switch (buf[off] & 0x7)
            {
                default:
                    return;
                case 5:
                case 6:
                case 7:
                    break;
                case 4:
                    if ((buf[off + 1] & 0x30) == 0)
                    {
                        return;
                    }

                    break;
            }

            throw new InvalidTokenException(off);
        }

        /// <summary>
        /// </summary>
        /// <param name="buf"> </param>
        /// <param name="off"> </param>
        /// <param name="end"> </param>
        /// <returns> </returns>
        private int extendData(byte[] buf, int off, int end)
        {
            while (off != end)
            {
                int type = utf8TypeTable[buf[off] & 0xFF];
                if (type >= 0)
                {
                    off++;
                }
                else if (type < BT_LEAD4)
                {
                    break;
                }
                else
                {
                    if (end - off + type < 0)
                    {
                        break;
                    }

                    switch (type)
                    {
                        case BT_LEAD3:
                            check3(buf, off);
                            break;
                        case BT_LEAD4:
                            check4(buf, off);
                            break;
                    }

                    off -= type; // this is an ugly hack, James
                }
            }

            return off;
        }

        #endregion

        /* A 3 byte UTF-8 representation splits the characters 16 bits
           between the bottom 4, 6 and 6 bits of the bytes. */

        /* This will (incorrectly) return BT_LEAD4 for surrogates, but that
           doesn't matter. */
    }
}