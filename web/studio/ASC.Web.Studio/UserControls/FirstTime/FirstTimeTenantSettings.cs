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


using System;
using System.Collections.Specialized;
using System.Net;
using System.Web.Configuration;
using ASC.Common.Logging;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;


namespace ASC.Web.Studio.UserControls.FirstTime
{
    public static class FirstTimeTenantSettings
    {
        public static void SendInstallInfo(UserInfo user)
        {
            try
            {
                var url = WebConfigurationManager.AppSettings["web.install-url"];
                if (string.IsNullOrEmpty(url)) return;

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var q = new MailQuery
                    {
                        Email = user.Email,
                        Id = CoreContext.Configuration.GetKey(tenant.TenantId),
                        Alias = tenant.TenantDomain,
                    };

                var index = url.IndexOf("?v=", StringComparison.InvariantCultureIgnoreCase);
                if (0 < index)
                {
                    q.Version = url.Substring(index + 3) + Environment.OSVersion;
                    url = url.Substring(0, index);
                }

                using (var webClient = new WebClient())
                {
                    var values = new NameValueCollection
                        {
                            {"query", Signature.Create(q, "4be71393-0c90-41bf-b641-a8d9523fba5c")}
                        };
                    webClient.UploadValues(url, values);
                }
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error);
            }
        }

        private class MailQuery
        {
            public string Email { get; set; }
            public string Version { get; set; }
            public string Id { get; set; }
            public string Alias { get; set; }
        }
    }
}