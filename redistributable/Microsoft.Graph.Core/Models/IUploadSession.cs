// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The IUploadSession interface
    /// </summary>
    public interface IUploadSession
    {
        /// <summary>
        /// Expiration date of the upload session
        /// </summary>
        DateTimeOffset? ExpirationDateTime { get; set; }

        /// <summary>
        /// The ranges yet to be uploaded to the server
        /// </summary>
        IEnumerable<string> NextExpectedRanges { get; set; }

        /// <summary>
        /// The URL for upload
        /// </summary>
        string UploadUrl { get; set; }

    }
}
