// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

using Microsoft.Graph;

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The interface IUploadChunkRequest.
    /// </summary>
    public partial interface IUploadChunkRequest : IBaseRequest
    {
        /// <summary>
        /// Puts the specified Chunk.
        /// </summary>
        /// <returns>The task to await.</returns>
        Task<UploadChunkResult> PutAsync(Stream stream);
    }
}
