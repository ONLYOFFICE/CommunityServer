/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Common.Threading.Progress;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;

namespace ASC.Api.CRM
{
    ///<name>crm</name>
    public partial class CRMApi
    {
        /// <summary>
        /// Returns a list of all the currencies currently available on the portal.
        /// </summary>
        /// <short>Get available currencies</short> 
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyInfoWrapper, ASC.Api.CRM">
        /// List of available currencies
        /// </returns>
        /// <path>api/2.0/crm/settings/currency</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"settings/currency")]
        public IEnumerable<CurrencyInfoWrapper> GetAvaliableCurrency()
        {
            return CurrencyProvider.GetAll().ConvertAll(item => new CurrencyInfoWrapper(item)).ToItemList();
        }

        /// <summary>
        /// Returns a result of converting one currency into another.
        /// </summary>
        /// <param type="System.Decimal, System" method="url" name="amount">Amount to convert</param>
        /// <param type="System.String, System" method="url" name="fromcurrency">Currency to convert</param>
        /// <param type="System.String, System" method="url" name="tocurrency">Currency into which the original currency will be converted</param>
        /// <short>Convert a currency</short> 
        /// <category>Currencies</category>
        /// <returns>
        /// Decimal result of converting
        /// </returns>
        /// <path>api/2.0/crm/settings/currency/convert</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"settings/currency/convert")]
        public Decimal ConvertAmount(Decimal amount, String fromcurrency, String tocurrency)
        {
            return CurrencyProvider.MoneyConvert(amount, fromcurrency, tocurrency);
        }

        /// <summary>
        /// Returns a summary table with the rates for the currency specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by API">Currency (abbreviation)</param>
        /// <short>Get currency summary table</short> 
        /// <category>Currencies</category>
        /// <returns type = "ASC.Api.CRM.CurrencyRateInfoWrapper, ASC.Api.CRM">
        /// Dictionary of currency rates for the specified currency
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <path>api/2.0/crm/settings/currency/summarytable</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
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
        /// Updates the contact status setting with the parameter specified in the request.  
        /// </summary>
        /// <param type="System.Nullable{System.Boolean}, System" name="changeContactStatusGroupAuto" remark="true, false or null">Defines if the contact status setting is changed automatically or not</param>
        /// <short>Update the contact status setting</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// Updated contact status setting value (true, false or null)
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <path>api/2.0/crm/contact/status/settings</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// Updates the setting for writing mails to the history with the parameter specified in the request.   
        /// </summary>
        /// <param type="System.Boolean, System" name="writeMailToHistoryAuto" remark="true or false">Defines if the mails are written to the history automatically or not</param>
        /// <short>Update the setting for writing mails to the history</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// Updated setting for writing mails to the history (true or false)
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <path>api/2.0/crm/contact/mailtohistory/settings</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// Updates the setting for adding tags to the contact with the parameter specified in the request.    
        /// </summary>
        /// <param type="System.Nullable{System.Boolean}, System" name="addTagToContactGroupAuto" remark="true, false or null">Defines if a tag is added to the contact automatically or not</param>
        /// <short>Update the setting for adding tags to the contact</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// Updated setting for adding tags to the contact (true, false or null)
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <path>api/2.0/crm/contact/tag/settings</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// Sets the tenant settings specified in the request to the portal.
        /// </summary>
        /// <param type="System.Nullable{System.Boolean}, System" name="configured">Defines if the portal is configured or not</param>
        /// <param type="System.Nullable{System.Guid}, System" name="webFormKey">Website contact form key</param>
        /// <short>Set the tenant settings</short> 
        /// <category>Contacts</category>
        /// <returns>
        /// The tenant setting for the portal configuration value (true or false)
        /// </returns>
        /// <path>api/2.0/crm/settings</path>
        /// <httpMethod>PUT</httpMethod>
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
        ///  Updates a company name with the one specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="companyName">New company name</param>
        /// <short>Update a company name</short>
        /// <category>Organization</category>
        /// <returns>Updated company name</returns>
        /// <path>api/2.0/crm/settings/organisation/base</path>
        /// <httpMethod>PUT</httpMethod>
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
        ///  Updates a company address with the one specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="street">New company street/building/apartment</param>
        /// <param type="System.String, System" name="city">New company city</param>
        /// <param type="System.String, System" name="state">New company state</param>
        /// <param type="System.String, System" name="zip">New company zip</param>
        /// <param type="System.String, System" name="country">New company country</param>
        /// <short>Update a company address</short>
        /// <category>Organization</category>
        /// <returns>Updated company address</returns>
        /// <exception cref="SecurityException"></exception>
        /// <path>api/2.0/crm/settings/organisation/address</path>
        /// <httpMethod>PUT</httpMethod>
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
        ///  Updates the organization logo setting with the parameter specified in the request.
        /// </summary>
        /// <param type="System.Boolean, System" name="reset">Resets the organization logo or not</param>
        /// <short>Update the organization logo setting</short>
        /// <category>Organization</category>
        /// <returns>Organization logo ID</returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="Exception"></exception>
        /// <path>api/2.0/crm/settings/organisation/logo</path>
        /// <httpMethod>PUT</httpMethod>
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
        ///  Returns an organization logo with the ID specified in the request in the base64 format.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id" remark="If this parameter is equal to 0, then the current logo is taken">Organization logo ID</param>
        /// <short>Get an organization logo</short>
        /// <category>Organization</category>
        /// <returns>Organization logo in the base64 format</returns>
        /// <exception cref="Exception"></exception>
        /// <path>api/2.0/crm/settings/organisation/logo</path>
        /// <httpMethod>GET</httpMethod>
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
        ///  Updates the website contact form key.
        /// </summary>
        /// <short>Update the web form key</short>
        /// <category>Contacts</category>
        /// <returns>Updated web form key</returns>
        /// <exception cref="SecurityException"></exception>
        /// <path>api/2.0/crm/settings/webformkey/change</path>
        /// <httpMethod>PUT</httpMethod>
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
        /// Updates the default CRM currency with the currency specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by API">Currency (abbreviation)</param>
        /// <short>Update a currency</short>
        /// <category>Currencies</category>
        /// <returns type="ASC.Api.CRM.CurrencyInfoWrapper, ASC.Api.CRM">Updated currency</returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <path>api/2.0/crm/settings/currency</path>
        /// <httpMethod>PUT</httpMethod>
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

        /// <summary>
        /// Starts an import of the contacts, opportunities, cases, or tasks from the csv file specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="entityType" remark="Allowed values: contact, task, opportunity, case">Entity type</param>
        /// <param type="System.String, System" name="csvFileURI">URI to the csv file</param>
        /// <param type="System.String, System" name="jsonSettings">JSON settings in the string format</param>
        /// <short>Start import from csv file</short>
        /// <category>Import</category>
        /// <path>api/2.0/crm/{entityType}/import/start</path>
        /// <httpMethod>POST</httpMethod>
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

        /// <summary>
        /// Returns an import status of the csv file.
        /// </summary>
        /// <param type="System.String, System" name="entityType" remark="Allowed values: contact, task, opportunity, case">Entity type</param>
        /// <short>Get import status</short>
        /// <category>Import</category>
        /// <returns>Import status</returns>
        /// <path>api/2.0/crm/{entityType}/import/status</path>
        /// <httpMethod>GET</httpMethod>
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

        /// <summary>
        /// Returns a sample row from the imported csv file specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="csvFileURI">URI to the csv file</param>
        /// <param type="System.Int32, System" name="indexRow">Sample row index</param>
        /// <param type="System.String, System" name="jsonSettings">JSON settings in the string format</param>
        /// <short>Get a sample row</short>
        /// <category>Import</category>
        /// <returns>Sample row</returns>
        /// <path>api/2.0/crm/import/samplerow</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read(@"import/samplerow")]
        public String GetImportFromCSVSampleRow(string csvFileURI, int indexRow, string jsonSettings)
        {
            if (String.IsNullOrEmpty(csvFileURI) || indexRow < 0) throw new ArgumentException();

            if (!Global.GetStore().IsFile("temp", csvFileURI)) throw new ArgumentException();

            var CSVFileStream = Global.GetStore().GetReadStream("temp", csvFileURI);

            return ImportFromCSV.GetRow(CSVFileStream, indexRow, jsonSettings);
        }

        /// <summary>
        /// Processes a fake upload of the csv file specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="csvFileURI">URI to the csv file</param>
        /// <param type="System.String, System" name="jsonSettings">JSON settings in the string format</param>
        /// <short>Process fake upload</short>
        /// <category>Import</category>
        /// <returns>Uploaded file</returns>
        /// <path>api/2.0/crm/import/uploadfake</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create(@"import/uploadfake")]
        public FileUploadResult ProcessUploadFake(string csvFileURI, string jsonSettings)
        {
            return new ImportFromCSVManager().ProcessUploadFake(csvFileURI, jsonSettings);
        }

        /// <summary>
        /// Returns an export status of the csv file.
        /// </summary>
        /// <short>Get export status</short>
        /// <category>Export</category>
        /// <returns>Export status</returns>
        /// <path>api/2.0/crm/export/status</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read(@"export/status")]
        public IProgressItem GetExportStatus()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();
            return ExportToCsv.GetStatus(false);
        }

        /// <summary>
        /// Cancels an export to the csv file.
        /// </summary>
        /// <short>Cancel export to csv file</short>
        /// <category>Export</category>
        /// <returns>Export status</returns>
        /// <path>api/2.0/crm/export/cancel</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"export/cancel")]
        public IProgressItem CancelExport()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();
            ExportToCsv.Cancel(false);
            return ExportToCsv.GetStatus(false);
        }

        /// <summary>
        /// Starts an export to the csv file.
        /// </summary>
        /// <short>Start export to csv file</short>
        /// <category>Export</category>
        /// <returns>Export data operation</returns>
        /// <path>api/2.0/crm/export/start</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create(@"export/start")]
        public IProgressItem StartExport()
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            MessageService.Send(Request, MessageAction.CrmAllDataExported);

            return ExportToCsv.Start(null, string.Format("{0}_{1}.zip", CRMSettingResource.Export, DateTime.UtcNow.Ticks));
        }

        /// <summary>
        /// Returns a status of partial export to the csv file.
        /// </summary>
        /// <short>Get status of partial export</short>
        /// <category>Export</category>
        /// <returns>Partial export status</returns>
        /// <path>api/2.0/crm/export/partial/status</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read(@"export/partial/status")]
        public IProgressItem GetPartialExportStatus()
        {
            return ExportToCsv.GetStatus(true);
        }

        /// <summary>
        /// Cancels a partial export to the csv file.
        /// </summary>
        /// <short>Cancel partial export to csv file</short>
        /// <category>Export</category>
        /// <returns>Partial export status</returns>
        /// <path>api/2.0/crm/export/partial/cancel</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"export/partial/cancel")]
        public IProgressItem CancelPartialExport()
        {
            ExportToCsv.Cancel(true);
            return ExportToCsv.GetStatus(true);
        }

        /// <summary>
        /// Starts a partial export to the csv file.
        /// </summary>
        /// <param type="System.String, System" name="entityType" remark="Allowed values: contact, task, opportunity, case, invoiceitem">Entity type</param>
        /// <param type="System.String, System" name="base64FilterString">Filter string in the base64 format</param>
        /// <short>Start partial export to csv file</short>
        /// <category>Export</category>
        /// <returns>Export data operation</returns>
        /// <path>api/2.0/crm/export/partial/{entityType}/start</path>
        /// <httpMethod>POST</httpMethod>
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