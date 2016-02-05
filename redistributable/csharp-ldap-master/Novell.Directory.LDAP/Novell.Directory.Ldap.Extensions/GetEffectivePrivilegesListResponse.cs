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
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Extensions
{
	public class GetEffectivePrivilegesListResponse:LdapExtendedResponse
	{
		/// <summary>  Retrieves the effective rights from an GetEffectivePrivilegesListResponse object.
		/// 
		/// An object in this class is generated from an ExtendedResponse object
		/// using the ExtendedResponseFactory class.
		/// 
		/// The getEffectivePrivilegesListResponse extension uses the following OID:
		/// 2.16.840.1.113719.1.27.100.104
		/// 
		/// </summary>

		//Identity returned by the server
		private int[] privileges= {0};
		//Number of responses
		private int no_privileges;
	
		public GetEffectivePrivilegesListResponse (RfcLdapMessage rfcMessage):base(rfcMessage)
		{
			/// <summary> Constructs an object from the responseValue which contains the effective
			/// privileges.
			/// 
			/// The constructor parses the responseValue which has the following
			/// format:
			/// responseValue ::=<br>
			/// <p>SEQUENCE numberofresponses ::= INTEGER <br>
			/// SET of [<br>
			/// SEQUENCES of {privileges INTEGER}]<br>
			///  
			/// </summary>
			/// <exception> IOException The responseValue could not be decoded.
			/// </exception>
			if (ResultCode == LdapException.SUCCESS)
			{
				// parse the contents of the reply
				sbyte [] returnedValue = this.Value;
				if (returnedValue == null)
					throw new System.IO.IOException("No returned value");

				//Create a decoder object
				LBERDecoder decoder = new LBERDecoder();
				if (decoder == null)
					throw new System.IO.IOException("Decoding error");
			
				Asn1Sequence asn1_seq1 = (Asn1Sequence)decoder.decode(returnedValue);
				if (asn1_seq1 == null)
					throw new System.IO.IOException("Decoding error");
				Asn1Sequence asn1_seq2 = (Asn1Sequence)asn1_seq1.get_Renamed(0);
				no_privileges = ((Asn1Integer)asn1_seq2.get_Renamed(0)).intValue();
			
				Asn1Set set_privileg_response = null;
				
				set_privileg_response = ((Asn1Set)asn1_seq1.get_Renamed(1));
				Asn1Sequence seq2 = null;
				privileges = new int[no_privileges];
				for(int index=0; index < no_privileges; index++)
				{
					seq2 = (Asn1Sequence)set_privileg_response.get_Renamed(index);
					privileges[index]=((Asn1Integer)seq2.get_Renamed(0)).intValue();
				}
			}
		}
	
		public int[] getPrivileges()
		{
			return privileges;
		}
	}
}