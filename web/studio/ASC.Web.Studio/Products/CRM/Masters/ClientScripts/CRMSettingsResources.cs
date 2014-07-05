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
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.Data.Storage;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.CRM.Masters.ClientScripts
{
    public class CRMSettingsResources : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject("ProfileRemoved", ASC.Core.Users.Constants.LostUser.DisplayUserName());

            yield return RegisterObject("CurrencyDecimalSeparator", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
            yield return RegisterObject("KeyCodeCurrencyDecimalSeparator", (int)System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0]);

            yield return RegisterObject("VisiblePageCount", Global.VisiblePageCount);
            yield return RegisterObject("DefaultEntryCountOnPage", Global.EntryCountOnPage);

            yield return RegisterObject("DefaultCustomFieldSize", Global.DefaultCustomFieldSize);
            yield return RegisterObject("DefaultCustomFieldRows", Global.DefaultCustomFieldRows);
            yield return RegisterObject("DefaultCustomFieldCols", Global.DefaultCustomFieldCols);
            yield return RegisterObject("MaxCustomFieldSize", Global.MaxCustomFieldSize);
            yield return RegisterObject("MaxCustomFieldRows", Global.MaxCustomFieldRows);
            yield return RegisterObject("MaxCustomFieldCols", Global.MaxCustomFieldCols);

            yield return RegisterObject("IsCRMAdmin", CRMSecurity.IsAdmin);


            var imgs = new Dictionary<string, string>
                {
                    {"empty_screen_filter", WebImageSupplier.GetAbsoluteWebPath("empty_screen_filter.png") },
                    {"empty_screen_tasks", WebImageSupplier.GetAbsoluteWebPath("empty_screen_tasks.png", ProductEntryPoint.ID) },
                    {"empty_screen_deals", WebImageSupplier.GetAbsoluteWebPath("empty_screen_deals.png", ProductEntryPoint.ID) },
                    {"empty_screen_cases", WebImageSupplier.GetAbsoluteWebPath("empty_screen_cases.png", ProductEntryPoint.ID) },
                    {"empty_screen_invoices", WebImageSupplier.GetAbsoluteWebPath("empty_screen_invoices.png", ProductEntryPoint.ID) },
                    {"empty_screen_products_services", WebImageSupplier.GetAbsoluteWebPath("empty_screen_products_services.png", ProductEntryPoint.ID) },
                    {"empty_screen_taxes", WebImageSupplier.GetAbsoluteWebPath("empty_screen_taxes.png", ProductEntryPoint.ID) },
                    {"empty_screen_persons", WebImageSupplier.GetAbsoluteWebPath("empty_screen_persons.png", ProductEntryPoint.ID) },
                    {"empty_people_logo_40_40", WebImageSupplier.GetAbsoluteWebPath("empty_people_logo_40_40.png",ProductEntryPoint.ID)},
                    {"empty_screen_opportunity_participants", WebImageSupplier.GetAbsoluteWebPath("empty_screen_opportunity_participants.png", ProductEntryPoint.ID)},
                    {"empty_screen_company_participants",WebImageSupplier.GetAbsoluteWebPath("empty_screen_company_participants.png", ProductEntryPoint.ID)},
                    {"empty_screen_case_participants", WebImageSupplier.GetAbsoluteWebPath("empty_screen_case_participants.png", ProductEntryPoint.ID)},
                    {"empty_screen_projects", WebImageSupplier.GetAbsoluteWebPath("empty_screen_projects.png", ProductEntryPoint.ID)},
                    {"empty_screen_userfields", WebImageSupplier.GetAbsoluteWebPath("empty_screen_userfields.png", ProductEntryPoint.ID)},
                    {"empty_screen_tags", WebImageSupplier.GetAbsoluteWebPath("empty_screen_tags.png", ProductEntryPoint.ID)}

                };
            yield return RegisterObject("EmptyScrImgs", imgs);

            yield return RegisterObject("ContactSelectorTypeEnum",
                new Dictionary<string, int>
                {
                    {"All", (int)ContactSelectorTypeEnum.All },
                    {"Companies", (int)ContactSelectorTypeEnum.Companies },
                    {"CompaniesAndPersonsWithoutCompany", (int)ContactSelectorTypeEnum.CompaniesAndPersonsWithoutCompany },
                    {"Persons", (int)ContactSelectorTypeEnum.Persons },
                    {"PersonsWithoutCompany", (int)ContactSelectorTypeEnum.PersonsWithoutCompany }
                });

            yield return RegisterObject("HistoryCategorySystem",
                new Dictionary<string, int>
                {
                    {"TaskClosed", (int)HistoryCategorySystem.TaskClosed },
                    {"FilesUpload", (int)HistoryCategorySystem.FilesUpload },
                    {"MailMessage", (int)HistoryCategorySystem.MailMessage }
                });

            yield return RegisterObject("DefaultContactPhoto",
                new Dictionary<string, string>
                {
                    {"CompanyBigSizePhoto", ContactPhotoManager.GetBigSizePhoto(0, true) },
                    {"PersonBigSizePhoto", ContactPhotoManager.GetBigSizePhoto(0, false) },
                    {"CompanyMediumSizePhoto", ContactPhotoManager.GetMediumSizePhoto(0, true) },
                    {"PersonMediumSizePhoto", ContactPhotoManager.GetMediumSizePhoto(0, false) }
                });

            yield return RegisterObject("CookieKeyForPagination",
                new Dictionary<string, string>
                {
                    {"contacts", "contactPageNumber" },
                    {"tasks", "taskPageNumber" },
                    {"deals", "dealPageNumber" },
                    {"cases", "casesPageNumber"},
                    {"invoices", "invoicePageNumber"},
                    {"invoiceitems", "invoiceItemsPageNumber"}
                });

            yield return RegisterObject("CanCreateProjects", Global.CanCreateProjects());
        }

        protected override string GetCacheHash()
        {
            /* return user last mod time + culture */
            var hash = SecurityContext.CurrentAccount.ID.ToString()
                       + CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).LastModified.ToString(CultureInfo.InvariantCulture)
                       + CoreContext.TenantManager.GetCurrentTenant().LastModified.ToString(CultureInfo.InvariantCulture);
            return hash;
        }
    }
}