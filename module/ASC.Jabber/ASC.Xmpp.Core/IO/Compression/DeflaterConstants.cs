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