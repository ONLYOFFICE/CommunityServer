/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.AUTH
{
    #region usings

    using System;
    using System.Security.Cryptography;
    using System.Text;

    #endregion

    /// <summary>
    /// Provides helper methods for authentications(APOP,CRAM-MD5,DIGEST-MD5).
    /// </summary>
    [Obsolete]
    public class AuthHelper
    {
        #region Methods

        /// <summary>
        /// Calculates APOP authentication compare value.
        /// </summary>
        /// <param name="password">Password.</param>
        /// <param name="passwordTag">Password tag.</param>
        /// <returns>Returns value what must be used for comparing passwords.</returns>
        public static string Apop(string password, string passwordTag)
        {
            /* RFC 1939 7. APOP
			 *
			 * value = Hex(Md5(passwordTag + password))
			*/

            return Hex(Md5(passwordTag + password));
        }

        /// <summary>
        /// Calculates CRAM-MD5 authentication compare value.
        /// </summary>
        /// <param name="password">Password.</param>
        /// <param name="hashKey">Hash calculation key</param>
        /// <returns>Returns value what must be used for comparing passwords.</returns>
        public static string Cram_Md5(string password, string hashKey)
        {
            /* RFC 2195 AUTH CRAM-MD5
			 * 
			 * value = Hex(HmacMd5(hashKey,password))
			*/

            return Hex(HmacMd5(hashKey, password));
        }

        /// <summary>
        /// Calculates DIGEST-MD5 authentication compare value.
        /// </summary>
        /// <param name="client_server">Specifies if client or server value calculated. 
        /// Client and server has diffrent calculation method.</param>
        /// <param name="realm">Use domain or machine name for this.</param>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <param name="nonce">Server password tag.</param>
        /// <param name="cnonce">Client password tag.</param>
        /// <param name="digest_uri"></param>
        /// <returns>Returns value what must be used for comparing passwords.</returns>
        public static string Digest_Md5(bool client_server,
                                        string realm,
                                        string userName,
                                        string password,
                                        string nonce,
                                        string cnonce,
                                        string digest_uri)
        {
            /* RFC 2831 AUTH DIGEST-MD5
			 * 
			 * qop = "auth";      // We support auth only auth-int and auth-conf isn't supported
			 * nc  = "00000001"
			 * 
			 * A1 = Md5(userName + ":" + realm + ":" + passw) + ":" + nonce + ":" + cnonce
			 * A2(client response) = "AUTHENTICATE:" + digest_uri
			 * A2(server response) = ":" + digest_uri
			 * 
			 * resp-value = Hex(Md5(Hex(Md5(a1)) + ":" + (nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + Hex(Md5(a2)))));
			*/

            //	string realm      = "elwood.innosoft.com";
            //	string userName   = "chris";			
            //	string passw      = "secret";
            //	string nonce      = "OA6MG9tEQGm2hh";
            //	string cnonce     = "OA6MHXh6VqTrRk";
            //	string digest_uri = "imap/elwood.innosoft.com";

            string qop = "auth";
            string nc = "00000001";
            //****
            string a1 = Md5(userName + ":" + realm + ":" + password) + ":" + nonce + ":" + cnonce;
            string a2 = "";
            if (client_server)
            {
                a2 = "AUTHENTICATE:" + digest_uri;
            }
            else
            {
                a2 = ":" + digest_uri;
            }

            return
                Hex(
                    Md5(Hex(Md5(a1)) + ":" +
                        (nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + Hex(Md5(a2)))));
        }

        /// <summary>
        /// Creates AUTH Digest-md5 server response what server must send to client.
        /// </summary>
        /// <param name="realm">Use domain or machine name for this.</param>
        /// <param name="nonce">Server password tag. Random hex string is suggested.</param>
        /// <returns></returns>
        public static string Create_Digest_Md5_ServerResponse(string realm, string nonce)
        {
            return "realm=\"" + realm + "\",nonce=\"" + nonce + "\",qop=\"auth\",algorithm=md5-sess";
        }

        /// <summary>
        /// Generates random nonce value.
        /// </summary>
        /// <returns></returns>
        public static string GenerateNonce()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
        }

        /// <summary>
        /// Calculates keyed md5 hash from specifieed text and with specified hash key.
        /// </summary>
        /// <param name="hashKey"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string HmacMd5(string hashKey, string text)
        {
            HMACMD5 kMd5 = new HMACMD5(Encoding.Default.GetBytes(text));
            return Encoding.Default.GetString(kMd5.ComputeHash(Encoding.ASCII.GetBytes(hashKey)));
        }

        /// <summary>
        /// Calculates md5 hash from specified string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Md5(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(text));

            return Encoding.Default.GetString(hash);
        }

        /// <summary>
        /// Converts specified string to hexa string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns> 
        public static string Hex(string text)
        {
            return BitConverter.ToString(Encoding.Default.GetBytes(text)).ToLower().Replace("-", "");
        }

        /// <summary>
        /// Encodes specified string to base64 string.
        /// </summary>
        /// <param name="text">Text to encode.</param>
        /// <returns>Returns encoded string.</returns>
        public static string Base64en(string text)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(text));
        }

        /// <summary>
        /// Decodes specified base64 string.
        /// </summary>
        /// <param name="text">Base64 string to decode.</param>
        /// <returns>Returns decoded string.</returns>
        public static string Base64de(string text)
        {
            return Encoding.Default.GetString(Convert.FromBase64String(text));
        }

        #endregion
    }
}