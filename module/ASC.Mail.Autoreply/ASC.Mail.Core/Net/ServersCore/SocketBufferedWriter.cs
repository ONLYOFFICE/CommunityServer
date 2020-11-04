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
    using System.Text;

    #endregion

    /// <summary>
    /// Implements buffered writer for socket.
    /// </summary>
    public class SocketBufferedWriter
    {
        #region Members

        private readonly byte[] m_Buffer;
        private readonly SocketEx m_pSocket;
        private int m_AvailableInBuffer;
        private int m_BufferSize = 8000;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="socket">Socket where to write data.</param>
        public SocketBufferedWriter(SocketEx socket)
        {
            m_pSocket = socket;

            m_Buffer = new byte[m_BufferSize];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Forces to send all data in buffer to destination host.
        /// </summary>
        public void Flush()
        {
            if (m_AvailableInBuffer > 0)
            {
                m_pSocket.Write(m_Buffer, 0, m_AvailableInBuffer);
                m_AvailableInBuffer = 0;
            }
        }

        /// <summary>
        /// Queues specified data to write buffer. If write buffer is full, buffered data will be sent to detination host.
        /// </summary>
        /// <param name="data">Data to queue.</param>
        public void Write(string data)
        {
            Write(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Queues specified data to write buffer. If write buffer is full, buffered data will be sent to detination host.
        /// </summary>
        /// <param name="data">Data to queue.</param>
        public void Write(byte[] data)
        {
            // There is no room to accomodate data to buffer
            if ((m_AvailableInBuffer + data.Length) > m_BufferSize)
            {
                // Send buffer data
                m_pSocket.Write(m_Buffer, 0, m_AvailableInBuffer);
                m_AvailableInBuffer = 0;

                // Store new data to buffer
                if (data.Length < m_BufferSize)
                {
                    Array.Copy(data, m_Buffer, data.Length);
                    m_AvailableInBuffer = data.Length;
                }
                    // Buffer is smaller than data, send it directly
                else
                {
                    m_pSocket.Write(data);
                }
            }
                // Store data to buffer
            else
            {
                Array.Copy(data, 0, m_Buffer, m_AvailableInBuffer, data.Length);
                m_AvailableInBuffer += data.Length;
            }
        }

        #endregion
    }
}