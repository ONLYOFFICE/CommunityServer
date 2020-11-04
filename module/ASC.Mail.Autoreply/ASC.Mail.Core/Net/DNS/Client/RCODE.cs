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
    /// Dns server reply codes.
    /// </summary>
    public enum RCODE
    {
        /// <summary>
        /// No error condition.
        /// </summary>
        NO_ERROR = 0,

        /// <summary>
        /// Format error - The name server was unable to interpret the query.
        /// </summary>
        FORMAT_ERRROR = 1,

        /// <summary>
        /// Server failure - The name server was unable to process this query due to a problem with the name server.
        /// </summary>
        SERVER_FAILURE = 2,

        /// <summary>
        /// Name Error - Meaningful only for responses from an authoritative name server, this code signifies that the
        /// domain name referenced in the query does not exist.
        /// </summary>
        NAME_ERROR = 3,

        /// <summary>
        /// Not Implemented - The name server does not support the requested kind of query.
        /// </summary>
        NOT_IMPLEMENTED = 4,

        /// <summary>
        /// Refused - The name server refuses to perform the specified operation for policy reasons.
        /// </summary>
        REFUSED = 5,
    }
}