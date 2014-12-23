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