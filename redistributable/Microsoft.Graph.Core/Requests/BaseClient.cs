// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;
    using Microsoft.Graph.Core.Requests;

    /// <summary>
    /// A default <see cref="IBaseClient"/> implementation.
    /// </summary>
    public class BaseClient : IBaseClient
    {
        private string baseUrl;
        
        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="baseUrl">The base service URL. For example, "https://graph.microsoft.com/v1.0."</param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating request messages.</param>
        /// <param name="httpProvider">The <see cref="IHttpProvider"/> for sending requests.</param>
        public BaseClient(
            string baseUrl,
            IAuthenticationProvider authenticationProvider,
            IHttpProvider httpProvider = null)
        {
            this.BaseUrl = baseUrl;
            this.AuthenticationProvider = authenticationProvider;
            this.HttpProvider = httpProvider ?? new HttpProvider(new Serializer());
        }

        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="baseUrl">The base service URL. For example, "https://graph.microsoft.com/v1.0."</param>
        /// <param name="httpClient">The custom <see cref="HttpClient"/> to be used for making requests</param>
        public BaseClient(
            string baseUrl,
            HttpClient httpClient)
        {
            this.BaseUrl = baseUrl;
            this.HttpProvider = new SimpleHttpProvider(httpClient);
        }

        /// <summary>
        /// Gets the <see cref="IAuthenticationProvider"/> for authenticating requests.
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Gets or sets the base URL for requests of the client.
        /// </summary>
        public string BaseUrl
        {
            get { return this.baseUrl; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.InvalidRequest,
                            Message = ErrorConstants.Messages.BaseUrlMissing,
                        });
                }

                this.baseUrl = value.TrimEnd('/');
            }
        }

        /// <summary>
        /// Gets the <see cref="IHttpProvider"/> for sending HTTP requests.
        /// </summary>
        public IHttpProvider HttpProvider { get; private set; }

        /// <summary>
        /// Gets or Sets the <see cref="IAuthenticationProvider"/> for authenticating a single HTTP requests. 
        /// </summary>
        public Func<IAuthenticationProvider> PerRequestAuthProvider { get; set; }

        /// <summary>
        /// Gets the <see cref="IBatchRequestBuilder"/> for building batch Requests
        /// </summary>
        public IBatchRequestBuilder Batch
        {
            get
            {
                return new BatchRequestBuilder(this.BaseUrl + "/$batch", this);
            }
        }
    }
}
