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


namespace ASC.Mail.Net.SMTP
{
    /// <summary>
    ///  This class holds known SMTP service extensions. Defined in http://www.iana.org/assignments/mail-parameters.
    /// </summary>
    public class SMTP_ServiceExtensions
    {
        #region Members

        /// <summary>
        /// Use 8-bit data. Defined in RFC 1652.
        /// </summary>
        public static readonly string _8BITMIME = "8BITMIME";

        /// <summary>
        /// Authenticated TURN. Defined in RFC 2645.
        /// </summary>
        public static readonly string ATRN = "ATRN";

        /// <summary>
        /// Authentication. Defined in RFC 4954.
        /// </summary>
        public static readonly string AUTH = "AUTH";

        /// <summary>
        /// Binary MIME. Defined in RFC 3030.
        /// </summary>
        public static readonly string BINARYMIME = "BINARYMIME";

        /// <summary>
        /// Remote Content. Defined in RFC 4468.
        /// </summary>
        public static readonly string BURL = "BURL";

        /// <summary>
        /// Checkpoint/Restart. Defined in RFC 1845.
        /// </summary>
        public static readonly string CHECKPOINT = "CHECKPOINT";

        /// <summary>
        /// Chunking. Defined in RFC 3030.
        /// </summary>
        public static readonly string CHUNKING = "CHUNKING";

        /// <summary>
        /// Delivery Status Notification. Defined in RFC 1891.
        /// </summary>
        public static readonly string DSN = "DSN";

        /// <summary>
        /// Enhanced Status Codes. Defined in RFC 2034.
        /// </summary>
        public static readonly string ENHANCEDSTATUSCODES = "ENHANCEDSTATUSCODES";

        /// <summary>
        /// Extended Turn. Defined in RFC 1985.
        /// </summary>
        public static readonly string ETRN = "ETRN";

        /// <summary>
        /// Expand the mailing list. Defined in RFC 821,
        /// </summary>
        public static readonly string EXPN = "EXPN";

        /// <summary>
        /// Future Message Release. Defined in RFC 4865.
        /// </summary>
        public static readonly string FUTURERELEASE = "FUTURERELEASE";

        /// <summary>
        /// Supply helpful information. Defined in RFC 821.
        /// </summary>
        public static readonly string HELP = "HELP";

        /// <summary>
        /// Message Tracking. Defined in RFC 3885.
        /// </summary>
        public static readonly string MTRK = "MTRK";

        /// <summary>
        /// Notification of no soliciting. Defined in RFC 3865.
        /// </summary>
        public static readonly string NO_SOLICITING = "NO-SOLICITING";

        /// <summary>
        /// Command Pipelining. Defined in RFC 2920.
        /// </summary>
        public static readonly string PIPELINING = "PIPELINING";

        /// <summary>
        /// Send as mail and terminal. Defined in RFC 821.
        /// </summary>
        public static readonly string SAML = "SAML";

        /// <summary>
        /// Send as mail. Defined in RFC RFC 821.
        /// </summary>
        public static readonly string SEND = "SEND";

        /// <summary>
        /// Message size declaration. Defined in RFC 1870.
        /// </summary>
        public static readonly string SIZE = "SIZE";

        /// <summary>
        /// Send as mail or terminal. Defined in RFC 821.
        /// </summary>
        public static readonly string SOML = "SOML";

        /// <summary>
        /// Start TLS. Defined in RFC 3207.
        /// </summary>
        public static readonly string STARTTLS = "STARTTLS";

        /// <summary>
        /// SMTP Responsible Submitter. Defined in RFC 4405.
        /// </summary>
        public static readonly string SUBMITTER = "SUBMITTER";

        /// <summary>
        /// Turn the operation around. Defined in RFC 821.
        /// </summary>
        public static readonly string TURN = "TURN";

        #endregion
    }
}