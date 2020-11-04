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
using ASC.Web.Community.Forum.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.Community.Forum
{
    public partial class EditTopic : MainPage
    {
        private TopicEditorControl _topicEditorControl;

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.EditTopic);

            _topicEditorControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/TopicEditorControl.ascx") as TopicEditorControl;
            _topicEditorControl.SettingsID = ForumManager.Settings.ID;
            topicEditorHolder.Controls.Add(_topicEditorControl);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            (Master as ForumMasterPage).CurrentPageCaption = ForumResource.EditTopicBreadCrumbs;
            Title = HeaderStringHelper.GetPageTitle(ForumResource.EditTopicBreadCrumbs);
        }
    }
}