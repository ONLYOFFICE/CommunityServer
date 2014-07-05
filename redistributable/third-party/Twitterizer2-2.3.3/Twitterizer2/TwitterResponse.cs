using System;

namespace Twitterizer
{
    /// <summary>
    /// The twitter response class provides details of the response from an api call to the twitter api.
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterResponse<T>
        where T : Core.ITwitterObject
    {
        /// <summary>
        /// Gets or sets the object that represents the data returned by the request to Twitter.
        /// </summary>
        /// <value>The response object.</value>
        public T ResponseObject { get; set; }

        /// <summary>
        /// Gets or sets the result of the request.
        /// </summary>
        /// <value>The result.</value>
        public RequestResult Result { get; set; }

        /// <summary>
        /// Gets or sets the request URL.
        /// </summary>
        /// <value>The request URL.</value>
        public string RequestUrl { get; set; }

        /// <summary>
        /// Gets the raw json or xml response provided by Twitter.
        /// </summary>
        /// <value>The response body.</value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the error message returned by the Twitter.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the oauth tokens provided for the request.
        /// </summary>
        /// <value>The tokens.</value>
        internal OAuthTokens Tokens { get; set; }

        /// <summary>
        /// Gets or sets the rate limiting details.
        /// </summary>
        /// <value>The rate limiting object.</value>
        public RateLimiting RateLimiting { get; set; }

        /// <summary>
        /// Gets or sets the OAuth Token Access Level details.
        /// </summary>
        /// <value>The access level.</value>
        public AccessLevel AccessLevel { get; set; }
    }
}
