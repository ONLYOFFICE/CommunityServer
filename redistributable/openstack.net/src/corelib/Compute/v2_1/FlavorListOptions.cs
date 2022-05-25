using System.Collections.Generic;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Optional filter and paging options when listing flavors.
    /// </summary>
    public class FlavorListOptions : PageOptions
    {
        /// <summary>
        /// The minimum disk size provided by the flavor.
        /// </summary>
        public int? MininumDiskSize { get; set; }

        /// <summary>
        /// The minimum amount of RAM provided by the flavor.
        /// </summary>
        public int? MininumMemorySize { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = base.BuildQueryString();
            queryString["minDisk"] = MininumDiskSize;
            queryString["minRam"] = MininumMemorySize;

            return queryString;
        }
    }
}