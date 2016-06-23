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


using ASC.Web.Projects.Controls.Common;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.UserControls.EmptyScreens;
using ASC.Web.Studio.Utility;
using ASC.Projects.Engine;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.Projects
{
    public partial class Projects : BasePage
    {
        protected override bool CheckSecurity
        {
            get
            {
                var action = UrlParameters.ActionType;
                if (!action.HasValue) return true;

                if (RequestContext.IsInConcreteProject)
                {
                    if (action.Value == UrlAction.Edit)
                    {
                        return ProjectSecurity.CanEdit(Project);
                    }
                }
                else
                {
                    if (action.Value == UrlAction.Add)
                    {
                        return ProjectSecurity.CanCreateProject();
                    }
                }

                return true;
            }
        }

        protected override void PageLoad()
        {
            var action = UrlParameters.ActionType;

            if (RequestContext.IsInConcreteProject)
            {
                if (action.HasValue && action.Value == UrlAction.Edit)
                {
                    _content.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Projects/ProjectAction.ascx")));
                    Master.DisabledPrjNavPanel = true;
                    return;
                }

                Response.Redirect(string.Concat(PathProvider.BaseAbsolutePath, "tasks.aspx?prjID=", RequestContext.GetCurrentProjectId().ToString()));
            }
            else
            {
                if (action.HasValue && action.Value == UrlAction.Add)
                {
                    _content.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Projects/ProjectAction.ascx")));
                    return;
                }
            }

            RenderControls();

            Title = HeaderStringHelper.GetPageTitle(ProjectResource.Projects);
        }

        private void RenderControls()
        {
            _content.Controls.Add(LoadControl(CommonList.Location));
            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            if (ProjectSecurity.CanCreateProject() && EngineFactory.ProjectEngine.Count() <= 0)
            {
                var emptyScreen = (ProjectsDashboardEmptyScreen)Page.LoadControl(ProjectsDashboardEmptyScreen.Location);
                emptyScreen.IsAdmin = Participant.IsAdmin;
                _content.Controls.Add(emptyScreen);
            }
        }
    }
}