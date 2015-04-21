using System;
using System.Text;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.GoogleDocs
{
    public static class GoogleDocsAuthorizationHelper
    {
        public static GoogleDocsRequestToken GetGoogleDocsRequestToken(GoogleDocsConfiguration configuration, String consumerKey, String consumerSecret)
        {
            var consumerContext = new OAuthConsumerContext(consumerKey, consumerSecret);
            var serviceContext = new OAuthServiceContext(GetRequestTokenUrl(configuration),
                                                         configuration.OAuthAuthorizeTokenUrl.ToString(),
                                                         configuration.AuthorizationCallBack.ToString(),
                                                         GetAccessTokenUrl(configuration, null));
            var service = new OAuthService();
            var token = service.GetRequestToken(serviceContext, consumerContext);
            return token != null ? new GoogleDocsRequestToken(token) : null;
        }

        public static String GetGoogleDocsAuthorizationUrl(GoogleDocsConfiguration configuration, GoogleDocsRequestToken token)
        {
            return OAuthUrlGenerator.GenerateAuthorizationUrl(configuration.OAuthAuthorizeTokenUrl.ToString(),
                                                              configuration.AuthorizationCallBack.ToString(),
                                                              token.RealToken);
        }

        public static ICloudStorageAccessToken ExchangeGoogleDocsRequestTokenIntoAccessToken(GoogleDocsConfiguration configuration, String consumerKey, String consumerSecret, GoogleDocsRequestToken requestToken, String oAuthVerifier)
        {
            var consumerContext = new OAuthConsumerContext(consumerKey, consumerSecret);
            var serviceContext = new OAuthServiceContext(configuration.OAuthGetRequestTokenUrl.ToString(),
                                                         configuration.OAuthAuthorizeTokenUrl.ToString(),
                                                         configuration.AuthorizationCallBack.ToString(),
                                                         GetAccessTokenUrl(configuration, oAuthVerifier));
            var service = new OAuthService();
            var accessToken = service.GetAccessToken(serviceContext, consumerContext, requestToken.RealToken);
            if (accessToken == null) throw new UnauthorizedAccessException();
            return new GoogleDocsToken(accessToken, consumerKey, consumerSecret);
        }

        public static ICloudStorageAccessToken BuildToken(String tokenKey, String tokenSecret, String consumerKey, String consumerSecret)
        {
            return new GoogleDocsToken(OAuthBase.UrlEncode(tokenKey), OAuthBase.UrlEncode(tokenSecret), consumerKey, consumerSecret);
        }

        private static String GetAccessTokenUrl(GoogleDocsConfiguration configuration, String oAuthVerifier)
        {
            String accessTokenUrl = configuration.OAuthGetAccessTokenUrl.ToString();

            if (!String.IsNullOrEmpty(oAuthVerifier))
            {
                accessTokenUrl = String.Format("{0}?oauth_verifier={1}", accessTokenUrl, oAuthVerifier);
            }

            return accessTokenUrl;
        }

        private static String GetRequestTokenUrl(GoogleDocsConfiguration configuration)
        {
            String requestTokenUrl = configuration.OAuthGetRequestTokenUrl.ToString();

            if (!String.IsNullOrEmpty(configuration.AccessUrlScope))
            {
                var sb = new StringBuilder(requestTokenUrl);
                sb.AppendFormat("?scope={0}", OAuthBase.UrlEncode(configuration.AccessUrlScope, '.'));
                sb.AppendFormat("&oauth_callback={0}", OAuthBase.UrlEncode(configuration.AuthorizationCallBack.ToString(), '.'));
                requestTokenUrl = sb.ToString();
            }

            return requestTokenUrl;
        }
    }
}