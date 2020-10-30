// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Graph;

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The type ThumbnailRequest.
    /// </summary>
    public partial class ThumbnailRequest : BaseRequest, IThumbnailRequest
    {
        /// <summary>
        /// Constructs a new ThumbnailRequest.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">Query and header option name value pairs for the request.</param>
        public ThumbnailRequest(
            string requestUrl,
            IBaseClient client,
            IEnumerable<Option> options)
            : base(requestUrl, client, options)
        {
        }

        /// <summary>
        /// Creates the specified Thumbnail using PUT.
        /// </summary>
        /// <param name="thumbnailToCreate">The Thumbnail to create.</param>
        /// <returns>The created Thumbnail.</returns>
        public Task<Thumbnail> CreateAsync(Thumbnail thumbnailToCreate)
        {
            return this.CreateAsync(thumbnailToCreate, CancellationToken.None);
        }

        /// <summary>
        /// Creates the specified Thumbnail using PUT.
        /// </summary>
        /// <param name="thumbnailToCreate">The Thumbnail to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created Thumbnail.</returns>
        public async Task<Thumbnail> CreateAsync(Thumbnail thumbnailToCreate, CancellationToken cancellationToken)
        {
            this.ContentType = "application/json";
            this.Method = "PUT";
            var newEntity = await this.SendAsync<Thumbnail>(thumbnailToCreate, cancellationToken).ConfigureAwait(false);
            this.InitializeCollectionProperties(newEntity);
            return newEntity;
        }

        /// <summary>
        /// Deletes the specified Thumbnail.
        /// </summary>
        /// <returns>The task to await.</returns>
        public Task DeleteAsync()
        {
            return this.DeleteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Deletes the specified Thumbnail.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The task to await.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            this.Method = "DELETE";
            await this.SendAsync<Thumbnail>(null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the specified Thumbnail.
        /// </summary>
        /// <returns>The Thumbnail.</returns>
        public Task<Thumbnail> GetAsync()
        {
            return this.GetAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the specified Thumbnail.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The Thumbnail.</returns>
        public async Task<Thumbnail> GetAsync(CancellationToken cancellationToken)
        {
            this.Method = "GET";
            var retrievedEntity = await this.SendAsync<Thumbnail>(null, cancellationToken).ConfigureAwait(false);
            this.InitializeCollectionProperties(retrievedEntity);
            return retrievedEntity;
        }

        /// <summary>
        /// Updates the specified Thumbnail using PATCH.
        /// </summary>
        /// <param name="thumbnailToUpdate">The Thumbnail to update.</param>
        /// <returns>The updated Thumbnail.</returns>
        public Task<Thumbnail> UpdateAsync(Thumbnail thumbnailToUpdate)
        {
            return this.UpdateAsync(thumbnailToUpdate, CancellationToken.None);
        }

        /// <summary>
        /// Updates the specified Thumbnail using PATCH.
        /// </summary>
        /// <param name="thumbnailToUpdate">The Thumbnail to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The updated Thumbnail.</returns>
        public async Task<Thumbnail> UpdateAsync(Thumbnail thumbnailToUpdate, CancellationToken cancellationToken)
        {
            this.ContentType = "application/json";
            this.Method = "PATCH";
            var updatedEntity = await this.SendAsync<Thumbnail>(thumbnailToUpdate, cancellationToken).ConfigureAwait(false);
            this.InitializeCollectionProperties(updatedEntity);
            return updatedEntity;
        }

        /// <summary>
        /// Adds the specified expand value to the request.
        /// </summary>
        /// <param name="value">The expand value.</param>
        /// <returns>The request object to send.</returns>
        public IThumbnailRequest Expand(string value)
        {
            this.QueryOptions.Add(new QueryOption("$expand", value));
            return this;
        }

        /// <summary>
        /// Adds the specified select value to the request.
        /// </summary>
        /// <param name="value">The select value.</param>
        /// <returns>The request object to send.</returns>
        public IThumbnailRequest Select(string value)
        {
            this.QueryOptions.Add(new QueryOption("$select", value));
            return this;
        }

        /// <summary>
        /// Initializes any collection properties after deserialization, like next requests for paging.
        /// </summary>
        /// <param name="thumbnailToInitialize">The <see cref="Thumbnail"/> with the collection properties to initialize.</param>
        private void InitializeCollectionProperties(Thumbnail thumbnailToInitialize)
        {
        
        }
    }
}
