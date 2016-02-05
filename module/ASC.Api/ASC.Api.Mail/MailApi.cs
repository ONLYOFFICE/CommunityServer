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
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Core;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Api.Mail
{
    public partial class MailApi : IApiEntryPoint
    {
        private readonly ApiContext _context;

        private MailBoxManager _mailBoxManager;
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
            get { return _mailBoxManager ?? (_mailBoxManager = new MailBoxManager(Logger)); }
        }

        private ILogger Logger
        {
            get { return _log ?? (_log = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "ASC.Api")); }
        }

        private int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }

        private CultureInfo CurrentCulture
        {
            get
            {
                var u = CoreContext.UserManager.GetUsers(new Guid(Username));

                var culture = !string.IsNullOrEmpty(u.CultureName) ? u.GetCulture() : CoreContext.TenantManager.GetCurrentTenant().GetCulture();

                return culture;
            }
        }

        private bool IsSignalRAvailable
        {
            get { return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["web.hub"]); }
        }

        private string MailDaemonEmail
        {
            get { return ConfigurationManager.AppSettings["mail.daemon-email"] ?? "mail-daemon@onlyoffice.com"; }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public MailApi(ApiContext context)
        {
            _context = context;
        }
    }
}
