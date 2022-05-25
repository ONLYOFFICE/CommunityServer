namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the basic information related to the configuration of a
    /// scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <typeparam name="TPolicyConfiguration">The type modeling the JSON representation of a scaling policy.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ScalingGroupConfiguration<TPolicyConfiguration> : ExtensibleJsonObject
        where TPolicyConfiguration : PolicyConfiguration
    {
        /// <summary>
        /// This is the backing field for the <see cref="GroupConfiguration"/> property.
        /// </summary>
        [JsonProperty("groupConfiguration", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private GroupConfiguration _groupConfiguration;

        /// <summary>
        /// This is the backing field for the <see cref="LaunchConfiguration"/> property.
        /// </summary>
        [JsonProperty("launchConfiguration", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private JObject _launchConfiguration;

        /// <summary>
        /// This is the backing field for the <see cref="ScalingPolicies"/> property.
        /// </summary>
        [JsonProperty("scalingPolicies", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private TPolicyConfiguration[] _scalingPolicies;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalingGroupConfiguration{TPolicyConfiguration}"/>
        /// class during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ScalingGroupConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalingGroupConfiguration{TPolicyConfiguration}"/>
        /// class with the specified configuration.
        /// </summary>
        /// <param name="groupConfiguration">The group configuration for the scaling group.</param>
        /// <param name="launchConfiguration">The launch configuration for the scaling group.</param>
        /// <param name="scalingPolicies">A collection of scaling policies to initially create with the scaling group.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupConfiguration"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="launchConfiguration"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="scalingPolicies"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="scalingPolicies"/> contains any <see langword="null"/> values.</exception>
        protected ScalingGroupConfiguration(GroupConfiguration groupConfiguration, LaunchConfiguration launchConfiguration, IEnumerable<TPolicyConfiguration> scalingPolicies)
        {
            if (groupConfiguration == null)
                throw new ArgumentNullException("groupConfiguration");
            if (launchConfiguration == null)
                throw new ArgumentNullException("launchConfiguration");
            if (scalingPolicies == null)
                throw new ArgumentNullException("scalingPolicies");
            if (scalingPolicies.Contains(null))
                throw new ArgumentException("scalingPolicies cannot contain any null values", "scalingPolicies");

            _groupConfiguration = groupConfiguration;
            _launchConfiguration = JObject.FromObject(launchConfiguration);
            _scalingPolicies = scalingPolicies.ToArray();
        }

        /// <summary>
        /// Gets a <see cref="GroupConfiguration"/> object describing the group configuration for the scaling group.
        /// </summary>
        public GroupConfiguration GroupConfiguration
        {
            get
            {
                return _groupConfiguration;
            }
        }

        /// <summary>
        /// Gets a <see cref="LaunchConfiguration"/> object describing the launch configuration for the scaling group.
        /// </summary>
        public LaunchConfiguration LaunchConfiguration
        {
            get
            {
                if (_launchConfiguration == null)
                    return null;

                return LaunchConfiguration.FromJObject(_launchConfiguration);
            }
        }

        /// <summary>
        /// Gets a collection of <typeparamref name="TPolicyConfiguration"/> objects describing the
        /// scaling policies of the scaling group.
        /// </summary>
        public ReadOnlyCollection<TPolicyConfiguration> ScalingPolicies
        {
            get
            {
                if (_scalingPolicies == null)
                    return null;

                return new ReadOnlyCollection<TPolicyConfiguration>(_scalingPolicies);
            }
        }
    }
}
