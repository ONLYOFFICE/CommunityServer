namespace net.openstack.Core.Synchronous
{
    using System;
    using System.Collections.ObjectModel;
    using net.openstack.Core.Collections;
    using net.openstack.Providers.Rackspace;
    using net.openstack.Providers.Rackspace.Objects.Databases;
    using CancellationToken = System.Threading.CancellationToken;
    using WebException = System.Net.WebException;

    /// <summary>
    /// Provides extension methods to allow synchronous calls to the methods in <see cref="IDatabaseService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
    public static class DatabaseServiceExtensions
    {
        #region Database instances

        /// <summary>
        /// Create a new database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="configuration">A <see cref="DatabaseInstanceConfiguration"/> object describing the configuration of the new database instance.</param>
        /// <returns>
        /// A <see cref="DatabaseInstance"/> object describing the new database instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_createInstance__version___accountId__instances_.html">Create Database Instance (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DatabaseInstance CreateDatabaseInstance(this IDatabaseService service, DatabaseInstanceConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateDatabaseInstanceAsync(configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Gets a collection of all database instances.
        /// </summary>
        /// <remarks>
        /// <note>
        /// This is a <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">paginated collection</see>.
        /// </note>
        /// </remarks>
        /// <param name="service">The database service instance.</param>
        /// <param name="marker">The database instance ID of the last <see cref="DatabaseInstance"/> in the previous page of results. This parameter is used for <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of <see cref="DatabaseInstance"/> objects to return in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A collection of <see cref="DatabaseInstance"/> objects describing the database instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getInstance__version___accountId__instances_.html">List All Database Instances (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">Pagination (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<DatabaseInstance> ListDatabaseInstances(this IDatabaseService service, DatabaseInstanceId marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListDatabaseInstancesAsync(marker, limit, CancellationToken.None).Result;
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
        /// Gets a database instance by ID.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <returns>
        /// A <see cref="DatabaseInstance"/> object describing the database instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getInstanceById__version___accountId__instances__instanceId__.html">List Database Instance Status and Details (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DatabaseInstance GetDatabaseInstance(this IDatabaseService service, DatabaseInstanceId instanceId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetDatabaseInstanceAsync(instanceId, CancellationToken.None).Result;
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
        /// Removes and deletes a database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/DELETE_deleteInstance__version___accountId__instances__instanceId__.html">Delete Database Instance (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveDatabaseInstance(this IDatabaseService service, DatabaseInstanceId instanceId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveDatabaseInstanceAsync(instanceId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Enables login from any host for the root user, and returns the root username and generated password.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// Changes you make as a root user may cause detrimental effects to the database instance and unpredictable
        /// behavior for API operations. When you enable the root user, you accept the possibility that the provider
        /// will not be able to support your database instance. While enabling root does not prevent the provider
        /// from a "best effort" approach to helping you if something goes wrong with your instance, the provider
        /// cannot ensure that they will be able to assist you if you change core MySQL settings. These changes can
        /// be (but are not limited to) turning off binlogs, removing users that the provider uses to access your
        /// instance, and so forth.
        /// </note>
        /// </remarks>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <returns>
        /// A <see cref="RootUser"/> object containing the username and password of the root database user.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_createRoot__version___accountId__instances__instanceId__root_.html">Enable Root User (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static RootUser EnableRootUser(this IDatabaseService service, DatabaseInstanceId instanceId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.EnableRootUserAsync(instanceId, CancellationToken.None).Result;
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
        /// Checks whether or not root access has been enabled for a database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <returns>
        /// <see langword="true"/> if root access is enabled for the database instance; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_isRootEnabled__version___accountId__instances__instanceId__root_.html">List Root-Enabled Status (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static bool? CheckRootEnabledStatus(this IDatabaseService service, DatabaseInstanceId instanceId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CheckRootEnabledStatusAsync(instanceId, CancellationToken.None).Result;
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

        #region Database instance actions

        /// <summary>
        /// Restarts the database service on the instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_restartInstance__version___accountId__instances__instanceId__action_.html">Restart Instance (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RestartDatabaseInstance(this IDatabaseService service, DatabaseInstanceId instanceId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RestartDatabaseInstanceAsync(instanceId, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Resize the memory of the database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="flavorRef">The new flavor to use for the database instance. This is obtained from <see cref="DatabaseFlavor.Href">DatabaseFlavor.Href</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="flavorRef"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_resizeInstance__version___accountId__instances__instanceId__action_.html">Resize the Instance (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void ResizeDatabaseInstance(this IDatabaseService service, DatabaseInstanceId instanceId, FlavorRef flavorRef)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.ResizeDatabaseInstanceAsync(instanceId, flavorRef, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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
        /// Resize the volume attached to the database instance.
        /// </summary>
        /// <remarks>
        /// The provider may limit database volume resize operations to <em>increasing</em> the volume size.
        /// </remarks>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="volumeSize">The new volume size for the database instance.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="volumeSize"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_resizeVolume__version___accountId__instances__instanceId__action_.html">Resize the Instance Volume (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void ResizeDatabaseInstanceVolume(this IDatabaseService service, DatabaseInstanceId instanceId, int volumeSize)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.ResizeDatabaseInstanceVolumeAsync(instanceId, volumeSize, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Wait();
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

        #region Databases

        /// <summary>
        /// Create a new database within a database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="configuration">A <see cref="DatabaseConfiguration"/> object describing the configuration of the new database.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_createDatabase__version___accountId__instances__instanceId__databases_.html">Create Database (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void CreateDatabase(this IDatabaseService service, DatabaseInstanceId instanceId, DatabaseConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.CreateDatabaseAsync(instanceId, configuration, CancellationToken.None).Wait();
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
        /// Gets a collection of all databases within a database instance.
        /// </summary>
        /// <remarks>
        /// <note>
        /// This is a <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">paginated collection</see>.
        /// </note>
        /// </remarks>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="marker">The <see cref="Database.Name"/> of the last <see cref="Database"/> in the previous page of results. This parameter is used for <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of <see cref="Database"/> objects to return in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A collection of <see cref="Database"/> objects describing the databases.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getDatabases__version___accountId__instances__instanceId__databases_.html">List Databases for Instance (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">Pagination (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<Database> ListDatabases(this IDatabaseService service, DatabaseInstanceId instanceId, DatabaseName marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListDatabasesAsync(instanceId, marker, limit, CancellationToken.None).Result;
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
        /// Removes and deletes a database from a database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="databaseName">The database name. This is obtained from <see cref="Database.Name">Database.Name</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="databaseName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/DELETE_deleteDatabase__version___accountId__instances__instanceId__databases__databaseName__.html">Delete Database (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveDatabase(this IDatabaseService service, DatabaseInstanceId instanceId, DatabaseName databaseName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveDatabaseAsync(instanceId, databaseName, CancellationToken.None).Wait();
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

        #region Users

        /// <summary>
        /// Create a new user in a database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="configuration">A <see cref="UserConfiguration"/> object describing the configuration of the new user.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_createUser__version___accountId__instances__instanceId__users_.html">Create User (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void CreateUser(this IDatabaseService service, DatabaseInstanceId instanceId, UserConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.CreateUserAsync(instanceId, configuration, CancellationToken.None).Wait();
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
        /// Gets a collection of all users within a database instance.
        /// </summary>
        /// <remarks>
        /// <note>
        /// This is a <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">paginated collection</see>.
        /// </note>
        /// </remarks>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="marker">The <see cref="UserConfiguration.UserName"/> of the last user in the previous page of results. This parameter is used for <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of <see cref="Database"/> objects to return in a single page of results. This parameter is used for <see href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <returns>
        /// A collection of <see cref="DatabaseUser"/> objects describing the database instance users.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getUsers__version___accountId__instances__instanceId__users_.html">List Users in Database Instance (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/pagination.html">Pagination (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<DatabaseUser> ListDatabaseUsers(this IDatabaseService service, DatabaseInstanceId instanceId, UserName marker, int? limit)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListDatabaseUsersAsync(instanceId, marker, limit, CancellationToken.None).Result;
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
        /// Set the password for a database user.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="userName">A <see cref="UserName"/> object identifying the database user. This is obtained from <see cref="UserConfiguration.UserName">UserConfiguration.UserName</see>.</param>
        /// <param name="password">The new password for the user.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="userName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="password"/> is empty.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/PUT_changePass__version___accountId__instances__instanceId__users_.html">Change User(s) Password (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void SetUserPassword(this IDatabaseService service, DatabaseInstanceId instanceId, UserName userName, string password)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.SetUserPasswordAsync(instanceId, userName, password, CancellationToken.None).Wait();
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
        /// Update properties of a database user.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="userName">A <see cref="UserName"/> object identifying the database user. This is obtained from <see cref="UserConfiguration.UserName">UserConfiguration.UserName</see>.</param>
        /// <param name="configuration">An <see cref="UpdateUserConfiguration"/> object describing the updates to apply to the user.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="userName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="configuration"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/PUT_modifyUser__version___accountId__instances__instanceId__users__name__.html">Modify User Attributes (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void UpdateUser(this IDatabaseService service, DatabaseInstanceId instanceId, UserName userName, UpdateUserConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.UpdateUserAsync(instanceId, userName, configuration, CancellationToken.None).Wait();
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
        /// Get a database user by ID.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="userName">A <see cref="UserName"/> object identifying the database user. This is obtained from <see cref="UserConfiguration.UserName">UserConfiguration.UserName</see>.</param>
        /// <returns>
        /// A <see cref="DatabaseUser"/> object describing the user.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="userName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_listUser__version___accountId__instances__instanceId__users__name__.html">List User (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DatabaseUser GetUser(this IDatabaseService service, DatabaseInstanceId instanceId, UserName userName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetUserAsync(instanceId, userName, CancellationToken.None).Result;
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
        /// Remove and delete a user from a database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="userName">A <see cref="UserName"/> object identifying the database user. This is obtained from <see cref="UserConfiguration.UserName">UserConfiguration.UserName</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="userName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/DELETE_deleteUser__version___accountId__instances__instanceId__users__name__.html">Delete User (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveUser(this IDatabaseService service, DatabaseInstanceId instanceId, UserName userName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveUserAsync(instanceId, userName, CancellationToken.None).Wait();
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
        /// Gets a list of all databases a user has permission to access.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="userName">A <see cref="UserName"/> object identifying the database user. This is obtained from <see cref="UserConfiguration.UserName">UserConfiguration.UserName</see>.</param>
        /// <returns>
        /// A collection of <see cref="DatabaseName"/> objects identifying the databases the user can access.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="userName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getUserAccess__version___accountId__instances__instanceId__users__name__databases_.html">List User Access (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<DatabaseName> ListUserAccess(this IDatabaseService service, DatabaseInstanceId instanceId, UserName userName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListUserAccessAsync(instanceId, userName, CancellationToken.None).Result;
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
        /// Grant access to a database for a particular user.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="databaseName">The database name. This is obtained from <see cref="Database.Name">Database.Name</see>.</param>
        /// <param name="userName">A <see cref="UserName"/> object identifying the database user. This is obtained from <see cref="UserConfiguration.UserName">UserConfiguration.UserName</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="databaseName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="userName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/PUT_grantUserAccess__version___accountId__instances__instanceId__users__name__databases_.html">Grant User Access (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void GrantUserAccess(this IDatabaseService service, DatabaseInstanceId instanceId, DatabaseName databaseName, UserName userName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.GrantUserAccessAsync(instanceId, databaseName, userName, CancellationToken.None).Wait();
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
        /// Revoke access to a database for a particular user.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="databaseName">The database name. This is obtained from <see cref="Database.Name">Database.Name</see>.</param>
        /// <param name="userName">A <see cref="UserName"/> object identifying the database user. This is obtained from <see cref="UserConfiguration.UserName">UserConfiguration.UserName</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="databaseName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="userName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/DELETE_revokeUserAccess__version___accountId__instances__instanceId__users__name__databases__databaseName__.html">Revoke User Access (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RevokeUserAccess(this IDatabaseService service, DatabaseInstanceId instanceId, DatabaseName databaseName, UserName userName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RevokeUserAccessAsync(instanceId, databaseName, userName, CancellationToken.None).Wait();
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

        #region Flavors

        /// <summary>
        /// Get a collection of all database instance flavors available with the provider.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <returns>
        /// A collection of <see cref="DatabaseFlavor"/> objects describing the database instance flavors.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getFlavors__version___accountId__flavors_.html">List Flavors (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<DatabaseFlavor> ListFlavors(this IDatabaseService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListFlavorsAsync(CancellationToken.None).Result;
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
        /// Get a database instance flavor by ID.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="flavorId">The flavor ID. This is obtained from <see cref="DatabaseFlavor.Id">DatabaseFlavor.Id</see></param>
        /// <returns>
        /// A <see cref="DatabaseFlavor"/> object describing the flavor.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="flavorId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getFlavorById__version___accountId__flavors__flavorId__.html">List Flavor By ID (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static DatabaseFlavor GetFlavor(this IDatabaseService service, FlavorId flavorId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetFlavorAsync(flavorId, CancellationToken.None).Result;
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

        #region Backups

        /// <summary>
        /// Create a backup of a database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="configuration">A <see cref="BackupConfiguration"/> object containing the backup parameters.</param>
        /// <returns>
        /// A <see cref="Backup"/> object describing the backup.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="configuration"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_createBackup__version___accountId__backups_.html">Create Backup (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Backup CreateBackup(this IDatabaseService service, BackupConfiguration configuration)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.CreateBackupAsync(configuration, AsyncCompletionOption.RequestSubmitted, CancellationToken.None, null).Result;
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
        /// Get a collection of all backups for database instances in an account.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <returns>
        /// A collection of <see cref="Backup"/> objects describing the database instance backups.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getBackups__version___accountId__backups_.html">List Backups (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<Backup> ListBackups(this IDatabaseService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListBackupsAsync(CancellationToken.None).Result;
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
        /// Get information about a database instance backup by ID.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="backupId">The backup ID. This is obtained from <see cref="Backup.Id">Backup.Id</see></param>
        /// <returns>
        /// A <see cref="Backup"/> object describing the backup.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="backupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getBackupById__version___accountId__backups__backupId__.html">List Backup by ID (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static Backup GetBackup(this IDatabaseService service, BackupId backupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.GetBackupAsync(backupId, CancellationToken.None).Result;
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
        /// Remove and delete a database instance backup.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="backupId">The backup ID. This is obtained from <see cref="Backup.Id">Backup.Id</see></param>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="backupId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/DELETE_deleteBackup__version___accountId__backups__backupId__.html">Delete Backup (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static void RemoveBackup(this IDatabaseService service, BackupId backupId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                service.RemoveBackupAsync(backupId, CancellationToken.None).Wait();
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
        /// Get a collection of all backups for a particular database instance.
        /// </summary>
        /// <param name="service">The database service instance.</param>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <returns>
        /// A collection of <see cref="Backup"/> objects describing the backups for the database instance
        /// identified by <paramref name="instanceId"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="instanceId"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/GET_getBackups__version___accountId__backups_.html">List Backups (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<Backup> ListBackupsForInstance(this IDatabaseService service, DatabaseInstanceId instanceId)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return service.ListBackupsForInstanceAsync(instanceId, CancellationToken.None).Result;
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
