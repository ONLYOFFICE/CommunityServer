using System;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Aggregator.Common.Clients
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

        public MailBox Mailbox { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MailClientEventArgs" /> class.
        /// </summary>
        /// <remarks>
        /// Creates a new <see cref="T:MailKit.AuthenticatedEventArgs" />.
        /// </remarks>
        /// <param name="message">The free-form text.</param>
        /// <param name="mailBox"></param>
        public MailClientEventArgs(string message, MailBox mailBox)
        {
            this.Message = message;
            this.Mailbox = mailBox;
        }
    }

    public class MailClientMessageEventArgs : EventArgs
    {
        public MimeKit.MimeMessage Message { get; private set; }

        public MailBox Mailbox { get; private set; }

        public bool Unread { get; private set; }

        public MailFolder Folder { get; private set; }

        public string MessageUid { get; private set; }

        public ILogger Logger { get; private set; }

        public MailClientMessageEventArgs(MimeKit.MimeMessage message, string messageUid, bool uread, MailFolder folder,
            MailBox mailBox, ILogger logger)
        {
            Message = message;
            Unread = uread;
            Folder = folder;
            Mailbox = mailBox;
            MessageUid = messageUid;
            Logger = logger;
        }
    }
}
