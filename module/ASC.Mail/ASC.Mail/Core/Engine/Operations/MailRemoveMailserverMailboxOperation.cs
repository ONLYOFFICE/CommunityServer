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
using System.Data;
using System.IO;
using System.Linq;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Server.Core.Entities;

namespace ASC.Mail.Core.Engine.Operations
{
    public class MailRemoveMailserverMailboxOperation : MailOperation
    {
        private readonly MailBoxData _mailBox;

        public override MailOperationType OperationType
        {
            get { return MailOperationType.RemoveMailbox; }
        }

        public MailRemoveMailserverMailboxOperation(Tenant tenant, IAccount user, MailBoxData mailBox)
            : base(tenant, user)
        {
            _mailBox = mailBox;
            SetSource(_mailBox.MailBoxId.ToString());
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?)MailOperationRemoveMailboxProgress.Init, "Setup tenant and user");

                var tenant = _mailBox.TenantId;
                var user = _mailBox.UserId;

                CoreContext.TenantManager.SetCurrentTenant(tenant);

                try
                {
                    SecurityContext.AuthenticateMe(new Guid(user));
                }
                catch
                {
                    // User was removed
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                }

                SetProgress((int?)MailOperationRemoveMailboxProgress.RemoveFromDb, "Remove mailbox from Db");

                var engine = new EngineFactory(tenant);

                engine.ServerMailboxEngine.RemoveMailbox(_mailBox);

                SetProgress((int?)MailOperationRemoveMailboxProgress.RecalculateFolder, "Recalculate folders counters");

                engine.OperationEngine.RecalculateFolders();

                SetProgress((int?)MailOperationRemoveMailboxProgress.ClearCache, "Clear accounts cache");

                CacheEngine.Clear(user);

                SetProgress((int?)MailOperationRemoveMailboxProgress.RemoveIndex, "Remove Elastic Search index by messages");

                engine.IndexEngine.Remove(_mailBox);
            }
            catch (Exception e)
            {
                Logger.Error("Mail operation error -> Remove mailbox: {0}", e);
                Error = "InternalServerError";
            }
        }
    }
}
