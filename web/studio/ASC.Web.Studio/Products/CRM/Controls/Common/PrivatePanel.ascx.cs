/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

            sb.AppendFormat(@"ASC.CRM.PrivatePanel.init({0}, ASC.CRM.Resources.CRMCommonResource.Notify, {1}, null, {2});",
                (!HideNotifyPanel).ToString().ToLower(),
                JsonConvert.SerializeObject(UsersWhoHasAccess),
                JsonConvert.SerializeObject(DisabledUsers)
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}