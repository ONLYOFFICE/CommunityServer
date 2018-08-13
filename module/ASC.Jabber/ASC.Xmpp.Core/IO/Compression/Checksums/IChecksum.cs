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


namespace ASC.Xmpp.Core.IO.Compression.Checksums
{
    /// <summary>
    ///   Interface to compute a data checksum used by checked input/output streams. A data checksum can be updated by one byte or with a byte array. After each update the value of the current checksum can be returned by calling <code>getValue</code> . The complete checksum object can also be reset so it can be used again with new data.
    /// </summary>
    public interface IChecksum
    {
        #region Properties

        /// <summary>
        ///   Returns the data checksum computed so far.
        /// </summary>
        long Value { get; }

        #endregion

        #region Methods

        /// <summary>
        ///   Resets the data checksum as if no update was ever called.
        /// </summary>
        void Reset();

        /// <summary>
        ///   Adds one byte to the data checksum.
        /// </summary>
        /// <param name="bval"> the data value to add. The high byte of the int is ignored. </param>
        void Update(int bval);

        /// <summary>
        ///   Updates the data checksum with the bytes taken from the array.
        /// </summary>
        /// <param name="buffer"> buffer an array of bytes </param>
        void Update(byte[] buffer);

        /// <summary>
        ///   Adds the byte array to the data checksum.
        /// </summary>
        /// <param name="buf"> the buffer which contains the data </param>
        /// <param name="off"> the offset in the buffer where the data starts </param>
        /// <param name="len"> the length of the data </param>
        void Update(byte[] buf, int off, int len);

        #endregion
    }
}