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


namespace ASC.Mail.Net.STUN.Client
{
    #region usings

    using System.Net;

    #endregion

    /// <summary>
    /// This class holds STUN_Client.Query method return data.
    /// </summary>
    public class STUN_Result
    {
        #region Members

        private readonly STUN_NetType m_NetType = STUN_NetType.OpenInternet;
        private readonly IPEndPoint m_pPublicEndPoint;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="netType">Specifies UDP network type.</param>
        /// <param name="publicEndPoint">Public IP end point.</param>
        public STUN_Result(STUN_NetType netType, IPEndPoint publicEndPoint)
        {
            m_NetType = netType;
            m_pPublicEndPoint = publicEndPoint;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets UDP network type.
        /// </summary>
        public STUN_NetType NetType
        {
            get { return m_NetType; }
        }

        /// <summary>
        /// Gets public IP end point. This value is null if failed to get network type.
        /// </summary>
        public IPEndPoint PublicEndPoint
        {
            get { return m_pPublicEndPoint; }
        }

        #endregion
    }
}