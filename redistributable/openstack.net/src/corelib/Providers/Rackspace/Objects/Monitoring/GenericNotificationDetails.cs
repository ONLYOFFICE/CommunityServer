using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// generic notification.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class GenericNotificationDetails : NotificationDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Properties"/> property.
        /// </summary>
        [JsonExtensionData]
        private IDictionary<string, JToken> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNotificationDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GenericNotificationDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNotificationDetails"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="properties">A collection of configuration properties for the notification.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="properties"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="properties"/> contains a <see langword="null"/> or empty key.</exception>
        public GenericNotificationDetails(IDictionary<string, JToken> properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");
            if (properties.ContainsKey(string.Empty))
                throw new ArgumentException("properties cannot contain any empty keys", "properties");

            _properties = properties;
        }

        /// <summary>
        /// Gets a collection of configuration properties for the notification.
        /// </summary>
        public ReadOnlyDictionary<string, JToken> Properties
        {
            get
            {
                return new ReadOnlyDictionary<string, JToken>(_properties);
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class can be used for any notification type. Clients using this class are responsible
        /// for adding the necessary properties for their specific notification type.
        /// </remarks>
        protected internal override bool SupportsNotificationType(NotificationTypeId notificationTypeId)
        {
            return true;
        }
    }
}
