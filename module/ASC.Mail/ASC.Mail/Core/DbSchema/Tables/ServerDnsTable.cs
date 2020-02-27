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


// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using ASC.Mail.Core.DbSchema.Interfaces;

namespace ASC.Mail.Core.DbSchema.Tables
{
    public class ServerDnsTable : ITable
    {
        public const string TABLE_NAME = "mail_server_dns";

        public static class Columns
        {
            public const string Id = "id";
            public const string Tenant = "tenant";
            public const string User = "id_user";
            public const string DomainId = "id_domain";

            public const string DkimSelector = "dkim_selector";
            public const string DkimPrivateKey = "dkim_private_key";
            public const string DkimPublicKey = "dkim_public_key";
            public const string DkimTtl = "dkim_ttl";
            public const string DkimVerified = "dkim_verified";
            public const string DkimDateChecked = "dkim_date_checked";

            public const string DomainCheck = "domain_check";
            
            public const string Spf = "spf";
            public const string SpfTtl = "spf_ttl";
            public const string SpfVerified = "spf_verified";
            public const string SpfDateChecked = "spf_date_checked";

            public const string Mx = "mx";
            public const string MxTtl = "mx_ttl";
            public const string MxVerified = "mx_verified";
            public const string MxDateChecked = "mx_date_checked";

            public const string TimeModified = "time_modified";
        }

        public string Name
        {
            get { return TABLE_NAME; }
        }

        public IEnumerable<string> OrderedColumnCollection { get; private set; }

        public ServerDnsTable()
        {
            OrderedColumnCollection = new List<string>
            {
                Columns.Id,
                Columns.Tenant,
                Columns.User,
                Columns.DomainId,
                Columns.DomainCheck,
                Columns.DkimSelector,
                Columns.DkimPrivateKey,
                Columns.DkimPublicKey, 
                Columns.DkimTtl,
                Columns.DkimVerified,
                Columns.DkimDateChecked,
                Columns.Spf,
                Columns.SpfTtl,
                Columns.SpfVerified,
                Columns.SpfDateChecked,
                Columns.Mx,
                Columns.MxTtl,
                Columns.MxVerified,
                Columns.MxDateChecked,
                Columns.TimeModified
            };
        }
    }
}