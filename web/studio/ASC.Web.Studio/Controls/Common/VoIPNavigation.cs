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

using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.VoipService.Dao;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Studio.Core.Voip;

namespace ASC.Web.Studio.Controls.Common
{
    public class VoipNavigation
    {
        public static string RenderCustomNavigation(Page page)
        {
            if (!VoipEnabled) return string.Empty;

            page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/asc/core/voip.countries.js"));
            page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/js/asc/core/voip.phone.js"));
            page.RegisterClientScript(typeof(VoipNumberData));

            return
                string.Format(@"<li class=""top-item-box voip"">
                                  <a class=""voipActiveBox inner-text"" title=""{0}"">
                                      <span class=""inner-label"">{1}</span>
                                  </a>
                                </li>",
                              "VoIP",
                              0);
        }

        public static bool VoipEnabled
        {
            get
            {
                return VoipPaymentSettings.IsVisibleSettings && 
                    VoipPaymentSettings.IsEnabled && 
                    new CachedVoipDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, "crm").GetCurrentNumber() != null;
            }
        }
    }

    public class VoipNumberData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master"; }
        }

        protected override string GetCacheHash()
        {
            return SecurityContext.CurrentAccount.ID + GetNumber() +
                   (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal
                        ? (CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                           CoreContext.GroupManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture))
                        : string.Empty);
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject("numberId", GetNumber());
        }

        private static string GetNumber()
        {
            return new CachedVoipDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, "crm").GetCurrentNumber().Number;
        }
    }
}