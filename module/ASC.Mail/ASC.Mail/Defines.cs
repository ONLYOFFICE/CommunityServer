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
using System.Configuration;
using System.Web;
using ASC.Web.Studio.Core;

namespace ASC.Mail
{
    public class Defines
    {
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

        public const long ATTACHMENTS_TOTAL_SIZE_LIMIT = 25*1024*1024; // 25 megabytes

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
            get
            {
                return ConfigurationManager.AppSettings["mail.default-login-delay"] != null
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["mail.default-login-delay"])
                    : 30;
            }
        }

        public static string DefaultServerLoginDelayStr
        {
            get { return DefaultServerLoginDelay.ToString(); }
        }

        public static readonly DateTime MinBeginDate = new DateTime(1975, 1, 1, 0, 0, 0);

        public static bool SslCertificatesErrorPermit
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.certificate-permit"] != null &&
                       Convert.ToBoolean(ConfigurationManager.AppSettings["mail.certificate-permit"]);
            }
        }

        public static bool IsSignalRAvailable
        {
            get { return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["web.hub"]); }
        }

        public static TimeSpan AuthErrorWarningTimeout
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.auth-error-warning-in-minutes"] != null
                    ? TimeSpan.FromMinutes(
                        Convert.ToInt32(ConfigurationManager.AppSettings["mail.auth-error-warning-in-minutes"]))
                    : TimeSpan.FromHours(1);
            }
        }

        public static TimeSpan AuthErrorDisableTimeout
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.auth-error-disable-mailbox-in-minutes"] != null
                    ? TimeSpan.FromMinutes(
                        Convert.ToInt32(
                            ConfigurationManager.AppSettings["mail.auth-error-disable-mailbox-in-minutes"]))
                    : TimeSpan.FromDays(3);
            }
        }

        public static string MailDaemonEmail
        {
            get { return ConfigurationManager.AppSettings["mail.daemon-email"] ?? "mail-daemon@onlyoffice.com"; }
        }

        public static bool NeedProxyHttp
        {
            get { return SetupInfo.IsVisibleSettings("ProxyHttpContent"); }
        }

        public static string DefaultApiSchema
        {
            get
            {
                if (ConfigurationManager.AppSettings["mail.default-api-scheme"] != null)
                {
                    return ConfigurationManager.AppSettings["mail.default-api-scheme"] == Uri.UriSchemeHttps
                        ? Uri.UriSchemeHttps
                        : Uri.UriSchemeHttp;
                }

                return HttpContext.Current == null
                    ? Uri.UriSchemeHttp
                    : HttpContext.Current.Request.GetUrlRewriter().Scheme;
            }
        }

        /// <summary>
        /// Test mailbox connection tcp timeout (10 sec is default)
        /// </summary>
        public static int TcpTimeout
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.tcp-timeout"] != null
                    ? Convert.ToInt32(ConfigurationManager.AppSettings["mail.tcp-timeout"])
                    : 10000;
            }
        }

        public static int RecalculateFoldersTimeout
        {
            get
            {
                var limit = ConfigurationManager.AppSettings["mail.recalculate-folders-timeout"];
                return limit != null ? Convert.ToInt32(limit) : 99999;
            }
        }

        public static int RemoveDomainTimeout
        {
            get
            {
                var limit = ConfigurationManager.AppSettings["mail.remove-domain-timeout"];
                return limit != null ? Convert.ToInt32(limit) : 99999;
            }
        }

        public static int RemoveMailboxTimeout
        {
            get
            {
                var limit = ConfigurationManager.AppSettings["mail.remove-mailbox-timeout"];
                return limit != null ? Convert.ToInt32(limit) : 99999;
            }
        }

        public static bool SaveOriginalMessage
        {
            get
            {
                var saveFlag = ConfigurationManager.AppSettings["mail.save-original-message"];
                return saveFlag != null && Convert.ToBoolean(saveFlag);
            }
        }

        public static int ServerDomainMailboxPerUserLimit
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["mail.server-mailbox-limit-per-user"] ?? "2");
            }
        }

        public static string ServerDnsSpfRecordValue
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.server.spf-record-value"] ?? "v=spf1 +mx ~all";
            }
        }

        public static int ServerDnsMxRecordPriority
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["mail.server.mx-record-priority"] ?? "0");
            }
        }

        public static string ServerDnsDkimSelector
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.server-dkim-selector"] ?? "dkim";
            }
        }

        public static string ServerDnsDomainCheckPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.server-dns-check-prefix"] ?? "onlyoffice-domain";
            }
        }

        public static int ServerDnsDefaultTtl
        {
            get
            {
                var ttl = ConfigurationManager.AppSettings["mail.server-dns-default-ttl"];
                return ttl != null ? Convert.ToInt32(ttl) : 300;
            }
        }
    }
}
