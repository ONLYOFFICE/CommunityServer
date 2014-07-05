// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Contains several static methods providing cryptographic digesting and transformations.
    /// </summary>
    public abstract class Crypto
    {
        /// <summary>
        /// Digests the given string using the MD5 algorithm.
        /// </summary>
        /// <param name="data">The data to be digested.</param>
        /// <remarks>This method is used for APOP authentication.</remarks>
        /// <returns>A 16 bytes digest representing the data.</returns>
        /// <example>
        /// The example below illustrates the use of this method.
        /// 
        /// <code>
        /// C#
        /// 
        /// string data = "ActiveMail rocks ! Let's see how this string is digested...";
        /// string digest = Crypto.MD5Digest(data);
        /// </code>
        /// 
        /// digest returns 3ff3501885f8602c4d8bf7edcd2ceca1
        /// 
        /// Digesting is used to check data equivalence.
        /// Different data result in different digests.
        /// </example>
        public static string MD5Digest(string data)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bufe = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(data));
            return System.BitConverter.ToString(bufe).ToLower().Replace("-","");
        }
        /// <summary>
        /// Applies the HMAC-MD5 keyed algorithm to the given string using the given key.
        /// </summary>
        /// <param name="key">The key to be used.</param>
        /// <param name="data">The data to be digested.</param>
        /// <remarks>This method is used for CRAM-MD5 authentication.</remarks>
        /// <returns>The transformed data as a 16 bytes digest.</returns>
        /// <example>
        /// The example below illustrates the use of this method.
        /// 
        /// <code>
        /// C#
        /// 
        /// string key = "key";
        /// string data = "ActiveMail rocks ! Let's see how this string is digested...";
        /// string digest = Crypto.HMACMD5Digest(key,data);
        /// </code>
        /// 
        /// digest returns 5db4f178a3ff817a9bc1092a2bcdda24
        /// 
        /// Digesting is used to check data equivalence.
        /// Different data result in different digests.
        /// </example>
        public static string HMACMD5Digest(string key, string data)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            if(key.Length>64) key = ActiveUp.Net.Mail.Crypto.MD5Digest(key);
            key = key.PadRight(64,'\0');
            byte[] bkey = System.Text.Encoding.ASCII.GetBytes(key);
            for(int i=0;i<64;i++) bkey[i] ^= 0x36;
            byte[] bdata = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] bresult = new byte[bdata.Length+64];
            for(int i=0;i<64;i++) bresult[i] = bkey[i];
            for(int i=64;i<bresult.Length;i++) bresult[i] = bdata[i-64];
            byte[] buf = md5.ComputeHash(bresult);
            bkey = System.Text.Encoding.ASCII.GetBytes(key);
            for(int i=0;i<64;i++) bkey[i] ^= 0x5C;
            byte[] bres = new byte[buf.Length+64];
            for(int i=0;i<64;i++) bres[i] = bkey[i];
            for(int i=64;i<bres.Length;i++) bres[i] = buf[i-64];;
            return System.BitConverter.ToString(md5.ComputeHash(bres)).ToLower().Replace("-","");
        }
    }
}
