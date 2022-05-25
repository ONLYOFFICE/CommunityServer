namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of an Alarm Example resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// View provides examples alarms for the various checks in the system. They are presented as a template with parameters. Each of the parameters is documented with a type, name and description. There are quite a few different examples in the system.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-examples.html">Alarm Examples (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class AlarmExample : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private AlarmExampleId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label")]
        private string _label;

        /// <summary>
        /// This is the backing field for the <see cref="Description"/> property.
        /// </summary>
        [JsonProperty("description")]
        private string _description;

        /// <summary>
        /// This is the backing field for the <see cref="CheckTypeId"/> property.
        /// </summary>
        [JsonProperty("check_type")]
        private CheckTypeId _checkTypeId;

        /// <summary>
        /// This is the backing field for the <see cref="Criteria"/> property.
        /// </summary>
        [JsonProperty("criteria")]
        private string _criteria;

        /// <summary>
        /// This is the backing field for the <see cref="Fields"/> property.
        /// </summary>
        [JsonProperty("fields")]
        private AlarmExampleField[] _fields;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmExample"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AlarmExample()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the alarm example resource.
        /// </summary>
        public AlarmExampleId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the name of the alarm example.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// Gets a description of the alarm example.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Gets the type of check this alarm example applies to.
        /// </summary>
        public CheckTypeId CheckTypeId
        {
            get
            {
                return _checkTypeId;
            }
        }

        /// <summary>
        /// Gets the example criteria as a template.
        /// </summary>
        public string Criteria
        {
            get
            {
                return _criteria;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="AlarmExampleField"/> objects describing the replaceable
        /// template parameters in the example <see cref="Criteria"/> string.
        /// </summary>
        public ReadOnlyCollection<AlarmExampleField> Fields
        {
            get
            {
                if (_fields == null)
                    return null;

                return new ReadOnlyCollection<AlarmExampleField>(_fields);
            }
        }
    }
}
