using System;
using System.Collections.Generic;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Optional filter and paging options when listing images.
    /// </summary>
    public class ImageListOptions : PageOptions
    {
        /// <summary>
        /// Filters the list of images to those that have changed since the specified date.
        /// </summary>
        public DateTimeOffset? UpdatedAfter { get; set; }

        /// <summary>
        /// Filters the list of images by server.
        /// </summary>
        public Identifier ServerId { get; set; }

        /// <summary>
        /// Filters the list of images by image name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The minimum disk size required to create a server with the image.
        /// </summary>
        public int? MininumDiskSize { get; set; }

        /// <summary>
        /// The minimum amount of RAM required to create a server with the image.
        /// </summary>
        public int? MininumMemorySize { get; set; }

        /// <summary>
        /// Filters by base images or custom server images that you have created.
        /// </summary>
        public ImageType Type { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = base.BuildQueryString();
            queryString["changes-since"] = UpdatedAfter?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            queryString["server"] = ServerId;
            queryString["name"] = Name;
            queryString["minDisk"] = MininumDiskSize;
            queryString["minRam"] = MininumMemorySize;
            queryString["type"] = Type;

            return queryString;
        }
    }
}