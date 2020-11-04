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
using System.Web.Http;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Configuration;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Masters.ClientScripts;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Files.Api;
using Autofac;


namespace ASC.Web.CRM.Configuration
{

    public class ProductEntryPoint : Product
    {
        public static readonly Guid ID = WebItemManager.CRMProductID;
        private ProductContext context;

        private static readonly object Locker = new object();
        private static bool registered;


        public override Guid ProductID { get { return ID; } }

        public override string Name { get { return CRMCommonResource.ProductName; } }

        public override string Description
        {
            get
            {
                var id = SecurityContext.CurrentAccount.ID;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupAdmin.ID) || CoreContext.UserManager.IsUserInGroup(id, ID))
                    return CRMCommonResource.ProductDescriptionEx;

                return CRMCommonResource.ProductDescription;
            }
        }

        public override string StartURL { get { return PathProvider.StartURL(); } }

        public override string HelpURL { get { return string.Concat(PathProvider.BaseVirtualPath, "Help.aspx"); } }

        public override string ProductClassName { get { return "crm"; } }

        public override bool Visible { get { return true; } }

        public override ProductContext Context { get { return context; } }

        public string ModuleSysName { get; set; }


        public override void Init()
        {
            context = new ProductContext
            {
                MasterPageFile = String.Concat(PathProvider.BaseVirtualPath, "Masters/BasicTemplate.Master"),
                DisabledIconFileName = "product_disabled_logo.png",
                IconFileName = "product_logo.png",
                LargeIconFileName = "product_logolarge.svg",
                DefaultSortOrder = 30,
                SubscriptionManager = new ProductSubscriptionManager(),
                SpaceUsageStatManager = new CRMSpaceUsageStatManager(),
                AdminOpportunities = () => CRMCommonResource.ProductAdminOpportunities.Split('|').ToList(),
                UserOpportunities = () => CRMCommonResource.ProductUserOpportunities.Split('|').ToList(),
            };

            if (!FilesIntegration.IsRegisteredFileSecurityProvider("crm", "crm_common"))
            {
                FilesIntegration.RegisterFileSecurityProvider("crm", "crm_common", new FileSecurityProvider());
            }
            if (!FilesIntegration.IsRegisteredFileSecurityProvider("crm", "opportunity"))
            {
                FilesIntegration.RegisterFileSecurityProvider("crm", "opportunity", new FileSecurityProvider());
            }

            SearchHandlerManager.Registry(new SearchHandler());

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "Twilio", 
                routeTemplate: "twilio/{action}", 
                defaults: new {controller = "Twilio", action = "index" });

            ClientScriptLocalization = new ClientLocalizationResources();
            DIHelper.Register();
        }


        public static void ConfigurePortal()
        {
            if (!Global.TenantSettings.IsConfiguredPortal)
            {
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    // Task Category
                    var listItemDao = daoFactory.ListItemDao;
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Call, "task_category_call.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Deal, "task_category_deal.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Demo, "task_category_demo.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Email, "task_category_email.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Fax, "task_category_fax.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_FollowUP, "task_category_follow_up.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Lunch, "task_category_lunch.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Meeting, "task_category_meeting.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Note, "task_category_note.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_Ship, "task_category_ship.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_SocialNetworks, "task_category_social_networks.png"));
                    listItemDao.CreateItem(ListType.TaskCategory, new ListItem(CRMTaskResource.TaskCategory_ThankYou, "task_category_thank_you.png"));

                    // Deal Milestone New
                    var milestoneDao = daoFactory.DealMilestoneDao;
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_InitialContact_Title,
                        Description = CRMDealResource.DealMilestone_InitialContact_Description,
                        Probability = 1,
                        Color = "#e795c1",
                        Status = DealMilestoneStatus.Open
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Preapproach_Title,
                        Description = CRMDealResource.DealMilestone_Preapproach_Description,
                        Probability = 2,
                        Color = "#df7895",
                        Status = DealMilestoneStatus.Open
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Suspect_Title,
                        Description = CRMDealResource.DealMilestone_Suspect_Description,
                        Probability = 3,
                        Color = "#f48454",
                        SortOrder = 1,
                        Status = DealMilestoneStatus.Open
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Champion_Title,
                        Description = CRMDealResource.DealMilestone_Champion_Description,
                        Probability = 20,
                        Color = "#b58fd6",
                        SortOrder = 2,
                        Status = DealMilestoneStatus.Open
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Opportunity_Title,
                        Description = CRMDealResource.DealMilestone_Opportunity_Description,
                        Probability = 50,
                        Color = "#d28cc8",
                        SortOrder = 3,
                        Status = DealMilestoneStatus.Open
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Prospect_Title,
                        Description = CRMDealResource.DealMilestone_Prospect_Description,
                        Probability = 75,
                        Color = "#ffb45e",
                        SortOrder = 4,
                        Status = DealMilestoneStatus.Open
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Verbal_Title,
                        Description = CRMDealResource.DealMilestone_Verbal_Description,
                        Probability = 90,
                        Color = "#ffd267",
                        SortOrder = 5,
                        Status = DealMilestoneStatus.Open
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Won_Title,
                        Description = CRMDealResource.DealMilestone_Won_Description,
                        Probability = 100,
                        Color = "#6bbd72",
                        SortOrder = 6,
                        Status = DealMilestoneStatus.ClosedAndWon
                    });
                    milestoneDao.Create(new DealMilestone
                    {
                        Title = CRMDealResource.DealMilestone_Lost_Title,
                        Description = CRMDealResource.DealMilestone_Lost_Description,
                        Probability = 0,
                        Color = "#f2a9be",
                        SortOrder = 7,
                        Status = DealMilestoneStatus.ClosedAndLost
                    });

                    // Contact Status
                    listItemDao.CreateItem(ListType.ContactStatus, new ListItem {Title = CRMContactResource.ContactStatus_Cold, Color = "#8a98d8", SortOrder = 1});
                    listItemDao.CreateItem(ListType.ContactStatus, new ListItem {Title = CRMContactResource.ContactStatus_Warm, Color = "#ffd267", SortOrder = 2});
                    listItemDao.CreateItem(ListType.ContactStatus, new ListItem {Title = CRMContactResource.ContactStatus_Hot, Color = "#df7895", SortOrder = 3});
                        // Contact Type
                    listItemDao.CreateItem(ListType.ContactType, new ListItem {Title = CRMContactResource.ContactType_Client, SortOrder = 1});
                    listItemDao.CreateItem(ListType.ContactType, new ListItem {Title = CRMContactResource.ContactType_Supplier, SortOrder = 2});
                    listItemDao.CreateItem(ListType.ContactType, new ListItem {Title = CRMContactResource.ContactType_Partner, SortOrder = 3});
                    listItemDao.CreateItem(ListType.ContactType, new ListItem {Title = CRMContactResource.ContactType_Competitor, SortOrder = 4});

                    // History Category
                    listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Note, "event_category_note.png"));
                    listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Email, "event_category_email.png"));
                    listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Call, "event_category_call.png"));
                    listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Meeting, "event_category_meeting.png"));
                    // Tags
                    daoFactory.TagDao.AddTag(EntityType.Contact, CRMContactResource.Lead, true);
                    daoFactory.TagDao.AddTag(EntityType.Contact, CRMContactResource.Customer, true);
                    daoFactory.TagDao.AddTag(EntityType.Contact, CRMContactResource.Supplier, true);
                    daoFactory.TagDao.AddTag(EntityType.Contact, CRMContactResource.Staff, true);

                    var tenantSettings = Global.TenantSettings;
                    tenantSettings.WebFormKey = Guid.NewGuid();
                    tenantSettings.IsConfiguredPortal = true;
                    tenantSettings.Save();
                }
            }

            if (!Global.TenantSettings.IsConfiguredSmtp)
            {
                var smtp = CRMSettings.Load().SMTPServerSettingOld;
                if (smtp != null && CoreContext.Configuration.SmtpSettings.IsDefaultSettings)
                {
                    try
                    {
                        var newSettings = new SmtpSettings(smtp.Host, smtp.Port, smtp.SenderEmailAddress,
                            smtp.SenderDisplayName)
                        {
                            EnableSSL = smtp.EnableSSL,
                            EnableAuth = smtp.RequiredHostAuthentication,
                        };

                        if (!string.IsNullOrEmpty(smtp.HostLogin) && !string.IsNullOrEmpty(smtp.HostPassword))
                        {
                            newSettings.SetCredentials(smtp.HostLogin, smtp.HostPassword);
        }

                        CoreContext.Configuration.SmtpSettings = newSettings;
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("ASC").Error("ConfigurePortal", e);
                    }
                }
                var tenantSettings = Global.TenantSettings;
                tenantSettings.IsConfiguredSmtp = true;
                tenantSettings.Save();
            }
        }

        public override void Shutdown()
        {
            if (registered)
            {
                NotifyClient.Instance.Client.UnregisterSendMethod(NotifyClient.SendAutoReminderAboutTask);
              
            }
        }

        public static void RegisterSendMethods()
        {
            lock (Locker)
            {
                if (!registered)
                {
                    registered = true;

                    NotifyClient.Instance.Client.RegisterSendMethod(NotifyClient.SendAutoReminderAboutTask, "0 * * ? * *");

                }
            }
        }
    }
}