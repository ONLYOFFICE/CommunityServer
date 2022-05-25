namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the login data reported agents for
    /// the <see cref="HostInformationType.Who"/> information type.
    /// </summary>
    /// <see cref="IMonitoringService.GetLoginInformationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoginInformation : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="User"/> property.
        /// </summary>
        [JsonProperty("user")]
        private string _user;

        /// <summary>
        /// This is the backing field for the <see cref="Device"/> property.
        /// </summary>
        [JsonProperty("device")]
        private string _device;

        /// <summary>
        /// This is the backing field for the <see cref="Time"/> property.
        /// </summary>
        [JsonProperty("time")]
        private long? _time;

        /// <summary>
        /// This is the backing field for the <see cref="Host"/> property.
        /// </summary>
        [JsonProperty("host")]
        private string _host;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginInformation"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoginInformation()
        {
        }

        /// <summary>
        /// Gets the username of the logged in user.
        /// </summary>
        public string User
        {
            get
            {
                return _user;
            }
        }

        /// <summary>
        /// Gets the device the user is attached to.
        /// </summary>
        public string Device
        {
            get
            {
                return _device;
            }
        }

        /// <summary>
        /// Gets the login time.
        /// </summary>
        public DateTimeOffset? Time
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_time);
            }
        }

        /// <summary>
        /// Gets the originating host of the login.
        /// </summary>
        public string Host
        {
            get
            {
                return _host;
            }
        }
    }
}
