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
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.CRM.Classes;
using ASC.CRM.Core;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core.Dao;
using ASC.Web.Core;
using ASC.Web.CRM.Configuration;
using ASC.CRM.Core.Entities;
using ASC.Web.Core.Client;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Web.CRM.Masters.ClientScripts
{
    public abstract class CrmClientScriptData : ClientScript
    {
        public static Func<string, string, bool, object> CsdDefaultConverter = (name, title, isHeader) => new { name, title, isHeader };
        public static Func<string, string, object> CsdConverter = (name, title) => CsdDefaultConverter(name, title, false);

        public static Converter<CustomField, object> CfConverter = customField =>
            CsdDefaultConverter("customField_" + customField.ID,
                customField.Label.HtmlEncode(),
                customField.FieldType == CustomFieldType.Heading);

        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        public List<object> GetList(string name, string items, string list, string item)
        {
            var result = new List<object>(12)
            {
                CsdDefaultConverter(string.Empty, items, true),
            };

            if (!string.IsNullOrEmpty(list))
            {
                result.Add(CsdConverter(name, list));
            }

            for (var i = 1; i < 10; i++)
            {
                result.Add(CsdConverter(name, String.Format("{0} {1}", item, i)));
            }

            return result;
        }

        public Converter<ListItem, object> GetCategoryConverter(string cssClass)
        {
            return item => new
            {
                id = item.ID,
                value = item.ID,
                title = item.Title.HtmlEncode(),
                cssClass = cssClass + " " + item.AdditionalParams.Split('.').FirstOrDefault()
            };
        }
    }

    #region Data for Common Data

    public class CommonData : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var admins = CoreContext.UserManager.GetUsersByGroup(Constants.GroupAdmin.ID).ToList();
            admins.AddRange(WebItemSecurity.GetProductAdministrators(ProductEntryPoint.ID).ToList());

            var securityInfo = WebItemSecurity.GetSecurityInfo(ProductEntryPoint.ID.ToString());
            var crmAvailable = securityInfo.Users.ToList();

            foreach (var group in securityInfo.Groups)
            {
                crmAvailable.AddRange(CoreContext.UserManager.GetUsersByGroup(group.ID));
            }

            var crmAvailableWithAdmins = new List<UserInfo>();
            crmAvailableWithAdmins.AddRange(crmAvailable);
            crmAvailableWithAdmins.AddRange(admins);
            crmAvailableWithAdmins = crmAvailableWithAdmins.Distinct().SortByUserName().ToList();

            using (var scope = DIHelper.Resolve())
            {
                var taskCategories = scope.Resolve<DaoFactory>().ListItemDao.GetItems(ListType.TaskCategory);
                Converter<UserInfo, object> converter = item =>
                    new
                    {
                        avatarSmall = item.GetSmallPhotoURL(),
                        displayName = item.DisplayUserName(),
                        id = item.ID,
                        title = item.Title.HtmlEncode()
                    };

                var categoryConverter = GetCategoryConverter("task_category");

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            crmAdminList = admins.ConvertAll(converter),
                            isCrmAvailableForAllUsers = crmAvailable.Count == 0,
                            crmAvailableWithAdminList = crmAvailableWithAdmins.ConvertAll(converter),
                            smtpSettings = GetSMTPSettingWithoutPassword(Global.TenantSettings.SMTPServerSetting),
                            taskCategories = taskCategories.ConvertAll(categoryConverter)
                        })
                };
            }
        }

        private SMTPServerSetting GetSMTPSettingWithoutPassword(SMTPServerSetting smtpSettings)
        {
            return new SMTPServerSetting
            {
                Host = smtpSettings.Host,
                Port = smtpSettings.Port,
                EnableSSL = smtpSettings.EnableSSL,
                RequiredHostAuthentication = smtpSettings.RequiredHostAuthentication,
                HostLogin = smtpSettings.HostLogin,
                HostPassword = string.Empty,
                SenderDisplayName = smtpSettings.SenderDisplayName,
                SenderEmailAddress = smtpSettings.SenderEmailAddress,
            };
        }
    }

    #endregion

    #region classes for Contact Views

    public class ListViewData : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();

                var allTags = daoFactory.TagDao.GetAllTags();
                var contactTags = allTags.Where(r => r.Key == EntityType.Contact).Select(r => r.Value).ToList();
                var caseTags = allTags.Where(r => r.Key == EntityType.Case).Select(r => r.Value).ToList();
                var opportunityTags = allTags.Where(r => r.Key == EntityType.Opportunity).Select(r => r.Value).ToList();

                var allListItems = daoFactory.ListItemDao.GetItems();
                var contactStatusListItems = allListItems.Where(r => r.ListType == ListType.ContactStatus).ToList();
                contactStatusListItems.Insert(0,
                    new ListItem {ID = 0, Title = CRMCommonResource.NotSpecified, Color = "0"});

                var contactStages = contactStatusListItems.ConvertAll(item => new
                {
                    value = item.ID,
                    title = item.Title.HtmlEncode(),
                    classname = "colorFilterItem color_" + item.Color.Replace("#", "").ToLower()
                });

                var contactTypeListItems = allListItems.Where(r => r.ListType == ListType.ContactType).ToList();
                contactTypeListItems.Insert(0, new ListItem {ID = 0, Title = CRMContactResource.CategoryNotSpecified});

                var contactTypes = contactTypeListItems.ConvertAll(item => new
                {
                    value = item.ID,
                    title = item.Title.HtmlEncode()
                });

                var dealMilestones = daoFactory.DealMilestoneDao.GetAll();

                Converter<string, object> tagsConverter = item => new
                {
                    value = item.HtmlEncode(),
                    title = item.HtmlEncode()
                };
                Converter<InvoiceStatus, object> invoiceStatusesConverter = item => new
                {
                    value = (int) item,
                    displayname = item.ToLocalizedString(),
                    apiname = item.ToString().ToLower()
                };

                var invoiceStatuses = new List<InvoiceStatus>(4)
                {
                    InvoiceStatus.Draft,
                    InvoiceStatus.Sent,
                    InvoiceStatus.Rejected,
                    InvoiceStatus.Paid
                }
                    .ConvertAll(invoiceStatusesConverter);

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            contactStages,
                            contactTypes,
                            contactTags = contactTags.ConvertAll(tagsConverter),
                            caseTags = caseTags.ConvertAll(tagsConverter),
                            dealTags = opportunityTags.ConvertAll(tagsConverter),
                            mailQuotas = MailSender.GetQuotas(),
                            dealMilestones = dealMilestones.ConvertAll(item =>
                                new
                                {
                                    value = item.ID,
                                    title = item.Title,
                                    classname = "colorFilterItem color_" + item.Color.Replace("#", "").ToLower()
                                }),
                            invoiceStatuses,
                            currencies = CurrencyProvider.GetAll()
                        })
                };
            }
        }
    }

    #endregion


    #region classes for Opportunity Views

    public class ExchangeRateViewData : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var defaultCurrency = Global.TenantSettings.DefaultCurrency;
            var publisherDate = CurrencyProvider.GetPublisherDate;

            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                           new
                           {
                               defaultCurrency,
                               ratesPublisherDisplayDate = string.Format("{0} {1}", publisherDate.ToShortDateString(), publisherDate.ToShortTimeString())
                           })
                   };
        }
    }

    #endregion

    #region Data for History View

    public class HistoryViewData : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var listItemDao = daoFactory.ListItemDao;
                var eventsCategories = listItemDao.GetItems(ListType.HistoryCategory);
                var systemCategories = listItemDao.GetSystemItems();

                var categoryConverter = GetCategoryConverter("event_category");

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            eventsCategories = eventsCategories.ConvertAll(categoryConverter),
                            systemCategories = systemCategories.ConvertAll(categoryConverter),
                            historyEntityTypes = new[]
                            {
                                new
                                {
                                    value = (int) EntityType.Opportunity,
                                    displayname = CRMDealResource.Deal,
                                    apiname = EntityType.Opportunity.ToString().ToLower()
                                },
                                new
                                {
                                    value = (int) EntityType.Case,
                                    displayname = CRMCasesResource.Case,
                                    apiname = EntityType.Case.ToString().ToLower()
                                }
                            }
                        })
                };
            }
        }

    }

    #endregion

    #region Data for Invoice Views

    public class InvoiceItemActionViewData : CrmClientScriptData
    {
        protected override string GetCacheHash()
        {
            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<DaoFactory>().InvoiceTaxDao.GetMaxLastModified().Ticks.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                //yield return RegisterObject("currencies", CurrencyProvider.GetAll());
                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(new {taxes = scope.Resolve<DaoFactory>().InvoiceTaxDao.GetAll()})
                };
            }
        }
    }

    #endregion

    #region Data for CRM Settings Views

    public class OrganisationProfileViewData : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var settings = Global.TenantSettings.InvoiceSetting ?? InvoiceSetting.DefaultSettings;
            var logo_base64 = OrganisationLogoManager.GetOrganisationLogoSrc(settings.CompanyLogoID);

            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                           new
                           {
                               InvoiceSetting = settings,
                               InvoiceSetting_logo_src = logo_base64
                           })
                   };
        }
    }

    public class WebToLeadFormViewData : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            Func<string, string, int, string, object> defaultConverter = (name, title, type, mask) =>
                new
                {
                    type = -1,
                    name,
                    title,
                    mask = ""
                };

            Func<string, string, object> converter = (name, title) => defaultConverter(name, title, -1, "");

            var columnSelectorData = new List<object>
            {
                converter("firstName", CRMContactResource.FirstName),
                converter("lastName", CRMContactResource.LastName),
                converter("jobTitle", CRMContactResource.JobTitle),
                converter("companyName", CRMContactResource.CompanyName),
                converter("about", CRMContactResource.About)
            };

            foreach (ContactInfoType infoTypeEnum in Enum.GetValues(typeof (ContactInfoType)))
            {
                var localName = string.Format("contactInfo_{0}_{1}", infoTypeEnum, ContactInfo.GetDefaultCategory(infoTypeEnum));
                var localTitle = infoTypeEnum.ToLocalizedString();

                if (infoTypeEnum == ContactInfoType.Address)
                {
                    foreach (AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart)))
                        columnSelectorData.Add(converter(
                            string.Format(localName + "_{0}_{1}", addressPartEnum, (int) AddressCategory.Work),
                            string.Format(localTitle + " {0}", addressPartEnum.ToLocalizedString().ToLower())));
                }
                else
                {
                    columnSelectorData.Add(converter(localName, localTitle));
                }
            }

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var columnSelectorDataCompany = columnSelectorData.ToList();
                var columnSelectorDataPerson = columnSelectorData.ToList();
                var customFieldDao = daoFactory.CustomFieldDao;

                Predicate<CustomField> customFieldPredicate = customField =>
                    customField.FieldType == CustomFieldType.TextField ||
                    customField.FieldType == CustomFieldType.TextArea ||
                    customField.FieldType == CustomFieldType.CheckBox ||
                    customField.FieldType == CustomFieldType.SelectBox;

                Converter<CustomField, object> customFieldConverter =
                    customField =>
                        defaultConverter("customField_" + customField.ID, customField.Label, (int) customField.FieldType,
                            customField.Mask);

                Func<EntityType, List<object>> getFieldsDescription = entityType =>
                    customFieldDao.GetFieldsDescription(entityType)
                        .FindAll(customFieldPredicate)
                        .ConvertAll(customFieldConverter);

                columnSelectorDataCompany.AddRange(getFieldsDescription(EntityType.Company));
                columnSelectorDataPerson.AddRange(getFieldsDescription(EntityType.Person));

                var tagList = daoFactory.TagDao.GetAllTags(EntityType.Contact).ToList();

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            tagList,
                            columnSelectorDataCompany,
                            columnSelectorDataPerson
                        })
                };
            }
        }
    }

    #endregion

    #region Data for CRM Import From CSV Views

    public class ImportFromCSVViewDataContacts : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var columnSelectorData = new List<object>(10)
                {
                    CsdConverter(string.Empty, CRMContactResource.NoMatchSelect),
                    CsdConverter("-1", CRMContactResource.DoNotImportThisField),
                    CsdDefaultConverter(string.Empty, CRMContactResource.GeneralInformation, true),
                    CsdConverter("firstName", CRMContactResource.FirstName),
                    CsdConverter("lastName", CRMContactResource.LastName),
                    CsdConverter("title", CRMContactResource.JobTitle),
                    CsdConverter("companyName", CRMContactResource.CompanyName),
                    CsdConverter("contactStage", CRMContactResource.ContactStage),
                    CsdConverter("contactType", CRMContactResource.ContactType),
                    CsdConverter("notes", CRMContactResource.About)
                };

                foreach (ContactInfoType infoTypeEnum in Enum.GetValues(typeof(ContactInfoType)))
                    foreach (Enum categoryEnum in Enum.GetValues(ContactInfo.GetCategory(infoTypeEnum)))
                    {
                        var localName = string.Format("contactInfo_{0}_{1}", infoTypeEnum, Convert.ToInt32(categoryEnum));
                        var localTitle = string.Format("{1} ({0})", categoryEnum.ToLocalizedString().ToLower(),
                            infoTypeEnum.ToLocalizedString());

                        if (infoTypeEnum == ContactInfoType.Address)
                        {
                            foreach (AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart)))
                            {
                                var name = string.Format(localName + "_{0}", addressPartEnum);
                                var title = string.Format(localTitle + " {0}",
                                    addressPartEnum.ToLocalizedString().ToLower());
                                columnSelectorData.Add(CsdConverter(name, title));
                            }
                        }
                        else
                        {
                            columnSelectorData.Add(CsdConverter(localName, localTitle));
                        }
                    }

                var fieldsDescription = daoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Company);

                daoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Person).ForEach(item =>
                {
                    var alreadyContains = fieldsDescription.Any(field => field.ID == item.ID);

                    if (!alreadyContains)
                        fieldsDescription.Add(item);
                });

                columnSelectorData.AddRange(fieldsDescription.ConvertAll(CfConverter));

                columnSelectorData.AddRange(GetList("tag", CRMContactResource.ContactTags,
                    CRMContactResource.ContactTagList, CRMContactResource.ContactTag));

                var tagList = daoFactory.TagDao.GetAllTags(EntityType.Contact).ToList();

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            tagList,
                            columnSelectorData
                        })
                };
            }
        }
    }

    public class ImportFromCSVViewDataTasks : ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var columnSelectorData = new List<object>(11)
            {
                CrmClientScriptData.CsdConverter(string.Empty, CRMContactResource.NoMatchSelect),
                CrmClientScriptData.CsdConverter("-1", CRMContactResource.DoNotImportThisField),
                CrmClientScriptData.CsdDefaultConverter(string.Empty, CRMContactResource.GeneralInformation, true),
                CrmClientScriptData.CsdConverter("title", CRMTaskResource.TaskTitle),
                CrmClientScriptData.CsdConverter("description", CRMTaskResource.Description),
                CrmClientScriptData.CsdConverter("due_date", CRMTaskResource.DueDate),
                CrmClientScriptData.CsdConverter("responsible", CRMTaskResource.Responsible),
                CrmClientScriptData.CsdConverter("contact", CRMContactResource.ContactTitle),
                CrmClientScriptData.CsdConverter("status", CRMTaskResource.TaskStatus),
                CrmClientScriptData.CsdConverter("taskCategory", CRMTaskResource.TaskCategory),
                CrmClientScriptData.CsdConverter("alertValue", CRMCommonResource.Alert)
            };

            return new List<KeyValuePair<string, object>>(1)
                   {
                       RegisterObject(
                           new
                           {
                               columnSelectorData
                           })
                   };
        }
    }

    public class ImportFromCSVViewDataCases : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var columnSelectorData = new List<object>(4)
                {
                    CsdConverter(string.Empty, CRMContactResource.NoMatchSelect),
                    CsdConverter("-1", CRMContactResource.DoNotImportThisField),
                    CsdDefaultConverter(String.Empty, CRMContactResource.GeneralInformation, true),
                    CsdConverter("title", CRMCasesResource.CaseTitle)
                };

                var fieldsDescription = daoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Case);
                columnSelectorData.AddRange(fieldsDescription.ConvertAll(CfConverter));
                columnSelectorData.AddRange(GetList("member", CRMCasesResource.CasesParticipants, string.Empty,
                    CRMCasesResource.CasesParticipant));
                columnSelectorData.AddRange(GetList("tag", CRMCasesResource.CasesTag, CRMCasesResource.CasesTagList,
                    CRMCasesResource.CasesTag));

                var tagList = daoFactory.TagDao.GetAllTags(EntityType.Case).ToList();

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            tagList,
                            columnSelectorData
                        })
                };
            }
        }
    }

    public class ImportFromCSVViewDataDeals : CrmClientScriptData
    {
        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var columnSelectorData = new List<object>(15)
            {
                CsdConverter(string.Empty, CRMContactResource.NoMatchSelect),
                CsdConverter("-1", CRMContactResource.DoNotImportThisField),
                CsdDefaultConverter(String.Empty, CRMContactResource.GeneralInformation, true),
                CsdConverter("title", CRMDealResource.NameDeal),
                CsdConverter("client", CRMDealResource.ClientDeal),
                CsdConverter("description", CRMDealResource.DescriptionDeal),
                CsdConverter("bid_currency", CRMCommonResource.Currency),
                CsdConverter("bid_amount", CRMDealResource.DealAmount),
                CsdConverter("bid_type", CRMDealResource.BidType),
                CsdConverter("per_period_value", CRMDealResource.BidTypePeriod),
                CsdConverter("responsible", CRMDealResource.ResponsibleDeal),
                CsdConverter("expected_close_date", CRMJSResource.ExpectedCloseDate),
                CsdConverter("actual_close_date", CRMJSResource.ActualCloseDate),
                CsdConverter("deal_milestone", CRMDealResource.CurrentDealMilestone),
                CsdConverter("probability_of_winning", CRMDealResource.ProbabilityOfWinning + " %")
            };

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var fieldsDescription = daoFactory.CustomFieldDao.GetFieldsDescription(EntityType.Opportunity);
                columnSelectorData.AddRange(fieldsDescription.ConvertAll(CfConverter));
                columnSelectorData.AddRange(GetList("member", CRMDealResource.DealParticipants, string.Empty, CRMDealResource.DealParticipant));
                columnSelectorData.AddRange(GetList("tag", CRMDealResource.DealTags, CRMDealResource.DealTagList, CRMDealResource.DealTag));

                var tagList = daoFactory.TagDao.GetAllTags(EntityType.Opportunity).ToList();

                return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                        {
                            tagList,
                            columnSelectorData
                        })
                };
            }
        }
    }

    #endregion

    #region Data for Reports Views

    public class ReportsViewData : CrmClientScriptData
    {
        protected override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey;
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterObject(
                        new
                            {
                                ReportType = new
                                    {
                                        SalesByManagers = (int) ReportType.SalesByManagers,
                                        SalesForecast = (int) ReportType.SalesForecast,
                                        SalesFunnel = (int) ReportType.SalesFunnel,
                                        WorkloadByContacts = (int) ReportType.WorkloadByContacts,
                                        WorkloadByTasks = (int) ReportType.WorkloadByTasks,
                                        WorkloadByDeals = (int) ReportType.WorkloadByDeals,
                                        WorkloadByInvoices = (int) ReportType.WorkloadByInvoices,
                                        WorkloadByVoip = (int) ReportType.WorkloadByVoip,
                                        SummaryForThePeriod = (int) ReportType.SummaryForThePeriod,
                                        SummaryAtThisMoment = (int) ReportType.SummaryAtThisMoment
                                    }
                            })
                };
        }
    }

    #endregion
}