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


using System.Collections.Generic;
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;

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
            var imgs = new Dictionary<string, string>
                {
                    {"empty_screen_filter", WebImageSupplier.GetAbsoluteWebPath("empty_screen_filter.png")},
                    {"empty_screen_tasks", WebImageSupplier.GetAbsoluteWebPath("empty_screen_tasks.png", ProductEntryPoint.ID)},
                    {"empty_screen_deals", WebImageSupplier.GetAbsoluteWebPath("empty_screen_deals.png", ProductEntryPoint.ID)},
                    {"empty_screen_cases", WebImageSupplier.GetAbsoluteWebPath("empty_screen_cases.png", ProductEntryPoint.ID)},
                    {"empty_screen_invoices", WebImageSupplier.GetAbsoluteWebPath("empty_screen_invoices.png", ProductEntryPoint.ID)},
                    {"empty_screen_products_services", WebImageSupplier.GetAbsoluteWebPath("empty_screen_products_services.png", ProductEntryPoint.ID)},
                    {"empty_screen_taxes", WebImageSupplier.GetAbsoluteWebPath("empty_screen_taxes.png", ProductEntryPoint.ID)},
                    {"empty_screen_persons", WebImageSupplier.GetAbsoluteWebPath("empty_screen_persons.png", ProductEntryPoint.ID)},
                    {"empty_screen_phones", WebImageSupplier.GetAbsoluteWebPath("empty_screen_phones.png", ProductEntryPoint.ID)},
                    {"empty_screen_voip_settings", WebImageSupplier.GetAbsoluteWebPath("empty_screen_voip_settings.png", ProductEntryPoint.ID)},
                    {"empty_people_logo_40_40", WebImageSupplier.GetAbsoluteWebPath("empty_people_logo_40_40.png", ProductEntryPoint.ID)},
                    {"empty_screen_opportunity_participants", WebImageSupplier.GetAbsoluteWebPath("empty_screen_opportunity_participants.png", ProductEntryPoint.ID)},
                    {"empty_screen_company_participants", WebImageSupplier.GetAbsoluteWebPath("empty_screen_company_participants.png", ProductEntryPoint.ID)},
                    {"empty_screen_case_participants", WebImageSupplier.GetAbsoluteWebPath("empty_screen_case_participants.png", ProductEntryPoint.ID)},
                    {"empty_screen_projects", WebImageSupplier.GetAbsoluteWebPath("empty_screen_projects.png", ProductEntryPoint.ID)},
                    {"empty_screen_userfields", WebImageSupplier.GetAbsoluteWebPath("empty_screen_userfields.png", ProductEntryPoint.ID)},
                    {"empty_screen_tags", WebImageSupplier.GetAbsoluteWebPath("empty_screen_tags.png", ProductEntryPoint.ID)},
                    {"empty_screen_twitter", WebImageSupplier.GetAbsoluteWebPath("empty_screen_twitter.png", ProductEntryPoint.ID)}
                };

            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                           new
                           {
                               MediumSizePhotoCompany = ContactPhotoManager.GetMediumSizePhoto(0, true),
                               MediumSizePhoto = ContactPhotoManager.GetMediumSizePhoto(0, false),
                               SmallSizePhotoCompany = ContactPhotoManager.GetSmallSizePhoto(0, true),
                               SmallSizePhoto = ContactPhotoManager.GetSmallSizePhoto(0, false),
                               ProfileRemoved = Constants.LostUser.DisplayUserName(),
                               System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator,
                               KeyCodeCurrencyDecimalSeparator = (int)System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0],
                               Global.VisiblePageCount,
                               DefaultEntryCountOnPage = Global.EntryCountOnPage,
                               Global.DefaultCustomFieldSize,
                               Global.DefaultCustomFieldRows,
                               Global.DefaultCustomFieldCols,
                               Global.MaxCustomFieldSize,
                               Global.MaxCustomFieldRows,
                               Global.MaxCustomFieldCols,
                               Global.MaxHistoryEventCharacters,
                               Global.MaxInvoiceItemPrice,
                               Global.CanDownloadInvoices,
                               IsCRMAdmin = CRMSecurity.IsAdmin,
                               EmptyScrImgs = imgs,
                               ImageWebPath = WebImageSupplier.GetImageFolderAbsoluteWebPath(ProductEntryPoint.ID),

                               ContactSelectorTypeEnum = 
                                    new Dictionary<string, int>
                                    {
                                        {"All", (int)ContactSelectorTypeEnum.All},
                                        {"Companies", (int)ContactSelectorTypeEnum.Companies},
                                        {"CompaniesAndPersonsWithoutCompany", (int)ContactSelectorTypeEnum.CompaniesAndPersonsWithoutCompany},
                                        {"Persons", (int)ContactSelectorTypeEnum.Persons},
                                        {"PersonsWithoutCompany", (int)ContactSelectorTypeEnum.PersonsWithoutCompany}
                                    },

                               HistoryCategorySystem = 
                                   new Dictionary<string, int>
                                       {
                                           {"TaskClosed", (int)HistoryCategorySystem.TaskClosed},
                                           {"FilesUpload", (int)HistoryCategorySystem.FilesUpload},
                                           {"MailMessage", (int)HistoryCategorySystem.MailMessage}
                                       },

                               DefaultContactPhoto = 
                                   new Dictionary<string, string>
                                       {
                                           {"CompanyBigSizePhoto", ContactPhotoManager.GetBigSizePhoto(0, true)},
                                           {"PersonBigSizePhoto", ContactPhotoManager.GetBigSizePhoto(0, false)},
                                           {"CompanyMediumSizePhoto", ContactPhotoManager.GetMediumSizePhoto(0, true)},
                                           {"PersonMediumSizePhoto", ContactPhotoManager.GetMediumSizePhoto(0, false)}
                                       },

                               CookieKeyForPagination = 
                                   new Dictionary<string, string>
                                       {
                                           {"contacts", "contactPageNumber"},
                                           {"tasks", "taskPageNumber"},
                                           {"deals", "dealPageNumber"},
                                           {"cases", "casesPageNumber"},
                                           {"invoices", "invoicePageNumber"},
                                           {"invoiceitems", "invoiceItemsPageNumber"}
                                       },

                               CanCreateProjects = Global.CanCreateProjects()
                           })
                   };
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