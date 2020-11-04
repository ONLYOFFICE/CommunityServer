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


namespace ASC.Mail.Net.RTP
{
    /// <summary>
    /// This class holds known RTCP packet types.
    /// </summary>
    public class RTCP_PacketType
    {
        #region Members

        /// <summary>
        /// Application specifiec data.
        /// </summary>
        public const int APP = 204;

        /// <summary>
        /// BYE.
        /// </summary>
        public const int BYE = 203;

        /// <summary>
        /// Receiver report.
        /// </summary>
        public const int RR = 201;

        /// <summary>
        /// Session description.
        /// </summary>
        public const int SDES = 202;

        /// <summary>
        /// Sender report.
        /// </summary>
        public const int SR = 200;

        #endregion
    }
}