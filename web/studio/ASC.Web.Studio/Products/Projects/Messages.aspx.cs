/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls.Messages;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects
{
    public partial class Messages : BasePage
    {
        protected override bool CanRead { get { return !RequestContext.IsInConcreteProject || ProjectSecurity.CanRead<Message>(Project); } }

        protected override void PageLoad()
        {
            var action = UrlParameters.ActionType;

            var discussionId = UrlParameters.EntityID;
            if (discussionId >= 0)
            {
                var discussion = EngineFactory.MessageEngine.GetByID(discussionId);

                if (action.HasValue && action.Value == UrlAction.Edit)
                {
                    if (ProjectSecurity.CanEdit(discussion))
                    {
                        LoadDiscussionActionControl(discussion);
                    }
                    else
                    {
                        Response.Redirect("messages.aspx", true);
                    }
                }
                else
                {
                    if (discussion != null && (!ProjectSecurity.CanRead(discussion.Project) ||
                        discussion.Project.ID != Project.ID))
                    {
                        RedirectNotFound(string.Format("messages.aspx?prjID={0}", Project.ID));
                    }
                }

                if (discussion != null)
                {
                    Title = HeaderStringHelper.GetPageTitle(discussion.Title);
                }
            }
            else
            {
                if (action.HasValue && action.Value == UrlAction.Add)
                {
                    if (!RequestContext.IsInConcreteProject || ProjectSecurity.CanCreate<Message>(RequestContext.GetCurrentProject(false)))
                    {
                        LoadDiscussionActionControl(null);

                        Title = HeaderStringHelper.GetPageTitle(MessageResource.CreateMessage);
                    }
                    else
                    {
                        Response.Redirect("messages.aspx", true);
                    }
                }

            }
        }

        private void LoadDiscussionActionControl(Message discussion)
        {
            var discussionAction = (DiscussionAction)LoadControl(PathProvider.GetFileStaticRelativePath("Messages/DiscussionAction.ascx"));
            discussionAction.Discussion = discussion;
            Master.AddControl(discussionAction);
        }
    }
}
