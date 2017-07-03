using System;

namespace ASC.SingleSignOn.Common
{
    [Serializable]
    public class SaveSettingsResult
    {
        public string Status { get; set; }
        public string PublicKey { get; set; }
    }
}
