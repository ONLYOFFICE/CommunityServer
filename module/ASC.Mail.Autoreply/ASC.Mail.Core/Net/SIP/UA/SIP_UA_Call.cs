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


namespace ASC.Mail.Net.SIP.UA
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Message;
    using SDP;
    using Stack;

    #endregion

    /// <summary>
    /// This class represent SIP UA call.
    /// </summary>
    public class SIP_UA_Call : IDisposable
    {
        #region Events

        /// <summary>
        /// Is raised when call state has changed.
        /// </summary>
        public event EventHandler StateChanged = null;

        #endregion

        #region Members

        private readonly List<SIP_Dialog> m_pEarlyDialogs;
        private readonly SIP_ServerTransaction m_pInitialInviteTransaction;
        private readonly SIP_Request m_pInvite;
        private readonly AbsoluteUri m_pLocalUri;
        private readonly AbsoluteUri m_pRemoteUri;
        private readonly SIP_UA m_pUA;
        private bool m_IsRedirected;
        private SIP_Dialog m_pDialog;
        private SIP_RequestSender m_pInitialInviteSender;
        private object m_pLock = "";
        private SDP_Message m_pRemoteSDP;
        private DateTime m_StartTime;
        private SIP_UA_CallState m_State = SIP_UA_CallState.WaitingForStart;

        #endregion

        #region Constructor

        /// <summary>
        /// Default outgoing call constructor.
        /// </summary>
        /// <param name="ua">Owner UA.</param>
        /// <param name="invite">INVITE request.</param>
        /// <exception cref="ArgumentNullException">Is riased when <b>ua</b> or <b>invite</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the argumnets has invalid value.</exception>
        internal SIP_UA_Call(SIP_UA ua, SIP_Request invite)
        {
            if (ua == null)
            {
                throw new ArgumentNullException("ua");
            }
            if (invite == null)
            {
                throw new ArgumentNullException("invite");
            }
            if (invite.RequestLine.Method != SIP_Methods.INVITE)
            {
                throw new ArgumentException("Argument 'invite' is not INVITE request.");
            }

            m_pUA = ua;
            m_pInvite = invite;
            m_pLocalUri = invite.From.Address.Uri;
            m_pRemoteUri = invite.To.Address.Uri;

            m_State = SIP_UA_CallState.WaitingForStart;

            m_pEarlyDialogs = new List<SIP_Dialog>();
        }

        /// <summary>
        /// Default incoming call constructor.
        /// </summary>
        /// <param name="ua">Owner UA.</param>
        /// <param name="invite">INVITE server transaction.</param>
        /// <exception cref="ArgumentNullException">Is riased when <b>ua</b> or <b>invite</b> is null reference.</exception>
        internal SIP_UA_Call(SIP_UA ua, SIP_ServerTransaction invite)
        {
            if (ua == null)
            {
                throw new ArgumentNullException("ua");
            }
            if (invite == null)
            {
                throw new ArgumentNullException("invite");
            }

            m_pUA = ua;
            m_pInitialInviteTransaction = invite;
            m_pLocalUri = invite.Request.To.Address.Uri;
            m_pRemoteUri = invite.Request.From.Address.Uri;
            m_pInitialInviteTransaction.Canceled += delegate
                                                        {
                                                            // If transaction canceled, terminate call.
                                                            SetState(SIP_UA_CallState.Terminated);
                                                        };

            m_State = SIP_UA_CallState.WaitingToAccept;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets call duration in seconds.
        /// </summary>
        public int Duration
        {
            get
            {
                return ((DateTime.Now - m_StartTime)).Seconds;

                // TODO: if terminated, we need static value here.
            }
        }

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_State == SIP_UA_CallState.Disposed; }
        }

        /// <summary>
        /// Gets if call is on hold.
        /// </summary>
        public bool IsOnhold
        {
            // TODO:

            get { return false; }
        }

        /// <summary>
        /// Gets if call has been redirected by remote party.
        /// </summary>
        public bool IsRedirected
        {
            get { return m_IsRedirected; }
        }

        /// <summary>
        /// Gets call local party URI.
        /// </summary>
        public AbsoluteUri LocalUri
        {
            get { return m_pLocalUri; }
        }

        /// <summary>
        /// Gets remote SDP.
        /// </summary>
        public SDP_Message RemoteSDP
        {
            get { return m_pRemoteSDP; }
        }

        /// <summary>
        /// Gets call remote party URI.
        /// </summary>
        public AbsoluteUri RemoteUri
        {
            get { return m_pRemoteUri; }
        }

        /// <summary>
        /// Gets call start time.
        /// </summary>
        public DateTime StartTime
        {
            get { return m_StartTime; }
        }

        /// <summary>
        /// Gets current call state.
        /// </summary>
        public SIP_UA_CallState State
        {
            get { return m_State; }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resource being used.
        /// </summary>
        public void Dispose()
        {
            lock (m_pLock)
            {
                if (m_State == SIP_UA_CallState.Disposed)
                {
                    return;
                }
                SetState(SIP_UA_CallState.Disposed);

                // TODO: Clean up

                StateChanged = null;
            }
        }

        /// <summary>
        /// Starts terminating call. To get when call actually terminates, monitor <b>StateChanged</b> event.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        public void Terminate()
        {
            lock (m_pLock)
            {
                if (m_State == SIP_UA_CallState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                if (m_State == SIP_UA_CallState.Terminating || m_State == SIP_UA_CallState.Terminated)
                {
                    return;
                }
                else if (m_State == SIP_UA_CallState.WaitingForStart)
                {
                    SetState(SIP_UA_CallState.Terminated);
                }
                else if (m_State == SIP_UA_CallState.WaitingToAccept)
                {
                    m_pInitialInviteTransaction.SendResponse(
                        m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x487_Request_Terminated,
                                                   m_pInitialInviteTransaction.Request));

                    SetState(SIP_UA_CallState.Terminated);
                }
                else if (m_State == SIP_UA_CallState.Active)
                {
                    m_pDialog.Terminate();

                    SetState(SIP_UA_CallState.Terminated);
                }
                else if (m_pInitialInviteSender != null)
                {
                    /* RFC 3261 15.
                        If we are caller and call is not active yet, we must do following actions:
                            *) Send CANCEL, set call Terminating flag.
                            *) If we get non 2xx final response, we are done. (Normally cancel causes '408 Request terminated')
                            *) If we get 2xx response (2xx sent by remote party before our CANCEL reached), we must send BYE to active dialog.
                    */

                    SetState(SIP_UA_CallState.Terminating);

                    m_pInitialInviteSender.Cancel();
                }
            }
        }

        /// <summary>
        /// Starts calling.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when call is not in valid state.</exception>
        public void Start()
        {
            lock (m_pLock)
            {
                if (m_State == SIP_UA_CallState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (m_State != SIP_UA_CallState.WaitingForStart)
                {
                    throw new InvalidOperationException(
                        "Start method can be called only in 'SIP_UA_CallState.WaitingForStart' state.");
                }

                SetState(SIP_UA_CallState.Calling);

                m_pInitialInviteSender = m_pUA.Stack.CreateRequestSender(m_pInvite);
                m_pInitialInviteSender.ResponseReceived += m_pInitialInviteSender_ResponseReceived;
                m_pInitialInviteSender.Start();
            }
        }

        /// <summary>
        /// Accepts call.
        /// </summary>
        public void Accept()
        {
            if (m_State == SIP_UA_CallState.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (m_State != SIP_UA_CallState.WaitingToAccept)
            {
                throw new InvalidOperationException(
                    "Accept method can be called only in 'SIP_UA_CallState.WaitingToAccept' state.");
            }

            // TODO: We must add Contact header and SDP to response.

            SIP_Response response = m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x200_Ok,
                                                               m_pInitialInviteTransaction.Request,
                                                               m_pInitialInviteTransaction.Flow);
            response.ContentType = "application/sdp";
            response.Data =
                Encoding.Default.GetBytes("v=0\r\n" + "o=LSSIP 333 242452 IN IP4 192.168.1.70\r\n" +
                                          "s=SIP Call\r\n" + "c=IN IP4 192.168.1.70\r\n" + "t=0 0\r\n" +
                                          "m=audio 11000 RTP/AVP 0\r\n" + "a=rtpmap:0 PCMU/8000\r\n" +
                                          "a=sendrecv\r\n");
            m_pInitialInviteTransaction.SendResponse(response);

            SetState(SIP_UA_CallState.Active);

            m_pDialog = m_pUA.Stack.TransactionLayer.GetOrCreateDialog(m_pInitialInviteTransaction, response);
            m_pDialog.StateChanged += m_pDialog_StateChanged;
        }

        /// <summary>
        /// Rejects incoming call.
        /// </summary>
        /// <param name="statusCode_reason">Status-code reasonText.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when call is not in valid state and this method is called.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>statusCode_reason</b> is null.</exception>
        public void Reject(string statusCode_reason)
        {
            lock (m_pLock)
            {
                if (State == SIP_UA_CallState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (State != SIP_UA_CallState.WaitingToAccept)
                {
                    throw new InvalidOperationException("Call is not in valid state.");
                }
                if (statusCode_reason == null)
                {
                    throw new ArgumentNullException("statusCode_reason");
                }

                m_pInitialInviteTransaction.SendResponse(m_pUA.Stack.CreateResponse(statusCode_reason,
                                                                                    m_pInitialInviteTransaction
                                                                                        .Request));

                SetState(SIP_UA_CallState.Terminated);
            }
        }

        /// <summary>
        /// Redirects incoming call to speified contact(s).
        /// </summary>
        /// <param name="contacts">Redirection targets.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when call is not in valid state and this method is called.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>contacts</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Redirect(SIP_t_ContactParam[] contacts)
        {
            lock (m_pLock)
            {
                if (State == SIP_UA_CallState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (State != SIP_UA_CallState.WaitingToAccept)
                {
                    throw new InvalidOperationException("Call is not in valid state.");
                }
                if (contacts == null)
                {
                    throw new ArgumentNullException("contacts");
                }
                if (contacts.Length == 0)
                {
                    throw new ArgumentException("Arguments 'contacts' must contain at least 1 value.");
                }

                // TODO:
                //m_pUA.Stack.CreateResponse(SIP_ResponseCodes.,m_pInitialInviteTransaction);

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Is called when SIP dialog state has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pDialog_StateChanged(object sender, EventArgs e)
        {
            if (State == SIP_UA_CallState.Terminated)
            {
                return;
            }

            if (m_pDialog.State == SIP_DialogState.Terminated)
            {
                SetState(SIP_UA_CallState.Terminated);

                m_pDialog.Dispose();
            }
        }

        /// <summary>
        /// This method is called when initial INVITE sender got response.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pInitialInviteSender_ResponseReceived(object sender, SIP_ResponseReceivedEventArgs e)
        {
            lock (m_pLock)
            {
                // If remote party provided SDP, parse it.               
                if (e.Response.ContentType != null &&
                    e.Response.ContentType.ToLower().IndexOf("application/sdp") > -1)
                {
                    m_pRemoteSDP = SDP_Message.Parse(Encoding.UTF8.GetString(e.Response.Data));

                    // TODO: If parsing failed, end call.
                }

                if (e.Response.StatusCodeType == SIP_StatusCodeType.Provisional)
                {
                    if (e.Response.StatusCode == 180)
                    {
                        SetState(SIP_UA_CallState.Ringing);
                    }
                    else if (e.Response.StatusCode == 182)
                    {
                        SetState(SIP_UA_CallState.Queued);
                    }
                    // We don't care other status responses.

                    /* RFC 3261 13.2.2.1.
                        Zero, one or multiple provisional responses may arrive before one or
                        more final responses are received.  Provisional responses for an
                        INVITE request can create "early dialogs".  If a provisional response
                        has a tag in the To field, and if the dialog ID of the response does
                        not match an existing dialog, one is constructed using the procedures
                        defined in Section 12.1.2.
                    */
                    if (e.Response.StatusCode > 100 && e.Response.To.Tag != null)
                    {
                        m_pEarlyDialogs.Add(m_pUA.Stack.TransactionLayer.GetOrCreateDialog(
                                                e.ClientTransaction, e.Response));
                    }
                }
                else if (e.Response.StatusCodeType == SIP_StatusCodeType.Success)
                {
                    m_StartTime = DateTime.Now;
                    SetState(SIP_UA_CallState.Active);

                    m_pDialog = m_pUA.Stack.TransactionLayer.GetOrCreateDialog(e.ClientTransaction, e.Response);
                    m_pDialog.StateChanged += m_pDialog_StateChanged;

                    /* Exit all all other dialogs created by this call (due to forking).
                       That is not defined in RFC but, since UAC can send BYE to early and confirmed dialogs, 
                       because of this all 100% valid.
                    */
                    foreach (SIP_Dialog dialog in m_pEarlyDialogs.ToArray())
                    {
                        if (!m_pDialog.Equals(dialog))
                        {
                            dialog.Terminate("Another forking leg accepted.", true);
                        }
                    }
                }
                else
                {
                    /* RFC 3261 13.2.2.3.
                        All early dialogs are considered terminated upon reception of the non-2xx final response.
                    */
                    foreach (SIP_Dialog dialog in m_pEarlyDialogs.ToArray())
                    {
                        dialog.Terminate(
                            "All early dialogs are considered terminated upon reception of the non-2xx final response. (RFC 3261 13.2.2.3)",
                            false);
                    }
                    m_pEarlyDialogs.Clear();

                    Error();

                    SetState(SIP_UA_CallState.Terminated);
                }
            }
        }

        /// <summary>
        /// Changes call state.
        /// </summary>
        /// <param name="state">New call state.</param>
        private void SetState(SIP_UA_CallState state)
        {
            m_State = state;

            OnStateChanged(state);
        }

        /// <summary>
        /// Raises <b>StateChanged</b> event.
        /// </summary>
        /// <param name="state">New call state.</param>
        private void OnStateChanged(SIP_UA_CallState state)
        {
            if (StateChanged != null)
            {
                StateChanged(this, new EventArgs());
            }
        }

        // public event EventHandler Error

        private void Error() {}

        #endregion
    }
}