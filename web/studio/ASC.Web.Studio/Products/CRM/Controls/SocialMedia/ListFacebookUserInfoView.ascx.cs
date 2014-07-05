/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ASC.SocialMedia.Facebook;

namespace ASC.Web.CRM.Controls.SocialMedia
{
    public partial class ListFacebookUserInfoView : System.Web.UI.UserControl
    {
        public List<FacebookUserInfo> UserInfoCollection { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserInfoCollection != null && UserInfoCollection.Count > 0)
            {
                BindRepeater();
                ShowRepeater();
            }
            else
                ShowNotFoundText();

        }

        private void BindRepeater()
        {
            _ctrlRptrUsers.DataSource = UserInfoCollection;
            _ctrlRptrUsers.DataBind();

        }

        private void ShowRepeater()
        {
            _ctrlDivNotFound.Visible = false;
            _ctrlRptrUsers.Visible = true;
        }

        private void ShowNotFoundText()
        {
            _ctrlDivNotFound.Visible = true;
            _ctrlRptrUsers.Visible = false;
        }

        protected void _ctrlRptrUsers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            string userID = ((FacebookUserInfo)e.Item.DataItem).UserID;
            string _ctrlRelateAccountScript = String.Format("ASC.CRM.SocialMedia.ShowAccountRelationPanel('{0}','{1}'); return false;", userID, "facebook");
            ((HtmlAnchor)e.Item.FindControl("_ctrlRelateContactWithAccount")).Attributes.Add("onclick", _ctrlRelateAccountScript);
        }
    }
}