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
