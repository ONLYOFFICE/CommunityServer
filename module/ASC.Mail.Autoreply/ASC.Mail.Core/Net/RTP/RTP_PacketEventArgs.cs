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
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class provides data for RTP packet related events/methods.
    /// </summary>
    public class RTP_PacketEventArgs : EventArgs
    {
        #region Members

        private readonly RTP_Packet m_pPacket;

        #endregion

        #region Properties

        /// <summary>
        /// Gets RTP packet.
        /// </summary>
        public RTP_Packet Packet
        {
            get { return m_pPacket; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="packet">RTP packet.</param>
        public RTP_PacketEventArgs(RTP_Packet packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            m_pPacket = packet;
        }

        #endregion
    }
}