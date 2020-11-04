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

using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Core;
using ASC.Web.Projects.Core.Search;
using ASC.Web.Projects.Masters.ClientScripts;
using ASC.Web.Projects.Resources;

using AutoMapper;


namespace ASC.Web.Projects.Configuration
{
    public class ProductEntryPoint : Product
    {
        private ProductContext context;
        internal static IMapper Mapper { get; set; }

        public static readonly Guid ID = EngineFactory.ProductId;
        public static readonly Guid MilestoneModuleID = new Guid("{AF4AFD50-5553-47f3-8F91-651057BC930B}");
        public static readonly Guid TasksModuleID = new Guid("{04339423-70E6-4b81-A2DF-3C31C723BD90}");
        public static readonly Guid GanttChartModuleID = new Guid("{23CD2123-3C4C-4868-B927-A26BB49CA458}");
        public static readonly Guid MessagesModuleID = new Guid("{9FF0FADE-6CFA-44ee-901F-6185593E4594}");
        public static readonly Guid DocumentsModuleID = new Guid("{81402440-557D-401d-9EE1-D570748F426D}");
        public static readonly Guid TimeTrackingModuleID = new Guid("{57E87DA0-D59B-443d-99D1-D9ABCAB31084}");
        public static readonly Guid ProjectTeamModuleID = new Guid("{C42F993E-5D22-497e-AC26-1E9592515898}");
        public static readonly Guid ContactsModuleID = new Guid("{ec12f0ba-14cb-413c-b5e5-65f6ddd5fc19}");


        public override Guid ProductID
        {
            get { return ID; }
        }

        public override string Name
        {
            get { return ProjectsCommonResource.ProductName; }
        }

        public override string Description
        {
            get
            {
                var id = SecurityContext.CurrentAccount.ID;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupVisitor.ID))
                    return ProjectsCommonResource.ProductDescriptionShort;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupAdmin.ID) || CoreContext.UserManager.IsUserInGroup(id, ID))
                    return ProjectsCommonResource.ProductDescriptionEx;

                return ProjectsCommonResource.ProductDescription;
            }
        }

        public override string StartURL
        {
            get { return PathProvider.BaseVirtualPath; }
        }

        public override string HelpURL
        {
            get { return string.Concat(PathProvider.BaseVirtualPath, "Help.aspx"); }
        }

        public override string ProductClassName
        {
            get { return "projects"; }
        }

        public override bool Visible { get { return true; } }

        public override ProductContext Context
        {
            get { return context; }
        }

        public override void Init()
        {
            context = new ProductContext
            {
                MasterPageFile = String.Concat(PathProvider.BaseVirtualPath, "Masters/BasicTemplate.Master"),
                DisabledIconFileName = "product_disabled_logo.png",
                IconFileName = "product_logo.png",
                LargeIconFileName = "product_logolarge.svg",
                SubscriptionManager = new ProductSubscriptionManager(),
                DefaultSortOrder = 20,
                SpaceUsageStatManager = new ProjectsSpaceUsageStatManager(),
                AdminOpportunities = () => ProjectsCommonResource.ProductAdminOpportunities.Split('|').ToList(),
                UserOpportunities = () => ProjectsCommonResource.ProductUserOpportunities.Split('|').ToList(),
                HasComplexHierarchyOfAccessRights = true,
            };

            FileEngine.RegisterFileSecurityProvider();
            SearchHandlerManager.Registry(new SearchHandler());
            NotifyClient.RegisterSecurityInterceptor();
            ClientScriptLocalization = new ClientLocalizationResources();
            DIHelper.Register();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Task, TasksWrapper>().ForMember(r => r.TenantId, opt => opt.MapFrom(r => GetCurrentTenant()));
                cfg.CreateMap<Message, DiscussionsWrapper>().ForMember(r => r.TenantId, opt => opt.MapFrom(r => GetCurrentTenant()));
                cfg.CreateMap<Milestone, MilestonesWrapper>().ForMember(r => r.TenantId, opt => opt.MapFrom(r => GetCurrentTenant()));
                cfg.CreateMap<Project, ProjectsWrapper>().ForMember(r => r.TenantId, opt => opt.MapFrom(r => GetCurrentTenant()));
                cfg.CreateMap<Subtask, SubtasksWrapper>().ForMember(r => r.TenantId, opt => opt.MapFrom(r => GetCurrentTenant()));
                cfg.CreateMap<Comment, CommentsWrapper>().ForMember(r => r.TenantId, opt => opt.MapFrom(r => GetCurrentTenant())).ForMember(r => r.LastModifiedOn, opt => opt.Ignore());
            });
            configuration.AssertConfigurationIsValid();
            Mapper = configuration.CreateMapper();
        }

        private int GetCurrentTenant()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId;
        }
        public override void Shutdown()
        {
            NotifyClient.UnregisterSendMethods();
        }

        public static void RegisterSendMethods()
        {
            NotifyClient.RegisterSendMethods();
        }
    }
}