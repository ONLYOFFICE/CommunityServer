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
using System.ServiceModel.Security;
using System.Web;
using System.Web.UI;
using ASC.Core.Common.Logging;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Core.Notify;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("AjaxPro.ChangeMobileNumber")]
    public partial class ChangeMobileNumber : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/SmsControls/ChangeMobileNumber.ascx"; }
        }

        public UserInfo User;

        protected void Page_Load(object sender, EventArgs e)
        {
            _changePhoneContainer.Options.IsPopup = true;

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/smscontrols/js/changemobile.js"));

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        [AjaxMethod]
        public string SendNotificationToChange(string userId)
        {
            var user = CoreContext.UserManager.GetUsers(
                string.IsNullOrEmpty(userId)
                    ? SecurityContext.CurrentAccount.ID
                    : new Guid(userId));

            var canChange =
                user.IsMe()
                || SecurityContext.CheckPermissions(new UserSecurityProvider(user.ID), ASC.Core.Users.Constants.Action_EditUser);

            if (!canChange)
                throw new SecurityAccessDeniedException(Resource.ErrorAccessDenied);

            user.MobilePhoneActivationStatus = MobilePhoneActivationStatus.NotActivated;
            CoreContext.UserManager.SaveUserInfo(user);

            if (user.IsMe())
            {
                return CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PhoneActivation);
            }

            AdminLog.PostAction("UserProfile: erase phone number for user with id {0}", user.ID);
            StudioNotifyService.Instance.SendMsgMobilePhoneChange(user);
            return string.Empty;
        }
    }
}