// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.Graph;

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The type ThumbnailRequestBuilder.
    /// </summary>
    public partial class ThumbnailRequestBuilder : BaseRequestBuilder, IThumbnailRequestBuilder
    {

        /// <summary>
        /// Constructs a new ThumbnailRequestBuilder.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        public ThumbnailRequestBuilder(
            string requestUrl,
            IBaseClient client)
            : base(requestUrl, client)
        {
        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <returns>The built request.</returns>
        public IThumbnailRequest Request()
        {
            return this.Request(null);
        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>The built request.</returns>
        public IThumbnailRequest Request(IEnumerable<Option> options)
        {
            return new ThumbnailRequest(this.RequestUrl, this.Client, options);
        }
    
        /// <summary>
        /// Gets the request builder for Content.
        /// </summary>
        /// <returns>The <see cref="IThumbnailContentRequestBuilder"/>.</returns>
        public IThumbnailContentRequestBuilder Content
        {
            get
            {
                return new ThumbnailContentRequestBuilder(this.AppendSegmentToRequestUrl("content"), this.Client);
            }
        }
    
    }
}
