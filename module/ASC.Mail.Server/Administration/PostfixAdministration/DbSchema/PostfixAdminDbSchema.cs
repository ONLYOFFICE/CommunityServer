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


namespace ASC.Mail.Server.PostfixAdministration.DbSchema
{
    public static class AdminTable
    {
        public const string name = "admin";

        public static class Columns
        {
            public const string username = "username";
            public const string password = "password";
            public const string created = "created";
            public const string modified = "modified";
            public const string active = "active";
        }
    }

    public static class AliasTable
    {
        public const string name = "alias";

        public static class Columns
        {
            public const string address = "address";
            public const string redirect = "goto";
            public const string domain = "domain";
            public const string created = "created";
            public const string modified = "modified";
            public const string active = "active";
            public const string is_group = "islist";
        }
    }

    public static class AliasDomainTable
    {
        public const string name = "alias_domain";

        public static class Columns
        {
            public const string alias_domain = "alias_domain";
            public const string target_domain = "target_domain";
            public const string created = "created";
            public const string modified = "modified";
            public const string active = "active";
        }
    }

    // ReSharper disable MemberHidesStaticFromOuterClass
    public static class ConfigTable
    {
        public const string name = "config";

        public static class Columns
        {
            public const string id = "id";
            public const string name = "name";
            public const string value = "value";
        }
    }
    // ReSharper restore MemberHidesStaticFromOuterClass

    public static class DomainTable
    {
        public const string name = "domain";

        public static class Columns
        {
            public const string domain = "domain";
            public const string description = "description";
            public const string aliases = "aliases";
            public const string mailboxes = "mailboxes";
            public const string maxquota = "maxquota";
            public const string quota = "quota";
            public const string transport = "transport";
            public const string backupmx = "backupmx";
            public const string created = "created";
            public const string modified = "modified";
            public const string active = "active";
        }
    }

    public static class DomainAdminsTable
    {
        public const string name = "domain_admins";

        public static class Columns
        {
            public const string username = "username";
            public const string domain = "domain";
            public const string created = "created";
            public const string active = "active";
        }
    }

    public static class FetchMailTable
    {
        public const string name = "fetchmail";

        public static class Columns
        {
            public const string id = "id";
            public const string mailbox = "mailbox";
            public const string src_server = "src_server";
            public const string src_auth = "src_auth";
            public const string src_user = "src_user";
            public const string src_password = "src_password";
            public const string src_folder = "src_folder";
            public const string poll_time = "poll_time";
            public const string fetchall = "fetchall";
            public const string keep = "keep";
            public const string protocol = "protocol";
            public const string usessl = "usessl";
            public const string extra_options = "extra_options";
            public const string returned_text = "returned_text";
            public const string mda = "mda";
            public const string date = "date";
        }
    }

    public static class LogTable
    {
        public const string name = "log";

        public static class Columns
        {
            public const string timestamp = "timestamp";
            public const string username = "username";
            public const string domain = "domain";
            public const string action = "action";
            public const string data = "data";
        }
    }

    // ReSharper disable MemberHidesStaticFromOuterClass
    public static class MailboxTable
    {
        public const string name = "mailbox";

        public static class Columns
        {
            public const string username = "username";
            public const string password = "password";
            public const string name = "name";
            public const string maildir = "maildir";
            public const string maxquota = "maxquota";
            public const string quota = "quota";
            public const string local_part = "local_part";
            public const string domain = "domain";
            public const string created = "created";
            public const string modified = "modified";
            public const string active = "active";
        }
    }
    // ReSharper restore MemberHidesStaticFromOuterClass

    public static class QuotaTable
    {
        public const string name = "quota";

        public static class Columns
        {
            public const string username = "username";
            public const string path = "path";
            public const string current = "current";
        }
    }

    public static class Quota2Table
    {
        public const string name = "quota2";

        public static class Columns
        {
            public const string username = "username";
            public const string bytes = "bytes";
            public const string messages = "messages";
        }
    }

    public static class VacationTable
    {
        public const string name = "vacation";

        public static class Columns
        {
            public const string email = "email";
            public const string subject = "subject";
            public const string body = "body";
            public const string cache = "cache";
            public const string domain = "domain";
            public const string created = "created";
            public const string active = "active";
        }
    }

    public static class VacationNotificationTable
    {
        public const string name = "vacation_notification";

        public static class Columns
        {
            public const string email = "on_vacation";
            public const string subject = "notified";
            public const string body = "notified_at";
        }
    }

    public static class DkimTable
    {
        public const string name = "dkim";

        public static class Columns
        {
            public const string id = "id";
            public const string domain_name = "domain_name";
            public const string selector = "selector";
            public const string private_key = "private_key";
            public const string public_key = "public_key";
        }
    }
}
