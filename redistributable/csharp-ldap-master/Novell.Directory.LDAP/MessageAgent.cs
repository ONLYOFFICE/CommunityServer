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
// Novell.Directory.Ldap.MessageAgent.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    internal class MessageAgent
    {
        private void InitBlock()
        {
            messages = new MessageVector(5, 5);
        }

        /// <summary>
        ///     empty and return all messages owned by this agent
        /// </summary>
        internal virtual object[] RemoveAll()
        {
            return messages.RemoveAll();
        }

        /// <summary>
        ///     Get a list of message ids controlled by this agent
        /// </summary>
        /// <returns>
        ///     an array of integers representing the message ids
        /// </returns>
        internal virtual int[] MessageIDs
        {
            get
            {
                var size = messages.Count;
                var ids = new int[size];
                Message info;

                for (var i = 0; i < size; i++)
                {
                    info = (Message) messages[i];
                    ids[i] = info.MessageID;
                }
                return ids;
            }
        }

        /// <summary>
        ///     Get the maessage agent number for debugging
        /// </summary>
        /// <returns>
        ///     the agent number
        /// </returns>
        internal virtual string AgentName
        {
            /*packge*/
            get { return name; }
        }

        /// <summary> Get a count of all messages queued</summary>
        internal virtual int Count
        {
            get
            {
                var count = 0;
                var msgs = messages.ToArray();
                for (var i = 0; i < msgs.Length; i++)
                {
                    var m = (Message) msgs[i];
                    count += m.Count;
                }
                return count;
            }
        }

        private MessageVector messages;
        private int indexLastRead;
        private static object nameLock; // protect agentNum
        private static int agentNum = 0; // Debug, agent number
        private string name; // String name for debug


        internal MessageAgent()
        {
            InitBlock();
            // Get a unique agent id for debug
        }

        /// <summary>
        ///     merges two message agents
        /// </summary>
        /// <param name="fromAgent">
        ///     the agent to be merged into this one
        /// </param>
        internal void merge(MessageAgent fromAgent)
        {
            var msgs = fromAgent.RemoveAll();
            for (var i = 0; i < msgs.Length; i++)
            {
                messages.Add(msgs[i]);
                ((Message) msgs[i]).Agent = this;
            }
            lock (messages)
            {
                if (msgs.Length > 1)
                {
                    Monitor.PulseAll(messages); // wake all threads waiting for messages
                }
                else if (msgs.Length == 1)
                {
                    Monitor.Pulse(messages); // only wake one thread
                }
            }
        }


        /// <summary>
        ///     Wakes up any threads waiting for messages in the message agent
        /// </summary>
        internal void sleepersAwake(bool all)
        {
            lock (messages)
            {
                if (all)
                    Monitor.PulseAll(messages);
                else
                    Monitor.Pulse(messages);
            }
        }

        /// <summary>
        ///     Returns true if any responses are queued for any of the agent's messages
        ///     return false if no responses are queued, otherwise true
        /// </summary>
        internal bool isResponseReceived()
        {
            var size = messages.Count;
            var next = indexLastRead + 1;
            Message info;
            for (var i = 0; i < size; i++)
            {
                if (next == size)
                {
                    next = 0;
                }
                info = (Message) messages[next];
                if (info.hasReplies())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Returns true if any responses are queued for the specified msgId
        ///     return false if no responses are queued, otherwise true
        /// </summary>
        internal bool isResponseReceived(int msgId)
        {
            try
            {
                var info = messages.FindMessageById(msgId);
                return info.hasReplies();
            }
            catch (FieldAccessException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Abandon the request associated with MsgId
        /// </summary>
        /// <param name="msgId">
        ///     the message id to abandon
        /// </param>
        /// <param name="cons">
        ///     constraints associated with this request
        /// </param>
        internal void Abandon(int msgId, LdapConstraints cons)
            //, boolean informUser)
        {
            Message info = null;
            try
            {
                // Send abandon request and remove from connection list
                info = messages.FindMessageById(msgId);
                SupportClass.VectorRemoveElement(messages, info); // This message is now dead
                info.Abandon(cons, null);
            }
            catch (FieldAccessException ex)
            {
                Logger.Log.LogWarning("Exception swallowed", ex);
            }
        }

        /// <summary> Abandon all requests on this MessageAgent</summary>
        internal void AbandonAll()
        {
            var size = messages.Count;
            Message info;

            for (var i = 0; i < size; i++)
            {
                info = (Message) messages[i];
                // Message complete and no more replies, remove from id list
                SupportClass.VectorRemoveElement(messages, info);
                info.Abandon(null, null);
            }
        }

        /// <summary>
        ///     Indicates whether a specific operation is complete
        /// </summary>
        /// <returns>
        ///     true if a specific operation is complete
        /// </returns>
        internal bool isComplete(int msgid)
        {
            try
            {
                var info = messages.FindMessageById(msgid);
                if (!info.Complete)
                {
                    return false;
                }
            }
            catch (FieldAccessException ex)
            {
                // return true, if no message, it must be complete
                Logger.Log.LogWarning("Exception swallowed", ex);
            }
            return true;
        }

        /// <summary>
        ///     Returns the Message object for a given messageID
        /// </summary>
        /// <param name="msgid">
        ///     the message ID.
        /// </param>
        internal Message getMessage(int msgid)
        {
            return messages.FindMessageById(msgid);
        }

        /// <summary>
        ///     Send a request to the server.  A Message class is created
        ///     for the specified request which causes the message to be sent.
        ///     The request is added to the list of messages being managed by
        ///     this agent.
        /// </summary>
        /// <param name="conn">
        ///     the connection that identifies the server.
        /// </param>
        /// <param name="msg">
        ///     the LdapMessage to send
        /// </param>
        /// <param name="timeOut">
        ///     the interval to wait for the message to complete or
        ///     <code>null</code> if infinite.
        /// </param>
        /// <param name="queue">
        ///     the LdapMessageQueue associated with this request.
        /// </param>
        internal void sendMessage(Connection conn, LdapMessage msg, int timeOut, LdapMessageQueue queue,
            BindProperties bindProps)
        {
            // creating a messageInfo causes the message to be sent
            // and a timer to be started if needed.
            var message = new Message(msg, timeOut, conn, this, queue, bindProps);
            messages.Add(message);
            message.sendMessage(); // Now send message to server
        }

        /// <summary>
        ///     Returns a response queued, or waits if none queued
        /// </summary>

//		internal System.Object getLdapMessage(System.Int32 msgId)
        internal object getLdapMessage(int msgId)
        {
            return getLdapMessage(new Integer32(msgId));
        }

        internal object getLdapMessage(Integer32 msgId)
        {
            object rfcMsg;
            // If no messages for this agent, just return null
            if (messages.Count == 0)
            {
                return null;
            }
            if (msgId != null)
            {
                // Request messages for a specific ID
                try
                {
                    // Get message for this ID
//					Message info = messages.findMessageById(msgId);
                    var info = messages.FindMessageById(msgId.intValue);
                    rfcMsg = info.waitForReply(); // blocks for a response
                    if (!info.acceptsReplies() && !info.hasReplies())
                    {
                        // Message complete and no more replies, remove from id list
                        SupportClass.VectorRemoveElement(messages, info);
                        info.Abandon(null, null); // Get rid of resources
                    }
                    return rfcMsg;
                }
                catch (FieldAccessException)
                {
                    // no such message id
                    return null;
                }
            }
            // A msgId was NOT specified, any message will do
            lock (messages)
            {
                while (true)
                {
                    var next = indexLastRead + 1;
                    Message info;
                    for (var i = 0; i < messages.Count; i++)
                    {
                        if (next >= messages.Count)
                        {
                            next = 0;
                        }
                        info = (Message) messages[next];
                        indexLastRead = next++;
                        rfcMsg = info.Reply;
                        // Check this request is complete
                        if (!info.acceptsReplies() && !info.hasReplies())
                        {
                            // Message complete & no more replies, remove from id list
                            SupportClass.VectorRemoveElement(messages, info); // remove from list
                            info.Abandon(null, null); // Get rid of resources
                            // Start loop at next message that is now moved
                            // to the current position in the Vector.
                            i -= 1;
                        }
                        if (rfcMsg != null)
                        {
                            // We got a reply
                            return rfcMsg;
                        }
                    } // end for loop */
                    // Messages can be removed in this loop, we we must
                    // check if any messages left for this agent
                    if (messages.Count == 0)
                    {
                        return null;
                    }

                    // No data, wait for something to come in.
                    Monitor.Wait(messages);
                } /* end while */
            } /* end synchronized */
        }

        /// <summary> Debug code to print messages in message vector</summary>
        private void debugDisplayMessages()
        {
        }

        static MessageAgent()
        {
            nameLock = new object();
        }
    }
}