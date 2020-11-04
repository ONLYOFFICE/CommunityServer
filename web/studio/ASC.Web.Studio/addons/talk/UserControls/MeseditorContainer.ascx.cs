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
using System.Web.UI;

namespace ASC.Web.Talk.UserControls
{
    public partial class MeseditorContainer : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/js/uploader/jquery.fileupload.js");
            Page.RegisterBodyScripts("~/addons/talk/js/talk.meseditorcontainer.js");
            Page.RegisterInlineScript("ASC.TMTalk.meseditorContainer.init('talkTextareaContainer', '" + GetMeseditorStyle() + "');");

            var cfg = new TalkConfiguration();
            talkHistoryButton.Visible = cfg.EnabledHistory;
            talkMassendButton.Visible = cfg.EnabledMassend;
            talkConferenceButton.Visible = cfg.EnabledConferences;
        }

        public String GetMeseditorStyle()
        {
            return VirtualPathUtility.ToAbsolute("~/addons/talk/css/default/talk.messagearea.css");
        }
    }
}