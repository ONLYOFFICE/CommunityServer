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


namespace ASC.Mail.Enums
{
    /// <summary>
    /// Mechanisms available for authentication.
    /// </summary>
    public enum SaslMechanism
    {
        /// <summary>
        /// Authentication doesn't needed
        /// </summary>
        None = 0,
        /// <summary>
        /// The LOGIN mechanism (BASE64 encoded exchanges).
        /// </summary>
        Login = 1,
        /*/// <summary>
        /// The PLAIN mechanism (identity<NUL>username<NUL>password).
        /// </summary>
        Plain = 2,*/
        /*/// <summary>
        /// The DIGEST-MD5 mechanism. [RFC2831]
        /// </summary>
        DigestMd5 = 3,*/
        /// <summary>
        /// The CRAM-MD5 mechanism. [RFC2195]
        /// </summary>
        CramMd5 = 4,
        /// <summary>
        /// The OAuth 2.0 mechanism. [draft-ietf-oauth-v2-31]
        /// </summary>
        OAuth2 = 5
        /*/// <summary>
        /// The KERBEROS_V4 mechanism. [RFC2222]
        /// </summary>
        KerberosV4 = 6*/
    }
}
