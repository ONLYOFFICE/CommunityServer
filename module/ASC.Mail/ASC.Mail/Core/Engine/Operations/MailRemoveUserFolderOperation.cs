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

namespace ASC.Mail.Core.Engine.Operations
{
    public class MailRemoveUserFolderOperation : MailOperation
    {
        private readonly uint _userFolderId;
        private readonly EngineFactory _engineFactory;

        public ILog Log { get; set; }

        public override MailOperationType OperationType
        {
            get { return MailOperationType.RemoveUserFolder; }
        }

        public MailRemoveUserFolderOperation(Tenant tenant, IAccount user, uint userFolderId)
            : base(tenant, user)
        {
            _userFolderId = userFolderId;

            _engineFactory = new EngineFactory(CurrentTenant.TenantId, CurrentUser.ID.ToString());

            Log = LogManager.GetLogger("ASC.Mail.RemoveUserFolderOperation");

            SetSource(userFolderId.ToString());
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?) MailOperationRemoveUserFolderProgress.Init, "Setup tenant and user");

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(CurrentUser);

                SetProgress((int?) MailOperationRemoveUserFolderProgress.DeleteFolders, "Delete folders");

                _engineFactory.UserFolderEngine.Delete(_userFolderId);

                SetProgress((int?) MailOperationRemoveUserFolderProgress.Finished);
            }
            catch (Exception e)
            {
                Logger.Error("Mail operation error -> Remove user folder: {0}", e);
                Error = "InternalServerError";
            }
        }
    }
}
