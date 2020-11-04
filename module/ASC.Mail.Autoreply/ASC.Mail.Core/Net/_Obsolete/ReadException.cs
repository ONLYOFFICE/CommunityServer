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

    #endregion

    #region public enum ReadReplyCode

    /// <summary>
    /// Reply reading return codes.
    /// </summary>
    public enum ReadReplyCode
    {
        /// <summary>
        /// Read completed successfully.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Read timed out.
        /// </summary>
        TimeOut = 1,

        /// <summary>
        /// Maximum allowed Length exceeded.
        /// </summary>
        LengthExceeded = 2,

        /// <summary>
        /// Connected client closed connection.
        /// </summary>
        SocketClosed = 3,

        /// <summary>
        /// UnKnown error, eception raised.
        /// </summary>
        UnKnownError = 4,
    }

    #endregion

    /// <summary>
    /// Summary description for ReadException.
    /// </summary>
    public class ReadException : Exception
    {
        #region Members

        private readonly ReadReplyCode m_ReadReplyCode;

        #endregion

        #region Properties

        /// <summary>
        /// Gets read error.
        /// </summary>
        public ReadReplyCode ReadReplyCode
        {
            get { return m_ReadReplyCode; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ReadException(ReadReplyCode code, string message) : base(message)
        {
            m_ReadReplyCode = code;
        }

        #endregion
    }
}