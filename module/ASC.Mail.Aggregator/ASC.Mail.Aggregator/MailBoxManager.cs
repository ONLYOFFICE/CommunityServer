/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager : IDisposable
    {
        #region -- Global Values --

        public const string ConnectionStringName = "mail";

        public const string MAIL_DAEMON_EMAIL = "mail-daemon@teamlab.com";

        private readonly ILogger _log;

        private TimeSpan _authErrorWarningTimeout = TimeSpan.FromHours(2);
        private TimeSpan _authErrorDisableTimeout = TimeSpan.FromDays(3);

        private const string DATA_TAG = "666ceac1-4532-4f8c-9cba-8f510eca2fd1";

        private const string GoogleHost = "gmail.com";

        public bool EnableActivityLog { get; set; }

        public int TenantOverdueDays { get; set; }


        public TimeSpan AuthErrorWarningTimeout
        {
            get { return _authErrorWarningTimeout == TimeSpan.MinValue ? TimeSpan.FromHours(2) : _authErrorWarningTimeout; }
            set { _authErrorWarningTimeout = value; }
        }

        public TimeSpan AuthErrorDisableTimeout
        {
            get { return _authErrorDisableTimeout == TimeSpan.MinValue ? TimeSpan.FromDays(3) : _authErrorDisableTimeout; }
            set { _authErrorDisableTimeout = value; }
        }

        public bool SaveOriginalMessage
        {
            get
            {
                var save_flag = ConfigurationManager.AppSettings["mail.SaveOriginalMessage"];
                return save_flag != null && Convert.ToBoolean(save_flag);
            }
        }

        #endregion

        #region -- Mail Ticks --
        private class MailTicks
        {
            public MailTicks(long inactive_mail_ratio)
            {
                var mail_ratio = inactive_mail_ratio;
                if (mail_ratio > 50)
                {
                    k = -1;
                    mail_ratio = 100 - mail_ratio;
                }
                var window_double = 100.0 / mail_ratio;
                window = (long)Math.Round(window_double);
                shift = (long)Math.Round((window_double - window) * mail_ratio);
            }

            // return true for inactive users ticks
            public bool Tick()
            {
                var res = (0==(count + shift) % window);
                if (k < 0)
                    res = !res;

                if (++count == 100)
                    count = 0;
                return res;
            }

            private long count = 0;
            private long window;
            private long shift;
            private long k = 1;
        }

        private MailTicks ticks;
        #endregion

        #region -- Constructor --

        // inactive_mail_ratio(from 0 to 100) - Percentage of processed inactive boxes\
        //Todo: remove unused parameters
        public MailBoxManager(long inactive_mail_ratio, ILogger logger)
        {
            // active/inactive balance
            ticks = new MailTicks(inactive_mail_ratio);
            _log = logger;
        }

        public MailBoxManager(long inactive_mail_ratio)
            : this(inactive_mail_ratio, LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "MailBoxManager"))
        {
        }

        #endregion

        #region -- Methods --

        #region -- Public Methods --

        public DateTime GetMailboxFolderModifyDate(int mailbox_id, int folder_id)
        {
            using (var db = GetDb())
            {
                var date_string = db.ExecuteScalar<string>(
                    new SqlQuery(MailTable.name)
                    .SelectMax(MailTable.Columns.time_modified)
                    .Where(MailTable.Columns.id_mailbox, mailbox_id)
                    .Where(Exp.Eq(MailTable.Columns.folder, folder_id)));

                var date_time = new DateTime();

                return DateTime.TryParse(date_string, out date_time) ? date_time.ToUniversalTime() : new DateTime(1974, 1, 1).ToUniversalTime();
            }
        }

        #endregion

        #region -- Private Methods --

        private DbManager GetDb()
        {
            return new DbManager(ConnectionStringName);
        }

        private string EncryptPassword(string password)
        {
            return ASC.Security.Cryptography.InstanceCrypto.Encrypt(password);
        }

        private string DecryptPassword(string password)
        {
            return ASC.Security.Cryptography.InstanceCrypto.Decrypt(password);
        }

        private static MailMessageItem ConvertToMailMessageItem(object[] r, int tenant)
        {
            var now = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(tenant), DateTime.UtcNow);
            var date = TenantUtil.DateTimeFromUtc(CoreContext.TenantManager.GetTenant(tenant), (DateTime)r[7]);
            var isToday = (now.Year == date.Year && now.Date == date.Date);
            var isYesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            return new MailMessageItem()
            {
                Id              = Convert.ToInt64(r[0]),
                From            = (string)r[1],
                To              = (string)r[2],
                ReplyTo         = (string)r[3],
                Subject         = (string)r[4],
                Important       = Convert.ToBoolean(r[5]),
                Date            = date,
                Size            = Convert.ToInt32(r[8]),
                HasAttachments  = Convert.ToBoolean(r[9]),
                IsNew           = Convert.ToBoolean(r[10]),
                IsAnswered      = Convert.ToBoolean(r[11]),
                IsForwarded     = Convert.ToBoolean(r[12]),
                IsFromCRM       = Convert.ToBoolean(r[13]),
                IsFromTL        = Convert.ToBoolean(r[14]),
                LabelsString    = (string)r[15],
                RestoreFolderId = r[16] != null ? Convert.ToInt32(r[16]) : -1,
                ChainId         = (string)(r[17] ?? ""),
                ChainLength     = r[17] == null ? 1 : Convert.ToInt32(r[18]),
                Folder          = Convert.ToInt32(r[19]),
                IsToday         = isToday,
                IsYesterday     = isYesterday
            };
        }

        private string ConvertToString(object obj)
        {
            if (obj == DBNull.Value || obj == null)
            {
                return null;
            }

            return Convert.ToString(obj);
        }

        private Exp GetUserWhere(string id_user, int id_tenant, string alias)
        {
            return Exp.Eq(alias + ".tenant", id_tenant) & Exp.Eq(alias + ".id_user", id_user.ToLowerInvariant());
        }

        private Exp GetUserWhere(string id_user, int id_tenant)
        {
            return Exp.Eq("tenant", id_tenant) & Exp.Eq("id_user", id_user.ToLowerInvariant());
        }

        private string GetAddress(MailAddress email)
        {
            return email.Address.ToLowerInvariant();
        }

        #endregion

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

    public class ContactEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string contact1, string contact2)
        {
            var contact1_parts = contact1.Split('<');
            var contact2_parts = contact2.Split('<');

            return contact1_parts.Last().Replace(">", "") == contact2_parts.Last().Replace(">", "");
        }

        public int GetHashCode(string str)
        {
            var str_parts = str.Split('<');
            return str_parts.Last().Replace(">", "").GetHashCode();
        }
    }
}
