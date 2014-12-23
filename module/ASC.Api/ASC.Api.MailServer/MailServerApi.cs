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
using System.Configuration;
using System.Security;
using ASC.Api.Impl;
using ASC.Core;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.MockAdministration;
using ASC.Mail.Server.PostfixAdministration;
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
                if (!IsAdmin)
                    throw new SecurityException("Need admin privileges.");

                return _log ?? (_log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api"));
            }
        }

        private IMailServerFactory MailServerFactory
        {
            get
            {
                if (!IsAdmin)
                    throw new SecurityException("Need admin privileges.");

                if (_mailserverfactory == null)
                {
                    var server_info = TenantServerDal.GetTenantServer();
                    switch ((ServerType)server_info.type)
                    {
                        case ServerType.MockServer:
                            _mailserverfactory = new MockFactory();
                            break;
                        case ServerType.Postfix:
                            _mailserverfactory = new PostfixFactory();
                            break;
                        default:
                            throw new ArgumentException("Server_type creation wasn't added. Server_type: " +
                                                        (ServerType) server_info.type);
                    }
                }

                return _mailserverfactory;
            }
        }

        private MailServerBase MailServer
        {
            get
            {
                if (!IsAdmin)
                    throw new SecurityException("Need admin privileges.");

                if (_mailServer == null)
                {
                    var server_data = TenantServerDal.GetTenantServer();

                    var limits = new ServerLimits.Builder()
                        .SetMailboxMaxCountPerUser(MailboxPerUserLimit)
                        .Build();

                    var dns_presets = new DnsPresets.Builder()
                        .SetMX(server_data.mx_record, MxRecordPriority)
                        .SetSpfValue(SpfRecordValue)
                        .SetDKIMSelector(DkimSelector)
                        .SetDomainCheckPrefix(DomainCheckPrefix)
                        .Build();

                    var setup = new ServerSetup
                        .Builder(server_data.id, TenantId, UserId)
                        .SetConnectionString(server_data.connection_string)
                        .SetLogger(Logger)
                        .SetServerLimits(limits)
                        .SetDnsPresets(dns_presets)
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
                if (!IsAdmin)
                    throw new SecurityException("Need admin privileges.");

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

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public MailServerApi(ApiContext context)
        {
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
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
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
