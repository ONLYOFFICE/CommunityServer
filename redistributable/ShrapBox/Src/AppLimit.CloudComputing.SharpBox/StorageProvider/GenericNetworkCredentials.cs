using System;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider
{
    /// <summary>
    /// This class contains the needed access credentials for a specific webdav 
    /// user
    /// </summary>
    public class GenericNetworkCredentials : GenericCurrentCredentials
    {
        /// <summary>
        /// Useraccount of the end user with access to the WebDav share
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// Password of the end user with access to the WebDav share
        /// </summary>
        public String Password { get; set; }

        /// <summary>
        /// returns network credentials which are usable in a webrequest
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authType"></param>
        /// <returns></returns>
        public override NetworkCredential GetCredential(Uri uri, string authType)
        {
            if (UserName.Contains("\\"))
            {
                var domain = UserName.Split('\\')[0];
                var user = UserName.Split('\\')[1];

                return new NetworkCredential(user, Password, domain);
            }
            return new NetworkCredential(UserName, Password);
        }

        public override string ToString()
        {
            return string.Format("{0}+{1}", UserName, Password);
        }
    }
}