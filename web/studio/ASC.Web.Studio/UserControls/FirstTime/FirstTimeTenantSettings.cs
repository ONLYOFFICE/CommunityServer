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
using System.Net;
using System.Collections.Specialized;
using System.Web.Configuration;
using ASC.Common.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Studio.Core.Notify;
using log4net;

namespace ASC.Web.Studio.UserControls.FirstTime
{
    public static class FirstTimeTenantSettings
    {
        public static void SetDefaultTenantSettings()
        {
            try
            {
                WebItemSecurity.SetSecurity("community-wiki", false, null);
                WebItemSecurity.SetSecurity("community-forum", false, null);
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error);
            }
        }

        public static void SendInstallInfo(UserInfo user)
        {
            try
            {
                StudioNotifyService.Instance.SendCongratulations(user);
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Web").Error(error);
            }
            try
            {
                var url = WebConfigurationManager.AppSettings["web.install-url"];
                if (!string.IsNullOrEmpty(url))
                {
                    var tenant = CoreContext.TenantManager.GetCurrentTenant();
                    var q = new MailQuery
                    {
                        Email = user.Email,
                        Id = CoreContext.Configuration.GetKey(tenant.TenantId),
                        Alias = tenant.TenantDomain,
                    };
                    var index = url.IndexOf("?v=");
                    if (0 < index)
                    {
                        q.Version = url.Substring(index + 3) + Environment.OSVersion;
                        url = url.Substring(0, index);
                    }
                    using (var webClient = new WebClient())
                    {
                        var values = new NameValueCollection();
                        values.Add("query", Signature.Create<MailQuery>(q, "4be71393-0c90-41bf-b641-a8d9523fba5c"));
                        webClient.UploadValues(url, values);
                    }
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