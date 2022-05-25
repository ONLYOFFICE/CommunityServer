namespace net.openstack.Core.Synchronous
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.Sockets;
    using net.openstack.Core.Collections;
    using net.openstack.Providers.Rackspace;
    using net.openstack.Providers.Rackspace.Objects.LoadBalancers;
    using CancellationToken = System.Threading.CancellationToken;

    /// <summary>
    /// Provides extension methods to allow synchronous calls to the methods in <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
    public static class LoadBalancerServiceExtensions
    {
        #region Load Balancers

        /// <summary>
        /// Gets a collection of current load balancers.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="markerId">The <see cref="LoadBalancer.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancer"/> objects describing the current load balancers.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancers-d1e1367.html">List Load Balancers (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<LoadBalancer> ListLoadBalancers(this ILoadBalancerService service, LoadBalancerId markerId, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListLoadBalancersAsync(markerId, limit, CancellationToken.None).Result;
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
        /// Gets detailed information about a specific load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A <see cref="LoadBalancer"/> object containing detailed information about the specified load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancer_Details-d1e1522.html">List Load Balancer Details (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LoadBalancer GetLoadBalancer(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetLoadBalancerAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Creates a new load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="configuration">The configuration for the new load balancer.</param>
        /// <returns>
        /// A <see cref="LoadBalancer"/> object describing the new load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancer_Details-d1e1522.html">List Load Balancer Details (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LoadBalancer CreateLoadBalancer(this ILoadBalancerService service, LoadBalancerConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateLoadBalancerAsync(configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Updates attributes for an existing load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="configuration">The updated configuration for the load balancer.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Update_Load_Balancer_Attributes-d1e1812.html">Update Load Balancer Attributes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateLoadBalancer(this ILoadBalancerService service, LoadBalancerId loadBalancerId, LoadBalancerUpdate configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateLoadBalancerAsync(loadBalancerId, configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Removes a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Load_Balancer-d1e2093.html">Remove Load Balancer (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveLoadBalancer(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveLoadBalancerAsync(loadBalancerId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Removes one or more load balancers.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerIds">The IDs of load balancers to remove. These is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerIds"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="loadBalancerIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Load_Balancer-d1e2093.html">Remove Load Balancer (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveLoadBalancerRange(this ILoadBalancerService service, IEnumerable<LoadBalancerId> loadBalancerIds)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveLoadBalancerRangeAsync(loadBalancerIds, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Load Balancers

        #region Error Page

        /// <summary>
        /// Gets the HTML content of the page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// The HTML content of the error page which is shown to an end user who is attempting to access a load balancer
        /// node that is offline or unavailable.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Errorpage-d1e2218.html">Error Page Operations (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static string GetErrorPage(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetErrorPageAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Sets the HTML content of the custom error page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="content">The HTML content of the error page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="content"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="content"/> is empty.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Errorpage-d1e2218.html">Error Page Operations (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetErrorPage(this ILoadBalancerService service, LoadBalancerId loadBalancerId, string content)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetErrorPageAsync(loadBalancerId, content, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Removes the custom error page which is shown to an end user who is attempting to access a load balancer node that is offline or unavailable.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Errorpage-d1e2218.html">Error Page Operations (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveErrorPage(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveErrorPageAsync(loadBalancerId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Error Page

        #region Load Balancer Statistics

        /// <summary>
        /// Get detailed statistics for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A <see cref="LoadBalancerStatistics"/> object containing the detailed statistics for the
        /// load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancer_Stats-d1e1524.html">List Load Balancer Stats (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LoadBalancerStatistics GetStatistics(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetStatisticsAsync(loadBalancerId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Load Balancer Statistics

        #region Nodes

        /// <summary>
        /// List the load balancer nodes associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="Node"/> objects describing the load balancer nodes associated with the specified
        /// load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Nodes-d1e2218.html">List Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<Node> ListNodes(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListNodesAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Get detailed information about a load balancer node.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <returns>
        /// A <see cref="Node"/> object describing the specified load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Nodes-d1e2218.html">List Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Node GetNode(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetNodeAsync(loadBalancerId, nodeId, CancellationToken.None).Result;
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
        /// Add a node to a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeConfiguration">A <see cref="NodeConfiguration"/> object describing the load balancer node to add.</param>
        /// <returns>
        /// A <see cref="Node"/> object describing the new load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeConfiguration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Nodes-d1e2379.html">Add Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Node AddNode(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeConfiguration nodeConfiguration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.AddNodeAsync(loadBalancerId, nodeConfiguration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Add one or more nodes to a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeConfigurations">A collection of <see cref="NodeConfiguration"/> objects describing the load balancer nodes to add.</param>
        /// <returns>
        /// A collection of <see cref="Node"/> objects describing the new load balancer nodes.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeConfigurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="nodeConfigurations"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Nodes-d1e2379.html">Add Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<Node> AddNodeRange(this ILoadBalancerService service, LoadBalancerId loadBalancerId, IEnumerable<NodeConfiguration> nodeConfigurations)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.AddNodeRangeAsync(loadBalancerId, nodeConfigurations, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Update the configuration of a load balancer node.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node IDs. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="configuration">The updated configuration for the load balancer node.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Modify_Nodes-d1e2503.html">Modify Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateNode(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId, NodeUpdate configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateNodeAsync(loadBalancerId, nodeId, configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Remove a nodes from a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node IDs. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Nodes-d1e2675.html">Remove Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveNode(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveNodeAsync(loadBalancerId, nodeId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Remove one or more nodes from a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeIds">The load balancer node IDs of nodes to remove. These are obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="nodeIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Nodes-d1e2675.html">Remove Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveNodeRange(this ILoadBalancerService service, LoadBalancerId loadBalancerId, IEnumerable<NodeId> nodeIds)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveNodeRangeAsync(loadBalancerId, nodeIds, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// List the service events for a load balancer node.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="markerId">The <see cref="NodeServiceEvent.Id"/> of the last item in the previous list. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Paginated_Collections-d1e786.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A collection of <see cref="NodeServiceEvent"/> objects describing the service events for the load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Node-Events-d1e264.html">View Node Service Events (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<NodeServiceEvent> ListNodeServiceEvents(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeServiceEventId markerId, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListNodeServiceEventsAsync(loadBalancerId, markerId, limit, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Nodes

        #region Virtual IPs

        /// <summary>
        /// Get a list of all virtual addresses associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerVirtualAddress"/> objects describing the virtual addresses
        /// associated with the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Virtual_IPs-d1e2809.html">List Virtual IPs (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerVirtualAddress> ListVirtualAddresses(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListVirtualAddressesAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Add a virtual address to a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="type">The virtual address type.</param>
        /// <param name="addressFamily">The family of address to add. This should be <see cref="AddressFamily.InterNetwork"/> or <see cref="AddressFamily.InterNetworkV6"/>.</param>
        /// <returns>
        /// A <see cref="LoadBalancerVirtualAddress"/> object describing the added virtual address.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
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
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LoadBalancerVirtualAddress AddVirtualAddress(this ILoadBalancerService service, LoadBalancerId loadBalancerId, LoadBalancerVirtualAddressType type, AddressFamily addressFamily)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.AddVirtualAddressAsync(loadBalancerId, type, addressFamily, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Remove a virtual address associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="virtualAddressId">The virtual address ID. This is obtained from <see cref="LoadBalancerVirtualAddress.Id">LoadBalancerVirtualAddress.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddressId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Virtual_IP-d1e2919.html">Remove Virtual IP (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveVirtualAddress(this ILoadBalancerService service, LoadBalancerId loadBalancerId, VirtualAddressId virtualAddressId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveVirtualAddressAsync(loadBalancerId, virtualAddressId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Remove a collection of virtual addresses associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="virtualAddressIds">The virtual address IDs. These are obtained from <see cref="LoadBalancerVirtualAddress.Id">LoadBalancerVirtualAddress.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddressIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="virtualAddressIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Remove_Virtual_IP-d1e2919.html">Remove Virtual IP (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveVirtualAddressRange(this ILoadBalancerService service, LoadBalancerId loadBalancerId, IEnumerable<VirtualAddressId> virtualAddressIds)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveVirtualAddressRangeAsync(loadBalancerId, virtualAddressIds, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Virtual IPs

        #region Allowed Domains

        /// <summary>
        /// Gets the domain name restrictions in place for adding load balancer nodes.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <returns>
        /// A collection of strings containing the allowed domain names used for adding load balancer nodes.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Node-Events-d1e264.html">View Node Service Events (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<string> ListAllowedDomains(this ILoadBalancerService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAllowedDomainsAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Allowed Domains

        #region Usage Reports

        /// <summary>
        /// List billable load balancers for a given date range.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="startTime">The start date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage prior to the specified <paramref name="endTime"/>.</param>
        /// <param name="endTime">The end date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage following the specified <paramref name="startTime"/>.</param>
        /// <param name="offset">The index of the last item in the previous page of results. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Gets the maximum number of load balancers to return in a single page of results. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancer"/> objects describing the load balancers active in the specified
        /// date range.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="endTime"/> occurs before <paramref name="startTime"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than or equal to 0.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<LoadBalancer> ListBillableLoadBalancers(this ILoadBalancerService service, DateTimeOffset? startTime, DateTimeOffset? endTime, int? offset, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListBillableLoadBalancersAsync(startTime, endTime, offset, limit, CancellationToken.None).Result;
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
        /// List all usage for an account during a specified date range.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="startTime">The start date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage prior to the specified <paramref name="endTime"/>.</param>
        /// <param name="endTime">The end date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage following the specified <paramref name="startTime"/>.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerUsage"/> objects describing the load balancer usage for an account
        /// in the specified date range.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="endTime"/> occurs before <paramref name="startTime"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerUsage> ListAccountLevelUsage(this ILoadBalancerService service, DateTimeOffset? startTime, DateTimeOffset? endTime)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAccountLevelUsageAsync(startTime, endTime, CancellationToken.None).Result;
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
        /// List all usage for a specific load balancer during a specified date range.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="startTime">The start date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage prior to the specified <paramref name="endTime"/>.</param>
        /// <param name="endTime">The end date to consider. The time component, if any, is ignored. If the value is <see langword="null"/>, the result includes all usage following the specified <paramref name="startTime"/>.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerUsage"/> objects describing the usage for the load balancer in
        /// the specified date range.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="endTime"/> occurs before <paramref name="startTime"/>.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerUsage> ListHistoricalUsage(this ILoadBalancerService service, LoadBalancerId loadBalancerId, DateTimeOffset? startTime, DateTimeOffset? endTime)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListHistoricalUsageAsync(loadBalancerId, startTime, endTime, CancellationToken.None).Result;
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
        /// List all usage for a specific load balancer during a preceding 24 hours.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerUsage"/> objects describing the usage for the load balancer in
        /// the preceding 24 hours.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerUsage> ListCurrentUsage(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListCurrentUsageAsync(loadBalancerId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Usage Reports

        #region Access Lists

        /// <summary>
        /// Gets the access list configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="NetworkItem"/> objects describing the access list configuration for the load
        /// balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<NetworkItem> ListAccessList(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAccessListAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Add a network item to the access list for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItem">A <see cref="NetworkItem"/> object describing the network item to add to the load balancer's access list.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItem"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void CreateAccessList(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NetworkItem networkItem)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.CreateAccessListAsync(loadBalancerId, networkItem, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Add a collection of network items to the access list for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItems">A collection of <see cref="NetworkItem"/> objects describing the network items to add to the load balancer's access list.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItems"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="networkItems"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void CreateAccessList(this ILoadBalancerService service, LoadBalancerId loadBalancerId, IEnumerable<NetworkItem> networkItems)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.CreateAccessListAsync(loadBalancerId, networkItems, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Remove a network item from the access list of a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItemId">The network item ID. This is obtained from <see cref="NetworkItem.Id"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItemId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveAccessList(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NetworkItemId networkItemId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveAccessListAsync(loadBalancerId, networkItemId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Remove a collection of network items from the access list of a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="networkItemIds">The network item IDs. These are obtained from <see cref="NetworkItem.Id"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkItemIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="networkItemIds"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveAccessListRange(this ILoadBalancerService service, LoadBalancerId loadBalancerId, IEnumerable<NetworkItemId> networkItemIds)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveAccessListRangeAsync(loadBalancerId, networkItemIds, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Remove all network items from the access list of a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Access_Lists-d1e3187.html">Manage Access Lists (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void ClearAccessList(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.ClearAccessListAsync(loadBalancerId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Access Lists

        #region Monitors

        /// <summary>
        /// Gets the health monitor currently configured for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A <see cref="HealthMonitor"/> object describing the health monitor configured for the
        /// load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Monitor_Health-d1e3434.html">Monitor Health (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HealthMonitor GetHealthMonitor(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetHealthMonitorAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Sets the health monitor configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="monitor">The updated health monitor configuration.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="monitor"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Monitor_Health-d1e3434.html">Monitor Health (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetHealthMonitor(this ILoadBalancerService service, LoadBalancerId loadBalancerId, HealthMonitor monitor)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetHealthMonitorAsync(loadBalancerId, monitor, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Remove the health monitor currently configured for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Monitor_Health-d1e3434.html">Monitor Health (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveHealthMonitor(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveHealthMonitorAsync(loadBalancerId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Monitors

        #region Sessions

        /// <summary>
        /// Gets the session persistence configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A <see cref="SessionPersistence"/> object describing the session persistence configuration for the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Session_Persistence-d1e3733.html">Manage Session Persistence (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static SessionPersistence GetSessionPersistence(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetSessionPersistenceAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="sessionPersistence">The session persistence configuration.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="sessionPersistence"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Session_Persistence-d1e3733.html">Manage Session Persistence (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetSessionPersistence(this ILoadBalancerService service, LoadBalancerId loadBalancerId, SessionPersistence sessionPersistence)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetSessionPersistenceAsync(loadBalancerId, sessionPersistence, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Removes the session persistence configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Manage_Session_Persistence-d1e3733.html">Manage Session Persistence (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveSessionPersistence(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveSessionPersistenceAsync(loadBalancerId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Sessions

        #region Connections

        /// <summary>
        /// Gets whether or not connection logging is enabled for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// <see langword="true"/> if content caching is enabled for the load balancer; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Log_Connections-d1e3924.html">Log Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static bool GetConnectionLogging(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetConnectionLoggingAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Enables or disables connection logging for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="enabled"><see langword="true"/> to enable connection logging on the load balancer; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Log_Connections-d1e3924.html">Log Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetConnectionLogging(this ILoadBalancerService service, LoadBalancerId loadBalancerId, bool enabled)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetConnectionLoggingAsync(loadBalancerId, enabled, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Gets the connection throttling configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A <see cref="ConnectionThrottles"/> object describing the connection throttling configuration in effect on the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Throttle_Connections-d1e4057.html">Throttle Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ConnectionThrottles ListThrottles(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListThrottlesAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Updates the connection throttling configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="throttleConfiguration">A <see cref="ConnectionThrottles"/> object describing the throttling configuration to apply for the load balancer.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="throttleConfiguration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Throttle_Connections-d1e4057.html">Throttle Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateThrottles(this ILoadBalancerService service, LoadBalancerId loadBalancerId, ConnectionThrottles throttleConfiguration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateThrottlesAsync(loadBalancerId, throttleConfiguration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Removes the connection throttling configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Throttle_Connections-d1e4057.html">Throttle Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveThrottles(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveThrottlesAsync(loadBalancerId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

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
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// <see langword="true"/> if content caching is enabled for the load balancer; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/ContentCaching-d1e3358.html">Content Caching (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static bool GetContentCaching(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetContentCachingAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="enabled"><see langword="true"/> to enable content caching on the load balancer; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/ContentCaching-d1e3358.html">Content Caching (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetContentCaching(this ILoadBalancerService service, LoadBalancerId loadBalancerId, bool enabled)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetContentCachingAsync(loadBalancerId, enabled, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Content Caching

        #region Protocols

        /// <summary>
        /// Gets a collection of supported load balancing protocols.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancingProtocol"/> objects describing the load balancing
        /// protocols supported by this service.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancing_Protocols-d1e4269.html">List Load Balancing Protocols (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancingProtocol> ListProtocols(this ILoadBalancerService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListProtocolsAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Protocols

        #region Algorithms

        /// <summary>
        /// Gets a collection of supported load balancing algorithms.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancingAlgorithm"/> objects describing the load balancing
        /// algorithms supported by this service.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancing_Algorithms-d1e4459.html">List Load Balancing Algorithms (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancingAlgorithm> ListAlgorithms(this ILoadBalancerService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAlgorithmsAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Algorithms

        #region SSL Termination

        /// <summary>
        /// Gets the SSL termination configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A <see cref="LoadBalancerSslConfiguration"/> object describing the SSL termination
        /// configuration for the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/SSLTermination-d1e2479.html#d6e3823">SSL Termination (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LoadBalancerSslConfiguration GetSslConfiguration(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetSslConfigurationAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Update the SSL termination configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="configuration">The updated SSL termination configuration.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/SSLTermination-d1e2479.html#d6e3823">SSL Termination (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateSslConfiguration(this ILoadBalancerService service, LoadBalancerId loadBalancerId, LoadBalancerSslConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateSslConfigurationAsync(loadBalancerId, configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Update the SSL termination configuration for a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/SSLTermination-d1e2479.html#d6e3823">SSL Termination (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveSslConfiguration(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveSslConfigurationAsync(loadBalancerId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion SSL Termination

        #region Metadata

        /// <summary>
        /// Gets the metadata associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerMetadataItem"/> objects describing the metadata
        /// associated with a load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerMetadataItem> ListLoadBalancerMetadata(this ILoadBalancerService service, LoadBalancerId loadBalancerId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListLoadBalancerMetadataAsync(loadBalancerId, CancellationToken.None).Result;
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
        /// Gets a specific metadata item associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <returns>
        /// A <see cref="LoadBalancerMetadataItem"/> object describing the metadata item.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LoadBalancerMetadataItem GetLoadBalancerMetadataItem(this ILoadBalancerService service, LoadBalancerId loadBalancerId, MetadataId metadataId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetLoadBalancerMetadataItemAsync(loadBalancerId, metadataId, CancellationToken.None).Result;
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
        /// Gets the metadata associated with a load balancer node.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerMetadataItem"/> objects describing the metadata
        /// associated with the load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerMetadataItem> ListNodeMetadata(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListNodeMetadataAsync(loadBalancerId, nodeId, CancellationToken.None).Result;
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
        /// Gets a specific metadata item associated with a load balancer node.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <returns>
        /// A <see cref="LoadBalancerMetadataItem"/> object describing the metadata item.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodeId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static LoadBalancerMetadataItem GetNodeMetadataItem(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId, MetadataId metadataId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetNodeMetadataItemAsync(loadBalancerId, nodeId, metadataId, CancellationToken.None).Result;
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
        /// Updates the metadata associated with a load balancer.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// The behavior is unspecified if <paramref name="metadata"/> contains a pair whose key matches the name of an existing metadata item associated with the load balancer.
        /// </note>
        /// </remarks>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadata">A collection of metadata items to associate with the load balancer.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerMetadataItem"/> objects describing the updated
        /// metadata associated with the load balancer.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
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
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerMetadataItem> AddLoadBalancerMetadata(this ILoadBalancerService service, LoadBalancerId loadBalancerId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.AddLoadBalancerMetadataAsync(loadBalancerId, metadata, CancellationToken.None).Result;
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
        /// Updates the metadata associated with a load balancer node.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// The behavior is unspecified if <paramref name="metadata"/> contains a pair whose key matches the name of an existing metadata item associated with the node.
        /// </note>
        /// </remarks>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadata">A collection of metadata items to associate with the node.</param>
        /// <returns>
        /// A collection of <see cref="LoadBalancerMetadataItem"/> objects describing the updated
        /// metadata associated with the load balancer node.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
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
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LoadBalancerMetadataItem> AddNodeMetadata(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.AddNodeMetadataAsync(loadBalancerId, nodeId, metadata, CancellationToken.None).Result;
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
        /// Sets the value for a metadata item associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="loadBalancerId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadataId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Modify_Metadata-d1e2503.html">Modify Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateLoadBalancerMetadataItem(this ILoadBalancerService service, LoadBalancerId loadBalancerId, MetadataId metadataId, string value)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateLoadBalancerMetadataItemAsync(loadBalancerId, metadataId, value, CancellationToken.None).Wait();
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
        /// Sets the value for a metadata item associated with a load balancer node.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadataId">The metadata item ID. This is obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
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
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateNodeMetadataItem(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId, MetadataId metadataId, string value)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateNodeMetadataItemAsync(loadBalancerId, nodeId, metadataId, value, CancellationToken.None).Wait();
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
        /// Removes one or more metadata items associated with a load balancer.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="metadataIds">The metadata item IDs. These are obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
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
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveLoadBalancerMetadataItem(this ILoadBalancerService service, LoadBalancerId loadBalancerId, IEnumerable<MetadataId> metadataIds)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveLoadBalancerMetadataItemAsync(loadBalancerId, metadataIds, CancellationToken.None).Wait();
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
        /// Removes one or more metadata items associated with a load balancer node.
        /// </summary>
        /// <param name="service">The load balancer service instance.</param>
        /// <param name="loadBalancerId">The load balancer ID. This is obtained from <see cref="LoadBalancer.Id">LoadBalancer.Id</see>.</param>
        /// <param name="nodeId">The load balancer node ID. This is obtained from <see cref="Node.Id">Node.Id</see>.</param>
        /// <param name="metadataIds">The metadata item IDs. These are obtained from <see cref="LoadBalancerMetadataItem.Id">LoadBalancerMetadataItem.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
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
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveNodeMetadataItem(this ILoadBalancerService service, LoadBalancerId loadBalancerId, NodeId nodeId, IEnumerable<MetadataId> metadataIds)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveNodeMetadataItemAsync(loadBalancerId, nodeId, metadataIds, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Metadata
    }
}
