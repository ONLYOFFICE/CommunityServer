/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
    using System.Threading;
    using System.Timers;
    using Message;

    #endregion

    /// <summary>
    /// Implements SIP client transaction. Defined in rfc 3261 17.1.
    /// </summary>
    public class SIP_ClientTransaction : SIP_Transaction
    {
        #region Events

        /// <summary>
        /// Is raised when transaction received response from remote party.
        /// </summary>
        public event EventHandler<SIP_ResponseReceivedEventArgs> ResponseReceived = null;

        #endregion

        #region Members

        private bool m_IsCanceling;

        private TimerEx m_pTimerA;
        private TimerEx m_pTimerB;
        private TimerEx m_pTimerD;
        private TimerEx m_pTimerE;
        private TimerEx m_pTimerF;
        private TimerEx m_pTimerK;
        private int m_RSeq = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets RSeq value. Value -1 means no reliable provisional response received.
        /// </summary>
        internal int RSeq
        {
            get { return m_RSeq; }

            set { m_RSeq = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">Owner SIP stack.</param>
        /// <param name="flow">SIP data flow which is used to send request.</param>
        /// <param name="request">SIP request that transaction will handle.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b>,<b>flow</b> or <b>request</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        internal SIP_ClientTransaction(SIP_Stack stack, SIP_Flow flow, SIP_Request request)
            : base(stack, flow, request)
        {
            // Log
            if (Stack.Logger != null)
            {
                Stack.Logger.AddText(ID,
                                     "Transaction [branch='" + ID + "';method='" + Method +
                                     "';IsServer=false] created.");
            }

            SetState(SIP_TransactionState.WaitingToStart);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public override void Dispose()
        {
            lock (SyncRoot)
            {
                if (IsDisposed)
                {
                    return;
                }

                // Log
                if (Stack.Logger != null)
                {
                    Stack.Logger.AddText(ID,
                                         "Transaction [branch='" + ID + "';method='" + Method +
                                         "';IsServer=false] disposed.");
                }

                // Kill timers.
                if (m_pTimerA != null)
                {
                    m_pTimerA.Dispose();
                    m_pTimerA = null;
                }
                if (m_pTimerB != null)
                {
                    m_pTimerB.Dispose();
                    m_pTimerB = null;
                }
                if (m_pTimerD != null)
                {
                    m_pTimerD.Dispose();
                    m_pTimerD = null;
                }
                if (m_pTimerE != null)
                {
                    m_pTimerE.Dispose();
                    m_pTimerE = null;
                }
                if (m_pTimerF != null)
                {
                    m_pTimerF.Dispose();
                    m_pTimerF = null;
                }
                if (m_pTimerK != null)
                {
                    m_pTimerK.Dispose();
                    m_pTimerK = null;
                }

                ResponseReceived = null;

                base.Dispose();
            }
        }

        /// <summary>
        /// Starts transaction processing.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when <b>Start</b> is called other state than 'WaitingToStart'.</exception>
        public void Start()
        {
            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                else if (State != SIP_TransactionState.WaitingToStart)
                {
                    throw new InvalidOperationException(
                        "Start method is valid only in 'WaitingToStart' state.");
                }

                // Move processing to thread pool.
                ThreadPool.QueueUserWorkItem(delegate
                                                 {
                                                     lock (SyncRoot)
                                                     {
                                                         #region INVITE

                                                         if (Method == SIP_Methods.INVITE)
                                                         {
                                                             /* RFC 3261 17.1.1.2.
                                The initial state, "calling", MUST be entered when the TU
                                initiates a new client transaction with an INVITE request.  The
                                client transaction MUST pass the request to the transport layer for
                                transmission (see Section 18).  If an unreliable transport is being
                                used, the client transaction MUST start timer A with a value of T1.
                                If a reliable transport is being used, the client transaction SHOULD
                                NOT start timer A (Timer A controls request retransmissions).  For
                                any transport, the client transaction MUST start timer B with a value
                                of 64*T1 seconds (Timer B controls transaction timeouts).
                            */

                                                             SetState(SIP_TransactionState.Calling);

                                                             try
                                                             {
                                                                 // Send initial request.
                                                                 Stack.TransportLayer.SendRequest(Flow,
                                                                                                  Request,
                                                                                                  this);
                                                             }
                                                             catch (Exception x)
                                                             {
                                                                 OnTransportError(x);
                                                                 // NOTE: TransportError event handler could Dispose this transaction, so we need to check it.
                                                                 if (State != SIP_TransactionState.Disposed)
                                                                 {
                                                                     SetState(SIP_TransactionState.Terminated);
                                                                 }
                                                                 return;
                                                             }

                                                             // Start timer A for unreliable transports.
                                                             if (!Flow.IsReliable)
                                                             {
                                                                 m_pTimerA = new TimerEx(
                                                                     SIP_TimerConstants.T1, false);
                                                                 m_pTimerA.Elapsed += m_pTimerA_Elapsed;
                                                                 m_pTimerA.Enabled = true;

                                                                 // Log
                                                                 if (Stack.Logger != null)
                                                                 {
                                                                     Stack.Logger.AddText(ID,
                                                                                          "Transaction [branch='" +
                                                                                          ID + "';method='" +
                                                                                          Method +
                                                                                          "';IsServer=false] timer A(INVITE request retransmission) started, will triger after " +
                                                                                          m_pTimerA.Interval +
                                                                                          ".");
                                                                 }
                                                             }

                                                             // Start timer B.
                                                             m_pTimerB = new TimerEx(
                                                                 64*SIP_TimerConstants.T1, false);
                                                             m_pTimerB.Elapsed += m_pTimerB_Elapsed;
                                                             m_pTimerB.Enabled = true;

                                                             // Log
                                                             if (Stack.Logger != null)
                                                             {
                                                                 Stack.Logger.AddText(ID,
                                                                                      "Transaction [branch='" +
                                                                                      ID + "';method='" +
                                                                                      Method +
                                                                                      "';IsServer=false] timer B(INVITE calling state timeout) started, will triger after " +
                                                                                      m_pTimerB.Interval + ".");
                                                             }
                                                         }

                                                             #endregion

                                                             #region Non-INVITE

                                                         else
                                                         {
                                                             /* RFC 3261 17.1.2.2.
                                The "Trying" state is entered when the TU initiates a new client
                                transaction with a request.  When entering this state, the client
                                transaction SHOULD set timer F to fire in 64*T1 seconds.  The request
                                MUST be passed to the transport layer for transmission.  If an
                                unreliable transport is in use, the client transaction MUST set timer
                                E to fire in T1 seconds.
                            */

                                                             SetState(SIP_TransactionState.Trying);

                                                             // Start timer F.
                                                             m_pTimerF = new TimerEx(
                                                                 64*SIP_TimerConstants.T1, false);
                                                             m_pTimerF.Elapsed += m_pTimerF_Elapsed;
                                                             m_pTimerF.Enabled = true;

                                                             // Log
                                                             if (Stack.Logger != null)
                                                             {
                                                                 Stack.Logger.AddText(ID,
                                                                                      "Transaction [branch='" +
                                                                                      ID + "';method='" +
                                                                                      Method +
                                                                                      "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) started, will triger after " +
                                                                                      m_pTimerF.Interval + ".");
                                                             }

                                                             try
                                                             {
                                                                 // Send initial request.
                                                                 Stack.TransportLayer.SendRequest(Flow,
                                                                                                  Request,
                                                                                                  this);
                                                             }
                                                             catch (Exception x)
                                                             {
                                                                 OnTransportError(x);
                                                                 // NOTE: TransportError event handler could Dispose this transaction, so we need to check it.
                                                                 if (State != SIP_TransactionState.Disposed)
                                                                 {
                                                                     SetState(SIP_TransactionState.Terminated);
                                                                 }
                                                                 return;
                                                             }

                                                             // Start timer E for unreliable transports.
                                                             if (!Flow.IsReliable)
                                                             {
                                                                 m_pTimerE = new TimerEx(
                                                                     SIP_TimerConstants.T1, false);
                                                                 m_pTimerE.Elapsed += m_pTimerE_Elapsed;
                                                                 m_pTimerE.Enabled = true;

                                                                 // Log
                                                                 if (Stack.Logger != null)
                                                                 {
                                                                     Stack.Logger.AddText(ID,
                                                                                          "Transaction [branch='" +
                                                                                          ID + "';method='" +
                                                                                          Method +
                                                                                          "';IsServer=false] timer E(Non-INVITE request retransmission) started, will triger after " +
                                                                                          m_pTimerE.Interval +
                                                                                          ".");
                                                                 }
                                                             }
                                                         }

                                                         #endregion
                                                     }
                                                 });
            }
        }

        /// <summary>
        /// Starts canceling transaction. 
        /// </summary>
        /// <remarks>If client transaction has got final response, canel has no effect and will be ignored.</remarks>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        public override void Cancel()
        {
            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                else if (State == SIP_TransactionState.WaitingToStart)
                {
                    SetState(SIP_TransactionState.Terminated);
                    return;
                }
                else if (m_IsCanceling)
                {
                    return;
                }
                else if (State == SIP_TransactionState.Terminated)
                {
                    // RFC 3261 9.1. We got final response, nothing to cancel.
                    return;
                }
                if (FinalResponse != null)
                {
                    return;
                }

                m_IsCanceling = true;

                /* RFC 3261 9.1.
                    If no provisional response has been received, the CANCEL request MUST
                    NOT be sent; rather, the client MUST wait for the arrival of a
                    provisional response before sending the request.
                */
                if (Responses.Length == 0)
                {
                    // We set canceling flag, so if provisional response arrives, we do cancel.
                }
                else
                {
                    SendCancel();
                }
            }
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Is raised when INVITE timer A triggered.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimerA_Elapsed(object sender, ElapsedEventArgs e)
        {
            /* RFC 3261 17.1.1.2.
                When timer A fires, the client transaction MUST retransmit the
                request by passing it to the transport layer, and MUST reset the
                timer with a value of 2*T1.  The formal definition of retransmit
                within the context of the transaction layer is to take the message
                previously sent to the transport layer and pass it to the transport
                layer once more.
             
                When timer A fires 2*T1 seconds later, the request MUST be
                retransmitted again (assuming the client transaction is still in this
                state).  This process MUST continue so that the request is
                retransmitted with intervals that double after each transmission.
                These retransmissions SHOULD only be done while the client
                transaction is in the "calling" state.
            */

            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Calling)
                {
                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer A(INVITE request retransmission) triggered.");
                    }

                    try
                    {
                        // Retransmit request.
                        Stack.TransportLayer.SendRequest(Flow, Request, this);
                    }
                    catch (Exception x)
                    {
                        OnTransportError(x);
                        SetState(SIP_TransactionState.Terminated);
                        return;
                    }

                    // Update(double current) next transmit time.
                    m_pTimerA.Interval *= 2;
                    m_pTimerA.Enabled = true;

                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer A(INVITE request retransmission) updated, will triger after " +
                                             m_pTimerA.Interval + ".");
                    }
                }
            }
        }

        /// <summary>
        /// Is raised when INVITE timer B triggered.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimerB_Elapsed(object sender, ElapsedEventArgs e)
        {
            /* RFC 3261 17.1.1.2.
                If the client transaction is still in the "Calling" state when timer
                B fires, the client transaction SHOULD inform the TU that a timeout
                has occurred.  The client transaction MUST NOT generate an ACK.  The
                value of 64*T1 is equal to the amount of time required to send seven
                requests in the case of an unreliable transport.
            */

            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Calling)
                {
                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer B(INVITE calling state timeout) triggered.");
                    }

                    OnTimedOut();
                    SetState(SIP_TransactionState.Terminated);

                    // Stop timers A,B.
                    if (m_pTimerA != null)
                    {
                        m_pTimerA.Dispose();
                        m_pTimerA = null;
                    }
                    if (m_pTimerB != null)
                    {
                        m_pTimerB.Dispose();
                        m_pTimerB = null;
                    }
                }
            }
        }

        /// <summary>
        /// Is raised when INVITE timer D triggered.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimerD_Elapsed(object sender, ElapsedEventArgs e)
        {
            /* RFC 3261 17.1.1.2.
                If timer D fires while the client transaction is in the "Completed"
                state, the client transaction MUST move to the terminated state.
            */

            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Completed)
                {
                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer D(INVITE 3xx - 6xx response retransmission wait) triggered.");
                    }

                    SetState(SIP_TransactionState.Terminated);
                }
            }
        }

        /// <summary>
        /// Is raised when Non-INVITE timer E triggered.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimerE_Elapsed(object sender, ElapsedEventArgs e)
        {
            /* RFC 3261 17.1.2.2.
                If timer E fires while in Trying state, the timer is reset, but this time with a value of MIN(2*T1, T2).
                When the timer fires again, it is reset to a MIN(4*T1, T2). This process continues so that 
                retransmissions occur with an exponentially increasing interval that caps at T2. The default 
                value of T2 is 4s, and it represents the amount of time a non-INVITE server transaction will take to
                respond to a request, if it does not respond immediately. For the default values of T1 and T2, 
                this results in intervals of 500 ms, 1 s, 2 s, 4 s, 4 s, 4 s, etc.
            */

            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Trying)
                {
                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer E(-NonINVITE request retransmission) triggered.");
                    }

                    try
                    {
                        // Retransmit request.
                        Stack.TransportLayer.SendRequest(Flow, Request, this);
                    }
                    catch (Exception x)
                    {
                        OnTransportError(x);
                        SetState(SIP_TransactionState.Terminated);
                        return;
                    }

                    // Update(double current) next transmit time.
                    m_pTimerE.Interval = Math.Min(m_pTimerE.Interval*2, SIP_TimerConstants.T2);
                    m_pTimerE.Enabled = true;

                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer E(Non-INVITE request retransmission) updated, will triger after " +
                                             m_pTimerE.Interval + ".");
                    }
                }
            }
        }

        /// <summary>
        /// Is raised when Non-INVITE timer F triggered.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimerF_Elapsed(object sender, ElapsedEventArgs e)
        {
            /* RFC 3261 17.1.2.2.
                If Timer F fires while in the "Trying" state, the client transaction SHOULD inform the TU about the
                timeout, and then it SHOULD enter the "Terminated" state.
            
                If timer F fires while in the "Proceeding" state, the TU MUST be informed of a timeout, and the
                client transaction MUST transition to the terminated state.
            */

            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Trying || State == SIP_TransactionState.Proceeding)
                {
                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) triggered.");
                    }

                    OnTimedOut();
                    SetState(SIP_TransactionState.Terminated);

                    if (m_pTimerE != null)
                    {
                        m_pTimerE.Dispose();
                        m_pTimerE = null;
                    }
                    if (m_pTimerF != null)
                    {
                        m_pTimerF.Dispose();
                        m_pTimerF = null;
                    }
                }
            }
        }

        /// <summary>
        /// Is raised when Non-INVITE timer K triggered.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimerK_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Completed)
                {
                    // Log
                    if (Stack.Logger != null)
                    {
                        Stack.Logger.AddText(ID,
                                             "Transaction [branch='" + ID + "';method='" + Method +
                                             "';IsServer=false] timer K(Non-INVITE 3xx - 6xx response retransmission wait) triggered.");
                    }

                    SetState(SIP_TransactionState.Terminated);
                }
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Creates and send CANCEL request to remote target.
        /// </summary>
        private void SendCancel()
        {
            /* RFC 3261 9.1.
                The following procedures are used to construct a CANCEL request.  The
                Request-URI, Call-ID, To, the numeric part of CSeq, and From header
                fields in the CANCEL request MUST be identical to those in the
                request being cancelled, including tags.  A CANCEL constructed by a
                client MUST have only a single Via header field value matching the
                top Via value in the request being cancelled.  Using the same values
                for these header fields allows the CANCEL to be matched with the
                request it cancels (Section 9.2 indicates how such matching occurs).
                However, the method part of the CSeq header field MUST have a value
                of CANCEL.  This allows it to be identified and processed as a
                transaction in its own right (See Section 17).

                If the request being cancelled contains a Route header field, the
                CANCEL request MUST include that Route header field's values.

                    This is needed so that stateless proxies are able to route CANCEL
                    requests properly.
            */

            SIP_Request cancelRequest = new SIP_Request(SIP_Methods.CANCEL);
            cancelRequest.RequestLine.Uri = Request.RequestLine.Uri;
            cancelRequest.Via.Add(Request.Via.GetTopMostValue().ToStringValue());
            cancelRequest.CallID = Request.CallID;
            cancelRequest.From = Request.From;
            cancelRequest.To = Request.To;
            cancelRequest.CSeq = new SIP_t_CSeq(Request.CSeq.SequenceNumber, SIP_Methods.CANCEL);
            foreach (SIP_t_AddressParam route in Request.Route.GetAllValues())
            {
                cancelRequest.Route.Add(route.ToStringValue());
            }
            cancelRequest.MaxForwards = 70;

            // We must use same data flow to send CANCEL what sent initial request.
            SIP_ClientTransaction transaction = Stack.TransactionLayer.CreateClientTransaction(Flow,
                                                                                               cancelRequest,
                                                                                               false);
            transaction.Start();
        }

        /// <summary>
        /// Creates and sends ACK for final(3xx - 6xx) failure response.
        /// </summary>
        /// <param name="response">SIP response.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null.</exception>
        private void SendAck(SIP_Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("resposne");
            }

            /* RFC 3261 17.1.1.3 Construction of the ACK Request.
                The ACK request constructed by the client transaction MUST contain
                values for the Call-ID, From, and Request-URI that are equal to the
                values of those header fields in the request passed to the transport
                by the client transaction (call this the "original request").  The To
                header field in the ACK MUST equal the To header field in the
                response being acknowledged, and therefore will usually differ from
                the To header field in the original request by the addition of the
                tag parameter.  The ACK MUST contain a single Via header field, and
                this MUST be equal to the top Via header field of the original
                request.  The CSeq header field in the ACK MUST contain the same
                value for the sequence number as was present in the original request,
                but the method parameter MUST be equal to "ACK".
              
                If the INVITE request whose response is being acknowledged had Route
                header fields, those header fields MUST appear in the ACK.  This is
                to ensure that the ACK can be routed properly through any downstream
                stateless proxies.
            */

            SIP_Request ackRequest = new SIP_Request(SIP_Methods.ACK);
            ackRequest.RequestLine.Uri = Request.RequestLine.Uri;
            ackRequest.Via.AddToTop(Request.Via.GetTopMostValue().ToStringValue());
            ackRequest.CallID = Request.CallID;
            ackRequest.From = Request.From;
            ackRequest.To = response.To;
            ackRequest.CSeq = new SIP_t_CSeq(Request.CSeq.SequenceNumber, "ACK");
            foreach (SIP_HeaderField h in response.Header.Get("Route:"))
            {
                ackRequest.Header.Add("Route:", h.Value);
            }
            ackRequest.MaxForwards = 70;

            try
            {
                // Send request to target.
                Stack.TransportLayer.SendRequest(Flow, ackRequest, this);
            }
            catch (SIP_TransportException x)
            {
                OnTransportError(x);
                SetState(SIP_TransactionState.Terminated);
            }
        }

        // FIX ME:

        /// <summary>
        /// Raises ResponseReceived event.
        /// </summary>
        /// <param name="response">SIP response received.</param>
        private void OnResponseReceived(SIP_Response response)
        {
            if (ResponseReceived != null)
            {
                ResponseReceived(this, new SIP_ResponseReceivedEventArgs(Stack, this, response));
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Processes specified response through this transaction.
        /// </summary>
        /// <param name="flow">SIP data flow what received response.</param>
        /// <param name="response">SIP response to process.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>flow</b>,<b>response</b> is null reference.</exception>
        internal void ProcessResponse(SIP_Flow flow, SIP_Response response)
        {
            if (flow == null)
            {
                throw new ArgumentNullException("flow");
            }
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            lock (SyncRoot)
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    return;
                }
                    /* RFC 3261 9.1. CANCEL.
                    *) If provisional response, send CANCEL, we should get '478 Request terminated'.
                    *) If final response, skip canceling, nothing to cancel.
                */
                else if (m_IsCanceling && response.StatusCodeType == SIP_StatusCodeType.Provisional)
                {
                    SendCancel();
                    return;
                }

                // Log
                if (Stack.Logger != null)
                {
                    byte[] responseData = response.ToByteData();

                    Stack.Logger.AddRead(Guid.NewGuid().ToString(),
                                         null,
                                         0,
                                         "Response [transactionID='" + ID + "'; method='" +
                                         response.CSeq.RequestMethod + "'; cseq='" +
                                         response.CSeq.SequenceNumber + "'; " + "transport='" + flow.Transport +
                                         "'; size='" + responseData.Length + "'; statusCode='" +
                                         response.StatusCode + "'; " + "reason='" + response.ReasonPhrase +
                                         "'; received '" + flow.LocalEP + "' <- '" + flow.RemoteEP + "'.",
                                         flow.LocalEP,
                                         flow.RemoteEP,
                                         responseData);
                }

                #region INVITE

                /* RFC 3261 17.1.1.2.
                                                   |INVITE from TU
                                 Timer A fires     |INVITE sent
                                 Reset A,          V                      Timer B fires
                                 INVITE sent +-----------+                or Transport Err.
                                   +---------|           |---------------+inform TU
                                   |         |  Calling  |               |
                                   +-------->|           |-------------->|
                                             +-----------+ 2xx           |
                                                |  |       2xx to TU     |
                                                |  |1xx                  |
                        300-699 +---------------+  |1xx to TU            |
                       ACK sent |                  |                     |
                    resp. to TU |  1xx             V                     |
                                |  1xx to TU  -----------+               |
                                |  +---------|           |               |
                                |  |         |Proceeding |-------------->|
                                |  +-------->|           | 2xx           |
                                |            +-----------+ 2xx to TU     |
                                |       300-699    |                     |
                                |       ACK sent,  |                     |
                                |       resp. to TU|                     |
                                |                  |                     |      NOTE:
                                |  300-699         V                     |
                                |  ACK sent  +-----------+Transport Err. |  transitions
                                |  +---------|           |Inform TU      |  labeled with
                                |  |         | Completed |-------------->|  the event
                                |  +-------->|           |               |  over the action
                                |            +-----------+               |  to take
                                |              ^   |                     |
                                |              |   | Timer D fires       |
                                +--------------+   | -                   |
                                                   |                     |
                                                   V                     |
                                             +-----------+               |
                                             |           |               |
                                             | Terminated|<--------------+
                                             |           |
                                             +-----------+

                */

                if (Method == SIP_Methods.INVITE)
                {
                    #region Calling

                    if (State == SIP_TransactionState.Calling)
                    {
                        // Store response.
                        AddResponse(response);

                        // Stop timer A,B
                        if (m_pTimerA != null)
                        {
                            m_pTimerA.Dispose();
                            m_pTimerA = null;

                            // Log
                            if (Stack.Logger != null)
                            {
                                Stack.Logger.AddText(ID,
                                                     "Transaction [branch='" + ID + "';method='" + Method +
                                                     "';IsServer=false] timer A(INVITE request retransmission) stoped.");
                            }
                        }
                        if (m_pTimerB != null)
                        {
                            m_pTimerB.Dispose();
                            m_pTimerB = null;

                            // Log
                            if (Stack.Logger != null)
                            {
                                Stack.Logger.AddText(ID,
                                                     "Transaction [branch='" + ID + "';method='" + Method +
                                                     "';IsServer=false] timer B(INVITE calling state timeout) stoped.");
                            }
                        }

                        // 1xx response.
                        if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
                        {
                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Proceeding);
                        }
                            // 2xx response.
                        else if (response.StatusCodeType == SIP_StatusCodeType.Success)
                        {
                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Terminated);
                        }
                            // 3xx - 6xx response.
                        else
                        {
                            SendAck(response);
                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Completed);

                            /* RFC 3261 17.1.1.2. 
                                The client transaction SHOULD start timer D when it enters the "Completed" state, 
                                with a value of at least 32 seconds for unreliable transports, and a value of zero 
                                seconds for reliable transports.
                            */
                            m_pTimerD = new TimerEx(Flow.IsReliable ? 0 : Workaround.Definitions.MaxStreamLineLength, false);
                            m_pTimerD.Elapsed += m_pTimerD_Elapsed;
                            // Log
                            if (Stack.Logger != null)
                            {
                                Stack.Logger.AddText(ID,
                                                     "Transaction [branch='" + ID + "';method='" + Method +
                                                     "';IsServer=false] timer D(INVITE 3xx - 6xx response retransmission wait) started, will triger after " +
                                                     m_pTimerD.Interval + ".");
                            }
                            m_pTimerD.Enabled = true;
                        }
                    }

                        #endregion

                        #region Proceeding

                    else if (State == SIP_TransactionState.Proceeding)
                    {
                        // Store response.
                        AddResponse(response);

                        // 1xx response.
                        if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
                        {
                            OnResponseReceived(response);
                        }
                            // 2xx response.
                        else if (response.StatusCodeType == SIP_StatusCodeType.Success)
                        {
                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Terminated);
                        }
                            // 3xx - 6xx response.
                        else
                        {
                            SendAck(response);
                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Completed);

                            /* RFC 3261 17.1.1.2. 
                                The client transaction SHOULD start timer D when it enters the "Completed" state, 
                                with a value of at least 32 seconds for unreliable transports, and a value of zero 
                                seconds for reliable transports.
                            */
                            m_pTimerD = new TimerEx(Flow.IsReliable ? 0 : Workaround.Definitions.MaxStreamLineLength, false);
                            m_pTimerD.Elapsed += m_pTimerD_Elapsed;
                            // Log
                            if (Stack.Logger != null)
                            {
                                Stack.Logger.AddText(ID,
                                                     "Transaction [branch='" + ID + "';method='" + Method +
                                                     "';IsServer=false] timer D(INVITE 3xx - 6xx response retransmission wait) started, will triger after " +
                                                     m_pTimerD.Interval + ".");
                            }
                            m_pTimerD.Enabled = true;
                        }
                    }

                        #endregion

                        #region Completed

                    else if (State == SIP_TransactionState.Completed)
                    {
                        // 3xx - 6xx
                        if (response.StatusCode >= 300)
                        {
                            SendAck(response);
                        }
                    }

                        #endregion

                        #region Terminated

                    else if (State == SIP_TransactionState.Terminated)
                    {
                        // We should never reach here, but if so, do nothing.
                    }

                    #endregion
                }

                    #endregion

                    #region Non-INVITE

                    /* RFC 3251 17.1.2.2
                                               |Request from TU
                                               |send request
                           Timer E             V
                           send request  +-----------+
                               +---------|           |-------------------+
                               |         |  Trying   |  Timer F          |
                               +-------->|           |  or Transport Err.|
                                         +-----------+  inform TU        |
                            200-699         |  |                         |
                            resp. to TU     |  |1xx                      |
                            +---------------+  |resp. to TU              |
                            |                  |                         |
                            |   Timer E        V       Timer F           |
                            |   send req +-----------+ or Transport Err. |
                            |  +---------|           | inform TU         |
                            |  |         |Proceeding |------------------>|
                            |  +-------->|           |-----+             |
                            |            +-----------+     |1xx          |
                            |              |      ^        |resp to TU   |
                            | 200-699      |      +--------+             |
                            | resp. to TU  |                             |
                            |              |                             |
                            |              V                             |
                            |            +-----------+                   |
                            |            |           |                   |
                            |            | Completed |                   |
                            |            |           |                   |
                            |            +-----------+                   |
                            |              ^   |                         |
                            |              |   | Timer K                 |
                            +--------------+   | -                       |
                                               |                         |
                                               V                         |
                         NOTE:           +-----------+                   |
                                         |           |                   |
                     transitions         | Terminated|<------------------+
                     labeled with        |           |
                     the event           +-----------+
                     over the action
                     to take
                */

                else
                {
                    #region Trying

                    if (State == SIP_TransactionState.Trying)
                    {
                        // Store response.
                        AddResponse(response);

                        // Stop timer E
                        if (m_pTimerE != null)
                        {
                            m_pTimerE.Dispose();
                            m_pTimerE = null;

                            // Log
                            if (Stack.Logger != null)
                            {
                                Stack.Logger.AddText(ID,
                                                     "Transaction [branch='" + ID + "';method='" + Method +
                                                     "';IsServer=false] timer E(Non-INVITE request retransmission) stoped.");
                            }
                        }

                        // 1xx response.
                        if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
                        {
                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Proceeding);
                        }
                            // 2xx - 6xx response.
                        else
                        {
                            // Stop timer F
                            if (m_pTimerF != null)
                            {
                                m_pTimerF.Dispose();
                                m_pTimerF = null;

                                // Log
                                if (Stack.Logger != null)
                                {
                                    Stack.Logger.AddText(ID,
                                                         "Transaction [branch='" + ID + "';method='" + Method +
                                                         "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) stoped.");
                                }
                            }

                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Completed);

                            /* RFC 3261 17.1.2.2. 
                                The client transaction enters the "Completed" state, it MUST set
                                Timer K to fire in T4 seconds for unreliable transports, and zero
                                seconds for reliable transports.
                            */
                            m_pTimerK = new TimerEx(Flow.IsReliable ? 1 : SIP_TimerConstants.T4, false);
                            m_pTimerK.Elapsed += m_pTimerK_Elapsed;
                            // Log
                            if (Stack.Logger != null)
                            {
                                Stack.Logger.AddText(ID,
                                                     "Transaction [branch='" + ID + "';method='" + Method +
                                                     "';IsServer=false] timer K(Non-INVITE 3xx - 6xx response retransmission wait) started, will triger after " +
                                                     m_pTimerK.Interval + ".");
                            }
                            m_pTimerK.Enabled = true;
                        }
                    }

                        #endregion

                        #region Proceeding

                    else if (State == SIP_TransactionState.Proceeding)
                    {
                        // Store response.
                        AddResponse(response);

                        // 1xx response.
                        if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
                        {
                            OnResponseReceived(response);
                        }                        
                            // 2xx - 6xx response.
                        else
                        {
                            // Stop timer F
                            if (m_pTimerF != null)
                            {
                                m_pTimerF.Dispose();
                                m_pTimerF = null;

                                // Log
                                if (Stack.Logger != null)
                                {
                                    Stack.Logger.AddText(ID,
                                                         "Transaction [branch='" + ID + "';method='" + Method +
                                                         "';IsServer=false] timer F(Non-INVITE trying,proceeding state timeout) stoped.");
                                }
                            }

                            OnResponseReceived(response);
                            SetState(SIP_TransactionState.Completed);

                            /* RFC 3261 17.1.2.2. 
                                The client transaction enters the "Completed" state, it MUST set
                                Timer K to fire in T4 seconds for unreliable transports, and zero
                                seconds for reliable transports.
                            */
                            m_pTimerK = new TimerEx(Flow.IsReliable ? 0 : SIP_TimerConstants.T4, false);
                            m_pTimerK.Elapsed += m_pTimerK_Elapsed;
                            // Log
                            if (Stack.Logger != null)
                            {
                                Stack.Logger.AddText(ID,
                                                     "Transaction [branch='" + ID + "';method='" + Method +
                                                     "';IsServer=false] timer K(Non-INVITE 3xx - 6xx response retransmission wait) started, will triger after " +
                                                     m_pTimerK.Interval + ".");
                            }
                            m_pTimerK.Enabled = true;
                        }
                    }

                        #endregion

                        #region Completed

                    else if (State == SIP_TransactionState.Completed)
                    {
                        // Eat retransmited response.
                    }

                        #endregion

                        #region Terminated

                    else if (State == SIP_TransactionState.Terminated)
                    {
                        // We should never reach here, but if so, do nothing.
                    }

                    #endregion
                }

                #endregion
            }
        }

        #endregion
    }
}