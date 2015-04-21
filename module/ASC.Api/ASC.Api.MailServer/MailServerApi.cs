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


using ASC.Core;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.MockAdministration;
using ASC.Mail.Server.PostfixAdministration;
using System;
using System.Configuration;
using SecurityContext = ASC.Core.SecurityContext;
using ServerType = ASC.Mail.Server.Dal.ServerType;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi : Interfaces.IApiEntryPoint
    {
        private MailServerBase _mailServer;
        private ServerDal _serverDal;
        private IMailServerFactory _mailserverfactory;
        private ILogger _log;

        private ILogger Logger
        {
            get
            {
                return _log ?? (_log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api"));
            }
        }

        private IMailServerFactory MailServerFactory
        {
            get
            {
                if (_mailserverfactory == null)
                {
                    var serverInfo = TenantServerDal.GetTenantServer();
                    switch ((ServerType)serverInfo.type)
                    {
                        case ServerType.MockServer:
                            _mailserverfactory = new MockFactory();
                            break;
                        case ServerType.Postfix:
                            _mailserverfactory = new PostfixFactory();
                            break;
                        default:
                            throw new ArgumentException("Server_type creation wasn't added. Server_type: " +
                                                        (ServerType) serverInfo.type);
                    }
                }

                return _mailserverfactory;
            }
        }

        private MailServerBase MailServer
        {
            get
            {
                if (_mailServer == null)
                {
                    var serverData = TenantServerDal.GetTenantServer();

                    var limits = new ServerLimits.Builder()
                        .SetMailboxMaxCountPerUser(MailboxPerUserLimit)
                        .Build();

                    var dnsPresets = new DnsPresets.Builder()
                        .SetMx(serverData.mx_record, MxRecordPriority)
                        .SetSpfValue(SpfRecordValue)
                        .SetDkimSelector(DkimSelector)
                        .SetDomainCheckPrefix(DomainCheckPrefix)
                        .Build();

                    var setup = new ServerSetup
                        .Builder(serverData.id, TenantId, UserId)
                        .SetConnectionString(serverData.connection_string)
                        .SetLogger(Logger)
                        .SetServerLimits(limits)
                        .SetDnsPresets(dnsPresets)
                        .Build();

                    _mailServer = MailServerFactory.CreateServer(setup);
                }

                return _mailServer;
            }
        }

        private ServerDal TenantServerDal
        {
            get
            {
                return _serverDal ?? (_serverDal = new ServerDal(TenantId));
            }
        }

        ///<summary>
        /// Api name entry
        ///</summary>
        public string Name
        {
            get { return "mailserver"; }
        }

        private int TenantId
        {
            get
            {
                return CoreContext.TenantManager.GetCurrentTenant().TenantId;
            }
        }

        private string UserId
        {
            get 
            { 
                return SecurityContext.CurrentAccount.ID.ToString();
            }
        }

        protected bool IsAdmin
        {
            get
            {
                return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Core.Users.Constants.GroupAdmin.ID);
            }
        }

        protected string TxtRecordName
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.server.txt-record-name"];
            }
        }

        protected string SpfRecordValue
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.server.spf-record-value"];
            }
        }

        protected int MxRecordPriority
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["mail.server.mx-record-priority"]);
            }
        }

        protected int MailboxPerUserLimit
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["mail.server-mailbox-limit-per-user"]);
            }
        }

        protected string DkimSelector
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.server-dkim-selector"];
            }
        }

        protected string DomainCheckPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings["mail.server-dns-check-prefix"];
            }
        }
    }
}
