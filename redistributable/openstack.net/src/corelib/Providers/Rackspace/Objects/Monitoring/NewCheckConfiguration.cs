namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to create or test a new
    /// <see cref="Check"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.CreateCheckAsync"/>
    /// <seealso cref="IMonitoringService.TestCheckAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewCheckConfiguration : CheckConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewCheckConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NewCheckConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewCheckConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The friendly name of the check. If this is <see langword="null"/>, the label assigned to the new check is unspecified.</param>
        /// <param name="checkTypeId">The check type ID. This is obtained from <see cref="CheckType.Id">CheckType.Id</see>, or from the predefined values in <see cref="CheckTypeId"/>.</param>
        /// <param name="details">A <see cref="CheckDetails"/> object containing detailed configuration information for the specific check type.</param>
        /// <param name="monitoringZonesPoll">A collection of <see cref="MonitoringZoneId"/> objects identifying the monitoring zones to poll from.</param>
        /// <param name="timeout">The timeout of a check operation. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="period">The period between check operations. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="targetAlias">The alias of the target for this check in the associated entity's <see cref="EntityConfiguration.IPAddresses"/> map.</param>
        /// <param name="targetHostname">The hostname this check should target.</param>
        /// <param name="resolverType">The type of resolver to use for converting <paramref name="targetHostname"/> to an IP address.</param>
        /// <param name="metadata">A collection of metadata to associate with the check. If this parameter is <see langword="null"/>, the check is created without any custom metadata.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="checkTypeId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="details"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is non-<see langword="null"/> but empty.
        /// <para>-or-</para>
        /// <para>If the specified <paramref name="details"/> object does support checks of type <paramref name="checkTypeId"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="checkTypeId"/> is a remote check (i.e. the <see cref="Monitoring.CheckTypeId.IsRemote"/> property is <see langword="true"/>) and <paramref name="monitoringZonesPoll"/> is <see langword="null"/> or empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="checkTypeId"/> is a remote check (i.e. the <see cref="Monitoring.CheckTypeId.IsRemote"/> property is <see langword="true"/>) and both <paramref name="targetAlias"/> and <paramref name="targetHostname"/> are <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="monitoringZonesPoll"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="period"/> is less than or equal to <paramref name="timeout"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="targetAlias"/> is non-<see langword="null"/> but empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="targetHostname"/> is non-<see langword="null"/> but empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys, or any <see langword="null"/> values.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="period"/> is less than or equal to <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="targetAlias"/> and <paramref name="targetHostname"/> are both non-<see langword="null"/>.
        /// </exception>
        public NewCheckConfiguration(string label, CheckTypeId checkTypeId, CheckDetails details, IEnumerable<MonitoringZoneId> monitoringZonesPoll, TimeSpan? timeout, TimeSpan? period, string targetAlias, string targetHostname, TargetResolverType resolverType, IDictionary<string, string> metadata)
            : base(label, checkTypeId, details, monitoringZonesPoll, timeout, period, targetAlias, targetHostname, resolverType, metadata)
        {
            if (checkTypeId == null)
                throw new ArgumentNullException("checkTypeId");
            if (details == null)
                throw new ArgumentNullException("details");
            if (checkTypeId.IsRemote && monitoringZonesPoll == null)
                throw new ArgumentException("monitoringZonesPoll cannot be null or empty for remote checks", "monitoringZonesPoll");
            if (checkTypeId.IsRemote && targetAlias == null && targetHostname == null)
                throw new ArgumentException("targetAlias and targetHostname cannot both be null for remote checks");
            if (checkTypeId.IsRemote && MonitoringZonesPoll.Count == 0)
                throw new ArgumentException("monitoringZonesPoll cannot be null or empty for remote checks", "monitoringZonesPoll");
        }
    }
}
