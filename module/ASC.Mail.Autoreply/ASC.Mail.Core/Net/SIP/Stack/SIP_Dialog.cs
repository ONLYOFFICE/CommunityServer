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


namespace ASC.Mail.Net.SIP.Stack
{
    #region usings

    using System;
    using Message;

    #endregion

    /// <summary>
    /// This class is base class for SIP dialogs. Defined in RFC 3261 12.
    /// </summary>
    public class SIP_Dialog
    {
        #region Events

        /// <summary>
        /// This event is raised when Dialog state has changed.
        /// </summary>
        public event EventHandler StateChanged = null;

        /// <summary>
        /// This event is raised when remote-party terminates dialog with BYE request.
        /// </summary>
        /// <remarks>This event is useful only if the application is interested in processing the headers in the BYE message.</remarks>
        public event EventHandler<SIP_RequestReceivedEventArgs> TerminatedByRemoteParty = null;

        #endregion

        #region Members

        private readonly DateTime m_CreateTime;
        private string m_CallID = "";
        private bool m_IsSecure;
        private bool m_IsTerminatedByRemoteParty;
        private int m_LocalSeqNo;
        private string m_LocalTag = "";
        private SIP_Flow m_pFlow;
        private SIP_Uri m_pLocalContact;
        private AbsoluteUri m_pLocalUri;
        private object m_pLock = new object();
        private SIP_Uri m_pRemoteTarget;
        private AbsoluteUri m_pRemoteUri;
        private SIP_t_AddressParam[] m_pRouteSet;
        private SIP_Stack m_pStack;
        private int m_RemoteSeqNo;
        private string m_RemoteTag = "";
        private SIP_DialogState m_State = SIP_DialogState.Early;

        #endregion

        #region Properties

        /// <summary>
        /// Gets an object that can be used to synchronize access to the dialog.
        /// </summary>
        public object SyncRoot
        {
            get { return m_pLock; }
        }

        /// <summary>
        /// Gets dialog state.
        /// </summary>
        public SIP_DialogState State
        {
            get { return m_State; }
        }

        /// <summary>
        /// Gets owner stack.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Stack Stack
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pStack;
            }
        }

        /// <summary>
        /// Gets dialog creation time.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public DateTime CreateTime
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_CreateTime;
            }
        }

        /// <summary>
        /// Gets dialog ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public string ID
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return CallID + "-" + LocalTag + "-" + RemoteTag;
            }
        }

        /// <summary>
        /// Get call ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public string CallID
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_CallID;
            }
        }

        /// <summary>
        /// Gets local-tag.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public string LocalTag
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LocalTag;
            }
        }

        /// <summary>
        /// Gets remote-tag.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public string RemoteTag
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_RemoteTag;
            }
        }

        /// <summary>
        /// Gets local sequence number.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public int LocalSeqNo
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_LocalSeqNo;
            }
        }

        /// <summary>
        /// Gets remote sequence number.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public int RemoteSeqNo
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_RemoteSeqNo;
            }
        }

        /// <summary>
        /// Gets local URI.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public AbsoluteUri LocalUri
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pLocalUri;
            }
        }

        /// <summary>
        /// Gets remote URI.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public AbsoluteUri RemoteUri
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRemoteUri;
            }
        }

        /// <summary>
        /// Gets local contact URI.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Uri LocalContact
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pLocalContact;
            }
        }

        /// <summary>
        /// Gets remote target URI.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Uri RemoteTarget
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRemoteTarget;
            }
        }

        /// <summary>
        /// Gets if dialog uses secure transport.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public bool IsSecure
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_IsSecure;
            }
        }

        /// <summary>
        /// Gets route set.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_t_AddressParam[] RouteSet
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRouteSet;
            }
        }

        /// <summary>
        /// Gets if dialog was terminated by remote-party.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public bool IsTerminatedByRemoteParty
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_IsTerminatedByRemoteParty;
            }
        }

        /// <summary>
        /// Gets data flow used to send or receive last SIP message.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Flow Flow
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pFlow;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_Dialog()
        {
            m_CreateTime = DateTime.Now;
            m_pRouteSet = new SIP_t_AddressParam[0];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public virtual void Dispose()
        {
            lock (m_pLock)
            {
                if (State == SIP_DialogState.Disposed)
                {
                    return;
                }

                SetState(SIP_DialogState.Disposed, true);

                m_pStack = null;
                m_CallID = null;
                m_LocalTag = null;
                m_RemoteTag = null;
                m_pLocalUri = null;
                m_pRemoteUri = null;
                m_pLocalContact = null;
                m_pRemoteTarget = null;
                m_pRouteSet = null;
                m_pFlow = null;
                m_pLock = null;
            }
        }

        /// <summary>
        /// Terminates dialog.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        public void Terminate()
        {
            Terminate(null, true);
        }

        /// <summary>
        /// Terminates dialog.
        /// </summary>
        /// <param name="reason">Termination reason. This value may be null.</param>
        /// <param name="sendBye">If true BYE is sent to remote party.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        public virtual void Terminate(string reason, bool sendBye)
        {
            lock (m_pLock)
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (State == SIP_DialogState.Terminating || State == SIP_DialogState.Terminated)
                {
                    return;
                }

                /* RFC 3261 15.
                    The caller's UA MAY send a BYE for either confirmed or early dialogs, and the callee's UA MAY send a BYE on
                    confirmed dialogs, but MUST NOT send a BYE on early dialogs.
                 
                   RFC 3261 15.1.
                    Once the BYE is constructed, the UAC core creates a new non-INVITE client transaction, and passes it the BYE request.
                    The UAC MUST consider the session terminated (and therefore stop sending or listening for media) as soon as the BYE 
                    request is passed to the client transaction. If the response for the BYE is a 481 (Call/Transaction Does Not Exist) 
                    or a 408 (Request Timeout) or no response at all is received for the BYE (that is, a timeout is returned by the 
                    client transaction), the UAC MUST consider the session and the dialog terminated.
                */

                SetState(SIP_DialogState.Terminating, true);

                if (sendBye)
                {
                    // TODO: UAS early

                    if (State == SIP_DialogState.Confirmed)
                    {
                        SIP_Request bye = CreateRequest(SIP_Methods.BYE);
                        if (!string.IsNullOrEmpty(reason))
                        {
                            SIP_t_ReasonValue r = new SIP_t_ReasonValue();
                            r.Protocol = "SIP";
                            r.Text = reason;
                            bye.Reason.Add(r.ToStringValue());
                        }

                        // Send BYE, just wait BYE to complete, we don't care about response code.
                        SIP_RequestSender sender = CreateRequestSender(bye);
                        sender.Completed += delegate { SetState(SIP_DialogState.Terminated, true); };
                        sender.Start();
                    }
                }
                else
                {
                    SetState(SIP_DialogState.Terminated, true);
                }
            }
        }

        /// <summary>
        /// Creates new SIP request using this dialog info.
        /// </summary>
        /// <param name="method">SIP method.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>method</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <returns>Returns created request.</returns>
        public SIP_Request CreateRequest(string method)
        {
            if (State == SIP_DialogState.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (method == string.Empty)
            {
                throw new ArgumentException("Argument 'method' value must be specified.");
            }

            /* RFC 3261 12.2.1.1.
                A request within a dialog is constructed by using many of the
                components of the state stored as part of the dialog.

                The URI in the To field of the request MUST be set to the remote URI
                from the dialog state.  The tag in the To header field of the request
                MUST be set to the remote tag of the dialog ID.  The From URI of the
                request MUST be set to the local URI from the dialog state.  The tag
                in the From header field of the request MUST be set to the local tag
                of the dialog ID.  If the value of the remote or local tags is null,
                the tag parameter MUST be omitted from the To or From header fields,
                respectively.

                The Call-ID of the request MUST be set to the Call-ID of the dialog.
                Requests within a dialog MUST contain strictly monotonically
                increasing and contiguous CSeq sequence numbers (increasing-by-one)
                in each direction (excepting ACK and CANCEL of course, whose numbers
                equal the requests being acknowledged or cancelled).  Therefore, if
                the local sequence number is not empty, the value of the local
                sequence number MUST be incremented by one, and this value MUST be
                placed into the CSeq header field.  If the local sequence number is
                empty, an initial value MUST be chosen using the guidelines of
                Section 8.1.1.5.  The method field in the CSeq header field value
                MUST match the method of the request.

                    With a length of 32 bits, a client could generate, within a single
                    call, one request a second for about 136 years before needing to
                    wrap around.  The initial value of the sequence number is chosen
                    so that subsequent requests within the same call will not wrap
                    around.  A non-zero initial value allows clients to use a time-
                    based initial sequence number.  A client could, for example,
                    choose the 31 most significant bits of a 32-bit second clock as an
                    initial sequence number.

                The UAC uses the remote target and route set to build the Request-URI
                and Route header field of the request.

                If the route set is empty, the UAC MUST place the remote target URI
                into the Request-URI.  The UAC MUST NOT add a Route header field to
                the request.

                If the route set is not empty, and the first URI in the route set
                contains the lr parameter (see Section 19.1.1), the UAC MUST place
                the remote target URI into the Request-URI and MUST include a Route
                header field containing the route set values in order, including all
                parameters.

                If the route set is not empty, and its first URI does not contain the
                lr parameter, the UAC MUST place the first URI from the route set
                into the Request-URI, stripping any parameters that are not allowed
                in a Request-URI.  The UAC MUST add a Route header field containing
                the remainder of the route set values in order, including all
                parameters.  The UAC MUST then place the remote target URI into the
                Route header field as the last value.

                For example, if the remote target is sip:user@remoteua and the route
                set contains:
                    <sip:proxy1>,<sip:proxy2>,<sip:proxy3;lr>,<sip:proxy4>

                The request will be formed with the following Request-URI and Route
                header field:
                    METHOD sip:proxy1
                    Route: <sip:proxy2>,<sip:proxy3;lr>,<sip:proxy4>,<sip:user@remoteua>

                If the first URI of the route set does not contain the lr
                parameter, the proxy indicated does not understand the routing
                mechanisms described in this document and will act as specified in
                RFC 2543, replacing the Request-URI with the first Route header
                field value it receives while forwarding the message.  Placing the
                Request-URI at the end of the Route header field preserves the
                information in that Request-URI across the strict router (it will
                be returned to the Request-URI when the request reaches a loose-
                router).

                A UAC SHOULD include a Contact header field in any target refresh
                requests within a dialog, and unless there is a need to change it,
                the URI SHOULD be the same as used in previous requests within the
                dialog.  If the "secure" flag is true, that URI MUST be a SIPS URI.
                As discussed in Section 12.2.2, a Contact header field in a target
                refresh request updates the remote target URI.  This allows a UA to
                provide a new contact address, should its address change during the
                duration of the dialog.

                However, requests that are not target refresh requests do not affect
                the remote target URI for the dialog.

                The rest of the request is formed as described in Section 8.1.1.
            */

            lock (m_pLock)
            {
                SIP_Request request = m_pStack.CreateRequest(method,
                                                             new SIP_t_NameAddress("", m_pRemoteUri),
                                                             new SIP_t_NameAddress("", m_pLocalUri));
                if (m_pRouteSet.Length == 0)
                {
                    request.RequestLine.Uri = m_pRemoteTarget;
                }
                else
                {
                    SIP_Uri topmostRoute = ((SIP_Uri) m_pRouteSet[0].Address.Uri);
                    if (topmostRoute.Param_Lr)
                    {
                        request.RequestLine.Uri = m_pRemoteTarget;
                        for (int i = 0; i < m_pRouteSet.Length; i++)
                        {
                            request.Route.Add(m_pRouteSet[i].ToStringValue());
                        }
                    }
                    else
                    {
                        request.RequestLine.Uri = SIP_Utils.UriToRequestUri(topmostRoute);
                        for (int i = 1; i < m_pRouteSet.Length; i++)
                        {
                            request.Route.Add(m_pRouteSet[i].ToStringValue());
                        }
                    }
                }
                request.To.Tag = m_RemoteTag;
                request.From.Tag = m_LocalTag;
                request.CallID = m_CallID;
                request.CSeq.SequenceNumber = ++m_LocalSeqNo;
                request.Contact.Add(m_pLocalContact.ToString());

                return request;
            }
        }

        /// <summary>
        /// Creates SIP request sender for the specified request.
        /// </summary>
        /// <remarks>All requests sent through this dialog SHOULD use this request sender to send out requests.</remarks>
        /// <param name="request">SIP request.</param>
        /// <returns>Returns created sender.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>request</b> is null.</exception>
        public SIP_RequestSender CreateRequestSender(SIP_Request request)
        {
            lock (m_pLock)
            {
                if (State == SIP_DialogState.Terminated)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                SIP_RequestSender sender = m_pStack.CreateRequestSender(request, Flow);

                return sender;
            }
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Initializes dialog.
        /// </summary>
        /// <param name="stack">Owner stack.</param>
        /// <param name="transaction">Owner transaction.</param>
        /// <param name="response">SIP response what caused dialog creation.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b>,<b>transaction</b> or <b>response</b>.</exception>
        protected internal virtual void Init(SIP_Stack stack,
                                             SIP_Transaction transaction,
                                             SIP_Response response)
        {
            if (stack == null)
            {
                throw new ArgumentNullException("stack");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            m_pStack = stack;

            #region UAS

            /* RFC 3261 12.1.1.
                The UAS then constructs the state of the dialog.  This state MUST be
                maintained for the duration of the dialog.

                If the request arrived over TLS, and the Request-URI contained a SIPS
                URI, the "secure" flag is set to TRUE.

                The route set MUST be set to the list of URIs in the Record-Route
                header field from the request, taken in order and preserving all URI
                parameters.  If no Record-Route header field is present in the
                request, the route set MUST be set to the empty set.  This route set,
                even if empty, overrides any pre-existing route set for future
                requests in this dialog.  The remote target MUST be set to the URI
                from the Contact header field of the request.

                The remote sequence number MUST be set to the value of the sequence
                number in the CSeq header field of the request.  The local sequence
                number MUST be empty.  The call identifier component of the dialog ID
                MUST be set to the value of the Call-ID in the request.  The local
                tag component of the dialog ID MUST be set to the tag in the To field
                in the response to the request (which always includes a tag), and the
                remote tag component of the dialog ID MUST be set to the tag from the
                From field in the request.  A UAS MUST be prepared to receive a
                request without a tag in the From field, in which case the tag is
                considered to have a value of null.

                    This is to maintain backwards compatibility with RFC 2543, which
                    did not mandate From tags.

                The remote URI MUST be set to the URI in the From field, and the
                local URI MUST be set to the URI in the To field.
            */

            if (transaction is SIP_ServerTransaction)
            {
                // TODO: Validate request or client transaction must do it ?

                m_IsSecure = ((SIP_Uri) transaction.Request.RequestLine.Uri).IsSecure;
                m_pRouteSet =
                    (SIP_t_AddressParam[]) Core.ReverseArray(transaction.Request.RecordRoute.GetAllValues());
                m_pRemoteTarget = (SIP_Uri) transaction.Request.Contact.GetTopMostValue().Address.Uri;
                m_RemoteSeqNo = transaction.Request.CSeq.SequenceNumber;
                m_LocalSeqNo = 0;
                m_CallID = transaction.Request.CallID;
                m_LocalTag = response.To.Tag;
                m_RemoteTag = transaction.Request.From.Tag;
                m_pRemoteUri = transaction.Request.From.Address.Uri;
                m_pLocalUri = transaction.Request.To.Address.Uri;
                m_pLocalContact = (SIP_Uri) response.Contact.GetTopMostValue().Address.Uri;
            }

                #endregion

                #region UAC

                /* RFC 3261 12.1.2.
                When a UAC receives a response that establishes a dialog, it
                constructs the state of the dialog.  This state MUST be maintained
                for the duration of the dialog.

                If the request was sent over TLS, and the Request-URI contained a
                SIPS URI, the "secure" flag is set to TRUE.

                The route set MUST be set to the list of URIs in the Record-Route
                header field from the response, taken in reverse order and preserving
                all URI parameters.  If no Record-Route header field is present in
                the response, the route set MUST be set to the empty set.  This route
                set, even if empty, overrides any pre-existing route set for future
                requests in this dialog.  The remote target MUST be set to the URI
                from the Contact header field of the response.

                The local sequence number MUST be set to the value of the sequence
                number in the CSeq header field of the request.  The remote sequence
                number MUST be empty (it is established when the remote UA sends a
                request within the dialog).  The call identifier component of the
                dialog ID MUST be set to the value of the Call-ID in the request.
                The local tag component of the dialog ID MUST be set to the tag in
                the From field in the request, and the remote tag component of the
                dialog ID MUST be set to the tag in the To field of the response.  A
                UAC MUST be prepared to receive a response without a tag in the To
                field, in which case the tag is considered to have a value of null.

                    This is to maintain backwards compatibility with RFC 2543, which
                    did not mandate To tags.

                The remote URI MUST be set to the URI in the To field, and the local
                URI MUST be set to the URI in the From field.
            */

            else
            {
                // TODO: Validate request or client transaction must do it ?

                m_IsSecure = ((SIP_Uri) transaction.Request.RequestLine.Uri).IsSecure;
                m_pRouteSet = (SIP_t_AddressParam[]) Core.ReverseArray(response.RecordRoute.GetAllValues());
                m_pRemoteTarget = (SIP_Uri) response.Contact.GetTopMostValue().Address.Uri;
                m_LocalSeqNo = transaction.Request.CSeq.SequenceNumber;
                m_RemoteSeqNo = 0;
                m_CallID = transaction.Request.CallID;
                m_LocalTag = transaction.Request.From.Tag;
                m_RemoteTag = response.To.Tag;
                m_pRemoteUri = transaction.Request.To.Address.Uri;
                m_pLocalUri = transaction.Request.From.Address.Uri;
                m_pLocalContact = (SIP_Uri) transaction.Request.Contact.GetTopMostValue().Address.Uri;
            }

            #endregion

            m_pFlow = transaction.Flow;
        }

        /// <summary>
        /// Processes specified request through this dialog.
        /// </summary>
        /// <param name="e">Method arguments.</param>
        /// <returns>Returns true if this dialog processed specified response, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>e</b> is null reference.</exception>
        protected internal virtual bool ProcessRequest(SIP_RequestReceivedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (e.Request.RequestLine.Method == SIP_Methods.BYE)
            {
                e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, e.Request));

                m_IsTerminatedByRemoteParty = true;
                OnTerminatedByRemoteParty(e);
                SetState(SIP_DialogState.Terminated, true);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Processes specified response through this dialog.
        /// </summary>
        /// <param name="response">SIP response to process.</param>
        /// <returns>Returns true if this dialog processed specified response, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null.</exception>
        protected internal virtual bool ProcessResponse(SIP_Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            return false;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises <b>StateChanged</b> event.
        /// </summary>
        private void OnStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises <b>TerminatedByRemoteParty</b> event.
        /// </summary>
        /// <param name="bye">BYE request.</param>
        private void OnTerminatedByRemoteParty(SIP_RequestReceivedEventArgs bye)
        {
            if (TerminatedByRemoteParty != null)
            {
                TerminatedByRemoteParty(this, bye);
            }
        }

        #endregion

        /// <summary>
        /// Gets if sepcified request method is target-refresh method.
        /// </summary>
        /// <param name="method">SIP request method.</param>
        /// <returns>Returns true if specified method is target-refresh method.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>method</b> is null reference.</exception>
        protected bool IsTargetRefresh(string method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            method = method.ToUpper();

            // RFC 5057 5.4. Target Refresh Requests.         
            if (method == SIP_Methods.INVITE)
            {
                return true;
            }
            else if (method == SIP_Methods.UPDATE)
            {
                return true;
            }
            else if (method == SIP_Methods.SUBSCRIBE)
            {
                return true;
            }
            else if (method == SIP_Methods.NOTIFY)
            {
                return true;
            }
            else if (method == SIP_Methods.REFER)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets dialog state.
        /// </summary>
        /// <param name="state">New dialog state,</param>
        /// <param name="raiseEvent">If true, StateChanged event is raised after state change.</param>
        protected void SetState(SIP_DialogState state, bool raiseEvent)
        {
            m_State = state;

            if (raiseEvent)
            {
                OnStateChanged();
            }

            if (m_State == SIP_DialogState.Terminated)
            {
                Dispose();
            }
        }
    }
}