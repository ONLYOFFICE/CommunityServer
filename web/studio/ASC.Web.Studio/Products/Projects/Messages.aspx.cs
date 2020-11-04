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
                        Response.Redirect("Messages.aspx", true);
                    }
                }
                else
                {
                    if (discussion != null && (!ProjectSecurity.CanRead(discussion.Project) ||
                        discussion.Project.ID != Project.ID))
                    {
                        RedirectNotFound(string.Format("Messages.aspx?prjID={0}", Project.ID));
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
                        Response.Redirect("Messages.aspx", true);
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
