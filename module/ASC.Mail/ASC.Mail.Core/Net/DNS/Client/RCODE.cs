/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


namespace ASC.Mail.Net.Dns.Client
{
    /// <summary>
    /// Dns server reply codes.
    /// </summary>
    public enum RCODE
    {
        /// <summary>
        /// No error condition.
        /// </summary>
        NO_ERROR = 0,

        /// <summary>
        /// Format error - The name server was unable to interpret the query.
        /// </summary>
        FORMAT_ERRROR = 1,

        /// <summary>
        /// Server failure - The name server was unable to process this query due to a problem with the name server.
        /// </summary>
        SERVER_FAILURE = 2,

        /// <summary>
        /// Name Error - Meaningful only for responses from an authoritative name server, this code signifies that the
        /// domain name referenced in the query does not exist.
        /// </summary>
        NAME_ERROR = 3,

        /// <summary>
        /// Not Implemented - The name server does not support the requested kind of query.
        /// </summary>
        NOT_IMPLEMENTED = 4,

        /// <summary>
        /// Refused - The name server refuses to perform the specified operation for policy reasons.
        /// </summary>
        REFUSED = 5,
    }
}