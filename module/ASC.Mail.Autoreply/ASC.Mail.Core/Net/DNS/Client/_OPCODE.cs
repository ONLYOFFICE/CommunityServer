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


namespace ASC.Mail.Net.Dns.Client
{
    /// <summary>
    /// 
    /// </summary>
    internal enum OPCODE
    {
        /// <summary>
        /// A standard query.
        /// </summary>
        QUERY = 0,

        /// <summary>
        /// An inverse query.
        /// </summary>
        IQUERY = 1,

        /// <summary>
        /// A server status request.
        /// </summary>
        STATUS = 2,
    }
}