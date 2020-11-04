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
using System.Globalization;

using ASC.Common.Caching;
using ASC.Common.Security.Authentication;
using ASC.Common.Threading.Progress;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Data.Storage;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Studio.Utility;
using ASC.Common.Logging;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Web.CRM.Classes
{
    class ImportDataCache
    {
        public static readonly ICache Cache = AscCache.Default;

        public static String GetStateCacheKey(EntityType entityType, int tenantId = -1)
        {
            if (tenantId == -1)
            {
                tenantId = TenantProvider.CurrentTenantID;
            }

            return String.Format("{0}:crm:queue:importtocsv:{1}", tenantId.ToString(CultureInfo.InvariantCulture), entityType.ToString());
        }

        public static String GetCancelCacheKey(EntityType entityType, int tenantId = -1)
        {
            if (tenantId == -1)
            {
                tenantId = TenantProvider.CurrentTenantID;
            }

            return String.Format("{0}:crm:queue:importtocsv:{1}:cancel", tenantId.ToString(CultureInfo.InvariantCulture), entityType.ToString());
        }

        public static ImportDataOperation Get(EntityType entityType)
        {
            return Cache.Get<ImportDataOperation>(GetStateCacheKey(entityType));
        }

        public static void Insert(EntityType entityType,ImportDataOperation data)
        {
            Cache.Insert(GetStateCacheKey(entityType), data, TimeSpan.FromMinutes(1));
        }

        public static bool CheckCancelFlag(EntityType entityType)
        {
            var fromCache = Cache.Get<String>(GetCancelCacheKey(entityType));

            if (!String.IsNullOrEmpty(fromCache))
                return true;

            return false;

        }

        public static void SetCancelFlag(EntityType entityType)
        {
            Cache.Insert(GetCancelCacheKey(entityType), true, TimeSpan.FromMinutes(1));
        }

        public static void ResetAll(EntityType entityType, int tenantId = -1)
        {
            Cache.Remove(GetStateCacheKey(entityType, tenantId));
            Cache.Remove(GetCancelCacheKey(entityType, tenantId));
        }
    }

    public partial class ImportDataOperation : IProgressItem
    {
        #region Constructor

        public ImportDataOperation()
            : this(EntityType.Contact, String.Empty, String.Empty)
        {
        }

        public ImportDataOperation(EntityType entityType, String CSVFileURI, String importSettingsJSON)
        {
            _CSVFileURI = CSVFileURI;
            _dataStore = Global.GetStore();
            _tenantID = TenantProvider.CurrentTenantID;
            _entityType = entityType;
            _author = SecurityContext.CurrentAccount;

            _notifyClient = NotifyClient.Instance;

            Id = String.Format("{0}_{1}", TenantProvider.CurrentTenantID, (int)_entityType);

            _log = LogManager.GetLogger("ASC.CRM");

            if (!String.IsNullOrEmpty(importSettingsJSON))
                _importSettings = new ImportCSVSettings(importSettingsJSON);
        }

        #endregion

        #region Members

        private readonly ILog _log;

        private readonly IDataStore _dataStore;

        private readonly IAccount _author;

        private readonly NotifyClient _notifyClient;

        private readonly int _tenantID;

        private readonly String _CSVFileURI;

        private readonly ImportCSVSettings _importSettings;

        private readonly EntityType _entityType;

        private String[] _columns;

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ImportDataOperation)) return false;

            var dataOperation = (ImportDataOperation)obj;

            if (_tenantID == dataOperation._tenantID && _entityType == dataOperation._entityType) return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _tenantID.GetHashCode() + _entityType.GetHashCode();
        }

        public object Clone()
        {
            var cloneObj = new ImportDataOperation
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

        private String GetPropertyValue(String propertyName)
        {
            if (_importSettings.ColumnMapping[propertyName] == null) return String.Empty;

            var values =
                _importSettings.ColumnMapping[propertyName].Values<int>().ToList().ConvertAll(columnIndex => _columns[columnIndex]);

            values.RemoveAll(item => item == String.Empty);

            return String.Join(",", values.ToArray());
        }

        private void Complete()
        {
            IsCompleted = true;

            Percentage = 100;

            _log.Debug("Import is completed");

            _notifyClient.SendAboutImportCompleted(_author.ID, _entityType);
                       
            ImportDataCache.Insert(_entityType, (ImportDataOperation)Clone());
        }

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

                    ImportDataCache.Insert(_entityType, (ImportDataOperation) Clone());

                    switch (_entityType)
                    {
                        case EntityType.Contact:
                            ImportContactsData(daoFactory);
                            break;
                        case EntityType.Opportunity:
                            ImportOpportunityData(daoFactory);
                            break;
                        case EntityType.Case:
                            ImportCaseData(daoFactory);
                            break;
                        case EntityType.Task:
                            ImportTaskData(daoFactory);
                            break;
                        default:
                            throw new ArgumentException(CRMErrorsResource.EntityTypeUnknown);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("Queue canceled");
            }
            finally
            {
                ImportDataCache.ResetAll(_entityType, _tenantID);
            }
        }
    }

}