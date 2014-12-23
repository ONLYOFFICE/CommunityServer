/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
