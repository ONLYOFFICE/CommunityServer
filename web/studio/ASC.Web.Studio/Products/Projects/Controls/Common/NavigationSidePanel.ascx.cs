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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Web.Studio.UserControls.Common.HelpCenter;
using ASC.Web.Studio.UserControls.Common.Support;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
using ASC.Web.Studio.UserControls.Common.UserForum;


namespace ASC.Web.Projects.Controls.Common
{
    public partial class NavigationSidePanel : BaseUserControl
    {
        public Project Project { get { return Page.Project; } }

        public List<Project> MyProjects { get; set; }

        protected Dictionary<string, bool> ParticipantSecurityInfo { get; set; }

        protected bool ShowCreateButton { get; set; }

        protected bool IsProjectAdmin { get; set; }

        protected bool IsFullAdmin { get; set; }

        protected bool IsOutsider { get; set; }

        protected bool IsStandalone
        {
            get { return CoreContext.Configuration.Standalone; }
        }

        protected bool DisplayAppsBanner
        {
            get { return ConfigurationManager.AppSettings["web.display.mobapps.banner"] == "true"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            MyProjects = RequestContext.CurrentUserProjects;

            InitControls();

            IsProjectAdmin = Page.Participant.IsAdmin;
            IsFullAdmin = Page.Participant.IsFullAdmin;
            IsOutsider = Page.Participant.UserInfo.IsOutsider();

            ParticipantSecurityInfo = new Dictionary<string, bool>
                                          {
                                              {"Project", IsProjectAdmin},
                                              {"Milestone", RequestContext.CanCreateMilestone()},
                                              {"Task", RequestContext.CanCreateTask()},
                                              {"Discussion", RequestContext.CanCreateDiscussion()},
                                              {"Time", RequestContext.CanCreateTime()},
                                              {"ProjectTemplate", IsProjectAdmin}
                                          };

            ShowCreateButton = (ParticipantSecurityInfo.Any(r => r.Value) || Page is TMDocs) && !Page.Participant.UserInfo.IsOutsider();
        }

        private void InitControls()
        {
            _taskAction.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Tasks/TaskAction.ascx")));
            _milestoneAction.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Milestones/MilestoneAction.ascx")));

            if (Page is TMDocs)
            {
                CreateDocsHolder.Controls.Add(LoadControl(Files.Controls.CreateMenu.Location));
                RenderFolderTree();
            }

            var help = (HelpCenter)LoadControl(HelpCenter.Location);
            help.IsSideBar = true;
            HelpHolder.Controls.Add(help);
            SupportHolder.Controls.Add(LoadControl(Support.Location));
            VideoGuides.Controls.Add(LoadControl(VideoGuidesControl.Location));
            UserForumHolder.Controls.Add(LoadControl(UserForum.Location));
        }

        private void RenderFolderTree()
        {
            var tree = (Files.Controls.TreeBuilder) LoadControl(Files.Controls.TreeBuilder.Location);
            tree.FolderIDCurrentRoot = Files.Classes.Global.FolderProjects;
            placeHolderFolderTree.Controls.Add(tree);
        }

        protected bool IsInConcreteProject()
        {
            return Project != null && MyProjects.Any(r => r.ID == Project.ID);
        }
    }
}