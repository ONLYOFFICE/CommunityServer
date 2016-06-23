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
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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

        public MockMailGroup(int id, int tenant, IMailAddress address, List<IMailAddress> inAddresses, MailServerBase server) 
            : base(id, tenant, address, inAddresses, server)
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