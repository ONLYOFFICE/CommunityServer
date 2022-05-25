namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of a request to create a new
    /// <see cref="Alarm"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.CreateAlarmAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewAlarmConfiguration : AlarmConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewAlarmConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NewAlarmConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewAlarmConfiguration"/> class with the specified
        /// values.
        /// </summary>
        /// <param name="checkId">The ID of the check to alert on. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <param name="notificationPlanId">The ID of the notification plan to execute when the state changes. This is obtained from <see cref="NotificationPlan.Id">NotificationPlan.Id</see>.</param>
        /// <param name="criteria">The <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/alerts-language.html">alarm DSL</see> for describing alerting conditions and their output states.</param>
        /// <param name="enabled"><see langword="true"/> to enable processing and alerts on this alarm; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, <placeholder/>.</param>
        /// <param name="label">A friendly label for the alarm.</param>
        /// <param name="metadata">A collection of metadata to associate with the alarm.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="checkId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="notificationPlanId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadata"/> contains any values with empty keys.
        /// </exception>
        public NewAlarmConfiguration(CheckId checkId, NotificationPlanId notificationPlanId, string criteria = null, bool? enabled = null, string label = null, IDictionary<string, string> metadata = null)
            : base(checkId, notificationPlanId, criteria, enabled, label, metadata)
        {
            if (checkId == null)
                throw new ArgumentNullException("checkId");
            if (notificationPlanId == null)
                throw new ArgumentNullException("notificationPlanId");
        }
    }
}
