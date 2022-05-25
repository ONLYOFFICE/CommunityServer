namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents the changes made to various DNS resources as a result of
    /// logical actions made during a particular interval of time.
    /// </summary>
    /// <seealso cref="IDnsService.ListDomainChangesAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Domain_Changes.html">List Domain Changes (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomainChanges : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="TotalEntries"/> property.
        /// </summary>
        [JsonProperty("totalEntries")]
        private int? _totalEntries;

        /// <summary>
        /// This is the backing field for the <see cref="From"/> property.
        /// </summary>
        [JsonProperty("from")]
        private DateTimeOffset? _from;

        /// <summary>
        /// This is the backing field for the <see cref="To"/> property.
        /// </summary>
        [JsonProperty("to")]
        private DateTimeOffset? _to;

        /// <summary>
        /// This is the backing field for the <see cref="Changes"/> property.
        /// </summary>
        [JsonProperty("changes")]
        private DnsDomainChange[] _changes;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomainChanges"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsDomainChanges()
        {
        }

        /// <summary>
        /// Gets the total number of <see cref="DnsDomainChange"/> objects in the time interval
        /// described by <see cref="From"/> and <see cref="To"/>.
        /// </summary>
        /// <remarks>
        /// This value may exceed the number of items in the collection returned by the
        /// <see cref="Changes"/> property since the <see cref="IDnsService.ListDomainChangesAsync"/>
        /// method returns a subset of the complete paginated collection.
        /// </remarks>
        /// <value>
        /// The total number of changes in this time interval, or <see langword="null"/> if the JSON response
        /// from the server did not include this property.
        /// </value>
        public int? TotalEntries
        {
            get
            {
                return _totalEntries;
            }
        }

        /// <summary>
        /// Gets the starting timestamp of this collection of changes to DNS resources.
        /// </summary>
        /// <value>
        /// The starting timestamp of this collection of changes to DNS resources, or <see langword="null"/>
        /// if the JSON response from the server did not include this property.
        /// </value>
        public DateTimeOffset? From
        {
            get
            {
                return _from;
            }
        }

        /// <summary>
        /// Gets the ending timestamp of this collection of changes to DNS resources.
        /// </summary>
        /// <value>
        /// The ending timestamp of this collection of changes to DNS resources, or <see langword="null"/>
        /// if the JSON response from the server did not include this property.
        /// </value>
        public DateTimeOffset? To
        {
            get
            {
                return _to;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsDomainChange"/> objects describing changes made
        /// to DNS resources as a result of logical actions performed during the time interval.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsDomainChange"/> objects describing changes made to
        /// DNS resources, or <see langword="null"/> if the JSON response from the server did not include
        /// this property.
        /// </value>
        public ReadOnlyCollection<DnsDomainChange> Changes
        {
            get
            {
                if (_changes == null)
                    return null;

                return new ReadOnlyCollection<DnsDomainChange>(_changes);
            }
        }
    }
}
