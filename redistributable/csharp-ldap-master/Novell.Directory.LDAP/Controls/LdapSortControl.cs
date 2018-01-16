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
// Novell.Directory.Ldap.Controls.LdapSortControl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
    /// <summary>
    ///     LdapSortControl is a Server Control to specify how search results are
    ///     to be sorted by the server. If a server does not support
    ///     sorting in general or for a particular query, the results will be
    ///     returned unsorted, along with a control indicating why they were not
    ///     sorted (or that sort controls are not supported). If the control was
    ///     marked "critical", the whole search operation will fail if the sort
    ///     control is not supported.
    /// </summary>
    public class LdapSortControl : LdapControl
    {
        private static readonly int ORDERING_RULE = 0;
        private static readonly int REVERSE_ORDER = 1;

        /// <summary> The requestOID of the sort control</summary>
        private static readonly string requestOID = "1.2.840.113556.1.4.473";

        /// <summary> The responseOID of the sort control</summary>
        private static readonly string responseOID = "1.2.840.113556.1.4.474";

        /// <summary>
        ///     Constructs a sort control with a single key.
        /// </summary>
        /// <param name="key">
        ///     A sort key object, which specifies attribute,
        ///     order, and optional matching rule.
        /// </param>
        /// <param name="critical	True">
        ///     if the search operation is to fail if the
        ///     server does not support this control.
        /// </param>
        public LdapSortControl(LdapSortKey key, bool critical) : this(new[] {key}, critical)
        {
        }

        /// <summary>
        ///     Constructs a sort control with multiple sort keys.
        /// </summary>
        /// <param name="keys		An">
        ///     array of sort key objects, to be processed in
        ///     order.
        /// </param>
        /// <param name="critical	True">
        ///     if the search operation is to fail if the
        ///     server does not support this control.
        /// </param>
        public LdapSortControl(LdapSortKey[] keys, bool critical) : base(requestOID, critical, null)
        {
            var sortKeyList = new Asn1SequenceOf();

            for (var i = 0; i < keys.Length; i++)
            {
                var key = new Asn1Sequence();

                key.add(new Asn1OctetString(keys[i].Key));

                if ((object) keys[i].MatchRule != null)
                {
                    key.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, ORDERING_RULE),
                        new Asn1OctetString(keys[i].MatchRule), false));
                }

                if (keys[i].Reverse)
                {
                    // only add if true
                    key.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, REVERSE_ORDER),
                        new Asn1Boolean(true), false));
                }

                sortKeyList.add(key);
            }

            setValue(sortKeyList.getEncoding(new LBEREncoder()));
        }

        static LdapSortControl()
        {
            /*
            * This is where we register the control responses
            */
            {
                /*
                * Register the Server Sort Control class which is returned by the
                * server in response to a Sort Request
                */
                try
                {
                    register(responseOID, Type.GetType("Novell.Directory.Ldap.Controls.LdapSortResponse"));
                }
                catch (Exception e)
                {
                    Logger.Log.LogWarning("Exception swallowed", e);
                }
            }
        }
    }
}