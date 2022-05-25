using System.Collections.Generic;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Optional filter and paging options when listing servers.
    /// </summary>
    public class RouterListOptions : FilterOptions
    {
        /// <summary>
        /// Filter by status.
        /// </summary>
        public RouterStatus Status { get; set; }

        /// <summary>
        /// Filter by router name.
        /// </summary>
        public string Name { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = new Dictionary<string, object>
            {
                ["status"] = Status,
                ["name"] = Name
            };

            return queryString;
        }
    }
}
