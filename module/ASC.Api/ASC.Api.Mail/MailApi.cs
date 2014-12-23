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

using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Mail.Configuration;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Api.Mail
{
    public partial class MailApi : IApiEntryPoint
    {
        private readonly ApiContext _context;
        private readonly MailApiConfiguration _configuration;

        private MailBoxManager _mailBoxManager;
        private MailSendQueue _mailSendQueue;
        private ILogger _log;


        ///<summary>
        /// Api name entry
        ///</summary>
        public string Name
        {
            get { return "mail"; }
        }


        private MailBoxManager MailBoxManager
        {
            get { return _mailBoxManager ?? (_mailBoxManager = new MailBoxManager(25, Logger)); }
        }

        private ILogger Logger
        {
            get { return _log ?? (_log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api")); }
        }

        private MailSendQueue SendQueue
        {
            get { return _mailSendQueue ?? (_mailSendQueue = new MailSendQueue(MailBoxManager, Logger, _configuration.GetHandlers(Logger, "mail"))); }
        }

        private int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }


        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        ///<param name="configuration"></param>
        public MailApi(ApiContext context, MailApiConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
    }
}
