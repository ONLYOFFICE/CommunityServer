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
    /// This class provides well known TCP/UDP service ports.
    /// </summary>
    public static class WellKnownPorts
    {
        #region Members

        /// <summary>
        /// DNS protocol.
        /// </summary>
        public const int DNS = 53;

        /// <summary>
        /// FTP - control (command) port.
        /// </summary>
        public const int FTP_CONTROL = 21;

        /// <summary>
        /// FTP over SSL protocol.
        /// </summary>
        public const int FTP_CONTROL_SSL = 990;

        /// <summary>
        /// FTP - data port.
        /// </summary>
        public const int FTP_DATA = 20;

        /// <summary>
        /// HTTP protocol.
        /// </summary>
        public const int HTTP = 80;

        /// <summary>
        /// HTTPS protocol.
        /// </summary>
        public const int HTTPS = 443;

        /// <summary>
        /// IMAP4 protocol.
        /// </summary>
        public const int IMAP4 = 143;

        /// <summary>
        /// IMAP4 over SSL protocol.
        /// </summary>
        public const int IMAP4_SSL = 993;

        /// <summary>
        /// NNTP (Network News Transfer Protocol)  protocol.
        /// </summary>
        public const int NNTP = 119;

        /// <summary>
        /// NTP (Network Time Protocol) protocol.
        /// </summary>
        public const int NTP = 123;

        /// <summary>
        /// POP3 protocol.
        /// </summary>
        public const int POP3 = 110;

        /// <summary>
        /// POP3 over SSL protocol.
        /// </summary>
        public const int POP3_SSL = 995;

        /// <summary>
        /// SMTP protocol.
        /// </summary>
        public const int SMTP = 25;

        /// <summary>
        /// SMTP over SSL protocol.
        /// </summary>
        public const int SMTP_SSL = 465;

        #endregion
    }
}