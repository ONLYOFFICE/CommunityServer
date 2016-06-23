/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


namespace ASC.Mail.Aggregator.DbSchema
{
    public static class MailboxTable
    {
        public const string Name = "mail_mailbox";

        public static class Columns
        {
            public const string Id = "id";
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string Address = "address";
            public const string Enabled = "enabled";
            public const string Password = "pop3_password";
            public const string MsgCountLast = "msg_count_last";
            public const string SizeLast = "size_last";
            public const string SmtpPassword = "smtp_password";
            public const string AddressName = "name";
            public const string LoginDelay = "login_delay";
            //public const string time_checked = "time_checked";
            public const string IsProcessed = "is_processed";
            //public const string user_time_checked = "user_time_checked";
            //public const string login_delay_expires = "login_delay_expires";
            public const string IsRemoved = "is_removed";
            public const string QuotaError = "quota_error";
            //public const string auth_error = "auth_error";
            public const string Imap = "imap";
            public const string BeginDate = "begin_date";
            public const string ServiceType = "service_type";
            public const string RefreshToken = "refresh_token";
            public const string ImapIntervals = "imap_intervals";
            public const string SmtpServerId = "id_smtp_server";
            public const string ServerId = "id_in_server";
            public const string EmailInFolder = "email_in_folder";
            public const string IsTeamlabMailbox = "is_teamlab_mailbox";

            public const string DateCreated = "date_created ";
            public const string DateChecked = "date_checked ";
            public const string DateUserChecked = "date_user_checked ";
            public const string UserOnline = "user_online ";
            public const string DateLoginDelayExpires = "date_login_delay_expires ";
            public const string DateAuthError = "date_auth_error ";
        }
    }

    public static class MailboxProviderTable
    {
        public const string Name = "mail_mailbox_provider";

        public static class Columns
        {
            public const string Id = "id";
            public const string ProviderName = "name";
            public const string DisplayName = "display_name";
            public const string DisplayShortName = "display_short_name";
            public const string Documentation = "documentation";
        }
    }

    public static class AttachmentTable
    {
        public const string Name = "mail_attachment";

        public static class Columns
        {
            public const string Id = "id";
            public const string MailId = "id_mail";
            public const string RealName = "name";
            public const string StoredName = "stored_name";
            public const string Type = "type";
            public const string Size = "size";
            public const string NeedRemove = "need_remove";
            public const string FileNumber = "file_number";
            public const string ContentId = "content_id";
            public const string IdTenant = "tenant";
            public const string IdMailbox = "id_mailbox";
        }
    }


    public static class GarbageTable
    {
        public const string Name = "mail_garbage";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string Path = "path";
            public const string IsProcessed = "is_processed";
            public const string TimeModified = "time_modified";
        };
    }


    public static class MailTable
    {
        public const string Name = "mail_mail";

        public static class Columns
        {
            public const string Id = "id";
            public const string MailboxId = "id_mailbox";
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string Address = "address";
            public const string Uidl = "uidl";
            public const string Md5 = "md5";
            public const string From = "from_text";
            public const string To = "to_text";
            public const string Reply = "reply_to";
            public const string Cc = "cc";
            public const string Bcc = "bcc";
            public const string Subject = "subject";
            public const string Introduction = "introduction";
            public const string Importance = "importance";
            public const string DateReceived = "date_received";
            public const string DateSent = "date_sent";
            public const string Size = "size";
            public const string AttachCount = "attachments_count";
            public const string Unread = "unread";
            public const string IsAnswered = "is_answered";
            public const string IsForwarded = "is_forwarded";
            public const string Stream = "stream";
            public const string Folder = "folder";
            public const string FolderRestore = "folder_restore";
            public const string Spam = "spam";
            public const string IsRemoved = "is_removed";
            public const string TimeModified = "time_modified";
            public const string MimeMessageId = "mime_message_id";
            public const string MimeInReplyTo = "mime_in_reply_to";
            public const string ChainId = "chain_id";
            public const string ChainDate = "chain_date";
            public const string IsTextBodyOnly = "is_text_body_only";
            public const string HasParseError = "has_parse_error";
            public const string CalendarUid = "calendar_uid";
        };
    }


    public static class ImapFlags
    {
        public const string Name = "mail_imap_flags";

        public static class Columns
        {
            public const string FlagName = "name";
            public const string FolderId = "folder_id";
            public const string Skip = "skip";
        };
    }


    public static class ImapSpecialMailbox
    {
        public const string Name = "mail_imap_special_mailbox";

        public static class Columns
        {
            public const string MailboxName = "name";
            public const string Server = "server";
            public const string FolderId = "folder_id";
            public const string Skip = "skip";
        };
    }


    public static class PopUnorderedDomain
    {
        public const string Name = "mail_pop_unordered_domain";

        public static class Columns
        {
            public const string Server = "server";
        };
    }


    public static class ChainTable
    {
        public const string Name = "mail_chain";

        public static class Columns
        {
            public const string Id = "id";
            public const string MailboxId = "id_mailbox";
            public const string Tenant = "tenant";
            public const string User = "id_user";
            public const string Folder = "folder";
            public const string Length = "length";
            public const string Unread = "unread";
            public const string HasAttachments = "has_attachments";
            public const string Importance = "importance";
            public const string Tags = "tags";
        };
    }

    public static class ChainXCrmContactEntity
    {
        public const string Name = "mail_chain_x_crm_entity";

        public static class Columns
        {
            public const string Tenant = "id_tenant";
            public const string MailboxId = "id_mailbox";
            public const string ChainId = "id_chain";
            public const string EntityId = "entity_id";
            public const string EntityType = "entity_type";
        }
    }

    public static class SignatureTable
    {
        public const string Name = "mail_mailbox_signature";

        public static class Columns
        {
            public const string MailboxId = "id_mailbox";
            public const string Tenant = "tenant";
            public const string Html = "html";
            public const string IsActive = "is_active";
        };
    }

    public static class AutoreplyTable
    {
        public const string Name = "mail_mailbox_autoreply";

        public static class Columns
        {
            public const string MailboxId = "id_mailbox";
            public const string Tenant = "tenant";
            public const string TurnOn = "turn_on";
            public const string OnlyContacts = "only_contacts";
            public const string TurnOnToDate = "turn_on_to_date";
            public const string FromDate = "from_date";
            public const string ToDate = "to_date";
            public const string Subject = "subject";
            public const string Html = "html";
        };
    }

    public static class AutoreplyHistoryTable
    {
        public const string Name = "mail_mailbox_autoreply_history";

        public static class Columns
        {
            public const string MailboxId = "id_mailbox";
            public const string Tenant = "tenant";
            public const string SendingEmail = "sending_email";
            public const string SendingDate = "sending_date";
        };
    }

    public static class ContactsTable
    {
        public const string Name = "mail_contacts";

        public static class Columns
        {
            public const string Id = "id";
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string ContactName = "name";
            public const string Address = "address";
            public const string Description = "description";
            public const string Type = "type";
            public const string HasPhoto = "has_photo";
            public const string LastModified = "last_modified";
        }
    }

    public static class ContactInfoTable
    {
        public const string Name = "mail_contact_info";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string User = "id_user";
            public const string ContactId = "id_contact";
            public const string Data = "data";
            public const string Type = "type";
            public const string IsPrimary = "is_primary";
            public const string LastModified = "last_modified";
        }
    }

    public static class MailboxDomainTable
    {
        public const string Name = "mail_mailbox_domain";

        public static class Columns
        {
            public const string Id = "id";
            public const string ProviderId = "id_provider";
            public const string DomainName = "name";
        };
    }

    public static class MailboxServerTable
    {
        public const string Name = "mail_mailbox_server";

        public static class Columns
        {
            public const string Id = "id";
            public const string ProviderId = "id_provider";
            public const string Type = "type";
            public const string Hostname = "hostname";
            public const string Port = "port";
            public const string SocketType = "socket_type";
            public const string Username = "username";
            public const string Authentication = "authentication";
            public const string IsUserData = "is_user_data";
        };
    }

    public static class AddressTable
    {
        public const string Name = "mail_server_address";

        public static class Columns
        {
            public const string Id = "id";
            public const string AddressName = "name";
            public const string Tenant = "tenant";
            public const string DomainId = "id_domain";
            public const string MailboxId = "id_mailbox";
            public const string IsMailGroup = "is_mail_group";
            public const string IsAlias = "is_alias";
            public const string DateCreated = "date_created";
        };
    }


    public static class DomainTable
    {
        public const string Name = "mail_server_domain";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string DomainName = "name";
            public const string IsVerified = "is_verified";
            public const string DateAdded = "date_added";
            public const string DateChecked = "date_checked";
        };
    }

    public static class ServerTable
    {
        public const string Name = "mail_server_server";

        public static class Columns
        {
            public const string Id = "id";
            public const string MxRecord = "mx_record";
            public const string ConnectionString = "connection_string";
            public const string ServerType = "server_type";
            public const string SmtpSettingsId = "smtp_settings_id";
            public const string ImapSettingsId = "imap_settings_id";
        };
    }

    public static class TenantXServerTable
    {
        public const string Name = "mail_server_server_x_tenant";

        public static class Columns
        {
            public const string Tenant = "id_tenant";
            public const string ServerId = "id_server";
            public const string Cname = "cname";
        };
    }

    public static class ServerTypeTable
    {
        public const string Name = "mail_server_server_type";

        public static class Columns
        {
            public const string Id = "id";
            public const string TypeName = "name";
        };
    }

    public static class MailGroupTable
    {
        public const string Name = "mail_server_mail_group";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "id_tenant";
            public const string AddressId = "id_address";
            public const string Address = "address";
            public const string DateCreated = "date_created";
        }
    }

    public static class MailGroupXAddressesTable
    {
        public const string Name = "mail_server_mail_group_x_mail_server_address";

        public static class Columns
        {
            public const string MailGroupId = "id_mail_group";
            public const string AddressId = "id_address";
        }
    }

    public static class DnsTable
    {
        public const string Name = "mail_server_dns";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string User = "id_user";
            public const string DomainId = "id_domain";
            public const string DkimSelector = "dkim_selector";
            public const string DkimPrivateKey = "dkim_private_key";
            public const string DkimPublicKey = "dkim_public_key";
            public const string DomainCheck = "domain_check";
            public const string Spf = "spf";
            public const string TimeModified = "time_modified";
        }
    }

    public static class MailAlertsTable
    {
        public const string Name = "mail_alerts";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string User = "id_user";
            public const string MailboxId = "id_mailbox";
            public const string Type = "type";
            public const string Data = "data";
        }
    }

    public static class FolderTable
    {
        public const string Name = "mail_folder";

        public static class Columns
        {
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string Folder = "folder";
            public const string TimeModified = "time_modified";
            public const string UnreadMessagesCount = "unread_messages_count";
            public const string TotalMessagesCount = "total_messages_count";
            public const string UnreadConversationsCount = "unread_conversations_count";
            public const string TotalConversationsCount = "total_conversations_count";
        }
    }

    public static class DisplayImagesTable
    {
        public const string Name = "mail_display_images";

        public static class Columns
        {
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string Address = "address";
        }
    }

    public static class TagMailTable
    {
        public const string Name = "mail_tag_mail";

        public static class Columns
        {
            public const string MailId = "id_mail";
            public const string TagId = "id_tag";
            public const string TimeCreated = "time_created";
            public const string Tenant = "tenant";
            public const string User = "id_user";
        }
    }

    public static class TagTable
    {
        public const string Name = "mail_tag";

        public static class Columns
        {
            public const string Id = "id";
            public const string User = "id_user";
            public const string Tenant = "tenant";
            public const string TagName = "name";
            public const string Style = "style";
            public const string Addresses = "addresses";
            public const string Count = "count";
            public const string CrmId = "crm_id";
        }
    }

    public static class TagAddressTable
    {
        public const string Name = "mail_tag_addresses";

        public static class Columns
        {
            public const string TagId = "id_tag";
            public const string Address = "address";
            public const string Tenant = "tenant";
        }
    }

    public static class CrmTagTable
    {
        public const string Name = "crm_tag";

        public static class Columns
        {
            public const string Id = "id";
            public const string Title = "title";
            public const string Tenant = "tenant_id";
            public const string EntityType = "entity_type";
        }
    }

    public static class CrmEntityTagTable
    {
        public const string Name = "crm_entity_tag";

        public static class Columns
        {
            public const string TagId = "tag_id";
            public const string EntityType = "entity_type";
            public const string EntityId = "entity_id";
        }
    }

    public static class CrmContactTable
    {
        public const string Name = "crm_contact";

        public static class Columns
        {
            public const string Id = "id";
            public const string IsCompany = "is_company";
            public const string FirstName = "first_name";
            public const string LastName = "last_name";
            public const string CompanyName = "company_name";
            public const string DisplayName = "display_name";
            public const string Data = "data";
            public const string Tenant = "tenant_id";
            public const string IsShared = "is_shared";
        }
    }

    public static class CrmContactInfoTable
    {
        public const string Name = "crm_contact_info";

        public static class Columns
        {
            public const string Id = "id";
            public const string Data = "data";
            public const string Tenant = "tenant_id";
            public const string ContactId = "contact_id";
            public const string Type = "type";
        }
    }
}
