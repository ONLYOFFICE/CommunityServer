/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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