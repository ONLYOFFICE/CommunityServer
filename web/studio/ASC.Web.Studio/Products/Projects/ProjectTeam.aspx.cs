/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Web;
using ASC.Core;
using ASC.Core.Users;

using ASC.Projects.Engine;

using ASC.Web.Studio.Utility;
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

            Manager = EngineFactory.ParticipantEngine.GetByID(Project.Responsible).UserInfo;
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