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