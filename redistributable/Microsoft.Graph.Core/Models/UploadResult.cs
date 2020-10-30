// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    /// <summary>
    /// Result that we get from uploading a slice
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UploadResult<T>
    {
        /// <summary>
        /// The UploadSession containing information about the created upload session.
        /// </summary>
        public IUploadSession UploadSession;

        /// <summary>
        /// The uploaded item, once upload has completed.
        /// </summary>
        public T ItemResponse;

        /// <summary>
        /// The uploaded item location, once upload has completed.
        /// </summary>
        public Uri Location;

        /// <summary>
        /// Status of the request.
        /// </summary>
        public bool UploadSucceeded => (this.ItemResponse != null) || (this.Location != null);

    }
}