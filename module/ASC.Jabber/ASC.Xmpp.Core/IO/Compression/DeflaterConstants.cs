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

namespace ASC.Xmpp.Core.IO.Compression
{

    #region usings

    #endregion

    /// <summary>
    ///   This class contains constants used for deflation.
    /// </summary>
    public class DeflaterConstants
    {
        #region Constants

        /// <summary>
        ///   Sets internal buffer sizes for Huffman encoding
        /// </summary>
        public const int DEFAULT_MEM_LEVEL = 8;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int DEFLATE_FAST = 1;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int DEFLATE_SLOW = 2;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int DEFLATE_STORED = 0;

        /// <summary>
        ///   Identifies dynamic tree in Zip file
        /// </summary>
        public const int DYN_TREES = 2;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int HASH_BITS = DEFAULT_MEM_LEVEL + 7;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int HASH_MASK = HASH_SIZE - 1;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int HASH_SHIFT = (HASH_BITS + MIN_MATCH - 1)/MIN_MATCH;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int HASH_SIZE = 1 << HASH_BITS;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int MAX_DIST = WSIZE - MIN_LOOKAHEAD;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int MAX_MATCH = 258;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int MAX_WBITS = 15;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int MIN_LOOKAHEAD = MAX_MATCH + MIN_MATCH + 1;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int MIN_MATCH = 3;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int PENDING_BUF_SIZE = 1 << (DEFAULT_MEM_LEVEL + 8);

        /// <summary>
        ///   Header flag indicating a preset dictionary for deflation
        /// </summary>
        public const int PRESET_DICT = 0x20;

        /// <summary>
        ///   Identifies static tree in Zip file
        /// </summary>
        public const int STATIC_TREES = 1;

        /// <summary>
        ///   Written to Zip file to identify a stored block
        /// </summary>
        public const int STORED_BLOCK = 0;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int WMASK = WSIZE - 1;

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public const int WSIZE = 1 << MAX_WBITS;

        #endregion

        #region Members

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public static int[] COMPR_FUNC = {0, 1, 1, 1, 1, 2, 2, 2, 2, 2};

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public static int[] GOOD_LENGTH = {0, 4, 4, 4, 4, 8, 8, 8, 32, 32};

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public static int MAX_BLOCK_SIZE = Math.Min(65535, PENDING_BUF_SIZE - 5);

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public static int[] MAX_CHAIN = {0, 4, 8, 32, 16, 32, 128, 256, 1024, 4096};

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public static int[] MAX_LAZY = {0, 4, 5, 6, 4, 16, 16, 32, 128, 258};

        /// <summary>
        ///   Internal compression engine constant
        /// </summary>
        public static int[] NICE_LENGTH = {0, 8, 16, 32, 16, 32, 128, 128, 258, 258};

        #endregion
    }
}