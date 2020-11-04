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
    using System.Text;

    #endregion

    /// <summary>
    /// This class provides data to asynchronous read to stream methods callback.
    /// </summary>
    public class ReadToStream_EventArgs
    {
        #region Members

        private readonly int m_Count;
        private readonly Exception m_pException;
        private readonly Stream m_pStream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets exception what happened while reading data. Returns null if data reading completed sucessfully.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        /// <summary>
        /// Gets stream where data is stored.
        /// </summary>
        public Stream Stream
        {
            get { return m_pStream; }
        }

        /// <summary>
        /// Gets number of bytes readed and written to <b>Stream</b>.
        /// </summary>
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        /// Gets readed data. NOTE: This property is available only is Stream supports seeking !
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (!m_pStream.CanSeek)
                {
                    throw new InvalidOperationException("Underlaying stream won't support seeking !");
                }

                long currentPos = m_pStream.Position;
                m_pStream.Position = 0;
                byte[] data = new byte[m_pStream.Length];
                m_pStream.Read(data, 0, data.Length);

                return data;
            }
        }

        /// <summary>
        /// Gets readed line data as string with system <b>default</b> encoding. 
        /// NOTE: This property is available only is Stream supports seeking !
        /// </summary>
        public string DataStringDefault
        {
            get { return DataToString(Encoding.Default); }
        }

        /// <summary>
        /// Gets readed line data as string with <b>ASCII</b> encoding. 
        /// NOTE: This property is available only is Stream supports seeking !
        /// </summary>
        public string DataStringAscii
        {
            get { return DataToString(Encoding.ASCII); }
        }

        /// <summary>
        /// Gets readed line data as string with <b>UTF8</b> encoding. 
        /// NOTE: This property is available only is Stream supports seeking !
        /// </summary>
        public string DataStringUtf8
        {
            get { return DataToString(Encoding.UTF8); }
        }

        /// <summary>
        /// Gets or stes user data.
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="exception">Exception what happened while reading data or null if data reading was successfull.</param>
        /// <param name="stream">Stream where data was stored.</param>
        /// <param name="count">Number of bytes readed.</param>
        /// <param name="tag">User data.</param>
        public ReadToStream_EventArgs(Exception exception, Stream stream, int count, object tag)
        {
            m_pException = exception;
            m_pStream = stream;
            m_Count = count;
            Tag = tag;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts byte[] line data to the specified encoding string.
        /// </summary>
        /// <param name="encoding">Encoding to use for convert.</param>
        /// <returns>Returns line data as string.</returns>
        public string DataToString(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if (Data == null)
            {
                return null;
            }
            else
            {
                return encoding.GetString(Data);
            }
        }

        #endregion
    }
}