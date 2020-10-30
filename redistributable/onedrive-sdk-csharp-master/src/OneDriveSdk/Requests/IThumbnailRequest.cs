// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Graph;

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The interface IThumbnailRequest.
    /// </summary>
    public partial interface IThumbnailRequest : IBaseRequest
    {
        /// <summary>
        /// Creates the specified Thumbnail using PUT.
        /// </summary>
        /// <param name="thumbnailToCreate">The Thumbnail to create.</param>
        /// <returns>The created Thumbnail.</returns>
        Task<Thumbnail> CreateAsync(Thumbnail thumbnailToCreate);        /// <summary>
        /// Creates the specified Thumbnail using PUT.
        /// </summary>
        /// <param name="thumbnailToCreate">The Thumbnail to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created Thumbnail.</returns>
        Task<Thumbnail> CreateAsync(Thumbnail thumbnailToCreate, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the specified Thumbnail.
        /// </summary>
        /// <returns>The task to await.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Deletes the specified Thumbnail.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The task to await.</returns>
        Task DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified Thumbnail.
        /// </summary>
        /// <returns>The Thumbnail.</returns>
        Task<Thumbnail> GetAsync();

        /// <summary>
        /// Gets the specified Thumbnail.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The Thumbnail.</returns>
        Task<Thumbnail> GetAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Updates the specified Thumbnail using PATCH.
        /// </summary>
        /// <param name="thumbnailToUpdate">The Thumbnail to update.</param>
        /// <returns>The updated Thumbnail.</returns>
        Task<Thumbnail> UpdateAsync(Thumbnail thumbnailToUpdate);

        /// <summary>
        /// Updates the specified Thumbnail using PATCH.
        /// </summary>
        /// <param name="thumbnailToUpdate">The Thumbnail to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The updated Thumbnail.</returns>
        Task<Thumbnail> UpdateAsync(Thumbnail thumbnailToUpdate, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the specified expand value to the request.
        /// </summary>
        /// <param name="value">The expand value.</param>
        /// <returns>The request object to send.</returns>
        IThumbnailRequest Expand(string value);

        /// <summary>
        /// Adds the specified select value to the request.
        /// </summary>
        /// <param name="value">The select value.</param>
        /// <returns>The request object to send.</returns>
        IThumbnailRequest Select(string value);
    }
}
