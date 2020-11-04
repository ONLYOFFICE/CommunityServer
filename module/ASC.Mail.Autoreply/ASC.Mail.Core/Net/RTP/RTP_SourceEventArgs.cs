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
    /// This class provides data for RTP source related evetns.
    /// </summary>
    public class RTP_SourceEventArgs : EventArgs
    {
        #region Members

        private readonly RTP_Source m_pSource;

        #endregion

        #region Properties

        /// <summary>
        /// Gets RTP source.
        /// </summary>
        public RTP_Source Source
        {
            get { return m_pSource; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="source">RTP source.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>source</b> is null reference.</exception>
        public RTP_SourceEventArgs(RTP_Source source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            m_pSource = source;
        }

        #endregion
    }
}