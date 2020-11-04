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


namespace ASC.Mail.Data.lsDB
{
    /// <summary>
    ///LDB utility methods.
    /// </summary>
    internal class ldb_Utils
    {
        #region Methods

        /// <summary>
        /// Convert long value to byte[8].
        /// </summary>
        /// <param name="val">Long value.</param>
        /// <returns></returns>
        public static byte[] LongToByte(long val)
        {
            byte[] retVal = new byte[8];
            retVal[0] = (byte) ((val >> 56) & 0xFF);
            retVal[1] = (byte) ((val >> 48) & 0xFF);
            retVal[2] = (byte) ((val >> 40) & 0xFF);
            retVal[3] = (byte) ((val >> 32) & 0xFF);
            retVal[4] = (byte) ((val >> 24) & 0xFF);
            retVal[5] = (byte) ((val >> 16) & 0xFF);
            retVal[6] = (byte) ((val >> 8) & 0xFF);
            retVal[7] = (byte) ((val >> 0) & 0xFF);

            return retVal;
        }

        /// <summary>
        /// Converts 8 bytes to long value. Offset byte is included.
        /// </summary>
        /// <param name="array">Data array.</param>
        /// <param name="offset">Offset where 8 bytes long value starts. Offset byte is included.</param>
        /// <returns></returns>
        public static long ByteToLong(byte[] array, int offset)
        {
            long retVal = 0;
            retVal |= (long) array[offset + 0] << 56;
            retVal |= (long) array[offset + 1] << 48;
            retVal |= (long) array[offset + 2] << 40;
            retVal |= (long) array[offset + 3] << 32;
            retVal |= (long) array[offset + 4] << 24;
            retVal |= (long) array[offset + 5] << 16;
            retVal |= (long) array[offset + 6] << 8;
            retVal |= (long) array[offset + 7] << 0;

            return retVal;
        }

        /// <summary>
        /// Convert int value to byte[4].
        /// </summary>
        /// <param name="val">Int value.</param>
        /// <returns></returns>
        public static byte[] IntToByte(int val)
        {
            byte[] retVal = new byte[4];
            retVal[0] = (byte) ((val >> 24) & 0xFF);
            retVal[1] = (byte) ((val >> 16) & 0xFF);
            retVal[2] = (byte) ((val >> 8) & 0xFF);
            retVal[3] = (byte) ((val >> 0) & 0xFF);

            return retVal;
        }

        /// <summary>
        /// Converts 4 bytes to int value.  Offset byte is included.
        /// </summary>
        /// <param name="array">Data array.</param>
        /// <param name="offset">Offset where 4 bytes int value starts. Offset byte is included.</param>
        /// <returns></returns>
        public static int ByteToInt(byte[] array, int offset)
        {
            int retVal = 0;
            retVal |= array[offset + 0] << 24;
            retVal |= array[offset + 1] << 16;
            retVal |= array[offset + 2] << 8;
            retVal |= array[offset + 3] << 0;

            return retVal;
        }

        #endregion
    }
}