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
using System.Configuration;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Web.Studio.Core;

namespace ASC.Mail
{
    public static class Defines
    {
        static Defines()
        {
            DefaultServerLoginDelay = ConfigurationManagerExtension.AppSettings["mail.default-login-delay"] != null
                    ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.default-login-delay"])
                    : 30;

            DefaultServerLoginDelayStr = DefaultServerLoginDelay.ToString();

            SslCertificatesErrorPermit = ConfigurationManagerExtension.AppSettings["mail.certificate-permit"] != null &&
                           Convert.ToBoolean(ConfigurationManagerExtension.AppSettings["mail.certificate-permit"]);

            IsSignalRAvailable = !string.IsNullOrEmpty(ConfigurationManagerExtension.AppSettings["web.hub"]);

            AuthErrorWarningTimeout = ConfigurationManagerExtension.AppSettings["mail.auth-error-warning-in-minutes"] != null
                        ? TimeSpan.FromMinutes(
                            Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.auth-error-warning-in-minutes"]))
                        : TimeSpan.FromHours(1);

            AuthErrorDisableTimeout = ConfigurationManagerExtension.AppSettings["mail.auth-error-disable-mailbox-in-minutes"] != null
                        ? TimeSpan.FromMinutes(
                            Convert.ToInt32(
                                ConfigurationManagerExtension.AppSettings["mail.auth-error-disable-mailbox-in-minutes"]))
                        : TimeSpan.FromDays(3);

            MailDaemonEmail = ConfigurationManagerExtension.AppSettings["mail.daemon-email"] ?? "mail-daemon@onlyoffice.com";

            NeedProxyHttp = SetupInfo.IsVisibleSettings("ProxyHttpContent");

            TcpTimeout = ConfigurationManagerExtension.AppSettings["mail.tcp-timeout"] != null
                    ? Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.tcp-timeout"])
                    : 10000;

            var limit = ConfigurationManagerExtension.AppSettings["mail.recalculate-folders-timeout"];

            RecalculateFoldersTimeout = limit != null ? Convert.ToInt32(limit) : 99999;

            limit = ConfigurationManagerExtension.AppSettings["mail.remove-domain-timeout"];

            RemoveDomainTimeout = limit != null ? Convert.ToInt32(limit) : 99999;

            limit = ConfigurationManagerExtension.AppSettings["mail.remove-mailbox-timeout"];

            RemoveMailboxTimeout = limit != null ? Convert.ToInt32(limit) : 99999;

            var saveFlag = ConfigurationManagerExtension.AppSettings["mail.save-original-message"];

            SaveOriginalMessage = saveFlag != null && Convert.ToBoolean(saveFlag);

            ServerDomainMailboxPerUserLimit = Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.server-mailbox-limit-per-user"] ?? "2");

            ServerDnsSpfRecordValue = ConfigurationManagerExtension.AppSettings["mail.server.spf-record-value"] ?? "v=spf1 +mx ~all";

            ServerDnsMxRecordPriority = Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.server.mx-record-priority"] ?? "0");

            ServerDnsDkimSelector = ConfigurationManagerExtension.AppSettings["mail.server-dkim-selector"] ?? "dkim";

            ServerDnsDomainCheckPrefix = ConfigurationManagerExtension.AppSettings["mail.server-dns-check-prefix"] ?? "onlyoffice-domain";

            var ttl = ConfigurationManagerExtension.AppSettings["mail.server-dns-default-ttl"];

            ServerDnsDefaultTtl = ttl != null ? Convert.ToInt32(ttl) : 300;

            MaximumMessageBodySize = Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.maximum-message-body-size"] ?? "524288");

            var operations = ConfigurationManagerExtension.AppSettings["mail.attachments-group-operations"];

            IsAttachmentsGroupOperationsAvailable = operations != null ? Convert.ToBoolean(operations) : true;

            var linkAvailable = ConfigurationManagerExtension.AppSettings["mail.ms-migration-link-available"];

            IsMSExchangeMigrationLinkAvailable = linkAvailable != null
                ? Convert.ToBoolean(linkAvailable)
                : CoreContext.Configuration.Standalone;

            var timeout = ConfigurationManagerExtension.AppSettings["mail.server-connection-timeout"];
            MailServerConnectionTimeout = timeout != null ? Convert.ToInt32(timeout) : 15000;

            timeout = ConfigurationManagerExtension.AppSettings["mail.server-enable-utf8-timeout"];

            MailServerEnableUtf8Timeout = timeout != null ? Convert.ToInt32(timeout) : 15000;

            timeout = ConfigurationManagerExtension.AppSettings["mail.server-login-timeout"];

            MailServerLoginTimeout = timeout != null ? Convert.ToInt32(timeout) : 30000;

            ApiSchema = ConfigurationManagerExtension.AppSettings["mail.default-api-scheme"] != null ?
                 ConfigurationManagerExtension.AppSettings["mail.default-api-scheme"] == Uri.UriSchemeHttps
                        ? Uri.UriSchemeHttps
                        : Uri.UriSchemeHttp
                        : null;

            ApiPrefix = (ConfigurationManagerExtension.AppSettings["api.url"] ?? "").Trim('~', '/');

            ApiVirtualDirPrefix = (ConfigurationManagerExtension.AppSettings["api.virtual-dir"] ?? "").Trim('/');

            ApiHost = ConfigurationManagerExtension.AppSettings["api.host"];

            ApiPort = ConfigurationManagerExtension.AppSettings["api.port"];

            AutoreplyDaysInterval = Convert.ToInt32(ConfigurationManagerExtension.AppSettings["mail.autoreply-days-interval"] ?? "1");

            var config = ConfigurationManagerExtension.AppSettings["mail.disable-imap-send-sync-servers"] ?? "imap.googlemail.com|imap.gmail.com|imap-mail.outlook.com";

            DisableImapSendSyncServers = string.IsNullOrEmpty(config) ? new List<string>() : config.Split('|').ToList();

            var list = new Dictionary<string, string>
                {
                    {".outlook.", "office365.com"},
                    {".google.", "gmail.com"},
                    {".yandex.", "yandex.ru"},
                    {".mail.ru", "mail.ru"},
                    {".yahoodns.", "yahoo.com"}
                };

            try
            {
                if (!string.IsNullOrEmpty(ConfigurationManagerExtension.AppSettings["mail.busines-vendors-mx-domains"]))
                // ".outlook.:office365.com|.google.:gmail.com|.yandex.:yandex.ru|.mail.ru:mail.ru|.yahoodns.:yahoo.com"
                {
                    list = ConfigurationManagerExtension.AppSettings["mail.busines-vendors-mx-domains"]
                        .Split('|')
                        .Select(s => s.Split(':'))
                        .ToDictionary(s => s[0], s => s[1]);
                }
            }
            catch (Exception ex)
            {
                // skip error
            }

            MxToDomainBusinessVendorsList = list;

            limit = ConfigurationManagerExtension.AppSettings["mail.operations-limit"];

            MailOperationsLimit = limit != null ? Convert.ToInt32(limit) : 100;
        }

        public const string MODULE_NAME = "mailaggregator";
        public const string MD5_EMPTY = "d41d8cd98f00b204e9800998ecf8427e";
        public const int SHARED_TENANT_ID = -1;
        public const int UNUSED_DNS_SETTING_DOMAIN_ID = -1;
        public const string ICAL_REQUEST = "REQUEST";
        public const string ICAL_REPLY = "REPLY";
        public const string ICAL_CANCEL = "CANCEL";
        public const string GOOGLE_HOST = "gmail.com";
        public const int HARDCODED_LOGIN_TIME_FOR_MS_MAIL = 900;
        public const string IMAP = "imap";
        public const string POP3 = "pop3";
        public const string SMTP = "smtp";
        public const string SSL = "ssl";
        public const string START_TLS = "starttls";
        public const string PLAIN = "plain";
        public const string OAUTH2 = "oauth2";
        public const string PASSWORD_CLEARTEXT = "password-cleartext";
        public const string PASSWORD_ENCRYPTED = "password-encrypted";
        public const string NONE = "none";
        public const string GET_IMAP_POP_SETTINGS = "get_imap_pop_settings";
        public const string GET_IMAP_SERVER = "get_imap_server";
        public const string GET_IMAP_SERVER_FULL = "get_imap_server_full";
        public const string GET_POP_SERVER = "get_pop_server";
        public const string GET_POP_SERVER_FULL = "get_pop_server_full";
        public const string MAIL_QUOTA_TAG = "666ceac1-4532-4f8c-9cba-8f510eca2fd1";
        public const long ATTACHMENTS_TOTAL_SIZE_LIMIT = 25 * 1024 * 1024; // 25 megabytes
        public const string ASCENDING = "ascending";
        public const string DESCENDING = "descending";
        public const string ORDER_BY_DATE_SENT = "datesent";
        public const string ORDER_BY_DATE_CHAIN = "chaindate";
        public const string ORDER_BY_SENDER = "sender";
        public const string ORDER_BY_SUBJECT = "subject";
        public const string CONNECTION_STRING_NAME = "mail";
        public const string DNS_DEFAULT_ORIGIN = "@";
        public const string ARCHIVE_NAME = "download.zip";
        public static readonly DateTime BaseJsDateTime = new DateTime(1970, 1, 1);

        public enum TariffType
        {
            Active = 0,
            Overdue,
            LongDead
        };

        public static int DefaultServerLoginDelay
        {
            get; private set;
        }

        public static string DefaultServerLoginDelayStr
        {
            get; private set;
        }

        public static readonly DateTime MinBeginDate = new DateTime(1975, 1, 1, 0, 0, 0);

        public static bool SslCertificatesErrorPermit
        {
            get; private set;
        }

        public static bool IsSignalRAvailable
        {
            get; private set;
        }

        public static TimeSpan AuthErrorWarningTimeout
        {
            get; private set;
        }

        public static TimeSpan AuthErrorDisableTimeout
        {
            get; private set;
        }

        public static string MailDaemonEmail
        {
            get; private set;
        }

        public static bool NeedProxyHttp
        {
            get; private set;
        }

        /// <summary>
        /// Test mailbox connection tcp timeout (10 sec is default)
        /// </summary>
        public static int TcpTimeout
        {
            get; private set;
        }

        public static int RecalculateFoldersTimeout
        {
            get; private set;
        }

        public static int RemoveDomainTimeout
        {
            get; private set;
        }

        public static int RemoveMailboxTimeout
        {
            get; private set;
        }

        public static bool SaveOriginalMessage
        {
            get; private set;
        }

        public static int ServerDomainMailboxPerUserLimit
        {
            get; private set;
        }

        public static string ServerDnsSpfRecordValue
        {
            get; private set;
        }

        public static int ServerDnsMxRecordPriority
        {
            get; private set;
        }

        public static string ServerDnsDkimSelector
        {
            get; private set;
        }

        public static string ServerDnsDomainCheckPrefix
        {
            get; private set;
        }

        public static int ServerDnsDefaultTtl
        {
            get; private set;
        }

        public static int MaximumMessageBodySize
        {
            get; private set;
        }

        public static bool IsAttachmentsGroupOperationsAvailable
        {
            get; private set;
        }

        public static bool IsMSExchangeMigrationLinkAvailable
        {
            get; private set;
        }

        public static int MailServerConnectionTimeout
        {
            get; private set;
        }

        public static int MailServerEnableUtf8Timeout
        {
            get; private set;
        }

        public static int MailServerLoginTimeout
        {
            get; private set;
        }

        public static string ApiSchema { get; private set; }

        public static string DefaultApiSchema
        {
            get
            {
                if (!string.IsNullOrEmpty(ApiSchema))
                {
                    return ApiSchema;
                }

                return HttpContext.Current == null
                    ? Uri.UriSchemeHttp
                    : HttpContext.Current.Request.GetUrlRewriter().Scheme;
            }
        }

        public static string ApiPrefix
        {
            get; private set;
        }

        public static string ApiVirtualDirPrefix
        {
            get; private set;
        }

        public static string ApiHost
        {
            get; private set;
        }

        public static string ApiPort
        {
            get; private set;
        }
        public static int AutoreplyDaysInterval
        {
            get; private set;
        }

        public static List<string> DisableImapSendSyncServers
        {
            get; private set;
        }

        public static Dictionary<string, string> MxToDomainBusinessVendorsList
        { 
            get; private set; 
        }

        public static int MailOperationsLimit
        {
            get; private set;
        }
    }
}
