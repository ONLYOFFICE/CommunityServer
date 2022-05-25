namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to update the properties
    /// of a <see cref="NotificationPlan"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.UpdateNotificationPlanAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateNotificationPlanConfiguration : NotificationPlanConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateNotificationPlanConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UpdateNotificationPlanConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateNotificationPlanConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The label for the notification plan. If this value is <see langword="null"/>, the existing value for the notification plan is not changed.</param>
        /// <param name="criticalState">The notification list to send to when the state is <see cref="AlarmState.Critical"/>. If this value is <see langword="null"/>, the existing value for the notification plan is not changed.</param>
        /// <param name="warningState">The notification list to send to when the state is <see cref="AlarmState.Warning"/>. If this value is <see langword="null"/>, the existing value for the notification plan is not changed.</param>
        /// <param name="okState">The notification list to send to when the state is <see cref="AlarmState.OK"/>. If this value is <see langword="null"/>, the existing value for the notification plan is not changed.</param>
        /// <param name="metadata">The metadata to associate with the notification plan. If this value is <see langword="null"/>, the existing value for the notification plan is not changed.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is non-<see langword="null"/> but empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="criticalState"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="warningState"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="okState"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        public UpdateNotificationPlanConfiguration(string label = null, IEnumerable<NotificationId> criticalState = null, IEnumerable<NotificationId> warningState = null, IEnumerable<NotificationId> okState = null, IDictionary<string, string> metadata = null)
            : base(label, criticalState, warningState, okState, metadata)
        {
        }
    }
}
