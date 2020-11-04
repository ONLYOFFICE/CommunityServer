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
    /// <summary>
    /// Specifies SIP status code type. Defined in rfc 3261.
    /// </summary>
    public enum SIP_StatusCodeType
    {
        /// <summary>
        /// Request received, continuing to process the request. 1xx status code.
        /// </summary>
        Provisional,

        /// <summary>
        /// Action was successfully received, understood, and accepted. 2xx status code.
        /// </summary>
        Success,

        /// <summary>
        /// Request must be redirected(forwarded). 3xx status code.
        /// </summary>
        Redirection,

        /// <summary>
        /// Request contains bad syntax or cannot be fulfilled at this server. 4xx status code.
        /// </summary>
        RequestFailure,

        /// <summary>
        /// Server failed to fulfill a valid request. 5xx status code.
        /// </summary>
        ServerFailure,

        /// <summary>
        /// Request cannot be fulfilled at any server. 6xx status code.
        /// </summary>
        GlobalFailure
    }
}