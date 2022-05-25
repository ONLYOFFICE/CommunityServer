namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class extends the <see cref="LaunchConfiguration"/> class with
    /// strongly-typed launch arguments.
    /// </summary>
    /// <typeparam name="TArguments">The type modeling the arguments for a launch configuration.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class LaunchConfiguration<TArguments> : LaunchConfiguration
    {
        /// <summary>
        /// This is the backing field for the <see cref="LaunchType"/> property.
        /// </summary>
        [JsonProperty("type")]
        private LaunchType _launchType;

        /// <summary>
        /// This is the backing field for the <see cref="Arguments"/> property.
        /// </summary>
        [JsonProperty("args", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private TArguments _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchConfiguration{TArguments}"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LaunchConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchConfiguration{TArguments}"/> class
        /// with the specified launch type and arguments.
        /// </summary>
        /// <param name="launchType">The server launch type.</param>
        /// <param name="arguments">The arguments for launching a server.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="launchType"/> is <see langword="null"/>.</exception>
        protected LaunchConfiguration(LaunchType launchType, TArguments arguments)
        {
            if (launchType == null)
                throw new ArgumentNullException("launchType");

            _launchType = launchType;
            _arguments = arguments;
        }

        /// <inheritdoc/>
        public override LaunchType LaunchType
        {
            get
            {
                return _launchType;
            }
        }

        /// <summary>
        /// Gets the launch arguments.
        /// </summary>
        public TArguments Arguments
        {
            get
            {
                return _arguments;
            }
        }
    }
}
