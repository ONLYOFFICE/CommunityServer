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
using System.Linq;
using System.Collections.Generic;
using System.Web;
using ASC.Web.CRM.Resources;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.CRM.Classes;
using ASC.CRM.Core;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.CRM.Configuration;
using System.Globalization;

namespace ASC.Web.CRM.Masters.ClientScripts
{

    #region Data for Common Data

    public class CommonData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var admins = CoreContext.UserManager.GetUsersByGroup(Constants.GroupAdmin.ID).ToList();
            admins.AddRange(WebItemSecurity.GetProductAdministrators(ProductEntryPoint.ID).ToList());
            admins = admins.Distinct().ToList();
            admins = admins.SortByUserName();

            var securityInfo = WebItemSecurity.GetSecurityInfo(ProductEntryPoint.ID.ToString());
            var crmAvailable = securityInfo.Users.ToList();

            foreach (var group in securityInfo.Groups)
            {
                crmAvailable.AddRange(CoreContext.UserManager.GetUsersByGroup(group.ID));
            }

            var crmAvailableWithAdmins = new List<UserInfo>();
            crmAvailableWithAdmins.AddRange(crmAvailable);
            crmAvailableWithAdmins.AddRange(admins);
            crmAvailableWithAdmins = crmAvailableWithAdmins.Distinct().ToList();
            crmAvailableWithAdmins = crmAvailableWithAdmins.SortByUserName();

            yield return RegisterObject("crmAdminList",
                                        admins.ConvertAll(item => new
                                            {
                                                avatarSmall = item.GetSmallPhotoURL(),
                                                displayName = item.DisplayUserName(),
                                                id = item.ID,
                                                title = item.Title.HtmlEncode()
                                            }));

            yield return RegisterObject("isCrmAvailableForAllUsers", crmAvailable.Count == 0);

            yield return RegisterObject("crmAvailableWithAdminList",
                                        crmAvailableWithAdmins.ConvertAll(item => new
                                            {
                                                avatarSmall = item.GetSmallPhotoURL(),
                                                displayName = item.DisplayUserName(),
                                                id = item.ID,
                                                title = item.Title.HtmlEncode()
                                            }));
        }
    }

    #endregion

    #region Data for SmtpSender

    public class SmtpSenderData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject("smtpSettings", Global.TenantSettings.SMTPServerSetting);
        }
    }

    #endregion

    #region classes for Contact Views

    public class ListContactViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var listItems = Global.DaoFactory.GetListItemDao().GetItems(ListType.ContactStatus);
            var tags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Contact).ToList();

            var contactStages = listItems.ConvertAll(item => new
                {
                    value = item.ID,
                    title = item.Title.HtmlEncode(),
                    classname = "colorFilterItem color_" + item.Color.Replace("#", "").ToLower()
                });
            contactStages.Insert(0, new
                {
                    value = 0,
                    title = CRMCommonResource.NotSpecified,
                    classname = "colorFilterItem color_0"
                });


            listItems = Global.DaoFactory.GetListItemDao().GetItems(ListType.ContactType);
            var contactTypes = listItems.ConvertAll(item => new
                {
                    value = item.ID,
                    title = item.Title.HtmlEncode()
                });
            contactTypes.Insert(0, new
                {
                    value = 0,
                    title = CRMContactResource.CategoryNotSpecified,
                });

            yield return RegisterObject("contactStages", contactStages);
            yield return RegisterObject("contactTypes", contactTypes);
            yield return RegisterObject("contactTags", tags.ConvertAll(item => new
                {
                    value = item.HtmlEncode(),
                    title = item.HtmlEncode()
                }));
            yield return RegisterObject("smtpSettings", Global.TenantSettings.SMTPServerSetting);

            yield return RegisterObject("mailQuotas", MailSender.GetQuotas());
        }
    }

    #endregion

    #region classes for Task Views

    public class ListTaskViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var taskCategories = Global.DaoFactory.GetListItemDao().GetItems(ListType.TaskCategory);

            yield return RegisterObject("taskCategories",
                                        taskCategories.ConvertAll(item => new
                                            {
                                                value = item.ID,
                                                title = item.Title.HtmlEncode(),
                                                cssClass = "task_category " + item.AdditionalParams.Split('.').FirstOrDefault()
                                            })
                );
        }
    }

    public class TaskActionViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var taskCategories = Global.DaoFactory.GetListItemDao().GetItems(ListType.TaskCategory);

            yield return RegisterObject("taskActionViewCategories",
                                        taskCategories.ConvertAll(item => new
                                            {
                                                id = item.ID,
                                                title = item.Title.HtmlEncode(),
                                                cssClass = "task_category " + item.AdditionalParams.Split('.').FirstOrDefault()
                                            }));
        }
    }

    #endregion

    #region classes for Cases Views

    public class ListCasesViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var tags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Case).ToList();

            yield return RegisterObject("caseTags", tags.ConvertAll(item => new
                {
                    value = item.HtmlEncode(),
                    title = item.HtmlEncode()
                }));
        }
    }

    #endregion

    #region classes for Opportunity Views

    public class ListDealViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var dealMilestones = Global.DaoFactory.GetDealMilestoneDao().GetAll();
            var tags = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Opportunity).ToList();

            yield return RegisterObject("dealMilestones", dealMilestones.ConvertAll(
                item => new
                    {
                        value = item.ID,
                        title = item.Title,
                        classname = "colorFilterItem color_" + item.Color.Replace("#", "").ToLower()
                    }));
            yield return RegisterObject("dealTags", tags.ConvertAll(
                item => new
                    {
                        value = item.HtmlEncode(),
                        title = item.HtmlEncode()
                    }));
        }
    }

    public class ExchangeRateViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var defaultCurrency = Global.TenantSettings.DefaultCurrency;
            var publisherDate = CurrencyProvider.GetPublisherDate;

            yield return RegisterObject("defaultCurrency", defaultCurrency);
            yield return RegisterObject("ratesPublisherDisplayDate", String.Format("{0} {1}", publisherDate.ToShortDateString(), publisherDate.ToShortTimeString()));
        }
    }

    #endregion

    #region Data for History View

    public class HistoryViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var eventsCategories = Global.DaoFactory.GetListItemDao().GetItems(ListType.HistoryCategory);
            var systemCategories = Global.DaoFactory.GetListItemDao().GetSystemItems();

            yield return RegisterObject("eventsCategories", eventsCategories.ConvertAll
                                                                (item => new
                                                                    {
                                                                        id = item.ID,
                                                                        value = item.ID,
                                                                        title = item.Title.HtmlEncode(),
                                                                        cssClass = "event_category " + item.AdditionalParams.Split('.').FirstOrDefault()
                                                                    }
                                                                ));
            yield return RegisterObject("systemCategories", systemCategories.ConvertAll
                                                                (item => new
                                                                    {
                                                                        id = item.ID,
                                                                        value = item.ID,
                                                                        title = item.Title.HtmlEncode(),
                                                                        cssClass = "event_category " + item.AdditionalParams.Split('.').FirstOrDefault()
                                                                    }
                                                                ));
            yield return RegisterObject("historyEntityTypes", new[]
                {
                    new
                        {
                            value = (int)EntityType.Opportunity,
                            displayname = CRMDealResource.Deal,
                            apiname = EntityType.Opportunity.ToString().ToLower()
                        },
                    new
                        {
                            value = (int)EntityType.Case,
                            displayname = CRMCasesResource.Case,
                            apiname = EntityType.Case.ToString().ToLower()
                        }
                });
        }
    }

    #endregion

    #region Data for Invoice Views

    public class ListInvoiceViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject("invoiceStatuses", new[]
                {
                    new
                        {
                            value = (int)InvoiceStatus.Draft,
                            displayname = InvoiceStatus.Draft.ToLocalizedString(),
                            apiname = InvoiceStatus.Draft.ToString().ToLower()
                        },
                    new
                        {
                            value = (int)InvoiceStatus.Sent,
                            displayname = InvoiceStatus.Sent.ToLocalizedString(),
                            apiname = InvoiceStatus.Sent.ToString().ToLower()
                        },
                    new
                        {
                            value = (int)InvoiceStatus.Rejected,
                            displayname = InvoiceStatus.Rejected.ToLocalizedString(),
                            apiname = InvoiceStatus.Rejected.ToString().ToLower()
                        },
                    new
                        {
                            value = (int)InvoiceStatus.Paid,
                            displayname = InvoiceStatus.Paid.ToLocalizedString(),
                            apiname = InvoiceStatus.Paid.ToString().ToLower()
                        }
                });


            yield return RegisterObject("currencies", CurrencyProvider.GetAll());
        }
    }

    public class InvoiceItemActionViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            //yield return RegisterObject("currencies", CurrencyProvider.GetAll());
            yield return RegisterObject("taxes", Global.DaoFactory.GetInvoiceTaxDao().GetAll());
        }
    }

    #endregion

    #region Data for CRM Settings Views

    public class OrganisationProfileViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var settings = Global.TenantSettings.InvoiceSetting ?? InvoiceSetting.DefaultSettings;
            var logo_base64 = OrganisationLogoManager.GetOrganisationLogoSrc(settings.CompanyLogoID);

            yield return RegisterObject("InvoiceSetting", settings);
            yield return RegisterObject("InvoiceSetting_logo_src", logo_base64);
            yield return RegisterObject("CountryListExt", Global.GetCountryListExt());
            yield return RegisterObject("CurrentCultureName", new RegionInfo(CultureInfo.CurrentCulture.Name).EnglishName);
        }
    }

    public class WebToLeadFormViewData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var columnSelectorData = new[]
                {
                    new
                        {
                            type = -1,
                            name = "firstName",
                            title = CRMContactResource.FirstName,
                            mask = ""
                        },
                    new
                        {
                            type = -1,
                            name = "lastName",
                            title = CRMContactResource.LastName,
                            mask = ""
                        },
                    new
                        {
                            type = -1,
                            name = "jobTitle",
                            title = CRMContactResource.JobTitle,
                            mask = ""
                        },
                    new
                        {
                            type = -1,
                            name = "companyName",
                            title = CRMContactResource.CompanyName,
                            mask = ""
                        },
                    new
                        {
                            type = -1,
                            name = "about",
                            title = CRMContactResource.About,
                            mask = ""
                        }
                }.ToList();

            foreach (ContactInfoType infoTypeEnum in Enum.GetValues(typeof(ContactInfoType)))
            {
                var localName = String.Format("contactInfo_{0}_{1}", infoTypeEnum, ContactInfo.GetDefaultCategory(infoTypeEnum));
                var localTitle = infoTypeEnum.ToLocalizedString();

                if (infoTypeEnum == ContactInfoType.Address)
                    foreach (AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart)))
                        columnSelectorData.Add(new
                            {
                                type = -1,
                                name = String.Format(localName + "_{0}_{1}", addressPartEnum, (int)AddressCategory.Work),
                                title = String.Format(localTitle + " {0}", addressPartEnum.ToLocalizedString().ToLower()),
                                mask = ""
                            });
                else
                    columnSelectorData.Add(new
                        {
                            type = -1,
                            name = localName,
                            title = localTitle,
                            mask = ""
                        });
            }

            var columnSelectorDataCompany = columnSelectorData.GetRange(0, columnSelectorData.Count).ToList();
            var columnSelectorDataPerson = columnSelectorData.GetRange(0, columnSelectorData.Count).ToList();

            columnSelectorDataCompany.AddRange(Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Company)
                                                     .FindAll(customField =>
                                                              customField.FieldType == CustomFieldType.TextField ||
                                                              customField.FieldType == CustomFieldType.TextArea ||
                                                              customField.FieldType == CustomFieldType.CheckBox ||
                                                              customField.FieldType == CustomFieldType.SelectBox)
                                                     .ConvertAll(customField => new
                                                         {
                                                             type = (int)customField.FieldType,
                                                             name = "customField_" + customField.ID,
                                                             title = customField.Label,
                                                             mask = customField.Mask
                                                         }));
            columnSelectorDataPerson.AddRange(Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Person)
                                                    .FindAll(customField =>
                                                             customField.FieldType == CustomFieldType.TextField ||
                                                             customField.FieldType == CustomFieldType.TextArea ||
                                                             customField.FieldType == CustomFieldType.CheckBox ||
                                                             customField.FieldType == CustomFieldType.SelectBox)
                                                    .ConvertAll(customField => new
                                                        {
                                                            type = (int)customField.FieldType,
                                                            name = "customField_" + customField.ID,
                                                            title = customField.Label,
                                                            mask = customField.Mask
                                                        }));

            var tagList = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Contact);

            yield return RegisterObject("tagList", tagList.ToList());
            yield return RegisterObject("columnSelectorDataCompany", columnSelectorDataCompany);
            yield return RegisterObject("columnSelectorDataPerson", columnSelectorDataPerson);
        }
    }

    #endregion

    #region Data for CRM Import From CSV Views

    public class ImportFromCSVViewDataContacts : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var columnSelectorData = new[]
                {
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.NoMatchSelect,
                            isHeader = false
                        },
                    new
                        {
                            name = "-1",
                            title = CRMContactResource.DoNotImportThisField,
                            isHeader = false
                        },
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.GeneralInformation,
                            isHeader = true
                        },
                    new
                        {
                            name = "firstName",
                            title = CRMContactResource.FirstName,
                            isHeader = false
                        },
                    new
                        {
                            name = "lastName",
                            title = CRMContactResource.LastName,
                            isHeader = false
                        },
                    new
                        {
                            name = "title",
                            title = CRMContactResource.JobTitle,
                            isHeader = false
                        },
                    new
                        {
                            name = "companyName",
                            title = CRMContactResource.CompanyName,
                            isHeader = false
                        },
                    new
                        {
                            name = "contactStage",
                            title = CRMContactResource.ContactStage,
                            isHeader = false
                        },
                    new
                        {
                            name = "contactType",
                            title = CRMContactResource.ContactType,
                            isHeader = false
                        },
                    new
                        {
                            name = "notes",
                            title = CRMContactResource.About,
                            isHeader = false
                        }
                }.ToList();

            foreach (ContactInfoType infoTypeEnum in Enum.GetValues(typeof(ContactInfoType)))
                foreach (Enum categoryEnum in Enum.GetValues(ContactInfo.GetCategory(infoTypeEnum)))
                {
                    var localName = String.Format("contactInfo_{0}_{1}", infoTypeEnum, Convert.ToInt32(categoryEnum));
                    var localTitle = String.Format("{1} ({0})", categoryEnum.ToLocalizedString().ToLower(), infoTypeEnum.ToLocalizedString());

                    if (infoTypeEnum == ContactInfoType.Address)
                        foreach (AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart)))
                            columnSelectorData.Add(new
                                {
                                    name = String.Format(localName + "_{0}", addressPartEnum),
                                    title = String.Format(localTitle + " {0}", addressPartEnum.ToLocalizedString().ToLower()),
                                    isHeader = false
                                });
                    else
                        columnSelectorData.Add(new
                            {
                                name = localName,
                                title = localTitle,
                                isHeader = false
                            });
                }

            var fieldsDescription = Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Company);

            Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Person).ForEach(item =>
                {
                    var alreadyContains = fieldsDescription.Any(field => field.ID == item.ID);

                    if (!alreadyContains)
                        fieldsDescription.Add(item);
                });

            columnSelectorData.AddRange(fieldsDescription
                                            .ConvertAll(customField => new
                                                {
                                                    name = "customField_" + customField.ID,
                                                    title = customField.Label.HtmlEncode(),
                                                    isHeader = customField.FieldType == CustomFieldType.Heading
                                                }));

            columnSelectorData.AddRange(
                new[]
                    {
                        new
                            {
                                name = String.Empty,
                                title = CRMContactResource.ContactTags,
                                isHeader = true
                            },
                        new
                            {
                                name = "tag",
                                title = CRMContactResource.ContactTagList,
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 1),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 2),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 3),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 4),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 5),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 6),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 7),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 8),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 9),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMContactResource.ContactTag, 10),
                                isHeader = false
                            }
                    }.ToList()
                );

            var tagList = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Contact);

            yield return RegisterObject("tagList", tagList.ToList());
            yield return RegisterObject("columnSelectorData", columnSelectorData);
        }
    }

    public class ImportFromCSVViewDataTasks : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var columnSelectorData = new[]
                {
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.NoMatchSelect,
                            isHeader = false
                        },
                    new
                        {
                            name = "-1",
                            title = CRMContactResource.DoNotImportThisField,
                            isHeader = false
                        },
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.GeneralInformation,
                            isHeader = true
                        },
                    new
                        {
                            name = "title",
                            title = CRMTaskResource.TaskTitle,
                            isHeader = false
                        },
                    new
                        {
                            name = "description",
                            title = CRMTaskResource.Description,
                            isHeader = false
                        },
                    new
                        {
                            name = "due_date",
                            title = CRMTaskResource.DueDate,
                            isHeader = false
                        },
                    new
                        {
                            name = "responsible",
                            title = CRMTaskResource.Responsible,
                            isHeader = false
                        },
                    new
                        {
                            name = "contact",
                            title = CRMContactResource.ContactTitle,
                            isHeader = false
                        },
                    new
                        {
                            name = "status",
                            title = CRMTaskResource.TaskStatus,
                            isHeader = false
                        },
                    new
                        {
                            name = "taskCategory",
                            title = CRMTaskResource.TaskCategory,
                            isHeader = false
                        },
                    new
                        {
                            name = "alertValue",
                            title = CRMCommonResource.Alert,
                            isHeader = false
                        }
                }.ToList();

            yield return RegisterObject("columnSelectorData", columnSelectorData);
        }
    }

    public class ImportFromCSVViewDataCases : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var columnSelectorData = new[]
                {
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.NoMatchSelect,
                            isHeader = false
                        },
                    new
                        {
                            name = "-1",
                            title = CRMContactResource.DoNotImportThisField,
                            isHeader = false
                        },
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.GeneralInformation,
                            isHeader = true
                        },
                    new
                        {
                            name = "title",
                            title = CRMCasesResource.CaseTitle,
                            isHeader = false
                        }
                }.ToList();

            var fieldsDescription = Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Case);
            columnSelectorData.AddRange(fieldsDescription
                                            .ConvertAll(customField => new
                                                {
                                                    name = "customField_" + customField.ID,
                                                    title = customField.Label.HtmlEncode(),
                                                    isHeader = customField.FieldType == CustomFieldType.Heading
                                                }));

            columnSelectorData.AddRange(
                new[]
                    {
                        new
                            {
                                name = String.Empty,
                                title = CRMCasesResource.CasesParticipants,
                                isHeader = true
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 1),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 2),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 3),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 4),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 5),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 6),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 7),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 8),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 9),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesParticipant, 10),
                                isHeader = false
                            }
                    });

            columnSelectorData.AddRange(
                new[]
                    {
                        new
                            {
                                name = String.Empty,
                                title = CRMCasesResource.CasesTag,
                                isHeader = true
                            },
                        new
                            {
                                name = "tag",
                                title = CRMCasesResource.CasesTagList,
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 1),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 2),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 3),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 4),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 5),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 6),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 7),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 8),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 9),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMCasesResource.CasesTag, 10),
                                isHeader = false
                            }
                    }
                );

            var tagList = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Case);

            yield return RegisterObject("tagList", tagList.ToList());
            yield return RegisterObject("columnSelectorData", columnSelectorData);
        }
    }

    public class ImportFromCSVViewDataDeals : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.CRM.Data"; }
        }

        protected override string GetCacheHash()
        {
            return Guid.NewGuid().ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            var columnSelectorData = new[]
                {
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.NoMatchSelect,
                            isHeader = false
                        },
                    new
                        {
                            name = "-1",
                            title = CRMContactResource.DoNotImportThisField,
                            isHeader = false
                        },
                    new
                        {
                            name = String.Empty,
                            title = CRMContactResource.GeneralInformation,
                            isHeader = true
                        },
                    new
                        {
                            name = "title",
                            title = CRMDealResource.NameDeal,
                            isHeader = false
                        },
                    new
                        {
                            name = "client",
                            title = CRMDealResource.ClientDeal,
                            isHeader = false
                        },
                    new
                        {
                            name = "description",
                            title = CRMDealResource.DescriptionDeal,
                            isHeader = false
                        },
                    new
                        {
                            name = "bid_currency",
                            title = CRMCommonResource.Currency,
                            isHeader = false
                        },
                    new
                        {
                            name = "bid_amount",
                            title = CRMDealResource.DealAmount,
                            isHeader = false
                        },
                    new
                        {
                            name = "bid_type",
                            title = CRMDealResource.BidType,
                            isHeader = false
                        },
                    new
                        {
                            name = "per_period_value",
                            title = CRMDealResource.BidTypePeriod,
                            isHeader = false
                        },
                    new
                        {
                            name = "responsible",
                            title = CRMDealResource.ResponsibleDeal,
                            isHeader = false
                        },
                    new
                        {
                            name = "expected_close_date",
                            title = CRMJSResource.ExpectedCloseDate,
                            isHeader = false
                        },
                    new
                        {
                            name = "actual_close_date",
                            title = CRMJSResource.ActualCloseDate,
                            isHeader = false
                        },
                    new
                        {
                            name = "deal_milestone",
                            title = CRMDealResource.CurrentDealMilestone,
                            isHeader = false
                        },
                    new
                        {
                            name = "probability_of_winning",
                            title = CRMDealResource.ProbabilityOfWinning + " %",
                            isHeader = false
                        }
                }.ToList();


            var fieldsDescription = Global.DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Opportunity);
            columnSelectorData.AddRange(fieldsDescription
                                            .ConvertAll(customField => new
                                                {
                                                    name = "customField_" + customField.ID,
                                                    title = customField.Label.HtmlEncode(),
                                                    isHeader = customField.FieldType == CustomFieldType.Heading
                                                }));
            columnSelectorData.AddRange(
                new[]
                    {
                        new
                            {
                                name = String.Empty,
                                title = CRMDealResource.DealParticipants,
                                isHeader = true
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 1),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 2),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 3),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 4),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 5),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 6),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 7),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 8),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 9),
                                isHeader = false
                            },
                        new
                            {
                                name = "member",
                                title = String.Format("{0} {1}", CRMDealResource.DealParticipant, 10),
                                isHeader = false
                            }
                    });

            columnSelectorData.AddRange(
                new[]
                    {
                        new
                            {
                                name = String.Empty,
                                title = CRMDealResource.DealTags,
                                isHeader = true
                            },
                        new
                            {
                                name = "tag",
                                title = CRMDealResource.DealTagList,
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 1),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 2),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 3),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 4),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 5),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 6),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 7),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 8),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 9),
                                isHeader = false
                            },
                        new
                            {
                                name = "tag",
                                title = String.Format("{0} {1}", CRMDealResource.DealTag, 10),
                                isHeader = false
                            }
                    }
                );

            var tagList = Global.DaoFactory.GetTagDao().GetAllTags(EntityType.Opportunity);

            yield return RegisterObject("tagList", tagList.ToList());
            yield return RegisterObject("columnSelectorData", columnSelectorData);
        }
    }

    #endregion
}