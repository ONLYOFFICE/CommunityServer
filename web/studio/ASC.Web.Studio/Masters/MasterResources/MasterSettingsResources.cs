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
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.Masters.MasterResources
{
    public class MasterSettingsResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var curQuota = TenantExtra.GetTenantQuota();

            var result = new List<KeyValuePair<string, object>>(32)
                         {
                             RegisterObject("ApiPath", SetupInfo.WebApiBaseUrl),
                             RegisterObject("IsAuthenticated", SecurityContext.IsAuthenticated),
                             RegisterObject("IsAdmin", CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID)),
                             RegisterObject("IsVisitor", CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()),
                             RegisterObject("CurrentTenantId", CoreContext.TenantManager.GetCurrentTenant().TenantId),
                             RegisterObject("CurrentTenantCreatedDate", CoreContext.TenantManager.GetCurrentTenant().CreatedDateTime),
                             RegisterObject("CurrentTenantVersion", CoreContext.TenantManager.GetCurrentTenant().Version),
                             RegisterObject("CurrentTenantUtcOffset", CoreContext.TenantManager.GetCurrentTenant().TimeZone),
                             RegisterObject("CurrentTenantUtcHoursOffset", CoreContext.TenantManager.GetCurrentTenant().TimeZone.BaseUtcOffset.Hours),
                             RegisterObject("CurrentTenantUtcMinutesOffset", CoreContext.TenantManager.GetCurrentTenant().TimeZone.BaseUtcOffset.Minutes),
                             RegisterObject("TimezoneDisplayName", CoreContext.TenantManager.GetCurrentTenant().TimeZone.DisplayName),
                             RegisterObject("TimezoneOffsetMinutes", CoreContext.TenantManager.GetCurrentTenant().TimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes),
                             RegisterObject("TenantIsPremium", curQuota.Trial ? "No" : "Yes"),
                             RegisterObject("TenantTariff", curQuota.Id),
                             RegisterObject("TenantTariffDocsEdition", curQuota.DocsEdition),
                             RegisterObject("EmailRegExpr", @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$"),
                             RegisterObject("GroupSelector_MobileVersionGroup", new { Id = -1, Name = UserControlsCommonResource.LblSelect.HtmlEncode().ReplaceSingleQuote() }),
                             RegisterObject("GroupSelector_WithGroupEveryone", new { Id = Constants.GroupEveryone.ID, Name = UserControlsCommonResource.Everyone.HtmlEncode().ReplaceSingleQuote() }),
                             RegisterObject("GroupSelector_WithGroupAdmin", new { Id = Constants.GroupAdmin.ID, Name = UserControlsCommonResource.Admin.HtmlEncode().ReplaceSingleQuote() }),
                             RegisterObject("SetupInfoNotifyAddress", SetupInfo.NotifyAddress),
                             RegisterObject("SetupInfoTipsAddress", SetupInfo.TipsAddress),
                             RegisterObject("CKEDITOR_BASEPATH", WebPath.GetPath("/usercontrols/common/ckeditor/")),
                             RegisterObject("MaxImageFCKWidth", ConfigurationManager.AppSettings["MaxImageFCKWidth"] ?? "620"),
                             RegisterObject("UserPhotoHandlerUrl", VirtualPathUtility.ToAbsolute("~/UserPhoto.ashx")),
                             RegisterObject("ImageWebPath", WebImageSupplier.GetImageFolderAbsoluteWebPath()),
                             RegisterObject("UrlShareGooglePlus", SetupInfo.ShareGooglePlusUrl),
                             RegisterObject("UrlShareTwitter", SetupInfo.ShareTwitterUrl),
                             RegisterObject("UrlShareFacebook", SetupInfo.ShareFacebookUrl),
                             RegisterObject("ZeroClipboardMoviePath", CommonLinkUtility.ToAbsolute("~/js/flash/zeroclipboard/zeroclipboard10.swf"))
                         };

            if (CoreContext.Configuration.Personal)
            {
                result.Add(RegisterObject("Personal", CoreContext.Configuration.Personal));
            }

            if (CoreContext.Configuration.Standalone)
            {
                result.Add(RegisterObject("Standalone", CoreContext.Configuration.Standalone));
            }

            if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
            {
                result.Add(RegisterObject("FilterHelpCenterLink", CommonLinkUtility.GetHelpLink() + "/tipstricks/using-search.aspx"));
            }

            return result;
        }

        protected override string GetCacheHash()
        {
            /* return user last mod time + culture */
            var hash = SecurityContext.CurrentAccount.ID.ToString()
                       + CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).LastModified.ToString(CultureInfo.InvariantCulture)
                       + CoreContext.TenantManager.GetCurrentTenant().LastModified.ToString(CultureInfo.InvariantCulture);
            if (CoreContext.Configuration.Standalone)
            {
                // flush javascript for due tariff
                hash += DateTime.UtcNow.Date.ToString(CultureInfo.InvariantCulture);
            }
            return hash;
        }
    }
}