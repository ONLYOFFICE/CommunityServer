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
