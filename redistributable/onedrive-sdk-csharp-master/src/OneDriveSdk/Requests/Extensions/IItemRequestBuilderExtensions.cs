// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The type  ItemRequestBuilder.
    /// </summary>
    public partial interface IItemRequestBuilder
    {
        /// <summary>
        /// Gets item request builder for the specified item path.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IItemRequestBuilder ItemWithPath(string path);
    }
}