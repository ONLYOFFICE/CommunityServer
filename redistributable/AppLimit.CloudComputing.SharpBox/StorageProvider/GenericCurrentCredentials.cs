using System;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider
{
    /// <summary>
    /// This class contains the needed access credentials for a specific webdav 
    /// user
    /// </summary>
    public class GenericCurrentCredentials : ICloudStorageAccessToken, ICredentials
    {
        #region ICredentials Members

        /// <summary>
        /// Returns the network credentials which can be used in webrequest. This method
        /// uses the standard credentials of the logged on user
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authType"></param>
        /// <returns></returns>
        public virtual NetworkCredential GetCredential(Uri uri, string authType)
        {
            return CredentialCache.DefaultNetworkCredentials;
        }

        #endregion
    }
}