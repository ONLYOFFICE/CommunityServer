/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
    /// This class provides well known TCP/UDP service ports.
    /// </summary>
    public class WellKnownPorts
    {
        #region Members

        /// <summary>
        /// DNS protocol.
        /// </summary>
        public static readonly int DNS = 53;

        /// <summary>
        /// FTP - control (command) port.
        /// </summary>
        public static readonly int FTP_Control = 21;

        /// <summary>
        /// FTP over SSL protocol.
        /// </summary>
        public static readonly int FTP_Control_SSL = 990;

        /// <summary>
        /// FTP - data port.
        /// </summary>
        public static readonly int FTP_Data = 20;

        /// <summary>
        /// HTTP protocol.
        /// </summary>
        public static readonly int HTTP = 80;

        /// <summary>
        /// HTTPS protocol.
        /// </summary>
        public static readonly int HTTPS = 443;

        /// <summary>
        /// IMAP4 protocol.
        /// </summary>
        public static readonly int IMAP4 = 143;

        /// <summary>
        /// IMAP4 over SSL protocol.
        /// </summary>
        public static readonly int IMAP4_SSL = 993;

        /// <summary>
        /// NNTP (Network News Transfer Protocol)  protocol.
        /// </summary>
        public static readonly int NNTP = 119;

        /// <summary>
        /// NTP (Network Time Protocol) protocol.
        /// </summary>
        public static readonly int NTP = 123;

        /// <summary>
        /// POP3 protocol.
        /// </summary>
        public static readonly int POP3 = 110;

        /// <summary>
        /// POP3 over SSL protocol.
        /// </summary>
        public static readonly int POP3_SSL = 995;

        /// <summary>
        /// SMTP protocol.
        /// </summary>
        public static readonly int SMTP = 25;

        /// <summary>
        /// SMTP over SSL protocol.
        /// </summary>
        public static readonly int SMTP_SSL = 465;

        #endregion
    }
}