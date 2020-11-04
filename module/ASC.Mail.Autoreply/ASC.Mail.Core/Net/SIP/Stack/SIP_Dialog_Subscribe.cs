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
    #region usings

    using System;

    #endregion

    /// <summary>
    /// This class represent SUBSCRIBE dialog. Defined in RFC 3265.
    /// </summary>
    public class SIP_Dialog_Subscribe
    {
        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal SIP_Dialog_Subscribe() {}

        #endregion

        #region Methods

        /// <summary>
        /// Sends notify request to remote end point.
        /// </summary>
        /// <param name="notify">SIP NOTIFY request.</param>
        public void Notify(SIP_Request notify)
        {
            if (notify == null)
            {
                throw new ArgumentNullException("notify");
            }

            // TODO:
        }

        #endregion
    }
}