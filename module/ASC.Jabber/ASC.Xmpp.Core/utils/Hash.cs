/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


#region using

using System.Security.Cryptography;
using System.Text;

#endregion

#if !CF

#endif

namespace ASC.Xmpp.Core.utils
{

    #region usings

    #endregion

    /// <summary>
    ///   Summary description for Hash.
    /// </summary>
    public class Hash
    {
        #region << SHA1 Hash Desktop Framework and Mono >>		

#if !CF

        /// <summary>
        /// </summary>
        /// <param name="pass"> </param>
        /// <returns> </returns>
        public static string Sha1Hash(string pass)
        {
            SHA1 sha = SHA1.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(pass));
            return HexToString(hash);
        }

        /// <summary>
        /// </summary>
        /// <param name="pass"> </param>
        /// <returns> </returns>
        public static byte[] Sha1HashBytes(string pass)
        {
            SHA1 sha = SHA1.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(pass));
        }

#endif

        /// <summary>
        ///   Converts all bytes in the Array to a string representation.
        /// </summary>
        /// <param name="buf"> </param>
        /// <returns> string representation </returns>
        public static string HexToString(byte[] buf)
        {
            var sb = new StringBuilder();
            foreach (byte b in buf)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        #endregion

        #region << SHA1 Hash Compact Framework >>

#if CF
		

    // <summary>
    /// return a SHA1 Hash on PPC and Smartphone
    /// </summary>
    /// <param name="pass"></param>
    /// <returns></returns>
		public static byte[] Sha1Hash(byte[] pass)
		{
			IntPtr hProv;
			bool retVal = WinCeApi.CryptAcquireContext( out hProv, null, null, (int) WinCeApi.SecurityProviderType.RSA_FULL, 0 );
			IntPtr hHash;
			retVal = WinCeApi.CryptCreateHash( hProv, (int) WinCeApi.SecurityProviderType.CALG_SHA1, IntPtr.Zero, 0, out hHash );
			
			byte [] publicKey = pass;
			int publicKeyLen = publicKey.Length;
			retVal = WinCeApi.CryptHashData( hHash, publicKey, publicKeyLen, 0 );
			int bufferLen = 20; // SHA1 size
			byte [] buffer = new byte[bufferLen];
			retVal = WinCeApi.CryptGetHashParam( hHash, (int) WinCeApi.SecurityProviderType.HP_HASHVAL, buffer, ref bufferLen, 0 );
			retVal = WinCeApi.CryptDestroyHash( hHash );
			retVal = WinCeApi.CryptReleaseContext( hProv, 0 );
			
			return buffer;
		}

		/// <summary>
		/// return a SHA1 Hash on PPC and Smartphone
		/// </summary>
		/// <param name="pass"></param>
		/// <returns></returns>
		public static string Sha1Hash(string pass)
		{
			return HexToString(Sha1Hash(System.Text.Encoding.ASCII.GetBytes(pass)));		
		}

		/// <summary>
		/// return a SHA1 Hash on PPC and Smartphone
		/// </summary>
		/// <param name="pass"></param>
		/// <returns></returns>
		public static byte[] Sha1HashBytes(string pass)
		{
			return Sha1Hash(System.Text.Encoding.UTF8.GetBytes(pass));
		}

		/// <summary>
		/// omputes the MD5 hash value for the specified byte array.		
		/// </summary>
		/// <param name="pass">The input for which to compute the hash code.</param>
		/// <returns>The computed hash code.</returns>
		public static byte[] MD5Hash(byte[] pass)
		{
			IntPtr hProv;
			bool retVal = WinCeApi.CryptAcquireContext( out hProv, null, null, (int) WinCeApi.SecurityProviderType.RSA_FULL, 0 );
			IntPtr hHash;
			retVal = WinCeApi.CryptCreateHash( hProv, (int) WinCeApi.SecurityProviderType.CALG_MD5, IntPtr.Zero, 0, out hHash );
			
			byte [] publicKey = pass;
			int publicKeyLen = publicKey.Length;
			retVal = WinCeApi.CryptHashData( hHash, publicKey, publicKeyLen, 0 );
			int bufferLen = 16; // SHA1 size
			byte [] buffer = new byte[bufferLen];
			retVal = WinCeApi.CryptGetHashParam( hHash, (int) WinCeApi.SecurityProviderType.HP_HASHVAL, buffer, ref bufferLen, 0 );
			retVal = WinCeApi.CryptDestroyHash( hHash );
			retVal = WinCeApi.CryptReleaseContext( hProv, 0 );

			return buffer;
		}

#endif

        #endregion
    }
}