namespace net.openstack.Providers.Rackspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using net.openstack.Providers.Rackspace.Objects.LoadBalancers;

    /// <summary>
    /// Represents a provider for the Rackspace Cloud Load Balancers service.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Overview-d1e82.html">Rackspace Cloud Load Balancers Developer Guide - API v1.0</seealso>
    /// <preliminary/>
    public interface ILoadBalancerService
    {
        #region Load Balancers

        /// <summary>
        /// Gets a collection of current load balancers.
        /// </summary>
        /// <param name="markerId">The <see cref="LoadBalancer.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="LoadBalancer"/> objects describing the current load balancers.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancers-d1e1367.html">List Load Balancers (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollectionPage<LoadBalancer>> ListLoadBalancersAsync(LoadBalancerId markerId, int? limit, CancellationToken cancellationToken);

        /// <summary>
        /// Gets detailed information about a specific load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="LoadBalancer"/>
        /// object containing detailed information about the specified load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancer_Details-d1e1522.html">List Load Balancer Details (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<LoadBalancer> GetLoadBalancerAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new load balancer.
        /// </summary>
        /// <param name="configuration">The configuration for the new load balancer.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully,
        /// the <see cref="Task{TResult}.Result"/> property will return a <see cref="LoadBalancer"/> object
        /// describing the new load balancer. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.Build"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancer_Details-d1e1522.html">List Load Balancer Details (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<LoadBalancer> CreateLoadBalancerAsync(LoadBalancerConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Updates attributes for an existing load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="configuration">The updated configuration for the load balancer.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Update_Load_Balancer_Attributes-d1e1812.html">Update Load Balancer Attributes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task UpdateLoadBalancerAsync(LoadBalancerId loadBalancerId, LoadBalancerUpdate configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Removes a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If
        /// <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>,
        /// the task will not be considered complete until the load balancer transitions
        /// out of the <see cref="LoadBalancerStatus.PendingDelete"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Load_Balancer-d1e2093.html">Remove Load Balancer (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveLoadBalancerAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Removes one or more load balancers.
        /// </summary>
        /// <param name="loadBalancerIds">The IDs of load balancers to remove. These is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If
        /// <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>,
        /// the task will not be considered complete until all of the load balancers
        /// transition out of the <see cref="LoadBalancerStatus.PendingDelete"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerIds"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="loadBalancerIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Load_Balancer-d1e2093.html">Remove Load Balancer (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveLoadBalancerRangeAsync(IEnumerable<LoadBalancerId> loadBalancerIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer[]> progress);

        #endregion Load Balancers

        #region Error Page

        /// <summary>
        /// Gets the HTML content of the page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain the HTML content
        /// of the error page which is shown to an end user who is attempting to access a load balancer
        /// node that is offline or unavailable.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Errorpage-d1e2218.html">Error Page Operations (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<string> GetErrorPageAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the HTML content of the custom error page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="content">The HTML content of the error page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="content"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="content"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Errorpage-d1e2218.html">Error Page Operations (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task SetErrorPageAsync(LoadBalancerId loadBalancerId, string content, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Removes the custom error page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Errorpage-d1e2218.html">Error Page Operations (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveErrorPageAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion Error Page

        #region Load Balancer Statistics

        /// <summary>
        /// Get detailed statistics for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="LoadBalancerStatistics"/> object containing the detailed statistics for the
        /// load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancer_Stats-d1e1524.html">List Load Balancer Stats (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<LoadBalancerStatistics> GetStatisticsAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        #endregion Load Balancer Statistics

        #region Nodes

        /// <summary>
        /// List the load balancer nodes associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="Node"/> objects describing the load balancer nodes associated with the specified
        /// load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Nodes-d1e2218.html">List Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<Node>> ListNodesAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Get detailed information about a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="Node"/>
        /// object describing the specified load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Nodes-d1e2218.html">List Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<Node> GetNodeAsync(LoadBalancerId loadBalancerId, NodeId nodeId, CancellationToken cancellationToken);

        /// <summary>
        /// Add a node to a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeConfiguration">A <see cref="NodeConfiguration"/> object describing the load balancer node to add.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="Node"/>
        /// object describing the new load balancer node. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeConfiguration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Nodes-d1e2379.html">Add Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<Node> AddNodeAsync(LoadBalancerId loadBalancerId, NodeConfiguration nodeConfiguration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Add one or more nodes to a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeConfigurations">A collection of <see cref="NodeConfiguration"/> objects describing the load balancer nodes to add.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="Node"/> objects describing the new load balancer nodes. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeConfigurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="nodeConfigurations"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Nodes-d1e2379.html">Add Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<Node>> AddNodeRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<NodeConfiguration> nodeConfigurations, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Update the configuration of a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node IDs. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="configuration">The updated configuration for the load balancer node.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Modify_Nodes-d1e2503.html">Modify Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task UpdateNodeAsync(LoadBalancerId loadBalancerId, NodeId nodeId, NodeUpdate configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove a nodes from a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node IDs. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Nodes-d1e2675.html">Remove Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveNodeAsync(LoadBalancerId loadBalancerId, NodeId nodeId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove one or more nodes from a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeIds">The load balancer node IDs of nodes to remove. These are obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="nodeIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Nodes-d1e2675.html">Remove Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveNodeRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<NodeId> nodeIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// List the service events for a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="markerId">The <see cref="NodeServiceEvent.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="NodeServiceEvent"/> objects describing the service events for the load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Node-Events-d1e264.html">View Node Service Events (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollectionPage<NodeServiceEvent>> ListNodeServiceEventsAsync(LoadBalancerId loadBalancerId, NodeServiceEventId markerId, int? limit, CancellationToken cancellationToken);

        #endregion Nodes

        #region Virtual IPs

        /// <summary>
        /// Get a list of all virtual addresses associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="LoadBalancerVirtualAddress"/> objects describing the virtual addresses
        /// associated with the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Virtual_IPs-d1e2809.html">List Virtual IPs (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerVirtualAddress>> ListVirtualAddressesAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Add a virtual address to a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="type">The virtual address type.</param>
        /// <param name="addressFamily">The family of address to add. This should be <see cref="AddressFamily.InterNetwork"/> or <see cref="AddressFamily.InterNetworkV6"/>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="LoadBalancerVirtualAddress"/> object describing the added virtual address.
        /// If <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>,
        /// the task will not be considered complete until the load balancer transitions out of the
        /// <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="type"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the specified <paramref name="addressFamily"/> is not supported by this provider.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Virtual_IP_Version_6.html">Add Virtual IP Version 6 (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<LoadBalancerVirtualAddress> AddVirtualAddressAsync(LoadBalancerId loadBalancerId, LoadBalancerVirtualAddressType type, AddressFamily addressFamily, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove a virtual address associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="virtualAddressId">The virtual address ID. This is obtained from <see cref="LoadBalancerVirtualAddress.Id">LoadBalancerVirtualAddress.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddressId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Virtual_IP-d1e2919.html">Remove Virtual IP (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveVirtualAddressAsync(LoadBalancerId loadBalancerId, VirtualAddressId virtualAddressId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove a collection of virtual addresses associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="virtualAddressIds">The virtual address IDs. These are obtained from <see cref="LoadBalancerVirtualAddress.Id">LoadBalancerVirtualAddress.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddressIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="virtualAddressIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Virtual_IP-d1e2919.html">Remove Virtual IP (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveVirtualAddressRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<VirtualAddressId> virtualAddressIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion Virtual IPs

        #region Allowed Domains

        /// <summary>
        /// Gets the domain name restrictions in place for adding load balancer nodes.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// strings containing the allowed domain names used for adding load balancer nodes.
        /// </returns>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Node-Events-d1e264.html">View Node Service Events (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<string>> ListAllowedDomainsAsync(CancellationToken cancellationToken);

        #endregion Allowed Domains

        #region Usage Reports

        /// <summary>
        /// List billable load balancers for a given date range.
        /// </summary>
        /// <param name="startTime">The start date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage prior to the specified <paramref name="endTime"/>.</param>
        /// <param name="endTime">The end date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage following the specified <paramref name="startTime"/>.</param>
        /// <param name="offset">The index of the last item in the previous page of results. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Gets the maximum number of load balancers to return in a single page of results. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="LoadBalancer"/> objects describing the load balancers active in the specified
        /// date range.
        /// </returns>
        /// <exception cref="ArgumentException">If <paramref name="endTime"/> occurs before <paramref name="startTime"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than or equal to 0.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollectionPage<LoadBalancer>> ListBillableLoadBalancersAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, int? offset, int? limit, CancellationToken cancellationToken);

        /// <summary>
        /// List all usage for an account during a specified date range.
        /// </summary>
        /// <param name="startTime">The start date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage prior to the specified <paramref name="endTime"/>.</param>
        /// <param name="endTime">The end date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage following the specified <paramref name="startTime"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="LoadBalancerUsage"/> objects describing the load balancer usage for an account
        /// in the specified date range.
        /// </returns>
        /// <exception cref="ArgumentException">If <paramref name="endTime"/> occurs before <paramref name="startTime"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerUsage>> ListAccountLevelUsageAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken);

        /// <summary>
        /// List all usage for a specific load balancer during a specified date range.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="startTime">The start date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage prior to the specified <paramref name="endTime"/>.</param>
        /// <param name="endTime">The end date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage following the specified <paramref name="startTime"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="LoadBalancerUsage"/> objects describing the usage for the load balancer in
        /// the specified date range.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="endTime"/> occurs before <paramref name="startTime"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerUsage>> ListHistoricalUsageAsync(LoadBalancerId loadBalancerId, DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken);

        /// <summary>
        /// List all usage for a specific load balancer during a preceding 24 hours.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="LoadBalancerUsage"/> objects describing the usage for the load balancer in
        /// the preceding 24 hours.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerUsage>> ListCurrentUsageAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        #endregion Usage Reports

        #region Access Lists

        /// <summary>
        /// Gets the access list configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a collection of
        /// <see cref="NetworkItem"/> objects describing the access list configuration for the load
        /// balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<NetworkItem>> ListAccessListAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Add a network item to the access list for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItem">A <see cref="NetworkItem"/> object describing the network item to add to the load balancer's access list.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation.
        /// If <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>,
        /// the task will not be considered complete until the load balancer transitions out of the
        /// <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItem"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task CreateAccessListAsync(LoadBalancerId loadBalancerId, NetworkItem networkItem, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Add a collection of network items to the access list for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItems">A collection of <see cref="NetworkItem"/> objects describing the network items to add to the load balancer's access list.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItems"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="networkItems"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task CreateAccessListAsync(LoadBalancerId loadBalancerId, IEnumerable<NetworkItem> networkItems, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove a network item from the access list of a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItemId">The network item ID. This is obtained from <see cref="NetworkItem.Id"/>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItemId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveAccessListAsync(LoadBalancerId loadBalancerId, NetworkItemId networkItemId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove a collection of network items from the access list of a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItemIds">The network item IDs. These are obtained from <see cref="NetworkItem.Id"/>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItemIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="networkItemIds"/> contains any <see langword="null"/> values.
        /// <para>-or-</para>
        /// <para>If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveAccessListRangeAsync(LoadBalancerId loadBalancerId, IEnumerable<NetworkItemId> networkItemIds, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove all network items from the access list of a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task ClearAccessListAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion Access Lists

        #region Monitors

        /// <summary>
        /// Gets the health monitor currently configured for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="HealthMonitor"/> object describing the health monitor configured for the
        /// load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Monitor_Health-d1e3434.html">Monitor Health (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<HealthMonitor> GetHealthMonitorAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the health monitor configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="monitor">The updated health monitor configuration.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="monitor"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Monitor_Health-d1e3434.html">Monitor Health (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task SetHealthMonitorAsync(LoadBalancerId loadBalancerId, HealthMonitor monitor, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Remove the health monitor currently configured for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Monitor_Health-d1e3434.html">Monitor Health (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveHealthMonitorAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion Monitors

        #region Sessions

        /// <summary>
        /// Gets the session persistence configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="SessionPersistence"/>
        /// object describing the session persistence configuration for the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Session_Persistence-d1e3733.html">Manage Session Persistence (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<SessionPersistence> GetSessionPersistenceAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the session persistence configuration for a load balancer.
        /// </summary>
        /// <remarks>
        /// You can only set one of the session persistence modes on a load balancer, and it can only support one
        /// protocol, so if you set <see cref="SessionPersistenceType.HttpCookie"/> mode for an HTTP load balancer,
        /// then it will support session persistence for HTTP requests only. Likewise, if you set
        /// <see cref="SessionPersistenceType.SourceAddress"/> mode for an HTTPS load balancer, then it will support
        /// session persistence for HTTPS requests only.
        ///
        /// <para>
        /// If you want to support session persistence for both HTTP and HTTPS requests concurrently, then you have 2 choices:
        /// </para>
        ///
        /// <list type="bullet">
        /// <item>Use two load balancers, one configured for session persistence for HTTP requests and the other
        /// configured for session persistence for HTTPS requests. That way, the load balancers together will support
        /// session persistence for both HTTP and HTTPS requests concurrently, with each load balancer supporting one
        /// of the protocols.</item>
        /// <item>Use one load balancer, configure it for session persistence for HTTP requests, and then enable SSL
        /// termination for that load balancer (refer to Section 4.17, "SSL Termination" for details). The load
        /// balancer will then support session persistence for both HTTP and HTTPS requests concurrently.</item>
        /// </list>
        /// </remarks>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="sessionPersistence">The session persistence configuration.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="sessionPersistence"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Session_Persistence-d1e3733.html">Manage Session Persistence (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task SetSessionPersistenceAsync(LoadBalancerId loadBalancerId, SessionPersistence sessionPersistence, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Removes the session persistence configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Session_Persistence-d1e3733.html">Manage Session Persistence (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveSessionPersistenceAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion Sessions

        #region Connections

        /// <summary>
        /// Gets whether or not connection logging is enabled for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will be <see langword="true"/> if content
        /// caching is enabled for the load balancer; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Log_Connections-d1e3924.html">Log Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<bool> GetConnectionLoggingAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Enables or disables connection logging for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="enabled"><see langword="true"/> to enable connection logging on the load balancer; otherwise, <see langword="false"/>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Log_Connections-d1e3924.html">Log Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task SetConnectionLoggingAsync(LoadBalancerId loadBalancerId, bool enabled, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Gets the connection throttling configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="ConnectionThrottles"/>
        /// object describing the connection throttling configuration in effect on the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Throttle_Connections-d1e4057.html">Throttle Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ConnectionThrottles> ListThrottlesAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the connection throttling configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="throttleConfiguration">A <see cref="ConnectionThrottles"/> object describing the throttling configuration to apply for the load balancer.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="throttleConfiguration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Throttle_Connections-d1e4057.html">Throttle Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task UpdateThrottlesAsync(LoadBalancerId loadBalancerId, ConnectionThrottles throttleConfiguration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Removes the connection throttling configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Throttle_Connections-d1e4057.html">Throttle Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveThrottlesAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion Connections

        #region Content Caching

        /// <summary>
        /// Gets whether or not content caching is enabled for a load balancer.
        /// </summary>
        /// <remarks>
        /// When content caching is enabled, recently-accessed files are stored on the load balancer
        /// for easy retrieval by web clients. Content caching improves the performance of high
        /// traffic web sites by temporarily storing data that was recently accessed. While it's
        /// cached, requests for that data will be served by the load balancer, which in turn reduces
        /// load off the back end nodes. The result is improved response times for those requests and
        /// less load on the web server.
        ///
        /// <para>
        /// For more information about content caching, refer to the following Knowledge Center
        /// article:
        /// <see href="http://www.rackspace.com/knowledge_center/content/content-caching-cloud-load-balancers">Content Caching for Cloud Load Balancers</see>.
        /// </para>
        /// </remarks>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will be <see langword="true"/> if content
        /// caching is enabled for the load balancer; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/ContentCaching-d1e3358.html">Content Caching (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<bool> GetContentCachingAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Enables or disables content caching for a load balancer.
        /// </summary>
        /// <remarks>
        /// When content caching is enabled, recently-accessed files are stored on the load balancer
        /// for easy retrieval by web clients. Content caching improves the performance of high
        /// traffic web sites by temporarily storing data that was recently accessed. While it's
        /// cached, requests for that data will be served by the load balancer, which in turn reduces
        /// load off the back end nodes. The result is improved response times for those requests and
        /// less load on the web server.
        ///
        /// <para>
        /// For more information about content caching, refer to the following Knowledge Center
        /// article:
        /// <see href="http://www.rackspace.com/knowledge_center/content/content-caching-cloud-load-balancers">Content Caching for Cloud Load Balancers</see>.
        /// </para>
        /// </remarks>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="enabled"><see langword="true"/> to enable content caching on the load balancer; otherwise, <see langword="false"/>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/ContentCaching-d1e3358.html">Content Caching (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task SetContentCachingAsync(LoadBalancerId loadBalancerId, bool enabled, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion Content Caching

        #region Protocols

        /// <summary>
        /// Gets a collection of supported load balancing protocols.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a collection of <see cref="LoadBalancingProtocol"/> objects describing the load balancing
        /// protocols supported by this service.
        /// </returns>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancing_Protocols-d1e4269.html">List Load Balancing Protocols (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancingProtocol>> ListProtocolsAsync(CancellationToken cancellationToken);

        #endregion Protocols

        #region Algorithms

        /// <summary>
        /// Gets a collection of supported load balancing algorithms.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a collection of <see cref="LoadBalancingAlgorithm"/> objects describing the load balancing
        /// algorithms supported by this service.
        /// </returns>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancing_Algorithms-d1e4459.html">List Load Balancing Algorithms (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancingAlgorithm>> ListAlgorithmsAsync(CancellationToken cancellationToken);

        #endregion Algorithms

        #region SSL Termination

        /// <summary>
        /// Gets the SSL termination configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="LoadBalancerSslConfiguration"/> object describing the SSL termination
        /// configuration for the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/SSLTermination-d1e2479.html#d6e3823">SSL Termination (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<LoadBalancerSslConfiguration> GetSslConfigurationAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Update the SSL termination configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="configuration">The updated SSL termination configuration.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/SSLTermination-d1e2479.html#d6e3823">SSL Termination (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task UpdateSslConfigurationAsync(LoadBalancerId loadBalancerId, LoadBalancerSslConfiguration configuration, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        /// <summary>
        /// Update the SSL termination configuration for a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="completionOption">Specifies when the <see cref="Task"/> representing the asynchronous server operation should be considered complete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="progress">An optional callback object to receive progress notifications, if <paramref name="completionOption"/> is <see cref="AsyncCompletionOption.RequestCompleted"/>. If this is <see langword="null"/>, no progress notifications are sent.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If <paramref name="completionOption"/> is
        /// <see cref="AsyncCompletionOption.RequestCompleted"/>, the task will not be considered complete until
        /// the load balancer transitions out of the <see cref="LoadBalancerStatus.PendingUpdate"/> state.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="completionOption"/> is not a valid <see cref="AsyncCompletionOption"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/SSLTermination-d1e2479.html#d6e3823">SSL Termination (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveSslConfigurationAsync(LoadBalancerId loadBalancerId, AsyncCompletionOption completionOption, CancellationToken cancellationToken, IProgress<LoadBalancer> progress);

        #endregion SSL Termination

        #region Metadata

        /// <summary>
        /// Gets the metadata associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a collection of <see cref="LoadBalancerMetadataItem"/> objects describing the metadata
        /// associated with a load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerMetadataItem>> ListLoadBalancerMetadataAsync(LoadBalancerId loadBalancerId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific metadata item associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a <see cref="LoadBalancerMetadataItem"/> object describing the metadata item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<LoadBalancerMetadataItem> GetLoadBalancerMetadataItemAsync(LoadBalancerId loadBalancerId, MetadataId metadataId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the metadata associated with a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a collection of <see cref="LoadBalancerMetadataItem"/> objects describing the metadata
        /// associated with the load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerMetadataItem>> ListNodeMetadataAsync(LoadBalancerId loadBalancerId, NodeId nodeId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific metadata item associated with a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a <see cref="LoadBalancerMetadataItem"/> object describing the metadata item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<LoadBalancerMetadataItem> GetNodeMetadataItemAsync(LoadBalancerId loadBalancerId, NodeId nodeId, MetadataId metadataId, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the metadata associated with a load balancer.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// The behavior is unspecified if <paramref name="metadata"/> contains a pair whose key matches the name of an existing metadata item associated with the load balancer.
        /// </note>
        /// </remarks>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadata">A collection of metadata items to associate with the load balancer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a collection of <see cref="LoadBalancerMetadataItem"/> objects describing the updated
        /// metadata associated with the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadata"/> contains a pair whose <see cref="KeyValuePair{TKey, TValue}.Key"/> is <see langword="null"/> or empty, or whose <see cref="KeyValuePair{TKey, TValue}.Value"/> is is <see langword="null"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Metadata-d1e2379.html">Add Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerMetadataItem>> AddLoadBalancerMetadataAsync(LoadBalancerId loadBalancerId, IEnumerable<KeyValuePair<string, string>> metadata, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the metadata associated with a load balancer node.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// The behavior is unspecified if <paramref name="metadata"/> contains a pair whose key matches the name of an existing metadata item associated with the node.
        /// </note>
        /// </remarks>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadata">A collection of metadata items to associate with the node.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the operation
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// a collection of <see cref="LoadBalancerMetadataItem"/> objects describing the updated
        /// metadata associated with the load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadata"/> contains a pair whose <see cref="KeyValuePair{TKey, TValue}.Key"/> is <see langword="null"/> or empty, or whose <see cref="KeyValuePair{TKey, TValue}.Value"/> is is <see langword="null"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Metadata-d1e2379.html">Add Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task<ReadOnlyCollection<LoadBalancerMetadataItem>> AddNodeMetadataAsync(LoadBalancerId loadBalancerId, NodeId nodeId, IEnumerable<KeyValuePair<string, string>> metadata, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the value for a metadata item associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Modify_Metadata-d1e2503.html">Modify Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task UpdateLoadBalancerMetadataItemAsync(LoadBalancerId loadBalancerId, MetadataId metadataId, string value, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the value for a metadata item associated with a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Modify_Metadata-d1e2503.html">Modify Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task UpdateNodeMetadataItemAsync(LoadBalancerId loadBalancerId, NodeId nodeId, MetadataId metadataId, string value, CancellationToken cancellationToken);

        /// <summary>
        /// Removes one or more metadata items associated with a load balancer.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadataIds">The metadata item IDs. These are obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadataIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Metadata-d1e2675.html">Remove Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveLoadBalancerMetadataItemAsync(LoadBalancerId loadBalancerId, IEnumerable<MetadataId> metadataIds, CancellationToken cancellationToken);

        /// <summary>
        /// Removes one or more metadata items associated with a load balancer node.
        /// </summary>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadataIds">The metadata item IDs. These are obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="metadataIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Metadata-d1e2675.html">Remove Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        Task RemoveNodeMetadataItemAsync(LoadBalancerId loadBalancerId, NodeId nodeId, IEnumerable<MetadataId> metadataIds, CancellationToken cancellationToken);

        #endregion Metadata
    }
}
