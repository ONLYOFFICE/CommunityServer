using System.Collections.Generic;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Optional filter and paging options when listing security group rules.
    /// </summary>
    public class SecurityGroupRuleListOptions : FilterOptions
    {
        /// <summary>
        /// Filter by the group name.
        /// </summary>
        public TrafficDirection Direction { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = new Dictionary<string, object>
            {
                ["direction"] = Direction
            };

            return queryString;
        }
    }
}