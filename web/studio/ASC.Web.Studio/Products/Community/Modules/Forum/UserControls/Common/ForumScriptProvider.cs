/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Text;
using System.Web;
using System.Web.UI;

using ASC.Web.Community.Modules.Forum.UserControls.Resources;

namespace ASC.Web.UserControls.Forum.Common
{
    public class ForumScriptProvider : Control
    {
        public bool RegistrySearchHelper { get; set; }

        public Guid SettingsID { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            Page.RegisterBodyScripts(Community.Forum.ForumManager.BaseVirtualPath + "/js/forum.js");

            if (RegistrySearchHelper)
                Page.RegisterBodyScripts(Community.Forum.ForumManager.BaseVirtualPath + "/js/searchhelper.js");


            var script = new StringBuilder();
            script.Append("if (typeof(ForumManager)=== 'undefined'){ForumManager = {};}");
            script.Append("ForumManager.QuestionEmptyMessage = '" + ForumUCResource.QuestionEmptyMessage + "';");
            script.Append("ForumManager.SubjectEmptyMessage = '" + ForumUCResource.SubjectEmptyMessage + "';");
            script.Append("ForumManager.ApproveTopicButton = '" + ForumUCResource.ApproveButton + "';");
            script.Append("ForumManager.OpenTopicButton = '" + ForumUCResource.OpenTopicButton + "';");
            script.Append("ForumManager.CloseTopicButton = '" + ForumUCResource.CloseTopicButton + "';");
            script.Append("ForumManager.StickyTopicButton = '" + ForumUCResource.StickyTopicButton + "';");
            script.Append("ForumManager.ClearStickyTopicButton = '" + ForumUCResource.ClearStickyTopicButton + "';");
            script.Append("ForumManager.DeleteTopicButton = '" + ForumUCResource.DeleteButton + "';");
            script.Append("ForumManager.EditTopicButton = '" + ForumUCResource.EditButton + "';");
            script.Append("ForumManager.ConfirmMessage = '" + ForumUCResource.ConfirmMessage + "';");
            script.Append("ForumManager.NameEmptyString = '" + ForumUCResource.NameEmptyString + "';");
            script.Append("ForumManager.SaveButton = '" + ForumUCResource.SaveButton + "';");
            script.Append("ForumManager.CancelButton = '" + ForumUCResource.CancelButton + "';");
            script.Append("ForumManager.SettingsID = '" + SettingsID + "';");
            script.Append("ForumManager.TextEmptyMessage = '" + ForumUCResource.TextEmptyMessage + "';");

            Page.RegisterInlineScript(script.ToString());

        }

    }
}
