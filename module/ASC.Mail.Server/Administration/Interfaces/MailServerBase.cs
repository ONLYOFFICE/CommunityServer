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

        public abstract IWebDomain CreateWebDomain(string name, bool isVerified, IMailServerFactory factory);
        public abstract void DeleteWebDomain(IWebDomain webDomain, IMailServerFactory factory);
        public abstract ICollection<IWebDomain> GetWebDomains(IMailServerFactory factory);
        public abstract IWebDomain GetWebDomain(int domainId, IMailServerFactory factory);
        public abstract bool IsDomainExists(string name);

        public abstract IMailGroup CreateMailGroup(string groupName, IWebDomain domain, List<int> addressIds, IMailServerFactory factory);
        public abstract IMailGroup GetMailGroup(int id, IMailServerFactory factory);
        public abstract void DeleteMailGroup(int mailgroupId, IMailServerFactory factory);
        public abstract ICollection<IMailGroup> GetMailGroups(IMailServerFactory factory);

        public abstract IMailbox CreateMailbox(string name, string localpart, string password, IWebDomain domain, IMailAccount account, IMailServerFactory factory);
        public abstract void UpdateMailbox(IMailbox mailbox, string name, IMailServerFactory factory);
        public abstract void DeleteMailbox(IMailbox mailbox);
        public abstract ICollection<IMailbox> GetMailboxes(IMailServerFactory factory);
        public abstract IMailbox GetMailbox(int mailboxId, IMailServerFactory factory);

        public abstract INotificationAddress CreateNotificationAddress(string localpart, string password, IWebDomain domain, IMailServerFactory factory);
        public abstract void DeleteNotificationAddress(string address);

        public abstract IDnsSettings GetFreeDnsRecords(IMailServerFactory factory);
        

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