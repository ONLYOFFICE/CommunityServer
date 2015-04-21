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

using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;

using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls.Common;
using ASC.Web.Projects.Controls.Messages;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.Projects
{
    public partial class Messages : BasePage
    {
        protected Message Discussion { get; set; }
        protected bool CanCreate { get; set; }

        protected override bool CanRead { get { return !RequestContext.IsInConcreteProject || ProjectSecurity.CanReadMessages(Project); } }

        protected override void PageLoad()
        {
            Utility.RegisterTypeForAjax(typeof(CommonControlsConfigurer), Page);

            var messageEngine = Global.EngineFactory.GetMessageEngine();

            CanCreate = RequestContext.CanCreateDiscussion(true);

            int discussionId;
            if (int.TryParse(UrlParameters.EntityID, out discussionId))
            {
                Discussion = messageEngine.GetByID(discussionId);

                if (string.Compare(UrlParameters.ActionType, "edit", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (ProjectSecurity.CanEdit(Discussion))
                    {
                        LoadDiscussionActionControl(Discussion);
                    }
                    else
                    {
                        Response.Redirect("messages.aspx", true);
                    }

                    Title = HeaderStringHelper.GetPageTitle(Discussion.Title);
                }
                else if (Discussion != null && ProjectSecurity.CanRead(Discussion.Project) && Discussion.Project.ID == Project.ID)
                {
                    LoadDiscussionDetailsControl(Discussion);
                    
                    IsSubcribed = messageEngine.IsSubscribed(Discussion);
                    EssenceTitle = Discussion.Title;

                    Title = HeaderStringHelper.GetPageTitle(Discussion.Title);
                }
                else
                {
                    RedirectNotFound(string.Format("messages.aspx?prjID={0}", Project.ID));
                }

            }
            else
            {
                if (string.Compare(UrlParameters.ActionType, "add", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (CanCreate)
                    {
                        LoadDiscussionActionControl(null);

                        Title = HeaderStringHelper.GetPageTitle(MessageResource.CreateMessage);
                    }
                    else
                    {
                        Response.Redirect("messages.aspx", true);
                    }
                }
                else
                {
                    contentHolder.Controls.Add(LoadControl(CommonList.Location));
                    loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
                }
            }
        }

        private void LoadDiscussionDetailsControl(Message discussion)
        {
            var discussionDetails = (DiscussionDetails)LoadControl(PathProvider.GetFileStaticRelativePath("Messages/DiscussionDetails.ascx"));
            discussionDetails.Discussion = discussion;
            contentHolder.Controls.Add(discussionDetails);
        }

        private void LoadDiscussionActionControl(Message discussion)
        {
            var discussionAction = (DiscussionAction)LoadControl(PathProvider.GetFileStaticRelativePath("Messages/DiscussionAction.ascx"));
            discussionAction.Discussion = discussion;
            contentHolder.Controls.Add(discussionAction);

            Master.DisabledPrjNavPanel = true;
        }
    }
}
