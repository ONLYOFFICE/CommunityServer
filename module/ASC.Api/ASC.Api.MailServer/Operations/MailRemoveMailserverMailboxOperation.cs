/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
 * Pursuant to Section 7 ยง 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 ยง 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/

using System;
using System.Linq;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.ComplexOperations.Base;
using ASC.Mail.Server.Administration.Interfaces;

namespace ASC.Api.MailServer.Operations
{
    public class MailRemoveMailserverMailboxOperation : MailOperation
    {
        private readonly IMailbox _mailBox;

        private readonly MailBoxManager _mailBoxManager;

        private readonly MailServerBase _mailServer;

        private readonly IMailServerFactory _mailServerFactory;

        public override MailOperationType OperationType
        {
            get { return MailOperationType.RemoveMailbox; }
        }

        public MailRemoveMailserverMailboxOperation(Tenant tenant, IAccount user, IMailbox mailBox, MailBoxManager mailBoxManager, MailServerBase mailServer, IMailServerFactory mailServerFactory)
            : base(tenant, user)
        {
            _mailBox = mailBox;

            _mailBoxManager = mailBoxManager;

            _mailServer = mailServer;

            _mailServerFactory = mailServerFactory;

            SetSource(_mailBox.Id.ToString());
        }

        protected override void Do()
        {
            try
            {
                SetProgress((int?)MailOperationRemoveMailboxProgress.Init, "Setup tenant and user");

                CoreContext.TenantManager.SetCurrentTenant(_mailBox.Tenant);

                try
                {
                    SecurityContext.AuthenticateMe(_mailBox.Account.TeamlabAccount.ID);
                }
                catch
                {
                    // User was removed
                    SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);
                }             

                var user = _mailBox.Account.TeamlabAccount.ID.ToString();

                SetProgress((int?)MailOperationRemoveMailboxProgress.RemoveFromDb, "Remove mailbox from Db");

                var groups = _mailServer.GetMailGroups(_mailServerFactory);

                var groupsContainsMailbox = groups.Where(g => g.InAddresses.Contains(_mailBox.Address))
                    .Select(g => g);

                foreach (var mailGroup in groupsContainsMailbox)
                {
                    if (mailGroup.InAddresses.Count == 1)
                    {
                        _mailServer.DeleteMailGroup(mailGroup.Id, _mailServerFactory);
                    }
                    else
                    {
                        mailGroup.RemoveMember(_mailBox.Address.Id);
                    }
                }

                _mailServer.DeleteMailbox(_mailBox);

                SetProgress((int?)MailOperationRemoveMailboxProgress.RecalculateFolder, "Recalculate folders counters");

                _mailBoxManager.RecalculateFolders(_mailBox.Tenant, user);

                SetProgress((int?)MailOperationRemoveMailboxProgress.ClearCache, "Clear accounts cache");

                _mailBoxManager.CachedAccounts.Clear(user);
            }
            catch (Exception e)
            {
                Logger.Error("Mail operation error -> Remove mailbox: {0}", e);
                Error = "InternalServerError";
            }
        }
    }
}
