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

    using System;

    #endregion

    /// <summary>
    /// Specifies SIP proxy mode.
    /// <example>
    /// All flags may be combined, except Stateless,Statefull,B2BUA.
    /// For example: (Stateless | Statefull) not allowed, but (Registrar | Presence | Statefull) is allowed.
    /// </example>
    /// </summary>
    [Flags]
    public enum SIP_ProxyMode
    {
        /// <summary>
        /// Proxy implements SIP registrar.
        /// </summary>
        Registrar = 1,

        /// <summary>
        /// Proxy implements SIP presence server.
        /// </summary>
        Presence = 2,

        /// <summary>
        /// Proxy runs in stateless mode.
        /// </summary>
        Stateless = 4,

        /// <summary>
        /// Proxy runs in statefull mode.
        /// </summary>
        Statefull = 8,

        /// <summary>
        /// Proxy runs in B2BUA(back to back user agent) mode.
        /// </summary>
        B2BUA = 16,
    }
}