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

/*
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 *
*/

using System;
using System.IO;

namespace Lucene.Net.Analysis.CJK
{
	/* ====================================================================
	 * The Apache Software License, Version 1.1
	 *
	 * Copyright (c) 2004 The Apache Software Foundation.  All rights
	 * reserved.
	 *
	 * Redistribution and use in source and binary forms, with or without
	 * modification, are permitted provided that the following conditions
	 * are met:
	 *
	 * 1. Redistributions of source code must retain the above copyright
	 *    notice, this list of conditions and the following disclaimer.
	 *
	 * 2. Redistributions in binary form must reproduce the above copyright
	 *    notice, this list of conditions and the following disclaimer in
	 *    the documentation and/or other materials provided with the
	 *    distribution.
	 *
	 * 3. The end-user documentation included with the redistribution,
	 *    if any, must include the following acknowledgment:
	 *       "This product includes software developed by the
	 *        Apache Software Foundation (http://www.apache.org/)."
	 *    Alternately, this acknowledgment may appear in the software itself,
	 *    if and wherever such third-party acknowledgments normally appear.
	 *
	 * 4. The names "Apache" and "Apache Software Foundation" and
	 *    "Apache Lucene" must not be used to endorse or promote products
	 *    derived from this software without prior written permission. For
	 *    written permission, please contact apache@apache.org.
	 *
	 * 5. Products derived from this software may not be called "Apache",
	 *    "Apache Lucene", nor may "Apache" appear in their name, without
	 *    prior written permission of the Apache Software Foundation.
	 *
	 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED
	 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
	 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	 * DISCLAIMED.  IN NO EVENT SHALL THE APACHE SOFTWARE FOUNDATION OR
	 * ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
	 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
	 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
	 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
	 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
	 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
	 * SUCH DAMAGE.
	 * ====================================================================
	 *
	 * This software consists of voluntary contributions made by many
	 * individuals on behalf of the Apache Software Foundation.  For more
	 * information on the Apache Software Foundation, please see
	 * <http://www.apache.org/>.
	 */

	/// <summary>
	/// <p>
	/// CJKTokenizer was modified from StopTokenizer which does a decent job for
	/// most European languages. and it perferm other token method for double-byte
	/// Characters: the token will return at each two charactors with overlap match.<br/>
	/// Example: "java C1C2C3C4" will be segment to: "java" "C1C2" "C2C3" "C3C4" it
	/// also need filter filter zero length token ""<br/>
	/// for Digit: digit, '+', '#' will token as letter<br/>
	/// for more info on Asia language(Chinese Japanese Korean) text segmentation:
	/// please search  <a
	/// href="http://www.google.com/search?q=word+chinese+segment">google</a>
	/// </p>
	/// 
	/// @author Che, Dong
	/// @version $Id: CJKTokenizer.java,v 1.3 2003/01/22 20:54:47 otis Exp $
	/// </summary>
	sealed class CJKTokenizer : Tokenizer 
	{
		//~ Static fields/initializers ---------------------------------------------

		/// <summary>
		/// Max word length
		/// </summary>
		private static int MAX_WORD_LEN = 255;

		/// <summary>
		/// buffer size
		/// </summary>
		private static int IO_BUFFER_SIZE = 256;

		//~ Instance fields --------------------------------------------------------

		/// <summary>
		/// word offset, used to imply which character(in ) is parsed
		/// </summary>
		private int offset = 0;

		/// <summary>
		/// the index used only for ioBuffer
		/// </summary>
		private int bufferIndex = 0;

		/// <summary>
		/// data length
		/// </summary>
		private int dataLen = 0;

		/// <summary>
		/// character buffer, store the characters which are used to compose <br/>
		/// the returned Token
		/// </summary>
		private char[] buffer = new char[MAX_WORD_LEN];

		/// <summary>
		/// I/O buffer, used to store the content of the input(one of the <br/>
		/// members of Tokenizer)
		/// </summary>
		private char[] ioBuffer = new char[IO_BUFFER_SIZE];

		/// <summary>
		/// word type: single=>ASCII  double=>non-ASCII word=>default 
		/// </summary>
		private String tokenType = "word";

		/// <summary>
		/// tag: previous character is a cached double-byte character  "C1C2C3C4"
		/// ----(set the C1 isTokened) C1C2 "C2C3C4" ----(set the C2 isTokened)
		/// C1C2 C2C3 "C3C4" ----(set the C3 isTokened) "C1C2 C2C3 C3C4"
		/// </summary>
		private bool preIsTokened = false;

		//~ Constructors -----------------------------------------------------------

		/// <summary>
		/// Construct a token stream processing the given input.
		/// </summary>
		/// <param name="_in">I/O reader</param>
		public CJKTokenizer(TextReader _in) 
		{
			input = _in;
		}

		//~ Methods ----------------------------------------------------------------

		/// <summary>
		///  Returns the next token in the stream, or null at EOS.
		/// </summary>
		/// <returns>Token</returns>
        [Obsolete(@"The returned Token is a ""full private copy"" (not re-used across calls to Next()) but will be slower than calling {@link #Next(Token)} or using the new IncrementToken() method with the new AttributeSource API.")]
		public override Token Next()
		{
			/** how many character(s) has been stored in buffer */
			int length = 0;

			/** the position used to create Token */
			int start = offset;

			while (true) 
			{
				/** current charactor */
				char c;

				/** unicode block of current charactor for detail */
				//Character.UnicodeBlock ub;

				offset++;

				if (bufferIndex >= dataLen) 
				{
					dataLen = input.Read(ioBuffer, 0, ioBuffer.Length);
					bufferIndex = 0;
				}

				if (dataLen == 0) 
				{
					if (length > 0) 
					{
						if (preIsTokened == true) 
						{
							length = 0;
							preIsTokened = false;
						}

						break;
					} 
					else 
					{
						return null;
					}
				} 
				else 
				{
					//get current character
					c = ioBuffer[bufferIndex++];

					//get the UnicodeBlock of the current character
					//ub = Character.UnicodeBlock.of(c);
				}

				//if the current character is ASCII or Extend ASCII
				if (('\u0000' <= c && c <= '\u007F') || 
					('\uFF00' <= c && c <= '\uFFEF')) 
				{
					if ('\uFF00' <= c && c <= '\uFFEF')
					{
						/** convert  HALFWIDTH_AND_FULLWIDTH_FORMS to BASIC_LATIN */
						int i = (int) c;
						i = i - 65248;
						c = (char) i;
					}

					// if the current character is a letter or "_" "+" "#"
					if (Char.IsLetterOrDigit(c)
						|| ((c == '_') || (c == '+') || (c == '#'))
						) 
					{
						if (length == 0) 
						{
							// "javaC1C2C3C4linux" <br/>
							//      ^--: the current character begin to token the ASCII
							// letter
							start = offset - 1;
						} 
						else if (tokenType == "double") 
						{
							// "javaC1C2C3C4linux" <br/>
							//              ^--: the previous non-ASCII
							// : the current character
							offset--;
							bufferIndex--;
							tokenType = "single";

							if (preIsTokened == true) 
							{
								// there is only one non-ASCII has been stored
								length = 0;
								preIsTokened = false;

								break;
							} 
							else 
							{
								break;
							}
						}

						// store the LowerCase(c) in the buffer
						buffer[length++] = Char.ToLower(c);
						tokenType = "single";

						// break the procedure if buffer overflowed!
						if (length == MAX_WORD_LEN) 
						{
							break;
						}
					} 
					else if (length > 0) 
					{
						if (preIsTokened == true) 
						{
							length = 0;
							preIsTokened = false;
						} 
						else 
						{
							break;
						}
					}
				} 
				else 
				{
					// non-ASCII letter, eg."C1C2C3C4"
					if (Char.IsLetter(c)) 
					{
						if (length == 0) 
						{
							start = offset - 1;
							buffer[length++] = c;
							tokenType = "double";
						} 
						else 
						{
							if (tokenType == "single") 
							{
								offset--;
								bufferIndex--;

								//return the previous ASCII characters
								break;
							} 
							else 
							{
								buffer[length++] = c;
								tokenType = "double";

								if (length == 2) 
								{
									offset--;
									bufferIndex--;
									preIsTokened = true;

									break;
								}
							}
						}
					} 
					else if (length > 0) 
					{
						if (preIsTokened == true) 
						{
							// empty the buffer
							length = 0;
							preIsTokened = false;
						} 
						else 
						{
							break;
						}
					}
				}
			}

			return new Token(new String(buffer, 0, length), start, start + length,
				tokenType
				);
		}
	}

}
