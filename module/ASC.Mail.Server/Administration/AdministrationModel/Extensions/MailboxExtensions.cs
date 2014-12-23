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
using System.Linq;
using ASC.Common.Security.Authentication;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Dal;

namespace ASC.Mail.Server.Administration.ServerModel.Extensions
{
    public static class MailboxExtensions
    {
        public static IMailbox CompleteMailbox(this IMailbox mailbox, 
            MailboxDto mailbox_dto, MailAddressDto mailbox_address_dto,
            List<MailAddressDto> list_alias_dto, MailServerBase server)
        {
            mailbox.Id = mailbox_dto.id;
            mailbox.Tenant = server.Tenant;
            mailbox.DateCreated = mailbox_dto.date_created;

            mailbox.Account.Login = mailbox_dto.address;
            mailbox.Account.Password = "";
            mailbox.Account.TeamlabAccount = new Account(new Guid(mailbox_dto.user_id), "", false);
            mailbox.Account.ServerInformation.ConnectionString = server.ConnectionString;

            mailbox.Address.Id = mailbox_address_dto.id;
            mailbox.Address.Name = mailbox_address_dto.name;
            mailbox.Address.Tenant = mailbox_address_dto.tenant;
            mailbox.Address.DateCreated = mailbox_address_dto.date_created;

            mailbox.Address.Domain.Id = mailbox_address_dto.Domain.id;
            mailbox.Address.Domain.Name = mailbox_address_dto.Domain.name;
            mailbox.Address.Domain.DateCreated = mailbox_address_dto.Domain.date_added;

            foreach (var server_alias in mailbox.Aliases)
            {
                var alias = list_alias_dto.Find(d => d.name == server_alias.Name);
                server_alias.Id = alias.id;
                server_alias.Name = alias.name;
                server_alias.Tenant = alias.tenant;
                server_alias.Domain.Id = alias.Domain.id;
                server_alias.Domain.Name = alias.Domain.name;
                server_alias.Domain.DateCreated = alias.Domain.date_added;
            }

            mailbox.Server = server;

            return mailbox;
        }
    }
}
