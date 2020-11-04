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
    /// <summary>
    /// This enum specifies SIP proxy server 'forking' mode.
    /// </summary>
    public enum SIP_ForkingMode
    {
        /// <summary>
        /// No forking. The contact with highest q value is used.
        /// </summary>
        None,

        /// <summary>
        /// All contacts are processed parallel at same time.
        /// </summary>
        Parallel,

        /// <summary>
        /// In a sequential search, a proxy server attempts each contact address in sequence, 
        /// proceeding to the next one only after the previous has generated a final response. 
        /// Contacts are processed from highest q value to lower.
        /// </summary>
        Sequential,
    }
}