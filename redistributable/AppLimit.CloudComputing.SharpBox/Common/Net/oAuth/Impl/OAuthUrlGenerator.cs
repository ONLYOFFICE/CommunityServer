using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;


namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl
{
    internal class OAuthUrlGenerator
    {
        public static string GenerateRequestTokenUrl(string baseRequestTokenUrl, OAuthConsumerContext context)
        {
            return GenerateSignedUrl(baseRequestTokenUrl, WebRequestMethodsEx.Http.Get, context, null);
        }

        public static string GenerateAuthorizationUrl(string baseAuthorizeUrl, string callbackUrl, OAuthToken requestToken)
        {
            var sb = new StringBuilder(baseAuthorizeUrl);

            sb.AppendFormat("?oauth_token={0}", requestToken.TokenKey);
            sb.AppendFormat("&oauth_callback={0}", OAuthBase.UrlEncode(callbackUrl, '.'));

            return sb.ToString();
        }

        public static string GenerateAccessTokenUrl(string baseAccessTokenUrl, OAuthConsumerContext context, OAuthToken requestToken)
        {
            return GenerateSignedUrl(baseAccessTokenUrl, WebRequestMethodsEx.Http.Get, context, requestToken);
        }

        private static string GenerateSignedUrl(string baseRequestTokenUrl, string method, OAuthConsumerContext context, OAuthToken token)
        {
            string normalizedUrl;
            string normalizedRequestParameters;

            var oAuth = new OAuthBase();

            var nonce = oAuth.GenerateNonce();
            var timeStamp = oAuth.GenerateTimeStamp();
            var sig = oAuth.GenerateSignature(new Uri(baseRequestTokenUrl),
                                              context.ConsumerKey, context.ConsumerSecret,
                                              token == null ? string.Empty : token.TokenKey, token == null ? string.Empty : token.TokenSecret,
                                              method, timeStamp, nonce,
                                              context.SignatureMethod,
                                              out normalizedUrl, out normalizedRequestParameters);

            //
            // The signature as self has to be encoded to be ensure that no not url compatibale characters 
            // will be used. The SHA1 hash can contain a bunch of this characters
            //
            sig = HttpUtility.UrlEncode(sig);

            var sb = new StringBuilder(normalizedUrl);

            sb.AppendFormat("?");
            sb.AppendFormat(normalizedRequestParameters);
            sb.AppendFormat("&oauth_signature={0}", sig);

            return sb.ToString();
        }

        public static string GenerateSignedUrl(string baseResourceUrl, string method, OAuthConsumerContext context, OAuthToken token, Dictionary<string, string> urlParameter)
        {
            var sb = new StringBuilder(baseResourceUrl);

            if (urlParameter != null)
            {
                sb.Append('?');

                foreach (var kvp in urlParameter)
                {
                    if (sb[sb.Length - 1] == '?')
                        sb.AppendFormat("{0}={1}", kvp.Key, OAuthBase.UrlEncode(kvp.Value));
                    else
                        sb.AppendFormat("&{0}={1}", kvp.Key, OAuthBase.UrlEncode(kvp.Value));
                }
            }

            return GenerateSignedUrl(sb.ToString(), method, context, token);
        }
    }
}