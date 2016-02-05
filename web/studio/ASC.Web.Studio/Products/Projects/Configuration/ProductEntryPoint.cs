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
using System.Linq;

using ASC.Notify;
using ASC.Notify.Engine;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;

using log4net;


namespace ASC.Web.Projects.Configuration
{
    public class ProductEntryPoint : Product
    {
        private static readonly object Locker = new object();
        private static bool registered;

        private ProductContext context;

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

        public override string ExtendedDescription
        {
            get { return string.Format(ProjectsCommonResource.ProductDescriptionEx, "<span style='display:none'>", "</span>"); }
        }

        public override string Description
        {
            get { return ProjectsCommonResource.ProductDescription; }
        }

        public override string StartURL
        {
            get { return string.Concat(PathProvider.BaseVirtualPath, "projects.aspx"); }
        }

        public override string ProductClassName
        {
            get { return "projects"; }
        }

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
                              LargeIconFileName = "product_logolarge.png",
                              SubscriptionManager = new ProductSubscriptionManager(),
                              DefaultSortOrder = 20,
                              SpaceUsageStatManager = new ProjectsSpaceUsageStatManager(),
                              AdminOpportunities = () => ProjectsCommonResource.ProductAdminOpportunities.Split('|').ToList(),
                              UserOpportunities = () => ProjectsCommonResource.ProductUserOpportunities.Split('|').ToList(),
                              HasComplexHierarchyOfAccessRights = true,
                          };

            FileEngine.RegisterFileSecurityProvider();
            SearchHandlerManager.Registry(new SearchHandler());

            var securityInterceptor = new SendInterceptorSkeleton(
                "ProjectInterceptorSecurity",
                InterceptorPlace.DirectSend,
                InterceptorLifetime.Global,
                (r, p) =>
                    {
                        try
                        {
                            var data = r.ObjectID.Split('_');
                            var entityType = data[0];
                            var entityId = Convert.ToInt32(data[1]);

                            var projectId = 0;
                            
                            if(data.Length == 3)
                                projectId = Convert.ToInt32(r.ObjectID.Split('_')[2]);

                            switch (entityType)
                            {
                                case "Task":
                                    var task = Global.EngineFactory.TaskEngine.GetByID(entityId, false);

                                    if (task == null && projectId != 0)
                                    {
                                        var project = Global.EngineFactory.ProjectEngine.GetByID(projectId, false);
                                        return !ProjectSecurity.CanRead(project, new Guid(r.Recipient.ID));
                                    }

                                    return !ProjectSecurity.CanRead(task, new Guid(r.Recipient.ID));
                                case "Message":
                                    var discussion = Global.EngineFactory.MessageEngine.GetByID(entityId, false);

                                    if (discussion == null && projectId != 0)
                                    {
                                        var project = Global.EngineFactory.ProjectEngine.GetByID(projectId, false);
                                        return !ProjectSecurity.CanRead(project, new Guid(r.Recipient.ID));
                                    }

                                    return !ProjectSecurity.CanRead(discussion, new Guid(r.Recipient.ID));
                                case "Milestone":
                                    var milestone = Global.EngineFactory.MilestoneEngine.GetByID(entityId, false);

                                    if (milestone == null && projectId != 0)
                                    {
                                        var project = Global.EngineFactory.ProjectEngine.GetByID(projectId, false);
                                        return !ProjectSecurity.CanRead(project, new Guid(r.Recipient.ID));
                                    }

                                    return !ProjectSecurity.CanRead(milestone, new Guid(r.Recipient.ID));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.GetLogger("ASC.Projects.Tasks").Error("Send", ex);
                        }
                        return false;
                    });

            NotifyClient.Instance.Client.AddInterceptor(securityInterceptor);
        }

        public override void Shutdown()
        {
            if (registered)
            {
                NotifyClient.Instance.Client.UnregisterSendMethod(NotifyHelper.SendMsgMilestoneDeadline);
                NotifyClient.Instance.Client.UnregisterSendMethod(NotifyHelper.SendAutoReports);
                NotifyClient.Instance.Client.UnregisterSendMethod(NotifyHelper.SendAutoReminderAboutTask);

                NotifyClient.Instance.Client.RemoveInterceptor("ProjectInterceptorSecurity");
            }
        }

        public static void RegisterSendMethods()
        {
            lock (Locker)
            {
                if (!registered)
                {
                    registered = true;
                    NotifyClient.Instance.Client.RegisterSendMethod(NotifyHelper.SendMsgMilestoneDeadline, "0 0 7 ? * *");
                    NotifyClient.Instance.Client.RegisterSendMethod(NotifyHelper.SendAutoReports, "0 0 * ? * *");
                    NotifyClient.Instance.Client.RegisterSendMethod(NotifyHelper.SendAutoReminderAboutTask, "0 0 * ? * *");
                }
            }
        }
    }
}