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