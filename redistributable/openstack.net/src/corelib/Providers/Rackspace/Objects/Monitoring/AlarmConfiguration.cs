using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the basic properties of an Alarm resource
    /// in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Alarm"/>
    /// <seealso cref="NewAlarmConfiguration"/>
    /// <seealso cref="UpdateAlarmConfiguration"/>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarms.html">Alarms (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class AlarmConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="CheckId"/> property.
        /// </summary>
        [JsonProperty("check_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private CheckId _checkId;

        /// <summary>
        /// This is the backing field for the <see cref="NotificationPlanId"/> property.
        /// </summary>
        [JsonProperty("notification_plan_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NotificationPlanId _notificationPlanId;

        /// <summary>
        /// This is the backing field for the <see cref="Criteria"/> property.
        /// </summary>
        [JsonProperty("criteria", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _criteria;

        /// <summary>
        /// This is the backing field for the <see cref="Enabled"/> property.
        /// </summary>
        [JsonProperty("disabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private bool? _disabled;

        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _label;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private IDictionary<string, string> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AlarmConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmConfiguration"/> class with the specified
        /// values.
        /// </summary>
        /// <param name="checkId">The ID of the check to alert on. This is obtained from <see cref="Check.Id">Check.Id</see>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="notificationPlanId">The ID of the notification plan to execute when the state changes. This is obtained from <see cref="NotificationPlan.Id">NotificationPlan.Id</see>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="criteria">The <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/alerts-language.html">alarm DSL</see> for describing alerting conditions and their output states. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="enabled"><see langword="true"/> to enable processing and alerts on this alarm; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="label">A friendly label for the alarm. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="metadata">A collection of metadata to associate with the alarm. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadata"/> contains any values with empty keys.
        /// </exception>
        protected AlarmConfiguration(CheckId checkId, NotificationPlanId notificationPlanId, string criteria, bool? enabled, string label, IDictionary<string, string> metadata)
        {
            if (metadata != null && metadata.ContainsKey(string.Empty))
                throw new ArgumentException("metadata cannot contain any empty keys", "metadata");

            _checkId = checkId;
            _notificationPlanId = notificationPlanId;
            _criteria = criteria;
            _disabled = !enabled;
            _label = label;
            _metadata = metadata;
        }

        /// <summary>
        /// Gets the ID of the check to alert on.
        /// </summary>
        /// <seealso cref="Check.Id"/>
        public CheckId CheckId
        {
            get
            {
                return _checkId;
            }
        }

        /// <summary>
        /// Gets the ID of the notification plan to execute when the state changes.
        /// </summary>
        /// <seealso cref="NotificationPlan.Id"/>
        public NotificationPlanId NotificationPlanId
        {
            get
            {
                return _notificationPlanId;
            }
        }

        /// <summary>
        /// Gets the alarm DSL for describing alerting conditions and their output states.
        /// </summary>
        public string Criteria
        {
            get
            {
                return _criteria;
            }
        }

        /// <summary>
        /// Gets a value indicating whether processing and alerts are enabled on the alarm.
        /// </summary>
        public bool? Enabled
        {
            get
            {
                return !_disabled;
            }
        }

        /// <summary>
        /// Gets the friendly label for the alarm.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// Gets a collection of metadata associated with the alarm.
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
