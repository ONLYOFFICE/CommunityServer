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

    using System;
    using Stack;

    #endregion

    /// <summary>
    /// Represents SIP proxy target in the SIP proxy "target set". Defined in RFC 3261 16.
    /// </summary>
    public class SIP_ProxyTarget
    {
        #region Members

        private readonly SIP_Flow m_pFlow;
        private readonly SIP_Uri m_pTargetUri;

        #endregion

        #region Properties

        /// <summary>
        /// Gets target URI.
        /// </summary>
        public SIP_Uri TargetUri
        {
            get { return m_pTargetUri; }
        }

        /// <summary>
        /// Gets data flow. Value null means that new flow must created.
        /// </summary>
        public SIP_Flow Flow
        {
            get { return m_pFlow; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="targetUri">Target request-URI.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>targetUri</b> is null reference.</exception>
        public SIP_ProxyTarget(SIP_Uri targetUri) : this(targetUri, null) {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="targetUri">Target request-URI.</param>
        /// <param name="flow">Data flow to try for forwarding..</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>targetUri</b> is null reference.</exception>
        public SIP_ProxyTarget(SIP_Uri targetUri, SIP_Flow flow)
        {
            if (targetUri == null)
            {
                throw new ArgumentNullException("targetUri");
            }

            m_pTargetUri = targetUri;
            m_pFlow = flow;
        }

        #endregion
    }
}