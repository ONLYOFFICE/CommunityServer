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