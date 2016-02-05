/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Common.Security.Authentication;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;

namespace ASC.Mail.Server.MockAdministration
{
    public class MockFactory : MailServerFactoryBase
    {
        public override MailServerBase CreateServer(ServerSetup setup)
        {
            return new MockServer(setup);
        }

        public override IMailAddress CreateMailAddress(int id, int tenant, string name, IWebDomain domain)
        {
            return new MockAddress(id, tenant, name, domain);
        }

        public override IWebDomain CreateWebDomain(int id, int tenant, string name, bool isVerified, MailServerBase server)
        {
            return new MockDomain(id, tenant, name, isVerified, server);
        }


        public override IMailbox CreateMailbox(int id, int tenant, IMailAddress address, IMailAccount account, List<IMailAddress> aliases, MailServerBase server)
        {
            return new MockMailbox(id, tenant, address, account, aliases, server);
        }

        public override IMailGroup CreateMailGroup(int id, int tenant, IMailAddress address, List<IMailAddress> inAddresses, MailServerBase server)
        {
            return new MockMailGroup(id, tenant, address, inAddresses, server);
        }

        public override IMailAccount CreateMailAccount(IAccount teamlabAccount, string login)
        {
            return new MockAccount(teamlabAccount, login);
        }

        public override IDnsSettings CreateDnsSettings(int id, int tenant, string user, string domainName, string dkimSelector, string dkimPrivateKey,
                                string dkimPublicKey, string domainCheckName, string domainCheckRecord,
                                string spfName, string spfRecord, string mxHost, int mxPriority, ILogger logger = null)
        {
            return new MockDnsSettings(id, tenant, user, domainName, dkimSelector, dkimPrivateKey, dkimPublicKey, domainCheckName,
            domainCheckRecord, spfName, spfRecord, mxHost, mxPriority, logger);
        }

        public override INotificationAddress CreateNotificationAddress(string localPart, IWebDomain domain, string smtpServer, int smtpPort,
                                                                       string smtpAccount, bool smtpAuth, EncryptionType smptEncryptionType,
                                                                       AuthenticationType smtpAuthenticationType)
        {
            return new MockNotificationAddress(localPart, domain, smtpServer, smtpPort,
                   smtpAccount, smtpAuth, smptEncryptionType,
                   smtpAuthenticationType);
        }
    }
}
