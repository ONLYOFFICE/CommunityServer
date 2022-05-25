using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Content;

// ReSharper disable once CheckNamespace
namespace Flurl.Http
{
    /// <summary>
    /// Represents a prepared Flurl request which can be executed at a later time.
    /// </summary>
    /// <exclude />
    public class PreparedRequest : FlurlRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedRequest"/> class.
        /// </summary>
        public PreparedRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedRequest"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        public PreparedRequest(string url) : base(url)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedRequest"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        public PreparedRequest(Url url) : base(url)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedRequest"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="autoDispose">Specifies if the request should be automatically disposed.</param>
        public PreparedRequest(string url, bool autoDispose) : base(url)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedRequest"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="autoDispose">Specifies if the request should be automatically disposed.</param>
        public PreparedRequest(Url url, bool autoDispose) : base(url)
        {
        }

        /// <summary>
        /// The HTTP verb which will be used in the request.
        /// </summary>
        public HttpMethod Verb { get; protected set; }

        /// <summary>
        /// The HTTP content which will be used in the request.
        /// </summary>
        public HttpContent Content { get; protected set; }

        /// <summary>
        /// The optional canellation token which will be used in the request, defaults to None.
        /// </summary>
        public CancellationToken CancellationToken { get; protected set; }

        /// <summary>
        /// Prepares the client to send a DELETE request
        /// </summary>
        public PreparedRequest PrepareDelete(CancellationToken cancellationToken = default(CancellationToken))
        {
            Verb = HttpMethod.Delete;
            CancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Prepares the client to send a GET request
        /// </summary>
        public PreparedRequest PrepareGet(CancellationToken cancellationToken = default(CancellationToken))
        {
            Verb = HttpMethod.Get;
            CancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Prepares the client to send a PATCH request containing json
        /// </summary>
        public PreparedRequest PreparePatchJson(object data, CancellationToken cancellationToken = default(CancellationToken))
        {
            Verb = new HttpMethod("PATCH");
            Content = new CapturedJsonContent(Settings.JsonSerializer.Serialize(data));
            CancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Prepares the client to send a POST request containing json
        /// </summary>
        public PreparedRequest PreparePostJson(object data, CancellationToken cancellationToken = default(CancellationToken))
        {
            Verb = HttpMethod.Post;
            Content = new CapturedJsonContent(Settings.JsonSerializer.Serialize(data));
            CancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Prepares the client to send a PUT request containing json
        /// </summary>
        public PreparedRequest PreparePutJson(object data, CancellationToken cancellationToken = default(CancellationToken))
        {
            Verb = HttpMethod.Put;
            Content = new CapturedJsonContent(Settings.JsonSerializer.Serialize(data));
            CancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Executes the built request
        /// </summary>
        public Task<IFlurlResponse> SendAsync()
        {
            if(Verb == null)
                throw new InvalidOperationException("Unable to execute request as nothing has been built yet.");

            return SendAsync(Verb, Content, CancellationToken);
        }
    }

    /// <summary />
    public static class PreparedRequestExtensions
    {
        /// <summary>
        /// Allow a specific set of HTTP status codes.
        /// </summary>
        /// <param name="request">The prepared request.</param>
        /// <param name="statusCodes">The allowed status codes.</param>
        /// <returns></returns>
        public static PreparedRequest AllowHttpStatus(this PreparedRequest request, params HttpStatusCode[] statusCodes)
        {
            return (PreparedRequest)((FlurlRequest)request).AllowHttpStatus(statusCodes);
        }
    }
}