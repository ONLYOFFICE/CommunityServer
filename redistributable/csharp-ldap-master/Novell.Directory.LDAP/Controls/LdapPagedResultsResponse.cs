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
// Novell.Directory.Ldap.Controls.LdapPagedResultsResponse.cs
//
// Author:
//   Igor Shmukler
//
// (C) 2014 VQ Communications Ltd. (http://www.vqcomms.com)
//

using System;
using System.Text;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
    public class LdapPagedResultsResponse : LdapControl
    {
        public virtual int Size
        {
            get
            {
                return m_size;
            }

        }

        public virtual sbyte[] Cookie
        {
            get
            {
                return m_cookie;
            }

        }

        /* The parsed fields are stored in these private variables */
        private readonly int m_size;
        private readonly sbyte[] m_cookie;

        [CLSCompliantAttribute(false)]
        public LdapPagedResultsResponse(string oid, bool critical, sbyte[] values)
            : base(oid, critical, values)
        {

            /* Create a decoder object */
            var decoder = new LBERDecoder();
            if (decoder == null)
                throw new System.IO.IOException("Decoding error");

            /* We should get back an ASN.1 Sequence object */
            var asnObj = decoder.decode(values);
            if ((asnObj == null) || (!(asnObj is Asn1Sequence)))
                throw new System.IO.IOException("Decoding error");

            /* 
             * Get the 1st element which should be an integer containing the
             * size (RFC 2696).
             */
            var asn1Size = ((Asn1Sequence)asnObj).get_Renamed(0);
            if ((asn1Size != null) && (asn1Size is Asn1Integer))
                m_size = ((Asn1Integer)asn1Size).intValue();
            else
                throw new System.IO.IOException("Decoding error");

            /*
             * Get the 2nd element which should be an octet string containing the
             * cookie (RFC 2696).
             */
            var asn1Cookie = ((Asn1Sequence)asnObj).get_Renamed(1);
            if ((asn1Cookie != null) && (asn1Cookie is Asn1OctetString))
            {
                var t = ((Asn1OctetString) asn1Cookie).byteValue();

                /*byte[] bytes = Array.ConvertAll(t, b => unchecked((byte)b));

                var s = "";
                foreach (byte b in bytes)
                    s += (char)b;*/

                //var utf8bytes = Encoding.UTF8.GetBytes(t);
                /*var encoder = Encoding.GetEncoding("utf-8");
                var dchar = encoder.GetChars(SupportClass.ToByteArray(t));
                s = new string(dchar);*/

                m_cookie = t;
            }
            else
                throw new System.IO.IOException("Decoding error");
        }
    }
}