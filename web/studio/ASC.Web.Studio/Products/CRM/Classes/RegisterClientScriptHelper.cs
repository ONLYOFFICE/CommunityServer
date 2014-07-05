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

#region Import

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Thrdparty.Configuration;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Configuration;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core;
using Newtonsoft.Json;

#endregion

namespace ASC.Web.CRM.Classes
{
    public static class RegisterClientScriptHelper
    {
        private static bool IsFacebookSearchEnabled
        {
            get { return !string.IsNullOrEmpty(KeyStorage.Get("facebookAppID")) && !string.IsNullOrEmpty(KeyStorage.Get("facebookAppSecret")); }
        }

        private static bool IsLinkedInSearchEnabled
        {
            get { return !string.IsNullOrEmpty(KeyStorage.Get("linkedInKey")) && !string.IsNullOrEmpty(KeyStorage.Get("linkedInSecret")); }
        }

        private static bool IsTwitterSearchEnabled
        {
            get { return !string.IsNullOrEmpty(KeyStorage.Get("twitterKey")) && !string.IsNullOrEmpty(KeyStorage.Get("twitterSecret")); }
        }


        public static void DataCommon(Page page)
        {
        }

        public static void DataUserSelectorListView(BasePage page, String ObjId, Dictionary<Guid, String> SelectedUsers)
        {
            var ids = SelectedUsers != null && SelectedUsers.Count > 0 ? SelectedUsers.Select(i => i.Key).ToArray() : new List<Guid>().ToArray();
            var names = SelectedUsers != null && SelectedUsers.Count > 0 ? SelectedUsers.Select(i => i.Value).ToArray() : new List<string>().ToArray();

            page.RegisterInlineScript(String.Format(" SelectedUsers{0} = {1}; ",
                                                        ObjId,
                                                        JsonConvert.SerializeObject(
                                                            new
                                                            {
                                                                IDs = ids,
                                                                Names = names,
                                                                DeleteImgSrc = WebImageSupplier.GetAbsoluteWebPath("remove_12.png", ProductEntryPoint.ID),
                                                                DeleteImgTitle = CRMCommonResource.DeleteUser,
                                                                CurrentUserID = SecurityContext.CurrentAccount.ID
                                                            })), onReady: false);
        }

        #region Data for History View

        public static void DataHistoryView(BasePage page, List<Guid> UserList)
        {
            page.RegisterInlineScript(String.Format(" var historyView_dateTimeNowShortDateString = '{0}'; ",TenantUtil.DateTimeNow().ToShortDateString()), onReady: false);

            if (UserList != null)
            {
                page.RegisterInlineScript(String.Format(" UserList_HistoryUserSelector = {0}; ", JsonConvert.SerializeObject(UserList)), onReady: false);
            }

            RegisterClientScriptHelper.DataUserSelectorListView(page, "_HistoryUserSelector", null);
        }

        #endregion

        #region Data for Contact Views

        public static void DataListContactTab(BasePage page, Int32 entityID, EntityType entityType)
        {
            page.RegisterInlineScript(String.Format(" var entityData = {0}; ",
                                            JsonConvert.SerializeObject(new
                                            {
                                                id = entityID,
                                                type = entityType.ToString().ToLower()
                                            })), onReady: false);
        }

        public static void DataContactFullCardView(BasePage page, Contact targetContact)
        {
            List<CustomField> data;

            if (targetContact is Company)
                data = Global.DaoFactory.GetCustomFieldDao().GetEnityFields(EntityType.Company, targetContact.ID, false);
            else
                data = Global.DaoFactory.GetCustomFieldDao().GetEnityFields(EntityType.Person, targetContact.ID, false);

            var networks =
                Global.DaoFactory.GetContactInfoDao().GetList(targetContact.ID, null, null, null).ConvertAll(
                    n => new
                    {
                        data = n.Data.HtmlEncode(),
                        infoType = n.InfoType,
                        isPrimary = n.IsPrimary,
                        categoryName = n.CategoryToString(),
                        infoTypeLocalName = n.InfoType.ToLocalizedString()
                    });

            String json;
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(data.GetType());
                serializer.WriteObject(stream, data);
                json = Encoding.UTF8.GetString(stream.ToArray());
            }

            var listItems = Global.DaoFactory.GetListItemDao().GetItems(ListType.ContactStatus);

            var tags = Global.DaoFactory.GetTagDao().GetEntityTags(EntityType.Contact, targetContact.ID);
            var availableTags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Contact).Where(item => !tags.Contains(item));
            var responsibleIDs = CRMSecurity.GetAccessSubjectGuidsTo(targetContact);

            var script = String.Format(@"
                            var customFieldList = {0};
                            var contactNetworks = {1};
                            var sliderListItems = {2};
                            var contactTags = {3};
                            var contactAvailableTags = {4};
                            var contactResponsibleIDs = {5}; ",
                        json,
                        JsonConvert.SerializeObject(networks),
                        JsonConvert.SerializeObject(new
                        {
                            id = targetContact.ID,
                            status = targetContact.StatusID,
                            positionsCount = listItems.Count,
                            items = listItems.ConvertAll(n => new
                            {
                                id = n.ID,
                                color = n.Color,
                                title = n.Title.HtmlEncode()
                            })
                        }),
                        JsonConvert.SerializeObject(tags.ToList().ConvertAll(t => t.HtmlEncode())),
                        JsonConvert.SerializeObject(availableTags.ToList().ConvertAll(t => t.HtmlEncode())),
                        JsonConvert.SerializeObject(responsibleIDs)
                        );

            page.RegisterInlineScript(script, onReady: false);
        }

        public static void DataContactActionView(BasePage page, Contact targetContact, List<CustomField> data, List<ContactInfo> networks)
        {
            var tags = targetContact != null ? Global.DaoFactory.GetTagDao().GetEntityTags(EntityType.Contact, targetContact.ID) : new string[] { };
            var availableTags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Contact).Where(item => !tags.Contains(item));

            String json;
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(data.GetType());
                serializer.WriteObject(stream, data);
                json = Encoding.UTF8.GetString(stream.ToArray());
            }

            var listItems = Global.DaoFactory.GetListItemDao().GetItems(ListType.ContactType);

            var presetCompanyForPersonJson = "";
            if (targetContact != null && targetContact is Person && ((Person)targetContact).CompanyID > 0)
            {
                var company = Global.DaoFactory.GetContactDao().GetByID(((Person)targetContact).CompanyID);
                if (company == null)
                {
                    log4net.LogManager.GetLogger("ASC.CRM").ErrorFormat("Can't find parent company (CompanyID = {0}) for person with ID = {1}", ((Person)targetContact).CompanyID, targetContact.ID);
                }
                else
                {
                    presetCompanyForPersonJson = JsonConvert.SerializeObject(new
                    {
                        id = company.ID,
                        displayName = company.GetTitle().HtmlEncode().ReplaceSingleQuote().Replace(@"\", @"\\"),
                        smallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(company.ID, true)
                    });
                }
            }

            var presetPersonsForCompanyJson = "";
            if (targetContact != null && targetContact is Company)
            {
                var people = Global.DaoFactory.GetContactDao().GetMembers(targetContact.ID);
                if (people.Count != 0) {
                    presetPersonsForCompanyJson = JsonConvert.SerializeObject(people.ConvertAll(item => new
                        {
                            id = item.ID,
                            displayName = item.GetTitle().HtmlEncode().ReplaceSingleQuote().Replace(@"\", @"\\"),
                            smallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(item.ID, false)
                        }));
                }
            }

            var script = String.Format(@"
                                var customFieldList = {0};
                                var contactNetworks = {1};
                                var contactActionTags = {2};
                                var contactActionAvailableTags = {3};
                                var contactAvailableTypes = {4};
                                var presetCompanyForPersonJson = '{5}';
                                var presetPersonsForCompanyJson = '{6}';
                                var facebokSearchEnabled = {7};
                                var linkedinSearchEnabled = {8};
                                var twitterSearchEnabled = {9};
                                var contactActionCurrencies = {10};",
                              json,
                              JsonConvert.SerializeObject(networks),
                              JsonConvert.SerializeObject(tags.ToList().ConvertAll(t => t.HtmlEncode())),
                              JsonConvert.SerializeObject(availableTags.ToList().ConvertAll(t => t.HtmlEncode())),
                              JsonConvert.SerializeObject(
                                    listItems.ConvertAll(n => new
                                    {
                                        id = n.ID,
                                        title = n.Title.HtmlEncode()
                                    })),
                              presetCompanyForPersonJson,
                              presetPersonsForCompanyJson,
                              IsFacebookSearchEnabled.ToString().ToLower(),
                              IsLinkedInSearchEnabled.ToString().ToLower(),
                              IsTwitterSearchEnabled.ToString().ToLower(),
                              JsonConvert.SerializeObject(CurrencyProvider.GetAll())
                              );

            page.RegisterInlineScript(script, onReady: false);
        }

        #endregion

        #region Data for Task Views

        public static void DataContactDetailsViewForTaskAction(BasePage page, Contact TargetContact)
        {
            var isPrivate = !CRMSecurity.CanAccessTo(TargetContact);
            var contactAccessList = new List<Guid>();
            if (isPrivate)
            {
                contactAccessList = CRMSecurity.GetAccessSubjectTo(TargetContact).Keys.ToList<Guid>();
            }

            page.RegisterInlineScript(String.Format(" var contactForInitTaskActionPanel = {0}; ",
                                                    JsonConvert.SerializeObject(new
                                                    {
                                                        id = TargetContact.ID,
                                                        displayName = TargetContact.GetTitle().HtmlEncode().ReplaceSingleQuote(),
                                                        smallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(TargetContact.ID, TargetContact is Company),
                                                        isPrivate = isPrivate,
                                                        accessList = contactAccessList.ConvertAll(n => new { id = n })
                                                    })), onReady: false);
        }

        #endregion

        #region Data for Cases Views

        public static void DataCasesFullCardView(BasePage page, ASC.CRM.Core.Entities.Cases targetCase)
        {
            var customFieldList = Global.DaoFactory.GetCustomFieldDao().GetEnityFields(EntityType.Case, targetCase.ID, false);
            var tags = Global.DaoFactory.GetTagDao().GetEntityTags(EntityType.Case, targetCase.ID);
            var availableTags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Case).Where(item => !tags.Contains(item));
            var responsibleIDs = new List<Guid>();
            if (CRMSecurity.IsPrivate(targetCase)) {
                responsibleIDs = CRMSecurity.GetAccessSubjectGuidsTo(targetCase);
            }
            var script = String.Format(@"
                                        var caseTags = {0};
                                        var caseAvailableTags = {1};
                                        var caseResponsibleIDs = {2}; ",
                                    JsonConvert.SerializeObject(tags),
                                    JsonConvert.SerializeObject(availableTags),
                                    JsonConvert.SerializeObject(responsibleIDs));

            page.RegisterInlineScript(script, onReady: false);
            page.JsonPublisher(customFieldList, "casesCustomFieldList");
        }

        public static void DataCasesActionView(BasePage page, ASC.CRM.Core.Entities.Cases targetCase)
        {
            var customFieldList = targetCase != null
                ? Global.DaoFactory.GetCustomFieldDao().GetEnityFields(EntityType.Case, targetCase.ID, true)
                : Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Case);
            var tags = targetCase != null ? Global.DaoFactory.GetTagDao().GetEntityTags(EntityType.Case, targetCase.ID) : new string[] { };
            var availableTags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Case).Where(item => !tags.Contains(item));


            var presetContactsJson = "";
            var selectedContacts = new List<Contact>();
            if (targetCase != null)
            {
                selectedContacts = Global.DaoFactory.GetContactDao().GetContacts(Global.DaoFactory.GetCasesDao().GetMembers(targetCase.ID));
            }
            else
            {
                var URLContactID = UrlParameters.ContactID;
                if (URLContactID != 0)
                {
                    var target = Global.DaoFactory.GetContactDao().GetByID(URLContactID);
                    if (target != null)
                    {
                        selectedContacts.Add(target);
                    }
                }
            }

            if (selectedContacts.Count > 0)
            {
                presetContactsJson = JsonConvert.SerializeObject(selectedContacts.ConvertAll(item => new
                                {
                                    id = item.ID,
                                    displayName = item.GetTitle().HtmlEncode().ReplaceSingleQuote().Replace(@"\", @"\\"),
                                    smallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(item.ID, item is Company)
                                }));
            }

            var script = String.Format(@"
                                        var casesActionTags = {0};
                                        var casesActionAvailableTags = {1};
                                        var casesActionSelectedContacts = '{2}'; ",
                                    JsonConvert.SerializeObject(tags),
                                    JsonConvert.SerializeObject(availableTags),
                                    presetContactsJson
                                    );

            page.RegisterInlineScript(script, onReady: false);
            page.JsonPublisher(customFieldList, "casesEditCustomFieldList");
        }

        #endregion

        #region Data for Opportunity Views

        public static void DataDealFullCardView(BasePage page, Deal targetDeal)
        {
            var customFieldList = Global.DaoFactory.GetCustomFieldDao().GetEnityFields(EntityType.Opportunity, targetDeal.ID, false);
            var tags = Global.DaoFactory.GetTagDao().GetEntityTags(EntityType.Opportunity, targetDeal.ID);
            var availableTags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Opportunity).Where(item => !tags.Contains(item));

            var responsibleIDs = new List<Guid>();
            if (CRMSecurity.IsPrivate(targetDeal)) {
                responsibleIDs = CRMSecurity.GetAccessSubjectGuidsTo(targetDeal);
            }

            var script = String.Format(@"
                                            var dealTags = {0};
                                            var dealAvailableTags = {1};
                                            var dealResponsibleIDs = {2}; ",
                                        JsonConvert.SerializeObject(tags),
                                        JsonConvert.SerializeObject(availableTags),
                                        JsonConvert.SerializeObject(responsibleIDs)
                                        );

            page.RegisterInlineScript(script, onReady: false);
            page.JsonPublisher(customFieldList, "customFieldList");
        }

        public static void DataDealActionView(BasePage page, Deal targetDeal)
        {
             var customFieldList = targetDeal != null
                ? Global.DaoFactory.GetCustomFieldDao().GetEnityFields(EntityType.Opportunity, targetDeal.ID, true)
                : Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Opportunity);
           
            var dealExcludedIDs = new List<Int32>();
            var dealClientIDs = new List<Int32>();
            var dealMembersIDs = new List<Int32>();
            
            if (targetDeal != null)
            {
                dealExcludedIDs = Global.DaoFactory.GetDealDao().GetMembers(targetDeal.ID).ToList();
                dealMembersIDs = new List<int>(dealExcludedIDs);
                if (targetDeal.ContactID != 0) {
                    dealMembersIDs.Remove(targetDeal.ContactID);
                    dealClientIDs.Add(targetDeal.ContactID);
                }
            }


            var presetClientContactsJson = "";
            var presetMemberContactsJson = "";
            var showMembersPanel = false;
            var selectedContacts = new List<Contact>();
            var hasTargetClient = false;

            if (targetDeal != null && targetDeal.ContactID != 0)
            {
                var contact = Global.DaoFactory.GetContactDao().GetByID(targetDeal.ContactID);
                if(contact != null)
                {
                    selectedContacts.Add(contact);
                }
            }
            else
            {
                var URLContactID = UrlParameters.ContactID;
                if (URLContactID != 0)
                {
                    var target = Global.DaoFactory.GetContactDao().GetByID(URLContactID);
                    if (target != null)
                    {
                        selectedContacts.Add(target);
                        hasTargetClient = true;
                    }
                }
            }
            if (selectedContacts.Count > 0)
            {
                presetClientContactsJson = JsonConvert.SerializeObject(selectedContacts.ConvertAll(item => new
                                {
                                    id = item.ID,
                                    displayName = item.GetTitle().HtmlEncode().ReplaceSingleQuote().Replace(@"\", @"\\"),
                                    smallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(item.ID, item is Company)
                                }));
            }


            selectedContacts = new List<Contact>();
            selectedContacts.AddRange(Global.DaoFactory.GetContactDao().GetContacts(dealMembersIDs.ToArray()));
            if (selectedContacts.Count > 0)
            {
                showMembersPanel = true;
                presetMemberContactsJson = JsonConvert.SerializeObject(selectedContacts.ConvertAll(item => new
                                {
                                    id = item.ID,
                                    displayName = item.GetTitle().HtmlEncode().ReplaceSingleQuote().Replace(@"\", @"\\"),
                                    smallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(item.ID, item is Company)
                                }));
            }

            var ResponsibleSelectedUserId = targetDeal == null ?
                                                SecurityContext.CurrentAccount.ID :
                                                (targetDeal.ResponsibleID != Guid.Empty ? targetDeal.ResponsibleID : Guid.Empty);

            var script = String.Format(@"
                                            var presetClientContactsJson = '{0}';
                                            var presetMemberContactsJson = '{1}';
                                            var hasDealTargetClient = {2};
                                            var showMembersPanel = {3};
                                            var dealClientIDs = {4};
                                            var dealMembersIDs = {5};
                                            var responsibleId = '{6}'; ",
                                        presetClientContactsJson,
                                        presetMemberContactsJson,
                                        hasTargetClient.ToString().ToLower(),
                                        showMembersPanel.ToString().ToLower(),
                                        JsonConvert.SerializeObject(dealClientIDs),
                                        JsonConvert.SerializeObject(dealMembersIDs),
                                        ResponsibleSelectedUserId
                                    );

            page.RegisterInlineScript(script, onReady: false);
            page.JsonPublisher(customFieldList, "customFieldList");
            page.JsonPublisher(Global.DaoFactory.GetDealMilestoneDao().GetAll(), "dealMilestones");

            if (targetDeal != null) {
                page.JsonPublisher(targetDeal, "targetDeal");
            }
        }

        #endregion

        #region Data for Invoices Views

        public static void DataInvoicesActionView(BasePage page, Invoice targetInvoice)
        {
            var invoiceItems = Global.DaoFactory.GetInvoiceItemDao().GetAll();
            var invoiceItemsJson = JsonConvert.SerializeObject(invoiceItems.ConvertAll(item => new
                {
                    id = item.ID,
                    title = item.Title,
                    stockKeepingUnit = item.StockKeepingUnit,
                    description = item.Description,
                    price = item.Price,
                    quantity = item.Quantity,
                    stockQuantity = item.StockQuantity,
                    trackInventory = item.TrackInventory,
                    invoiceTax1ID = item.InvoiceTax1ID,
                    invoiceTax2ID = item.InvoiceTax2ID
                }));

            var invoiceTaxes = Global.DaoFactory.GetInvoiceTaxDao().GetAll();
            var invoiceTaxesJson = JsonConvert.SerializeObject(invoiceTaxes.ConvertAll(item => new
                {
                    id = item.ID,
                    name = item.Name,
                    rate = item.Rate,
                    description = item.Description
                }));

            var invoiceSettings = Global.TenantSettings.InvoiceSetting ?? InvoiceSetting.DefaultSettings;
            var invoiceSettingsJson = JsonConvert.SerializeObject(new
                {
                    autogenerated = invoiceSettings.Autogenerated,
                    prefix = invoiceSettings.Prefix,
                    number = invoiceSettings.Number,
                    terms = invoiceSettings.Terms
                });

            var presetContactsJson = string.Empty;
            var presetContactID = UrlParameters.ContactID;
            if (targetInvoice == null && presetContactID != 0)
            {
                var targetContact = Global.DaoFactory.GetContactDao().GetByID(presetContactID);
                if (targetContact != null)
                {
                    presetContactsJson = JsonConvert.SerializeObject(new
                    {
                        id = targetContact.ID,
                        displayName = targetContact.GetTitle().HtmlEncode().ReplaceSingleQuote(),
                        smallFotoUrl = ContactPhotoManager.GetSmallSizePhoto(targetContact.ID, targetContact is Company),
                        currencyAbbreviation = targetContact.Currency
                    });
                }
            }

            var currencyRates = Global.DaoFactory.GetCurrencyRateDao().GetAll();
            var currencyRatesJson = JsonConvert.SerializeObject(currencyRates.ConvertAll(item => new
            {
                id = item.ID,
                fromCurrency = item.FromCurrency,
                toCurrency = item.ToCurrency,
                rate = item.Rate
            }));

            var apiServer = new Api.ApiServer();
            const string apiUrlFormat = "{0}crm/contact/{1}/data.json";

            var contactInfoData = string.Empty;
            var consigneeInfoData = string.Empty;
            
            if(targetInvoice != null)
            {
                if (targetInvoice.ContactID > 0)
                {
                    contactInfoData = apiServer.GetApiResponse(String.Format(apiUrlFormat, SetupInfo.WebApiBaseUrl, targetInvoice.ContactID), "GET");
                }
                if (targetInvoice.ConsigneeID > 0)
                {
                    consigneeInfoData = apiServer.GetApiResponse(String.Format(apiUrlFormat, SetupInfo.WebApiBaseUrl, targetInvoice.ConsigneeID), "GET");
                }
            } else if (presetContactID != 0)
            {
                contactInfoData = apiServer.GetApiResponse(String.Format(apiUrlFormat, SetupInfo.WebApiBaseUrl, presetContactID), "GET");
            }

            var apiUrl = String.Format("{0}crm/invoice/{1}.json",
                                       SetupInfo.WebApiBaseUrl,
                                       targetInvoice != null ? targetInvoice.ID.ToString(CultureInfo.InvariantCulture) : "sample");
            var invoiceData = apiServer.GetApiResponse(apiUrl, "GET");

            var script = String.Format(@"
                                        var invoiceItems = '{0}';
                                        var invoiceTaxes = '{1}';
                                        var invoiceSettings = '{2}';
                                        var invoicePresetContact = '{3}';
                                        var currencyRates = '{4}';
                                        var invoiceJsonData = '{5}'; ",
                                       Global.EncodeTo64(invoiceItemsJson),
                                       Global.EncodeTo64(invoiceTaxesJson),
                                       Global.EncodeTo64(invoiceSettingsJson),
                                       Global.EncodeTo64(presetContactsJson),
                                       Global.EncodeTo64(currencyRatesJson),
                                       targetInvoice != null ? Global.EncodeTo64(targetInvoice.JsonData) : ""
                );

            page.RegisterInlineScript(script, onReady: false);
            page.JsonPublisher(contactInfoData, "invoiceContactInfo");
            page.JsonPublisher(consigneeInfoData, "invoiceConsigneeInfo");
            page.JsonPublisher(invoiceData, "invoice");
        }

        public static void DataInvoicesDetailsView(BasePage page, Invoice targetInvoice)
        {
            if(targetInvoice == null)
                return;

            var script = String.Format(@"var invoiceData = '{0}';", Global.EncodeTo64(targetInvoice.JsonData));

            var apiServer = new Api.ApiServer();
            var apiUrl = String.Format("{0}crm/invoice/{1}.json",SetupInfo.WebApiBaseUrl, targetInvoice.ID);
            var invoice = apiServer.GetApiResponse(apiUrl, "GET");

            page.RegisterInlineScript(script, onReady: false);
            page.JsonPublisher(invoice, "invoice");
        }

        #endregion


    }
}