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
    /// This claass provides data for <b>RTP_MultimediaSession.NewParticipant</b> event.
    /// </summary>
    public class RTP_ParticipantEventArgs : EventArgs
    {
        #region Members

        private readonly RTP_Participant_Remote m_pParticipant;

        #endregion

        #region Properties

        /// <summary>
        /// Gets participant.
        /// </summary>
        public RTP_Participant_Remote Participant
        {
            get { return m_pParticipant; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="participant">RTP participant.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>participant</b> is null reference.</exception>
        public RTP_ParticipantEventArgs(RTP_Participant_Remote participant)
        {
            if (participant == null)
            {
                throw new ArgumentNullException("participant");
            }

            m_pParticipant = participant;
        }

        #endregion
    }
}