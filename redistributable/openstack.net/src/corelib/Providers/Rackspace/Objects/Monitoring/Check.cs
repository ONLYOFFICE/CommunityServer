namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents an check in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// A check is one of the foundational building blocks of the monitoring system. The
    /// check determines the parts or pieces of the entity that you want to monitor, the
    /// monitoring frequency, how many monitoring zones are originating the check, and so
    /// on. When you create a new check in the monitoring system, you specify the following
    /// information:
    ///
    /// <list type="bullet">
    /// <item>A name for the check</item>
    /// <item>The check's parent entity</item>
    /// <item>The type of check you're creating</item>
    /// <item>Details of the check</item>
    /// <item>The monitoring zones that will launch the check</item>
    /// </list>
    ///
    /// <para>The check, as created, will not trigger alert messages until you create an
    /// alarm to generate notifications, to enable the creation of a single alarm that
    /// acts upon multiple checks (e.g. alert if any of ten different servers stops
    /// responding) or multiple alarms off of a single check. (e.g. ensure both that a
    /// HTTPS server is responding and that it has a valid certificate).</para>
    /// </remarks>
    /// <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html">Checks (Rackspace Cloud Monitoring Developer Guide - API v1.0)</see>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Check : CheckConfiguration
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private CheckId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Created"/> property.
        /// </summary>
        [JsonProperty("created_at")]
        private long? _createdAt;

        /// <summary>
        /// This is the backing field for the <see cref="LastModified"/> property.
        /// </summary>
        [JsonProperty("updated_at")]
        private long? _updatedAt;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="Check"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Check()
        {
        }

        /// <summary>
        /// Gets the unique identifier for the check.
        /// </summary>
        public CheckId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when the check was first created.
        /// </summary>
        public DateTimeOffset? Created
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_createdAt);
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when the check was last modified.
        /// </summary>
        public DateTimeOffset? LastModified
        {
            get
            {
                return DateTimeOffsetExtensions.ToDateTimeOffset(_updatedAt);
            }
        }
    }
}
