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


using System;
using ASC.Common.Logging;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Clients
{
    /// <summary>Authenticated event arguments.</summary>
    /// <remarks>
    /// Some servers, such as GMail IMAP, will send some free-form text in
    /// the response to a successful login.
    /// </remarks>
    public class MailClientEventArgs : EventArgs
    {
        /// <summary>Get the free-form text sent by the server.</summary>
        /// <remarks>Gets the free-form text sent by the server.</remarks>
        /// <value>The free-form text sent by the server.</value>
        public string Message { get; private set; }

        public MailBoxData Mailbox { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MailClientEventArgs" /> class.
        /// </summary>
        /// <remarks>
        /// Creates a new <see cref="T:MailKit.AuthenticatedEventArgs" />.
        /// </remarks>
        /// <param name="message">The free-form text.</param>
        /// <param name="mailBoxData"></param>
        public MailClientEventArgs(string message, MailBoxData mailBoxData)
        {
            Message = message;
            Mailbox = mailBoxData;
        }
    }

    public class MailClientMessageEventArgs : EventArgs
    {
        public MimeKit.MimeMessage Message { get; private set; }

        public MailBoxData Mailbox { get; private set; }

        public bool Unread { get; private set; }

        public MailFolder Folder { get; private set; }

        public string MessageUid { get; private set; }

        public ILog Logger { get; private set; }

        public MailClientMessageEventArgs(MimeKit.MimeMessage message, string messageUid, bool uread, MailFolder folder,
            MailBoxData mailBoxData, ILog logger)
        {
            Message = message;
            Unread = uread;
            Folder = folder;
            Mailbox = mailBoxData;
            MessageUid = messageUid;
            Logger = logger;
        }
    }
}
