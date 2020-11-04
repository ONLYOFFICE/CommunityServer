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
using System.Reflection;
using System.Threading;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.UserForum;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;

using Resources;

namespace ASC.Web.Studio
{
    public partial class Management : MainPage
    {
        protected static readonly Lazy<Dictionary<ManagementType, ManagementControlAttribute[]>> ManagementModules =
            new Lazy<Dictionary<ManagementType, ManagementControlAttribute[]>>(LoadModules, LazyThreadSafetyMode.PublicationOnly);

        public TenantAccessSettings TenantAccess { get; private set; }
        protected ManagementType CurrentModule { get; private set; }
        protected List<ManagementType> NavigationList { get; private set; }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (CoreContext.Configuration.Personal)
            {
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);
            }

            if (!SecurityContext.CheckPermissions(SecutiryConstants.EditPortalSettings))
            {
                Response.Redirect(CommonLinkUtility.GetDefault());
            }

            TenantAccess = TenantAccessSettings.Load();
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));

            CurrentModule = GetCurrentModule();
            LogManager.GetLogger("ASC").Debug("Test " + CurrentModule);
            NavigationList = GetNavigationList();
            Page.Title = HeaderStringHelper.GetPageTitle(GetNavigationTitle(CurrentModule));

            if (CurrentModule == ManagementType.HelpCenter)
            {
                SettingsContainer.Controls.Add(LoadControl(HelpCenter.Location));
                return;
            }

            foreach (var control in ManagementModules.Value[CurrentModule])
            {
                SettingsContainer.Controls.Add(LoadControl(control.Location));
            }
        }

        protected ManagementType GetCurrentModule()
        {
            const ManagementType defaultModule = ManagementType.Customization;
            ManagementType currentModule;

            if (!Enum.TryParse(Request["type"], out currentModule))
            {
                return defaultModule;
            }

            if (!ManagementModules.Value.ContainsKey(currentModule) && currentModule != ManagementType.HelpCenter)
            {
                return defaultModule;
            }

            return currentModule;
        }

        private static List<ManagementType> GetNavigationList()
        {
            return ManagementModules.Value
                                    .Where(keyValue => keyValue.Value.Any())
                                    .Select(keyValue => keyValue.Key)
                                    .ToList();
        }

        protected string GetNavigationTitle(ManagementType module)
        {
            switch (module)
            {
                case ManagementType.Statistic:
                    return Resource.StatisticsTitle;
                case ManagementType.AuditTrail:
                    return AuditResource.AuditTrailNav;
                case ManagementType.LoginHistory:
                    return AuditResource.LoginHistoryNav;
                case ManagementType.PortalSecurity:
                    return Resource.PortalSecurity;
                case ManagementType.SmtpSettings:
                    return Resource.SmtpSettings;
                case ManagementType.FullTextSearch:
                    return Resource.FullTextSearchSettings;
                case ManagementType.DeletionPortal:
                    return Resource.DeactivationDeletionPortal;
                case ManagementType.DocService:
                    return Resource.DocService;
                case ManagementType.WhiteLabel:
                    return Resource.WhiteLabel;
                case ManagementType.MailService:
                    return Resource.MailService;
                case ManagementType.Customization:
                    return Resource.Customization;
                case ManagementType.ThirdPartyAuthorization:
                    return Resource.ThirdPartyAuthorization;
                case ManagementType.AccessRights:
                    return Resource.AccessRights;
                case ManagementType.ProductsAndInstruments:
                    return Resource.ProductsAndInstruments;
                case ManagementType.Backup:
                    return Resource.Backup;
                case ManagementType.Storage:
                    return Resource.Storage;
                case ManagementType.PrivacyRoom:
                    return Resource.Encryption;
                default:
                    return Resource.ResourceManager.GetString(module.ToString()) ?? module.ToString();
            }
        }

        protected static bool DisplayModule(ManagementControlAttribute control)
        {
            var result = DisplayModule(control.Module);
            if (control.SubModule.HasValue)
                result = result && DisplayModule(control.SubModule.Value);

            return result;
        }

        protected static bool DisplayModule(ManagementType module)
        {
            if (!SetupInfo.IsVisibleSettings(module.ToString())) return false;

            switch (module)
            {
                case ManagementType.Migration:
                    return TransferPortal.TransferRegions.Count > 1;
                case ManagementType.Backup:
                    //only SaaS features
                    return !CoreContext.Configuration.Standalone &&
                        !TenantAccessSettings.Load().Anyone;
                case ManagementType.AuditTrail:
                case ManagementType.LoginHistory:
                case ManagementType.LdapSettings:
                case ManagementType.WhiteLabel:
                case ManagementType.SingleSignOnSettings:
                    //only SaaS features
                    return !CoreContext.Configuration.Standalone;
                case ManagementType.DeletionPortal:
                    //only SaaS or Server+ControlPanel
                    return !CoreContext.Configuration.Standalone || TenantExtra.Enterprise && CoreContext.TenantManager.GetTenants().Count() > 1;
                case ManagementType.MailService:
                    //only if MailServer available
                    return SetupInfo.IsVisibleSettings("AdministrationPage");
                case ManagementType.Storage:
                    //only standalone feature
                    return CoreContext.Configuration.Standalone;
                case ManagementType.PrivacyRoom:
                    return !CoreContext.Configuration.Standalone && PrivacyRoomSettings.Available;
            }

            return true;
        }

        protected bool DisplayModuleList(CategorySettings category)
        {
            return NavigationList.Intersect(category.Modules).Any();
        }

        protected class CategorySettings
        {
            public String Title { get; set; }
            public ManagementType ModuleUrl { get; set; }
            public String ClassName { get; set; }

            private List<ManagementType> modules;
            public IEnumerable<ManagementType> Modules { get { return modules; } }

            public CategorySettings() { }

            public CategorySettings(IEnumerable<ManagementType> managementTypes)
            {
                modules = managementTypes.Where(DisplayModule).ToList();
            }

            public void AddModules(params ManagementType[] newModules)
            {
                if (modules == null) modules = new List<ManagementType>();
                modules.AddRange(newModules.Where(DisplayModule));
            }

            public string GetNavigationUrl()
            {
                return GetNavigationUrl(modules == null ? ModuleUrl : modules.First());
            }

            public string GetNavigationUrl(ManagementType module)
            {
                return CommonLinkUtility.GetAdministration(module);
            }
        }

        protected List<CategorySettings> GetCategoryList()
        {
            var securityCategorySettings = new CategorySettings(new[]
                                                                {
                                                                    ManagementType.PortalSecurity,
                                                                    ManagementType.AccessRights,
                                                                    ManagementType.PrivacyRoom,
                                                                    ManagementType.LoginHistory,
                                                                    ManagementType.AuditTrail
                                                                })
            {
                Title = Resource.ManagementCategorySecurity,
                ClassName = "security"
            };

            var generalSettings = new CategorySettings(new[]
                                                       {
                                                           ManagementType.Customization,
                                                           ManagementType.ProductsAndInstruments,
                                                           ManagementType.WhiteLabel
                                                       })
            {
                Title = Resource.ManagementCategoryCommon,
                ClassName = "general"
            };

            var backupSettings =
                new CategorySettings(new[]
                                     {
                                         ManagementType.Migration,
                                         ManagementType.Backup,
                                         ManagementType.DeletionPortal,
                                         ManagementType.Storage
                                     })
                {
                    Title = Resource.DataManagement,
                    ClassName = "backup"
                };

            var integrationCategorySettings = new CategorySettings(new[]
                                                                   {
                                                                       ManagementType.LdapSettings,
                                                                       ManagementType.ThirdPartyAuthorization,
                                                                       ManagementType.DocService,
                                                                       ManagementType.MailService,
                                                                       ManagementType.SmtpSettings
                                                                   })
            {
                Title = Resource.ManagementCategoryIntegration,
                ClassName = "productsandinstruments"
            };

            if (CoreContext.Configuration.Standalone)
            {
                integrationCategorySettings.AddModules(ManagementType.FullTextSearch);
            }

            var statisticSettings = new CategorySettings
            {
                Title = Resource.ManagementCategoryStatistic,
                ModuleUrl = ManagementType.Statistic,
                ClassName = "statistic"
            };
            var monitoringSettings = new CategorySettings
            {
                Title = Resource.Monitoring,
                ModuleUrl = ManagementType.Monitoring,
                ClassName = "monitoring"
            };

            var result = new List<CategorySettings>
                   {
                       generalSettings,
                       securityCategorySettings,
                       backupSettings,
                       integrationCategorySettings,
                       statisticSettings,
                       monitoringSettings
                   };

            return result;
        }

        private static Dictionary<ManagementType, ManagementControlAttribute[]> LoadModules()
        {
            return Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Select(type => type.GetCustomAttribute<ManagementControlAttribute>())
                           .Where(control => control != null && DisplayModule(control))
                           .GroupBy(control => control.Module)
                           .OrderBy(group => (int)group.Key)
                           .ToDictionary(
                           group => group.Key,
                           group => group.OrderBy(control => control.SortOrder).ToArray());
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ManagementControlAttribute : Attribute
    {
        public ManagementType Module { get; private set; }

        public ManagementType? SubModule { get; private set; }

        public string Location { get; private set; }

        public int SortOrder { get; set; }

        public ManagementControlAttribute(ManagementType module, string location)
        {
            Module = module;
            Location = location;
        }

        public ManagementControlAttribute(ManagementType module, ManagementType submodule, string location)
        {
            Module = module;
            SubModule = submodule;
            Location = location;
        }
    }
}