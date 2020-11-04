/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
    /// Implements "CRAM-MD5" authenticaiton. Defined in RFC 2195.
    /// </summary>
    public class AUTH_SASL_ServerMechanism_CramMd5 : AUTH_SASL_ServerMechanism
    {
        #region Events

        /// <summary>
        /// Is called when authentication mechanism needs to get user info to complete atuhentication.
        /// </summary>
        public event EventHandler<AUTH_e_UserInfo> GetUserInfo = null;

        #endregion

        #region Members

        private readonly bool m_RequireSSL;
        private bool m_IsAuthenticated;
        private bool m_IsCompleted;
        private string m_Key = "";
        private int m_State;
        private string m_UserName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets if the authentication exchange has completed.
        /// </summary>
        public override bool IsCompleted
        {
            get { return m_IsCompleted; }
        }

        /// <summary>
        /// Gets if user has authenticated sucessfully.
        /// </summary>
        public override bool IsAuthenticated
        {
            get { return m_IsAuthenticated; }
        }

        /// <summary>
        /// Returns always "CRAM-MD5".
        /// </summary>
        public override string Name
        {
            get { return "CRAM-MD5"; }
        }

        /// <summary>
        /// Gets if specified SASL mechanism is available only to SSL connection.
        /// </summary>
        public override bool RequireSSL
        {
            get { return m_RequireSSL; }
        }

        /// <summary>
        /// Gets user login name.
        /// </summary>
        public override string UserName
        {
            get { return m_UserName; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="requireSSL">Specifies if this mechanism is available to SSL connections only.</param>
        public AUTH_SASL_ServerMechanism_CramMd5(bool requireSSL)
        {
            m_RequireSSL = requireSSL;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Continues authentication process.
        /// </summary>
        /// <param name="clientResponse">Client sent SASL response.</param>
        /// <returns>Retunrns challange response what must be sent to client or null if authentication has completed.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>clientResponse</b> is null reference.</exception>
        public override string Continue(string clientResponse)
        {
            if (clientResponse == null)
            {
                throw new ArgumentNullException("clientResponse");
            }

            /* RFC 2195 2. Challenge-Response Authentication Mechanism.
                The authentication type associated with CRAM is "CRAM-MD5".

                The data encoded in the first ready response contains an
                presumptively arbitrary string of random digits, a timestamp, and the
                fully-qualified primary host name of the server.  The syntax of the
                unencoded form must correspond to that of an RFC 822 'msg-id'
                [RFC822] as described in [POP3].

                The client makes note of the data and then responds with a string
                consisting of the user name, a space, and a 'digest'.  The latter is
                computed by applying the keyed MD5 algorithm from [KEYED-MD5] where
                the key is a shared secret and the digested text is the timestamp
                (including angle-brackets).

                This shared secret is a string known only to the client and server.
                The `digest' parameter itself is a 16-octet value which is sent in
                hexadecimal format, using lower-case ASCII characters.

                When the server receives this client response, it verifies the digest
                provided.  If the digest is correct, the server should consider the
                client authenticated and respond appropriately.
              
                Example:
                    The examples in this document show the use of the CRAM mechanism with
                    the IMAP4 AUTHENTICATE command [IMAP-AUTH].  The base64 encoding of
                    the challenges and responses is part of the IMAP4 AUTHENTICATE
                    command, not part of the CRAM specification itself.

                    S: * OK IMAP4 Server
                    C: A0001 AUTHENTICATE CRAM-MD5
                    S: + PDE4OTYuNjk3MTcwOTUyQHBvc3RvZmZpY2UucmVzdG9uLm1jaS5uZXQ+
                    C: dGltIGI5MTNhNjAyYzdlZGE3YTQ5NWI0ZTZlNzMzNGQzODkw
                    S: A0001 OK CRAM authentication successful

                    In this example, the shared secret is the string
                    'tanstaaftanstaaf'.  Hence, the Keyed MD5 digest is produced by
                    calculating

                    MD5((tanstaaftanstaaf XOR opad),
                        MD5((tanstaaftanstaaf XOR ipad),
                        <1896.697170952@postoffice.reston.mci.net>))

                    where ipad and opad are as defined in the keyed-MD5 Work in
                    Progress [KEYED-MD5] and the string shown in the challenge is the
                    base64 encoding of <1896.697170952@postoffice.reston.mci.net>. The
                    shared secret is null-padded to a length of 64 bytes. If the
                    shared secret is longer than 64 bytes, the MD5 digest of the
                    shared secret is used as a 16 byte input to the keyed MD5
                    calculation.

                    This produces a digest value (in hexadecimal) of

                        b913a602c7eda7a495b4e6e7334d3890

                    The user name is then prepended to it, forming

                        tim b913a602c7eda7a495b4e6e7334d3890

                    Which is then base64 encoded to meet the requirements of the IMAP4
                    AUTHENTICATE command (or the similar POP3 AUTH command), yielding

                    dGltIGI5MTNhNjAyYzdlZGE3YTQ5NWI0ZTZlNzMzNGQzODkw
            */

            if (m_State == 0)
            {
                m_State++;
                m_Key = "<" + Guid.NewGuid() + "@host" + ">";

                return m_Key;
            }
            else
            {
                // Parse client response. response = userName SP hash.
                string[] user_hash = clientResponse.Split(' ');
                if (user_hash.Length == 2 && !string.IsNullOrEmpty(user_hash[0]))
                {
                    m_UserName = user_hash[0];
                    AUTH_e_UserInfo result = OnGetUserInfo(user_hash[0]);
                    if (result.UserExists)
                    {
                        // hash = Hex(HmacMd5(hashKey,password))
                        string hash = Net_Utils.Hex(HmacMd5(m_Key, result.Password));
                        if (hash == user_hash[1])
                        {
                            m_IsAuthenticated = true;
                        }
                    }
                }

                m_IsCompleted = true;
            }

            return null;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Calculates keyed md5 hash from specifieed text and with specified hash key.
        /// </summary>
        /// <param name="hashKey">MD5 key.</param>
        /// <param name="text">Text to hash.</param>
        /// <returns>Returns MD5 hash.</returns>
        private string HmacMd5(string hashKey, string text)
        {
            HMACMD5 kMd5 = new HMACMD5(Encoding.Default.GetBytes(text));
            return Encoding.Default.GetString(kMd5.ComputeHash(Encoding.ASCII.GetBytes(hashKey)));
        }

        /// <summary>
        /// Raises <b>GetUserInfo</b> event.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>Returns specified user info.</returns>
        private AUTH_e_UserInfo OnGetUserInfo(string userName)
        {
            AUTH_e_UserInfo retVal = new AUTH_e_UserInfo(userName);

            if (GetUserInfo != null)
            {
                GetUserInfo(this, retVal);
            }

            return retVal;
        }

        #endregion
    }
}