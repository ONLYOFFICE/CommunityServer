using System;

namespace ASC.Mail.Aggregator.ComplexOperations.Base
{
    public class MailOperationAlreadyRunningException: Exception
    {
        public MailOperationAlreadyRunningException(string message)
            : base(message)
        {
        }
    }
}
