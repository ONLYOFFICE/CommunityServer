// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public partial interface IOneDriveClient
    {

        /// <summary>
        /// Gets the default drive.
        /// </summary>
        IDriveRequestBuilder Drive { get; }

        /// <summary>
        /// Gets item request builder for the specified item path.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IItemRequestBuilder ItemWithPath(string path);
    }
}