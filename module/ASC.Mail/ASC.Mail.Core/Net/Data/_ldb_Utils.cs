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