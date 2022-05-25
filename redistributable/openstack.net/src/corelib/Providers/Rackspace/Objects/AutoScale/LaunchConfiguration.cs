namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents the launch configuration for a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <remarks>
    /// A launch configuration defines what to do when a new server is created, including information
    /// about the server image, the flavor of the server image, and the load balancer to which to
    /// connect. Currently, the only supported <see cref="LaunchType"/> is <see cref="AutoScale.LaunchType.LaunchServer"/>.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class LaunchConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LaunchConfiguration()
        {
        }

        /// <summary>
        /// Gets the launch type for the Auto Scale launch configuration.
        /// </summary>
        public abstract LaunchType LaunchType
        {
            get;
        }

        /// <summary>
        /// Deserializes a JSON object to a <see cref="LaunchConfiguration"/> instance of the proper type.
        /// </summary>
        /// <param name="jsonObject">The JSON object representing the launch configuration.</param>
        /// <returns>A <see cref="LaunchConfiguration"/> object corresponding to the JSON object.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="jsonObject"/> is <see langword="null"/>.</exception>
        public static LaunchConfiguration FromJObject(JObject jsonObject)
        {
            if (jsonObject == null)
                throw new ArgumentNullException("jsonObject");

            JToken launchType = jsonObject["type"];
            if (launchType == null || launchType.ToObject<LaunchType>() == LaunchType.LaunchServer)
                return jsonObject.ToObject<ServerLaunchConfiguration>();

            return jsonObject.ToObject<GenericLaunchConfiguration>();
        }
    }
}
