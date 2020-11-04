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


namespace ASC.Mail.Net.AUTH
{
    /// <summary>
    /// SASL authentications
    /// </summary>
    public enum SaslAuthTypes
    {
        /// <summary>
        /// Non authentication
        /// </summary>
        None = 0,

        /// <summary>
        /// Plain text authentication. For POP3 USER/PASS commands, for IMAP LOGIN command.
        /// </summary>
        Plain = 1,

        /// <summary>
        /// LOGIN.
        /// </summary>
        Login = 2,

        /// <summary>
        /// CRAM-MD5
        /// </summary>
        Cram_md5 = 4,

        /// <summary>
        /// DIGEST-MD5.
        /// </summary>
        Digest_md5 = 8,

        /// <summary>
        /// All authentications.
        /// </summary>
        All = 0xF,
    }
}