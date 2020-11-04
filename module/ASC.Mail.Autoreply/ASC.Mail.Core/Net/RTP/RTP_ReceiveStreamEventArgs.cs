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
    /// This method provides data for RTP receive stream related events and methods.
    /// </summary>
    public class RTP_ReceiveStreamEventArgs : EventArgs
    {
        #region Members

        private readonly RTP_ReceiveStream m_pStream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets RTP stream.
        /// </summary>
        public RTP_ReceiveStream Stream
        {
            get { return m_pStream; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">RTP stream.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public RTP_ReceiveStreamEventArgs(RTP_ReceiveStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            m_pStream = stream;
        }

        #endregion
    }
}