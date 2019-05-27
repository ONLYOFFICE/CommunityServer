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
// Novell.Directory.Ldap.LdapSearchResults.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     An LdapSearchResults object is returned from a synchronous search
    ///     operation. It provides access to all results received during the
    ///     operation (entries and exceptions).
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapSearchResults
    {
        /// <summary>
        ///     Returns a count of the items in the search result.
        ///     Returns a count of the entries and exceptions remaining in the object.
        ///     If the search was submitted with a batch size greater than zero,
        ///     getCount reports the number of results received so far but not enumerated
        ///     with next().  If batch size equals zero, getCount reports the number of
        ///     items received, since the application thread blocks until all results are
        ///     received.
        /// </summary>
        /// <returns>
        ///     The number of items received but not retrieved by the application
        /// </returns>
        public virtual int Count
        {
            get
            {
                var qCount = queue.MessageAgent.Count;
                return entryCount - entryIndex + referenceCount - referenceIndex + qCount;
            }
        }

        /// <summary>
        ///     Returns the latest server controls returned by the server
        ///     in the context of this search request, or null
        ///     if no server controls were returned.
        /// </summary>
        /// <returns>
        ///     The server controls returned with the search request, or null
        ///     if none were returned.
        /// </returns>
        public virtual LdapControl[] ResponseControls
        {
            get { return controls; }
        }

        /// <summary>
        ///     Collects batchSize elements from an LdapSearchQueue message
        ///     queue and places them in a Vector.
        ///     If the last message from the server,
        ///     the result message, contains an error, it will be stored in the Vector
        ///     for nextElement to process. (although it does not increment the search
        ///     result count) All search result entries will be placed in the Vector.
        ///     If a null is returned from getResponse(), it is likely that the search
        ///     was abandoned.
        /// </summary>
        /// <returns>
        ///     true if all search results have been placed in the vector.
        /// </returns>
        private bool BatchOfResults
        {
            get
            {
                LdapMessage msg;

                // <=batchSize so that we can pick up the result-done message
                for (var i = 0; i < batchSize;)
                {
                    try
                    {
                        if ((msg = queue.getResponse()) != null)
                        {
                            // Only save controls if there are some
                            var ctls = msg.Controls;
                            if (ctls != null)
                            {
                                controls = ctls;
                            }

                            if (msg is LdapSearchResult)
                            {
                                // Search Entry
                                object entry = ((LdapSearchResult) msg).Entry;
                                entries.Add(entry);
                                i++;
                                entryCount++;
                            }
                            else if (msg is LdapSearchResultReference)
                            {
                                // Search Ref
                                var refs = ((LdapSearchResultReference) msg).Referrals;

                                if (cons.ReferralFollowing)
                                {
//									referralConn = conn.chaseReferral(queue, cons, msg, refs, 0, true, referralConn);
                                }
                                else
                                {
                                    references.Add(refs);
                                    referenceCount++;
                                }
                            }
                            else
                            {
                                // LdapResponse
                                var resp = (LdapResponse) msg;
                                var resultCode = resp.ResultCode;
                                // Check for an embedded exception
                                if (resp.hasException())
                                {
                                    // Fake it, results in an exception when msg read
                                    resultCode = LdapException.CONNECT_ERROR;
                                }

                                if (resultCode == LdapException.REFERRAL && cons.ReferralFollowing)
                                {
                                    // Following referrals
//									referralConn = conn.chaseReferral(queue, cons, resp, resp.Referrals, 0, false, referralConn);
                                }
                                else if (resultCode != LdapException.SUCCESS)
                                {
                                    // Results in an exception when message read
                                    entries.Add(resp);
                                    entryCount++;
                                }
                                // We are done only when we have read all messages
                                // including those received from following referrals
                                var msgIDs = queue.MessageIDs;
                                if (msgIDs.Length == 0)
                                {
                                    // Release referral exceptions
//									conn.releaseReferralConnections(referralConn);
                                    return true; // search completed
                                }
                            }
                        }
                        else
                        {
                            // We get here if the connection timed out
                            // we have no responses, no message IDs and no exceptions
                            var e = new LdapException(null, LdapException.Ldap_TIMEOUT, null);
                            entries.Add(e);
                            break;
                        }
                    }
                    catch (LdapException e)
                    {
                        // Hand exception off to user
                        entries.Add(e);
                    }
                }
                return false; // search not completed
            }
        }

        private readonly ArrayList entries; // Search entries
        private int entryCount; // # Search entries in vector
        private int entryIndex; // Current position in vector
        private readonly ArrayList references; // Search Result References
        private int referenceCount; // # Search Result Reference in vector
        private int referenceIndex; // Current position in vector
        private readonly int batchSize; // Application specified batch size
        private bool completed; // All entries received
        private LdapControl[] controls; // Last set of controls
        private readonly LdapSearchQueue queue;
        private static object nameLock; // protect resultsNum
        private static int resultsNum = 0; // used for debug
        private string name; // used for debug
        private LdapConnection conn; // LdapConnection which started search
        private readonly LdapSearchConstraints cons; // LdapSearchConstraints for search
        private ArrayList referralConn = null; // Referral Connections

        /// <summary>
        ///     Constructs a queue object for search results.
        /// </summary>
        /// <param name="conn">
        ///     The LdapConnection which initiated the search
        /// </param>
        /// <param name="queue">
        ///     The queue for the search results.
        /// </param>
        /// <param name="cons">
        ///     The LdapSearchConstraints associated with this search
        /// </param>
        internal LdapSearchResults(LdapConnection conn, LdapSearchQueue queue, LdapSearchConstraints cons)
        {
            // setup entry Vector
            this.conn = conn;
            this.cons = cons;
            var batchSize = cons.BatchSize;
            var vectorIncr = batchSize == 0 ? 64 : 0;
            entries = new ArrayList(batchSize == 0 ? 64 : batchSize);
            entryCount = 0;
            entryIndex = 0;

            // setup search reference Vector
            references = new ArrayList(5);
            referenceCount = 0;
            referenceIndex = 0;

            this.queue = queue;
            this.batchSize = batchSize == 0 ? int.MaxValue : batchSize;
        }

        /// <summary>
        ///     Reports if there are more search results.
        /// </summary>
        /// <returns>
        ///     true if there are more search results.
        /// </returns>
        public virtual bool hasMore()
        {
            var ret = false;
            if (entryIndex < entryCount || referenceIndex < referenceCount)
            {
                // we have data
                ret = true;
            }
            else if (completed == false)
            {
                // reload the Vector by getting more results
                resetVectors();
                ret = entryIndex < entryCount || referenceIndex < referenceCount;
            }
            return ret;
        }

        /*
        * If both of the vectors are empty, get more data for them.
        */

        private void resetVectors()
        {
            // If we're done, no further checking needed
            if (completed)
            {
                return;
            }
            // Checks if we have run out of references
            if (referenceIndex != 0 && referenceIndex >= referenceCount)
            {
                SupportClass.SetSize(references, 0);
                referenceCount = 0;
                referenceIndex = 0;
            }
            // Checks if we have run out of entries
            if (entryIndex != 0 && entryIndex >= entryCount)
            {
                SupportClass.SetSize(entries, 0);
                entryCount = 0;
                entryIndex = 0;
            }
            // If no data at all, must reload enumeration
            if (referenceIndex == 0 && referenceCount == 0 && entryIndex == 0 && entryCount == 0)
            {
                completed = BatchOfResults;
            }
        }

        /// <summary>
        ///     Returns the next result as an LdapEntry.
        ///     If automatic referral following is disabled or if a referral
        ///     was not followed, next() will throw an LdapReferralException
        ///     when the referral is received.
        /// </summary>
        /// <returns>
        ///     The next search result as an LdapEntry.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        /// <exception>
        ///     LdapReferralException A referral was received and not
        ///     followed.
        /// </exception>
        public virtual LdapEntry next()
        {
            if (completed && entryIndex >= entryCount && referenceIndex >= referenceCount)
            {
                throw new ArgumentOutOfRangeException("LdapSearchResults.next() no more results");
            }
            // Check if the enumeration is empty and must be reloaded
            resetVectors();

            object element = null;
            // Check for Search References & deliver to app as they come in
            // We only get here if not following referrals/references
            if (referenceIndex < referenceCount)
            {
                var refs = (string[]) references[referenceIndex++];
                var rex = new LdapReferralException(ExceptionMessages.REFERENCE_NOFOLLOW);
                rex.setReferrals(refs);
                throw rex;
            }
            if (entryIndex < entryCount)
            {
                // Check for Search Entries and the Search Result
                element = entries[entryIndex++];
                if (element is LdapResponse)
                {
                    // Search done w/bad status
                    if (((LdapResponse) element).hasException())
                    {
                        var lr = (LdapResponse) element;
                        var ri = lr.ActiveReferral;

                        if (ri != null)
                        {
                            // Error attempting to follow a search continuation reference
                            var rex = new LdapReferralException(ExceptionMessages.REFERENCE_ERROR, lr.Exception);
                            rex.setReferrals(ri.ReferralList);
                            rex.FailedReferral = ri.ReferralUrl.ToString();
                            throw rex;
                        }
                    }
                    // Throw an exception if not success
                    ((LdapResponse) element).chkResultCode();
                }
                else if (element is LdapException)
                {
                    throw (LdapException) element;
                }
            }
            else
            {
                // If not a Search Entry, Search Result, or search continuation
                // we are very confused.
                // LdapSearchResults.next(): No entry found & request is not complete
                throw new LdapException(ExceptionMessages.REFERRAL_LOCAL, new object[] {"next"},
                    LdapException.LOCAL_ERROR, null);
            }
            return (LdapEntry) element;
        }

        /// <summary> Cancels the search request and clears the message and enumeration.</summary>
        /*package*/
        internal virtual void Abandon()
        {
            // first, remove message ID and timer and any responses in the queue
            queue.MessageAgent.AbandonAll();

            // next, clear out enumeration
            resetVectors();
            completed = true;
        }

        static LdapSearchResults()
        {
            nameLock = new object();
        }
    }
}