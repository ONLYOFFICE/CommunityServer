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

using System.Collections.Generic;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Server.Administration.Interfaces
{
    public abstract class MailServerBase : ITeamlabObject
    {
        private readonly ServerSetup _setup;

        protected MailServerBase(ServerSetup setup)
        {
            _setup = setup;
        }

        public int Id
        {
            get { return _setup.ServerId; }
        }

        public int Tenant
        {
            get { return _setup.Tenant; }
        }

        public string User
        {
            get { return _setup.User; }
        }

        public string ConnectionString
        {
            get { return _setup.ConnectionString; }
        }

        public ILogger Logger
        {
            get { return _setup.Logger; }
        }

        public ServerSetup SetupInfo
        {
            get { return _setup; }
        }

        public abstract IWebDomain CreateWebDomain(string name, bool is_verified, IMailServerFactory factory);
        public abstract void DeleteWebDomain(IWebDomain web_domain, IMailServerFactory factory);
        public abstract ICollection<IWebDomain> GetWebDomains(IMailServerFactory factory);
        public abstract IWebDomain GetWebDomain(int id, IMailServerFactory factory);

        public abstract IMailGroup CreateMailGroup(string group_name, IWebDomain domain, List<int> address_ids, IMailServerFactory factory);
        public abstract IMailGroup GetMailGroup(int id, IMailServerFactory factory);
        public abstract void DeleteMailGroup(int id, IMailServerFactory factory);
        public abstract ICollection<IMailGroup> GetMailGroups(IMailServerFactory factory);

        public abstract IMailbox CreateMailbox(string localpart, string password, IWebDomain domain, IMailAccount account, IMailServerFactory factory);
        public abstract void UpdateMailbox(IMailbox mailbox);
        public abstract void DeleteMailbox(IMailbox mailbox);
        public abstract ICollection<IMailbox> GetMailboxes(IMailServerFactory factory);
        public abstract IMailbox GetMailbox(int mailbox_id, IMailServerFactory factory);
        public abstract IDnsSettings GetFreeDnsRecords(IMailServerFactory factory);
        public abstract bool IsDomainExists(string name);

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MailServerBase))
            {
                return false;
            }

            var other = (MailServerBase)obj;

            return SetupInfo == other.SetupInfo;
        }

        public override int GetHashCode()
        {
            return SetupInfo.GetHashCode();
        }
    }
}