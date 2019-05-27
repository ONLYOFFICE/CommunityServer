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
// Novell.Directory.Ldap.LdapSearchResult.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Encapsulates a single search result that is in response to an asynchronous
    ///     search operation.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapSearchResult : LdapMessage
    {
        /// <summary>
        ///     Returns the entry of a server's search response.
        /// </summary>
        /// <returns>
        ///     The LdapEntry associated with this LdapSearchResult
        /// </returns>
        public virtual LdapEntry Entry
        {
            get
            {
                if (entry == null)
                {
                    var attrs = new LdapAttributeSet();

                    var attrList = ((RfcSearchResultEntry) message.Response).Attributes;

                    var seqArray = attrList.toArray();
                    for (var i = 0; i < seqArray.Length; i++)
                    {
                        var seq = (Asn1Sequence) seqArray[i];
                        var attr = new LdapAttribute(((Asn1OctetString) seq.get_Renamed(0)).stringValue());

                        var set_Renamed = (Asn1Set) seq.get_Renamed(1);
                        object[] setArray = set_Renamed.toArray();
                        for (var j = 0; j < setArray.Length; j++)
                        {
                            attr.addValue(((Asn1OctetString) setArray[j]).byteValue());
                        }
                        attrs.Add(attr);
                    }

                    entry = new LdapEntry(((RfcSearchResultEntry) message.Response).ObjectName.stringValue(), attrs);
                }
                return entry;
            }
        }

        private LdapEntry entry;

        /// <summary>
        ///     Constructs an LdapSearchResult object.
        /// </summary>
        /// <param name="message">
        ///     The RfcLdapMessage with a search result.
        /// </param>
        /*package*/
        internal LdapSearchResult(RfcLdapMessage message) : base(message)
        {
        }

        /// <summary>
        ///     Constructs an LdapSearchResult object from an LdapEntry.
        /// </summary>
        /// <param name="entry">
        ///     the LdapEntry represented by this search result.
        /// </param>
        /// <param name="cont">
        ///     controls associated with the search result
        /// </param>
        public LdapSearchResult(LdapEntry entry, LdapControl[] cont)
        {
            if (entry == null)
            {
                throw new ArgumentException("Argument \"entry\" cannot be null");
            }
            this.entry = entry;
        }

        /// <summary>
        ///     Return a String representation of this object.
        /// </summary>
        /// <returns>
        ///     a String representing this object.
        /// </returns>
        public override string ToString()
        {
            string str;
            if (entry == null)
            {
                str = base.ToString();
            }
            else
            {
                str = entry.ToString();
            }
            return str;
        }
    }
}