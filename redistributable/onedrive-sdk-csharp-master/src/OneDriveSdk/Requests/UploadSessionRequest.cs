// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.OneDrive.Sdk.Helpers
{
    public class UploadSessionRequest : BaseRequest
    {
        private readonly UploadSession session;

        public UploadSessionRequest(UploadSession session, IBaseClient client, IEnumerable<Option> options)
            : base(session.UploadUrl, client, options)
        {
            this.session = session;
        }

        /// <summary>
        /// Deletes the specified Session
        /// </summary>
        /// <returns>The task to await.</returns>
        public Task DeleteAsync()
        {
            return this.DeleteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Deletes the specified Session
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The task to await.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            this.Method = "DELETE";
            await this.SendAsync<UploadSession>(null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <returns>The Item.</returns>
        public Task<UploadSession> GetAsync()
        {
            return this.GetAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The Item.</returns>
        public async Task<UploadSession> GetAsync(CancellationToken cancellationToken)
        {
            this.Method = "GET";
            var retrievedEntity = await this.SendAsync<UploadSession>(null, cancellationToken).ConfigureAwait(false);
            return retrievedEntity;
        }
    }
}
