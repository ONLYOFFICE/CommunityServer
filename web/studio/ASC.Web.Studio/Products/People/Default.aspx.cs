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

#region Import

using System;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.People.Core;
using ASC.Web.People.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

#endregion

namespace ASC.Web.People
{
    public partial class Default : MainPage
    {
        #region Properies

        protected bool IsAdmin { get; private set; }

        protected bool IsFreeTariff { get; private set; }

        private UserInfo userInfo;

        #endregion

        #region Events

        public AllowedActions Actions;


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AjaxPro.Utility.RegisterTypeForAjax(typeof(UserProfileControl));
            this.Page.RegisterBodyScripts(LoadControl(VirtualPathUtility.ToAbsolute("~/products/people/masters/DefaultBodyScripts.ascx")));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            IsAdmin = userInfo.IsAdmin();
            Actions = new AllowedActions(userInfo);

            var quota = TenantExtra.GetTenantQuota();
            IsFreeTariff = quota.Free && !quota.Open;

            _confirmationDeleteDepartmentPanel.Options.IsPopup = true;
            _resendInviteDialog.Options.IsPopup = true;
            _changeStatusDialog.Options.IsPopup = true;
            _changeTypeDialog.Options.IsPopup = true;
            _deleteUsersDialog.Options.IsPopup = true;
            _deleteProfileContainer.Options.IsPopup = true;

            var emptyContentForPeopleFilter = new EmptyScreenControl
                {
                    ID = "emptyContentForPeopleFilter",
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_filter.png"),
                    Header = PeopleResource.NotFoundTitle,
                    Describe = PeopleResource.NotFoundDescription,
                    ButtonHTML = String.Format(@"<a class='clearFilterButton link dotline' href='javascript:void(0);' 
                                            onclick='ASC.People.PeopleController.resetAllFilters();'>{0}</a>",
                                               PeopleResource.ClearButton),
                    CssClass = "display-none"
                };

            emptyScreen.Controls.Add(emptyContentForPeopleFilter);


            var controlEmailChange = (UserEmailChange)LoadControl(UserEmailChange.Location);
            controlEmailChange.UserInfo = userInfo;
            userEmailChange.Controls.Add(controlEmailChange);

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
            userConfirmationDelete.Controls.Add(LoadControl(ConfirmationDeleteUser.Location));

            if (Actions.AllowEdit)
            {
                userPwdChange.Controls.Add(LoadControl(PwdTool.Location));
            }
            Title = HeaderStringHelper.GetPageTitle(PeopleResource.ProductName);
        }

        #endregion
    }
}