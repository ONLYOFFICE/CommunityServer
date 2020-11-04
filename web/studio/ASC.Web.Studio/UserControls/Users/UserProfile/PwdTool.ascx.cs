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
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio.UserControls.Users.UserProfile
{
    public partial class PwdTool : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/PwdTool.ascx"; }
        }

        public Guid UserID { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _pwdRemainderContainer.Options.IsPopup = true;
            _pwdRemainderContainer.Options.InfoMessageText = "";
            _pwdRemainderContainer.Options.InfoType = InfoType.Info;

            Page.RegisterBodyScripts("~/UserControls/Users/UserProfile/js/pwdtool.js");
        }
    }
}