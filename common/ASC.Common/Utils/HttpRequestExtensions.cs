/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
            if (headers == null || headers.Count == 0 || requestUri == null)
            {
                return requestUri;
            }

            var rewriterUri = ParseRewriterUrl(headers[UrlRewriterHeader]);
            if (rewriterUri != null)
            {
                var result = new UriBuilder(requestUri);
                result.Scheme = rewriterUri.Scheme;
                result.Host = rewriterUri.Host;
                result.Port = rewriterUri.Port;
                return result.Uri;
            }
            else
            {
                return requestUri;
            }
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
                        typeof(HttpRequest).InvokeMember("_url",
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


        private static Uri ParseRewriterUrl(string s)
        {
            Uri result = null;
            var cmp = StringComparison.OrdinalIgnoreCase;

            if (string.IsNullOrEmpty(s))
            {
                return result;
            }
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

            Uri.TryCreate(s, UriKind.Absolute, out result);
            return result;
        }
    }
}
