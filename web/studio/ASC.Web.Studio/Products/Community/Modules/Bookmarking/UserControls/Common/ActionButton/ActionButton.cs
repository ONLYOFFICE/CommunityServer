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
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.UserControls.Bookmarking.Common
{
    [ToolboxData("<{0}:ActionButton runat=server></{0}:ActionButton>")]
    public class ActionButton : WebControl
    {
        public string ButtonCssClass { get; set; }

        public string ButtonID { get; set; }

        public string ButtonContainer { get; set; }

        public string ButtonText { get; set; }

        public string AjaxRequestText { get; set; }

        public string OnClickJavascript { get; set; }

        public bool EnableRedirectAfterAjax { get; set; }

        public bool DisableInputs { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(ButtonID))
            {
                ButtonID = Guid.NewGuid().ToString();
            }

            sb.AppendFormat(@"<a class='{0}' id='{1}' href='javascript:void(0);' onclick='actionButtonClick({4}, {5}); {2}' >{3}</a>",
                            ButtonCssClass, ButtonID, OnClickJavascript, ButtonText, EnableRedirectAfterAjax.ToString().ToLower(), ButtonContainer);

            writer.Write(sb);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Page.RegisterBodyScripts("~/Products/Community/Modules/Bookmarking/UserControls/Common/ActionButton/js/actionbutton.js");
        }
    }
}