/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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