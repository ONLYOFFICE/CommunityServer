namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to update the properties
    /// of an <see cref="Alarm"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.UpdateAlarmAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateAlarmConfiguration : AlarmConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAlarmConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UpdateAlarmConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAlarmConfiguration"/> class with the specified
        /// values.
        /// </summary>
        /// <param name="checkId">The ID of the check to alert on. This is obtained from <see cref="Check.Id">Check.Id</see>. If this value is <see langword="null"/>, the existing value for the alarm is not changed.</param>
        /// <param name="notificationPlanId">The ID of the notification plan to execute when the state changes. This is obtained from <see cref="NotificationPlan.Id">NotificationPlan.Id</see>. If this value is <see langword="null"/>, the existing value for the alarm is not changed.</param>
        /// <param name="criteria">The <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/alerts-language.html">alarm DSL</see> for describing alerting conditions and their output states. If this value is <see langword="null"/>, the existing value for the alarm is not changed.</param>
        /// <param name="enabled"><see langword="true"/> to enable processing and alerts on this alarm; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, the existing value for the alarm is not changed.</param>
        /// <param name="label">A friendly label for the alarm. If this value is <see langword="null"/>, the existing value for the alarm is not changed.</param>
        /// <param name="metadata">A collection of metadata to associate with the alarm. If this value is <see langword="null"/>, the existing value for the alarm is not changed.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadata"/> contains any values with empty keys.
        /// </exception>
        public UpdateAlarmConfiguration(CheckId checkId = null, NotificationPlanId notificationPlanId = null, string criteria = null, bool? enabled = null, string label = null, IDictionary<string, string> metadata = null)
            : base(checkId, notificationPlanId, criteria, enabled, label, metadata)
        {
        }
    }
}
