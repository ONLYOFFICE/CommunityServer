/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
        public string AddListButtonText { get; set; }

        public string AddPopupWindowText { get; set; }

        public string EditText { get; set; }
        public string EditPopupWindowText { get; set; }

        public string AjaxProgressText { get; set; }
        public string DeleteText { get; set; }

        public string DescriptionText { get; set; }
        public string DescriptionTextEditDelete { get; set; }


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