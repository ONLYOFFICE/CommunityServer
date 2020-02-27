/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
