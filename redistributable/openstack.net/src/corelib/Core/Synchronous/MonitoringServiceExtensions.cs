namespace net.openstack.Core.Synchronous
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using net.openstack.Providers.Rackspace;
    using net.openstack.Providers.Rackspace.Objects.Monitoring;
    using Newtonsoft.Json.Linq;
    using CancellationToken = System.Threading.CancellationToken;

    /// <summary>
    /// Provides extension methods to allow synchronous calls to the methods in <see cref="IMonitoringService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
    public static class MonitoringServiceExtensions
    {
        #region Core

        #region Account

        /// <summary>
        /// Gets information about a monitoring account.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <returns>A <see cref="MonitoringAccount"/> object describing the account.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-account.html#service-account-root">Get Account (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static MonitoringAccount GetAccount(this IMonitoringService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAccountAsync(CancellationToken.None).Result;
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
        /// Updates a monitoring account.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="accountId">The account ID. This is obtained from <see cref="MonitoringAccount.Id">MonitoringAccount.Id</see>.</param>
        /// <param name="configuration">An <see cref="AccountConfiguration"/> object describing the changes to apply to the account.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="accountId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-account.html#service-account-put-account">Update Account (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateAccount(this IMonitoringService service, MonitoringAccountId accountId, AccountConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateAccountAsync(accountId, configuration, CancellationToken.None).Wait();
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
        /// Gets the resource and rate limits enforced by the monitoring service.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <returns>
        /// A <see cref="MonitoringLimits"/> object describing the resource and rate
        /// limits of the monitoring service.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-account.html#service-account-get-limits">Get Limits (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static MonitoringLimits GetLimits(this IMonitoringService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetLimitsAsync(CancellationToken.None).Result;
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
        /// Gets a collection of monitoring audits.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="from">The beginning timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="to">The ending timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, the current time is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="to"/> occurs before <paramref name="from"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="limit"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="from"/> represents a date before January 1, 1970 UTC.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="to"/> represents a date before January 1, 1970 UTC.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-account.html#service-account-list-audits">List Audits (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">Time Series Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Audit, AuditId> ListAudits(this IMonitoringService service, AuditId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAuditsAsync(marker, limit, from, to, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Account

        #region Entities

        /// <summary>
        /// Creates a new monitoring entity.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="configuration">A <see cref="NewEntityConfiguration"/> object describing the new entity.</param>
        /// <returns>The <see cref="EntityId"/> identifying the new entity.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-entities.html#service-entities-create">Create Entities (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static EntityId CreateEntity(this IMonitoringService service, NewEntityConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateEntityAsync(configuration, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring entities.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-entities.html#service-entities-list">List Entities (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Entity, EntityId> ListEntities(this IMonitoringService service, EntityId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListEntitiesAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring entity by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <returns>An <see cref="Entity"/> object describing the entity.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="entityId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-entities.html#service-entities-get">Get Entity (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Entity GetEntity(this IMonitoringService service, EntityId entityId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetEntityAsync(entityId, CancellationToken.None).Result;
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
        /// Updates a monitoring entity.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="configuration">An <see cref="UpdateEntityConfiguration"/> object describing the changes to apply to the entity.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-entities.html#service-entities-update">Update Entity (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateEntity(this IMonitoringService service, EntityId entityId, UpdateEntityConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateEntityAsync(entityId, configuration, CancellationToken.None).Wait();
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
        /// Remove and delete a monitoring entity by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="entityId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-entities.html#service-entities-delete">Delete Entity (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveEntity(this IMonitoringService service, EntityId entityId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveEntityAsync(entityId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Entities

        #region Checks

        /// <summary>
        /// Creates a new check.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="configuration">A <see cref="NewCheckConfiguration"/> object describing the new check.</param>
        /// <returns>The <see cref="CheckId"/> identifying the new check.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-create">Create Check (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static CheckId CreateCheck(this IMonitoringService service, EntityId entityId, NewCheckConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateCheckAsync(entityId, configuration, CancellationToken.None).Result;
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
        /// Test a monitoring check.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="configuration">A <see cref="NewCheckConfiguration"/> object describing the check to test.</param>
        /// <param name="debug"><see langword="true"/> to include debug information in the result; otherwise, <see langword="false"/>. If the value is <see langword="null"/>, a provider-specific default is used.</param>
        /// <returns>A collection <see cref="CheckData"/> objects describing the test results.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-test">Test Check (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-test-debug">Test Check and Include Debug Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<CheckData> TestCheck(this IMonitoringService service, EntityId entityId, NewCheckConfiguration configuration, bool? debug)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.TestCheckAsync(entityId, configuration, debug, CancellationToken.None).Result;
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
        /// Test an existing check by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <returns>A collection <see cref="CheckData"/> objects describing the test results.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-test-existing">Test Existing Check (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<CheckData> TestExistingCheck(this IMonitoringService service, EntityId entityId, CheckId checkId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.TestExistingCheckAsync(entityId, checkId, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring checks.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="entityId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-list">List Checks (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Check, CheckId> ListChecks(this IMonitoringService service, EntityId entityId, CheckId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListChecksAsync(entityId, marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring check by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <returns>A <see cref="Check"/> object describing the check.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-get">Get Check (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Check GetCheck(this IMonitoringService service, EntityId entityId, CheckId checkId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetCheckAsync(entityId, checkId, CancellationToken.None).Result;
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
        /// Updates a monitoring check.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <param name="configuration">An <see cref="UpdateCheckConfiguration"/> object describing the changes to apply to the check.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-update">Update Check (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateCheck(this IMonitoringService service, EntityId entityId, CheckId checkId, UpdateCheckConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateCheckAsync(entityId, checkId, configuration, CancellationToken.None).Wait();
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
        /// Remove and delete a monitoring check by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-checks.html#service-checks-delete">Delete Check (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveCheck(this IMonitoringService service, EntityId entityId, CheckId checkId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveCheckAsync(entityId, checkId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Checks

        #region Check Types

        /// <summary>
        /// Gets a collection of monitoring check types.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-check-types.html#service-check-types-list">List Check Types (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<CheckType, CheckTypeId> ListCheckTypes(this IMonitoringService service, CheckTypeId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListCheckTypesAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring check type by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="checkTypeId">The check type ID. This is obtained from <see cref="CheckType.Id">CheckType.Id</see>, or from the predefined values in <see cref="CheckTypeId"/>.</param>
        /// <returns>A <see cref="CheckType"/> object describing the check type.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="checkTypeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-check-types.html#service-check-types-get">Get Check Type (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static CheckType GetCheckType(this IMonitoringService service, CheckTypeId checkTypeId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetCheckTypeAsync(checkTypeId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Check Types

        #region Metrics

        /// <summary>
        /// Gets a collection of monitoring metrics.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/metrics-api.html#list-metrics">List Metrics (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Metric, MetricName> ListMetrics(this IMonitoringService service, EntityId entityId, CheckId checkId, MetricName marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListMetricsAsync(entityId, checkId, marker, limit, CancellationToken.None).Result;
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
        /// Gets a collection of data points collected for a metric by the monitoring service.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <param name="metricName">The metric name. This is obtained from <see cref="Metric.Name">Metric.Name</see>.</param>
        /// <param name="points">The number of data points to return.</param>
        /// <param name="resolution">The granularity of the returned data points.</param>
        /// <param name="select">A collection of <see cref="DataPointStatistic"/> objects identifying the statistics to compute for the data.</param>
        /// <param name="from">The beginning timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>.</param>
        /// <param name="to">The ending timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>.</param>
        /// <returns>
        /// A collection of <see cref="DataPoint"/> objects describing the data points
        /// collected for the metric.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metricName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If both <paramref name="points"/> and <paramref name="resolution"/> are <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="to"/> occurs before <paramref name="from"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="points"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="from"/> represents a date before January 1, 1970 UTC.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="to"/> represents a date before January 1, 1970 UTC.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/metrics-api.html#fetch-data-points">Fetch Data Points (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<DataPoint> GetDataPoints(this IMonitoringService service, EntityId entityId, CheckId checkId, MetricName metricName, int? points, DataPointGranularity resolution, IEnumerable<DataPointStatistic> select, DateTimeOffset from, DateTimeOffset to)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetDataPointsAsync(entityId, checkId, metricName, points, resolution, select, from, to, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Metrics

        #region Alarms

        /// <summary>
        /// Creates a new alarm.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="configuration">A <see cref="NewAlarmConfiguration"/> object describing the new alarm.</param>
        /// <returns>The <see cref="AlarmId"/> identifying the new alarm.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarms.html#service-alarms-create">Create Alarm (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static AlarmId CreateAlarm(this IMonitoringService service, EntityId entityId, NewAlarmConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateAlarmAsync(entityId, configuration, CancellationToken.None).Result;
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
        /// Test a monitoring alarm.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="configuration">A <see cref="TestAlarmConfiguration"/> object describing the alarm test configuration.</param>
        /// <returns>A collection <see cref="AlarmData"/> objects describing the test results.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarms.html#service-alarms-test">Test Alarm (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<AlarmData> TestAlarm(this IMonitoringService service, EntityId entityId, TestAlarmConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.TestAlarmAsync(entityId, configuration, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring entities.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="entityId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarms.html#service-alarms-list">List Alarms (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Alarm, AlarmId> ListAlarms(this IMonitoringService service, EntityId entityId, AlarmId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAlarmsAsync(entityId, marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring alarm by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="alarmId">The alarm ID. This is obtained from <see cref="Alarm.Id">Alarm.Id</see>.</param>
        /// <returns>An <see cref="Alarm"/> object describing the alarm.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="alarmId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarms.html#service-alarms-get">Get Alarm (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Alarm GetAlarm(this IMonitoringService service, EntityId entityId, AlarmId alarmId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAlarmAsync(entityId, alarmId, CancellationToken.None).Result;
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
        /// Updates a monitoring alarm.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="alarmId">The alarm ID. This is obtained from <see cref="Alarm.Id">Alarm.Id</see>.</param>
        /// <param name="configuration">An <see cref="UpdateAlarmConfiguration"/> object describing the changes to apply to the alarm.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="alarmId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarms.html#service-alarms-update">Update Alarm (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateAlarm(this IMonitoringService service, EntityId entityId, AlarmId alarmId, UpdateAlarmConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateAlarmAsync(entityId, alarmId, configuration, CancellationToken.None).Wait();
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
        /// Remove and delete a monitoring alarm by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="alarmId">The alarm ID. This is obtained from <see cref="Alarm.Id">Alarm.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="alarmId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarms.html#service-alarms-delete">Remove Alarm (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveAlarm(this IMonitoringService service, EntityId entityId, AlarmId alarmId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveAlarmAsync(entityId, alarmId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Alarms

        #region Notification Plans

        /// <summary>
        /// Creates a new notification plan.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="configuration">A <see cref="NewNotificationPlanConfiguration"/> object describing the new notification plan.</param>
        /// <returns>The <see cref="NotificationPlanId"/> identifying the new notification plan.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-plans.html#service-notification-plans-create">Create Notification Plan (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static NotificationPlanId CreateNotificationPlan(this IMonitoringService service, NewNotificationPlanConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateNotificationPlanAsync(configuration, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring notification plans.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-plans.html#service-notification-plans-list">List Notification Plans (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<NotificationPlan, NotificationPlanId> ListNotificationPlans(this IMonitoringService service, NotificationPlanId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListNotificationPlansAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring notification plan by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationPlanId">The notification plan ID. This is obtained from <see cref="NotificationPlan.Id">NotificationPlan.Id</see>.</param>
        /// <returns>A <see cref="NotificationPlan"/> object describing the notification plan.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="notificationPlanId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-plans.html#service-notification-plans-get">Get Notification Plan (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static NotificationPlan GetNotificationPlan(this IMonitoringService service, NotificationPlanId notificationPlanId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetNotificationPlanAsync(notificationPlanId, CancellationToken.None).Result;
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
        /// Updates a monitoring notification plan.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationPlanId">The notification plan ID. This is obtained from <see cref="NotificationPlan.Id">NotificationPlan.Id</see>.</param>
        /// <param name="configuration">An <see cref="UpdateNotificationPlanConfiguration"/> object describing the changes to apply to the notification plan.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="notificationPlanId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-plans.html#service-notification-plans-update">Update Notification Plans (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateNotificationPlan(this IMonitoringService service, NotificationPlanId notificationPlanId, UpdateNotificationPlanConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateNotificationPlanAsync(notificationPlanId, configuration, CancellationToken.None).Wait();
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
        /// Remove and delete a monitoring notification plan by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationPlanId">The notification plan ID. This is obtained from <see cref="NotificationPlan.Id">NotificationPlan.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="notificationPlanId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-plans.html#service-notification-plans-delete">Delete Notification Plans (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveNotificationPlan(this IMonitoringService service, NotificationPlanId notificationPlanId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveNotificationPlanAsync(notificationPlanId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Notification Plans

        #region Monitoring Zones

        /// <summary>
        /// Gets a collection of monitoring zones.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-monitoring-zones.html#service-monitoring-zones-list">List Monitoring Zones (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<MonitoringZone, MonitoringZoneId> ListMonitoringZones(this IMonitoringService service, MonitoringZoneId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListMonitoringZonesAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring zone by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="monitoringZoneId">The monitoring zone ID. This is obtained from <see cref="MonitoringZone.Id">MonitoringZone.Id</see>.</param>
        /// <returns>A <see cref="MonitoringZone"/> object describing the monitoring zone.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="monitoringZoneId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-monitoring-zones.html#service-monitoring-zones-get">Get Monitoring Zone (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static MonitoringZone GetMonitoringZone(this IMonitoringService service, MonitoringZoneId monitoringZoneId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetMonitoringZoneAsync(monitoringZoneId, CancellationToken.None).Result;
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
        /// Perform a traceroute operation from a monitoring zone to a particular target.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="monitoringZoneId">The monitoring zone ID. This is obtained from <see cref="MonitoringZone.Id">MonitoringZone.Id</see>.</param>
        /// <param name="configuration">A <see cref="TraceRouteConfiguration"/> object containing the traceroute parameters.</param>
        /// <returns>A <see cref="TraceRoute"/> object describing the results of the traceroute operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="monitoringZoneId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-monitoring-zones.html#service-monitoring-zones-traceroute">Perform a "traceroute" from a Monitoring Zone (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static TraceRoute PerformTraceRouteFromMonitoringZone(this IMonitoringService service, MonitoringZoneId monitoringZoneId, TraceRouteConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.PerformTraceRouteFromMonitoringZoneAsync(monitoringZoneId, configuration, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Monitoring Zones

        #region Alarm Notification History

        /// <summary>
        /// Gets a collection of <see cref="CheckId"/> objects identifying the particular checks
        /// for which an alarm notification history is present for a particular entity/alarm
        /// combination.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="alarmId">The alarm ID. This is obtained from <see cref="Alarm.Id">Alarm.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="CheckId"/> objects identifying the checks for which
        /// an alarm notification history is present.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="alarmId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-notification-history.html#service-alarm-notification-history-discover">Discover Alarm Notification History (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<CheckId> DiscoverAlarmNotificationHistory(this IMonitoringService service, EntityId entityId, AlarmId alarmId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.DiscoverAlarmNotificationHistoryAsync(entityId, alarmId, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring alarm notification history items.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="alarmId">The alarm ID. This is obtained from <see cref="Alarm.Id">Alarm.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="from">The beginning timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="to">The ending timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, the current time is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="alarmId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="to"/> occurs before <paramref name="from"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="limit"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="from"/> represents a date before January 1, 1970 UTC.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="to"/> represents a date before January 1, 1970 UTC.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-notification-history.html#service-alarm-notification-history-list">List Alarm Notification History (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">Time Series Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<AlarmNotificationHistoryItem, AlarmNotificationHistoryItemId> ListAlarmNotificationHistory(this IMonitoringService service, EntityId entityId, AlarmId alarmId, CheckId checkId, AlarmNotificationHistoryItemId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAlarmNotificationHistoryAsync(entityId, alarmId, checkId, marker, limit, from, to, CancellationToken.None).Result;
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
        /// Gets a monitoring alarm notification history item by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="alarmId">The alarm ID. This is obtained from <see cref="Alarm.Id">Alarm.Id</see>.</param>
        /// <param name="checkId">The check ID. This is obtained from <see cref="Check.Id">Check.Id</see>.</param>
        /// <param name="alarmNotificationHistoryItemId">The alarm notification history item ID. This is obtained from <see cref="AlarmNotificationHistoryItem.Id">AlarmNotificationHistoryItem.Id</see>.</param>
        /// <returns>An <see cref="AlarmNotificationHistoryItem"/> object describing the alarm notification history item.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="alarmId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="checkId"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="alarmNotificationHistoryItemId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-notification-history.html#service-alarm-notification-history-get">Get Alarm Notification History (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static AlarmNotificationHistoryItem GetAlarmNotificationHistory(this IMonitoringService service, EntityId entityId, AlarmId alarmId, CheckId checkId, AlarmNotificationHistoryItemId alarmNotificationHistoryItemId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAlarmNotificationHistoryAsync(entityId, alarmId, checkId, alarmNotificationHistoryItemId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Alarm Notification History

        #region Notifications

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="configuration">A <see cref="NewNotificationConfiguration"/> object describing the new notification.</param>
        /// <returns>The <see cref="NotificationId"/> identifying the new notification.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html#service-notifications-create">Create Notification (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static NotificationId CreateNotification(this IMonitoringService service, NewNotificationConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateNotificationAsync(configuration, CancellationToken.None).Result;
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
        /// Test a monitoring notification.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="configuration">A <see cref="NewNotificationConfiguration"/> object describing the notification to test.</param>
        /// <returns>A <see cref="NotificationData"/> object describing the test results.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html#service-notifications-test-new">Test Notification (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static NotificationData TestNotification(this IMonitoringService service, NewNotificationConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.TestNotificationAsync(configuration, CancellationToken.None).Result;
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
        /// Test an existing notification by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationId">The notification ID. This is obtained from <see cref="Notification.Id">Notification.Id</see>.</param>
        /// <returns>A <see cref="NotificationData"/> object describing the test results.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="notificationId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html#service-notifications-test-existing">Test Existing Notification (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static NotificationData TestExistingNotification(this IMonitoringService service, NotificationId notificationId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.TestExistingNotificationAsync(notificationId, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring notifications.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html#service-notifications-list">List Notifications (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Notification, NotificationId> ListNotifications(this IMonitoringService service, NotificationId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListNotificationsAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring notification by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationId">The notification ID. This is obtained from <see cref="Notification.Id">Notification.Id</see>.</param>
        /// <returns>A <see cref="Notification"/> object describing the notification.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="notificationId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html#service-notifications-get">Get Notification (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Notification GetNotification(this IMonitoringService service, NotificationId notificationId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetNotificationAsync(notificationId, CancellationToken.None).Result;
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
        /// Updates a monitoring notification.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationId">The notification ID. This is obtained from <see cref="Notification.Id">Notification.Id</see>.</param>
        /// <param name="configuration">An <see cref="UpdateNotificationConfiguration"/> object describing the changes to apply to the notification.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="notificationId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html#service-notifications-update">Update Notification (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateNotification(this IMonitoringService service, NotificationId notificationId, UpdateNotificationConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateNotificationAsync(notificationId, configuration, CancellationToken.None).Wait();
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
        /// Remove and delete a monitoring notification by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationId">The notification ID. This is obtained from <see cref="Notification.Id">Notification.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="notificationId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notifications.html#service-notifications-delete">Delete Notification (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveNotification(this IMonitoringService service, NotificationId notificationId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveNotificationAsync(notificationId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Notifications

        #region Notification Types

        /// <summary>
        /// Gets a collection of monitoring notification types.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-types-crud.html#Service-Notification-Types-List">List Notification Types (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<NotificationType, NotificationTypeId> ListNotificationTypes(this IMonitoringService service, NotificationTypeId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListNotificationTypesAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring notification type by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="notificationTypeId">The notification type ID. This is obtained from <see cref="NotificationType.Id">NotificationType.Id</see>, or from the predefined values in <see cref="NotificationTypeId"/>.</param>
        /// <returns>A <see cref="NotificationType"/> object describing the notification type.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="notificationTypeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-notification-types-crud.html#Service-Notification-Types-get">Get Notification Type (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static NotificationType GetNotificationType(this IMonitoringService service, NotificationTypeId notificationTypeId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetNotificationTypeAsync(notificationTypeId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Notification Types

        #region Changelogs

        /// <summary>
        /// Gets a collection of monitoring alarm changelogs.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="from">The beginning timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="to">The ending timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, the current time is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="to"/> occurs before <paramref name="from"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="limit"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="from"/> represents a date before January 1, 1970 UTC.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="to"/> represents a date before January 1, 1970 UTC.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-changelogs.html#service-changelogs-alarms-list">List Alarm Changelogs (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">Time Series Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<AlarmChangelog, AlarmChangelogId> ListAlarmChangelogs(this IMonitoringService service, AlarmChangelogId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAlarmChangelogsAsync(marker, limit, from, to, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring alarm changelogs.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID to filter alarm changelogs. This is obtained from <see cref="Entity.Id">Entity.Id</see>. If the value is <see langword="null"/>, changelogs for all entities are returned.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="from">The beginning timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="to">The ending timestamp of the items to include in the collection. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">time series collections</see>. If the value is <see langword="null"/>, the current time is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="to"/> occurs before <paramref name="from"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="limit"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="from"/> represents a date before January 1, 1970 UTC.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="to"/> represents a date before January 1, 1970 UTC.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-changelogs.html#service-changelogs-alarms-list">List Alarm Changelogs (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-time-series-collections.html">Time Series Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<AlarmChangelog, AlarmChangelogId> ListAlarmChangelogs(this IMonitoringService service, EntityId entityId, AlarmChangelogId marker, int? limit, DateTimeOffset? from, DateTimeOffset? to)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAlarmChangelogsAsync(entityId, marker, limit, from, to, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Changelogs

        #region Views

        /// <summary>
        /// Gets a collection of monitoring entity overviews.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-views.html#service-views-overview">Get Overview (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<EntityOverview, EntityId> ListEntityOverviews(this IMonitoringService service, EntityId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListEntityOverviewsAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring entity overviews, filtered by entity ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="entityIdFilter">A collection of entity IDs for filter the results. If the value is <see langword="null"/>, overviews for all entities are returned.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="entityIdFilter"/> is non-<see langword="null"/> and contains any <see langword="null"/> values.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-views.html#service-views-overview">Get Overview (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<EntityOverview, EntityId> ListEntityOverviews(this IMonitoringService service, EntityId marker, int? limit, IEnumerable<EntityId> entityIdFilter)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListEntityOverviewsAsync(marker, limit, entityIdFilter, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Views

        #region Alarm Examples

        /// <summary>
        /// Gets a collection of monitoring alarm examples.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-examples.html#service-alarm-examples-list">List Alarm Examples (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<AlarmExample, AlarmExampleId> ListAlarmExamples(this IMonitoringService service, AlarmExampleId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAlarmExamplesAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring alarm example by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="alarmExampleId">The alarm example ID. This is obtained from <see cref="AlarmExample.Id">AlarmExample.Id</see>.</param>
        /// <returns>An <see cref="AlarmExample"/> object describing the alarm example.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="alarmExampleId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-examples.html#service-alarm-examples-get">Get Alarm Example (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static AlarmExample GetAlarmExample(this IMonitoringService service, AlarmExampleId alarmExampleId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAlarmExampleAsync(alarmExampleId, CancellationToken.None).Result;
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
        /// Evaluate the template of a single alarm example.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="alarmExampleId">The alarm example ID. This is obtained from <see cref="AlarmExample.Id">AlarmExample.Id</see>.</param>
        /// <param name="exampleParameters">A dictionary containing the values to insert in the alarm example. The dictionary should contain a key/value pair for each field described in <see cref="AlarmExample.Fields"/> for the alarm example.</param>
        /// <returns>A <see cref="BoundAlarmExample"/> object containing the evaluated alarm example.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="alarmExampleId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="exampleParameters"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="exampleParameters"/> contains any empty keys.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-alarm-examples.html#service-alarm-examples-post">Evaluate Alarm Example (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static BoundAlarmExample EvaluateAlarmExample(this IMonitoringService service, AlarmExampleId alarmExampleId, IDictionary<string, object> exampleParameters)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.EvaluateAlarmExampleAsync(alarmExampleId, exampleParameters, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Alarm Examples

        #endregion Core

        #region Agent

        #region Agents

        /// <summary>
        /// Gets a collection of monitoring agents.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent.html#service-agent-list-agents">List Agents (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Agent, AgentId> ListAgents(this IMonitoringService service, AgentId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAgentsAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring agent by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>An <see cref="Agent"/> object describing the agent.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent.html#service-agent-list-agent">Get Agent (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Agent GetAgent(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAgentAsync(agentId, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring agent connections.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent.html#service-agent-list-agent-connections">List Agent Connections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<AgentConnection, AgentConnectionId> ListAgentConnections(this IMonitoringService service, AgentId agentId, AgentConnectionId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAgentConnectionsAsync(agentId, marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring agent connection by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <param name="agentConnectionId">The agent connection ID. This is obtained from <see cref="AgentConnection.Id">AgentConnection.Id</see>.</param>
        /// <returns>An <see cref="AgentConnection"/> object describing the agent connection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="agentId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="agentConnectionId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent.html#service-agent-list-agent-connection">Get Agent Connection (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static AgentConnection GetAgentConnection(this IMonitoringService service, AgentId agentId, AgentConnectionId agentConnectionId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAgentConnectionAsync(agentId, agentConnectionId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Agents

        #region Agent Token

        /// <summary>
        /// Creates a new agent token.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="configuration">An <see cref="AgentTokenConfiguration"/> object describing the new agent token.</param>
        /// <returns>The <see cref="AgentTokenId"/> identifying the new agent token.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-tokens.html#service-agent-token-create-token">Create Agent Token (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static AgentTokenId CreateAgentToken(this IMonitoringService service, AgentTokenConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateAgentTokenAsync(configuration, CancellationToken.None).Result;
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
        /// Gets a collection of monitoring agent tokens.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-tokens.html#service-agent-token-list">List Agent Tokens (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<AgentToken, AgentTokenId> ListAgentTokens(this IMonitoringService service, AgentTokenId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAgentTokensAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a monitoring agent token by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentTokenId">The agent token ID. This is obtained from <see cref="AgentToken.Id">AgentToken.Id</see>.</param>
        /// <returns>An <see cref="AgentToken"/> object describing the agent token.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentTokenId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-tokens.html#service-agent-token-get">Get Agent Token (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static AgentToken GetAgentToken(this IMonitoringService service, AgentTokenId agentTokenId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAgentTokenAsync(agentTokenId, CancellationToken.None).Result;
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
        /// Updates a monitoring agent token.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentTokenId">The agent token ID. This is obtained from <see cref="AgentToken.Id">AgentToken.Id</see>.</param>
        /// <param name="configuration">An <see cref="AgentTokenConfiguration"/> object describing the changes to apply to the agent token.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="agentTokenId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-tokens.html#service-agent-token-update">Update Agent Token (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateAgentToken(this IMonitoringService service, AgentTokenId agentTokenId, AgentTokenConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateAgentTokenAsync(agentTokenId, configuration, CancellationToken.None).Wait();
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
        /// Remove and delete a monitoring agent token by ID.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentTokenId">The agent token ID. This is obtained from <see cref="AgentToken.Id">AgentToken.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentTokenId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-tokens.html#service-agent-token-delete">Delete Agent Token (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveAgentToken(this IMonitoringService service, AgentTokenId agentTokenId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveAgentTokenAsync(agentTokenId, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Agent Token

        #region Agent Host Information

        /// <summary>
        /// Gets agent host information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <param name="hostInformation">The host information type.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="agentId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="hostInformation"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html">Agent Host Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<JToken> GetAgentHostInformation(this IMonitoringService service, AgentId agentId, HostInformationType hostInformation)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetAgentHostInformationAsync(agentId, hostInformation, CancellationToken.None).Result;
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
        /// Gets CPU information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-cpus">Get CPUs Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<ReadOnlyCollection<CpuInformation>> GetCpuInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetCpuInformationAsync(agentId, CancellationToken.None).Result;
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
        /// Gets disk information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-disks">Get Disks Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<ReadOnlyCollection<DiskInformation>> GetDiskInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetDiskInformationAsync(agentId, CancellationToken.None).Result;
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
        /// Gets filesystem information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-filesystems">Get Filesystems Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<ReadOnlyCollection<FilesystemInformation>> GetFilesystemInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetFilesystemInformationAsync(agentId, CancellationToken.None).Result;
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
        /// Gets memory information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-memory">Get Memory Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<MemoryInformation> GetMemoryInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetMemoryInformationAsync(agentId, CancellationToken.None).Result;
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
        /// Gets network interface information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-network_interfaces">Get Network Interfaces Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<ReadOnlyCollection<NetworkInterfaceInformation>> GetNetworkInterfaceInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetNetworkInterfaceInformationAsync(agentId, CancellationToken.None).Result;
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
        /// Gets process information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-processes">Get Processes Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<ReadOnlyCollection<ProcessInformation>> GetProcessInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetProcessInformationAsync(agentId, CancellationToken.None).Result;
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
        /// Gets system information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-system">Get System Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<SystemInformation> GetSystemInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetSystemInformationAsync(agentId, CancellationToken.None).Result;
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
        /// Gets login information reported by a monitoring agent.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="agentId">The agent ID. This is obtained from <see cref="Agent.Id">Agent.Id</see>.</param>
        /// <returns>A <see cref="HostInformation{T}"/> object containing the host information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="agentId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-host_info.html#service-agent-host_info-who">Get Logged-in User Information (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static HostInformation<ReadOnlyCollection<LoginInformation>> GetLoginInformation(this IMonitoringService service, AgentId agentId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetLoginInformationAsync(agentId, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Agent Host Information

        #region Agent Targets

        /// <summary>
        /// Gets a collection of monitoring agent check targets.
        /// </summary>
        /// <param name="service">The monitoring service instance.</param>
        /// <param name="entityId">The entity ID. This is obtained from <see cref="Entity.Id">Entity.Id</see>.</param>
        /// <param name="agentCheckType">The agent check type ID. This is obtained from <see cref="CheckType.Id">CheckType.Id</see>, or from the predefined values in <see cref="CheckTypeId"/>.</param>
        /// <param name="marker">A marker identifying the next page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>, and is obtained from <see cref="ReadOnlyCollectionPage{T, TMarker}.NextMarker"/>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of items to include in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A <see cref="ReadOnlyCollectionPage{T, TMarker}"/> object containing the page
        /// of results and its associated pagination metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="agentCheckType"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="agentCheckType"/> is not an agent check type (i.e. the <see cref="CheckTypeId.IsAgent"/> property is <see langword="false"/>).</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-agent-targets.html#service-agent-list-check-targets">List Agent Check Targets (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/api-paginated-collections.html">Paginated Collections (Rackspace Cloud Monitoring Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<CheckTarget, CheckTargetId> ListAgentCheckTargets(this IMonitoringService service, EntityId entityId, CheckTypeId agentCheckType, CheckTargetId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListAgentCheckTargetsAsync(entityId, agentCheckType, marker, limit, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Agent Targets

        #endregion Agent
    }
}
