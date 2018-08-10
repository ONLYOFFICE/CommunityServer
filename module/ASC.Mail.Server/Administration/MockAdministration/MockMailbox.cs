/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System.Collections.Generic;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;

namespace ASC.Mail.Server.MockAdministration
{
    internal class MockMailbox : MailboxModel
    {
        private readonly MockServerData _serverData;

        public MockMailbox(int id, int tenant, IMailAddress address, string name, IMailAccount account, 
            List<IMailAddress> aliases, MailServerBase server) 
            : base(id, tenant, address, name, account, aliases, server)
        {
            _serverData = MockServerData.GetInstance();
        }

        protected override void _AddAlias(MailAddressBase aliasToAdd)
        {
            var mailbox = _serverData.Mailboxes.Find(m => m.Address.ToString().Equals(Address.ToString()));
            mailbox.Aliases.Add(aliasToAdd);
        }

        protected override void _RemoveAlias(MailAddressBase aliasToRemove)
        {
            var mailbox = _serverData.Mailboxes.Find(m => m.Address.ToString().Equals(Address.ToString()));
            mailbox.Aliases.Remove(aliasToRemove);
        }
    }
}