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


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// This class provides bit debugging methods.
    /// </summary>
    internal class BitDebuger
    {
        #region Methods

        /// <summary>
        /// Converts byte array to bit(1 byte = 8 bit) representation.
        /// </summary>
        /// <param name="buffer">Data buffer.</param>
        /// <param name="count">Numer of bytes to convert.</param>
        /// <param name="bytesPerLine">Number of bytes per line.</param>
        /// <returns>Returns byte array as bit(1 byte = 8 bit) representation.</returns>
        public static string ToBit(byte[] buffer, int count, int bytesPerLine)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            StringBuilder retVal = new StringBuilder();

            int offset = 0;
            int bytesInCurrentLine = 1;
            while (offset < count)
            {
                byte currentByte = buffer[offset];
                char[] bits = new char[8];
                for (int i = 7; i >= 0; i--)
                {
                    bits[i] = ((currentByte >> (7 - i)) & 0x1).ToString()[0];
                }
                retVal.Append(bits);

                if (bytesInCurrentLine == bytesPerLine)
                {
                    retVal.AppendLine();
                    bytesInCurrentLine = 0;
                }
                else
                {
                    retVal.Append(" ");
                }
                bytesInCurrentLine++;
                offset++;
            }

            return retVal.ToString();
        }

        #endregion
    }
}