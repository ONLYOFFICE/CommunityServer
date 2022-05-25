namespace net.openstack.Core.Domain
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the root object of the home document described by
    /// <strong>Home Documents for HTTP APIs</strong>.
    /// </summary>
    /// <seealso href="http://tools.ietf.org/html/draft-nottingham-json-home-03#section-2">JSON Home Documents (Home Documents for HTTP APIs - draft-nottingham-json-home-03)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class HomeDocument : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// The backing field for the <see cref="Resources"/> property.
        /// </summary>
        [JsonProperty("resources")]
        private Dictionary<string, ResourceObject> _resources;
#pragma warning restore 649

        /// <summary>
        /// Gets the resources. The keys of this dictionary are link relation types
        /// (as defined by <see href="http://tools.ietf.org/html/rfc5988">RFC5988</see>).
        /// </summary>
        public Dictionary<string, ResourceObject> Resources
        {
            get
            {
                return _resources;
            }
        }
    }
}
