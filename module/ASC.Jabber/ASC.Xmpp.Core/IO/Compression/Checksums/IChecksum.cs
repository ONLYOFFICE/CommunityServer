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
// // <copyright company="Ascensio System Limited" file="IChecksum.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

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