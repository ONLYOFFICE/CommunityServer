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


using System;
using System.Configuration;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Server.Administration.Interfaces;

namespace ASC.Mail.Server.Administration.TestCases
{
    public abstract class TestContextBase
    {
        public abstract IMailServerFactory ServerFactory { get; }

        public MailServerBase CreateServer()
        {
            var serverConnectionString = HttpUtility.UrlDecode(ConfigurationManager.AppSettings["ConnectionString"]);
            var tenantId = int.Parse(ConfigurationManager.AppSettings["TenantId"]);
            var userId = ConfigurationManager.AppSettings["TeamlabUserGuid"].ToString(CultureInfo.InvariantCulture);
            var serverId = int.Parse(ConfigurationManager.AppSettings["ServerId"]);
            var logger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Null, string.Empty);

            CoreContext.TenantManager.SetCurrentTenant(tenantId);

            var limits = new ServerLimits.Builder()
                        .SetMailboxMaxCountPerUser(10)
                        .Build();

            var setup = new ServerSetup
                .Builder(serverId, tenantId, userId)
                .SetConnectionString(serverConnectionString)
                .SetLogger(logger)
                .SetServerLimits(limits)
                .Build();

            return ServerFactory.CreateServer(setup);
        }

        public IMailAccount GetMailAccount(string localPart, string domainName)
        {
            var userGuid = ConfigurationManager.AppSettings["TeamlabUserGuid"];
            var login = String.Format("{0}@{1}", localPart, domainName);
            return ServerFactory.CreateMailAccount(CoreContext.Authentication.GetAccountByID(new Guid(userGuid)), login);
        }

        public static int unique_id = 0;

        public string CreateNewPeterName()
        {
            return "peter" + unique_id++;
        }

        public IMailAddress CreateRandomMailAddress()
        {
            var domain = ServerFactory.CreateWebDomain(1, 0, "testov.test", true, CreateServer());
            var address = CreateRandomMailAddress(domain);
            return address;
        }

        public IMailAddress CreateRandomMailAddress(IWebDomain domain)
        {
            var address = ServerFactory.CreateMailAddress(1, 0, CreateNewPeterName(), domain);
            return address;
        }
    }
}