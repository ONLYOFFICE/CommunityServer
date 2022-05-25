namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a monitoring entity overview.
    /// </summary>
    /// <seealso cref="O:net.openstack.Providers.Rackspace.IMonitoringService.ListEntityOverviewsAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class EntityOverview : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Entity"/> property.
        /// </summary>
        [JsonProperty("entity")]
        private Entity _entity;

        /// <summary>
        /// This is the backing field for the <see cref="Checks"/> property.
        /// </summary>
        [JsonProperty("checks")]
        private Check[] _checks;

        /// <summary>
        /// This is the backing field for the <see cref="Alarms"/> property.
        /// </summary>
        [JsonProperty("alarms")]
        private Alarm[] _alarms;

        /// <summary>
        /// This is the backing field for the <see cref="LatestAlarmStates"/> property.
        /// </summary>
        [JsonProperty("latest_alarm_states")]
        private AlarmStateHistory[] _latestAlarmStates;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityOverview"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected EntityOverview()
        {
        }

        /// <summary>
        /// Gets an <see cref="Entity"/> object describing the monitoring entity.
        /// </summary>
        public Entity Entity
        {
            get
            {
                return _entity;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="Check"/> objects describing the checks associated
        /// with the entity.
        /// </summary>
        public ReadOnlyCollection<Check> Checks
        {
            get
            {
                if (_checks == null)
                    return null;

                return new ReadOnlyCollection<Check>(_checks);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="Alarm"/> objects describing the alarms associated
        /// with the entity.
        /// </summary>
        public ReadOnlyCollection<Alarm> Alarms
        {
            get
            {
                if (_alarms == null)
                    return null;

                return new ReadOnlyCollection<Alarm>(_alarms);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="AlarmStateHistory"/> objects describing recent
        /// changes to the state of alarms associated with the entity.
        /// </summary>
        public ReadOnlyCollection<AlarmStateHistory> LatestAlarmStates
        {
            get
            {
                if (_latestAlarmStates == null)
                    return null;

                return new ReadOnlyCollection<AlarmStateHistory>(_latestAlarmStates);
            }
        }
    }
}
