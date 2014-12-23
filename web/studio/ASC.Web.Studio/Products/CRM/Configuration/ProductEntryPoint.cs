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

using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Web.Files.Api;
using ASC.Web.Studio.Utility;
using System;
using System.Linq;


namespace ASC.Web.CRM.Configuration
{

    public class ProductEntryPoint : Product
    {
        public static readonly Guid ID = new Guid("{6743007C-6F95-4d20-8C88-A8601CE5E76D}");
        private ProductContext context;

        private static readonly object Locker = new object();
        private static bool registered;


        public override Guid ProductID { get { return ID; } }

        public override string Name { get { return CRMCommonResource.ProductName; } }

        public override string ExtendedDescription { get { return string.Format(CRMCommonResource.ProductDescriptionEx, "<span style='display:none'>", "</span>"); } }

        public override string Description { get { return CRMCommonResource.ProductDescription; } }

        public override string StartURL { get { return PathProvider.StartURL(); } }

        public override string ProductClassName { get { return "crm"; } }

        public override ProductContext Context { get { return context; } }

        public string ModuleSysName { get; set; }


        public override void Init()
        {
            context = new ProductContext
            {
                MasterPageFile = String.Concat(PathProvider.BaseVirtualPath, "Masters/BasicTemplate.Master"),
                DisabledIconFileName = "product_disabled_logo.png",
                IconFileName = "product_logo.png",
                LargeIconFileName = "product_logolarge.png",
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
        }


        public static void ConfigurePortal()
        {
            if (!Global.TenantSettings.IsConfiguredPortal)
            {
                // Task Category
                var listItemDao = Global.DaoFactory.GetListItemDao();
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
                var milestoneDao = Global.DaoFactory.GetDealMilestoneDao();
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
                listItemDao.CreateItem(ListType.ContactStatus, new ListItem { Title = CRMContactResource.ContactStatus_Cold, Color = "#8a98d8", SortOrder = 1 });
                listItemDao.CreateItem(ListType.ContactStatus, new ListItem { Title = CRMContactResource.ContactStatus_Warm, Color = "#ffd267", SortOrder = 2 });
                listItemDao.CreateItem(ListType.ContactStatus, new ListItem { Title = CRMContactResource.ContactStatus_Hot, Color = "#df7895", SortOrder = 3 });                // Contact Type
                listItemDao.CreateItem(ListType.ContactType, new ListItem { Title = CRMContactResource.ContactType_Client, SortOrder = 1 });
                listItemDao.CreateItem(ListType.ContactType, new ListItem { Title = CRMContactResource.ContactType_Supplier, SortOrder = 2 });
                listItemDao.CreateItem(ListType.ContactType, new ListItem { Title = CRMContactResource.ContactType_Partner, SortOrder = 3 });
                listItemDao.CreateItem(ListType.ContactType, new ListItem { Title = CRMContactResource.ContactType_Competitor, SortOrder = 4 });

                // History Category
                listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Note, "event_category_note.png"));
                listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Email, "event_category_email.png"));
                listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Call, "event_category_call.png"));
                listItemDao.CreateItem(ListType.HistoryCategory, new ListItem(CRMCommonResource.HistoryCategory_Meeting, "event_category_meeting.png"));
                // Tags
                Global.DaoFactory.GetTagDao().AddTag(EntityType.Contact, CRMContactResource.Lead);
                Global.DaoFactory.GetTagDao().AddTag(EntityType.Contact, CRMContactResource.Customer);
                Global.DaoFactory.GetTagDao().AddTag(EntityType.Contact, CRMContactResource.Supplier);
                Global.DaoFactory.GetTagDao().AddTag(EntityType.Contact, CRMContactResource.Staff);

                var tenantSettings = Global.TenantSettings;
                tenantSettings.WebFormKey = Guid.NewGuid();
                tenantSettings.IsConfiguredPortal = true;
                SettingsManager.Instance.SaveSettings(tenantSettings, TenantProvider.CurrentTenantID);
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