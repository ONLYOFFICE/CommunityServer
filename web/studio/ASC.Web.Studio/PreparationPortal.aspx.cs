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


using System.Collections.Specialized;
using System.Text;
using System.Web;
using AjaxPro;
using Resources;
using System;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Studio
{
    public partial class PreparationPortal : BasePage
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            InitInlineScript();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            PreparationPortalContent.Controls.Add(LoadControl(PreparationPortalCnt.Location));

            var type = Request["type"] ?? "";

            switch (type){ 
               //Migration
                case "0" :
                    Title = Resource.TransferPortalTitle;
                    break;
                //Backup
                case "1":
                    Title = Resource.RestoreTitle;
                    break;
                default:
                    Title = Resource.MainPageTitle;
                    break;
            }
        }
        private void InitInlineScript()
        {
            var scripts = HttpContext.Current.Items[Constant.AjaxID + ".pagescripts"] as ListDictionary;

            if (scripts == null) return;

            var sb = new StringBuilder();

            foreach (var key in scripts.Keys)
            {
                sb.Append(scripts[key]);
            }

            InlineScript.Scripts.Add(new Tuple<string, bool>(sb.ToString(), false));
        }
    }
}