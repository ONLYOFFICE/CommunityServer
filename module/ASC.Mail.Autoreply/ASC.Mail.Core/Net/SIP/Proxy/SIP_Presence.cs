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

    using Stack;

    #endregion

    /// <summary>
    /// This class implements SIP presence server.
    /// </summary>
    public class SIP_Presence
    {
        #region Internal methods

        /// <summary>
        /// Handles SUBSCRIBE method.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        internal void Subscribe(SIP_RequestReceivedEventArgs e) {}

        /// <summary>
        /// Handles NOTIFY method.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        internal void Notify(SIP_RequestReceivedEventArgs e) {}

        #endregion
    }
}