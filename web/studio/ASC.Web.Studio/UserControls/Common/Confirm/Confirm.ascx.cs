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

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class Confirm : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/Confirm/Confirm.ascx"; }
        }

        public string Title { get; set; }
        public string Value { get; set; }
        public string SelectTitle { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _studioConfirm.Options.IsPopup = true;

            Page.RegisterBodyScripts("~/UserControls/Common/Confirm/js/confirm.js");
        }
    }
}