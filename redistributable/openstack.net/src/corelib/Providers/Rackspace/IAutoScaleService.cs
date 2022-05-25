namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using net.openstack.Core.Collections;
    using net.openstack.Providers.Rackspace.Objects.AutoScale;

    /// <summary>
    /// Represents a provider for the Rackspace Cloud Auto Scale service.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/Overview.html">Rackspace Auto Scale Developer Guide - API v1.0</seealso>
    /// <preliminary/>
    public interface IAutoScaleService
    {
        #region Groups

        /// <summary>
        /// Gets a collection of scaling groups.
        /// </summary>
        /// <param name="marker">The <see cref="ScalingGroup.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="ScalingGroup"/> objects describing the current scaling groups.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_listGroups_v1.0__tenantId__groups_autoscale-groups.html">List scaling groups (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollectionPage<ScalingGroup>> ListScalingGroupsAsync(ScalingGroupId marker, int? limit, CancellationToken cancellationToken);

        /// <summary>
        /// Create a new scaling group.
        /// </summary>
        /// <param name="configuration">A <see cref="ScalingGroupConfiguration"/> object describing the scaling group configuration.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="ScalingGroup"/>
        /// object describing the newly created scaling group.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createGroup_v1.0__tenantId__groups_autoscale-groups.html">Create Group (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<ScalingGroup> CreateGroupAsync(ScalingGroupConfiguration configuration, CancellationToken cancellationToken);

        /// <summary>
        /// Get detailed information about a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="ScalingGroup"/>
        /// object describing the scaling group.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_showGroupManifest_v1.0__tenantId__groups__groupId__autoscale-groups.html">Show scaling group details (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<ScalingGroup> GetGroupAsync(ScalingGroupId groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Remove and delete a scaling group.
        /// </summary>
        /// <remarks>
        /// If <paramref name="force"/> is <see langword="true"/>, active servers in the group will be
        /// immediately deleted, and pending servers will be deleted once they finish building and become
        /// active.
        /// </remarks>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="force"><see langword="true"/> to delete the scaling group even if it has active or pending entities; otherwise, <see langword="false"/> to only delete the group if it does not contain any entities. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/DELETE_deleteGroup_v1.0__tenantId__groups__groupId__autoscale-groups.html">Delete scaling group (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task DeleteGroupAsync(ScalingGroupId groupId, bool? force, CancellationToken cancellationToken);

        /// <summary>
        /// Get information about the current state of a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="GroupState"/>
        /// object describing the current state of the scaling group.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getGroupState_v1.0__tenantId__groups__groupId__state_autoscale-groups.html">Get scaling group state (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<GroupState> GetGroupStateAsync(ScalingGroupId groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Suspend the execution of scaling policies for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_pauseGroup_v1.0__tenantId__groups__groupId__pause_autoscale-groups.html">Pause group policy execution (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task PauseGroupAsync(ScalingGroupId groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Resume the execution of scaling policies for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_resumeGroup_v1.0__tenantId__groups__groupId__resume_autoscale-groups.html">Resume group policy execution (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task ResumeGroupAsync(ScalingGroupId groupId, CancellationToken cancellationToken);

        #endregion Groups

        #region Configurations

        /// <summary>
        /// Get the group configuration for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="GroupConfiguration"/>
        /// object describing the group configuration of the scaling group.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getGroupConfig_v1.0__tenantId__groups__groupId__config_Configurations.html">Show scaling group configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<GroupConfiguration> GetGroupConfigurationAsync(ScalingGroupId groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Set the group configuration for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="configuration">The new group configuration for the scaling group.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putGroupConfig_v1.0__tenantId__groups__groupId__config_Configurations.html">Update scaling group configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task SetGroupConfigurationAsync(ScalingGroupId groupId, GroupConfiguration configuration, CancellationToken cancellationToken);

        /// <summary>
        /// Get the launch configuration for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="LaunchConfiguration"/>
        /// object describing the launch configuration of the scaling group.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getLaunchConfig_v1.0__tenantId__groups__groupId__launch_Configurations.html">Show launch configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<LaunchConfiguration> GetLaunchConfigurationAsync(ScalingGroupId groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Set the launch configuration for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="configuration">The new launch configuration for the scaling group.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putLaunchConfig_v1.0__tenantId__groups__groupId__launch_Configurations.html">Update launch configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task SetLaunchConfigurationAsync(ScalingGroupId groupId, LaunchConfiguration configuration, CancellationToken cancellationToken);

        #endregion Configurations

        #region Policies

        /// <summary>
        /// Gets a collection of scaling policies for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="marker">The <see cref="Policy.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="Policy"/> objects describing the scaling policies for the scaling group.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="groupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getPolicies_v1.0__tenantId__groups__groupId__policies_autoscale-policies.html">List policies (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollectionPage<Policy>> ListPoliciesAsync(ScalingGroupId groupId, PolicyId marker, int? limit, CancellationToken cancellationToken);

        /// <summary>
        /// Create a new scaling policy for a scaling group.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="configuration">A <see cref="PolicyConfiguration"/> object describing the scaling policy configuration.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="Policy"/>
        /// object describing the newly created scaling policy.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createPolicies_v1.0__tenantId__groups__groupId__policies_autoscale-policies.html">Create policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<Policy> CreatePolicyAsync(ScalingGroupId groupId, PolicyConfiguration configuration, CancellationToken cancellationToken);

        /// <summary>
        /// Get detailed information about a scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="Policy"/>
        /// object describing the scaling policy.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getPolicy_v1.0__tenantId__groups__groupId__policies__policyId__autoscale-policies.html">Show policy details (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<Policy> GetPolicyAsync(ScalingGroupId groupId, PolicyId policyId, CancellationToken cancellationToken);

        /// <summary>
        /// Replace the configuration for a scaling policy.
        /// </summary>
        /// <remarks>
        /// This method can be used to replace the behavior associated with webhooks which
        /// have already in use by applications.
        /// </remarks>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="configuration">A <see cref="PolicyConfiguration"/> object describing the new scaling policy configuration.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putPolicy_v1.0__tenantId__groups__groupId__policies__policyId__autoscale-policies.html">Replace policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task SetPolicyAsync(ScalingGroupId groupId, PolicyId policyId, PolicyConfiguration configuration, CancellationToken cancellationToken);

        /// <summary>
        /// Remove and delete a scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/DELETE_deletePolicy_v1.0__tenantId__groups__groupId__policies__policyId__autoscale-policies.html">Delete policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task DeletePolicyAsync(ScalingGroupId groupId, PolicyId policyId, CancellationToken cancellationToken);

        /// <summary>
        /// Execute a scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_executePolicy_v1.0__tenantId__groups__groupId__policies__policyId__execute_autoscale-policies.html">Execute policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task ExecutePolicyAsync(ScalingGroupId groupId, PolicyId policyId, CancellationToken cancellationToken);

        #endregion Policies

        #region Webhooks

        /// <summary>
        /// Gets a collection of webhooks which trigger the execution of a particular
        /// scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="marker">The <see cref="Webhook.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="Webhook"/> objects describing the webhooks for the scaling policy.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getWebhooks_v1.0__tenantId__groups__groupId__policies__policyId__webhooks_autoscale-webhooks.html">List webhooks for the policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollectionPage<Webhook>> ListWebhooksAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId marker, int? limit, CancellationToken cancellationToken);

        /// <summary>
        /// Create a webhook capable of anonymously executing a scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="configuration">A <see cref="NewWebhookConfiguration"/> object describing the webhook configuration.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="Webhook"/>
        /// object describing the newly created webhook.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks_autoscale-webhooks.html">Create a webhook (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<Webhook> CreateWebhookAsync(ScalingGroupId groupId, PolicyId policyId, NewWebhookConfiguration configuration, CancellationToken cancellationToken);

        /// <summary>
        /// Create a collection of webhooks capable of anonymously executing a scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="configurations">A collection of <see cref="NewWebhookConfiguration"/> objects describing the webhook configurations.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="Webhook"/> objects describing the newly created webhooks.
        /// </returns>
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
        Task<ReadOnlyCollection<Webhook>> CreateWebhookRangeAsync(ScalingGroupId groupId, PolicyId policyId, IEnumerable<NewWebhookConfiguration> configurations, CancellationToken cancellationToken);

        /// <summary>
        /// Get detailed information about a webhook associated with a scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="webhookId">The ID of the webhook. This is obtained from <see cref="Webhook.Id">Webhook.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="Webhook"/>
        /// object describing the webhook.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="webhookId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/GET_getWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks__webhookId__autoscale-webhooks.html">Show webhook details (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task<Webhook> GetWebhookAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId, CancellationToken cancellationToken);

        /// <summary>
        /// Update the configuration for a webhook.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="webhookId">The ID of the webhook. This is obtained from <see cref="Webhook.Id">Webhook.Id</see>.</param>
        /// <param name="configuration">An <see cref="UpdateWebhookConfiguration"/> object containing the updated configuration for the webhook.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
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
        Task UpdateWebhookAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId, UpdateWebhookConfiguration configuration, CancellationToken cancellationToken);

        /// <summary>
        /// Remove and delete a webhook associated with a scaling policy.
        /// </summary>
        /// <param name="groupId">The ID of the scaling group. This is obtained from <see cref="ScalingGroup.Id">ScalingGroup.Id</see>.</param>
        /// <param name="policyId">The ID of the scaling policy. This is obtained from <see cref="Policy.Id">Policy.Id</see>.</param>
        /// <param name="webhookId">The ID of the webhook. This is obtained from <see cref="Webhook.Id">Webhook.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="groupId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="policyId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="webhookId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/DELETE_deleteWebhook_v1.0__tenantId__groups__groupId__policies__policyId__webhooks__webhookId__autoscale-webhooks.html">Delete webhook (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
        Task DeleteWebhookAsync(ScalingGroupId groupId, PolicyId policyId, WebhookId webhookId, CancellationToken cancellationToken);

        #endregion Webhooks
    }
}
