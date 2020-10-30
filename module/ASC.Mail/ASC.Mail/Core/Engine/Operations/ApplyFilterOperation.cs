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
using System.Linq;
using ASC.Common.Logging;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums.Filter;

namespace ASC.Mail.Core.Engine.Operations
{
    public class ApplyFilterOperation : MailOperation
    {
        public EngineFactory Factory { get; private set; }

        public MailSieveFilterData Filter { get; set; }

        public ILog Log { get; set; }

        public override MailOperationType OperationType
        {
            get { return MailOperationType.ApplyFilter; }
        }

        public ApplyFilterOperation(Tenant tenant, IAccount user, int filterId)
            : base(tenant, user)
        {
            Factory = new EngineFactory(CurrentTenant.TenantId, CurrentUser.ID.ToString());

            var filter = Factory.FilterEngine.Get(filterId);

            if (filter == null)
                throw new ArgumentException("Filter not found");

            Filter = filter;

            Log = LogManager.GetLogger("ASC.Mail.ApplyFilterOperation");

            SetSource(filter.Id.ToString());
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?)MailOperationApplyFilterProgress.Init, "Setup tenant and user");

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(CurrentUser);

                SetProgress((int?)MailOperationApplyFilterProgress.Filtering, "Filtering");

                long total;
                const int size = 100;
                var page = 0;

                var messages = Factory.MessageEngine.GetFilteredMessages(Filter, page, size, out total);

                while (messages.Any())
                {
                    SetProgress((int?) MailOperationApplyFilterProgress.FilteringAndApplying, "Filtering and applying action");

                    var ids = messages.Select(m => m.Id).ToList();

                    foreach (var action in Filter.Actions)
                    {
                        Factory.FilterEngine.ApplyAction(ids, action);
                    }

                    if(messages.Count < size)
                        break;

                    if (!Filter.Actions.Exists(a => a.Action == ActionType.DeleteForever || a.Action == ActionType.MoveTo))
                    {
                        page++;
                    }

                    messages = Factory.MessageEngine.GetFilteredMessages(Filter, page, size, out total);
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
