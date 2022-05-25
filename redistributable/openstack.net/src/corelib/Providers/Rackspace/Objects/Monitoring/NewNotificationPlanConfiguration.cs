namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to create a new
    /// <see cref="NotificationPlan"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.CreateNotificationPlanAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewNotificationPlanConfiguration : NotificationPlanConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewNotificationPlanConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NewNotificationPlanConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewNotificationPlanConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The label for the notification plan.</param>
        /// <param name="criticalState">The notification list to send to when the state is <see cref="AlarmState.Critical"/>. If this value is <see langword="null"/>, notifications are not sent for this state.</param>
        /// <param name="warningState">The notification list to send to when the state is <see cref="AlarmState.Warning"/>. If this value is <see langword="null"/>, notifications are not sent for this state.</param>
        /// <param name="okState">The notification list to send to when the state is <see cref="AlarmState.OK"/>. If this value is <see langword="null"/>, notifications are not sent for this state.</param>
        /// <param name="metadata">The metadata to associate with the notification plan. If this value is <see langword="null"/>, no custom metadata is associated with the notification plan.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="label"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="criticalState"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="warningState"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="okState"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        public NewNotificationPlanConfiguration(string label, IEnumerable<NotificationId> criticalState = null, IEnumerable<NotificationId> warningState = null, IEnumerable<NotificationId> okState = null, IDictionary<string, string> metadata = null)
            : base(label, criticalState, warningState, okState, metadata)
        {
            if (label == null)
                throw new ArgumentNullException("label");
        }
    }
}
