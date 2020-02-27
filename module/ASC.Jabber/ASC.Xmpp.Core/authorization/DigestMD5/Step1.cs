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
using System.Text;

#endregion

namespace ASC.Xmpp.Core.authorization.DigestMD5
{

    #region usings

    #endregion

    /// <summary>
    ///   Summary description for Step1.
    /// </summary>
    public class Step1 : Mechanism
    {
        #region Members

        /// <summary>
        /// </summary>
        private string m_Algorithm;

        /// <summary>
        /// </summary>
        private string m_Charset = "utf-8";

        /// <summary>
        /// </summary>
        private string m_Nonce;

        /// <summary>
        /// </summary>
        private string m_Qop; // 			= "auth";		

        /// <summary>
        /// </summary>
        private string m_Realm;

        /// <summary>
        /// </summary>
        private string m_Rspauth;

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        public Step1()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="message"> </param>
        public Step1(string message)
        {
            Parse(message);
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public string Algorithm
        {
            get { return m_Algorithm; }

            set { m_Algorithm = value; }
        }

        /// <summary>
        /// </summary>
        public string Charset
        {
            get { return m_Charset; }

            set { m_Charset = value; }
        }

        public Encoding Encoding
        {
            get
            {
                try
                {
                    return Encoding.GetEncoding(Charset);
                }
                catch (Exception)
                {
                    return Encoding.UTF8;
                }
            }
        }

        /// <summary>
        /// </summary>
        public string Nonce
        {
            get { return m_Nonce; }

            set { m_Nonce = value; }
        }

        /// <summary>
        /// </summary>
        public string Qop
        {
            get { return m_Qop; }

            set { m_Qop = value; }
        }

        /// <summary>
        /// </summary>
        public string Realm
        {
            get { return m_Realm; }

            set { m_Realm = value; }
        }

        /// <summary>
        /// </summary>
        public string Rspauth
        {
            get { return m_Rspauth; }

            set { m_Rspauth = value; }
        }

        #endregion

        public override void Init()
        {            
        }

        public override void Parse(utils.Xml.Dom.Node e)
        {            
        }

        #region Utility methods

        /// <summary>
        /// </summary>
        /// <param name="message"> </param>
        /// <exception cref="ChallengeParseException"></exception>
        private void Parse(string message)
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

        /// <summary>
        /// </summary>
        /// <param name="pair"> </param>
        private void ParsePair(string pair)
        {
            int equalPos = pair.IndexOf("=");
            if (equalPos > 0)
            {
                string key = pair.Substring(0, equalPos);
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
                        m_Realm = data;
                        break;
                    case "nonce":
                        m_Nonce = data;
                        break;
                    case "qop":
                        m_Qop = data;
                        break;
                    case "charset":
                        m_Charset = data;
                        break;
                    case "algorithm":
                        m_Algorithm = data;
                        break;
                    case "rspauth":
                        m_Rspauth = data;
                        break;
                }
            }
        }

        #endregion

        // Mechanism 

        /*
            nonce="deqOGux/N6hDPtf9vkGMU5Vzae+zfrqpBIvh6LovbBM=",
            realm="amessage.de",
            qop="auth,auth-int,auth-conf",
            cipher="rc4-40,rc4-56,rc4,des,3des",
            maxbuf=1024,
            charset=utf-8,
            algorithm=md5-sess
        */

        #region Nested type: ChallengeParseException

        /// <summary>
        ///   Exception occurs when we were unable to parse the challenge
        /// </summary>
        public class ChallengeParseException : Exception
        {
            #region Constructor

            /// <summary>
            /// </summary>
            /// <param name="message"> </param>
            public ChallengeParseException(string message)
                : base(message)
            {
            }

            #endregion
        }

        #endregion
    }
}