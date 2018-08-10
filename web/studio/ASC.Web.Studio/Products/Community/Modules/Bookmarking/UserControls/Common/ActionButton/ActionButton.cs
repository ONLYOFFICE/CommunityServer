/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
            Page.RegisterBodyScripts("~/products/community/modules/bookmarking/UserControls/Common/ActionButton/js/actionbutton.js");
        }
    }
}