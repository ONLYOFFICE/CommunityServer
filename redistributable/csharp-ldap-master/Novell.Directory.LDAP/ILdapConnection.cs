using System;

namespace Novell.Directory.Ldap
{
    public interface ILdapConnection : IDisposable
    {
        /// <summary>
        ///     Starts Transport Layer Security (TLS) protocol on this connection
        ///     to enable session privacy.
        ///     This affects the LdapConnection object and all cloned objects. A
        ///     socket factory that implements LdapTLSSocketFactory must be set on the
        ///     connection.
        /// </summary>
        /// <exception>
        ///     LdapException Thrown if TLS cannot be started.  If a
        ///     SocketFactory has been specified that does not implement
        ///     LdapTLSSocketFactory an LdapException is thrown.
        /// </exception>
        void StartTls();

        /// <summary>
        ///     Stops Transport Layer Security(TLS) on the LDAPConnection and reverts
        ///     back to an anonymous state.
        ///     @throws LDAPException This can occur for the following reasons:
        ///     <UL>
        ///         <LI>StartTLS has not been called before stopTLS</LI>
        ///         <LI>
        ///             There exists outstanding messages that have not received all
        ///             responses
        ///         </LI>
        ///         <LI>The sever was not able to support the operation</LI>
        ///     </UL>
        ///     <p>
        ///         Note: The Sun and IBM implementions of JSSE do not currently allow
        ///         stopping TLS on an open Socket.  In order to produce the same results
        ///         this method currently disconnects the socket and reconnects, giving
        ///         the application an anonymous connection to the server, as required
        ///         by StopTLS
        ///     </p>
        /// </summary>
        void StopTls();

        /// <summary>
        ///     Synchronously adds an entry to the directory.
        /// </summary>
        /// <param name="entry">
        ///     LdapEntry object specifying the distinguished
        ///     name and attributes of the new entry.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Add(LdapEntry entry);

        /// <summary>
        ///     Synchronously adds an entry to the directory, using the specified
        ///     constraints.
        /// </summary>
        /// <param name="entry">
        ///     LdapEntry object specifying the distinguished
        ///     name and attributes of the new entry.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Add(LdapEntry entry, LdapConstraints cons);

        /// <summary>
        ///     Asynchronously adds an entry to the directory.
        /// </summary>
        /// <param name="entry">
        ///     LdapEntry object specifying the distinguished
        ///     name and attributes of the new entry.
        /// </param>
        /// <param name="queue">
        ///     Handler for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Add(LdapEntry entry, LdapResponseQueue queue);

        /// <summary>
        ///     Asynchronously adds an entry to the directory, using the specified
        ///     constraints.
        /// </summary>
        /// <param name="entry">
        ///     LdapEntry object specifying the distinguished
        ///     name and attributes of the new entry.
        /// </param>
        /// <param name="queue">
        ///     Handler for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Add(LdapEntry entry, LdapResponseQueue queue, LdapConstraints cons);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) as an Ldapv3 bind, using the specified name and
        ///     password.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Bind(string dn, string passwd);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password,
        ///     and Ldap version.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Bind(int version, string dn, string passwd);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) as an Ldapv3 bind, using the specified name,
        ///     password, and constraints.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Bind(string dn, string passwd, LdapConstraints cons);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password, Ldap version,
        ///     and constraints.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Bind(int version, string dn, string passwd, LdapConstraints cons);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password,
        ///     and Ldap version.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The version of the Ldap protocol to use
        ///     in the bind, use Ldap_V3.  Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Bind(int version, string dn, sbyte[] passwd);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password, Ldap version,
        ///     and constraints.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Bind(int version, string dn, sbyte[] passwd, LdapConstraints cons);

        /// <summary>
        ///     Asynchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password, Ldap
        ///     version, and queue.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <param name="queue">
        ///     Handler for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Bind(int version, string dn, sbyte[] passwd, LdapResponseQueue queue);

        /// <summary>
        ///     Asynchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password, Ldap
        ///     version, queue, and constraints.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     had already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <param name="queue">
        ///     Handler for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Bind(int version, string dn, sbyte[] passwd, LdapResponseQueue queue, LdapConstraints cons);

        /// <summary>
        ///     Connects to the specified host and port.
        ///     If this LdapConnection object represents an open connection, the
        ///     connection is closed first before the new connection is opened.
        ///     At this point, there is no authentication, and any operations are
        ///     conducted as an anonymous client.
        ///     When more than one host name is specified, each host is contacted
        ///     in turn until a connection can be established.
        /// </summary>
        /// <param name="host">
        ///     A host name or a dotted string representing the IP address
        ///     of a host running an Ldap server. It may also
        ///     contain a list of host names, space-delimited. Each host
        ///     name can include a trailing colon and port number.
        /// </param>
        /// <param name="port">
        ///     The TCP or UDP port number to connect to or contact.
        ///     The default Ldap port is 389. The port parameter is
        ///     ignored for any host hame which includes a colon and
        ///     port number.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Connect(string host, int port);

        /// <summary>
        ///     Synchronously deletes the entry with the specified distinguished name
        ///     from the directory.
        ///     Note: A Delete operation will not remove an entry that contains
        ///     subordinate entries, nor will it dereference alias entries.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to delete.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Delete(string dn);

        /// <summary>
        ///     Synchronously deletes the entry with the specified distinguished name
        ///     from the directory, using the specified constraints.
        ///     Note: A Delete operation will not remove an entry that contains
        ///     subordinate entries, nor will it dereference alias entries.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to delete.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Delete(string dn, LdapConstraints cons);

        /// <summary>
        ///     Asynchronously deletes the entry with the specified distinguished name
        ///     from the directory and returns the results to the specified queue.
        ///     Note: A Delete operation will not remove an entry that contains
        ///     subordinate entries, nor will it dereference alias entries.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Delete(string dn, LdapResponseQueue queue);

        /// <summary>
        ///     Asynchronously deletes the entry with the specified distinguished name
        ///     from the directory, using the specified contraints and queue.
        ///     Note: A Delete operation will not remove an entry that contains
        ///     subordinate entries, nor will it dereference alias entries.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to delete.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Delete(string dn, LdapResponseQueue queue, LdapConstraints cons);

        /// <summary>
        ///     Synchronously disconnects from the Ldap server.
        ///     Before the object can perform Ldap operations again, it must
        ///     reconnect to the server by calling connect.
        ///     The disconnect method abandons any outstanding requests, issues an
        ///     unbind request to the server, and then closes the socket.
        /// </summary>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Disconnect();

        /// <summary>
        ///     Provides a synchronous means to access extended, non-mandatory
        ///     operations offered by a particular Ldapv3 compliant server.
        /// </summary>
        /// <param name="op">
        ///     The object which contains (1) an identifier of an extended
        ///     operation which should be recognized by the particular Ldap
        ///     server this client is connected to and (2)
        ///     an operation-specific sequence of octet strings
        ///     or BER-encoded values.
        /// </param>
        /// <returns>
        ///     An operation-specific object, containing an ID and either an octet
        ///     string or BER-encoded values.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapExtendedResponse ExtendedOperation(LdapExtendedOperation op);

        /// <summary>
        ///     Provides a synchronous means to access extended, non-mandatory
        ///     operations offered by a particular Ldapv3 compliant server.
        /// </summary>
        /// <param name="op">
        ///     The object which contains (1) an identifier of an extended
        ///     operation which should be recognized by the particular Ldap
        ///     server this client is connected to and (2) an
        ///     operation-specific sequence of octet strings
        ///     or BER-encoded values.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     An operation-specific object, containing an ID and either an
        ///     octet string or BER-encoded values.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapExtendedResponse ExtendedOperation(LdapExtendedOperation op, LdapConstraints cons);

        /// <summary>
        ///     Provides an asynchronous means to access extended, non-mandatory
        ///     operations offered by a particular Ldapv3 compliant server.
        /// </summary>
        /// <param name="op">
        ///     The object which contains (1) an identifier of an extended
        ///     operation which should be recognized by the particular Ldap
        ///     server this client is connected to and (2) an
        ///     operation-specific sequence of octet strings
        ///     or BER-encoded values.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a queue
        ///     object is created internally.
        /// </param>
        /// <returns>
        ///     An operation-specific object, containing an ID and either an octet
        ///     string or BER-encoded values.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue ExtendedOperation(LdapExtendedOperation op, LdapResponseQueue queue);

        /// <summary>
        ///     Provides an asynchronous means to access extended, non-mandatory
        ///     operations offered by a particular Ldapv3 compliant server.
        /// </summary>
        /// <param name="op">
        ///     The object which contains (1) an identifier of an extended
        ///     operation which should be recognized by the particular Ldap
        ///     server this client is connected to and (2) an operation-
        ///     specific sequence of octet strings or BER-encoded values.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a queue
        ///     object is created internally.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to this operation.
        /// </param>
        /// <returns>
        ///     An operation-specific object, containing an ID and either an
        ///     octet string or BER-encoded values.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue ExtendedOperation(LdapExtendedOperation op, LdapConstraints cons, LdapResponseQueue queue);

        /// <summary>
        ///     Synchronously makes a single change to an existing entry in the
        ///     directory.
        ///     For example, this modify method changes the value of an attribute,
        ///     adds a new attribute value, or removes an existing attribute value.
        ///     The LdapModification object specifies both the change to be made and
        ///     the LdapAttribute value to be changed.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modification.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mod">
        ///     A single change to be made to the entry.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Modify(string dn, LdapModification mod);

        /// <summary>
        ///     Synchronously makes a single change to an existing entry in the
        ///     directory, using the specified constraints.
        ///     For example, this modify method changes the value of an attribute,
        ///     adds a new attribute value, or removes an existing attribute value.
        ///     The LdapModification object specifies both the change to be
        ///     made and the LdapAttribute value to be changed.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modification.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mod">
        ///     A single change to be made to the entry.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Modify(string dn, LdapModification mod, LdapConstraints cons);

        /// <summary>
        ///     Synchronously makes a set of changes to an existing entry in the
        ///     directory.
        ///     For example, this modify method changes attribute values, adds
        ///     new attribute values, or removes existing attribute values.
        ///     Because the server applies all changes in an LdapModification array
        ///     atomically, the application can expect that no changes
        ///     have been performed if an error is returned.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modifications.
        /// </summary>
        /// <param name="dn">
        ///     Distinguished name of the entry to modify.
        /// </param>
        /// <param name="mods">
        ///     The changes to be made to the entry.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Modify(string dn, LdapModification[] mods);

        /// <summary>
        ///     Synchronously makes a set of changes to an existing entry in the
        ///     directory, using the specified constraints.
        ///     For example, this modify method changes attribute values, adds new
        ///     attribute values, or removes existing attribute values.
        ///     Because the server applies all changes in an LdapModification array
        ///     atomically, the application can expect that no changes
        ///     have been performed if an error is returned.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modifications.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mods">
        ///     The changes to be made to the entry.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an
        ///     error message and an Ldap error code.
        /// </exception>
        void Modify(string dn, LdapModification[] mods, LdapConstraints cons);

        /// <summary>
        ///     Asynchronously makes a single change to an existing entry in the
        ///     directory.
        ///     For example, this modify method can change the value of an attribute,
        ///     add a new attribute value, or remove an existing attribute value.
        ///     The LdapModification object specifies both the change to be made and
        ///     the LdapAttribute value to be changed.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modification.
        /// </summary>
        /// <param name="dn">
        ///     Distinguished name of the entry to modify.
        /// </param>
        /// <param name="mod">
        ///     A single change to be made to the entry.
        /// </param>
        /// <param name="queue">
        ///     Handler for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Modify(string dn, LdapModification mod, LdapResponseQueue queue);

        /// <summary>
        ///     Asynchronously makes a single change to an existing entry in the
        ///     directory, using the specified constraints and queue.
        ///     For example, this modify method can change the value of an attribute,
        ///     add a new attribute value, or remove an existing attribute value.
        ///     The LdapModification object specifies both the change to be made
        ///     and the LdapAttribute value to be changed.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modification.
        /// </summary>
        /// <param name="dn">
        ///     Distinguished name of the entry to modify.
        /// </param>
        /// <param name="mod">
        ///     A single change to be made to the entry.
        /// </param>
        /// <param name="queue">
        ///     Handler for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Modify(string dn, LdapModification mod, LdapResponseQueue queue, LdapConstraints cons);

        /// <summary>
        ///     Asynchronously makes a set of changes to an existing entry in the
        ///     directory.
        ///     For example, this modify method can change attribute values, add new
        ///     attribute values, or remove existing attribute values.
        ///     Because the server applies all changes in an LdapModification array
        ///     atomically, the application can expect that no changes
        ///     have been performed if an error is returned.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modifications.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mods">
        ///     The changes to be made to the entry.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Modify(string dn, LdapModification[] mods, LdapResponseQueue queue);

        /// <summary>
        ///     Asynchronously makes a set of changes to an existing entry in the
        ///     directory, using the specified constraints and queue.
        ///     For example, this modify method can change attribute values, add new
        ///     attribute values, or remove existing attribute values.
        ///     Because the server applies all changes in an LdapModification array
        ///     atomically, the application can expect that no changes
        ///     have been performed if an error is returned.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modifications.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mods">
        ///     The changes to be made to the entry.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Modify(string dn, LdapModification[] mods, LdapResponseQueue queue, LdapConstraints cons);

        /// <summary>
        ///     Synchronously reads the entry for the specified distiguished name (DN)
        ///     and retrieves all attributes for the entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server.
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found
        /// </exception>
        LdapEntry Read(string dn);

        /// <summary>
        ///     Synchronously reads the entry for the specified distiguished name (DN),
        ///     using the specified constraints, and retrieves all attributes for the
        ///     entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found
        /// </exception>
        LdapEntry Read(string dn, LdapSearchConstraints cons);

        /// <summary>
        ///     Synchronously reads the entry for the specified distinguished name (DN)
        ///     and retrieves only the specified attributes from the entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <param name="attrs">
        ///     The names of the attributes to retrieve.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found
        /// </exception>
        LdapEntry Read(string dn, string[] attrs);

        /// <summary>
        ///     Synchronously reads the entry for the specified distinguished name (DN),
        ///     using the specified constraints, and retrieves only the specified
        ///     attributes from the entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <param name="attrs">
        ///     The names of the attributes to retrieve.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found
        /// </exception>
        LdapEntry Read(string dn, string[] attrs, LdapSearchConstraints cons);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Rename(string dn, string newRdn, bool deleteOldRdn);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory, using the
        ///     specified constraints.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Rename(string dn, string newRdn, bool deleteOldRdn, LdapConstraints cons);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory, possibly
        ///     repositioning the entry in the directory tree.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="newParentdn">
        ///     The distinguished name of an existing entry which
        ///     is to be the new parent of the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory, using the
        ///     specified constraints and possibly repositioning the entry in the
        ///     directory tree.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="newParentdn">
        ///     The distinguished name of an existing entry which
        ///     is to be the new parent of the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn, LdapConstraints cons);

        /// <summary>
        ///     Asynchronously renames an existing entry in the directory.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Rename(string dn, string newRdn, bool deleteOldRdn, LdapResponseQueue queue);

        /// <summary>
        ///     Asynchronously renames an existing entry in the directory, using the
        ///     specified constraints.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Rename(string dn, string newRdn, bool deleteOldRdn, LdapResponseQueue queue,
            LdapConstraints cons);

        /// <summary>
        ///     Asynchronously renames an existing entry in the directory, possibly
        ///     repositioning the entry in the directory.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="newParentdn">
        ///     The distinguished name of an existing entry which
        ///     is to be the new parent of the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn,
            LdapResponseQueue queue);

        /// <summary>
        ///     Asynchronously renames an existing entry in the directory, using the
        ///     specified constraints and possibily repositioning the entry in the
        ///     directory.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="newParentdn">
        ///     The distinguished name of an existing entry which
        ///     is to be the new parent of the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapResponseQueue Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn,
            LdapResponseQueue queue, LdapConstraints cons);

        /// <summary>
        ///     Synchronously performs the search specified by the parameters.
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     Search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     Names of attributes to retrieve.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found. If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapSearchResults Search(string @base, int scope, string filter, string[] attrs, bool typesOnly);

        /// <summary>
        ///     Synchronously performs the search specified by the parameters,
        ///     using the specified search constraints (such as the
        ///     maximum number of entries to find or the maximum time to wait for
        ///     search results).
        ///     As part of the search constraints, the method allows specifying
        ///     whether or not the results are to be delivered all at once or in
        ///     smaller batches. If specified that the results are to be delivered in
        ///     smaller batches, each iteration blocks only until the next batch of
        ///     results is returned.
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     The names of attributes to retrieve.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found.  If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the search.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapSearchResults Search(string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchConstraints cons);

        /// <summary>
        ///     Asynchronously performs the search specified by the parameters.
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     Search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     Names of attributes to retrieve.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found.  If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <param name="queue">
        ///     Handler for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapSearchQueue Search(string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchQueue queue);

        /// <summary>
        ///     Asynchronously performs the search specified by the parameters,
        ///     also allowing specification of constraints for the search (such
        ///     as the maximum number of entries to find or the maximum time to
        ///     wait for search results).
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     The names of attributes to retrieve.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found.  If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the search.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        LdapSearchQueue Search(string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchQueue queue, LdapSearchConstraints cons);
    }
}