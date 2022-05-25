using System;
using System.Collections.Generic;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Optional filter and paging options when listing servers.
    /// </summary>
    public class ServerListOptions : PageOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerListOptions"/> class.
        /// </summary>
        public ServerListOptions()
        {
            Metadata = new Dictionary<string, string>();
        }

        /// <summary>
        /// Filter by a date and time stamp when the server last changed status. 
        /// </summary>
        public DateTimeOffset? UpdatedAfter { get; set; }

        /// <summary>
        /// Filter by an image.
        /// </summary>
        public Identifier ImageId { get; set; }

        /// <summary>
        /// Filter by a flavor.
        /// </summary>
        public string FlavorId { get; set; }

        /// <summary>
        /// Filter by a server name.
        /// <para>You can use regular expressions in the query. For example, the ?name=bob regular expression returns both bob and bobb.</para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Filter by a server status.
        /// </summary>
        public ServerStatus Status { get; set; }

        /// <summary>
        /// Filter by associated server metadata.
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; } 

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = base.BuildQueryString();
            queryString["changes-since"] = UpdatedAfter?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            queryString["image"] = ImageId;
            queryString["flavor"] = FlavorId;
            queryString["name"] = Name;
            queryString["status"] = Status;
            queryString["metadata"] = Metadata;

            return queryString;
        }
    }
}