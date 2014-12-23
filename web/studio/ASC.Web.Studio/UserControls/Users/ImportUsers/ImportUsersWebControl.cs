/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
