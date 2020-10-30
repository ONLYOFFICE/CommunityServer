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
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Core.Engine.Operations.Base;

namespace ASC.Mail.Core.Engine.Operations
{
    public class MailRecalculateFoldersOperation: MailOperation
    {
        public override MailOperationType OperationType
        {
            get { return MailOperationType.RecalculateFolders; }
        }

        public MailRecalculateFoldersOperation(Tenant tenant, IAccount user)
            : base(tenant, user)
        {
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?) MailOperationRecalculateMailboxProgress.Init, "Setup tenant and user");

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(CurrentUser);

                var engine = new EngineFactory(CurrentTenant.TenantId, CurrentUser.ID.ToString());

                engine.FolderEngine.RecalculateFolders(progress =>
                {
                    SetProgress((int?)progress);
                });
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Mail operation error -> Recalculate folders: {0}", e.ToString());
                Error = "InternalServerError";
            }
        }
    }
}
