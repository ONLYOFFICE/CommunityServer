/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using ASC.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common.InviteLink;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
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

        private readonly bool auditTrailEnabled = true;
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

            TenantAccess = SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            VideoGuides.Controls.Add(LoadControl(VideoGuidesControl.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
            InviteUserHolder.Controls.Add(LoadControl(InviteLink.Location));

            CurrentModule = GetCurrentModule();
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
            var tenantAccessAnyone = SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID);
            switch (module)
            {
                case ManagementType.Migration:
                    return SetupInfo.IsVisibleSettings(module.ToString()) && TransferPortal.TransferRegions.Count > 1;

                case ManagementType.Backup:
                    //only SaaS features
                    return !CoreContext.Configuration.Standalone
                           && !tenantAccessAnyone.Anyone && SetupInfo.IsVisibleSettings(module.ToString());

                case ManagementType.AuditTrail:
                case ManagementType.LoginHistory:
                case ManagementType.LdapSettings:
                case ManagementType.DeletionPortal:
                    //only SaaS features
                    return !CoreContext.Configuration.Standalone && SetupInfo.IsVisibleSettings(module.ToString());
                case ManagementType.WhiteLabel:
                    return Web.UserControls.WhiteLabel.WhiteLabel.AvailableControl;
                case ManagementType.SingleSignOnSettings:
                    return !CoreContext.Configuration.Standalone;

                default:
                    return SetupInfo.IsVisibleSettings(module.ToString());
            }
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
                                                                    ManagementType.AccessRights
                                                                })
                                           {
                                               Title = Resource.ManagementCategorySecurity,
                                               ClassName = "security"
                                           };
            if (auditTrailEnabled)
            {
                securityCategorySettings.AddModules(ManagementType.LoginHistory, ManagementType.AuditTrail);
            }

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
                                         ManagementType.DeletionPortal
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

            return new List<CategorySettings>
                   {
                       generalSettings,
                       securityCategorySettings,
                       backupSettings,
                       integrationCategorySettings,
                       statisticSettings,
                       monitoringSettings
                   };
        }

        private static Dictionary<ManagementType, ManagementControlAttribute[]> LoadModules()
        {
            return Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Select(type => type.GetCustomAttribute<ManagementControlAttribute>())
                           .Where(control => control != null  && DisplayModule(control))
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