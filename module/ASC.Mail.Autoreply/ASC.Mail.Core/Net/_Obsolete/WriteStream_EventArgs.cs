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


namespace ASC.Mail.Net.IO
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// This class provides data to asynchronous write from stream methods callback.
    /// </summary>
    public class WriteStream_EventArgs
    {
        #region Members

        private readonly int m_CountReaded;
        private readonly int m_CountWritten;
        private readonly Exception m_pException;
        private readonly Stream m_pStream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets exception happened during write or null if operation was successfull.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        /// <summary>
        /// Gets stream what data was written.
        /// </summary>
        public Stream Stream
        {
            get { return m_pStream; }
        }

        /// <summary>
        /// Gets number of bytes readed from <b>Stream</b>.
        /// </summary>
        public int CountReaded
        {
            get { return m_CountReaded; }
        }

        /// <summary>
        /// Gets number of bytes written to source stream.
        /// </summary>
        public int CountWritten
        {
            get { return m_CountWritten; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="exception">Exception happened during write or null if operation was successfull.</param>
        /// <param name="stream">Stream which data was written.</param>
        /// <param name="countReaded">Number of bytes readed from <b>stream</b>.</param>
        /// <param name="countWritten">Number of bytes written to source stream.</param>
        internal WriteStream_EventArgs(Exception exception, Stream stream, int countReaded, int countWritten)
        {
            m_pException = exception;
            m_pStream = stream;
            m_CountReaded = countReaded;
            m_CountWritten = countWritten;
        }

        #endregion
    }
}