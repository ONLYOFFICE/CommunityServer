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
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Engine.Operations
{
    public class ApplyFiltersOperation : MailOperation
    {
        public List<int> Ids { get; private set; }

        public EngineFactory Factory { get; private set; }

        public ILog Log { get; set; }

        public override MailOperationType OperationType
        {
            get { return MailOperationType.ApplyAnyFilters; }
        }

        public ApplyFiltersOperation(Tenant tenant, IAccount user, List<int> ids)
            : base(tenant, user)
        {
            Ids = ids;

            if (ids == null || !ids.Any())
                throw new ArgumentException("No ids");

            Factory = new EngineFactory(CurrentTenant.TenantId, CurrentUser.ID.ToString());

            Log = LogManager.GetLogger("ASC.Mail.ApplyFiltersOperation");
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?)MailOperationApplyFilterProgress.Init, "Setup tenant and user");

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(CurrentUser);

                SetProgress((int?)MailOperationApplyFilterProgress.Filtering, "Filtering");

                var filters = Factory.FilterEngine.GetList();

                if (!filters.Any())
                {
                    SetProgress((int?)MailOperationApplyFilterProgress.Finished);

                    return;
                }

                SetProgress((int?)MailOperationApplyFilterProgress.FilteringAndApplying, "Filtering and applying action");

                var mailboxes = new List<MailBoxData>();

                var index = 0; 
                var max = Ids.Count;

                foreach (var id in Ids)
                {
                    var progressState= string.Format("Message id = {0} ({1}/{2})", id, ++index, max);

                    try
                    {
                        SetSource(progressState);

                        var message = Factory.MessageEngine.GetMessage(id, new MailMessageData.Options());

                        if (message.Folder != FolderType.Spam && message.Folder != FolderType.Sent && message.Folder != FolderType.Inbox)
                            continue;

                        var mailbox = mailboxes.FirstOrDefault(mb => mb.MailBoxId == message.MailboxId);

                        if (mailbox == null)
                        {
                            mailbox =
                                Factory.MailboxEngine.GetMailboxData(new ConcreteSimpleMailboxExp(message.MailboxId));

                            if (mailbox == null)
                                continue;

                            mailboxes.Add(mailbox);
                        }

                        Factory.FilterEngine.ApplyFilters(message, mailbox, new MailFolder(message.Folder, ""), filters);

                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Error processing: {0}. Exception: {1}", progressState, ex);
                    }
                }

                SetProgress((int?) MailOperationApplyFilterProgress.Finished);
            }
            catch (Exception e)
            {
                Logger.Error("Mail operation error -> Remove user folder: {0}", e);
                Error = "InternalServerError";
            }
        }
    }
}
