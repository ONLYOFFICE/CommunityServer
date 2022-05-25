namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Newtonsoft.Json;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the configurable properties of the JSON representation of
    /// a notification plan resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="NotificationPlan"/>
    /// <seealso cref="NewNotificationPlanConfiguration"/>
    /// <seealso cref="UpdateNotificationPlanConfiguration"/>
    /// <seealso cref="IMonitoringService.CreateNotificationPlanAsync"/>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-plans.html">Notification Plans (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class NotificationPlanConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _label;

        /// <summary>
        /// This is the backing field for the <see cref="CriticalState"/> property.
        /// </summary>
        [JsonProperty("critical_state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NotificationId[] _criticalState;

        /// <summary>
        /// This is the backing field for the <see cref="WarningState"/> property.
        /// </summary>
        [JsonProperty("warning_state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NotificationId[] _warningState;

        /// <summary>
        /// This is the backing field for the <see cref="OkState"/> property.
        /// </summary>
        [JsonProperty("ok_state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NotificationId[] _okState;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private IDictionary<string, string> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationPlanConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NotificationPlanConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationPlanConfiguration"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="label">The label for the notification plan. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="criticalState">The notification list to send to when the state is <see cref="AlarmState.Critical"/>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="warningState">The notification list to send to when the state is <see cref="AlarmState.Warning"/>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="okState">The notification list to send to when the state is <see cref="AlarmState.OK"/>. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="metadata">The metadata to associate with the notification plan. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
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
        protected NotificationPlanConfiguration(string label, IEnumerable<NotificationId> criticalState, IEnumerable<NotificationId> warningState, IEnumerable<NotificationId> okState, IDictionary<string, string> metadata)
        {
            if (label == string.Empty)
                throw new ArgumentException("label cannot be empty");

            _label = label;

            if (criticalState != null)
            {
                _criticalState = criticalState.ToArray();
                if (_criticalState.Contains(null))
                    throw new ArgumentException("criticalState cannot contain any null values", "criticalState");
            }

            if (warningState != null)
            {
                _warningState = warningState.ToArray();
                if (_warningState.Contains(null))
                    throw new ArgumentException("warningState cannot contain any null values", "warningState");
            }

            if (okState != null)
            {
                _okState = okState.ToArray();
                if (_okState.Contains(null))
                    throw new ArgumentException("okState cannot contain any null values", "okState");
            }

            if (metadata != null)
            {
                _metadata = metadata;
                if (metadata.ContainsKey(string.Empty))
                    throw new ArgumentException("metadata cannot contain any empty keys", "metadata");
            }
        }

        /// <summary>
        /// Gets the friendly name for the notification plan.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// Gets the notification list to send to when the state is <see cref="AlarmState.Critical"/>.
        /// </summary>
        public ReadOnlyCollection<NotificationId> CriticalState
        {
            get
            {
                if (_criticalState == null)
                    return null;

                return new ReadOnlyCollection<NotificationId>(_criticalState);
            }
        }

        /// <summary>
        /// Gets the notification list to send to when the state is <see cref="AlarmState.Warning"/>.
        /// </summary>
        public ReadOnlyCollection<NotificationId> WarningState
        {
            get
            {
                if (_warningState == null)
                    return null;

                return new ReadOnlyCollection<NotificationId>(_warningState);
            }
        }

        /// <summary>
        /// Gets the notification list to send to when the state is <see cref="AlarmState.OK"/>.
        /// </summary>
        public ReadOnlyCollection<NotificationId> OkState
        {
            get
            {
                if (_okState == null)
                    return null;

                return new ReadOnlyCollection<NotificationId>(_okState);
            }
        }

        /// <summary>
        /// Gets a collection of metadata associated with the notification plan.
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
