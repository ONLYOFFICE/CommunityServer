/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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