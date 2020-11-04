/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
using ASC.Security.Cryptography;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.Masters.MasterResources
{
    public class MasterSettingsResources : ClientScript
    {
        protected override bool CheckAuth { get { return false; } }

        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var curQuota = TenantExtra.GetTenantQuota();
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var helpLink = CommonLinkUtility.GetHelpLink();

            var result = new List<KeyValuePair<string, object>>(4)
            {
                RegisterObject(
                new {
                        ApiPath = SetupInfo.WebApiBaseUrl,
                        IsAuthenticated = SecurityContext.IsAuthenticated,
                        IsAdmin = CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID),
                        IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor(),
                        //CurrentTenantId = tenant.TenantId,
                        CurrentTenantCreatedDate = tenant.CreatedDateTime,
                        CurrentTenantVersion = tenant.Version,
                        CurrentTenantTimeZone = new
                            {
                                Id = tenant.TimeZone.Id,
                                DisplayName = Common.Utils.TimeZoneConverter.GetTimeZoneName(tenant.TimeZone),
                                BaseUtcOffset = tenant.TimeZone.GetOffset(true).TotalMinutes,
                                UtcOffset = tenant.TimeZone.GetOffset().TotalMinutes
                            },
                        TenantIsPremium = curQuota.Trial ? "No" : "Yes",
                        TenantTariff = curQuota.Id,
                        EmailRegExpr = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$",
                        UserNameRegExpr = UserFormatter.UserNameRegex,
                        GroupSelector_MobileVersionGroup = new { Id = -1, Name = UserControlsCommonResource.LblSelect.HtmlEncode().ReplaceSingleQuote() },
                        GroupSelector_WithGroupEveryone = new { Id = Constants.GroupEveryone.ID, Name = UserControlsCommonResource.Everyone.HtmlEncode().ReplaceSingleQuote() },
                        GroupSelector_WithGroupAdmin = new { Id = Constants.GroupAdmin.ID, Name = UserControlsCommonResource.Admin.HtmlEncode().ReplaceSingleQuote() },
                        SetupInfoNotifyAddress = SetupInfo.NotifyAddress,
                        SetupInfoTipsAddress = SetupInfo.TipsAddress,
                        CKEDITOR_BASEPATH = WebPath.GetPath("/UserControls/Common/ckeditor/"),
                        MaxImageFCKWidth = ConfigurationManagerExtension.AppSettings["MaxImageFCKWidth"] ?? "620",
                        UserPhotoHandlerUrl = VirtualPathUtility.ToAbsolute("~/UserPhoto.ashx"),
                        UserDefaultBigPhotoURL = UserPhotoManager.GetDefaultBigPhotoURL(),
                        ImageWebPath = WebImageSupplier.GetImageFolderAbsoluteWebPath(),
                        UrlShareTwitter = SetupInfo.ShareTwitterUrl,
                        UrlShareFacebook = SetupInfo.ShareFacebookUrl,
                        LogoDarkUrl = CommonLinkUtility.GetFullAbsolutePath(TenantLogoManager.GetLogoDark(true)),
                        HelpLink = helpLink ?? "",
                        MailMaximumMessageBodySize = ConfigurationManagerExtension.AppSettings["mail.maximum-message-body-size"] ?? "524288",
                        PasswordHasher.PasswordHashSize,
                        PasswordHasher.PasswordHashIterations,
                        PasswordHasher.PasswordHashSalt,
                })
            };

            if (CoreContext.Configuration.Personal)
            {
                result.Add(RegisterObject(new { CoreContext.Configuration.Personal }));
            }

            if (CoreContext.Configuration.CustomMode)
            {
                result.Add(RegisterObject(new { CoreContext.Configuration.CustomMode }));
            }

            if (CoreContext.Configuration.Standalone)
            {
                result.Add(RegisterObject(new { CoreContext.Configuration.Standalone }));
            }

            if (!string.IsNullOrEmpty(helpLink))
            {
                result.Add(RegisterObject(new { FilterHelpCenterLink = helpLink.TrimEnd('/') + "/tipstricks/using-search.aspx" }));
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