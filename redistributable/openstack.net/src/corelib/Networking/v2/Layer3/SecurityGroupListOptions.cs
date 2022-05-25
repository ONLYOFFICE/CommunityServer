using System.Collections.Generic;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Optional filter and paging options when listing security groups.
    /// </summary>
    public class SecurityGroupListOptions : FilterOptions
    {
        /// <summary>
        /// Filter by the group name.
        /// </summary>
        public string Name { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = new Dictionary<string, object>
            {
                ["name"] = Name
            };

            return queryString;
        }
    }
}
