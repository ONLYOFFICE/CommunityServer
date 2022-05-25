namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents a change made to a DNS record.
    /// </summary>
    /// <seealso cref="DnsDomainChange"/>
    /// <seealso cref="IDnsService.ListDomainChangesAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Domain_Changes.html">List Domain Changes (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsChange : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Field"/> property.
        /// </summary>
        [JsonProperty("field")]
        private string _field;

        /// <summary>
        /// This is the backing field for the <see cref="OriginalValue"/> property.
        /// </summary>
        [JsonProperty("originalValue")]
        private string _originalValue;

        /// <summary>
        /// This is the backing field for the <see cref="NewValue"/> property.
        /// </summary>
        [JsonProperty("newValue")]
        private string _newValue;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsChange"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsChange()
        {
        }

        /// <summary>
        /// Gets the name of the field which changed.
        /// </summary>
        /// <value>
        /// The name of the field which changed, or <see langword="null"/> if the JSON response from the
        /// server did not include this property.
        /// </value>
        public string Field
        {
            get
            {
                return _field;
            }
        }

        /// <summary>
        /// Gets the value of the field before the change was made.
        /// </summary>
        /// <value>
        /// The original value of the field which changed, or <see langword="null"/> if the JSON response
        /// from the server did not include this property.
        /// </value>
        public string OriginalValue
        {
            get
            {
                return _originalValue;
            }
        }

        /// <summary>
        /// Gets the value of the field after the change was made.
        /// </summary>
        /// <value>
        /// The new value of the field which changed, or <see langword="null"/> if the JSON response
        /// from the server did not include this property.
        /// </value>
        public string NewValue
        {
            get
            {
                return _newValue;
            }
        }
    }
}
