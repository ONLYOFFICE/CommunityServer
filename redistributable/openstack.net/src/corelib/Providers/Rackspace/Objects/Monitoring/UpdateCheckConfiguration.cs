namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to update the properties
    /// of a <see cref="Check"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.UpdateCheckAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateCheckConfiguration : CheckConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCheckConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UpdateCheckConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCheckConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The friendly name of the check. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="checkTypeId">The check type ID. This is obtained from <see cref="CheckType.Id">CheckType.Id</see>, or from the predefined values in <see cref="CheckTypeId"/>. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="details">A <see cref="CheckDetails"/> object containing detailed configuration information for the specific check type. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="monitoringZonesPoll">A collection of <see cref="MonitoringZoneId"/> objects identifying the monitoring zones to poll from. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="timeout">The timeout of a check operation. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="period">The period between check operations. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="targetAlias">The alias of the target for this check in the associated entity's <see cref="EntityConfiguration.IPAddresses"/> map. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="targetHostname">The hostname this check should target. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="resolverType">The type of resolver to use for converting <paramref name="targetHostname"/> to an IP address. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
        /// <param name="metadata">A collection of metadata to associate with the check. If this value is <see langword="null"/>, the existing value for the check is not changed.</param>
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
        public UpdateCheckConfiguration(string label = null, CheckTypeId checkTypeId = null, CheckDetails details = null, IEnumerable<MonitoringZoneId> monitoringZonesPoll = null, TimeSpan? timeout = null, TimeSpan? period = null, string targetAlias = null, string targetHostname = null, TargetResolverType resolverType = null, IDictionary<string, string> metadata = null)
            : base(label, checkTypeId, details, monitoringZonesPoll, timeout, period, targetAlias, targetHostname, resolverType, metadata)
        {
        }
    }
}
