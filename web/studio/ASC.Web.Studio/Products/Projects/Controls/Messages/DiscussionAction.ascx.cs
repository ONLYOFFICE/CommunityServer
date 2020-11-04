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
using System.Web;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Resources;


namespace ASC.Web.Projects.Controls.Messages
{
    public partial class DiscussionAction : BaseUserControl
    {       
        public Project Project { get { return Page.Project; } }
        public Message Discussion { get; set; }
        public string Text { get; set; }
        public int ProjectFolderId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Common/ckeditor/ckeditor-connector.js");

            Text = "";

            if (Discussion != null)
            {
                discussionTitle.Text = Discussion.Title;
                Text = Discussion.Description;
            }
        }

        protected string GetPageTitle()
        {
            return Discussion == null ? MessageResource.CreateDiscussion : MessageResource.EditMessage;
        }
    }
}
