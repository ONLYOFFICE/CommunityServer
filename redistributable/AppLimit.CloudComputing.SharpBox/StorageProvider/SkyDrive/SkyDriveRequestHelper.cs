using System;
using System.IO;
using System.Net;
using System.Text;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth20;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Authorization;
using AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive
{
    internal static class SkyDriveRequestHelper
    {
        public static string SignUri(IStorageProviderSession session, string uri)
        {
            var token = GetValidToken(session);
            var signedUri = string.Format("{0}?access_token={1}", uri, token.AccessToken);
            return signedUri;
        }

        private static OAuth20Token GetValidToken(IStorageProviderSession session)
        {
            var token = session.SessionToken as OAuth20Token;
            if (token == null) throw new ArgumentException("Can not retrieve valid oAuth 2.0 token from given session", "session");
            if (token.IsExpired)
            {
                token = (OAuth20Token)SkyDriveAuthorizationHelper.RefreshToken(token);
                var sdSession = session as SkyDriveStorageProviderSession;
                if (sdSession != null)
                {
                    sdSession.SessionToken = token;
                }
            }
            return token;
        }

        public static string PerformRequest(IStorageProviderSession session, string uri)
        {
            return PerformRequest(session, uri, true);
        }

        public static string PerformRequest(IStorageProviderSession session, string uri, bool signUri)
        {
            return PerformRequest(session, uri, "GET", signUri);
        }

        public static string PerformRequest(IStorageProviderSession session, string uri, string method, bool signUri)
        {
            return PerformRequest(session, uri, method, null, signUri);
        }

        public static string PerformRequest(IStorageProviderSession session, string uri, string method, string data, bool signUri)
        {
            return PerformRequest(session, uri, method, data, signUri, 2);
        }

        public static string PerformRequest(IStorageProviderSession session, string uri, string method, string data, bool signUri, int countAttempts)
        {
            if (string.IsNullOrEmpty(method))
                method = "GET";

            if (!string.IsNullOrEmpty(data) && method == "GET")
                return null;

            if (signUri)
                uri = SignUri(session, uri);


            var attemptsToComplete = countAttempts;
            while (attemptsToComplete > 0)
            {
                var request = WebRequest.Create(uri);
                request.Method = method;
                request.Timeout = 5000;

                if (!signUri)
                    request.Headers.Add("Authorization", "Bearer " + GetValidToken(session).AccessToken);

                if (!string.IsNullOrEmpty(data))
                {
                    var bytes = Encoding.UTF8.GetBytes(data);
                    request.ContentType = "application/json";
                    request.ContentLength = bytes.Length;
                    using (var rs = request.GetRequestStream())
                    {
                        rs.Write(bytes, 0, bytes.Length);
                    }
                }

                try
                {
                    using (var response = request.GetResponse())
                    using (var rs = response.GetResponseStream())
                    {
                        if (rs != null)
                        {
                            using (var streamReader = new StreamReader(rs))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                    return null;
                }
                catch (WebException exception)
                {
                    attemptsToComplete--;

                    if (exception.Response != null && ((HttpWebResponse)exception.Response).StatusCode == HttpStatusCode.NotFound)
                        return null;
                }
            }
            return null;
        }

        public static string PerformRequest(string uri, string contentType, string method, string queryString, int countAttempts)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentException("Uri can't be empty string.", "uri");

            if (string.IsNullOrEmpty(method))
                method = "GET";

            byte[] bytes = null;
            if (!string.IsNullOrEmpty(queryString))
                bytes = Encoding.UTF8.GetBytes(queryString);

            var attemptsToTry = countAttempts;
            while (attemptsToTry > 0)
            {
                var request = WebRequest.Create(uri);
                request.Method = method;
                request.Timeout = 5000;
                if (request.Method != "GET" && !string.IsNullOrEmpty(contentType) && bytes != null)
                {
                    request.ContentType = contentType;
                    request.ContentLength = bytes.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                try
                {
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream != null)
                            using (var streamReader = new StreamReader(stream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        return null;
                    }
                }
                catch (WebException)
                {
                    attemptsToTry--;
                    request.Abort();
                }
            }
            return null;
        }
    }
}