/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Globalization;
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Web.Core;

namespace ASC.Web.Studio.Core
{
    public class WebStudioCommonModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.AcquireRequestState += AcquireRequestState;
            context.BeginRequest += BeginRequest;
            context.EndRequest += EndRequest;
        }

        public void Dispose()
        {
        }


        private void AcquireRequestState(object sender, EventArgs e)
        {
            Authenticate();
            ResolveUserCulture();
        }

        private void BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current != null && !RestrictRewriter(HttpContext.Current.Request.Url))
            {
                HttpContext.Current.PushRewritenUri();
            }
        }

        private void EndRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current != null && !RestrictRewriter(HttpContext.Current.Request.Url))
            {
                HttpContext.Current.PopRewritenUri();
            }
        }


        public static bool Authenticate()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null && !SecurityContext.IsAuthenticated)
            {
                var cookie = CookiesManager.GetCookies(CookiesType.AuthKey);
                if (!string.IsNullOrEmpty(cookie))
                {
                    return SecurityContext.AuthenticateMe(cookie);
                }
            }
            return false;
        }

        public static void ResolveUserCulture()
        {
            CultureInfo culture = null;
            
            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null)
            {
                culture = tenant.GetCulture();
            }
            
            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            if (!string.IsNullOrEmpty(user.CultureName))
            {
                culture = CultureInfo.GetCultureInfo(user.CultureName);
            }

            if (culture != null && Thread.CurrentThread.CurrentCulture != culture)
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
            if (culture != null && Thread.CurrentThread.CurrentUICulture != culture)
            {
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }


        private bool RestrictRewriter(Uri uri)
        {
            return uri.AbsolutePath.IndexOf(".svc", StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}