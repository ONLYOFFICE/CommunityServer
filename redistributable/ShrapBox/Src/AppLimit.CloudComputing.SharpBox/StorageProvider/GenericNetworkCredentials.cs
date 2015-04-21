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
                String Domain = UserName.Split('\\')[0];
                String User = UserName.Split('\\')[1];

                return new NetworkCredential(User, Password, Domain);
            }
            else
                return new NetworkCredential(UserName, Password);
        }

        public override string ToString()
        {
            return string.Format("{0}+{1}",UserName,Password);
        }
    }
}
