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
    using System.Collections.Generic;
    using System.Timers;
    using Message;
    using SDP;

    #endregion

    /// <summary>
    /// This class represent INVITE dialog. Defined in RFC 3261.
    /// </summary>
    public class SIP_Dialog_Invite : SIP_Dialog
    {
        #region Nested type: UacInvite2xxRetransmissionWaiter

        /// <summary>
        /// This class waits INVITE 2xx response retransmissions and answers with ACK request to them.
        /// For more info see RFC 3261 13.2.2.4.
        /// </summary>
        private class UacInvite2xxRetransmissionWaiter
        {
            #region Members

            private bool m_IsDisposed;
            private SIP_Dialog_Invite m_pDialog;
            private SIP_Request m_pInvite;
            private object m_pLock = "";
            private TimerEx m_pTimer;

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="dialog">Owner INVITE dialog.</param>
            /// <param name="invite">INVITE request which 2xx retransmission to wait.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>dialog</b> or <b>invite</b> is null reference.</exception>
            public UacInvite2xxRetransmissionWaiter(SIP_Dialog_Invite dialog, SIP_Request invite)
            {
                if (dialog == null)
                {
                    throw new ArgumentNullException("dialog");
                }
                if (invite == null)
                {
                    throw new ArgumentNullException("invite");
                }

                m_pDialog = dialog;
                m_pInvite = invite;

                /* RFC 3261 13.2.2.4.
                    The UAC core considers the INVITE transaction completed 64*T1 seconds
                    after the reception of the first 2xx response.
                */
                m_pTimer = new TimerEx(64*SIP_TimerConstants.T1, false);
                m_pTimer.Elapsed += m_pTimer_Elapsed;
                m_pTimer.Enabled = true;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Cleans up any resources being used.
            /// </summary>
            public void Dispose()
            {
                lock (m_pLock)
                {
                    if (m_IsDisposed)
                    {
                        return;
                    }
                    m_IsDisposed = true;

                    m_pDialog.m_pUacInvite2xxRetransmitWaits.Remove(this);

                    m_pDialog = null;
                    m_pInvite = null;
                    if (m_pTimer != null)
                    {
                        m_pTimer.Dispose();
                        m_pTimer = null;
                    }
                }
            }

            /// <summary>
            /// Checks if specified response matches this 2xx response retransmission wait entry.
            /// </summary>
            /// <param name="response">INVITE 2xx response.</param>
            /// <returns>Returns true if response matches, othwerwise false.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null reference value.</exception>
            public bool Match(SIP_Response response)
            {
                if (response == null)
                {
                    throw new ArgumentNullException("response");
                }

                if (m_pInvite.CSeq.RequestMethod == response.CSeq.RequestMethod &&
                    m_pInvite.CSeq.SequenceNumber == response.CSeq.SequenceNumber)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Processes retransmited INVITE 2xx response.
            /// </summary>
            /// <param name="response">INVITE 2xx response.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null reference.</exception>
            public void Process(SIP_Response response)
            {
                if (response == null)
                {
                    throw new ArgumentNullException("response");
                }

                lock (m_pLock)
                {
                    SIP_Request ack = CreateAck();

                    try
                    {
                        // Try existing flow.
                        m_pDialog.Flow.Send(ack);

                        // Log
                        if (m_pDialog.Stack.Logger != null)
                        {
                            byte[] ackBytes = ack.ToByteData();

                            m_pDialog.Stack.Logger.AddWrite(m_pDialog.ID,
                                                            null,
                                                            ackBytes.Length,
                                                            "Dialog [id='" + m_pDialog.ID +
                                                            "] ACK sent for 2xx response.",
                                                            m_pDialog.Flow.LocalEP,
                                                            m_pDialog.Flow.RemoteEP,
                                                            ackBytes);
                        }
                    }
                    catch
                    {
                        /* RFC 3261 13.2.2.4.
                            Once the ACK has been constructed, the procedures of [4] are used to
                            determine the destination address, port and transport.  However, the
                            request is passed to the transport layer directly for transmission,
                            rather than a client transaction.
                        */
                        try
                        {
                            m_pDialog.Stack.TransportLayer.SendRequest(ack);
                        }
                        catch (Exception x)
                        {
                            // Log
                            if (m_pDialog.Stack.Logger != null)
                            {
                                m_pDialog.Stack.Logger.AddText("Dialog [id='" + m_pDialog.ID +
                                                               "'] ACK send for 2xx response failed: " +
                                                               x.Message + ".");
                            }
                        }
                    }
                }
            }

            #endregion

            #region Event handlers

            /// <summary>
            /// Is raised when INVITE 2xx retrasmission wait time expired.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event data.</param>
            private void m_pTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                Dispose();
            }

            #endregion

            #region Utility methods

            /// <summary>
            /// Creates ACK request for pending INVITE.
            /// </summary>
            /// <returns>Returns created ACK request.</returns>
            private SIP_Request CreateAck()
            {
                /* RFC 3261 13.2.2.4.
                    The header fields of the ACK are constructed
                    in the same way as for any request sent within a dialog (see Section 12) with the 
                    exception of the CSeq and the header fields related to authentication. The sequence 
                    number of the CSeq header field MUST be the same as the INVITE being acknowledged, 
                    but the CSeq method MUST be ACK. The ACK MUST contain the same credentials as the INVITE.
                
                    ACK must have same branch.
                */

                SIP_Request ackRequest = m_pDialog.CreateRequest(SIP_Methods.ACK);
                ackRequest.Via.AddToTop(m_pInvite.Via.GetTopMostValue().ToStringValue());
                ackRequest.CSeq = new SIP_t_CSeq(m_pInvite.CSeq.SequenceNumber, SIP_Methods.ACK);
                // Authorization
                foreach (SIP_HeaderField h in m_pInvite.Authorization.HeaderFields)
                {
                    ackRequest.Authorization.Add(h.Value);
                }
                // Proxy-Authorization 
                foreach (SIP_HeaderField h in m_pInvite.ProxyAuthorization.HeaderFields)
                {
                    ackRequest.Authorization.Add(h.Value);
                }

                return ackRequest;
            }

            #endregion
        }

        #endregion

        #region Nested type: UasInvite2xxRetransmit

        /// <summary>
        /// This class is responsible for retransmitting INVITE 2xx response while ACK is received form target UAC.
        /// For more info see RFC 3261 13.3.1.4.
        /// </summary>
        private class UasInvite2xxRetransmit
        {
            #region Members

            private bool m_IsDisposed;
            private SIP_Dialog_Invite m_pDialog;
            private object m_pLock = "";
            private SIP_Response m_pResponse;
            private TimerEx m_pTimer;
            private DateTime m_StartTime = DateTime.Now;

            #endregion

            #region Constructor

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="dialog">Owner INVITE dialog.</param>
            /// <param name="response">INVITE 2xx response.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>dialog</b> or <b>response</b> is null reference.</exception>
            public UasInvite2xxRetransmit(SIP_Dialog_Invite dialog, SIP_Response response)
            {
                if (dialog == null)
                {
                    throw new ArgumentNullException("dialog");
                }
                if (response == null)
                {
                    throw new ArgumentNullException("response");
                }

                m_pDialog = dialog;
                m_pResponse = response;

                /* RFC 3261 13.3.1.4.
                    Once the response has been constructed, it is passed to the INVITE
                    server transaction.  Note, however, that the INVITE server
                    transaction will be destroyed as soon as it receives this final
                    response and passes it to the transport.  Therefore, it is necessary
                    to periodically pass the response directly to the transport until the
                    ACK arrives.  The 2xx response is passed to the transport with an
                    interval that starts at T1 seconds and doubles for each
                    retransmission until it reaches T2 seconds (T1 and T2 are defined in
                    Section 17).  Response retransmissions cease when an ACK request for
                    the response is received.  This is independent of whatever transport
                    protocols are used to send the response.
                 
                        Since 2xx is retransmitted end-to-end, there may be hops between
                        UAS and UAC that are UDP.  To ensure reliable delivery across
                        these hops, the response is retransmitted periodically even if the
                        transport at the UAS is reliable.                    
                */

                m_pTimer = new TimerEx(SIP_TimerConstants.T1, false);
                m_pTimer.Elapsed += m_pTimer_Elapsed;
                m_pTimer.Enabled = true;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Cleans up any resources being used.
            /// </summary>
            public void Dispose()
            {
                lock (m_pLock)
                {
                    if (m_IsDisposed)
                    {
                        return;
                    }
                    m_IsDisposed = true;

                    m_pDialog.m_pUasInvite2xxRetransmits.Remove(this);

                    if (m_pTimer != null)
                    {
                        m_pTimer.Dispose();
                        m_pTimer = null;
                    }
                    m_pDialog = null;
                    m_pResponse = null;
                }
            }

            /// <summary>
            /// Checks if specified ACK request matches this 2xx response retransmission.
            /// </summary>
            /// <param name="ackRequest">ACK request.</param>
            /// <returns>Returns true if ACK matches, othwerwise false.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>ackRequest</b> is null reference value.</exception>
            public bool MatchAck(SIP_Request ackRequest)
            {
                if (ackRequest == null)
                {
                    throw new ArgumentNullException("ackRequest");
                }

                if (ackRequest.CSeq.SequenceNumber == m_pResponse.CSeq.SequenceNumber)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion

            #region Event handlers

            /// <summary>
            /// This method is called when INVITE 2xx retransmit timer triggered.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event data.</param>
            private void m_pTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                /* RFC 3261 13.3.1.4.
                    Once the response has been constructed, it is passed to the INVITE
                    server transaction.  Note, however, that the INVITE server
                    transaction will be destroyed as soon as it receives this final
                    response and passes it to the transport.  Therefore, it is necessary
                    to periodically pass the response directly to the transport until the
                    ACK arrives.  The 2xx response is passed to the transport with an
                    interval that starts at T1 seconds and doubles for each
                    retransmission until it reaches T2 seconds (T1 and T2 are defined in
                    Section 17).  Response retransmissions cease when an ACK request for
                    the response is received.  This is independent of whatever transport
                    protocols are used to send the response.
                 
                        Since 2xx is retransmitted end-to-end, there may be hops between
                        UAS and UAC that are UDP.  To ensure reliable delivery across
                        these hops, the response is retransmitted periodically even if the
                        transport at the UAS is reliable.
                  
                    If the server retransmits the 2xx response for 64*T1 seconds without
                    receiving an ACK, the dialog is confirmed, but the session SHOULD be
                    terminated.  This is accomplished with a BYE, as described in Section
                    15.
                */

                lock (m_pLock)
                {
                    if (m_StartTime.AddMilliseconds(64*SIP_TimerConstants.T1) < DateTime.Now)
                    {
                        // Log
                        if (m_pDialog.Stack.Logger != null)
                        {
                            m_pDialog.Stack.Logger.AddText("Dialog [id='" + m_pDialog.ID +
                                                           "'] ACK was not received for (re-)INVITE, terminating INVITE session.");
                        }

                        m_pDialog.Terminate("Dialog Error: ACK was not received for (re-)INVITE.", true);
                        Dispose();
                    }
                    else
                    {
                        // Retransmit response.
                        try
                        {
                            m_pDialog.Flow.Send(m_pResponse);

                            // Log
                            if (m_pDialog.Stack.Logger != null)
                            {
                                m_pDialog.Stack.Logger.AddText("Dialog [id='" + m_pDialog.ID + "';statusCode=" +
                                                               m_pResponse.StatusCode +
                                                               "] UAS 2xx response retransmited");
                            }
                        }
                        catch (Exception x)
                        {
                            // Log
                            if (m_pDialog.Stack.Logger != null)
                            {
                                m_pDialog.Stack.Logger.AddText("Dialog [id='" + m_pDialog.ID +
                                                               "'] UAS 2xx response retransmission failed: " +
                                                               x.Message);
                            }
                        }

                        m_pTimer.Interval = Math.Min(m_pTimer.Interval*2, SIP_TimerConstants.T2);
                        m_pTimer.Enabled = true;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Events

        /// <summary>
        /// Is raised when re-INVITE is received.
        /// </summary>
        /// <remarks>INVITE dialog consumer always should respond to this event(accept or decline it).</remarks>
        public event EventHandler Reinvite = null;

        #endregion

        #region Members

        private SIP_Transaction m_pActiveInvite;
        private List<UacInvite2xxRetransmissionWaiter> m_pUacInvite2xxRetransmitWaits;
        private List<UasInvite2xxRetransmit> m_pUasInvite2xxRetransmits;

        #endregion

        #region Properties

        /// <summary>
        /// Gets if dialog has pending INVITE transaction.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public bool HasPendingInvite
        {
            get
            {
                if (State == SIP_DialogState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                if (m_pActiveInvite != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal SIP_Dialog_Invite()
        {
            m_pUasInvite2xxRetransmits = new List<UasInvite2xxRetransmit>();
            m_pUacInvite2xxRetransmitWaits = new List<UacInvite2xxRetransmissionWaiter>();
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
                if (State == SIP_DialogState.Disposed)
                {
                    return;
                }

                foreach (UasInvite2xxRetransmit t in m_pUasInvite2xxRetransmits.ToArray())
                {
                    t.Dispose();
                }
                m_pUasInvite2xxRetransmits = null;

                foreach (UacInvite2xxRetransmissionWaiter w in m_pUacInvite2xxRetransmitWaits.ToArray())
                {
                    w.Dispose();
                }
                m_pUacInvite2xxRetransmitWaits = null;

                m_pActiveInvite = null;

                base.Dispose();
            }
        }

        /// <summary>
        /// Starts terminating dialog.
        /// </summary>
        /// <param name="reason">Termination reason. This value may be null.</param>
        /// <param name="sendBye">If true BYE is sent to remote party.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        public override void Terminate(string reason, bool sendBye)
        {
            lock (SyncRoot)
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

                if (sendBye)
                {
                    if ((State == SIP_DialogState.Early && m_pActiveInvite is SIP_ClientTransaction) ||
                        State == SIP_DialogState.Confirmed)
                    {
                        SetState(SIP_DialogState.Terminating, true);

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
                    else
                    {
                        /* We are "early" UAS dialog, we need todo follwoing:
                            *) If we havent sent final response, send '408 Request terminated' and we are done.
                            *) We have sen't final response, we need to wait ACK to arrive or timeout.
                                If will ACK arrives or timeout, send BYE.                                
                        */

                        if (m_pActiveInvite != null && m_pActiveInvite.FinalResponse == null)
                        {
                            Stack.CreateResponse(SIP_ResponseCodes.x408_Request_Timeout,
                                                 m_pActiveInvite.Request);

                            SetState(SIP_DialogState.Terminated, true);
                        }
                        else
                        {
                            // Wait ACK to arrive or timeout. 

                            SetState(SIP_DialogState.Terminating, true);
                        }
                    }
                }
                else
                {
                    SetState(SIP_DialogState.Terminated, true);
                }
            }
        }

        /// <summary>
        /// Re-invites remote party.
        /// </summary>
        /// <param name="contact">New contact value. Value null means current contact value used.</param>
        /// <param name="sdp">SDP media offer.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when there is pending invite and this method is called or dialog is in invalid state.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>sdp</b> is null reference.</exception>
        public void ReInvite(SIP_Uri contact, SDP_Message sdp)
        {
            if (State == SIP_DialogState.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (HasPendingInvite)
            {
                throw new InvalidOperationException("There is pending INVITE.");
            }
            if (State != SIP_DialogState.Confirmed)
            {
                throw new InvalidOperationException("ReInvite is only available in Confirmed state.");
            }
            if (sdp == null)
            {
                throw new ArgumentNullException("sdp");
            }

            lock (SyncRoot)
            {
                // TODO:

                SIP_Request reinvite = CreateRequest(SIP_Methods.INVITE);
                if (contact != null)
                {
                    reinvite.Contact.RemoveAll();
                    reinvite.Contact.Add(contact.ToString());
                }
                reinvite.ContentType = "application/sdp";
                // reinvite.Data = sdp.ToStringData();

                // TODO: Create request sender
                // TODO: Try to reuse existing data flow
                //SIP_RequestSender sender = this.Stack.CreateRequestSender(reinvite);
                //sender.Start();
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Initializes dialog.
        /// </summary>
        /// <param name="stack">Owner stack.</param>
        /// <param name="transaction">Owner transaction.</param>
        /// <param name="response">SIP response what caused dialog creation.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b>,<b>transaction</b> or <b>response</b>.</exception>
        protected internal override void Init(SIP_Stack stack,
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

            base.Init(stack, transaction, response);

            if (transaction is SIP_ServerTransaction)
            {
                if (response.StatusCodeType == SIP_StatusCodeType.Success)
                {
                    SetState(SIP_DialogState.Early, false);

                    // We need to retransmit 2xx response while we get ACK or timeout. (RFC 3261 13.3.1.4.)
                    m_pUasInvite2xxRetransmits.Add(new UasInvite2xxRetransmit(this, response));
                }
                else if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
                {
                    SetState(SIP_DialogState.Early, false);

                    m_pActiveInvite = transaction;
                    m_pActiveInvite.StateChanged += delegate
                                                        {
                                                            if (m_pActiveInvite != null &&
                                                                m_pActiveInvite.State ==
                                                                SIP_TransactionState.Terminated)
                                                            {
                                                                m_pActiveInvite = null;
                                                            }
                                                        };
                    // Once we send 2xx response, we need to retransmit it while get ACK or timeout. (RFC 3261 13.3.1.4.)
                    ((SIP_ServerTransaction) m_pActiveInvite).ResponseSent +=
                        delegate(object s, SIP_ResponseSentEventArgs a)
                            {
                                if (a.Response.StatusCodeType == SIP_StatusCodeType.Success)
                                {
                                    m_pUasInvite2xxRetransmits.Add(new UasInvite2xxRetransmit(this, a.Response));
                                }
                            };
                }
                else
                {
                    throw new ArgumentException(
                        "Argument 'response' has invalid status code, 1xx - 2xx is only allowed.");
                }
            }
            else
            {
                if (response.StatusCodeType == SIP_StatusCodeType.Success)
                {
                    SetState(SIP_DialogState.Confirmed, false);

                    //  Wait for retransmited 2xx responses. (RFC 3261 13.2.2.4.)
                    m_pUacInvite2xxRetransmitWaits.Add(new UacInvite2xxRetransmissionWaiter(this,
                                                                                            transaction.
                                                                                                Request));
                }
                else if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
                {
                    SetState(SIP_DialogState.Early, false);

                    m_pActiveInvite = transaction;
                    m_pActiveInvite.StateChanged += delegate
                                                        {
                                                            if (m_pActiveInvite != null &&
                                                                m_pActiveInvite.State ==
                                                                SIP_TransactionState.Terminated)
                                                            {
                                                                m_pActiveInvite = null;
                                                            }
                                                        };
                    // Once we receive 2xx response, we need to wait for retransmitted 2xx responses. (RFC 3261 13.2.2.4)
                    ((SIP_ClientTransaction) m_pActiveInvite).ResponseReceived +=
                        delegate(object s, SIP_ResponseReceivedEventArgs a)
                            {
                                if (a.Response.StatusCodeType == SIP_StatusCodeType.Success)
                                {
                                    UacInvite2xxRetransmissionWaiter waiter =
                                        new UacInvite2xxRetransmissionWaiter(this, m_pActiveInvite.Request);
                                    m_pUacInvite2xxRetransmitWaits.Add(waiter);
                                    // Force to send initial ACK to 2xx response.
                                    waiter.Process(a.Response);

                                    SetState(SIP_DialogState.Confirmed, true);
                                }
                            };
                }
                else
                {
                    throw new ArgumentException(
                        "Argument 'response' has invalid status code, 1xx - 2xx is only allowed.");
                }
            }
        }

        /// <summary>
        /// Processes specified request through this dialog.
        /// </summary>
        /// <param name="e">Method arguments.</param>
        /// <returns>Returns true if this dialog processed specified request, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>e</b> is null reference.</exception>
        protected internal override bool ProcessRequest(SIP_RequestReceivedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (base.ProcessRequest(e))
            {
                return true;
            }

            // We must support: INVITE(re-invite),UPDATE,ACK,  [BYE will be handled by base class]

            #region INVITE

            if (e.Request.RequestLine.Method == SIP_Methods.INVITE)
            {
                /* RFC 3261 14.2.
                    A UAS that receives a second INVITE before it sends the final
                    response to a first INVITE with a lower CSeq sequence number on the
                    same dialog MUST return a 500 (Server Internal Error) response to the
                    second INVITE and MUST include a Retry-After header field with a
                    randomly chosen value of between 0 and 10 seconds.

                    A UAS that receives an INVITE on a dialog while an INVITE it had sent
                    on that dialog is in progress MUST return a 491 (Request Pending)
                    response to the received INVITE.
                */

                if (m_pActiveInvite != null && m_pActiveInvite is SIP_ServerTransaction &&
                    (m_pActiveInvite).Request.CSeq.SequenceNumber < e.Request.CSeq.SequenceNumber)
                {
                    SIP_Response response =
                        Stack.CreateResponse(
                            SIP_ResponseCodes.x500_Server_Internal_Error +
                            ": INVITE with a lower CSeq is pending(RFC 3261 14.2).",
                            e.Request);
                    response.RetryAfter = new SIP_t_RetryAfter("10");
                    e.ServerTransaction.SendResponse(response);

                    return true;
                }
                if (m_pActiveInvite != null && m_pActiveInvite is SIP_ClientTransaction)
                {
                    e.ServerTransaction.SendResponse(
                        Stack.CreateResponse(SIP_ResponseCodes.x491_Request_Pending, e.Request));

                    return true;
                }

                // Force server transaction creation and set it as active INVITE transaction.
                m_pActiveInvite = e.ServerTransaction;
                m_pActiveInvite.StateChanged += delegate
                                                    {
                                                        if (m_pActiveInvite.State ==
                                                            SIP_TransactionState.Terminated)
                                                        {
                                                            m_pActiveInvite = null;
                                                        }
                                                    };
                // Once we send 2xx response, we need to retransmit it while get ACK or timeout. (RFC 3261 13.3.1.4.)
                ((SIP_ServerTransaction) m_pActiveInvite).ResponseSent +=
                    delegate(object s, SIP_ResponseSentEventArgs a)
                        {
                            if (a.Response.StatusCodeType == SIP_StatusCodeType.Success)
                            {
                                m_pUasInvite2xxRetransmits.Add(new UasInvite2xxRetransmit(this, a.Response));
                            }
                        };

                OnReinvite(((SIP_ServerTransaction) m_pActiveInvite));

                return true;
            }

                #endregion

                #region ACK

            else if (e.Request.RequestLine.Method == SIP_Methods.ACK)
            {
                // Search corresponding INVITE 2xx retransmit entry and dispose it.
                foreach (UasInvite2xxRetransmit t in m_pUasInvite2xxRetransmits)
                {
                    if (t.MatchAck(e.Request))
                    {
                        t.Dispose();
                        if (State == SIP_DialogState.Early)
                        {
                            SetState(SIP_DialogState.Confirmed, true);

                            // TODO: If Terminating
                        }

                        return true;
                    }
                }

                return false;
            }

                #endregion

                #region UPDATE

                //else if(request.RequestLine.Method == SIP_Methods.UPDATE){
                // TODO:
                //}

                #endregion

                // RFC 5057 5.6. Refusing New Usages. Decline(603 Decline) new dialog usages.
            else if (SIP_Utils.MethodCanEstablishDialog(e.Request.RequestLine.Method))
            {
                e.ServerTransaction.SendResponse(
                    Stack.CreateResponse(
                        SIP_ResponseCodes.x603_Decline + " : New dialog usages not allowed (RFC 5057).",
                        e.Request));

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Processes specified response through this dialog.
        /// </summary>
        /// <param name="response">SIP response to process.</param>
        /// <returns>Returns true if this dialog processed specified response, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null.</exception>
        protected internal override bool ProcessResponse(SIP_Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (response.StatusCodeType == SIP_StatusCodeType.Success)
            {
                // Search pending INVITE 2xx response retransmission waite entry.
                foreach (UacInvite2xxRetransmissionWaiter w in m_pUacInvite2xxRetransmitWaits)
                {
                    if (w.Match(response))
                    {
                        w.Process(response);

                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Raises <b>Reinvite</b> event.
        /// </summary>
        /// <param name="reinvite">Re-INVITE server transaction.</param>
        private void OnReinvite(SIP_ServerTransaction reinvite)
        {
            if (Reinvite != null)
            {
                Reinvite(this, new EventArgs());
            }
        }

        #endregion
    }
}