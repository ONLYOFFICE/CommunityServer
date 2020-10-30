// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    public partial class ThumbnailSet
    {
        /// <summary>
        /// Allows for the lookup of custom thumbnails from this thumbnail set.
        /// </summary>
        /// <param name="customThumbnailName">The name of the custom thumbnail.</param>
        /// <returns>The custom thumbnail.</returns>
        public Thumbnail this[string customThumbnailName]
        {
            get
            {
                if (this.AdditionalData != null)
                {
                    object thumbnail;
                    if (this.AdditionalData.TryGetValue(customThumbnailName, out thumbnail))
                    {
                        return thumbnail as Thumbnail;
                    }
                }

                return null;
            }
        }
    }
}
