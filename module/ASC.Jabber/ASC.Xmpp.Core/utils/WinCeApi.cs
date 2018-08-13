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
