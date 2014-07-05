/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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