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


namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using System.Net;

    #endregion

    /// <summary>
    /// 
    /// </summary>
    public class SIP_Route
    {
        #region Properties

        /// <summary>
        /// Gets regex match pattern.
        /// </summary>
        public string MatchPattern
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets matched URI.
        /// </summary>
        public string Uri
        {
            get { return ""; }
        }

        /// <summary>
        /// Gets SIP targets for <b>Uri</b>.
        /// </summary>
        public string[] Targets
        {
            get { return null; }
        }

        /// <summary>
        /// Gets targets processing mode.
        /// </summary>
        public SIP_ForkingMode ProcessMode
        {
            get { return SIP_ForkingMode.Parallel; }
        }

        /// <summary>
        /// Gets if user needs to authenticate to use this route.
        /// </summary>
        public bool RequireAuthentication
        {
            get { return true; }
        }

        /// <summary>
        /// Gets targets credentials.
        /// </summary>
        public NetworkCredential[] Credentials
        {
            get { return null; }
        }

        #endregion
    }
}