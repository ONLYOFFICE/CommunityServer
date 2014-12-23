/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Security;
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Core;
using ASC.Mail.Server.Utils;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Create mailbox
        /// </summary>
        /// <param name="name"></param>
        /// <param name="domain_id"></param>
        /// <param name="user_id"></param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Create mailbox</short> 
        /// <category>Mailboxes</category>
        [Create(@"mailboxes/add")]
        public MailboxData CreateMailbox(string name, int domain_id, string user_id)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid mailbox name.", "name");

            if (domain_id < 0)
                throw new ArgumentException("Invalid domain id.", "domain_id");

            Guid user;

            if(!Guid.TryParse(user_id, out user))
                throw new ArgumentException("Invalid user id.", "user_id");

            var teamlab_account = CoreContext.Authentication.GetAccountByID(user);

            if (teamlab_account == null)
                throw new InvalidDataException("Unknown user.");

            if (name.Length > 64)
                throw new ArgumentException("Local part of mailbox localpart exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException("Incorrect mailbox name.");

            var mailbox_name = name.ToLowerInvariant();

            var domain = MailServer.GetWebDomain(domain_id, MailServerFactory);

            var login = string.Format("{0}@{1}", mailbox_name, domain.Name);

            var rand = new Random();
            var password = Membership.GeneratePassword(12, 0);
            password = Regex.Replace(password, @"[^a-zA-Z0-9]", m => rand.Next(10).ToString());

            var account = MailServerFactory.CreateMailAccount(teamlab_account, login);

            var mailbox = MailServer.CreateMailbox(mailbox_name, password, domain, account, MailServerFactory);

            return mailbox.ToMailboxData();

        }

        /// <summary>
        ///    Returns list of the mailboxes associated with tenant
        /// </summary>
        /// <returns>List of MailboxData for current tenant</returns>
        /// <short>Get mailboxes list</short> 
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/get")]
        public List<MailboxData> GetMailboxes()
        {
            var mailboxes = MailServer.GetMailboxes(MailServerFactory);
            return mailboxes
                .Select(mailbox => mailbox.ToMailboxData())
                .ToList();
        }

        /// <summary>
        ///    Deletes the selected mailbox
        /// </summary>
        /// <param name="id">id of mailbox</param>
        /// <returns>id of mailbox</returns>
        /// <short>Remove mailbox from mail server</short> 
        /// <category>Mailboxes</category>
        [Delete(@"mailboxes/remove/{id}")]
        public int RemoveMailbox(int id)
        {
            if (id < 0)
                throw new ArgumentException("Invalid domain id.", "id");

            var mailbox = MailServer.GetMailbox(id, MailServerFactory);

            var groups = MailServer.GetMailGroups(MailServerFactory);

            var groups_contains_mailbox = groups.Where(g => g.InAddresses.Contains(mailbox.Address))
                  .Select(g => g);

            foreach (var mail_group in groups_contains_mailbox)
            {
                if (mail_group.InAddresses.Count == 1)
                {
                    MailServer.DeleteMailGroup(mail_group.Id, MailServerFactory);
                }
                else
                {
                    mail_group.RemoveMember(mailbox.Address.Id);
                }
            }

            MailServer.DeleteMailbox(mailbox);

            return id;
        }

        /// <summary>
        ///    Add alias to mailbox
        /// </summary>
        /// <param name="mailbox_id">id of mailbox</param>
        /// <param name="alias_name">name of alias</param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Add mailbox's aliases</short>
        /// <category>AddressData</category>
        [Update(@"mailboxes/alias/add")]
        public AddressData AddMailboxAlias(int mailbox_id, string alias_name)
        {
            if (string.IsNullOrEmpty(alias_name))
                throw new ArgumentException("Invalid alias name.", "alias_name");

            if (mailbox_id < 0)
                throw new ArgumentException("Invalid mailbox id.", "mailbox_id");

            if (alias_name.Length > 64)
                throw new ArgumentException("Local part of mailbox alias exceed limitation of 64 characters.", "alias_name");

            if (!Parser.IsEmailLocalPartValid(alias_name))
                throw new ArgumentException("Incorrect mailbox alias.");

            var mailbox = MailServer.GetMailbox(mailbox_id, MailServerFactory);

            var mailbox_alias_name = alias_name.ToLowerInvariant();

            var alias = mailbox.AddAlias(mailbox_alias_name, mailbox.Address.Domain, MailServerFactory);

            return alias.ToAddressData();
        }

        /// <summary>
        ///    Remove alias from mailbox
        /// </summary>
        /// <param name="mailbox_id">id of mailbox</param>
        /// <param name="address_id"></param>
        /// <returns>id of mailbox</returns>
        /// <short>Remove mailbox's aliases</short>
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/alias/remove")]
        public int RemoveMailboxAlias(int mailbox_id, int address_id)
        {
            if (address_id < 0)
                throw new ArgumentException("Invalid address id.", "address_id");

            if (mailbox_id < 0)
                throw new ArgumentException("Invalid mailbox id.", "mailbox_id");

            var mailbox = MailServer.GetMailbox(mailbox_id, MailServerFactory);

            mailbox.RemoveAlias(address_id);

            return mailbox_id;
        }
    }
}
