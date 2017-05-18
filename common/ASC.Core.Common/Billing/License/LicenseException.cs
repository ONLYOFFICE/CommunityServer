using System;

namespace ASC.Core.Billing
{
    [Serializable]
    public class LicenseException : BillingException
    {
        public LicenseException(string message, object debugInfo = null) : base(message, debugInfo)
        {
        }
    }

    [Serializable]
    public class LicenseExpiredException : LicenseException
    {
        public LicenseExpiredException(string message, object debugInfo = null) : base(message, debugInfo)
        {
        }
    }

    [Serializable]
    public class LicenseQuotaException : LicenseException
    {
        public LicenseQuotaException(string message, object debugInfo = null) : base(message, debugInfo)
        {
        }
    }

    [Serializable]
    public class LicensePortalException : LicenseException
    {
        public LicensePortalException(string message, object debugInfo = null) : base(message, debugInfo)
        {
        }
    }
}