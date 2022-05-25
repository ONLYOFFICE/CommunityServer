namespace net.openstack.Core.Synchronous
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading;
    using net.openstack.Core.Collections;
    using net.openstack.Providers.Rackspace;
    using net.openstack.Providers.Rackspace.Objects.AutoScale;

    /// <summary>
    /// Provides extension methods to allow synchronous calls to the methods in <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
    public static class AutoScaleServiceExtensions
    {
        #region Groups

        /// <summary>
        /// Gets a collection of scaling groups.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="marker">The <see cref="ScalingGroup.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>A collection of <see cref="ScalingGroup"/> objects describing the current scaling groups.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_listGroups_v1.0__tenantId__groups_autoscale-groups.html">List scaling groups (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<ScalingGroup> ListScalingGroups(this IAutoScaleService service, ScalingGroupId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListScalingGroupsAsync(marker, limit, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Create a new scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="configuration">A <see cref="ScalingGroupConfiguration"/> object describing the scaling group configuration.</param>
        /// <returns>A <see cref="ScalingGroup"/> object describing the newly created scaling group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createGroup_v1.0__tenantId__groups_autoscale-groups.html">Create group (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ScalingGroup CreateGroup(this IAutoScaleService service, ScalingGroupConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateGroupAsync(configuration, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Get detailed information about a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <returns>A <see cref="ScalingGroup"/> object describing the scaling group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_showGroupManifest_v1.0__tenantId__groups__groupId__autoscale-groups.html">Show scaling group details (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ScalingGroup GetGroup(this IAutoScaleService service, ScalingGroupId groupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetGroupAsync(groupId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Remove and delete a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="force"><see langword="true"/> to delete the scaling group even if it has active or pending entities; otherwise, <see langword="false"/> to only delete the group if it does not contain any entities. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/DELETE_deleteGroup_v1.0__tenantId__groups__groupId__autoscale-groups.html">Delete scaling group (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void DeleteGroup(this IAutoScaleService service, ScalingGroupId groupId, bool? force)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.DeleteGroupAsync(groupId, force, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Get information about the current state of a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <returns>A <see cref="GroupState"/> object describing the current state of the scaling group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getGroupState_v1.0__tenantId__groups__groupId__state_autoscale-groups.html">Get scaling group state (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static GroupState GetGroupState(this IAutoScaleService service, ScalingGroupId groupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetGroupStateAsync(groupId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Suspend the execution of scaling policies for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_pauseGroup_v1.0__tenantId__groups__groupId__pause_autoscale-groups.html">Pause group policy execution (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void PauseGroup(this IAutoScaleService service, ScalingGroupId groupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.PauseGroupAsync(groupId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Resume the execution of scaling policies for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_resumeGroup_v1.0__tenantId__groups__groupId__resume_autoscale-groups.html">Resume group policy execution (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void ResumeGroup(this IAutoScaleService service, ScalingGroupId groupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.ResumeGroupAsync(groupId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Groups

        #region Configurations

        /// <summary>
        /// Get the group configuration for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <returns>A <see cref="GroupConfiguration"/> object describing the group configuration of the scaling group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getGroupConfig_v1.0__tenantId__groups__groupId__config_Configurations.html">Show scaling group configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static GroupConfiguration GetGroupConfiguration(this IAutoScaleService service, ScalingGroupId groupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetGroupConfigurationAsync(groupId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Set the group configuration for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="configuration">The new group configuration for the scaling group.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putGroupConfig_v1.0__tenantId__groups__groupId__config_Configurations.html">Update scaling group configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetGroupConfiguration(this IAutoScaleService service, ScalingGroupId groupId, GroupConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetGroupConfigurationAsync(groupId, configuration, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Get the launch configuration for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <returns>A <see cref="LaunchConfiguration"/> object describing the launch configuration of the scaling group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getLaunchConfig_v1.0__tenantId__groups__groupId__launch_Configurations.html">Show launch configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LaunchConfiguration GetLaunchConfiguration(this IAutoScaleService service, ScalingGroupId groupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetLaunchConfigurationAsync(groupId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Set the launch configuration for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="configuration">The new launch configuration for the scaling group.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putLaunchConfig_v1.0__tenantId__groups__groupId__launch_Configurations.html">Update launch configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetLaunchConfiguration(this IAutoScaleService service, ScalingGroupId groupId, LaunchConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetLaunchConfigurationAsync(groupId, configuration, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Configurations

        #region Policies

        /// <summary>
        /// Gets a collection of scaling policies for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="marker">The <see cref="Policy.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>A collection of <see cref="Policy"/> objects describing the scaling policies for the scaling group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getPolicies_v1.0__tenantId__groups__groupId__policies_autoscale-policies.html">List policies (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Policy> ListPolicies(this IAutoScaleService service, ScalingGroupId groupId, PolicyId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListPoliciesAsync(groupId, marker, limit, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Create a new scaling policy for a scaling group.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="configuration">A <see cref="PolicyConfiguration"/> object describing the scaling policy configuration.</param>
        /// <returns>A <see cref="Policy"/> object describing the newly created scaling policy.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createPolicies_v1.0__tenantId__groups__groupId__policies_autoscale-policies.html">Create policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Policy CreatePolicy(this IAutoScaleService service, ScalingGroupId groupId, PolicyConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreatePolicyAsync(groupId, configuration, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Get detailed information about a scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <returns>A <see cref="Policy"/> object describing the scaling policy.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getPolicy_v1.0__tenantId__groups__groupId__policies__policyId__autoscale-policies.html">Show policy details (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Policy GetPolicy(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetPolicyAsync(groupId, policyId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Replace the configuration for a scaling policy.
        /// </summary>
        /// <remarks>
        /// This method can be used to replace the behavior associated with webhooks which
        /// have already in use by applications.
        /// </remarks>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="configuration">A <see cref="PolicyConfiguration"/> object describing the new scaling policy configuration.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putPolicy_v1.0__tenantId__groups__groupId__policies__policyId__autoscale-policies.html">Replace policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetPolicy(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId, PolicyConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetPolicyAsync(groupId, policyId, configuration, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Remove and delete a scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/DELETE_deletePolicy_v1.0__tenantId__groups__groupId__policies__policyId__autoscale-policies.html">Delete policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void DeletePolicy(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.DeletePolicyAsync(groupId, policyId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Execute a scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_executePolicy_v1.0__tenantId__groups__groupId__policies__policyId__execute_autoscale-policies.html">Execute policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void ExecutePolicy(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.ExecutePolicyAsync(groupId, policyId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Policies

        #region Webhooks

        /// <summary>
        /// Gets a collection of webhooks which trigger the execution of a particular
        /// scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="marker">The <see cref="Webhook.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>A collection of <see cref="Webhook"/> objects describing the webhooks for the scaling policy.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getWebhooks_v1.0__tenantId__groups__groupId__policies__policyId__webhooks_autoscale-webhooks.html">List webhooks for the policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Webhook> ListWebhooks(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId, WebhookId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListWebhooksAsync(groupId, policyId, marker, limit, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Create a webhook capable of anonymously executing a scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="configuration">A <see cref="NewWebhookConfiguration"/> object describing the webhook configuration.</param>
        /// <returns>A <see cref="Webhook"/> object describing the newly created webhook.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks_autoscale-webhooks.html">Create a webhook (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Webhook CreateWebhook(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId, NewWebhookConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateWebhookAsync(groupId, policyId, configuration, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Create a collection of webhooks capable of anonymously executing a scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="configurations">A collection of <see cref="NewWebhookConfiguration"/> objects describing the webhook configurations.</param>
        /// <returns>A collection of <see cref="Webhook"/> objects describing the newly created webhooks.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="configurations"/> contains any <see langword="null"/> values.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks_autoscale-webhooks.html">Create a webhook (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<Webhook> CreateWebhookRange(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId, IEnumerable<NewWebhookConfiguration> configurations)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateWebhookRangeAsync(groupId, policyId, configurations, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Get detailed information about a webhook associated with a scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="webhookId">The ID of the webhook. This is obtained from <see cref="Webhook.Id">Webhook.Id</see>.</param>
        /// <returns>A <see cref="Webhook"/> object describing the webhook.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="webhookId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks__webhookId__autoscale-webhooks.html">Show webhook details (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Webhook GetWebhook(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetWebhookAsync(groupId, policyId, webhookId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Update the configuration for a webhook.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="webhookId">The ID of the webhook. This is obtained from <see cref="Webhook.Id">Webhook.Id</see>.</param>
        /// <param name="configuration">An <see cref="UpdateWebhookConfiguration"/> object containing the updated configuration for the webhook.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="webhookId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks__webhookId__autoscale-webhooks.html">Update webhook (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateWebhook(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId, UpdateWebhookConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateWebhookAsync(groupId, policyId, webhookId, configuration, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Remove and delete a webhook associated with a scaling policy.
        /// </summary>
        /// <param name="service">The Auto Scale service instance.</param>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="webhookId">The ID of the webhook. This is obtained from <see cref="Webhook.Id">Webhook.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="webhookId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/DELETE_deleteWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks__webhookId__autoscale-webhooks.html">Delete webhook (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void DeleteWebhook(this IAutoScaleService service, ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.DeleteWebhookAsync(groupId, policyId, webhookId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Webhooks
    }
}
