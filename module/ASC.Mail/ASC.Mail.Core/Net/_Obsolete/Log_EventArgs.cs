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
    /// <summary>
    /// Provides data for the SessionLog event.
    /// </summary>
    public class Log_EventArgs
    {
        #region Members

        private readonly bool m_FirstLogPart = true;
        private readonly bool m_LastLogPart;
        private readonly SocketLogger m_pLoggger;

        #endregion

        #region Properties

        /// <summary>
        /// Gets log text.
        /// </summary>
        public string LogText
        {
            get { return SocketLogger.LogEntriesToString(m_pLoggger, m_FirstLogPart, m_LastLogPart); }
        }

        /// <summary>
        /// Gets logger.
        /// </summary>
        public SocketLogger Logger
        {
            get { return m_pLoggger; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="logger">Socket logger.</param>
        /// <param name="firstLogPart">Specifies if first log part of multipart log.</param>
        /// <param name="lastLogPart">Specifies if last log part (logging ended).</param>
        public Log_EventArgs(SocketLogger logger, bool firstLogPart, bool lastLogPart)
        {
            m_pLoggger = logger;
            m_FirstLogPart = firstLogPart;
            m_LastLogPart = lastLogPart;
        }

        #endregion
    }
}