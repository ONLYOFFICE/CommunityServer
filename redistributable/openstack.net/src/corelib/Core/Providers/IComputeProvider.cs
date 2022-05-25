using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using net.openstack.Core.Domain;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Exceptions.Response;

namespace net.openstack.Core.Providers
{
    /// <summary>
    /// <para>DEPRECATED. Use <see cref="OpenStack.Compute.v2_1.ComputeService"/> or Rackspace.CloudServers.v2.CloudServerService (from the Rackspace NuGet package).</para>
    /// <para>Represents a provider for the OpenStack Compute service.</para>
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/">OpenStack Compute API v2 and Extensions Reference</seealso>
    //[Obsolete("This will be removed in v2.0. Use OpenStack.Compute.v2_1.ComputeService or Rackspace.CloudServers.v2.CloudServerService (from the Rackspace NuGet package).")]
    public interface IComputeProvider
    {
        /// <summary>
        /// Returns a list of basic information for servers in the account.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="changesSince"/> parameter is specified, servers which have been
        /// deleted since the specified time are returned by this method. Otherwise, deleted servers
        /// are not included in the list of servers returned by this method.
        /// </remarks>
        /// <param name="imageId">The image to filter the returned servers list. If
        /// the value is <see langword="null"/>, servers for all images are returned. This is
        /// specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
        /// <param name="flavorId">The flavor to filter the returned servers list. If
        /// the value is <see langword="null"/>, servers for all flavors are returned. This
        /// is specified as a flavor ID (see <see cref="Flavor.Id"/>) or a full URL.</param>
        /// <param name="name">Filters the list to those with a name that matches.
        /// If the value is <see langword="null"/>, servers are not filtered by name.</param>
        /// <param name="status">Filters the list to those with a status that matches.
        /// If the value is <see langword="null"/>, servers are not filtered by status. See
        /// <see cref="ServerState"/> for the allowed values.</param>
        /// <param name="markerId">The <see cref="ServerBase.Id"/> of the last item in the previous list. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="changesSince">Filters the list to those that have changed since the given date.
        /// If the value is <see langword="null"/>, servers are not filtered by timestamp.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="SimpleServer"/> objects describing the requested servers.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Servers-d1e2078.html">List Servers (OpenStack Compute API v2 and Extensions Reference)</seealso>
        IEnumerable<SimpleServer> ListServers(string imageId = null, string flavorId = null, string name = null, ServerState status = null, string markerId = null, int? limit = null, DateTimeOffset? changesSince = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Returns a list of detailed information servers for servers in the account.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="changesSince"/> parameter is specified, servers which have been
        /// deleted since the specified time are returned by this method. Otherwise, deleted servers
        /// are not included in the list of servers returned by this method.
        /// </remarks>
        /// <param name="imageId">The image to filter the returned servers list. If
        /// the value is <see langword="null"/>, servers for all images are returned. This is
        /// specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
        /// <param name="flavorId">The flavor to filter the returned servers list. If
        /// the value is <see langword="null"/>, servers for all flavors are returned. This
        /// is specified as a flavor ID (see <see cref="Flavor.Id"/>) or a full URL.</param>
        /// <param name="name">Filters the list to those with a name that matches.
        /// If the value is <see langword="null"/>, servers are not filtered by name.</param>
        /// <param name="status">Filters the list to those with a status that matches.
        /// If the value is <see langword="null"/>, servers are not filtered by status. See
        /// <see cref="ServerState"/> for the allowed values.</param>
        /// <param name="markerId">The <see cref="ServerBase.Id"/> of the last item in the previous list. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="changesSince">Filters the list to those that have changed since the given date.
        /// If the value is <see langword="null"/>, servers are not filtered by timestamp.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="Server"/> objects describing the requested servers.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Servers-d1e2078.html">List Servers (OpenStack Compute API v2 and Extensions Reference)</seealso>
        IEnumerable<Server> ListServersWithDetails(string imageId = null, string flavorId = null, string name = null, ServerState status = null, string markerId = null, int? limit = null, DateTimeOffset? changesSince = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Creates a new server.
        /// </summary>
        /// <remarks>
        /// This operation asynchronously provisions a new server. The progress of this operation depends on
        /// several factors including location of the requested image, network i/o, host load, and the selected
        /// flavor. The progress of the request can be checked by calling <see cref="GetDetails"/> and getting
        /// the value of <see cref="Server.Status"/> and <see cref="Server.Progress"/>.
        ///
        /// <note type="caller">
        /// This is the only time the server's admin password is returned. Make sure to retain the value.
        /// </note>
        ///
        /// <note>
        /// The <paramref name="diskConfig"/> parameter is ignored if the provider does not support the
        /// <see href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#diskconfig_attribute"><newTerm>disk configuration extension</newTerm></see>.
        /// </note>
        /// </remarks>
        /// <param name="cloudServerName">Name of the cloud server.</param>
        /// <param name="imageName">The image to use for the new server instance. This is
        /// specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
        /// <param name="flavor">The flavor to use for the new server instance. This
        /// is specified as a flavor ID (see <see cref="Flavor.Id"/>) or a full URL.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <param name="metadata">The metadata to associate with the server.</param>
        /// <param name="personality">A collection of <see cref="Personality"/> objects describing the paths and contents of files to inject in the target file system during the creation process. If the value is <see langword="null"/>, no files are injected.</param>
        /// <param name="attachToServiceNetwork"><see langword="true"/> if the private network will be attached to the newly created server; otherwise, <see langword="false"/>.</param>
        /// <param name="attachToPublicNetwork"><see langword="true"/> if the public network will be attached to the newly created server; otherwise, <see langword="false"/>.</param>
        /// <param name="networks">A collection of IDs of networks to attach to the server. This is obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="NewServer"/> instance containing the details for the newly created server.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cloudServerName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para><paramref name="imageName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="flavor"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="cloudServerName"/> is empty.
        /// <para>-or-</para>
        /// <para><paramref name="imageName"/> is empty.</para>
        /// <para>-or-</para>
        /// <para><paramref name="flavor"/> is empty.</para>
        /// <para>-or-</para>
        /// <para><paramref name="metadata"/> contains a value with a null or empty key.</para>
        /// <para>-or-</para>
        /// <para><paramref name="networks"/> contains a null or empty value.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="diskConfig"/>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/CreateServers.html">Create Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        NewServer CreateServer(string cloudServerName, string imageName, string flavor, DiskConfiguration diskConfig = null, Metadata metadata = null, Personality[] personality = null, bool attachToServiceNetwork = false, bool attachToPublicNetwork = false, IEnumerable<string> networks = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets the detailed information for a specific server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Server"/> object containing the details for the given server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Server_Details-d1e2623.html">Get Server Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
        Server GetDetails(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Updates the editable attributes for the specified server.
        /// </summary>
        /// <remarks>
        /// Server names are not guaranteed to be unique.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="name">The new name for the server. If the value is <see langword="null"/>, the server name is not changed.</param>
        /// <param name="accessIPv4">The new IP v4 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v4 address for the server. If the value is <see langword="null"/>, the server's IP v4 address is not updated.</param>
        /// <param name="accessIPv6">The new IP v6 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v6 address for the server. If the value is <see langword="null"/>, the server's IP v6 address is not updated.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the server was successfully updated; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv4"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv4"/> is not <see cref="AddressFamily.InterNetwork"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv6"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv6"/> is not <see cref="AddressFamily.InterNetworkV6"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/ServerUpdate.html">Update Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool UpdateServer(string serverId, string name = null, IPAddress accessIPv4 = null, IPAddress accessIPv6 = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Marks a server for asynchronous deletion.
        /// </summary>
        /// <remarks>
        /// The server deletion operation is completed asynchronously. The <see cref="WaitForServerDeleted"/>
        /// method may be used to block execution until the server is finally deleted.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the server was successfully marked for deletion; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Server-d1e2883.html">Delete Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool DeleteServer(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists all networks and server addresses associated with a specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="ServerAddresses"/> object containing the list of network addresses for the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses-d1e3014.html">List Addresses (OpenStack Compute API v2 and Extensions Reference)</seealso>
        ServerAddresses ListAddresses(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists addresses associated with a specified server and network.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="network">The network label. This is obtained from <see cref="CloudNetwork.Label">CloudNetwork.Label</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="IPAddress"/> containing the network addresses associated with the server on the specified network.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="network"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="network"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses_by_Network-d1e3118.html">List Addresses by Network (OpenStack Compute API v2 and Extensions Reference)</seealso>
        IEnumerable<IPAddress> ListAddressesByNetwork(string serverId, string network, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Changes the administrator password for a specified server.
        /// </summary>
        /// <remarks>
        /// The password change operation is performed asynchronously. If the password does not
        /// meet the server's complexity requirements, the server may end up in an <see cref="ServerState.Error"/>
        /// state. In this case, the client may call <see cref="ChangeAdministratorPassword"/> again to
        /// select a new password.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="password">The new administrator password.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the administrator password change operation was successfully started; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Change_Password-d1e3234.html">Change Administrator Password (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool ChangeAdministratorPassword(string serverId, string password, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Initiates an asynchronous reboot operation on the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="rebootType">The type of reboot to perform. See <see cref="RebootType"/> for predefined values.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the reboot operation was successfully initiated; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="rebootType"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the specified <paramref name="rebootType"/>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Reboot_Server-d1e3371.html">Reboot Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool RebootServer(string serverId, RebootType rebootType, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Initiates an asynchronous rebuild of the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="serverName">The new name for the server. If the value is <see langword="null"/>, the server name is not changed.</param>
        /// <param name="imageName">The image to rebuild the server from. This is specified as an image ID (see <see cref="SimpleServerImage.Id"/>) or a full URL.</param>
        /// <param name="flavor">The new flavor for server. This is obtained from <see cref="Flavor.Id"/>.</param>
        /// <param name="adminPassword">The new admin password for the server.</param>
        /// <param name="accessIPv4">The new IP v4 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v4 address for the server. If the value is <see langword="null"/>, the server's IP v4 address is not updated.</param>
        /// <param name="accessIPv6">The new IP v6 address for the server, or <see cref="IPAddress.None"/> to remove the configured IP v6 address for the server. If the value is <see langword="null"/>, the server's IP v6 address is not updated.</param>
        /// <param name="metadata">The list of metadata to associate with the server. If the value is <see langword="null"/>, the metadata associated with the server is not changed during the rebuild operation.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <param name="personality">The path and contents of a file to inject in the target file system during the rebuild operation. If the value is <see langword="null"/>, no file is injected.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Server"/> object containing the details for the given server.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="imageName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="adminPassword"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="imageName"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="adminPassword"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv4"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv4"/> is not <see cref="AddressFamily.InterNetwork"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessIPv6"/> is not <see cref="IPAddress.None"/> and the <see cref="AddressFamily"/> of <paramref name="accessIPv6"/> is not <see cref="AddressFamily.InterNetworkV6"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="diskConfig"/>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Rebuild_Server-d1e3538.html">Rebuild Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        Server RebuildServer(string serverId, string serverName, string imageName, string flavor, string adminPassword, IPAddress accessIPv4 = null, IPAddress accessIPv6 = null, Metadata metadata = null, DiskConfiguration diskConfig = null, Personality personality = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Initiates an asynchronous resize of the specified server. A server resize is performed by
        /// specifying a new <see cref="Flavor"/> for the server.
        /// </summary>
        /// <remarks>
        /// Following a resize operation, the original server is not immediately removed. After testing
        /// if the resulting server is operating successfully, a call should be made to <see cref="ConfirmServerResize"/>
        /// to keep the resized server, or to <see cref="RevertServerResize"/> to revert to the original server.
        /// If 24 hours pass and neither of these methods is called, the server will be automatically confirmed.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="serverName">The new name for the resized server.</param>
        /// <param name="flavor">The new flavor. This is obtained from <see cref="Flavor.Id">Flavor.Id</see>.</param>
        /// <param name="diskConfig">The disk configuration. If the value is <see langword="null"/>, the default configuration for the specified image is used.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the resize operation is successfully started; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="serverName"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="serverName"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="flavor"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="diskConfig"/>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Resize_Server-d1e3707.html">Resize Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool ResizeServer(string serverId, string serverName, string flavor, DiskConfiguration diskConfig = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Confirms a completed asynchronous server resize action.
        /// </summary>
        /// <remarks>
        /// If a server resize operation is not manually confirmed or reverted within 24 hours,
        /// the operation is automatically confirmed.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the resize operation was confirmed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Confirm_Resized_Server-d1e3868.html">Confirm Resized Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool ConfirmServerResize(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Cancels and reverts a server resize action.
        /// </summary>
        /// <remarks>
        /// If a server resize operation is not manually confirmed or reverted within 24 hours,
        /// the operation is automatically confirmed.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the resize operation was reverted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Revert_Resized_Server-d1e4024.html">Revert Resized Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool RevertServerResize(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Places a server in rescue mode.
        /// </summary>
        /// <remarks>
        /// This operation is completed asynchronously. To wait for the server to enter rescue mode,
        /// call <see cref="O:net.openstack.Core.Providers.IComputeProvider.WaitForServerState"/> with the state <see cref="ServerState.Rescue"/>.
        ///
        /// <note>
        /// The provider may limit the duration of rescue mode, after which the rescue image is destroyed
        /// and the server attempts to reboot. Rescue mode may be explicitly exited at any time by
        /// calling <see cref="UnRescueServer"/>.
        /// </note>
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>The root password assigned for use during rescue mode.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/rescue_mode.html">Rescue Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        string RescueServer(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Takes a server out of rescue mode.
        /// </summary>
        /// <remarks>
        /// This operation is completed asynchronously. To wait for the server to exit rescue mode,
        /// call <see cref="WaitForServerActive"/>.
        ///
        /// <note>
        /// The provider may limit the duration of rescue mode, after which the rescue image is destroyed
        /// and the server attempts to reboot. Rescue mode may be explicitly exited at any time by
        /// calling <see cref="UnRescueServer"/>.
        /// </note>
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the server exited rescue mode; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/exit_rescue_mode.html">Unrescue Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        bool UnRescueServer(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Creates a new snapshot image for a specified server at its current state.
        /// </summary>
        /// <remarks>
        /// The server snapshot process is completed asynchronously. To wait for the image
        /// to be completed, you may call <see cref="WaitForImageActive"/>.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="imageName">Name of the new image.</param>
        /// <param name="metadata">The metadata to associate to the new image.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the image creation process was successfully started; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="imageName"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="imageName"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_Image-d1e4655.html">Create Image (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool CreateImage(string serverId, string imageName, Metadata metadata = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Attaches a volume to the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id"/>.</param>
        /// <param name="volumeId">The volume ID. This is obtained from <see cref="Volume.Id"/>.</param>
        /// <param name="storageDevice">The name of the device, such as <localUri>/dev/xvdb</localUri>. If the value is <see langword="null"/>, an automatically generated device name will be used.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="ServerVolume"/> object containing the details about the volume.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeId"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Attach_Volume_to_Server.html">Attach Volume to Server (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        ServerVolume AttachServerVolume(string serverId, string volumeId, string storageDevice = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists the volume attachments for the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id"/>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="ServerVolume"/> objects describing the volumes attached to the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/List_Volume_Attachments.html">List Volume Attachments (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        IEnumerable<ServerVolume> ListServerVolumes(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets detailed information about the specified server-attached volume.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id"/>.</param>
        /// <param name="volumeId">The volume attachment ID. This is obtained from <see cref="ServerVolume.Id">ServerVolume.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="ServerVolume"/> object containing details about the volume attachment.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeId"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Get_Volume_Attachment_Details.html">Get Volume Attachment Details (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        ServerVolume GetServerVolumeDetails(string serverId, string volumeId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Detaches the specified volume from the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="volumeId">The volume attachment ID. This is obtained from <see cref="ServerVolume.Id">ServerVolume.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the volume was successfully detached; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeId"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Delete_Volume_Attachment.html">Delete Volume Attachment (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
        bool DetachServerVolume(string serverId, string volumeId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists the virtual interfaces for the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="VirtualInterface"/> objects describing the virtual interfaces for the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        IEnumerable<VirtualInterface> ListVirtualInterfaces(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Creates a virtual interface for the specified network and attaches the network to the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="networkId">The network ID. This is obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="VirtualInterface"/> object containing the details of the newly-created virtual network.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="networkId"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/api_create_virtual_interface.html">Create Virtual Interface (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        VirtualInterface CreateVirtualInterface(string serverId, string networkId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Deletes the specified virtual interface from the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="virtualInterfaceId">The virtual interface ID. This is obtained from <see cref="VirtualInterface.Id">VirtualInterface.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the virtual interface was successfully removed from the server; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualInterfaceId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualInterfaceId"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/delete_virt_interface_api.html">Delete Virtual Interface (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        bool DeleteVirtualInterface(string serverId, string virtualInterfaceId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists basic information for all available flavors.
        /// </summary>
        /// <param name="minDiskInGB">Filters the list of flavors to those with the specified minimum number of gigabytes of disk storage. If the value is <see langword="null"/>, the results are not filtered by storage space.</param>
        /// <param name="minRamInMB">Filters the list of flavors to those with the specified minimum amount of RAM in megabytes. If the value is <see langword="null"/>, the results are not filtered by memory capacity.</param>
        /// <param name="markerId">The <see cref="Flavor.Id"/> of the last item in the previous list. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>.</param>
        /// <param name="limit">The maximum number of items to return. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="Flavor"/> objects describing the available flavors.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="minDiskInGB"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="minRamInMB"/> is less than 0.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than 0.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="ListFlavorsWithDetails"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Flavors-d1e4188.html">List Flavors (OpenStack Compute API v2 and Extensions Reference)</seealso>
        IEnumerable<Flavor> ListFlavors(int? minDiskInGB = null, int? minRamInMB = null, string markerId = null, int? limit = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists full details for all available flavors.
        /// </summary>
        /// <param name="minDiskInGB">Filters the list of flavors to those with the specified minimum number of gigabytes of disk storage. If the value is <see langword="null"/>, the results are not filtered by storage space.</param>
        /// <param name="minRamInMB">Filters the list of flavors to those with the specified minimum amount of RAM in megabytes. If the value is <see langword="null"/>, the results are not filtered by memory capacity.</param>
        /// <param name="markerId">The <see cref="Flavor.Id"/> of the last item in the previous list. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>.</param>
        /// <param name="limit">The maximum number of items to return. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="FlavorDetails"/> objects containing detailed information for the available flavors.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="minDiskInGB"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="minRamInMB"/> is less than 0.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="limit"/> is less than 0.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="ListFlavors"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Flavors-d1e4188.html">List Flavors (OpenStack Compute API v2 and Extensions Reference)</seealso>
        IEnumerable<FlavorDetails> ListFlavorsWithDetails(int? minDiskInGB = null, int? minRamInMB = null, string markerId = null, int? limit = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets details for the specified flavor.
        /// </summary>
        /// <param name="id">The flavor ID. This is obtained from <see cref="Flavor.Id">Flavor.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="FlavorDetails"/> object containing details of the flavor.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Flavor_Details-d1e4317.html">Get Flavor Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
        FlavorDetails GetFlavor(string id, string region = null, CloudIdentity identity = null);

        // Images

        /// <summary>
        /// Lists basic information for all available images.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="changesSince"/> parameter is not specified, deleted images are
        /// not returned by this method. If the <paramref name="changesSince"/> parameter is specified,
        /// the result includes images which were deleted since the specified time.
        /// </remarks>
        /// <param name="server">Filters the list of images by server. This is specified as a server ID (see <see cref="ServerBase.Id"/>) or a full URL. If the value is <see langword="null"/>, the results are not filtered by ID.</param>
        /// <param name="imageName">Filters the list of images by image name. If the value is <see langword="null"/>, the results are not filtered by name.</param>
        /// <param name="imageStatus">Filters the list of images by status. If the value is <see langword="null"/>, the results are not filtered by status.</param>
        /// <param name="changesSince">Filters the list of images to those that have changed since the specified time. If the value is <see langword="null"/>, the results are not filtered by timestamp.</param>
        /// <param name="markerId">The <see cref="SimpleServerImage.Id"/> of the last item in the previous list. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, the results start at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="imageType">Filters base Rackspace images or any custom server images that you have created. If the value is <see langword="null"/>, the results are not filtered by image type.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="SimpleServerImage"/> objects containing basic information for the images.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="ListImagesWithDetails"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Images-d1e4435.html">List Images (OpenStack Compute API v2 and Extensions Reference)</seealso>
        IEnumerable<SimpleServerImage> ListImages(string server = null, string imageName = null, ImageState imageStatus = null, DateTimeOffset? changesSince = null, string markerId = null, int? limit = null, ImageType imageType = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists detailed information for all available images.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="changesSince"/> parameter is not specified, deleted images are
        /// not returned by this method. If the <paramref name="changesSince"/> parameter is specified,
        /// the result includes images which were deleted since the specified time.
        /// </remarks>
        /// <param name="server">Filters the list of images by server. This is specified as a server ID (see <see cref="ServerBase.Id"/>) or a full URL. If the value is <see langword="null"/>, the results are not filtered by ID.</param>
        /// <param name="imageName">Filters the list of images by image name. If the value is <see langword="null"/>, the results are not filtered by name.</param>
        /// <param name="imageStatus">Filters the list of images by status. If the value is <see langword="null"/>, the results are not filtered by status.</param>
        /// <param name="changesSince">Filters the list of images to those that have changed since the specified time. If the value is <see langword="null"/>, the results are not filtered by timestamp.</param>
        /// <param name="markerId">The <see cref="SimpleServerImage.Id"/> of the last item in the previous list. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, the results start at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for <see href="http://docs.openstack.org/api/openstack-compute/2/content/Paginated_Collections-d1e664.html">pagination</see>. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="imageType">Filters base Rackspace images or any custom server images that you have created. If the value is <see langword="null"/>, the results are not filtered by image type.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="ServerImage"/> objects containing detailed information for the images.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than 0.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="ListImages"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Images-d1e4435.html">List Images (OpenStack Compute API v2 and Extensions Reference)</seealso>
        IEnumerable<ServerImage> ListImagesWithDetails(string server = null, string imageName = null, ImageState imageStatus = null, DateTimeOffset? changesSince = null, string markerId = null, int? limit = null, ImageType imageType = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets detailed information for the specified image.
        /// </summary>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="ServerImage"/> object containing detailed information about the specified image.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="imageId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="imageId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Image_Details-d1e4848.html">Get Image Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
        ServerImage GetImage(string imageId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Deletes the specified image.
        /// </summary>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the image was successfully deleted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="imageId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="imageId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Image-d1e4957.html">Delete Image (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool DeleteImage(string imageId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets the metadata associated with the specified server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Metadata"/> object containing the metadata associated with the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serverId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="serverId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Metadata-d1e5089.html">List Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        Metadata ListServerMetadata(string serverId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Sets the metadata associated with the specified server, replacing any existing metadata.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="metadata">The metadata to associate with the server.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any values with empty keys.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Replace_Metadata-d1e5358.html">Set Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool SetServerMetadata(string serverId, Metadata metadata, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Updates the metadata for the specified server.
        /// </summary>
        /// <remarks>
        /// For each item in <paramref name="metadata"/>, if the key exists, the value is updated; otherwise, the item is added.
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="metadata">The server metadata to update.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any values with empty keys.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Update_Metadata-d1e5208.html">Update Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool UpdateServerMetadata(string serverId, Metadata metadata, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets the specified metadata item.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>The metadata value for the associated with the server for the specified key.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Metadata_Item-d1e5507.html">Get Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        string GetServerMetadataItem(string serverId, string key, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Sets the value for the specified metadata item. If the key already exists, it is updated; otherwise, a new metadata item is added.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata for the server was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Update_a_Metadata_Item-d1e5633.html">Set Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool SetServerMetadataItem(string serverId, string key, string value, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Deletes the specified metadata item from the server.
        /// </summary>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata item was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Metadata_Item-d1e5790.html">Delete Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool DeleteServerMetadataItem(string serverId, string key, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets the metadata associated with the specified image.
        /// </summary>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Metadata"/> object containing the metadata associated with the image.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="imageId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="imageId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Metadata-d1e5089.html">List Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        Metadata ListImageMetadata(string imageId, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Sets the metadata associated with the specified image, replacing any existing metadata.
        /// </summary>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="metadata">The metadata to associate with the image.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any values with empty keys.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Replace_Metadata-d1e5358.html">Set Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool SetImageMetadata(string imageId, Metadata metadata, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Updates the metadata for the specified image.
        /// </summary>
        /// <remarks>
        /// For each item in <paramref name="metadata"/>, if the key exists, the value is updated; otherwise, the item is added.
        /// </remarks>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="metadata">The image metadata to update.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any values with empty keys.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Update_Metadata-d1e5208.html">Update Metadata (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool UpdateImageMetadata(string imageId, Metadata metadata, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Gets the specified metadata item.
        /// </summary>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>The metadata value for the associated with the image for the specified key.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Metadata_Item-d1e5507.html">Get Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        string GetImageMetadataItem(string imageId, string key, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Sets the value for the specified metadata item. If the key already exists, it is updated; otherwise, a new metadata item is added.
        /// </summary>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The new value for the metadata item.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata for the image was successfully updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Create_or_Update_a_Metadata_Item-d1e5633.html">Set Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool SetImageMetadataItem(string imageId, string key, string value, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Deletes the specified metadata item from the image.
        /// </summary>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the metadata item was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="key"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Metadata_Item-d1e5790.html">Delete Metadata Item (OpenStack Compute API v2 and Extensions Reference)</seealso>
        bool DeleteImageMetadataItem(string imageId, string key, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the server to enter a specified state.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters either the expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="expectedState">The expected state.</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the server enters.</param>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Server"/> object containing the server details, including the final <see cref="Server.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedState"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ServerEnteredErrorStateException">If the method returned due to the server entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        Server WaitForServerState(string serverId, ServerState expectedState, ServerState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the server to enter any one of a set of specified states.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters either an expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="expectedStates">The expected state(s).</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the server enters.</param>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Server"/> object containing the server details, including the final <see cref="Server.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedStates"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedStates"/> is empty.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ServerEnteredErrorStateException">If the method returned due to the server entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        Server WaitForServerState(string serverId, ServerState[] expectedStates, ServerState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the server to enter the <see cref="ServerState.Active"/> state.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters the <see cref="ServerState.Active"/> state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Server"/> object containing the server details, including the final <see cref="Server.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        Server WaitForServerActive(string serverId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the server to enter the <see cref="ServerState.Deleted"/> state or to be removed.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the server enters the <see cref="ServerState.Deleted"/> state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="serverId">The server ID. This is obtained from <see cref="ServerBase.Id">ServerBase.Id</see>.</param>
        /// <param name="refreshCount">Number of times to poll the server's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the server status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="Server.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="serverId"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="serverId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        void WaitForServerDeleted(string serverId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the image to enter a specified state.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the image enters either the expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="expectedState">The expected state.</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the image enters.</param>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="ServerImage"/> object containing the image details, including the final <see cref="ServerImage.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedState"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ImageEnteredErrorStateException">If the method returned due to the image entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        ServerImage WaitForImageState(string imageId, ImageState expectedState, ImageState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the image to enter any one of a set of specified states.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the image enters either an expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="expectedStates">The expected state(s).</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the image enters.</param>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="ServerImage"/> object containing the image details, including the final <see cref="ServerImage.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedStates"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="expectedStates"/> is empty.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ImageEnteredErrorStateException">If the method returned due to the image entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        ServerImage WaitForImageState(string imageId, ImageState[] expectedStates, ImageState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the image to enter the <see cref="ImageState.Active"/> state.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// This is a blocking operation and will not return until the image enters either the
        /// <see cref="ImageState.Active"/> state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="ServerImage"/> object containing the image details, including the final <see cref="ServerImage.Status"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        ServerImage WaitForImageActive(string imageId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);

        /// <summary>
        /// Waits for the image to enter the <see cref="ImageState.Deleted"/> state or to be removed.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// This is a blocking operation and will not return until the image enters either the
        /// <see cref="ImageState.Deleted"/> state, an error state, is removed, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="imageId">The image ID. This is obtained from <see cref="SimpleServerImage.Id">SimpleServerImage.Id</see>.</param>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <param name="region">The region in which to execute this action. If not specified, the user's default region will be used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="imageId"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="imageId"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the given <paramref name="identity"/> type.
        /// <para>-or-</para>
        /// <para>The specified <paramref name="region"/> is not supported.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.
        /// <para>-or-</para>
        /// <para>If <paramref name="region"/> is <see langword="null"/> and no default region is available for the provider.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        void WaitForImageDeleted(string imageId, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null, string region = null, CloudIdentity identity = null);
    }
}
