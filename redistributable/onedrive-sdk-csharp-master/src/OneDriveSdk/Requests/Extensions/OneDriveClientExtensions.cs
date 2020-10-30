// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public partial class OneDriveClient
    {
        /// <summary>
        /// Gets the default drive.
        /// </summary>
        public IDriveRequestBuilder Drive
        {
            get
            {
                return new DriveRequestBuilder(string.Format("{0}/{1}", this.BaseUrl, Constants.Url.Drive), this);
            }
        }

        /// <summary>
        /// Gets item request builder for the specified item path.
        /// <returns>The item request builder.</returns>
        /// </summary>
        public IItemRequestBuilder ItemWithPath(string path)
        {
            return new ItemRequestBuilder(
                string.Format("{0}{1}:", this.BaseUrl, path),
                this);
        }
    }
}