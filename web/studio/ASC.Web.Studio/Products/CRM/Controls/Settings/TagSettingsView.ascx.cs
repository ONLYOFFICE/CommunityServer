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
using System.Linq;
using ASC.Web.CRM.Configuration;
using ASC.Web.Core.Utility.Skins;
using ASC.CRM.Core;
using Newtonsoft.Json;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Controls.Common;
using System.Text;

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class TagSettingsView : BaseUserControl
    {
        #region Members

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/TagSettingsView.ascx"); }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterInlineScript(@"ASC.CRM.TagSettingsView.init();");
        }

        #endregion
    }
}