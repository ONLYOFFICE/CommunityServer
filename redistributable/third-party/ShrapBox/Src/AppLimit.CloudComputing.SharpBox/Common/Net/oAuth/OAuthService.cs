using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Http;

#if SILVERLIGHT
using AppLimit.CloudComputing.oAuth.WP7.SilverLightHelper;
#endif

namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth
{
    internal class OAuthService : HttpService
    {
        #region Get Token Method

        private OAuthToken GetToken(String requestTokenUrl)
        {
            // get the token data 
            MemoryStream tokenData = PerformSimpleWebCall(requestTokenUrl, WebRequestMethodsEx.Http.Get, null, null);

            // generate the token as self                       
            return tokenData != null ? OAuthStreamParser.ParseTokenInformation(tokenData) : null;
        }

        public OAuthToken GetRequestToken(OAuthServiceContext svcContext, OAuthConsumerContext conContext)
        {
            String url = OAuthUrlGenerator.GenerateRequestTokenUrl(svcContext.RequestTokenUrl, conContext);

            return GetToken(url);
        }

        public OAuthToken GetAccessToken(OAuthServiceContext svcContext, OAuthConsumerContext conContext, OAuthToken requestToken)
        {
            String url = OAuthUrlGenerator.GenerateAccessTokenUrl(svcContext.AccessTokenUrl, conContext, requestToken);

            return GetToken(url);
        }

        #endregion

        #region Special oAuth WebRequest routines

        public virtual WebRequest CreateWebRequest(String url, String method, ICredentials credentials, Object context, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<String, String> parameter)
        {
            // generate the signed url
            String signedUrl = GetProtectedResourceUrl(url, conContext, accessToken, parameter, method);

            // generate the web request as self
            return CreateWebRequest(signedUrl, method, credentials, false, context);
        }

        #endregion
       
        #region Signed URL helpers
        
        public String GetProtectedResourceUrl(String resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<String, String> parameter, String webMethod)
        {
            // build url
            return OAuthUrlGenerator.GenerateSignedUrl(resourceUrl, webMethod, conContext, accessToken, parameter);            
        }

        public String GetSignedUrl(String resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<String, String> parameter)
        {
            return OAuthUrlGenerator.GenerateSignedUrl(resourceUrl, WebRequestMethodsEx.Http.Post, conContext, accessToken, parameter);
        }

        public String GetOAuthAuthorizationHeader(String resourceUrl, OAuthConsumerContext conContext, OAuthToken accessToken, Dictionary<String, String> parameter, String method)
        {
            var oAuth = new OAuthBase();

            String normalizedUrl;
            String normalizedRequestParameters;

            var timestamp = oAuth.GenerateTimeStamp();
            var nonce = oAuth.GenerateNonce();
            var signature = oAuth.GenerateSignature(new Uri(resourceUrl),
                                                    conContext.ConsumerKey, conContext.ConsumerSecret,
                                                    accessToken == null ? string.Empty : accessToken.TokenKey, accessToken == null ? string.Empty : accessToken.TokenSecret,
                                                    method, timestamp, nonce,
                                                    conContext.SignatureMethod,
                                                    out normalizedUrl, out normalizedRequestParameters);
            signature = HttpUtility.UrlEncode(signature);

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
