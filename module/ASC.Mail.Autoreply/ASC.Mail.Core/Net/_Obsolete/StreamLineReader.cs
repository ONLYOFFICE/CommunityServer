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
    using System.IO;

    #endregion

    /// <summary>
    /// Stream line reader.
    /// </summary>
    //[Obsolete("Use StreamHelper instead !")]
    public class StreamLineReader
    {
        #region Members

        private readonly byte[] m_Buffer = new byte[1024];
        private readonly Stream m_StrmSource;
        private bool m_CRLF_LinesOnly = true;
        private string m_Encoding = "";
        private int m_ReadBufferSize = 1024;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets charset encoding to use for string based methods. Default("") encoding is system default encoding.
        /// </summary>
        public string Encoding
        {
            get { return m_Encoding; }

            set
            {
                // Check if encoding is valid
                System.Text.Encoding.GetEncoding(value);

                m_Encoding = value;
            }
        }

        /// <summary>
        /// Gets or sets if lines must be CRLF terminated or may be only LF terminated too.
        /// </summary>
        public bool CRLF_LinesOnly
        {
            get { return m_CRLF_LinesOnly; }

            set { m_CRLF_LinesOnly = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="strmSource">Source stream from where to read data. Reading begins from stream current position.</param>
        public StreamLineReader(Stream strmSource)
        {
            m_StrmSource = strmSource;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads byte[] line from stream. NOTE: Returns null if end of stream reached.
        /// </summary>
        /// <returns>Return null if end of stream reached.</returns>
        public byte[] ReadLine()
        {
            // TODO: Allow to buffer source stream reads

            byte[] buffer = m_Buffer;
            int posInBuffer = 0;

            int prevByte = m_StrmSource.ReadByte();
            int currByteInt = m_StrmSource.ReadByte();
            while (prevByte > -1)
            {
                // CRLF line found
                if ((prevByte == (byte) '\r' && (byte) currByteInt == (byte) '\n'))
                {
                    byte[] retVal = new byte[posInBuffer];
                    Array.Copy(buffer, retVal, posInBuffer);
                    return retVal;
                }
                    // LF line found and only LF lines allowed
                else if (!m_CRLF_LinesOnly && currByteInt == '\n')
                {
                    byte[] retVal = new byte[posInBuffer + 1];
                    Array.Copy(buffer, retVal, posInBuffer + 1);
                    retVal[posInBuffer] = (byte) prevByte;
                    return retVal;
                }

                // Buffer is full, add addition m_ReadBufferSize bytes
                if (posInBuffer == buffer.Length)
                {
                    byte[] newBuffer = new byte[buffer.Length + m_ReadBufferSize];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    buffer = newBuffer;
                }
                buffer[posInBuffer] = (byte) prevByte;
                posInBuffer++;
                prevByte = currByteInt;

                // Read next byte
                currByteInt = m_StrmSource.ReadByte();
            }

            // Line isn't terminated with <CRLF> and has some bytes left, return them.
            if (posInBuffer > 0)
            {
                byte[] retVal = new byte[posInBuffer];
                Array.Copy(buffer, retVal, posInBuffer);
                return retVal;
            }

            return null;
        }

        /// <summary>
        /// Reads string line from stream. String is converted with specified Encoding property from byte[] line. NOTE: Returns null if end of stream reached.
        /// </summary>
        /// <returns></returns>
        public string ReadLineString()
        {
            byte[] line = ReadLine();
            if (line != null)
            {
                if (m_Encoding == null || m_Encoding == "")
                {
                    return System.Text.Encoding.Default.GetString(line);
                }
                else
                {
                    return System.Text.Encoding.GetEncoding(m_Encoding).GetString(line);
                }
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}