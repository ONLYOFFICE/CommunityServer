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


using System;

namespace ASC.Mail.Aggregator.DbSchema {
    // ReSharper disable MemberHidesStaticFromOuterClass
    public static class AttachmentTable
    {
        public const string name = "mail_attachment";

        public static class Columns
        {
            public const string id = "id";
            public const string id_mail = "id_mail";
            public const string name = "name";
            public const string stored_name = "stored_name";
            public const string type = "type";
            public const string size = "size";
            public const string need_remove = "need_remove";
            public const string file_number = "file_number";
            public const string content_id = "content_id";
            public const string id_tenant = "tenant";
        }
    }


    public static class Garbage
    {
        public const string table = "mail_garbage";

        public static class Columns
        {
            public const string id = "id";
            public const string tenant = "tenant";
            public const string path = "path";
            public const string is_processed = "is_processed";
            public const string time_modified = "time_modified";
        };
    }


    public static class MailTable
    {
        public const string name = "mail_mail";

        public static class Columns
        {
            public const string id = "id";
            public const string id_mailbox = "id_mailbox";
            public const string id_user = "id_user";
            public const string id_tenant = "tenant";
            public const string address = "address";
            public const string uidl = "uidl";
            public const string md5 = "md5";
            public const string from = "from_text";
            public const string to = "to_text";
            public const string reply = "reply_to";
            public const string cc = "cc";
            public const string bcc = "bcc";
            public const string subject = "subject";
            public const string introduction = "introduction";
            public const string importance = "importance";
            public const string date_received = "date_received";
            public const string date_sent = "date_sent";
            public const string size = "size";
            public const string attach_count = "attachments_count";
            public const string unread = "unread";
            public const string is_answered = "is_answered";
            public const string is_forwarded = "is_forwarded";
            public const string is_from_crm = "is_from_crm";
            public const string is_from_tl = "is_from_tl";
            public const string stream = "stream";
            public const string folder = "folder";
            public const string folder_restore = "folder_restore";
            public const string spam = "spam";
            public const string is_removed = "is_removed";
            public const string time_modified = "time_modified";
            public const string mime_message_id = "mime_message_id";
            public const string mime_in_reply_to = "mime_in_reply_to";
            public const string chain_id = "chain_id";
            public const string chain_date = "chain_date";
            public const string is_text_body_only = "is_text_body_only";
        };
    }


    public static class ImapFlags
    {
        public const string table = "mail_imap_flags";

        public static class Columns
        {
            public const string name = "name";
            public const string folder_id = "folder_id";
            public const string skip = "skip";
        };
    }


    public static class ImapSpecialMailbox
    {
        public const string table = "mail_imap_special_mailbox";

        public static class Columns
        {
            public const string name = "name";
            public const string server = "server";
            public const string folder_id = "folder_id";
            public const string skip = "skip";
        };
    }


    public static class PopUnorderedDomain
    {
        public const string table = "mail_pop_unordered_domain";

        public static class Columns
        {
            public const string server = "server";
        };
    }


    public static class ChainTable
    {
        public const string name = "mail_chain";

        public static class Columns
        {
            public const string id = "id";
            public const string id_mailbox = "id_mailbox";
            public const string id_tenant = "tenant";
            public const string id_user = "id_user";
            public const string folder = "folder";
            public const string length = "length";
            public const string unread = "unread";
            public const string has_attachments = "has_attachments";
            public const string importance = "importance";
            public const string tags = "tags";
        };
    }

    public class CrmContactEntity
    {
        public int Id { get; set;}
        public ChainXCrmContactEntity.EntityTypes Type { get; set;}
    }

    public static class ChainXCrmContactEntity
    {
        public const string name = "mail_chain_x_crm_entity";

        public static class Columns
        {
            public const string id_tenant = "id_tenant";
            public const string id_mailbox = "id_mailbox";
            public const string id_chain = "id_chain";
            public const string entity_id = "entity_id";
            public const string entity_type = "entity_type";
        }

        public enum EntityTypes
        {
            Contact = 1,
            Case = 2,
            Opportunity = 3
        }

        public static class CrmEntityTypeNames
        {
            public const string contact = "contact";
            public const string Case = "case";
            public const string opportunity = "opportunity";
        }

        public static string StringName(this EntityTypes type)
        {
            switch (type)
            {
                case EntityTypes.Contact:
                    return CrmEntityTypeNames.contact;
                case EntityTypes.Case:
                    return CrmEntityTypeNames.Case;
                case EntityTypes.Opportunity:
                    return CrmEntityTypeNames.opportunity;
                default:
                    throw new ArgumentException(String.Format("Invalid CrmEntityType: {0}", type), "type");
            }
        }
    }

    public static class SignatureTable
    {
        public const string name = "mail_mailbox_signature";

        public static class Columns
        {
            public const string id_mailbox = "id_mailbox";
            public const string id_tenant = "tenant";
            public const string html = "html";
            public const string is_active = "is_active";
        };
    }

    public static class MailboxTable
    {
        public const string name = "mail_mailbox";

        public static class Columns
        {
            public const string id = "id";
            public const string id_user = "id_user";
            public const string id_tenant = "tenant";
            public const string address = "address";
            public const string enabled = "enabled";
            public const string password = "pop3_password";
            public const string msg_count_last = "msg_count_last";
            public const string size_last = "size_last";
            public const string smtp_password = "smtp_password";
            public const string name = "name";
            public const string login_delay = "login_delay";
            public const string time_checked = "time_checked";
            public const string is_processed = "is_processed";
            public const string user_time_checked = "user_time_checked";
            public const string login_delay_expires = "login_delay_expires";
            public const string is_removed = "is_removed";
            public const string quota_error = "quota_error";
            public const string auth_error = "auth_error";
            public const string imap = "imap";
            public const string begin_date = "begin_date";
            public const string service_type = "service_type";
            public const string refresh_token = "refresh_token";
            public const string imap_folders = "imap_folders";
            public const string id_smtp_server = "id_smtp_server";
            public const string id_in_server = "id_in_server";
            public const string is_teamlab_mailbox = "is_teamlab_mailbox ";
            public const string date_created = "date_created ";
        }
    }

    public static class MailboxProviderTable
    {
        public const string name = "mail_mailbox_provider";

        public static class Columns
        {
            public const string id = "id";
            public const string name = "name";
            public const string display_name = "display_name";
            public const string display_short_name = "display_short_name";
            public const string documentation = "documentation";
        };
    }

    public static class MailboxDomainTable
    {
        public const string name = "mail_mailbox_domain";

        public static class Columns
        {
#pragma warning disable 169
            public const string id = "id";
#pragma warning restore 169
            public const string id_provider = "id_provider";
            public const string name = "name";
        };
    }

    public static class MailboxServerTable
    {
        public const string name = "mail_mailbox_server";

        public static class Columns
        {
            public const string id = "id";
            public const string id_provider = "id_provider";
            public const string type = "type";
            public const string hostname = "hostname";
            public const string port = "port";
            public const string socket_type = "socket_type";
            public const string username = "username";
            public const string authentication = "authentication";
            public const string is_user_data = "is_user_data";
        };
    }

    public static class AddressTable
    {
        public const string name = "mail_server_address";

        public static class Columns
        {
            public const string id = "id";
            public const string name = "name";
            public const string tenant = "tenant";
            public const string id_domain = "id_domain";
            public const string id_mailbox = "id_mailbox";
            public const string is_mail_group = "is_mail_group";
            public const string is_alias = "is_alias";
            public const string date_created = "date_created";
        };
    }


    public static class DomainTable
    {
        public const string name = "mail_server_domain";

        public static class Columns
        {
            public const string id = "id";
            public const string tenant = "tenant";
            public const string name = "name";
            public const string date_added = "date_added";
        };
    }

    public static class ServerTable
    {
        public const string name = "mail_server_server";

        public static class Columns
        {
            public const string id = "id";
            public const string mx_record = "mx_record";
            public const string connection_string = "connection_string";
            public const string server_type = "server_type";
            public const string smtp_settings_id = "smtp_settings_id";
            public const string imap_settings_id = "imap_settings_id";
        };
    }

    public static class TenantXServerTable
    {
        public const string name = "mail_server_server_x_tenant";

        public static class Columns
        {
            public const string id_tenant = "id_tenant";
            public const string id_server = "id_server";
            public const string cname = "cname";
        };
    }

    public static class ServerTypeTable
    {
        public const string name = "mail_server_server_type";

        public static class Columns
        {
            public const string id = "id";
            public const string name = "name";
        };
    }

    public static class MailGroupTable
    {
        public const string name = "mail_server_mail_group";

        public static class Columns
        {
            public const string id = "id";
            public const string id_tenant = "id_tenant";
            public const string id_address = "id_address";
            public const string date_created = "date_created";
        }
    }

    public static class MailGroupXAddressesTable
    {
        public const string name = "mail_server_mail_group_x_mail_server_address";

        public static class Columns
        {
            public const string id_mail_group = "id_mail_group";
            public const string id_address = "id_address";
        }
    }

    public static class CNameXDomainTable
    {
        public const string name = "mail_server_domain_x_cname";

        public static class Columns
        {
            public const string id_domain = "id_domain";
            public const string cname = "cname";
            public const string reference_url = "reference_url";
            public const string verified = "verified";
        }
    }
    // ReSharper restore MemberHidesStaticFromOuterClass
}