/******************************************************************************
* The MIT License
* Copyright (c) 2009 Novell Inc.  www.novell.com
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
// Novell.Directory.Ldap.Extensions.GetEffectivePrivilegesResponse.cs
//
// Author:
//   Arpit Rastogi (Rarpit@novell.com)
//
// (C) 2009 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap.Extensions
{
    public class GetEffectivePrivilegesListRequest : LdapExtendedOperation
    {
        /// <summary>
        ///     Returns the effective rights of one object to a list of attributes of another object.
        ///     To use this class, you must instantiate an object of this class and then
        ///     call the extendedOperation method with this object as the required
        ///     LdapExtendedOperation parameter.
        ///     The returned LdapExtendedResponse object can then be converted to
        ///     a GetEffectivePrivilegesListResponse object with the ExtendedResponseFactory class.
        ///     The GetEffectivePrivilegesListResponse class  contains methods for
        ///     retrieving the list of effective rights.
        ///     The getEffectivePrivilegesRequest extension uses the following OID:
        ///     2.16.840.1.113719.1.27.100.103
        ///     The requestValue has the following format:
        ///     requestValue ::=
        ///     dn         LdapDN
        ///     trusteeDN  LdapDN
        ///     SEQUENCE of attrNames   LdapDN
        /// </summary>
        static GetEffectivePrivilegesListRequest()
        {
            LdapExtendedResponse.register(ReplicationConstants.GET_EFFECTIVE_LIST_PRIVILEGES_RES,
                typeof(GetEffectivePrivilegesListResponse));
        }

        /// <summary>
        ///     Constructs an extended operation object for checking effective rights.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry whose attribute is
        ///     being checked.
        /// </param>
        /// <param name="trusteeDN">
        ///     The distinguished name of the entry whose trustee rights
        ///     are being returned
        /// </param>
        /// <param name={"attr1","attr2",..., null}>
        ///     The Ldap attribute names list.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public GetEffectivePrivilegesListRequest(string dn, string trusteeDN, string[] attrName)
            : base(ReplicationConstants.GET_EFFECTIVE_LIST_PRIVILEGES_REQ, null)
        {
            try
            {
                if ((object) dn == null)
                    throw new ArgumentException(ExceptionMessages.PARAM_ERROR);

                var encodedData = new MemoryStream();
                var encoder = new LBEREncoder();

                var asn1_trusteeDN = new Asn1OctetString(trusteeDN);
                var asn1_dn = new Asn1OctetString(dn);
                asn1_trusteeDN.encode(encoder, encodedData);
                asn1_dn.encode(encoder, encodedData);

                var asn1_seqattr = new Asn1Sequence();
                for (var i = 0; attrName[i] != null; i++)
                {
                    var asn1_attrName = new Asn1OctetString(attrName[i]);
                    asn1_seqattr.add(asn1_attrName);
                }
                asn1_seqattr.encode(encoder, encodedData);
                setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
            }
            catch (IOException ioe)
            {
                throw new LdapException(ExceptionMessages.ENCODING_ERROR, LdapException.ENCODING_ERROR, null, ioe);
            }
        }
    }
}