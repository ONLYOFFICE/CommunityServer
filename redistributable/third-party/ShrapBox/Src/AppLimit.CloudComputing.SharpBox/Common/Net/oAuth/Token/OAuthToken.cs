using System;

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token
{
    /// <summary>
    /// The OAuthToken will be generated during the login process in a oauth compliant 
    /// application and can be used to sign the url
    /// </summary>
    internal class OAuthToken
    {
        /// <summary>
        /// The key for uri signing
        /// </summary>
        public String TokenKey { get; protected set; }

        /// <summary>
        /// The secret for uri signing
        /// </summary>
        public String TokenSecret { get; protected set; }

        /// <summary>
        /// The token can only be generated from our API
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="tokenSecret"></param>
        internal OAuthToken(String tokenKey, String tokenSecret)
        {
            TokenKey = tokenKey;
            TokenSecret = tokenSecret;
        }
    }
}
