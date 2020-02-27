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


namespace ASC.Xmpp.Core.utils.Xml.xpnet
{
    /// <summary>
    ///   A token that was parsed.
    /// </summary>
    public class Token
    {
        #region Members

        /// <summary>
        /// </summary>
        private int nameEnd = -1;

        /// <summary>
        /// </summary>
        private char refChar1 = (char) 0;

        /// <summary>
        /// </summary>
        private char refChar2 = (char) 0;

        /// <summary>
        /// </summary>
        private int tokenEnd = -1;

        #endregion

        #region Properties

        /// <summary>
        ///   The end of the current token's name, in relation to the beginning of the buffer.
        /// </summary>
        public int NameEnd
        {
            get { return nameEnd; }

            set { nameEnd = value; }
        }

        // public char RefChar
        // {
        // get {return refChar1;}
        // }

        /// <summary>
        ///   The parsed-out character. &amp; for &amp;amp;
        /// </summary>
        public char RefChar1
        {
            get { return refChar1; }

            set { refChar1 = value; }
        }

        /// <summary>
        ///   The second of two parsed-out characters. TODO: find example.
        /// </summary>
        public char RefChar2
        {
            get { return refChar2; }

            set { refChar2 = value; }
        }

        /// <summary>
        ///   The end of the current token, in relation to the beginning of the buffer.
        /// </summary>
        public int TokenEnd
        {
            get { return tokenEnd; }

            set { tokenEnd = value; }
        }

        #endregion

        /*
        public void getRefCharPair(char[] ch, int off) {
            ch[off] = refChar1;
            ch[off + 1] = refChar2;
        }
        */
    }
}