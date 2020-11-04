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


namespace ASC.Mail.Net.Dns.Client
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// DNS client exception.
    /// </summary>
    public class DNS_ClientException : Exception
    {
        #region Members

        private readonly RCODE m_RCode;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="rcode">DNS server returned error code.</param>
        public DNS_ClientException(RCODE rcode)
        {
            m_RCode = rcode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets DNS server returned error code.
        /// </summary>
        public RCODE ErrorCode
        {
            get { return m_RCode; }
        }

        #endregion
    }
}