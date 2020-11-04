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
using System.IO;
using System.Linq;
using System.Net.Mail;
using ASC.Common.Logging;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Search;
using ASC.Mail.Enums;
using ASC.Mail.Extensions;
using MimeKit;
using ContactInfo = ASC.Mail.Core.Entities.ContactInfo;

namespace ASC.Mail.Utils
{
    public static class Converter
    {
        public static List<MailBoxData> ToMailboxList(this ClientConfig config, string email, string password, int tenant,
            string user)
        {
            var tempList = new List<MailBoxData>();

            if (!config.EmailProvider.OutgoingServer.Any() || !config.EmailProvider.IncomingServer.Any())
                return tempList;

            var address = new MailAddress(email);
            var host = address.Host.ToLowerInvariant();

            tempList.AddRange(from outServer in config.EmailProvider.OutgoingServer
                from inServer in config.EmailProvider.IncomingServer
                let smtpAuthenticationType = outServer.Authentication.ToSaslMechanism()
                select new MailBoxData
                {
                    EMail = address,
                    Name = "",
                    UserId = user,
                    TenantId = tenant,
                    Enabled = true,
                    BeginDate = DateTime.UtcNow.Subtract(new TimeSpan(MailBoxData.DefaultMailLimitedTimeDelta)),
                    Imap = inServer.Type == "imap",
                    Account =
                        inServer.Username.Replace("%EMAILADDRESS%", address.Address)
                            .Replace("%EMAILLOCALPART%", address.User)
                            .Replace("%EMAILDOMAIN%", host)
                            .Replace("%EMAILHOSTNAME%", Path.GetFileNameWithoutExtension(host)),
                    Password = password,
                    Server = inServer.Hostname.Replace("%EMAILDOMAIN%", host),
                    Port = inServer.Port,
                    Authentication = inServer.Authentication.ToSaslMechanism(),
                    Encryption = inServer.SocketType.ToEncryptionType(),
                    SmtpAccount =
                        outServer.Username.Replace("%EMAILADDRESS%", address.Address)
                            .Replace("%EMAILLOCALPART%", address.User)
                            .Replace("%EMAILDOMAIN%", host)
                            .Replace("%EMAILHOSTNAME%", Path.GetFileNameWithoutExtension(host)),
                    SmtpPassword = password,
                    SmtpServer = outServer.Hostname.Replace("%EMAILDOMAIN%", host),
                    SmtpPort = outServer.Port,

                    SmtpAuthentication = smtpAuthenticationType,
                    SmtpEncryption = outServer.SocketType.ToEncryptionType()
                });

            tempList = tempList.OrderByDescending(t => t.Imap).ToList();

            return tempList;
        }

        public static EncryptionType ToEncryptionType(this string type)
        {
            switch (type.ToLower().Trim())
            {
                case Defines.SSL:
                    return EncryptionType.SSL;
                case Defines.START_TLS:
                    return EncryptionType.StartTLS;
                case Defines.PLAIN:
                    return EncryptionType.None;
                default:
                    throw new ArgumentException("Unknown mail server socket type: " + type);
            }
        }

        public static string ToNameString(this EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.SSL:
                    return Defines.SSL;
                case EncryptionType.StartTLS:
                    return Defines.START_TLS;
                case EncryptionType.None:
                    return Defines.PLAIN;
                default:
                    throw new ArgumentException("Unknown mail server EncryptionType: " + Enum.GetName(typeof(EncryptionType), encryptionType));
            }
        }

        public static SaslMechanism ToSaslMechanism(this string type)
        {
            switch (type.ToLower().Trim())
            {
                case "":
                case Defines.OAUTH2:
                case Defines.PASSWORD_CLEARTEXT:
                    return SaslMechanism.Login;
                case Defines.NONE:
                    return SaslMechanism.None;
                case Defines.PASSWORD_ENCRYPTED:
                    return SaslMechanism.CramMd5;
                default:
                    throw new ArgumentException("Unknown mail server authentication type: " + type);
            }
        }

        public static string ToNameString(this SaslMechanism saslType)
        {
            switch (saslType)
            {
                case SaslMechanism.Login:
                    return "";
                case SaslMechanism.None:
                    return Defines.NONE;
                case SaslMechanism.CramMd5:
                    return Defines.PASSWORD_ENCRYPTED;
                default:
                    throw new ArgumentException("Unknown mail server SaslMechanism: " + Enum.GetName(typeof(SaslMechanism), saslType));
            }
        }

        public static string ToLoginFormat(this MailAddress address, string username)
        {
            var addressLower = address.Address.ToLower();
            var usernameLower = username.ToLower();
            var mailparts = addressLower.Split('@');

            var localpart = mailparts[0];
            var domain = mailparts[1];
            var hostNameVariant1 = Path.GetFileNameWithoutExtension(domain);
            var hostNameVariant2 = domain.Split('.')[0];

            var resultFormat = usernameLower.Replace(addressLower, "%EMAILADDRESS%");
            var pos = resultFormat.IndexOf(localpart, StringComparison.InvariantCulture);
            if (pos >= 0)
            {
                resultFormat = resultFormat.Substring(0, pos) + "%EMAILLOCALPART%" + resultFormat.Substring(pos + localpart.Length);
            }
            resultFormat = resultFormat.Replace(domain, "%EMAILDOMAIN%");
            if (hostNameVariant1 != null)
                resultFormat = resultFormat.Replace(hostNameVariant1, "%EMAILHOSTNAME%");
            resultFormat = resultFormat.Replace(hostNameVariant2, "%EMAILHOSTNAME%");

            return resultFormat == usernameLower ? null : resultFormat;
        }

        public static string ToLogin(this MailAddress address, string format)
        {
            return format.Replace("%EMAILADDRESS%", address.Address)
                         .Replace("%EMAILLOCALPART%", address.User)
                         .Replace("%EMAILDOMAIN%", address.Host.ToLowerInvariant())
                         .Replace("%EMAILHOSTNAME%", Path.GetFileNameWithoutExtension(address.Host.ToLowerInvariant()));
        }

        public static string ToHost(this string host, string format)
        {
            return format.Replace("%EMAILDOMAIN%", host);
        }

        public static MailWrapper ToMailWrapper(this MailMessageData message, int tenant, Guid userId)
        {
            var now = DateTime.UtcNow;

            var mailWrapper = new MailWrapper
            {
                Id = message.Id,
                TenantId = tenant,
                UserId = userId,
                FromText = message.From,
                ToText = message.To,
                Cc = message.Cc,
                Bcc = message.Bcc,
                Subject = message.Subject,
                Folder = (byte)message.Folder,
                DateSent = message.Date,
                MailboxId = message.MailboxId,
                ChainId = message.ChainId,
                ChainDate = message.ChainDate,
                IsRemoved = false,
                Unread = message.IsNew,
                Importance = message.Important,
                HasAttachments = message.HasAttachments,
                WithCalendar = !string.IsNullOrEmpty(message.CalendarUid),
                LastModifiedOn = now
            };

            if (message.Folder == FolderType.UserFolder && message.UserFolderId.HasValue)
            {
                mailWrapper.UserFolders = new List<UserFolderWrapper>
                {
                    new UserFolderWrapper
                    {
                        Id = (int) message.UserFolderId.Value
                    }
                };
            }
            else
            {
                mailWrapper.UserFolders = new List<UserFolderWrapper>();
            }

            if (message.TagIds != null && message.TagIds.Any())
            {
                mailWrapper.Tags = message.TagIds.ConvertAll(tagId => new TagWrapper
                {
                    Id = tagId,
                    TenantId = tenant,
                    LastModifiedOn = now
                });
            }
            else
            {
                mailWrapper.Tags = new List<TagWrapper>();
            }

            return mailWrapper;
        }

        public static MailMessageData ConvertToMailMessage(this MimeMessage mimeMessage, MailFolder folder, bool unread,
            string chainId, DateTime? chainDate, string streamId, int mailboxId, bool createFailedFake = true, ILog log = null)
        {
            MailMessageData message;

            try
            {
                message = mimeMessage.CreateMailMessage(mailboxId, folder.Folder, unread, chainId, chainDate, streamId, log);
            }
            catch (Exception ex)
            {
                if (!createFailedFake)
                    throw;

                var logger = log ?? new NullLog();

                logger.ErrorFormat("Convert MimeMessage->MailMessage: Exception: {0}", ex.ToString());

                logger.Debug("Creating fake message with original MimeMessage in attachments");

                message = mimeMessage.CreateCorruptedMesage(folder.Folder, unread, chainId, streamId);
            }

            return message;
        }

        public static MailContactWrapper ToMailContactWrapper(this ContactCard contactCard)
        {
            var now = DateTime.UtcNow;

            var infoList = contactCard.ContactItems.ConvertAll(ToMailContactInfoWrapper);

            var contact = new MailContactWrapper
            {
                Id = contactCard.ContactInfo.Id,
                TenantId = contactCard.ContactInfo.Tenant,
                User = Guid.Parse(contactCard.ContactInfo.User),
                Name = contactCard.ContactInfo.ContactName,
                ContactType = (int) contactCard.ContactInfo.Type,
                Description = contactCard.ContactInfo.Description,
                InfoList = infoList,
                LastModifiedOn = now
            };

            return contact;
        }

        public static MailContactInfoWrapper ToMailContactInfoWrapper(this ContactInfo contactInfo)
        {
            var now = DateTime.UtcNow;

            var info = new MailContactInfoWrapper
            {
                Id = contactInfo.Id,
                TenantId = contactInfo.Tenant,
                User = Guid.Parse(contactInfo.User),
                ContactId = contactInfo.ContactId,
                InfoType = contactInfo.Type,
                Text = contactInfo.Data,
                IsPrimary = contactInfo.IsPrimary,
                LastModifiedOn = now
            };

            return info;
        }
    }
}
