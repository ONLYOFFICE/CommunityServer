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

using System;
using System.Runtime.InteropServices;

namespace agsXMPP.util
{
	/// <summary>
	/// Crypto API for Windows CE, Pocket PC and Smartphone
	/// will be used for Hashing and the RandomNumberGenerator
	/// </summary>
	internal class WinCeApi
	{
		public enum SecurityProviderType
		{
			RSA_FULL		    = 1,
			HP_HASHVAL		    = 2,
			CALG_MD5		    = 32771,
			CALG_SHA1		    = 32772            
		}        

		[DllImport("coredll.dll")]
		public static extern bool CryptAcquireContext(out IntPtr hProv, string pszContainer, string pszProvider, int dwProvType,int dwFlags);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptCreateHash(IntPtr hProv, int Algid, IntPtr hKey, int dwFlags, out IntPtr phHash);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptHashData(IntPtr hHash, byte [] pbData, int dwDataLen, int dwFlags);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptGetHashParam(IntPtr hHash, int dwParam, byte[] pbData, ref int pdwDataLen, int dwFlags);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptDestroyHash(IntPtr hHash);
		
		[DllImport("coredll.dll")]
		public static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);

		[DllImport("coredll.dll", EntryPoint="CryptGenRandom", SetLastError=true)]
		public static extern bool CryptGenRandomCe(IntPtr hProv, int dwLen, byte[] pbBuffer);
		
		[DllImport("advapi32.dll", EntryPoint="CryptGenRandom", SetLastError=true)]
		public static extern bool CryptGenRandomXp(IntPtr hProv, int dwLen, byte[] pbBuffer);
		
	}
}
