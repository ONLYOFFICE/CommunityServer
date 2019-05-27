/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.LdapConnection.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Text;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The central class that encapsulates the connection
    ///     to a directory server through the Ldap protocol.
    ///     LdapConnection objects are used to perform common Ldap
    ///     operations such as search, modify and add.
    ///     In addition, LdapConnection objects allow you to bind to an
    ///     Ldap server, set connection and search constraints, and perform
    ///     several other tasks.
    ///     An LdapConnection object is not connected on
    ///     construction and can only be connected to one server at one
    ///     port. Multiple threads may share this single connection, typically
    ///     by cloning the connection object, one for each thread. An
    ///     application may have more than one LdapConnection object, connected
    ///     to the same or different directory servers.
    /// </summary>
    public class LdapConnection : ILdapConnection
    {
        private void InitBlock()
        {
            defSearchCons = new LdapSearchConstraints();
            responseCtlSemaphore = new object();
        }

        /// <summary>
        ///     Returns the protocol version uses to authenticate.
        ///     0 is returned if no authentication has been performed.
        /// </summary>
        /// <returns>
        ///     The protol version used for authentication or 0
        ///     not authenticated.
        /// </returns>
        public virtual int ProtocolVersion
        {
            get
            {
                var prop = conn.BindProperties;
                if (prop == null)
                {
                    return Ldap_V3;
                }
                return prop.ProtocolVersion;
            }
        }

        /// <summary>
        ///     Returns the distinguished name (DN) used for as the bind name during
        ///     the last successful bind operation.  <code>null</code> is returned
        ///     if no authentication has been performed or if the bind resulted in
        ///     an aonymous connection.
        /// </summary>
        /// <returns>
        ///     The distinguished name if authenticated; otherwise, null.
        /// </returns>
        public virtual string AuthenticationDN
        {
            get
            {
                var prop = conn.BindProperties;
                if (prop == null)
                {
                    return null;
                }
                if (prop.Anonymous)
                {
                    return null;
                }
                return prop.AuthenticationDN;
            }
        }

        /// <summary>
        ///     Returns the method used to authenticate the connection. The return
        ///     value is one of the following:
        ///     <ul>
        ///         <li>"none" indicates the connection is not authenticated.</li>
        ///         <li>
        ///             "simple" indicates simple authentication was used or that a null
        ///             or empty authentication DN was specified.
        ///         </li>
        ///         <li>"sasl" indicates that a SASL mechanism was used to authenticate</li>
        ///     </ul>
        /// </summary>
        /// <returns>
        ///     The method used to authenticate the connection.
        /// </returns>
        public virtual string AuthenticationMethod
        {
            get
            {
                var prop = conn.BindProperties;
                if (prop == null)
                {
                    return "simple";
                }
                return conn.BindProperties.AuthenticationMethod;
            }
        }

        /// <summary>
        ///     Returns the properties if any specified on binding with a
        ///     SASL mechanism.
        ///     Null is returned if no authentication has been performed
        ///     or no authentication Map is present.
        /// </summary>
        /// <returns>
        ///     The bind properties Map Object used for SASL bind or null if
        ///     the connection is not present or not authenticated.
        /// </returns>
        public virtual IDictionary SaslBindProperties
        {
            get
            {
                var prop = conn.BindProperties;
                if (prop == null)
                {
                    return null;
                }
                return conn.BindProperties.SaslBindProperties;
            }
        }

        /// <summary>
        ///     Returns the call back handler if any specified on binding with a
        ///     SASL mechanism.
        ///     Null is returned if no authentication has been performed
        ///     or no authentication call back handler is present.
        /// </summary>
        /// <returns>
        ///     The call back handler used for SASL bind or null if the
        ///     object is not present or not authenticated.
        /// </returns>
        public virtual object SaslBindCallbackHandler
        {
            get
            {
                var prop = conn.BindProperties;
                if (prop == null)
                {
                    return null;
                }
                return conn.BindProperties.SaslCallbackHandler;
            }
        }

        /// <summary>
        ///     Returns a copy of the set of constraints associated with this
        ///     connection. These constraints apply to all operations performed
        ///     through this connection (unless a different set of constraints is
        ///     specified when calling an operation method).
        /// </summary>
        /// <returns>
        ///     The set of default contraints that apply to this connection.
        /// </returns>
        /// <summary>
        ///     Sets the constraints that apply to all operations performed through
        ///     this connection (unless a different set of constraints is specified
        ///     when calling an operation method).  An LdapSearchConstraints object
        ///     which is passed to this method sets all constraints, while an
        ///     LdapConstraints object passed to this method sets only base constraints.
        /// </summary>
        /// <param name="cons">
        ///     An LdapConstraints or LdapSearchConstraints Object
        ///     containing the contstraint values to set.
        /// </param>
        /// <seealso cref="Constraints()">
        /// </seealso>
        /// <seealso cref="SearchConstraints()">
        /// </seealso>
        public virtual LdapConstraints Constraints
        {
            get { return (LdapConstraints) defSearchCons.Clone(); }

            set
            {
                // Set all constraints, replace the object with a new one
                if (value is LdapSearchConstraints)
                {
                    defSearchCons = (LdapSearchConstraints) value.Clone();
                    return;
                }

                // We set the constraints this way, so a thread doesn't get an
                // conconsistant view of the referrals.
                var newCons = (LdapSearchConstraints) defSearchCons.Clone();
                newCons.HopLimit = value.HopLimit;
                newCons.TimeLimit = value.TimeLimit;
                newCons.setReferralHandler(value.getReferralHandler());
                newCons.ReferralFollowing = value.ReferralFollowing;
                var lsc = value.getControls();
                if (lsc != null)
                {
                    newCons.setControls(lsc);
                }
                var lp = newCons.Properties;
                if (lp != null)
                {
                    newCons.Properties = lp;
                }
                defSearchCons = newCons;
            }
        }

        /// <summary>
        ///     Returns the host name of the Ldap server to which the object is or
        ///     was last connected, in the format originally specified.
        /// </summary>
        /// <returns>
        ///     The host name of the Ldap server to which the object last
        ///     connected or null if the object has never connected.
        /// </returns>
        public virtual string Host
        {
            get { return conn.Host; }
        }

        /// <summary>
        ///     Returns the port number of the Ldap server to which the object is or
        ///     was last connected.
        /// </summary>
        /// <returns>
        ///     The port number of the Ldap server to which the object last
        ///     connected or -1 if the object has never connected.
        /// </returns>
        public virtual int Port
        {
            get { return conn.Port; }
        }

        /// <summary>
        ///     Returns a copy of the set of search constraints associated with this
        ///     connection. These constraints apply to search operations performed
        ///     through this connection (unless a different set of
        ///     constraints is specified when calling the search operation method).
        /// </summary>
        /// <returns>
        ///     The set of default search contraints that apply to
        ///     this connection.
        /// </returns>
        /// <seealso cref="Constraints">
        /// </seealso>
        /// <seealso cref="LdapSearchConstraints">
        /// </seealso>
        public virtual LdapSearchConstraints SearchConstraints
        {
            get { return (LdapSearchConstraints) defSearchCons.Clone(); }
        }


        /// <summary>
        ///     Indicates whther the perform Secure Operation or not
        /// </summary>
        /// <returns>
        ///     True if SSL is on
        ///     False if its not on
        /// </returns>
        public bool SecureSocketLayer
        {
            get { return conn.Ssl; }
            set { conn.Ssl = value; }
        }

        /// <summary>
        ///     Connection timeout in milliseconds, default is 0 which will use
        ///     the platform default timeout for TCP connections.
        /// </summary>
        /// <returns>
        ///     The timeout value in milliseconds
        /// </returns>
        public int ConnectionTimeout
        {
            get { return conn.ConnectionTimeout; }
            set { conn.ConnectionTimeout = value; }
        }

        /// <summary>
        ///     Indicates whether the object has authenticated to the connected Ldap
        ///     server.
        /// </summary>
        /// <returns>
        ///     True if the object has authenticated; false if it has not
        ///     authenticated.
        /// </returns>
        public virtual bool Bound
        {
            get { return conn.Bound; }
        }

        /// <summary>
        ///     Indicates whether the connection represented by this object is open
        ///     at this time.
        /// </summary>
        /// <returns>
        ///     True if connection is open; false if the connection is closed.
        /// </returns>
        public virtual bool Connected
        {
            get { return conn.Connected; }
        }

        /// <summary>
        ///     Indicatates if the connection is protected by TLS.
        /// </summary>
        /// <returns>
        ///     If startTLS has completed this method returns true.
        ///     If stopTLS has completed or start tls failed, this method returns false.
        /// </returns>
        /// <returns>
        ///     True if the connection is protected by TLS.
        /// </returns>
        public virtual bool TLS
        {
            get { return conn.TLS; }
        }


        /// <summary>
        ///     Returns the Server Controls associated with the most recent response
        ///     to a synchronous request on this connection object, or null
        ///     if the latest response contained no Server Controls. The method
        ///     always returns null for asynchronous requests. For asynchronous
        ///     requests, the response controls are available in LdapMessage.
        /// </summary>
        /// <returns>
        ///     The server controls associated with the most recent response
        ///     to a synchronous request or null if the response contains no server
        ///     controls.
        /// </returns>
        /// <seealso cref="LdapMessage.Controls">
        /// </seealso>
        public virtual LdapControl[] ResponseControls
        {
            get
            {
                if (responseCtls == null)
                {
                    return null;
                }


                // We have to clone the control just in case
                // we have two client threads that end up retreiving the
                // same control.
                var clonedControl = new LdapControl[responseCtls.Length];

                // Also note we synchronize access to the local response
                // control object just in case another message containing controls
                // comes in from the server while we are busy duplicating
                // this one.
                lock (responseCtlSemaphore)
                {
                    for (var i = 0; i < responseCtls.Length; i++)
                    {
                        clonedControl[i] = (LdapControl) responseCtls[i].Clone();
                    }
                }

                // Return the cloned copy.  Note we have still left the
                // control in the local responseCtls variable just in case
                // somebody requests it again.
                return clonedControl;
            }
        }

        /// <summary>
        ///     Return the Connection object associated with this LdapConnection
        /// </summary>
        /// <returns>
        ///     the Connection object
        /// </returns>
        internal virtual Connection Connection
        {
            get { return conn; }
        }

        /// <summary>
        ///     Return the Connection object name associated with this LdapConnection
        /// </summary>
        /// <returns>
        ///     the Connection object name
        /// </returns>
        internal virtual string ConnectionName
        {
            get { return name; }
        }

        private LdapSearchConstraints defSearchCons;
        private LdapControl[] responseCtls;

        // Synchronization Object used to synchronize access to responseCtls
        private object responseCtlSemaphore;

        private Connection conn;

        private static object nameLock; // protect agentNum
        private static int lConnNum = 0; // Debug, LdapConnection number
        private string name; // String name for debug

        /// <summary>
        ///     Used with search to specify that the scope of entrys to search is to
        ///     search only the base obect.
        ///     SCOPE_BASE = 0
        /// </summary>
        public const int SCOPE_BASE = 0;

        /// <summary>
        ///     Used with search to specify that the scope of entrys to search is to
        ///     search only the immediate subordinates of the base obect.
        ///     SCOPE_ONE = 1
        /// </summary>
        public const int SCOPE_ONE = 1;

        /// <summary>
        ///     Used with search to specify that the scope of entrys to search is to
        ///     search the base object and all entries within its subtree.
        ///     SCOPE_ONE = 2
        /// </summary>
        public const int SCOPE_SUB = 2;

        /// <summary>
        ///     Used with search instead of an attribute list to indicate that no
        ///     attributes are to be returned.
        ///     NO_ATTRS = "1.1"
        /// </summary>
        public const string NO_ATTRS = "1.1";

        /// <summary>
        ///     Used with search instead of an attribute list to indicate that all
        ///     attributes are to be returned.
        ///     ALL_USER_ATTRS = "*"
        /// </summary>
        public const string ALL_USER_ATTRS = "*";

        /// <summary>
        ///     Specifies the Ldapv3 protocol version when performing a bind operation.
        ///     Specifies Ldap version V3 of the protocol, and is specified
        ///     when performing bind operations.
        ///     You can use this identifier in the version parameter
        ///     of the bind method to specify an Ldapv3 bind.
        ///     Ldap_V3 is the default protocol version
        ///     Ldap_V3 = 3
        /// </summary>
        public const int Ldap_V3 = 3;

        /// <summary>
        ///     The default port number for Ldap servers.
        ///     You can use this identifier to specify the port when establishing
        ///     a clear text connection to a server.  This the default port.
        ///     DEFAULT_PORT = 389
        /// </summary>
        public const int DEFAULT_PORT = 389;


        /// <summary>
        ///     The default SSL port number for Ldap servers.
        ///     DEFAULT_SSL_PORT = 636
        ///     You can use this identifier to specify the port when establishing
        ///     a an SSL connection to a server..
        /// </summary>
        public const int DEFAULT_SSL_PORT = 636;

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_SDK = "version.sdk"
        ///     You can use this string to request the version of the SDK.
        /// </summary>
        public const string Ldap_PROPERTY_SDK = "version.sdk";

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_PROTOCOL = "version.protocol"
        ///     You can use this string to request the version of the
        ///     Ldap protocol.
        /// </summary>
        public const string Ldap_PROPERTY_PROTOCOL = "version.protocol";

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_SECURITY = "version.security"
        ///     You can use this string to request the type of security
        ///     being used.
        /// </summary>
        public const string Ldap_PROPERTY_SECURITY = "version.security";

        /// <summary>
        ///     A string that corresponds to the server shutdown notification OID.
        ///     This notification may be used by the server to advise the client that
        ///     the server is about to close the connection due to an error
        ///     condition.
        ///     SERVER_SHUTDOWN_OID = "1.3.6.1.4.1.1466.20036"
        /// </summary>
        public const string SERVER_SHUTDOWN_OID = "1.3.6.1.4.1.1466.20036";

        /// <summary> The OID string that identifies a StartTLS request and response.</summary>
        private const string START_TLS_OID = "1.3.6.1.4.1.1466.20037";


        public event RemoteCertificateValidationCallback UserDefinedServerCertValidationDelegate
        {
            add { conn.OnCertificateValidation += value; }

            remove { conn.OnCertificateValidation -= value; }
        }

        /*
        * Constructors
        */


        /// <summary>
        ///     Constructs a new LdapConnection object, which will use the supplied
        ///     class factory to construct a socket connection during
        ///     LdapConnection.connect method.
        /// </summary>
        /// <param name="factory">
        ///     An object capable of producing a Socket.
        /// </param>
        public LdapConnection()
        {
            InitBlock();
            // Get a unique connection name for debug
            conn = new Connection();
        }

        /*
        * The following are methods that affect the operation of
        * LdapConnection, but are not Ldap requests.
        */

        /// <summary>
        ///     Returns a copy of the object with a private context, but sharing the
        ///     network connection if there is one.
        ///     The network connection remains open until all clones have
        ///     disconnected or gone out of scope. Any connection opened after
        ///     cloning is private to the object making the connection.
        ///     The clone can issue requests and freely modify options and search
        ///     constraints, and , without affecting the source object or other clones.
        ///     If the clone disconnects or reconnects, it is completely dissociated
        ///     from the source object and other clones. Reauthenticating in a clone,
        ///     however, is a global operation which will affect the source object
        ///     and all associated clones, because it applies to the single shared
        ///     physical connection. Any request by an associated object after one
        ///     has reauthenticated will carry the new identity.
        /// </summary>
        /// <returns>
        ///     A of the object.
        /// </returns>
        public object Clone()
        {
            LdapConnection newClone;
            object newObj;
            try
            {
                newObj = MemberwiseClone();
                newClone = (LdapConnection) newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
            newClone.conn = conn; // same underlying connection

            //now just duplicate the defSearchCons and responseCtls
            if (defSearchCons != null)
            {
                newClone.defSearchCons = (LdapSearchConstraints) defSearchCons.Clone();
            }
            else
            {
                newClone.defSearchCons = null;
            }
            if (responseCtls != null)
            {
                newClone.responseCtls = new LdapControl[responseCtls.Length];
                for (var i = 0; i < responseCtls.Length; i++)
                {
                    newClone.responseCtls[i] = (LdapControl) responseCtls[i].Clone();
                }
            }
            else
            {
                newClone.responseCtls = null;
            }
            conn.incrCloneCount(); // Increment the count of clones
            return newObj;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                DisconnectImpl();
            }
        }

        /// <summary>
        ///     Returns a property of a connection object.
        /// </summary>
        /// <param name="name">
        ///     Name of the property to be returned.
        ///     The following read-only properties are available
        ///     for any given connection:
        ///     <ul>
        ///         <li>
        ///             Ldap_PROPERTY_SDK returns the version of this SDK,
        ///             as a Float data type.
        ///         </li>
        ///         <li>
        ///             Ldap_PROPERTY_PROTOCOL returns the highest supported version of
        ///             the Ldap protocol, as a Float data type.
        ///         </li>
        ///         <li>
        ///             Ldap_PROPERTY_SECURITY returns a comma-separated list of the
        ///             types of authentication supported, as a
        ///             string.
        ///         </li>
        ///     </ul>
        ///     A deep copy of the property is provided where applicable; a
        ///     client does not need to clone the object received.
        /// </param>
        /// <returns>
        ///     The object associated with the requested property,
        ///     or null if the property is not defined.
        /// </returns>
        /// <seealso cref="LdapConstraints.getProperty">
        /// </seealso>
        /// <seealso cref="object">
        /// </seealso>
        public virtual object getProperty(string name)
        {
            if (name.ToUpper().Equals(Ldap_PROPERTY_SDK.ToUpper()))
                return Connection.sdk;
            if (name.ToUpper().Equals(Ldap_PROPERTY_PROTOCOL.ToUpper()))
                return Connection.protocol;
            if (name.ToUpper().Equals(Ldap_PROPERTY_SECURITY.ToUpper()))
                return Connection.security;
            return null;
        }

        /// <summary>
        ///     Registers an object to be notified on arrival of an unsolicited
        ///     message from a server.
        ///     An unsolicited message has the ID 0. A new thread is created and
        ///     the method "messageReceived" in each registered object is called in
        ///     turn.
        /// </summary>
        /// <param name="listener">
        ///     An object to be notified on arrival of an
        ///     unsolicited message from a server.  This object must
        ///     implement the LdapUnsolicitedNotificationListener interface.
        /// </param>
        public virtual void AddUnsolicitedNotificationListener(LdapUnsolicitedNotificationListener listener)
        {
            if (listener != null)
                conn.AddUnsolicitedNotificationListener(listener);
        }


        /// <summary>
        ///     Deregisters an object so that it will no longer be notified on
        ///     arrival of an unsolicited message from a server. If the object is
        ///     null or was not previously registered for unsolicited notifications,
        ///     the method does nothing.
        /// </summary>
        /// <param name="listener">
        ///     An object to no longer be notified on arrival of
        ///     an unsolicited message from a server.
        /// </param>
        public virtual void RemoveUnsolicitedNotificationListener(LdapUnsolicitedNotificationListener listener)
        {
            if (listener != null)
                conn.RemoveUnsolicitedNotificationListener(listener);
        }

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
        public virtual void StartTls()
        {
            var startTLS = MakeExtendedOperation(new LdapExtendedOperation(START_TLS_OID, null), null);

            var tlsID = startTLS.MessageID;

            conn.acquireWriteSemaphore(tlsID);
            try
            {
                if (!conn.AreMessagesComplete())
                {
                    throw new LdapLocalException(ExceptionMessages.OUTSTANDING_OPERATIONS,
                        LdapException.OPERATIONS_ERROR);
                }
                // Stop reader when response to startTLS request received
                conn.stopReaderOnReply(tlsID);

                // send tls message
                var queue = SendRequestToServer(startTLS, defSearchCons.TimeLimit, null, null);

                var response = (LdapExtendedResponse) queue.getResponse();
                response.chkResultCode();

                conn.startTLS();
            }
            finally
            {
                //Free this semaphore no matter what exceptions get thrown                
                conn.freeWriteSemaphore(tlsID);
            }
        }

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
        public virtual void StopTls()
        {
            if (!TLS)
            {
                throw new LdapLocalException(ExceptionMessages.NO_STARTTLS, LdapException.OPERATIONS_ERROR);
            }

            var semaphoreID = conn.acquireWriteSemaphore();
            try
            {
                if (!conn.AreMessagesComplete())
                {
                    throw new LdapLocalException(ExceptionMessages.OUTSTANDING_OPERATIONS,
                        LdapException.OPERATIONS_ERROR);
                }
                //stopTLS stops and starts the reader thread for us.
                conn.stopTLS();
            }
            finally
            {
                conn.freeWriteSemaphore(semaphoreID);
            }
            /* Now that the TLS socket is closed, reset everything.  This next
            line is temporary until JSSE is fixed to properly handle TLS stop */
            /* After stopTls the stream is very likely unusable */
            Connect(Host, Port);
        }


        //*************************************************************************
        // Below are all of the Ldap protocol operation methods
        //*************************************************************************

        //*************************************************************************
        // abandon methods
        //*************************************************************************

        /// <summary>
        ///     Notifies the server not to send additional results associated with
        ///     this LdapSearchResults object, and discards any results already
        ///     received.
        /// </summary>
        /// <param name="results">
        ///     An object returned from a search.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual void Abandon(LdapSearchResults results)
        {
            Abandon(results, defSearchCons);
        }

        /// <summary>
        ///     Notifies the server not to send additional results associated with
        ///     this LdapSearchResults object, and discards any results already
        ///     received.
        /// </summary>
        /// <param name="results">
        ///     An object returned from a search.
        /// </param>
        /// <param name="cons">
        ///     The contraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual void Abandon(LdapSearchResults results, LdapConstraints cons)
        {
            results.Abandon();
        }

        /// <summary>
        ///     Abandons an asynchronous operation.
        /// </summary>
        /// <param name="id">
        ///     The ID of the asynchronous operation to abandon. The ID
        ///     can be obtained from the response queue for the
        ///     operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual void Abandon(int id)
        {
            Abandon(id, defSearchCons);
        }

        /// <summary>
        ///     Abandons an asynchronous operation, using the specified
        ///     constraints.
        /// </summary>
        /// <param name="id">
        ///     The ID of the asynchronous operation to abandon.
        ///     The ID can be obtained from the search
        ///     queue for the operation.
        /// </param>
        /// <param name="cons">
        ///     The contraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual void Abandon(int id, LdapConstraints cons)
        {
            // We need to inform the Message Agent which owns this messageID to
            // remove it from the queue.
            try
            {
                var agent = conn.getMessageAgent(id);
                agent.Abandon(id, cons);
            }
            catch (FieldAccessException fae)
            {
                Logger.Log.LogWarning("Exception swallowed", fae);
            }
        }

        /// <summary>
        ///     Abandons all outstanding operations managed by the queue.
        ///     All operations in progress, which are managed by the specified queue,
        ///     are abandoned.
        /// </summary>
        /// <param name="queue">
        ///     The queue returned from an asynchronous request.
        ///     All outstanding operations managed by the queue
        ///     are abandoned, and the queue is emptied.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual void Abandon(LdapMessageQueue queue)
        {
            Abandon(queue, defSearchCons);
        }

        /// <summary>
        ///     Abandons all outstanding operations managed by the queue.
        ///     All operations in progress, which are managed by the specified
        ///     queue, are abandoned.
        /// </summary>
        /// <param name="queue">
        ///     The queue returned from an asynchronous request.
        ///     All outstanding operations managed by the queue
        ///     are abandoned, and the queue is emptied.
        /// </param>
        /// <param name="cons">
        ///     The contraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual void Abandon(LdapMessageQueue queue, LdapConstraints cons)
        {
            if (queue != null)
            {
                MessageAgent agent;
                if (queue is LdapSearchQueue)
                {
                    agent = queue.MessageAgent;
                }
                else
                {
                    agent = queue.MessageAgent;
                }
                var msgIds = agent.MessageIDs;
                for (var i = 0; i < msgIds.Length; i++)
                {
                    agent.Abandon(msgIds[i], cons);
                }
            }
        }

        //*************************************************************************
        // add methods
        //*************************************************************************

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
        public virtual void Add(LdapEntry entry)
        {
            Add(entry, defSearchCons);
        }

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
        public virtual void Add(LdapEntry entry, LdapConstraints cons)
        {
            var queue = Add(entry, null, cons);

            // Get a handle to the add response
            var addResponse = (LdapResponse) queue.getResponse();

            // Set local copy of responseControls synchronously if there were any
            lock (responseCtlSemaphore)
            {
                responseCtls = addResponse.Controls;
            }
            ChkResultCode(queue, cons, addResponse);
        }

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
        public virtual LdapResponseQueue Add(LdapEntry entry, LdapResponseQueue queue)
        {
            return Add(entry, queue, defSearchCons);
        }

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
        public virtual LdapResponseQueue Add(LdapEntry entry, LdapResponseQueue queue, LdapConstraints cons)
        {
            if (cons == null)
                cons = defSearchCons;

            // error check the parameters
            if (entry == null)
            {
                throw new ArgumentException("The LdapEntry parameter" + " cannot be null");
            }
            if ((object) entry.DN == null)
            {
                throw new ArgumentException("The DN value must be present" + " in the LdapEntry object");
            }

            LdapMessage msg = new LdapAddRequest(entry, cons.getControls());

            return SendRequestToServer(msg, cons.TimeLimit, queue, null);
        }

        //*************************************************************************
        // bind methods
        //*************************************************************************

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
        public virtual void Bind(string dn, string passwd)
        {
            Bind(Ldap_V3, dn, passwd, defSearchCons);
        }

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
        public virtual void Bind(int version, string dn, string passwd)
        {
            Bind(version, dn, passwd, defSearchCons);
        }

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
        public virtual void Bind(string dn, string passwd, LdapConstraints cons)
        {
            Bind(Ldap_V3, dn, passwd, cons);
        }

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
        public virtual void Bind(int version, string dn, string passwd, LdapConstraints cons)
        {
            sbyte[] pw = null;
            if ((object) passwd != null)
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(passwd);
                pw = SupportClass.ToSByteArray(ibytes);
            }
            Bind(version, dn, pw, cons);
        }

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
        [CLSCompliant(false)]
        public virtual void Bind(int version, string dn, sbyte[] passwd)
        {
            Bind(version, dn, passwd, defSearchCons);
        }

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
        [CLSCompliant(false)]
        public virtual void Bind(int version, string dn, sbyte[] passwd, LdapConstraints cons)
        {
            var queue = Bind(version, dn, passwd, null, cons);
            var res = (LdapResponse) queue.getResponse();
            if (res != null)
            {
                // Set local copy of responseControls synchronously if any
                lock (responseCtlSemaphore)
                {
                    responseCtls = res.Controls;
                }

                ChkResultCode(queue, cons, res);
            }
        }

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
        [CLSCompliant(false)]
        public virtual LdapResponseQueue Bind(int version, string dn, sbyte[] passwd, LdapResponseQueue queue)
        {
            return Bind(version, dn, passwd, queue, defSearchCons);
        }

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
        [CLSCompliant(false)]
        public virtual LdapResponseQueue Bind(int version, string dn, sbyte[] passwd, LdapResponseQueue queue,
            LdapConstraints cons)
        {
            int msgId;
            BindProperties bindProps;
            if (cons == null)
                cons = defSearchCons;

            if ((object) dn == null)
            {
                dn = "";
            }
            else
            {
                dn = dn.Trim();
            }

            if (passwd == null)
                passwd = new sbyte[] {};

            var anonymous = false;
            if (passwd.Length == 0)
            {
                anonymous = true; // anonymous, passwd length zero with simple bind
                dn = ""; // set to null if anonymous
            }

            LdapMessage msg = new LdapBindRequest(version, dn, passwd, cons.getControls());

            msgId = msg.MessageID;
            bindProps = new BindProperties(version, dn, "simple", anonymous, null, null);

            // For bind requests, if not connected, attempt to reconnect
            if (!conn.Connected)
            {
                if ((object) conn.Host != null)
                {
                    conn.connect(conn.Host, conn.Port);
                }
                else
                {
                    throw new LdapException(ExceptionMessages.CONNECTION_IMPOSSIBLE, LdapException.CONNECT_ERROR, null);
                }
            }

            // The semaphore is released when the bind response is queued.
            conn.acquireWriteSemaphore(msgId);

            return SendRequestToServer(msg, cons.TimeLimit, queue, bindProps);
        }

        //*************************************************************************
        // compare methods
        //*************************************************************************

        /// <summary>
        ///     Synchronously checks to see if an entry contains an attribute
        ///     with a specified value.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to use in the
        ///     comparison.
        /// </param>
        /// <param name="attr">
        ///     The attribute to compare against the entry. The
        ///     method checks to see if the entry has an
        ///     attribute with the same name and value as this
        ///     attribute.
        /// </param>
        /// <returns>
        ///     True if the entry has the value,
        ///     and false if the entry does not
        ///     have the value or the attribute.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual bool Compare(string dn, LdapAttribute attr)
        {
            return Compare(dn, attr, defSearchCons);
        }

        /// <summary>
        ///     Synchronously checks to see if an entry contains an attribute with a
        ///     specified value, using the specified constraints.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to use in the
        ///     comparison.
        /// </param>
        /// <param name="attr">
        ///     The attribute to compare against the entry. The
        ///     method checks to see if the entry has an
        ///     attribute with the same name and value as this
        ///     attribute.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     True if the entry has the value,
        ///     and false if the entry does not
        ///     have the value or the attribute.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public virtual bool Compare(string dn, LdapAttribute attr, LdapConstraints cons)
        {
            var ret = false;

            var queue = Compare(dn, attr, null, cons);

            var res = (LdapResponse) queue.getResponse();

            // Set local copy of responseControls synchronously - if there were any
            lock (responseCtlSemaphore)
            {
                responseCtls = res.Controls;
            }

            if (res.ResultCode == LdapException.COMPARE_TRUE)
            {
                ret = true;
            }
            else if (res.ResultCode == LdapException.COMPARE_FALSE)
            {
                ret = false;
            }
            else
            {
                ChkResultCode(queue, cons, res);
            }
            return ret;
        }

        /// <summary>
        ///     Asynchronously compares an attribute value with one in the directory,
        ///     using the specified queue.
        ///     Please note that a successful completion of this command results in
        ///     one of two status codes: LdapException.COMPARE_TRUE if the entry
        ///     has the value, and LdapException.COMPARE_FALSE if the entry
        ///     does not have the value or the attribute.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry containing an
        ///     attribute to compare.
        /// </param>
        /// <param name="attr">
        ///     An attribute to compare.
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
        /// <seealso cref="LdapException.COMPARE_TRUE">
        /// </seealso>
        /// <seealso cref="LdapException.COMPARE_FALSE">
        /// </seealso>
        public virtual LdapResponseQueue Compare(string dn, LdapAttribute attr, LdapResponseQueue queue)
        {
            return Compare(dn, attr, queue, defSearchCons);
        }

        /// <summary>
        ///     Asynchronously compares an attribute value with one in the directory,
        ///     using the specified queue and contraints.
        ///     Please note that a successful completion of this command results in
        ///     one of two status codes: LdapException.COMPARE_TRUE if the entry
        ///     has the value, and LdapException.COMPARE_FALSE if the entry
        ///     does not have the value or the attribute.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry containing an
        ///     attribute to compare.
        /// </param>
        /// <param name="attr">
        ///     An attribute to compare.
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
        /// <seealso cref="LdapException.COMPARE_TRUE">
        /// </seealso>
        /// <seealso cref="LdapException.COMPARE_FALSE">
        /// </seealso>
        public virtual LdapResponseQueue Compare(string dn, LdapAttribute attr, LdapResponseQueue queue,
            LdapConstraints cons)
        {
            if (attr.size() != 1)
            {
                throw new ArgumentException("compare: Exactly one value " + "must be present in the LdapAttribute");
            }

            if ((object) dn == null)
            {
                // Invalid parameter
                throw new ArgumentException("compare: DN cannot be null");
            }

            if (cons == null)
                cons = defSearchCons;

            LdapMessage msg = new LdapCompareRequest(dn, attr.Name, attr.ByteValue, cons.getControls());

            return SendRequestToServer(msg, cons.TimeLimit, queue, null);
        }

        //*************************************************************************
        // connect methods
        //*************************************************************************

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
        public virtual void Connect(string host, int port)
        {
            // connect doesn't affect other clones
            // If not a clone, destroys old connection.
            // Step through the space-delimited list
            var hostList = new SupportClass.Tokenizer(host, " ");
            while (hostList.HasMoreTokens())
            {
                try
                {
                    var specifiedPort = port;
                    var address = hostList.NextToken();
                    var colonIndex = address.IndexOf(':'); //after the colon is the port
                    if (colonIndex != -1 && colonIndex + 1 != address.Length)
                    {
                        //parse Port out of address
                        try
                        {
                            specifiedPort = int.Parse(address.Substring(colonIndex + 1));
                            address = address.Substring(0, colonIndex - 0);
                        }
                        catch (Exception e)
                        {
                            throw new ArgumentException(ExceptionMessages.INVALID_ADDRESS, e);
                        }
                    }
                    // This may return a different conn object
                    // Disassociate this clone with the underlying connection.
                    conn = conn.destroyClone();
                    conn.connect(address, specifiedPort);
                    break;
                }
                catch (LdapException)
                {
                    if (!hostList.HasMoreTokens())
                        throw;
                }
            }
        }

        //*************************************************************************
        // delete methods
        //*************************************************************************

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
        public virtual void Delete(string dn)
        {
            Delete(dn, defSearchCons);
        }


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
        public virtual void Delete(string dn, LdapConstraints cons)
        {
            var queue = Delete(dn, null, cons);

            // Get a handle to the delete response
            var deleteResponse = (LdapResponse) queue.getResponse();

            // Set local copy of responseControls synchronously - if there were any
            lock (responseCtlSemaphore)
            {
                responseCtls = deleteResponse.Controls;
            }
            ChkResultCode(queue, cons, deleteResponse);
        }

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
        public virtual LdapResponseQueue Delete(string dn, LdapResponseQueue queue)
        {
            return Delete(dn, queue, defSearchCons);
        }

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
        public virtual LdapResponseQueue Delete(string dn, LdapResponseQueue queue, LdapConstraints cons)
        {
            if ((object) dn == null)
            {
                // Invalid DN parameter
                throw new ArgumentException(ExceptionMessages.DN_PARAM_ERROR);
            }

            if (cons == null)
                cons = defSearchCons;

            LdapMessage msg = new LdapDeleteRequest(dn, cons.getControls());

            return SendRequestToServer(msg, cons.TimeLimit, queue, null);
        }

        //*************************************************************************
        // disconnect method
        //*************************************************************************

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
        public virtual void Disconnect()
        {
            // disconnect from API call
            DisconnectImpl();
        }


        /// <summary>
        ///     Synchronously disconnect from the server
        /// </summary>
        /// <param name="how">
        ///     true if application call disconnect API, false if finalize.
        /// </param>
        private void DisconnectImpl()
        {
            // disconnect doesn't affect other clones
            // If not a clone, distroys connection
            conn = conn.destroyClone();
        }

        //*************************************************************************
        // extendedOperation methods
        //*************************************************************************

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
        public virtual LdapExtendedResponse ExtendedOperation(LdapExtendedOperation op)
        {
            return ExtendedOperation(op, defSearchCons);
        }

        /*
        *  Synchronous Ldap extended request with SearchConstraints
        */

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
        public virtual LdapExtendedResponse ExtendedOperation(LdapExtendedOperation op, LdapConstraints cons)
        {
            // Call asynchronous API and get back handler to reponse queue
            var queue = ExtendedOperation(op, cons, null);
            var response = (LdapExtendedResponse) queue.getResponse();

            // Set local copy of responseControls synchronously - if there were any
            lock (responseCtlSemaphore)
            {
                responseCtls = response.Controls;
            }

            ChkResultCode(queue, cons, response);
            return response;
        }


        /*
        * Asynchronous Ldap extended request
        */

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
        public virtual LdapResponseQueue ExtendedOperation(LdapExtendedOperation op, LdapResponseQueue queue)
        {
            return ExtendedOperation(op, defSearchCons, queue);
        }


        /*
        *  Asynchronous Ldap extended request with SearchConstraints
        */

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
        public virtual LdapResponseQueue ExtendedOperation(LdapExtendedOperation op, LdapConstraints cons,
            LdapResponseQueue queue)
        {
            // Use default constraints if none-specified
            if (cons == null)
                cons = defSearchCons;
            var msg = MakeExtendedOperation(op, cons);
            return SendRequestToServer(msg, cons.TimeLimit, queue, null);
        }

        /// <summary>
        ///     Formulates the extended operation, constraints into an
        ///     LdapMessage and returns the LdapMessage.  This is used by
        ///     extendedOperation and startTLS which needs the LdapMessage to
        ///     get the MessageID.
        /// </summary>
        protected internal virtual LdapMessage MakeExtendedOperation(LdapExtendedOperation op, LdapConstraints cons)
        {
            // Use default constraints if none-specified
            if (cons == null)
                cons = defSearchCons;

            // error check the parameters
            if ((object) op.getID() == null)
            {
                // Invalid extended operation parameter, no OID specified
                throw new ArgumentException(ExceptionMessages.OP_PARAM_ERROR);
            }

            return new LdapExtendedRequest(op, cons.getControls());
        }

        //*************************************************************************
        // getResponseControls method
        //*************************************************************************

        //*************************************************************************
        // modify methods
        //*************************************************************************

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
        public virtual void Modify(string dn, LdapModification mod)
        {
            Modify(dn, mod, defSearchCons);
        }

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
        public virtual void Modify(string dn, LdapModification mod, LdapConstraints cons)
        {
            var mods = new LdapModification[1];
            mods[0] = mod;
            Modify(dn, mods, cons);
        }

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
        public virtual void Modify(string dn, LdapModification[] mods)
        {
            Modify(dn, mods, defSearchCons);
        }

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
        public virtual void Modify(string dn, LdapModification[] mods, LdapConstraints cons)
        {
            var queue = Modify(dn, mods, null, cons);

            // Get a handle to the modify response
            var modifyResponse = (LdapResponse) queue.getResponse();

            // Set local copy of responseControls synchronously - if there were any
            lock (responseCtlSemaphore)
            {
                responseCtls = modifyResponse.Controls;
            }

            ChkResultCode(queue, cons, modifyResponse);
        }

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
        public virtual LdapResponseQueue Modify(string dn, LdapModification mod, LdapResponseQueue queue)
        {
            return Modify(dn, mod, queue, defSearchCons);
        }

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
        public virtual LdapResponseQueue Modify(string dn, LdapModification mod, LdapResponseQueue queue,
            LdapConstraints cons)
        {
            var mods = new LdapModification[1];
            mods[0] = mod;
            return Modify(dn, mods, queue, cons);
        }

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
        public virtual LdapResponseQueue Modify(string dn, LdapModification[] mods, LdapResponseQueue queue)
        {
            return Modify(dn, mods, queue, defSearchCons);
        }

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
        public virtual LdapResponseQueue Modify(string dn, LdapModification[] mods, LdapResponseQueue queue,
            LdapConstraints cons)
        {
            if ((object) dn == null)
            {
                // Invalid DN parameter
                throw new ArgumentException(ExceptionMessages.DN_PARAM_ERROR);
            }

            if (cons == null)
                cons = defSearchCons;

            LdapMessage msg = new LdapModifyRequest(dn, mods, cons.getControls());

            return SendRequestToServer(msg, cons.TimeLimit, queue, null);
        }

        //*************************************************************************
        // read methods
        //*************************************************************************

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
        public virtual LdapEntry Read(string dn)
        {
            return Read(dn, defSearchCons);
        }


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
        public virtual LdapEntry Read(string dn, LdapSearchConstraints cons)
        {
            return Read(dn, null, cons);
        }

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
        public virtual LdapEntry Read(string dn, string[] attrs)
        {
            return Read(dn, attrs, defSearchCons);
        }

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
        public virtual LdapEntry Read(string dn, string[] attrs, LdapSearchConstraints cons)
        {
            var sr = Search(dn, SCOPE_BASE, null, attrs, false, cons);

            LdapEntry ret = null;
            if (sr.hasMore())
            {
                ret = sr.next();
                if (sr.hasMore())
                {
                    // "Read response is ambiguous, multiple entries returned"
                    throw new LdapLocalException(ExceptionMessages.READ_MULTIPLE, LdapException.AMBIGUOUS_RESPONSE);
                }
            }
            return ret;
        }

        /// <summary>
        ///     Synchronously reads the entry specified by the Ldap URL.
        ///     When this read method is called, a new connection is created
        ///     automatically, using the host and port specified in the URL. After
        ///     finding the entry, the method closes the connection (in other words,
        ///     it disconnects from the Ldap server).
        ///     If the URL specifies a filter and scope, they are not used. Of the
        ///     information specified in the URL, this method only uses the Ldap host
        ///     name and port number, the base distinguished name (DN), and the list
        ///     of attributes to return.
        /// </summary>
        /// <param name="toGet">
        ///     Ldap URL specifying the entry to read.
        /// </param>
        /// <returns>
        ///     The entry specified by the base DN.
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found
        /// </exception>
        public static LdapEntry Read(LdapUrl toGet)
        {
            var lconn = new LdapConnection();
            lconn.Connect(toGet.Host, toGet.Port);
            var toReturn = lconn.Read(toGet.getDN(), toGet.AttributeArray);
            lconn.Disconnect();
            return toReturn;
        }

        /// <summary>
        ///     Synchronously reads the entry specified by the Ldap URL, using the
        ///     specified constraints.
        ///     When this method is called, a new connection is created
        ///     automatically, using the host and port specified in the URL. After
        ///     finding the entry, the method closes the connection (in other words,
        ///     it disconnects from the Ldap server).
        ///     If the URL specifies a filter and scope, they are not used. Of the
        ///     information specified in the URL, this method only uses the Ldap host
        ///     name and port number, the base distinguished name (DN), and the list
        ///     of attributes to return.
        /// </summary>
        /// <returns>
        ///     The entry specified by the base DN.
        /// </returns>
        /// <param name="toGet">
        ///     Ldap URL specifying the entry to read.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException if the object was not found
        /// </exception>
        public static LdapEntry Read(LdapUrl toGet, LdapSearchConstraints cons)
        {
            var lconn = new LdapConnection();
            lconn.Connect(toGet.Host, toGet.Port);
            var toReturn = lconn.Read(toGet.getDN(), toGet.AttributeArray, cons);
            lconn.Disconnect();
            return toReturn;
        }

        //*************************************************************************
        // rename methods
        //*************************************************************************

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
        public virtual void Rename(string dn, string newRdn, bool deleteOldRdn)
        {
            Rename(dn, newRdn, deleteOldRdn, defSearchCons);
        }

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
        public virtual void Rename(string dn, string newRdn, bool deleteOldRdn, LdapConstraints cons)
        {
            // null for newParentdn means that this is originating as an Ldapv2 call
            Rename(dn, newRdn, null, deleteOldRdn, cons);
        }

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
        public virtual void Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn)
        {
            Rename(dn, newRdn, newParentdn, deleteOldRdn, defSearchCons);
        }

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
        public virtual void Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn, LdapConstraints cons)
        {
            var queue = Rename(dn, newRdn, newParentdn, deleteOldRdn, null, cons);

            // Get a handle to the rename response
            var renameResponse = (LdapResponse) queue.getResponse();

            // Set local copy of responseControls synchronously - if there were any
            lock (responseCtlSemaphore)
            {
                responseCtls = renameResponse.Controls;
            }

            ChkResultCode(queue, cons, renameResponse);
        }

        /*
        * rename
        */

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
        public virtual LdapResponseQueue Rename(string dn, string newRdn, bool deleteOldRdn, LdapResponseQueue queue)
        {
            return Rename(dn, newRdn, deleteOldRdn, queue, defSearchCons);
        }

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
        public virtual LdapResponseQueue Rename(string dn, string newRdn, bool deleteOldRdn, LdapResponseQueue queue,
            LdapConstraints cons)
        {
            return Rename(dn, newRdn, null, deleteOldRdn, queue, cons);
        }

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
        public virtual LdapResponseQueue Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn,
            LdapResponseQueue queue)
        {
            return Rename(dn, newRdn, newParentdn, deleteOldRdn, queue, defSearchCons);
        }

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
        public virtual LdapResponseQueue Rename(string dn, string newRdn, string newParentdn, bool deleteOldRdn,
            LdapResponseQueue queue, LdapConstraints cons)
        {
            if ((object) dn == null || (object) newRdn == null)
            {
                // Invalid DN or RDN parameter
                throw new ArgumentException(ExceptionMessages.RDN_PARAM_ERROR);
            }

            if (cons == null)
                cons = defSearchCons;

            LdapMessage msg = new LdapModifyDNRequest(dn, newRdn, newParentdn, deleteOldRdn, cons.getControls());

            return SendRequestToServer(msg, cons.TimeLimit, queue, null);
        }

        //*************************************************************************
        // search methods
        //*************************************************************************

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
        public virtual LdapSearchResults Search(string @base, int scope, string filter, string[] attrs, bool typesOnly)
        {
            return Search(@base, scope, filter, attrs, typesOnly, defSearchCons);
        }

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
        public virtual LdapSearchResults Search(string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchConstraints cons)
        {
            var queue = Search(@base, scope, filter, attrs, typesOnly, null, cons);

            if (cons == null)
                cons = defSearchCons;
            return new LdapSearchResults(this, queue, cons);
        }

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
        public virtual LdapSearchQueue Search(string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchQueue queue)
        {
            return Search(@base, scope, filter, attrs, typesOnly, queue, defSearchCons);
        }

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
        public virtual LdapSearchQueue Search(string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchQueue queue, LdapSearchConstraints cons)
        {
            if ((object) filter == null)
            {
                filter = "objectclass=*";
            }
            if (cons == null)
                cons = defSearchCons;

            LdapMessage msg = new LdapSearchRequest(@base, scope, filter, attrs, cons.Dereference, cons.MaxResults,
                cons.ServerTimeLimit, typesOnly, cons.getControls());
            MessageAgent agent;
            var myqueue = queue;
            if (myqueue == null)
            {
                agent = new MessageAgent();
                myqueue = new LdapSearchQueue(agent);
            }
            else
            {
                agent = queue.MessageAgent;
            }

            agent.sendMessage(conn, msg, cons.TimeLimit, myqueue, null);
            return myqueue;
        }

        /*
        * Ldap URL search
        */

        /// <summary>
        ///     Synchronously performs the search specified by the Ldap URL, returning
        ///     an enumerable LdapSearchResults object.
        /// </summary>
        /// <param name="toGet">
        ///     The Ldap URL specifying the entry to read.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public static LdapSearchResults Search(LdapUrl toGet)
        {
            // Get a clone of default search constraints, method alters batchSize
            return Search(toGet, null);
        }

        /*
        * Ldap URL search
        */

        /// <summary>
        ///     Synchronously perfoms the search specified by the Ldap URL, using
        ///     the specified search constraints (such as the maximum number of
        ///     entries to find or the maximum time to wait for search results).
        ///     When this method is called, a new connection is created
        ///     automatically, using the host and port specified in the URL. After
        ///     all search results have been received from the server, the method
        ///     closes the connection (in other words, it disconnects from the Ldap
        ///     server).
        ///     As part of the search constraints, a choice can be made as to whether
        ///     to have the results delivered all at once or in smaller batches. If
        ///     the results are to be delivered in smaller batches, each iteration
        ///     blocks only until the next batch of results is returned.
        /// </summary>
        /// <param name="toGet">
        ///     Ldap URL specifying the entry to read.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the search.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public static LdapSearchResults Search(LdapUrl toGet, LdapSearchConstraints cons)
        {
            var lconn = new LdapConnection();
            lconn.Connect(toGet.Host, toGet.Port);
            if (cons == null)
            {
                // This is a clone, so we already have our own copy
                cons = lconn.SearchConstraints;
            }
            else
            {
                // get our own copy of user's constraints because we modify it
                cons = (LdapSearchConstraints) cons.Clone();
            }
            cons.BatchSize = 0; // Must wait until all results arrive
            var toReturn = lconn.Search(toGet.getDN(), toGet.Scope, toGet.Filter, toGet.AttributeArray, false, cons);
            lconn.Disconnect();
            return toReturn;
        }

        /// <summary>
        ///     Sends an Ldap request to a directory server.
        ///     The specified the Ldap request is sent to the directory server
        ///     associated with this connection using default constraints. An Ldap
        ///     request object is a subclass {@link LdapMessage} with the operation
        ///     type set to one of the request types. You can build a request by using
        ///     the request classes found in this package
        ///     You should note that, since Ldap requests sent to the server
        ///     using sendRequest are asynchronous, automatic referral following
        ///     does not apply to these requests.
        /// </summary>
        /// <param name="request">
        ///     The Ldap request to send to the directory server.
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
        /// <seealso cref="LdapMessage.Type">
        /// </seealso>
        /// <seealso cref="RfcLdapMessage.isRequest">
        /// </seealso>
        public virtual LdapMessageQueue SendRequest(LdapMessage request, LdapMessageQueue queue)
        {
            return SendRequest(request, queue, null);
        }

        /// <summary>
        ///     Sends an Ldap request to a directory server.
        ///     The specified the Ldap request is sent to the directory server
        ///     associated with this connection. An Ldap request object is an
        ///     {@link LdapMessage} with the operation type set to one of the request
        ///     types. You can build a request by using the request classes found in this
        ///     package
        ///     You should note that, since Ldap requests sent to the server
        ///     using sendRequest are asynchronous, automatic referral following
        ///     does not apply to these requests.
        /// </summary>
        /// <param name="request">
        ///     The Ldap request to send to the directory server.
        /// </param>
        /// <param name="queue">
        ///     The queue for messages returned from a server in
        ///     response to this request. If it is null, a
        ///     queue object is created internally.
        /// </param>
        /// <param name="cons">
        ///     The constraints that apply to this request
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        /// <seealso cref="LdapMessage.Type">
        /// </seealso>
        /// <seealso cref="RfcLdapMessage.isRequest">
        /// </seealso>
        public virtual LdapMessageQueue SendRequest(LdapMessage request, LdapMessageQueue queue, LdapConstraints cons)
        {
            if (!request.Request)
            {
                throw new Exception("Object is not a request message");
            }

            if (cons == null)
            {
                cons = defSearchCons;
            }

            // Get the correct queue for a search request
            MessageAgent agent;
            var myqueue = queue;
            if (myqueue == null)
            {
                agent = new MessageAgent();
                if (request.Type == LdapMessage.SEARCH_REQUEST)
                {
                    myqueue = new LdapSearchQueue(agent);
                }
                else
                {
                    myqueue = new LdapResponseQueue(agent);
                }
            }
            else
            {
                if (request.Type == LdapMessage.SEARCH_REQUEST)
                {
                    agent = queue.MessageAgent;
                }
                else
                {
                    agent = queue.MessageAgent;
                }
            }

            try
            {
                agent.sendMessage(conn, request, cons.TimeLimit, myqueue, null);
            }
            catch (LdapException lex)
            {
                throw lex;
            }
            return myqueue;
        }

        //*************************************************************************
        // helper methods
        //*************************************************************************

        /// <summary>
        ///     Locates the appropriate message agent and sends
        ///     the Ldap request to a directory server.
        /// </summary>
        /// <param name="msg">
        ///     the message to send
        /// </param>
        /// <param name="timeout">
        ///     the timeout value
        /// </param>
        /// <param name="queue">
        ///     the response queue or null
        /// </param>
        /// <returns>
        ///     the LdapResponseQueue for this request
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        private LdapResponseQueue SendRequestToServer(LdapMessage msg, int timeout, LdapResponseQueue queue,
            BindProperties bindProps)
        {
            MessageAgent agent;
            if (queue == null)
            {
                agent = new MessageAgent();
                queue = new LdapResponseQueue(agent);
            }
            else
            {
                agent = queue.MessageAgent;
            }

            agent.sendMessage(conn, msg, timeout, queue, bindProps);
            return queue;
        }

        /// <summary>
        ///     get an LdapConnection object so that we can follow a referral.
        ///     This function is never called if cons.getReferralFollowing() returns
        ///     false.
        /// </summary>
        /// <param name="referrals">
        ///     the array of referral strings
        /// </param>
        /// <returns>
        ///     The referralInfo object
        /// </returns>
        /// <exception>
        ///     LdapReferralException A general exception which includes
        ///     an error message and an Ldap error code.
        /// </exception>
        private ReferralInfo GetReferralConnection(string[] referrals)
        {
            ReferralInfo refInfo = null;
            Exception ex = null;
            LdapConnection rconn = null;
            var rh = defSearchCons.getReferralHandler();
            // Check if we use LdapRebind to get authentication credentials
            if (rh == null || rh is LdapAuthHandler)
            {
                int i;
                for (i = 0; i < referrals.Length; i++)
                {
                    // dn, pw are null in the default case (anonymous bind)
                    string dn = null;
                    sbyte[] pw = null;
                    try
                    {
                        rconn = new LdapConnection();
                        rconn.Constraints = defSearchCons;
                        var url = new LdapUrl(referrals[i]);
                        rconn.Connect(url.Host, url.Port);
                        if (rh is LdapAuthHandler)
                        {
                            // Get application supplied dn and pw
                            var ap = ((LdapAuthHandler) rh).getAuthProvider(url.Host, url.Port);
                            dn = ap.DN;
                            pw = ap.Password;
                        }
                        rconn.Bind(Ldap_V3, dn, pw);
                        ex = null;
                        refInfo = new ReferralInfo(rconn, referrals, url);
                        // Indicate this connection created to follow referral
                        rconn.Connection.ActiveReferral = refInfo;
                        break;
                    }
                    catch (Exception lex)
                    {
                        if (rconn != null)
                        {
                            try
                            {
                                rconn.Disconnect();
                                rconn = null;
                                ex = lex;
                            }
                            catch (LdapException ldapException)
                            {
                                Logger.Log.LogWarning("Exception swallowed", ldapException);
                            }
                        }
                    }
                }
            }
            // Check if application gets connection and does bind
            else
            {
                //  rh instanceof LdapBind
                try
                {
                    rconn = ((LdapBindHandler) rh).Bind(referrals, this);
                    if (rconn == null)
                    {
                        var rex = new LdapReferralException(ExceptionMessages.REFERRAL_ERROR);
                        rex.setReferrals(referrals);
                        throw rex;
                    }
                    // Figure out which Url belongs to the connection
                    for (var idx = 0; idx < referrals.Length; idx++)
                    {
                        try
                        {
                            var url = new LdapUrl(referrals[idx]);
                            if (url.Host.ToUpper().Equals(rconn.Host.ToUpper()) && url.Port == rconn.Port)
                            {
                                refInfo = new ReferralInfo(rconn, referrals, url);
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log.LogWarning("Exception swallowed", e);
                        }
                    }
                    if (refInfo == null)
                    {
                        // Could not match LdapBind.bind() connecction with URL list
                        ex = new LdapLocalException(ExceptionMessages.REFERRAL_BIND_MATCH, LdapException.CONNECT_ERROR);
                    }
                }
                catch (Exception lex)
                {
                    rconn = null;
                    ex = lex;
                }
            }
            if (ex != null)
            {
                // Could not connect to any server, throw an exception
                LdapException ldapex;
                if (ex is LdapReferralException)
                {
                    throw (LdapReferralException) ex;
                }
                if (ex is LdapException)
                {
                    ldapex = (LdapException) ex;
                }
                else
                {
                    ldapex = new LdapLocalException(ExceptionMessages.SERVER_CONNECT_ERROR, new object[] {conn.Host},
                        LdapException.CONNECT_ERROR, ex);
                }
                // Error attempting to follow a referral
                var rex = new LdapReferralException(ExceptionMessages.REFERRAL_ERROR, ldapex);
                rex.setReferrals(referrals);
                // Use last URL string for the failed referral
                rex.FailedReferral = referrals[referrals.Length - 1];
                throw rex;
            }

            // We now have an authenticated connection
            // to be used to follow the referral.
            return refInfo;
        }

        /// <summary>
        ///     Check the result code and throw an exception if needed.
        ///     If referral following is enabled, checks if we need to
        ///     follow a referral
        /// </summary>
        /// <param name="queue">
        ///     - the message queue of the current response
        /// </param>
        /// <param name="cons">
        ///     - the constraints that apply to the request
        /// </param>
        /// <param name="response">
        ///     - the LdapResponse to check
        /// </param>
        private void ChkResultCode(LdapMessageQueue queue, LdapConstraints cons, LdapResponse response)
        {
            if (response.ResultCode == LdapException.REFERRAL && cons.ReferralFollowing)
            {
                // Perform referral following and return
                ArrayList refConn = null;
                try
                {
                    ChaseReferral(queue, cons, response, response.Referrals, 0, false, null);
                }
                finally
                {
                    ReleaseReferralConnections(refConn);
                }
            }
            else
            {
                // Throws exception for non success result
                response.chkResultCode();
            }
        }

        /// <summary>
        ///     Follow referrals if necessary referral following enabled.
        ///     This function is called only by synchronous requests.
        ///     Search responses come here only if referral following is
        ///     enabled and if we are processing a SearchResultReference
        ///     or a Response with a status of REFERRAL, i.e. we are
        ///     going to follow a referral.
        ///     This functions recursively follows a referral until a result
        ///     is returned or until the hop limit is reached.
        /// </summary>
        /// <param name="queue">
        ///     The LdapResponseQueue for this request
        /// </param>
        /// <param name="cons">
        ///     The constraints that apply to the request
        /// </param>
        /// <param name="msg">
        ///     The referral or search reference response message
        /// </param>
        /// <param name="initialReferrals">
        ///     The referral array returned from the
        ///     initial request.
        /// </param>
        /// <param name="hopCount">
        ///     the number of hops already used while
        ///     following this referral
        /// </param>
        /// <param name="searchReference">
        ///     true if the message is a search reference
        /// </param>
        /// <param name="connectionList">
        ///     An optional array list used to store
        ///     the LdapConnection objects used in following the referral.
        /// </param>
        /// <returns>
        ///     The array list used to store the all LdapConnection objects
        ///     used in following the referral.  The list will be empty
        ///     if there were none.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        internal virtual ArrayList ChaseReferral(LdapMessageQueue queue, LdapConstraints cons, LdapMessage msg,
            string[] initialReferrals, int hopCount, bool searchReference, ArrayList connectionList)
        {
            var connList = connectionList;
            ReferralInfo rinfo = null; // referral info
            LdapMessage origMsg;

            // Get a place to store new connections
            if (connList == null)
            {
                connList = new ArrayList(cons.HopLimit);
            }
            // Following referrals or search reference
            string[] refs; // referral list
            if (initialReferrals != null)
            {
                // Search continuation reference from a search request
                refs = initialReferrals;
                origMsg = msg.RequestingMessage;
            }
            else
            {
                // Not a search request
                var resp = (LdapResponse) queue.getResponse();
                if (resp.ResultCode != LdapException.REFERRAL)
                {
                    // Not referral result,throw Exception if nonzero result
                    resp.chkResultCode();
                    return connList;
                }
                // We have a referral response
                refs = resp.Referrals;
                origMsg = resp.RequestingMessage;
            }
            LdapUrl refUrl; // referral represented as URL
            try
            {
                // increment hop count, check max hops
                if (hopCount++ > cons.HopLimit)
                {
                    throw new LdapLocalException("Max hops exceeded", LdapException.REFERRAL_LIMIT_EXCEEDED);
                }
                // Get a connection to follow the referral
                rinfo = GetReferralConnection(refs);
                var rconn = rinfo.ReferralConnection; // new conn for following referral
                refUrl = rinfo.ReferralUrl;
                connList.Add(rconn);


                // rebuild msg into new msg changing msgID,dn,scope,filter
                var newMsg = RebuildRequest(origMsg, refUrl, searchReference);


                // Send new message on new connection
                try
                {
                    MessageAgent agent;
                    if (queue is LdapResponseQueue)
                    {
                        agent = queue.MessageAgent;
                    }
                    else
                    {
                        agent = queue.MessageAgent;
                    }
                    agent.sendMessage(rconn.Connection, newMsg, defSearchCons.TimeLimit, queue, null);
                }
                catch (InterThreadException ex)
                {
                    // Error ending request to referred server
                    var rex = new LdapReferralException(ExceptionMessages.REFERRAL_SEND, LdapException.CONNECT_ERROR,
                        null, ex);
                    rex.setReferrals(initialReferrals);
                    var ref_Renamed = rconn.Connection.ActiveReferral;
                    rex.FailedReferral = ref_Renamed.ReferralUrl.ToString();
                    throw rex;
                }

                if (initialReferrals == null)
                {
                    // For operation results, when all responses are complete,
                    // the stack unwinds back to the original and returns
                    // to the application.
                    // An exception is thrown for an error
                    connList = ChaseReferral(queue, cons, null, null, hopCount, false, connList);
                }
                else
                {
                    // For search, just return to LdapSearchResults object
                    return connList;
                }
            }
            catch (Exception ex)
            {
                if (ex is LdapReferralException)
                {
                    throw (LdapReferralException) ex;
                }
                // Set referral list and failed referral
                var rex = new LdapReferralException(ExceptionMessages.REFERRAL_ERROR, ex);
                rex.setReferrals(refs);
                if (rinfo != null)
                {
                    rex.FailedReferral = rinfo.ReferralUrl.ToString();
                }
                else
                {
                    rex.FailedReferral = refs[refs.Length - 1];
                }
                throw rex;
            }
            return connList;
        }

        /// <summary>
        ///     Builds a new request replacing dn, scope, and filter where approprate
        /// </summary>
        /// <param name="msg">
        ///     the original LdapMessage to build the new request from
        /// </param>
        /// <param name="url">
        ///     the referral url
        /// </param>
        /// <returns>
        ///     a new LdapMessage with appropriate information replaced
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        private LdapMessage RebuildRequest(LdapMessage msg, LdapUrl url, bool reference)
        {
            var dn = url.getDN(); // new base
            string filter = null;

            switch (msg.Type)
            {
                case LdapMessage.SEARCH_REQUEST:
                    if (reference)
                    {
                        filter = url.Filter;
                    }
                    break;
                // We are allowed to get a referral for the following

                case LdapMessage.ADD_REQUEST:
                case LdapMessage.BIND_REQUEST:
                case LdapMessage.COMPARE_REQUEST:
                case LdapMessage.DEL_REQUEST:
                case LdapMessage.EXTENDED_REQUEST:
                case LdapMessage.MODIFY_RDN_REQUEST:
                case LdapMessage.MODIFY_REQUEST:
                    break;
                // The following return no response

                case LdapMessage.ABANDON_REQUEST:
                case LdapMessage.UNBIND_REQUEST:
                default:
                    throw new LdapLocalException(ExceptionMessages.IMPROPER_REFERRAL, new object[] {msg.Type},
                        LdapException.LOCAL_ERROR);
            }

            return msg.Clone(dn, filter, reference);
        }

        /*
        * Release connections acquired by following referrals
        *
        * @param list the list of the connections
        */


        internal virtual void ReleaseReferralConnections(ArrayList list)
        {
            if (list == null)
            {
                return;
            }
            // Release referral connections
            for (var i = list.Count - 1; i >= 0; i--)
            {
                LdapConnection rconn = null;
                try
                {
                    rconn = (LdapConnection) list[i];
                    list.RemoveAt(i);
//					rconn = (LdapConnection) list.RemoveAt(i);
                    rconn.Disconnect();
                }
                catch (IndexOutOfRangeException ex)
                {
                    Logger.Log.LogWarning("Exception swallowed", ex);
                }
                catch (LdapException lex)
                {
                    Logger.Log.LogWarning("Exception swallowed", lex);
                }
            }
        }

        //*************************************************************************
        // Schema Related methods
        //*************************************************************************

        /// <summary>
        ///     Retrieves the schema associated with a particular schema DN in the
        ///     directory server.
        ///     The schema DN for a particular entry is obtained by calling the
        ///     getSchemaDN method of LDAPConnection
        /// </summary>
        /// <param name="schemaDN">
        ///     The schema DN used to fetch the schema.
        /// </param>
        /// <returns>
        ///     An LDAPSchema entry containing schema attributes.  If the
        ///     entry contains no schema attributes then the returned LDAPSchema object
        ///     will be empty.
        /// </returns>
        /// <exception>
        ///     LDAPException     This exception occurs if the schema entry
        ///     cannot be retrieved with this connection.
        /// </exception>
        /// <seealso cref="GetSchemaDN()">
        /// </seealso>
        /// <seealso cref="GetSchemaDN(string)">
        /// </seealso>
        public virtual LdapSchema FetchSchema(string schemaDN)
        {
            var ent = Read(schemaDN, LdapSchema.schemaTypeNames);
            return new LdapSchema(ent);
        }

        /// <summary>
        ///     Retrieves the Distiguished Name (DN) for the schema advertised in the
        ///     root DSE of the Directory Server.
        ///     The DN can be used with the methods fetchSchema and modify to retreive
        ///     and extend schema definitions.  The schema entry is located by reading
        ///     subschemaSubentry attribute of the root DSE.  This is equivalent to
        ///     calling {@link #getSchemaDN(String) } with the DN parameter as an empty
        ///     string: <code>getSchemaDN("")</code>.
        /// </summary>
        /// <returns>
        ///     Distinguished Name of a schema entry in effect for the
        ///     Directory.
        /// </returns>
        /// <exception>
        ///     LDAPException     This exception occurs if the schema DN
        ///     cannot be retrieved, or if the subschemaSubentry attribute associated
        ///     with the root DSE contains multiple values.
        /// </exception>
        /// <seealso cref="FetchSchema">
        /// </seealso>
        /// <seealso cref="Modify">
        /// </seealso>
        public virtual string GetSchemaDN()
        {
            return GetSchemaDN("");
        }

        /// <summary>
        ///     Retrieves the Distiguished Name (DN) of the schema associated with a
        ///     entry in the Directory.
        ///     The DN can be used with the methods fetchSchema and modify to retreive
        ///     and extend schema definitions.  Reads the subschemaSubentry of the entry
        ///     specified.
        /// </summary>
        /// <param name="dn">
        ///     Distinguished Name of any entry.  The subschemaSubentry
        ///     attribute is queried from this entry.
        /// </param>
        /// <returns>
        ///     Distinguished Name of a schema entry in effect for the entry
        ///     identified by <code>dn</code>.
        /// </returns>
        /// <exception>
        ///     LDAPException     This exception occurs if a null or empty
        ///     value is passed as dn, if the subschemasubentry attribute cannot
        ///     be retrieved, or the subschemasubentry contains multiple values.
        /// </exception>
        /// <seealso cref="FetchSchema">
        /// </seealso>
        /// <seealso cref="Modify">
        /// </seealso>
        public virtual string GetSchemaDN(string dn)
        {
            string[] attrSubSchema = {"subschemaSubentry"};

            /* Read the entries subschemaSubentry attribute. Throws an exception if
            * no entries are returned. */
            var ent = Read(dn, attrSubSchema);

            var attr = ent.getAttribute(attrSubSchema[0]);
            var values = attr.StringValueArray;
            if (values == null || values.Length < 1)
            {
                throw new LdapLocalException(ExceptionMessages.NO_SCHEMA, new object[] {dn},
                    LdapException.NO_RESULTS_RETURNED);
            }
            if (values.Length > 1)
            {
                throw new LdapLocalException(ExceptionMessages.MULTIPLE_SCHEMA, new object[] {dn},
                    LdapException.CONSTRAINT_VIOLATION);
            }
            return values[0];
        }
    }
}