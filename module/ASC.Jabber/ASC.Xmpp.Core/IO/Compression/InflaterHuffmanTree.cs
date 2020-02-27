/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using System;
using ASC.Xmpp.Core.IO.Compression.Streams;

namespace ASC.Xmpp.Core.IO.Compression
{

    #region usings

    #endregion

    /// <summary>
    ///   Huffman tree used for inflation
    /// </summary>
    public class InflaterHuffmanTree
    {
        #region Members

        /// <summary>
        ///   Distance tree
        /// </summary>
        public static InflaterHuffmanTree defDistTree;

        /// <summary>
        ///   Literal length tree
        /// </summary>
        public static InflaterHuffmanTree defLitLenTree;

        /// <summary>
        /// </summary>
        private static int MAX_BITLEN = 15;

        /// <summary>
        /// </summary>
        private short[] tree;

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        /// <exception cref="SharpZipBaseException"></exception>
        static InflaterHuffmanTree()
        {
            try
            {
                var codeLengths = new byte[288];
                int i = 0;
                while (i < 144)
                {
                    codeLengths[i++] = 8;
                }

                while (i < 256)
                {
                    codeLengths[i++] = 9;
                }

                while (i < 280)
                {
                    codeLengths[i++] = 7;
                }

                while (i < 288)
                {
                    codeLengths[i++] = 8;
                }

                defLitLenTree = new InflaterHuffmanTree(codeLengths);

                codeLengths = new byte[32];
                i = 0;
                while (i < 32)
                {
                    codeLengths[i++] = 5;
                }

                defDistTree = new InflaterHuffmanTree(codeLengths);
            }
            catch (Exception)
            {
                throw new SharpZipBaseException("InflaterHuffmanTree: static tree length illegal");
            }
        }

        /// <summary>
        ///   Constructs a Huffman tree from the array of code lengths.
        /// </summary>
        /// <param name="codeLengths"> the array of code lengths </param>
        public InflaterHuffmanTree(byte[] codeLengths)
        {
            BuildTree(codeLengths);
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Reads the next symbol from input. The symbol is encoded using the huffman tree.
        /// </summary>
        /// <param name="input"> input the input source. </param>
        /// <returns> the next symbol, or -1 if not enough input is available. </returns>
        public int GetSymbol(StreamManipulator input)
        {
            int lookahead, symbol;
            if ((lookahead = input.PeekBits(9)) >= 0)
            {
                if ((symbol = tree[lookahead]) >= 0)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }

                int subtree = -(symbol >> 4);
                int bitlen = symbol & 15;
                if ((lookahead = input.PeekBits(bitlen)) >= 0)
                {
                    symbol = tree[subtree | (lookahead >> 9)];
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    int bits = input.AvailableBits;
                    lookahead = input.PeekBits(bits);
                    symbol = tree[subtree | (lookahead >> 9)];
                    if ((symbol & 15) <= bits)
                    {
                        input.DropBits(symbol & 15);
                        return symbol >> 4;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            else
            {
                int bits = input.AvailableBits;
                lookahead = input.PeekBits(bits);
                symbol = tree[lookahead];
                if (symbol >= 0 && (symbol & 15) <= bits)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    return -1;
                }
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// </summary>
        /// <param name="codeLengths"> </param>
        private void BuildTree(byte[] codeLengths)
        {
            var blCount = new int[MAX_BITLEN + 1];
            var nextCode = new int[MAX_BITLEN + 1];

            for (int i = 0; i < codeLengths.Length; i++)
            {
                int bits = codeLengths[i];
                if (bits > 0)
                {
                    blCount[bits]++;
                }
            }

            int code = 0;
            int treeSize = 512;
            for (int bits = 1; bits <= MAX_BITLEN; bits++)
            {
                nextCode[bits] = code;
                code += blCount[bits] << (16 - bits);
                if (bits >= 10)
                {
                    /* We need an extra table for bit lengths >= 10. */
                    int start = nextCode[bits] & 0x1ff80;
                    int end = code & 0x1ff80;
                    treeSize += (end - start) >> (16 - bits);
                }
            }

            /* -jr comment this out! doesnt work for dynamic trees and pkzip 2.04g
			if (code != 65536) 
			{
				throw new SharpZipBaseException("Code lengths don't add up properly.");
			}
*/
            /* Now create and fill the extra tables from longest to shortest
			* bit len.  This way the sub trees will be aligned.
			*/
            tree = new short[treeSize];
            int treePtr = 512;
            for (int bits = MAX_BITLEN; bits >= 10; bits--)
            {
                int end = code & 0x1ff80;
                code -= blCount[bits] << (16 - bits);
                int start = code & 0x1ff80;
                for (int i = start; i < end; i += 1 << 7)
                {
                    tree[DeflaterHuffman.BitReverse(i)] = (short) ((-treePtr << 4) | bits);
                    treePtr += 1 << (bits - 9);
                }
            }

            for (int i = 0; i < codeLengths.Length; i++)
            {
                int bits = codeLengths[i];
                if (bits == 0)
                {
                    continue;
                }

                code = nextCode[bits];
                int revcode = DeflaterHuffman.BitReverse(code);
                if (bits <= 9)
                {
                    do
                    {
                        tree[revcode] = (short) ((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < 512);
                }
                else
                {
                    int subTree = tree[revcode & 511];
                    int treeLen = 1 << (subTree & 15);
                    subTree = -(subTree >> 4);
                    do
                    {
                        tree[subTree | (revcode >> 9)] = (short) ((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < treeLen);
                }

                nextCode[bits] = code + (1 << (16 - bits));
            }
        }

        #endregion
    }
}