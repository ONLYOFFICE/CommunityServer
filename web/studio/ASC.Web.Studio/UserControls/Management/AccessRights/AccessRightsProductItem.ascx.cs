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