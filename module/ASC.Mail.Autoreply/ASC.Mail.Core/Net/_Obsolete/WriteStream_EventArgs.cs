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