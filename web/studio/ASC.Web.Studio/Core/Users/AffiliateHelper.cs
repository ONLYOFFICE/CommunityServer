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
using System.IO;
using System.Net;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Users
{
    public class AffiliateHelper
    {
        public static string JoinAffilliateLink
        {
            get { return WebConfigurationManager.AppSettings["web.affiliates.link"]; }

        }

        private static bool Available(UserInfo user)
        {
            return !String.IsNullOrEmpty(JoinAffilliateLink) &&
                   user.ActivationStatus == EmployeeActivationStatus.Activated &&
                   user.Status == EmployeeStatus.Active;
        }

        public static bool BannerAvailable
        {
            get
            {   
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                return Available(user) && (TenantExtra.GetTenantQuota().NonProfit || user.IsVisitor());
            }
        }

        public static bool ButtonAvailable(UserInfo user)
        {
            return Available(user) && user.IsMe();
        }

        public static string Join()
        {
            if (!string.IsNullOrEmpty(JoinAffilliateLink))
            {
                return JoinAffilliateLink;

/*                var request = WebRequest.Create(string.Format("{2}/Account/Register?uid={1}&tenantAlias={0}",
                                                    CoreContext.TenantManager.GetCurrentTenant().TenantAlias, SecurityContext.CurrentAccount.ID,
                                                    JoinAffilliateLink));
                request.Method = "PUT";
                request.ContentLength = 0;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var origin = streamReader.ReadToEnd();
                        if (response.StatusCode != HttpStatusCode.BadRequest)
                        {
                            return string.Format("{0}/home/Account/SignIn?ticketKey={1}", JoinAffilliateLink, origin);
                        }
                    }
                }*/
            }

            return "";
        }
    }
}
