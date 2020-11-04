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
using System.Linq;
using System.Security;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.CRM.Core.Entities;
using ASC.Common.Threading.Progress;
using ASC.CRM.Core;
using ASC.MessagingSystem;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Classes;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///     Returns the list of all currencies currently available on the portal
        /// </summary>
        /// <short>Get currency list</short> 
        /// <category>Common</category>
        /// <returns>
        ///    List of available currencies
        /// </returns>
        [Read(@"settings/currency")]
        public IEnumerable<CurrencyInfoWrapper> GetAvaliableCurrency()
        {
            return CurrencyProvider.GetAll().ConvertAll(item => new CurrencyInfoWrapper(item)).ToItemList();
        }

        /// <summary>
        ///     Returns the result of convertation from one currency to another
        /// </summary>
        /// <param name="amount">Amount to convert</param>
        /// <param name="fromcurrency">Old currency key</param>
        /// <param name="tocurrency">New currency key</param>
        /// <short>Get the result of convertation</short> 
        /// <category>Common</category>
        /// <returns>
        ///    Decimal result of convertation
        /// </returns>
        [Read(@"settings/currency/convert")]
        public Decimal ConvertAmount(Decimal amount, String fromcurrency, String tocurrency)
        {
            return CurrencyProvider.MoneyConvert(amount, fromcurrency, tocurrency);
        }

        /// <summary>
        ///     Returns the summary table with rates for selected currency
        /// </summary>
        /// <param name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by api">Currency (Abbreviation)</param>
        /// <short>Get the summary table</short> 
        /// <category>Common</category>
        /// <returns>
        ///    Dictionary of currencies and rates
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        [Read(@"settings/currency/summarytable")]
        public IEnumerable<CurrencyRateInfoWrapper> GetSummaryTable(String currency)
        {
            var result = new List<CurrencyRateInfoWrapper>();

            if (string.IsNullOrEmpty(currency))
            {
                throw new ArgumentException();
            }

            var cur = CurrencyProvider.Get(currency.ToUpper());
            if (cur == null) throw new ArgumentException();

            var table = CurrencyProvider.MoneyConvert(cur);
            table.ToList().ForEach(tableItem => result.Add(ToCurrencyRateInfoWrapper(tableItem.Key, tableItem.Value)));
            return result;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="changeContactStatusGroupAuto" remark="true, false or null">Change contact status group auto</param>
        /// <short></short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    ChangeContactStatusGroupAuto setting value (true, false or null)
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"contact/status/settings")]
        public Boolean? UpdateCRMContactStatusSettings(Boolean? changeContactStatusGroupAuto)
        {
            var tenantSettings = Global.TenantSettings;
            tenantSettings.ChangeContactStatusGroupAuto = changeContactStatusGroupAuto;
            tenantSettings.Save();

            MessageService.Send(Request, MessageAction.ContactTemperatureLevelSettingsUpdated);

            return changeContactStatusGroupAuto;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="writeMailToHistoryAuto" remark="true or false">Write mail to history auto</param>
        /// <short></short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    WriteMailToHistoryAuto setting value (true or false)
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"contact/mailtohistory/settings")]
        public Boolean UpdateCRMWriteMailToHistorySettings(Boolean writeMailToHistoryAuto)
        {
            var tenantSettings = Global.TenantSettings;
            tenantSettings.WriteMailToHistoryAuto = writeMailToHistoryAuto;
            tenantSettings.Save();
            //MessageService.Send(Request, MessageAction.ContactTemperatureLevelSettingsUpdated);

            return writeMailToHistoryAuto;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="addTagToContactGroupAuto" remark="true, false or null">add tag to contact group auto</param>
        /// <short></short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    AddTagToContactGroupAuto setting value (true, false or null)
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"contact/tag/settings")]
        public Boolean? UpdateCRMContactTagSettings(Boolean? addTagToContactGroupAuto)
        {
            var tenantSettings = Global.TenantSettings;
            tenantSettings.AddTagToContactGroupAuto = addTagToContactGroupAuto;
            tenantSettings.Save();

            MessageService.Send(Request, MessageAction.ContactsTagSettingsUpdated);

            return addTagToContactGroupAuto;
        }


        /// <summary>
        ///    Set IsConfiguredPortal tenant setting and website contact form key specified in the request
        /// </summary>
        /// <short>Set tenant settings</short> 
        /// <category>Common</category>
        /// <returns>
        ///    IsConfiguredPortal setting value (true or false)
        /// </returns>
        [Update(@"settings")]
        public Boolean SetIsPortalConfigured(Boolean? configured, Guid? webFormKey)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();
            var tenantSettings = Global.TenantSettings;
            tenantSettings.IsConfiguredPortal = configured ?? true;
            tenantSettings.WebFormKey = webFormKey ?? Guid.NewGuid();
            tenantSettings.Save();
            return tenantSettings.IsConfiguredPortal;
        }

        /// <summary>
        ///  Save organisation company name
        /// </summary>
        /// <param name="companyName">Organisation company name</param>
        /// <short>Save organisation company name</short>
        /// <category>Organisation</category>
        /// <returns>Organisation company name</returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"settings/organisation/base")]
        public String UpdateOrganisationSettingsCompanyName(String companyName)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();
            var tenantSettings = Global.TenantSettings;
            if (tenantSettings.InvoiceSetting == null)
            {
                tenantSettings.InvoiceSetting = InvoiceSetting.DefaultSettings;
            }
            tenantSettings.InvoiceSetting.CompanyName = companyName;

            tenantSettings.Save();

            MessageService.Send(Request, MessageAction.OrganizationProfileUpdatedCompanyName, companyName);

            return companyName;
        }

        /// <summary>
        ///  Save organisation company address
        /// </summary>
        /// <param name="street">Organisation company street/building/apartment address</param>
        /// <param name="city">City</param>
        /// <param name="state">State</param>
        /// <param name="zip">Zip</param>
        /// <param name="country">Country</param>
        /// <short>Save organisation company address</short>
        /// <category>Organisation</category>
        /// <returns>Returns a JSON object with the organization company address details</returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"settings/organisation/address")]
        public String UpdateOrganisationSettingsCompanyAddress(String street, String city, String state, String zip, String country)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var tenantSettings = Global.TenantSettings;

            if (tenantSettings.InvoiceSetting == null)
            {
                tenantSettings.InvoiceSetting = InvoiceSetting.DefaultSettings;
            }

            var companyAddress = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    type = AddressCategory.Billing.ToString(),
                    street,
                    city,
                    state,
                    zip,
                    country
                });

            tenantSettings.InvoiceSetting.CompanyAddress = companyAddress;

            tenantSettings.Save();

            MessageService.Send(Request, MessageAction.OrganizationProfileUpdatedAddress);

            return companyAddress;
        }

        /// <summary>
        ///  Save organisation logo
        /// </summary>
        /// <param name="reset">Reset organisation logo</param>
        /// <short>Save organisation logo</short>
        /// <category>Organisation</category>
        /// <returns>Organisation logo ID</returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="Exception"></exception>
        [Update(@"settings/organisation/logo")]
        public Int32 UpdateOrganisationSettingsLogo(bool reset)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();
            int companyLogoID;
            if (!reset)
            {
                companyLogoID = OrganisationLogoManager.TryUploadOrganisationLogoFromTmp(DaoFactory);
                if (companyLogoID == 0)
                {
                    throw new Exception("Downloaded image not found");
                }
            }
            else
            {
                companyLogoID = 0;
            }

            var tenantSettings = Global.TenantSettings;
            if (tenantSettings.InvoiceSetting == null)
            {
                tenantSettings.InvoiceSetting = InvoiceSetting.DefaultSettings;
            }
            tenantSettings.InvoiceSetting.CompanyLogoID = companyLogoID;

            tenantSettings.Save();
            MessageService.Send(Request, MessageAction.OrganizationProfileUpdatedInvoiceLogo);

            return companyLogoID;
        }

        /// <summary>
        ///  Get organisation logo in base64 format  (if 'id' is 0 then take current logo)
        /// </summary>
        /// <param name="id">organisation logo id</param>
        /// <short>Get organisation logo</short>
        /// <category>Organisation</category>
        /// <returns>Organisation logo content in base64</returns>
        /// <exception cref="Exception"></exception>
        [Read(@"settings/organisation/logo")]
        public String GetOrganisationSettingsLogo(int id)
        {
            if (id != 0)
            {
                return OrganisationLogoManager.GetOrganisationLogoBase64(id);
            }
            else
            {
                var tenantSettings = Global.TenantSettings;
                if (tenantSettings.InvoiceSetting == null)
                {
                    return string.Empty;
                }

                return OrganisationLogoManager.GetOrganisationLogoBase64(tenantSettings.InvoiceSetting.CompanyLogoID);
            }
        }

        /// <summary>
        ///  Change Website Contact Form key
        /// </summary>
        /// <short>Change web form key</short>
        /// <category>Common</category>
        /// <returns>Web form key</returns>
        /// <exception cref="SecurityException"></exception>
        [Update(@"settings/webformkey/change")]
        public string ChangeWebToLeadFormKey()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var tenantSettings = Global.TenantSettings;
            tenantSettings.WebFormKey = Guid.NewGuid();

            tenantSettings.Save();
            MessageService.Send(Request, MessageAction.WebsiteContactFormUpdatedKey);

            return tenantSettings.WebFormKey.ToString();
        }

        /// <summary>
        ///  Change default CRM currency
        /// </summary>
        /// <param name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by api">Currency (Abbreviation)</param>
        /// <short>Change currency</short>
        /// <category>Common</category>
        /// <returns>currency</returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [Update(@"settings/currency")]
        public CurrencyInfoWrapper UpdateCRMCurrency(String currency)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            if (string.IsNullOrEmpty(currency))
            {
                throw new ArgumentException();
            }
            currency = currency.ToUpper();
            var cur = CurrencyProvider.Get(currency);
            if (cur == null) throw new ArgumentException();

            Global.SaveDefaultCurrencySettings(cur);
            MessageService.Send(Request, MessageAction.CrmDefaultCurrencyUpdated);

            return ToCurrencyInfoWrapper(cur);
        }

        /// <visible>false</visible>
        [Create(@"{entityType:(contact|opportunity|case|task)}/import/start")]
        public string StartImportFromCSV(string entityType, string csvFileURI, string jsonSettings)
        {
            EntityType entityTypeObj;

            if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(csvFileURI) || string.IsNullOrEmpty(jsonSettings)) throw new ArgumentException();
            switch (entityType.ToLower())
            {
                case "contact":
                    entityTypeObj = EntityType.Contact;
                    break;
                case "opportunity":
                    entityTypeObj = EntityType.Opportunity;
                    break;
                case "case":
                    entityTypeObj = EntityType.Case;
                    break;
                case "task":
                    entityTypeObj = EntityType.Task;
                    break;
                default:
                    throw new ArgumentException();
            }

            new ImportFromCSVManager().StartImport(entityTypeObj, csvFileURI, jsonSettings);
            return "";
        }

        /// <visible>false</visible>
        [Read(@"{entityType:(contact|opportunity|case|task)}/import/status")]
        public IProgressItem GetImportFromCSVStatus(string entityType)
        {
            EntityType entityTypeObj;

            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException();
            switch (entityType.ToLower())
            {
                case "contact":
                    entityTypeObj = EntityType.Contact;
                    break;
                case "opportunity":
                    entityTypeObj = EntityType.Opportunity;
                    break;
                case "case":
                    entityTypeObj = EntityType.Case;
                    break;
                case "task":
                    entityTypeObj = EntityType.Task;
                    break;
                default:
                    throw new ArgumentException();
            }

            return ImportFromCSV.GetStatus(entityTypeObj);
        }

        /// <visible>false</visible>
        [Read(@"import/samplerow")]
        public String GetImportFromCSVSampleRow(string csvFileURI, int indexRow, string jsonSettings)
        {
            if (String.IsNullOrEmpty(csvFileURI) || indexRow < 0) throw new ArgumentException();

            if (!Global.GetStore().IsFile("temp", csvFileURI)) throw new ArgumentException();

            var CSVFileStream = Global.GetStore().GetReadStream("temp", csvFileURI);

            return ImportFromCSV.GetRow(CSVFileStream, indexRow, jsonSettings);
        }

        /// <visible>false</visible>
        [Create(@"import/uploadfake")]
        public FileUploadResult ProcessUploadFake(string csvFileURI, string jsonSettings)
        {
            return new ImportFromCSVManager().ProcessUploadFake(csvFileURI, jsonSettings);
        }

        /// <visible>false</visible>
        [Read(@"export/status")]
        public IProgressItem GetExportStatus()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();
            return ExportToCsv.GetStatus(false);
        }

        /// <visible>false</visible>
        [Update(@"export/cancel")]
        public IProgressItem CancelExport()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();
            ExportToCsv.Cancel(false);
            return ExportToCsv.GetStatus(false);
        }

        /// <visible>false</visible>
        [Create(@"export/start")]
        public IProgressItem StartExport()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            MessageService.Send(Request, MessageAction.CrmAllDataExported);

            return ExportToCsv.Start(null, string.Format("{0}_{1}.zip", CRMSettingResource.Export, DateTime.UtcNow.Ticks));
        }

        /// <visible>false</visible>
        [Read(@"export/partial/status")]
        public IProgressItem GetPartialExportStatus()
        {
            return ExportToCsv.GetStatus(true);
        }

        /// <visible>false</visible>
        [Update(@"export/partial/cancel")]
        public IProgressItem CancelPartialExport()
        {
            ExportToCsv.Cancel(true);
            return ExportToCsv.GetStatus(true);
        }

        /// <visible>false</visible>
        [Create(@"export/partial/{entityType:(contact|opportunity|case|task|invoiceitem)}/start")]
        public IProgressItem StartPartialExport(string entityType, string base64FilterString)
        {
            if (string.IsNullOrEmpty(base64FilterString)) throw new ArgumentException();
            
            FilterObject filterObject;
            String fileName;

            switch (entityType.ToLower())
            {
                case "contact":
                    filterObject = new ContactFilterObject(base64FilterString);
                    fileName = CRMContactResource.Contacts + ".csv";
                    MessageService.Send(Request, MessageAction.ContactsExportedToCsv);
                    break;
                case "opportunity":
                    filterObject = new DealFilterObject(base64FilterString);
                    fileName = CRMCommonResource.DealModuleName + ".csv";
                    MessageService.Send(Request, MessageAction.OpportunitiesExportedToCsv);
                    break;
                case "case":
                    filterObject = new CasesFilterObject(base64FilterString);
                    fileName = CRMCommonResource.CasesModuleName + ".csv";
                    MessageService.Send(Request, MessageAction.CasesExportedToCsv);
                    break;
                case "task":
                    filterObject = new TaskFilterObject(base64FilterString);
                    fileName = CRMCommonResource.TaskModuleName + ".csv";
                    MessageService.Send(Request, MessageAction.CrmTasksExportedToCsv);
                    break;
                case "invoiceitem":
                    fileName = CRMCommonResource.ProductsAndServices + ".csv";
                    filterObject = new InvoiceItemFilterObject(base64FilterString);
                    break;
                default:
                    throw new ArgumentException();
            }

            return ExportToCsv.Start(filterObject, fileName);
        }


        protected CurrencyInfoWrapper ToCurrencyInfoWrapper(CurrencyInfo currencyInfo)
        {
            return currencyInfo == null ? null : new CurrencyInfoWrapper(currencyInfo);
        }

        protected CurrencyRateInfoWrapper ToCurrencyRateInfoWrapper(CurrencyInfo currencyInfo, Decimal rate)
        {
            return currencyInfo == null ? null : new CurrencyRateInfoWrapper(currencyInfo, rate);
        }
    }
}