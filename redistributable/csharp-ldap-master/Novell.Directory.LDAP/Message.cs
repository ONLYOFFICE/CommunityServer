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
// Novell.Directory.Ldap.Message.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary> Encapsulates an Ldap message, its state, and its replies.</summary>
    internal class Message
    {
        private void InitBlock()
        {
            replies = new MessageVector(5, 5);
        }

        /// <summary>
        ///     Get number of messages queued.
        ///     Don't count the last message containing result code.
        /// </summary>
        internal virtual int Count
        {
            get
            {
                var size = replies.Count;
                if (complete)
                {
                    return size > 0 ? size - 1 : size;
                }
                return size;
            }
        }

        /// <summary> sets the agent for this message</summary>
        internal virtual MessageAgent Agent
        {
            set { agent = value; }
        }

        /// <summary>
        ///     Returns true if replies are queued
        /// </summary>
        /// <returns>
        ///     false if no replies are queued, otherwise true
        /// </returns>
        internal virtual bool hasReplies()
        {
            if (replies == null)
            {
                // abandoned request
                return false;
            }
            return replies.Count > 0;
        }

        internal virtual int MessageType
        {
            get
            {
                if (msg == null)
                {
                    return -1;
                }
                return msg.Type;
            }
        }

        internal virtual int MessageID
        {
            get { return msgId; }
        }

        /// <summary>
        ///     gets the operation complete status for this message
        /// </summary>
        /// <returns>
        ///     the true if the operation is complete, i.e.
        ///     the LdapResult has been received.
        /// </returns>
        internal virtual bool Complete
        {
            get { return complete; }
        }

        /// <summary>
        ///     Gets the next reply from the reply queue or waits until one is there
        /// </summary>
        /// <returns>
        ///     the next reply message on the reply queue or null
        /// </returns>
        internal virtual object waitForReply()
        {
            if (replies == null)
            {
                return null;
            }
            // sync on message so don't confuse with timer thread
            lock (replies)
            {
                object msg = null;
                while (waitForReply_Renamed_Field)
                {
                    if (replies.Count == 0)
                    {
                        Monitor.Wait(replies);
                        if (waitForReply_Renamed_Field)
                        {
                            continue;
                        }
                        break;
                    }
                    object temp_object;
                    temp_object = replies[0];
                    replies.RemoveAt(0);
                    msg = temp_object; // Atomic get and remove
                    if ((complete || !acceptReplies) && replies.Count == 0)
                    {
                        // Remove msg from connection queue when last reply read
                        conn.removeMessage(this);
                    }
                    return msg;
                }
                return null;
            }
        }


        /// <summary>
        ///     Gets the next reply from the reply queue if one exists
        /// </summary>
        /// <returns>
        ///     the next reply message on the reply queue or null if none
        /// </returns>
        internal virtual object Reply
        {
            get
            {
                object msg;
                if (replies == null)
                {
                    return null;
                }
                lock (replies)
                {
                    // Test and remove must be atomic
                    if (replies.Count == 0)
                    {
                        return null; // No data
                    }
                    object temp_object;
                    temp_object = replies[0];
                    replies.RemoveAt(0);
                    msg = temp_object; // Atomic get and remove
                }
                if (conn != null && (complete || !acceptReplies) && replies.Count == 0)
                {
                    // Remove msg from connection queue when last reply read
                    conn.removeMessage(this);
                }
                return msg;
            }
        }

        /// <summary>
        ///     Returns true if replies are accepted for this request.
        /// </summary>
        /// <returns>
        ///     false if replies are no longer accepted for this request
        /// </returns>
        internal virtual bool acceptsReplies()
        {
            return acceptReplies;
        }

        /// <summary>
        ///     gets the LdapMessage request associated with this message
        /// </summary>
        /// <returns>
        ///     the LdapMessage request associated with this message
        /// </returns>
        internal virtual LdapMessage Request
        {
            /*package*/
            get { return msg; }
        }


        internal virtual bool BindRequest
        {
            get { return bindprops != null; }
        }


        /// <summary>
        ///     gets the MessageAgent associated with this message
        /// </summary>
        /// <returns>
        ///     the MessageAgent associated with this message
        /// </returns>
        internal virtual MessageAgent MessageAgent
        {
            get { return agent; }
        }

        private readonly string _stackTraceCreation;
        private string _stackTraceCleanup;

        private LdapMessage msg; // msg request sent to server
        private Connection conn; // Connection object where msg sent
        private MessageAgent agent; // MessageAgent handling this request
        private LdapMessageQueue queue; // Application message queue
        private int mslimit; // client time limit in milliseconds
        private SupportClass.ThreadClass timer; // Timeout thread
        // Note: MessageVector is synchronized
        private MessageVector replies; // place to store replies
        private readonly int msgId; // message ID of this request
        private bool acceptReplies = true; // false if no longer accepting replies
        private bool waitForReply_Renamed_Field = true; // true if wait for reply
        private bool complete; // true LdapResult received
        private BindProperties bindprops; // Bind properties if a bind request

        internal Message(LdapMessage msg, int mslimit, Connection conn, MessageAgent agent, LdapMessageQueue queue,
            BindProperties bindprops)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            _stackTraceCreation = Environment.StackTrace;
            InitBlock();
            this.msg = msg;
            this.conn = conn;
            this.agent = agent;
            this.queue = queue;
            this.mslimit = mslimit;
            msgId = msg.MessageID;
            this.bindprops = bindprops;
        }

        internal void sendMessage()
        {
            conn.writeMessage(this);
            // Start the timer thread
            if (mslimit != 0)
            {
                // Don't start the timer thread for abandon or Unbind
                switch (msg.Type)
                {
                    case LdapMessage.ABANDON_REQUEST:
                    case LdapMessage.UNBIND_REQUEST:
                        mslimit = 0;
                        break;

                    default:
                        timer = new Timeout(this, mslimit, this);
                        timer.IsBackground = true; // If this is the last thread running, allow exit.
                        timer.Start();
                        break;
                }
            }
        }

        internal virtual void Abandon(LdapConstraints cons, InterThreadException informUserEx)
        {
            if (!waitForReply_Renamed_Field)
            {
                return;
            }
            acceptReplies = false; // don't listen to anyone
            waitForReply_Renamed_Field = false; // don't let sleeping threads lie
            if (!complete)
            {
                try
                {
                    // If a bind, release bind semaphore & wake up waiting threads
                    // Must do before writing abandon message, otherwise deadlock
                    if (bindprops != null)
                    {
                        int id;
                        if (conn.BindSemIdClear)
                        {
                            // Semaphore id for normal operations
                            id = msgId;
                        }
                        else
                        {
                            // Semaphore id for sasl bind
                            id = conn.BindSemId;
                            conn.clearBindSemId();
                        }
                        conn.freeWriteSemaphore(id);
                    }

                    // Create the abandon message, but don't track it.
                    LdapControl[] cont = null;
                    if (cons != null)
                    {
                        cont = cons.getControls();
                    }
                    LdapMessage msg = new LdapAbandonRequest(msgId, cont);
                    // Send abandon message to server
                    conn.writeMessage(msg);
                }
                catch (LdapException ex)
                {
                    Logger.Log.LogWarning("Exception swallowed", ex);
                }
                // If not informing user, remove message from agent
                if (informUserEx == null)
                {
                    agent.Abandon(msgId, null);
                }
                conn.removeMessage(this);
            }
            // Get rid of all replies queued
            if (informUserEx != null)
            {
                replies.Add(new LdapResponse(informUserEx, conn.ActiveReferral));
                stopTimer();
                // wake up waiting threads to receive exception
                sleepersAwake();
                // Message will get cleaned up when last response removed from queue
            }
            else
            {
                // Wake up any waiting threads, so they can terminate.
                // If informing the user, we wake sleepers after
                // caller queues dummy response with error status
                sleepersAwake();
                cleanup();
            }
        }

        private void cleanup()
        {
            stopTimer(); // Make sure timer stopped
            try
            {
                acceptReplies = false;
                if (conn != null)
                {
                    conn.removeMessage(this);
                }
                // Empty out any accumuluated replies
                if (replies != null)
                {
                    while (!(replies.Count == 0))
                    {
                        object temp_object;
                        temp_object = replies[0];
                        replies.RemoveAt(0);
                        var generatedAux = temp_object;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.LogWarning("Exception swallowed", ex);
            }
            _stackTraceCleanup = Environment.StackTrace;
            // Let GC clean up this stuff, leave name in case finalized is called
            conn = null;
            msg = null;
            // agent = null;  // leave this reference
            queue = null;
            //replies = null; //leave this since we use it as a semaphore
            bindprops = null;
        }


        internal virtual void putReply(RfcLdapMessage message)
        {
            if (!acceptReplies)
            {
                return;
            }
            lock (replies)
            {
                replies.Add(message);
            }
            message.RequestingMessage = msg; // Save request message info
            switch (message.Type)
            {
                case LdapMessage.SEARCH_RESPONSE:
                case LdapMessage.SEARCH_RESULT_REFERENCE:
                case LdapMessage.INTERMEDIATE_RESPONSE:
                    break;

                default:
                    int res;
                    stopTimer();
                    // Accept no more results for this message
                    // Leave on connection queue so we can abandon if necessary
                    acceptReplies = false;
                    complete = true;
                    if (bindprops != null)
                    {
                        res = ((RfcResponse) message.Response).getResultCode().intValue();
                        if (res != LdapException.SASL_BIND_IN_PROGRESS)
                        {
                            if (conn == null)
                            {
                                Logger.Log.ErrorFormat("Null connection; creation stack {0}, cleanup stack {1}",
                                    _stackTraceCreation, _stackTraceCleanup);
                            }

                            int id;
                            // We either have success or failure on the bind
                            if (res == LdapException.SUCCESS)
                            {
                                // Set bind properties into connection object
                                conn.BindProperties = bindprops;
                            }
                            // If not a sasl bind in-progress, release the bind
                            // semaphore and wake up all waiting threads
                            if (conn.BindSemIdClear)
                            {
                                // Semaphore id for normal operations
                                id = msgId;
                            }
                            else
                            {
                                // Semaphore id for sasl bind
                                id = conn.BindSemId;
                                conn.clearBindSemId();
                            }
                            conn.freeWriteSemaphore(id);
                        }
                    }
                    break;
            }
            // wake up waiting threads
            sleepersAwake();
        }

        /// <summary> stops the timeout timer from running</summary>
        internal virtual void stopTimer()
        {
            // If timer thread started, stop it
            if (timer != null)
            {
                timer.Stop();
            }
        }

        /// <summary> Notifies all waiting threads</summary>
        private void sleepersAwake()
        {
            // Notify any thread waiting for this message id
            lock (replies)
            {
                Monitor.Pulse(replies);
            }
            // Notify a thread waiting for any message id
            agent.sleepersAwake(false);
        }

        /// <summary>
        ///     Timer class to provide timing for messages.  Only called
        ///     if time to wait is non zero.
        /// </summary>
        private sealed class Timeout : SupportClass.ThreadClass
        {
            private void InitBlock(Message enclosingInstance)
            {
                this.enclosingInstance = enclosingInstance;
            }

            private Message enclosingInstance;

            public Message Enclosing_Instance
            {
                get { return enclosingInstance; }
            }

            private readonly int timeToWait;
            private readonly Message message;


            internal Timeout(Message enclosingInstance, int interval, Message msg)
            {
                InitBlock(enclosingInstance);
                timeToWait = interval;
                message = msg;
            }

            /// <summary>
            ///     The timeout thread.  If it wakes from the sleep, future input
            ///     is stopped and the request is timed out.
            /// </summary>
            public override void Run()
            {
                for (var i = 0; i < 10000; i++)
                {
                    if (!IsStopping)
                    {
                        Thread.Sleep(new TimeSpan(timeToWait));
                        message.acceptReplies = false;
                        // Note: Abandon clears the bind semaphore after failed bind.
                        message.Abandon(null,
                            new InterThreadException("Client request timed out", null, LdapException.Ldap_TIMEOUT, null,
                                message));
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary> sets the agent for this message</summary>
    }
}