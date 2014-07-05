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
// // <copyright company="Ascensio System Limited" file="Exceptions.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

#region using

using System;

#endregion

namespace ASC.Xmpp.Core.utils.Xml.xpnet
{

    #region usings

    #endregion

    /// <summary>
    ///   Base class for other exceptions
    /// </summary>
    public class TokenException : Exception
    {
    }

    /// <summary>
    ///   An empty token was detected. This only happens with a buffer of length 0 is passed in to the parser.
    /// </summary>
    public class EmptyTokenException : TokenException
    {
    }

    /// <summary>
    ///   End of prolog.
    /// </summary>
    public class EndOfPrologException : TokenException
    {
    }

    /**
     * Thrown to indicate that the byte subarray being tokenized is a legal XML
     * token, but that subsequent bytes in the same entity could be part of
     * the token.  For example, <code>Encoding.tokenizeProlog</code>
     * would throw this if the byte subarray consists of a legal XML name.
     * @version $Revision: 1.3 $ $Date: 1998/02/17 04:24:06 $
     */

    /// <summary>
    /// </summary>
    public class ExtensibleTokenException : TokenException
    {
        #region Members

        /// <summary>
        /// </summary>
        private readonly TOK tokType;

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        /// <param name="tokType"> </param>
        public ExtensibleTokenException(TOK tokType)
        {
            this.tokType = tokType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public TOK TokenType
        {
            get { return tokType; }
        }

        #endregion

        /**
         * Returns the type of token in the byte subarrary.
         */
    }

    /// <summary>
    ///   Several kinds of token problems.
    /// </summary>
    public class InvalidTokenException : TokenException
    {
        #region Constants

        /// <summary>
        ///   More than one attribute with the same name on the same element
        /// </summary>
        public const byte DUPLICATE_ATTRIBUTE = 2;

        /// <summary>
        ///   An illegal character
        /// </summary>
        public const byte ILLEGAL_CHAR = 0;

        /// <summary>
        ///   Doc prefix wasn't XML
        /// </summary>
        public const byte XML_TARGET = 1;

        #endregion

        #region Members

        /// <summary>
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// </summary>
        private readonly byte type;

        #endregion

        #region Constructor

        /// <summary>
        ///   Some other type of bad token detected
        /// </summary>
        /// <param name="offset"> </param>
        /// <param name="type"> </param>
        public InvalidTokenException(int offset, byte type)
        {
            this.offset = offset;
            this.type = type;
        }

        /// <summary>
        ///   Illegal character detected
        /// </summary>
        /// <param name="offset"> </param>
        public InvalidTokenException(int offset)
        {
            this.offset = offset;
            type = ILLEGAL_CHAR;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Offset into the buffer where the problem ocurred.
        /// </summary>
        public int Offset
        {
            get { return offset; }
        }

        /// <summary>
        ///   Type of exception
        /// </summary>
        public int Type
        {
            get { return type; }
        }

        #endregion
    }

    /**
     * Thrown to indicate that the subarray being tokenized is not the
     * complete encoding of one or more characters, but might be if
     * more bytes were added.
     * @version $Revision: 1.2 $ $Date: 1998/02/17 04:24:11 $
     */

    /// <summary>
    /// </summary>
    public class PartialCharException : PartialTokenException
    {
        #region Members

        /// <summary>
        /// </summary>
        private readonly int leadByteIndex;

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        /// <param name="leadByteIndex"> </param>
        public PartialCharException(int leadByteIndex)
        {
            this.leadByteIndex = leadByteIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public int LeadByteIndex
        {
            get { return leadByteIndex; }
        }

        #endregion

        /**
         * Returns the index of the first byte that is not part of the complete
         * encoding of a character.
         */
    }

    /// <summary>
    ///   A partial token was received. Try again, after you add more bytes to the buffer.
    /// </summary>
    public class PartialTokenException : TokenException
    {
    }
}