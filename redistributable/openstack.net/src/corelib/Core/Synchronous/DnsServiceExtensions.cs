namespace net.openstack.Core.Synchronous
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using net.openstack.Core.Collections;
    using net.openstack.Providers.Rackspace;
    using net.openstack.Providers.Rackspace.Objects.Dns;
    using Newtonsoft.Json;
    using CancellationToken = System.Threading.CancellationToken;
    using ServiceCatalog = net.openstack.Core.Domain.ServiceCatalog;

    /// <summary>
    /// Provides extension methods to allow synchronous calls to the methods in <see cref="IDnsService"/>.
    /// </summary>
    /// <preliminary/>
    [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
    public static class DnsServiceExtensions
    {
        #region Limits

        /// <summary>
        /// Get information about the provider-specific limits of this service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <returns>A <see cref="DnsServiceLimits"/> object containing detailed information about the limits for the service provider.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_All_Limits.html">List All Limits (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsServiceLimits ListLimits(this IDnsService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListLimitsAsync(CancellationToken.None).Result;
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
        /// Get information about the types of provider-specific limits in place for this service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <returns>A collection of <see cref="LimitType"/> objects containing the limit types supported by the service.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Limit_Types.html">List Limit Types (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<LimitType> ListLimitTypes(this IDnsService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListLimitTypesAsync(CancellationToken.None).Result;
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
        /// Get information about the provider-specific limits of this service for a particular <see cref="LimitType"/>.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="type">The limit type.</param>
        /// <returns>A <see cref="DnsServiceLimits"/> object containing detailed information about the limits of the specified <paramref name="type"/> for the service provider.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="service"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="type"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Specific_Limit.html">List Specific Limit (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsServiceLimits ListLimits(this IDnsService service, LimitType type)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListLimitsAsync(type, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Limits

        #region Jobs

        /// <summary>
        /// Gets information about an asynchronous task being executed by the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="job">The <see cref="DnsJob"/> to query.</param>
        /// <param name="showDetails"><see langword="true"/> to include detailed information about the job; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="DnsJob"/> object containing the updated job information.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="service"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="job"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/sync_asynch_responses.html">Synchronous and Asynchronous Responses (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob GetJobStatus(this IDnsService service, DnsJob job, bool showDetails)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetJobStatusAsync(job, showDetails, CancellationToken.None).Result;
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
        /// Gets information about an asynchronous task with a strongly-typed result being executed by the DNS service.
        /// </summary>
        /// <typeparam name="TResponse">The class modeling the JSON result of the asynchronous operation.</typeparam>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="job">The <see cref="DnsJob{TResponse}"/> to query.</param>
        /// <param name="showDetails"><see langword="true"/> to include detailed information about the job; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="DnsJob{TResult}"/> object containing the updated job information.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="service"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="job"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="JsonSerializationException">If an error occurs while deserializing the response object.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/sync_asynch_responses.html">Synchronous and Asynchronous Responses (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob<TResponse> GetJobStatus<TResponse>(this IDnsService service, DnsJob<TResponse> job, bool showDetails)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetJobStatusAsync(job, showDetails, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion Jobs

        #region Domains

        /// <summary>
        /// Gets information about domains currently listed in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainName">If specified, the list will be filtered to only include the specified domain and its subdomains (if any exist).</param>
        /// <param name="offset">The index of the last item in the previous page of results. If not specified, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of domains to return in a single page.</param>
        /// <returns>
        /// A tuple of the resulting collection of <see cref="DnsDomain"/> objects and the total number of domains in
        /// the list. If the total number of domains in the list is not available, the second element of the tuple will
        /// be <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than or equal to 0.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/list_domains.html">List Domains (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/search_domains_w_filters.html">Search Domains with Filtering (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Tuple<ReadOnlyCollectionPage<DnsDomain>, int?> ListDomains(this IDnsService service, string domainName, int? offset, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListDomainsAsync(domainName, offset, limit, CancellationToken.None).Result;
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
        /// Gets detailed information about a specific domain.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="showRecords"><see langword="true"/> to populate the <see cref="DnsDomain.Records"/> property of the result; otherwise, <see langword="false"/>.</param>
        /// <param name="showSubdomains"><see langword="true"/> to populate the <see cref="DnsDomain.Subdomains"/> property of the result; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="DnsDomain"/> object containing the DNS information for the requested domain.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="domainId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/list_domain_details.html">List Domain Details (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsDomain ListDomainDetails(this IDnsService service, DomainId domainId, bool showRecords, bool showSubdomains)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListDomainDetailsAsync(domainId, showRecords, showSubdomains, CancellationToken.None).Result;
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
        /// Gets information about all changes made to a domain since a specified time.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="since">The timestamp of the earliest changes to consider. If this is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>A <see cref="DnsDomainChanges"/> object describing the changes made to a domain registered in the DNS service.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="domainId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Domain_Changes.html">List Domain Changes (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsDomainChanges ListDomainChanges(this IDnsService service, DomainId domainId, DateTimeOffset? since)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListDomainChangesAsync(domainId, since, CancellationToken.None).Result;
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
        /// Exports a domain registered in the DNS service.
        /// </summary>
        /// <remarks>
        /// The exported domain represents a single domain, and does not include subdomains.
        ///
        /// <note>
        /// The <see cref="SerializedDomainFormat.Bind9"/> format does not support comments, so any
        /// comments associated with a domain or its records will not be included in the exported
        /// result.
        /// </note>
        /// </remarks>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <returns>A <see cref="DnsJob{TResponse}"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="domainId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/export_domain.html">Export Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob<ExportedDomain> ExportDomain(this IDnsService service, DomainId domainId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ExportDomainAsync(domainId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Registers one or more new domains in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="configuration">A <see cref="DnsConfiguration"/> object describing the domains to register in the DNS service.</param>
        /// <returns>A <see cref="DnsJob{TResponse}"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/create_domains.html">Create Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob<DnsDomains> CreateDomains(this IDnsService service, DnsConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateDomainsAsync(configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Updates one or more domains in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="configuration">A <see cref="DnsUpdateConfiguration"/> object describing updates to apply to the domains.</param>
        /// <returns>A <see cref="DnsJob"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Modify_Domain_s_-d1e3848.html">Modify Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob UpdateDomains(this IDnsService service, DnsUpdateConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.UpdateDomainsAsync(configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Clones a domain registered in the DNS service, optionally cloning its subdomains as well.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="cloneName">The name of the new (cloned) domain.</param>
        /// <param name="cloneSubdomains"><see langword="true"/> to recursively clone subdomains; otherwise, <see langword="false"/> to only clone the top-level domain and its records. Cloned subdomain configurations are modified the same way that cloned top-level domain configurations are modified. If this is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="modifyRecordData"><see langword="true"/> to replace occurrences of the reference domain name with the new domain name in comments on the cloned (new) domain. If this is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="modifyEmailAddress"><see langword="true"/> to replace occurrences of the reference domain name with the new domain name in email addresses on the cloned (new) domain. If this is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="modifyComment"><true>true</true> to replace occurrences of the reference domain name with the new domain name in data fields (of records) on the cloned (new) domain. Does not affect NS records. If this is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>A <see cref="DnsJob{TResponse}"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="domainId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="cloneName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="cloneName"/> is empty.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/clone_domain-dle846.html">Clone Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob<DnsDomains> CloneDomain(this IDnsService service, DomainId domainId, string cloneName, bool? cloneSubdomains, bool? modifyRecordData, bool? modifyEmailAddress, bool? modifyComment)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CloneDomainAsync(domainId, cloneName, cloneSubdomains, modifyRecordData, modifyEmailAddress, modifyComment, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Imports domains into the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="serializedDomains">A collection of <see cref="SerializedDomain"/> objects containing the serialized domain information to import.</param>
        /// <returns>A <see cref="DnsJob{TResponse}"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="serializedDomains"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serializedDomains"/> is contains any <see langword="null"/> values.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/import_domain.html">Import Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob<DnsDomains> ImportDomain(this IDnsService service, IEnumerable<SerializedDomain> serializedDomains)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ImportDomainAsync(serializedDomains, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Removes one or more domains from the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainIds">A collection of IDs for the domains to remove. These are obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="deleteSubdomains"><see langword="true"/> to delete any subdomains associated with the specified domains; otherwise, <see langword="false"/> to promote any subdomains to top-level domains.</param>
        /// <returns>A <see cref="DnsJob"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="domainIds"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="domainIds"/> contains any <see langword="null"/> values.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Remove_Domain_s_-d1e4022.html">Remove Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob RemoveDomains(this IDnsService service, IEnumerable<DomainId> domainIds, bool deleteSubdomains)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.RemoveDomainsAsync(domainIds, deleteSubdomains, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion

        #region Subdomains

        /// <summary>
        /// Gets information about subdomains currently associated with a domain in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The top-level domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="offset">The index of the last item in the previous page of results. If not specified, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of subdomains to return in a single page.</param>
        /// <returns>
        /// A tuple of the resulting collection of <see cref="DnsSubdomain"/> objects and the total number
        /// of domains in the list. If the total number of subdomains in the list is not available, the second
        /// element of the tuple will be <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="domainId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than or equal to 0.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Subdomains-d1e4295.html">List Subdomains (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Tuple<ReadOnlyCollectionPage<DnsSubdomain>, int?> ListSubdomains(this IDnsService service, DomainId domainId, int? offset, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListSubdomainsAsync(domainId, offset, limit, CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion

        #region Records

        /// <summary>
        /// Gets information about records currently associated with a domain in the DNS service, optionally filtering the results
        /// to include only records of a specific type, name, and/or data.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="recordType">The specific record type to consider, or <see langword="null"/> to consider all record types.</param>
        /// <param name="recordName">The record name, which is matched to the <see cref="DnsRecord.Name"/> property, or <see langword="null"/> to consider all records.</param>
        /// <param name="recordData">The record data, which is matched to the <see cref="DnsRecord.Data"/> property, or <see langword="null"/> to consider all records.</param>
        /// <param name="offset">The index of the last item in the previous page of results. If not specified, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of records to return in a single page.</param>
        /// <returns>
        /// A tuple of the resulting collection of <see cref="DnsRecord"/> objects and the total number of records
        /// in the list. If the total number of records in the list is not available, the second element of the
        /// tuple will be <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="domainId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than or equal to 0.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Records-d1e4629.html">List Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Search_Records-e338d7e0.html">Search Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Tuple<ReadOnlyCollectionPage<DnsRecord>, int?> ListRecords(this IDnsService service, DomainId domainId, DnsRecordType recordType, string recordName, string recordData, int? offset, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListRecordsAsync(domainId, recordType, recordName, recordData, offset, limit, CancellationToken.None).Result;
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
        /// Gets detailed information about a specific DNS record.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="recordId">The record ID. This is obtained from <see cref="DnsRecord.Id">DnsRecord.Id</see>.</param>
        /// <returns>A <see cref="DnsRecord"/> object containing the details of the specified DNS record.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="domainId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Record_Details-d1e4770.html">List Record Details (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsRecord ListRecordDetails(this IDnsService service, DomainId domainId, RecordId recordId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListRecordDetailsAsync(domainId, recordId, CancellationToken.None).Result;
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
        /// Adds records to a domain in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="recordConfigurations">A collection of <see cref="DnsDomainRecordConfiguration"/> objects describing the records to add.</param>
        /// <returns>A <see cref="DnsJob{TResponse}"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="domainId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordConfigurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="recordConfigurations"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Add_Records-d1e4895.html">Add Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob<DnsRecordsList> AddRecords(this IDnsService service, DomainId domainId, IEnumerable<DnsDomainRecordConfiguration> recordConfigurations)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.AddRecordsAsync(domainId, recordConfigurations, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Updates domain records in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="recordConfigurations">A collection of <see cref="DnsDomainRecordUpdateConfiguration"/> objects describing the updates to apply to domain records.</param>
        /// <returns>A <see cref="DnsJob"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="domainId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordConfigurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="recordConfigurations"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Modify_Records-d1e5033.html">Modify Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob UpdateRecords(this IDnsService service, DomainId domainId, IEnumerable<DnsDomainRecordUpdateConfiguration> recordConfigurations)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.UpdateRecordsAsync(domainId, recordConfigurations, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Removes one or more domain records from the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="domainId">The domain ID. This is obtained from <see cref="DnsDomain.Id">DnsDomain.Id</see>.</param>
        /// <param name="recordId">A collection of IDs for the records to remove. These are obtained from <see cref="DnsRecord.Id">DnsRecord.Id</see>.</param>
        /// <returns>A <see cref="DnsJob"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="domainId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="recordId"/> contains any <see langword="null"/> values.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Remove_Records-d1e5188.html">Remove Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob RemoveRecords(this IDnsService service, DomainId domainId, IEnumerable<RecordId> recordId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.RemoveRecordsAsync(domainId, recordId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion

        #region Reverse DNS

        /// <summary>
        /// Gets information about reverse DNS records currently associated with a cloud resource in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="serviceName">The name of the service which owns the cloud resource. This is obtained from <see cref="ServiceCatalog.Name"/>.</param>
        /// <param name="deviceResourceUri">The absolute URI of the cloud resource.</param>
        /// <param name="offset">The index of the last item in the previous page of results. If not specified, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of records to return in a single page.</param>
        /// <returns>
        /// A tuple of the resulting collection of <see cref="DnsRecord"/> objects and the total number of domains
        /// in the list. If the total number of subdomains in the list is not available, the second element of the
        /// tuple will be <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="deviceResourceUri"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="serviceName"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than or equal to 0.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/ReverseDNS-123457000.html">List PTR Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Tuple<ReadOnlyCollectionPage<DnsRecord>, int?> ListPtrRecords(this IDnsService service, string serviceName, Uri deviceResourceUri, int? offset, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListPtrRecordsAsync(serviceName, deviceResourceUri, offset, limit, CancellationToken.None).Result;
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
        /// Gets detailed information about a reverse DNS record currently associated with a cloud resource in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="serviceName">The name of the service which owns the cloud resource. This is obtained from <see cref="ServiceCatalog.Name"/>.</param>
        /// <param name="deviceResourceUri">The absolute URI of the cloud resource.</param>
        /// <param name="recordId">The record ID. This is obtained from <see cref="DnsRecord.Id">DnsRecord.Id</see>.</param>
        /// <returns>A <see cref="DnsRecord"/> object containing the details of the specified reverse DNS record.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="deviceResourceUri"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="recordId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceName"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordId"/> is empty.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/ReverseDNS-123457001.html">List PTR Record Details (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsRecord ListPtrRecordDetails(this IDnsService service, string serviceName, Uri deviceResourceUri, RecordId recordId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListPtrRecordDetailsAsync(serviceName, deviceResourceUri, recordId, CancellationToken.None).Result;
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
        /// Adds reverse DNS records to a cloud resource in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="serviceName">The name of the service which owns the cloud resource. This is obtained from <see cref="ServiceCatalog.Name"/>.</param>
        /// <param name="deviceResourceUri">The absolute URI of the cloud resource.</param>
        /// <param name="recordConfigurations">A collection of <see cref="DnsDomainRecordConfiguration"/> objects describing the records to add.</param>
        /// <returns>A <see cref="DnsJob{DnsRecordsList}"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="deviceResourceUri"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="recordConfigurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceName"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordConfigurations"/> contains any <see langword="null"/> values.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/ReverseDNS-123457003.html">Add PTR Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob<DnsRecordsList> AddPtrRecords(this IDnsService service, string serviceName, Uri deviceResourceUri, IEnumerable<DnsDomainRecordConfiguration> recordConfigurations)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.AddPtrRecordsAsync(serviceName, deviceResourceUri, recordConfigurations, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Update reverse DNS records for a cloud resource in the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="serviceName">The name of the service which owns the cloud resource. This is obtained from <see cref="ServiceCatalog.Name"/>.</param>
        /// <param name="deviceResourceUri">The absolute URI of the cloud resource.</param>
        /// <param name="recordConfigurations">A collection of <see cref="DnsDomainRecordUpdateConfiguration"/> objects describing the updates to apply to domain records.</param>
        /// <returns>A <see cref="DnsJob"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="deviceResourceUri"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="recordConfigurations"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceName"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordConfigurations"/> contains any <see langword="null"/> values.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/ReverseDNS-123457004.html">Modify PTR Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob UpdatePtrRecords(this IDnsService service, string serviceName, Uri deviceResourceUri, IEnumerable<DnsDomainRecordUpdateConfiguration> recordConfigurations)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.UpdatePtrRecordsAsync(serviceName, deviceResourceUri, recordConfigurations, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Removes one or more reverse DNS records from the DNS service.
        /// </summary>
        /// <param name="service">The DNS service instance.</param>
        /// <param name="serviceName">The name of the service which owns the cloud resource. This is obtained from <see cref="ServiceCatalog.Name"/>.</param>
        /// <param name="deviceResourceUri">The absolute URI of the cloud resource.</param>
        /// <param name="ipAddress">The specific record to remove. If this is <see langword="null"/>, all reverse DNS records associated with the specified device are removed.</param>
        /// <returns>A <see cref="DnsJob"/> object describing the asynchronous server operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serviceName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="deviceResourceUri"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serviceName"/> is empty.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/ReverseDNS-123457005.html">Remove PTR Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DnsJob RemovePtrRecords(this IDnsService service, string serviceName, Uri deviceResourceUri, IPAddress ipAddress)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.RemovePtrRecordsAsync(serviceName, deviceResourceUri, ipAddress, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        #endregion
    }
}
