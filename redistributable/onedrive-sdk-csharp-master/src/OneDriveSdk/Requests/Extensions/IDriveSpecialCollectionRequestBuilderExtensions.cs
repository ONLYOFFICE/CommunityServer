// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public partial interface IDriveSpecialCollectionRequestBuilder
    {
        /// <summary>
        /// Gets app root special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IItemRequestBuilder AppRoot { get; }

        /// <summary>
        /// Gets documents special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IItemRequestBuilder Documents { get; }

        /// <summary>
        /// Gets photos special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IItemRequestBuilder Photos { get; }

        /// <summary>
        /// Gets camera roll special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IItemRequestBuilder CameraRoll { get; }

        /// <summary>
        /// Gets Music special folder item request builder.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IItemRequestBuilder Music { get; }
    }
}