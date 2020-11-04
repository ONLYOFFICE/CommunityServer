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


namespace ASC.Mail.Net
{
    /// <summary>
    /// This class represents well known URI schemes.
    /// </summary>
    public class UriSchemes
    {
        #region Constants

        /// <summary>
        /// HTTP Extensions for Distributed Authoring (WebDAV).
        /// </summary>
        public const string dav = "dav";

        /// <summary>
        /// Addressing files on local or network file systems.
        /// </summary>
        public const string file = "file";

        /// <summary>
        /// FTP resources.
        /// </summary>
        public const string ftp = "ftp";

        /// <summary>
        /// HTTP resources.
        /// </summary>
        public const string http = "http";

        /// <summary>
        /// HTTP connections secured using SSL/TLS.
        /// </summary>
        public const string https = "https";

        /// <summary>
        /// SMTP e-mail addresses and default content.
        /// </summary>
        public const string mailto = "mailto";

        /// <summary>
        /// Session Initiation Protocol (SIP).
        /// </summary>
        public const string sip = "sip";

        /// <summary>
        /// Session Initiation Protocol (SIP) using TLS.
        /// </summary>
        public const string sips = "sips";

        /// <summary>
        /// Telephone numbers.
        /// </summary>
        public const string tel = "tel";

        #endregion
    }
}