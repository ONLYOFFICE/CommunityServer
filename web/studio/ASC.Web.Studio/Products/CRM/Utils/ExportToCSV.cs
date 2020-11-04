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


using System.Globalization;
using ASC.Common.Caching;
using ASC.Common.Security.Authentication;
using ASC.Common.Threading.Progress;
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ASC.Common.Logging;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Web.CRM.Classes
{
    class ExportDataCache
    {
        public static readonly ICache Cache = AscCache.Default;

        public static String GetStateCacheKey(string key)
        {
            return String.Format("{0}:crm:queue:exporttocsv", key);
        }

        public static String GetCancelCacheKey(string key)
        {
            return String.Format("{0}:crm:queue:exporttocsv:cancel", key);
        }

        public static ExportDataOperation Get(string key)
        {
            return Cache.Get<ExportDataOperation>(GetStateCacheKey(key));
        }

        public static void Insert(string key, ExportDataOperation data)
        {
            Cache.Insert(GetStateCacheKey(key), data, TimeSpan.FromMinutes(1));
        }

        public static bool CheckCancelFlag(string key)
        {
            var fromCache = Cache.Get<String>(GetCancelCacheKey(key));

            return !String.IsNullOrEmpty(fromCache);
        }

        public static void SetCancelFlag(string key)
        {
            Cache.Insert(GetCancelCacheKey(key), "true", TimeSpan.FromMinutes(1));
        }

        public static void ResetAll(string key)
        {
            Cache.Remove(GetStateCacheKey(key));
            Cache.Remove(GetCancelCacheKey(key));
        }
    }

    class ExportDataOperation : IProgressItem
    {
        #region Constructor

        public ExportDataOperation(FilterObject filterObject, string fileName)
        {
            _tenantId = TenantProvider.CurrentTenantID;
            _author = SecurityContext.CurrentAccount;
            _dataStore = Global.GetStore();
            _notifyClient = NotifyClient.Instance;
            _filterObject = filterObject;
            _log = LogManager.GetLogger("ASC.CRM");

            Id = ExportToCsv.GetKey(filterObject != null);
            Status = ProgressStatus.Queued;
            Error = null;
            Percentage = 0;
            IsCompleted = false;
            FileName = fileName ?? string.Format("{0}_{1}.{2}", CRMSettingResource.Export, DateTime.UtcNow.Ticks, filterObject == null ? "zip" : "csv");
            FileUrl = null;
        }

        public ExportDataOperation()
            : this(null, null)
        {
        }

        #endregion

        #region Members

        private readonly int _tenantId;

        private readonly IAccount _author;

        private readonly IDataStore _dataStore;

        private readonly NotifyClient _notifyClient;

        private readonly FilterObject _filterObject;

        private readonly ILog _log;

        private int _totalCount;

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var exportDataOperation = obj as ExportDataOperation;

            if (exportDataOperation == null) return false;

            return Id == exportDataOperation.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public object Clone()
        {
            var cloneObj = new ExportDataOperation
                {
                    Id = Id,
                    Status = Status,
                    Error = Error,
                    Percentage = Percentage,
                    IsCompleted = IsCompleted,
                    FileName = FileName,
                    FileUrl = FileUrl
                };

            return cloneObj;
        }

        #region Property

        public object Id { get; set; }

        public object Status { get; set; }

        public object Error { get; set; }

        public double Percentage { get; set; }

        public bool IsCompleted { get; set; }

        public string FileName { get; set; }

        public string FileUrl { get; set; }

        #endregion

        #region Private Methods

        private static String WrapDoubleQuote(String value)
        {
            return "\"" + value.Trim().Replace("\"", "\"\"") + "\"";
        }

        private static String DataTableToCsv(DataTable dataTable)
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
                Status = ProgressStatus.Started;
                
                CoreContext.TenantManager.SetCurrentTenant(_tenantId);
                SecurityContext.AuthenticateMe(_author);

                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    var userCulture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();

                    System.Threading.Thread.CurrentThread.CurrentCulture = userCulture;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = userCulture;

                    _log.Debug("Start Export Data");

                    ExportDataCache.Insert((string) Id, (ExportDataOperation) Clone());

                    if (_filterObject == null)
                        ExportAllData(daoFactory);
                    else
                        ExportPartData(daoFactory);
                }

                Complete(100, ProgressStatus.Done, null);

                _log.Debug("Export is completed");
            }
            catch (OperationCanceledException ocex)
            {
                Complete(0, ProgressStatus.Failed, ocex.Message);

                _log.Debug("Export is cancel");
            }
            catch (Exception ex)
            {
                Complete(0, ProgressStatus.Failed, ex.Message);

                _log.Error(ex);
            }
            finally
            {
                ExportDataCache.ResetAll((string) Id);
            }
        }

        private void Complete(double percentage, ProgressStatus status, object error)
        {
            IsCompleted = true;
            Percentage = percentage;
            Status = status;
            Error = error;

            ExportDataCache.Insert((string) Id, (ExportDataOperation)Clone());
        }

        private void ExportAllData(DaoFactory daoFactory)
        {
            using (var stream = TempStream.Create())
            {
                var contactDao = daoFactory.ContactDao;
                var contactInfoDao = daoFactory.ContactInfoDao;
                var dealDao = daoFactory.DealDao;
                var casesDao = daoFactory.CasesDao;
                var taskDao = daoFactory.TaskDao;
                var historyDao = daoFactory.RelationshipEventDao;
                var invoiceItemDao = daoFactory.InvoiceItemDao;

                _totalCount += contactDao.GetAllContactsCount();
                _totalCount += dealDao.GetDealsCount();
                _totalCount += casesDao.GetCasesCount();
                _totalCount += taskDao.GetAllTasksCount();
                _totalCount += historyDao.GetAllItemsCount();
                _totalCount += invoiceItemDao.GetInvoiceItemsCount();

                using (var zipStream = new ZipOutputStream(stream, true))
                {
                    zipStream.AlternateEncoding = Encoding.UTF8;
                    zipStream.AlternateEncodingUsage = ZipOption.Always;

                    zipStream.PutNextEntry(CRMContactResource.Contacts + ".csv");
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

                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportContactsToCsv(contactData, contactInfos, daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry(CRMCommonResource.DealModuleName + ".csv");
                    var dealData = dealDao.GetAllDeals();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportDealsToCsv(dealData, daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry(CRMCommonResource.CasesModuleName + ".csv");
                    var casesData = casesDao.GetAllCases();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportCasesToCsv(casesData, daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry(CRMCommonResource.TaskModuleName + ".csv");
                    var taskData = taskDao.GetAllTasks();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportTasksToCsv(taskData, daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry(CRMCommonResource.History + ".csv");
                    var historyData = historyDao.GetAllItems();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportHistoryToCsv(historyData, daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.PutNextEntry(CRMCommonResource.ProductsAndServices + ".csv");
                    var invoiceItemData = invoiceItemDao.GetAll();
                    using (var zipEntryData = new MemoryStream(Encoding.UTF8.GetBytes(ExportInvoiceItemsToCsv(invoiceItemData, daoFactory))))
                    {
                        zipEntryData.StreamCopyTo(zipStream);
                    }

                    zipStream.Flush();
                    zipStream.Close();

                    stream.Position = 0;
                }

                if (_dataStore.IsDirectory("temp", "export"))
                {
                    _dataStore.DeleteFiles("temp", "export", "*.*", true);
                }

                FileUrl = CommonLinkUtility.GetFullAbsolutePath(_dataStore.SavePrivate("temp", Path.Combine("export", FileName), stream, DateTime.Now.AddDays(1)));

                _notifyClient.SendAboutExportCompleted(_author.ID, FileName, FileUrl);
            }
        }

        private void ExportPartData(DaoFactory daoFactory)
        {
            var items = _filterObject.GetItemsByFilter(daoFactory);

            string fileContent;

            _totalCount = items.Count;

            if (_totalCount == 0)
                throw new ArgumentException(CRMErrorsResource.ExportToCSVDataEmpty);

            if (items is List<Contact>)
            {
                var contactInfoDao = daoFactory.ContactInfoDao;

                var contacts = (List<Contact>) items;

                var contactInfos = new StringDictionary();

                contactInfoDao.GetAll(contacts.Select(item => item.ID).ToArray())
                              .ForEach(item =>
                                  {
                                      var contactInfoKey = String.Format("{0}_{1}_{2}", item.ContactID,
                                                                         (int) item.InfoType,
                                                                         item.Category);

                                      if (contactInfos.ContainsKey(contactInfoKey))
                                          contactInfos[contactInfoKey] += "," + item.Data;
                                      else
                                          contactInfos.Add(contactInfoKey, item.Data);
                                  });

                fileContent = ExportContactsToCsv(contacts, contactInfos, daoFactory);
            }
            else if (items is List<Deal>)
            {
                fileContent = ExportDealsToCsv((List<Deal>) items, daoFactory);
            }
            else if (items is List<ASC.CRM.Core.Entities.Cases>)
            {
                fileContent = ExportCasesToCsv((List<ASC.CRM.Core.Entities.Cases>) items, daoFactory);
            }
            else if (items is List<RelationshipEvent>)
            {
                fileContent = ExportHistoryToCsv((List<RelationshipEvent>) items, daoFactory);
            }
            else if (items is List<Task>)
            {
                fileContent = ExportTasksToCsv((List<Task>) items, daoFactory);
            }
            else if (items is List<InvoiceItem>)
            {
                fileContent = ExportInvoiceItemsToCsv((List<InvoiceItem>) items, daoFactory);
            }
            else
                throw new ArgumentException();

            FileUrl = SaveCsvFileInMyDocument(FileName, fileContent);
        }

        private String ExportContactsToCsv(IReadOnlyCollection<Contact> contacts, StringDictionary contactInfos, DaoFactory daoFactory)
        {
            var key = (string) Id;
            var listItemDao = daoFactory.ListItemDao;
            var tagDao = daoFactory.TagDao;
            var customFieldDao = daoFactory.CustomFieldDao;
            var contactDao = daoFactory.ContactDao;

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
                if (ExportDataCache.CheckCancelFlag(key))
                {
                    ExportDataCache.ResetAll(key);

                    throw new OperationCanceledException();                   
                }

                ExportDataCache.Insert(key, (ExportDataOperation) Clone());

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
                        var personCompany = contacts.SingleOrDefault(item => item.ID == people.CompanyID) ??
                                            contactDao.GetByID(people.CompanyID);

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

            return DataTableToCsv(dataTable);
        }

        private String ExportDealsToCsv(IEnumerable<Deal> deals, DaoFactory daoFactory)
        {
            var key = (string) Id;
            var tagDao = daoFactory.TagDao;
            var customFieldDao = daoFactory.CustomFieldDao;
            var dealMilestoneDao = daoFactory.DealMilestoneDao;
            var contactDao = daoFactory.ContactDao;

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
                if (ExportDataCache.CheckCancelFlag(key))
                {
                    ExportDataCache.ResetAll(key);

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert(key, (ExportDataOperation) Clone());

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

                var dataRow = dataTable.Rows.Add(new object[]
                    {
                        deal.Title,
                        contactTitle,
                        deal.Description,
                        deal.BidCurrency,
                        deal.BidValue.ToString(CultureInfo.InvariantCulture),
                        bidType,
                        deal.PerPeriodValue == 0 ? "" : deal.PerPeriodValue.ToString(CultureInfo.InvariantCulture),
                        deal.ExpectedCloseDate.Date == DateTime.MinValue.Date ? "" : deal.ExpectedCloseDate.ToString(DateTimeExtension.DateFormatPattern),
                        deal.ActualCloseDate.Date == DateTime.MinValue.Date ? "" : deal.ActualCloseDate.ToString(DateTimeExtension.DateFormatPattern),
                        CoreContext.UserManager.GetUsers(deal.ResponsibleID).DisplayUserName(false),
                        currentDealMilestone.Title,
                        currentDealMilestoneStatus,
                        deal.DealMilestoneProbability.ToString(CultureInfo.InvariantCulture),
                        contactTags
                    });

                if (customFieldEntity.ContainsKey(deal.ID))
                    customFieldEntity[deal.ID].ForEach(item => dataRow["customField_" + item.ID] = item.Value);
            }

            return DataTableToCsv(dataTable);
        }

        private String ExportCasesToCsv(IEnumerable<ASC.CRM.Core.Entities.Cases> cases, DaoFactory daoFactory)
        {
            var key = (string) Id;
            var tagDao = daoFactory.TagDao;
            var customFieldDao = daoFactory.CustomFieldDao;

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
                if (ExportDataCache.CheckCancelFlag(key))
                {
                    ExportDataCache.ResetAll(key);

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert(key, (ExportDataOperation) Clone());

                Percentage += 1.0 * 100 / _totalCount;

                var contactTags = String.Empty;

                if (tags.ContainsKey(item.ID))
                    contactTags = String.Join(",", tags[item.ID].OrderBy(x => x));

                var dataRow = dataTable.Rows.Add(new object[]
                    {
                        item.Title,
                        contactTags
                    });

                if (customFieldEntity.ContainsKey(item.ID))
                    customFieldEntity[item.ID].ForEach(row => dataRow["customField_" + row.ID] = row.Value);
            }

            return DataTableToCsv(dataTable);
        }

        private String ExportHistoryToCsv(IEnumerable<RelationshipEvent> events, DaoFactory daoFactory)
        {
            var key = (string) Id;
            var listItemDao = daoFactory.ListItemDao;
            var dealDao = daoFactory.DealDao;
            var casesDao = daoFactory.CasesDao;
            var contactDao = daoFactory.ContactDao;

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
                if (ExportDataCache.CheckCancelFlag(key))
                {
                    ExportDataCache.ResetAll(key);

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert(key, (ExportDataOperation) Clone());

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

                dataTable.Rows.Add(new object[]
                    {
                        item.Content,
                        categoryTitle,
                        contactTitle,
                        entityTitle,
                        CoreContext.UserManager.GetUsers(item.CreateBy).DisplayUserName(false),
                        item.CreateOn.ToShortString()
                    });
            }

            return DataTableToCsv(dataTable);
        }

        private String ExportTasksToCsv(IEnumerable<Task> tasks, DaoFactory daoFactory)
        {
            var key = (string) Id;
            var listItemDao = daoFactory.ListItemDao;
            var dealDao = daoFactory.DealDao;
            var casesDao = daoFactory.CasesDao;
            var contactDao = daoFactory.ContactDao;

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
                if (ExportDataCache.CheckCancelFlag(key))
                {
                    ExportDataCache.ResetAll(key);

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert(key, (ExportDataOperation) Clone());

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

                dataTable.Rows.Add(new object[]
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
                        item.AlertValue.ToString(CultureInfo.InvariantCulture)
                    });
            }

            return DataTableToCsv(dataTable);
        }

        private String ExportInvoiceItemsToCsv(IEnumerable<InvoiceItem> invoiceItems, DaoFactory daoFactory)
        {
            var key = (string) Id;
            var taxes = daoFactory.InvoiceTaxDao.GetAll();
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
                if (ExportDataCache.CheckCancelFlag(key))
                {
                    ExportDataCache.ResetAll(key);

                    throw new OperationCanceledException();
                }

                ExportDataCache.Insert(key, (ExportDataOperation) Clone());

                Percentage += 1.0 * 100 / _totalCount;

                var tax1 = item.InvoiceTax1ID != 0 ? taxes.Find(t => t.ID == item.InvoiceTax1ID) : null;
                var tax2 = item.InvoiceTax2ID != 0 ? taxes.Find(t => t.ID == item.InvoiceTax2ID) : null;

                dataTable.Rows.Add(new object[]
                    {
                        item.Title,
                        item.Description,
                        item.StockKeepingUnit,
                        item.Price.ToString(CultureInfo.InvariantCulture),
                        item.StockQuantity.ToString(CultureInfo.InvariantCulture),
                        item.TrackInventory.ToString(),
                        item.Currency,
                        tax1 != null ? tax1.Name : "",
                        tax1 != null ? tax1.Rate.ToString(CultureInfo.InvariantCulture) : "",
                        tax2 != null ? tax2.Name : "",
                        tax2 != null ? tax2.Rate.ToString(CultureInfo.InvariantCulture) : ""
                    });
            }

            return DataTableToCsv(dataTable);
        }

        private static String SaveCsvFileInMyDocument(String title, String data)
        {
            string fileUrl;

            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var file = FileUploader.Exec(Files.Classes.Global.FolderMy.ToString(), title, memStream.Length, memStream, true);

                if (FileUtility.CanWebView(title) || FileUtility.CanWebEdit(title))
                {
                    fileUrl = FilesLinkUtility.GetFileWebEditorUrl((int)file.ID);
                    fileUrl += string.Format("&options={{\"delimiter\":{0},\"codePage\":{1}}}",
                                     (int)FileUtility.CsvDelimiter.Comma,
                                     Encoding.UTF8.CodePage);
                }
                else
                {
                    fileUrl = FilesLinkUtility.GetFileDownloadUrl((int)file.ID);
                }
            }

            return fileUrl;
        }
    }

    public class ExportToCsv
    {
        #region Members

        private static readonly object Locker = new object();

        private static readonly ProgressQueue Queue = new ProgressQueue(1, TimeSpan.FromSeconds(60), true);

        #endregion

        #region Public Methods

        public static IProgressItem GetStatus(bool partialDataExport)
        {
            var key = GetKey(partialDataExport);

            return Queue.GetStatus(key) ?? ExportDataCache.Get(key);
        }

        public static IProgressItem Start(FilterObject filterObject, string fileName)
        {
            lock (Locker)
            {
                var key = GetKey(filterObject != null);

                var operation = Queue.GetStatus(key);

                if (operation == null )
                {
                    var fromCache = ExportDataCache.Get(key);

                    if (fromCache != null)
                        return fromCache;
                }

                if (operation == null)
                {
                    ExportDataCache.ResetAll(key);

                    operation = new ExportDataOperation(filterObject, fileName);

                    Queue.Add(operation);
                }

                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return operation;
            }
        }

        public static void Cancel(bool partialDataExport)
        {
            lock (Locker)
            {
                var key = GetKey(partialDataExport);

                var findedItem = Queue.GetItems().FirstOrDefault(elem => (string)elem.Id == key);

                if (findedItem != null)
                {
                    Queue.Remove(findedItem);
                }

                ExportDataCache.SetCancelFlag(key);
            }
        }

        public static string GetKey(bool partialDataExport)
        {
            return string.Format("{0}_{1}", TenantProvider.CurrentTenantID,
                                 partialDataExport ? SecurityContext.CurrentAccount.ID : Guid.Empty);
        }

        public static String ExportItems(FilterObject filterObject, string fileName)
        {
            var operation = new ExportDataOperation(filterObject, fileName);

            operation.RunJob();

            return operation.FileUrl;
        }

        #endregion
    }
}