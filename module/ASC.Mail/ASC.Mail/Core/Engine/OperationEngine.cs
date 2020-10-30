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
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Mail.Core.Engine.Operations;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Web.Studio.Utility;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Mail.Core.Engine
{
    public class OperationEngine
    {
        private readonly DistributedTaskQueue _mailOperations;

        public DistributedTaskQueue MailOperations
        {
            get { return _mailOperations; }
        }

        public OperationEngine()
        {
            _mailOperations = new DistributedTaskQueue("mailOperations", Defines.MailOperationsLimit);
        }

        public MailOperationStatus RemoveMailbox(MailBoxData mailbox,
            Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.RemoveMailbox;
                })
                .ToList();

            var sameOperation = operations.FirstOrDefault(o =>
            {
                var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                return oSource == mailbox.MailBoxId.ToString();
            });

            if (sameOperation != null)
            {
                return GetMailOperationStatus(sameOperation.Id, translateMailOperationStatus);
            }

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                throw new MailOperationAlreadyRunningException("Remove mailbox operation already running.");

            var op = new MailRemoveMailboxOperation(tenant, user, mailbox);

            return QueueTask(op, translateMailOperationStatus);
        }

        public MailOperationStatus DownloadAllAttachments(int messageId,
            Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.DownloadAllAttachments;
                })
                .ToList();

            var sameOperation = operations.FirstOrDefault(o =>
            {
                var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                return oSource == messageId.ToString();
            });

            if (sameOperation != null)
            {
                return GetMailOperationStatus(sameOperation.Id, translateMailOperationStatus);
            }

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                throw new MailOperationAlreadyRunningException("Download all attachments operation already running.");

            var op = new MailDownloadAllAttachmentsOperation(tenant, user, messageId);

            return QueueTask(op, translateMailOperationStatus);
        }

        public MailOperationStatus RecalculateFolders(Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.RecalculateFolders;
                });

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                return GetMailOperationStatus(runningOperation.Id, translateMailOperationStatus);

            var op = new MailRecalculateFoldersOperation(tenant, user);

            return QueueTask(op, translateMailOperationStatus);
        }

        public MailOperationStatus CheckDomainDns(string domainName, ServerDns dns,
            Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.CheckDomainDns &&
                           oSource == domainName;
                });

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                return GetMailOperationStatus(runningOperation.Id, translateMailOperationStatus);

            var op = new MailCheckMailserverDomainsDnsOperation(tenant, user, domainName, dns);

            return QueueTask(op, translateMailOperationStatus);
        }

        public MailOperationStatus RemoveUserFolder(uint userFolderId,
            Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.RemoveUserFolder;
                })
                .ToList();

            var sameOperation = operations.FirstOrDefault(o =>
            {
                var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                return oSource == userFolderId.ToString();
            });

            if (sameOperation != null)
            {
                return GetMailOperationStatus(sameOperation.Id, translateMailOperationStatus);
            }

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                throw new MailOperationAlreadyRunningException("Remove user folder operation already running.");

            var op = new MailRemoveUserFolderOperation(tenant, user, userFolderId);

            return QueueTask(op, translateMailOperationStatus);
        }

        public MailOperationStatus ApplyFilter(int filterId,
            Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var operations = MailOperations.GetTasks()
                .Where(o =>
                {
                    var oTenant = o.GetProperty<int>(MailOperation.TENANT);
                    var oUser = o.GetProperty<string>(MailOperation.OWNER);
                    var oType = o.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
                    return oTenant == tenant.TenantId &&
                           oUser == user.ID.ToString() &&
                           oType == MailOperationType.ApplyFilter;
                })
                .ToList();

            var sameOperation = operations.FirstOrDefault(o =>
            {
                var oSource = o.GetProperty<string>(MailOperation.SOURCE);
                return oSource == filterId.ToString();
            });

            if (sameOperation != null)
            {
                return GetMailOperationStatus(sameOperation.Id, translateMailOperationStatus);
            }

            var runningOperation = operations.FirstOrDefault(o => o.Status <= DistributedTaskStatus.Running);

            if (runningOperation != null)
                throw new MailOperationAlreadyRunningException("Apply filter operation already running.");

            var op = new ApplyFilterOperation(tenant, user, filterId);

            return QueueTask(op, translateMailOperationStatus);
        }

        public MailOperationStatus ApplyFilters(List<int> ids,
            Func<DistributedTask, string> translateMailOperationStatus = null)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = SecurityContext.CurrentAccount;

            var op = new ApplyFiltersOperation(tenant, user, ids);

            return QueueTask(op, translateMailOperationStatus);
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

            var operationTypeIndex = (int)operation.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);

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
    }
}
