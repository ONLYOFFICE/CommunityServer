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


#region Import

using System;
using System.Web;
using System.Text;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class ListItemView : BaseUserControl
    {
        #region Members

        public static string Location { get { return PathProvider.GetFileStaticRelativePath("Settings/ListItemView.ascx"); } }

        public ListType CurrentTypeValue { get; set;}
        
        public string AddButtonText { get; set; }

        public string AddPopupWindowText { get; set; }

        public string EditText { get; set; }
        public string EditPopupWindowText { get; set; }

        public string AjaxProgressText { get; set; }
        public string DeleteText { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterScript();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"
                    ASC.CRM.ListItemView.init({0}, '{1}', '{2}');

                    ASC.CRM.ListItemView.EditItemHeaderText = '{3}';
                    ASC.CRM.ListItemView.AddItemProcessText = '{4}';",
                (int)CurrentTypeValue,
                AddPopupWindowText.ReplaceSingleQuote(),
                AddButtonText.ReplaceSingleQuote(),
                EditPopupWindowText.ReplaceSingleQuote(),
                AjaxProgressText.ReplaceSingleQuote()
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}