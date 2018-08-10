/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2007 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

#if CF
using System;

namespace agsXMPP.util
{
	/// <summary>
	/// Implements a cryptographic Random Number Generator (RNG) using the implementation 
	/// provided by the cryptographic service provider (CSP).
	/// Its a replacement for System.Security.Cryptography.RandomNumberGenerator which
	/// is not available in the compact framework.
	/// </summary>
	public class RNGCryptoServiceProvider : RandomNumberGenerator
	{
		public RNGCryptoServiceProvider()
		{

		}

		/// <summary>
		/// Fills an array of bytes with a cryptographically strong random sequence of values.
		/// </summary>
		/// <param name="seed">The array to fill with cryptographically strong random bytes.</param>
		public override void GetBytes(byte[] seed)
		{
			seed = _GetRandomBytes(seed);
		}

		/// <summary>
		/// Fills an array of bytes with a cryptographically strong random sequence of nonzero values.
		/// </summary>
		/// <param name="seed">The array to fill with cryptographically strong random nonzero bytes.</param>
		public override void GetNonZeroBytes(byte[] seed)
		{
			seed = _GetNonZeroBytes(seed);
		}
		
		#region << private functions >>
		private byte [] _GetRandomBytes(byte[] seed)
		{			
			IntPtr prov;
			bool retVal = WinCeApi.CryptAcquireContext(out prov, null, null, (int) WinCeApi.SecurityProviderType.RSA_FULL, 0);
			retVal = _CryptGenRandom(prov, seed.Length, seed);			
			WinCeApi.CryptReleaseContext(prov, 0);
			return seed;
		}

		private bool _CryptGenRandom(IntPtr hProv, int dwLen, byte[] pbBuffer)
		{
			if(System.Environment.OSVersion.Platform == PlatformID.WinCE)
				return WinCeApi.CryptGenRandomCe(hProv, dwLen, pbBuffer);
			else
				return WinCeApi.CryptGenRandomXp(hProv, dwLen, pbBuffer);
		}

		private byte [] _GetNonZeroBytes(byte[] seed)
		{
			byte [] buf = _GetRandomBytes(seed);
			for(int i=0; i<buf.Length; i++)
			{
				if(buf[i] == 0)
					buf[i] = 1;
			}
			return buf;
		}
		#endregion

	}
}
#endif