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
using System.Data;
using System.Linq;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;

namespace ASC.Mail.Server.MockAdministration
{
    internal class MockMailGroup : MailGroupModel
    {
        private readonly MockServerData _serverData;

        public MockMailGroup(int id, int tenant, IMailAddress address, List<IMailAddress> in_addresses, MailServerBase server) 
            : base(id, tenant, address, in_addresses, server)
        {
            _serverData = MockServerData.GetInstance();
        }

        protected override void _AddMember(MailAddressBase address)
        {
            var group = _serverData.Groups.Find(g => g.Address.ToString().Equals(Address.ToString()));
            if (group.InAddresses.Any(a => a.Equals(address)))
            {
                throw new DuplicateNameException("You want to add already existed address");
            }
            group.InAddresses.Add(address);
        }


        protected override void _RemoveMember(MailAddressBase address)
        {
            var group = _serverData.Groups.Find(g => g.Address.ToString().Equals(Address.ToString()));
            if (group.InAddresses.All(a => !Equals(address.ToString(), a.ToString())))
                throw new ArgumentException();
            group.InAddresses.Remove(address);
        }

        protected override ICollection<MailAddressBase> _GetMembers()
        {
            var group = _serverData.Groups.Find(g => g.Address.ToString().Equals(Address.ToString()));
            return group.InAddresses;
        }
    }
}