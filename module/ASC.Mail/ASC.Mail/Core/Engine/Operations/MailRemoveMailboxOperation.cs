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
using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Core.Engine.Operations
{
    public class MailRemoveMailboxOperation: MailOperation
    {
        private readonly MailBoxData _mailBoxData;

        public ILog Log { get; set; }

        public override MailOperationType OperationType
        {
            get { return MailOperationType.RemoveMailbox; }
        }

        public MailRemoveMailboxOperation(Tenant tenant, IAccount user, MailBoxData mailBoxData)
            : base(tenant, user)
        {
            _mailBoxData = mailBoxData;

            Log = LogManager.GetLogger("ASC.Mail.RemoveMailboxOperation");

            SetSource(_mailBoxData.MailBoxId.ToString());
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?) MailOperationRemoveMailboxProgress.Init, "Setup tenant and user");

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(CurrentUser);

                var engine = new EngineFactory(_mailBoxData.TenantId, _mailBoxData.UserId);

                SetProgress((int?)MailOperationRemoveMailboxProgress.RemoveFromDb, "Remove mailbox from Db");

                var freedQuotaSize = engine.MailboxEngine.RemoveMailBoxInfo(_mailBoxData);

                SetProgress((int?)MailOperationRemoveMailboxProgress.FreeQuota, "Decrease newly freed quota space");

                engine.QuotaEngine.QuotaUsedDelete(freedQuotaSize);

                SetProgress((int?)MailOperationRemoveMailboxProgress.RecalculateFolder, "Recalculate folders counters");

                engine.FolderEngine.RecalculateFolders();

                SetProgress((int?)MailOperationRemoveMailboxProgress.ClearCache, "Clear accounts cache");

                CacheEngine.Clear(_mailBoxData.UserId);

                SetProgress((int?)MailOperationRemoveMailboxProgress.RemoveIndex, "Remove Elastic Search index by messages");

                engine.IndexEngine.Remove(_mailBoxData);

                SetProgress((int?)MailOperationRemoveMailboxProgress.Finished);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Mail operation error -> Remove mailbox: {0}", e.ToString());
                Error = "InternalServerError";
            }
        }

        
    }
}
