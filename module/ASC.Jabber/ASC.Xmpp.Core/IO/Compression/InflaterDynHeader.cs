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


#region using

using System;
using ASC.Xmpp.Core.IO.Compression.Streams;

#endregion

namespace ASC.Xmpp.Core.IO.Compression
{

    #region usings

    #endregion

    /// <summary>
    /// </summary>
    internal class InflaterDynHeader
    {
        #region Constants

        /// <summary>
        /// </summary>
        private const int BLLENS = 3;

        /// <summary>
        /// </summary>
        private const int BLNUM = 2;

        /// <summary>
        /// </summary>
        private const int DNUM = 1;

        /// <summary>
        /// </summary>
        private const int LENS = 4;

        /// <summary>
        /// </summary>
        private const int LNUM = 0;

        /// <summary>
        /// </summary>
        private const int REPS = 5;

        #endregion

        #region Members

        /// <summary>
        /// </summary>
        private static readonly int[] BL_ORDER = {
                                                     16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1
                                                     , 15
                                                 };

        /// <summary>
        /// </summary>
        private static readonly int[] repBits = {2, 3, 7};

        /// <summary>
        /// </summary>
        private static readonly int[] repMin = {3, 3, 11};

        /// <summary>
        /// </summary>
        private byte[] blLens;

        /// <summary>
        /// </summary>
        private InflaterHuffmanTree blTree;

        /// <summary>
        /// </summary>
        private int blnum;

        /// <summary>
        /// </summary>
        private int dnum;

        /// <summary>
        /// </summary>
        private byte lastLen;

        /// <summary>
        /// </summary>
        private byte[] litdistLens;

        /// <summary>
        /// </summary>
        private int lnum;

        /// <summary>
        /// </summary>
        private int mode;

        /// <summary>
        /// </summary>
        private int num;

        /// <summary>
        /// </summary>
        private int ptr;

        /// <summary>
        /// </summary>
        private int repSymbol;

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="input"> </param>
        /// <returns> </returns>
        /// <exception cref="SharpZipBaseException"></exception>
        public bool Decode(StreamManipulator input)
        {
            decode_loop:
            for (;;)
            {
                switch (mode)
                {
                    case LNUM:
                        lnum = input.PeekBits(5);
                        if (lnum < 0)
                        {
                            return false;
                        }

                        lnum += 257;
                        input.DropBits(5);

                        // 	    System.err.println("LNUM: "+lnum);
                        mode = DNUM;
                        goto case DNUM; // fall through
                    case DNUM:
                        dnum = input.PeekBits(5);
                        if (dnum < 0)
                        {
                            return false;
                        }

                        dnum++;
                        input.DropBits(5);

                        // 	    System.err.println("DNUM: "+dnum);
                        num = lnum + dnum;
                        litdistLens = new byte[num];
                        mode = BLNUM;
                        goto case BLNUM; // fall through
                    case BLNUM:
                        blnum = input.PeekBits(4);
                        if (blnum < 0)
                        {
                            return false;
                        }

                        blnum += 4;
                        input.DropBits(4);
                        blLens = new byte[19];
                        ptr = 0;

                        // 	    System.err.println("BLNUM: "+blnum);
                        mode = BLLENS;
                        goto case BLLENS; // fall through
                    case BLLENS:
                        while (ptr < blnum)
                        {
                            int len = input.PeekBits(3);
                            if (len < 0)
                            {
                                return false;
                            }

                            input.DropBits(3);

                            // 		System.err.println("blLens["+BL_ORDER[ptr]+"]: "+len);
                            blLens[BL_ORDER[ptr]] = (byte) len;
                            ptr++;
                        }

                        blTree = new InflaterHuffmanTree(blLens);
                        blLens = null;
                        ptr = 0;
                        mode = LENS;
                        goto case LENS; // fall through
                    case LENS:
                        {
                            int symbol;
                            while (((symbol = blTree.GetSymbol(input)) & ~15) == 0)
                            {
                                /* Normal case: symbol in [0..15] */

                                // 		  System.err.println("litdistLens["+ptr+"]: "+symbol);
                                litdistLens[ptr++] = lastLen = (byte) symbol;

                                if (ptr == num)
                                {
                                    /* Finished */
                                    return true;
                                }
                            }

                            /* need more input ? */
                            if (symbol < 0)
                            {
                                return false;
                            }

                            /* otherwise repeat code */
                            if (symbol >= 17)
                            {
                                /* repeat zero */
                                // 		  System.err.println("repeating zero");
                                lastLen = 0;
                            }
                            else
                            {
                                if (ptr == 0)
                                {
                                    throw new SharpZipBaseException();
                                }
                            }

                            repSymbol = symbol - 16;
                        }

                        mode = REPS;
                        goto case REPS; // fall through
                    case REPS:
                        {
                            int bits = repBits[repSymbol];
                            int count = input.PeekBits(bits);
                            if (count < 0)
                            {
                                return false;
                            }

                            input.DropBits(bits);
                            count += repMin[repSymbol];

                            // 	      System.err.println("litdistLens repeated: "+count);
                            if (ptr + count > num)
                            {
                                throw new SharpZipBaseException();
                            }

                            while (count-- > 0)
                            {
                                litdistLens[ptr++] = lastLen;
                            }

                            if (ptr == num)
                            {
                                /* Finished */
                                return true;
                            }
                        }

                        mode = LENS;
                        goto decode_loop;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public InflaterHuffmanTree BuildLitLenTree()
        {
            var litlenLens = new byte[lnum];
            Array.Copy(litdistLens, 0, litlenLens, 0, lnum);
            return new InflaterHuffmanTree(litlenLens);
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public InflaterHuffmanTree BuildDistTree()
        {
            var distLens = new byte[dnum];
            Array.Copy(litdistLens, lnum, distLens, 0, dnum);
            return new InflaterHuffmanTree(distLens);
        }

        #endregion
    }
}