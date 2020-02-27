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
    using System.Text;

    #endregion

    /// <summary>
    /// This class proviedes data to asynchronous read line callback method.
    /// NOTE: ReadLine_EventArgs is reused for next read line call, so don't store references to this class.
    /// </summary>
    public class ReadLine_EventArgs
    {
        #region Members

        private int m_Count;
        private Exception m_pException;
        private byte[] m_pLineBuffer;
        private object m_pTag;
        private int m_ReadedCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets exception what happened while reading line. Returns null if read line completed sucessfully.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        /// <summary>
        /// Gets number of bytes actualy readed from source stream. Returns 0 if end of stream reached
        /// and no more data. This value includes any readed byte, including line feed, ... .
        /// </summary>
        public int ReadedCount
        {
            get { return m_ReadedCount; }
        }

        /// <summary>
        /// Gets <b>buffer</b> argumnet what was passed to BeginReadLine mehtod.
        /// </summary>
        public byte[] LineBuffer
        {
            get { return m_pLineBuffer; }
        }

        /// <summary>
        /// Gets number of bytes stored to <b>LineBuffer</b>.
        /// </summary>
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        /// Gets readed line data or null if end of stream reached and no more data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                byte[] retVal = new byte[m_Count];
                Array.Copy(m_pLineBuffer, retVal, m_Count);

                return retVal;
            }
        }

        /// <summary>
        /// Gets readed line data as string with system <b>default</b> encoding or returns null if end of stream reached and no more data.
        /// </summary>
        public string DataStringDefault
        {
            get { return DataToString(Encoding.Default); }
        }

        /// <summary>
        /// Gets readed line data as string with <b>ASCII</b> encoding or returns null if end of stream reached and no more data.
        /// </summary>
        public string DataStringAscii
        {
            get { return DataToString(Encoding.ASCII); }
        }

        /// <summary>
        /// Gets readed line data as string with <b>UTF8</b> encoding or returns null if end of stream reached and no more data.
        /// </summary>
        public string DataStringUtf8
        {
            get { return DataToString(Encoding.UTF8); }
        }

        /// <summary>
        /// Gets <b>tag</b> argument what was pased to BeginReadLine method.
        /// </summary>
        public object Tag
        {
            get { return m_pTag; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal ReadLine_EventArgs() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="exception">Exception what happened while reading data or null if read line was successfull.</param>
        /// <param name="readedCount">Specifies how many raw bytes was readed.</param>
        /// <param name="data">Line data buffer.</param>
        /// <param name="count">Specifies how many bytes stored to <b>data</b>.</param>
        /// <param name="tag">User data.</param>
        internal ReadLine_EventArgs(Exception exception, int readedCount, byte[] data, int count, object tag)
        {
            m_pException = exception;
            m_ReadedCount = readedCount;
            m_pLineBuffer = data;
            m_Count = count;
            m_pTag = tag;
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

            if (m_pLineBuffer == null)
            {
                return null;
            }
            else
            {
                return encoding.GetString(m_pLineBuffer, 0, m_Count);
            }
        }

        #endregion

        #region Internal methods

        internal void Reuse(Exception exception, int readedCount, byte[] data, int count, object tag)
        {
            m_pException = exception;
            m_ReadedCount = readedCount;
            m_pLineBuffer = data;
            m_Count = count;
            m_pTag = tag;
        }

        #endregion
    }
}