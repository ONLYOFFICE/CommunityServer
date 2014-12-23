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
using System.Web;
using System.Linq;
using System.Web.UI;
using System.Collections.Generic;
using ASC.Web.Core.Utility.Skins;
using ASC.Core;
using ASC.Web.Studio.Controls.Users;
using AjaxPro;
using ASC.Core.Users;
using ASC.Web.Studio.Core.Users;
using System.Text;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class AccessRightsProductItem : UserControl
    {
        #region Properies

        public static string Location
        {
            get { return "~/UserControls/Management/AccessRights/AccessRightsProductItem.ascx"; }
        }

        public Item ProductItem { get; set; }

        protected string PeopleImgSrc
        {
            get { return WebImageSupplier.GetAbsoluteWebPath("user_12.png"); }
        }

        protected string GroupImgSrc
        {
            get { return WebImageSupplier.GetAbsoluteWebPath("group_12.png"); }
        }

        protected string TrashImgSrc
        {
            get { return WebImageSupplier.GetAbsoluteWebPath("trash_12.png"); }
        }

        protected bool PublicModule
        {
            get { return ProductItem.SelectedUsers.Count == 0 && ProductItem.SelectedGroups.Count == 0; }
        }

        protected Guid PortalOwnerId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().OwnerId; }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(ProductItem.ItemName)) return;

            RegisterClientScript();
        }


        private void RegisterClientScript()
        {
            var sb = new StringBuilder();

            var ids = ProductItem.SelectedUsers.Select(i => i.ID).ToArray();
            var names = ProductItem.SelectedUsers.Select(i => i.DisplayUserName()).ToArray();

            sb.AppendFormat("SelectedUsers_{0} = {1};",
                ProductItem.ItemName,
                JavaScriptSerializer.Serialize(
                new
                {
                    IDs = ids,
                    Names = names,
                    PeopleImgSrc = PeopleImgSrc,
                    TrashImgSrc = TrashImgSrc,
                    TrashImgTitle = Resources.Resource.DeleteButton,
                    CurrentUserID = SecurityContext.CurrentAccount.ID
                })
            );

            ids = ProductItem.SelectedGroups.Select(i => i.ID).ToArray();
            names = ProductItem.SelectedGroups.Select(i => i.Name.HtmlEncode()).ToArray();

            sb.AppendFormat("SelectedGroups_{0} = {1};",
                ProductItem.ItemName,
                JavaScriptSerializer.Serialize(
                new
                {
                    IDs = ids,
                    Names = names,
                    GroupImgSrc = GroupImgSrc,
                    TrashImgSrc = TrashImgSrc,
                    TrashImgTitle = Resources.Resource.DeleteButton
                })
            );

            if (!ProductItem.CanNotBeDisabled)
            {
                sb.AppendFormat("ASC.Settings.AccessRights.initProduct(\"{0}\",\"{1}\",{2});",
                                ProductItem.ID,
                                ProductItem.ItemName,
                                PublicModule.ToString().ToLower()
                    );
            }


            Page.RegisterInlineScript(sb.ToString());
        }
    }
}