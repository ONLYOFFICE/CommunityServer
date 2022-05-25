using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Http;


namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth
{
    internal class OAuthService : HttpService
    {
        #region Get Token Method

        private OAuthToken GetToken(string requestTokenUrl)
        {
            // get the token data 
            var tokenData = PerformSimpleWebCall(requestTokenUrl, WebRequestMethodsEx.Http.Get, null, null);

            // generate the token as self                       
            return tokenData != null ? OAuthStreamParser.ParseTokenInformation(tokenData) : null;
        }

        public OAuthToken GetRequestToken(OAuthServiceContext svcContext, OAuthConsumerContext conContext)
        {
            var url = OAuthUrlGenerator.GenerateRequestTokenUrl(svcContext.RequestTokenUrl, conContext);

            return GetToken(url);
        }

        public OAuthToken GetAccessToken(OAuthServiceContext svcContext, OAuthConsumerContext conContext, OAuthToken requestToken)
        {
            var url = OAuthUrlGenerator.GenerateAccessTokenUrl(svcContext.AccessTokenUrl, conContext, requestToken);

            return GetToken(url);
        }

        #endregion

        #region Special oAuth WebRequest routines

        public virtual WebRequest CreateWebRequest(string url, string method, ICredentials credentials, object context, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<string, string> parameter)
        {
            // generate the signed url
            var signedUrl = GetProtectedResourceUrl(url, conContext, accessToken, parameter, method);

            // generate the web request as self
            return CreateWebRequest(signedUrl, method, credentials, false, context);
        }

        #endregion

        #region Signed URL helpers

        public string GetProtectedResourceUrl(string resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<string, string> parameter, string webMethod)
        {
            // build url
            return OAuthUrlGenerator.GenerateSignedUrl(resourceUrl, webMethod, conContext, accessToken, parameter);
        }

        public string GetSignedUrl(string resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<string, string> parameter)
        {
            return OAuthUrlGenerator.GenerateSignedUrl(resourceUrl, WebRequestMethodsEx.Http.Post, conContext, accessToken, parameter);
        }

        public string GetOAuthAuthorizationHeader(string resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<string, string> parameter, string method)
        {
            var oAuth = new OAuthBase();

            string normalizedUrl;
            string normalizedRequestParameters;

            var timestamp = oAuth.GenerateTimeStamp();
            var nonce = oAuth.GenerateNonce();
            var signature = oAuth.GenerateSignature(new Uri(resourceUrl),
                                                    conContext.ConsumerKey, conContext.ConsumerSecret,
                                                    accessToken == null ? string.Empty : accessToken.TokenKey, accessToken == null ? string.Empty : accessToken.TokenSecret,
                                                    method, timestamp, nonce,
                                                    conContext.SignatureMethod,
                                                    out normalizedUrl, out normalizedRequestParameters);
            signature = System.Web.HttpUtility.UrlEncode(signature);

            var sb = new StringBuilder("OAuth");
            sb.Append(", oauth_version=1.0");
            sb.AppendFormat(", oauth_nonce={0}", nonce);
            sb.AppendFormat(", oauth_timestamp={0}", timestamp);
            sb.AppendFormat(", oauth_consumer_key={0}", conContext.ConsumerKey);
            if (accessToken != null)
            {
                sb.AppendFormat(", , oauth_token={0}", accessToken.TokenKey);
            }
            sb.Append(", oauth_signature_method=\"HMAC-SHA1\"");
            sb.AppendFormat(", oauth_signature={0}", signature);

            return sb.ToString();
        }

        #endregion
    }
}