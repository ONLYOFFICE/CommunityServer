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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Management;

namespace ASC.Web.Studio.UserControls.Users
{
    public class ImportUsersWebControl: WebControl
    {
        private ImportUsers _users;
        private Container _localContainer;

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        public string Text { get; set; }
        public string LinkStyle { get; set; }
        public bool RenderLink { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!ChildControlsCreated)
                CreateChildControls();
        }

        protected override void CreateChildControls()
        {
            if (RenderLink)
            {
                var link = new HtmlAnchor {InnerText = Text, HRef = "#"};
                link.Attributes.Add("onclick", "ImportUsersManager.ShowImportControl();");
                if (!string.IsNullOrEmpty(LinkStyle))
                    link.Attributes.Add("class", LinkStyle);

                Controls.Add(link);
            }
            Controls.Add(Page.LoadControl(ImportUsersTemplate.Location));
            _users = new ImportUsers();
            _users = (ImportUsers)_users.LoadControl(ImportUsers.Location);

            Controls.Add(new LiteralControl("<div id=\"importAreaBlock\" class=\"importAreaBlock\" style=\"display:none\">"));

            _localContainer = new Container { Body = new PlaceHolder(), Header = new PlaceHolder() };
            _localContainer.Body.Controls.Add(_users);
            var html = new HtmlGenericControl("DIV") { InnerHtml = CustomNamingPeople.Substitute<Resources.Resource>("ImportContactsHeader").HtmlEncode() };
            _localContainer.Header.Controls.Add(html);
            Controls.Add(_localContainer);
            Controls.Add(new LiteralControl("</div>"));


            Controls.Add(Page.LoadControl(TariffLimitExceed.Location));

            base.CreateChildControls();

            ChildControlsCreated = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            _localContainer.Options.IsPopup = true;
            _localContainer.Options.OnCancelButtonClick = "ImportUsersManager.HideImportWindow();";
        }
    }
}
