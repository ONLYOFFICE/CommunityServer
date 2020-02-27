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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Core.Engine;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Web.Studio.Utility;
using ContactInfo = ASC.Mail.Data.Contracts.ContactInfo;

namespace ASC.Mail.Extensions
{
    public static class DataContractsExtensions
    {
        public static List<MailAccountData> ToAccountData(this AccountInfo account)
        {
            var fromEmailList = new List<MailAccountData>();
            var mailBoxAccountSettings = MailBoxAccountSettings.LoadForCurrentUser();

            var emailData = new MailAccountData
            {
                MailboxId = account.Id,
                Email = account.Email,
                Name = account.Name,
                Enabled = account.Enabled,
                OAuthConnection = account.OAuthConnection,
                AuthError = account.AuthError,
                QuotaError = account.QuotaError,
                Signature = account.Signature,
                Autoreply = account.Autoreply,
                EMailInFolder = account.EMailInFolder,
                IsAlias = false,
                IsGroup = false,
                IsTeamlabMailbox = account.IsTeamlabMailbox,
                IsDefault = mailBoxAccountSettings.DefaultEmail == account.Email,
                IsSharedDomainMailbox = account.IsSharedDomainMailbox
            };
            fromEmailList.Add(emailData);

            foreach (var alias in account.Aliases)
            {
                emailData = new MailAccountData
                {
                    MailboxId = account.Id,
                    Email = alias.Email,
                    Name = account.Name,
                    Enabled = account.Enabled,
                    OAuthConnection = false,
                    AuthError = account.AuthError,
                    QuotaError = account.QuotaError,
                    Signature = account.Signature,
                    Autoreply = account.Autoreply,
                    EMailInFolder = account.EMailInFolder,
                    IsAlias = true,
                    IsGroup = false,
                    IsTeamlabMailbox = account.IsTeamlabMailbox,
                    IsDefault = mailBoxAccountSettings.DefaultEmail == alias.Email
                };
                fromEmailList.Add(emailData);
            }

            foreach (
                var @group in
                    account.Groups.Where(@group => fromEmailList.FindIndex(e => e.Email.Equals(@group.Email)) == -1))
            {
                emailData = new MailAccountData
                {
                    MailboxId = account.Id,
                    Email = @group.Email,
                    Name = "",
                    Enabled = true,
                    OAuthConnection = false,
                    AuthError = false,
                    QuotaError = false,
                    Signature = new MailSignatureData(-1, account.Signature.Tenant, "", false),
                    Autoreply = new MailAutoreplyData(-1, account.Signature.Tenant, false, false,
                        false, DateTime.MinValue, DateTime.MinValue, String.Empty, String.Empty), 
                    EMailInFolder = "",
                    IsAlias = false,
                    IsGroup = true,
                    IsTeamlabMailbox = true
                };
                fromEmailList.Add(emailData);
            }

            return fromEmailList;
        }

        public static List<MailAccountData> ToAccountData(this List<AccountInfo> accounts)
        {
            var fromEmailList = new List<MailAccountData>();

            fromEmailList = accounts.Aggregate(fromEmailList, (current, account) => current.Concat(account.ToAccountData()).ToList());

            return fromEmailList.DistinctBy(a => a.Email).ToList();
        }

        private const string BASE_VIRTUAL_PATH = "~/addons/mail/";

        public static MailContactData ToMailContactData(this ContactCard contactCard)
        {
            var baseAbsolutePath = CommonLinkUtility.ToAbsolute(BASE_VIRTUAL_PATH).ToLower();

            var emails = new MailContactData.EmailsList<ContactInfo>();
            var phones = new MailContactData.PhoneNumgersList<ContactInfo>();

            foreach (var contact in contactCard.ContactItems)
            {
                if (contact.Type == (int) ContactInfoType.Email)
                {
                    if (contact.IsPrimary)
                        emails.Insert(0,
                            new ContactInfo {Id = contact.Id, Value = contact.Data, IsPrimary = contact.IsPrimary});
                    else
                        emails.Add(new ContactInfo
                        {
                            Id = contact.Id,
                            Value = contact.Data,
                            IsPrimary = contact.IsPrimary
                        });
                }
                else if (contact.Type == (int) ContactInfoType.Phone)
                {
                    if (contact.IsPrimary)
                        phones.Insert(0,
                            new ContactInfo {Id = contact.Id, Value = contact.Data, IsPrimary = contact.IsPrimary});
                    else
                        phones.Add(new ContactInfo
                        {
                            Id = contact.Id,
                            Value = contact.Data,
                            IsPrimary = contact.IsPrimary
                        });
                }
            }

            var contactData = new MailContactData
            {
                ContactId = contactCard.ContactInfo.Id,
                Name = contactCard.ContactInfo.ContactName,
                Description = contactCard.ContactInfo.Description,
                Emails = emails,
                PhoneNumbers = phones,
                Type = (int) contactCard.ContactInfo.Type,
                SmallFotoUrl =
                    string.Format("{0}HttpHandlers/contactphoto.ashx?cid={1}&ps=1", baseAbsolutePath, contactCard.ContactInfo.Id)
                        .ToLower(),
                MediumFotoUrl =
                    string.Format("{0}HttpHandlers/contactphoto.ashx?cid={1}&ps=2", baseAbsolutePath, contactCard.ContactInfo.Id)
                        .ToLower()
            };

            return contactData;
        }

        public static List<MailContactData> ToMailContactDataList(this List<ContactCard> contacts)
        {
            var contactsList = contacts.Select(contact => contact.ToMailContactData()).ToList();

            return contactsList;
        }

        public static void GetNeededAccounts(this List<MailAccountData> accounts, out MailAccountData defaultAccount,
            out List<MailAccountData> commonAccounts, out List<MailAccountData> serverAccounts,
            out List<MailAccountData> aliases, out List<MailAccountData> groups)
        {
            defaultAccount = null;
            commonAccounts = new List<MailAccountData>();
            serverAccounts = new List<MailAccountData>();
            aliases = new List<MailAccountData>();
            groups = new List<MailAccountData>();

            if (accounts == null)
            {
                return;
            }

            foreach (var account in accounts)
            {
                if (account.IsDefault)
                {
                    defaultAccount = account;
                }
                else if (account.IsGroup)
                {
                    groups.Add(account);
                }
                else if (account.IsAlias)
                {
                    aliases.Add(account);
                }
                else if (account.IsTeamlabMailbox)
                {
                    serverAccounts.Add(account);
                }
                else
                {
                    commonAccounts.Add(account);
                }
            }
        }

        public static List<MailFolderData> ToFolderData(this List<FolderEngine.MailFolderInfo> folders)
        {
            return folders.Select(ToFolderData).ToList();
        }

        public static MailFolderData ToFolderData(this FolderEngine.MailFolderInfo folder)
        {
            return new MailFolderData
                {
                    Id = folder.id,
                    UnreadCount = folder.unread,
                    UnreadMessagesCount = folder.unreadMessages,
                    TotalCount = folder.total,
                    TotalMessgesCount = folder.totalMessages,
                    TimeModified = folder.timeModified
                };
        }

        public static MailTagData ToTagData(this Tag tag)
        {
            return new MailTagData
                {
                    Id = tag.Id,
                    Name = tag.TagName,
                    Style = tag.Style,
                    Addresses = new MailTagData.AddressesList<string>(tag.Addresses.Split(';')),
                    LettersCount = tag.Count
                };
        }

        public static List<MailTagData> ToTagData(this List<Tag> tags)
        {
            return tags
                .Select(t => new MailTagData
                {
                    Id = t.Id,
                    Name = t.TagName,
                    Style = t.Style,
                    Addresses = new MailTagData.AddressesList<string>(t.Addresses.Split(';')),
                    LettersCount = t.Count
                })
                .ToList();
        }

        public static Attachment ToAttachmnet(this MailAttachmentData a, int mailId, bool isRemoved = false)
        {
            var attachment = new Attachment
            {
                Id = a.fileId,
                MailId = mailId,
                Name = a.fileName,
                StoredName = a.storedName,
                Type = a.contentType,
                Size = a.size,
                FileNumber = a.fileNumber,
                IsRemoved = isRemoved,
                ContentId = a.contentId,
                Tenant = a.tenant,
                MailboxId = a.mailboxId
            };

            return attachment;
        }
    }
}
