using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if SILVERLIGHT || MONODROID
using System.Net;
#else
using System.Web;
using System.Net;
#endif

using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Context;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Token;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;      


namespace AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl
{
    internal class OAuthUrlGenerator
    {
        static public String GenerateRequestTokenUrl(String baseRequestTokenUrl, OAuthConsumerContext context)
        {
            return GenerateSignedUrl(baseRequestTokenUrl, WebRequestMethodsEx.Http.Get, context, null);            
        }

        static public String GenerateAuthorizationUrl(String baseAuthorizeUrl, String callbackUrl, OAuthToken requestToken)
        {
            var sb = new StringBuilder(baseAuthorizeUrl);

            sb.AppendFormat("?oauth_token={0}", requestToken.TokenKey);
            sb.AppendFormat("&oauth_callback={0}", OAuthBase.UrlEncode(callbackUrl, '.'));

            return sb.ToString();
        }

        static public String GenerateAccessTokenUrl(String baseAccessTokenUrl, OAuthConsumerContext context, OAuthToken requestToken)
        {
            return GenerateSignedUrl(baseAccessTokenUrl, WebRequestMethodsEx.Http.Get, context, requestToken);            
        }

        static private String GenerateSignedUrl(String baseRequestTokenUrl, String method, OAuthConsumerContext context, OAuthToken token)
        {
            String normalizedUrl;
            String normalizedRequestParameters;

            var oAuth = new OAuthBase();

            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string sig = oAuth.GenerateSignature(   new Uri(baseRequestTokenUrl),
                                                    context.ConsumerKey, context.ConsumerSecret,
                                                    token == null ? string.Empty : token.TokenKey, token == null ? string.Empty : token.TokenSecret,
                                                    method.ToString(), timeStamp, nonce,
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

        static public String GenerateSignedUrl(String baseResourceUrl, String method, OAuthConsumerContext context, OAuthToken token, Dictionary<String, String> urlParameter)
        {            
            var sb = new StringBuilder(baseResourceUrl);

            if (urlParameter != null)
            {
                sb.Append('?');

                foreach (KeyValuePair<String, String> kvp in urlParameter)
                {   
                    if ( sb[sb.Length - 1] == '?' )
                        sb.AppendFormat("{0}={1}", kvp.Key, OAuthBase.UrlEncode(kvp.Value));
                    else
                        sb.AppendFormat("&{0}={1}", kvp.Key, OAuthBase.UrlEncode(kvp.Value));
                }
            }                        

            return GenerateSignedUrl(sb.ToString(), method, context, token);
        }

        static private String GetSignatureTypeString( SignatureTypes type )
        {
            switch(type)
            {
                case SignatureTypes.HMACSHA1:
                    return "HMACSHA1";
                case SignatureTypes.PLAINTEXT:
                    return "PLAINTEXT";
                case SignatureTypes.RSASHA1:
                    return "RSASHA1";
                default:
                    return "";
            }
        }
    }
}
