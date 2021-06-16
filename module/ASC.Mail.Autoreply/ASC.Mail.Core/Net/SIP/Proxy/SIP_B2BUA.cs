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


namespace ASC.Mail.Net.SIP.Proxy
{
    #region usings

    using System;
    using System.Collections.Generic;
    using AUTH;
    using Message;
    using Stack;

    #endregion

    /// <summary>
    /// This class implements SIP b2bua(back to back user agent). Defined in RFC 3261.
    /// </summary>
    public class SIP_B2BUA : IDisposable
    {
        #region Events

        /// <summary>
        /// Is called when new call is created.
        /// </summary>
        public event EventHandler CallCreated = null;

        /// <summary>
        /// Is called when call has terminated.
        /// </summary>
        public event EventHandler CallTerminated = null;

        #endregion

        #region Members

        private readonly List<SIP_B2BUA_Call> m_pCalls;
        private readonly SIP_ProxyCore m_pProxy;
        private bool m_IsDisposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets B2BUA owner SIP stack.
        /// </summary>
        public SIP_Stack Stack
        {
            get { return m_pProxy.Stack; }
        }

        /// <summary>
        /// Gets active calls.
        /// </summary>
        public SIP_B2BUA_Call[] Calls
        {
            get { return m_pCalls.ToArray(); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="owner">Onwer SIP proxy.</param>
        internal SIP_B2BUA(SIP_ProxyCore owner)
        {
            m_pProxy = owner;
            m_pCalls = new List<SIP_B2BUA_Call>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            m_IsDisposed = true;

            // Terminate all calls.
            foreach (SIP_B2BUA_Call call in m_pCalls)
            {
                call.Terminate();
            }
        }

        /// <summary>
        /// Gets call by call ID.
        /// </summary>
        /// <param name="callID">Call ID.</param>
        /// <returns>Returns call with specified ID or null if no call with specified ID.</returns>
        public SIP_B2BUA_Call GetCallByID(string callID)
        {
            foreach (SIP_B2BUA_Call call in m_pCalls.ToArray())
            {
                if (call.CallID == callID)
                {
                    return call;
                }
            }

            return null;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// This method is called when new request is received.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        internal void OnRequestReceived(SIP_RequestReceivedEventArgs e)
        {
            SIP_Request request = e.Request;

            if (request.RequestLine.Method == SIP_Methods.CANCEL)
            {
                /* RFC 3261 9.2.
                    If the UAS did not find a matching transaction for the CANCEL
                    according to the procedure above, it SHOULD respond to the CANCEL
                    with a 481 (Call Leg/Transaction Does Not Exist).
                  
                    Regardless of the method of the original request, as long as the
                    CANCEL matched an existing transaction, the UAS answers the CANCEL
                    request itself with a 200 (OK) response.
                */

                SIP_ServerTransaction trToCancel =
                    m_pProxy.Stack.TransactionLayer.MatchCancelToTransaction(e.Request);
                if (trToCancel != null)
                {
                    trToCancel.Cancel();
                    //e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x200_Ok));
                }
                else
                {
                    //e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist));
                }
            }
                // We never should ge BYE here, because transport layer must match it to dialog.
            else if (request.RequestLine.Method == SIP_Methods.BYE)
            {
                /* RFC 3261 15.1.2.
                    If the BYE does not match an existing dialog, the UAS core SHOULD generate a 481
                    (Call/Transaction Does Not Exist) response and pass that to the server transaction.
                */
                //e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist));
            }
                // We never should ge ACK here, because transport layer must match it to dialog.
            else if (request.RequestLine.Method == SIP_Methods.ACK)
            {
                // ACK is response less request, so we may not return error to it.
            }
                // B2BUA must respond to OPTIONS request, not to forward it.
            else if (request.RequestLine.Method == SIP_Methods.OPTIONS)
            {
                /*          
                SIP_Response response = e.Request.CreateResponse(SIP_ResponseCodes.x200_Ok);                
                // Add Allow to non ACK response.
                if(e.Request.RequestLine.Method != SIP_Methods.ACK){
                    response.Allow.Add("INVITE,ACK,OPTIONS,CANCEL,BYE,PRACK,MESSAGE,UPDATE");
                }
                // Add Supported to 2xx non ACK response.
                if(response.StatusCodeType == SIP_StatusCodeType.Success && e.Request.RequestLine.Method != SIP_Methods.ACK){
                    response.Supported.Add("100rel,timer");
                }
                e.ServerTransaction.SendResponse(response);*/
            }
                // We never should get PRACK here, because transport layer must match it to dialog.
            else if (request.RequestLine.Method == SIP_Methods.PRACK)
            {
                //e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist));
            }
                // We never should get UPDATE here, because transport layer must match it to dialog.
            else if (request.RequestLine.Method == SIP_Methods.UPDATE)
            {
                //e.ServerTransaction.SendResponse(request.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist));
            }
            else
            {
                /* draft-marjou-sipping-b2bua-00 4.1.3.
                    When the UAS of the B2BUA receives an upstream SIP request, its
                    associated UAC generates a new downstream SIP request with its new
                    Via, Max-Forwards, Call-Id, CSeq, and Contact header fields. Route
                    header fields of the upstream request are copied in the downstream
                    request, except the first Route header if it is under the
                    responsibility of the B2BUA.  Record-Route header fields of the
                    upstream request are not copied in the new downstream request, as
                    Record-Route is only meaningful for the upstream dialog.  The UAC
                    SHOULD copy other header fields and body from the upstream request
                    into this downstream request before sending it.
                */

                SIP_Request b2buaRequest = e.Request.Copy();
                b2buaRequest.Via.RemoveAll();
                b2buaRequest.MaxForwards = 70;
                b2buaRequest.CallID = SIP_t_CallID.CreateCallID().CallID;
                b2buaRequest.CSeq.SequenceNumber = 1;
                b2buaRequest.Contact.RemoveAll();
                // b2buaRequest.Contact.Add(m_pProxy.CreateContact(b2buaRequest.To.Address).ToStringValue());
                if (b2buaRequest.Route.Count > 0 &&
                    m_pProxy.IsLocalRoute(
                        SIP_Uri.Parse(b2buaRequest.Route.GetTopMostValue().Address.Uri.ToString())))
                {
                    b2buaRequest.Route.RemoveTopMostValue();
                }
                b2buaRequest.RecordRoute.RemoveAll();

                // Remove our Authorization header if it's there.
                foreach (
                    SIP_SingleValueHF<SIP_t_Credentials> header in
                        b2buaRequest.ProxyAuthorization.HeaderFields)
                {
                    try
                    {
                        Auth_HttpDigest digest = new Auth_HttpDigest(header.ValueX.AuthData,
                                                                     b2buaRequest.RequestLine.Method);
                        if (m_pProxy.Stack.Realm == digest.Realm)
                        {
                            b2buaRequest.ProxyAuthorization.Remove(header);
                        }
                    }
                    catch
                    {
                        // We don't care errors here. This can happen if remote server xxx auth method here and
                        // we don't know how to parse it, so we leave it as is.
                    }
                }

                //--- Add/replace default fields. ------------------------------------------
                b2buaRequest.Allow.RemoveAll();
                b2buaRequest.Supported.RemoveAll();
                // Accept to non ACK,BYE request.
                if (request.RequestLine.Method != SIP_Methods.ACK &&
                    request.RequestLine.Method != SIP_Methods.BYE)
                {
                    b2buaRequest.Allow.Add("INVITE,ACK,OPTIONS,CANCEL,BYE,PRACK");
                }
                // Supported to non ACK request. 
                if (request.RequestLine.Method != SIP_Methods.ACK)
                {
                    b2buaRequest.Supported.Add("100rel,timer");
                }
                // Remove Require: header.
                b2buaRequest.Require.RemoveAll();

                // RFC 4028 7.4. For re-INVITE and UPDATE we need to add Session-Expires and Min-SE: headers.
                if (request.RequestLine.Method == SIP_Methods.INVITE ||
                    request.RequestLine.Method == SIP_Methods.UPDATE)
                {
                    b2buaRequest.SessionExpires = new SIP_t_SessionExpires(m_pProxy.Stack.SessionExpries,
                                                                           "uac");
                    b2buaRequest.MinSE = new SIP_t_MinSE(m_pProxy.Stack.MinimumSessionExpries);
                }

                // Forward request.
                m_pProxy.ForwardRequest(true, e, b2buaRequest, false);
            }
        }

        /// <summary>
        /// This method is called when new response is received.
        /// </summary>
        /// <param name="e">Response event arguments.</param>
        internal void OnResponseReceived(SIP_ResponseReceivedEventArgs e)
        {
            // If we get response here, that means we have stray response, just do nothing.
            // All reponses must match to transactions, so we never should reach here.
        }

        /// <summary>
        /// Adds specified call to calls list.
        /// </summary>
        /// <param name="caller">Caller side dialog.</param>
        /// <param name="calee">Calee side dialog.</param>
        internal void AddCall(SIP_Dialog caller, SIP_Dialog calee)
        {
            lock (m_pCalls)
            {
                SIP_B2BUA_Call call = new SIP_B2BUA_Call(this, caller, calee);
                m_pCalls.Add(call);

                OnCallCreated(call);
            }
        }

        /// <summary>
        /// Removes specified call from calls list.
        /// </summary>
        /// <param name="call">Call to remove.</param>
        internal void RemoveCall(SIP_B2BUA_Call call)
        {
            m_pCalls.Remove(call);

            OnCallTerminated(call);
        }

        #endregion

        /// <summary>
        /// Raises CallCreated event.
        /// </summary>
        /// <param name="call">Call created.</param>
        protected void OnCallCreated(SIP_B2BUA_Call call)
        {
            if (CallCreated != null)
            {
                CallCreated(call, new EventArgs());
            }
        }

        /// <summary>
        /// Raises CallTerminated event.
        /// </summary>
        /// <param name="call">Call terminated.</param>
        protected internal void OnCallTerminated(SIP_B2BUA_Call call)
        {
            if (CallTerminated != null)
            {
                CallTerminated(call, new EventArgs());
            }
        }
    }
}