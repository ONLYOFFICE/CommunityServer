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

#endregion

namespace ASC.Xmpp.Core.utils.Xml.xpnet
{

    #region usings

    #endregion

    /// <summary>
    ///   Represents information returned by <code>Encoding.tokenizeContent</code> . @see Encoding#tokenizeContent
    /// </summary>
    public class ContentToken : Token
    {
        #region Constants

        /// <summary>
        /// </summary>
        private const int INIT_ATT_COUNT = 8;

        #endregion

        #region Members

        /// <summary>
        /// </summary>
        private int attCount;

        /// <summary>
        /// </summary>
        private int[] attNameEnd = new int[INIT_ATT_COUNT];

        /// <summary>
        /// </summary>
        private int[] attNameStart = new int[INIT_ATT_COUNT];

        /// <summary>
        /// </summary>
        private bool[] attNormalized = new bool[INIT_ATT_COUNT];

        /// <summary>
        /// </summary>
        private int[] attValueEnd = new int[INIT_ATT_COUNT];

        /// <summary>
        /// </summary>
        private int[] attValueStart = new int[INIT_ATT_COUNT];

        #endregion

        #region Methods

        /// <summary>
        ///   Returns the number of attributes specified in the start-tag or empty element tag.
        /// </summary>
        /// <returns> </returns>
        public int getAttributeSpecifiedCount()
        {
            return attCount;
        }

        /// <summary>
        ///   Returns the index of the first character of the name of the attribute index <code>i</code> .
        /// </summary>
        /// <param name="i"> </param>
        /// <returns> </returns>
        public int getAttributeNameStart(int i)
        {
            if (i >= attCount)
            {
                throw new IndexOutOfRangeException();
            }

            return attNameStart[i];
        }

        /**
		 * Returns the index following the last character of the name of the
		 * attribute index <code>i</code>.
		 */

        /// <summary>
        /// </summary>
        /// <param name="i"> </param>
        /// <returns> </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int getAttributeNameEnd(int i)
        {
            if (i >= attCount)
            {
                throw new IndexOutOfRangeException();
            }

            return attNameEnd[i];
        }

        /**
		 * Returns the index of the character following the opening quote of
		 * attribute index <code>i</code>.
		 */

        /// <summary>
        /// </summary>
        /// <param name="i"> </param>
        /// <returns> </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int getAttributeValueStart(int i)
        {
            if (i >= attCount)
            {
                throw new IndexOutOfRangeException();
            }

            return attValueStart[i];
        }

        /**
		 * Returns the index of the closing quote attribute index <code>i</code>.
		 */

        /// <summary>
        /// </summary>
        /// <param name="i"> </param>
        /// <returns> </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int getAttributeValueEnd(int i)
        {
            if (i >= attCount)
            {
                throw new IndexOutOfRangeException();
            }

            return attValueEnd[i];
        }

        /**
		 * Returns true if attribute index <code>i</code> does not need to
		 * be normalized.  This is an optimization that allows further processing
		 * of the attribute to be avoided when it is known that normalization
		 * cannot change the value of the attribute.
		 */

        /// <summary>
        /// </summary>
        /// <param name="i"> </param>
        /// <returns> </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool isAttributeNormalized(int i)
        {
            if (i >= attCount)
            {
                throw new IndexOutOfRangeException();
            }

            return attNormalized[i];
        }

        /// <summary>
        ///   Clear out all of the current attributes
        /// </summary>
        public void clearAttributes()
        {
            attCount = 0;
        }

        /// <summary>
        ///   Add a new attribute
        /// </summary>
        /// <param name="nameStart"> </param>
        /// <param name="nameEnd"> </param>
        /// <param name="valueStart"> </param>
        /// <param name="valueEnd"> </param>
        /// <param name="normalized"> </param>
        public void appendAttribute(int nameStart, int nameEnd, int valueStart, int valueEnd, bool normalized)
        {
            if (attCount == attNameStart.Length)
            {
                attNameStart = grow(attNameStart);
                attNameEnd = grow(attNameEnd);
                attValueStart = grow(attValueStart);
                attValueEnd = grow(attValueEnd);
                attNormalized = grow(attNormalized);
            }

            attNameStart[attCount] = nameStart;
            attNameEnd[attCount] = nameEnd;
            attValueStart[attCount] = valueStart;
            attValueEnd[attCount] = valueEnd;
            attNormalized[attCount] = normalized;
            ++attCount;
        }

        /// <summary>
        ///   Is the current attribute unique?
        /// </summary>
        /// <param name="buf"> </param>
        public void checkAttributeUniqueness(byte[] buf)
        {
            for (int i = 1; i < attCount; i++)
            {
                int len = attNameEnd[i] - attNameStart[i];
                for (int j = 0; j < i; j++)
                {
                    if (attNameEnd[j] - attNameStart[j] == len)
                    {
                        int n = len;
                        int s1 = attNameStart[i];
                        int s2 = attNameStart[j];
                        do
                        {
                            if (--n < 0)
                            {
                                throw new InvalidTokenException(attNameStart[i],
                                                                InvalidTokenException.DUPLICATE_ATTRIBUTE);
                            }
                        } while (buf[s1++] == buf[s2++]);
                    }
                }
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// </summary>
        /// <param name="v"> </param>
        /// <returns> </returns>
        private static int[] grow(int[] v)
        {
            int[] tem = v;
            v = new int[tem.Length << 1];
            Array.Copy(tem, 0, v, 0, tem.Length);
            return v;
        }

        /// <summary>
        /// </summary>
        /// <param name="v"> </param>
        /// <returns> </returns>
        private static bool[] grow(bool[] v)
        {
            bool[] tem = v;
            v = new bool[tem.Length << 1];
            Array.Copy(tem, 0, v, 0, tem.Length);
            return v;
        }

        #endregion
    }
}