using System;
using System.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth20;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Authorization
{
    /// <summary>
    /// Functions for requesting, refresh and maintain the access tokens for SkyDrive
    /// </summary>
    public static class SkyDriveAuthorizationHelper
    {
        /// <summary>
        /// Build uri string to obtain the authorization code
        /// </summary>
        /// <param name="clientID">ID of your app</param>
        /// <returns>Uri string</returns>
        public static String BuildAuthCodeUrl(String clientID)
        {
            return BuildAuthCodeUrl(clientID, null);
        }

        /// <summary>
        /// Build uri string to obtain the authorization code
        /// </summary>
        /// <param name="clientID">ID of your app</param>
        /// <param name="scopes">Scopes to which you want to get the access</param>
        /// <returns>Uri string</returns>
        public static String BuildAuthCodeUrl(String clientID, String scopes)
        {
            return BuildAuthCodeUrl(clientID, scopes, null);
        }

        /// <summary>
        /// Build uri string to obtain the authorization code
        /// </summary>
        /// <param name="clientID">ID of your app</param>
        /// <param name="scopes">Scopes to which you want to get the access</param>
        /// <param name="redirectUri">Page to which you will be redirect with the authorization obtained code</param>
        /// <returns>Uri string</returns>
        public static String BuildAuthCodeUrl(String clientID, String scopes, String redirectUri)
        {
            if (String.IsNullOrEmpty(clientID))
                throw new ArgumentNullException("clientID");

            if (String.IsNullOrEmpty(scopes))
                scopes = SkyDriveConstants.DefaultScopes;

            if (String.IsNullOrEmpty(redirectUri))
                redirectUri = SkyDriveConstants.DefaultRedirectUri;

            return String.Format("{0}?client_id={1}&scope={2}&response_type=code&redirect_uri={3}",
                                 SkyDriveConstants.OAuth20AuthUrl, HttpUtility.UrlEncode(clientID),
                                 HttpUtility.UrlEncode(scopes), redirectUri);
        }

        /// <summary>
        /// Exchange your authorization code for a valid access token
        /// </summary>
        /// <param name="clientID">ID of your app</param>
        /// <param name="clientSecret">Secret of your app</param>
        /// <param name="authCode">Authorization code</param>
        /// <returns>Access token</returns>
        public static ICloudStorageAccessToken GetAccessToken(String clientID, String clientSecret, String authCode)
        {
            return GetAccessToken(clientID, clientSecret, null, authCode);
        }

        /// <summary>
        /// Exchange your authorization code for a valid access token
        /// </summary>
        /// <param name="clientID">ID of your app</param>
        /// <param name="clientSecret">Secret of your app</param>
        /// <param name="redirectUri">Redirect uri you used to obtained authorization code. Serves as request validator</param>
        /// <param name="authCode">Authorization code</param>
        /// <returns>Access token</returns>
        public static ICloudStorageAccessToken GetAccessToken(String clientID, String clientSecret, String redirectUri, String authCode)
        {
            if (String.IsNullOrEmpty(clientID)) throw new ArgumentNullException("clientID");
            if (String.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException("clientSecret");
            if (String.IsNullOrEmpty(authCode)) throw new ArgumentNullException("authCode");

            if (String.IsNullOrEmpty(redirectUri))
                redirectUri = SkyDriveConstants.DefaultRedirectUri;

            var query = String.Format("client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&grant_type=authorization_code",
                                      clientID, redirectUri, clientSecret, authCode);

            var json = SkyDriveRequestHelper.PerformRequest(SkyDriveConstants.OAuth20TokenUrl, "application/x-www-form-urlencoded", "POST", query, 2);
            if (json != null)
            {
                var token = OAuth20Token.FromJson(json);
                token.ClientID = clientID;
                token.ClientSecret = clientSecret;
                token.RedirectUri = redirectUri;
                token.Timestamp = DateTime.UtcNow;
                return token;
            }

            return null;
        }

        /// <summary>
        /// Refresh expired access token
        /// </summary>
        /// <param name="token">Access token</param>
        /// <returns>Refreshed access token</returns>
        public static ICloudStorageAccessToken RefreshToken(ICloudStorageAccessToken token)
        {
            var sdToken = token as OAuth20Token;
            if (sdToken == null || !CanRefresh(sdToken))
                throw new ArgumentException("Can not refresh given token", "token");

            var query = String.Format("client_id={0}&client_secret={1}&redirect_uri={2}&grant_type=refresh_token&refresh_token={3}",
                                      sdToken.ClientID, sdToken.ClientSecret, sdToken.RedirectUri, sdToken.RefreshToken);

            var json = SkyDriveRequestHelper.PerformRequest(SkyDriveConstants.OAuth20TokenUrl, "application/x-www-form-urlencoded", "POST", query, 2);
            if (json != null)
            {
                var refreshed = OAuth20Token.FromJson(json);
                refreshed.ClientID = sdToken.ClientID;
                refreshed.ClientSecret = sdToken.ClientSecret;
                refreshed.RedirectUri = sdToken.RedirectUri;
                refreshed.Timestamp = DateTime.UtcNow;
                return refreshed;
            }

            return token;
        }

        private static bool CanRefresh(OAuth20Token token)
        {
            return !String.IsNullOrEmpty(token.ClientID) && !String.IsNullOrEmpty(token.ClientSecret) && !String.IsNullOrEmpty(token.RedirectUri);
        }
    }
}