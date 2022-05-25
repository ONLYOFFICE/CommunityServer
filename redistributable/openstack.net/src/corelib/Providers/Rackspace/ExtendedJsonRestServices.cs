namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using JSIStudios.SimpleRESTServices.Client;
    using JSIStudios.SimpleRESTServices.Client.Json;

    /// <summary>
    /// This class extends <see cref="JsonRestServices"/> to add support for handling response
    /// codes other than <see cref="HttpStatusCode.Continue"/> in response to an
    /// <c>Expect: 100-Continue</c>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class ExtendedJsonRestServices : JsonRestServices
    {
        /// <inheritdoc/>
        /// <remarks>
        /// This method expands on the base implementation by avoiding writing to the request stream
        /// if <see cref="HttpWebRequest.HaveResponse"/> is <see langword="true"/>, which prevents
        /// the requirement that data be sent in the case where sending the <c>Expect: 100-Continue</c>
        /// header resulted in an immediate error response with first send a
        /// <see cref="HttpStatusCode.Continue"/>.
        /// </remarks>
        public override Response Stream(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, Stream content, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            if (content == null)
                throw new ArgumentNullException("content");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");
            if (maxReadLength < 0)
                throw new ArgumentOutOfRangeException("maxReadLength");

            return ExecuteRequest(url, method, responseBuilderCallback, headers, queryStringParameters, settings, (req) =>
            {
                long bytesWritten = 0;

                if (settings.ChunkRequest || maxReadLength > 0)
                {
                    req.SendChunked = settings.ChunkRequest;
                    req.AllowWriteStreamBuffering = false;
                    req.ContentLength = maxReadLength > 0 && content.Length > maxReadLength ? maxReadLength : content.Length;
                }

                using (Stream stream = req.GetRequestStream())
                {
                    var buffer = new byte[bufferSize];
                    int count;
                    while (!req.HaveResponse && (count = content.Read(buffer, 0, maxReadLength > 0 ? (int)Math.Min(bufferSize, maxReadLength - bytesWritten) : bufferSize)) > 0)
                    {
                        bytesWritten += count;
                        stream.Write(buffer, 0, count);

                        if (progressUpdated != null)
                            progressUpdated(bytesWritten);

                        if (maxReadLength > 0 && bytesWritten >= maxReadLength)
                            break;
                    }
                }

                return "[STREAM CONTENT]";
            });
        }
    }
}
