/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Globalization;
using ASC.Common.Caching;
using ASC.Common.Security.Authentication;
using ASC.Common.Threading.Progress;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Data.Storage;
using ASC.Web.Core.Files;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Files.Utils;
using ASC.Web.Studio.Utility;
using Ionic.Zip;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Web.CRM.Classes
{
    class ExportDataCache
    {
        public static readonly ICache Cache = AscCache.Default;

        public static String GetStateCacheKey(int tenantId = -1)
        {
            if (tenantId == -1)
            {
                tenantId = TenantProvider.CurrentTenantID;
            }

            return String.Format("{0}:crm:queue:exporttocsv", tenantId.ToString(CultureInfo.InvariantCulture));
        }

        public static String GetCancelCacheKey(int tenantId = -1)
        {
            if (tenantId == -1)
            {
                tenantId = TenantProvider.CurrentTenantID;
            }

            return String.Format("{0}:crm:queue:exporttocsv:cancel", tenantId.ToString(CultureInfo.InvariantCulture));
        }

        public static ExportDataOperation Get()
        {
            return Cache.Get<ExportDataOperation>(GetStateCacheKey());
        }

        public static void Insert(ExportDataOperation data)
        {
            Cache.Insert(GetStateCacheKey(), data, TimeSpan.FromMinutes(1));
        }

        public static bool CheckCancelFlag()
        {
            var fromCache = Cache.Get<String>(GetCancelCacheKey());

            if (!String.IsNullOrEmpty(fromCache))
                return true;

            return false;

        }

        public static void SetCancelFlag()
        {
            Cache.Insert(GetCancelCacheKey(), "true", TimeSpan.FromMinutes(1));
        }

        public static void ResetAll(int tenantId = -1)
        {
            Cache.Remove(GetStateCacheKey(tenantId));
            Cache.Remove(GetCancelCacheKey(tenantId));
        }
    }

    class ExportDataOperation : IProgressItem
    {
        #region Constructor

        public ExportDataOperation(ICollection externalData)
        {
            _author = SecurityContext.CurrentAccount;
            _dataStore = Global.GetStore();
            _tenantID = TenantProvider.CurrentTenantID;
            _notifyClient = NotifyClient.Instance;
            _log = LogManager.GetLogger("ASC.CRM");
            Id = TenantProvider.CurrentTenantID;
            _externalData = externalData;
        }

        public ExportDataOperation()
            : this(null)
        {
        }

        #endregion

        #region Members

        private readonly ILog _log;

        private readonly IDataStore _dataStore;

        private readonly IAccount _author;

        private readonly int _tenantID;

        private readonly NotifyClient _notifyClient;

        private readonly ICollection _externalData;

        private double _totalCount;

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ExportDataOperation)) return false;
            if (_tenantID == ((ExportDataOperation)obj)._tenantID) return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _tenantID.GetHashCode();
        }

        public object Clone()
        {
            var cloneObj = new ExportDataOperation
                {
                    Error = Error,
                    Id = Id,
                    IsCompleted = IsCompleted,
                    Percentage = Percentage,
                    Status = Status
                };

            return cloneObj;
        }

        #region Property

        public object Id { get; set; }

        public object Status { get; set; }

        public object Error { get; set; }

        public double Percentage { get; set; }

        public bool IsCompleted { get; set; }

        #endregion

        #region Private Methods

        private static String WrapDoubleQuote(String value)
        {
            return "\"" + value.Trim().Replace("\"", "\"\"") + "\"";
        }

        private static String DataTableToCSV(DataTable dataTable)
        {
            var result = new StringBuilder();

            var columnsCount = dataTable.Columns.Count;

            for (var index = 0; index < columnsCount; index++)
            {
                if (index != columnsCount - 1)
                    result.Append(dataTable.Columns[index].Caption + ",");
                else
                    result.Append(dataTable.Columns[index].Caption);
            }

            result.Append(Environment.NewLine);

            foreach (DataRow row in dataTable.Rows)
            {
                for (var i = 0; i < columnsCount; i++)
                {
                    var itemValue = WrapDoubleQuote(row[i].ToString());

                    if (i != columnsCount - 1)
                        result.Append(itemValue + ",");
                    else
                        result.Append(itemValue);
                }

                result.Append(Environment.NewLine);
            }

            return result.ToString();
        }

        #endregion

        public void RunJob()
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(_tenantID);
                SecurityContext.AuthenticateMe(_author);

                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    var userCulture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();

                    System.Threading.Thread.CurrentThread.CurrentCulture = userCulture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = userCulture;

                    _log.Debug("Start Export Data");

                    ExportDataCache.Insert((ExportDataOperation) Clone());

                    if (_externalData == null)
                        ExportAllData(daoFactory);
                    else
                        ExportPartData(daoFactory);
                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("Export is cancel");
            }
            finally
            {
                System.Threading.Thread.Sleep(10000);
                ExportDataCache.ResetAll(_tenantID);
            }
        }

        private void Complete()
        {
            IsCompleted = true;

            Percentage = 100;

            _log.Debug("Export is completed");

            ExportDataCache.Insert((ExportDataOperation)Clone());
        }

        private void ExportAllData(DaoFactory _daoFactory)
        {
            using (var stream = TempStream.Create())
            {
                var contactDao = _daoFactory.ContactDao;
                var contactInfoDao = _daoFactory.ContactInfoDao;
                var dealDao = _daoFactory.DealDao;
                var casesDao = _daoFactory.CasesDao;
                var taskDao = _daoFactory.TaskDao;
                var historyDao = _daoFactory.RelationshipEventDao;
                var invoiceItemDao = _daoFactory.InvoiceItemDao;

                _totalCount += contactDao.GetAllContactsCount();
                _totalCount += dealDao.GetDealsCount();
                _totalCount += casesDao.GetCasesCount();
                _totalCount += taskDao.GetAllTasksCount();
                _totalCount += historyDao.GetAllItemsCount();
                _totalCount += invoiceItemDao.GetInvoiceItemsCount();

                using (var zipStream = new ZipOutputStream(stream, true))
                {
                    zipStream.PutNextEntry("contacts.csv");
                    var contactData = contactDao.GetAllContacts();
                    var contactInfos = new StringDictionary();
                    contactInfoDao.GetAll()
                                  .ForEach(item =>
                                               {
                                                   var contactInfoKey = String.Format("{0}_{1}_{2}", item.ContactID, (int)item.InfoType, item.Category);
                                                   if (contactInfos.ContainsKey(contactInfoKey))
                                                   {
                                                       contactInfos[contactInfoKey] += "," + item.Data;
                                                   }
                                                   else
                                                   {
                                                       contactInfos.Add(contactInfoKey, item.Data);
                                                   }
                                               });

                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportContactsToCSV(contactData, contactInfos, _daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry("oppotunities.csv");
                    var dealData = dealDao.GetAllDeals();

                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportDealsToCSV(dealData, _daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry("cases.csv");
                    var casesData = casesDao.GetAllCases();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportCasesToCSV(casesData, _daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry("tasks.csv");
                    var taskData = taskDao.GetAllTasks();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportTasksToCSV(taskData, _daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry("history.csv");
                    var historyData = historyDao.GetAllItems();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportHistoryToCSV(historyData, _daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry("products_services.csv");
                    var invoiceItemData = invoiceItemDao.GetAll();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportInvoiceItemsToCSV(invoiceItemData, _daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.Flush();
                    zipStream.Close();

                    stream.Position = 0;
                }

                var assignedURI = CommonLinkUtility.GetFullAbsolutePath(_dataStore.SavePrivate(String.Empty, "exportdata.zip", stream, DateTime.Now.AddDays(1)));
                Status = assignedURI;
                _notifyClient.SendAboutExportCompleted(_author.ID, assignedURI);
                Complete();
            }
        }

        private void ExportPartData(DaoFactory _daoFactory)
        {
            _totalCount = _externalData.Count;

            if (_totalCount == 0)
                throw new ArgumentException(CRMErrorsResource.ExportToCSVDataEmpty);

            if (_externalData is List<Contact>)
            {
                var contactInfoDao = _daoFactory.ContactInfoDao;

                var contacts = (List<Contact>)_externalData;

                var contactInfos = new StringDictionary();

                contactInfoDao.GetAll(contacts.Select(item => item.ID).ToArray())
                              .ForEach(item =>
                                           {
                                               var contactInfoKey = String.Format("{0}_{1}_{2}", item.ContactID,
                                                                                  (int)item.InfoType,
                                                                                  item.Category);

                                               if (contactInfos.ContainsKey(contactInfoKey))
                                                   contactInfos[contactInfoKey] += "," + item.Data;
                                               else
                                                   contactInfos.Add(contactInfoKey, item.Data);
                                           });

                Status = ExportContactsToCSV(contacts, contactInfos, _daoFactory);
            }
            else if (_externalData is List<Deal>)
                Status = ExportDealsToCSV((List<Deal>)_externalData, _daoFactory);
            else if (_externalData is List<ASC.CRM.Core.Entities.Cases>)
                Status = ExportCasesToCSV((List<ASC.CRM.Core.Entities.Cases>)_externalData, _daoFactory);
            else if (_externalData is List<RelationshipEvent>)
                Status = ExportHistoryToCSV((List<RelationshipEvent>)_externalData, _daoFactory);
            else if (_externalData is List<Task>)
                Status = ExportTasksToCSV((List<Task>)_externalData, _daoFactory);
            else if (_externalData is List<InvoiceItem>)
                Status = ExportInvoiceItemsToCSV((List<InvoiceItem>)_externalData, _daoFactory);
            else
                throw new ArgumentException();

            Complete();
        }

        private String ExportContactsToCSV(IEnumerable<Contact> contacts, StringDictionary contactInfos, DaoFactory _daoFactory)
        {
            var listItemDao = _daoFactory.ListItemDao;
            var tagDao = _daoFactory.TagDao;
            var customFieldDao = _daoFactory.CustomFieldDao;
            var contactDao = _daoFactory.ContactDao;

            var dataTable = new DataTable();

            dataTable.Columns.AddRange(new[]
                {
                    new DataColumn
                        {
                            Caption = CRMCommonResource.TypeCompanyOrPerson,
                            ColumnName = "company/person"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.FirstName,
                            ColumnName = "firstname"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.LastName,
                            ColumnName = "lastname"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.CompanyName,
                            ColumnName = "companyname"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.JobTitle,
                            ColumnName = "jobtitle"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.About,
                            ColumnName = "about"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.ContactStage,
                            ColumnName = "contact_stage"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.ContactType,
                            ColumnName = "contact_type"
                        },
                    new DataColumn
                        {
                            Caption = CRMContactResource.ContactTagList,
                            ColumnName = "contact_tag_list"
                        }
                });

            foreach (ContactInfoType infoTypeEnum in Enum.GetValues(typeof(ContactInfoType)))
                foreach (Enum categoryEnum in Enum.GetValues(ContactInfo.GetCategory(infoTypeEnum)))
                {
                    var localTitle = String.Format("{1} ({0})", categoryEnum.ToLocalizedString().ToLower(), infoTypeEnum.ToLocalizedString());

                    if (infoTypeEnum == ContactInfoType.Address)
                        dataTable.Columns.AddRange((from AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart))
                                                    select new DataColumn
                                                        {
                                                            Caption = String.Format(localTitle + " {0}", addressPartEnum.ToLocalizedString().ToLower()),
                                                            ColumnName = String.Format("contactInfo_{0}_{1}_{2}", (int)infoTypeEnum, categoryEnum, (int)addressPartEnum)
                                                        }).ToArray());

                    else
                        dataTable.Columns.Add(new DataColumn
                            {
                                Caption = localTitle,
                                ColumnName = String.Format("contactInfo_{0}_{1}", (int)infoTypeEnum, categoryEnum)
                            });
                }

            var fieldsDescription = customFieldDao.GetFieldsDescription(EntityType.Company);

            customFieldDao.GetFieldsDescription(EntityType.Person).ForEach(item =>
                                                                               {
                                                                                   var alreadyContains = fieldsDescription.Any(field => field.ID == item.ID);

                                                                                   if (!alreadyContains)
                                                                                       fieldsDescription.Add(item);
                                                                               });

            fieldsDescription.ForEach(
                item =>
                {
                    if (item.FieldType == CustomFieldType.Heading) return;

                    dataTable.Columns.Add(
                        new DataColumn
                            {
                                Caption = item.Label,
                                ColumnName = "customField_" + item.ID
                            }
                        );
                });

            var customFieldEntity = new Dictionary<int, List<CustomField>>();

            var entityFields = customFieldDao.GetEnityFields(EntityType.Company, 0, false);

            customFieldDao.GetEnityFields(EntityType.Person, 0, false).ForEach(item =>
                                                                                   {
                                                                                       var alreadyContains = entityFields.Any(field => field.ID == item.ID && field.EntityID == item.EntityID);

                                                                                       if (!alreadyContains)
                                                                                           entityFields.Add(item);
                                                                                   });

            entityFields.ForEach(
                item =>
                {
                    if (!customFieldEntity.ContainsKey(item.EntityID))
                        customFieldEntity.Add(item.EntityID, new List<CustomField> { item });
                    else
                        customFieldEntity[item.EntityID].Add(item);
                });

            var tags = tagDao.GetEntitiesTags(EntityType.Contact);

            foreach (var contact in contacts)
            {
                if (ExportDataCache.CheckCancelFlag())
                {
                    ExportDataCache.ResetAll();

                    throw new OperationCanceledException();                   
                }

                ExportDataCache.Insert((ExportDataOperation)Clone());

                Percentage += 1.0 * 100 / _totalCount;
                
                var isCompany = contact is Company;

                var compPersType = (isCompany) ? CRMContactResource.Company : CRMContactResource.Person;

                var contactTags = String.Empty;

                if (tags.ContainsKey(contact.ID))
                    contactTags = String.Join(",", tags[contact.ID].OrderBy(x => x));

                String firstName;
                String lastName;

                String companyName;
                String title;

                if (contact is Company)
                {
                    firstName = String.Empty;
                    lastName = String.Empty;
                    title = String.Empty;
                    companyName = ((Company)contact).CompanyName;
                }
                else
                {
                    var people = (Person)contact;

                    firstName = people.FirstName;
                    lastName = people.LastName;
                    title = people.JobTitle;

                    companyName = String.Empty;

                    if (people.CompanyID > 0)
                    {
                        var personCompany = contacts.SingleOrDefault(item => item.ID == people.CompanyID);

                        if (personCompany == null)
                            personCompany = contactDao.GetByID(people.CompanyID);

                        if (personCompany != null)
                            companyName = personCompany.GetTitle();
                    }
                }

                var contactStatus = String.Empty;

                if (contact.StatusID > 0)
                {

                    var listItem = listItemDao.GetByID(contact.StatusID);

                    if (listItem != null)
                        contactStatus = listItem.Title;
                }

                var contactType = String.Empty;

                if (contact.ContactTypeID > 0)
                {

                    var listItem = listItemDao.GetByID(contact.ContactTypeID);

                    if (listItem != null)
                        contactType = listItem.Title;
                }

                var dataRowItems = new List<String>
                    {
                        compPersType,
                        firstName,
                        lastName,
                        companyName,
                        title,
                        contact.About,
                        contactStatus,
                        contactType,
                        contactTags
                    };

                foreach (ContactInfoType infoTypeEnum in Enum.GetValues(typeof(ContactInfoType)))
                    foreach (Enum categoryEnum in Enum.GetValues(ContactInfo.GetCategory(infoTypeEnum)))
                    {
                        var contactInfoKey = String.Format("{0}_{1}_{2}", contact.ID,
                                                           (int)infoTypeEnum,
                                                           Convert.ToInt32(categoryEnum));

                        var columnValue = "";

                        if (contactInfos.ContainsKey(contactInfoKey))
                            columnValue = contactInfos[contactInfoKey];

                        if (infoTypeEnum == ContactInfoType.Address)
                        {
                            if (!String.IsNullOrEmpty(columnValue))
                            {
                                var addresses = JArray.Parse(String.Concat("[", columnValue, "]"));

                                dataRowItems.AddRange((from AddressPart addressPartEnum in Enum.GetValues(typeof(AddressPart))
                                                       select String.Join(",", addresses.Select(item => (String)item.SelectToken(addressPartEnum.ToString().ToLower())).ToArray())).ToArray());
                            }
                            else
                            {
                                dataRowItems.AddRange(new[] { "", "", "", "", "" });
                            }
                        }
                        else
                        {
                            dataRowItems.Add(columnValue);
                        }
                    }

                var dataRow = dataTable.Rows.Add(dataRowItems.ToArray());

                if (customFieldEntity.ContainsKey(contact.ID))
                    customFieldEntity[contact.ID].ForEach(item => dataRow["customField_" + item.ID] = item.Value);
            }

            return DataTableToCSV(dataTable);
        }

        private String ExportDealsToCSV(IEnumerable<Deal> deals, DaoFactory _daoFactory)
        {
            var tagDao = _daoFactory.TagDao;
            var customFieldDao = _daoFactory.CustomFieldDao;
            var dealMilestoneDao = _daoFactory.DealMilestoneDao;
            var contactDao = _daoFactory.ContactDao;

            var dataTable = new DataTable();

            dataTable.Columns.AddRange(new[]
                {
                    new DataColumn
                        {
                            Caption = CRMDealResource.NameDeal,
                            ColumnName = "title"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.ClientDeal,
                            ColumnName = "client_deal"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.DescriptionDeal,
                            ColumnName = "description"
                        },
                    new DataColumn
                        {
                            Caption = CRMCommonResource.Currency,
                            ColumnName = "currency"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.DealAmount,
                            ColumnName = "amount"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.BidType,
                            ColumnName = "bid_type"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.BidTypePeriod,
                            ColumnName = "bid_type_period"
                        },
                    new DataColumn
                        {
                            Caption = CRMJSResource.ExpectedCloseDate,
                            ColumnName = "expected_close_date"
                        },
                    new DataColumn
                        {
                            Caption = CRMJSResource.ActualCloseDate,
                            ColumnName = "actual_close_date"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.ResponsibleDeal,
                            ColumnName = "responsible_deal"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.CurrentDealMilestone,
                            ColumnName = "current_deal_milestone"
                        },
                    new DataColumn
                        {
                            Caption = CRMDealResource.DealMilestoneType,
                            ColumnName = "deal_milestone_type"
                        },
                    new DataColumn
                        {
                            Caption = (CRMDealResource.ProbabilityOfWinning + " %"),
                            ColumnName = "probability_of_winning"
                        },
                    new DataColumn
                        {
                            Caption = (CRMDealResource.DealTagList),
                            ColumnName = "tag_list"
                        }
                });

            customFieldDao.GetFieldsDescription(EntityType.Opportunity).ForEach(
                item =>
                {
                    if (item.FieldType == CustomFieldType.Heading) return;

                    dataTable.Columns.Add(new DataColumn
                        {
                            Caption = item.Label,
                            ColumnName = "customField_" + item.ID
                        });
                });

            var customFieldEntity = new Dictionary<int, List<CustomField>>();

            customFieldDao.GetEnityFields(EntityType.Opportunity, 0, false).ForEach(
                item =>
                {
                    if (!customFieldEntity.ContainsKey(item.EntityID))
                        customFieldEntity.Add(item.EntityID, new List<CustomField> { item });
                    else
                        customFieldEntity[item.EntityID].Add(item);
                });

            var tags = tagDao.GetEntitiesTags(EntityType.Opportunity);

            foreach (var deal in deals)
            {
                if (ExportDataCache.CheckCancelFlag())
                {
                    ExportDataCache.ResetAll();

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert((ExportDataOperation)Clone());

                Percentage += 1.0 * 100 / _totalCount;

                var contactTags = String.Empty;

                if (tags.ContainsKey(deal.ID))
                    contactTags = String.Join(",", tags[deal.ID].OrderBy(x => x));

                String bidType;

                switch (deal.BidType)
                {
                    case BidType.FixedBid:
                        bidType = CRMDealResource.BidType_FixedBid;
                        break;
                    case BidType.PerDay:
                        bidType = CRMDealResource.BidType_PerDay;
                        break;
                    case BidType.PerHour:
                        bidType = CRMDealResource.BidType_PerHour;
                        break;
                    case BidType.PerMonth:
                        bidType = CRMDealResource.BidType_PerMonth;
                        break;
                    case BidType.PerWeek:
                        bidType = CRMDealResource.BidType_PerWeek;
                        break;
                    case BidType.PerYear:
                        bidType = CRMDealResource.BidType_PerYear;
                        break;
                    default:
                        throw new ArgumentException();
                }

                var currentDealMilestone = dealMilestoneDao.GetByID(deal.DealMilestoneID);
                var currentDealMilestoneStatus = currentDealMilestone.Status.ToLocalizedString();
                var contactTitle = String.Empty;

                if (deal.ContactID != 0)
                    contactTitle = contactDao.GetByID(deal.ContactID).GetTitle();

                var dataRow = dataTable.Rows.Add(new[]
                    {
                        deal.Title,
                        contactTitle,
                        deal.Description,
                        deal.BidCurrency,
                        deal.BidValue.ToString(),
                        bidType,
                        deal.PerPeriodValue == 0 ? "" : deal.PerPeriodValue.ToString(),
                        deal.ExpectedCloseDate.Date == DateTime.MinValue.Date ? "" : deal.ExpectedCloseDate.ToString(DateTimeExtension.DateFormatPattern),
                        deal.ActualCloseDate.Date == DateTime.MinValue.Date ? "" : deal.ActualCloseDate.ToString(DateTimeExtension.DateFormatPattern),
                        CoreContext.UserManager.GetUsers(deal.ResponsibleID).DisplayUserName(false),
                        currentDealMilestone.Title,
                        currentDealMilestoneStatus,
                        deal.DealMilestoneProbability.ToString(),
                        contactTags
                    });

                if (customFieldEntity.ContainsKey(deal.ID))
                    customFieldEntity[deal.ID].ForEach(item => dataRow["customField_" + item.ID] = item.Value);
            }

            return DataTableToCSV(dataTable);
        }

        private String ExportCasesToCSV(IEnumerable<ASC.CRM.Core.Entities.Cases> cases, DaoFactory _daoFactory)
        {
            var tagDao = _daoFactory.TagDao;
            var customFieldDao = _daoFactory.CustomFieldDao;

            var dataTable = new DataTable();

            dataTable.Columns.AddRange(new[]
                {
                    new DataColumn
                        {
                            Caption = CRMCasesResource.CaseTitle,
                            ColumnName = "title"
                        },
                    new DataColumn(CRMCasesResource.CasesTagList)
                        {
                            Caption = CRMCasesResource.CasesTagList,
                            ColumnName = "tag_list"
                        }
                });

            customFieldDao.GetFieldsDescription(EntityType.Case).ForEach(
                item =>
                {
                    if (item.FieldType == CustomFieldType.Heading) return;

                    dataTable.Columns.Add(new DataColumn
                        {
                            Caption = item.Label,
                            ColumnName = "customField_" + item.ID
                        });
                });

            var customFieldEntity = new Dictionary<int, List<CustomField>>();

            customFieldDao.GetEnityFields(EntityType.Case, 0, false).ForEach(
                item =>
                {
                    if (!customFieldEntity.ContainsKey(item.EntityID))
                        customFieldEntity.Add(item.EntityID, new List<CustomField> { item });
                    else
                        customFieldEntity[item.EntityID].Add(item);
                });

            var tags = tagDao.GetEntitiesTags(EntityType.Case);

            foreach (var item in cases)
            {
                if (ExportDataCache.CheckCancelFlag())
                {
                    ExportDataCache.ResetAll();

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert((ExportDataOperation)Clone());

                Percentage += 1.0 * 100 / _totalCount;

                var contactTags = String.Empty;

                if (tags.ContainsKey(item.ID))
                    contactTags = String.Join(",", tags[item.ID].OrderBy(x => x));

                var dataRow = dataTable.Rows.Add(new[]
                    {
                        item.Title,
                        contactTags
                    });

                if (customFieldEntity.ContainsKey(item.ID))
                    customFieldEntity[item.ID].ForEach(row => dataRow["customField_" + row.ID] = row.Value);
            }

            return DataTableToCSV(dataTable);
        }

        private String ExportHistoryToCSV(IEnumerable<RelationshipEvent> events, DaoFactory _daoFactory)
        {
            var listItemDao = _daoFactory.ListItemDao;
            var dealDao = _daoFactory.DealDao;
            var casesDao = _daoFactory.CasesDao;
            var contactDao = _daoFactory.ContactDao;

            var dataTable = new DataTable();

            dataTable.Columns.AddRange(new[]
                {
                    new DataColumn
                        {
                            Caption = (CRMContactResource.Content),
                            ColumnName = "content"
                        },
                    new DataColumn
                        {
                            Caption = (CRMCommonResource.Category),
                            ColumnName = "category"
                        },
                    new DataColumn
                        {
                            Caption = (CRMContactResource.ContactTitle),
                            ColumnName = "contact_title"
                        },
                    new DataColumn
                        {
                            Caption = (CRMContactResource.RelativeEntity),
                            ColumnName = "relative_entity"
                        },
                    new DataColumn
                        {
                            Caption = (CRMCommonResource.Author),
                            ColumnName = "author"
                        },
                    new DataColumn
                        {
                            Caption = (CRMCommonResource.CreateDate),
                            ColumnName = "create_date"
                        }
                });

            foreach (var item in events)
            {
                if (ExportDataCache.CheckCancelFlag())
                {
                    ExportDataCache.ResetAll();

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert((ExportDataOperation)Clone());

                Percentage += 1.0 * 100 / _totalCount;

                var entityTitle = String.Empty;

                if (item.EntityID > 0)
                    switch (item.EntityType)
                    {
                        case EntityType.Case:
                            var casesObj = casesDao.GetByID(item.EntityID);

                            if (casesObj != null)
                                entityTitle = String.Format("{0}: {1}", CRMCasesResource.Case,
                                                            casesObj.Title);
                            break;
                        case EntityType.Opportunity:
                            var dealObj = dealDao.GetByID(item.EntityID);

                            if (dealObj != null)
                                entityTitle = String.Format("{0}: {1}", CRMDealResource.Deal,
                                                            dealObj.Title);
                            break;
                    }

                var contactTitle = String.Empty;

                if (item.ContactID > 0)
                {
                    var contactObj = contactDao.GetByID(item.ContactID);

                    if (contactObj != null)
                        contactTitle = contactObj.GetTitle();
                }

                var categoryTitle = String.Empty;

                if (item.CategoryID > 0)
                {
                    var categoryObj = listItemDao.GetByID(item.CategoryID);

                    if (categoryObj != null)
                        categoryTitle = categoryObj.Title;

                }
                else if (item.CategoryID == (int)HistoryCategorySystem.TaskClosed)
                    categoryTitle = HistoryCategorySystem.TaskClosed.ToLocalizedString();
                else if (item.CategoryID == (int)HistoryCategorySystem.FilesUpload)
                    categoryTitle = HistoryCategorySystem.FilesUpload.ToLocalizedString();
                else if (item.CategoryID == (int)HistoryCategorySystem.MailMessage)
                    categoryTitle = HistoryCategorySystem.MailMessage.ToLocalizedString();

                dataTable.Rows.Add(new[]
                    {
                        item.Content,
                        categoryTitle,
                        contactTitle,
                        entityTitle,
                        CoreContext.UserManager.GetUsers(item.CreateBy).DisplayUserName(false),
                        item.CreateOn.ToShortString()
                    });
            }

            return DataTableToCSV(dataTable);
        }

        private String ExportTasksToCSV(IEnumerable<Task> tasks, DaoFactory _daoFactory)
        {
            var listItemDao = _daoFactory.ListItemDao;
            var dealDao = _daoFactory.DealDao;
            var casesDao = _daoFactory.CasesDao;
            var contactDao = _daoFactory.ContactDao;

            var dataTable = new DataTable();

            dataTable.Columns.AddRange(new[]
                {
                    new DataColumn
                        {
                            Caption = (CRMTaskResource.TaskTitle),
                            ColumnName = "title"
                        },
                    new DataColumn
                        {
                            Caption = (CRMTaskResource.Description),
                            ColumnName = "description"
                        },
                    new DataColumn
                        {
                            Caption = (CRMTaskResource.DueDate),
                            ColumnName = "due_date"
                        },
                    new DataColumn
                        {
                            Caption = (CRMTaskResource.Responsible),
                            ColumnName = "responsible"
                        },
                    new DataColumn
                        {
                            Caption = (CRMContactResource.ContactTitle),
                            ColumnName = "contact_title"
                        },
                    new DataColumn
                        {
                            Caption = (CRMTaskResource.TaskStatus),
                            ColumnName = "task_status"
                        },
                    new DataColumn
                        {
                            Caption = (CRMTaskResource.TaskCategory),
                            ColumnName = "task_category"
                        },
                    new DataColumn
                        {
                            Caption = (CRMContactResource.RelativeEntity),
                            ColumnName = "relative_entity"
                        },
                    new DataColumn
                        {
                            Caption = (CRMCommonResource.Alert),
                            ColumnName = "alert_value"
                        }
                });

            foreach (var item in tasks)
            {
                if (ExportDataCache.CheckCancelFlag())
                {
                    ExportDataCache.ResetAll();

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert((ExportDataOperation)Clone());

                Percentage += 1.0 * 100 / _totalCount;

                var entityTitle = String.Empty;

                if (item.EntityID > 0)
                    switch (item.EntityType)
                    {
                        case EntityType.Case:
                            var caseObj = casesDao.GetByID(item.EntityID);

                            if (caseObj != null)
                                entityTitle = String.Format("{0}: {1}", CRMCasesResource.Case, caseObj.Title);
                            break;
                        case EntityType.Opportunity:
                            var dealObj = dealDao.GetByID(item.EntityID);

                            if (dealObj != null)
                                entityTitle = String.Format("{0}: {1}", CRMDealResource.Deal, dealObj.Title);
                            break;
                    }

                var contactTitle = String.Empty;

                if (item.ContactID > 0)
                {
                    var contact = contactDao.GetByID(item.ContactID);

                    if (contact != null)
                        contactTitle = contact.GetTitle();
                }

                dataTable.Rows.Add(new[]
                    {
                        item.Title,
                        item.Description,
                        item.DeadLine == DateTime.MinValue
                            ? ""
                            : item.DeadLine.ToShortString(),
                        CoreContext.UserManager.GetUsers(item.ResponsibleID).DisplayUserName(false),
                        contactTitle,
                        item.IsClosed
                            ? CRMTaskResource.TaskStatus_Closed
                            : CRMTaskResource.TaskStatus_Open,
                        listItemDao.GetByID(item.CategoryID).Title,
                        entityTitle,
                        item.AlertValue.ToString()
                    });
            }

            return DataTableToCSV(dataTable);
        }

        private String ExportInvoiceItemsToCSV(IEnumerable<InvoiceItem> invoiceItems, DaoFactory _daoFactory)
        {
            var taxes = _daoFactory.InvoiceTaxDao.GetAll();
            var dataTable = new DataTable();

            dataTable.Columns.AddRange(new[]
                {
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.InvoiceItemName),
                            ColumnName = "title"
                        },
                    new DataColumn
                        {
                            Caption = (CRMSettingResource.Description),
                            ColumnName = "description"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.StockKeepingUnit),
                            ColumnName = "sku"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.InvoiceItemPrice),
                            ColumnName = "price"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.FormInvoiceItemQuantity),
                            ColumnName = "quantity"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.FormInvoiceItemStockQuantity),
                            ColumnName = "stock_quantity"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.TrackInventory),
                            ColumnName = "track_inventory"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.Currency),
                            ColumnName = "currency"
                        },

                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.InvoiceTax1Name),
                            ColumnName = "tax1_name"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.InvoiceTax1Rate),
                            ColumnName = "tax1_rate"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.InvoiceTax2Name),
                            ColumnName = "tax2_name"
                        },
                    new DataColumn
                        {
                            Caption = (CRMInvoiceResource.InvoiceTax2Rate),
                            ColumnName = "tax2_rate"
                        }

                });


            foreach (var item in invoiceItems)
            {
                if (ExportDataCache.CheckCancelFlag())
                {
                    ExportDataCache.ResetAll();

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert((ExportDataOperation)Clone());

                Percentage += 1.0 * 100 / _totalCount;

                var tax1 = item.InvoiceTax1ID != 0 ? taxes.Find(t => t.ID == item.InvoiceTax1ID) : null;
                var tax2 = item.InvoiceTax2ID != 0 ? taxes.Find(t => t.ID == item.InvoiceTax2ID) : null;

                dataTable.Rows.Add(new[]
                    {
                        item.Title,
                        item.Description,
                        item.StockKeepingUnit,
                        item.Price.ToString(),
                        item.Quantity.ToString(),
                        item.StockQuantity.ToString(),
                        item.TrackInventory.ToString(),
                        item.Currency,
                        tax1 != null ? tax1.Name : "",
                        tax1 != null ? tax1.Rate.ToString() : "",
                        tax2 != null ? tax2.Name : "",
                        tax2 != null ? tax2.Rate.ToString() : ""
                    });
            }

            return DataTableToCSV(dataTable);
        }
    }

    public class ExportToCSV
    {
        #region Members

        private static readonly Object _syncObj = new Object();

        private static readonly ProgressQueue _exportQueue = new ProgressQueue(1, TimeSpan.FromSeconds(60), true);

        #endregion

        #region Public Methods

        public static IProgressItem GetStatus()
        {
            var result = _exportQueue.GetStatus(TenantProvider.CurrentTenantID);

            if (result == null)
                result = ExportDataCache.Get();

            return result;
        }

        public static IProgressItem Start()
        {
            lock (_syncObj)
            {
                var operation = _exportQueue.GetStatus(TenantProvider.CurrentTenantID);

                if (operation == null )
                {
                    var fromCache = ExportDataCache.Get();

                    if (fromCache != null)
                        return fromCache;
                }

                if (operation == null)
                {
                    operation = new ExportDataOperation();

                    _exportQueue.Add(operation);
                }

                if (!_exportQueue.IsStarted)
                    _exportQueue.Start(x => x.RunJob());

                return operation;
            }
        }

        private static String ExportEntityData(ICollection externalData, bool recieveURL, String fileName)
        {
            var operation = new ExportDataOperation(externalData);

            operation.RunJob();

            var data = (String)operation.Status;

            if (recieveURL) return SaveCSVFileInMyDocument(fileName, data);

            return data;
        }

        private static String SaveCSVFileInMyDocument(String title, String data)
        {
            string fileURL;

            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var file = FileUploader.Exec(Files.Classes.Global.FolderMy.ToString(), title, data.Length, memStream, true);

                fileURL = FilesLinkUtility.GetFileWebEditorUrl((int)file.ID);
            }
            fileURL += string.Format("&options={{\"delimiter\":{0},\"codePage\":{1}}}",
                                     (int)FileUtility.CsvDelimiter.Comma,
                                     Encoding.UTF8.CodePage);

            return fileURL;
        }

        public static String ExportContactsToCSV(List<Contact> contacts, bool recieveURL)
        {
            return ExportEntityData(contacts, recieveURL, "contacts.csv");
        }

        public static String ExportDealsToCSV(List<Deal> deals, bool recieveURL)
        {
            return ExportEntityData(deals, recieveURL, "opportunity.csv");
        }

        public static String ExportHistoryToCSV(List<RelationshipEvent> events, bool recieveURL)
        {
            return ExportEntityData(events, recieveURL, "history.csv");
        }

        public static String ExportCasesToCSV(List<ASC.CRM.Core.Entities.Cases> cases, bool recieveURL)
        {
            return ExportEntityData(cases, recieveURL, "cases.csv");
        }

        public static String ExportTasksToCSV(List<Task> tasks, bool recieveURL)
        {
            return ExportEntityData(tasks, recieveURL, "tasks.csv");
        }

        public static String ExportInvoiceItemsToCSV(List<InvoiceItem> invoiceItems, bool recieveURL)
        {
            return ExportEntityData(invoiceItems, recieveURL, "products_services.csv");
        }

        public static void Cancel()
        {
            lock (_syncObj)
            {
                var findedItem = _exportQueue.GetItems().Where(elem => (int)elem.Id == TenantProvider.CurrentTenantID);

                if (findedItem.Any())
                {
                    _exportQueue.Remove(findedItem.ElementAt(0));

                    ExportDataCache.ResetAll();

                }
                else
                {
                    ExportDataCache.SetCancelFlag();
                }
            }
        }

        #endregion


    }
}