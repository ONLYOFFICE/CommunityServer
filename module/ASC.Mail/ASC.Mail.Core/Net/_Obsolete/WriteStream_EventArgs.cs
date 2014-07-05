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