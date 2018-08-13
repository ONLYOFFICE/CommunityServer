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
    using System.Diagnostics;

    #endregion

    /// <summary>
    /// Provides data for the SysError event for servers.
    /// </summary>
    public class Error_EventArgs
    {
        #region Members

        private readonly Exception m_pException;
        private readonly StackTrace m_pStackTrace;
        private string m_Text = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="stackTrace"></param>
        public Error_EventArgs(Exception x, StackTrace stackTrace)
        {
            m_pException = x;
            m_pStackTrace = stackTrace;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Occured error's exception.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        /// <summary>
        /// Occured error's stacktrace.
        /// </summary>
        public StackTrace StackTrace
        {
            get { return m_pStackTrace; }
        }

        /// <summary>
        /// Gets comment text.
        /// </summary>
        public string Text
        {
            get { return m_Text; }
        }

        #endregion
    }
}