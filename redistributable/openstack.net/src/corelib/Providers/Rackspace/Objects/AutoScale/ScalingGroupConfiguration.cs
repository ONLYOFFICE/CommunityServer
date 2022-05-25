namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of the configuration for a new scaling group in the
    /// <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="IAutoScaleService.CreateGroupAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ScalingGroupConfiguration : ScalingGroupConfiguration<PolicyConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalingGroupConfiguration"/> class with the specified
        /// configurations.
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
        public ScalingGroupConfiguration(GroupConfiguration groupConfiguration, LaunchConfiguration launchConfiguration, IEnumerable<PolicyConfiguration> scalingPolicies)
            : base(groupConfiguration, launchConfiguration, scalingPolicies)
        {
        }
    }
}
