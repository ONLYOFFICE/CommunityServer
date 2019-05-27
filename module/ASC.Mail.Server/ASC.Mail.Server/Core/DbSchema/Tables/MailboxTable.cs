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


using System.Collections.Generic;
using ASC.Mail.Server.Core.DbSchema.Interfaces;

namespace ASC.Mail.Server.Core.DbSchema.Tables
{
    public class MailboxTable : ITable
    {
        public const string TABLE_NAME = "mailbox";

        public static class Columns
        {
            public const string USERNAME = "username";
            public const string PASSWORD = "password";
            public const string NAME = "name";
            public const string MAILDIR = "maildir";
            public const string MAXQUOTA = "maxquota";
            public const string QUOTA = "quota";
            public const string LOCAL_PART = "local_part";
            public const string DOMAIN = "domain";
            public const string CREATED = "created";
            public const string MODIFIED = "modified";
            public const string ACTIVE = "active";
            public const string ENABLE_IMAP = "enableimap";
            public const string ENABLE_IMAP_SECURED = "enableimapsecured";
            public const string ENABLE_POP = "enablepop3";
            public const string ENABLE_POP_SECURED = "enablepop3secured";
            public const string ENABLE_DELIVER = "enabledeliver";
            public const string ENABLE_LDA = "enablelda";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public MailboxTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.USERNAME,
                Columns.PASSWORD,
                Columns.NAME,
                Columns.MAILDIR,
                Columns.MAXQUOTA,
                Columns.QUOTA,
                Columns.LOCAL_PART,
                Columns.DOMAIN,
                Columns.CREATED,
                Columns.MODIFIED,
                Columns.ACTIVE,
                Columns.ENABLE_IMAP,
                Columns.ENABLE_IMAP_SECURED,
                Columns.ENABLE_POP,
                Columns.ENABLE_POP_SECURED,
                Columns.ENABLE_DELIVER,
                Columns.ENABLE_LDA
            };
        }
    }
}