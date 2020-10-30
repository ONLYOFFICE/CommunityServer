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
using System.Collections.Generic;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Core;
using AjaxPro;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ProductsAndInstruments, Location, SortOrder = 50)]
    public partial class CustomNavigation : UserControl
    {
        public const string Location = "~/UserControls/Management/CustomNavigation/CustomNavigation.ascx";

        protected bool Enabled { get; set; }

        protected List<CustomNavigationItem> Items { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Enabled = SetupInfo.IsVisibleSettings("CustomNavigation");
            
            if (!Enabled) return;

            Page
                .RegisterBodyScripts(
                    "~/js/uploader/ajaxupload.js",
                    "~/UserControls/Management/CustomNavigation/js/customnavigation.js")
                .RegisterStyle(
                    "~/UserControls/Management/CustomNavigation/css/customnavigation.less");

            Items = CustomNavigationSettings.Load().Items;
        }

    }
}