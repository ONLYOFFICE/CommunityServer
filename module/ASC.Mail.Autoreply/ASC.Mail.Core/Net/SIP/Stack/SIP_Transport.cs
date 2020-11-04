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


namespace ASC.Mail.Net.SIP.Stack
{
    /// <summary>
    /// This class holds SIP transports. Defined in RFC 3261.
    /// </summary>
    public class SIP_Transport
    {
        #region Constants

        /// <summary>
        /// TCP protocol.
        /// </summary>
        public const string TCP = "TCP";

        /// <summary>
        /// TCP + SSL protocol.
        /// </summary>
        public const string TLS = "TLS";

        /// <summary>
        /// UDP protocol.
        /// </summary>
        public const string UDP = "UDP";

        #endregion
    }
}