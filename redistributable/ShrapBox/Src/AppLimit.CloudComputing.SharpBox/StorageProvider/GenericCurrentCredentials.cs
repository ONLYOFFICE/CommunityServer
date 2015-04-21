using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
#if MONODROID || WINDOWS_PHONE
            throw new Exception("CredentialCache.DefaultNetworkCredentials not supported on windows phone 7, please use a derived class with specifci override!");
#else
            return CredentialCache.DefaultNetworkCredentials;
#endif
        }

        #endregion     
    }
}
