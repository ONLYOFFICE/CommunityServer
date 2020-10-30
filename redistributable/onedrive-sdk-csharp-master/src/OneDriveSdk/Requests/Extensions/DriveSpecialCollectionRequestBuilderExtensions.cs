// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public partial class DriveSpecialCollectionRequestBuilder
    {
        /// <summary>
        /// Gets app root special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder AppRoot
        {
            get { return new ItemRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.AppRoot), this.Client); }
        }

        /// <summary>
        /// Gets Documents special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder Documents
        {
            get { return new ItemRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.Documents), this.Client); }
        }

        /// <summary>
        /// Gets Photos special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder Photos
        {
            get { return new ItemRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.Photos), this.Client); }
        }

        /// <summary>
        /// Gets Camera Roll special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder CameraRoll
        {
            get { return new ItemRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.CameraRoll), this.Client); }
        }

        /// <summary>
        /// Gets Music special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder Music
        {
            get { return new ItemRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.Music), this.Client); }
        }
    }
}