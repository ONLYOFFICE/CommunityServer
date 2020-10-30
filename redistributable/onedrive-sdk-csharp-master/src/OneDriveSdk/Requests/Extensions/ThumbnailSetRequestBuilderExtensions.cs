// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The type ThumbnailSetRequestBuilder.
    /// </summary>
    public partial class ThumbnailSetRequestBuilder
    {
        public IThumbnailRequestBuilder this[string size]
        {
            get
            {
                return new ThumbnailRequestBuilder(
                    this.AppendSegmentToRequestUrl(size),
                    this.Client);
            }
        }
    }
}