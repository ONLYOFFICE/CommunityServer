using System;
using System.Collections.Generic;

using net.openstack.Core.Domain;
using net.openstack.Core.Exceptions.Response;
using net.openstack.Core.Providers;

namespace net.openstack.Providers.Rackspace
{
    /// <summary>
    /// Represents an identity provider that implements Rackspace-specific extensions to the
    /// OpenStack Identity API.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/auth/api/v2.0/auth-client-devguide/content/index.html">Rackspace Cloud Identity Client Developer Guide - API v2.0</seealso>
    public interface IExtendedCloudIdentityProvider : IIdentityProvider
    {
        /// <summary>
        /// Lists all roles.
        /// <note type="warning">The behavior of this API method is not defined. Do not use.</note>
        /// </summary>
        /// <param name="serviceId">The "serviceId".</param>
        /// <param name="marker">The index of the last item in the previous list. Used for pagination. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for pagination. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="Role"/> objects describing the requested roles.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than 0.</exception>
        /// <exception cref="NotSupportedException">If the provider does not support the given <paramref name="identity"/> type.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudIdentityProvider"/>
        /// implementation of <see cref="IExtendedCloudIdentityProvider"/>. For more information about creating the provider, see
        /// <see cref="CloudIdentityProvider(CloudIdentity)"/>.</para>
        /// <code source="..\Samples\CSharpCodeSamples\IdentityProviderExamples.cs" region="ListRoles" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\IdentityProviderExamples.vb" region="ListRoles" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\IdentityProviderExamples.cpp" region="ListRoles" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\IdentityProviderExamples.fs" region="ListRoles" language="fs"/>
        /// </example>
        /// <seealso href="http://docs.rackspace.com/openstack-extensions/auth/OS-KSADM-admin-devguide/content/GET_listRoles_v2.0_OS-KSADM_roles_Admin_API_Service_Developer_Operations-d1e1357.html">List Roles (Rackspace OS-KSADM Extension - API v2.0)</seealso>
        IEnumerable<Role> ListRoles(string serviceId = null, int? marker = null, int? limit = null, CloudIdentity identity = null);

        /// <summary>
        /// Lists all users for a given role.
        /// </summary>
        /// <param name="roleId">The role ID. This is obtained from <see cref="Role.Id">Role.Id</see>.</param>
        /// <param name="enabled">Allows you to filter enabled or un-enabled users. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="marker">The index of the last item in the previous list. Used for pagination. If the value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">Indicates the maximum number of items to return. Used for pagination. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A collection of <see cref="Role"/> objects describing the requested roles.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than 0 or greater than 1000.</exception>
        /// <exception cref="NotSupportedException">If the provider does not support the given <paramref name="identity"/> type.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        IEnumerable<User> ListUsersByRole(string roleId, bool? enabled = null, int? marker = null, int? limit = null, CloudIdentity identity = null);

        /// <summary>
        /// Create a new role.
        /// </summary>
        /// <param name="name">The name for the new role.</param>
        /// <param name="description">The description for the new role.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Role"/> containing the details of the added role.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_addRole_v2.0_OS-KSADM_roles_Role_Operations_OS-KSADM.html">Add Role (OpenStack Identity Service API v2.0 Reference)</seealso>
        Role AddRole(string name, string description, CloudIdentity identity = null);

        /// <summary>
        /// Gets details about the specified role.
        /// </summary>
        /// <param name="roleId">The role ID. This is obtained from <see cref="Role.Id">Role.Id</see>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="Role"/> containing the details of the role.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="roleId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="roleId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_getRoleByName_v2.0_OS-KSADM_roles_Role_Operations_OS-KSADM.html">Get Role by Name (OpenStack Identity Service API v2.0 Reference)</seealso>
        Role GetRole(string roleId, CloudIdentity identity = null);

        /// <summary>
        /// Adds the specified global role to the user.
        /// </summary>
        /// <param name="userId">The user ID. This is obtained from <see cref="NewUser.Id"/> or <see cref="User.Id"/>.</param>
        /// <param name="roleId">The role ID. This is obtained from <see cref="Role.Id"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the role was removed from the user; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="userId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="roleId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="userId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="roleId"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudIdentityProvider"/>
        /// implementation of <see cref="IExtendedCloudIdentityProvider"/>. For more information about creating the provider, see
        /// <see cref="CloudIdentityProvider(CloudIdentity)"/>.</para>
        /// <code source="..\Samples\CSharpCodeSamples\IdentityProviderExamples.cs" region="AddRoleToUser" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\IdentityProviderExamples.vb" region="AddRoleToUser" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\IdentityProviderExamples.cpp" region="AddRoleToUser" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\IdentityProviderExamples.fs" region="AddRoleToUser" language="fs"/>
        /// </example>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/PUT_addUserRole_v2.0_users__userId__roles_OS-KSADM__roleId__.html">Add Global Role to User (OpenStack Identity Service API v2.0 Reference)</seealso>
        bool AddRoleToUser(string userId, string roleId, CloudIdentity identity = null);

        /// <summary>
        /// Deletes the specified global role from the user.
        /// </summary>
        /// <param name="userId">The user ID. This is obtained from <see cref="NewUser.Id"/> or <see cref="User.Id"/>.</param>
        /// <param name="roleId">The role ID. This is obtained from <see cref="Role.Id"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the role was removed from the user; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="userId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="roleId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="userId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="roleId"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudIdentityProvider"/>
        /// implementation of <see cref="IExtendedCloudIdentityProvider"/>. For more information about creating the provider, see
        /// <see cref="CloudIdentityProvider(CloudIdentity)"/>.</para>
        /// <code source="..\Samples\CSharpCodeSamples\IdentityProviderExamples.cs" region="DeleteRoleFromUser" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\IdentityProviderExamples.vb" region="DeleteRoleFromUser" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\IdentityProviderExamples.cpp" region="DeleteRoleFromUser" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\IdentityProviderExamples.fs" region="DeleteRoleFromUser" language="fs"/>
        /// </example>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/DELETE_deleteUserRole_v2.0_users__userId__roles_OS-KSADM__roleId__.html">Delete Global Role from User (OpenStack Identity Service API v2.0 Reference)</seealso>
        bool DeleteRoleFromUser(string userId, string roleId, CloudIdentity identity = null);

        /// <summary>
        /// Updates the API key for the specified user.
        /// </summary>
        /// <remarks>
        /// This method is a Rackspace-specific usage scenario for the
        /// <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials</see>
        /// method, where the credentials are always specified in the form of an API key.
        /// </remarks>
        /// <returns>A <see cref="UserCredential"/> object containing the updated user information.</returns>
        /// <param name="userId">The user ID. This is obtained from <see cref="NewUser.Id"/> or <see cref="User.Id"/>.</param>
        /// <param name="apiKey">The new API key.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="userId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="apiKey"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="userId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="apiKey"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
        UserCredential UpdateUserCredentials(string userId, string apiKey, CloudIdentity identity = null);

        /// <summary>
        /// Deletes API key credentials for the specified user.
        /// </summary>
        /// <remarks>
        /// This method is a Rackspace-specific usage scenario for the
        /// <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/DELETE_deleteUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Delete User Credentials</see>
        /// method, where the credentials are always specified in the form of an API key.
        /// </remarks>
        /// <param name="userId">The user ID. This is obtained from <see cref="NewUser.Id"/> or <see cref="User.Id"/>.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the credentials were removed from the user; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="userId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="userId"/> is empty.</exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/DELETE_deleteUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Delete User Credentials</seealso>
        bool DeleteUserCredentials(string userId, CloudIdentity identity = null);

        /// <summary>
        /// Sets the password for the specified user.
        /// </summary>
        /// <param name="userId">The user ID. This is obtained from <see cref="NewUser.Id"/> or <see cref="User.Id"/>.</param>
        /// <param name="password">The new password.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="userId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="userId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
        bool SetUserPassword(string userId, string password, CloudIdentity identity = null);

        /// <summary>
        /// Updates the username and password for the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The new password.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="user"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="password"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="user"/>.<see cref="User.Id"/> is <see langword="null"/> or empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="user"/>.<see cref="User.Username"/> is <see langword="null"/> or empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
        bool SetUserPassword(User user, string password, CloudIdentity identity = null);

        /// <summary>
        /// Updates the username and password for the specified user.
        /// </summary>
        /// <param name="userId">The user ID. This is obtained from <see cref="NewUser.Id"/> or <see cref="User.Id"/>.</param>
        /// <param name="username">The new username.</param>
        /// <param name="password">The new password.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="userId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="username"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="userId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="username"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
        bool SetUserPassword(string userId, string username, string password, CloudIdentity identity = null);

        /// <summary>
        /// Updates the username and API key for the specified user.
        /// </summary>
        /// <remarks>
        /// This method is a Rackspace-specific usage scenario for the
        /// <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials</see>
        /// method, where the credentials are always specified in the form of an API key.
        /// </remarks>
        /// <param name="user">The user.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="UserCredential"/> object containing the updated user information.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="user"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="apiKey"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="apiKey"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="user"/>.<see cref="User.Id"/> is <see langword="null"/> or empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="user"/>.<see cref="User.Username"/> is <see langword="null"/> or empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
        UserCredential UpdateUserCredentials(User user, string apiKey, CloudIdentity identity = null);

        /// <summary>
        /// Updates the username and API key for the specified user.
        /// </summary>
        /// <remarks>
        /// This method is a Rackspace-specific usage scenario for the
        /// <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials</see>
        /// method, where the credentials are always specified in the form of an API key.
        /// </remarks>
        /// <param name="userId">The user ID. This is obtained from <see cref="NewUser.Id"/> or <see cref="User.Id"/>.</param>
        /// <param name="username">The new username.</param>
        /// <param name="apiKey">The new API key.</param>
        /// <param name="identity">The cloud identity to use for this request. If not specified, the default identity for the current provider instance will be used.</param>
        /// <returns>A <see cref="UserCredential"/> object containing the updated user information.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="userId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="username"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="apiKey"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="userId"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="username"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="apiKey"/> is empty.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If the provider does not support the <see href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/Admin_API_Service_Developer_Operations-d1e1357.html">OS-KSADM Admin Extension</see>.
        /// <para>-or-</para>
        /// <para>If the provider does not support the given <paramref name="identity"/> type.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">If <paramref name="identity"/> is <see langword="null"/> and no default identity is available for the provider.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
        UserCredential UpdateUserCredentials(string userId, string username, string apiKey, CloudIdentity identity = null);
    }
}
