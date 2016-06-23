using System;

namespace ASC.Mail.Aggregator.Core.Clients
{
    public class LoginResult
    {
        public bool IngoingSuccess { get; set; }
        public Exception IngoingException { get; set; }

        public bool OutgoingSuccess { get; set; }
        public Exception OutgoingException { get; set; }

        public bool Imap { get; set; }
    }
}
