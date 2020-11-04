using System;
using System.Net;
using System.Text;
using System.Web;
using AppLimit.CloudComputing.SharpBox.Common.IO;

namespace AppLimit.CloudComputing.SharpBox.Common.Net
{
    /// <summary>
    /// This class exposes some extensions to the .NET HttpUtility class
    /// </summary>
    public class HttpUtilityEx
    {
        /// <summary>
        /// This method encodes an url
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string UrlEncodeUTF8(string text)
        {
            if (text == null)
                return null;

            if (text.Length == 0)
                return "";

            // encode with url encoder
            var enc = HttpUtility.UrlEncode(text, Encoding.UTF8);

            // fix the missing space
            enc = enc.Replace("+", "%20");

            // fix the exclamation point
            enc = enc.Replace("!", "%21");

            // fix the quote
            enc = enc.Replace("'", "%27");

            // fix the parentheses
            enc = enc.Replace("(", "%28");
            enc = enc.Replace(")", "%29");

            enc = enc.Replace("%2f", "/");

            // uppercase the encoded stuff            
            var enc2 = new StringBuilder();

            for (var i = 0; i < enc.Length; i++)
            {
                // copy char
                enc2.Append(enc[i]);

                // upper stuff
                if (enc[i] == '%')
                {
                    enc2.Append(Char.ToUpper(enc[i + 1]));
                    enc2.Append(Char.ToUpper(enc[i + 2]));

                    i += 2;
                }
            }

            return enc2.ToString();
        }

        /// <summary>
        /// This methid decodes a UTF8 encoded path
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PathDecodeUTF8(String text)
        {
            var output = String.Empty;

            if (text.StartsWith("/"))
                output = "/";

            var elements = text.Split('/');

            foreach (var s in elements)
            {
                if (s == String.Empty)
                    continue;

                if (!output.EndsWith("/"))
                    output += "/";

                // do the normal stuff
                output += HttpUtility.UrlDecode(s);
            }

            if (text.EndsWith("/"))
                output += "/";

            return output;
        }

        /// <summary>
        /// This method returns true if the give http error code is a success 
        /// error code, this means in 2XX
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Boolean IsSuccessCode(HttpStatusCode code)
        {
            return (((int)code >= 200 && (int)code < 300));
        }

        /// <summary>
        /// This method returns true if the give http error code is a success 
        /// error code, this means in 2XX
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Boolean IsSuccessCode(int code)
        {
            return IsSuccessCode((HttpStatusCode)code);
        }

        /// <summary>
        /// This method generates a well encoded uri string
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static String GenerateEncodedUriString(Uri uri)
        {
            // save the trailing /
            var bTrailingSlash = uri.ToString().EndsWith("/");

            // first part of string
            var uriString = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;

            for (var i = 0; i < uri.Segments.Length; i++)
            {
                var partString = uri.Segments[i];
                partString = partString.TrimEnd('/');

                uriString = PathHelper.Combine(uriString, partString);
            }

            if (bTrailingSlash)
                uriString += "/";

            return uriString;

        }

        /// <summary>
        /// Reduces the url to the root service url
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri GetPathAndQueryLessUri(Uri uri)
        {
            return new Uri(uri.Scheme + Uri.SchemeDelimiter + uri.DnsSafeHost + ":" + uri.Port);
        }
    }
}