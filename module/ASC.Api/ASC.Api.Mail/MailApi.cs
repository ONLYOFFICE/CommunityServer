/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Globalization;
using System.Linq;

using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Entities;
using ASC.Web.Mail.Resources;

namespace ASC.Api.Mail
{
    ///<summary>
    /// Mail API.
    ///</summary>
    ///<name>mail</name>
    public partial class MailApi : IApiEntryPoint
    {
        private readonly ApiContext _context;

        private EngineFactory _engineFactory;

        private UserActionEngine _actionEngine;

        private ILog _log;

        public const int DEFAULT_PAGE_SIZE = 25;

        public string Name
        {
            get { return "mail"; }
        }

        private EngineFactory MailEngineFactory
        {
            get { return _engineFactory ?? (_engineFactory = new EngineFactory(TenantId, Username, Logger)); }
        }

        private UserActionEngine ActionEngine
        {
            get { return _actionEngine ?? (_actionEngine = new UserActionEngine(TenantId, Username, MailEngineFactory)); }
        }

        private ILog Logger
        {
            get { return _log ?? (_log = LogManager.GetLogger("ASC.Api.Mail")); }
        }

        private static int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private static string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }

        private static CultureInfo CurrentCulture
        {
            get
            {
                var u = CoreContext.UserManager.GetUsers(new Guid(Username));

                var culture = !string.IsNullOrEmpty(u.CultureName)
                    ? u.GetCulture()
                    : CoreContext.TenantManager.GetCurrentTenant().GetCulture();

                return culture;
            }
        }

        /// <summary>
        /// Limit result per Contact System
        /// </summary>
        private static int MailAutocompleteMaxCountPerSystem
        {
            get
            {
                var count = 20;
                if (ConfigurationManagerExtension.AppSettings["mail.autocomplete-max-count"] == null)
                    return count;

                int.TryParse(ConfigurationManagerExtension.AppSettings["mail.autocomplete-max-count"], out count);
                return count;
            }
        }

        /// <summary>
        /// Timeout in milliseconds
        /// </summary>
        private static int MailAutocompleteTimeout
        {
            get
            {
                var count = 3000;
                if (ConfigurationManagerExtension.AppSettings["mail.autocomplete-timeout"] == null)
                    return count;

                int.TryParse(ConfigurationManagerExtension.AppSettings["mail.autocomplete-timeout"], out count);
                return count;
            }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public MailApi(ApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all the running mail operations.
        /// </summary>
        /// <path>api/2.0/mail/operations</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <short>
        /// Get running mail operations
        /// </short>
        /// <category>Operations</category>
        /// <returns type="ASC.Mail.Core.Engine.Operations.Base.MailOperationStatus, ASC.Mail">List of running mail operations</returns>
        [Read("operations")]
        public List<MailOperationStatus> GetMailOperations()
        {
            var list = MailEngineFactory.OperationEngine.GetMailOperations(TranslateMailOperationStatus);
            return list;
        }

        /// <summary>
        /// Returns a status of the mail operation with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a mail operation status
        /// </short>
        /// <category>Operations</category>
        /// <param type="System.String, System" method="url" name="operationId">Operation ID</param>
        /// <returns type="ASC.Mail.Core.Engine.Operations.Base.MailOperationStatus, ASC.Mail">Mail operation status</returns>
        /// <path>api/2.0/mail/operations/{operationId}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("operations/{operationId}")]
        public MailOperationStatus GetMailOperation(string operationId)
        {
            return MailEngineFactory.OperationEngine.GetMailOperationStatus(operationId, TranslateMailOperationStatus);
        }

        /// <summary>
        /// Translates a mail operation status.
        /// </summary>
        /// <short>Translate a mail operation status</short>
        /// <category>Operations</category>
        /// <param type="ASC.Common.Threading.DistributedTask, ASC.Common.Threading" name="op">Distributed task instance</param>
        /// <returns>Translated status</returns>
        private static string TranslateMailOperationStatus(DistributedTask op)
        {
            var type = op.GetProperty<MailOperationType>(MailOperation.OPERATION_TYPE);
            var status = op.GetProperty<string>(MailOperation.STATUS);
            //TODO: Move strings to Resource file
            switch (type)
            {
                case MailOperationType.DownloadAllAttachments:
                {
                    var progress = op.GetProperty<MailOperationDownloadAllAttachmentsProgress>(MailOperation.PROGRESS);
                    switch (progress)
                    {
                        case MailOperationDownloadAllAttachmentsProgress.Init:
                            return MailApiResource.SetupTenantAndUserHeader;
                        case MailOperationDownloadAllAttachmentsProgress.GetAttachments:
                            return MailApiResource.GetAttachmentsHeader;
                        case MailOperationDownloadAllAttachmentsProgress.Zipping:
                            return MailApiResource.ZippingAttachmentsHeader;
                        case MailOperationDownloadAllAttachmentsProgress.ArchivePreparation:
                            return MailApiResource.PreparationArchiveHeader;
                        case MailOperationDownloadAllAttachmentsProgress.CreateLink:
                            return MailApiResource.CreatingLinkHeader;
                        case MailOperationDownloadAllAttachmentsProgress.Finished:
                            return MailApiResource.FinishedHeader;
                        default:
                            return status;
                    }
                }
                case MailOperationType.RemoveMailbox:
                {
                    var progress = op.GetProperty<MailOperationRemoveMailboxProgress>(MailOperation.PROGRESS);
                    switch (progress)
                    {
                        case MailOperationRemoveMailboxProgress.Init:
                            return "Setup tenant and user";
                        case MailOperationRemoveMailboxProgress.RemoveFromDb:
                            return "Remove mailbox from Db";
                        case MailOperationRemoveMailboxProgress.FreeQuota:
                            return "Decrease newly freed quota space";
                        case MailOperationRemoveMailboxProgress.RecalculateFolder:
                            return "Recalculate folders counters";
                        case MailOperationRemoveMailboxProgress.ClearCache:
                            return "Clear accounts cache";
                        case MailOperationRemoveMailboxProgress.Finished:
                            return "Finished";
                        default:
                            return status;
                    }
                }
                case MailOperationType.RecalculateFolders:
                {
                    var progress = op.GetProperty<MailOperationRecalculateMailboxProgress>(MailOperation.PROGRESS);
                    switch (progress)
                    {
                        case MailOperationRecalculateMailboxProgress.Init:
                            return "Setup tenant and user";
                        case MailOperationRecalculateMailboxProgress.CountUnreadMessages:
                            return "Calculate unread messages";
                        case MailOperationRecalculateMailboxProgress.CountTotalMessages:
                            return "Calculate total messages";
                        case MailOperationRecalculateMailboxProgress.CountUreadConversation:
                            return "Calculate unread conversations";
                        case MailOperationRecalculateMailboxProgress.CountTotalConversation:
                            return "Calculate total conversations";
                        case MailOperationRecalculateMailboxProgress.UpdateFoldersCounters:
                            return "Update folders counters";
                        case MailOperationRecalculateMailboxProgress.CountUnreadUserFolderMessages:
                            return "Calculate unread messages in user folders";
                        case MailOperationRecalculateMailboxProgress.CountTotalUserFolderMessages:
                            return "Calculate total messages in user folders";
                        case MailOperationRecalculateMailboxProgress.CountUreadUserFolderConversation:
                            return "Calculate unread conversations in user folders";
                        case MailOperationRecalculateMailboxProgress.CountTotalUserFolderConversation:
                            return "Calculate total conversations in user folders";
                        case MailOperationRecalculateMailboxProgress.UpdateUserFoldersCounters:
                            return "Update user folders counters";
                        case MailOperationRecalculateMailboxProgress.Finished:
                            return "Finished";
                        default:
                            return status;
                    }
                }
                case MailOperationType.RemoveUserFolder:
                {
                    var progress = op.GetProperty<MailOperationRemoveUserFolderProgress>(MailOperation.PROGRESS);
                    switch (progress)
                    {
                        case MailOperationRemoveUserFolderProgress.Init:
                            return "Setup tenant and user";
                        case MailOperationRemoveUserFolderProgress.MoveMailsToTrash:
                            return "Move mails into Trash folder";
                        case MailOperationRemoveUserFolderProgress.DeleteFolders:
                            return "Delete folder";
                        case MailOperationRemoveUserFolderProgress.Finished:
                            return "Finished";
                        default:
                            return status;
                    }
                }
                default:
                    return status;
            }
        }
    }
}
