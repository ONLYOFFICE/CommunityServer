/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
