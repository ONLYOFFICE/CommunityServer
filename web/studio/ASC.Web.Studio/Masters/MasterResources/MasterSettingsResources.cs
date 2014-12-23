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

            yield return RegisterObject("UploadFlashUrl", FileUploaderFlashParams.GetFlashUrl);
            yield return RegisterObject("UploadDefaultRuntimes", FileUploaderFlashParams.DefaultRuntimes);
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