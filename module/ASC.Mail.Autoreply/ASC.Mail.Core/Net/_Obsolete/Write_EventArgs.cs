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


namespace ASC.Mail.Net.IO
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for BeginWriteCallback delegate.
    /// </summary>
    public class Write_EventArgs
    {
        #region Members

        private readonly Exception m_pException;

        #endregion

        #region Properties

        /// <summary>
        /// Gets exception happened during write or null if operation was successfull.
        /// </summary>
        public Exception Exception
        {
            get { return m_pException; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="exception">Exception happened during write or null if operation was successfull.</param>
        internal Write_EventArgs(Exception exception)
        {
            m_pException = exception;
        }

        #endregion
    }
}