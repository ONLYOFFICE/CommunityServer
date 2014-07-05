/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System.Web;
using System.Linq;

using ASC.Core;
using ASC.Core.Users;

using ASC.Projects.Engine;

using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects
{
    public partial class ProjectTeam : BasePage
    {

        #region Properties

        public UserInfo Manager { get; set; }

        public bool CanEditTeam { get; set; }

        public string UserProfileLink
        {
            get { return CommonLinkUtility.GetUserProfile(); }
        }

        public string ManagerName { get; set; }

        public string ManagerAvatar { get; set; }

        public string ManagerProfileUrl { get; set; }

        public string ManagerDepartmentUrl { get; set; }

        #endregion

        #region Methods

        protected void InitView()
        {
            CanEditTeam = ProjectSecurity.CanEditTeam(Project);
        }

        public bool CanCreateTask()
        {
            return ProjectSecurity.CanCreateTask(Project);
        }


        #endregion

        #region Events

        protected override void PageLoad()
        {
            InitView();

            Manager = Global.EngineFactory.GetParticipantEngine().GetByID(Project.Responsible).UserInfo;
            ManagerName = Manager.DisplayUserName();
            ManagerAvatar = Manager.GetBigPhotoURL();
            ManagerProfileUrl = Manager.GetUserProfilePageURL();
            foreach (var g in CoreContext.UserManager.GetUserGroups(Manager.ID))
            {
                ManagerDepartmentUrl += string.Format("<a href=\"{0}\" class=\"linkMedium\">{1}</a>, ", CommonLinkUtility.GetDepartment(g.ID), HttpUtility.HtmlEncode(g.Name));
            }
            if (!string.IsNullOrEmpty(ManagerDepartmentUrl))
            {
                ManagerDepartmentUrl = ManagerDepartmentUrl.Substring(0, ManagerDepartmentUrl.Length - 2);
            }

            Title = HeaderStringHelper.GetPageTitle(ProjectResource.ProjectTeam);

        }

        #endregion
    }
}