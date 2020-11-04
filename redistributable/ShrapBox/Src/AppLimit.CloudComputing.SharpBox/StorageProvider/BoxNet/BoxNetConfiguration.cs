using System;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet
{
    /// <summary>
    /// The specific configuration for box.net
    /// </summary>
    public class BoxNetConfiguration : WebDavConfiguration
    {
        /// <summary>
        /// ctor
        /// </summary>
        public BoxNetConfiguration()
            : base(new Uri("https://dav.box.com/dav"))
        {
        }

        /// <summary>
        /// This method returns a standard configuration for 1and1 
        /// </summary>
        /// <returns></returns>
        public static BoxNetConfiguration GetBoxNetConfiguration()
        {
            return new BoxNetConfiguration
                {
                    Limits = new CloudStorageLimits(2L*1024L*1024L*1024L, -1),
                    TrustUnsecureSSLConnections = true
                };
        }
    }
}