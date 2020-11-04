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

namespace ASC.Web.Studio.UserControls.FirstTime
{
    public partial class StepContainer : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/FirstTime/StepContainer.ascx"; }
        }

        public Control ChildControl { get; set; }
        public string SaveButtonEvent { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/FirstTime/js/view.js")
                .RegisterStyle("~/UserControls/FirstTime/css/stepcontainer.less");

            SaveButtonEvent = "ASC.Controls.FirstTimeView.SaveRequiredStep();";
            content1.Controls.Add(LoadControl(EmailAndPassword.Location));
        }
    }
}