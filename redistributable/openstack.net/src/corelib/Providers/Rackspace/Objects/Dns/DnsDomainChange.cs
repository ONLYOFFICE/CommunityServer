namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;
    using ProjectId = net.openstack.Core.Domain.ProjectId;
    using Tenant = net.openstack.Core.Domain.Tenant;

    /// <summary>
    /// This object represents changes made to a specific resource in the DNS service as the
    /// result of a single logical action.
    /// </summary>
    /// <seealso cref="IDnsService.ListDomainChangesAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Domain_Changes.html">List Domain Changes (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomainChange : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This the backing field for the <see cref="Action"/> property.
        /// </summary>
        [JsonProperty("action")]
        private string _action;

        /// <summary>
        /// This the backing field for the <see cref="TargetType"/> property.
        /// </summary>
        [JsonProperty("targetType")]
        private string _targetType;

        /// <summary>
        /// This the backing field for the <see cref="TargetId"/> property.
        /// </summary>
        [JsonProperty("targetId")]
        private string _targetId;

        /// <summary>
        /// This the backing field for the <see cref="Details"/> property.
        /// </summary>
        [JsonProperty("changeDetails")]
        private DnsChange[] _details;

        /// <summary>
        /// This the backing field for the <see cref="AccountId"/> property.
        /// </summary>
        [JsonProperty("accountId")]
        private ProjectId _accountId;

        /// <summary>
        /// This the backing field for the <see cref="DomainName"/> property.
        /// </summary>
        [JsonProperty("domain")]
        private string _domainName;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomainChange"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsDomainChange()
        {
        }

        /// <summary>
        /// Gets the name of the action which made this change.
        /// </summary>
        /// <value>
        /// The name of the action which made this change, or <see langword="null"/> if the JSON response from the
        /// server did not include this property.
        /// </value>
        public string Action
        {
            get
            {
                return _action;
            }
        }

        /// <summary>
        /// Gets the resource type which was changed.
        /// </summary>
        /// <value>
        /// The type of resource which was changed, or <see langword="null"/> if the JSON response from the
        /// server did not include this property.
        /// </value>
        public string TargetType
        {
            get
            {
                return _targetType;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the resource which was changed.
        /// </summary>
        /// <value>
        /// The unique identifier of the resource which was changed, or <see langword="null"/> if the JSON
        /// response from the server did not include this property.
        /// </value>
        public string TargetId
        {
            get
            {
                return _targetId;
            }
        }

        /// <summary>
        /// Gets a collection of specific changes made to the target resource.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsChange"/> objects describing the changes made to the resource,
        /// or <see langword="null"/> if the JSON response from the server did not include this property.
        /// </value>
        public ReadOnlyCollection<DnsChange> Details
        {
            get
            {
                if (_details == null)
                    return null;

                return new ReadOnlyCollection<DnsChange>(_details);
            }
        }

        /// <summary>
        /// Gets the account ID under which this change was made. The account ID within the DNS service
        /// is equivalent to the <see cref="Tenant.Id">Tenant.Id</see> referenced by other services.
        /// </summary>
        /// <value>
        /// The account ID under which this change was made, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public ProjectId AccountId
        {
            get
            {
                return _accountId;
            }
        }

        /// <summary>
        /// Gets the name of the domain this resource belongs to.
        /// </summary>
        /// <value>
        /// The name of the domain the changed resource belongs to, or <see langword="null"/> if the JSON response from
        /// the server did not include this property.
        /// </value>
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }
    }
}
