/******************************************************************************
* The MIT License
* Copyright (c) 2014 VQ Communications Ltd.  www.vqcomms.com
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
// Novell.Directory.Ldap.Controls.LdapPagedResultsControl.cs
//
// Author:
//   Igor Shmukler
//
// (C) 2014 VQ Communications Ltd. (http://www.vqcomms.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
    /* 
     * The following is the ASN.1 of the Page Results control:
     *
     * pagedResultsControl ::= SEQUENCE {
     *   controlType     1.2.840.113556.1.4.319,
     *   criticality     BOOLEAN DEFAULT FALSE,
     *   controlValue    searchControlValue
     * }
     * 
     * realSearchControlValue ::= SEQUENCE {
     *   size            INTEGER (0..maxInt),
     *                           -- requested page size from client
     *                           -- result set size estimate from server
     *   cookie          OCTET STRING
     * }
     */

    public sealed class LdapPagedResultsControl : LdapControl
    {
        /// <summary> The Request OID for a Paged Results Request</summary>
        private static string requestOID = "1.2.840.113556.1.4.319";
        /// <summary> The Response stOID for a Paged Results Response</summary>
        private static string responseOID = "1.2.840.113556.1.4.319";

        /*
         * The encoded ASN.1 Paged Search Request Control is stored in this variable
         */
        private Asn1Sequence m_pagedSearchRequest;

        /* Private instance variables go here.
         * These variables are used to store copies of various fields
         * that can be set in a Paged Results control. One could have managed
         * without really defining these private variables by reverse
         * engineering each field from the ASN.1 encoded control.
         * However that would have complicated and slowed down the code.
         */
        private readonly int m_pageSize;
        private readonly sbyte[] m_cookie;

        // Put together search control value and have the base class - LdapControl package the request.
        public LdapPagedResultsControl(int pageSize, sbyte[] cookie)
            : base(requestOID, true, null)
        {
            m_pageSize = pageSize;
            m_cookie = cookie;

            BuildPagedSearchRequest();

            /* Set the request data field in the in the parent LdapControl to
             * the ASN.1 encoded value of this control.  This encoding will be
             * appended to the search request when the control is sent.
             */
            setValue(m_pagedSearchRequest.getEncoding(new LBEREncoder()));
        }

        /// <summary>Private method used to construct the ber encoded control
        /// </summary>
        private void BuildPagedSearchRequest()
        {
            /* Create a new Asn1Sequence object */
            m_pagedSearchRequest = new Asn1Sequence(2);

            /* Add the pageSize and cookie to the sequence */
            m_pagedSearchRequest.add(new Asn1Integer(m_pageSize));
            m_pagedSearchRequest.add(new Asn1OctetString(m_cookie));
        }

        static LdapPagedResultsControl()
        {

            // This is where we register the control responses
            {
                // Register the Paged Results Response class, which is returned by the server
                // in response to a Paged Results Request - same OID
                try
                {
                    register(responseOID, Type.GetType("Novell.Directory.Ldap.Controls.LdapPagedResultsResponse"));
                }
                catch (Exception e)
                {
                    Logger.Log.WarnFormat("Could not register response control for LdapPagedResultsControl.", e);
                }
            }
        }
    }
}
