/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


namespace ASC.Mail.Net.SIP.Stack
{
    /// <summary>
    /// Specifies SIP status code type. Defined in rfc 3261.
    /// </summary>
    public enum SIP_StatusCodeType
    {
        /// <summary>
        /// Request received, continuing to process the request. 1xx status code.
        /// </summary>
        Provisional,

        /// <summary>
        /// Action was successfully received, understood, and accepted. 2xx status code.
        /// </summary>
        Success,

        /// <summary>
        /// Request must be redirected(forwarded). 3xx status code.
        /// </summary>
        Redirection,

        /// <summary>
        /// Request contains bad syntax or cannot be fulfilled at this server. 4xx status code.
        /// </summary>
        RequestFailure,

        /// <summary>
        /// Server failed to fulfill a valid request. 5xx status code.
        /// </summary>
        ServerFailure,

        /// <summary>
        /// Request cannot be fulfilled at any server. 6xx status code.
        /// </summary>
        GlobalFailure
    }
}