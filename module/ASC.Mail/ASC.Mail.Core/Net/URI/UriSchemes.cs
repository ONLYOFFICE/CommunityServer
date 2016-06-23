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