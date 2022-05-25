namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class describes the launch configuration for the <see cref="LaunchType.LaunchServer"/>
    /// launch type.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerLaunchConfiguration : LaunchConfiguration<ServerLaunchArguments>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerLaunchConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ServerLaunchConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerLaunchConfiguration"/> class
        /// with the specified arguments.
        /// </summary>
        /// <param name="arguments">A <see cref="ServerLaunchArguments"/> object describing the launch arguments.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="arguments"/> is <see langword="null"/>.</exception>
        public ServerLaunchConfiguration(ServerLaunchArguments arguments)
            : base(LaunchType.LaunchServer, arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");
        }
    }
}
