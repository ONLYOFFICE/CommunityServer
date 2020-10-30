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


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Runtime.Serialization;
using ASC.Common.Logging;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Mail.Authorization;
using ASC.Mail.Data.Imap;
using ASC.Mail.Enums;
using ASC.Mail.Utils;
using Newtonsoft.Json;

namespace ASC.Mail.Data.Contracts
{
    [DataContract(Namespace = "")]
    public class MailBoxData : IEquatable<MailBoxData>
    {
        public enum AuthProblemType
        {
            NoProblems = 0,
            ConnectError = 1,
            TooManyErrors = 2
        }

        public int TenantId { get; set; }

        [DataMember(Name = "id")]
        public int MailBoxId { get; set; }

        private string _userId;

        public string UserId
        {
            get { return _userId; }
            set { _userId = value.ToLowerInvariant(); }
        }

        [IgnoreDataMember]
        public MailAddress EMail { get; set; }

        [DataMember(Name = "email")]
        public string EMailView
        {
            get { return EMail == null ? string.Empty : EMail.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    EMail = new MailAddress(value);
                }
            }
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "account")]
        public string Account { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "server")]
        public string Server { get; set; }

        [DataMember(Name = "smtp_server")]
        public string SmtpServer { get; set; }

        public int SmtpPort
        {
            get
            {
                int port;
                if (!string.IsNullOrEmpty(SmtpPortStr) && int.TryParse(SmtpPortStr, out port))
                {
                    return port;
                }
                return SmtpEncryption != EncryptionType.None ? WellKnownPorts.SMTP_SSL : WellKnownPorts.SMTP;
            }
            set { SmtpPortStr = value.ToString(CultureInfo.InvariantCulture); }
        }

        [DataMember(Name = "smtp_port")]
        public string SmtpPortStr { get; set; }

        [DataMember(Name = "smtp_account")]
        public string SmtpAccount { get; set; }

        [DataMember(Name = "smtp_password")]
        public string SmtpPassword { get; set; }

        [DataMember(Name = "smtp_auth")]
        public bool SmtpAuth {
            get { return SmtpAuthentication != SaslMechanism.None; }
        }

        public int Port
        {
            get
            {
                int port;
                if (!string.IsNullOrEmpty(PortStr) && int.TryParse(PortStr, out port))
                {
                    return port;
                }
                return Encryption != EncryptionType.None ? WellKnownPorts.POP3_SSL : WellKnownPorts.POP3;
            }
            set { PortStr = value.ToString(CultureInfo.InvariantCulture); }
        }

        [DataMember(Name = "port")]
        public string PortStr { get; set; }

        [DataMember(Name = "incoming_encryption_type")]
        public EncryptionType Encryption { get; set;}

        [DataMember(Name = "outcoming_encryption_type")]
        public EncryptionType SmtpEncryption { get; set; }

        [DataMember(Name = "auth_type_in")]
        public SaslMechanism Authentication { get; set; }

        [DataMember(Name = "auth_type_smtp")]
        public SaslMechanism SmtpAuthentication { get; set; }

        public long Size { get; set; }

        public int MessagesCount { get; set; }

        public bool Enabled { get; set; }
        public bool Active { get; set; }

        public bool QuotaError { get; set; }
        public bool QuotaErrorChanged { get; set; }

        public DateTime? AuthErrorDate { get; set; }

        /// <summary>
        /// This value is the initialized while testing a created mail account. 
        /// It is obtained from "LOGIN-DELAY" tag of CAPA command
        /// Measured in seconds
        /// </summary>
        public int ServerLoginDelay { get; set; }

        /// <summary>
        /// Limiting the period of time when limiting download emails
        /// Measured in seconds
        /// </summary>
        public Int64 MailLimitedTimeDelta { get; set; }

        /// <summary>
        /// Begin timestamp loading mails
        /// </summary>
        public DateTime MailBeginTimestamp { get; set; }

        /// <summary>
        /// This default value is assumed based on experiments on the following mail servers: qip.ru, 
        /// </summary>
// ReSharper disable InconsistentNaming
        public const int DefaultServerLoginDelay = 30;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Limiting the period of time when limiting download emails
        /// </summary>
// ReSharper disable InconsistentNaming
        public const Int64 DefaultMailLimitedTimeDelta = 25920000000000; // 30 days;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Default begin timestamp loading emails
        /// </summary>
// ReSharper disable InconsistentNaming
        public const Int64 DefaultMailBeginTimestamp = 622933632000000000; // 01/01/1975 00:00:00
// ReSharper restore InconsistentNaming

        public int AuthorizeTimeoutInMilliseconds
        {
            get { return 10000; }
        }

        private string _oAuthToken;

        private OAuth20Token _token;

        [DataMember(Name = "imap")]
        public bool Imap { get; set; }

        [DataMember(Name = "begin_date")]
        public DateTime BeginDate { get; set; }

        [DataMember(Name = "is_oauth")]
        public bool IsOAuth {
            get { return !string.IsNullOrEmpty(_oAuthToken); }
        }

        [IgnoreDataMember]
        public byte OAuthType { get; set; }

        [IgnoreDataMember]
        public string OAuthToken {
            get { return _oAuthToken; }
            set
            {
                _oAuthToken = value;

                try
                {
                    _token = OAuth20Token.FromJson(_oAuthToken);
                }
                catch (Exception)
                {
                    try
                    {
                        if((AuthorizationServiceType)OAuthType != AuthorizationServiceType.Google)
                            return;

                        // If it is old refresh token then change to oAuthToken
                        var auth = new GoogleOAuth2Authorization(new NullLog());
                        _token = auth.RequestAccessToken(_oAuthToken);

                        AccessTokenRefreshed = true;
                    }
                    catch (Exception)
                    {
                        // skip
                    }
                }
            }
        }

        [IgnoreDataMember]
        public string AccessToken
        {
            get
            {
                if (_token == null)
                    throw new Exception("Cannot create OAuth session with given token");

                if (!_token.IsExpired)
                    return _token.AccessToken;

                switch ((AuthorizationServiceType)OAuthType)
                {
                    case AuthorizationServiceType.Google:
                        _token = OAuth20TokenHelper.RefreshToken<GoogleLoginProvider>(_token);
                        break;
                    case AuthorizationServiceType.None:
                        _token = OAuth20TokenHelper.RefreshToken("", _token);
                        break;
                    default:
                        _token = OAuth20TokenHelper.RefreshToken("", _token);
                        break;
                }

                OAuthToken = _token.ToJson();

                AccessTokenRefreshed = true;

                return _token.AccessToken;
            }
        }

        [IgnoreDataMember]
        public bool AccessTokenRefreshed { get; set; }

        [DataMember(Name = "restrict")]
        public bool Restrict {
            get { return !(BeginDate.Equals(MailBeginTimestamp)); }
        }

        public bool ImapFolderChanged { get; set; }

        public bool BeginDateChanged { get; set; }

        public Dictionary<string, ImapFolderUids> ImapIntervals { get; set; }

        public int SmtpServerId { get; set; }

        public int InServerId { get; set; }

        public string ImapIntervalsJson
        {
            get
            {
                var contentJson = JsonConvert.SerializeObject(ImapIntervals);
                return contentJson;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                try
                {
                    ImapIntervals = MailUtil.ParseImapIntervals(value);
                }
                catch (Exception)
                {
                    // skip
                }                
            }
        }

        [DataMember(Name = "email_in_folder")]
        public string EMailInFolder { get; set; }

        [DataMember(Name = "is_teamlab")]
        public bool IsTeamlab { get; set; }

        public bool IsRemoved { get; set; }

        [IgnoreDataMember]
        public MailSignatureData MailSignature { get; set; }

        [IgnoreDataMember]
        public MailAutoreplyData MailAutoreply { get; set; }

        [IgnoreDataMember]
        public List<string> MailAutoreplyHistory { get; set; }

        [IgnoreDataMember]
        public List<MailAddressInfo> Aliases { get; set; }

        [IgnoreDataMember]
        public List<MailAddressInfo> Groups { get; set; }

        [IgnoreDataMember]
        public DateTime? LastSignalrNotify { get; set; }

        [IgnoreDataMember]
        public bool LastSignalrNotifySkipped { get; set; }

        public MailBoxData()
        {
            ServerLoginDelay = DefaultServerLoginDelay; //This value can be changed in test mailbox connection
            ImapIntervals = new Dictionary<string, ImapFolderUids>();
        }

        public MailBoxData(int tenant, string user, int mailboxId, string name,
            MailAddress email, string account, string password, string server,
            EncryptionType encryption, SaslMechanism authentication, bool imap,
            string smtpAccount, string smtpPassword, string smtpServer, 
            EncryptionType smtpEncryption, SaslMechanism smtpAuthentication,
            byte oAuthType, string oAuthToken)
        {
            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
            if (email == null) throw new ArgumentNullException("email");
            if (string.IsNullOrEmpty(account)) throw new ArgumentNullException("account");
            if (string.IsNullOrEmpty(server)) throw new ArgumentNullException("server");
            if (!server.Contains(":")) throw new FormatException("Invalid server string format is <server:port>");
            if (!smtpServer.Contains(":")) throw new FormatException("Invalid smtp server string format is <server:port>");

            TenantId = tenant;
            UserId = user;
            MailBoxId = mailboxId;

            Name = name;
            EMail = email;

            Account = account;
            Password = password;
            Server = server.Split(':')[0];
            Port = int.Parse(server.Split(':')[1]);
            Encryption = encryption;
            Authentication = authentication;
            Imap = imap;

            SmtpAccount = smtpAccount;
            SmtpPassword = smtpPassword;
            SmtpServer = smtpServer.Split(':')[0];
            SmtpPort = int.Parse(smtpServer.Split(':')[1]);
            SmtpEncryption = smtpEncryption;
            SmtpAuthentication = smtpAuthentication;

            OAuthType = oAuthType;
            OAuthToken = oAuthToken;

            MailLimitedTimeDelta = DefaultMailLimitedTimeDelta;
            MailBeginTimestamp = new DateTime(DefaultMailBeginTimestamp);

            ServerLoginDelay = DefaultServerLoginDelay;
            BeginDate = MailBeginTimestamp;
           
            ImapIntervals = new Dictionary<string, ImapFolderUids>();
        }

        public override string ToString()
        {
            return EMail.ToString();
        }

        public override int GetHashCode()
        {
            return (TenantId.GetHashCode() + UserId.GetHashCode()) ^ EMail.Address.ToLowerInvariant().GetHashCode();
        }

        public bool Equals(MailBoxData other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;

            return
                TenantId == other.TenantId &&
                string.Compare(UserId, other.UserId, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                string.Compare(EMail.Address, other.EMail.Address, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || Equals(obj as MailBoxData);
        }

        //TODO: Remove this struct
        public struct MailboxInfo
        {
            public FolderType folder_id;
            public bool skip;
        }
    }
}
