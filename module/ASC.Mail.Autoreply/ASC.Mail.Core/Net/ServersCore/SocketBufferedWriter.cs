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