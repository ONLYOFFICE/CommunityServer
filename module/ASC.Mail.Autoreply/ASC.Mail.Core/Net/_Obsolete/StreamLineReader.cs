/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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