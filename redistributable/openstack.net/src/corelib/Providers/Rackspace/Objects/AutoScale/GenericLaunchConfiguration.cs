namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class extends the <see cref="LaunchConfiguration{TArguments}"/> class
    /// to model custom launch configurations that are not directly supported by this SDK.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class GenericLaunchConfiguration : LaunchConfiguration<JToken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLaunchConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GenericLaunchConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLaunchConfiguration"/> class
        /// with the specified launch type and arguments.
        /// </summary>
        /// <param name="launchType">The launch type.</param>
        /// <param name="arguments">An object modeling the JSON representation of the launch arguments.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="launchType"/> is <see langword="null"/>.</exception>
        public GenericLaunchConfiguration(LaunchType launchType, object arguments)
            : base(launchType, JToken.FromObject(arguments))
        {
            if (launchType == null)
                throw new ArgumentNullException("launchType");
        }
    }
}
