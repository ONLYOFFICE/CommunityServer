/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;

namespace LumiSoft.Net.AUTH
{
	/// <summary>
	/// Authentication type.
	/// </summary>
	public enum AuthType
	{
		/// <summary>
		/// Clear text username/password authentication.
		/// </summary>
		PLAIN = 0,

		/// <summary>
		/// APOP.This is used by POP3 only. RFC 1939 7. APOP.
		/// </summary>
		APOP  = 1,
	
		/// <summary>
		/// CRAM-MD5 authentication. RFC 2195 AUTH CRAM-MD5.
		/// </summary>
		CRAM_MD5 = 3,

		/// <summary>
		/// DIGEST-MD5 authentication. RFC 2831 AUTH DIGEST-MD5.
		/// </summary>
		DIGEST_MD5 = 4,
	}
}
