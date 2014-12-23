/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.SIP.UA
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Stack;

    #endregion

    /// <summary>
    /// This class implements SIP UA. Defined in RFC 3261 8.1.
    /// </summary>
    public class SIP_UA : IDisposable
    {
        #region Events

        /// <summary>
        /// Is raised when new incoming call.
        /// </summary>
        public event EventHandler<SIP_UA_Call_EventArgs> IncomingCall = null;

        /// <summary>
        /// Is raised when user agent get new SIP request.
        /// </summary>
        public event EventHandler<SIP_RequestReceivedEventArgs> RequestReceived = null;

        #endregion

        #region Members

        private readonly List<SIP_UA_Call> m_pCalls;
        private readonly object m_pLock = new object();
        private bool m_IsDisposed;
        private SIP_Stack m_pStack;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_UA()
        {
            m_pStack = new SIP_Stack();
            m_pStack.RequestReceived += m_pStack_RequestReceived;

            m_pCalls = new List<SIP_UA_Call>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets active calls.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
        public SIP_UA_Call[] Calls
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pCalls.ToArray();
            }
        }

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_IsDisposed; }
        }

        /// <summary>
        /// Gets SIP stack.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
        public SIP_Stack Stack
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pStack;
            }
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

                // Hang up all calls.
                foreach (SIP_UA_Call call in m_pCalls.ToArray())
                {
                    call.Terminate();
                }

                // Wait till all registrations and calls disposed or wait timeout reached.
                DateTime start = DateTime.Now;
                while (m_pCalls.Count > 0)
                {
                    Thread.Sleep(500);

                    // Timeout, just kill all UA.
                    if (((DateTime.Now - start)).Seconds > 15)
                    {
                        break;
                    }
                }

                m_IsDisposed = true;

                RequestReceived = null;
                IncomingCall = null;

                m_pStack.Dispose();
                m_pStack = null;
            }
        }

        /// <summary>
        /// Creates call to <b>invite</b> specified recipient.
        /// </summary>
        /// <param name="invite">INVITE request.</param>
        /// <returns>Returns created call.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>invite</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the argumnets has invalid value.</exception>
        public SIP_UA_Call CreateCall(SIP_Request invite)
        {
            if (invite == null)
            {
                throw new ArgumentNullException("invite");
            }
            if (invite.RequestLine.Method != SIP_Methods.INVITE)
            {
                throw new ArgumentException("Argument 'invite' is not INVITE request.");
            }

            lock (m_pLock)
            {
                SIP_UA_Call call = new SIP_UA_Call(this, invite);
                call.StateChanged += Call_StateChanged;
                m_pCalls.Add(call);

                return call;
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// This method is called when SIP stack received new message.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pStack_RequestReceived(object sender, SIP_RequestReceivedEventArgs e)
        {
            // TODO: Performance: rise events on thread pool or see if this method called on pool aready, then we may not keep lock for events ?

            if (e.Request.RequestLine.Method == SIP_Methods.CANCEL)
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
                    m_pStack.TransactionLayer.MatchCancelToTransaction(e.Request);
                if (trToCancel != null)
                {
                    trToCancel.Cancel();
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok,
                                                                             e.Request));
                }
                else
                {
                    e.ServerTransaction.SendResponse(
                        m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist,
                                                e.Request));
                }
            }
            else if (e.Request.RequestLine.Method == SIP_Methods.BYE)
            {
                /* RFC 3261 15.1.2.
                    If the BYE does not match an existing dialog, the UAS core SHOULD generate a 481
                    (Call/Transaction Does Not Exist) response and pass that to the server transaction.
                */
                // TODO:

                SIP_Dialog dialog = m_pStack.TransactionLayer.MatchDialog(e.Request);
                if (dialog != null)
                {
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok,
                                                                             e.Request));
                    dialog.Terminate();
                }
                else
                {
                    e.ServerTransaction.SendResponse(
                        m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist,
                                                e.Request));
                }
            }
            else if (e.Request.RequestLine.Method == SIP_Methods.INVITE)
            {
                SIP_ServerTransaction transaction = e.ServerTransaction;
                transaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x180_Ringing, e.Request));

                // TODO: See if we support SDP media.

                // Create call.
                SIP_UA_Call call = new SIP_UA_Call(this, transaction);
                call.StateChanged += Call_StateChanged;
                m_pCalls.Add(call);

                OnIncomingCall(call);
            }
            else
            {
                OnRequestReceived(e);
            }
        }

        /// <summary>
        /// Thsi method is called when call state has chnaged.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void Call_StateChanged(object sender, EventArgs e)
        {
            SIP_UA_Call call = (SIP_UA_Call) sender;
            if (call.State == SIP_UA_CallState.Terminated)
            {
                m_pCalls.Remove(call);
            }
        }

        /// <summary>
        /// Raises event <b>IncomingCall</b>.
        /// </summary>
        /// <param name="call">Incoming call.</param>
        private void OnIncomingCall(SIP_UA_Call call)
        {
            if (IncomingCall != null)
            {
                IncomingCall(this, new SIP_UA_Call_EventArgs(call));
            }
        }

        #endregion

        /// <summary>
        /// Raises <b>RequestReceived</b> event.
        /// </summary>
        /// <param name="request">SIP request.</param>
        protected void OnRequestReceived(SIP_RequestReceivedEventArgs request)
        {
            if (RequestReceived != null)
            {
                RequestReceived(this, request);
            }
        }
    }
}