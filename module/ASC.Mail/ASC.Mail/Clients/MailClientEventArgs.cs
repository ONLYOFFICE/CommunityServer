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
