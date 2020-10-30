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


namespace ASC.Mail.Enums
{
    /// <summary>
    /// Mechanisms available for authentication.
    /// </summary>
    public enum SaslMechanism
    {
        /// <summary>
        /// Authentication doesn't needed
        /// </summary>
        None = 0,
        /// <summary>
        /// The LOGIN mechanism (BASE64 encoded exchanges).
        /// </summary>
        Login = 1,
        /*/// <summary>
        /// The PLAIN mechanism (identity<NUL>username<NUL>password).
        /// </summary>
        Plain = 2,*/
        /*/// <summary>
        /// The DIGEST-MD5 mechanism. [RFC2831]
        /// </summary>
        DigestMd5 = 3,*/
        /// <summary>
        /// The CRAM-MD5 mechanism. [RFC2195]
        /// </summary>
        CramMd5 = 4,
        /// <summary>
        /// The OAuth 2.0 mechanism. [draft-ietf-oauth-v2-31]
        /// </summary>
        OAuth2 = 5
        /*/// <summary>
        /// The KERBEROS_V4 mechanism. [RFC2222]
        /// </summary>
        KerberosV4 = 6*/
    }
}
