namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the configurable properties of the JSON representation of
    /// a Check resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Check"/>
    /// <seealso cref="NewCheckConfiguration"/>
    /// <seealso cref="UpdateCheckConfiguration"/>
    /// <seealso cref="IMonitoringService.CreateCheckAsync"/>
    /// <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html">Checks (Rackspace Cloud Monitoring Developer Guide - API v1.0)</see>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class CheckConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _label;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private CheckTypeId _type;

        /// <summary>
        /// This is the backing field for the <see cref="Details"/> property.
        /// </summary>
        [JsonProperty("details", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private JObject _details;

        /// <summary>
        /// This is the backing field for the <see cref="MonitoringZonesPoll"/> property.
        /// </summary>
        [JsonProperty("monitoring_zones_poll", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private MonitoringZoneId[] _monitoringZonesPoll;

        /// <summary>
        /// This is the backing field for the <see cref="Timeout"/> property.
        /// </summary>
        [JsonProperty("timeout", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _timeout;

        /// <summary>
        /// This is the backing field for the <see cref="Period"/> property.
        /// </summary>
        [JsonProperty("period", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _period;

        /// <summary>
        /// This is the backing field for the <see cref="TargetAlias"/> property.
        /// </summary>
        [JsonProperty("target_alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _targetAlias;

        /// <summary>
        /// This is the backing field for the <see cref="TargetHostname"/> property.
        /// </summary>
        [JsonProperty("target_hostname", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _targetHostname;

        /// <summary>
        /// This is the backing field for the <see cref="ResolverType"/> property.
        /// </summary>
        [JsonProperty("target_resolver", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private TargetResolverType _resolverType;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private IDictionary<string, string> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected CheckConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The friendly name of the check. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="checkTypeId">The check type ID. This is obtained from <see cref="CheckType.Id">CheckType.Id</see>, or from the predefined values in <see cref="CheckTypeId"/>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="details">A <see cref="CheckDetails"/> object containing detailed configuration information for the specific check type. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="monitoringZonesPoll">A collection of <see cref="MonitoringZoneId"/> objects identifying the monitoring zones to poll from. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="timeout">The timeout of a check operation. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="period">The period between check operations. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="targetAlias">The alias of the target for this check in the associated entity's <see cref="EntityConfiguration.IPAddresses"/> map. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="targetHostname">The hostname this check should target. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="resolverType">The type of resolver to use for converting <paramref name="targetHostname"/> to an IP address. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="metadata">A collection of metadata to associate with the check. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is non-<see langword="null"/> but empty.
        /// <para>-or-</para>
        /// <para>If the specified <paramref name="details"/> object does support checks of type <paramref name="checkTypeId"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="monitoringZonesPoll"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="period"/> is less than or equal to <paramref name="timeout"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="targetAlias"/> is non-<see langword="null"/> but empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="targetHostname"/> is non-<see langword="null"/> but empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="period"/> is less than or equal to <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="targetAlias"/> and <paramref name="targetHostname"/> are both non-<see langword="null"/>.
        /// </exception>
        protected CheckConfiguration(string label, CheckTypeId checkTypeId, CheckDetails details, IEnumerable<MonitoringZoneId> monitoringZonesPoll, TimeSpan? timeout, TimeSpan? period, string targetAlias, string targetHostname, TargetResolverType resolverType, IDictionary<string, string> metadata)
        {
            if (label == string.Empty)
                throw new ArgumentException("label cannot be empty");
            if (targetAlias == string.Empty)
                throw new ArgumentException("targetAlias cannot be empty");
            if (targetHostname == string.Empty)
                throw new ArgumentException("targetHostname cannot be empty");
            if (details != null && !details.SupportsCheckType(checkTypeId))
                throw new ArgumentException(string.Format("The check details object does not support '{0}' checks.", checkTypeId), "details");
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeout");
            if (period <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("period");
            if (period <= timeout)
                throw new ArgumentException("period cannot be less than or equal to timeout", "period");
            if (targetAlias != null && targetHostname != null)
                throw new InvalidOperationException("targetAlias and targetHostname cannot both be specified");
            if (metadata != null && metadata.ContainsKey(string.Empty))
                throw new ArgumentException("metadata cannot contain any empty keys", "metadata");

            _label = label;
            _type = checkTypeId;
            _details = details != null ? JObject.FromObject(details) : null;
            _monitoringZonesPoll = monitoringZonesPoll != null ? monitoringZonesPoll.ToArray() : null;
            _timeout = timeout.HasValue ? (int?)timeout.Value.TotalSeconds : null;
            _period = period.HasValue ? (int?)period.Value.TotalSeconds : null;
            _targetAlias = targetAlias;
            _targetHostname = targetHostname;
            _resolverType = resolverType;
            _metadata = metadata;

            // this check is at the end of the constructor so monitoringZonesPoll is only enumerated once
            if (_monitoringZonesPoll != null && _monitoringZonesPoll.Contains(null))
                throw new ArgumentException("monitoringZonesPoll cannot contain any null values", "monitoringZonesPoll");
        }

        /// <summary>
        /// Gets the friendly name of the check.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// Gets the ID of the check type.
        /// </summary>
        public CheckTypeId CheckTypeId
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets a <see cref="CheckDetails"/> object describing the detailed properties specific
        /// to this type of check.
        /// </summary>
        /// <remarks>
        /// The exact type returned by this property depends on the <see cref="CheckTypeId"/>
        /// for the current check. For additional information about the predefined check types,
        /// see <see cref="Monitoring.CheckTypeId"/>.
        /// </remarks>
        public CheckDetails Details
        {
            get
            {
                if (_details == null)
                    return null;

                return CheckDetails.FromJObject(CheckTypeId, _details);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="MonitoringZoneId"/> objects identifying the monitoring
        /// zones to poll from.
        /// </summary>
        public ReadOnlyCollection<MonitoringZoneId> MonitoringZonesPoll
        {
            get
            {
                if (_monitoringZonesPoll == null)
                    return null;

                return new ReadOnlyCollection<MonitoringZoneId>(_monitoringZonesPoll);
            }
        }

        /// <summary>
        /// Gets the timeout for the check.
        /// </summary>
        public TimeSpan? Timeout
        {
            get
            {
                if (_timeout == null)
                    return null;

                return TimeSpan.FromSeconds(_timeout.Value);
            }
        }

        /// <summary>
        /// Gets the delay between check operations.
        /// </summary>
        public TimeSpan? Period
        {
            get
            {
                if (_period == null)
                    return null;

                return TimeSpan.FromSeconds(_period.Value);
            }
        }

        /// <summary>
        /// Gets the key for looking up the target for this check in the associated
        /// entity's <see cref="EntityConfiguration.IPAddresses"/> map.
        /// </summary>
        public string TargetAlias
        {
            get
            {
                return _targetAlias;
            }
        }

        /// <summary>
        /// Gets the target hostname this check should target.
        /// </summary>
        public string TargetHostname
        {
            get
            {
                return _targetHostname;
            }
        }

        /// <summary>
        /// Gets the type of resolver that should be used to convert the <see cref="TargetHostname"/>
        /// to an IP address.
        /// </summary>
        public TargetResolverType ResolverType
        {
            get
            {
                return _resolverType;
            }
        }

        /// <summary>
        /// Gets a collection of custom metadata associated with the check.
        /// </summary>
        public ReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                if (_metadata == null)
                    return null;

                return new ReadOnlyDictionary<string, string>(_metadata);
            }
        }
    }
}
