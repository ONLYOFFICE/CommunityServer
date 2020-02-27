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


namespace ASC.Mail.Net
{
    #region usings

    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    #endregion

    /// <summary>
    /// This stream just junks all written data.
    /// </summary>
    public class JunkingStream : Stream
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the stream supports reading. This property always returns false.
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking. This property always returns false.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value that indicates whether the stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the length of the data available on the stream. This property always throws a NotSupportedException.
        /// </summary>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets or sets the current position in the stream. This property always throws a NotSupportedException.
        /// </summary>
        public override long Position
        {
            get { throw new NotSupportedException(); }

            set { throw new NotSupportedException(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Not used.
        /// </summary>
        public override void Flush() {}

        /// <summary>
        /// Sets the current position of the stream to the given value. This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="offset">This parameter is not used.</param>
        /// <param name="origin">This parameter is not used.</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the stream. This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="value">This parameter is not used.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads data from the stream. This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="buffer">This parameter is not used.</param>
        /// <param name="offset">This parameter is not used.</param>
        /// <param name="size">This parameter is not used.</param>
        /// <returns></returns>
        public override int Read([In, Out] byte[] buffer, int offset, int size)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to write to the stream.</param>
        /// <param name="offset">The location in buffer from which to start writing data.</param>
        /// <param name="size">The number of bytes to write to the stream.</param>
        public override void Write(byte[] buffer, int offset, int size) {}

        #endregion
    }
}