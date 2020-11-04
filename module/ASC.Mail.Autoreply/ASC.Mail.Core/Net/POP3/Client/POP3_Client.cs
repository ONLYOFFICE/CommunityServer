/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System.Globalization;

namespace ASC.Mail.Net.POP3.Client
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Principal;
    using IO;
    using TCP;

    #endregion

    /// <summary>
    /// POP3 Client. Defined in RFC 1939.
    /// </summary>
    /// <example>
    /// <code>
    /// 
    /// /*
    ///  To make this code to work, you need to import following namespaces:
    ///  using LumiSoft.Net.Mime;
    ///  using LumiSoft.Net.POP3.Client; 
    ///  */
    /// 
    /// using(POP3_Client c = new POP3_Client()){
    ///		c.Connect("ivx",WellKnownPorts.POP3);
    ///		c.Authenticate("test","test",true);
    ///				
    ///		// Get first message if there is any
    ///		if(c.Messages.Count > 0){
    ///			// Do your suff
    ///			
    ///			// Parse message
    ///			Mime m = Mime.Parse(c.Messages[0].MessageToByte());
    ///			string from = m.MainEntity.From;
    ///			string subject = m.MainEntity.Subject;			
    ///			// ... 
    ///		}		
    ///	}
    /// </code>
    /// </example>
    public class POP3_Client : TCP_Client
    {
        #region Nested type: AuthenticateDelegate

        /// <summary>
        /// Internal helper method for asynchronous Authenticate method.
        /// </summary>
        private delegate void AuthenticateDelegate(string userName, string password, bool tryApop);

        #endregion

        #region Nested type: NoopDelegate

        /// <summary>
        /// Internal helper method for asynchronous Noop method.
        /// </summary>
        private delegate void NoopDelegate();

        #endregion

        #region Nested type: ResetDelegate

        /// <summary>
        /// Internal helper method for asynchronous Reset method.
        /// </summary>
        private delegate void ResetDelegate();

        #endregion

        #region Nested type: StartTLSDelegate

        /// <summary>
        /// Internal helper method for asynchronous StartTLS method.
        /// </summary>
        private delegate void StartTLSDelegate();

        #endregion

        #region Members

        private readonly List<string> m_pExtCapabilities;
        private string m_ApopHashKey = "";
        private string m_GreetingText = "";
        private bool m_IsUidlSupported;
        private GenericIdentity m_pAuthdUserIdentity;
        private POP3_ClientMessageCollection m_pMessages;

        #endregion

        #region Properties

        /// <summary>
        /// Gets greeting text which was sent by POP3 server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and POP3 client is not connected.</exception>
        public string GreetingText
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_GreetingText;
            }
        }

        /// <summary>
        /// Gets POP3 exteneded capabilities supported by POP3 server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and POP3 client is not connected.</exception>
        [Obsolete("USe ExtendedCapabilities instead !")]
        public string[] ExtenededCapabilities
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pExtCapabilities.ToArray();
            }
        }

        /// <summary>
        /// Gets POP3 exteneded capabilities supported by POP3 server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and POP3 client is not connected.</exception>
        public string[] ExtendedCapabilities
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pExtCapabilities.ToArray();
            }
        }

        /// <summary>
        /// Gets if POP3 server supports UIDL command.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and 
        /// POP3 client is not connected and authenticated.</exception>
        public bool IsUidlSupported
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }
                if (!IsAuthenticated)
                {
                    throw new InvalidOperationException("You must authenticate first.");
                }

                return m_IsUidlSupported;
            }
        }

        /// <summary>
        /// Gets messages collection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and 
        /// POP3 client is not connected and authenticated.</exception>
        public POP3_ClientMessageCollection Messages
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }
                if (!IsAuthenticated)
                {
                    throw new InvalidOperationException("You must authenticate first.");
                }

                return m_pMessages;
            }
        }

        /// <summary>
        /// Gets session authenticated user identity, returns null if not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and POP3 client is not connected.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_pAuthdUserIdentity;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public POP3_Client(int default_login_delay)
        {
            m_pExtCapabilities = new List<string>();
            this.default_login_delay = default_login_delay;
            AuthSucceed += OnAuthSucceed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Closes connection to POP3 server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected.</exception>
        public override void Disconnect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("POP3 client is not connected.");
            }

            try
            {
                // Send QUIT command to server.                
                WriteLine("QUIT");
            }
            catch {}

            try
            {
                base.Disconnect();
            }
            catch {}

            if (m_pMessages != null)
            {
                m_pMessages.Dispose();
                m_pMessages = null;
            }
            m_ApopHashKey = "";
            m_pExtCapabilities.Clear();
            m_IsUidlSupported = false;
        }

        /// <summary>
        /// Starts switching to SSL.
        /// </summary>
        /// <returns>An IAsyncResult that references the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected or is authenticated or is already secure connection.</exception>
        public IAsyncResult BeginStartTLS(AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The STLS command is only valid in non-authenticated state.");
            }
            if (IsSecureConnection)
            {
                throw new InvalidOperationException("Connection is already secure.");
            }

            StartTLSDelegate asyncMethod = StartTLS;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous StartTLS request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void EndStartTLS(IAsyncResult asyncResult)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultState castedAsyncResult = asyncResult as AsyncResultState;
            if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this)
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginReset was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is StartTLSDelegate)
            {
                ((StartTLSDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
        }

        /// <summary>
        /// Switches POP3 connection to SSL.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected or is authenticated or is already secure connection.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void StartTLS()
        {
            /* RFC 2595 4. POP3 STARTTLS extension.
                Arguments: none

                Restrictions:
                    Only permitted in AUTHORIZATION state.
             
                Possible Responses:
                     +OK -ERR

                 Examples:
                     C: STLS
                     S: +OK Begin TLS negotiation
                     <TLS negotiation, further commands are under TLS layer>
                       ...
                     C: STLS
                     S: -ERR Command not permitted when TLS active
            */

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "The STLS command is only valid in non-authenticated state.");
            }
            if (IsSecureConnection)
            {
                throw new InvalidOperationException("Connection is already secure.");
            }

            WriteLine("STLS");

            string line = ReadLine();
            if (!line.ToUpper().StartsWith("+OK"))
            {
                throw new POP3_ClientException(line);
            }

            SwitchToSecure();
        }

        /// <summary>
        /// Starts authentication.
        /// </summary>
        /// <param name="userName">User login name.</param>
        /// <param name="password">Password.</param>
        /// <param name="tryApop"> If true and POP3 server supports APOP, then APOP is used, otherwise normal login used.</param>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected or is already authenticated.</exception>
        public IAsyncResult BeginAuthenticate(string userName,
                                              string password,
                                              bool tryApop,
                                              AsyncCallback callback,
                                              object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException("Session is already authenticated.");
            }

            AuthenticateDelegate asyncMethod = Authenticate;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(userName,
                                                              password,
                                                              tryApop,
                                                              asyncState.CompletedCallback,
                                                              null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous authentication request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void EndAuthenticate(IAsyncResult asyncResult)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultState castedAsyncResult = asyncResult as AsyncResultState;
            if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this)
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginAuthenticate method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginAuthenticate was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is AuthenticateDelegate)
            {
                ((AuthenticateDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(
                    castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginAuthenticate method.");
            }
        }

        public delegate void AuthSucceedDelegate();
        public event AuthSucceedDelegate AuthSucceed;

        public delegate void AuthFailedDelegate(string response_line);
        public event AuthFailedDelegate AuthFailed;

        /// <summary>
        /// Authenticates user.
        /// </summary>
        /// <param name="userName">User login name.</param>
        /// <param name="password">Password.</param>
        /// <param name="tryApop"> If true and POP3 server supports APOP, then APOP is used, otherwise normal login used.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected or is already authenticated.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void Authenticate(string userName, string password, bool tryApop)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (IsAuthenticated)
            {
                throw new InvalidOperationException("Session is already authenticated.");
            }

            // Supports APOP, use it.
            if (tryApop && m_ApopHashKey.Length > 0)
            {
                string hexHash = Core.ComputeMd5(m_ApopHashKey + password, true);

                int countWritten = TcpStream.WriteLine("APOP " + userName + " " + hexHash);
                LogAddWrite(countWritten, "APOP " + userName + " " + hexHash);

                string line = ReadLine();
                if (line.StartsWith("+OK"))
                {
                    m_pAuthdUserIdentity = new GenericIdentity(userName, "apop");
                }
                else
                {
                    if (AuthFailed != null)
                    {
                        AuthFailed.Invoke(line);
                    }
                    throw new POP3_ClientException(line);
                }
            }
                // Use normal LOGIN, don't support APOP.
            else
            {
                 int countWritten = TcpStream.WriteLine("USER " + userName);
                LogAddWrite(countWritten, "USER " + userName);

                string line = ReadLine();
                if (line.StartsWith("+OK"))
                {
                    countWritten = TcpStream.WriteLine("PASS " + password);
                    LogAddWrite(countWritten, "PASS <***REMOVED***>");

                    line = ReadLine();
                    if (line.StartsWith("+OK"))
                    {
                        m_pAuthdUserIdentity = new GenericIdentity(userName, "pop3-user/pass");
                    }
                    else
                    {
                        if (AuthFailed != null)
                        {
                            AuthFailed.Invoke(line);
                        }
                        throw new POP3_ClientException(line);
                    }
                }
                else
                {
                    if (AuthFailed != null)
                    {
                        AuthFailed.Invoke(line);
                    }
                    throw new POP3_ClientException(line);
                }
            }

            if (IsAuthenticated)
            {
                if (AuthSucceed != null)
                {
                    AuthSucceed.Invoke();
                }
                FillMessages();
            }
        }

        private void OnAuthSucceed()
        {
            if (need_precise_login_delay)
            {
                GetCAPA_Parameters();
                LoginDelay = ObtainLoginDelay(default_login_delay);
            }
        }

        /// <summary>
        /// Starts sending NOOP command to server. This method can be used for keeping connection alive(not timing out).
        /// </summary>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected.</exception>
        public IAsyncResult BeginNoop(AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            NoopDelegate asyncMethod = Noop;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous Noop request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void EndNoop(IAsyncResult asyncResult)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultState castedAsyncResult = asyncResult as AsyncResultState;
            if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this)
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginNoop method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginNoop was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is NoopDelegate)
            {
                ((NoopDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginNoop method.");
            }
        }

        /// <summary>
        /// Send NOOP command to server. This method can be used for keeping connection alive(not timing out).
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void Noop()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The NOOP command is only valid in TRANSACTION state.");
            }

            /* RFC 1939 5 NOOP.
                Arguments: none

                Restrictions:
                    may only be given in the TRANSACTION state

                Discussion:
                    The POP3 server does nothing, it merely replies with a
                    positive response.

                Possible Responses:
                    +OK

                Examples:
                    C: NOOP
                    S: +OK
            */

            WriteLine("NOOP");

            string line = ReadLine();
            if (!line.ToUpper().StartsWith("+OK"))
            {
                throw new POP3_ClientException(line);
            }
        }

        /// <summary>
        /// Starts resetting session. Messages marked for deletion will be unmarked.
        /// </summary>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected and authenticated.</exception>
        public IAsyncResult BeginReset(AsyncCallback callback, object state)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The RSET command is only valid in authenticated state.");
            }

            ResetDelegate asyncMethod = Reset;
            AsyncResultState asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(asyncState.CompletedCallback, null));

            return asyncState;
        }

        /// <summary>
        /// Ends a pending asynchronous reset request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid <b>asyncResult</b> passed to this method.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void EndReset(IAsyncResult asyncResult)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultState castedAsyncResult = asyncResult as AsyncResultState;
            if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this)
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
            if (castedAsyncResult.IsEndCalled)
            {
                throw new InvalidOperationException(
                    "BeginReset was previously called for the asynchronous connection.");
            }

            castedAsyncResult.IsEndCalled = true;
            if (castedAsyncResult.AsyncDelegate is ResetDelegate)
            {
                ((ResetDelegate) castedAsyncResult.AsyncDelegate).EndInvoke(castedAsyncResult.AsyncResult);
            }
            else
            {
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginReset method.");
            }
        }

        /// <summary>
        /// Resets session. Messages marked for deletion will be unmarked.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when POP3 client is not connected and authenticated.</exception>
        /// <exception cref="POP3_ClientException">Is raised when POP3 server returns error.</exception>
        public void Reset()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("The RSET command is only valid in TRANSACTION state.");
            }

            /* RFC 1939 5. RSET.
                Arguments: none

                Restrictions:
                    may only be given in the TRANSACTION state

                Discussion:
                    If any messages have been marked as deleted by the POP3
                    server, they are unmarked.  The POP3 server then replies
                    with a positive response.

                Possible Responses:
                    +OK

                Examples:
                    C: RSET
                    S: +OK maildrop has 2 messages (320 octets)
			*/

            WriteLine("RSET");

            // Read first line of reply, check if it's ok.
            string line = ReadLine();
            if (!line.StartsWith("+OK"))
            {
                throw new POP3_ClientException(line);
            }

            foreach (POP3_ClientMessage message in m_pMessages)
            {
                message.SetMarkedForDeletion(false);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// This method is called after TCP client has sucessfully connected.
        /// </summary>
        protected override void OnConnected()
        {
            // Read first line of reply, check if it's ok.
            string line = ReadLine();
            if (line.ToUpper().StartsWith("+OK"))
            {
                m_GreetingText = line.Substring(3).Trim();

                // Try to read APOP hash key, if supports APOP.
                if (line.IndexOf("<") > -1 && line.IndexOf(">") > -1)
                {
                    m_ApopHashKey = line.Substring(line.IndexOf("<"),
                                                   line.LastIndexOf(">") - line.IndexOf("<") + 1);
                }
            }
            else
            {
                throw new POP3_ClientException(line);
            }

            // Try to get POP3 server supported capabilities, if command not supported, just skip tat command.
            GetCAPA_Parameters();
            LoginDelay = ObtainLoginDelay(default_login_delay);
        }

        private static int GetIntParam(string capa, int position /* 1-based */, int unspecifiedValue)
        {
            var capaParams = capa.Split(' ');
            if (capaParams.Length >= position)
            {
                int param;
                if (int.TryParse(capaParams[position-1], NumberStyles.Integer, CultureInfo.InvariantCulture, out param))
                {
                    return param;
                }
            }
            return unspecifiedValue;
        }

        public int LoginDelay { get; set; }
        private bool need_precise_login_delay = false;
        private int default_login_delay;

        #endregion

        #region Utility methods

        /// <summary>
        /// Fills messages info.
        /// </summary>
        private void FillMessages()
        {
            m_pMessages = new POP3_ClientMessageCollection(this);

            /*
                First make messages info, then try to add UIDL if server supports.
            */

            /* NOTE: If reply is +OK, this is multiline respone and is terminated with '.'.
			Examples:
				C: LIST
				S: +OK 2 messages (320 octets)
				S: 1 120				
				S: 2 200
				S: .
				...
				C: LIST 3
				S: -ERR no such message, only 2 messages in maildrop
			*/

            WriteLine("LIST");

            // Read first line of reply, check if it's ok.
            string line = ReadLine();
            if (!string.IsNullOrEmpty(line) && line.StartsWith("+OK"))
            {
                // Read lines while get only '.' on line itshelf.
                while (true)
                {
                    line = ReadLine();

                    // End of data
                    if (line.Trim() == ".")
                    {
                        break;
                    }
                    else
                    {
                        string[] no_size = line.Trim().Split(new[] {' '});
                        m_pMessages.Add(Convert.ToInt32(no_size[1]));
                    }
                }
            }
            else
            {
                throw new POP3_ClientException(line);
            }

            // Try to fill messages UIDs.
            /* NOTE: If reply is +OK, this is multiline respone and is terminated with '.'.
			Examples:
				C: UIDL
				S: +OK
				S: 1 whqtswO00WBw418f9t5JxYwZ
				S: 2 QhdPYR:00WBw1Ph7x7
				S: .
				...
				C: UIDL 3
				S: -ERR no such message
			*/

            WriteLine("UIDL");

            // Read first line of reply, check if it's ok
            line = ReadLine();
            if (line.StartsWith("+OK"))
            {
                m_IsUidlSupported = true;

                // Read lines while get only '.' on line itshelf.
                while (true)
                {
                    line = ReadLine();

                    // End of data
                    if (line.Trim() == ".")
                    {
                        break;
                    }
                    else
                    {
                        string[] no_uid = line.Trim().Split(new[] {' '});
                        m_pMessages[Convert.ToInt32(no_uid[0]) - 1].UIDL = no_uid[1];
                    }
                }
            }
            else
            {
                m_IsUidlSupported = false;
            }
        }

        /// <summary>
        /// Executes CAPA command and reads its parameters.
        /// 
        /// RFC 2449 CAPA
        ///        Arguments:
        ///            none
        ///
        ///        Restrictions:
        ///            none
        ///
        ///        Discussion:
        ///            An -ERR response indicates the capability command is not
        ///            implemented and the client will have to probe for
        ///            capabilities as before.
        ///
        ///            An +OK response is followed by a list of capabilities, one
        ///            per line.  Each capability name MAY be followed by a single
        ///            space and a space-separated list of parameters.  Each
        ///            capability line is limited to 512 octets (including the
        ///            CRLF).  The capability list is terminated by a line
        ///            containing a termination octet (".") and a CRLF pair.
        ///
        ///        Possible Responses:
        ///            +OK -ERR
        ///
        ///        Examples:
        ///            C: CAPA
        ///            S: +OK Capability list follows
        ///            S: TOP
        ///            S: USER
        ///            S: SASL CRAM-MD5 KERBEROS_V4
        ///            S: RESP-CODES
        ///            S: LOGIN-DELAY 900
        ///            S: PIPELINING
        ///            S: EXPIRE 60
        ///            S: UIDL
        ///            S: IMPLEMENTATION Shlemazle-Plotz-v302
        ///            S: .
        ///
        /// Note: beware that parameters list may be different in authentication step and in transaction step
        /// </summary>
        private void GetCAPA_Parameters()
        {
            m_pExtCapabilities.Clear();

            WriteLine("CAPA");

            // CAPA command supported, read capabilities.
            if (ReadLine().ToUpper().StartsWith("+OK"))
            {
                string line;
                while ((line = ReadLine()) != ".")
                {
                    m_pExtCapabilities.Add(line.ToUpper());
                }
            }
        }
        
        /// <summary>
        /// Obtains LOOGIN-DELAY tag from CAPA parameters
        /// </summary>
        /// <returns>login delay read or 'default_value' if the parameter is absent</returns>
        private int ObtainLoginDelay(int default_value)
        {
            need_precise_login_delay = m_pExtCapabilities.Count != 0;
            foreach (string capa_line in m_pExtCapabilities)
            {
                if (capa_line.StartsWith("LOGIN-DELAY"))
                {
                    string[] capaParams = capa_line.Split(' ');
                    if (capaParams.Length > 1)
                    {
                        int delay;
                        if (int.TryParse(capaParams[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out delay))
                        {
                            need_precise_login_delay = capaParams.Length > 2 && capaParams[2].ToUpper() == "USER";
                            return delay;
                        }
                    }
                }
            }
            return default_value;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Marks specified message for deletion.
        /// </summary>
        /// <param name="sequenceNumber">Message sequence number.</param>
        internal void MarkMessageForDeletion(int sequenceNumber)
        {
            WriteLine("DELE " + sequenceNumber);

            // Read first line of reply, check if it's ok.
            string line = ReadLine();
            if (!line.StartsWith("+OK"))
            {
                throw new POP3_ClientException(line);
            }
        }

        /// <summary>
        /// Stores specified message to the specified stream.
        /// </summary>
        /// <param name="sequenceNumber">Message 1 based sequence number.</param>
        /// <param name="stream">Stream where to store message.</param>
        internal void GetMessage(int sequenceNumber, Stream stream)
        {
            WriteLine("RETR " + sequenceNumber);

            // Read first line of reply, check if it's ok.
            string line = ReadLine();
            if (line.StartsWith("+OK"))
            {
                SmartStream.ReadPeriodTerminatedAsyncOP readTermOP =
                    new SmartStream.ReadPeriodTerminatedAsyncOP(stream,
                                                                999999999,
                                                                SizeExceededAction.ThrowException);
                TcpStream.ReadPeriodTerminated(readTermOP, false);
                if (readTermOP.Error != null)
                {
                    throw readTermOP.Error;
                }
                LogAddWrite(readTermOP.BytesStored, readTermOP.BytesStored + " bytes read.");
            }
            else
            {
                throw new POP3_ClientException(line);
            }
        }

        /// <summary>
        /// Stores specified message header + specified lines of body to the specified stream.
        /// </summary>
        /// <param name="sequenceNumber">Message 1 based sequence number.</param>
        /// <param name="stream">Stream where to store data.</param>
        /// <param name="lineCount">Number of lines of message body to get.</param>
        internal void GetTopOfMessage(int sequenceNumber, Stream stream, int lineCount)
        {
            TcpStream.WriteLine("TOP " + sequenceNumber + " " + lineCount);

            // Read first line of reply, check if it's ok.
            string line = ReadLine();
            if (line.StartsWith("+OK"))
            {
                SmartStream.ReadPeriodTerminatedAsyncOP readTermOP =
                    new SmartStream.ReadPeriodTerminatedAsyncOP(stream,
                                                                999999999,
                                                                SizeExceededAction.ThrowException);
                TcpStream.ReadPeriodTerminated(readTermOP, false);
                if (readTermOP.Error != null)
                {
                    throw readTermOP.Error;
                }
                LogAddWrite(readTermOP.BytesStored, readTermOP.BytesStored + " bytes read.");
            }
            else
            {
                throw new POP3_ClientException(line);
            }
        }

        #endregion
    }
}