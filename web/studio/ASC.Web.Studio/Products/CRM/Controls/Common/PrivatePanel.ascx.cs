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
using System.Web;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio.Core.Users;
using ASC.Web.CRM.Resources;
using System.Collections.Generic;
using ASC.Web.CRM.Classes;
using System.Text;
using Newtonsoft.Json;


namespace ASC.Web.CRM.Controls.Common
{
    public partial class PrivatePanel : BaseUserControl
    {
        #region Properties

        public static String Location
        {
            get
            {
                return PathProvider.GetFileStaticRelativePath("Common/PrivatePanel.ascx");
            }
        }

        public String Title { get; set; }

        public String Description { get; set; }

        public String CheckBoxLabel { get; set; }

        public String AccessListLable { get; set; }

        public bool IsPrivateItem { get; set; }

        public List<String> UsersWhoHasAccess { get; set; }

        public Dictionary<Guid, String> SelectedUsers { get; set; }

        public List<Guid> DisabledUsers { get; set; }

        public bool HideNotifyPanel { get; set; }

        #endregion

        #region Events

        protected void Page_Init(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Title))
                Title = CRMCommonResource.PrivatePanelHeader;

            if (String.IsNullOrEmpty(Description))
                Description = CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelDescription");

            if (String.IsNullOrEmpty(CheckBoxLabel))
                CheckBoxLabel = CRMCommonResource.PrivatePanelCheckBoxLabel;

            if (String.IsNullOrEmpty(AccessListLable))
                AccessListLable = CustomNamingPeople.Substitute<CRMCommonResource>("PrivatePanelAccessListLable");  

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterClientScriptHelper.DataUserSelectorListView(Page, "", SelectedUsers);

            RegisterScript();
        }

        #endregion

        #region Methods

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.PrivatePanel.init({0},ASC.CRM.Resources.CRMCommonResource.Notify,{1},null,{2});",
                (!HideNotifyPanel).ToString().ToLower(),
                JsonConvert.SerializeObject(UsersWhoHasAccess),
                JsonConvert.SerializeObject(DisabledUsers)
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}