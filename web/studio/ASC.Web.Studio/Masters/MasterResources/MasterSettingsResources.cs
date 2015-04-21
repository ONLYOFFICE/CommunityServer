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
using ASC.Web.Studio.Controls.FileUploader;
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
            yield return RegisterObject("ApiPath", SetupInfo.WebApiBaseUrl);

            yield return RegisterObject("IsAuthenticated", SecurityContext.IsAuthenticated);
            yield return RegisterObject("IsAdmin", CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID));
            yield return RegisterObject("IsVisitor", CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor());

            yield return RegisterObject("CurrentTenantId", CoreContext.TenantManager.GetCurrentTenant().TenantId);
            yield return RegisterObject("CurrentTenantCreatedDate", CoreContext.TenantManager.GetCurrentTenant().CreatedDateTime);
            yield return RegisterObject("CurrentTenantVersion", CoreContext.TenantManager.GetCurrentTenant().Version);
            yield return RegisterObject("CurrentTenantUtcOffset", CoreContext.TenantManager.GetCurrentTenant().TimeZone);
            yield return RegisterObject("CurrentTenantUtcHoursOffset", CoreContext.TenantManager.GetCurrentTenant().TimeZone.BaseUtcOffset.Hours);
            yield return RegisterObject("CurrentTenantUtcMinutesOffset", CoreContext.TenantManager.GetCurrentTenant().TimeZone.BaseUtcOffset.Minutes);
            yield return RegisterObject("TimezoneDisplayName", CoreContext.TenantManager.GetCurrentTenant().TimeZone.DisplayName);
            yield return RegisterObject("TimezoneOffsetMinutes", CoreContext.TenantManager.GetCurrentTenant().TimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes);

            var curQuota = TenantExtra.GetTenantQuota();

            yield return RegisterObject("TenantIsPremium", curQuota.Trial ? "No" : "Yes");
            yield return RegisterObject("TenantTariff", curQuota.Id);
            yield return RegisterObject("TenantTariffDocsEdition", curQuota.DocsEdition);

            if (CoreContext.Configuration.Personal)
            {
                yield return RegisterObject("Personal", CoreContext.Configuration.Personal);
            }

            if (CoreContext.Configuration.Standalone)
            {
                yield return RegisterObject("Standalone", CoreContext.Configuration.Standalone);
            }

            yield return RegisterObject("EmailRegExpr", @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$");

            yield return RegisterObject("GroupSelector_MobileVersionGroup", new { Id = -1, Name = UserControlsCommonResource.LblSelect.HtmlEncode().ReplaceSingleQuote() });
            yield return RegisterObject("GroupSelector_WithGroupEveryone", new { Id = ASC.Core.Users.Constants.GroupEveryone.ID, Name = UserControlsCommonResource.Everyone.HtmlEncode().ReplaceSingleQuote() });
            yield return RegisterObject("GroupSelector_WithGroupAdmin", new { Id = ASC.Core.Users.Constants.GroupAdmin.ID, Name = UserControlsCommonResource.Admin.HtmlEncode().ReplaceSingleQuote() });
            if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
            {
                yield return RegisterObject("FilterHelpCenterLink", CommonLinkUtility.GetHelpLink() + "/tipstricks/using-search.aspx");
            }

            yield return RegisterObject("SetupInfoNotifyAddress", SetupInfo.NotifyAddress);
            yield return RegisterObject("SetupInfoTipsAddress", SetupInfo.TipsAddress);

            yield return RegisterObject("CKEDITOR_BASEPATH", WebPath.GetPath("/usercontrols/common/ckeditor/"));
            yield return RegisterObject("MaxImageFCKWidth", ConfigurationManager.AppSettings["MaxImageFCKWidth"] ?? "620");

            yield return RegisterObject("UserPhotoHandlerUrl", VirtualPathUtility.ToAbsolute("~/UserPhoto.ashx"));
            yield return RegisterObject("ImageWebPath", WebImageSupplier.GetImageFolderAbsoluteWebPath());


            yield return RegisterObject("UrlShareGooglePlus", SetupInfo.ShareGooglePlusUrl);
            yield return RegisterObject("UrlShareTwitter", SetupInfo.ShareTwitterUrl);
            yield return RegisterObject("UrlShareFacebook", SetupInfo.ShareFacebookUrl);

            yield return RegisterObject("ZeroClipboardMoviePath", CommonLinkUtility.ToAbsolute("~/js/flash/zeroclipboard/zeroclipboard10.swf"));
            
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