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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using ASC.Common.Data;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.ComplexOperations.Base;
using ASC.Security.Cryptography;
using ASC.Web.Studio.Utility;
using MailMessage = ASC.Mail.Aggregator.Common.MailMessage;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager : IDisposable
    {
        #region -- Global Values --

        public const string CONNECTION_STRING_NAME = "mail";

        private readonly ILogger _log;
        private CachedAccounts _cachedAccounts;

        private TimeSpan _authErrorWarningTimeout = TimeSpan.FromHours(2);
        private TimeSpan _authErrorDisableTimeout = TimeSpan.FromDays(3);

        private const string DATA_TAG = "666ceac1-4532-4f8c-9cba-8f510eca2fd1";

        private const string GOOGLE_HOST = "gmail.com";

        public CachedAccounts CachedAccounts
        {
            get { return _cachedAccounts ?? (_cachedAccounts = new CachedAccounts()); }
        }

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
                var saveFlag = ConfigurationManager.AppSettings["mail.save-original-message"];
                return saveFlag != null && Convert.ToBoolean(saveFlag);
            }
        }

        public string MailDaemonEmail
        {
            get { return ConfigurationManager.AppSettings["mail.daemon-email"] ?? "mail-daemon@onlyoffice.com"; }
        }

        public int MailOperationsLimit
        {
            get
            {
                var limit = ConfigurationManager.AppSettings["mail.operations-limit"];
                return limit != null ? Convert.ToInt32(limit) : 100;
            }
        }

        public int RecalculateFoldersTimeout
        {
            get
            {
                var limit = ConfigurationManager.AppSettings["mail.recalculate-folders-timeout"];
                return limit != null ? Convert.ToInt32(limit) : 99999;
            }
        }

        public Dictionary<string, string> MxToDomainBusinessVendorsList
        {
            get
            {
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
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mail.busines-vendors-mx-domains"]))
                        // ".outlook.:office365.com|.google.:gmail.com|.yandex.:yandex.ru|.mail.ru:mail.ru|.yahoodns.:yahoo.com"
                    {
                        list = ConfigurationManager.AppSettings["mail.busines-vendors-mx-domains"]
                            .Split('|')
                            .Select(s => s.Split(':'))
                            .ToDictionary(s => s[0], s => s[1]);
                    }
                }
                catch
                {
                    //ignore
                }

                return list;
            }

        }

        private readonly DistributedTaskQueue _mailOperations;

        public DistributedTaskQueue MailOperations
        {
            get { return _mailOperations; }
        }

        #endregion

        #region -- Constructor --

        public MailBoxManager(ILogger logger)
        {
            _log = logger;
            _mailOperations = new DistributedTaskQueue("mailOperations", MailOperationsLimit);
        }

        public MailBoxManager()
            : this(LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "MailBoxManager"))
        {
        }

        #endregion

        #region -- Methods --

        #region -- Public Methods --

        #endregion

        #region -- Private Methods --

        private DbManager GetDb(int? commandTimeout = null)
        {
            return new DbManager(CONNECTION_STRING_NAME, commandTimeout);
        }

        private string EncryptPassword(string password)
        {
            return InstanceCrypto.Encrypt(password);
        }

        private string DecryptPassword(string password)
        {
            return InstanceCrypto.Decrypt(password);
        }

        private bool TryDecryptPassword(string encryptedPassword, out string password)
        {
            password = "";
            try
            {
                password = DecryptPassword(encryptedPassword);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static MailMessage ConvertToMailMessage(object[] r, Tenant tenantInfo, DateTime utcNow)
        {
            var now = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, utcNow);
            var date = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, (DateTime)r[6]);
            var chainDate = TenantUtil.DateTimeFromUtc(tenantInfo.TimeZone, (DateTime)r[16]);

            var isToday = (now.Year == date.Year && now.Date == date.Date);
            var isYesterday = (now.Year == date.Year && now.Date == date.Date.AddDays(1));

            return new MailMessage
            {
                Id              = Convert.ToInt64(r[0]),
                From            = (string)r[1],
                To              = (string)r[2],
                ReplyTo         = (string)r[3],
                Subject         = (string)r[4],
                Important       = Convert.ToBoolean(r[5]),
                Date            = date,
                Size            = Convert.ToInt32(r[7]),
                HasAttachments  = Convert.ToBoolean(r[8]),
                IsNew           = Convert.ToBoolean(r[9]),
                IsAnswered      = Convert.ToBoolean(r[10]),
                IsForwarded     = Convert.ToBoolean(r[11]),

                LabelsString    = (string)r[12],

                RestoreFolderId = r[13] != null ? Convert.ToInt32(r[13]) : 1,
                Folder          = Convert.ToInt32(r[14]),

                ChainId         = (string)(r[15] ?? ""),
                ChainLength     = 1,
                ChainDate       = chainDate,

                IsToday         = isToday,
                IsYesterday     = isYesterday,

                MailboxId       = Convert.ToInt32(r[17]),

                CalendarUid     = (string)r[18]
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

        private Exp GetUserWhere(string user, int tenant, string alias)
        {
            return Exp.Eq(alias + ".tenant", tenant) & Exp.Eq(alias + ".id_user", user.ToLowerInvariant());
        }

        private Exp GetUserWhere(string user, int tenant)
        {
            return Exp.Eq("tenant", tenant) & Exp.Eq("id_user", user.ToLowerInvariant());
        }

        private string GetAddress(MailAddress email)
        {
            return email.Address.ToLowerInvariant();
        }

        public MailOperationStatus QueueTask(MailOperation op, Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var task = op.GetDistributedTask();
            MailOperations.QueueTask(op.RunJob, task);
            return GetMailOperationStatus(task.Id, translateMailOperationStatus);
        }

        public List<MailOperationStatus> GetMailOperations(Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var operations = MailOperations.GetTasks().Where(
                    o =>
                        o.GetProperty<int>(MailOperation.TENANT) == TenantProvider.CurrentTenantID &&
                        o.GetProperty<string>(MailOperation.OWNER) == SecurityContext.CurrentAccount.ID.ToString());

            var list = new List<MailOperationStatus>();

            foreach (var o in operations)
            {
                if (string.IsNullOrEmpty(o.Id))
                    continue;

                list.Add(GetMailOperationStatus(o.Id, translateMailOperationStatus));
            }

            return list;
        }

        public MailOperationStatus GetMailOperationStatus(string operationId, Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var defaultResult = new MailOperationStatus
            {
                Id = null,
                Completed = true,
                Percents = 100,
                Status = "",
                Error = "",
                Source = "",
                OperationType = -1
            };

            if (string.IsNullOrEmpty(operationId))
                return defaultResult;

            var operations = MailOperations.GetTasks().ToList();

            foreach (var o in operations)
            {
                if (!string.IsNullOrEmpty(o.InstanseId) &&
                    Process.GetProcesses().Any(p => p.Id == int.Parse(o.InstanseId)))
                    continue;

                o.SetProperty(MailOperation.PROGRESS, 100);
                MailOperations.RemoveTask(o.Id);
            }

            var operation = operations
                .FirstOrDefault(
                    o =>
                        o.GetProperty<int>(MailOperation.TENANT) == TenantProvider.CurrentTenantID &&
                        o.GetProperty<string>(MailOperation.OWNER) == SecurityContext.CurrentAccount.ID.ToString() &&
                        o.Id.Equals(operationId));

            if (operation == null)
                return defaultResult;

            if (DistributedTaskStatus.Running < operation.Status)
            {
                operation.SetProperty(MailOperation.PROGRESS, 100);
                MailOperations.RemoveTask(operation.Id);
            }

            var operationTypeIndex = (int) operation.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);

            var result = new MailOperationStatus
            {
                Id = operation.Id,
                Completed = operation.GetProperty<bool>(MailOperation.FINISHED),
                Percents = operation.GetProperty<int>(MailOperation.PROGRESS),
                Status = translateMailOperationStatus != null
                    ? translateMailOperationStatus(operation)
                    : operation.GetProperty<string>(MailOperation.STATUS),
                Error = operation.GetProperty<string>(MailOperation.ERROR),
                Source = operation.GetProperty<string>(MailOperation.SOURCE),
                OperationType = operationTypeIndex,
                Operation = Enum.GetName(typeof(MailOperationType), operationTypeIndex)
            };

            return result;
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
            var contact1Parts = contact1.Split('<');
            var contact2Parts = contact2.Split('<');

            return contact1Parts.Last().Replace(">", "") == contact2Parts.Last().Replace(">", "");
        }

        public int GetHashCode(string str)
        {
            var strParts = str.Split('<');
            return strParts.Last().Replace(">", "").GetHashCode();
        }
    }
}
