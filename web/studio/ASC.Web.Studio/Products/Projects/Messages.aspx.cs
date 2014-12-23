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
