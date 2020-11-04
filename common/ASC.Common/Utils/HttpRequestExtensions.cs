/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;

namespace System.Web
{
    public static class HttpRequestExtensions
    {
        public static readonly string UrlRewriterHeader = "X-REWRITER-URL";


        public static Uri GetUrlRewriter(this HttpRequest request)
        {
            return request != null ? GetUrlRewriter(request.Headers, request.Url) : null;
        }

        public static Uri GetUrlRewriter(this HttpRequestBase request)
        {
            return request != null ? GetUrlRewriter(request.Headers, request.Url) : null;
        }

        public static Uri GetUrlRewriter(NameValueCollection headers, Uri requestUri)
        {
            if (requestUri == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(requestUri.Query))
            {
                var urlRewriterQuery = HttpUtility.ParseQueryString(requestUri.Query);
                var rewriterUri = ParseRewriterUrl(urlRewriterQuery[UrlRewriterHeader]);
                if (rewriterUri != null)
                {
                    var result = new UriBuilder(requestUri)
                    {
                        Scheme = rewriterUri.Scheme,
                        Host = rewriterUri.Host,
                        Port = rewriterUri.Port
                    };
                    return result.Uri;
                }
            }

            if (headers != null && !string.IsNullOrEmpty(headers[UrlRewriterHeader]))
            {
                var rewriterUri = ParseRewriterUrl(headers[UrlRewriterHeader]);
                if (rewriterUri != null)
                {
                    var result = new UriBuilder(requestUri)
                    {
                        Scheme = rewriterUri.Scheme,
                        Host = rewriterUri.Host,
                        Port = rewriterUri.Port
                    };
                    return result.Uri;
                }
            }

            return requestUri;
        }

        public static Uri PushRewritenUri(this HttpContext context)
        {
            return context != null ? PushRewritenUri(context, GetUrlRewriter(context.Request)) : null;
        }

        public static Uri PushRewritenUri(this HttpContext context, Uri rewrittenUri)
        {
            Uri oldUri = null;
            if (context != null)
            {
                var request = context.Request;

                if (request.Url != rewrittenUri)
                {
                    var requestUri = request.Url;
                    try
                    {
                        //Push it
                        request.ServerVariables.Set("HTTPS", rewrittenUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "on" : "off");
                        request.ServerVariables.Set("SERVER_NAME", rewrittenUri.Host);
                        request.ServerVariables.Set("SERVER_PORT",
                                                    rewrittenUri.Port.ToString(CultureInfo.InvariantCulture));

                        if (rewrittenUri.IsDefaultPort)
                        {
                            request.ServerVariables.Set("HTTP_HOST",
                                                        rewrittenUri.Host);
                        }
                        else
                        {
                            request.ServerVariables.Set("HTTP_HOST",
                                                        rewrittenUri.Host + ":" + requestUri.Port);
                        }
                        //Hack:
                        typeof (HttpRequest).InvokeMember("_url",
                                                          BindingFlags.NonPublic | BindingFlags.SetField |
                                                          BindingFlags.Instance,
                                                          null, HttpContext.Current.Request,
                                                          new object[] { null });
                        oldUri = requestUri;
                        context.Items["oldUri"] = oldUri;

                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return oldUri;
        }

        public static Uri PopRewritenUri(this HttpContext context)
        {
            if (context != null && context.Items["oldUri"] != null)
            {
                var rewriteTo = context.Items["oldUri"] as Uri;
                if (rewriteTo != null)
                {
                    return PushRewritenUri(context, rewriteTo);
                }
            }
            return null;
        }

        public static bool DesktopApp(this HttpRequest request)
        {
            return request != null && !string.IsNullOrEmpty(request["desktop"]);
        }

        public static bool SailfishApp(this HttpRequest request)
        {
            return request != null
                   && (!string.IsNullOrEmpty(request["sailfish"])
                       || !string.IsNullOrEmpty(request.UserAgent) && request.UserAgent.Contains("SailfishOS"));
        }


        private static Uri ParseRewriterUrl(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            const StringComparison cmp = StringComparison.OrdinalIgnoreCase;
            if (0 < s.Length && (s.StartsWith("0", cmp)))
            {
                s = Uri.UriSchemeHttp + s.Substring(1);
            }
            else if (3 < s.Length && s.StartsWith("OFF", cmp))
            {
                s = Uri.UriSchemeHttp + s.Substring(3);
            }
            else if (0 < s.Length && (s.StartsWith("1", cmp)))
            {
                s = Uri.UriSchemeHttps + s.Substring(1);
            }
            else if (2 < s.Length && s.StartsWith("ON", cmp))
            {
                s = Uri.UriSchemeHttps + s.Substring(2);
            }
            else if (s.StartsWith(Uri.UriSchemeHttp + "%3A%2F%2F", cmp) || s.StartsWith(Uri.UriSchemeHttps + "%3A%2F%2F", cmp))
            {
                s = HttpUtility.UrlDecode(s);
            }

            Uri result;
            Uri.TryCreate(s, UriKind.Absolute, out result);
            return result;
        }
    }
}