using System;

namespace ASC.Mail.Exceptions
{
    public class LimitMessageException : Exception
    {
        public LimitMessageException(string message)
            : base(message) { }
    }
}
