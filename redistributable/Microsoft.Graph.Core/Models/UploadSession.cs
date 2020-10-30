// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Concrete implementation of the IUploadSession interface
    /// </summary>
    internal class UploadSession : IUploadSession
    {
        /// <summary>
        /// Expiration date of the upload session
        /// </summary>
        public DateTimeOffset? ExpirationDateTime { get; set; }

        /// <summary>
        /// The ranges yet to be uploaded to the server
        /// </summary>
        public IEnumerable<string> NextExpectedRanges { get; set; }

        /// <summary>
        /// The URL for upload
        /// </summary>
        public string UploadUrl { get; set; }
    }
}
