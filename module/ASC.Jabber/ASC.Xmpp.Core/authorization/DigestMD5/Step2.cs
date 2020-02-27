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


#region using

using System;
using System.Security.Cryptography;
using System.Text;
using ASC.Xmpp.Core.utils;

#endregion

#if CF
using agsXMPP.util;
#endif

namespace ASC.Xmpp.Core.authorization.DigestMD5
{

    #region usings

    #endregion

    /// <summary>
    ///   Summary description for Step2.
    /// </summary>
    public class Step2 : Step1
    {

        #region Members

        /// <summary>
        /// </summary>
        private string m_Authzid;

        /// <summary>
        /// </summary>
        private string m_Cnonce;

        /// <summary>
        /// </summary>
        private string m_DigestUri;

        /// <summary>
        /// </summary>
        private string m_Nc;

        /// <summary>
        /// </summary>
        private string m_Response;

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        public Step2()
        {
        }

        /// <summary>
        ///   parses a message and returns the step2 object
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="challengeEncoding">challenge encoding </param>
        public Step2(string message)
        {
            try
            {
                int start = 0;
                int end = 0;
                while (start < message.Length)
                {
                    int equalPos = message.IndexOf('=', start);
                    if (equalPos > 0)
                    {
                        // look if the next char is a quote
                        if (message.Substring(equalPos + 1, 1) == "\"")
                        {
                            // quoted value, find the end now
                            end = message.IndexOf('"', equalPos + 2);
                            ParsePair(message.Substring(start, end - start + 1));
                            start = end + 2;
                        }
                        else
                        {
                            // value is not quoted, ends at the next comma or end of string   
                            end = message.IndexOf(',', equalPos + 1);
                            if (end == -1)
                            {
                                end = message.Length;
                            }

                            ParsePair(message.Substring(start, end - start));

                            start = end + 1;
                        }
                    }
                }
            }
            catch
            {
                throw new ChallengeParseException("Unable to parse challenge");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public string Authzid
        {
            get { return m_Authzid; }

            set { m_Authzid = value; }
        }

        /// <summary>
        /// </summary>
        public string Cnonce
        {
            get { return m_Cnonce; }

            set { m_Cnonce = value; }
        }

        /// <summary>
        /// </summary>
        public string DigestUri
        {
            get { return m_DigestUri; }

            set { m_DigestUri = value; }
        }

        /// <summary>
        /// </summary>
        public string Nc
        {
            get { return m_Nc; }

            set { m_Nc = value; }
        }

        /// <summary>
        /// </summary>
        public string Response
        {
            get { return m_Response; }

            set { m_Response = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="username"> </param>
        /// <param name="pwddata"> </param>
        /// <returns> </returns>
        public bool Authorize(string username, string pwddata)
        {
            if (Response == GenerateResponse(username, pwddata, "AUTHENTICATE"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public override string ToString()
        {
            return GenerateMessage();
        }

        /// <summary>
        /// </summary>
        public string GenerateResponse(string username, string password, string method)
        {
            byte[] H1;
            byte[] H2;
            byte[] H3;

            // byte[] temp;
            string A1;
            string A2;
            string A3;
            string p1;
            string p2;

            var sb = new StringBuilder();
            sb.Append(username);
            sb.Append(":");
            sb.Append(Realm);
            sb.Append(":");
            sb.Append(password);

#if !CF
            H1 = new MD5CryptoServiceProvider().ComputeHash(Encoding.GetBytes(sb.ToString()));
#else
    
    // H1 = Encoding.GetBytes(util.Hash.MD5Hash(sb.ToString()));
			H1 = util.Hash.MD5Hash(Encoding.GetBytes(sb.ToString()));
#endif

            sb.Remove(0, sb.Length);
            sb.Append(":");
            sb.Append(Nonce);
            sb.Append(":");
            sb.Append(Cnonce);

            if (m_Authzid != null)
            {
                sb.Append(":");
                sb.Append(m_Authzid);
            }

            A1 = sb.ToString();

            // 			sb.Remove(0, sb.Length);			
            // 			sb.Append(Encoding.Default.GetChars(H1));
            // 			//sb.Append(Encoding.ASCII.GetChars(H1));
            // 			sb.Append(A1);			
            byte[] bA1 = Encoding.ASCII.GetBytes(A1);
            var bH1A1 = new byte[H1.Length + bA1.Length];

            // Array.Copy(H1, bH1A1, H1.Length);
            Array.Copy(H1, 0, bH1A1, 0, H1.Length);
            Array.Copy(bA1, 0, bH1A1, H1.Length, bA1.Length);
#if !CF
            H1 = new MD5CryptoServiceProvider().ComputeHash(bH1A1);

            // Console.WriteLine(util.Hash.HexToString(H1));
#else
    
    // H1 = Encoding.Default.GetBytes(util.Hash.MD5Hash(sb.ToString()));
    // H1 =util.Hash.MD5Hash(Encoding.Default.GetBytes(sb.ToString()));
			H1 =util.Hash.MD5Hash(bH1A1);
#endif
            sb.Remove(0, sb.Length);
            sb.Append(method);
            sb.Append(":");
            sb.Append(m_DigestUri);
            if (Qop.CompareTo("auth") != 0)
            {
                sb.Append(":00000000000000000000000000000000");
            }

            A2 = sb.ToString();
            H2 = Encoding.ASCII.GetBytes(A2);

#if !CF
            H2 = new MD5CryptoServiceProvider().ComputeHash(H2);
#else
    
    // H2 = Encoding.Default.GetBytes(util.Hash.MD5Hash(H2));
			H2 =util.Hash.MD5Hash(H2);
#endif

            // create p1 and p2 as the hex representation of H1 and H2
            p1 = Hash.HexToString(H1).ToLower();
            p2 = Hash.HexToString(H2).ToLower();

            sb.Remove(0, sb.Length);
            sb.Append(p1);
            sb.Append(":");
            sb.Append(Nonce);
            sb.Append(":");
            sb.Append(m_Nc);
            sb.Append(":");
            sb.Append(m_Cnonce);
            sb.Append(":");
            sb.Append(base.Qop);
            sb.Append(":");
            sb.Append(p2);

            A3 = sb.ToString();
#if !CF
            H3 = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(A3));
#else
    
    // H3 = Encoding.Default.GetBytes(util.Hash.MD5Hash(A3));
			H3 =util.Hash.MD5Hash(Encoding.ASCII.GetBytes(A3));
#endif
            return Hash.HexToString(H3).ToLower();
        }

        #endregion

        #region Utility methods

        /// <summary>
        ///   Does the server support Auth?
        /// </summary>
        /// <param name="qop"> </param>
        /// <returns> </returns>
        private bool SupportsAuth(string qop)
        {
            string[] auth = qop.Split(',');

            // This overload was not available in the CF, so updated this to the following
            // bool ret = Array.IndexOf(auth, "auth") < 0 ? false : true;
            bool ret = Array.IndexOf(auth, "auth", auth.GetLowerBound(0), auth.Length) < 0 ? false : true;
            return ret;
        }

        /// <summary>
        /// </summary>
        /// <param name="pair"> </param>
        private void ParsePair(string pair)
        {
            int equalPos = pair.IndexOf("=");
            if (equalPos > 0)
            {
                string key = pair.Substring(0, equalPos).Trim();
                string data;

                // is the value quoted?
                if (pair.Substring(equalPos + 1, 1) == "\"")
                {
                    data = pair.Substring(equalPos + 2, pair.Length - equalPos - 3);
                }
                else
                {
                    data = pair.Substring(equalPos + 1);
                }

                switch (key)
                {
                    case "realm":
                        Realm = data;
                        break;
                    case "nonce":
                        Nonce = data;
                        break;
                    case "cnonce":
                        Cnonce = data;
                        break;
                    case "qop":
                        Qop = data;
                        break;
                    case "username":
                        Username = data;
                        break;
                    case "charset":
                        Charset = data;
                        break;
                    case "algorithm":
                        Algorithm = data;
                        break;
                    case "rspauth":
                        Rspauth = data;
                        break;
                    case "nc":
                        Nc = data;
                        break;
                    case "digest-uri":
                        DigestUri = data;
                        break;
                    case "response":
                        Response = data;
                        break;
                }
            }
        }

        /// <summary>
        /// </summary>
        private void GenerateCnonce()
        {
            // Lenght of the Session ID on bytes,
            // 32 bytes equaly 64 chars
            // 16^64 possibilites for the session IDs (4.294.967.296)
            // This should be unique enough
            int m_lenght = 32;

            RandomNumberGenerator RNG = RandomNumberGenerator.Create();

            var buf = new byte[m_lenght];
            RNG.GetBytes(buf);

            m_Cnonce = Hash.HexToString(buf).ToLower();

            // 			m_Cnonce = "e163ceed6cfbf8c1559a9ff373b292c2f926b65719a67a67c69f7f034c50aba3";
        }

        /// <summary>
        /// </summary>
        private void GenerateNc()
        {
            int nc = 1;
            m_Nc = nc.ToString().PadLeft(8, '0');
        }

        /// <summary>
        /// </summary>
        private void GenerateDigestUri()
        {
            m_DigestUri = "xmpp/" + base.Server;
        }

        // 	HEX( KD ( HEX(H(A1)),
        // 	{
        // 		nonce-value, ":" nc-value, ":",
        // 		cnonce-value, ":", qop-value, ":", HEX(H(A2)) }))
        // 	If authzid is specified, then A1 is
        // 	A1 = { H( { username-value, ":", realm-value, ":", passwd } ),
        // 	":", nonce-value, ":", cnonce-value, ":", authzid-value }
        // 	If authzid is not specified, then A1 is
        // 	A1 = { H( { username-value, ":", realm-value, ":", passwd } ),
        // 	":", nonce-value, ":", cnonce-value }
        // 	where
        // 	passwd   = *OCTET

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        private string GenerateMessage()
        {
            var sb = new StringBuilder();
            sb.Append("username=");
            sb.Append(AddQuotes(Username));
            sb.Append(",");
            sb.Append("realm=");
            sb.Append(AddQuotes(Realm));
            sb.Append(",");
            sb.Append("nonce=");
            sb.Append(AddQuotes(Nonce));
            sb.Append(",");
            sb.Append("cnonce=");
            sb.Append(AddQuotes(Cnonce));
            sb.Append(",");
            sb.Append("nc=");
            sb.Append(Nc);
            sb.Append(",");
            sb.Append("qop=");
            sb.Append(base.Qop);
            sb.Append(",");
            sb.Append("digest-uri=");
            sb.Append(AddQuotes(DigestUri));
            sb.Append(",");
            sb.Append("charset=");
            sb.Append(Charset);
            sb.Append(",");
            sb.Append("response=");
            sb.Append(Response);

            return sb.ToString();
        }

        /// <summary>
        ///   return the given string with quotes
        /// </summary>
        /// <param name="s"> </param>
        /// <returns> </returns>
        private string AddQuotes(string s)
        {
            string quote = "\"";
            return quote + s + quote;
        }

        #endregion
    }
}