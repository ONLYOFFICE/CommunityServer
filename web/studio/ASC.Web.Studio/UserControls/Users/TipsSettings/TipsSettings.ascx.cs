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
using ASC.Core;
using ASC.Core.Common.Settings;

namespace ASC.Web.Studio.UserControls.Users.TipsSettings
{
    public partial class TipsSettings : UserControl
    {
        protected bool ShowTips { get; set; }

        public static string Location
        {
            get { return "~/UserControls/Users/TipsSettings/TipsSettings.ascx"; }
        }

        public TipsSettings()
        {
            ShowTips = Core.TipsSettings.LoadForCurrentUser().Show;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Users/TipsSettings/js/tipssettings.js")
                .RegisterStyle("~/UserControls/Users/TipsSettings/css/tipssettings.less");
        }
    }
}