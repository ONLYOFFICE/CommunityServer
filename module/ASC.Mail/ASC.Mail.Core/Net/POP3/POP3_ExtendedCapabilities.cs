/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.POP3
{
    /// <summary>
    /// This class holds known POP3 extended capabilities. Defined in http://www.iana.org/assignments/pop3-extension-mechanism.
    /// </summary>
    public class POP3_ExtendedCapabilities
    {
        /// <summary>
        /// The TOP capability indicates the optional TOP command is available. Defined in RFC 2449.
        /// </summary>
        public static readonly string TOP = "TOP";

        /// <summary>
        /// The USER capability indicates that the USER and PASS commands are supported. Defined in RFC 2449.
        /// </summary>
        public static readonly string USER = "USER";

        /// <summary>
        /// The SASL capability indicates that the AUTH command is available and that it supports an optional base64 
        /// encoded second argument for an initial client response as described in the SASL specification. Defined in RFC 2449.
        /// </summary>
        public static readonly string SASL = "SASL";

        /// <summary>
        /// The RESP-CODES capability indicates that any response text issued by this server which begins with an open 
        /// square bracket ("[") is an extended response code. Defined in RFC 2449.
        /// </summary>
        public static readonly string RESP_CODES = "RESP-CODES";

        /// <summary>
        /// LOGIN-DELAY capability. Defined in RFC 2449.
        /// </summary>
        public static readonly string LOGIN_DELAY = "LOGIN-DELAY";

        /// <summary>
        /// The PIPELINING capability indicates the server is capable of accepting multiple commands at a time; 
        /// the client does not have to wait for the response to a command before issuing a subsequent command.
        ///  Defined in RFC 2449.
        /// </summary>
        public static readonly string PIPELINING = "PIPELINING";

        /// <summary>
        /// EXPIRE capability. Defined in RFC 2449.
        /// </summary>
        public static readonly string EXPIRE = "EXPIRE";

        /// <summary>
        /// UIDL command is supported. Defined in RFC 2449.
        /// </summary>
        public static readonly string UIDL = "UIDL";

        /// <summary>
        /// STLS(start TLS) command supported.  Defined in RFC 2449.
        /// </summary>
        public static readonly string STLS = "STLS";
    }
}
