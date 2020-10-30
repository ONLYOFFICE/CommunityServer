// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.Graph;

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The interface IThumbnailRequestBuilder.
    /// </summary>
    public partial interface IThumbnailRequestBuilder : IBaseRequestBuilder
    {
        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <returns>The built request.</returns>
        IThumbnailRequest Request();

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>The built request.</returns>
        IThumbnailRequest Request(IEnumerable<Option> options);
    
        /// <summary>
        /// Gets the request builder for Content.
        /// </summary>
        /// <returns>The <see cref="IThumbnailContentRequestBuilder"/>.</returns>
        IThumbnailContentRequestBuilder Content { get; }
    
    }
}
